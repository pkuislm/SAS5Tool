using System.Diagnostics;

namespace SAS5CodeDisasembler.SecCode
{
    class ExecutorCommand
    {
        public CodeOffset Offset;
        public byte Type;
        public short ExecutorIndex;
        public List<Expression>? Expression;

        public bool HasExpression()
        {
            switch (ExecutorIndex)
            {
                //DecoderCmdExecutor .rdata:006AE720
                case 0x01:
                    return false;
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                    return true;
                case 0x06:
                    return false;
                case 0x07:
                    return true;

                //CalcCmdExecutor .rdata:006AE3D0
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                case 0x15:
                case 0x16:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1A:
                case 0x1B:
                case 0x1C:
                case 0x1D:
                    return true;

                //FeatureCmdExecutors_0 .rdata:006AE5B0
                case 0x2C:
                case 0x2D:
                    return true;

                //ResourceCmdExecutor .rdata:006AE340
                case 0x30:
                case 0x31:
                case 0x32:
                    return true;

                //ImageResourceCmdExecutor .rdata:006AE780
                case 0x34:
                case 0x35:
                    return true;

                //ExecSpeedCmdExecutor .rdata:006AE9E0 
                case 0x38:
                case 0x39:
                case 0x3A:
                case 0x3B:
                    return true;

                //WaitCmdExecutor(sub_4D0E30) NoExecutorIndexStruct
                case 0x3C:
                    return true;

                //ContextCmdExecutor .rdata:006AE520
                case 0x40:
                case 0x41:
                case 0x42:
                case 0x43:
                case 0x44:
                case 0x45:
                case 0x46:
                    return true;
                case 0x47:
                    return false;

                //FrameHiderCmdExecutor .data:0071B930
                case 0x50:
                    return true;
                case 0x51:
                case 0x52:
                    return false;

                //ClickCmdExecutor .data:0071BED0
                case 0x60:
                    return false;
                case 0x61:
                case 0x62:
                case 0x63:
                    return true;

                //AutoClickCmdExecutor .rdata:006AE610
                case 0x68:
                    return true;
                case 0x69:
                    return false;
                case 0x6A:
                    return true;

                //SelectCmdExecutor .data:0071B970
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                    return true;

                //FeatureCmdExecutor .rdata:006AEA50
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                    return true;
                case 0x84:
                    return false;

                //ScreenCmdExecutor
                case 0x90:
                case 0x91:
                    return true;

                //GraphCmdExecutor .data:0071BA00
                case 0xA0:
                case 0xA1:
                case 0xA2:
                case 0xA3:
                case 0xA4:
                case 0xA5:
                case 0xA6:
                case 0xA7:
                case 0xA8:
                    return true;

                //UserCmdCmdExecutor
                case 0xB0:
                case 0xB1:
                    return true;

                //UserSettingCmdExecutor
                case 0xB8:
                case 0xB9:
                    return true;

                //UserLayerAttributeCmdExecutor .rdata:006AE890
                case 0xBC:
                case 0xBD:
                    return true;

                //Camera3DCmdExecutor .rdata:006AECC0
                case 0xC0:
                case 0xC1:
                case 0xC2:
                    return true;
                //Camera3DLayerCmdExecutor .rdata:006AE900
                case 0xC4:
                case 0xC5:
                    return true;

                //DynamicLayerControllerCmdExecutor .data:0071BE40
                case 0xE0:
                case 0xE1:
                case 0xE2:
                case 0xE3:
                case 0xE4:
                case 0xE5:
                case 0xE6:
                case 0xE7:
                    return true;

                //MovieLayerCmdExecutor .data:0071B9B8
                case 0xF0:
                case 0xF1:
                case 0xF2:
                    return true;

                //AudioLayerCmdExecutor .rdata:006AEBB0
                case 0x100:
                case 0x101:
                    return true;

                //AudioCmdExecutor .data:0071BD60
                case 0x110:
                case 0x111:
                case 0x112:
                case 0x113:
                case 0x114:
                case 0x115:
                case 0x116:
                case 0x117:
                case 0x118:
                case 0x119:
                    return true;

                //TextLayerCmdExecutor .rdata:006AE370
                case 0x120:
                case 0x121:
                case 0x122:
                case 0x123:
                case 0x124:
                    return true;

                //CompositeLayerCmdExecutor .rdata:006AE7F0
                case 0x130:
                case 0x131:
                    return true;

                //UserCmdLayerCmdExecutor .rdata:006AE9B0
                case 0x140:
                case 0x141:
                    return true;

                //InputStringCmdExecutorKui
                case 0x150:
                    return true;

                //LayerManagerCmdExecutor .rdata:006AEC70
                case 0x170:
                case 0x171:
                case 0x172:
                case 0x173:
                case 0x174:
                    return true;

                //ImageLayerCmdExecutor .rdata:006AE280
                case 0x180:
                case 0x181:
                case 0x182:
                case 0x183:
                case 0x184:
                case 0x185:
                case 0x186:
                case 0x187:
                case 0x188:
                case 0x189:
                case 0x18A:
                case 0x18B:
                case 0x18C:
                case 0x18D:
                    return true;

                //ColorLayerCmdExecutor .rdata:006AEAE0
                case 0x190:
                case 0x191:
                    return true;

                //Mesh3DLayerCmdExecutor
                case 0x194:
                case 0x195:
                    return true;

                //LayerCursorCmdExecutor
                case 0x1A0:
                case 0x1A1:
                    return true;

                //EventManagerCmdExecutor .rdata:006AE874
                case 0x1B0:
                    return false;
                case 0x1B1:
                case 0x1B2:
                case 0x1B3:
                    return true;

                //LayerEventCmdExecutor .rdata:006AE6B0
                case 0x1C0:
                case 0x1C1:
                case 0x1C2:
                    return true;
                //TimerEventCmdExecutor .rdata:006AE4C0
                case 0x1D0:
                case 0x1D1:
                case 0x1D2:
                case 0x1D3:
                case 0x1D4:
                    return true;

                //AdjustmentLayerCmdExecutor .rdata:006AEC10
                case 0x1E0:
                case 0x1E1:
                    return true;

                //TextAreaLayerCmdExecutor .data:0071BAA0
                case -1:
                    return true;
                case 0x1F0:
                case 0x1F1:
                case 0x1F3:
                case 0x1F4:
                case 0x1F5:
                case 0x1F6:
                case 0x1F7:
                case 0x1F8:
                case 0x1F9:
                case 0x1FA:
                case 0x1FB:
                case 0x1FC:
                case 0x1FD:
                case 0x1FE:
                case 0x1FF:
                case 0x200:
                case 0x201:
                case 0x202:
                case 0x203:
                case 0x204:
                case 0x205:
                case 0x206:
                case 0x207:
                case 0x208:
                case 0x209:
                case 0x20A:
                case 0x20B:
                case 0x20C:
                case 0x20D:
                case 0x20E:
                case 0x20F:
                case 0x210:
                case 0x211:
                case 0x212:
                case 0x213:
                case 0x214:
                case 0x215:
                case 0x216:
                case 0x217:
                case 0x218:
                case 0x219:
                case 0x21A:
                    return true;

                //StageCmdExecutor .rdata:006AE090
                case 0x280:
                case 0x281:
                case 0x282:
                case 0x283:
                case 0x284:
                case 0x285:
                case 0x286:
                case 0x287:
                case 0x288:
                case 0x289:
                case 0x28A:
                case 0x28B:
                case 0x28C:
                    return true;
                case 0x28D:
                    return false;

                //LayerBmpCaptureCmdExecutor .rdata:006ADEE0
                case 0x2B0:
                    return true;

                default: return false;

            }
        }

        public void GetExpression(BinaryReader reader)
        {
            Expression = [];
            byte b;
            while ((b = reader.ReadByte()) != 0xFF)
            {
                var exprLen = reader.ReadInt16();
                var expr = new Expression(exprLen, b, reader.BaseStream.Position, reader.ReadBytes(exprLen));
                Expression.Add(expr);
            }
        }

        public void Write(BinaryWriter writer, ref Dictionary<long, long> addresses)
        {
            Offset.New = writer.BaseStream.Position;
            addresses.TryAdd(Offset.Old, Offset.New);

            writer.Write(Type);
            if (Type == 0x1B)
            {
                writer.Write(ExecutorIndex);
                if (Expression != null)
                {
                    foreach (var expr in Expression)
                    {
                        expr.Write(writer, ref addresses);
                    }
                }
                writer.Write((byte)0xFF);
            }
        }

        public override string ToString()
        {
            return $"ExecutorCommand(Offset: ({Offset.Old:X8},{Offset.New:X8}), ExecutorIndex: {ExecutorIndex:X4}, Expression(s): {Expression?.Count})";
        }
    }
}
