using UnityEngine;

namespace DirectIrrigationAdjacency
{
    // 周期性(每秒)在相邻种植容器之间「按元素需求」均衡灌溉液体 / 固体肥料，并让空容器在两个以上
    // 同元素消费者之间充当「安全导管桥」中转资源。
    //
    // 为什么不挂在 IrrigationMonitor.UpdateIrrigation 上：那个方法是「事件驱动」的，
    // 只在「种下那一刻」和「存储变化(OnStorageChange)」时才被调用。一个饿着、存储一直为空、
    // 又没有运送进来的砖根本不产生存储变化事件 → UpdateIrrigation 永不触发 → 它永远拉不到
    // 隔壁的盐水（已坐实的 bug）。用 ISim1000ms 周期触发即可根治：不管植物饿不饿都每秒跑一次。
    //
    // 范式参照 DirectIrrigation 自己的 LiquidPECStorageLimiter(ISim4000ms)。挂在种植容器
    // (PlantablePlot)建筑上，prefab 阶段由 ElementConsumerExtender 添加，走正常 spawn 生命周期。
    public sealed class AdjacencyBalancer : KMonoBehaviour, ISim1000ms
    {
        private Storage storage;
        private PlantablePlot plot;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            storage = GetComponent<Storage>();
            plot = GetComponent<PlantablePlot>();
        }

        public void Sim1000ms(float dt)
        {
            if (storage == null || plot == null) return;
            GameObject occupant = plot.Occupant;
            // 已种植：直接/共享灌溉已存有足够液体时，取消卡住的小人液体运送需求（独立于共享开关）。
            if (occupant != null) AdjacencySupply.SuppressManualDelivery(occupant, storage);
            // occupant 可能为 null：已种植容器走对等共享，空容器走安全导管桥（见 AdjacencySupply）。
            AdjacencySupply.BalancePlot(base.gameObject, storage, occupant);
        }
    }
}
