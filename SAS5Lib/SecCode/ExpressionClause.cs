using System.Diagnostics;
using SAS5Lib.SecVariable;

namespace SAS5Lib.SecCode
{
    public class NativeFunCall
    {
        public byte PresetObjIndex;
        public int NativeFuncIndex;

        public NativeFunCall(BinaryReader reader)
        {
            PresetObjIndex = reader.ReadByte();
            NativeFuncIndex = reader.ReadInt32();
        }

        public override string ToString()
        {
            return $"NativeFunCall(PresetObjIndex:{PresetObjIndex}, NativeFuncIndex:{NativeFuncIndex})";
        }
    }

    public class ExpressionClause
    {
        public CodeOffset DataOffset;
        public CodeOffset Offset;
        public byte Op;
        public object? Data;

        public enum JmpMode
        {
            None = 0,
            Direct,
            Offset
        }

        public ExpressionClause(byte op, object? data)
        {
            Offset.Old = -1;
            DataOffset.Old = -1;
            Op = op;
            Data = data;
        }

        public ExpressionClause(BinaryReader reader, long basePos = 0)
        {
            Offset.Old = reader.BaseStream.Position + basePos;
            Op = reader.ReadByte();
            DataOffset.Old = reader.BaseStream.Position + basePos;
            switch (Op)
            {
                //0 byte
                case 0x0D:
                case 0x10:
                case 0x0A://RET
                case 0x11://RET
                case 0x23:
                case 0x24:
                case 0x26:
                case 0x28:

                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                case 0x36://Same as 0x32
                case 0x37://Same as 0x33
                case 0x38:
                case 0x39:
                case 0x3A:
                case 0x3B:
                case 0x3C:
                case 0x3D:
                case 0x3E:
                case 0x3F:

                case 0x40:
                case 0x41:
                case 0x42:
                case 0x43:
                case 0x44:
                //45~4F:Unused

                case 0x50:
                case 0x51:
                case 0x52:
                case 0x54:
                case 0x56:
                case 0x57:
                case 0x5B:
                case 0x5C:
                case 0x5D:
                case 0x5E:
                //5F:Unused

                case 0x62:
                case 0x63:
                case 0x64:
                case 0x65:
                //66~6F:Unused

                case 0x71:
                case 0x72:

                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87:
                case 0x88:
                case 0x89:
                case 0x8A:
                case 0x8B:
                case 0x8C:
                case 0x8D:
                case 0x8E:
                case 0x8F:

                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                case 0x98:
                case 0x99:
                case 0x9A:
                case 0x9B:
                case 0x9C:
                case 0x9D:
                case 0x9E:
                case 0x9F:

                case 0xA0:
                case 0xA1:
                case 0xA2:
                case 0xA3:
                case 0xA4:
                case 0xA5:
                case 0xA6:
                case 0xA7:
                case 0xA8:
                case 0xA9:
                case 0xAA:
                case 0xAB:
                case 0xAC:
                case 0xAD:
                case 0xAE:
                case 0xAF:

                case 0xB0:
                case 0xB1:
                case 0xB2:
                case 0xB3:
                case 0xB4:
                case 0xB5:
                case 0xB6:
                case 0xB7:
                case 0xB8:
                case 0xB9:
                case 0xBA:
                case 0xBB:
                case 0xBC:
                case 0xBD:
                case 0xBE:
                case 0xBF:

                case 0xC0:
                case 0xC1:
                case 0xC2:
                case 0xC3:
                case 0xC4:
                case 0xC5:
                //C6:Unused
                //C7:Unused
                case 0xC8:
                case 0xC9:
                case 0xCA:
                case 0xCB:
                case 0xCC:
                case 0xCD:
                case 0xCE:
                case 0xCF:

                case 0xD0:
                case 0xD1:
                case 0xD2:
                case 0xD3:
                case 0xD4:
                case 0xD5:
                case 0xD6:
                case 0xD7:
                case 0xD8:
                case 0xD9:
                case 0xDA:
                case 0xDB:
                case 0xDC:
                case 0xDD:
                case 0xDE:
                case 0xDF:

                case 0xE0:
                case 0xE1:
                case 0xE2:
                case 0xE3:
                case 0xE4:
                case 0xE5:
                //E6~FD:Unused
                case 0xFF:
                    break;

                //1 byte
                case 0x13:
                case 0x2A:
                case 0x53:
                case 0x55:
                case 0x58:
                case 0x59:
                case 0x60:
                case 0x61:
                case 0x79:
                case 0x7B:
                    {
                        Data = reader.ReadByte();
                        break;
                    }

                //2 bytes
                case 0x14:
                    {
                        Data = reader.ReadInt16();
                        break;
                    }

                //4 bytes
                case 0x08:
                case 0x09:
                case 0x0B:
                case 0x0C:
                case 0x0E:
                case 0x0F:
                case 0x12:
                case 0x15:
                case 0x19:
                case 0x20:
                case 0x5A:
                case 0x70:
                case 0x78:
                    {
                        Data = reader.ReadInt32();
                        break;
                    }

                //8 bytes
                case 0x1C:
                    {
                        Data = reader.ReadDouble();
                        break;
                    }

                //2+length bytes
                case 0x7A:
                    {
                        var type = reader.ReadByte();
                        Trace.Assert(type == 0);
                        var length = reader.ReadByte();
                        Data = new EditableString(CodepageManager.Instance.ImportGetString(reader.ReadBytes(length)));
                        break;
                    }

                case 0x1D:
                    {
                        var index = reader.ReadInt32();
                        var vt = VariableManager.Instance.GetType(index);
                        Data = new Variable(index, vt, reader.ReadBytes(vt.Type.GetSize()));
                        break;
                    }

                case 0xFE:
                    {
                        Data = new NativeFunCall(reader);
                        break;
                    }

                default:
                    throw new Exception($"Unknown Clause OP Type: {Op:X2}");
            }
        }

