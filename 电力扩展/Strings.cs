﻿using STRINGS;

namespace PowerExtension
{
    public class Strings
    {
        // ======================== 自定义配方标签文本 ========================
        public class EXT_MISC
        {
            public class TAGS
            {
                public static LocString POWEREXT_IONMETAL = "离子活性金属";
            }
        }

        public class EXT_UI
        {
            public class FRONTEND
            {
                public class POWEREXTENSION
                {
                    public class MODOPTIONS
                    {
                        public class BATTERYCAPACITY
                        {
                            public static LocString NAME = "空气燃料电源容量 (千焦)";
                            public static LocString TOOLTIP = "设置空气燃料电源的最大电量。";
                        }
                        public class IONBATTERYCAPACITY
                        {
                            public static LocString NAME = "离子电池列容量 (千焦)";
                            public static LocString TOOLTIP = "设置高科技离子电池列的最大电量。";
                        }
                    }
                    public class IONBATTERY
                    {
                        public static LocString SLIDER_TITLE = "变压器输出上限";
                        public static LocString SLIDER_TOOLTIP = "限制该电池向右侧输出端口输送的最大功率。\n当前限制为：<b>{0} W</b>";
                    }
                }
            }
        }

        public class EXT_BUILDINGS
        {
            public class PREFABS
            {
                public class MEDIUMWIRE
                {
                    public static LocString NAME = global::STRINGS.UI.FormatAsLink("高压导线", "MEDIUMWIRE");
                    public static LocString DESC = "外观与导线相似，但采用橡胶包裹与强化金属进行了特殊加固，可承担更高负载。";
                    public static LocString EFFECT = "连接建筑物和<link=\"POWER\">电</link>源，负载上限 <b>10 千瓦</b>。\n\n需以精炼金属 + 橡胶/塑料建造。\n\n可穿过墙体和地面。";
                }
                public class MEDIUMWIREBRIDGE
                {
                    public static LocString NAME = global::STRINGS.UI.FormatAsLink("高压导线桥", "MEDIUMWIREBRIDGE");
                    public static LocString DESC = "能够承受 10 千瓦负载的导线桥。";
                    public static LocString EFFECT = "可以运载比普通<link=\"WIREREFINEDBRIDGE\">导线桥</link>更高的<link=\"POWER\">功率</link>而不会过载，上限 <b>10 千瓦</b>。\n\n让一条电线可以越过另一条电线，而不会连在一起。\n\n需以精炼金属 + 橡胶/塑料建造。\n\n可穿过墙体和地面。";
                }
                public class POWERTRANSFORMER2KW
                {
                    public static LocString NAME = global::STRINGS.UI.FormatAsLink("中级变压器", "POWERTRANSFORMER2KW");
                    public static LocString DESC = "这是一个变压器，但更精密一点。";
                    public static LocString EFFECT = "限制流过变压器的电力为2千瓦。 \n\n在大的一端接上蓄电池以作为一种调节阀，并防止电线获取电力超过2千瓦。 \n\n建造前可旋转。";
                }
                public class IONBATTERYARRAY
                {
                    public static LocString NAME = global::STRINGS.UI.FormatAsLink("先进离子电池列", "IONBATTERYARRAY");
                    public static LocString DESC = "一种基于多价离子技术的预储能式先进电池，预存储海量电能，即建即用，并提供自动化接口和可调节的变压输出功能。";
                    public static LocString EFFECT = "具有<link=\"BATTERYSMART\">智能电池</link>逻辑的重型先进储电设备。\n\n由于其高活性的化学反应，它在运作时会散发惊人的热量，并且拥有较快的自放电损耗。\n\n建造需要稀有的深空材料与特定的高活性金属。";
                }
                public class STACKBATTERY
                {
                    public static LocString NAME = global::STRINGS.UI.FormatAsLink("重型智能电池", "STACKBATTERY");
                    public static LocString DESC = "一种与蓄电池仓同一科技分支，以更高的成本换取极高的储能密度和智能控制，专门用于重型配电室部署的重型高科技电池。\n\n使用钢或硬化合金加固机身，不仅可以防水，据说自身还可以堆叠至十层。\n\n<color=#6E6E6E>那个啰嗦的工程师说最好不要超过十层，但也没说超过了会怎样，对吧:)</color>";
                    public static LocString EFFECT = "可<b>落地建造</b>并<b>堆叠部署</b>的重型储电设备，亦可建于水中、被淹不损毁。由于极高的能量密度，组成阵列的发热不能忽视。\n\n具有<link=\"BATTERYSMART\">智能电池</link>的自动化逻辑：按充电阈值输出绿/红信号，并可在侧栏设定激活范围。";
                    public static LocString STACK_LIMIT_MSG = "重型智能电池最多堆叠 15 层。";
                }
            }

            // [关键] 状态条目定义
            public class STATUSITEMS
            {
                public class IONBATTERY_OUTPUTLIMIT
                {
                    public static LocString NAME = "功率输出限制：{0} W";
                    public static LocString TOOLTIP = "该电池向外输送电力的功率被限制为 <b>{0} W</b>。";
                }
            }
        }

        public class EXT_ITEMS
        {
            public class INDUSTRIAL_PRODUCTS
            {
                public class AIRBATTERY
                {
                    public static LocString NAME = global::STRINGS.UI.FormatAsLink("空气燃料电源", "AIRBATTERY");
                    public static LocString DESC = "一种一次性的高能<link=\"ELECTROBANK\">移动电源</link>。\n\n可以通过<link=\"LARGEELECTROBANKDISCHARGER\">大型放电器</link>或<link=\"SMALLELECTROBANKDISCHARGER\">袖珍放电器</link>为建筑物供电。\n\n复制人可以在<link=\"ADVANCEDCRAFTINGTABLE\">焊接台</link>制造新的空气燃料电源。";
                }
                public class EMPTYAIRBATTERY
                {
                    public static LocString NAME = global::STRINGS.UI.FormatAsLink("耗尽的空气燃料电源", "EMPTYAIRBATTERY");
                    public static LocString DESC = "一个已经完全失去电荷的空气燃料电源外壳。\n\n可以在焊接台回收利用。";
                }
            }
        }
    }
}
