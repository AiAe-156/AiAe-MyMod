using System;
using System.Collections.Generic;
using UnityEngine;

namespace DirectIrrigationAdjacency
{
    // 相邻种植容器之间「按元素需求 + 纯均衡」共享灌溉液体 / 固体肥料。
    //
    // 共享门槛从「容器同类型」改为「逐元素的需求门槛」：对中心作物消耗的每个元素 X，
    // 只在「邻居容器里已种植的作物也消耗 X」时才共享 X。于是不同种类的种植砖/箱也能共享——
    // 但只共享双方都需要的那个元素（盐水大家共享、盐只在需盐的作物间、泥土只在需泥土的作物间）。
    //
    // 空容器导管桥（安全版，不当黑洞）：空种植容器本身不消耗元素，但当它「夹在两个以上、对同一
    // 元素存在真实落差的已种植消费者中间」时，把该元素从更富的一侧拉进自己（缺口侧消费者会在自己
    // 的 tick 从桥里抽走），从而打通隔着一个空格的两株作物。少于两个该元素消费者、或它们已均衡时，
    // 桥一滴都不抽 → 不会把单株作物抽干、也不会无谓稀释。
    //
    // 触发：由 AdjacencyBalancer(ISim1000ms) 周期调用 BalancePlot（含空容器）——不挂在事件驱动的
    // IrrigationMonitor.UpdateIrrigation 上（饿砖无存储变化 → 不触发，已坐实的 bug）。
    //
    // 范式：只读取/搬运，绝不用 AddLiquid + 手改质量；液体固体统一走 Storage.Transfer。
    internal static class AdjacencySupply
    {
        private const float MinTransferKg = 0.001f;
        // 均衡：来源比目标多出这个量以上才搬，且每次最多搬「差额的一半」（再夹上可调每秒上限），
        // 使两箱向中间收敛而非来回抖动；也保证绝不把来源搬到比目标更低（不偷料）。
        private const float BalanceMargin = 0.05f;

        public static ModOptions Options { get; set; } = new ModOptions();

        private static readonly CellOffset[] CardinalOffsets =
        {
            new CellOffset(0, -1),
            new CellOffset(-1, 0), new CellOffset(1, 0),
            new CellOffset(0, 1)
        };

        private static readonly CellOffset[] DiagonalOffsets =
        {
            new CellOffset(-1, -1), new CellOffset(1, -1),
            new CellOffset(-1, 1),  new CellOffset(1, 1)
        };

        private struct PlotInfo
        {
            public Storage storage;
            public GameObject occupant; // null = 空容器
        }

        // AdjacencyBalancer 每秒对每个种植容器（含空容器）调一次。occupant 可能为 null。
        public static void BalancePlot(GameObject building, Storage storage, GameObject occupant)
        {
            try { BalancePlotInner(building, storage, occupant); }
            catch (Exception ex) { Debug.LogWarning("[DirectIrrigationAdjacency] Balance skipped: " + ex.GetType().Name + ": " + ex.Message); }
        }

        // 直接/共享灌溉让存储已有足够液体时，取消作物对应的「小人运送」需求(ManualDeliveryKG)。
        // 原版 ManualDeliveryKG 只在 MassStored < refillMass 时「新建」运送单，却不会在存储被其他来源
        // （环境直取 / 邻居共享）填满后「取消」已下发的运送单——于是容器明明已有上百 kg 盐水，仍卡着
        // 「需要供应 XX 克」、小人继续搬运、甚至因存储已满而永远完不成。这里在液体足够(≥refillMass)时
        // 主动 AbortDelivery 把卡住的运送单清掉；液体不足时不动，运送照常作为兜底。
        // 只处理液体（环境可再生）；固体肥料无环境来源，保留小人运送以免整排断料。
        public static void SuppressManualDelivery(GameObject occupant, Storage storage)
        {
            try
            {
                if (!Options.SuppressDeliveryWhenIrrigated) return;
                if (occupant == null || storage == null) return;
                ManualDeliveryKG[] mdks = occupant.GetComponents<ManualDeliveryKG>();
                if (mdks == null) return;
                foreach (ManualDeliveryKG mdk in mdks)
                {
                    if (mdk == null) continue;
                    Tag tag = mdk.RequestedItemTag;
                    if (!tag.IsValid) continue;
                    Element el = ElementLoader.GetElement(tag);
                    if (el == null || !el.IsLiquid) continue;
                    float stored = storage.GetMassAvailable(tag);
                    if (stored >= Mathf.Max(mdk.refillMass, MinTransferKg))
                        mdk.AbortDelivery("DirectIrrigationAdjacency: irrigated directly/shared");
                }
            }
            catch (Exception ex) { Debug.LogWarning("[DirectIrrigationAdjacency] SuppressManualDelivery skipped: " + ex.GetType().Name + ": " + ex.Message); }
        }

