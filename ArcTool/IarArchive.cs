using System.Diagnostics;
using SixLabors.ImageSharp;

namespace ArcTool
{
    class IarArchive
    {
        List<long> m_arcFileOffset;
        bool m_isArcLongOffset;
        int m_arcVersion;
        BinaryReader m_reader;

        class IarImageFile
        {
            public short Flags;
            public byte unk02;
            public bool Compressed;
            public int unk04;
            public int UnpackedSize;
            public int PaletteSize;
            public int PackedSize;
            public int unk14;
            public int OffsetX;
            public int OffsetY;
            public int Width;
            public int Height;
            public int Stride;

            byte[]? PaletteData;
            byte[] ImageData;
            public IarImageFile(BinaryReader reader, int arcVersion)
            {
                var headPos = reader.BaseStream.Position;

                Flags = reader.ReadInt16();
                unk02 = reader.ReadByte();
                Compressed = reader.ReadByte() != 0;
                unk04 = reader.ReadInt32();
                UnpackedSize = reader.ReadInt32();
                PaletteSize = reader.ReadInt32();
                PackedSize = reader.ReadInt32();
                unk14 = reader.ReadInt32();
                OffsetX = reader.ReadInt32();
                OffsetY = reader.ReadInt32();
                Width = reader.ReadInt32();
                Height = reader.ReadInt32();
                Stride = reader.ReadInt32();

                reader.BaseStream.Position += GetImageHeaderSize(arcVersion) - (reader.BaseStream.Position - headPos);

                if(PaletteSize != 0)
                {
                    PaletteData = reader.ReadBytes(PaletteSize);
                }
                ImageData = reader.ReadBytes(PackedSize);
            }

            byte[] GetImageData()
            {
                if(Compressed)
                {
                    var decompressor = new IarDecompressor(new BinaryReader(new MemoryStream(ImageData)));
                    var data = new byte[UnpackedSize];
                    decompressor.Unpack(data);
                    return data;
                }
                else
                {
                    return ImageData;
                }
            }

            public void SaveImage(string outputPath)
            {
                var bpp = Flags & 0x3E;
                switch (bpp)
                {
                    case 0x02: 
                        bpp = 8;
                        {

                        }
                        break;
                    case 0x1C: 
                        bpp = 24; 
                        break;
                    case 0x3C: 
                        bpp = 32; 
                        break;
                    default: throw new NotSupportedException("Not supported IAR image format");
                }
            }

            static int GetImageHeaderSize(int iarVersion)
            {
                switch (iarVersion)
                {
                    case 1000:
                        return 0x30;
                    case 2000:
                    case 3000:
                        return 0x40;
                    case 4000:
                    case 4001:
                    case 4002:
                    case 4003:
                        return 0x48;
                    default:
                        return 0;
                }
            }
            internal sealed class IarDecompressor
            {
                BinaryReader m_input;

                public IarDecompressor(BinaryReader input)
                {
                    m_input = input;
                }

                int m_bits = 1;

                public void Unpack(byte[] output)
                {
                    m_bits = 1;
                    int dst = 0;
                    while (dst < output.Length)
                    {
                        if (1 == GetNextBit())
                        {
                            output[dst++] = m_input.ReadByte();
                            continue;
                        }
                        int offset, count;
                        if (1 == GetNextBit())
                        {
                            int tmp = GetNextBit();
                            if (1 == GetNextBit())
                                offset = 1;
                            else if (1 == GetNextBit())
                                offset = 0x201;
                            else
                            {
                                tmp = (tmp << 1) | GetNextBit();
                                if (1 == GetNextBit())
                                    offset = 0x401;
                                else
                                {
                                    tmp = (tmp << 1) | GetNextBit();
                                    if (1 == GetNextBit())
                                        offset = 0x801;
                                    else
                                    {
                                        offset = 0x1001;
                                        tmp = (tmp << 1) | GetNextBit();
                                    }
                                }
                            }
                            offset += (tmp << 8) | m_input.ReadByte();
                            if (1 == GetNextBit())
                                count = 3;
                            else if (1 == GetNextBit())
                                count = 4;
                            else if (1 == GetNextBit())
                                count = 5;
                            else if (1 == GetNextBit())
                                count = 6;
                            else if (1 == GetNextBit())
                                count = 7 + GetNextBit();
                            else if (1 == GetNextBit())
                                count = 17 + m_input.ReadByte();
                            else
                            {
                                count = GetNextBit() << 2;
                                count |= GetNextBit() << 1;
                                count |= GetNextBit();
                                count += 9;
                            }
                        }
                        else
                        {
                            count = 2;
                            if (1 == GetNextBit())
                            {
                                offset = GetNextBit() << 10;
                                offset |= GetNextBit() << 9;
                                offset |= GetNextBit() << 8;
                                offset = (offset | m_input.ReadByte()) + 0x100;
                            }
                            else
                            {
                                offset = 1 + m_input.ReadByte();
                                if (0x100 == offset)
                                    break;
                            }
                        }
                        CopyOverlapped(output, dst - offset, dst, count);
                        dst += count;
                    }
                }

                int GetNextBit()
                {
                    if (1 == m_bits)
                    {
                        m_bits = m_input.ReadUInt16() | 0x10000;
                    }
                    int b = m_bits & 1;
                    m_bits >>= 1;
                    return b;
                }

                public static void CopyOverlapped(byte[] data, int src, int dst, int count)
                {
                    if (dst > src)
                    {
                        while (count > 0)
                        {
                            int preceding = Math.Min(dst - src, count);
                            Buffer.BlockCopy(data, src, data, dst, preceding);
                            dst += preceding;
                            count -= preceding;
                        }
                    }
                    else
                    {
                        Buffer.BlockCopy(data, src, data, dst, count);
                    }
                }
            }
        }

        public IarArchive(string fileName) 
        {
            m_arcFileOffset = [];

            m_reader = new BinaryReader(File.OpenRead(fileName));

            Trace.Assert(m_reader.ReadUInt32() == 0x20726169);

            m_arcVersion = (m_reader.ReadInt16() << 16) | (int)m_reader.ReadInt16();
            m_isArcLongOffset = m_arcVersion >= 3000;

            var headerSize = m_reader.ReadInt32();
            var infoSize = m_reader.ReadInt32();

            var trash = m_reader.ReadInt64();

            var entryCount = m_reader.ReadInt32();
            var fileCount = m_reader.ReadInt32();

            Trace.Assert(entryCount == fileCount);

            for(int i = 0; i < entryCount; i++)
            {
                m_arcFileOffset.Add(m_isArcLongOffset ? m_reader.ReadInt64() : m_reader.ReadInt32());
            }

            Trace.Assert(m_reader.BaseStream.Position == m_arcFileOffset[0]);
        }

        public void ExtractTo(List<Tuple<int, string>> fileList, string outputPath)
        {
            foreach(var file in fileList)
            {
                m_reader.BaseStream.Position = m_arcFileOffset[file.Item1];
                IarImageFile iarImage = new(m_reader, m_arcVersion);
                iarImage.SaveImage(Path.Combine(outputPath, file.Item2));
            }
        }
    }
}
