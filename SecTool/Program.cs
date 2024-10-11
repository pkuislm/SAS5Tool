using SAS5Lib.SecCode;
using SAS5Lib.SecOption;
using SAS5Lib.SecVariable;
using System.Text;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using static SecTool.Program;

namespace SecTool
{
    internal class Program
    {
        public class BaseOptions
        {
            [Option('s', "sec5", Required = true, HelpText = "Path to sec5 file.")]
            public string InputFile { get; set; }

            [Option('r', "read-encoding", Default = null, HelpText = "Encoding of the strings inside the sec5. You can set the encoding to one of the following values: sjis, gbk, utf8.")]
            public string? ImportEncoding { get; set; }

            [Option('w', "write-encoding", Default = "utf8", HelpText = "Encoding of the strings inside the sec5. You can set the encoding to one of the following values: sjis, gbk, utf8.")]
            public string ExportEncoding { get; set; }
        }

        [Verb("export", aliases: ["ex"], HelpText = "Extracting dialogues inside the sec5 specified and output mutiple .txt files to the folder.")]
        public class ExportOptions : BaseOptions
        {
            [Option('o', "output", Required = true, HelpText = "Destnation of text files.")]
            public string OutputFolder { get; set; }

            [Usage(ApplicationAlias = "SecTool")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    yield return new Example("Export a file using default encoding", new ExportOptions { InputFile = "A:/B/Games/main.sec5", OutputFolder = "A:/B/Games/Texts" });
                    yield return new Example("Specify a read encoding", new ExportOptions { InputFile = "A:/B/Games/main.sec5", OutputFolder = "A:/B/Games/Texts", ImportEncoding = "sjis" });
                }
            }
        }

        [Verb("export-str", aliases: ["exstr"], HelpText = "Extracting strings inside the sec5 specified with code name(s) and output .txt file.")]
        public class ExportStrOptions : BaseOptions
        {
            [Option('o', "output", Required = false, HelpText = "Destnation of text files.")]
            public string OutputFolder { get; set; }

            [Option('n', "codename", Required = true, HelpText = "target file name.")]
            public IEnumerable<string> CodeNames { get; set; }

