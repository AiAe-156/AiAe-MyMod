using System.Collections.Generic;

namespace RadialMenuZH
{
    /// <summary>
    /// 英文标签 -> 中文 翻译表。
    /// 术语全部对齐 Klei 官方简体中文（来自游戏 strings_preinstalled_zh_klei.po）。
    ///
    /// 关键：同一个英文词在不同轮盘里官方译法不同，必须按上下文区分，例如
    ///   "Automation"：概览轮盘 = 自动化 / 建造轮盘 = 信号
    /// 因此按 ctx 分表查询。
    /// ctx: 0=工具轮盘  1=概览轮盘  2=建造分类轮盘  3=建筑列表(已是中文，不翻)
    /// </summary>
    public static class Translations
    {
        // 顶层轮盘名 + 中心文字特殊词，所有上下文通用
        private static readonly Dictionary<string, string> Common = new Dictionary<string, string>
        {
            { "Tools", "工具" },
            { "Overlays", "概览" },
            { "Build Menus", "建造" },
            { "Buildings", "建筑" },
        };

        // ctx 0：工具（对应 STRINGS.UI.TOOLS.*）
        private static readonly Dictionary<string, string> Tools = new Dictionary<string, string>
        {
            { "Dig", "挖掘" },
            { "Mop", "擦拭" },
            { "Sweep", "清扫" },
            { "Deconstruct", "拆除" },
            { "Prioritize", "优先度" },
            { "Attack", "攻击" },
            { "Disinfect", "消毒" },
            { "Harvest", "收获" },
            { "Empty Pipe", "清空管道" },
            { "Disconnect", "拆断" },
            { "Wrangle", "捕捉" },
            { "Cancel", "取消" },
        };

        // ctx 1：概览（对应 STRINGS.UI.OVERLAYS.*，去掉官方“概览”后缀以适配小按钮）
        private static readonly Dictionary<string, string> Overlays = new Dictionary<string, string>
        {
            { "Oxygen", "氧气" },
            { "Power", "电力" },
            { "Temperature", "温度" },
            { "Gas Pipe", "气体管道" },
            { "Liquid Pipe", "液体管道" },
            { "Automation", "自动化" },
            { "Shipping", "运输" },
            { "Decor", "装饰" },
            { "Light", "光照" },
            { "Germs", "病菌" },
            { "Agriculture", "耕种" },
            { "Rooms", "房间" },
            { "Suits", "太空服" },
            { "Materials", "材料" },
        };

        // ctx 2：建造分类（对应 STRINGS.UI.BUILDCATEGORIES.*）
        private static readonly Dictionary<string, string> Build = new Dictionary<string, string>
        {
            { "Base", "基地" },
            { "Oxygen", "氧气" },
            { "Power", "电力" },
            { "Food", "食物" },
            { "Plumbing", "水管" },
            { "Ventilation", "通风" },
            { "Refinement", "精炼" },
            { "Medicine", "医疗" },
            { "Furniture", "家具" },
            { "Equipment", "站台" },
            { "Utilities", "实用" },
            { "Automation", "信号" },
            { "Shipping", "运输" },
            { "Rocketry", "火箭" },
        };

        public static string Translate(string text, int ctx)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string val;
            if (Common.TryGetValue(text, out val)) return val;

            Dictionary<string, string> dict = null;
            switch (ctx)
            {
                case 0: dict = Tools; break;
                case 1: dict = Overlays; break;
                case 2: dict = Build; break;
                default: dict = null; break; // 建筑列表：建筑名本身已是游戏中文，原样返回
            }
            if (dict != null && dict.TryGetValue(text, out val)) return val;

            return text; // 未知文本（建筑名、动态内容）原样放行，零副作用
        }
    }
}
