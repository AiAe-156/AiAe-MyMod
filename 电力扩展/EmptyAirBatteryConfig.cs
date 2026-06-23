using System.Collections.Generic;
using TUNING;
using UnityEngine;

public class EmptyAirBatteryConfig : IEntityConfig
{
    public const string ID = "EmptyAirBattery";

    public GameObject CreatePrefab()
    {
        GameObject gameObject = EntityTemplates.CreateLooseEntity(
            id: ID,
            // [修复] 这里的引用路径增加了 .EXT_ITEMS
            name: PowerExtension.Strings.EXT_ITEMS.INDUSTRIAL_PRODUCTS.EMPTYAIRBATTERY.NAME.text,
            desc: PowerExtension.Strings.EXT_ITEMS.INDUSTRIAL_PRODUCTS.EMPTYAIRBATTERY.DESC.text,
            mass: 50f,
            unitMass: true,
            anim: Assets.GetAnim("electrobank_huge_kanim"), // 临时回退到 electrobank_huge_kanim 避免由于 _destroyed 贴图缺失导致的加载失败崩溃
            initialAnim: "idle1",
            sceneLayer: Grid.SceneLayer.Ore,
            collisionShape: EntityTemplates.CollisionShape.RECTANGLE,
            width: 0.8f,
            height: 0.8f,
            isPickupable: true,
            sortOrder: 0,
            element: SimHashes.Aluminum,
            additionalTags: new List<Tag>
            {
                GameTags.IndustrialProduct,
                GameTags.PedestalDisplayable
            }
        );

        gameObject.GetComponent<KCollider2D>();
        gameObject.AddOrGet<OccupyArea>().SetCellOffsets(EntityTemplates.GenerateOffsets(1, 1));
        gameObject.AddOrGet<DecorProvider>().SetValues(DECOR.PENALTY.TIER1);

        return gameObject;
    }

    public string[] GetDlcIds() => DlcManager.DLC3;
    public void OnPrefabInit(GameObject inst) { }
    public void OnSpawn(GameObject inst) { }
}