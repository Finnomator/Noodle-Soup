using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace Colors {
    public class KeyWords {
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

        public static string[] buildtIns = {
        "abs", "aiter", "all", "any", "anext", "ascii", "bin", "bool", "breakpoint", "bytearray", "bytes", "callable", "chr", "classmethod", "compile", "complex", "delattr", "dict", "dir", "divmod", "enumerate", "eval", "exec", "filter", "float", "format", "frozenset", "getattr", "globals", "hasattr", "hash", "help", "hex", "id", "input", "int", "isinstance", "issubclass", "iter", "len", "list", "locals", "map", "max", "memoryview", "min", "next", "object", "oct", "open", "ord", "pow", "print", "property", "range", "repr", "reversed", "round", "set", "setattr", "slice", "sorted", "staticmethod", "str", "sum", "super", "tuple", "type", "vars", "zip"};
        protected static SolidColorBrush keywordsColor = Brushes.Violet;
        protected static SolidColorBrush builtInsColor = new SolidColorBrush(Color.FromRgb(0xfc, 0xfa, 0x79));
        protected static SolidColorBrush defaultColor = Brushes.White;
        public readonly static SolidColorBrush integerColor = new SolidColorBrush(Color.FromRgb(224, 197, 153));
        public readonly static SolidColorBrush commentsColor = Brushes.Green;
        public readonly static SolidColorBrush FloatColor = new SolidColorBrush(Color.FromRgb(255, 244, 128));
    }


    public class ColorDict : KeyWords {
        private static Dictionary<string, SolidColorBrush> colorDict = new Dictionary<string, SolidColorBrush>();
        static ColorDict() {
            UpdateColors();
        }

        private static void UpdateColors() {
            foreach (string keyword in keywords)
                colorDict[keyword] = keywordsColor;

            foreach (string bi in buildtIns)
                colorDict[bi] = builtInsColor;

            colorDict = colorDict.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
        }

        public static SolidColorBrush Get(string key) {
            if (colorDict.ContainsKey(key))
                return colorDict[key];
            return defaultColor;
        }
    }
}