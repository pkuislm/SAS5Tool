using System.Diagnostics;
using System.Text;
using SAS5CodeDisasembler.SecVariable;
using SAS5CodeDisasembler.SecCode;
using SAS5CodeDisasembler.SecOption;
using System.Xml.Linq;

namespace SAS5CodeDisasembler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            CodepageManager.Instance.SetImportEncoding("sjis");
            CodepageManager.Instance.SetExportEncoding("utf8");

            SecScenarioProgram prog = new(@"E:\GalGames_Work\OnWork\夏彩恋歌\natsukoi_main.sec5.old");

            VariableManager.Instance.LoadVariablesList(prog.GetSectionData("DTDE"));

            {
                var charset = new SecCodePage(CodepageManager.Instance.ExportCodePage);

                var code = new ScenarioCode(prog.GetSectionData("CODE"));
                var option = new OptionManager(prog.GetSectionData("OPTN"));
                var source = new SecSource(prog.GetSectionData("CZIT"));
                code.Disasemble();

                //var text = code.GetText();
                //SAS5Code.ExportText(@"E:\GalGames_Work\OnWork\夏彩恋歌\Text", text);
                var secData = code.Assemble();
                //File.WriteAllBytes(@"E:\GalGames_Work\OnWork\夏彩恋歌\SEC5\CODE_NEW", secData.Item1);

                option.UpdateExportFuncAddr(secData.Item2);

                prog.SetSectionData("CODE", secData.Item1);
                prog.SetSectionData("CHAR", charset.GetCharSetData());
                prog.SetSectionData("OPTN", option.Save());
            }

            prog.Save(@"E:\GalGames_Work\OnWork\夏彩恋歌\natsukoi_main.sec5");
            Console.WriteLine("Hello, World!");
        }
    }
}
