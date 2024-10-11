using System.Diagnostics;

namespace SAS5Lib.SecCode
{
    public class NamedCode
    {
        public string Name;
        public List<object> Code;

        public NamedCode(string name, List<object> code)
        {
            Name = name;
            Code = code;
        }

        public override string ToString()
        {
            try
            {
                var c = (ExecutorCommand)Code.First(o => o is ExecutorCommand);
                return $"ScriptName: {Name}, StartAddr:({c.Offset.Old:X8}, {c.Offset.New:X8}), CodesCount: {Code.Count}";
            }
            catch(InvalidOperationException)
            {
                return $"ScriptName: {Name}, CodesCount: {Code.Count}";
            }
        }
    }

    public class ScenarioCode
    {
        readonly BinaryReader _reader;
        readonly SecSource? _source;
        public List<object> Code { get; private set;}

        public ScenarioCode(string code, SecSource? src = null)
        {
            _source = src;
            _reader = new BinaryReader(File.OpenRead(code));
            Code = [];
        }

        public ScenarioCode(byte[]? input, SecSource? src = null)
        {
            _source = src;
            Code = [];
            if (input == null)
            {
                _reader = new BinaryReader(new MemoryStream());
                return;
            }
            _reader = new BinaryReader(new MemoryStream(input));
        }

        public void Disasemble()
        {
            Console.WriteLine("Disasembling...");
            if(_source!= null)
            {
                var sourceFile = _source.SourceFiles;
                for(var i = 0; i < sourceFile.Count; i++)
                {
                    while(_reader.BaseStream.Position < sourceFile[i].Position)
                    {
                        Code.Add(GetCommand());
                    }
                    Trace.Assert(_reader.BaseStream.Position == sourceFile[i].Position);

                    List<object> code = [];
                    var endPos = i < sourceFile.Count - 1 ? sourceFile[i+1].Position : _reader.BaseStream.Length;

                    while(_reader.BaseStream.Position < endPos)
                    {
                        code.Add(GetCommand());
                    }
                    Trace.Assert(_reader.BaseStream.Position == endPos);

                    Code.Add(new NamedCode(sourceFile[i].Name, code));
                }
            }
            else
            {
                while (_reader.BaseStream.Position < _reader.BaseStream.Length)
                {
                    Code.Add(GetCommand());
                }
            }
        }

