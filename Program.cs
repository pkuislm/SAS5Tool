using System.Text;
using SecTool.SecVariable;
using SecTool.SecCode;
using SecTool.SecOption;

namespace SecTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            CodepageManager.Instance.SetImportEncoding("sjis");
            CodepageManager.Instance.SetExportEncoding("utf8");

            if(args.Length < 3)
            {
                Console.WriteLine("SAS5Tool.Sectool");
                Console.WriteLine("A tool to extract & import messages/strings inside .sec5 file.\n");
                Console.WriteLine("Usage:\nSecTool.exe export <sec5 file path> <output folder>\nSecTool.exe import <sec5 file path> <intput folder> [<sec5 file save path>]\n\n");
                Console.WriteLine("Modes:\n\textract\t\tExtracting all texts inside the sec5 specified and output mutiple .txt files to the folder.\n\timport\t\tImport all .txt files inside the folder and output a new sec5 file.");
                return;
            }

            SecScenarioProgram prog = new(args[1]);
            VariableManager.Instance.LoadVariablesList(prog.GetSectionData("DTDE"));

            var charset = new SecCodePage(CodepageManager.Instance.ExportCodePage);
            var source = new SecSource(prog.GetSectionData("CZIT"));
            var code = new ScenarioCode(prog.GetSectionData("CODE"), source);
            var option = new OptionManager(prog.GetSectionData("OPTN"));
            code.Disasemble();
            
            switch(args[0])
            {
                case "export":
                    var text = code.GetText();
                    ScenarioCode.ExportText(args[2], text);
                    option.Save(Path.Combine(args[2], "options.json"));
                    break;

                case "import":
                    var txt = ScenarioCode.ImportText(args[2]);
                    option.Load(Path.Combine(args[2], "options.json"));
                    code.SetText(txt);

                    var secData = code.Assemble();

                    option.UpdateExportFuncAddr(secData.Item2);

                    prog.SetSectionData("CODE", secData.Item1);
                    prog.SetSectionData("CHAR", charset.GetData());
                    prog.SetSectionData("OPTN", option.GetData());
                    prog.Save(args.Length < 4 ? args[1] + ".new" : args[3]);
                    break;
            }
        }
    }
}
