namespace SAS5Lib
{
    public class SecScenarioProgram
    {
        readonly Dictionary<string, byte[]> _sections;
        readonly int _version;

        public SecScenarioProgram(string path)
        {
            _sections = [];
            using var reader = new BinaryReader(File.OpenRead(path));
            if (reader.ReadInt32() != 0x35434553)//SEC5
            {
                Console.WriteLine("Invalid Sec5File format.");
                return;
            }
            _version = reader.ReadInt32();

            static string ReadSectionName(BinaryReader reader)
            {
                return CodepageManager.Instance.ImportGetString(reader.ReadBytes(4));
            }

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var sectionName = ReadSectionName(reader);
                var sectionData = reader.ReadBytes(reader.ReadInt32());
                if (sectionName == "CODE")
                {
                    byte k = 0, b = 0;
                    for (var i = 0; i < sectionData.Length; i++)
                    {
                        b = sectionData[i];
                        sectionData[i] ^= k;
                        k += (byte)(b + 0x12);
                    }
                }
                _sections.TryAdd(sectionName, sectionData);
            }
        }

        public void Save(string path)
        {
            using var writer = new BinaryWriter(File.Open(path, FileMode.Create));

            writer.Write(0x35434553);
            writer.Write(_version);

            static byte[] GetSectionName(string name)
            {
                return CodepageManager.Instance.ImportGetBytes(name)[..4];
            }

            foreach (var sectionName in _sections.Keys)
            {
                writer.Write(GetSectionName(sectionName));
                var sectionData = _sections[sectionName];
                writer.Write(sectionData.Length);
                if (sectionName == "CODE")
                {
                    byte k = 0;
                    for (var i = 0; i < sectionData.Length; i++)
                    {
                        sectionData[i] ^= k;
                        k += (byte)(sectionData[i] + 0x12);
                    }
                }
                writer.Write(sectionData);
            }
        }

        public byte[]? GetSectionData(string sectionName)
        {
            if (_sections.TryGetValue(sectionName, out var ret))
            {
                return ret;
            }
            else
            {
                return null;
            }
        }

        public void SetSectionData(string sectionName, byte[] sectionData)
        {
            if (_sections.ContainsKey(sectionName))
            {
                _sections[sectionName] = sectionData;
            }
        }
    }
}
