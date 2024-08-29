using System.Text;

namespace SAS5Lib
{
    public class CodepageManager : Singleton<CodepageManager>
    {
        readonly Dictionary<string, Encoding> _encodings = new Dictionary<string, Encoding>
        {
            { "utf8", Encoding.GetEncoding("utf-8")},
            { "gbk", Encoding.GetEncoding("gbk")},
            { "sjis", Encoding.GetEncoding("shift_jis")},
        };

        Encoding _exportEncoding;
        Encoding _importEncoding;

        public int ExportCodePage { get { return _exportEncoding.CodePage; } }
        public int ImportCodePage { get { return _importEncoding.CodePage; } }

        public void SetExportEncoding(string encoding)
        {
            _exportEncoding = _encodings[encoding];
        }

        public void SetExportEncoding(int encoding)
        {
            _exportEncoding = Encoding.GetEncoding(encoding);
        }

        public void SetImportEncoding(string encoding)
        {
            _importEncoding = _encodings[encoding];
        }

        public void SetImportEncoding(int encoding)
        {
            _importEncoding = Encoding.GetEncoding(encoding);
        }

        public string ExportGetString(byte[] bytes)
        {
            return _exportEncoding.GetString(bytes);
        }

        public byte[] ExportGetBytes(string str)
        {
            return _exportEncoding.GetBytes(str);
        }

        public string ImportGetString(byte[] bytes)
        {
            return _importEncoding.GetString(bytes);
        }

        public byte[] ImportGetBytes(string str)
        {
            return _importEncoding.GetBytes(str);
        }
    }

    public struct EditableString
    {
        public bool IsEdited;
        public string Text;

        public EditableString(string text, bool isEdited = true)
        {
            IsEdited = isEdited;
            Text = text;
        }

        public EditableString()
        {
            IsEdited = false;
            Text = "";
        }

        public override readonly string ToString()
        {
            return $"\"{Text}\", Edited: {IsEdited}";
        }

        public override readonly int GetHashCode()
        {
            return Text.GetHashCode();
        }
    }

    public struct CodeOffset
    {
        public long Old;
        public long New;
    }

    public class Utils
    {
        public static string ReadCString(BinaryReader reader)
        {
            List<byte> charArray = [];
            byte b = reader.ReadByte();
            while (b != 0)
            {
                charArray.Add(b);
                b = reader.ReadByte();
            }
            return CodepageManager.Instance.ImportGetString([.. charArray]);
        }
    }

    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T? instance;
        public static T Instance
        {
            get
            {
                instance ??= new T();
                return instance;
            }
        }
    }
}
