#!/usr/bin/env python3
"""
gen_ja2_sln.py - generate a modern Visual Studio solution for the original JA2 C source.

The original ships VC6 workspaces (JA2.dsw / *.dsp) that current Visual Studio cannot open.
This reads them and writes JA2.sln plus a .vcxproj next to each .dsp, using the include
paths, defines and file lists from the originals.

The result is meant for browsing / IntelliSense / Find in Files. It will NOT build as-is:
the original needs the 1999 DirectX SDK, Smacker (smackw32.lib) and Miles (mss32.lib).

Usage: python tools/gen_ja2_sln.py <path-to-jaggedalliance2-clone>
"""
import os
import re
import sys
import uuid
from xml.sax.saxutils import escape

PROJECTS = [  # (name, dsp relative to root)
    ('Standard Gaming Platform', 'Standard Gaming Platform/Standard Gaming Platform.dsp'),
    ('Utils', 'ja2/Build/Utils/Utils.dsp'),
    ('TileEngine', 'ja2/Build/TileEngine/TileEngine.dsp'),
    ('TacticalAI', 'ja2/Build/TacticalAI/TacticalAI.dsp'),
    ('Tactical', 'ja2/Build/Tactical/Tactical.dsp'),
    ('Strategic', 'ja2/Build/Strategic/Strategic.dsp'),
    ('Laptop', 'ja2/Build/Laptop/Laptop.dsp'),
    ('Editor', 'ja2/Build/Editor/Editor.dsp'),
    ('ja2', 'ja2/Build/JA2.dsp'),
]
CPP_EXT = {'.c', '.cpp', '.cc', '.cxx'}
H_EXT = {'.h', '.hpp'}


def guid(name):
    return '{' + str(uuid.uuid5(uuid.NAMESPACE_URL, 'ja2/' + name)).upper() + '}'


def config_block(text, proj, kind):
    """Return the text of the `<proj> - Win32 <kind>` block in a .dsp."""
    m = re.search(r'^!(?:IF|ELSEIF)\s+"\$\(CFG\)"\s*==\s*"%s - Win32 %s"\s*$' % (re.escape(proj), kind), text, re.M | re.I)
    if not m:
        return ''
    n = re.search(r'^!(?:ELSEIF|ENDIF)', text[m.end():], re.M)
    return text[m.end(): m.end() + (n.start() if n else len(text))]


def cpp_options(block):
    lines = [l for l in block.split('\n') if l.startswith('# ADD CPP')]
    if not lines:
        return [], []
    opt = lines[-1]
    inc = re.findall(r'/I\s+"([^"]+)"', opt)
    dfn = re.findall(r'/D\s+"([^"]+)"', opt)
    return inc, dfn


def fix_include(p, solrel_from_proj):
    # .dsp include dirs are rooted at the drive ("\ja2\Build\..."): re-root at the solution dir.
    p = p.replace('/', '\\')
    if p.startswith('\\'):
        return '$(JA2Root)' + p.lstrip('\\')
    return p


def sources(dsp_text, proj_dir):
    files = []
    for m in re.finditer(r'^SOURCE=("?)(.+?)\1\s*$', dsp_text, re.M):
        p = m.group(2).replace('\\', '/')
        if p.startswith('./'):
            p = p[2:]
        files.append(p)
    return files


