namespace SAS5Lib
{
    public class SecArcFileList
    {
        List<string> m_arcFiles;

        public SecArcFileList(byte[]? input)
        {
            m_arcFiles = [];
            if(input == null)
            {
                return;
            }
            using var reader = new BinaryReader(new MemoryStream(input));
            var count = reader.ReadInt32();
            for(int i = 0; i < count; i++)
            {
                m_arcFiles.Add(Utils.ReadCString(reader));
            }
        }

        public void AddArc(string arcName)
        {
            foreach(string arc in m_arcFiles)
            {
                if (arc == arcName)
                    return;
            }
            m_arcFiles.Add(arcName);
        }

        public void RemoveArc(string arcName)
        {
            m_arcFiles.Remove(arcName);
        }

        public byte[] GetData()
        {
            var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write(m_arcFiles.Count);
            foreach(var s in m_arcFiles)
            {
                writer.Write(CodepageManager.Instance.ExportGetBytes(s));
            }
            return ms.ToArray();
        }
    }
}
