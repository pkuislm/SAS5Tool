﻿using System.Diagnostics;
using System.Text.RegularExpressions;
using SAS5Lib.SecCode;
using SAS5Lib.SecVariable;

namespace SecTool
{
    using Script = List<Tuple<string, List<SecTextTool.Dialogue>>>;

    partial class SecRegex
    {
        [GeneratedRegex(@"(\{(.+?):(.+?)\})")]
        public static partial Regex RubyPattern();
    }

    public class SecTextTool
    {
        // "Flag" is actually index of variable type array.
        static int FLG_NAME;
        static int FLG_TITLE;
        static int FLG_SELECT;
        public SecTextTool()
        {
        }

        public static void SetTextFlag()
        {
            FLG_NAME = VariableManager.Instance.GetVariableTypeIndexByNameRegex(@"msg\.sal::name::");
            FLG_TITLE = VariableManager.Instance.GetVariableTypeIndexByNameRegex(@"scene\.sal::scene::");
            FLG_SELECT = VariableManager.Instance.GetVariableTypeIndexByNameRegex(@"selection\.sal::(.+?_?)option::");
        }

        static string GetRubyText(ExecutorCommand cmd)
        {
            Trace.Assert(cmd.Expression != null);
            var ret = "";
            for (int i = 0; i < cmd.Expression.Count; i += 2)
            {
                if (cmd.Expression[i].Operations[0].Data is EditableString origText && cmd.Expression[i + 1].Operations[0].Data is EditableString rubyText)
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

        public static List<string> GetString(List<object> secCode, string scriptName)
        {
            List<string> result = [];

            foreach (var obj in secCode)
            {
                if (obj is not NamedCode nc || !nc.Name.Contains(scriptName))
                {
                    continue;
                }
                foreach (var code in nc.Code)
                {
                    if (code is ExecutorCommand cmd)
                    {
                        if (cmd.Expression == null)
                        {
                            continue;
                        }
                        foreach (var exp in cmd.Expression)
                        {
                            if (exp.Operations == null)
                            {
                                continue;
                            }
                            foreach (var operation in exp.Operations)
                            {
                                if (operation.Data is EditableString str && !string.IsNullOrEmpty(str.Text))
                                {
                                    result.Add(str.Text);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
        public static void SetString(List<object> secCode, string scriptName, List<string> strs)
        {
            var idx = 0;
            foreach (var obj in secCode)
            {
                if (obj is not NamedCode nc || !nc.Name.Contains(scriptName))
                {
                    continue;
                }
                foreach (var code in nc.Code)
                {
                    if (code is ExecutorCommand cmd)
                    {
                        if (cmd.Expression == null)
                        {
                            continue;
                        }
                        foreach (var exp in cmd.Expression)
                        {
                            if (exp.Operations == null)
                            {
                                continue;
                            }
                            foreach (var operstion in exp.Operations)
                            {
                                if (operstion.Data is EditableString str && !string.IsNullOrEmpty(str.Text))
                                {
                                    str.Text = strs[idx];
                                    operstion.Data = str;
                                    idx++;
                                }
                            }
                        }
                    }
                }
            }
            Trace.Assert(idx == strs.Count);
        }

        public static Tuple<Script, Dictionary<string, string>> GetText(List<object> secCode)
        {
            Script text = [];
            Dictionary<string, string> nameMap = [];
            List<Dialogue> messages = [];
            string character = "";
            string dialogue = "";

            byte STR_PUSH_OP = 0x20;
            byte STR_LOAD_OP = 0x1E;
            int STR_LOAD_INDEX = 6;
            int STR_LOAD_ID2_INDEX = 14;
            int STR_LOAD_INDEX2 = 16;

            if(!SecScenarioProgram.LegacyVersion)
            {
                STR_PUSH_OP = 0x78;
                STR_LOAD_OP = 0x7A;
                STR_LOAD_INDEX = 2;
                STR_LOAD_ID2_INDEX = 5;
                STR_LOAD_INDEX2 = 6;
            }

            foreach (var obj in secCode)
            {
                if (obj is not NamedCode nc)
                {
                    continue;
                }
                foreach (var code in nc.Code)
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
                            if (dialogue != "")
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
                            if (cmd.Expression == null || (!SecScenarioProgram.LegacyVersion && cmd.Expression.Count < 2))
                            {
                                continue;
                            }
                            var operations = cmd.Expression[0].Operations;
                            if (operations[0].Op != STR_PUSH_OP || (SecScenarioProgram.LegacyVersion && operations.Count < STR_LOAD_INDEX))
                            {
                                continue;
                            }
                            //This is kind of string related
                            if (operations[0].Data is not uint id || operations[STR_LOAD_INDEX].Op != STR_LOAD_OP || operations[STR_LOAD_INDEX].Data is not EditableString data)
                            {
                                continue;
                            }
                            if (id == FLG_NAME)
                            {
                                character = data.Text;
                                if (operations.Count >= STR_LOAD_INDEX2)
                                {
                                    if (Convert.ToByte(operations[STR_LOAD_ID2_INDEX].Data) == 0x03 && operations[STR_LOAD_INDEX2].Data is EditableString data2)
                                    {
                                        character += $" -> {data2.Text}";
                                    }
                                }
                                nameMap.TryAdd(character, character);
                            }
                            else if (id == FLG_TITLE)
                            {
                                messages.Add(new Dialogue("标题", data.Text));
                            }
                            else if (id == FLG_SELECT)
                            {
                                messages.Add(new Dialogue("选项", data.Text));
                            }
                        }
                    }
                }
                if (messages.Count > 0)
                {
                    text.Add(new Tuple<string, List<Dialogue>>(nc.Name, messages));
                    messages = [];
                }
            }
            return new Tuple<Script, Dictionary<string, string>>(text, nameMap);
        }

        public static void SetText(Script text, List<object> secCode, Dictionary<string, string>? nameMap = null)
        {
            var TYPE_DIALOGUE = 1 << 0;
            var TYPE_CHARNAME = 1 << 1;
            var TYPE_TITLE = 1 << 2;
            var TYPE_SELECT = 1 << 3;
            var TITLE_SCENE = 0x80;
            var TITLE_PLOT = 0x40;

            byte STR_PUSH_OP = 0x20;
            byte STR_LOAD_POS_OP1 = 0x13;
            byte STR_LOAD_OP = 0x1E;
            byte STR_LOAD_POS_OP2 = 0x13;
            int STR_LOAD_POS = 4;
            int STR_LOAD_INDEX = 6;
            int STR_LOAD_ID2_INDEX = 14;
            int STR_LOAD_INDEX2 = 16;
            int NAME_EXPR_CP_POS1 = 13;
            int NAME_EXPR_CP_POS2 = 23;

            if (!SecScenarioProgram.LegacyVersion)
            {
                STR_PUSH_OP = 0x78;
                STR_LOAD_POS_OP1 = 0x79;
                STR_LOAD_OP = 0x7A;
                STR_LOAD_POS_OP2 = 0x7B;
                STR_LOAD_POS = 1;
                STR_LOAD_INDEX = 2;
                STR_LOAD_ID2_INDEX = 5;
                STR_LOAD_INDEX2 = 6;
                NAME_EXPR_CP_POS1 = 5;
                NAME_EXPR_CP_POS2 = 9;
            }

            NamedCode current;
            Dictionary<string, List<Dialogue>> dict = [];
            foreach (var t in text)
            {
                dict.Add(t.Item1, t.Item2);
            }

            object ConvertDialogue(int type, Dialogue dialogue, object? cmdref = null)
            {
                if ((type & TYPE_DIALOGUE) != 0)//normal text
                {
                    var ret = new List<object>();
                    var text = dialogue.Text;
                    foreach (var line in text.Split("\\n"))
                    {
                        if (SecRegex.RubyPattern().IsMatch(line))
                        {
                            ExecutorCommand cmd = new(0x1B, 0x1F9)
                            {
                                Expression = []
                            };

                            var group = SecRegex.RubyPattern().Matches(line);
                            var lastMatchEnd = 0;
                            for (int i = 0; i < group.Count; ++i)
                            {
                                if (lastMatchEnd != group[i].Index)
                                {
                                    if (cmd.Expression.Count > 0)
                                    {
                                        ret.Add(cmd);
                                        cmd = new(0x1B, 0x1F9)
                                        {
                                            Expression = []
                                        };
                                    }
                                    ret.Add(new EditableString(line[lastMatchEnd..group[i].Index], true));
                                }

                                var g = group[i].Groups;
                                var expr = new List<Expression>
                                    {
                                        new(0, 1, [
                                            new ExpressionOperation(STR_LOAD_OP, new EditableString(line.Substring(g[2].Index, g[2].Length), true)),
                                            new ExpressionOperation(0xFF, null)
                                        ]),
                                        new(0, 2, [
                                            new ExpressionOperation(STR_LOAD_OP, new EditableString(line.Substring(g[3].Index, g[3].Length), true)),
                                            new ExpressionOperation(0xFF, null)
                                        ])
                                    };
                                cmd.Expression.AddRange(expr);

                                lastMatchEnd = group[i].Index + group[i].Length;
                            }
                            ret.Add(cmd);
                            if (lastMatchEnd != line.Length)
                            {
                                ret.Add(new EditableString(line[lastMatchEnd..], true));
                            }
                        }
                        else
                        {
                            ret.Add(new EditableString(line, true));
                        }
                        ret.Add(new ExecutorCommand(0x1B, 0x1F8));//Line break;
                    }
                    ret.RemoveAt(ret.Count - 1);
                    return ret;
                }
                else if ((type & TYPE_CHARNAME) != 0)//this is a character name
                {
                    if (cmdref == null || cmdref is not ExecutorCommand cmd || cmd.Expression == null || (!SecScenarioProgram.LegacyVersion && cmd.Expression.Count < 2))
                    {
                        throw new Exception("Generate name need a correct command refrence");
                    }

                    var name = dialogue.Character.Split(" -> ");
                    if (nameMap != null)
                    {
                        if (cmd.Expression[0].Operations[0].Data is uint id 
                            && cmd.Expression[0].Operations[STR_LOAD_INDEX].Op == STR_LOAD_OP 
                            && cmd.Expression[0].Operations[STR_LOAD_INDEX].Data is EditableString data)
                        {
                            var character = data.Text;
                            if (cmd.Expression[0].Operations.Count >= STR_LOAD_INDEX2)
                            {
                                if (Convert.ToByte(cmd.Expression[0].Operations[STR_LOAD_ID2_INDEX].Data) == 0x03 && cmd.Expression[0].Operations[STR_LOAD_INDEX2].Data is EditableString data2)
                                {
                                    character += $" -> {data2.Text}";
                                }
                            }
                            if (nameMap.TryGetValue(character, out var newChar))
                            {
                                name = newChar.Split(" -> ");
                            }
                        }
                    }
                    var charName = name[0];
                    var charOverrideName = name.Length == 2 ? name[1] : name[0];
                    var expr = SecScenarioProgram.LegacyVersion ? new List<Expression>
                    {
                        new(0, 1, [
                            new ExpressionOperation(0x20, FLG_NAME),
                            new ExpressionOperation(0x50, null),
                            new ExpressionOperation(0x23, null),

                            new ExpressionOperation(0x50, null),
                            new ExpressionOperation(0x13, (byte)0x01),
                            new ExpressionOperation(0x24, null),
                            new ExpressionOperation(0x1E, new EditableString(charName, true)),

                            new ExpressionOperation(0x42, null),
                            new ExpressionOperation(0x50, null),
                            new ExpressionOperation(0x13, (byte)0x00),
                            new ExpressionOperation(0x24, null),
                            new ExpressionOperation(0x13, (byte)0x01),
                            new ExpressionOperation(0x3C, null),

                            new ExpressionOperation(0x50, null),
                            new ExpressionOperation(0x13, (byte)0x03),
                            new ExpressionOperation(0x24, null),
                            new ExpressionOperation(0x1E, new EditableString(charOverrideName, true)),

                            new ExpressionOperation(0x42, null),
                            new ExpressionOperation(0x50, null),
                            new ExpressionOperation(0x13, (byte)0x02),
                            new ExpressionOperation(0x24, null),
                            new ExpressionOperation(0x13, (byte)0x01),
                            new ExpressionOperation(0x3C, null),
                        ])
                    }:
                    new List<Expression>
                    {
                        new(0, 1, [
                            new ExpressionOperation(0x78, FLG_NAME),

                            new ExpressionOperation(0x79, (byte)0x01),
                            new ExpressionOperation(0x7A, new EditableString(charName, true)),
                            new ExpressionOperation(0x42, null),
                            new ExpressionOperation(0x7B, (byte)0x00),

                            new ExpressionOperation(0x79, (byte)0x03),
                            new ExpressionOperation(0x7A, new EditableString(charOverrideName, true)),
                            new ExpressionOperation(0x42, null),
                            new ExpressionOperation(0x7B, (byte)0x02)
                        ]),
                        cmd.Expression[1]
                    };

                    //Copy old clauses
                    if (cmd.Expression[0].Operations[STR_LOAD_ID2_INDEX].Op == STR_LOAD_POS_OP1 && Convert.ToByte(cmd.Expression[0].Operations[STR_LOAD_ID2_INDEX].Data) == 0x03)
                    {
                        //origially has overrde name, skip
                        expr[0].Operations.AddRange(cmd.Expression[0].Operations[NAME_EXPR_CP_POS2..]);
                    }
                    else
                    {
                        expr[0].Operations.AddRange(cmd.Expression[0].Operations[NAME_EXPR_CP_POS1..]);
                    }

                    var ret = new ExecutorCommand(0x1B, 0x12, expr, cmd.Offset.Old);
                    return ret;
                }
                else if ((type & TYPE_TITLE) != 0)//this is a title string
                {
                    if (cmdref == null || cmdref is not ExecutorCommand cmd || cmd.Expression == null || (!SecScenarioProgram.LegacyVersion && cmd.Expression.Count < 2))
                    {
                        throw new Exception("Generate title need a correct command refrence");
                    }
                    ExpressionOperation c1;
                    ExpressionOperation c2;
                    if ((type & TITLE_SCENE) != 0)
                    {
                        c1 = new ExpressionOperation(STR_LOAD_POS_OP1, (byte)0x01);
                        c2 = new ExpressionOperation(STR_LOAD_POS_OP2, (byte)0x00);
                    }
                    else
                    {
                        c1 = new ExpressionOperation(STR_LOAD_POS_OP1, (byte)0x03);
                        c2 = new ExpressionOperation(STR_LOAD_POS_OP2, (byte)0x02);
                    }
                    var expr = SecScenarioProgram.LegacyVersion ? new List<Expression>
                    {
                        new(0, 1, [
                            new ExpressionOperation(0x20, FLG_TITLE),
                            new ExpressionOperation(0x50, null),
                            new ExpressionOperation(0x23, null),

                            new ExpressionOperation(0x50, null),
                            c1,
                            new ExpressionOperation(0x24, null),

                            new ExpressionOperation(0x1E, new EditableString(dialogue.Text, true)),
                            new ExpressionOperation(0x42, null),

                            new ExpressionOperation(0x50, null),
                            c2,
                            new ExpressionOperation(0x24, null),

                            new ExpressionOperation(0x13, (byte)0x01),
                            new ExpressionOperation(0x3C, null),
                            new ExpressionOperation(0x52, null),
                            new ExpressionOperation(0xFF, null)
                        ])
                    }: 
                    new List<Expression>
                    {
                        new(0, 1, [
                            new ExpressionOperation(0x78, FLG_TITLE),

                            c1,
                            new ExpressionOperation(0x7A, new EditableString(dialogue.Text, true)),
                            new ExpressionOperation(0x42, null),
                            c2,

                            new ExpressionOperation(0x52, null),
                            new ExpressionOperation(0xFF, null)
                        ]),
                        cmd.Expression[1]
                    };
                    var ret = new ExecutorCommand(0x1B, 0x12, expr, cmd.Offset.Old);
                    return ret;
                }
                else if ((type & TYPE_SELECT) != 0)
                {
                    if (cmdref == null || cmdref is not ExecutorCommand cmd || cmd.Expression == null || (!SecScenarioProgram.LegacyVersion && cmd.Expression.Count < 2))
                    {
                        throw new Exception("Generate selection need a correct command refrence");
                    }
                    var expr = SecScenarioProgram.LegacyVersion ? new List<Expression>
                    {
                        new(0, 1, [
                            new ExpressionOperation(0x20, FLG_SELECT),
                            new ExpressionOperation(0x50, null),
                            new ExpressionOperation(0x23, null),
                            new ExpressionOperation(0x50, null),
                            new ExpressionOperation(0x13, (byte)0x01),
                            new ExpressionOperation(0x24, null),

                            new ExpressionOperation(0x1E, new EditableString(dialogue.Text, true)),
                            new ExpressionOperation(0x42, null),
                            new ExpressionOperation(0x50, null),
                            new ExpressionOperation(0x13, (byte)0x00),
                            new ExpressionOperation(0x24, null),
                            new ExpressionOperation(0x13, (byte)0x01),

                            new ExpressionOperation(0x3C, null),
                            new ExpressionOperation(0x52, null),
                            new ExpressionOperation(0xFF, null)
                        ])
                    } : 
                    new List<Expression>
                    {
                        new(0, 1, [
                            new ExpressionOperation(0x78, FLG_SELECT),

                            new ExpressionOperation(0x79, (byte)0x01),
                            new ExpressionOperation(0x7A, new EditableString(dialogue.Text, true)),
                            new ExpressionOperation(0x42, null),
                            new ExpressionOperation(0x7B, (byte)0x00),

                            new ExpressionOperation(0x52, null),
                            new ExpressionOperation(0xFF, null)
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

            IEnumerable<Tuple<int, int, int>> GetTextBlockProp()
            {
                var code = current.Code;
                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i] is EditableString s || (code[i] is ExecutorCommand e && e.ExecutorIndex == 0x1F9))
                    {
                        var start = i;
                        while (code[i] is not ExecutorCommand cmd || cmd.ExecutorIndex != 0x03)
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
                        if (cmd.Expression == null || (!SecScenarioProgram.LegacyVersion && cmd.Expression.Count < 2))
                        {
                            continue;
                        }
                        var operations = cmd.Expression[0].Operations;
                        if (operations[0].Op != STR_PUSH_OP || (SecScenarioProgram.LegacyVersion && operations.Count < STR_LOAD_INDEX))
                        {
                            continue;
                        }
                        //This is kind of string related
                        if (operations[0].Data is not uint id || operations[STR_LOAD_INDEX].Op != STR_LOAD_OP || operations[STR_LOAD_INDEX].Data is not EditableString data)
                        {
                            continue;
                        }
                        if (id == FLG_NAME)
                        {
                            yield return new Tuple<int, int, int>(TYPE_CHARNAME, i, i + 1);
                        }
                        else if (id == FLG_TITLE)
                        {
                            if (operations[STR_LOAD_POS].Op == STR_LOAD_POS_OP1)
                            {
                                if (Convert.ToByte(operations[STR_LOAD_POS].Data) == 0x01)
                                {
                                    yield return new Tuple<int, int, int>(TYPE_TITLE | TITLE_SCENE, i, i + 1);
                                }
                                else
                                {
                                    yield return new Tuple<int, int, int>(TYPE_TITLE | TITLE_PLOT, i, i + 1);
                                }
                            }
                        }
                        else if (id == FLG_SELECT)
                        {
                            yield return new Tuple<int, int, int>(TYPE_SELECT, i, i + 1);
                        }
                    }
                }
            }

            for (int i = 0; i < secCode.Count; i++)
            {
                if (secCode[i] is not NamedCode nc)
                {
                    continue;
                }

                if (!dict.TryGetValue(Path.GetFileName(nc.Name), out var dialogues))
                {
                    continue;
                }

                current = nc;
                Console.WriteLine($"Importing {nc.Name}...");

                try
                {
                    var nnc = new NamedCode(nc.Name, []);
                    int prevPos = 0;
                    foreach (var t in GetTextBlockProp())
                    {
                        if (dialogues.Count == 0)
                        {
                            throw new Exception("Dialogue count doesn't match");
                        }
                        nnc.Code.AddRange(nc.Code[prevPos..t.Item2]);
                        var generatedBlock = ConvertDialogue(t.Item1, dialogues[0], nc.Code[t.Item2]);
                        if (generatedBlock is List<object> l)
                        {
                            nnc.Code.AddRange(l);
                        }
                        else
                        {
                            nnc.Code.Add(generatedBlock);
                        }
                        prevPos = t.Item3;
                        if ((t.Item1 & TYPE_CHARNAME) == 0)
                        {
                            dialogues.RemoveAt(0);
                        }
                    }
                    nnc.Code.AddRange(current.Code[prevPos..]);
                    secCode[i] = nnc;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while trying to import {nc.Name}, this file will be skipped.");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        public static Script ImportText(string path)
        {
            bool haveFormatError = false;
            var ret = new Script();
            foreach (var file in Directory.EnumerateFiles(path, "*.txt", SearchOption.AllDirectories))
            {
                Console.WriteLine($"Reading {file}...");
                var messages = new List<Dialogue>();
                foreach (var line in File.ReadAllLines(file))
                {
                    if (line.StartsWith('★'))
                    {
                        var text = line.Split('★');
                        if (text.Length < 4)
                        {
                            haveFormatError = true;
                            Console.WriteLine($"Invalid format: {line}");
                            continue;
                        }
                        messages.Add(new Dialogue(text[2], text[3]));
                    }
                }
                ret.Add(new Tuple<string, List<Dialogue>>(Path.GetFileNameWithoutExtension(file), messages));
            }

            if (haveFormatError)
            {
                throw new ArgumentException($"Format error detected. See messages above.");
            }
            return ret;
        }

        public static void ExportText(string outputPath, Script text, bool blankLine = false)
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
                    if (blankLine && dialogue.Text != "")
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

        public static void ExportStrings(List<string> strs, string path)
        {
            using (var sw = new StreamWriter(File.OpenWrite(path)))
            {
                var idx = 0;
                foreach (var s in strs)
                {
                    sw.WriteLine($"☆{idx:X8}☆{s}");
                    sw.WriteLine($"★{idx:X8}★{s}\n");
                    idx++;
                }
            }
        }
        public static List<string> ImportStrings(string path)
        {
            List<string> result = [];
            foreach (var line in File.ReadAllLines(path))
            {
                if (line.StartsWith('★'))
                {
                    var text = line.Split('★');
                    result.Add(text[2]);
                }
            }
            return result;
        }
    }

}


