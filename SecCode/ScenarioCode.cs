using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SAS5CodeDisasembler.SecCode
{
    using Script = List<List<ScenarioCode.Dialogue>>;
    class ScenarioCode
    {
        readonly BinaryReader _reader;
        readonly List<object> _code;

        public ScenarioCode(string code)
        {
            _reader = new BinaryReader(File.OpenRead(code));
            _code = [];
        }

        public ScenarioCode(byte[]? input)
        {
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
            while (_reader.BaseStream.Position < _reader.BaseStream.Length)
            {
                _code.Add(GetCommand());
            }
        }

        public Tuple<byte[], Dictionary<long, long>> Assemble()
        {
            var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            Dictionary<long, long> addresses = [];
            foreach (var cb in _code)
            {
                if (cb is OrphanExpression orp)
                {
                    orp.Write(writer, ref addresses);
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

            void UpdateAddress(ExpressionClause clause, BinaryWriter writer)
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

            foreach (var obj in _code)
            {
                if (obj is OrphanExpression orpExpr)
                {
                    foreach (var clause in orpExpr.Clauses)
                    {
                        UpdateAddress(clause, writer);
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
                                UpdateAddress(clause, writer);
                            }
                        }
                    }
                }
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
                if (obj is EditableString s)
                {
                    dialogue += s.Text;
                }
                else if (obj is ExecutorCommand cmd)
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
                                if (clauses[1].Data is byte t && t == 0x01 && messages.Count > 0)
                                {
                                    text.Add(messages);
                                    messages = new List<Dialogue>();
                                }
                                dialogue = data.Text;
                                break;
                            case 0x72://selection
                                messages.Add(new Dialogue("选项", data.Text));
                                break;
                            default: break;
                        }
                    }
                }
            }
            return text;
        }

        public void SetText(Script text)
        {

        }

        public static Script ImportText(string path)
        {
            return [];
        }

        public static void ExportText(string outputPath, Script text)
        {
            var total = 0;
            foreach (var dialogues in text)
            {
                var output = File.CreateText(Path.Combine(outputPath, $"{dialogues[0].Text}.txt"));
                Console.Write($"Exporting {dialogues[0].Text}...");
                var idx = 0;
                var chars = 0;
                foreach (var dialogue in dialogues)
                {
                    output.WriteLine($"☆{idx:X8}☆{dialogue.Character}☆{dialogue.Text}");
                    output.WriteLine($"★{idx:X8}★{dialogue.Character}★{dialogue.Text}\n");
                    //output.WriteLine($"★{idx:X8}★{dialogue.Character}★{(dialogue.Text[0] == '「' ? "「」" : dialogue.Text[0] == '『' ? "『』" : "")}\n");
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
