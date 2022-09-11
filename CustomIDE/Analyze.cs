using Newtonsoft.Json;
using System.Diagnostics;

public class Analyzer {
    public Analyzer() {
        
    }

    public void ConvertToJSON(string filePath) {
        Process p = Process.Start(@"python E:\Users\finnd\Documents\Visual_Studio_Code\Ast\test.py "+filePath);
        p.WaitForExit();

    }
}