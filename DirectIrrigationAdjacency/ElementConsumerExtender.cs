using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DirectIrrigationAdjacency
{
    // 环境取水（通用版，覆盖一切作物——含 U59 新植物 / 其他 mod 添加的植物）。
    //
    // 范式（照抄 DirectIrrigation(DI) 自身做法，零自创危险路径）：
    //  DI 在每个植物 *Config.CreatePrefab 的 postfix 里 AddComponent 一个液体
    //  PassiveElementConsumer（采样自身格），再在 IrrigationMonitor.Instance.SetStorage 的
    //  postfix(WireLiquidPECs) 里把植物上「所有」液体 PEC 接到灌溉 Storage 并挂
    //  LiquidPECStorageLimiter(ISim4000ms) 做带滞回的安全限流。
    //
    //  本扩展只做一件事：在 prefab 注册阶段(Assets.RegisterOnAddPrefab)，对「任意」带液体
    //  PEC 的植物 prefab，克隆出几个采样偏移到邻格(sampleCellOffset += 邻格 delta)的同款 PEC。
    //  不再硬编码植物列表——凡 DI 给了基础液体 PEC 的作物（含未来新增 / 其他 mod）都自动覆盖；
    //  接线与限流完全交给 DI 的 WireLiquidPECs（它遍历所有液体 PEC，自然覆盖我们的克隆）。
    //  注：环境消费者挂在「植物」上、不在种植砖/箱上，所以宽种植箱等只是容器，作物被覆盖即可。
    //
    // 为什么必须在 prefab 阶段加：PassiveElementConsumer 是 SimComponent，只有走正常 spawn
    //  生命周期(OnPrefabInit/OnSpawn)才会向 Sim 注册并被接线。运行时 AddComponent 不触发这些回调，
    //  既不工作也是之前那条危险/无效路径，已弃。Assets.RegisterOnAddPrefab 还会立即回放所有已注册
    //  prefab，因此无论植物 prefab 在本回调注册前后生成都能全覆盖。
    internal static class ElementConsumerExtender
    {
        private static readonly CellOffset[] DiagonalOffsets =
        {
            new CellOffset(-1, -1), new CellOffset(1, -1),
            new CellOffset(-1, 1),  new CellOffset(1, 1)
        };

        // 防止同一 prefab 被重复扩展（回放 + 后续注册可能重入）。
        private static readonly ConditionalWeakTable<GameObject, object> Extended =
            new ConditionalWeakTable<GameObject, object>();

        private static ModOptions Options => AdjacencySupply.Options;

        // 在 Mod.OnLoad 里调用一次。
        public static void Register()
        {
            Assets.RegisterOnAddPrefab(OnAddPrefab);
        }

        private static void OnAddPrefab(KPrefabID prefab)
        {
            if (prefab == null || prefab.gameObject == null) return;
            GameObject go = prefab.gameObject;
            try
            {
                // 种植容器(PlanterBox/FarmTile/HydroponicFarm 及任何带 PlantablePlot 的 mod 容器)：
                // 挂周期性均衡组件做同类型相邻共享。与「环境取水」开关无关，故独立添加。
                if (go.GetComponent<PlantablePlot>() != null && go.GetComponent<AdjacencyBalancer>() == null)
                    go.AddComponent<AdjacencyBalancer>();

                // 植物：环境取水偏移消费者。
                if (Options.EnableEnvironmentConsumers) Extend(go);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[DirectIrrigationAdjacency] ElementConsumerExtender skipped: " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        // 上/左/右各 1 格；向下按 EnvironmentDownDepth 连续取 (0,-1)..(0,-depth)，
        // 以便越过实心种植砖本体、够到其下方的开放液体格。
        private static List<CellOffset> BuildOffsets()
        {
            var offsets = new List<CellOffset>
            {
                new CellOffset(0, 1),
                new CellOffset(-1, 0),
                new CellOffset(1, 0),
            };
            int downDepth = Mathf.Clamp(Options.EnvironmentDownDepth, 1, 3);
            for (int d = 1; d <= downDepth; d++) offsets.Add(new CellOffset(0, -d));
            if (Options.EnvironmentDiagonal) offsets.AddRange(DiagonalOffsets);
            return offsets;
        }

        private static void Extend(GameObject plant)
        {
            if (plant == null) return;
            if (Extended.TryGetValue(plant, out _)) return;

            // 早退门槛：只处理「有 PEC」或「有灌溉需求(IrrigationMonitor.Def)」的 prefab，
            // 其余绝大多数建筑/生物/物品直接跳过。
            bool hasPec = plant.GetComponent<PassiveElementConsumer>() != null;
            IrrigationMonitor.Def irrDef = plant.GetComponent<StateMachineController>()?.GetDef<IrrigationMonitor.Def>();
            if (!hasPec && irrDef == null) return;

            // 兜底：DI 没给基础 PEC、但有液体灌溉需求的作物，照 DI 同款自造一个（采样自身格）。
            if (Options.EnvironmentSynthesizeMissing) EnsureBaseLiquidConsumers(plant, irrDef);

            // 快照：只克隆此刻已存在的「基础」液体 PEC（DI 加的 + 上面自造的），
            // 绝不克隆我们即将追加的偏移克隆。
            PassiveElementConsumer[] basePecs = plant.GetComponents<PassiveElementConsumer>();
            if (basePecs == null || basePecs.Length == 0) return;

            Extended.Add(plant, null);

            var offsets = BuildOffsets();
            float rateMul = Mathf.Max(Options.EnvironmentRateMultiplier, 0.01f);
            float capKg = Mathf.Max(Options.EnvironmentCapacityKG, 1f);

            foreach (PassiveElementConsumer basePec in basePecs)
            {
                if (basePec == null) continue;
                Element element = ElementLoader.FindElementByHash(basePec.elementToConsume);
                if (element == null || !element.IsLiquid) continue;

                Vector3 baseOffset = basePec.sampleCellOffset;
                foreach (CellOffset off in offsets)
                {
                    PassiveElementConsumer clone = plant.AddComponent<PassiveElementConsumer>();
                    clone.elementToConsume = basePec.elementToConsume;
                    clone.configuration = basePec.configuration;
                    clone.consumptionRate = basePec.consumptionRate * rateMul; // 速率可调
                    clone.consumptionRadius = basePec.consumptionRadius;
                    clone.minimumMass = basePec.minimumMass;
                    clone.capacityKG = capKg; // 蓄水上限可调（限流器按各消费者 cap 的最大值生效）
                    clone.storeOnConsume = true;
                    clone.showDescriptor = false;
                    clone.showInStatusPanel = false;
                    clone.isRequired = false;
                    clone.sampleCellOffset = baseOffset + new Vector3(off.x, off.y, 0f);
                    clone.enabled = false; // 交给 WireLiquidPECs 在 SetStorage 时启用并限流
                }
            }
        }

        // 对「需要液体灌溉但身上没有对应液体 PEC」的作物，照 DI 同款自造基础消费者（采样自身格）。
        // 之后正常被偏移克隆，并由 DI 全局的 SetStorage→WireLiquidPECs 自动接线+限流。
        private static void EnsureBaseLiquidConsumers(GameObject plant, IrrigationMonitor.Def irrDef)
        {
            if (irrDef?.consumedElements == null || irrDef.consumedElements.Length == 0) return;

            // 已被现有液体 PEC 覆盖的元素，避免与 DI 重复自造导致双份自身格消费者。
            var covered = new HashSet<SimHashes>();
            foreach (PassiveElementConsumer pec in plant.GetComponents<PassiveElementConsumer>())
            {
                if (pec == null) continue;
                Element el = ElementLoader.FindElementByHash(pec.elementToConsume);
                if (el != null && el.IsLiquid) covered.Add(pec.elementToConsume);
            }

            foreach (PlantElementAbsorber.ConsumeInfo ci in irrDef.consumedElements)
            {
                Element el = ElementLoader.GetElement(ci.tag);
                if (el == null || !el.IsLiquid) continue;
                if (covered.Contains(el.id)) continue;

                PassiveElementConsumer pec = plant.AddComponent<PassiveElementConsumer>();
                pec.elementToConsume = el.id;
                pec.configuration = ElementConsumer.Configuration.Element;
                pec.consumptionRate = 0.5f * Mathf.Max(Options.EnvironmentRateMultiplier, 0.01f); // DI 基准 0.5，速率可调
                pec.consumptionRadius = 1;
                pec.capacityKG = Mathf.Max(Options.EnvironmentCapacityKG, 1f);
                pec.storeOnConsume = true;
                pec.showDescriptor = false;
                pec.showInStatusPanel = false;
                pec.isRequired = false;
                pec.sampleCellOffset = Vector3.zero;
                pec.enabled = false;
                covered.Add(el.id);
            }
        }
    }
}