        public void Write(BinaryWriter writer, ref Dictionary<long, long> addresses)
        {
            Offset.New = writer.BaseStream.Position;
            addresses.TryAdd(Offset.Old, Offset.New);

            writer.Write(Op);
            DataOffset.New = writer.BaseStream.Position;
            if (Data != null)
            {
                if (Data is EditableString str)
                {
                    writer.Write((byte)0);
                    var bs = str.IsEdited ? CodepageManager.Instance.ExportGetBytes(str.Text) : CodepageManager.Instance.ImportGetBytes(str.Text);
                    writer.Write(Convert.ToByte(bs.Length));
                    writer.Write(bs);
                }
                else if (Data is NativeFunCall nc)
                {
                    writer.Write(nc.PresetObjIndex);
                    writer.Write(nc.NativeFuncIndex);
                }
                else if (Data is Variable v)
                {
                    writer.Write(v.Index);
                    writer.Write(v.Data);
                }
                else if (Data is byte b)
                {
                    writer.Write(b);
                }
                else if (Data is short s)
                {
                    writer.Write(s);
                }
                else if (Data is int i)
                {
                    writer.Write(i);
                }
                else if (Data is double d)
                {
                    writer.Write(d);
                }
            }
        }

        public JmpMode GetClauseJmpMode()
        {
            switch (Op)
            {
                case 0x08://Jmp
                case 0x0B://CondJmp
                case 0x0E://Call
                case 0x70://PushAddr
                    return JmpMode.Direct;
                case 0x09://JmpOffset
                case 0x0C://CondJmpOffset
                case 0x0F://CallOffset
                case 0x12://PushOffset
                    return JmpMode.Offset;
                default:
                    return JmpMode.None;
            }
        }

        public override string ToString()
        {
            return $"ExpressionClause(ExprOP: {Op:X2}, Data: {Data:X4})";
        }
    }
}
