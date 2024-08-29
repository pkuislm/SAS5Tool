using SAS5Lib.SecVariable;
using System.Diagnostics;
using static SAS5Lib.SecCode.ExpressionClause;

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
            { 0x01, new() { HaveExpression = false, Version = [108000, 109000, 110000], Name = "DecoderCmd" } },
            { 0x02, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "DecoderCmd" } },
            { 0x03, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "DecoderCmd" } },
            { 0x04, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "DecoderCmd" } },
            { 0x05, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "DecoderCmd" } },
            { 0x06, new() { HaveExpression = false, Version = [108000, 109000, 110000], Name = "DecoderCmd" } },
            { 0x07, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "DecoderCmd" } },

            //CalcCmdExecutor .rdata:006AE3D0 .rdata:006AE3D0 .rdata:0074A028
            { 0x10, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },
            { 0x11, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },
            { 0x12, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },
            { 0x13, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },
            { 0x14, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },
            { 0x15, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },
            { 0x16, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },
            { 0x17, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },
            { 0x18, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },
            { 0x19, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },
            { 0x1A, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },
            { 0x1B, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },
            { 0x1C, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },
            { 0x1D, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CalcCmd" } },

            //FeatureCmdExecutor .rdata:006AE5B0
            { 0x2C, new() { HaveExpression = true, Version = [110000], Name = "FeatureCmd" } },
            { 0x2D, new() { HaveExpression = true, Version = [110000], Name = "FeatureCmd" } },

            //ResourceCmdExecutor .rdata:006AE340 .rdata:006AE398 .rdata:00749FEC
            { 0x30, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ResourceCmd" } },
            { 0x31, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ResourceCmd" } },
            { 0x32, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ResourceCmd" } },

            //ImageResourceCmdExecutor .rdata:006AE780 .rdata:006AE17C .rdata:00749DF4
            { 0x34, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageResourceCmd" } },
            { 0x35, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageResourceCmd" } },

            //ExecSpeedCmdExecutor .rdata:006AE9E0 .rdata:006AE2F .rdata:00749FB0
            { 0x38, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ExecSpeedCmd" } },
            { 0x39, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ExecSpeedCmd" } },
            { 0x3A, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ExecSpeedCmd" } },
            { 0x3B, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ExecSpeedCmd" } },

            //WaitCmdExecutor (sub_4D0E30 sub_534CE0 sub_4EE470) NoExecutorIndexStruct
            { 0x3C, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "WaitCmd" } },

            //ContextCmdExecutor .rdata:006AE520 .rdata:006ADD38 .rdata:00749A18
            { 0x40, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ContextCmd" } },
            { 0x41, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ContextCmd" } },
            { 0x42, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ContextCmd" } },
            { 0x43, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ContextCmd" } },
            { 0x44, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ContextCmd" } },
            { 0x45, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ContextCmd" } },
            { 0x46, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ContextCmd" } },
            { 0x47, new() { HaveExpression = false, Version = [108000, 109000, 110000], Name = "ContextCmd" } },

            //FrameHiderCmdExecutor .data:0071B930 .data:00706BE0 .data:007AC1C8
            { 0x50, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "FrameHiderCmd" } },
            { 0x51, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "FrameHiderCmd" } },
            { 0x52, new() { HaveExpression = false, Version = [108000, 109000, 110000], Name = "FrameHiderCmd" } },

            //ClickCmdExecutor .data:0071BED0 .data:007067A0 .data:007ABD88
            { 0x60, new() { HaveExpression = false, Version = [108000, 109000, 110000], Name = "ClickCmd" } },
            { 0x61, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ClickCmd" } },
            { 0x62, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ClickCmd" } },
            { 0x63, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ClickCmd" } },

            //AutoClickCmdExecutor .rdata:006AE610 .rdata:006AE29 .rdata:00749EE4
            { 0x68, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AutoClickCmd" } },
            { 0x69, new() { HaveExpression = false, Version = [108000, 109000, 110000], Name = "AutoClickCmd" } },
            { 0x6A, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AutoClickCmd" } },

            //SelectCmdExecutor .data:0071B970 .data:007067F0 .data:007ABDD8
            { 0x70, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "SelectCmd" } },
            { 0x71, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "SelectCmd" } },
            { 0x72, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "SelectCmd" } },
            { 0x73, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "SelectCmd" } },

            //MessageHistoryCmdExecutor .rdata:006AEA50 .rdata:006ADFC8 .rdata:00749C50
            { 0x80, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "MessageHistoryCmd" } },
            { 0x81, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "MessageHistoryCmd" } },
            { 0x82, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "MessageHistoryCmd" } },
            { 0x83, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "MessageHistoryCmd" } },
            { 0x84, new() { HaveExpression = false, Version = [110000], Name = "MessageHistoryCmd" } },

            //ScreenCmdExecutor .rdata:006AE950 .rdata:006ADE74 .rdata:00749AB4
            { 0x90, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ScreenCmd" } },
            { 0x91, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ScreenCmd" } },

            //GraphCmdExecutor .data:0071BA00 .data:00706880 .data:007ABE68
            { 0xA0, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "GraphCmd" } },
            { 0xA1, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "GraphCmd" } },
            { 0xA2, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "GraphCmd" } },
            { 0xA3, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "GraphCmd" } },
            { 0xA4, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "GraphCmd" } },
            { 0xA5, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "GraphCmd" } },
            { 0xA6, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "GraphCmd" } },
            { 0xA7, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "GraphCmd" } },
            { 0xA8, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "GraphCmd" } },

            //UserCmdCmdExecutor .data:0071BE10 .data:00706C20 .data:007AC208
            { 0xB0, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "UserCmdCmd" } },
            { 0xB1, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "UserCmdCmd" } },

            //UserSettingCmdExecutor .rdata:006AEB80 .rdata:006ADEC8 .rdata:00749B50
            { 0xB8, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "UserSettingCmd" } },
            { 0xB9, new() { HaveExpression = true, Version = [110000], Name = "UserSettingCmd" } },

            //UserLayerAttributeCmdExecutor .rdata:006AE890
            { 0xBC, new() { HaveExpression = true, Version = [110000], Name = "UserLayerAttributeCmd" } },
            { 0xBD, new() { HaveExpression = true, Version = [110000], Name = "UserLayerAttributeCmd" } },

            //Camera3DCmdExecutor .rdata:006AECC0 .rdata:006ADDA4 .rdata:00749A84
            { 0xC0, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "Camera3DCmd" } },
            { 0xC1, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "Camera3DCmd" } },
            { 0xC2, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "Camera3DCmd" } },

            //Camera3DLayerCmdExecutor .rdata:006AE900 .rdata:006AE214 .rdata:00749E68
            { 0xC4, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "Camera3DLayerCmd" } },
            { 0xC5, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "Camera3DLayerCmd" } },

            //DynamicLayerControllerCmdExecutor .data:0071BE40 .data:00706710 .data:007ABCF8
            { 0xE0, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE1, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE2, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE3, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE4, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE5, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE6, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "DynamicLayerControllerCmd" } },
            { 0xE7, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "DynamicLayerControllerCmd" } },

            //MovieLayerCmdExecutor .data:0071B9B8 .data:00706840 .data:007ABE28
            { 0xF0, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "MovieLayerCmd" } },
            { 0xF1, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "MovieLayerCmd" } },
            { 0xF2, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "MovieLayerCmd" } },

            //AudioLayerCmdExecutor .rdata:006AEBB0 .rdata:006ADF1C .rdata:00749BA4
            { 0x100, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AudioLayerCmd" } },
            { 0x101, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AudioLayerCmd" } },

            //AudioCmdExecutor .data:0071BD60 .data:00706C50 .data:007AC238
            { 0x110, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AudioCmd" } },
            { 0x111, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AudioCmd" } },
            { 0x112, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AudioCmd" } },
            { 0x113, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AudioCmd" } },
            { 0x114, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AudioCmd" } },
            { 0x115, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AudioCmd" } },
            { 0x116, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AudioCmd" } },
            { 0x117, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AudioCmd" } },
            { 0x118, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AudioCmd" } },
            { 0x119, new() { HaveExpression = true, Version = [109000, 110000], Name = "AudioCmd" } },

            //TextLayerCmdExecutor .rdata:006AE370 .rdata:006ADE28 .rdata:00749AF0
            { 0x120, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextLayerCmd" } },
            { 0x121, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextLayerCmd" } },
            { 0x122, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextLayerCmd" } },
            { 0x123, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextLayerCmd" } },
            { 0x124, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextLayerCmd" } },

            //CompositeLayerCmdExecutor .rdata:006AE7F0 .rdata:006ADEF8 .rdata:00749B80
            { 0x130, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CompositeLayerCmd" } },
            { 0x131, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "CompositeLayerCmd" } },

            //UserCmdLayerCmdExecutor .rdata:006AE9B0 .rdata:006AE2D8 .rdata:00749F14
            { 0x140, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "UserCmdLayerCmd" } },
            { 0x141, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "UserCmdLayerCmd" } },

            //InputStringCmdExecutorKui (sub_4CAA60 sub_56C470 sub_51C980) NoExecutorIndexStruct
            { 0x150, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "InputStringKuiCmd" } },

            //LayerManagerCmdExecutor .rdata:006AEC70 .rdata:006AE008 .rdata:00749C90
            { 0x170, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "LayerManagerCmd" } },
            { 0x171, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "LayerManagerCmd" } },
            { 0x172, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "LayerManagerCmd" } },
            { 0x173, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "LayerManagerCmd" } },
            { 0x174, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "LayerManagerCmd" } },

            //ImageLayerCmdExecutor .rdata:006AE280 .rdata:006AE0D0 .rdata:00749D30
            { 0x180, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },
            { 0x181, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },
            { 0x182, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },
            { 0x183, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },
            { 0x184, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },
            { 0x185, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },
            { 0x186, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },
            { 0x187, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },
            { 0x188, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },
            { 0x189, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },
            { 0x18A, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },
            { 0x18B, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },
            { 0x18C, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },
            { 0x18D, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ImageLayerCmd" } },

            //ColorLayerCmdExecutor .rdata:006AEAE0 .rdata:006ADF44 .rdata:00749BCC
            { 0x190, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ColorLayerCmd" } },
            { 0x191, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "ColorLayerCmd" } },

            //Mesh3DLayerCmdExecutor .rdata:006AE480 .rdata:006AE1F0
            { 0x194, new() { HaveExpression = true, Version = [109000, 110000], Name = "Mesh3DLayerCmd" } },
            { 0x195, new() { HaveExpression = true, Version = [109000, 110000], Name = "Mesh3DLayerCmd" } },

            //LayerCursorCmdExecutor .rdata:006AE8D0 .rdata:006AE0AC .rdata:00749DCC
            { 0x1A0, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "LayerCursorCmd" } },
            { 0x1A1, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "LayerCursorCmd" } },

            //EventManagerCmdExecutor .rdata:006AE874 .rdata:006ADDD4 .rdata:007499DC
            { 0x1B0, new() { HaveExpression = false, Version = [108000, 109000, 110000], Name = "EventManagerCmd" } },
            { 0x1B1, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "EventManagerCmd" } },
            { 0x1B2, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "EventManagerCmd" } },
            { 0x1B3, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "EventManagerCmd" } },

            //LayerEventCmdExecutor .rdata:006AE6B0 .rdata:006AE260 .rdata:00749EB4
            { 0x1C0, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "LayerEventCmd" } },
            { 0x1C1, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "LayerEventCmd" } },
            { 0x1C2, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "LayerEventCmd" } },

            //TimerEventCmdExecutor .rdata:006AE4C0 .rdata:006ADF68 .rdata:00749BF0
            { 0x1D0, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TimerEventCmd" } },
            { 0x1D1, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TimerEventCmd" } },
            { 0x1D2, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TimerEventCmd" } },
            { 0x1D3, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TimerEventCmd" } },
            { 0x1D4, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TimerEventCmd" } },

            //AdjustmentLayerCmdExecutor .rdata:006AEC10 .rdata:006AE06C .rdata:00749CF4
            { 0x1E0, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AdjustmentLayerCmd" } },
            { 0x1E1, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "AdjustmentLayerCmd" } },

            //TextAreaLayerCmdExecutor .data:0071BAA0 .data:00706920 .data:007ABF08
            { -1, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F0, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F1, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F3, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F4, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F5, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F6, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F7, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F8, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1F9, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1FA, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1FB, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1FC, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1FD, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1FE, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x1FF, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x200, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x201, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x202, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x203, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x204, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x205, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x206, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x207, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x208, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x209, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x20A, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x20B, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x20C, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x20D, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x20E, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x20F, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x210, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x211, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x212, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x213, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x214, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x215, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x216, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x217, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x218, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x219, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },
            { 0x21A, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "TextAreaLayerCmd" } },

            //StageCmdExecutor .rdata:006AE090 .data:00706610 .data:007ABBF8
            { 0x280, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "StageCmd" } },
            { 0x281, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "StageCmd" } },
            { 0x282, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "StageCmd" } },
            { 0x283, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "StageCmd" } },
            { 0x284, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "StageCmd" } },
            { 0x285, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "StageCmd" } },
            { 0x286, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "StageCmd" } },
            { 0x287, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "StageCmd" } },
            { 0x288, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "StageCmd" } },
            { 0x289, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "StageCmd" } },
            { 0x28A, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "StageCmd" } },
            { 0x28B, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "StageCmd" } },
            { 0x28C, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "StageCmd" } },
            { 0x28D, new() { HaveExpression = false, Version = [110000], Name = "StageCmd" } },

            //MainWindowCmdExecutor .data:007066F0 .data:007ABCD8
            { 0x2A0, new() { HaveExpression = true, Version = [108000, 109000], Name = "MainWindowCmd" } },

            //LayerBmpCaptureCmdExecutor .rdata:006ADEE0 .rdata:006AE4A8 .rdata:0074A0F8
            { 0x2B0, new() { HaveExpression = true, Version = [108000, 109000, 110000], Name = "LayerBmpCaptureCmd" } },
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
        public struct ClauseOpProp
        {
            public int[] Version;
            public JmpMode JumpMode;
            public ClauseReadObject? ReaderFunc;
            public string Name;

            public override readonly string ToString()
            {
                return $"V: [{string.Join(',', Version)}], M: {JumpMode}, N: \"{Name}\"";
            }
        }

        public static object ClauseReadChar(BinaryReader reader) { return reader.ReadSByte(); }

        public static object ClauseReadByte(BinaryReader reader) { return reader.ReadByte(); }

        public static object ClauseReadShort(BinaryReader reader) { return reader.ReadInt16(); }

        public static object ClauseReadWord(BinaryReader reader) { return reader.ReadUInt16(); }

        public static object ClauseReadInt(BinaryReader reader) { return reader.ReadInt32(); }

        public static object ClauseReadDword(BinaryReader reader) { return reader.ReadUInt32(); }

        public static object ClauseReadFloat(BinaryReader reader) { return reader.ReadSingle(); }

        public static object ClauseReadLong(BinaryReader reader) { return reader.ReadInt64(); }

        public static object ClauseReadQword(BinaryReader reader) { return reader.ReadUInt64(); }

        public static object ClauseReadDouble(BinaryReader reader) { return reader.ReadDouble(); }

        public static object ClauseReadString(BinaryReader reader)
        {
            var type = reader.ReadByte();
            Trace.Assert(type == 0);
            var length = reader.ReadByte();
            return new EditableString(CodepageManager.Instance.ImportGetString(reader.ReadBytes(length)));
        }

        public static object ClauseReadVar(BinaryReader reader)
        {
            var index = reader.ReadInt32();
            var vt = VariableManager.Instance.GetType(index);
            return new Variable(index, vt, reader.ReadBytes(vt.Type.GetSize()));
        }

        public static object ClauseReadVarArray(BinaryReader reader)
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
            return new Variable(index, vt, reader.ReadBytes(vt.Type.GetSize() * count));
        }

        public static object ClauseReadNativeFunCall(BinaryReader reader) { return new NativeFunCall(reader); }


        public static Dictionary<byte, ClauseOpProp> ClauseOpProps = new()
        {
            { 0x0D, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="CondJmpStack"} },
            { 0x10, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="CallStack"} },
            { 0x0A, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="Ret"} },
            { 0x11, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="Ret"} },
            { 0x21, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableCast"} },//TypeID 4 -> 4
            { 0x23, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableToRef"} },//TypeID 4 -> 5
            { 0x24, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="CastVariablePop0"} },
            { 0x26, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="CastArrayVariablePop0"} },
            { 0x28, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="PushNullTypeVariable"} },//TypeID = 4
            { 0x30, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableRefCast0"} },//TypeID 5 -> 0 PrimitiveTypeId = 0
            { 0x31, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableRefCast1"} },//TypeID 5 -> 0 PrimitiveTypeId = 1
            { 0x32, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableRefCast2"} },//TypeID 5 -> 0 PrimitiveTypeId = 2
            { 0x33, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableRefCast3"} },//TypeID 5 -> 0 PrimitiveTypeId = 3
            { 0x34, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableRefCast00"} },//TypeID 5 -> 0 PrimitiveTypeId = 0
            { 0x35, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableRefCast10"} },
            { 0x36, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableRefCast2"} },
            { 0x37, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableRefCast3"} },
            { 0x38, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableRefCast4"} },
            { 0x39, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableRefCast5"} },
            { 0x3A, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableRefCast6"} },
            { 0x3B, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="VariableRefCast7"} },
            { 0x3C, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x3D, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x3E, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x3F, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },

            { 0x40, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x41, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x42, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x43, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x44, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },

            { 0x50, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="Cmp_Eq"} },
            { 0x51, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x52, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="Pop"} },
            { 0x54, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="PushBool"} },
            { 0x56, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="PushLocalFrame"} },
            { 0x57, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="PopLocalFrame"} },
            { 0x5B, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x5C, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x5D, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x5E, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },

            { 0x62, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x63, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x64, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x65, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },

            { 0x71, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x72, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },

            { 0x80, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x81, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x82, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x83, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x84, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x85, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x86, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x87, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x88, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x89, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x8A, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x8B, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x8C, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x8D, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x8E, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x8F, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },

            { 0x90, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x91, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x92, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x93, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x94, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x95, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x96, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x97, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x98, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x99, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x9A, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x9B, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x9C, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x9D, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x9E, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0x9F, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },

            { 0xA0, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xA1, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xA2, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xA3, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xA4, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xA5, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xA6, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xA7, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xA8, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xA9, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xAA, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xAB, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xAC, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xAD, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xAE, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xAF, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },

            { 0xB0, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xB1, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xB2, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xB3, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xB4, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xB5, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xB6, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xB7, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xB8, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xB9, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xBA, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xBB, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xBC, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xBD, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xBE, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xBF, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },

            { 0xC0, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xC1, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xC2, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xC3, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xC4, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xC5, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xC8, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xC9, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xCA, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xCB, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xCC, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xCD, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xCE, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xCF, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },

            { 0xD0, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xD1, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xD2, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xD3, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xD4, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xD5, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xD6, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xD7, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xD8, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xD9, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xDA, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xDB, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xDC, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xDD, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xDE, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xDF, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },

            { 0xE0, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xE1, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xE2, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xE3, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xE4, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xE5, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name=""} },
            { 0xFF, new(){  JumpMode = JmpMode.None, ReaderFunc = null, Version=[108000, 109000, 110000], Name="END"} },

            { 0x13, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadChar, Version=[108000, 109000, 110000], Name="PushInt8"} },//TypeID = 0
            { 0x2A, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadByte, Version=[108000, 109000, 110000], Name="PushPresetObjRef"} },//TypeID = 5
            { 0x53, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadByte, Version=[108000, 109000, 110000], Name="PopRange"} },
            { 0x55, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadByte, Version=[108000, 109000, 110000], Name="PushBoolRange"} },
            { 0x58, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadByte, Version=[108000, 109000, 110000], Name=""} },
            { 0x59, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadByte, Version=[108000, 109000, 110000], Name=""} },
            { 0x60, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadByte, Version=[108000, 109000, 110000], Name=""} },
            { 0x61, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadByte, Version=[108000, 109000, 110000], Name=""} },
            { 0x79, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadByte, Version=[108000, 109000, 110000], Name=""} },
            { 0x7B, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadByte, Version=[108000, 109000, 110000], Name=""} },

            { 0x14, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadShort, Version=[108000, 109000, 110000], Name="PushInt16"} },//TypeID = 0
            { 0x18, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadWord, Version=[108000, 109000, 110000], Name="PushUInt16"} },//TypeID = 0

            { 0x08, new(){  JumpMode = JmpMode.Direct, ReaderFunc = ClauseReadDword, Version=[108000, 109000, 110000], Name="Jmp"} },
            { 0x09, new(){  JumpMode = JmpMode.Offset, ReaderFunc = ClauseReadDword, Version=[108000, 109000, 110000], Name="JmpOffset"} },
            { 0x0B, new(){  JumpMode = JmpMode.Direct, ReaderFunc = ClauseReadDword, Version=[108000, 109000, 110000], Name="CondJmp"} },
            { 0x0C, new(){  JumpMode = JmpMode.Offset, ReaderFunc = ClauseReadDword, Version=[108000, 109000, 110000], Name="CondJmpOffset"} },
            { 0x0E, new(){  JumpMode = JmpMode.Direct, ReaderFunc = ClauseReadDword, Version=[108000, 109000, 110000], Name="Call"} },
            { 0x0F, new(){  JumpMode = JmpMode.Offset, ReaderFunc = ClauseReadDword, Version=[108000, 109000, 110000], Name="CallOffset"} },
            { 0x12, new(){  JumpMode = JmpMode.Offset, ReaderFunc = ClauseReadDword, Version=[108000, 109000, 110000], Name="PushOffset"} },//TypeID = 6
            { 0x15, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadInt, Version=[108000, 109000, 110000], Name="PushInt"} },
            { 0x19, new(){  JumpMode = JmpMode.Direct, ReaderFunc = ClauseReadDword, Version=[108000, 109000, 110000], Name="PushUInt"} },// *In old version this command might contains an address
            { 0x1B, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadFloat, Version=[108000, 109000, 110000], Name="PushFloat"} },//TypeID = 2
            { 0x20, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadDword, Version=[108000, 109000, 110000], Name="PushEmptyVariable"} },//TypeID = 4
            { 0x5A, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadDword, Version=[108000, 109000, 110000], Name=""} },
            { 0x70, new(){  JumpMode = JmpMode.Direct, ReaderFunc = ClauseReadDword, Version=[108000, 109000, 110000], Name="PushAddr"} },
            { 0x78, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadDword, Version=[108000, 109000, 110000], Name=""} },

            { 0x1C, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadDouble, Version=[108000, 109000, 110000], Name="PushDouble"} },//TypeID = 3
            { 0x16, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadLong, Version=[108000, 109000, 110000], Name="PushLong"} },//TypeID = 1
            { 0x1A, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadQword, Version=[108000, 109000, 110000], Name="PushULong"} },//TypeID = 1

            { 0x7A, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadString, Version=[108000, 109000, 110000], Name=""} },

            { 0x1D, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadVar, Version=[108000, 109000, 110000], Name="PushVariable"} },//TypeID = 4
            { 0x1E, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadVarArray, Version=[108000, 109000, 110000], Name="PushVariableArray"} },//TypeID = 4 (int typeIndex, int elemCount)

            { 0xFE, new(){  JumpMode = JmpMode.None, ReaderFunc = ClauseReadNativeFunCall, Version=[108000, 109000, 110000], Name="NativeCall"} },
        };

        public static ClauseOpProp GetClauseOpProp(byte op)
        {
            if (ClauseOpProps.TryGetValue(op, out var opProp) && opProp.Version.Contains(SecScenarioProgram.Version))
            {
                return opProp;
            }
            else
            {
                throw new Exception($"Unknown clause op:{op} under version({SecScenarioProgram.Version})");
            }
        }
        public static JmpMode GetClauseJmpMode(byte op)
        {
            return GetClauseOpProp(op).JumpMode;
        }
    }
}
