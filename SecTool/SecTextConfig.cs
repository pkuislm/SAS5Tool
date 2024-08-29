using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SecTool
{
    public readonly struct SecTextFlag
    {
        public int NameFlag { get; }
        public int TitleFlag { get; }
        public int SelectFlag { get; }

        public SecTextFlag(int name, int title, int select)
        {
            NameFlag = name;
            TitleFlag = title;
            SelectFlag = select;
        }
    }

    public readonly struct SecGameDetail
    {
        public readonly string GameID { get; }
        public readonly string GameName { get; }
        public readonly string GameNameJP { get; }
        public readonly SecTextFlag GameFlag { get; }

        public SecGameDetail(string id, string name, string nameJP, SecTextFlag flag)
        {
            GameID = id;
            GameName = name;
            GameNameJP = nameJP;
            GameFlag = flag;
        }
    }

    public class SecTextConfig : Singleton<SecTextConfig>
    {
        private Dictionary<string, SecGameDetail>? m_config;

        public void LoadConfig()
        {
            m_config ??= [];

            var config = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("TextFlag.json"));

            if (config != null && config["flags"] is JArray arr)
            {
                foreach (var item in arr)
                {
                    try
                    {
                        m_config.TryAdd(
                            item["GameID"].Value<string>(),
                            new SecGameDetail(
                                item["GameID"].Value<string>(),
                                item["GameTitle"].Value<string>(),
                                item["GameTitleJP"].Value<string>(),
                                new SecTextFlag(
                                    item["FLG_NAME"].Value<int>(),
                                    item["FLG_TITLE"].Value<int>(),
                                    item["FLG_SELECT"].Value<int>())));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }
        }

        public bool HaveConfig(string gameID)
        {
            if (m_config == null)
                return false;
            return m_config.ContainsKey(gameID);
        }

        public SecTextFlag? GetGameFlag(string gameID)
        {
            if(HaveConfig(gameID))
            {
                return m_config[gameID].GameFlag;
            }
            return null;
        }

        public void PrintSupportedGames()
        {
            Console.WriteLine("Supported games(String inside the brackets is GameID):");
            if(m_config == null)
            {
                return;
            }
            foreach(var v in m_config.Values)
            {
                Console.WriteLine($"\t{v.GameName}/{v.GameNameJP} ({v.GameID})");
            }
        }
    }
}
