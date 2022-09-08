using System.Collections.Generic;
using System.Windows.Media;

class Colors {
    public static string[] keywords = {
        "False",
        "None",
        "True",
        "and",
        "as",
        "assert",
        "async",
        "await",
        "break",
        "class",
        "continue",
        "def",
        "del",
        "elif",
        "else",
        "except",
        "finally",
        "for",
        "from",
        "global",
        "if",
        "import",
        "in",
        "is",
        "lambda",
        "nonlocal",
        "not",
        "or",
        "pass",
        "raise",
        "return",
        "try",
        "while",
        "with",
        "yield"
    };

    public static string[] buildt_ins = {
        "abs", "aiter", "all", "any", "anext", "ascii", "bin", "bool", "breakpoint", "bytearray", "bytes", "callable", "chr", "classmethod", "compile", "complex", "delattr", "dict", "dir", "divmod", "enumerate", "eval", "exec", "filter", "float", "format", "frozenset", "getattr", "globals", "hasattr", "hash", "help", "hex", "id", "input", "int", "isinstance", "issubclass", "iter", "len", "list", "locals", "map", "max", "memoryview", "min", "next", "object", "oct", "open", "ord", "pow", "print", "property", "range", "repr", "reversed", "round", "set", "setattr", "slice", "sorted", "staticmethod", "str", "sum", "super", "tuple", "type", "vars", "zip"
    };

    public static Dictionary<string, SolidColorBrush> colors = new Dictionary<string, SolidColorBrush>();

    static Colors() {

        SolidColorBrush built_in_brush = new SolidColorBrush(Color.FromRgb(0xfc, 0xfa, 0x79));

        foreach (string keyword in keywords)
            colors[keyword] = Brushes.Violet;

        foreach (string bi in buildt_ins)
            colors[bi] = built_in_brush;
    }
}