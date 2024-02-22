namespace SecTool.SecOption
{
    abstract class OptionType
    {
        protected byte TypeID;

        public OptionType(byte typeID)
        {
            TypeID = typeID;
        }

        public static OptionType ReadOptionType(BinaryReader reader)
        {
            var typeID = reader.ReadByte();
            switch(typeID)
            {
                case 0:
                    return new SecOptionMap(reader);
                case 1:
                    return new SecOptionTuple(reader);
                case 2:
                    return new SecOptionInteger(reader);
                case 3:
                    return new SecOptionString(reader);
                case 4:
                    return new SecOptionResource(reader);
                case 5:
                    return new SecOptionLabel(reader);
                case 6:
                    return new SecOptionBinary(reader);
                default:
                    throw new Exception("Unknown OptionTypeID");
            }
        }

        public static EditableString ReadOptionString(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            return new EditableString(CodepageManager.Instance.ImportGetString(reader.ReadBytes(length)));
        }

        public static void WriteOptionString(BinaryWriter writer, EditableString str)
        {
            var b = str.IsEdited ? CodepageManager.Instance.ExportGetBytes(str.Text) : CodepageManager.Instance.ImportGetBytes(str.Text);
            writer.Write(b.Length);
            writer.Write(b);
        }

        public abstract void Save(BinaryWriter writer);
    }

    class SecOptionMap : OptionType
    {
        public Dictionary<string, OptionType> Map;

        public SecOptionMap() : base(0)
        {
            Map = [];
        }

        public SecOptionMap(BinaryReader reader) : base(0)
        {
            Map = [];
            var count = reader.ReadInt32();
            for(int i = 0; i < count; i++)
            {
                Map.TryAdd(ReadOptionString(reader).Text, ReadOptionType(reader));
            }
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(TypeID);
            writer.Write(Map.Count);
            foreach(var k in Map.Keys)
            {
                WriteOptionString(writer, new EditableString(k, false));
                Map[k].Save(writer);
            }
        }
    }

    class SecOptionTuple : OptionType
    {
        public List<OptionType> Members;

        public SecOptionTuple() : base(1)
        {
            Members = [];
        }

        public SecOptionTuple(BinaryReader reader) : base(1)
        {
            Members = [];
            var count = reader.ReadInt32();
            for(var i = 0; i < count; ++i)
            {
                Members.Add(ReadOptionType(reader));
            }
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(TypeID);
            writer.Write(Members.Count);
            foreach (var member in Members)
            {
                member.Save(writer);
            }
        }

        public override string ToString()
        {
            return $"Tuple(Length: {Members.Count})";
        }
    }

    class SecOptionInteger : OptionType
    {
        public int Value;

        public SecOptionInteger() : base(2)
        {
            Value = 0;
        }

        public SecOptionInteger(BinaryReader reader) : base(2)
        {
            Value = reader.ReadInt32();
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(TypeID);
            writer.Write(Value);
        }

        public override string ToString()
        {
            return $"Integer({Value})";
        }
    }

    class SecOptionString : OptionType
    {
        public EditableString Value;

        public SecOptionString() : base(3)
        {
        }

        public SecOptionString(BinaryReader reader) : base(3)
        {
            Value = ReadOptionString(reader);
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(TypeID);
            WriteOptionString(writer, Value);
        }


        public override string ToString()
        {
            return $"String({Value})";
        }
    }

    class SecOptionResource : OptionType
    {
        public int Value;

        public SecOptionResource() : base(4)
        {

        }

        public SecOptionResource(BinaryReader reader) : base(4)
        {
            Value = reader.ReadInt32();
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(TypeID);
            writer.Write(Value);
        }


        public override string ToString()
        {
            return $"Resource({Value})";
        }
    }

    class SecOptionLabel : OptionType
    {
        public int Value;

        public SecOptionLabel() : base(5)
        {

        }

        public SecOptionLabel(BinaryReader reader) : base(5)
        {
            Value = reader.ReadInt32();
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(TypeID);
            writer.Write(Value);
        }


        public override string ToString()
        {
            return $"Label({Value})";
        }
    }

    class SecOptionBinary : OptionType
    {
        public byte[] Value;

        public SecOptionBinary() : base(6)
        {
            Value = [];
        }

        public SecOptionBinary(BinaryReader reader) : base(6)
        {
            Value = reader.ReadBytes(reader.ReadInt32());
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(TypeID);
            writer.Write(Value.Length);
            writer.Write(Value);
        }


        public override string ToString()
        {
            return $"byte[{Value.Length}]";
        }
    }
}
