using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAS5CodeDisasembler.SecCode
{
    class Expression
    {
        public short Length;
        public byte ReturnType;
        public List<ExpressionClause> Clauses;

        public Expression(short exprLength, byte exprReturnType, long basePos, byte[]? exprData = null)
        {
            Clauses = [];
            Length = exprLength;
            ReturnType = exprReturnType;
            //Have clause(s)
            //Expression ends with 0xFF(VM_END)
            if (exprData != null)
            {
                using var reader = new BinaryReader(new MemoryStream(exprData));

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    Clauses.Add(new ExpressionClause(reader, basePos));
                }
            }
        }

        public void Write(BinaryWriter writer, ref Dictionary<long, long> addresses)
        {
            writer.Write(ReturnType);
            var lenPos = writer.BaseStream.Position;
            writer.Write(Length);

            if (Clauses.Count > 0)
            {
                foreach (var clause in Clauses)
                {
                    clause.Write(writer, ref addresses);
                }
            }
            var curPos = writer.BaseStream.Position;
            writer.BaseStream.Position = lenPos;
            writer.Write(Convert.ToInt16(curPos - lenPos - 2));
            writer.BaseStream.Position = curPos;
        }

        public override string ToString()
        {
            return $"Expression(ReturnType:{ReturnType:X2}, Clauses:{Clauses.Count})";
        }
    }
}
