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
            SecTextConfig.Instance.LoadConfig();

            if (args.Length < 3)
            {
                Console.WriteLine("SAS5Tool.Sectool");
                Console.WriteLine("A tool to extract & import messages/strings inside .sec5 file.\n");
                Console.WriteLine("Usage:\n\tSecTool.exe export <GameID> <sec5 file path> <output folder>\n\tSecTool.exe import <GameID> <sec5 file path> <intput folder> [<sec5 file save path>]\n\n");
                Console.WriteLine("Modes:\n\texport\t\tExtracting all texts inside the sec5 specified and output mutiple .txt files to the folder.\n\timport\t\tImport all .txt files inside the folder and output a new sec5 file.\n\n");
                SecTextConfig.Instance.PrintSupportedGames();
                return;
            }

            if(!SecTextConfig.Instance.HaveConfig(args[1].ToLower()))
            {
                Console.WriteLine($"We don't support {args[1]} now!");
                return;
            }
            SecTextTool.SetTextFlag(args[1]);

            SecScenarioProgram prog = new(args[2]);

            VariableManager.Instance.LoadVariablesList(prog.GetSectionData("DTDE"));

            var charset = new SecCodePage(CodepageManager.Instance.ExportCodePage);
            var source = new SecSource(prog.GetSectionData("CZIT"));
            var code = new ScenarioCode(prog.GetSectionData("CODE"), source);
            var option = new OptionManager(prog.GetSectionData("OPTN"));
            code.Disasemble();

            switch(args[0])
            {
                case "export":
                    var text = SecTextTool.GetText(code.Code);
                    if(!Path.Exists(args[3]))
                    {
                        Directory.CreateDirectory(args[3]);
                    }
                    SecTextTool.ExportText(args[3], text);
                    option.Save(Path.Combine(args[3], "options.json"));
                    break;

                case "import":
                {
                    if (Path.Exists(args[3]))
                    {
                        var txt = SecTextTool.ImportText(args[3]);
                        option.Load(Path.Combine(args[3], "options.json"));
                        SecTextTool.SetText(txt, code.Code);

                        var secData = code.Assemble();

                        option.UpdateExportFuncAddr(secData.Item2);

                        prog.SetSectionData("CODE", secData.Item1);
                        prog.SetSectionData("CHAR", charset.GetData());
                        prog.SetSectionData("OPTN", option.GetData());
                        prog.Save(args.Length < 5 ? args[2] + ".new" : args[4]);

                    }
                    else
                    {
                        Console.WriteLine($"Input directory \"{args[3]}\" dosen't exists.");
                    }
                    break;
                } 
            }
        }
    }
}
