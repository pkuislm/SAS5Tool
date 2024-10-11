using System.Text.RegularExpressions;

namespace SAS5Lib.SecVariable
{
    public class VariableManager : Singleton<VariableManager>
    {
        readonly List<ObjectType> VariableTypes = [];

        BasicType ReadType(BinaryReader reader)
        {
            var basicTypeID = reader.ReadByte();
            switch (basicTypeID)
            {
                case 0xFF:
                    return GetType(reader.ReadInt32()).Type;
                case 0x00:
                    return new PrimitiveType(reader.ReadByte());
                case 0x01:
                    return new ArrayType(reader.ReadInt32(), ReadType(reader));
                //case 0x02:
                case 0x03:
                    {
                        var typeList = new List<ObjectType>();
                        var count = reader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            typeList.Add(new ObjectType(Utils.ReadCString(reader), ReadType(reader)));
                        }
                        return new RecordType(count, typeList);
                    }
                default:
                    throw new Exception($"Unknown BasicTypeID:{basicTypeID}");
            }
        }

        public void LoadVariablesList(string varDefFile)
        {
            LoadVariablesList(File.ReadAllBytes(varDefFile));
        }

        public void LoadVariablesList(byte[]? input)
        {
            if (input == null)
            {
                return;
            }
            using var reader = new BinaryReader(new MemoryStream(input));
            VariableTypes.Capacity = reader.ReadInt32();
            for (int i = 0; i < VariableTypes.Capacity; i++)
            {
                VariableTypes.Add(new ObjectType(Utils.ReadCString(reader), ReadType(reader)));
            }
        }

        public ObjectType GetType(int typeIndex)
        {
            return VariableTypes[typeIndex];
        }

        public int GetVariableTypeIndexByNameRegex(string pattern)
        {
            var vt = VariableTypes.FirstOrDefault(x => Regex.IsMatch(x.Name, pattern));
            if(vt == null)
            {
                return -1;
            }
            return VariableTypes.IndexOf(vt);
        }

        public string GetVariableTypeName(int index)
        {
            return VariableTypes[index].Name;
        }
    }
}
