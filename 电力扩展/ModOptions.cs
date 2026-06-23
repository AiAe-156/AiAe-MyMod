using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace PowerExtension
{
    [JsonObject(MemberSerialization.OptIn)]
    [ModInfo("电力扩展")]
    public class ModOptions
    {
        private static ModOptions _instance;
        public static ModOptions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = POptions.ReadSettings<ModOptions>() ?? new ModOptions();
                return _instance;
            }
        }
        // [修改] 单位改为千焦，默认值改为 160
        [Option("离子电池列容量 (千焦)", "设置高科技离子电池列的最大电量。")]
        [JsonProperty]
        public float IonBatteryCapacity { get; set; } = 1200f;
        [Option("空气燃料电源容量 (千焦)", "设置空气燃料电源的最大电量。")]
        [JsonProperty]
        public float BatteryCapacity { get; set; } = 1200f; // 默认
        [Option("堆叠蓄电池容量 (千焦)", "设置堆叠蓄电池单层的最大电量。漏电按容量1.5%/周期、发热按容量2%千复制热/秒联动。")]
        [JsonProperty]
        public float StackBatteryCapacity { get; set; } = 100f;
    }
}
