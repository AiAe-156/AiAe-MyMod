using System;
using FMODUnity;
using Klei;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Substance
{
	public enum SubstanceTexture : byte
	{
		None,
		Magma,
		MoltenMetal,
		Polluted,
		Oil,
		Thick,
		Sap,
		CrystalFragments,
		Ovolene,
		Mucus,
		Ink,
		PollutedBrine
	}

	public string name;

	public SimHashes elementID;

	internal Tag nameTag;

	public Color32 colour;

	[SerializeField]
	private bool usesCaustics = false;

	[SerializeField]
	private bool glows = false;

	[SerializeField]
	private bool metalic = false;

	[SerializeField]
	private bool isOpaqueLiquid = false;

	[SerializeField]
	private Gradient gradient = null;

	[SerializeField]
	private SubstanceTexture texture;

	[FormerlySerializedAs("debugColour")]
	public Color32 uiColour;

	[FormerlySerializedAs("overlayColour")]
	public Color32 conduitColour = Color.white;

	[NonSerialized]
	internal bool renderedByWorld;

	[NonSerialized]
	internal int idx;

	public Material material;

	public KAnimFile anim;

	[SerializeField]
	internal bool showInEditor = true;

	[NonSerialized]
	internal KAnimFile[] anims;

	[NonSerialized]
	internal ElementsAudio.ElementAudioConfig audioConfig;

	[NonSerialized]
	internal MaterialPropertyBlock propertyBlock;

	public EventReference fallingStartSound;

	public EventReference fallingStopSound;

	public SubstanceTexture Texture => texture;

	public bool LiquidCaustics => usesCaustics;

	public bool Glows => glows;

	public bool Metalic => metalic;

	public bool IsOpaqueLiquid => isOpaqueLiquid;

	public Gradient Gradient
	{
		get
		{
			if (gradient == null)
			{
				gradient = new Gradient
				{
					colorKeys = new GradientColorKey[1]
					{
						new GradientColorKey
						{
							color = colour,
							time = 0f
						}
					}
				};
			}
			return gradient;
		}
	}

	public GameObject SpawnResource(Vector3 position, float mass, float temperature, byte disease_idx, int disease_count, bool prevent_merge = false, bool forceTemperature = false, bool manual_activation = false)
	{
		GameObject gameObject = null;
		PrimaryElement primaryElement = null;
		if (!prevent_merge)
		{
			int cell = Grid.PosToCell(position);
			GameObject gameObject2 = Grid.Objects[cell, 3];
			if (gameObject2 != null)
			{
				Pickupable component = gameObject2.GetComponent<Pickupable>();
				if (component != null)
				{
					Tag tag = GameTagExtensions.Create(elementID);
					for (ObjectLayerListItem objectLayerListItem = component.objectLayerListItem; objectLayerListItem != null; objectLayerListItem = objectLayerListItem.nextItem)
					{
						KPrefabID component2 = objectLayerListItem.gameObject.GetComponent<KPrefabID>();
						if (component2.PrefabTag == tag)
						{
							PrimaryElement component3 = component2.GetComponent<PrimaryElement>();
							if (component3.Mass + mass <= PrimaryElement.MAX_MASS)
							{
								gameObject = component2.gameObject;
								primaryElement = component3;
								temperature = SimUtil.CalculateFinalTemperature(primaryElement.Mass, primaryElement.Temperature, mass, temperature);
								position = gameObject.transform.GetPosition();
								break;
							}
						}
					}
				}
			}
		}
		if (gameObject == null)
		{
			GameObject prefab = Assets.GetPrefab(nameTag);
			gameObject = GameUtil.KInstantiate(prefab, Grid.SceneLayer.Ore);
			primaryElement = gameObject.GetComponent<PrimaryElement>();
			primaryElement.Mass = mass;
		}
		else
		{
			Debug.Assert(primaryElement != null);
			Pickupable component4 = primaryElement.GetComponent<Pickupable>();
			if (component4 != null)
			{
				component4.TotalAmount += mass / primaryElement.MassPerUnit;
			}
			else
			{
				primaryElement.Mass += mass;
			}
		}
		primaryElement.Temperature = temperature;
		position.z = Grid.GetLayerZ(Grid.SceneLayer.Ore);
		gameObject.transform.SetPosition(position);
		if (!manual_activation)
		{
			ActivateSubstanceGameObject(gameObject, disease_idx, disease_count);
		}
		return gameObject;
	}

	public void ActivateSubstanceGameObject(GameObject obj, byte disease_idx, int disease_count)
	{
		obj.SetActive(value: true);
		obj.GetComponent<PrimaryElement>().AddDisease(disease_idx, disease_count, "Substances.SpawnResource");
	}

	private void SetTexture(MaterialPropertyBlock block, string texture_name)
	{
		Texture texture = material.GetTexture(texture_name);
		if (texture != null)
		{
			propertyBlock.SetTexture(texture_name, texture);
		}
	}

	public void RefreshPropertyBlock()
	{
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		if (material != null)
		{
			SetTexture(propertyBlock, "_MainTex");
			float value = material.GetFloat("_WorldUVScale");
			propertyBlock.SetFloat("_WorldUVScale", value);
			Element element = ElementLoader.FindElementByHash(elementID);
			if (element.IsSolid)
			{
				SetTexture(propertyBlock, "_MainTex2");
				SetTexture(propertyBlock, "_HeightTex2");
				propertyBlock.SetFloat("_Frequency", material.GetFloat("_Frequency"));
				propertyBlock.SetColor("_ShineColour", material.GetColor("_ShineColour"));
				propertyBlock.SetColor("_ColourTint", material.GetColor("_ColourTint"));
			}
		}
	}

	internal AmbienceType GetAmbience()
	{
		return (audioConfig != null) ? audioConfig.ambienceType : AmbienceType.None;
	}

	internal SolidAmbienceType GetSolidAmbience()
	{
		return (audioConfig != null) ? audioConfig.solidAmbienceType : SolidAmbienceType.None;
	}

	internal string GetMiningSound()
	{
		return (audioConfig != null) ? audioConfig.miningSound : "";
	}

	internal string GetMiningBreakSound()
	{
		return (audioConfig != null) ? audioConfig.miningBreakSound : "";
	}

	internal string GetOreBumpSound()
	{
		return (audioConfig != null) ? audioConfig.oreBumpSound : "";
	}

	internal string GetFloorEventAudioCategory()
	{
		return (audioConfig != null) ? audioConfig.floorEventAudioCategory : "";
	}

	internal string GetCreatureChewSound()
	{
		return (audioConfig != null) ? audioConfig.creatureChewSound : "";
	}
}