        private static void BalancePlotInner(GameObject building, Storage storage, GameObject occupant)
        {
            if (!Options.EnableNeighborSharing) return;
            if (building == null || storage == null) return;
            // 管道供液容器（液培砖等）不参与相邻共享 / 导管桥。
            if (IsExcludedPlot(building)) return;

            int centerCell = Grid.PosToCell(building);
            if (!Grid.IsValidCell(centerCell)) return;

            // 邻居与本容器跨种类共享，可能不同层；优先取本建筑自身的 ObjectLayer，再退回
            // Building / FoundationTile 兜底。原版 PlanterBox/FarmTile/HydroponicFarm 都在 Building 层。
            ObjectLayer centerLayer = building.GetComponent<Building>()?.Def?.ObjectLayer ?? ObjectLayer.Building;

            if (occupant != null)
                BalancePlanted(storage, centerCell, centerLayer, occupant);
            else if (Options.EnableEmptyBridge)
                BalanceBridge(storage, centerCell, centerLayer);
        }

        // 已种植容器：对作物消耗的每个元素，从「也消耗该元素的已种植邻居」或「空导管容器」拉补。
        private static void BalancePlanted(Storage storage, int centerCell, ObjectLayer centerLayer, GameObject occupant)
        {
            var smc = occupant.GetComponent<StateMachineController>();
            if (smc == null) return;

            IrrigationMonitor.Def irrDef = smc.GetDef<IrrigationMonitor.Def>();
            if (irrDef?.consumedElements != null)
                foreach (PlantElementAbsorber.ConsumeInfo ci in irrDef.consumedElements)
                    PullForConsumer(storage, centerCell, centerLayer, ci.tag, wantLiquid: true);

            FertilizationMonitor.Def fertDef = smc.GetDef<FertilizationMonitor.Def>();
            if (fertDef?.consumedElements != null)
                foreach (PlantElementAbsorber.ConsumeInfo ci in fertDef.consumedElements)
                    PullForConsumer(storage, centerCell, centerLayer, ci.tag, wantLiquid: false);
        }

        // 中心(已种植)从更富的合格邻居均衡拉取元素 tag：每次最多搬 min(差额一半, maxPerTick)。
        private static void PullForConsumer(Storage targetStorage, int centerCell, ObjectLayer centerLayer, Tag tag, bool wantLiquid)
        {
            Element element = ElementLoader.GetElement(tag);
            if (element == null) return;
            if (wantLiquid ? !element.IsLiquid : !element.IsSolid) return;

            float cap = Mathf.Max(wantLiquid ? Options.LiquidTransferRate : Options.SolidTransferRate, MinTransferKg);
            float targetHave = GetStoredMass(targetStorage, tag);
            float moved = 0f;

            foreach (CellOffset offset in EnumerateOffsets())
            {
                PlotInfo n = GetNeighborPlot(centerCell, offset, centerLayer);
                if (n.storage == null || n.storage == targetStorage) continue;
                // 同元素门槛：邻居要么是「也消耗该元素的已种植作物」（对等共享），要么是「空容器」
                // （导管缓冲，允许消费者把桥里中转的该元素抽走，不会变黑洞）。
                if (n.occupant != null && !PlantConsumes(n.occupant, tag, wantLiquid)) continue;

                float sourceHave = GetStoredMass(n.storage, tag);
                float gap = sourceHave - (targetHave + moved);
                if (gap <= BalanceMargin) continue;

                float donate = Mathf.Clamp(gap * 0.5f, MinTransferKg, cap);
                moved += n.storage.Transfer(targetStorage, tag, donate, block_events: false, hide_popups: true);
            }
        }

