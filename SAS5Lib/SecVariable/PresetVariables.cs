using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAS5Lib.SecVariable
{
    public class PresetVariables
    {
        readonly int VarAIndex;
        readonly ObjectType VarAType;
        readonly int VarBIndex;
        readonly ObjectType VarBType;

        public PresetVariables(byte[]? input)
        {
            if(input == null)
            {
                VarAType = VarBType = new ObjectType("placeholder", new PrimitiveType(0));
                return;
            }

            using var reader = new BinaryReader(new MemoryStream(input));
            VarAIndex = reader.ReadInt32();
            VarBIndex = reader.ReadInt32();
            VarAType = VariableManager.Instance.GetType(VarAIndex);
            VarBType = VariableManager.Instance.GetType(VarBIndex);
        }

        public override string ToString()
        {
            return $"GlobalStatic:{VarAType}({VarAIndex}), GlobalPersistent:{VarBType}({VarBIndex})";
        }
    }
}
