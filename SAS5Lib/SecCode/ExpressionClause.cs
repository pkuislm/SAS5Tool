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
            var prop = SecCodeProp.GetClauseOpProp(Op);
            if(prop.ReaderFunc != null)
            {
                Data = prop.ReaderFunc(reader);
            }
        }

        public void Write(BinaryWriter writer, ref Dictionary<long, long> addresses)
        {
            Offset.New = writer.BaseStream.Position;
            addresses.TryAdd(Offset.Old, Offset.New);

            writer.Write(Op);
            DataOffset.New = writer.BaseStream.Position;
            switch (Data)
            {
                case EditableString str:
                {
                    var bs = str.IsEdited ? CodepageManager.Instance.ExportGetBytes(str.Text) : CodepageManager.Instance.ImportGetBytes(str.Text);
                    //Array<char> -> string
                    if (Op == 0x1E)
                    {
                        //Primitive type id(0)
                        writer.Write(0);
                        writer.Write(bs.Length);
                    }
                    else
                    {
                        writer.Write((byte)0);
                        writer.Write(Convert.ToByte(bs.Length));
                    }
                    writer.Write(bs);
                    break;
                }
                case NativeFunCall nc:
                {
                    writer.Write(nc.PresetObjIndex);
                    writer.Write(nc.NativeFuncIndex);
                    break;
                }
                case Variable v:
                {
                    writer.Write(v.Index);
                    //Array Type
                    var vtSize = v.Type.Type.GetSize();
                    if (vtSize != v.Data.Length)
                    {
                        writer.Write(Convert.ToUInt32(v.Data.Length / vtSize));
                    }
                    writer.Write(v.Data);
                    break;
                }
                case byte b: writer.Write(b); break;
                case sbyte sb: writer.Write(sb); break;
                case short s: writer.Write(s); break;
                case ushort us: writer.Write(us); break;
                case int i: writer.Write(i); break;
                case uint ui: writer.Write(ui); break;
                case float f: writer.Write(f); break;
                case double d: writer.Write(d); break;
                case long l: writer.Write(l); break;
                case ulong ul: writer.Write(ul); break;
                default:
                    break;
            }
        }

        public override string ToString()
        {
            return $"ExpressionClause(ExprOP: {Op:X2}, Data: {Data:X4})";
        }
    }
}
