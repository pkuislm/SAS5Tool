﻿namespace SAS5Lib.SecCode
{
    public class OrphanExpression
    {
        public CodeOffset Offset;
        public List<ExpressionOperation> Clauses;

        public OrphanExpression(BinaryReader reader)
        {
            Offset.Old = reader.BaseStream.Position;
            Clauses = [];

            byte b;
            do
            {
                b = reader.ReadByte();
                reader.BaseStream.Position--;
                Clauses.Add(new ExpressionOperation(reader));

            } while (b != 0x11);//0x11 -> RET
        }

        public void Write(BinaryWriter writer, ref Dictionary<long, long> addresses)
        {
            Offset.New = writer.BaseStream.Position;
            addresses.TryAdd(Offset.Old, Offset.New);

            foreach (var clause in Clauses)
            {
                clause.Write(writer, ref addresses);
            }
        }

        public override string ToString()
        {
            return $"OrphanExpression(Offset: ({Offset.Old:X8},{Offset.New:X8}), Clauses: {Clauses.Count})";
        }
    }
}
