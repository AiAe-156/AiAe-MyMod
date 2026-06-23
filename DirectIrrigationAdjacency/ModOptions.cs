using PeterHan.PLib.Options;

namespace DirectIrrigationAdjacency
{
    [ConfigFile("DirectIrrigationAdjacency.json", true, false)]
    public sealed class ModOptions
    {
        // 分类标题（数字前缀用于控制在选项面板里的显示顺序）。
        private const string CatEnv = "1·环境直接取水";
        private const string CatShare = "2·相邻共享 / 导管桥";
        private const string CatDeliver = "3·小人运送（兜底）";

        // ───────────────────────── 1·环境直接取水 ─────────────────────────
        [Option("启用环境四方向取水",
            "在植物上 / 左 / 右及下方各追加一个同款液体消费者\n" +
            "（复用 DirectIrrigation 的接线与限流），使其能从相邻格的环境液体直接取水。\n" +
            "关闭则只保留 DirectIrrigation 自身的「自身格」取水。",
            CatEnv)]
        public bool EnableEnvironmentConsumers { get; set; } = true;

        [Option("向下取水深度",
            "向下采样的格数。\n" +
            "种植砖（土培砖等）本体那一格是实心的，设为 2 可越过实心砖本体、把正下方那格的环境液体也取到；\n" +
            "种植箱本体非实心，无需调高。值越大每株多挂的 sim 消费者越多。",
            CatEnv)]
        [Limit(1, 3)]
        public int EnvironmentDownDepth { get; set; } = 2;

        [Option("环境取水含斜角",
            "在四方向之外再追加 4 个斜角方向的环境消费者。\n" +
            "会增加每株植物的 sim 消费者数量，按需开启。",
            CatEnv)]
        public bool EnvironmentDiagonal { get; set; } = false;

        [Option("环境取水速率倍率",
            "环境消费者每个的取水速率倍率（基准 0.5kg/秒/消费者）。\n" +
            "高需求作物（如顶针芦苇 160kg/周期）、或用一个水源喂一长排靠共享的砖时，可调大。\n" +
            "默认 1。",
            CatEnv)]
        [Limit(1.0, 20.0)]
        public float EnvironmentRateMultiplier { get; set; } = 1.0f;

        [Option("环境蓄水上限 (kg)",
            "每株环境取水蓄入灌溉存储的上限。默认 100（同 DirectIrrigation）。\n" +
            "高需求作物想要更大缓冲、或长排共享时可调高。",
            CatEnv)]
        [Limit(10.0, 2000.0)]
        public float EnvironmentCapacityKG { get; set; } = 100.0f;

        [Option("兜底覆盖未知作物",
            "对 DirectIrrigation 不认识、但确实需要液体灌溉的作物（如其他 mod 新增植物），\n" +
            "自造一个基础液体消费者，使其也能环境取水。\n" +
            "关闭则只克隆已有消费者（仅覆盖 DI 已支持的作物）。",
            CatEnv)]
        public bool EnvironmentSynthesizeMissing { get; set; } = true;

        // ───────────────────────── 2·相邻共享 / 导管桥 ─────────────────────────
        [Option("启用相邻共享",
            "在相邻种植箱 / 砖之间，按「元素需求」周期性均衡灌溉液体与固体肥料：\n" +
            "不同种类的容器也能共享，但只共享双方作物都需要的那个元素\n" +
            "（盐水大家共享、盐只在需盐的作物间、泥土只在需泥土的作物间）。\n" +
            "关闭则完全不共享。",
            CatShare)]
        public bool EnableNeighborSharing { get; set; } = true;

        [Option("启用空容器导管桥",
            "让空种植容器在「两个以上、对同一元素存在落差的已种植消费者」中间充当安全导管，\n" +
            "把该元素从更富一侧中转到缺口侧，打通隔着一个空格的两株作物。\n" +
            "不会把单株作物抽干（少于两个该元素消费者、或已均衡时空容器一滴都不抽）。\n" +
            "关闭则空容器不参与。",
            CatShare)]
        public bool EnableEmptyBridge { get; set; } = true;

        [Option("排除管道供液容器(液培砖)",
            "把液培砖(HydroponicFarm)、宽种植砖(WideFarmTile)等「管道供液」容器排除出\n" +
            "「相邻共享」与「导管桥」：它们仍可照常进行环境直接取水（本模组的环境取水不受影响），\n" +
            "只是不再和邻居互相均衡液体、也不充当导管桥。\n" +
            "关闭则这些容器也纳入相邻共享。",
            CatShare)]
        public bool ExcludeHydroponic { get; set; } = true;

        [Option("允许斜角补给",
            "开启后相邻种植箱 / 砖的均衡共享会包含 4 个斜角；\n" +
            "关闭时只使用上 / 下 / 左 / 右 4 格。",
            CatShare)]
        public bool AllowDiagonal { get; set; } = false;

        [Option("液体共享速率 (kg/秒)",
            "相邻箱 / 砖之间均衡液体时，每个邻居每秒最多搬运的质量。\n" +
            "调小=更平缓。默认 5。",
            CatShare)]
        [Limit(0.1, 50.0)]
        public float LiquidTransferRate { get; set; } = 5.0f;

        [Option("固体共享速率 (kg/秒)",
            "相邻箱 / 砖之间均衡固体肥料时，每个邻居每秒最多搬运的质量。\n" +
            "调小=更平缓。默认 2。",
            CatShare)]
        [Limit(0.1, 50.0)]
        public float SolidTransferRate { get; set; } = 2.0f;

        // ───────────────────────── 3·小人运送（兜底） ─────────────────────────
        [Option("灌溉充足时取消小人运送",
            "当容器靠直接灌溉 / 相邻共享已存有足够液体（≥该作物的运送补充阈值 refillMass）时，\n" +
            "自动取消卡住的「需要供应 XX 克」小人运送单——\n" +
            "原版只在存储不足时新建运送、却不会因存储被环境 / 共享填满而取消，\n" +
            "导致容器明明已有上百 kg 盐水仍一直要小人搬、甚至因存储已满永远完不成。\n" +
            "液体不足时运送照常兜底；只作用于液体，固体肥料保留运送以免整排断料。\n" +
            "关闭则恢复原版运送行为。",
            CatDeliver)]
        public bool SuppressDeliveryWhenIrrigated { get; set; } = true;
    }
}
