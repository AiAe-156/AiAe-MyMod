using System;
using UnityEngine;

namespace FUtility;

public abstract class EntityConfigBase : IEntityConfig
{
	public abstract GameObject CreatePrefab();

	public virtual void OnPrefabInit(GameObject inst)
	{
	}

	public virtual void OnSpawn(GameObject inst)
	{
	}

	[Obsolete]
	public virtual string[] GetDlcIds()
	{
		return null;
	}

	protected GameObject CreateBasicTemporary(string ID, string animFile = "barbeque_kanim")
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = EntityTemplates.CreateEntity(ID, ID, false);
		val.AddComponent<StateMachineController>();
		KBatchedAnimController val2 = EntityTemplateExtensions.AddOrGet<KBatchedAnimController>(val);
		((KAnimControllerBase)val2).AnimFiles = (KAnimFile[])(object)new KAnimFile[1] { Assets.GetAnim(HashedString.op_Implicit("barbeque_kanim")) };
		((KAnimControllerBase)val2).initialAnim = "none";
		((KAnimControllerBase)val2).initialMode = (PlayMode)2;
		val2.SetVisiblity(false);
		return val;
	}

	protected GameObject CreateBasic(string ID, string animFile = "barbeque_kanim")
	{
		GameObject obj = CreateBasicTemporary(ID, animFile);
		EntityTemplateExtensions.AddOrGet<SaveLoadRoot>(obj);
		return obj;
	}
}
