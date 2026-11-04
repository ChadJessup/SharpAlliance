#!/usr/bin/env python3
"""
c2cs.py - first-pass C -> C# skeleton generator for the SharpAlliance port.

It does NOT produce compiling code. It produces a file per C source with:
  * every function that is not already present in the C# solution (unless --all),
  * a translated signature, with C integers promoted to the port's enums
    (Items, InventorySlot, IC, ...) by inference from the existing C# code,
  * the body run through the same find/replace rules the README describes,
  * `&` on flag enums rewritten to HasFlag, `&x` call arguments rewritten to ref/out,
  * `// TODO(port):` markers on lines that need a human (pointers, goto, memcpy...).

Type inference (see TypeInfer): the existing C# is indexed for field, parameter, constant,
method and enum types. A C `int` parameter, local or return value is promoted to an enum when
  * it is compared / assigned / masked against something with a known enum type,
  * it indexes a Dictionary<EnumKey, ...>,
  * it is passed to (or receives the result of) a method whose C# signature says so,
  * its name is used consistently with one enum in already-ported code.

Usage:
  python tools/c2cs.py queue  <ja2-src-root> [--repo .]            > work queue (markdown) on stdout
  python tools/c2cs.py port   <file.c> --out <dir> [--repo .] [--all] [--map tools/c2cs.map.json]

Optional tools/c2cs.map.json:
  { "classes": {"Items.c": "ItemSubSystem"},        # force the target class for a C file
    "aliases": {"NOTHING": "Items.NONE"} }          # C token -> C# expression

The original C source is not part of this repo; clone it somewhere else:
  https://github.com/historicalsource/jaggedalliance2
"""
import argparse
import bisect
import collections
import json
import os
import re
import sys

# ----------------------------------------------------------------------------
# Conventions taken from the existing port (see README.md).
# ----------------------------------------------------------------------------
TYPE_MAP = {
    'UINT8': 'int', 'UINT16': 'int', 'UINT32': 'int', 'INT8': 'int', 'INT16': 'int',
    'INT32': 'int', 'UINT': 'int', 'INT': 'int', 'BYTE': 'int', 'WORD': 'int',
    'DWORD': 'int', 'CHAR8': 'int', 'SHORT': 'int', 'LONG': 'int', 'ULONG': 'int',
    'INT64': 'long', 'UINT64': 'long',
    'BOOLEAN': 'bool', 'BOOL': 'bool',
    'FLOAT': 'float', 'DOUBLE': 'double',
    'CHAR16': 'char', 'STR16': 'string', 'STR': 'string', 'STR8': 'string',
    'LPCSTR': 'string', 'wchar_t': 'char',
}
TOKEN_MAP = dict(TYPE_MAP)
TOKEN_MAP.update({'NULL': 'null', 'TRUE': 'true', 'FALSE': 'false'})
PRIMS = ('int', 'bool', 'long', 'float', 'double')

CS_KEYWORDS = {
    'ref', 'out', 'in', 'object', 'string', 'params', 'base', 'event', 'lock', 'checked',
    'unchecked', 'operator', 'namespace', 'internal', 'fixed', 'is', 'as', 'delegate',
    'decimal', 'char', 'byte', 'short', 'long', 'bool', 'int', 'float', 'double', 'default',
}

# Constructs that need a human.
TODO_PATTERNS = [
    (re.compile(r'\bgoto\b'), 'goto'),
    (re.compile(r'\bsizeof\b'), 'sizeof'),
    (re.compile(r'\b(memcpy|memset|memcmp|memmove)\b'), 'mem*'),
    (re.compile(r'\b(wcs\w+|str\w+|swprintf|sprintf|wsprintf)\s*\('), 'string function'),
    # address-of: a unary & (after an opening token), not a binary & between operands
    (re.compile(r'(?:^|[(,=?:!{]|\breturn)\s*&(?!&)\s*[A-Za-z_(]'), 'address-of'),
    (re.compile(r'(?<![\w)\]])\*\s*[A-Za-z_]\w*\s*(=|\+\+|--)'), 'pointer deref'),
    # a binary & that survived the HasFlag pass: operand types were not known to be a [Flags] enum
    (re.compile(r'[\w)\]]\s*&(?!&)\s*[\w(]'), 'bit test: check flag type'),
    (re.compile(r'\bMemAlloc|MemFree|MemRealloc\b'), 'manual memory'),
    (re.compile(r'\bFile(Read|Write|Open|Close)\b'), 'file io'),
]

# ----------------------------------------------------------------------------
# Small helpers
# ----------------------------------------------------------------------------
TYPE_RX = r'[\w\.]+(?:<[^;=(){}]*>)?(?:\[\])*\??'
IDENT = r'[A-Za-z_]\w*'
CHAIN = (IDENT + r'(?:\s*\.\s*' + IDENT + r'|\s*\[(?:[^\[\]]|\[[^\[\]]*\])*\]'
         r'|\s*\((?:[^()]|\([^()]*\))*\))*')
CHAIN_RE = re.compile(CHAIN)
STEP_RE = re.compile(r'\s*\.\s*(?P<f>' + IDENT + r')|\s*\[(?P<i>(?:[^\[\]]|\[[^\[\]]*\])*)\]'
                     r'|\s*\((?P<c>(?:[^()]|\([^()]*\))*)\)')
PAIR_RE = re.compile(r'(?P<a>' + CHAIN + r')\s*(?P<op>==|!=|<=|>=|<|>|=|&(?!&)|\|(?!\|))(?!=)\s*(?P<b>' + CHAIN + r')')
RET_RE = re.compile(r'\breturn\s*\(?\s*(?P<e>' + CHAIN + r')\s*\)?\s*;')
METHOD_RE = re.compile(
    r'\b(?:public|private|internal|protected)\s+(?:(?:static|async|unsafe|override|virtual|new|extern)\s+)*'
    r'(?P<ret>' + TYPE_RX + r')\s+(?P<name>\w+)\s*\((?P<params>[^()]*)\)\s*(?:\{|=>|where\b|;)')
FIELD_RE = re.compile(
    r'\b(?:public|internal|protected|private)\s+(?P<mods>(?:(?:static|readonly|const|volatile|new)\s+)*)'
    r'(?P<t>' + TYPE_RX + r')\s+(?P<n>\w+)\s*(?:=|;|\{|,)')
