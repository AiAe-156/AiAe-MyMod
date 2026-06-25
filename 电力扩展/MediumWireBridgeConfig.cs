﻿using UnityEngine;

namespace PowerExtension
{
    public class MediumWireBridgeConfig : WireBridgeConfig
    {
        public new const string ID = "MediumWireBridge";

        protected override string GetID() => ID;

        // [2026-06-25] 不再硬性要求任何 DLC：材料类别 RUBBER_OR_PLASTIC("Rubber&Plastic")
        // 会自动按可用元素降级——未开水生包时仅塑料(聚丙烯)可建，开了水生包则橡胶也作为候选。
        // 与原版 WireRubberConfig 一致(其同样不覆写 GetRequiredDlcIds，默认返回 null=无门槛)。

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = base.CreateBuildingDef();

            def.AnimFiles = new KAnimFile[1] { Assets.GetAnim("MediumWireBridge_kanim") };
            // 配方 = 原版绝缘导线桥的 2 倍：50kg 精炼金属 + 20kg 橡胶/塑料。
            def.Mass = new float[2]
            {
                50f,
                20f
            };
            def.MaterialCategory = new string[2]
            {
                "RefinedMetal",
                // “Rubber&Plastic” = 原版 4kW 橡胶导线同款合并类别，橡胶或塑料均可建造。
                TUNING.MATERIALS.RUBBER_OR_PLASTIC
            };
            def.BaseDecor = -3f;

            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.WireIDs, ID);
            return def;
        }

        protected override WireUtilityNetworkLink AddNetworkLink(GameObject go)
        {
            WireUtilityNetworkLink link = base.AddNetworkLink(go);
            // [2026-05-28] U59 官方 4kW Max4000=5，本 mod 错位取 Max8kW=6（8kW）
            link.maxWattageRating = (Wire.WattageRating)PowerExtensionPatches.ExtendedWattageRating.Max8kW;
            return link;
        }
    }
}