        public Tuple<byte[], Dictionary<long, long>> Assemble()
        {
            Console.WriteLine("Assembling...");
            var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            Dictionary<long, long> addresses = [];

            void WriteCodeObj(object cb)
            {
                if (cb is OrphanExpression orp)
                {
                    orp.Write(writer, ref addresses);
                }
                else if (cb is NamedCode nc)
                {
                    if(_source != null)
                    {
                        var src = _source.SourceFiles.Find(f => f.Name == nc.Name);
                        if(src != null)
                        {
                            src.Position = Convert.ToInt32(writer.BaseStream.Position);
                        }
                    }
                    foreach(var obj in nc.Code)
                    {
                        WriteCodeObj(obj);
                    }
                }
                else if (cb is ExecutorCommand expc)
                {
                    expc.Write(writer, ref addresses);
                }
                else if (cb is EditableString str)
                {
                    if (str.IsEdited)
                    {
                        writer.Write(CodepageManager.Instance.ExportGetBytes(str.Text));
                    }
                    else
                    {
                        writer.Write(CodepageManager.Instance.ImportGetBytes(str.Text));
                    }
                }
                else
                {
                    throw new Exception("Unknown object in code");
                }
            }

            foreach (var cb in Code)
            {
                WriteCodeObj(cb);
            }

            void UpdateAddress(ExpressionOperation clause)
            {
                var jmpMode = SecCodeProp.GetOpJmpMode(clause.Op);
                if (jmpMode == ExpressionOperation.JmpMode.Offset || jmpMode == ExpressionOperation.JmpMode.Direct)
                {
                    writer.BaseStream.Position = clause.DataOffset.New;
                    if (jmpMode == ExpressionOperation.JmpMode.Offset && clause.Data is int dest)
                    {
                        if (!addresses.TryGetValue(dest + clause.DataOffset.Old + 4, out var newAddr))
                        {
                            throw new Exception("Unknown jmp dest.");
                        }
                        writer.Write(Convert.ToInt32(newAddr - clause.DataOffset.New - 4));
                    }
                    else if(clause.Data is uint udest)
                    {
                        if (!addresses.TryGetValue(udest, out var newAddr))
                        {
                            // Fix: Workaround for old version
                            if (clause.Op == 0x19)
                            {
                                return;
                            }
                            throw new Exception("Unknown jmp dest.");
                        }
                        writer.Write(Convert.ToInt32(newAddr));
                    }
                    else
                    {
                        throw new Exception("Jump data error.");
                    }
                }
            }

            void FixCodeObj(object obj)
            {
                if (obj is OrphanExpression orpExpr)
                {
                    foreach (var clause in orpExpr.Clauses)
                    {
                        UpdateAddress(clause);
                    }
                }
                else if (obj is ExecutorCommand execCmd)
                {
                    if (execCmd.Expression != null)
                    {
                        foreach (var expr in execCmd.Expression)
                        {
                            foreach (var clause in expr.Operations)
                            {
                                UpdateAddress(clause);
                            }
                        }
                    }
                }
                else if(obj is NamedCode nc)
                {
                    foreach(var code in nc.Code)
                    {
                        FixCodeObj(code);
                    }
                }
            }

            foreach (var obj in Code)
            {
                FixCodeObj(obj);
            }

            return new Tuple<byte[], Dictionary<long, long>>(ms.ToArray(), addresses);
        }

        public object DisasembleSingle(long position)
        {
            _reader.BaseStream.Position = position;
            return GetCommand();
        }

        object GetCommand()
        {
            var cmd = new ExecutorCommand();

            cmd.Offset.Old = _reader.BaseStream.Position;
            cmd.Type = _reader.ReadByte();

            switch (cmd.Type)
            {
                case 0x1A:
                    {
                        return cmd;
                    }
                //WTF????
                case 0x69:
                    {
                        return cmd;
                    }
                //Regular command
                case 0x1B:
                    {
                        cmd.ExecutorIndex = _reader.ReadInt16();
                        if (SecCodeProp.ExecutorHaveExpression(cmd.ExecutorIndex))
                        {
                            cmd.GetExpression(_reader);
                        }
                        else
                        {
                            //This command must reaches its end at curent position
                            Trace.Assert(_reader.ReadByte() == 0xFF);
                        }
                        return cmd;
                    }
                //Only reaches here when we hits the end os the script
                case 0xFF:
                    {
                        return cmd;
                    }
                default:
                    {
                        _reader.BaseStream.Position--;
                        if (cmd.Type != 0x20 && (cmd.Type < 0x81 || cmd.Type > 0xFD))
                        {
                            //This is probably an orphan expression which uses 0x11(VM_RET) at the end of the expression. (like library function)
                            var prevPos = _reader.BaseStream.Position;
                            try
                            {
                                return new OrphanExpression(_reader);
                            }
                            catch(Exception)
                            {
                                //Unwind to previous position
                                _reader.BaseStream.Position = prevPos;
                                return new EditableString(ReadString(_reader));
                            }
                        }
                        else
                        {
                            //Assuming that it's a message string
                            return new EditableString(ReadString(_reader));
                        }
                    }
            }
        }

        static string ReadString(BinaryReader reader)
        {
            List<byte> charArray = new();
            byte b;
            while (true)
            {
                b = reader.ReadByte();
                if (b == 0 || b == 0x1A || b == 0x1B)
                {
                    if (b == 0x1A || b == 0x1B)
                    {
                        reader.BaseStream.Position--;
                    }
                    break;
                }
                charArray.Add(b);
            }
            return CodepageManager.Instance.ImportGetString([.. charArray]);
        }
    }
}