RECORD_RE = re.compile(r'\brecord(?:\s+(?:struct|class))?\s+(\w+)\s*\(([^()]*)\)')
TYPE_DECL_RE = re.compile(r'\b(class|struct|interface|enum|record)(?:\s+(?:struct|class))?\s+(\w+)')
ENUM_RE = re.compile(r'\benum\s+(\w+)\s*(?::\s*\w+)?\s*\{(.*?)\}', re.S)


def strip_cs_comments(src):
    src = re.sub(r'/\*.*?\*/', '', src, flags=re.S)
    return re.sub(r'//[^\n]*', '', src)


def split_params(ptext):
    parts, depth, cur = [], 0, ''
    for c in ptext:
        if c in '([<':
            depth += 1
        elif c in ')]>':
            depth -= 1
        if c == ',' and depth == 0:
            parts.append(cur)
            cur = ''
        else:
            cur += c
    if cur.strip():
        parts.append(cur)
    return [p.strip() for p in parts]


def norm_type(t):
    return t.strip().rstrip('?')


def split_cs_param(p):
    """'ref Items usItem = 0' -> (modifier, type, name)."""
    p = re.sub(r'=.*$', '', p).strip()
    mod = ''
    m = re.match(r'(ref|out|in|params|this)\s+(.*)', p)
    if m:
        mod, p = m.group(1), m.group(2)
    i = p.rfind(' ')
    return (mod, p[:i].strip(), p[i + 1:].strip()) if i > 0 else (mod, p, '')


# ----------------------------------------------------------------------------
# Index of the existing C# solution
# ----------------------------------------------------------------------------
class Repo:
    def __init__(self, root):
        self.ids = set()
        self.enum_tokens = {}                          # PREFIX_MEMBER -> Prefix.MEMBER
        self.enums = {}                                # name -> {'flags': bool, 'members': [...]}
        self.member_enum = collections.defaultdict(set)  # MEMBER -> {enum names}
        self.field_types = collections.defaultdict(collections.Counter)
        self.composite = set()                         # (enum, member) whose value has several bits set
        self.owner_fields = {}                         # (class, field) -> type
        self.param_types = collections.defaultdict(collections.Counter)
        self.const_types = {}                          # static/const name -> type
        self.methods = {}                              # name -> [[mods]]  (ref/out/in per param)
        self.sigs = collections.defaultdict(list)      # name -> [(ret, [ptypes])]
        self.method_class = collections.defaultdict(collections.Counter)
        self.type_names = set()
        self.defined_names = set()                     # fields/consts/methods (blocks bare enum-member rewrite)
        self._scan(root)

    def _scan(self, root):
        for dirpath, dirs, files in os.walk(root):
            dirs[:] = [d for d in dirs if d not in ('bin', 'obj', '.git', 'tools', 'Tests', 'SharpAlliance.Web', 'node_modules')]
            for f in files:
                if f.endswith('.cs'):
                    with open(os.path.join(dirpath, f), errors='ignore', encoding='utf-8-sig') as fh:
                        src = strip_cs_comments(fh.read())
                    self._scan_file(src)

    def _scan_file(self, src):
        self.ids.update(re.findall(IDENT, src))
        decls = [(m.start(), m.group(2)) for m in TYPE_DECL_RE.finditer(src) if m.group(1) != 'enum']
        for m in TYPE_DECL_RE.finditer(src):
            self.type_names.add(m.group(2))
        starts = [d[0] for d in decls]
        for m in ENUM_RE.finditer(src):
            name, body = m.group(1), m.group(2)
            flags = bool(re.search(r'\[\s*Flags\s*\]\s*(?:public\s+|internal\s+)?(?:\[[^\]]*\]\s*)*$', src[max(0, m.start() - 160):m.start()]))
            members = []
            for mm in re.finditer(r'^\s*(\w+)\s*(?:=\s*([^,]*))?,?', body, re.M):
                members.append(mm.group(1))
                val = (mm.group(2) or '').strip()
                if '|' in val:
                    self.composite.add((name, mm.group(1)))
                elif re.fullmatch(r'0[xX][0-9a-fA-F]+|\d+', val):
                    n = int(val, 0)
                    if n & (n - 1):
                        self.composite.add((name, mm.group(1)))
            self.enums[name] = {'flags': flags, 'members': members}
            for mem in members:
                self.enum_tokens.setdefault(f'{name}_{mem}', f'{name}.{mem}')
                self.member_enum[mem].add(name)
        for m in METHOD_RE.finditer(src):
            name = m.group('name')
            params = [split_cs_param(p) for p in split_params(m.group('params'))]
            self.defined_names.add(name)
            if params:
                self.methods.setdefault(name, []).append([p[0] if p[0] in ('ref', 'out', 'in') else '' for p in params])
            self.sigs[name].append((m.group('ret'), [p[1] for p in params]))
            for _, t, n in params:
                if n:
                    self.param_types[n][t] += 1
            i = bisect.bisect_right(starts, m.start()) - 1
            if i >= 0:
                self.method_class[name][decls[i][1]] += 1
        for m in FIELD_RE.finditer(src):
            n, t, mods = m.group('n'), m.group('t'), m.group('mods')
            i = bisect.bisect_right(starts, m.start()) - 1
            if i >= 0:
                self.owner_fields.setdefault((decls[i][1], n), t)
            self.field_types[n][t] += 1
            self.defined_names.add(n)
            if 'const' in mods or 'static' in mods:
                self.const_types.setdefault(n, t)
        for m in RECORD_RE.finditer(src):
            for p in split_params(m.group(2)):
                _, t, n = split_cs_param(p)
                if n:
                    self.owner_fields.setdefault((m.group(1), n), t)
                    self.field_types[n][t] += 1
                    self.defined_names.add(n)

    # -- queries -------------------------------------------------------------
    def is_enum(self, t):
        return bool(t) and norm_type(t) in self.enums

    def is_flags(self, t):
        return self.is_enum(t) and self.enums[norm_type(t)]['flags']

    def unique_member_enum(self, token):
        es = self.member_enum.get(token)
        if es and len(es) == 1 and token not in self.defined_names and token not in self.const_types:
            return next(iter(es))
        return None

    def field_type(self, name):
        c = self.field_types.get(name)
        if not c:
            return None
        t, n = c.most_common(1)[0]
        return t if n * 2 > sum(c.values()) else None   # require a clear majority

    def name_prior(self, name):
        """Enum type this identifier is consistently given in already-ported code."""
        c = collections.Counter()
        c.update(self.param_types.get(name, {}))
        c.update(self.field_types.get(name, {}))
        tot = sum(c.values())
        if tot < 2:
            return None
        t, n = c.most_common(1)[0]
        return norm_type(t) if self.is_enum(t) and n / tot >= 0.75 else None

    def sig(self, name, nargs):
        outs = [s for s in self.sigs.get(name, []) if len(s[1]) == nargs]
        return outs[0] if outs else None

    @staticmethod
    def elem_type(t):
        t = norm_type(t)
        if t.endswith('[]'):
            return t[:-2]
        m = re.match(r'(?:List|IList|IReadOnlyList|HashSet)<(.+)>$', t)
        if m:
            return m.group(1).strip()
        m = re.match(r'(?:Dictionary|IDictionary)<(.+)>$', t)
        if m:
            parts = split_params(m.group(1))
            return parts[-1] if len(parts) == 2 else None
        return None

    @staticmethod
    def key_type(t):
        m = re.match(r'(?:Dictionary|IDictionary)<(.+)>$', norm_type(t))
        if m:
            parts = split_params(m.group(1))
            return parts[0] if len(parts) == 2 else None
        return None

    def target_class(self, fn_names):
        votes = collections.Counter()
        for n in fn_names:
            if n in self.method_class:
                votes[self.method_class[n].most_common(1)[0][0]] += 1
        if votes:
            c, n = votes.most_common(1)[0]
            if n >= 3 or n * 2 >= len(fn_names):
                return c
        return None


