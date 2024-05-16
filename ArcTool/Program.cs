using SAS5Lib.SecResource;
using System.Text;
using System.Text.RegularExpressions;

namespace ArcTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            CodepageManager.Instance.SetImportEncoding("sjis");
            CodepageManager.Instance.SetExportEncoding("utf8");


            if(args.Length <  4)
            {
                Console.WriteLine("SAS5Tool.ArcTool");
                Console.WriteLine("A tool to extract & import resource file inside .iar/.war/.gar file.\n");
                Console.WriteLine("Usage:\nArcTool.exe <iar|war|gar> unpack <sec5 file path> <archive file path> [<output directory>]\nArcTool.exe <iar|war|gar> pack <sec5 file path> <folder-path to pack> [<new archive name>] [<new sec5 path>]\n\n");
                Console.WriteLine("Modes:\n\textract\t\tExtracting all texts inside the sec5 specified and output mutiple .txt files to the folder.\n\timport\t\tImport all .txt files inside the folder and output a new sec5 file.");
                return;
            }

            SecScenarioProgram prog = new(args[2]);
            var resource = new ResourceManager(prog.GetSectionData("RES2"));
            var archives = new SecArcFileList(prog.GetSectionData("RTFC"));

            if (args[1] == "unpack")
            {
                switch (args[0])
                {
                    case "gar":
                        var garArc = new GarArchive(args[3]);
                        garArc.ExtractTo(args.Length > 4 ? args[4] : args[3] + "_unpack");
                        break;
                    case "iar":
                        var iarArc = new IarArchive(args[3]);
                        iarArc.ExtractTo(resource.GetIarFileList(args[3]), args.Length > 4 ? args[4] : args[3] + "_unpack");
                        break;
                }
            }
            else if (args[1] == "pack")
            {
                var newArcFileName = args.Length > 4 ? args[4] : args[3];
                var newSec5Path = args.Length > 5 ? args[5] : args[2];
                switch (args[0])
                {
                    case "gar":
                    {
                        if(!newArcFileName.EndsWith(".gar"))
                            newArcFileName += ".gar";
                        var fileList = GarArchive.Create(args[3], newArcFileName);
                        resource.UpdateGarResourceRecord(fileList, newArcFileName);
                        break;
                    }
                    case "iar":
                    {
                        if (!newArcFileName.EndsWith(".iar"))
                            newArcFileName += ".iar";
                        var fileList = IarArchive.Create(args[3], newArcFileName);
                        resource.UpdateIarResourceRecord(fileList, newArcFileName);
                        break;
                    }
                }
                archives.AddArc(newArcFileName);
                prog.SetSectionData("RES2", resource.GetData());
                prog.SetSectionData("REFC", archives.GetData());
                prog.Save(newSec5Path);
                Console.WriteLine("Finished.");
            }
            else
            {
                Console.WriteLine($"Unknown operation: {args[1]}");
            }
        }
    }
}
