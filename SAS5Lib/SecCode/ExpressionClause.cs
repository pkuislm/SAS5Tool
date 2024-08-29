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
            if (Data != null)
            {
                if (Data is EditableString str)
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
                }
                else if (Data is NativeFunCall nc)
                {
                    writer.Write(nc.PresetObjIndex);
                    writer.Write(nc.NativeFuncIndex);
                }
                else if (Data is Variable v)
                {
                    writer.Write(v.Index);
                    //Array Type
                    var vtSize = v.Type.Type.GetSize();
                    if (vtSize != v.Data.Length)
                    {
                        writer.Write(Convert.ToUInt32(v.Data.Length / vtSize));
                    }
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

        public override string ToString()
        {
            return $"ExpressionClause(ExprOP: {Op:X2}, Data: {Data:X4})";
        }
    }
}
