using CustomIDE;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

public class Highlighter {

    CoderBox coderBox;
    RichTextBox textBox;

    public Highlighter(CoderBox cb) {
        coderBox = cb;
        textBox = coderBox.CodeTextBox;
    }

    public void HighlightAll() {

        IEnumerable<TextRange> wordRanges = GetAllWordRanges(textBox.Document);
        foreach (TextRange wordRange in wordRanges) {
            string word = wordRange.Text;
            if (Colors.colors.Keys.Contains(word)) {
                wordRange.ApplyPropertyValue(TextElement.ForegroundProperty, Colors.colors[word]);
            } else {
                wordRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            }
        }
    }


    public IEnumerable<TextRange> GetAllWordRanges(FlowDocument document) {
        string pattern = @"[^\W\d](\w|[-']{1,2}(?=\w))*";
        TextPointer pointer = document.ContentStart;
        while (pointer != null) {
            if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text) {
                string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                Debug.WriteLine(textRun);

                MatchCollection matches = Regex.Matches(textRun, pattern);
                foreach (Match match in matches) {
                    int startIndex = match.Index;
                    int length = match.Length;
                    TextPointer start = pointer.GetPositionAtOffset(startIndex);
                    TextPointer end = start.GetPositionAtOffset(length);
                    yield return new TextRange(start, end);
                }

                //pointer = pointer.GetPositionAtOffset(pointer.GetTextRunLength(LogicalDirection.Forward));
            }
            pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            // pointer not going to new position
        }
    }

}