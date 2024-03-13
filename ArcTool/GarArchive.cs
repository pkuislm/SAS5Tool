using System.Diagnostics;

namespace ArcTool
{
    class GarArchive
    {
        class Block
        {
            public long Offset;
            public long Size;

            public Block(long offset, long size)
            {
                Offset = offset;
                Size = size;
            }

            public override string ToString()
            {
                return $"Offset: {Offset}, Size: {Size}";
            }
        }

        struct Collection
        {
            //int notused
            public byte[] CollectionProperty;
            //int collectionElemSize
            public List<CollectionElement> CollectionElem;

            public Collection(BinaryReader br)
            {
                var a = br.ReadInt32();
                var propertySize = br.ReadInt32();
                CollectionProperty = br.ReadBytes(propertySize);
                var b = br.ReadInt32();
                CollectionElem = [];

                var elemCount = br.ReadInt32();
                for (int i = 0; i < elemCount; i++)
                {
                    CollectionElem.Add(new CollectionElement(br));
                }
            }

            public Collection(List<Tuple<int, string>> input)
            {
                                       //"\x03BID\x04NAME"
                CollectionProperty = [ 0x03, 0x42, 0x49, 0x44, 0x04, 0x4E, 0x41, 0x4D, 0x45 ];
                CollectionElem = [];

                foreach(var item in input)
                {
                    CollectionElem.Add(new CollectionElement(item.Item1, item.Item2));
                }
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(2);
                bw.Write(CollectionProperty.Length);
                bw.Write(CollectionProperty);
                bw.Write(2);
                bw.Write(CollectionElem.Count);
                foreach(var elem in CollectionElem)
                {
                    elem.Write(bw);
                }
            }
        }

        struct CollectionElement
        {
            //int propertyCount = 2;
            public List<Property> Properties;//BlockID, Name

            public CollectionElement(BinaryReader br)
            {
                Properties = [];
                var propCount = br.ReadInt32();
                for (int i = 0; i < propCount; i++)
                {
                    Property p = new()
                    {
                        PropertyIndex = br.ReadInt32(),
                        Value = new PropertyValueType(br.ReadBytes(br.ReadInt32()))
                    };
                    Properties.Add(p);
                }
            }

            public CollectionElement(int fileIndex, string fileName)
            {
                Properties = [];
                Properties.Add(new Property(0, new PropertyValueType(fileIndex)));
                Properties.Add(new Property(4, new PropertyValueType(fileName)));
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(2);
                foreach(var prop in Properties)
                {
                    prop.Write(bw);
                }
            }
        }

        struct Property
        {
            public int PropertyIndex;//0 or 4
            //int valueSize
            public PropertyValueType Value;

            public Property(int index, PropertyValueType val)
            {
                PropertyIndex = index;
                Value = val;
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(PropertyIndex);
                if(Value.Value is int numVal)
                {
                    bw.Write(5);
                    bw.Write((byte)5);
                    bw.Write(numVal);
                }
                else if(Value.Value is string strVal)
                {
                    var b = CodepageManager.Instance.ImportGetBytes(strVal);
                    bw.Write(b.Length + 1);
                    bw.Write((byte)1);
                    bw.Write(b);
                }
            }

            public override string ToString()
            {
                return $"{Value}";
            }
        }

        struct PropertyValueType
        {
            public byte Type;
            public object? Value;

            public PropertyValueType(byte[] input)
            {
                Type = input[0];
                switch(input[0])
                {
                    case 1://File
                    case 2:
                    case 3://BID
                    case 4://NAME
                        Value = CodepageManager.Instance.ImportGetString(input[1..]);
                        break;
                    case 5://File
                        Value = input[1] | input[2] << 8 | input[3] << 16 | input[4] << 24;
                        break;

                }
            }

            public PropertyValueType(object obj)
            {
                Value = obj;
            }

            public override string ToString()
            {
                return $"{Value}";
            }
        }

        long m_offset;

        //Block -1:?
        //Block 0: Header
        //Block 1: コレクション
        //Block 2: BlockAllocationTable
        //Block n(n>2) : Files
        readonly Dictionary<int, Block> m_blockAllocationTable;
        Collection m_collection;
        BinaryReader m_arcReader;

