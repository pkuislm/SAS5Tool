using System.Text;

namespace SAS5Lib
{
    public class SecScenarioProgram
    {
        readonly Dictionary<string, byte[]> _sections;
        public static int Version { get; private set; }
        public static bool LegacyVersion { get; private set; }

        public SecScenarioProgram(string path)
        {
            _sections = [];
            using var reader = new BinaryReader(File.OpenRead(path));
            if (reader.ReadInt32() != 0x35434553)//SEC5
            {
                Console.WriteLine("Invalid Sec5File format.");
                return;
            }
            Version = reader.ReadInt32();
            LegacyVersion = Version < 109000;

            if(Version < 108000)
            {
                throw new NotSupportedException($"This sec5 (version {Version}) is too old!");
            }

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var sectionName = Encoding.ASCII.GetString(reader.ReadBytes(4));
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
            PrintSecProgramInfo();
        }

        public void Save(string path)
        {
            using var writer = new BinaryWriter(File.Open(path, FileMode.Create));

            writer.Write(0x35434553);
            writer.Write(Version);

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

        public void PrintSecProgramInfo()
        {
            Console.WriteLine("--------------Code Info---------------");
            Console.WriteLine($"Version: {Version}");
            Console.WriteLine($"Section(s): {string.Join(',', _sections.Keys)}");
            Console.WriteLine("--------------------------------------\n");
        }
    }
}
