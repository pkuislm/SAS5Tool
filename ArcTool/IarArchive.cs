using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using SAS5Lib.SecResource;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ArcTool
{
    class IarArchive
    {
        public List<long> m_arcFileOffset;
        public bool m_isArcLongOffset;
        public int m_arcVersion;
        public BinaryReader m_reader;
        public string m_arcName;

        public class IarImageFile
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
            BinaryReader m_imageDataReader;

            public IarImageFile(BinaryReader reader, long offset, int arcVersion)
            {
                var headPos = offset;
                reader.BaseStream.Position = offset;

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
                m_imageDataReader = new BinaryReader(new MemoryStream(ImageData));
            }

            byte[] GetImageData()
            {
                if(Compressed)
                {
                    var decompressor = new IarDecompressor(m_imageDataReader);
                    var data = new byte[UnpackedSize];
                    decompressor.Unpack(data);
                    return data;
                }
                else
                {
                    return ImageData;
                }
            }

            public void SaveImage(Dictionary<int, string> fileListDic, string outputPath)
            {
                //SubLayer
                if ((Flags & 0x1000) != 0)
                {
                    var layerImageOut = File.CreateText(Path.ChangeExtension(outputPath, ".layerImg"));
                    layerImageOut.WriteLine($"LayerImg({Width},{Height},{Flags},{OffsetX},{OffsetY},{Stride});");
                    int offset_x = 0, offset_y = 0;

                    if (Compressed || PackedSize != UnpackedSize)
                        m_imageDataReader = new BinaryReader(new MemoryStream(GetImageData()));

                    while (m_imageDataReader.BaseStream.Position != m_imageDataReader.BaseStream.Length)
                    {
                        int cmd = m_imageDataReader.ReadByte();
                        switch (cmd)
                        {
                            case 0x21:
                                offset_x += m_imageDataReader.ReadInt16();
                                offset_y += m_imageDataReader.ReadInt16();
                                break;

                            case 0x00:
                            case 0x20:
                            {
                                var indexImg = fileListDic[m_imageDataReader.ReadInt32()];
 
                                OffsetX -= offset_x;
                                OffsetY -= offset_y;
                                if (cmd == 0x20)
                                {
                                    layerImageOut.WriteLine($"Mask({indexImg},{offset_x},{offset_y});");
                                }
                                else
                                {
                                    layerImageOut.WriteLine($"Blend({indexImg},{offset_x},{offset_y});");
                                }
                                break;
                            }
                            default:
                                Trace.WriteLine(string.Format("Unknown layer type 0x{0:X2}", cmd), "IAR");
                                break;
                        }
                    }
                    layerImageOut.Flush();
                    layerImageOut.Close();
                }
                //SubImage
                else if ((Flags & 0x800) != 0)
                {
                    if (Compressed || PackedSize != UnpackedSize)
                        m_imageDataReader = new BinaryReader(new MemoryStream(GetImageData()));
                    var baseImgName = fileListDic[m_imageDataReader.ReadInt32()];

                    using var writer = new BinaryWriter(File.Open($"{outputPath}.subimg_{baseImgName}.subImg", FileMode.Create));
                    writer.Write(Flags);
                    writer.Write(Width);
                    writer.Write(Height);
                    writer.Write(OffsetX);
                    writer.Write(OffsetY);
                    writer.Write(Stride);
                    writer.Write(PaletteSize);
                    if (PaletteSize > 0 && PaletteData != null)
                        writer.Write(PaletteData);
                    writer.Write(m_imageDataReader.ReadBytes(UnpackedSize - 4));
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    switch (Flags & 0x3E)
                    {
                        case 0x02:
                        {
                            var image = Image.LoadPixelData<L8>(GetImageData(), Width, Height);
                            image.SaveAsPng(Path.ChangeExtension(outputPath, "png"));
                            break;
                        }
                        case 0x1C:
                        {
                            var image = Image.LoadPixelData<Bgr24>(GetImageData(), Width, Height);
                            image.SaveAsPng(Path.ChangeExtension(outputPath, "png"));
                            break;
                        }
                        case 0x3C:
                        {
                            //File.WriteAllBytes(Path.ChangeExtension(outputPath, "png"), ImageData);
                            var image = Image.LoadPixelData<Bgra32>(GetImageData(), Width, Height);
                            image.SaveAsPng(Path.ChangeExtension(outputPath, "png"));
                            break;
                        }
                        default: throw new NotSupportedException("Not supported IAR image format");
                    }
                }
            }

            static int GetImageHeaderSize(int iarVersion)
            {
                switch (iarVersion)
                {
                    case 0x1000:
                        return 0x30;
                    case 0x2000:
                    case 0x3000:
                        return 0x40;
                    case 0x4000:
                    case 0x4001:
                    case 0x4002:
                    case 0x4003:
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
                    try
                    {
                        m_bits = 1;
                        int dst = 0;
                        while (dst < output.Length)
                        {
                            if (GetNextBit() == 1)
                            {
                                output[dst++] = m_input.ReadByte();
                                continue;
                            }
                            int offset, count;
                            if (GetNextBit() == 1)// 3 <= duplicate count < 272
                            {
                                //1~8192
                                int tmp = GetNextBit();
                                if (GetNextBit() == 1)
                                    offset = 1;
                                else if (GetNextBit() == 1)
                                    offset = 0x201;
                                else
                                {
                                    tmp = (tmp << 1) | GetNextBit();
                                    if (GetNextBit() == 1)
                                        offset = 0x401;
                                    else
                                    {
                                        tmp = (tmp << 1) | GetNextBit();
                                        if (GetNextBit() == 1)
                                            offset = 0x801;
                                        else
                                        {
                                            offset = 0x1001;
                                            tmp = (tmp << 1) | GetNextBit();
                                        }
                                    }
                                }
                                offset += (tmp << 8) | m_input.ReadByte();

                                if (GetNextBit() == 1)
                                    count = 3;
                                else if (GetNextBit() == 1)
                                    count = 4;
                                else if (GetNextBit() == 1)
                                    count = 5;
                                else if (GetNextBit() == 1)
                                    count = 6;
                                else if (GetNextBit() == 1)
                                    count = 7 + GetNextBit();
                                else if (GetNextBit() == 1)
                                    count = 17 + m_input.ReadByte(); //17 ~ 272
                                else
                                {
                                    //9 ~ 16
                                    count = GetNextBit() << 2;
                                    count |= GetNextBit() << 1;
                                    count |= GetNextBit();
                                    count += 9;
                                }
                            }
                            else//duplicate count == 2 && 1 <= offset < 0x100 && 0x100 < offset <= 2047
                            {
                                count = 2;
                                if (GetNextBit() == 1)
                                {
                                    //offset >= 0x100
                                    offset = GetNextBit() << 10;
                                    offset |= GetNextBit() << 9;
                                    offset |= GetNextBit() << 8;
                                    offset = (offset | m_input.ReadByte()) + 0x100;
                                }
                                else
                                {
                                    //offset < 0xFF
                                    offset = 1 + m_input.ReadByte();//maximum == 0xFE
                                    if (0x100 == offset)//offset == 0xFF -> End
                                        break;
                                }
                            }

                            CopyOverlapped(output, dst - offset, dst, count);
                            dst += count;
                        }
                    }catch(Exception e)
                    {

                    }
                    finally
                    {
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

            internal sealed class IarCompressor
            {
                ushort m_bits;
                int m_bitsPos;
                MemoryStream m_memoryStream;
                BinaryWriter m_writer;
                MemoryStream m_bufferMemoryStream;
                BinaryWriter m_bufferWriter;

                int m_bufferPtr;
                int m_maxBufLen = 8192;
                public IarCompressor()
                {
                    m_memoryStream = new MemoryStream();
                    m_bufferMemoryStream = new MemoryStream();
                    m_writer = new BinaryWriter(m_memoryStream);
                    m_bufferWriter = new BinaryWriter(m_bufferMemoryStream);
                    m_bufferPtr = 0;
                }

                public byte[] Pack(byte[] input)
                {
                    int dataOffset = 0;
                    while(dataOffset < input.Length)
                    {
                        //Tuple<Index, Length>
                        var match = Find(input, dataOffset);
                        if(match.Item2 == 0)
                        {
                            SetBits(1);
                            m_bufferWriter.Write(input[dataOffset++]);

                            if ((dataOffset - m_bufferPtr) > m_maxBufLen)
                                m_bufferPtr++;
                        }
                        else
                        {
                            if (match.Item2 == 2)
                            {
                                SetBits(0, 2);
                                if(match.Item1 <= 0xFF)
                                {
                                    SetBits(0);
                                    m_bufferWriter.Write(Convert.ToByte(match.Item1 - 1));
                                }
                                else
                                {
                                    SetBits(1);
                                    var offset = match.Item1 - 0x100;
                                    SetBits(Convert.ToUInt16((offset >> 10) & 1), 1);
                                    SetBits(Convert.ToUInt16((offset >>  9) & 1), 1);
                                    SetBits(Convert.ToUInt16((offset >>  8) & 1), 1);
                                    m_bufferWriter.Write(Convert.ToByte(offset & 0xFF));
                                }
                            }
                            else
                            {
                                //repeats greater than 2 bytes
                                SetBits(2, 2);
                                var offset = match.Item1;
                                byte offsetPart = (byte)((offset & 0xFF) - 1);

                                ushort[] elemA = [1, 0x201, 0x401, 0x801, 0x1001];
                                ushort[] elemB = [0x40, 0x20, 8, 2, 0];
                                bool flag = false;
                                for (int j = 0; j < 0xF; j++)
                                {
                                    if (flag)
                                        break;
                                    for (int i = 0; i < 5; i++)
                                    {
                                        if (elemA[i] > match.Item1)
                                            continue;
                                        if ((elemA[i] + j * 0x100 + offsetPart) == match.Item1)
                                        {
                                            var bitlen = i switch
                                            {
                                                0 => 2,
                                                1 => 3,
                                                2 => 5,
                                                3 => 7,
                                                _ => 8,
                                            };

                                            int code = 0;
                                            if (i < 2)
                                            {
                                                code = (j & 1) << 7 | elemB[i];
                                            }
                                            else if (i < 3)
                                            {
                                                //                          1 << 7
                                                code = (j & 1) << 4 | (j & 2) << 6 | elemB[i];
                                            }
                                            else if (i < 4)
                                            {
                                                //                          1 << 4         1 << 7
                                                code = (j & 1) << 2 | (j & 2) << 3 | (j & 4) << 5 | elemB[i];
                                            }
                                            else
                                            {
                                                //                          1 << 2         1 << 4         1 << 7
                                                code = (j & 1) << 0 | (j & 2) << 1 | (j & 4) << 2 | (j & 8) << 4 | elemB[i];
                                            }

                                            for (int b = 0; b < bitlen; b++)
                                            {
                                                SetBits((ushort)((code & 0x80) >> 7));
                                                code <<= 1;
                                            }
                                            m_bufferWriter.Write(Convert.ToByte(offsetPart));
                                            flag = true;
                                            break;
                                        }
                                    }
                                }

                                switch(match.Item2)
                                {
                                    case 3:
                                        SetBits(1);
                                        break;
                                    case 4:
                                        SetBits(2, 2);//01
                                        break;
                                    case 5:
                                        SetBits(4, 3);//001
                                        break;
                                    case 6:
                                        SetBits(8, 4);//0001
                                        break;
                                    case 7:
                                        SetBits(16, 6);//000010
                                        break;
                                    case 8:
                                        SetBits(48, 6);//000011
                                        break;
                                    default:
                                    {
                                        var count = match.Item2;
                                        if (count <= 16)
                                        {
                                            SetBits(0, 6);
                                            count -= 9;
                                            SetBits((ushort)(count >> 2));
                                            SetBits((ushort)(count >> 1));
                                            SetBits((ushort)(count >> 0));
                                        }
                                        else
                                        {
                                            SetBits(32, 6);
                                            count -= 17;
                                            m_bufferWriter.Write(Convert.ToByte(count));
                                        }
                                        break;
                                    }
                                }
                            }

                            dataOffset += match.Item2;
                            if(dataOffset - m_bufferPtr > m_maxBufLen)
                                m_bufferPtr += (dataOffset - m_bufferPtr) - m_maxBufLen;
                        }
                    }
                    //Set End Flag
                    SetBits(0, 3);
                    m_bufferWriter.Write(0xFF);
                    FlushBitsStream();

                    return m_memoryStream.ToArray();
                }

                void SetBits(ushort val, int bitCount = 1)
                {
                    while(bitCount != 0)
                    {
                        if (m_bitsPos == 16)
                        {
                            m_writer.Write(m_bits);
                            m_bitsPos = 0;
                            m_bits = 0;
                            m_bufferMemoryStream.WriteTo(m_memoryStream);
                            m_bufferMemoryStream.Position = 0;
                            m_bufferMemoryStream.SetLength(0);
                        }

                        m_bits |= (ushort)((val & 1) << m_bitsPos);
                        val >>= 1;

                        //m_bits <<= 1;
                        m_bitsPos++;

                        bitCount--;
                    }
                }

                void FlushBitsStream()
                {
                    m_writer.Write(m_bits);
                    m_bitsPos = 0;
                    m_bits = 0;
                    m_bufferMemoryStream.WriteTo(m_memoryStream);
                    m_bufferMemoryStream.Position = 0;
                    m_bufferMemoryStream.SetLength(0);
                }

                //Index, Length
                public Tuple<int, int> Find(byte[] input, int inputOffset)
                {
                    var ptr = m_bufferPtr;
                    List<Tuple<int, int>> offsets = [];

                    while(ptr < inputOffset)
                    {
                        var startPos = ptr;
                        var srcPos = inputOffset;
                        var length = 0;

                        while (srcPos < input.Length && input[startPos] == input[srcPos])
                        {
                            startPos++;
                            srcPos++;
                            length++;
                            if(length > 270)
                            {
                                break;
                            }
                        }
                        if(length >= 2)
                        {
                            offsets.Add(new(inputOffset - ptr, length));
                        }
                        ptr++;
                    }
                    if(offsets.Count > 0)
                    {
                        var result = offsets.OrderByDescending(k => k.Item2).ToList();
                        return result[0];
                    }
                    else
                    {
                        return new(0, 0);
                    }
                }
            }
        }

        public IarArchive(string fileName) 
        {
            m_arcFileOffset = [];
            m_arcName = Path.GetFileName(fileName);
            m_reader = new BinaryReader(File.OpenRead(fileName));

            Trace.Assert(m_reader.ReadUInt32() == 0x20726169);

            m_arcVersion = (m_reader.ReadInt16() << 12) | (int)m_reader.ReadInt16();
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

        public void ExtractTo(Dictionary<int, string> fileListDic, string outputPath)
        {
            if (!Path.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            foreach (var key in fileListDic.Keys)
            {
                Console.WriteLine($"Writing {fileListDic[key]}...");
                IarImageFile iarImage = new(m_reader, m_arcFileOffset[key], m_arcVersion);
                iarImage.SaveImage(fileListDic, Path.Combine(outputPath, fileListDic[key]));
            }
        }
    }
}
