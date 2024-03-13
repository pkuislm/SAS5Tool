using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SAS5Lib.SecResource
{
    public class ResourceManager
    {
        List<ResourceRecord> Resources;

        class ResourceRecord
        {
            public string Name;
            public string Type;
            public string Source;

            public Dictionary<string, object> Properties;

            public ResourceRecord(string name, string type, string source)
            {
                Name = name;
                Type = type;
                Source = source;
                Properties = [];
            }

            public void Write(BinaryWriter bw, Dictionary<string, int> refData)
            {
                WriteResObj(bw, Name, refData);
                WriteResObj(bw, Type, refData);
                WriteResObj(bw, Source, refData);

                WriteResObj(bw, Properties.Count, refData);
                foreach(var k in Properties.Keys)
                {
                    WriteResObj(bw, k, refData);
                    WriteResObj(bw, Properties[k], refData);
                }
            }

            public override string ToString()
            {
                return $"Name: {Name}, Type: {Type}, Location: {Source}";
            }
        }

        public ResourceManager(byte[]? input) 
        {
            Resources = [];
            if (input == null)
            {
                return;
            }
            using var reader = new BinaryReader(new MemoryStream(input));

            var size = reader.ReadInt32();
            var data = reader.ReadBytes(size);
            var count = reader.ReadInt32();

            void AddRecord()
            {
                var a = ReadResObj(reader, data);
                var b = ReadResObj(reader, data);
                var c = ReadResObj(reader, data);

                if (a is string resourceName && b is string resourceType && c is string resourcePos)
                {
                    ResourceRecord record = new(resourceName, resourceType, resourcePos);
                    if (ReadResObj(reader, data) is int valCount)
                    {
                        for (int i = 0; i < valCount; i++)
                        {
                            if (ReadResObj(reader, data) is string fileName)
                            {
                                record.Properties.TryAdd(fileName, ReadResObj(reader, data));
                            }
                        }
                        Resources.Add(record);
                    }
                    else
                    {
                        Trace.Assert(false);
                    }
                }
                else
                {
                    Trace.Assert(false);
                }
            }

            for(int j = 0; j < count; j++)
            {
                AddRecord();
            }
        }

        static int ReadResVal(BinaryReader reader, byte flag)
        {
            var bitCount = (flag & 7) + 1;
            var result = 0;
            Trace.Assert(bitCount > 0 && bitCount <= 4);

            for (int i = 0; i < bitCount; i++)
            {
                result |= reader.ReadByte() << (i * 8);//LE
            }

            if (bitCount == 4)
                return result;

            var v6 = result & (1 << (8 * bitCount - 1));
            if (v6 != 0)
                result -= 2 * v6;
            return result;
        }

        static void WriteResVal(BinaryWriter writer, int value, byte flag)
        {
            int x = Math.Abs(value) >> 8;
            byte byteLen = 0;
            while(x != 0)
            {
                x >>= 8;
                byteLen++;
            }

            if ((value & (1 << (8 * (byteLen + 1) - 1))) != 0 && value > 0)
                byteLen++;

            flag |= byteLen;
            writer.Write(flag);
            for(int i = 0; i < byteLen + 1; i++)
            {
                writer.Write((byte)(value >> i * 8));
            }
        }

        static object ReadResObj(BinaryReader reader, byte[] refData)
        {
            var type = reader.ReadByte();
            if((type & 0xE0) == 0)
            {
                return (type & 0xF) - (type & 0x10);
            }

            if((type & 0xF8) == 0x80)
            {
                return ReadResVal(reader, type);
            }

            if ((type & 0xF8) == 0x90)
            {
                var offset = ReadResVal(reader, type);
                var len = refData[offset++] | refData[offset++] << 8 | refData[offset++] << 16 | refData[offset++] << 24;
                return CodepageManager.Instance.ImportGetString(refData[offset..(offset + len)]);
            }

            Trace.Assert(false);
            return null;
        }

        static void WriteResObj(BinaryWriter writer, object obj, Dictionary<string, int> refData)
        {
            if(obj is int numVal)
            {
                if (numVal <= 15 && numVal >= -16)
                {
                    if(numVal < 0)
                        numVal += 16;
                    writer.Write(Convert.ToByte(numVal));
                }
                else
                {
                    WriteResVal(writer, numVal, 0x80);
                }
            }
            else if(obj is string strVal)
            {
                if(refData.TryGetValue(strVal, out int offset))
                {
                    WriteResVal(writer, offset, 0x90);
                }
            }
        }

        public void UpdateGarResourceRecord(List<Tuple<int, string>> fileList, string newArcName)
        {
            foreach(var file in fileList)
            {
                var records = Resources.Where(record => record.Properties.ContainsKey("arc-path") && record.Properties["arc-path"].ToString().Contains(file.Item2));

                foreach(var rec in records)
                {
                    rec.Properties["path"] = newArcName;
                }
            }
        }

        public byte[] GetData()
        {
            var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            var vals = Resources.Select(record => record.Source).Distinct().ToList();
            vals.AddRange(Resources.Select(record => record.Type).Distinct().ToList());
            vals.AddRange(Resources.SelectMany(record => record.Properties.Keys).Distinct().ToList());
            vals.AddRange(Resources.SelectMany(record => record.Properties.Values).OfType<string>().Distinct().ToList());
            vals.AddRange(Resources.Select(record => record.Name).Distinct().ToList());
            vals = vals.Distinct().ToList();

            Dictionary<string, int> offsetMap = [];
            var ms2 = new MemoryStream();
            using var stringPoolWriter = new BinaryWriter(ms2);
            foreach(var record in vals)
            {
                offsetMap.TryAdd(record, Convert.ToInt32(stringPoolWriter.BaseStream.Position));
                var bytes = CodepageManager.Instance.ImportGetBytes(record);
                stringPoolWriter.Write(bytes.Length);
                stringPoolWriter.Write(bytes);
            }

            var stringPool = ms2.ToArray();
            writer.Write(stringPool.Length);
            writer.Write(stringPool);
            writer.Write(Resources.Count);

            foreach(var resource in Resources)
            {
                resource.Write(writer, offsetMap);
            }

            return ms.ToArray();
        }
    }
}
