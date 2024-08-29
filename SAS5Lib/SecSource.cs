using System.Diagnostics;

namespace SAS5Lib
{
    public class SecSource
    {
        public class SecSourceLine
        {
            public int v1;
            public int v2;
            public int v3;
            public int v4;
            object v5;

            public SecSourceLine(BinaryReader reader)
            {
                v1 = reader.ReadInt32();
                v2 = reader.ReadInt32();
                v3 = reader.ReadInt32();
                v4 = reader.ReadInt32();
                if (v4 == 0)
                {
                    v5 = Array.Empty<byte>();
                    return;
                }
                if ((v1 & 1) != 0)
                {
                    var arr = new ushort[v4];
                    for (int i = 0; i < v4; i++)
                    {
                        arr[i] = reader.ReadUInt16();
                    }
                    v5 = arr;
                }
                else
                {
                    var arr = new uint[v4];
                    for (int i = 0; i < v4; i++)
                    {
                        arr[i] = reader.ReadUInt32();
                    }
                    v5 = arr;
                }
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(v1);
                bw.Write(v2);
                bw.Write(v3);
                bw.Write(v4);
                if (v4 == 0)
                {
                    return;
                }
                if ((v1 & 1) != 0)
                {
                    if (v5 is ushort[] arr)
                    {
                        foreach (var v in arr)
                        {
                            bw.Write(v);
                        }
                    }
                }
                else
                {
                    if (v5 is uint[] arr)
                    {
                        foreach (var v in arr)
                        {
                            bw.Write(v);
                        }
                    }
                }
            }

            public override string ToString()
            {
                return $"V1: {v1:X8} V2: {v2:X8} V3: {v3:X8} V4: {v4:X8}";
            }
        }

        public class SecSourceFile
        {
            public string Name;
            public int Position;
            public List<SecSourceLine> Positions;

            public SecSourceFile(BinaryReader reader)
            {
                Name = Utils.ReadCString(reader);
                Position = reader.ReadInt32();
                Positions = [];
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(CodepageManager.Instance.ExportGetBytes(Name));
                bw.Write((byte)0);
                bw.Write(Position);
            }

            public override string ToString()
            {
                return $"Source: {Name}, Pos: {Position:X8}, Points: {Positions.Count}";
            }
        }

        public List<SecSourceFile> SourceFiles;

        public SecSource(byte[]? input)
        {
            SourceFiles = [];

            if (input == null)
            {
                return;
            }

            using var reader = new BinaryReader(new MemoryStream(input));
            var count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                SourceFiles.Add(new SecSourceFile(reader));
            }

            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                Trace.Assert(reader.ReadInt32() == 0x54505A43);//CZPT

                foreach (var source in SourceFiles)
                {
                    var lines = reader.ReadInt32();
                    for (var i = 0; i < lines; ++i)
                    {
                        source.Positions.Add(new SecSourceLine(reader));
                    }
                }

                Trace.Assert(reader.BaseStream.Position == reader.BaseStream.Length);
            }
            else
            {
                //Old version ?
            }
        }

        public byte[] GetData()
        {
            var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(SourceFiles.Count);
            for (int i = 0; i < SourceFiles.Count; i++)
            {
                SourceFiles[i].Write(writer);
            }

            if (SourceFiles.Max(o => o.Positions.Count) > 0)
            {
                writer.Write(0x54505A43);
                for (int i = 0; i < SourceFiles.Count; i++)
                {
                    writer.Write(SourceFiles[i].Positions.Count);
                    foreach (var line in SourceFiles[i].Positions)
                    {
                        line.Write(writer);
                    }
                }
            }

            return ms.ToArray();
        }
    }
}
