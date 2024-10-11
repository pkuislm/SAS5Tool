namespace SAS5Lib.SecCode
{
    public class ExecutorCommand
    {
        public CodeOffset Offset;
        public byte Type;
        public short ExecutorIndex;
        public List<Expression>? Expression;

        public ExecutorCommand(byte type, short index, List<Expression>? expression = null, long offset = -1)
        {
            Offset.Old = offset;
            Type = type;
            ExecutorIndex = index;
            Expression = expression;
        }

        public ExecutorCommand()
        {
        }

        public void GetExpression(BinaryReader reader)
        {
            Expression ??= [];
            byte argID;
            while ((argID = reader.ReadByte()) != 0xFF)
            {
                var exprLen = SecScenarioProgram.LegacyVersion ? reader.ReadInt32() : reader.ReadInt16();
                var expr = new Expression(Convert.ToInt16(exprLen), argID, reader.BaseStream.Position, reader.ReadBytes(exprLen));
                Expression.Add(expr);
            }
        }

        public void Write(BinaryWriter writer, ref Dictionary<long, long> addresses)
        {
            Offset.New = writer.BaseStream.Position;
            addresses.TryAdd(Offset.Old, Offset.New);

            writer.Write(Type);
            if (Type == 0x1B)
            {
                writer.Write(ExecutorIndex);
                if (Expression != null)
                {
                    foreach (var expr in Expression)
                    {
                        expr.Write(writer, ref addresses);
                    }
                }
                writer.Write((byte)0xFF);
            }
        }

        public override string ToString()
        {
            return $"{SecCodeProp.ExecutorName(ExecutorIndex)}(Offset: ({Offset.Old:X8},{Offset.New:X8}), ExecutorIndex: {ExecutorIndex:X4}, Expression(s): {Expression?.Count})";
        }
    }
}