# ----------------------------------------------------------------------------
# Parsing C
# ----------------------------------------------------------------------------
def match_brace(text, start):
    """text[start] == '{'; return index of the matching '}' (skips strings/comments)."""
    depth, i, n = 0, start, len(text)
    while i < n:
        c = text[i]
        nxt = text[i + 1] if i + 1 < n else ''
        if c == '/' and nxt == '/':
            i = text.find('\n', i)
            if i < 0:
                return n - 1
        elif c == '/' and nxt == '*':
            i = text.find('*/', i + 2)
            if i < 0:
                return n - 1
            i += 1
        elif c in '"\'':
            q = c
            i += 1
            while i < n and text[i] != q:
                if text[i] == '\\':
                    i += 1
                i += 1
        elif c == '{':
            depth += 1
        elif c == '}':
            depth -= 1
            if depth == 0:
                return i
        i += 1
    return n - 1


FN_START = re.compile(r'^(?!\s|#|/|\{|\}|\*)[A-Za-z_][\w\s\*]*?\b([A-Za-z_]\w*)\s*\(', re.M)


def extract_functions(src):
    """Return [(name, signature_text, body_text)] for each function definition."""
    out, pos = [], 0
    for m in FN_START.finditer(src):
        if m.start() < pos:
            continue
        name = m.group(1)
        if name in ('if', 'while', 'for', 'switch', 'return', 'sizeof', 'else', 'define'):
            continue
        j, brace, depth = m.start(), -1, 0
        while j < len(src):
            c = src[j]
            if c == '(':
                depth += 1
            elif c == ')':
                depth -= 1
            elif c == ';' and depth == 0:
                break
            elif c == '{' and depth == 0:
                brace = j
                break
            j += 1
        if brace < 0:
            continue
        end = match_brace(src, brace)
        out.append((name, src[m.start():brace].strip(), src[brace:end + 1]))
        pos = end
    return out


def map_type(t):
    t = re.sub(r'\b(const|static|extern|volatile|register|unsigned|signed|struct|enum)\b', '', t).strip()
    return TYPE_MAP.get(t, t)


def safe_ident(n):
    return n + '_' if n in CS_KEYWORDS else n


class Func:
    """A C function in the middle of translation."""

    def __init__(self, name, sig, body):
        self.name, self.body_c = name, body
        self.ret, self.params, self.ok = 'void', [], False   # params: [{mod,type,name}]
        self.notes = []
        sig = re.sub(r'\s+', ' ', sig)
        m = re.match(r'^(.*?)\b([A-Za-z_]\w*)\s*\((.*)\)\s*$', sig)
        if not m:
            return
        ret, _, ptext = m.group(1), m.group(2), m.group(3)
        self.ret = map_type(ret.replace('*', ''))
        if '*' in ret and self.ret not in PRIMS + ('char',):
            self.ret += '?'
        if ptext.strip() not in ('', 'void'):
            for p in split_params(ptext):
                if p == '...':
                    self.params.append({'mod': 'params', 'type': 'object?[]', 'name': 'args'})
                    continue
                arr = re.search(r'\[[^\]]*\]\s*$', p)
                is_ptr = '*' in p
                base = re.sub(r'\[[^\]]*\]\s*$', '', p).replace('*', ' ').strip()
                pm = re.match(r'^(.*?)\s*([A-Za-z_]\w*)$', base)
                if not pm:
                    self.params.append({'mod': '', 'type': f'/* {p} */', 'name': ''})
                    continue
                t, n = map_type(pm.group(1)), safe_ident(pm.group(2))
                mod = ''
                if arr:
                    t += '[]'
                elif is_ptr and t in PRIMS:
                    mod = 'ref'
                    self.notes.append(f'{n}: pointer-to-primitive -> ref (use out if only written)')
                elif is_ptr and t == 'char':
                    t = 'string'
                elif is_ptr:
                    t += '?'
                self.params.append({'mod': mod, 'type': t, 'name': n})
        self.ok = True

    def render_sig(self):
        ps = ', '.join(f"{(p['mod'] + ' ') if p['mod'] else ''}{p['type']} {p['name']}".strip() for p in self.params)
        return f'public static {self.ret} {self.name}({ps})'


# ----------------------------------------------------------------------------
# Body translation (token level)
# ----------------------------------------------------------------------------
# Build configurations that are off in the port: their #ifdef branches are commented out, the #else branches kept.
DISABLED_DEFINES = {'JA2TESTVERSION', 'JA2DEMO', 'JA2BETAVERSION', 'JA2EDITOR', 'SGP_VIDEO_DEBUGGING',
                    'JA2BETAVERSION_ZZZ', 'JA2DEMOADS', 'GERMAN', 'RUSSIAN', 'DUTCH', 'FRENCH', 'ITALIAN',
                    'POLISH', 'TAIWAN', 'CHINESE', 'DEBUG_ZZZ'}


