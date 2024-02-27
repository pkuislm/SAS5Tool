using System.Diagnostics;
using SecTool.SecCode;

namespace SecTool
{
    using Script = List<Tuple<string, List<SecTextTool.Dialogue>>>;
    public class SecTextTool
    {
        static int FLG_NAME;
        static int FLG_TITLE;
        static int FLG_SELECT;
        public SecTextTool()
        {
        }

        public static void ExtractionSetTextFormatFlagVal(int gameType = 0)
        {
            if(gameType != 0)
            {
                //Nankoi
                FLG_NAME = 0x6B;
                FLG_TITLE = 0x82;
                FLG_SELECT = 0x8D;
            }
            else
            {
                //Natsukoi
                FLG_NAME = 0x53;
                FLG_TITLE = 0x67;
                FLG_SELECT = 0x72;
            }
        }

        static string GetRubyText(ExecutorCommand cmd)
        {
            Trace.Assert(cmd.Expression != null);
            var ret = "";
            for(int i = 0; i < cmd.Expression.Count; i+=2)
            {
                if (cmd.Expression[i].Clauses[0].Data is EditableString origText && cmd.Expression[i+1].Clauses[0].Data is EditableString rubyText)
                {
                    ret += $"{{{origText.Text}:{rubyText.Text}}}";
                }
                else
                {
                    throw new Exception("Invalid ruby text format");
                }
            }
            return ret;
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

        public static Script GetText(List<object> secCode)
        {
            Script text = [];
            List<Dialogue> messages = [];
            string character = "";
            string dialogue = "";

            foreach (var obj in secCode)
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
                            if(id == FLG_NAME)
                            {
                                character = data.Text;
                                if (clauses.Count < 7)
                                {
                                    break;
                                }
                                if (clauses[5].Data is byte id2 && id2 == 0x03 && clauses[6].Data is EditableString data2)
                                {
                                    character += $" -> {data2.Text}";
                                }
                            }
                            else if(id == FLG_TITLE)
                            {
                                messages.Add(new Dialogue("标题", data.Text));
                            }
                            else if(id == FLG_SELECT)
                            {
                                messages.Add(new Dialogue("选项", data.Text));
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

        public static void SetText(Script text, List<object> secCode)
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
                        //TODO: Regex to ruby text

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
                            new ExpressionClause(0x78, FLG_NAME), 

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
                            new ExpressionClause(0x78, FLG_TITLE), 

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
                            new ExpressionClause(0x78, FLG_SELECT), 

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
                        if(id == FLG_NAME)
                        {
                            yield return new Tuple<int, int, int>(TYPE_CHARNAME, i, i + 1);
                        }
                        else if(id == FLG_TITLE)
                        {
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
                        }
                        else if(id == FLG_SELECT)
                        {
                            yield return new Tuple<int, int, int>(TYPE_SELECT, i, i + 1);
                        }
                    }
                }                                             
            }

            for(int i = 0; i < secCode.Count; i++)
            {
                if(secCode[i] is not NamedCode nc)
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
                secCode[i] = nnc;
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
                    if(false && dialogue.Text != "")
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


