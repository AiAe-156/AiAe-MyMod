﻿using KSerialization;
using System.Diagnostics;
using UnityEngine;

namespace PowerExtension
{
    [SerializationConfig(MemberSerialization.OptIn)]
    [DebuggerDisplay("{name}")]
    public class IonBatterySmart : Battery, IActivationRangeTarget
    {
        public static readonly HashedString PORT_ID = "IonBatteryLogicPort";

        [Serialize]
        private int activateValue;

        [Serialize]
        private int deactivateValue = 100;

        [Serialize]
        private bool activated;

        [MyCmpGet]
        private LogicPorts logicPorts;

        // 是否在建造完成时直接充满电。由各建筑 Config 设置：
        // 先进离子电池列 = true(即建即用)；重型智能电池 = false(需正常充能)。
        public bool refillOnNewBuilding = false;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(-905833192, OnCopySettings);
            Subscribe((int)GameHashes.NewBuilding, OnNewBuilding);
        }

        private void OnCopySettings(object data)
        {
            IonBatterySmart component = ((GameObject)data).GetComponent<IonBatterySmart>();
            if (component != null)
            {
                ActivateValue = component.ActivateValue;
                DeactivateValue = component.DeactivateValue;
            }
        }

        private void OnNewBuilding(object data)
        {
            if (refillOnNewBuilding)
            {
                DEBUG_RefillPower();
            }
            UpdateLogicCircuit(null);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // 🚨 核心修改点：移除了原版 CreateLogicMeter()，防止寻找不到贴图而崩溃

            // 订阅自动化网络更新和逻辑值改变
            Subscribe(-801688580, OnLogicValueChanged);
            Subscribe(-592767678, UpdateLogicCircuit);
        }

        public override void EnergySim200ms(float dt)
        {
            base.EnergySim200ms(dt);
            UpdateLogicCircuit(null);
        }

        private void UpdateLogicCircuit(object _)
        {
            float num = Mathf.RoundToInt(PercentFull * 100f);
            if (activated)
            {
                if (num >= deactivateValue)
                {
                    activated = false;
                }
            }
            else if (num <= activateValue)
            {
                activated = true;
            }

            bool isOperational = operational.IsOperational;
            bool flag = activated && isOperational;

            if (logicPorts != null)
            {
                logicPorts.SendSignal(PORT_ID, flag ? 1 : 0);
            }
        }

        private void OnLogicValueChanged(object data)
        {
            // 由于没有逻辑指示灯贴图，这里不需要像原版那样去 SetLogicMeter
            // 保留此方法结构以防后续需要处理其他逻辑回调
        }

        // ================= IActivationRangeTarget 接口实现 (用于侧边栏 UI 滑块) =================

        public float ActivateValue
        {
            get => deactivateValue;
            set
            {
                deactivateValue = (int)value;
                UpdateLogicCircuit(null);
            }
        }

        public float DeactivateValue
        {
            get => activateValue;
            set
            {
                activateValue = (int)value;
                UpdateLogicCircuit(null);
            }
        }

        public float MinValue => 0f;
        public float MaxValue => 100f;
        public bool UseWholeNumbers => true;

        public string ActivateTooltip => STRINGS.BUILDINGS.PREFABS.BATTERYSMART.DEACTIVATE_TOOLTIP;
        public string DeactivateTooltip => STRINGS.BUILDINGS.PREFABS.BATTERYSMART.ACTIVATE_TOOLTIP;
        public string ActivationRangeTitleText => STRINGS.BUILDINGS.PREFABS.BATTERYSMART.SIDESCREEN_TITLE;
        public string ActivateSliderLabelText => STRINGS.BUILDINGS.PREFABS.BATTERYSMART.SIDESCREEN_DEACTIVATE;
        public string DeactivateSliderLabelText => STRINGS.BUILDINGS.PREFABS.BATTERYSMART.SIDESCREEN_ACTIVATE;
    }
}
