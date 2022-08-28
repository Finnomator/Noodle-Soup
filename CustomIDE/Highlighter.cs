using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

public class Highlighter {
    BrushConverter bc;
    public Highlighter() {

        bc = new BrushConverter();
    }

    public void highlight(RichTextBox rtb, string[] tokens) {

        string token_string = "(";

        for (int i = 0; i < tokens.Length; i++) {
            string token = tokens[i];

            token_string += token + (i == tokens.Length - 1 ? "" : "|");
        }

        token_string += ")";

        Regex regex = new Regex(token_string);

        TextPointer docStart = rtb.Document.ContentStart;
        TextPointer docEnd = rtb.Document.ContentEnd;

        TextRange tr = new TextRange(docStart, docEnd);

        string[] lines = tr.Text.Split('\n');

        for (int i = 0; i < lines.Length; i++) {
            string line = lines[i];
            MatchCollection matchCollection = regex.Matches(line);
            foreach (Match match in matchCollection) {
                int start = match.Index;
                int end = start + match.Length;

                TextPointer startPointer = rtb.CaretPosition.GetLineStartPosition(0).GetPositionAtOffset(start);
                TextPointer endPointer = rtb.CaretPosition.GetLineStartPosition(0).GetPositionAtOffset(end);

                TextRange textRange = new TextRange(startPointer, endPointer);
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, bc.ConvertFromString("#cc00ff"));
            }
        }
        
    }
}