def preprocess_ifdefs(body):
    """Resolve #if/#ifdef/#ifndef/#else/#endif lines. Unknown conditions keep the true branch."""
    out = []
    stack = []      # each: [parent_active, branch_active, condition_known_false_for_true_branch]
    active = True
    for line in body.split('\n'):
        t = line.strip()
        m = re.match(r'#\s*(ifdef|ifndef|if|elif|else|endif)\b(.*)', t)
        if not m:
            out.append(line if active else ('// ' + line.lstrip() if line.strip() else line))
            continue
        kw, rest = m.group(1), m.group(2)
        names = set(re.findall(r'[A-Za-z_]\w+', rest))
        off = bool(names & DISABLED_DEFINES)
        if kw in ('ifdef', 'if'):
            # "#if defined(X) || defined(Y)": off only if every named define is disabled
            cond = not off if kw == 'ifdef' else not (names - {'defined'} <= DISABLED_DEFINES and names - {'defined'})
            stack.append((active, cond))
            active = active and cond
        elif kw == 'ifndef':
            # "#ifndef X": X is never defined for the port's build, so the true branch is active
            cond = True
            stack.append((active, cond))
            active = active and cond
        elif kw == 'else':
            if stack:
                parent, cond = stack.pop()
                new = not cond
                stack.append((parent, new))
                active = parent and new
        elif kw == 'elif':
            if stack:
                parent, cond = stack.pop()
                stack.append((parent, True))
                active = parent and not cond
        elif kw == 'endif':
            if stack:
                parent, _ = stack.pop()
                active = parent
        out.append('// ' + t)
    return '\n'.join(out)


def translate_tokens(body, repo, aliases):
    body = preprocess_ifdefs(body)
    param_like = set()
    out = []
    for line in body.split('\n'):
        code, sep, comment = line.partition('//')
        code = re.sub(r'^(\s*)([A-Z][A-Z0-9_]+)\s*\*+\s*(\w+)', lambda m: (
            f'{m.group(1)}{map_type(m.group(2))} {m.group(3)}' if map_type(m.group(2)) in PRIMS
            else f'{m.group(1)}{m.group(2)}? {m.group(3)}'), code)
        code = code.replace('->', '.')
        # wide string / char literals: L"abc" -> "abc"
        code = re.sub(r'\bL(?=["\'])', '', code)
        # swprintf( buf, fmt, ... ) -> buf = wprintf( fmt, ... )   (wprintf is the port's string.Format helper)
        code = re.sub(r'\b(?:swprintf|sprintf)\(\s*(\w+)\s*,\s*', r'\1 = wprintf( ', code)
        # local arrays: `int a[N];` -> `int[] a = new int[N];`  (2-D: `int[,] a = new int[N, M];`)
        code = re.sub(r'^(\s*)([A-Za-z_]\w*\??)\s+(\w+)((?:\s*\[[^\]]*\])+)\s*;', lambda m: (
            lambda dims: f"{m.group(1)}{map_type(m.group(2))}[{',' * (len(dims) - 1)}] {m.group(3)} = new {map_type(m.group(2))}[{', '.join(dims)}];"
        )(re.findall(r'\[([^\]]*)\]', m.group(4))), code)

        def tok(m):
            w = m.group(0)
            if w in aliases:
                return aliases[w]
            if w in TOKEN_MAP:
                return TOKEN_MAP[w]
            if w in repo.enum_tokens:
                return repo.enum_tokens[w]
            if re.fullmatch(r'[A-Z][A-Z0-9_]+', w):
                e = repo.unique_member_enum(w)
                if e:
                    return f'{e}.{w}'
            return w
        # do not touch member names after a dot
        code = re.sub(r'(?<![\w.])\b[A-Za-z_]\w*\b', tok, code)
        code = re.sub(r'\bunsigned\s+(int|short|long|char)\b', 'int', code)
        out.append(code + (sep + comment if sep else ''))
    return '\n'.join(out)


# ----------------------------------------------------------------------------
# Type inference
# ----------------------------------------------------------------------------
class TypeInfer:
    def __init__(self, repo):
        self.repo = repo
        self.env = {}          # symbol -> current C# type (only symbols that start as 'int')
        self.vars = {}         # symbol -> declared class type (parameters / locals such as SOLDIERTYPE pSoldier)

    # type of an expression chain; `collect` receives (symbol, enum) constraints found on the way
    def chain_type(self, text, collect):
        m = re.match(r'\s*(' + IDENT + ')', text)
        if not m:
            return None
        name, pos = m.group(1), m.end()
        steps = []
        for s in STEP_RE.finditer(text, pos):
            if s.start() != pos:
                break
            steps.append(s)
            pos = s.end()
        t, i = None, 0
        if steps and steps[0].group('c') is not None:
            t = self.call(name, steps[0].group('c'), collect)
            i = 1
        elif name in self.repo.enums and steps and steps[0].group('f'):
            t, i = name, 1          # Enum.Member
        else:
            t = self.env.get(name) or self.vars.get(name) or self.repo.const_types.get(name)
        for s in steps[i:]:
            if s.group('f'):
                f = s.group('f')
                owner = norm_type(t) if t else None
                t = self.repo.owner_fields.get((owner, f)) or self.repo.field_type(f)
            elif s.group('i') is not None:
                idx = s.group('i').strip()
                if t:
                    key = Repo.key_type(t)
                    if key and self.repo.is_enum(key) and collect is not None and self.env.get(idx) == 'int':
                        collect.append((idx, norm_type(key)))
                    t = Repo.elem_type(t)
                self.scan(idx, collect)
            else:
                t = self.call(None, s.group('c'), collect)
        return t

    def call(self, name, args, collect):
        parts = split_params(args) if args.strip() else []
        sig = self.repo.sig(name, len(parts)) if name else None
        for k, a in enumerate(parts):
            a = a.strip()
            if sig and collect is not None and self.env.get(a) == 'int' and self.repo.is_enum(sig[1][k]):
                collect.append((a, norm_type(sig[1][k])))
            self.scan(a, collect)
        return sig[0] if sig else None

    def scan(self, text, collect):
        for m in CHAIN_RE.finditer(text):
            self.chain_type(m.group(0), collect)

    def is_sym(self, e):
        e = e.strip()
        return e if self.env.get(e) == 'int' else None

    def run(self, body, ret_holder, syms):
        """Infer enum types for the int-typed symbols in `syms`; returns True if anything changed."""
        collect = []
        for s in syms:
            self.env.setdefault(s, 'int')
        # relational / mask / assignment pairs
        for m in PAIR_RE.finditer(body):
            a, b = m.group('a'), m.group('b')
            ta, tb = self.chain_type(a, collect), self.chain_type(b, collect)
            sa, sb = self.is_sym(a), self.is_sym(b)
            if sa and tb and self.repo.is_enum(tb):
                collect.append((sa, norm_type(tb)))
            if sb and ta and self.repo.is_enum(ta):
                collect.append((sb, norm_type(ta)))
        # everything else (call arguments, indexers) is picked up by a plain scan
        self.scan(body, collect)
        # return statements: remember what each one returns; decided once the symbols converge
        ret_holder['exprs'] = []
        for m in RET_RE.finditer(body):
            e = m.group('e')
            ret_holder['exprs'].append((e, self.chain_type(e, collect)))
            if ret_holder['t'] != 'int' and self.is_sym(e) and self.repo.is_enum(ret_holder['t']):
                collect.append((self.is_sym(e), norm_type(ret_holder['t'])))
        changed = False
        for sym, t in collect:
            if self.env.get(sym) == 'int':
                self.env[sym] = t
                changed = True
        return changed