def main():
    root = os.path.abspath(sys.argv[1])
    sol_projects = []
    for name, dsp_rel in PROJECTS:
        dsp = os.path.join(root, dsp_rel)
        pdir = os.path.dirname(dsp)
        text = open(dsp, errors='ignore').read()
        is_exe = 'Application' in re.search(r'TARGTYPE "([^"]+)"', text).group(1)
        rel_dbg = config_block(text, name, 'Debug')
        rel_rel = config_block(text, name, 'Release')
        inc_r, def_r = cpp_options(rel_rel)
        inc_d, def_d = cpp_options(rel_dbg)
        if not inc_d:
            inc_d, def_d = inc_r, def_r
        inc = [fix_include(i, None) for i in (inc_r or inc_d)]

        cl, hdr, other = [], [], []
        seen = set()

        def add(rel):
            full = os.path.normpath(os.path.join(pdir, rel))
            if not os.path.exists(full):
                # some .dsp files use stale relative paths (..\..\..\Standard Gaming Platform\x.c):
                # fall back to the file name inside the project directory
                full = os.path.join(pdir, os.path.basename(rel))
                if not os.path.exists(full):
                    return
            key = full.lower()
            if key in seen:
                return
            seen.add(key)
            ext = os.path.splitext(full)[1].lower()
            r = os.path.relpath(full, pdir).replace('/', '\\')
            (cl if ext in CPP_EXT else hdr if ext in H_EXT else other).append((r, ext))

        for s in sources(text, pdir):
            add(s)
        # headers are only partly listed in the .dsp, so add every header in the project directory
        for f in sorted(os.listdir(pdir)):
            if os.path.splitext(f)[1].lower() in H_EXT:
                add(f)
        # Standard Gaming Platform keeps its sources beside the .dsp; the others too.

        libs = ''
        if is_exe:
            m = re.search(r'# ADD LINK32 (.+)', rel_rel or text)
            if m:
                libs = ' '.join(t for t in m.group(1).split() if t.lower().endswith('.lib'))
        deps = [n for n, _ in PROJECTS if not is_exe or True] if is_exe else []
        deps = [n for n, _ in PROJECTS if n != name] if is_exe else []

        out = ['<?xml version="1.0" encoding="utf-8"?>',
               '<Project DefaultTargets="Build" ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">',
               '  <ItemGroup Label="ProjectConfigurations">']
        for c in ('Debug', 'Release'):
            out += [f'    <ProjectConfiguration Include="{c}|Win32">',
                    f'      <Configuration>{c}</Configuration>', '      <Platform>Win32</Platform>',
                    '    </ProjectConfiguration>']
        out += ['  </ItemGroup>',
                '  <PropertyGroup Label="Globals">',
                f'    <ProjectGuid>{guid(name)}</ProjectGuid>',
                f'    <RootNamespace>{re.sub(r"[^A-Za-z0-9]", "", name)}</RootNamespace>',
                '    <Keyword>Win32Proj</Keyword>',
                f'    <JA2Root>$(MSBuildThisFileDirectory){os.path.relpath(root, pdir).replace("/", chr(92))}{chr(92)}</JA2Root>',
                '  </PropertyGroup>',
                '  <Import Project="$(VCTargetsPath)\\Microsoft.Cpp.Default.props" />']
        for c in ('Debug', 'Release'):
            out += [f'  <PropertyGroup Condition="\'$(Configuration)|$(Platform)\'==\'{c}|Win32\'" Label="Configuration">',
                    f'    <ConfigurationType>{"Application" if is_exe else "StaticLibrary"}</ConfigurationType>',
                    '    <PlatformToolset>v143</PlatformToolset>',
                    '    <CharacterSet>MultiByte</CharacterSet>',
                    '  </PropertyGroup>']
        out += ['  <Import Project="$(VCTargetsPath)\\Microsoft.Cpp.props" />',
                '  <PropertyGroup>',
                '    <OutDir>$(JA2Root)bin\\$(Configuration)\\</OutDir>',
                f'    <IntDir>$(JA2Root)obj\\{re.sub(r"[^A-Za-z0-9]", "", name)}\\$(Configuration)\\</IntDir>',
                f'    <TargetName>{name if is_exe else re.sub(r"[^A-Za-z0-9]", "", name)}</TargetName>',
                '  </PropertyGroup>']
        for c, dfn in (('Debug', def_d), ('Release', def_r)):
            out += [f'  <ItemDefinitionGroup Condition="\'$(Configuration)|$(Platform)\'==\'{c}|Win32\'">',
                    '    <ClCompile>',
                    '      <WarningLevel>Level3</WarningLevel>',
                    '      <SDLCheck>false</SDLCheck>',
                    '      <ConformanceMode>false</ConformanceMode>',
                    '      <PrecompiledHeader>NotUsing</PrecompiledHeader>',
                    '      <DisableSpecificWarnings>4996;4244;4018;4101;4133;4047;4024</DisableSpecificWarnings>',
                    f'      <AdditionalIncludeDirectories>{escape(";".join(inc))};%(AdditionalIncludeDirectories)</AdditionalIncludeDirectories>',
                    f'      <PreprocessorDefinitions>{escape(";".join(dfn))};%(PreprocessorDefinitions)</PreprocessorDefinitions>',
                    '    </ClCompile>']
            if is_exe:
                out += ['    <Link>', '      <SubSystem>Windows</SubSystem>',
                        f'      <AdditionalDependencies>{libs};%(AdditionalDependencies)</AdditionalDependencies>',
                        '    </Link>']
            out += ['  </ItemDefinitionGroup>']
        out += ['  <ItemGroup>']
        out += [f'    <ClCompile Include="{escape(r)}" />' for r, _ in sorted(cl, key=lambda t: t[0].lower())]
        out += ['  </ItemGroup>', '  <ItemGroup>']
        out += [f'    <ClInclude Include="{escape(r)}" />' for r, _ in sorted(hdr, key=lambda t: t[0].lower())]
        out += ['  </ItemGroup>']
        rcs = [r for r, e in other if e == '.rc']
        rest = [r for r, e in other if e != '.rc']
        if rcs:
            out += ['  <ItemGroup>'] + [f'    <ResourceCompile Include="{escape(r)}" />' for r in rcs] + ['  </ItemGroup>']
        if rest:
            out += ['  <ItemGroup>'] + [f'    <None Include="{escape(r)}" />' for r in rest] + ['  </ItemGroup>']
        if deps:
            out += ['  <ItemGroup>']
            for d in deps:
                dd = next(p for n, p in PROJECTS if n == d)
                rel = os.path.relpath(os.path.join(root, os.path.splitext(dd)[0] + '.vcxproj'), pdir).replace('/', '\\')
                out += [f'    <ProjectReference Include="{escape(rel)}">',
                        f'      <Project>{guid(d)}</Project>',
                        '    </ProjectReference>']
            out += ['  </ItemGroup>']
        out += ['  <Import Project="$(VCTargetsPath)\\Microsoft.Cpp.targets" />', '</Project>', '']
        vcx = os.path.splitext(dsp)[0] + '.vcxproj'
        open(vcx, 'w', encoding='utf-8-sig', newline='\r\n').write('\n'.join(out))
        print(f'{os.path.relpath(vcx, root)}: {len(cl)} source, {len(hdr)} header, {len(other)} other')
        sol_projects.append((name, os.path.relpath(vcx, root).replace('/', '\\')))

    s = ['', 'Microsoft Visual Studio Solution File, Format Version 12.00', '# Visual Studio Version 17',
         'VisualStudioVersion = 17.0.31903.59', 'MinimumVisualStudioVersion = 10.0.40219.1']
    for n, p in sol_projects:
        s += [f'Project("{{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}}") = "{n}", "{p}", "{guid(n)}"']
        if n == 'ja2':
            s += ['\tProjectSection(ProjectDependencies) = postProject']
            s += [f'\t\t{guid(d)} = {guid(d)}' for d, _ in sol_projects if d != 'ja2']
            s += ['\tEndProjectSection']
        s += ['EndProject']
    s += ['Global', '\tGlobalSection(SolutionConfigurationPlatforms) = preSolution',
          '\t\tDebug|x86 = Debug|x86', '\t\tRelease|x86 = Release|x86', '\tEndGlobalSection',
          '\tGlobalSection(ProjectConfigurationPlatforms) = postSolution']
    for n, _ in sol_projects:
        for c in ('Debug', 'Release'):
            s += [f'\t\t{guid(n)}.{c}|x86.ActiveCfg = {c}|Win32', f'\t\t{guid(n)}.{c}|x86.Build.0 = {c}|Win32']
    s += ['\tEndGlobalSection', '\tGlobalSection(SolutionProperties) = preSolution', '\t\tHideSolutionNode = FALSE',
          '\tEndGlobalSection', 'EndGlobal', '']
    open(os.path.join(root, 'JA2.sln'), 'w', encoding='utf-8-sig', newline='\r\n').write('\n'.join(s))
    print('JA2.sln written')


if __name__ == '__main__':
    main()
