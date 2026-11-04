"""Tests for c2cs.py. Run:  python -I -m unittest discover -s tools -p "test_c2cs.py"

Each case writes a tiny fake port (a few .cs files) and a C file to a temp dir, runs the generator and checks the
generated C#. They pin the behaviours the generator is relied on for; they do not check that the output compiles.
"""
import contextlib
import io
import os
import sys
import tempfile
import unittest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import c2cs  # noqa: E402

REPO_CS = """
namespace X;
public enum Mode
{
    NONE,
    ONE,
    TWO,
}
[System.Flags]
public enum Fl
{
    A = 1,
    B = 2,
    C = 4,
}
public static class Other { public static void Bar(ref int v) {} public static int Baz(int a) { return a; } }
public class Target { public static void Existing() {} }
"""


def generate(c_source, map_json=None, repo_cs=REPO_CS, include_all=True):
    """Run the generator on c_source and return the generated C# text."""
    with tempfile.TemporaryDirectory() as tmp:
        repo = os.path.join(tmp, 'repo')
        out = os.path.join(tmp, 'out')
        os.makedirs(repo)
        with open(os.path.join(repo, 'a.cs'), 'w') as f:
            f.write(repo_cs)
        cfile = os.path.join(tmp, 'sample.c')
        with open(cfile, 'w') as f:
            f.write(c_source)
        map_path = os.path.join(tmp, 'map.json')
        with open(map_path, 'w') as f:
            f.write(map_json or '{}')
        with contextlib.redirect_stdout(io.StringIO()):
            c2cs.port_file(cfile, repo, out, include_all, map_path)
        with open(os.path.join(out, 'sample.cs'), encoding='utf-8') as f:
            return f.read()


class SignatureTests(unittest.TestCase):
    def test_primitive_types_become_int(self):
        cs = generate('INT32 Foo( UINT8 ubMode, INT16 sVal )\n{\n\treturn 0;\n}\n')
        self.assertIn('public static int Foo(int ubMode, int sVal)', cs)

    def test_pointer_to_primitive_becomes_ref(self):
        cs = generate('void Foo( INT32 *piVal )\n{\n\t*piVal = 1;\n}\n')
        self.assertIn('ref int piVal', cs)

    def test_char_pointer_becomes_string(self):
        cs = generate('void Foo( CHAR16 *pStr )\n{\n}\n')
        self.assertIn('string pStr', cs)


class BodyTests(unittest.TestCase):
    def test_arrow_becomes_dot(self):
        cs = generate('void Foo( SOLDIERTYPE *pSoldier )\n{\n\tpSoldier->bLife = 1;\n}\n')
        self.assertIn('pSoldier.bLife = 1;', cs)
        self.assertNotIn('->', cs)

    def test_wide_string_literals_lose_the_L(self):
        cs = generate('void Foo( void )\n{\n\tChar( L"abc" );\n}\n')
        self.assertIn('"abc"', cs)
        self.assertNotIn('L"', cs)

    def test_swprintf_becomes_wprintf_assignment(self):
        cs = generate('void Foo( INT32 i )\n{\n\tswprintf( zString, L"%d", i );\n}\n')
        self.assertIn('zString = wprintf( "%d", i );', cs)

    def test_enum_prefix_tokens_are_translated(self):
        cs = generate('void Foo( void )\n{\n\tINT32 i = Mode_ONE;\n}\n')
        self.assertIn('Mode.ONE', cs)

    def test_address_of_argument_resolved_from_signature(self):
        cs = generate('void Foo( void )\n{\n\tINT32 iLocal = 0;\n\tBar( &iLocal );\n}\n')
        self.assertIn('Other.Bar( ref iLocal );', cs)

    def test_unresolvable_address_of_is_flagged(self):
        cs = generate('void Foo( void )\n{\n\tUnknownCall( &x );\n}\n')
        self.assertIn('TODO(port)', cs)

    def test_local_array_declaration(self):
        cs = generate('void Foo( void )\n{\n\tINT32 aiVals[10];\n}\n')
        self.assertIn('int[] aiVals = new int[10];', cs)

    def test_static_local_is_hoisted_to_a_field(self):
        cs = generate('void Foo( void )\n{\n\tstatic INT32 iCount = 5;\n\tiCount++;\n}\n')
        self.assertIn('private static int Foo_iCount = 5;', cs)
        self.assertIn('function-local static of Foo', cs)

    def test_disabled_ifdef_branch_is_commented_out(self):
        cs = generate('void Foo( void )\n{\n#ifdef JA2TESTVERSION\n\tDebugOnly();\n#else\n\tReal();\n#endif\n}\n')
        self.assertIn('Real();', cs)
        self.assertNotIn('\n    \tDebugOnly();', cs)