CLASS_DECL_RE = re.compile(r'^\s*([A-Z]\w*)\??\s+(\w+)\s*(?:[=;,])', re.M)


def infer_function(fn, body_cs, repo, locals_):
    """Promote int params / locals / return value to enums. Mutates fn; returns the type env."""
    ti = TypeInfer(repo)
    for p in fn.params:
        if p['type'] not in PRIMS and p['name']:
            ti.vars[p['name']] = norm_type(p['type']).rstrip('[]')
    for m in CLASS_DECL_RE.finditer(body_cs):
        if m.group(1) in repo.type_names:
            ti.vars.setdefault(m.group(2), m.group(1))
    ret = {'t': fn.ret}
    syms = [p['name'] for p in fn.params if p['type'] == 'int'] + list(locals_)
    for _ in range(6):
        if not ti.run(body_cs, ret, syms):
            break
    # decide the return type: an int symbol that stays int means the function really returns an
    # index/number (sentinel constants such as ITEM_NOT_FOUND are then just -1); otherwise the
    # most common enum wins, ties going to the first return statement.
    if ret['t'] == 'int':
        kinds = []
        for e, te in ret.get('exprs', []):
            ev = ti.env.get(e.strip())
            kinds.append(('int', ev) if ev == 'int' else ('enum', norm_type(te)) if te and repo.is_enum(te) else ('other', te))
        enums = [k[1] for k in kinds if k[0] == 'enum']
        has_int_sym = any(k[0] == 'int' for k in kinds)
        if enums and not has_int_sym:
            best = collections.Counter(enums)
            top = max(best.values())
            ret['t'] = next(e for e in enums if best[e] == top)
        if enums:
            others = sorted({e for e in enums if e != ret['t']})
            if others:
                ret['conflicts'] = set(others)
    # last resort for parameters: consistent naming in already-ported code
    for p in fn.params:
        if p['type'] == 'int' and ti.env.get(p['name']) == 'int':
            e = repo.name_prior(p['name'])
            if e:
                ti.env[p['name']] = e
                fn.notes.append(f"{p['name']}: type {e} guessed from the name")
    for p in fn.params:
        if p['type'] == 'int' and ti.env.get(p['name'], 'int') != 'int':
            p['type'] = ti.env[p['name']]
    if ret['t'] != fn.ret:
        fn.ret = ret['t']
    if ret.get('conflicts'):
        fn.notes.append(f"return type {fn.ret} also returns {', '.join(sorted(ret['conflicts']))}")
    return {**ti.vars, **ti.env}   # class-typed symbols too, for the later HasFlag pass


# ----------------------------------------------------------------------------
# Post passes on the translated body
# ----------------------------------------------------------------------------
DECL_RE = re.compile(r'^(\s*)int\s+([^;()]+);', re.M)


def local_names(body):
    names = []
    for m in DECL_RE.finditer(body):
        for part in split_params(m.group(2)):
            n = re.match(r'\s*\*?\s*(\w+)', part)
            if n:
                names.append(n.group(1))
    return names


def retype_locals(body, env):
    def sub(m):
        indent, rest = m.group(1), m.group(2)
        parts = split_params(rest)
        groups = collections.OrderedDict()
        for part in parts:
            n = re.match(r'\s*\*?\s*(\w+)', part)
            t = env.get(n.group(1), 'int') if n else 'int'
            groups.setdefault(t, []).append(part.strip())
        if list(groups) == ['int']:
            return m.group(0)
        return indent + ' '.join(f'{t} {", ".join(ps)};' for t, ps in groups.items())
    return DECL_RE.sub(sub, body)


def resolve_addr_of(code, repo):
    """Turn `Foo(&x, &y)` into `Foo(out x, ref y)` using the callee's C# parameter modifiers."""
    spans = []
    for m in re.finditer(r'\b(' + IDENT + r')\s*\(', code):
        name = m.group(1)
        if name not in repo.methods:
            continue
        i, depth = m.end(), 1
        while i < len(code) and depth:
            depth += {'(': 1, ')': -1}.get(code[i], 0)
            i += 1
        spans.append((name, m.end(), i - 1))
    for name, a, b in sorted(spans, key=lambda t: -t[1]):
        args = split_params(code[a:b])
        if not any(x.lstrip().startswith('&') and not x.lstrip().startswith('&&') for x in args):
            continue
        overloads = [o for o in repo.methods[name] if len(o) == len(args)]
        if not overloads:
            continue
        new_args = []
        for arg, mod in zip(args, overloads[0]):
            body = arg.lstrip()
            if body.startswith('&') and not body.startswith('&&'):
                body = (mod + ' ' if mod else '') + body[1:].lstrip()
            new_args.append(body)
        code = code[:a] + ' ' + ', '.join(new_args) + ' ' + code[b:]
    return code


OPER = r'(?:' + CHAIN + r'|\((?:[^()]|\([^()]*\))*\))'
MASK_RE = re.compile(r'(?P<a>' + CHAIN + r')\s*&(?!&)\s*(?P<b>' + OPER + r')')
ZERO_CMP_RE = re.compile(r'\)\s*(?P<op>==|!=)\s*0\b')
NOT_BOOL_AFTER = re.compile(r'^(==|!=|<=|>=|<|>|\+|-|\*|/|%|&|\||\^|<<|>>)')


