namespace SAS5Lib.SecCode
{
    /*
     * Expression
     * Prepare argument(s) for CmdExecutors
     */
    public class Expression
    {
        public short Length;
        public byte ArgID;
        public List<ExpressionOperation> Operations;

        public Expression(short exprLength, byte exprArgID, List<ExpressionOperation> operations)
        {
            Length = exprLength;
            ArgID = exprArgID;
            Operations = operations;
        }
        
        public Expression(short exprLength, byte exprArgID, long basePos, byte[]? exprData = null)
        {
            Operations = [];
            Length = exprLength;
            ArgID = exprArgID;
            //Have operation(s)
            //Expression ends with 0xFF(EXPR_END)
            if (exprData != null)
            {
                using var reader = new BinaryReader(new MemoryStream(exprData));

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    Operations.Add(new ExpressionOperation(reader, basePos));
                }
            }
        }

        public void Write(BinaryWriter writer, ref Dictionary<long, long> addresses)
        {
            writer.Write(ArgID);
            var lenPos = writer.BaseStream.Position;
            if(SecScenarioProgram.LegacyVersion)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(Length);
            }

            if (Operations.Count > 0)
            {
                foreach (var operation in Operations)
                {
                    operation.Write(writer, ref addresses);
                }
            }
            var curPos = writer.BaseStream.Position;
            writer.BaseStream.Position = lenPos;
            if (SecScenarioProgram.LegacyVersion)
            {
                writer.Write(Convert.ToInt32(curPos - lenPos - 4));
            }
            else
            {
                writer.Write(Convert.ToInt16(curPos - lenPos - 2));
            }
            writer.BaseStream.Position = curPos;
        }

        public override string ToString()
        {
            return $"ArgExpr(ArgID:{ArgID:X2}, OpCount:{Operations.Count})";
        }
    }
}
