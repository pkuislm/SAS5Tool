using System.Diagnostics;

namespace SecTool.SecCode
{
    using Script = List<Tuple<string, List<ScenarioCode.Dialogue>>>;
    class ScenarioCode
    {
        readonly BinaryReader _reader;
        readonly List<object> _code;
        readonly SecSource? _source;

        class NamedCode
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
                return $"ScriptName: {Name}, CodesCount: {Code.Count}";
            }
        }

        public ScenarioCode(string code, SecSource? src = null)
        {
            _source = src;
            _reader = new BinaryReader(File.OpenRead(code));
            _code = [];
        }

        public ScenarioCode(byte[]? input, SecSource? src = null)
        {
            _source = src;
            _code = [];
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
                        _code.Add(GetCommand());
                    }
                    Trace.Assert(_reader.BaseStream.Position == sourceFile[i].Position);

                    List<object> code = [];
                    var endPos = i < sourceFile.Count - 1 ? sourceFile[i+1].Position : _reader.BaseStream.Length;

                    while(_reader.BaseStream.Position < endPos)
                    {
                        code.Add(GetCommand());
                    }
                    Trace.Assert(_reader.BaseStream.Position == endPos);

                    _code.Add(new NamedCode(sourceFile[i].Name, code));
                }
            }
            else
            {
                while (_reader.BaseStream.Position < _reader.BaseStream.Length)
                {
                    _code.Add(GetCommand());
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

            foreach (var cb in _code)
            {
                WriteCodeObj(cb);
            }

            void UpdateAddress(ExpressionClause clause)
            {
                var jmpMode = clause.GetClauseJmpMode();
                if (jmpMode == ExpressionClause.JmpMode.Offset || jmpMode == ExpressionClause.JmpMode.Direct)
                {
                    if (clause.Data is int dest)
                    {
                        writer.BaseStream.Position = clause.DataOffset.New;
                        if (jmpMode == ExpressionClause.JmpMode.Offset)
                        {
                            if (!addresses.TryGetValue(dest + clause.DataOffset.Old + 4, out var newAddr))
                            {
                                throw new Exception("Unknown jmp dest.");
                            }
                            writer.Write(Convert.ToInt32(newAddr - clause.DataOffset.New - 4));
                        }
                        else
                        {
                            if (!addresses.TryGetValue(dest, out var newAddr))
                            {
                                throw new Exception("Unknown jmp dest.");
                            }
                            writer.Write(Convert.ToInt32(newAddr));
                        }
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
                            foreach (var clause in expr.Clauses)
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

            foreach (var obj in _code)
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
                //Regular command
                case 0x1B:
                    {
                        cmd.ExecutorIndex = _reader.ReadInt16();
                        if (cmd.HasExpression())
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
                            //This is an orphan expression which uses 0x11(VM_RET) at the end of the expression. (like library function)
                            return new OrphanExpression(_reader);
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

        static string GetRubyText(ExecutorCommand cmd)
        {
            Trace.Assert(cmd.Expression != null && cmd.Expression.Count == 2);
            if (cmd.Expression[0].Clauses[0].Data is EditableString origText && cmd.Expression[1].Clauses[0].Data is EditableString rubyText)
            {
                return $"{{{origText.Text}:{rubyText.Text}}}";
            }
            else
            {
                throw new Exception("Invalid ruby text format");
            }
        }

        public class Dialogue(string character, string text)
        {
            public string Character = character;
            public string Text = text;

            public override string ToString()
            {
                return $"[{Character}] {Text}";
            }
        }

        public Script GetText()
        {
            Script text = [];
            List<Dialogue> messages = [];
            string character = "";
            string dialogue = "";

            foreach (var obj in _code)
            {
                if(obj is not NamedCode nc)
                {
                    continue;
                }
                foreach(var code in nc.Code)
                {
                    if (code is EditableString s)
                    {
                        dialogue += s.Text;
                    }
                    else if (code is ExecutorCommand cmd)
                    {
                        if (cmd.ExecutorIndex == 0x03)
                        {
                            if (dialogue != null && dialogue != "")
                            {
                                messages.Add(new Dialogue(character, dialogue));
                                dialogue = "";
                                character = "";
                            }
                        }
                        else if (cmd.ExecutorIndex == 0x1F8)
                        {
                            if(dialogue != "")
                            {
                                dialogue += "\\n";
                            }
                        }
                        else if (cmd.ExecutorIndex == 0x1F9)
                        {
                            dialogue += GetRubyText(cmd);
                        }
                        else if (cmd.ExecutorIndex == 0x12)
                        {
                            if (cmd.Expression == null || cmd.Expression.Count < 2)
                            {
                                continue;
                            }
                            var clauses = cmd.Expression[0].Clauses;
                            if (clauses[0].Op != 0x78)
                            {
                                continue;
                            }
                            //This is kind of string related
                            if (clauses[0].Data is not int id || clauses[2].Op != 0x7A || clauses[2].Data is not EditableString data)
                            {
                                continue;
                            }
                            switch (id)
                            {
                                case 0x53://name
                                    character = data.Text;
                                    if (clauses.Count < 7)
                                    {
                                        break;
                                    }
                                    if (clauses[5].Data is byte id2 && id2 == 0x03 && clauses[6].Data is EditableString data2)
                                    {
                                        character += $" -> {data2.Text}";
                                    }
                                    break;
                                case 0x67://title
                                    messages.Add(new Dialogue("标题", data.Text));
                                    break;
                                case 0x72://selection
                                    messages.Add(new Dialogue("选项", data.Text));
                                    break;
                                default: break;
                            }
                        }
                    }
                }
                if(messages.Count > 0)
                {
                    text.Add(new Tuple<string, List<Dialogue>>(nc.Name, messages));
                    messages = [];
                }
            }
            return text;
        }

        public void SetText(Script text)
        {
            var TYPE_DIALOGUE = 1 << 0;
            var TYPE_CHARNAME = 1 << 1;
            var TYPE_TITLE    = 1 << 2;
            var TYPE_SELECT   = 1 << 3;
            var TITLE_SCENE   = 0x80;
            var TITLE_PLOT    = 0x40;

            NamedCode current;
            Dictionary<string, List<Dialogue>> dict = [];
            foreach(var t in text)
            {
                dict.Add(t.Item1, t.Item2);
            }

            object ConvertDialogue(int type, Dialogue dialogue, object? cmdref = null)
            {
                if((type & TYPE_DIALOGUE) != 0)//normal text
                {
                    var ret = new List<object>();
                    var text = dialogue.Text;
                    foreach(var line in text.Split("\\n"))
                    {
                        ret.Add(new EditableString(line, true));
                        ret.Add(new ExecutorCommand(0x1B, 0x1F8));//Line break;
                    }
                    ret.RemoveAt(ret.Count-1);
                    return ret;
                }
                else if((type & TYPE_CHARNAME) != 0)//this is a character name
                {
                    if(cmdref == null || cmdref is not ExecutorCommand cmd || cmd.Expression == null || cmd.Expression.Count < 2)
                    {
                        throw new Exception("Generate name need a correct command refrence");
                    }
                    var name = dialogue.Character.Split(" -> ");
                    var charName = name[0];
                    var charOverrideName = name.Length == 2 ? name[1] : name[0];
                    var expr = new List<Expression>
                    {
                        new(0, 1, [
                            new ExpressionClause(0x78, 0x53), 

                            new ExpressionClause(0x79, (byte)0x01),
                            new ExpressionClause(0x7A, new EditableString(charName, true)),
                            new ExpressionClause(0x42, null),
                            new ExpressionClause(0x7B, (byte)0x00),

                            new ExpressionClause(0x79, (byte)0x03),
                            new ExpressionClause(0x7A, new EditableString(charOverrideName, true)),
                            new ExpressionClause(0x42, null),
                            new ExpressionClause(0x7B, (byte)0x02)
                        ]),
                        cmd.Expression[1]
                    };

                    //Copy old clauses
                    if(cmd.Expression[0].Clauses[5].Op == 0x79 && cmd.Expression[0].Clauses[5].Data is byte b && b == 0x03)
                    {
                        //origially has overrde name, skip
                        expr[0].Clauses.AddRange(cmd.Expression[0].Clauses[9..]);
                    }
                    else
                    {
                        expr[0].Clauses.AddRange(cmd.Expression[0].Clauses[5..]);
                    }

                    var ret = new ExecutorCommand(0x1B, 0x12, expr, cmd.Offset.Old);
                    return ret;
                }
                else if((type & TYPE_TITLE) != 0)//this is a title string
                {
                    if(cmdref == null || cmdref is not ExecutorCommand cmd || cmd.Expression == null || cmd.Expression.Count < 2)
                    {
                        throw new Exception("Generate title need a correct command refrence");
                    }
                    ExpressionClause c1;
                    ExpressionClause c2;
                    if((type & TITLE_SCENE) != 0)
                    {
                        c1 = new ExpressionClause(0x79, (byte)0x01);
                        c2 = new ExpressionClause(0x7B, (byte)0x00);
                    }
                    else
                    {
                        c1 = new ExpressionClause(0x79, (byte)0x03);
                        c2 = new ExpressionClause(0x7B, (byte)0x02);
                    }
                    var expr = new List<Expression>
                    {
                        new(0, 1, [
                            new ExpressionClause(0x78, 0x67), 

                            c1,
                            new ExpressionClause(0x7A, new EditableString(dialogue.Text, true)),
                            new ExpressionClause(0x42, null),
                            c2,

                            new ExpressionClause(0x52, null),
                            new ExpressionClause(0xFF, null)
                        ]),
                        cmd.Expression[1]
                    };
                    var ret = new ExecutorCommand(0x1B, 0x12, expr, cmd.Offset.Old);
                    return ret;
                }
                else if((type & TYPE_SELECT) != 0)
                {
                    if(cmdref == null || cmdref is not ExecutorCommand cmd || cmd.Expression == null || cmd.Expression.Count < 2)
                    {
                        throw new Exception("Generate selection need a correct command refrence");
                    }
                    var expr = new List<Expression>
                    {
                        new(0, 1, [
                            new ExpressionClause(0x78, 0x72), 

                            new ExpressionClause(0x79, (byte)0x01),
                            new ExpressionClause(0x7A, new EditableString(dialogue.Text, true)),
                            new ExpressionClause(0x42, null),
                            new ExpressionClause(0x7B, (byte)0x00),

                            new ExpressionClause(0x52, null),
                            new ExpressionClause(0xFF, null)
                        ]),
                        cmd.Expression[1]
                    };
                    var ret = new ExecutorCommand(0x1B, 0x12, expr, cmd.Offset.Old);
                    return ret;
                }
                else
                {
                    throw new Exception("Unknown dialogue type.");
                }
            }

            IEnumerable<Tuple<int, int, int>>GetTextBlockProp()
            {
                var code = current.Code;
                for(int i = 0; i < code.Count; i++)
                {
                    if (code[i] is EditableString s || (code[i] is ExecutorCommand e && e.ExecutorIndex == 0x1F9))
                    {
                        var start = i;
                        while(code[i] is not ExecutorCommand cmd || cmd.ExecutorIndex != 0x03)
                        {
                            i++;
                        }
                        yield return new Tuple<int, int, int>(TYPE_DIALOGUE, start, i);
                    }
                    else if (code[i] is ExecutorCommand cmd)
                    {
                        if (cmd.ExecutorIndex != 0x12)
                        {
                            continue;
                        }
                        if (cmd.Expression == null || cmd.Expression.Count < 2)
                        {
                            continue;
                        }
                        var clauses = cmd.Expression[0].Clauses;
                        if (clauses[0].Op != 0x78)
                        {
                            continue;
                        }
                        //This is kind of string related
                        if (clauses[0].Data is not int id || clauses[2].Op != 0x7A || clauses[2].Data is not EditableString data)
                        {
                            continue;
                        }
                        switch (id)
                        {
                            case 0x53://name
                                yield return new Tuple<int, int, int>(TYPE_CHARNAME, i, i + 1);
                                break;
                            case 0x67://title
                                if(clauses[1].Op == 0x79 && clauses[1].Data is byte b)
                                {
                                    if(b == 0x01)
                                    {
                                        yield return new Tuple<int, int, int>(TYPE_TITLE | TITLE_SCENE, i, i + 1);
                                    }
                                    else
                                    {
                                        yield return new Tuple<int, int, int>(TYPE_TITLE | TITLE_PLOT, i, i + 1);
                                    }
                                }
                                break;
                            case 0x72://selection
                                yield return new Tuple<int, int, int>(TYPE_SELECT, i, i + 1);
                                break;
                            default: break;
                        }
                    }
                }                                             
            }

            for(int i = 0; i < _code.Count; i++)
            {
                if(_code[i] is not NamedCode nc)
                {
                    continue;
                }
                if(!dict.TryGetValue(Path.GetFileName(nc.Name), out var dialogues))
                {
                    continue;
                }
                current = nc;
                Console.WriteLine($"Importing {nc.Name}...");
                var nnc = new NamedCode(nc.Name, []);
                int prevPos = 0;
                foreach(var t in GetTextBlockProp())
                {
                    if(dialogues.Count == 0)
                    {
                        throw new Exception("Dialogue count doesn't match");
                    }
                    nnc.Code.AddRange(nc.Code[prevPos .. t.Item2]);
                    var generatedBlock = ConvertDialogue(t.Item1, dialogues[0], nc.Code[t.Item2]);
                    if(generatedBlock is List<object> l)
                    {
                        nnc.Code.AddRange(l);
                    }
                    else
                    {
                        nnc.Code.Add(generatedBlock);
                    }
                    prevPos = t.Item3;
                    if((t.Item1 & TYPE_CHARNAME) == 0)
                    {
                        dialogues.RemoveAt(0);
                    }
                }
                nnc.Code.AddRange(current.Code[prevPos ..]);
                _code[i] = nnc;
            }
        }

        public static Script ImportText(string path)
        {
            var ret = new Script();
            foreach(var file in Directory.EnumerateFiles(path, "*.txt"))
            {
                Console.WriteLine($"Reading {file}...");
                var messages = new List<Dialogue>();
                foreach(var line in File.ReadAllLines(file))
                {
                    if(line.StartsWith('☆') || line == "")
                    {
                        continue;
                    }
                    var text = line.Split('★');
                    messages.Add(new Dialogue(text[2], text[3]));
                }
                ret.Add(new Tuple<string, List<Dialogue>>(Path.GetFileNameWithoutExtension(file), messages));
            }
            return ret;
        }

        public static void ExportText(string outputPath, Script text)
        {
            var total = 0;
            foreach (var dialogues in text)
            {
                var output = File.CreateText(Path.Combine(outputPath, $"{Path.GetFileName(dialogues.Item1)}.txt"));
                Console.Write($"Exporting {dialogues.Item1}...");
                var idx = 0;
                var chars = 0;
                foreach (var dialogue in dialogues.Item2)
                {
                    output.WriteLine($"☆{idx:X8}☆{dialogue.Character}☆{dialogue.Text}");
                    //output.WriteLine($"★{idx:X8}★{dialogue.Character}★{dialogue.Text}\n");
                    if(dialogue.Text != "")
                    {
                        output.WriteLine($"★{idx:X8}★{dialogue.Character}★{(dialogue.Text[0] == '「' ? "「」" : dialogue.Text[0] == '『' ? "『』" : "")}\n");
                    }
                    else
                    {
                        output.WriteLine($"★{idx:X8}★{dialogue.Character}★{dialogue.Text}\n");
                    }
                    chars += dialogue.Text.Length;
                    idx++;
                }
                Console.WriteLine($" ({chars} characters)");
                total += chars;
                output.Flush();
                output.Close();
            }
            Console.WriteLine($"Total: {total} characters.");
        }
    }
}
