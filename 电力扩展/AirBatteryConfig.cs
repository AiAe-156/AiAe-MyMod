using System.Collections.Generic;
using TUNING;
using UnityEngine;

public class AirBatteryConfig : IEntityConfig
{
    public const string ID = "AirBattery";

    public GameObject CreatePrefab()
    {
        GameObject gameObject = EntityTemplates.CreateLooseEntity(
            id: ID,
            // [修复] 路径更新为 .EXT_ITEMS
            name: PowerExtension.Strings.EXT_ITEMS.INDUSTRIAL_PRODUCTS.AIRBATTERY.NAME.text,
            desc: PowerExtension.Strings.EXT_ITEMS.INDUSTRIAL_PRODUCTS.AIRBATTERY.DESC.text,
            mass: 100f,
            unitMass: true,
            anim: Assets.GetAnim("electrobank_huge_kanim"),
            initialAnim: "idle1",
            sceneLayer: Grid.SceneLayer.Ore,
            collisionShape: EntityTemplates.CollisionShape.RECTANGLE,
            width: 0.8f,
            height: 1.0f,
            isPickupable: true,
            sortOrder: 0,
            element: SimHashes.Aluminum,
            additionalTags: new List<Tag>
            {
                GameTags.ChargedPortableBattery,
                GameTags.PedestalDisplayable,
                GameTags.IndustrialProduct
            }
        );

        gameObject.GetComponent<KCollider2D>();
        gameObject.AddComponent<AirBattery>();
        gameObject.AddOrGet<OccupyArea>().SetCellOffsets(EntityTemplates.GenerateOffsets(1, 1));
        gameObject.AddOrGet<DecorProvider>().SetValues(DECOR.PENALTY.TIER1);

        return gameObject;
    }

    public string[] GetDlcIds() => DlcManager.DLC3;
    public void OnPrefabInit(GameObject inst) { }
    public void OnSpawn(GameObject inst) { }
}