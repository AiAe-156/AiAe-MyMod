using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;
using PowerExtension;

[SerializationConfig(MemberSerialization.OptIn)]
public class AirBattery : Electrobank
{
    // [千焦转换] 读取设置的 kJ，并乘以 1000 转为游戏底层的焦耳
    public float MaxCapacity => ModOptions.Instance.BatteryCapacity * 1000f;

    [Serialize]
    public float customCharge = 720000f;

    [Serialize]
    private float customHealth = 10f;

    [MyCmpGet]
    private Pickupable pickupable = null;

    private float waterCheckTimer = 0f;
    private float lastDamageTime = -1f;

    public bool IsFullyChargedInternal => customCharge >= MaxCapacity;

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        if (customCharge == 720000f && MaxCapacity != 720000f) customCharge = MaxCapacity;
    }

    public float RemovePowerInternal(float joules, bool dropWhenEmpty)
    {
        float num = Mathf.Min(customCharge, joules);
        customCharge -= num;
        if (customCharge <= 0f) ReplaceWithEmpty(base.gameObject, dropWhenEmpty);
        return num;
    }

    public float AddPowerInternal(float joules) => 0f;

    public override void Explode()
    {
        if (this.healthBar != null)
        {
            Util.KDestroyGameObject(this.healthBar.gameObject);
            this.healthBar = null;
        }

        int cell = Grid.PosToCell(base.gameObject.transform.position);
        float tempIncrease = 0f;
        float newTemp = Grid.Temperature[cell];

        if (Grid.IsValidCell(cell))
        {
            tempIncrease = customCharge / (Grid.Mass[cell] * Grid.Element[cell].specificHeatCapacity);
            tempIncrease = Mathf.Clamp(tempIncrease, 1f, 9999f);
            newTemp += tempIncrease;
            SimMessages.ReplaceElement(cell, Grid.Element[cell].id, CellEventLogger.Instance.SandBoxTool, Grid.Mass[cell], newTemp, Grid.DiseaseIdx[cell], Grid.DiseaseCount[cell]);
            // 损毁无掉落
        }

        Game.Instance.SpawnFX(SpawnFXHashes.MeteorImpactMetal, base.gameObject.transform.position, 0f);
        KFMOD.PlayOneShot(GlobalAssets.GetSound("Battery_explode"), base.gameObject.transform.position);
        Util.KDestroyGameObject(base.gameObject);
    }

    public override void Sim200ms(float dt)
    {
        if (this.healthBar != null && (pickupable.KPrefabID.HasTag(GameTags.Stored) || (Time.time - lastDamageTime > 3f)))
        {
            Util.KDestroyGameObject(this.healthBar.gameObject);
            this.healthBar = null;
        }

        waterCheckTimer += dt;
        if (waterCheckTimer >= 2f)
        {
            waterCheckTimer -= 2f;
            if (!pickupable.KPrefabID.HasTag(GameTags.Stored) &&
                Grid.IsValidCell(pickupable.cachedCell) &&
                Grid.Element[pickupable.cachedCell].HasTag(GameTags.AnyWater))
            {
                EvaluateWaterDamage(0.32f);
            }
        }
    }

    private void EvaluateWaterDamage(float damageAmount)
    {
        PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Negative, UI.GAMEOBJECTEFFECTS.DAMAGE_POPS.POWER_BANK_WATER_DAMAGE, base.transform);
        Damage(damageAmount);
    }

    public new void Damage(float amount)
    {
        Game.Instance.SpawnFX(SpawnFXHashes.ElectrobankDamage, Grid.PosToCell(base.gameObject), 0f);
        KFMOD.PlayOneShot(GlobalAssets.GetSound("Battery_sparks_short"), base.gameObject.transform.position, 1f);

        customHealth -= amount;
        lastDamageTime = Time.time;

        // [核心修复] 强制删除旧血条，在最新物理坐标重建新血条
        if (this.healthBar != null)
        {
            Util.KDestroyGameObject(this.healthBar.gameObject);
            this.healthBar = null;
        }

        CreateHealthBar();
        this.healthBar.Update();

        if (customHealth <= 0f) Explode();
    }

    public new void CreateHealthBar()
    {
        this.healthBar = ProgressBar.CreateProgressBar(base.gameObject, () => this.customHealth / 10f);
        this.healthBar.SetVisibility(true);
        this.healthBar.barColor = Util.ColorFromHex("CC3333");
    }

    public static GameObject ReplaceWithEmpty(GameObject currentBattery, bool dropFromStorage = false)
    {
        Vector3 position = currentBattery.transform.GetPosition();
        var batteryComp = currentBattery.GetComponent<AirBattery>();
        if (batteryComp != null && batteryComp.healthBar != null)
        {
            Util.KDestroyGameObject(batteryComp.healthBar.gameObject);
        }

        GameObject gameObject = Util.KInstantiate(Assets.GetPrefab("EmptyAirBattery"), position);
        gameObject.GetComponent<PrimaryElement>().SetElement(currentBattery.GetComponent<PrimaryElement>().Element.id);
        gameObject.SetActive(value: true);

        Storage storage = currentBattery.GetComponent<Pickupable>().storage;
        currentBattery.DeleteObject();

        if (storage != null && !dropFromStorage) storage.Store(gameObject);
        return gameObject;
    }

    protected override void OnCleanUp()
    {
        if (this.healthBar != null)
        {
            Util.KDestroyGameObject(this.healthBar.gameObject);
            this.healthBar = null;
        }
        base.OnCleanUp();
    }
}