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
            if(_secOptionMap != null 
                && _secOptionMap.Map.TryGetValue("EXPORT_FUNS", out var val) 
                && val is SecOptionMap mapVal)
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
    }
}