        // 空容器导管桥：收集周围已种植邻居消耗的元素，对每个元素判断是否需要中转。
        private static void BalanceBridge(Storage bridgeStorage, int centerCell, ObjectLayer centerLayer)
        {
            var neighbors = new List<PlotInfo>();
            var liquidTags = new HashSet<Tag>();
            var solidTags = new HashSet<Tag>();

            foreach (CellOffset offset in EnumerateOffsets())
            {
                PlotInfo n = GetNeighborPlot(centerCell, offset, centerLayer);
                if (n.storage == null || n.storage == bridgeStorage) continue;
                neighbors.Add(n);
                if (n.occupant != null) CollectConsumedTags(n.occupant, liquidTags, solidTags);
            }
            if (neighbors.Count == 0) return;

            foreach (Tag tag in liquidTags) BridgeElement(bridgeStorage, neighbors, tag, wantLiquid: true);
            foreach (Tag tag in solidTags) BridgeElement(bridgeStorage, neighbors, tag, wantLiquid: false);
        }

        // 桥对单个元素中转：仅当周围「消耗该元素的已种植邻居 ≥ 2 且存在真实落差」才把该元素从更富的
        // 消费者邻居拉进桥（缺口侧消费者随后在自己的 tick 从桥里抽走）。否则不动 → 不当黑洞。
        private static void BridgeElement(Storage bridgeStorage, List<PlotInfo> neighbors, Tag tag, bool wantLiquid)
        {
            Element element = ElementLoader.GetElement(tag);
            if (element == null) return;
            if (wantLiquid ? !element.IsLiquid : !element.IsSolid) return;

            int consumerCount = 0;
            float minHave = float.MaxValue, maxHave = float.MinValue;
            foreach (PlotInfo n in neighbors)
            {
                if (n.occupant == null || !PlantConsumes(n.occupant, tag, wantLiquid)) continue;
                consumerCount++;
                float have = GetStoredMass(n.storage, tag);
                if (have < minHave) minHave = have;
                if (have > maxHave) maxHave = have;
            }
            // 黑洞防护：少于两个该元素消费者、或它们已均衡 → 不中转。
            if (consumerCount < 2 || maxHave - minHave <= BalanceMargin) return;

            float cap = Mathf.Max(wantLiquid ? Options.LiquidTransferRate : Options.SolidTransferRate, MinTransferKg);
            float bridgeHave = GetStoredMass(bridgeStorage, tag);
            float moved = 0f;
            foreach (PlotInfo n in neighbors)
            {
                if (n.occupant == null || !PlantConsumes(n.occupant, tag, wantLiquid)) continue;
                float sourceHave = GetStoredMass(n.storage, tag);
                float gap = sourceHave - (bridgeHave + moved);
                if (gap <= BalanceMargin) continue;

                float donate = Mathf.Clamp(gap * 0.5f, MinTransferKg, cap);
                moved += n.storage.Transfer(bridgeStorage, tag, donate, block_events: false, hide_popups: true);
            }
        }

        // 判断某株作物是否消耗指定元素（液体查 IrrigationMonitor，固体查 FertilizationMonitor）。
        private static bool PlantConsumes(GameObject occupant, Tag tag, bool wantLiquid)
        {
            var smc = occupant.GetComponent<StateMachineController>();
            if (smc == null) return false;
            PlantElementAbsorber.ConsumeInfo[] consumed = wantLiquid
                ? smc.GetDef<IrrigationMonitor.Def>()?.consumedElements
                : smc.GetDef<FertilizationMonitor.Def>()?.consumedElements;
            if (consumed == null) return false;
            foreach (PlantElementAbsorber.ConsumeInfo ci in consumed)
                if (ci.tag == tag) return true;
            return false;
        }

