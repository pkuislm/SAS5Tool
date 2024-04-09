using SAS5Lib.SecCode;
using SAS5Lib.SecOption;
using SAS5Lib.SecVariable;
using System.Text;

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
                Console.WriteLine("Usage:\nSecTool.exe export <game> <sec5 file path> <output folder>\nSecTool.exe import <game> <sec5 file path> <intput folder> [<sec5 file save path>]\n\n");
                Console.WriteLine("Modes:\n\textract\t\tExtracting all texts inside the sec5 specified and output mutiple .txt files to the folder.\n\timport\t\tImport all .txt files inside the folder and output a new sec5 file.");
                Console.WriteLine("Supported games:");
                Console.WriteLine("Minamijuujisei Renka (nankoi)");
                Console.WriteLine("Natsuiro Koi Uta (natsukoi)");
                Console.WriteLine("Hanikami Clover (hanikami)");
                return;
            }

            SecScenarioProgram prog = new(args[2]);

            VariableManager.Instance.LoadVariablesList(prog.GetSectionData("DTDE"));

            var charset = new SecCodePage(CodepageManager.Instance.ExportCodePage);
            var source = new SecSource(prog.GetSectionData("CZIT"));
            var code = new ScenarioCode(prog.GetSectionData("CODE"), source);
            var option = new OptionManager(prog.GetSectionData("OPTN"));
            code.Disasemble();

            SecTextTool.ExtractionSetTextFormatFlagVal(args[1], args[1].Equals("natsukoi", StringComparison.CurrentCultureIgnoreCase) ? 0 : 1);
            switch(args[0])
            {
                case "export":
                    var text = SecTextTool.GetText(code.Code);
                    SecTextTool.ExportText(args[3], text);
                    option.Save(Path.Combine(args[3], "options.json"));
                    break;

                case "import":
                    var txt = SecTextTool.ImportText(args[3]);
                    option.Load(Path.Combine(args[3], "options.json"));
                    SecTextTool.SetText(txt, code.Code);

                    var secData = code.Assemble();

                    option.UpdateExportFuncAddr(secData.Item2);

                    prog.SetSectionData("CODE", secData.Item1);
                    prog.SetSectionData("CHAR", charset.GetData());
                    prog.SetSectionData("OPTN", option.GetData());
                    prog.Save(args.Length < 5 ? args[2] + ".new" : args[4]);
                    break;
            }
        }
    }
}
