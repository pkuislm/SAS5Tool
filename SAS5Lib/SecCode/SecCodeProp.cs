using SAS5Lib.SecVariable;
using System.Diagnostics;
using static SAS5Lib.SecCode.ExpressionOperation;

namespace SAS5Lib.SecCode
{
    static class SecCodeProp
    {
        public struct ExecutorProp
        {
            public int[] Version;
            public bool HaveExpression;
            public string Name;

            public override readonly string ToString()
            {
                return $"V: [{string.Join(',', Version)}], Expr: {(HaveExpression ? "true" : "false")}, N: \"{Name}\"";
            }
        }

        public static Dictionary<short, ExecutorProp> ExecutorProps = new()
        {
            //DecoderCmdExecutor .rdata:006AE720 .rdata:006AE338 .rdata:00749F50
            { 0x01, new() { HaveExpression = false, Version = [108000, 109000, 109002, 110000], Name = "DecoderCmd::Reset" } },
            { 0x02, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "DecoderCmd::Jmp" } },//CondJmp(uint dst, bool cond = true)
            { 0x03, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "DecoderCmd::Call" } },//CondCall(uint dst, bool cond = true)
            { 0x04, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "DecoderCmd::Ret" } },//CondRet(uint dst, bool cond = true)
            { 0x05, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "DecoderCmd" } },
            { 0x06, new() { HaveExpression = false, Version = [108000, 109000, 109002, 110000], Name = "DecoderCmd" } },
            { 0x07, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "DecoderCmd" } },

            //CalcCmdExecutor .rdata:006AE3D0 .rdata:006AE3D0 .rdata:0074A028
            { 0x10, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd" } },
            { 0x11, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd" } },
            { 0x12, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd::CallProcedureWithInstantiation" } },// PUSH new typename(data); CALL offset;
            { 0x13, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd" } },
            { 0x14, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd" } },
            { 0x15, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd" } },
            { 0x16, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd" } },
            { 0x17, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd" } },
            { 0x18, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd" } },
            { 0x19, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd" } },
            { 0x1A, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd" } },
            { 0x1B, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd" } },
            { 0x1C, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd" } },
            { 0x1D, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CalcCmd" } },

            //FeatureCmdExecutor .rdata:006AE5B0
            { 0x2C, new() { HaveExpression = true, Version = [109002, 110000], Name = "FeatureCmd" } },
            { 0x2D, new() { HaveExpression = true, Version = [109002, 110000], Name = "FeatureCmd" } },

            //ResourceCmdExecutor .rdata:006AE340 .rdata:006AE398 .rdata:00749FEC
            { 0x30, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ResourceCmd" } },
            { 0x31, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ResourceCmd" } },
            { 0x32, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ResourceCmd" } },

            //ImageResourceCmdExecutor .rdata:006AE780 .rdata:006AE17C .rdata:00749DF4
            { 0x34, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageResourceCmd" } },
            { 0x35, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageResourceCmd" } },

            //ExecSpeedCmdExecutor .rdata:006AE9E0 .rdata:006AE2F .rdata:00749FB0
            { 0x38, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ExecSpeedCmd" } },
            { 0x39, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ExecSpeedCmd" } },
            { 0x3A, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ExecSpeedCmd" } },
            { 0x3B, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ExecSpeedCmd" } },

            //WaitCmdExecutor (sub_4D0E30 sub_534CE0 sub_4EE470) NoExecutorIndexStruct
            { 0x3C, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "WaitCmd" } },

            //ContextCmdExecutor .rdata:006AE520 .rdata:006ADD38 .rdata:00749A18
            { 0x40, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ContextCmd" } },
            { 0x41, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ContextCmd" } },
            { 0x42, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ContextCmd" } },
            { 0x43, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ContextCmd" } },
            { 0x44, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ContextCmd" } },
            { 0x45, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ContextCmd" } },
            { 0x46, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ContextCmd" } },
            { 0x47, new() { HaveExpression = false, Version = [108000, 109000, 109002, 110000], Name = "ContextCmd" } },

            //FrameHiderCmdExecutor .data:0071B930 .data:00706BE0 .data:007AC1C8
            { 0x50, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "FrameHiderCmd" } },
            { 0x51, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "FrameHiderCmd" } },
            { 0x52, new() { HaveExpression = false, Version = [108000, 109000, 109002, 110000], Name = "FrameHiderCmd" } },

            //ClickCmdExecutor .data:0071BED0 .data:007067A0 .data:007ABD88
            { 0x60, new() { HaveExpression = false, Version = [108000, 109000, 109002, 110000], Name = "ClickCmd::Wait" } },
            { 0x61, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ClickCmd" } },
            { 0x62, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ClickCmd" } },
            { 0x63, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ClickCmd" } },

            //AutoClickCmdExecutor .rdata:006AE610 .rdata:006AE29 .rdata:00749EE4
            { 0x68, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AutoClickCmd" } },
            { 0x69, new() { HaveExpression = false, Version = [108000, 109000, 109002, 110000], Name = "AutoClickCmd" } },
            { 0x6A, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AutoClickCmd" } },

            //SelectCmdExecutor .data:0071B970 .data:007067F0 .data:007ABDD8
            { 0x70, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "SelectCmd" } },
            { 0x71, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "SelectCmd" } },
            { 0x72, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "SelectCmd" } },
            { 0x73, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "SelectCmd" } },

            //MessageHistoryCmdExecutor .rdata:006AEA50 .rdata:006ADFC8 .rdata:00749C50
            { 0x80, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "MessageHistoryCmd" } },
            { 0x81, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "MessageHistoryCmd" } },
            { 0x82, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "MessageHistoryCmd" } },
            { 0x83, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "MessageHistoryCmd" } },
            { 0x84, new() { HaveExpression = false, Version = [109002, 110000], Name = "MessageHistoryCmd" } },

            //ScreenCmdExecutor .rdata:006AE950 .rdata:006ADE74 .rdata:00749AB4
            { 0x90, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ScreenCmd" } },
            { 0x91, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ScreenCmd" } },

            //GraphCmdExecutor .data:0071BA00 .data:00706880 .data:007ABE68
            { 0xA0, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "GraphCmd" } },
            { 0xA1, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "GraphCmd" } },
            { 0xA2, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "GraphCmd" } },
            { 0xA3, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "GraphCmd" } },
            { 0xA4, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "GraphCmd" } },
            { 0xA5, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "GraphCmd" } },
            { 0xA6, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "GraphCmd" } },
            { 0xA7, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "GraphCmd" } },
            { 0xA8, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "GraphCmd" } },

            //UserCmdCmdExecutor .data:0071BE10 .data:00706C20 .data:007AC208
            { 0xB0, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "UserCmdCmd" } },
            { 0xB1, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "UserCmdCmd" } },

            //UserSettingCmdExecutor .rdata:006AEB80 .rdata:006ADEC8 .rdata:00749B50
            { 0xB8, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "UserSettingCmd" } },
            { 0xB9, new() { HaveExpression = true, Version = [109002, 110000], Name = "UserSettingCmd" } },

            //UserLayerAttributeCmdExecutor .rdata:006AE890
            { 0xBC, new() { HaveExpression = true, Version = [109002, 110000], Name = "UserLayerAttributeCmd" } },
            { 0xBD, new() { HaveExpression = true, Version = [109002, 110000], Name = "UserLayerAttributeCmd" } },

            //Camera3DCmdExecutor .rdata:006AECC0 .rdata:006ADDA4 .rdata:00749A84
            { 0xC0, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "Camera3DCmd" } },
            { 0xC1, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "Camera3DCmd" } },
            { 0xC2, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "Camera3DCmd" } },

            //Camera3DLayerCmdExecutor .rdata:006AE900 .rdata:006AE214 .rdata:00749E68
            { 0xC4, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "Camera3DLayerCmd" } },
            { 0xC5, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "Camera3DLayerCmd" } },

            //DynamicLayerControllerCmdExecutor .data:0071BE40 .data:00706710 .data:007ABCF8
            { 0xE0, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE1, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE2, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE3, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE4, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE5, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE6, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE7, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "DynamicLayerControllerCmd" } },

            //MovieLayerCmdExecutor .data:0071B9C0 .data:00706840 .data:007ABE28
            { 0xF0, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "MovieLayerCmd" } },
            { 0xF1, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "MovieLayerCmd" } },
            { 0xF2, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "MovieLayerCmd" } },

            //AudioLayerCmdExecutor .rdata:006AEBB0 .rdata:006ADF1C .rdata:00749BA4
            { 0x100, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AudioLayerCmd" } },
            { 0x101, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AudioLayerCmd" } },

            //AudioCmdExecutor .data:0071BD60 .data:00706C50 .data:007AC238
            { 0x110, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AudioCmd" } },
            { 0x111, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AudioCmd" } },
            { 0x112, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AudioCmd" } },
            { 0x113, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AudioCmd" } },
            { 0x114, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AudioCmd" } },
            { 0x115, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AudioCmd" } },
            { 0x116, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AudioCmd" } },
            { 0x117, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AudioCmd" } },
            { 0x118, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AudioCmd" } },
            { 0x119, new() { HaveExpression = true, Version = [109000, 109002, 110000], Name = "AudioCmd" } },

            //TextLayerCmdExecutor .rdata:006AE370 .rdata:006ADE28 .rdata:00749AF0
            { 0x120, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextLayerCmd" } },
            { 0x121, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextLayerCmd" } },
            { 0x122, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextLayerCmd" } },
            { 0x123, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextLayerCmd" } },
            { 0x124, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextLayerCmd" } },

            //CompositeLayerCmdExecutor .rdata:006AE7F0 .rdata:006ADEF8 .rdata:00749B80
            { 0x130, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CompositeLayerCmd" } },
            { 0x131, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "CompositeLayerCmd" } },

            //UserCmdLayerCmdExecutor .rdata:006AE9B0 .rdata:006AE2D8 .rdata:00749F14
            { 0x140, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "UserCmdLayerCmd" } },
            { 0x141, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "UserCmdLayerCmd" } },

            //InputStringCmdExecutorKui (sub_4CAA60 sub_56C470 sub_51C980) NoExecutorIndexStruct
            { 0x150, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "InputStringKuiCmd" } },

            //LayerManagerCmdExecutor .rdata:006AEC70 .rdata:006AE008 .rdata:00749C90
            { 0x170, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "LayerManagerCmd" } },
            { 0x171, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "LayerManagerCmd" } },
            { 0x172, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "LayerManagerCmd" } },
            { 0x173, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "LayerManagerCmd" } },
            { 0x174, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "LayerManagerCmd" } },

            //ImageLayerCmdExecutor .rdata:006AE280 .rdata:006AE0D0 .rdata:00749D30
            { 0x180, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },
            { 0x181, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },
            { 0x182, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },
            { 0x183, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },
            { 0x184, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },
            { 0x185, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },
            { 0x186, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },
            { 0x187, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },
            { 0x188, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },
            { 0x189, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },
            { 0x18A, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },
            { 0x18B, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },
            { 0x18C, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },
            { 0x18D, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ImageLayerCmd" } },

            //ColorLayerCmdExecutor .rdata:006AEAE0 .rdata:006ADF44 .rdata:00749BCC
            { 0x190, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ColorLayerCmd" } },
            { 0x191, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "ColorLayerCmd" } },

            //Mesh3DLayerCmdExecutor .rdata:006AE480 .rdata:006AE1F0
            { 0x194, new() { HaveExpression = true, Version = [109000, 109002, 110000], Name = "Mesh3DLayerCmd" } },
            { 0x195, new() { HaveExpression = true, Version = [109000, 109002, 110000], Name = "Mesh3DLayerCmd" } },

            //LayerCursorCmdExecutor .rdata:006AE8D0 .rdata:006AE0AC .rdata:00749DCC
            { 0x1A0, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "LayerCursorCmd" } },
            { 0x1A1, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "LayerCursorCmd" } },

            //EventManagerCmdExecutor .rdata:006AE874 .rdata:006ADDD4 .rdata:007499DC
            { 0x1B0, new() { HaveExpression = false, Version = [108000, 109000, 109002, 110000], Name = "EventManagerCmd" } },
            { 0x1B1, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "EventManagerCmd" } },
            { 0x1B2, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "EventManagerCmd" } },
            { 0x1B3, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "EventManagerCmd" } },

            //LayerEventCmdExecutor .rdata:006AE6B0 .rdata:006AE260 .rdata:00749EB4
            { 0x1C0, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "LayerEventCmd" } },
            { 0x1C1, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "LayerEventCmd" } },
            { 0x1C2, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "LayerEventCmd" } },

            //TimerEventCmdExecutor .rdata:006AE4C0 .rdata:006ADF68 .rdata:00749BF0
            { 0x1D0, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TimerEventCmd" } },
            { 0x1D1, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TimerEventCmd" } },
            { 0x1D2, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TimerEventCmd" } },
            { 0x1D3, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TimerEventCmd" } },
            { 0x1D4, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TimerEventCmd" } },

            //AdjustmentLayerCmdExecutor .rdata:006AEC10 .rdata:006AE06C .rdata:00749CF4
            { 0x1E0, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AdjustmentLayerCmd" } },
            { 0x1E1, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "AdjustmentLayerCmd" } },

            //TextAreaLayerCmdExecutor .data:0071BAA0 .data:00706920 .data:007ABF08
            { -1, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F0, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F1, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F3, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F4, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F5, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F6, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F7, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F8, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F9, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1FA, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1FB, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1FC, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1FD, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1FE, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1FF, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x200, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x201, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x202, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x203, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x204, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x205, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x206, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x207, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x208, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x209, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x20A, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x20B, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x20C, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x20D, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x20E, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x20F, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x210, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x211, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x212, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x213, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x214, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x215, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x216, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x217, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x218, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x219, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },
            { 0x21A, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "TextAreaLayerCmd" } },

            //StageCmdExecutor .rdata:006AE090 .data:00706610 .data:007ABBF8
            { 0x280, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "StageCmd" } },
            { 0x281, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "StageCmd" } },
            { 0x282, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "StageCmd" } },
            { 0x283, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "StageCmd" } },
            { 0x284, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "StageCmd" } },
            { 0x285, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "StageCmd" } },
            { 0x286, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "StageCmd" } },
            { 0x287, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "StageCmd" } },
            { 0x288, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "StageCmd" } },
            { 0x289, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "StageCmd" } },
            { 0x28A, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "StageCmd" } },
            { 0x28B, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "StageCmd" } },
            { 0x28C, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "StageCmd" } },
            { 0x28D, new() { HaveExpression = false, Version = [109002, 110000], Name = "StageCmd" } },

            //MainWindowCmdExecutor .data:007066F0 .data:007ABCD8
            { 0x2A0, new() { HaveExpression = true, Version = [108000, 109000], Name = "MainWindowCmd" } },

            //LayerBmpCaptureCmdExecutor .rdata:006ADEE0 .rdata:006AE4A8 .rdata:0074A0F8
            { 0x2B0, new() { HaveExpression = true, Version = [108000, 109000, 109002, 110000], Name = "LayerBmpCaptureCmd" } },
        };

        public static ExecutorProp GetExecutorProp(short index)
        {
            if (ExecutorProps.TryGetValue(index, out var e) && e.Version.Contains(SecScenarioProgram.Version))
            {
                return e;
            }
            else
            {
                throw new NotSupportedException($"Unknown ExecutorIndex:{index} under version({SecScenarioProgram.Version})");
            }
        }

        public static bool ExecutorHaveExpression(short index)
        {
            return GetExecutorProp(index).HaveExpression;
        }

        public static string ExecutorName(short index)
        {
            try
            {
                return GetExecutorProp(index).Name;
            }
            catch (NotSupportedException)
            {
                return "ExecutorCommand";
            }
        }

        public delegate object ClauseReadObject(BinaryReader reader);
        public delegate string ClauseArgFormatter(object? data);
        public struct ExprOpProp
        {
            public int[] Version;
            public JmpMode JumpMode;
            public ClauseReadObject? ReaderFunc;
            public ClauseArgFormatter FormatterFunc = OpFormatPrimtiveTypeArg;
            public string Name;

            public ExprOpProp(JmpMode mode, ClauseReadObject? f1, int[] ver, string name, ClauseArgFormatter? f2 = null) 
            {
                JumpMode = mode;
                ReaderFunc = f1;
                Version = ver;
                Name = name;
                if(f2 != null)
                {
                    FormatterFunc = f2;
                }
            }

            public override readonly string ToString()
            {
                return $"V: [{string.Join(',', Version)}], M: {JumpMode}, N: \"{Name}\"";
            }
        }

        public static string OpFormatPrimtiveTypeArg(object? data)
        {
            return $"{data}";
        }

        public static string OpFormatStringTypeArg(object? data)
        {
            if(data is EditableString s)
            {
                return $"\"{s.Text}\"";
            }
            return $"\"{data}\"";
        }

        public static string OpFormatVariableTypeIndexArg(object? data)
        {
            var index = Convert.ToInt32(data);
            return $"typeof(\"{VariableManager.Instance.GetVariableTypeName(index)}\")";
        }

        public static string OpFormatVariableTypeArg(object? data)
        {
            if(data is SecVariable.Object v)
            {
                return $"new {v.Type.Name}(...)";
            }
            else
            {
                return OpFormatPrimtiveTypeArg(data);
            }
        }
        public static string OpFormatVariableArrayTypeArg(object? data)
        {
            if(data is SecVariable.Object v)
            {
                return $"new {v.Type.Name}[{v.Data.Length / v.Type.Type.GetSize()}]{{...}}";
            }
            else
            {
                return OpFormatPrimtiveTypeArg(data);
            }
        }

        public static object OpReadChar(BinaryReader reader) { return reader.ReadSByte(); }

        public static object OpReadByte(BinaryReader reader) { return reader.ReadByte(); }

        public static object OpReadShort(BinaryReader reader) { return reader.ReadInt16(); }

        public static object OpReadWord(BinaryReader reader) { return reader.ReadUInt16(); }

        public static object OpReadInt(BinaryReader reader) { return reader.ReadInt32(); }

        public static object OpReadDword(BinaryReader reader) { return reader.ReadUInt32(); }

        public static object OpReadFloat(BinaryReader reader) { return reader.ReadSingle(); }

        public static object OpReadLong(BinaryReader reader) { return reader.ReadInt64(); }

        public static object OpReadQword(BinaryReader reader) { return reader.ReadUInt64(); }

        public static object OpReadDouble(BinaryReader reader) { return reader.ReadDouble(); }

        public static object OpReadString(BinaryReader reader)
        {
            var type = reader.ReadByte();
            Trace.Assert(type == 0);
            var length = reader.ReadByte();
            return new EditableString(CodepageManager.Instance.ImportGetString(reader.ReadBytes(length)));
        }

        public static object OpReadVar(BinaryReader reader)
        {
            var index = reader.ReadInt32();
            var vt = VariableManager.Instance.GetType(index);
            return new SecVariable.Object(index, vt, reader.ReadBytes(vt.Type.GetSize()));
        }

        public static object OpReadVarArray(BinaryReader reader)
        {
            var index = reader.ReadInt32();
            var count = reader.ReadInt32();
            var vt = VariableManager.Instance.GetType(index);
            if(vt.Type is PrimitiveType pt && pt.PrimitiveTypeID == 0)
            {
                //char array => string?
                var b = reader.ReadBytes(count);
                return new EditableString(CodepageManager.Instance.ImportGetString(b));
            }
            return new SecVariable.Object(index, vt, reader.ReadBytes(vt.Type.GetSize() * count));
        }

        public static object OpReadNativeFunCall(BinaryReader reader) { return new NativeFunCall(reader); }


        public static Dictionary<byte, ExprOpProp> ClauseOpProps = new()
        {
            { 0x00, new(JmpMode.None, null, [], "") },
            { 0x01, new(JmpMode.None, null, [], "") },
            { 0x02, new(JmpMode.None, null, [], "") },
            { 0x03, new(JmpMode.None, null, [], "") },
            { 0x04, new(JmpMode.None, null, [], "") },
            { 0x05, new(JmpMode.None, null, [], "") },
            { 0x06, new(JmpMode.None, null, [], "") },
            { 0x07, new(JmpMode.None, null, [], "") },
            { 0x08, new(JmpMode.Direct, OpReadDword, [108000, 109000, 109002, 110000], "ExprJmp") },
            { 0x09, new(JmpMode.Offset, OpReadInt, [108000, 109000, 109002, 110000], "ExprJmpOffset") },
            { 0x0A, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ExprRet") },
            { 0x0B, new(JmpMode.Direct, OpReadDword, [108000, 109000, 109002, 110000], "ExprCondJmp") },
            { 0x0C, new(JmpMode.Offset, OpReadInt, [108000, 109000, 109002, 110000], "ExprCondJmpOffset") },
            { 0x0D, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ExprCondJmpStack") },
            { 0x0E, new(JmpMode.Direct, OpReadDword, [108000, 109000, 109002, 110000], "ExprCall") },
            { 0x0F, new(JmpMode.Offset, OpReadInt, [108000, 109000, 109002, 110000], "ExprCallOffset") },
            { 0x10, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ExprCallStack") },
            { 0x11, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ExprRet") },
            { 0x12, new(JmpMode.Offset, OpReadInt, [108000, 109000, 109002, 110000], "PushOffset") },//TypeID = 6
            { 0x13, new(JmpMode.None, OpReadChar, [108000, 109000, 109002, 110000], "PushInt8") },//TypeID = 0
            { 0x14, new(JmpMode.None, OpReadShort, [108000, 109000, 109002, 110000], "PushInt16") },//TypeID = 0
            { 0x15, new(JmpMode.None, OpReadInt, [108000, 109000, 109002, 110000], "PushInt32") },
            { 0x16, new(JmpMode.None, OpReadLong, [108000, 109000, 109002, 110000], "PushInt64") },//TypeID = 1
            { 0x17, new(JmpMode.None, OpReadByte, [108000, 109000, 109002, 110000], "PushUInt8") },//TypeID = 0
            { 0x18, new(JmpMode.None, OpReadWord, [108000, 109000, 109002, 110000], "PushUInt16") },//TypeID = 0
            { 0x19, new(JmpMode.Direct, OpReadDword, [108000, 109000, 109002, 110000], "PushUInt32") },// *In old version this command might contains an address
            { 0x1A, new(JmpMode.None, OpReadQword, [108000, 109000, 109002, 110000], "PushUInt64") },//TypeID = 1
            { 0x1B, new(JmpMode.None, OpReadFloat, [108000, 109000, 109002, 110000], "PushFloat") },//TypeID = 2
            { 0x1C, new(JmpMode.None, OpReadDouble, [108000, 109000, 109002, 110000], "PushDouble") },//TypeID = 3
            { 0x1D, new(JmpMode.None, OpReadVar, [108000, 109000, 109002, 110000], "PushVariable", OpFormatVariableTypeArg) },//TypeID = 4
            { 0x1E, new(JmpMode.None, OpReadVarArray, [108000, 109000, 109002, 110000], "PushVariableArray4", OpFormatVariableArrayTypeArg) },//TypeID = 4 (int typeIndex, int elemCount)
            { 0x1F, new(JmpMode.None, null, [], "") },
            { 0x20, new(JmpMode.None, OpReadDword, [108000, 109000, 109002, 110000], "PushEmptyVariable", OpFormatVariableTypeIndexArg) },//TypeID = 4
            { 0x21, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "VariableCopyInPlace") },//TypeID 4 -> 4
            { 0x22, new(JmpMode.None, null, [], "") },
            { 0x23, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "VariableCopyInplaceToRef") },//TypeID 4 -> 5
            { 0x24, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "RecordGetProperty") },
            { 0x25, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "TupleGetProperty") },
            { 0x26, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ArrayGetProperty") },
            { 0x27, new(JmpMode.None, null, [], "") },
            { 0x28, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PushNullTypeVariable") },//TypeID = 4
            { 0x29, new(JmpMode.None, OpReadByte, [108000, 109000, 109002, 110000], "PushPresetObj") },//TypeID = 4
            { 0x2A, new(JmpMode.None, OpReadByte, [108000, 109000, 109002, 110000], "PushPresetObjRef") },//TypeID = 5
            { 0x2B, new(JmpMode.None, null, [], "") },
            { 0x2C, new(JmpMode.None, null, [], "") },
            { 0x2D, new(JmpMode.None, null, [], "") },
            { 0x2E, new(JmpMode.None, null, [], "") },
            { 0x2F, new(JmpMode.None, null, [], "") },
            { 0x30, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertyGetInt8") },//TypeID 5 -> 0 PrimitiveTypeId = 0
            { 0x31, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertyGetInt16") },//TypeID 5 -> 0 PrimitiveTypeId = 1
            { 0x32, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertyGetInt32") },//TypeID 5 -> 0 PrimitiveTypeId = 2
            { 0x33, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertyGetInt64") },//TypeID 5 -> 0 PrimitiveTypeId = 3
            { 0x34, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertyGetUInt8") },//TypeID 5 -> 0 PrimitiveTypeId = 0
            { 0x35, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertyGetUInt16") },
            { 0x36, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertyGetUInt32") },
            { 0x37, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertyGetUInt64") },
            { 0x38, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertyGetFloat") },
            { 0x39, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertyGetDouble") },
            { 0x3A, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertyGetObject") },
            { 0x3B, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertyGetObjectRef") },
            { 0x3C, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertySetInt8") },
            { 0x3D, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertySetInt16") },
            { 0x3E, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertySetInt32") },
            { 0x3F, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertySetInt64") },
            { 0x40, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertySetFloat") },
            { 0x41, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertySetDouble") },
            { 0x42, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertySetObject") },
            { 0x43, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertySetObjectRef") },
            { 0x44, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "SetVariable") },//operator=
            { 0x45, new(JmpMode.None, null, [], "") },
            { 0x46, new(JmpMode.None, null, [], "") },
            { 0x47, new(JmpMode.None, null, [], "") },
            { 0x48, new(JmpMode.None, null, [], "") },
            { 0x49, new(JmpMode.None, null, [], "") },
            { 0x4A, new(JmpMode.None, null, [], "") },
            { 0x4B, new(JmpMode.None, null, [], "") },
            { 0x4C, new(JmpMode.None, null, [], "") },
            { 0x4D, new(JmpMode.None, null, [], "") },
            { 0x4E, new(JmpMode.None, null, [], "") },
            { 0x4F, new(JmpMode.None, null, [], "") },
            { 0x50, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "DeepCopyStackVar") },
            { 0x51, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "CopyStackVar") },
            { 0x52, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "Pop") },
            { 0x53, new(JmpMode.None, OpReadByte, [108000, 109000, 109002, 110000], "PopN") },
            { 0x54, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "Push") },
            { 0x55, new(JmpMode.None, OpReadByte, [108000, 109000, 109002, 110000], "PushN") },
            { 0x56, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PushLocalFrame") },
            { 0x57, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PopLocalFrame") },
            { 0x58, new(JmpMode.None, OpReadByte, [108000, 109000, 109002, 110000], "CopyStackVar1") },
            { 0x59, new(JmpMode.None, OpReadByte, [108000, 109000, 109002, 110000], "CopyStackVar2") },
            { 0x5A, new(JmpMode.None, OpReadDword, [108000, 109000, 109002, 110000], "PushEmptyVariableArray") },
            { 0x5B, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ArrayConcat") },
            { 0x5C, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ArrayResize") },
            { 0x5D, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ArrayCompare") },
            { 0x5E, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ArraySize") },
            { 0x5F, new(JmpMode.None, null, [], "") },
            { 0x60, new(JmpMode.None, OpReadByte, [108000, 109000, 109002, 110000], "CopyStackVar3") },
            { 0x61, new(JmpMode.None, OpReadByte, [108000, 109000, 109002, 110000], "CopyStackVar4") },
            { 0x62, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "CopyStackVar5") },
            { 0x63, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "CopyStackVar6") },
            { 0x64, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PushLocalFramePtr") },
            { 0x65, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PushStackSize") },
            { 0x66, new(JmpMode.None, null, [], "") },
            { 0x67, new(JmpMode.None, null, [], "") },
            { 0x68, new(JmpMode.None, null, [], "") },
            { 0x69, new(JmpMode.None, null, [], "") },
            { 0x6A, new(JmpMode.None, null, [], "") },
            { 0x6B, new(JmpMode.None, null, [], "") },
            { 0x6C, new(JmpMode.None, null, [], "") },
            { 0x6D, new(JmpMode.None, null, [], "") },
            { 0x6E, new(JmpMode.None, null, [], "") },
            { 0x6F, new(JmpMode.None, null, [], "") },
            { 0x70, new(JmpMode.Direct, OpReadDword, [108000, 109000, 109002, 110000], "PushAddr") },
            { 0x71, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertyGetAddr") },
            { 0x72, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "PropertySetAddr") },
            { 0x73, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "AddrADD") },
            { 0x74, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "AddrEQ") },
            { 0x75, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "AddrNEQ") },
            { 0x76, new(JmpMode.None, null, [], "") },
            { 0x77, new(JmpMode.None, null, [], "") },
            { 0x78, new(JmpMode.None, OpReadDword, [108000, 109000, 109002, 110000], "PushEmptyVariableRef", OpFormatVariableTypeIndexArg) },
            { 0x79, new(JmpMode.None, OpReadByte, [108000, 109000, 109002, 110000], "RecordGetPerportyByIndex") },
            { 0x7A, new(JmpMode.None, OpReadString, [108000, 109000, 109002, 110000], "PushByteArrayVariable", OpFormatStringTypeArg) },
            { 0x7B, new(JmpMode.None, OpReadByte, [108000, 109000, 109002, 110000], "PerportySetTrueByIndex") },
            { 0x7C, new(JmpMode.None, null, [], "") },
            { 0x7D, new(JmpMode.None, null, [], "") },
            { 0x7E, new(JmpMode.None, null, [], "") },
            { 0x7F, new(JmpMode.None, null, [], "") },
            { 0x80, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ADD32") },
            { 0x81, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ADD64") },
            { 0x82, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ADDFloat") },
            { 0x83, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ADDDouble") },
            { 0x84, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "SUB32") },
            { 0x85, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "SUB64") },
            { 0x86, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "SUBFloat") },
            { 0x87, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "SUBDouble") },
            { 0x88, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "MUL32") },
            { 0x89, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "MUL64") },
            { 0x8A, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "MULFloat") },
            { 0x8B, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "MULDouble") },
            { 0x8C, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "IDIV32") },
            { 0x8D, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "IDIV64") },
            { 0x8E, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "DIVFloat") },
            { 0x8F, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "DIVDouble") },
            { 0x90, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "DIV32") },
            { 0x91, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "DIV64") },
            { 0x92, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "IMOD32") },
            { 0x93, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "IMOD64") },
            { 0x94, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "MOD32") },
            { 0x95, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "MOD64") },
            { 0x96, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "IDIV32_S") },
            { 0x97, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "IDIV64_S") },
            { 0x98, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "DIV32_S") },
            { 0x99, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "DIV64_S") },
            { 0x9A, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "IMOD32_S") },
            { 0x9B, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "IMOD64_S") },
            { 0x9C, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "MOD32_S") },
            { 0x9D, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "MOD64_S") },
            { 0x9E, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "BAND32") },
            { 0x9F, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "BAND64") },
            { 0xA0, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "BOR32") },
            { 0xA1, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "BOR64") },
            { 0xA2, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "XOR32") },
            { 0xA3, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "XOR64") },
            { 0xA4, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "SHL32") },
            { 0xA5, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "SHL64") },
            { 0xA6, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ISHR32") },
            { 0xA7, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "ISHR64") },
            { 0xA8, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "SHR32") },
            { 0xA9, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "SHR64") },
            { 0xAA, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "BEQ32") },
            { 0xAB, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "BEQ64") },
            { 0xAC, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "BEQFloat") },
            { 0xAD, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "BEQDouble") },
            { 0xAE, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "BEQObj") },
            { 0xAF, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "BEQObjRef") },
            { 0xB0, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "NEQ32") },
            { 0xB1, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "NEQ64") },
            { 0xB2, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "NEQFloat") },
            { 0xB3, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "NEQDouble") },
            { 0xB4, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "NEQObj") },
            { 0xB5, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "NEQObjRef") },
            { 0xB6, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "LT32") },
            { 0xB7, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "LT64") },
            { 0xB8, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "LTFloat") },
            { 0xB9, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "LTDouble") },
            { 0xBA, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "GT32") },
            { 0xBB, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "GT64") },
            { 0xBC, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "GTFloat") },
            { 0xBD, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "GTDouble") },
            { 0xBE, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "LEQ32") },
            { 0xBF, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "LEQ64") },
            { 0xC0, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "LEQFloat") },
            { 0xC1, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "LEQDouble") },
            { 0xC2, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "GEQ32") },
            { 0xC3, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "GEQ64") },
            { 0xC4, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "GEQFloat") },
            { 0xC5, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "GEQDouble") },
            { 0xC6, new(JmpMode.None, null, [], "") },
            { 0xC7, new(JmpMode.None, null, [], "") },
            { 0xC8, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "NEG32") },
            { 0xC9, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "NEG64") },
            { 0xCA, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "NEGFloat") },
            { 0xCB, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "NEGDouble") },
            { 0xCC, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "NOT32") },
            { 0xCD, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "NOT64") },
            { 0xCE, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "TestZero32") },
            { 0xCF, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "TestZero64") },
            { 0xD0, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "TestZeroFloat") },
            { 0xD1, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "TestZeroDouble") },
            { 0xD2, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "TestZeroObj") },
            { 0xD3, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "TestZeroObjRef") },
            { 0xD4, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "TestNotZero32") },
            { 0xD5, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "TestNotZero64") },
            { 0xD6, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "TestNotZeroFloat") },
            { 0xD7, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "TestNotZeroDouble") },
            { 0xD8, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "TestNotZeroObj") },
            { 0xD9, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "TestNotZeroObjRef") },
            { 0xDA, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "Int64ToInt32") },
            { 0xDB, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "FloatToInt32") },
            { 0xDC, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "DoubleToInt32") },
            { 0xDD, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "Int32ToInt64") },
            { 0xDE, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "FloatToInt64") },
            { 0xDF, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "DoubleToInt64") },
            { 0xE0, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "Int32ToFloat") },
            { 0xE1, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "Int64ToFloat") },
            { 0xE2, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "DoubleToFloat") },
            { 0xE3, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "Int32ToDouble") },
            { 0xE4, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "Int64ToDouble") },
            { 0xE5, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "FloatToDouble") },
            { 0xE6, new(JmpMode.None, null, [], "") },
            { 0xE7, new(JmpMode.None, null, [], "") },
            { 0xE8, new(JmpMode.None, null, [], "") },
            { 0xE9, new(JmpMode.None, null, [], "") },
            { 0xEA, new(JmpMode.None, null, [], "") },
            { 0xEB, new(JmpMode.None, null, [], "") },
            { 0xEC, new(JmpMode.None, null, [], "") },
            { 0xED, new(JmpMode.None, null, [], "") },
            { 0xEE, new(JmpMode.None, null, [], "") },
            { 0xEF, new(JmpMode.None, null, [], "") },
            { 0xF0, new(JmpMode.None, null, [], "") },
            { 0xF1, new(JmpMode.None, null, [], "") },
            { 0xF2, new(JmpMode.None, null, [], "") },
            { 0xF3, new(JmpMode.None, null, [], "") },
            { 0xF4, new(JmpMode.None, null, [], "") },
            { 0xF5, new(JmpMode.None, null, [], "") },
            { 0xF6, new(JmpMode.None, null, [], "") },
            { 0xF7, new(JmpMode.None, null, [], "") },
            { 0xF8, new(JmpMode.None, null, [], "") },
            { 0xF9, new(JmpMode.None, null, [], "") },
            { 0xFA, new(JmpMode.None, null, [], "") },
            { 0xFB, new(JmpMode.None, null, [], "") },
            { 0xFC, new(JmpMode.None, null, [], "") },
            { 0xFD, new(JmpMode.None, null, [], "") },
            { 0xFE, new(JmpMode.None, OpReadNativeFunCall, [108000, 109000, 109002, 110000], "NativeCall") },
            { 0xFF, new(JmpMode.None, null, [108000, 109000, 109002, 110000], "END") },
        };

        public static ExprOpProp GetOpProp(byte op)
        {
            if (ClauseOpProps.TryGetValue(op, out var opProp) && opProp.Version.Contains(SecScenarioProgram.Version))
            {
                return opProp;
            }
            else
            {
                throw new NotSupportedException($"Unknown op:{op} under version({SecScenarioProgram.Version})");
            }
        }
        public static JmpMode GetOpJmpMode(byte op)
        {
            return GetOpProp(op).JumpMode;
        }
        public static string ClauseName(byte index)
        {
            string name = string.Empty;
            try
            {
                name = GetOpProp(index).Name;
            }
            catch (NotSupportedException)
            {
            }

            if(string.IsNullOrEmpty(name))
            {
                name = $"Expr_{index:X2}";
            }

            return name;
        }
    }
}
