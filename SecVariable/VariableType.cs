namespace SecTool.SecVariable
{
    abstract class BasicType
    {
        protected byte BasicTypeID;
        protected int Size;
        public BasicType(byte typeID)
        {
            BasicTypeID = typeID;
        }

        public virtual int GetSize() { return Size; }
    }

    class PrimitiveType : BasicType
    {
        public byte PrimitiveTypeID;
        public PrimitiveType(byte primitiveTypeID) : base(0)
        {
            PrimitiveTypeID = primitiveTypeID;
        }

        public override string ToString()
        {
            return $"NativeType({PrimitiveTypeID})";
        }

        public override int GetSize()
        {
            switch (PrimitiveTypeID)
            {
                case 0:
                    return 1;
                case 1:
                    return 2;
                case 2:
                case 4:
                case 8:
                    return 4;
                case 3:
                case 5:
                    return 8;
                case 6:
                case 7:
                    return 0;
                default:
                    throw new Exception("Fatal: Unknown primitive type.");
            }
        }
    }

    class ArrayType : BasicType
    {
        public int ElementCount;
        public BasicType ElementType;

        public ArrayType(int elemCount, BasicType elemType) : base(1)
        {
            ElementCount = elemCount;
            ElementType = elemType;
            Size = elemCount * elemType.GetSize();
        }

        public override string ToString()
        {
            return $"Array<{ElementType}>[{ElementCount}]";
        }
    }

    class TupleType : BasicType
    {
        //TODO: Analyze this type(probably same as record type)
        public TupleType() : base(2)
        {
        }
    }

    class RecordType : BasicType
    {
        public int MemberCount;
        public List<VariableType> Members;

        public RecordType(int memberCount, List<VariableType> members) : base(3)
        {
            MemberCount = memberCount;
            Members = members;
            foreach (var member in Members)
            {
                Size += member.Type.GetSize();
            }
        }

        public override string ToString()
        {
            return "Struct";
        }
    }

    class VariableType
    {
        public string Name;
        public BasicType Type;

        public VariableType(string name, BasicType type)
        {
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return $"Name: {Name},  Type: {Type},  Size: {Type.GetSize()}";
        }
    }

    class Variable
    {
        public int Index;
        public VariableType Type;
        public byte[] Data;

        public Variable(int index, VariableType type, byte[] data)
        {
            Index = index;
            Type = type;
            Data = data;
        }

        public override string ToString()
        {
            return $"Variable.{Type}";
        }
    }
}