def flags_to_hasflag(body, repo, env):
    """Rewrite C flag tests on [Flags] enums.

    (a & FLAG)            -> a.HasFlag(FLAG)
    (a & FLAG) != 0       -> a.HasFlag(FLAG)       (== 0 -> !a.HasFlag(FLAG))
    (a & (F1 | F2))       -> (a & (F1 | F2)) != 0   (C's & is "any bit", HasFlag is "all bits")
    The same "any bit" form is used when FLAG itself has several bits set.
    """
    ti = TypeInfer(repo)
    ti.env = dict(env)

    def mask_type(b):
        b = b.strip()
        if b.startswith('('):
            m = re.search(r'\b(' + IDENT + r')\.' + IDENT, b)
            return m.group(1) if m and m.group(1) in repo.enums else None
        return ti.chain_type(b, None)

    def operands(a, b):
        ta, tb = ti.chain_type(a, None), mask_type(b)
        if tb and repo.is_flags(tb):
            return a, b
        if ta and repo.is_flags(ta) and not b.strip().startswith('('):
            return b, a
        return None

    def in_call_args(text, paren_pos):
        before = text[:paren_pos].rstrip()
        w = re.search(r'(\w+)$', before)
        return bool(w) and w.group(1) not in ('if', 'while', 'return', 'switch')

    edits = []
    for m in MASK_RE.finditer(body):
        before, after = body[:m.start()].rstrip(), body[m.end():].lstrip()
        wrapped = before.endswith('(') and after.startswith(')')
        if wrapped and in_call_args(body, len(before) - 1):
            continue
        ops = operands(m.group('a'), m.group('b'))
        if not ops:
            continue
        lhs, rhs = ops
        rm = re.fullmatch(r'(' + IDENT + r')\.(' + IDENT + r')', rhs.strip())
        # C's & means "any bit"; HasFlag means "all bits". They only agree for single-bit flags,
        # so parenthesised / combined / multi-bit masks keep the explicit `(a & mask) != 0` form.
        any_bit = rhs.strip().startswith('(') or bool(rm and (rm.group(1), rm.group(2)) in repo.composite)
        if wrapped:
            z = ZERO_CMP_RE.match(after)
            if z and not any_bit:
                start = len(before) - 1
                end = m.end() + (len(body[m.end():]) - len(after)) + z.end()
                edits.append((start, end, ('!' if z.group('op') == '==' else '') + f'{lhs.strip()}.HasFlag({rhs.strip()})'))
                continue
            if z:
                continue            # already `(a & mask) != 0`
            rest = after[1:].lstrip()
            if not rest.startswith(('&&', '||')) and NOT_BOOL_AFTER.match(rest):
                continue            # `(a & mask) == VALUE` and friends are numeric comparisons, leave them
        else:
            has_logic = after.startswith(('&&', '||'))
            ctx_ok = (before == '' or before.endswith(('(', '!', '&&', '||', '?')) or bool(re.search(r'\b(if|while)$', before)))
            after_ok = after == '' or after[0] in ');?' or has_logic
            if not ctx_ok or not after_ok or (not has_logic and NOT_BOOL_AFTER.match(after)):
                continue
        expr = f'({lhs.strip()} & {rhs.strip()}) != 0' if any_bit else f'{lhs.strip()}.HasFlag({rhs.strip()})'
        edits.append((m.start(), m.end(), expr))
    for start, end, text in sorted(edits, reverse=True):
        body = body[:start] + text + body[end:]
    return body


STATIC_LOCAL_RE = re.compile(r'^(?P<ind>[ \t]*)static[ \t]+(?P<type>[\w<>\[\]\?]+)[ \t]+(?P<name>\w+)[ \t]*(?P<init>=[^;\n]+)?;[ \t]*$', re.M)


def hoist_statics(fn_name, body):
    """C function-local `static T x = v;` -> class field `FnName_x` (C# has no static locals)."""
    fields = []

    def take(m):
        new = f'{fn_name}_{m.group("name")}'
        fields.append((m.group('type'), new, (m.group('init') or '').strip(), m.group('name')))
        return ''
    body = STATIC_LOCAL_RE.sub(take, body)
    for _, new, _, old in fields:
        body = re.sub(r'(?<![\w.])' + re.escape(old) + r'\b', new, body)
    return body, fields


# ----------------------------------------------------------------------------
# Qualifying calls that live in other classes
# ----------------------------------------------------------------------------
# Static (C global function) -> owning class in the port. Extend per project in the map json: "qualify": {"Class": ["Fn", ...]}
DEFAULT_QUALIFY = {
    'FontSubSystem': ['SetFont', 'SetFontForeground', 'SetFontBackground', 'SetFontShadow', 'SetFontDestBuffer',
                      'FindFontCenterCoordinates'],
    'ButtonSubSystem': ['RemoveButton', 'UnloadButtonImage', 'QuickCreateButton', 'LoadButtonImage', 'RenderButtons',
                        'SetButtonCursor'],
}
# Prefix rules: (regex matching a call, replacement). `NB` keeps us from touching member access / longer names.
NB = r'(?<![\w.])'
DEFAULT_CALL_RULES = [
    # video manager (an instance field named `video` in the port)
    (NB + r'InvalidateRegion\(', 'video.InvalidateRegion('),
    (NB + r'BltVideoObject\(\s*FRAME_BUFFER\b', 'video.BltVideoObject(SurfaceType.FRAME_BUFFER'),
    (NB + r'BltVideoObject\(\s*SurfaceType', 'video.BltVideoObject(SurfaceType'),
    (NB + r'DeleteVideoObjectFromIndex\(', 'video.DeleteVideoObject('),
    # GetVideoObject( &h, id ) / GetVideoObject( out h, id )  ->  h = id
    (NB + r'GetVideoObject\(\s*(?:&|out\s+)?(\w+)\s*,\s*([\w.\[\]]+)\s*\)', r'\1 = \2'),
    # mouse system
    # button user data lives on ButtonSubSystem, everything else MSYS_ is the mouse system
    (NB + r'(MSYS_(?:Set|Get)BtnUserData)\(', r'ButtonSubSystem.\1('),
    (NB + r'(MSYS_(?!MOUSE_IN_AREA\b|SetBtnUserData\b|GetBtnUserData\b)\w+)\(', r'MouseSubSystem.\1('),
    (NB + r'MSYS_MOUSE_IN_AREA\b', 'MouseRegionFlags.IN_AREA'),
    (r'MouseRegionFlags\.MOUSE_IN_AREA\b', 'MouseRegionFlags.IN_AREA'),
    # files
    (NB + r'(FileExists|FileDelete|FileClearAttributes)\(', r'files.\1('),
    # C library / macros
    (NB + r'Assert\(', 'System.Diagnostics.Debug.Assert('),
    (NB + r'min\(', 'Math.Min('),
    (NB + r'max\(', 'Math.Max('),
    (NB + r'GetJA2Clock\(\s*\)', '(int)GetJA2Clock()'),
]


# Owners that are instance services in the port: call them through the injected field instead of the type name.
INSTANCE_OWNERS = {'VeldridVideoManager': 'video', 'SDL2VideoManager': 'video', 'VideoManager': 'video'}


