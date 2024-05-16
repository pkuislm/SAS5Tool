using System.Diagnostics;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
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

        public class IarImage
        {
            public static byte[] Deflate(byte[] buffer)
            {
                Deflater deflater = new Deflater(Deflater.BEST_COMPRESSION);
                using (MemoryStream memoryStream = new MemoryStream())
                using (DeflaterOutputStream deflaterOutputStream = new DeflaterOutputStream(memoryStream, deflater))
                {
                    deflaterOutputStream.Write(buffer, 0, buffer.Length);
                    deflaterOutputStream.Flush();
                    deflaterOutputStream.Finish();

                    return memoryStream.ToArray();
                }
            }

            public static byte[] Inflate(byte[] buffer)
            {
                byte[] block = new byte[256];
                MemoryStream outputStream = new MemoryStream();

                Inflater inflater = new Inflater();
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                using (InflaterInputStream inflaterInputStream = new InflaterInputStream(memoryStream, inflater))
                {
                    while (true)
                    {
                        int numBytes = inflaterInputStream.Read(block, 0, block.Length);
                        if (numBytes < 1)
                            break;
                        outputStream.Write(block, 0, numBytes);
                    }
                }

                return outputStream.ToArray();
            }

            public static void Extract(BinaryReader reader, long offset, int arcVersion, Dictionary<int, string> fileListDic, string outputPath)
            {
                var headPos = offset;
                reader.BaseStream.Position = offset;

                var Flags = reader.ReadInt16();
                var unk02 = reader.ReadByte();
                var Compressed = reader.ReadByte() != 0;
                var unk04 = reader.ReadInt32();
                var UnpackedSize = reader.ReadInt32();
                var PaletteSize = reader.ReadInt32();
                var PackedSize = reader.ReadInt32();
                var unk14 = reader.ReadInt32();
                var OffsetX = reader.ReadInt32();
                var OffsetY = reader.ReadInt32();
                var Width = reader.ReadInt32();
                var Height = reader.ReadInt32();
                var Stride = reader.ReadInt32();

                var metadataSize = Convert.ToInt32(GetImageHeaderSize(arcVersion) - (reader.BaseStream.Position - headPos));
                var metadataStr = Convert.ToBase64String(Deflate(reader.ReadBytes(metadataSize))).Replace('/', '`');

                var PaletteData = PaletteSize != 0 ? reader.ReadBytes(PaletteSize) : [];


                byte[]? ImageData;
                if (Compressed)
                {
                    ImageData = new byte[UnpackedSize];
                    IarDecompressor.Unpack(reader, ImageData);
                }
                else
                {
                    ImageData = reader.ReadBytes(PackedSize);
                }

                using var imageDataReader = new BinaryReader(new MemoryStream(ImageData));
                //SubLayer
                if ((Flags & 0x1000) != 0)
                {
                    using var layerImageOut = File.CreateText($"{outputPath}.layerImg");
                    layerImageOut.WriteLine($"LayerImg({Width},{Height},{Flags},{OffsetX},{OffsetY},{Stride},{metadataStr});");
                    int offset_x = 0, offset_y = 0;

                    while (imageDataReader.BaseStream.Position != imageDataReader.BaseStream.Length)
                    {
                        int cmd = imageDataReader.ReadByte();
                        switch (cmd)
                        {
                            case 0x21:
                                offset_x += imageDataReader.ReadInt16();
                                offset_y += imageDataReader.ReadInt16();
                                break;

                            case 0x00:
                            case 0x20:
                            {
                                var indexImg = fileListDic[imageDataReader.ReadInt32()];
 
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
                                Trace.WriteLine($"Unknown layer type 0x{cmd:X8}", "IAR");
                                break;
                        }
                    }
                    layerImageOut.Flush();
                    layerImageOut.Close();
                }
                //SubImage
                else if ((Flags & 0x800) != 0)
                {
                    var baseImgName = fileListDic[imageDataReader.ReadInt32()];

                    using var writer = new BinaryWriter(File.Open($"{outputPath}.base_{baseImgName}.{metadataStr}.subImg", FileMode.Create));
                    writer.Write(Flags);
                    writer.Write(Width);
                    writer.Write(Height);
                    writer.Write(OffsetX);
                    writer.Write(OffsetY);
                    writer.Write(Stride);
                    writer.Write(PaletteSize);
                    if (PaletteSize > 0 && PaletteData != null)
                        writer.Write(PaletteData);
                    writer.Write(imageDataReader.ReadBytes(UnpackedSize - 4));
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    switch (Flags & 0x3E)
                    {
                        case 0x02:
                        {
                            using var image = Image.LoadPixelData<L8>(ImageData, Width, Height);
                            image.SaveAsPng($"{outputPath}.{OffsetX}_{OffsetY}.{metadataStr}.png");
                            break;
                        }
                        case 0x1C:
                        {
                            using var image = Image.LoadPixelData<Bgr24>(ImageData, Width, Height);
                            image.SaveAsPng($"{outputPath}.{OffsetX}_{OffsetY}.{metadataStr}.png");
                            break;
                        }
                        case 0x3C:
                        {
                            using var image = Image.LoadPixelData<Bgra32>(ImageData, Width, Height);
                            image.SaveAsPng($"{outputPath}.{OffsetX}_{OffsetY}.{metadataStr}.png");
                            break;
                        }
                        default: throw new NotSupportedException("Not supported IAR image format");
                    }
                }
            }

            public static void Import(BinaryWriter writer, string inputFile, int fileIndex, Dictionary<string, int> fileMap, int arcVersion)
            {
                short Flags = 0;
                byte unk02 = 0;
                byte Compressed;
                int unk04 = 0;
                int UnpackedSize;
                int PaletteSize = 0;
                int PackedSize;
                int unk14 = 0;
                int OffsetX = 0;
                int OffsetY = 0;
                int Width = 0;
                int Height = 0;
                int Stride = 0;
                byte[]? ImageData = null;
                byte[]? PaletteData = null;
                byte[]? MetaData = null;
                string entryName = "";
                switch (Path.GetExtension(inputFile))
                {
                    case ".png":
                    {
                        var match = Regex.Match(Path.GetFileName(inputFile), @"(.+)\.(.+)_(.+)\.(.+)\.png");
                        if(!match.Success)
                        {
                            Console.WriteLine($"Invalid png file name: {inputFile}.");
                            return;
                        }
                        entryName = match.Groups[1].Value;
                        using var image = Image.Load(inputFile);
                        Width = image.Width;
                        Height = image.Height;
                        Stride = image.Width;
                        OffsetX = Convert.ToInt32(match.Groups[2].Value);
                        OffsetY = Convert.ToInt32(match.Groups[3].Value);
                        MetaData = Inflate(Convert.FromBase64String(match.Groups[4].Value.Replace('`', '/')));

                        switch (image.PixelType.BitsPerPixel)
                        {
                            case 8:
                            {
                                Flags = 2;
                                ImageData = new byte[Stride * Height];
                                image.CloneAs<L8>().CopyPixelDataTo(ImageData);
                                break;
                            }
                            case 24:
                            {
                                Flags = 0x1C;
                                Stride *= 3;
                                ImageData = new byte[Stride * Height];
                                image.CloneAs<Bgr24>().CopyPixelDataTo(ImageData);
                                break;
                            }
                            case 32:
                            {
                                Flags = 0x3C;
                                Stride *= 4;
                                ImageData = new byte[Stride * Height];
                                image.CloneAs<Bgra32>().CopyPixelDataTo(ImageData);
                                break;
                            }
                            default:
                            {
                                Console.WriteLine($"Unsupported bpp({image.PixelType.BitsPerPixel}): {inputFile}.");
                                return;
                            }
                        }
                        break;
                    }
                    case ".layerImg":
                    {
                        entryName = Path.GetFileNameWithoutExtension(inputFile);
                        var texts = File.ReadAllLines(inputFile);
                        if (texts.Length == 0)
                        {
                            Console.WriteLine($"Empty layerImg: {inputFile}.");
                            return;
                        }

                        var header = Regex.Match(texts[0], @"LayerImg\((.+),(.+),(.+),(.+),(.+),(.+),(.+)\);");
                        if(!header.Success)
                        {
                            Console.WriteLine($"Invalid layerImg property: {inputFile}.");
                            return;
                        }

                        Width = Convert.ToInt32(header.Groups[1].Value);
                        Height = Convert.ToInt32(header.Groups[2].Value);
                        Flags = Convert.ToInt16(header.Groups[3].Value);
                        OffsetX = Convert.ToInt32(header.Groups[4].Value);
                        OffsetY = Convert.ToInt32(header.Groups[5].Value);
                        Stride = Convert.ToInt32(header.Groups[6].Value);
                        MetaData = Inflate(Convert.FromBase64String(header.Groups[7].Value.Replace('`', '/')));

                        var ms = new MemoryStream();

                        {
                            var imageDataWriter = new BinaryWriter(ms);
                            short offset_x = 0, offset_y = 0;
                            for (int i = 1; i < texts.Length; i++)
                            {
                                var cmd = Regex.Match(texts[i], @"(Mask|Blend)\((.+),(.+),(.+)\);");

                                if (cmd.Success)
                                {
                                    var x = Convert.ToInt16(cmd.Groups[3].Value);
                                    var y = Convert.ToInt16(cmd.Groups[4].Value);

                                    if (fileMap.TryGetValue(cmd.Groups[2].Value, out int k))
                                    {
                                        imageDataWriter.Write((byte)0x21);
                                        imageDataWriter.Write(Convert.ToInt16(x - offset_x));
                                        imageDataWriter.Write(Convert.ToInt16(y - offset_y));
                                        imageDataWriter.Write(cmd.Groups[1].Value == "Mask" ? (byte)0x20 : (byte)0x00);
                                        imageDataWriter.Write(k);
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Cannot find file {cmd.Groups[2].Value} needed by layerImg {inputFile}, skipping command \"{texts[i]}\".");
                                    }
                                    offset_x = x;
                                    offset_y = y;
                                }
                                else
                                {
                                    Console.WriteLine($"Invalid layerImg command: {texts[i]}.");
                                }
                            }
                        }
                        ImageData = ms.ToArray();
                        break;
                    }
                    case ".subImg":
                    {
                        var match = Regex.Match(Path.GetFileName(inputFile), @"(.+)\.base_(.+)\.(.+)\.subImg");
                        if(!match.Success)
                        {
                            Console.WriteLine($"Invalid subImage file name.");
                            return;
                        }
                        entryName = match.Groups[1].Value;

                        if (!fileMap.TryGetValue(match.Groups[2].Value, out int k))
                        {
                            Console.WriteLine($"Cannot find base image {match.Groups[2].Value} needed by {entryName}.");
                            return;
                        }

                        MetaData = Inflate(Convert.FromBase64String(match.Groups[3].Value.Replace('`', '/')));

                        var ms = new MemoryStream();

                        {
                            using var reader = new BinaryReader(File.Open(inputFile, FileMode.Open));
                            Flags = reader.ReadInt16();
                            Width = reader.ReadInt32();
                            Height = reader.ReadInt32();
                            OffsetX = reader.ReadInt32();
                            OffsetY = reader.ReadInt32();
                            Stride = reader.ReadInt32();
                            PaletteSize = reader.ReadInt32();

                            if (PaletteSize > 0)
                                PaletteData = reader.ReadBytes(PaletteSize);

                            var imageDataWriter = new BinaryWriter(ms);
                            imageDataWriter.Write(k);
                            imageDataWriter.Write(reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length - reader.BaseStream.Position)));
                        }
                        ImageData = ms.ToArray();
                        break;
                    }
                }

                Trace.Assert(ImageData != null);
                UnpackedSize = PackedSize = ImageData.Length;
                var packedData = IarCompressor.Pack(ImageData);

                if (UnpackedSize > packedData.Length)
                {
                    Compressed = 1;
                    ImageData = packedData;
                    PackedSize = packedData.Length;
                }
                else
                {
                    Compressed = 0;
                }

                var basePos = writer.BaseStream.Position;

                writer.Write(Flags);
                writer.Write(unk02);
                writer.Write(Compressed);
                writer.Write(unk04);
                writer.Write(UnpackedSize);
                writer.Write(PaletteSize);
                writer.Write(PackedSize);
                writer.Write(unk14);
                writer.Write(OffsetX);
                writer.Write(OffsetY);
                writer.Write(Width);
                writer.Write(Height);
                writer.Write(Stride);

                var metadataSize = Convert.ToInt32(GetImageHeaderSize(arcVersion) - (writer.BaseStream.Position - basePos));
                if(MetaData != null)
                {
                    if(MetaData.Length > metadataSize)
                    {
                        Console.WriteLine($"Metadata size not match({MetaData.Length}/{metadataSize}), turncating.");
                        MetaData = MetaData[..metadataSize];
                        writer.Write(MetaData);
                    }
                    else
                    {
                        writer.Write(MetaData);
                        writer.BaseStream.Position += metadataSize - MetaData.Length;
                    }
                }
                else
                {
                    writer.BaseStream.Position += metadataSize;
                }


                if (PaletteSize != 0 && PaletteData != null)
                {
                    writer.Write(PaletteData);
                }
                writer.Write(ImageData);

                fileMap.TryAdd(entryName, fileIndex);
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
                class BitHelper
                {
                    readonly BinaryReader m_reader;
                    int m_bits = 1;

                    public BitHelper(BinaryReader reader)
                    {
                        m_reader = reader;
                    }

                    public int GetNextBit()
                    {
                        if (1 == m_bits)
                        {
                            m_bits = m_reader.ReadUInt16() | 0x10000;
                        }
                        int b = m_bits & 1;
                        m_bits >>= 1;
                        return b;
                    }
                }

                public static void Unpack(BinaryReader input, byte[] output)
                {
                    var bh = new BitHelper(input);
                        
                    int dst = 0;
                    while (dst < output.Length)
                    {
                        if (bh.GetNextBit() == 1)
                        {
                            output[dst++] = input.ReadByte();
                            continue;
                        }
                        int offset, count;
                        if (bh.GetNextBit() == 1)// 3 <= duplicate count < 272
                        {
                            //1~8192
                            int tmp = bh.GetNextBit();
                            if (bh.GetNextBit() == 1)
                                offset = 1;
                            else if (bh.GetNextBit() == 1)
                                offset = 0x201;
                            else
                            {
                                tmp = (tmp << 1) | bh.GetNextBit();
                                if (bh.GetNextBit() == 1)
                                    offset = 0x401;
                                else
                                {
                                    tmp = (tmp << 1) | bh.GetNextBit();
                                    if (bh.GetNextBit() == 1)
                                        offset = 0x801;
                                    else
                                    {
                                        offset = 0x1001;
                                        tmp = (tmp << 1) | bh.GetNextBit();
                                    }
                                }
                            }
                            offset += (tmp << 8) | input.ReadByte();

                            if (bh.GetNextBit() == 1)
                                count = 3;
                            else if (bh.GetNextBit() == 1)
                                count = 4;
                            else if (bh.GetNextBit() == 1)
                                count = 5;
                            else if (bh.GetNextBit() == 1)
                                count = 6;
                            else if (bh.GetNextBit() == 1)
                                count = 7 + bh.GetNextBit();
                            else if (bh.GetNextBit() == 1)
                                count = 17 + input.ReadByte(); //17 ~ 272
                            else
                            {
                                //9 ~ 16
                                count = bh.GetNextBit() << 2;
                                count |= bh.GetNextBit() << 1;
                                count |= bh.GetNextBit();
                                count += 9;
                            }
                        }
                        else//duplicate count == 2
                        {
                            count = 2;
                            if (bh.GetNextBit() == 1)
                            {
                                //offset = 0x100 ~ 0x8FF (256 ~ 2303)
                                offset = bh.GetNextBit() << 10;
                                offset |= bh.GetNextBit() << 9;
                                offset |= bh.GetNextBit() << 8;
                                offset = (offset | input.ReadByte()) + 0x100;
                            }
                            else
                            {
                                //offset = 0x1 ~ 0xFF
                                offset = 1 + input.ReadByte();//maximum == 0xFE
                                if (0x100 == offset)//offset == 0xFF -> End
                                    break;
                            }
                        }
                        CopyOverlapped(output, dst - offset, dst, count);
                        dst += count;
                    }
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
                public class BufferWriter
                {
                    private ushort m_ctl;
                    private int m_pos;
                    private readonly List<byte> m_outputBuf;
                    private readonly List<byte> m_codeBuf;

                    static readonly ushort[] elemA = [1, 0x201, 0x401, 0x801, 0x1001];
                    static readonly ushort[] elemB = [0x40, 0x20, 8, 2, 0];
                    public BufferWriter()
                    {
                        m_outputBuf = [];
                        m_codeBuf = [];
                        m_ctl = 0;
                        m_pos = 0;
                    }

                    void SetBits(ushort val, int bitCount = 1)
                    {
                        while (bitCount != 0)
                        {
                            if (m_pos == 16)
                            {
                                Flush();
                            }

                            m_ctl |= (ushort)((val & 1) << m_pos++);
                            val >>= 1;

                            bitCount--;
                        }
                    }

                    public void PutUncoded(byte input)
                    {
                        SetBits(1);
                        m_codeBuf.Add(input);
                    }

                    public void PutPair(int offset, int length)
                    {
                        if (length < 2)
                            throw new ArgumentException("Cannot put pair that length lower than 2.");
                        if (length == 2)
                        {
                            SetBits(0, 2);
                            if (offset <= 0xFF)
                            {
                                SetBits(0);
                                m_codeBuf.Add(Convert.ToByte(offset - 1));
                            }
                            else
                            {
                                SetBits(1);
                                offset -= 0x100;
                                SetBits(Convert.ToUInt16((offset >> 10) & 1), 1);
                                SetBits(Convert.ToUInt16((offset >> 9) & 1), 1);
                                SetBits(Convert.ToUInt16((offset >> 8) & 1), 1);
                                m_codeBuf.Add(Convert.ToByte(offset & 0xFF));
                            }
                        }
                        else
                        {
                            //repeats greater than 2 bytes
                            SetBits(2, 2);
                            byte offsetPart = (byte)((offset & 0xFF) - 1);

                            bool flag = false;
                            for (int j = 0; j <= 0xF; j++)
                            {
                                if (flag)
                                    break;
                                for (int i = 0; i < 5; i++)
                                {
                                    if (elemA[i] > offset)
                                        continue;
                                    if ((elemA[i] + (j << 8) + offsetPart) == offset)
                                    {
                                        var bitlen = i switch
                                        {
                                            0 => 2,
                                            1 => 3,
                                            2 => 5,
                                            3 => 7,
                                            _ => 8,
                                        };

                                        int code;
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
                                        m_codeBuf.Add(Convert.ToByte(offsetPart));
                                        flag = true;
                                        break;
                                    }
                                }
                            }

                            switch (length)
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
                                    var count = length;
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
                                        m_codeBuf.Add(Convert.ToByte(count));
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    public void Flush()
                    {
                        m_pos = 0;

                        m_outputBuf.Add((byte)m_ctl);
                        m_outputBuf.Add((byte)(m_ctl >> 8));
                        m_ctl = 0;

                        m_outputBuf.AddRange(m_codeBuf);
                        m_codeBuf.Clear();
                    }

                    public byte[] GetBytes()
                    {
                        //Set End Flag
                        SetBits(0, 3);
                        m_codeBuf.Add(0xFF);
                        Flush();
                        return m_outputBuf.ToArray();
                    }
                }

                //RingBuffer, must be power of 2
                const int BufferSize = 1 << 13;
                //Maximum matching size
                const int SearchSize = 255;
                //Minimum pair length
                const int THRESHOLD = 2;

                const int NIL = BufferSize;
                //Original source: https://github.com/opensource-apple/kext_tools/blob/master/compression.c
                public class EncodeState
                {
                    /*
                     * initialize state, mostly the trees
                     *
                     * For i = 0 to BufferSize - 1, rchild[i] and lchild[i] will be the right and left 
                     * children of node i.  These nodes need not be initialized.  Also, parent[i] 
                     * is the parent of node i.  These (parent nodes) are initialized to NIL (= BufferSize), which stands 
                     * for 'not used.'  For i = 0 to 255, rchild[BufferSize + i + 1] is the root of the 
                     * tree for strings that begin with character i.  These are initialized to NIL. 
                     * Note there are 256 trees. 
                     */
                    public int[] lchild = new int[BufferSize + 1];
                    public int[] rchild = Enumerable.Repeat(NIL, BufferSize + 1 + 256).ToArray();
                    public int[] parent = Enumerable.Repeat(NIL, BufferSize + 1).ToArray();

                    public byte[] text_buf = Enumerable.Repeat((byte)0xFF, BufferSize + SearchSize + 1).ToArray();
                    public int[] text_buf_map = new int[BufferSize + SearchSize + 1];

                    public int match_position = 0;
                    public int match_length = 0;
                };

                /*
                 * Inserts string of (length=SearchSize, text_buf[index..index + SearchSize - 1]) into one of the trees
                 * (text_buf[index]'th tree) and returns the longest-match position and length
                 * via the global variables match_position and match_length.
                 * If match_length = SearchSize, then removes the old node in favor of the new one,
                 * because the old one will be deleted sooner. Note index plays double role,
                 * as tree node and position in buffer.
                 */
                static void InsertNode(EncodeState sp, int index)
                {
                    int cmp = 1;
                    int p = BufferSize + sp.text_buf[index] + 1;//find root node of text_buf[index]'s tree
                    sp.rchild[index] = sp.lchild[index] = NIL;
                    sp.match_length = 0;
                    for (; ; )
                    {
                        if (cmp >= 0)
                        {
                            if (sp.rchild[p] != NIL)
                                p = sp.rchild[p];
                            else
                            {
                                sp.rchild[p] = index;
                                sp.parent[index] = p;
                                return;
                            }
                        }
                        else
                        {
                            if (sp.lchild[p] != NIL)
                                p = sp.lchild[p];
                            else
                            {
                                sp.lchild[p] = index;
                                sp.parent[index] = p;
                                return;
                            }
                        }

                        //Faster string comparsion
                        var i = 1;
                        while(i < SearchSize)
                        {
                            var u = Vector256.Create(sp.text_buf, index + i);
                            var v = Vector256.Create(sp.text_buf,     p + i);
                            var w = BitOperations.TrailingZeroCount(~Avx2.MoveMask(Avx2.CompareEqual(u, v)));

                            i += w;
                            if(w != 32)
                            {
                                break;
                            }
                        }
                        if (i > SearchSize)
                            i = SearchSize;

                        cmp = sp.text_buf[index + i] - sp.text_buf[p + i];

                        if (i > sp.match_length)
                        {
                            sp.match_position = p;
                            if ((sp.match_length = i) >= SearchSize)
                                break;
                        }
                    }
                    sp.parent[index] = sp.parent[p];
                    sp.lchild[index] = sp.lchild[p];
                    sp.rchild[index] = sp.rchild[p];
                    sp.parent[sp.lchild[p]] = index;
                    sp.parent[sp.rchild[p]] = index;
                    if (sp.rchild[sp.parent[p]] == p)
                        sp.rchild[sp.parent[p]] = index;
                    else
                        sp.lchild[sp.parent[p]] = index;
                    sp.parent[p] = NIL;  /* remove p */
                }

                /* deletes node p from tree */
                static void DeleteNode(EncodeState sp, int p)
                {
                    int q;
                    if (sp.parent[p] == NIL)
                        return;  /* not in tree */
                    if (sp.rchild[p] == NIL)
                        q = sp.lchild[p];
                    else if (sp.lchild[p] == NIL)
                        q = sp.rchild[p];
                    else
                    {
                        q = sp.lchild[p];
                        if (sp.rchild[q] != NIL)
                        {
                            do
                            {
                                q = sp.rchild[q];
                            } while (sp.rchild[q] != NIL);
                            sp.rchild[sp.parent[q]] = sp.lchild[q];
                            sp.parent[sp.lchild[q]] = sp.parent[q];
                            sp.lchild[q] = sp.lchild[p];
                            sp.parent[sp.lchild[p]] = q;
                        }
                        sp.rchild[q] = sp.rchild[p];
                        sp.parent[sp.rchild[p]] = q;
                    }
                    sp.parent[q] = sp.parent[p];
                    if (sp.rchild[sp.parent[p]] == p)
                        sp.rchild[sp.parent[p]] = q;
                    else
                        sp.lchild[sp.parent[p]] = q;
                    sp.parent[p] = NIL;
                }

                public static byte[] Pack(byte[] input)
                {
                    EncodeState sp = new();

                    int i;
                    int len, last_match_length;

                    int r = BufferSize - SearchSize;
                    int s = 0;
                    int inputIdx = 0;

                    /* Read F bytes into the last F bytes of the buffer(wait for search) */
                    for (len = 0; len < SearchSize && inputIdx < input.Length; len++)
                    {
                        sp.text_buf[r + len] = input[inputIdx];
                        sp.text_buf_map[r + len] = inputIdx;
                        inputIdx++;
                    }

                    /*
                     * Insert the whole string just read.
                     * The global variables match_length and match_position are set.
                     */
                    InsertNode(sp, r);

                    var bw = new BufferWriter();

                    var encode_pos = 0;
                    do
                    {
                        if (encode_pos == 0 || sp.match_length < THRESHOLD)
                        {
                            sp.match_length = 1;
                            bw.PutUncoded(sp.text_buf[r]);
                            encode_pos++;
                        }
                        else
                        {
                            var offset = encode_pos - sp.text_buf_map[sp.match_position];

                            if(offset > 2048 && sp.match_length == THRESHOLD)
                            {
                                sp.match_length = 1;
                                bw.PutUncoded(sp.text_buf[r]);
                                encode_pos++;
                            }
                            else
                            {
                                bw.PutPair(offset, sp.match_length);
                                encode_pos += sp.match_length;
                            }
                        }

                        byte c;
                        last_match_length = sp.match_length;
                        for (i = 0; i < last_match_length && inputIdx < input.Length; i++)
                        {
                            DeleteNode(sp, s);    /* Delete old strings and */
                            c = input[inputIdx];
                            sp.text_buf[s] = c;    /* read new bytes */
                            sp.text_buf_map[s] = inputIdx;

                            /*
                             * If the position is near the end of buffer, extend the buffer
                             * to make string comparison easier.
                             */
                            if (s < (SearchSize - 1))
                            {
                                sp.text_buf[s + BufferSize] = c;
                                sp.text_buf_map[s + BufferSize] = inputIdx;
                            }

                            inputIdx++;
                            /* Since this is a ring buffer, increment the position modulo BufferSize. */
                            s = (s + 1) & (BufferSize - 1);
                            r = (r + 1) & (BufferSize - 1);

                            /* Register the string in text_buf[r..r+SearchSize-1] */
                            InsertNode(sp, r);
                        }
                        while (i++ < last_match_length)
                        {
                            DeleteNode(sp, s);

                            /* After the end of text, no need to read, */
                            s = (s + 1) & (BufferSize - 1);
                            r = (r + 1) & (BufferSize - 1);

                            /* but buffer may not be empty. */
                            if ((--len) > 0)
                                InsertNode(sp, r);

                            //Match length can't exceed the input length
                            if (sp.match_length > len)
                                sp.match_length = 1;
                        }

                    } while (len > 0);

                    return bw.GetBytes();
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

            var headerSize = m_reader.ReadInt32();//0xC
            var infoSize = m_reader.ReadInt32();//0x14

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
                IarImage.Extract(m_reader, m_arcFileOffset[key], m_arcVersion, fileListDic, Path.Combine(outputPath, fileListDic[key]));
            }
        }

        public static Dictionary<string, int> Create(string folder, string outputArcName)
        {
            Dictionary<string, int> fileList = [];

            var normalImgList = Directory.EnumerateFiles(folder, "*.png", SearchOption.TopDirectoryOnly);
            var layerImgList = Directory.EnumerateFiles(folder, "*.layerImg", SearchOption.TopDirectoryOnly);
            var subImgList = Directory.EnumerateFiles(folder, "*.subImg", SearchOption.TopDirectoryOnly);

            using var writer = new BinaryWriter(File.Open(Path.Combine(folder, "..", outputArcName), FileMode.Create));
            writer.Write(0x20726169);
            writer.Write(0x00010004);//Lock version to 0x4001
            writer.Write(0xC);
            writer.Write(0x14);
            writer.Write(0);
            writer.Write(0);

            var fileCountOffset = writer.BaseStream.Position;
            writer.BaseStream.Position += 8 * (normalImgList.Count() + layerImgList.Count() + subImgList.Count() + 1);

            List<long> fileOffsets = [];

            int inArcIndex = 0;

            {
                foreach(var file in normalImgList)
                {
                    Console.WriteLine($"WritingImg: {file} ...");
                    fileOffsets.Add(writer.BaseStream.Position);
                    IarImage.Import(writer, file, inArcIndex++, fileList, 0x4001);
                }

                foreach(var file in layerImgList)
                {
                    Console.WriteLine($"WritingLayerImg: {file} ...");
                    fileOffsets.Add(writer.BaseStream.Position);
                    IarImage.Import(writer, file, inArcIndex++, fileList, 0x4001);
                }

                foreach (var file in subImgList)
                {
                    Console.WriteLine($"WritingSubImg: {file} ...");
                    fileOffsets.Add(writer.BaseStream.Position);
                    IarImage.Import(writer, file, inArcIndex++, fileList, 0x4001);
                }
            }

            writer.BaseStream.Position = fileCountOffset;
            writer.Write(fileList.Count);
            writer.Write(fileList.Count);
            foreach(var o in fileOffsets)
            {
                writer.Write(o);
            }

            return fileList;
        }
    }
}
