using STRINGS;

namespace ConstantTemperatureCooler;

public static class STRINGS
{
    public static class UI
    {
        public static class UISIDESCREENS
        {
            public static class SMARTCONDITIONERCONTROL
            {
                public static LocString TITLE = (LocString)"温度调控";

                public static LocString TARGET_TITLE = (LocString)"目标温度";
                public static LocString TARGET_TOOLTIP = (LocString)"调节器最低将流体温度冷却至该值。\n当前目标: {0}";

                public static LocString MAXDELTA_TITLE = (LocString)"最大降温幅度";
                public static LocString MAXDELTA_TOOLTIP = (LocString)"单次通过管道可降低的最大温度。\n当前设置: {0}°C";

                public static LocString MAXPOWER_TITLE = (LocString)"最大功率限制";
                public static LocString MAXPOWER_TOOLTIP = (LocString)"设备允许消耗的最大功率。\n当前设置: {0}W";

                public static LocString CHECKBOX_TITLE = (LocString)"调控模式";
                public static LocString CHECKBOX_LABEL = (LocString)"功率限制模式";
                public static LocString CHECKBOX_TOOLTIP = (LocString)"勾选: 以功率限制为准\n取消: 以温度限制为准";

                public static LocString ACTIVE_SUFFIX = (LocString)" [生效中]";
                public static LocString INACTIVE_SUFFIX = (LocString)" [参考值]";
            }
        }
    }
}