def load_call_rules(cmap):
    """Built-in rules + project rules from the map json: "call_rules": [[regex, replacement], ...]."""
    rules = [(re.compile(rx), rep) for rx, rep in DEFAULT_CALL_RULES]
    rules += [(re.compile(rx), rep) for rx, rep in cmap.get('call_rules', [])]
    return rules


# ----------------------------------------------------------------------------
# Text drawing / encrypted file calls: the C argument lists differ from the port's overloads
# ----------------------------------------------------------------------------
JUSTIFY_RX = re.compile(r'(?<![\w.])(LEFT_JUSTIFIED|CENTER_JUSTIFIED|RIGHT_JUSTIFIED|TEXT_SHADOWED|INVALIDATE_TEXT|DONT_DISPLAY_TEXT)\b')


def split_call_args(text, open_idx):
    """text[open_idx] is '(' : return (args, index after the matching ')') with top-level commas split."""
    depth, args, cur, i = 0, [], [], open_idx
    in_str = False
    while i < len(text):
        c = text[i]
        if in_str:
            cur.append(c)
            if c == '\\':
                cur.append(text[i + 1])
                i += 1
            elif c == '"':
                in_str = False
        elif c == '"':
            in_str = True
            cur.append(c)
        elif c in '([{':
            depth += 1
            if depth > 1:
                cur.append(c)
        elif c in ')]}':
            depth -= 1
            if depth == 0:
                args.append(''.join(cur).strip())
                return args, i + 1
            cur.append(c)
        elif c == ',' and depth == 1:
            args.append(''.join(cur).strip())
            cur = []
        else:
            cur.append(c)
        i += 1
    return None, open_idx


def rewrite_text_calls(body):
    """DrawTextToScreen / DisplayWrappedString / LoadEncryptedDataFromFile: C argument order -> the port's overloads."""
    # string buffers read through LoadEncryptedDataFromFile become strings: `char[] s = new char[N];` -> `string s = string.Empty;`
    buffers = set(re.findall(r'\bchar\[\]\s+(\w+)\s*=\s*new char\[[^\]]*\];', body))
    body = re.sub(r'\bchar\[\]\s+(\w+)\s*=\s*new char\[[^\]]*\];', r'string \1 = string.Empty;', body)

    def justify(a):
        return JUSTIFY_RX.sub(r'TextJustifies.\1', a)

    out, i = [], 0
    call = re.compile(r'(?<![\w.])(DrawTextToScreen|DisplayWrappedString|LoadEncryptedDataFromFile|CreateIconAndTextButton|QuickCreateButton)\s*\(')
    while True:
        m = call.search(body, i)
        if not m:
            out.append(body[i:])
            break
        args, end = split_call_args(body, m.end() - 1)
        name = m.group(1)
        new = None
        if args is not None:
            if name == 'DrawTextToScreen' and len(args) == 9:
                new = (f'FontSubSystem.DrawTextToScreen({args[0]}, new({args[1]}, {args[2]}), {args[3]}, '
                       f'{args[4]}, {args[5]}, {args[6]}, {justify(args[8])})')
            elif name == 'DisplayWrappedString' and len(args) == 10:
                new = (f'FontSubSystem.DisplayWrappedString(new({args[0]}, {args[1]}), {args[2]}, {args[3]}, '
                       f'{args[4]}, {args[5]}, {args[6]}, {args[7]}, {justify(args[9])})')
            elif name == 'CreateIconAndTextButton' and len(args) == 14:
                # (image, text, font, fore, shadow, foreDown, shadowDown, justify, x, y, flags, priority, move, click)
                new = 'ButtonSubSystem.CreateIconAndTextButton(' + ', '.join(
                    args[:8] + [f'new Point({args[8]}, {args[9]})'] + args[10:]) + ')'
            elif name == 'QuickCreateButton' and len(args) == 7:
                # (image, x, y, flags, priority, move, click)
                new = 'ButtonSubSystem.QuickCreateButton(' + ', '.join(
                    [args[0], f'new Point({args[1]}, {args[2]})'] + args[3:]) + ')'
            elif name == 'LoadEncryptedDataFromFile' and len(args) == 4 and args[1] in buffers:
                new = f'files.LoadEncryptedDataFromFile({args[0]}, out {args[1]}, {args[2]}, {args[3]})'
        out.append(body[i:m.start()])
        if new:
            out.append(new)
            i = end
        else:
            out.append(body[m.start():m.end()])
            i = m.end()
    return ''.join(out)


def qualify_calls(body, repo, cls, batch_names, cmap):
    """Prefix calls to functions that are defined in some other class of the port with that class's name.

    Order: explicit regex rules, then the class tables, then (best effort) the owning class found by scanning the repo.
    Calls to functions of the file being ported, and names the port keeps in Globals, are left alone.
    """
    rules = load_call_rules(cmap)
    table = {}
    for owner, names in list(DEFAULT_QUALIFY.items()) + list(cmap.get('qualify', {}).items()):
        for n in names:
            table[n] = owner
    skip_classes = {cls, 'Globals', 'EnglishText'}
    call_rx = re.compile(NB + r'([A-Za-z_]\w*)\s*\(')

    def owner_of(name):
        if name in table:
            return table[name]
        if name in batch_names or name not in repo.method_class:
            return None
        owner, n = repo.method_class[name].most_common(1)[0]
        total = sum(repo.method_class[name].values())
        if owner in skip_classes or cls in repo.method_class[name] or n * 2 <= total:
            return None
        return owner

    def fix_line(code):
        for rx, rep in rules:
            code = rx.sub(rep, code)

        def sub(m):
            name = m.group(1)
            if name in CS_KEYWORDS or name in ('if', 'while', 'for', 'switch', 'return', 'sizeof', 'nameof', 'typeof'):
                return m.group(0)
            owner = owner_of(name)
            return f'{INSTANCE_OWNERS.get(owner, owner)}.{m.group(0)}' if owner else m.group(0)
        return call_rx.sub(sub, code)

    out = []
    for line in body.split('\n'):
        code, sep, comment = line.partition('//')
        out.append(fix_line(code) + sep + comment)
    return '\n'.join(out)


def add_todo_markers(text):
    final = []
    for line in text.split('\n'):
        code = line.partition('//')[0]
        todo = sorted({label for rx, label in TODO_PATTERNS if rx.search(code)})
        final.append(line + (f'  // TODO(port): {", ".join(todo)}' if todo else ''))
    return '\n'.join(final)


