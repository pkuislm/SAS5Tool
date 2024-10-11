namespace SAS5Lib.SecVariable
{
    /*
     * Variable Type (Runtime):
     * 0 : Byte, Word, Dword
     * 1 : Qword
     * 2 : Float
     * 3 : Double
     * 4 : Obj
     * 5 : ObjRef
     * 6 : InstructionAddr(CodePtr)
     */

    /*
     * PrimitiveType(NativeType) ID:
     * 0 : Byte
     * 1 : Word
     * 2 : Dword
     * 3 : Qword
     * 4 : Float
     * 5 : Double
     * 6 : Obj sizeof(Object*) == 4
     * 7 : ObjRef sizeof(ObjectRefrence) == 12
     * 8 : InstructionAddr sizeof(uint32_t*) == 4
     */

    /*
     * CompositionType ID:
     * 0 : PrimitiveType
     * 1 : ArrayType
     * 2 : TupleType
     * 3 : RecordType
     */
    public abstract class BasicType
    {
        protected byte BasicTypeID;
        protected int Size;
        public BasicType(byte typeID)
        {
            BasicTypeID = typeID;
        }

        public virtual int GetSize() { return Size; }
    }

    public class PrimitiveType : BasicType
    {
        public byte PrimitiveTypeID;
        public PrimitiveType(byte primitiveTypeID) : base(0)
        {
            PrimitiveTypeID = primitiveTypeID;
        }

        public override string ToString()
        {
            return PrimitiveTypeID switch
            {
                0 => "Byte",
                1 => "Word",
                2 => "Dword",
                3 => "Qword",
                4 => "Float",
                5 => "Double",
                6 => "Object",
                7 => "ObjRef",
                8 => "InstructionAddr",
                _ => $"NativeType({PrimitiveTypeID})"
            };
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

    public class ArrayType : BasicType
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

    public class TupleType : BasicType
    {
        //TODO: Analyze this type(probably same as record type)
        public TupleType() : base(2)
        {
        }
    }

    public class RecordType : BasicType
    {
        public int MemberCount;
        public List<ObjectType> Members;

        public RecordType(int memberCount, List<ObjectType> members) : base(3)
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

    public class ObjectType
    {
        public string Name;
        public BasicType Type;

        public ObjectType(string name, BasicType type)
        {
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return $"Name: {Name},  Type: {Type},  ElementSize: {Type.GetSize()}";
        }
    }

    public class Object
    {
        public int Index;
        public ObjectType Type;
        public byte[] Data;

        public Object(int index, ObjectType type, byte[] data)
        {
            Index = index;
            Type = type;
            Data = data;
        }

        public override string ToString()
        {
            return $"Object{{{Type}}}";
        }
    }
}