        // 收集某株作物消耗的液体 / 固体元素（供导管桥确定候选中转元素）。
        private static void CollectConsumedTags(GameObject occupant, HashSet<Tag> liquidTags, HashSet<Tag> solidTags)
        {
            var smc = occupant.GetComponent<StateMachineController>();
            if (smc == null) return;

            IrrigationMonitor.Def irrDef = smc.GetDef<IrrigationMonitor.Def>();
            if (irrDef?.consumedElements != null)
                foreach (PlantElementAbsorber.ConsumeInfo ci in irrDef.consumedElements)
                {
                    Element el = ElementLoader.GetElement(ci.tag);
                    if (el != null && el.IsLiquid) liquidTags.Add(ci.tag);
                }

            FertilizationMonitor.Def fertDef = smc.GetDef<FertilizationMonitor.Def>();
            if (fertDef?.consumedElements != null)
                foreach (PlantElementAbsorber.ConsumeInfo ci in fertDef.consumedElements)
                {
                    Element el = ElementLoader.GetElement(ci.tag);
                    if (el != null && el.IsSolid) solidTags.Add(ci.tag);
                }
        }

        private static PlotInfo GetNeighborPlot(int centerCell, CellOffset offset, ObjectLayer centerLayer)
        {
            PlotInfo info = default;
            int cell = Grid.OffsetCell(centerCell, offset);
            if (!Grid.IsValidCell(cell)) return info;

            GameObject building = GetPlotBuilding(cell, centerLayer);
            if (building == null) return info;
            if (building.GetComponent<PlantablePlot>() == null) return info;
            // 管道供液容器（液培砖等）既不当共享源也不当目标、不参与桥。
            if (IsExcludedPlot(building)) return info;

            info.storage = building.GetComponent<Storage>();
            if (info.storage != null)
                info.occupant = building.GetComponent<PlantablePlot>().Occupant;
            return info;
        }

        // 管道供液容器（液培砖 HydroponicFarm / 宽种植砖 WideFarmTile 等，has_liquid_pipe_input=true）
        // 由管道自管液体，按选项排除出相邻共享 / 导管桥。
        private static bool IsExcludedPlot(GameObject building)
        {
            if (!Options.ExcludeHydroponic) return false;
            PlantablePlot plot = building.GetComponent<PlantablePlot>();
            return plot != null && plot.has_liquid_pipe_input;
        }

        // 优先用「本容器自身的 ObjectLayer」找邻居；再退回 Building / FoundationTile 兜底。
        private static GameObject GetPlotBuilding(int cell, ObjectLayer preferredLayer)
        {
            if (!Grid.IsValidCell(cell)) return null;
            GameObject b = Grid.Objects[cell, (int)preferredLayer];
            if (b == null && preferredLayer != ObjectLayer.Building) b = Grid.Objects[cell, (int)ObjectLayer.Building];
            if (b == null) b = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
            return b;
        }

        private static IEnumerable<CellOffset> EnumerateOffsets()
        {
            foreach (CellOffset offset in CardinalOffsets) yield return offset;
            if (!Options.AllowDiagonal) yield break;
            foreach (CellOffset offset in DiagonalOffsets) yield return offset;
        }

        private static float GetStoredMass(Storage storage, Tag tag)
        {
            float mass = storage.GetAmountAvailable(tag);
            if (!float.IsNaN(mass) && mass >= 0f) return mass;

            mass = 0f;
            foreach (GameObject item in storage.items)
            {
                if (item == null || !item.HasTag(tag)) continue;
                var primaryElement = item.GetComponent<PrimaryElement>();
                if (primaryElement != null) mass += primaryElement.Mass;
            }
            return mass;
        }
    }
}
