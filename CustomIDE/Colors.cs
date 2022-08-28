using System.Collections.Generic;

class Colors {
    public Dictionary<string, string> colors = new Dictionary<string, string>();
    public string[] keywords = {
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
    public string keywords_color = "#cc00ff";

    public Colors() {
        foreach (string keyword in keywords) {
            colors[keyword] = keywords_color;
        }
        colors["default"] = "#ffffff";
    }
}