            [Usage(ApplicationAlias = "SecTool")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    yield return new Example("Export a file using default encoding", new ExportStrOptions { InputFile = "A:/B/Games/main.sec5", CodeNames = ["msg.sal"], OutputFolder = "A:/B/Games/Strings" });
                    yield return new Example("Specify a read encoding", new ExportStrOptions { InputFile = "A:/B/Games/main.sec5", CodeNames = ["msg.sal", "scene.sal"], OutputFolder = "A:/B/Games/Strings", ImportEncoding = "sjis" });
                }
            }
        }

        [Verb("import", aliases: ["im"], HelpText = "Import all .txt files inside the folder and output a new sec5 file.")]
        public class ImportOptions : BaseOptions
        {
            [Option('i', "input", Required = true, HelpText = "Path to text files you want to import.")]
            public string InputFolder { get; set; }

            [Option('o', "output", HelpText = "FullPath or FileName of the new sec5.")]
            public string OutputPath { get; set; }

            [Option('d', "debugbuild", HelpText = "Generates a datetime on game's title(isolate save data).", Default = false)]
            public bool DebugBuild { get; set; }

            [Usage(ApplicationAlias = "SecTool")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    yield return new Example("Import a file using default encoding", new ImportOptions { InputFile = "A:/B/Games/main.sec5", InputFolder = "A:/B/Games/Texts", OutputPath = "A:/B/Games/main.sec5.new" });
                    yield return new Example("Specify a read encoding", new ImportOptions { InputFile = "A:/B/Games/main.sec5", InputFolder = "A:/B/Games/Texts", OutputPath = "A:/B/Games/main.sec5.new", ImportEncoding = "sjis" });
                    yield return new Example("Specify a write encoding", new ImportOptions { InputFile = "A:/B/Games/main.sec5", InputFolder = "A:/B/Games/Texts", OutputPath = "A:/B/Games/main.sec5.new", ExportEncoding = "sjis" });
                    yield return new Example("Specify both read encoding and write encoding", new ImportOptions { InputFile = "A:/B/Games/main.sec5", InputFolder = "A:/B/Games/Texts", OutputPath = "A:/B/Games/main.sec5.new", ImportEncoding = "sjis", ExportEncoding = "sjis" });
                    yield return new Example("Don't specify the output path(will use input file's path and name + .new)", new ImportOptions { InputFile = "A:/B/Games/main.sec5", InputFolder = "A:/B/Games/Texts" });
                    yield return new Example("Specify only output file name(will use input file's path)", new ImportOptions { InputFile = "A:/B/Games/main.sec5", InputFolder = "A:/B/Games/Texts", OutputPath = "main.sec5.new" });
                    yield return new Example("Specify only output file path(will use input file's name + .new)", new ImportOptions { InputFile = "A:/B/Games/main.sec5", InputFolder = "A:/B/Games/Texts", OutputPath = "A:/B/Games/" });
                }
            }
        }

        [Verb("import-str", aliases: ["imstr"], HelpText = "Importing strings inside the sec5 specified with code name(s).")]
        public class ImportStrOptions : BaseOptions
        {
            [Option('o', "output", Required = false, HelpText = "FullPath or FileName of the new sec5.")]
            public string OutputPath { get; set; }

            [Option('i', "input", Required = true, HelpText = "Pairs of code name and input file path")]
            public IEnumerable<string> CodeNames { get; set; }

            [Usage(ApplicationAlias = "SecTool")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    yield return new Example("Export a file using default encoding", new ImportStrOptions { InputFile = "A:/B/Games/main.sec5", CodeNames = ["msg.sal", "A:/B/Games/Strings/msg.sal_strings.txt"], OutputPath = "main.sec5.new" });
                    yield return new Example("Specify a read encoding", new ImportStrOptions { InputFile = "A:/B/Games/main.sec5", CodeNames = ["msg.sal", "A:/B/Games/Strings/msg.sal_strings.txt", "scene.sal", "A:/B/Games/Strings/scene.sal_strings.txt"], OutputPath = "main.sec5.new", ImportEncoding = "sjis" });
                }
            }
        }

        [Verb("get", HelpText = "Get a raw data section from a .sec5 file.")]
        public class SectionGetOptions : BaseOptions
        {
            [Option('n', "section-name", Required = true, HelpText = "You can get one of the following sections: 'CODE', 'OPTN', 'CHAR', 'CZIT', 'DTDE', 'RES2', 'RTFC', 'VARI'")]
            public string SectionName { get; set; }

            [Option('o', "output")]
            public string OutputPath { get; set; }
        }

        [Verb("set", HelpText = "Set a raw data section to a .sec5 file.")]
        public class SectionSetOptions : BaseOptions
        {
            [Option('n', "section-name", Required = true, HelpText = "You can set one of the following sections: 'CODE', 'OPTN', 'CHAR', 'CZIT', 'DTDE', 'RES2', 'RTFC', 'VARI'")]
            public string SectionName { get; set; }

            [Option('i', "input", Required = true)]
            public string InputSectionFile { get; set; }

            [Option('o', "output")]
            public string OutputPath { get; set; }
        }

        [Verb("info", HelpText = "View the info of a .sec5 file.")]
        public class InfoOptions
        {
            [Option('s', "sec5", Required = true, HelpText = "Path to sec5 file.")]
            public string InputFile { get; set; }
        }

        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<ExportOptions, ExportStrOptions, ImportOptions, ImportStrOptions, SectionGetOptions, SectionSetOptions, InfoOptions>(args);
            parserResult
                .WithParsed<ExportOptions>(Run)
                .WithParsed<ExportStrOptions>(Run)
                .WithParsed<ImportOptions>(Run)
                .WithParsed<ImportStrOptions>(Run)
                .WithParsed<SectionGetOptions>(ProcessSection)
                .WithParsed<SectionSetOptions>(ProcessSection)
                .WithParsed<InfoOptions>(PrintInfo)
                .WithNotParsed(errs => DisplayHelp(parserResult));
        }

        static HelpText GetHelpText()
        {
            return new HelpText()
            {
                AutoHelp = false,
                AutoVersion = false,
                AdditionalNewLineAfterOption = false,
                Heading = "SAS5Tool.SecTool 1.0.0",
                Copyright = "A tool to extract & import messages/strings inside .sec5 file."
            };
        }

        static void DisplayHelp<T>(ParserResult<T> result)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                return HelpText.DefaultParsingErrorsHandler(result, GetHelpText());
            }, e => e,
            verbsIndex: true);
            Console.WriteLine(helpText);
        }

        static void Run(BaseOptions opt)
        {
            SecScenarioProgram prog = new(opt.InputFile);

            if (opt.ImportEncoding != null)
            {
                CodepageManager.Instance.SetImportEncoding(opt.ImportEncoding);
            }
            else
            {
                var c = new SecCodePage(prog.GetSectionData("CHAR"));
                if (c.FileReadingCodePage != 0)
                {
                    CodepageManager.Instance.SetImportEncoding(c.FileReadingCodePage);
                }
                else
                {
                    CodepageManager.Instance.SetImportEncoding("sjis");
                }
            }
            CodepageManager.Instance.SetExportEncoding(opt.ExportEncoding);

            VariableManager.Instance.LoadVariablesList(prog.GetSectionData("DTDE"));
            var vari = new PresetVariables(prog.GetSectionData("VARI"));
            SecTextTool.SetTextFlag();

            var charset = opt.ExportEncoding == "gbk" ? new SecCodePage(0) : new SecCodePage(CodepageManager.Instance.ExportCodePage);
            var source = new SecSource(prog.GetSectionData("CZIT"));
            var code = new ScenarioCode(prog.GetSectionData("CODE"), source);
            var option = new OptionManager(prog.GetSectionData("OPTN"));
            option.PrintGameInfo();
            code.Disasemble();

            switch (opt)
            {
                case ExportOptions expOpt:
                {
                    var text = SecTextTool.GetText(code.Code);
                    if (!Path.Exists(expOpt.OutputFolder))
                    {
                        Directory.CreateDirectory(expOpt.OutputFolder);
                    }
                    SecTextTool.ExportText(expOpt.OutputFolder, text.Item1);
                    File.WriteAllText(Path.Combine(expOpt.OutputFolder, "names.json"), JsonConvert.SerializeObject(text.Item2, Formatting.Indented));
                    option.Save(Path.Combine(expOpt.OutputFolder, "options.json"));
                    break;
                }
                case ExportStrOptions expsOpt:
                {
                    if (!Path.Exists(expsOpt.OutputFolder))
                    {
                        Directory.CreateDirectory(expsOpt.OutputFolder);
                    }
                    foreach (var name in expsOpt.CodeNames)
                    {
                        SecTextTool.ExportStrings(SecTextTool.GetString(code.Code, name), Path.Combine(expsOpt.OutputFolder, $"{name}_strings.txt"));
                    }
                    break;
                }
                case ImportOptions impOpt:
                {
                    if (Path.Exists(impOpt.InputFolder))
                    {
                        var txt = SecTextTool.ImportText(impOpt.InputFolder);
                        option.Load(Path.Combine(impOpt.InputFolder, "options.json"), impOpt.DebugBuild);

                        if (Path.Exists(Path.Combine(impOpt.InputFolder, "names.json")))
                        {
                            Console.WriteLine($"Using names.json...");
                            var nameMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(impOpt.InputFolder, "names.json")));
                            SecTextTool.SetText(txt, code.Code, nameMap);
                        }
                        else
                        {
                            SecTextTool.SetText(txt, code.Code);
                        }
                        var secData = code.Assemble();
                        option.UpdateExportFuncAddr(secData.Item2);

                        prog.SetSectionData("CODE", secData.Item1);
                        prog.SetSectionData("CZIT", source.GetData());
                        prog.SetSectionData("CHAR", charset.GetData());
                        prog.SetSectionData("OPTN", option.GetData());

                        prog.Save(GetOutputFilepath(impOpt.OutputPath, opt.InputFile));
                    }
                    else
                    {
                        Console.WriteLine($"Input directory '{impOpt.InputFolder}' dosen't exists.");
                    }
                    break;
                }
                case ImportStrOptions impsOpt:
                {
                    var lst = impsOpt.CodeNames.ToList();
                    for (int i = 0; i < lst.Count; i += 2)
                    {
                        SecTextTool.SetString(code.Code, lst[i], SecTextTool.ImportStrings(lst[i + 1]));
                    }
                    var secData = code.Assemble();
                    option.UpdateExportFuncAddr(secData.Item2);

                    prog.SetSectionData("CODE", secData.Item1);
                    prog.SetSectionData("CZIT", source.GetData());
                    prog.SetSectionData("CHAR", charset.GetData());
                    prog.SetSectionData("OPTN", option.GetData());

                    prog.Save(GetOutputFilepath(impsOpt.OutputPath, opt.InputFile));
                    break;
                }
            }
        }

        static void PrintInfo(object opt)
        {
            if(opt is InfoOptions infoOptions)
            {
                SecScenarioProgram prog = new(infoOptions.InputFile);

                var charset = new SecCodePage(prog.GetSectionData("CHAR"));
                if(charset.FileReadingCodePage != 0)
                {
                    CodepageManager.Instance.SetImportEncoding(charset.FileReadingCodePage);
                }
                else
                {
                    CodepageManager.Instance.SetImportEncoding("sjis");
                }
                
                var option = new OptionManager(prog.GetSectionData("OPTN"));
                option.PrintGameInfo();
            }
        }

        static void ProcessSection(BaseOptions opt)
        {
            SecScenarioProgram prog = new(opt.InputFile);

            switch (opt)
            {
                case SectionGetOptions sectionGetOptions:
                {
                    File.WriteAllBytes(sectionGetOptions.OutputPath ?? $"{sectionGetOptions.InputFile}.{sectionGetOptions.SectionName}", prog.GetSectionData(sectionGetOptions.SectionName));
                    break;
                }
                case SectionSetOptions sectionSetOptions:
                {
                    prog.SetSectionData(sectionSetOptions.SectionName, File.ReadAllBytes(sectionSetOptions.InputSectionFile));
                    prog.Save(GetOutputFilepath(sectionSetOptions.OutputPath, sectionSetOptions.InputFile));
                    break;
                }
            }
        }

        static string GetOutputFilepath(string path, string defaultPath)
        {
            if (path != null)
            {
                var dir = Path.GetDirectoryName(path);
                var file = Path.GetFileName(path);
                if (dir != "" && file != "")
                {
                    return path;
                }
                else
                {
                    if (dir != "")
                    {
                        return Path.Combine(dir, Path.GetFileName(defaultPath) + ".new");
                    }
                    else
                    {
                        return Path.Combine(Path.GetDirectoryName(defaultPath), file);
                    }
                }
            }
            else
            {
                return defaultPath + ".new";
            }
        }
    }
}
