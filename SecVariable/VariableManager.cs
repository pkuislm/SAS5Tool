using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAS5CodeDisasembler.SecVariable
{
    class VariableManager : Singleton<VariableManager>
    {
        readonly List<VariableType> VariableTypes = [];

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
                        var typeList = new List<VariableType>();
                        var count = reader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            typeList.Add(new VariableType(Utils.ReadCString(reader), ReadType(reader)));
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
                VariableTypes.Add(new VariableType(Utils.ReadCString(reader), ReadType(reader)));
            }
        }

        public VariableType GetType(int typeIndex)
        {
            return VariableTypes[typeIndex];
        }
    }
}
