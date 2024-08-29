using Newtonsoft.Json;
using System.Diagnostics;

namespace SAS5Lib.SecOption
{
    public class OptionManager
    {
        SecOptionMap? _secOptionMap;

        public OptionManager(byte[]? input)
        {
            if (input == null)
            {
                return;
            }
            using var reader = new BinaryReader(new MemoryStream(input));
            //Must be an OptionMap
            Trace.Assert(reader.ReadByte() == 0);
            _secOptionMap = new SecOptionMap(reader);
        }

        public byte[] GetData()
        {
            if(_secOptionMap == null)
            {
                return [];
            }
            var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            _secOptionMap.Save(writer);
            return ms.ToArray();
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(_secOptionMap, Formatting.Indented , new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects }));
        }

        public void Load(string path, bool debugBuild)
        {
            _secOptionMap = JsonConvert.DeserializeObject<SecOptionMap>(File.ReadAllText(path), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects });
            if (debugBuild)
            {
                if (_secOptionMap != null
                && _secOptionMap.Map.TryGetValue("Title", out var val)
                && val is SecOptionString s1
                && _secOptionMap.Map.TryGetValue("Registry", out var val2)
                && val2 is SecOptionMap m1
                && m1.Map.TryGetValue("Application", out var val3)
                && val3 is SecOptionString s2)
                {
                    var suffix = $" Debug build[{DateTime.Now.ToString().Replace('/', '-').Replace(':', '-')}]";
                    s1.Value.Text += suffix;
                    s2.Value.Text += suffix;
                    s1.Value.IsEdited = true;
                    s2.Value.IsEdited = true;
                }
            }
        }

        public void UpdateExportFuncAddr(Dictionary<long, long> addresses)
        {
            var opt = GetOptionByName("EXPORT_FUNS");
            if (opt is SecOptionMap mapVal)
            {
                foreach(var k in mapVal.Map.Keys)
                {
                    if (mapVal.Map[k] is not SecOptionInteger optionInt)
                    {
                        continue;
                    }
                    if(addresses.TryGetValue(optionInt.Value, out var newAddress))
                    {
                        Console.WriteLine($"{k}: {optionInt.Value} -> {newAddress}");
                        optionInt.Value = Convert.ToInt32(newAddress);
                    }
                    else
                    {
                        throw new Exception("Unknown export function addr.");
                    }
                }
            }
        }

        public OptionType? GetOptionByName(string name)
        {
            if (_secOptionMap != null && _secOptionMap.Map.TryGetValue(name, out var ret))
            {
                return ret;
            }
            else
            {
                return null;
            }
        }

        public void PrintGameInfo()
        {
            try
            {
                var saveDataGuid = ((SecOptionString)GetOptionByName("ContextFileGameGuid")).Value.Text;
                var saveDataKey = ((SecOptionInteger)GetOptionByName("ContextKey")).Value;
                var saveDataVersion = ((SecOptionString)GetOptionByName("ContextVersion")).Value.Text;
                var gameId = ((SecOptionString)GetOptionByName("GlobalAppId")).Value.Text;

                var gameInfo = ((SecOptionMap)GetOptionByName("Registry")).Map;
                var gameName = ((SecOptionString)gameInfo["Application"]).Value.Text;
                var gameVersion = ((SecOptionString)gameInfo["Category"]).Value.Text;
                var gameManufacturer = ((SecOptionString)gameInfo["Manufacturer"]).Value.Text;

                Console.WriteLine("--------------Game Info---------------");
                Console.WriteLine($"Name: {gameName} (Ver {gameVersion})");
                Console.WriteLine($"AppID: {gameId}");
                Console.WriteLine($"Manufacturer: {gameManufacturer}\n");
                Console.WriteLine($"SaveDataGUID: {saveDataGuid}");
                Console.WriteLine($"SaveDataKey: 0x{saveDataKey:X4}");
                Console.WriteLine($"SaveDataVersion: {saveDataVersion}");
                Console.WriteLine("--------------------------------------\n");
            }
            catch
            {

            }
        }
    }
}
