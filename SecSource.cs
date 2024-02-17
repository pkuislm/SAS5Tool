using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAS5CodeDisasembler
{
    class SecSource
    {
        class SecSourceLine
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
                if(v4 == 0)
                {
                    v5 = Array.Empty<byte>();
                    return;
                }
                if((v1 & 1) != 0)
                {
                    var arr = new ushort[v4];
                    for(int i = 0; i < v4; i++)
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

            public override string ToString()
            {
                return $"V1: {v1:X8} V2: {v2:X8} V3: {v3:X8} V4: {v4:X8}";
            }
        }

        class SecSourceFile
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

            public override string ToString()
            {
                return $"Source: {Name}, Pos: {Position:X8}, Points: {Positions.Count}";
            }
        }

        List<SecSourceFile> _sourceFiles;

        public SecSource(byte[]? input)
        {
            _sourceFiles = [];

            if(input == null)
            {
                return;
            }

            using var reader = new BinaryReader(new MemoryStream(input));
            var count = reader.ReadInt32();

            for(int i = 0; i < count; i++)
            {
                _sourceFiles.Add(new SecSourceFile(reader));
            }

            Trace.Assert(reader.ReadInt32() == 0x54505A43);//CZPT

            foreach(var source in _sourceFiles)
            {
                var lines = reader.ReadInt32();
                for(var i = 0; i < lines; ++i)
                {
                    source.Positions.Add(new SecSourceLine(reader));
                }
            }
            Trace.Assert(reader.BaseStream.Position == reader.BaseStream.Length);
        }
    }
}