class TextCallTests(unittest.TestCase):
    def test_draw_text_to_screen_gets_point_and_justification(self):
        cs = generate('void Foo( void )\n{\n\tDrawTextToScreen(s, 10, 20, 100, FONT12ARIAL, c, bg, FALSE, CENTER_JUSTIFIED);\n}\n')
        self.assertIn('FontSubSystem.DrawTextToScreen(s, new(10, 20), 100,', cs)
        self.assertIn('TextJustifies.CENTER_JUSTIFIED);', cs)

    def test_display_wrapped_string_gets_point(self):
        cs = generate('void Foo( void )\n{\n\tDisplayWrappedString(5, 6, 200, 2, f, c, s, bg, FALSE, LEFT_JUSTIFIED | INVALIDATE_TEXT);\n}\n')
        self.assertIn('FontSubSystem.DisplayWrappedString(new(5, 6), 200, 2,', cs)
        self.assertIn('TextJustifies.LEFT_JUSTIFIED | TextJustifies.INVALIDATE_TEXT);', cs)

    def test_encrypted_load_into_char_buffer_becomes_out_string(self):
        cs = generate('void Foo( void )\n{\n\twchar_t sText[400];\n\tLoadEncryptedDataFromFile(FILE, sText, 0, 800);\n}\n')
        self.assertIn('string sText = string.Empty;', cs)
        self.assertIn('files.LoadEncryptedDataFromFile(FILE, out sText, 0, 800);', cs)

    def test_nested_call_arguments_are_split_correctly(self):
        cs = generate('void Foo( void )\n{\n\tDrawTextToScreen(Get(a, b), x + (int)(y), 0, 10, f, c, bg, FALSE, RIGHT_JUSTIFIED);\n}\n')
        self.assertIn('FontSubSystem.DrawTextToScreen(Get(a, b), new(x + (int)(y), 0), 10,', cs)


class QualifyTests(unittest.TestCase):
    def test_call_into_another_ported_class_is_prefixed(self):
        cs = generate('INT32 Foo( INT32 i )\n{\n\treturn Baz( i );\n}\n')
        self.assertIn('Other.Baz( i )', cs)

    def test_calls_within_the_batch_are_not_prefixed(self):
        cs = generate('INT32 Baz2( INT32 i )\n{\n\treturn i;\n}\nINT32 Foo( INT32 i )\n{\n\treturn Baz2( i );\n}\n')
        self.assertIn('return Baz2( i );', cs)

    def test_mouse_system_calls(self):
        cs = generate('void Foo( void )\n{\n\tMSYS_RemoveRegion( r );\n}\n')
        self.assertIn('MouseSubSystem.MSYS_RemoveRegion(', cs)

    def test_video_and_font_rules(self):
        cs = generate('void Foo( void )\n{\n\tInvalidateRegion( 0, 0, 640, 480 );\n\tSetFont( f );\n}\n')
        self.assertIn('video.InvalidateRegion(', cs)
        self.assertIn('FontSubSystem.SetFont(', cs)

    def test_get_video_object_becomes_assignment(self):
        cs = generate('void Foo( void )\n{\n\tGetVideoObject( &hHandle, uiIdx );\n}\n')
        self.assertIn('hHandle = uiIdx', cs)

    def test_c_library_macros(self):
        cs = generate('void Foo( INT32 a, INT32 b )\n{\n\tAssert( a );\n\tINT32 c = min( a, b );\n}\n')
        self.assertIn('System.Diagnostics.Debug.Assert(', cs)
        self.assertIn('Math.Min(', cs)

    def test_already_qualified_calls_are_left_alone(self):
        qualified = c2cs.qualify_calls('video.InvalidateRegion( 0 );', c2cs.Repo(self._empty_repo()), 'X', set(), {})
        self.assertEqual(qualified, 'video.InvalidateRegion( 0 );')

    def test_comments_are_not_rewritten(self):
        qualified = c2cs.qualify_calls('x = 1; // min( a, b )', c2cs.Repo(self._empty_repo()), 'X', set(), {})
        self.assertEqual(qualified, 'x = 1; // min( a, b )')

    def test_map_qualify_table_and_call_rules(self):
        map_json = '{"qualify": {"Emails": ["AddEmail"]}, "call_rules": [["(?<![\\\\w.])Zap\\\\(", "Zapper.Zap("]]}'
        cs = generate('void Foo( void )\n{\n\tAddEmail( 1 );\n\tZap( 2 );\n}\n', map_json=map_json)
        self.assertIn('Emails.AddEmail( 1 );', cs)
        self.assertIn('Zapper.Zap( 2 );', cs)

    def test_instance_owner_uses_injected_field(self):
        repo_cs = REPO_CS + 'public class SDL2VideoManager { public void Present(int a) {} }\n'
        cs = generate('void Foo( void )\n{\n\tPresent( 1 );\n}\n', repo_cs=repo_cs)
        self.assertIn('video.Present( 1 );', cs)
        self.assertNotIn('SDL2VideoManager.', cs)

    def test_functions_already_in_the_target_class_are_not_prefixed(self):
        # `Existing` lives in Target; porting a file whose functions mostly live in Target must not prefix them
        qualified = c2cs.qualify_calls('Existing( );', c2cs.Repo(self._repo_with(REPO_CS)), 'Target', set(), {})
        self.assertEqual(qualified, 'Existing( );')

    @staticmethod
    def _repo_with(cs):
        tmp = tempfile.mkdtemp()
        with open(os.path.join(tmp, 'a.cs'), 'w') as f:
            f.write(cs)
        return tmp

    def _empty_repo(self):
        return self._repo_with('namespace X; public class Empty {}')


if __name__ == '__main__':
    unittest.main()