        static readonly byte[] unkSectionData =  [ 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x74, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x01, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 
            0x2C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 ];

        public GarArchive(string arcName)
        {
            m_blockAllocationTable = [];

            m_arcReader = new BinaryReader(File.OpenRead(arcName));
            
            var header = m_arcReader.ReadUInt32();
            Trace.Assert(header == 0x20524147);//"GAR "

            var ver1 = m_arcReader.ReadUInt16();
            var ver2 = m_arcReader.ReadUInt16();
            
            m_offset = m_arcReader.ReadInt64();

            var a = m_arcReader.ReadUInt32();
            var b = m_arcReader.ReadUInt32();
            var c = m_arcReader.ReadUInt32();


            m_arcReader.BaseStream.Position = m_offset;
            var blockCount = m_arcReader.ReadInt32();
            var unk = m_arcReader.ReadInt64();//reader.BaseStream.Position + 20
            for (int i = 0; i < blockCount; i++)
            {
                m_blockAllocationTable.TryAdd(m_arcReader.ReadInt32(), new Block(m_arcReader.ReadInt64(), m_arcReader.ReadInt64()));
            }

            if(!m_blockAllocationTable.TryGetValue(1, out var collectionBlock))
            {
                Trace.Assert(false);
                return;
            }
            m_arcReader.BaseStream.Position = collectionBlock.Offset;
            m_collection = new Collection(m_arcReader);
        }

        public void ExtractTo(string outputPath)
        {
            if(!Path.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            foreach(var elem in m_collection.CollectionElem)
            {
                if(elem.Properties[0].Value.Value is int blockIndex && elem.Properties[1].Value.Value is string fileName)
                {
                    if(m_blockAllocationTable.TryGetValue(blockIndex, out var collectionBlock))
                    {
                        m_arcReader.BaseStream.Position = collectionBlock.Offset;
                        File.WriteAllBytes(Path.Combine(outputPath, fileName), m_arcReader.ReadBytes(Convert.ToInt32(collectionBlock.Size)));
                    }
                }
            }
        }

        public static List<Tuple<int, string>> Create(string folder, string outputArcName)
        {
            Dictionary<int, Block> blocks = [];
            using var writer = new BinaryWriter(File.Open(Path.Combine(folder, "..", outputArcName), FileMode.Create));
            writer.Write(0x20524147);
            writer.Write(1);
            long blockOffset = writer.BaseStream.Position;
            writer.Write(blockOffset);
            writer.Write(0);
            writer.Write(2);
            writer.Write(1);
            blocks.Add(0, new Block(0, writer.BaseStream.Position));
            blocks.Add(-1, new Block(writer.BaseStream.Position, unkSectionData.Length));
            writer.Write(unkSectionData);

            int fileIndex = 3;
            List<Tuple<int, string>> fileList = [];
            foreach(var file in Directory.EnumerateFiles(folder))
            {
                byte[] input = File.ReadAllBytes(file);

                blocks.Add(fileIndex, new Block(writer.BaseStream.Position, input.LongLength));
                fileList.Add(new (fileIndex, Path.GetFileName(file)));
                
                writer.Write(input);

                fileIndex++;
            }
            var curOffset = writer.BaseStream.Position;

            var collection = new Collection(fileList);
            collection.Write(writer);
            blocks.Add(1, new Block(curOffset, writer.BaseStream.Position - curOffset));

            curOffset = writer.BaseStream.Position;
            writer.BaseStream.Position = blockOffset;
            writer.Write(curOffset);
            writer.BaseStream.Position = curOffset;

            writer.Write(blocks.Count + 1);
            curOffset = writer.BaseStream.Position;
            writer.Write(curOffset);

            foreach(var k in blocks.Keys)
            {
                writer.Write(k);
                var blk = blocks[k];
                writer.Write(blk.Offset);
                writer.Write(blk.Size);
            }
            //last block
            writer.Write(2);
            writer.Write(curOffset - 4);
            var endOffset = writer.BaseStream.Position + 8;
            writer.Write(endOffset - curOffset + 24);

            writer.BaseStream.Position = curOffset;
            writer.Write(endOffset + 20);

            return fileList;
        }
    }
}
