using HarmonyLib;
using KSerialization;
using UnityEngine;

namespace PowerExtension
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class IonBatteryTransformer : Generator, ISingleSliderControl
    {
        [MyCmpReq]
        private Battery battery;

        [Serialize]
        private float outputWattageLimit = 20000f;

        private float lastJoulesAvailable = 0f;
        private static StatusItem outputLimitStatusItem;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            if (outputLimitStatusItem == null)
            {
                // [修复] 使用 .text 而不是 .key
                // 确保这里传入的是 string 类型
                outputLimitStatusItem = new StatusItem(
                    "IonBatteryOutputLimit",
                    PowerExtension.Strings.EXT_BUILDINGS.STATUSITEMS.IONBATTERY_OUTPUTLIMIT.NAME.text,
                    PowerExtension.Strings.EXT_BUILDINGS.STATUSITEMS.IONBATTERY_OUTPUTLIMIT.TOOLTIP.text,
                    "",
                    StatusItem.IconType.Info,
                    NotificationType.Neutral,
                    false,
                    OverlayModes.None.ID
                );

                outputLimitStatusItem.resolveStringCallback = (str, data) =>
                {
                    IonBatteryTransformer component = (IonBatteryTransformer)data;
                    return str.Replace("{0}", component.outputWattageLimit.ToString("F0"));
                };
            }

            GetComponent<KSelectable>().AddStatusItem(outputLimitStatusItem, this);
            UpdateGeneratorWattage(outputWattageLimit);
        }

        public override void EnergySim200ms(float dt)
        {
            base.EnergySim200ms(dt);
            float consumedLastTick = lastJoulesAvailable - this.JoulesAvailable;
            if (consumedLastTick > 0f && battery != null) battery.ConsumeEnergy(consumedLastTick);
            float maxJoulesCanGenerate = outputWattageLimit * dt;
            float joulesToMakeAvailable = Mathf.Min(battery != null ? battery.JoulesAvailable : 0f, maxJoulesCanGenerate);
            base.AssignJoulesAvailable(joulesToMakeAvailable);
            lastJoulesAvailable = joulesToMakeAvailable;
        }

        public int SliderDecimalPlaces(int index) => 0;
        public float GetSliderMin(int index) => 0f;
        public float GetSliderMax(int index) => 20000f;
        public float GetSliderValue(int index) => outputWattageLimit;
        public void SetSliderValue(float value, int index) { outputWattageLimit = value; UpdateGeneratorWattage(value); }

        private void UpdateGeneratorWattage(float newVal)
        {
            Traverse.Create(this).Field("capacity").SetValue(newVal);
        }

        public string GetSliderTooltipKey(int index) => "";
        // [修复] 使用 .text 获取正确的文本
        public string GetSliderTooltip(int index) => string.Format(PowerExtension.Strings.EXT_UI.FRONTEND.POWEREXTENSION.IONBATTERY.SLIDER_TOOLTIP.text, outputWattageLimit);
        public string SliderTitleKey => PowerExtension.Strings.EXT_UI.FRONTEND.POWEREXTENSION.IONBATTERY.SLIDER_TITLE.text;
        public string SliderUnits => "  W";
    }
}