namespace SAS5Lib
{
    public class SecCodePage
    {
        public int FileReadingCodePage;
        public int ExecutorCodePage;
        public int ExpressionCodePage;
        public int ReservedCodePage;

        public SecCodePage(BinaryReader reader)
        {
            FileReadingCodePage = reader.ReadInt32();
            ExecutorCodePage = reader.ReadInt32();
            ExpressionCodePage = reader.ReadInt32();
            ReservedCodePage = reader.ReadInt32();
        }

        public SecCodePage(int charset)
        {
            SetCharset(charset);
        }

        public void SetCharset(int charset)
        {
            FileReadingCodePage = ExecutorCodePage = ExpressionCodePage = charset;
        }

        public byte[] GetData()
        {
            var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write(FileReadingCodePage);
            writer.Write(ExecutorCodePage);
            writer.Write(ExpressionCodePage);
            writer.Write(ReservedCodePage);
            return ms.ToArray();
        }
    }
}