# ----------------------------------------------------------------------------
# Driver
# ----------------------------------------------------------------------------
def load_map(path):
    if path and os.path.exists(path):
        with open(path, encoding='utf-8') as f:
            return json.load(f)
    return {}


def class_name(cfile, repo, fn_names, cmap):
    base = os.path.basename(cfile)
    if base in cmap.get('classes', {}):
        return cmap['classes'][base], True
    existing = repo.target_class(fn_names)
    if existing:
        return existing, True
    name = re.sub(r'[^A-Za-z0-9]+', '', os.path.splitext(base)[0])
    name = name if name and not name[0].isdigit() else '_' + name
    if name in repo.type_names:
        name += 'SubSystem'      # e.g. Items.c would clash with the Items enum
    return name, False


def port_file(cfile, repo_root, outdir, include_all, map_path):
    repo = Repo(repo_root)
    cmap = load_map(map_path)
    aliases = cmap.get('aliases', {})
    with open(cfile, errors='ignore') as f:
        src = f.read()
    fns = [Func(n, s, b) for n, s, b in extract_functions(src)]
    # register all signatures first (so calls between functions of this batch resolve),
    # then infer twice so that return types discovered late reach earlier callers
    bodies = {}
    static_fields = {}
    for fn in fns:
        if not fn.ok:
            continue
        cs = translate_tokens(fn.body_c, repo, aliases)
        cs, static_fields[fn.name] = hoist_statics(fn.name, cs)
        bodies[fn.name] = cs
        repo.sigs[fn.name].append((fn.ret, [p['type'] for p in fn.params]))
        repo.methods.setdefault(fn.name, []).append([p['mod'] for p in fn.params])
    envs = {}
    for _ in range(2):
        for fn in fns:
            if fn.ok and fn.name in bodies:
                fn.notes = [n for n in fn.notes if 'pointer-to-primitive' in n]
                envs[fn.name] = infer_function(fn, bodies[fn.name], repo, local_names(bodies[fn.name]))
                repo.sigs[fn.name][-1] = (fn.ret, [p['type'] for p in fn.params])

    cls, existing = class_name(cfile, repo, [f.name for f in fns], cmap)
    chunks, skipped, todo_total, promoted = [], 0, 0, 0
    for fn in fns:
        if fn.name in repo.ids and not include_all:
            skipped += 1
            continue
        if not fn.ok:
            chunks.append(f'    // TODO(port): could not parse signature of {fn.name}\n')
            continue
        env = envs.get(fn.name, {})
        body = retype_locals(bodies[fn.name], env)
        body = resolve_addr_of(body, repo)
        body = flags_to_hasflag(body, repo, env)
        body = rewrite_text_calls(body)
        body = qualify_calls(body, repo, cls, {f.name for f in fns}, cmap)
        body = add_todo_markers(body)
        todo_total += body.count('TODO(port)')
        promoted += sum(1 for p in fn.params if repo.is_enum(p['type'])) + (1 if repo.is_enum(fn.ret) else 0)
        ind = '\n'.join('    ' + l if l.strip() else l for l in body.split('\n'))
        note_txt = ''.join(f'    // note: {n}\n' for n in fn.notes)
        field_txt = ''.join(
            f'    // was a function-local static of {fn.name}\n    private static {t} {nm}{(" " + init) if init else ""};\n'
            for t, nm, init, _ in static_fields.get(fn.name, []))
        chunks.append(f'{field_txt}{note_txt}    {fn.render_sig()}\n{ind}\n')

    text = (
        '// Generated by tools/c2cs.py from ' + os.path.basename(cfile) + ' - first pass, will not compile as-is.\n'
        f'// {len(chunks)} functions emitted, {skipped} skipped (already present), {todo_total} TODO(port) markers,\n'
        f'// {promoted} enum-typed parameters/returns inferred.\n'
        + (f'// Paste these members into the existing class {cls}.\n' if existing else '')
        + 'using System;\nusing System.Collections.Generic;\nusing static SharpAlliance.Core.Globals;\n\n'
        'namespace SharpAlliance.Core.SubSystems;\n\n'
        f'public class {cls}\n{{\n' + '\n'.join(chunks) + '}\n'
    )
    os.makedirs(outdir, exist_ok=True)
    path = os.path.join(outdir, re.sub(r'[^A-Za-z0-9]+', '', os.path.splitext(os.path.basename(cfile))[0]) + '.cs')
    with open(path, 'w', encoding='utf-8') as f:
        f.write(text)
    print(f'{path}: {len(chunks)} functions, {skipped} already ported, {todo_total} TODO markers, '
          f'{promoted} enum-typed signature slots, class {cls}{" (existing)" if existing else ""}')


def queue(srcroot, repo_root):
    repo = Repo(repo_root)
    rows = []
    for root, _, files in os.walk(srcroot):
        for f in files:
            if not f.endswith('.c'):
                continue
            p = os.path.join(root, f)
            rel = os.path.relpath(p, srcroot).replace('\\', '/')
            if 'Editor/' in rel or re.search(r'_(Dutch|French|German|Italian|Polish|Russian|Taiwanese)Text\.c$', rel):
                continue
            s = open(p, errors='ignore').read()
            names = {n for n, _, _ in extract_functions(s)}
            missing = sorted(n for n in names if n not in repo.ids)
            if names:
                rows.append((rel, s.count('\n'), len(names), len(missing)))
    rows.sort(key=lambda r: -r[3])
    print('# Port work queue (functions whose names do not yet appear in the C# code)\n')
    print('| Missing | Total | C lines | File |\n|---:|---:|---:|---|')
    for rel, n, t, m in rows:
        if m:
            print(f'| {m} | {t} | {n} | {rel} |')
    print(f'\nTotals: {sum(r[3] for r in rows)} missing of {sum(r[2] for r in rows)} functions')


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest='cmd', required=True)
    q = sub.add_parser('queue')
    q.add_argument('srcroot')
    q.add_argument('--repo', default='.')
    p = sub.add_parser('port')
    p.add_argument('cfile')
    p.add_argument('--out', required=True)
    p.add_argument('--repo', default='.')
    p.add_argument('--all', action='store_true', help='also emit functions already present in the C#')
    p.add_argument('--map', default=os.path.join(os.path.dirname(os.path.abspath(__file__)), 'c2cs.map.json'))
    a = ap.parse_args()
    if a.cmd == 'queue':
        queue(a.srcroot, a.repo)
    else:
        port_file(a.cfile, a.repo, a.out, a.all, a.map)


if __name__ == '__main__':
    sys.exit(main())
