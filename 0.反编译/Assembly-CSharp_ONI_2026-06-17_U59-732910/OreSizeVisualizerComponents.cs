using System;
using System.Collections.Generic;
using UnityEngine;

public class OreSizeVisualizerComponents : KGameObjectComponentManager<OreSizeVisualizerData>
{
	private struct MassTier
	{
		public HashedString animName;

		public float massRequired;

		public float colliderRadius;
	}

	public enum TiersSetType
	{
		Ores,
		PokeShells,
		WoodPokeShells,
		PlantFiber
	}

	private static readonly MassTier[] MassTiers = new MassTier[3]
	{
		new MassTier
		{
			animName = "idle1",
			massRequired = 50f,
			colliderRadius = 0.15f
		},
		new MassTier
		{
			animName = "idle2",
			massRequired = 600f,
			colliderRadius = 0.2f
		},
		new MassTier
		{
			animName = "idle3",
			massRequired = float.MaxValue,
			colliderRadius = 0.25f
		}
	};

	private static readonly MassTier[] PokeShellMassTiers = new MassTier[3]
	{
		new MassTier
		{
			animName = "idle1",
			massRequired = 45f,
			colliderRadius = 0.15f
		},
		new MassTier
		{
			animName = "idle2",
			massRequired = 90f,
			colliderRadius = 0.2f
		},
		new MassTier
		{
			animName = "idle3",
			massRequired = float.MaxValue,
			colliderRadius = 0.25f
		}
	};

	private static readonly MassTier[] WoodPokeShellMassTiers = new MassTier[3]
	{
		new MassTier
		{
			animName = "idle1",
			massRequired = 75f,
			colliderRadius = 0.15f
		},
		new MassTier
		{
			animName = "idle2",
			massRequired = 150f,
			colliderRadius = 0.2f
		},
		new MassTier
		{
			animName = "idle3",
			massRequired = float.MaxValue,
			colliderRadius = 0.25f
		}
	};

	private static readonly MassTier[] PlantMatterMassTiers = new MassTier[3]
	{
		new MassTier
		{
			animName = "idle1",
			massRequired = 10f,
			colliderRadius = 0.15f
		},
		new MassTier
		{
			animName = "idle2",
			massRequired = 50f,
			colliderRadius = 0.2f
		},
		new MassTier
		{
			animName = "idle3",
			massRequired = float.MaxValue,
			colliderRadius = 0.25f
		}
	};

	private static readonly Dictionary<TiersSetType, MassTier[]> TierSets = new Dictionary<TiersSetType, MassTier[]>
	{
		[TiersSetType.Ores] = MassTiers,
		[TiersSetType.PokeShells] = PokeShellMassTiers,
		[TiersSetType.WoodPokeShells] = WoodPokeShellMassTiers,
		[TiersSetType.PlantFiber] = PlantMatterMassTiers
	};

	private static Action<object, object> OnMassChangedDispatcher = delegate(object context, object data)
	{
		OnMassChanged((HandleVector<int>.Handle)context, data);
	};

	public HandleVector<int>.Handle Add(GameObject go)
	{
		HandleVector<int>.Handle handle = Add(go, new OreSizeVisualizerData(go));
		OnPrefabInit(handle);
		return handle;
	}

	public static HashedString GetAnimForMass(float mass)
	{
		return GetAnimForMass(TiersSetType.Ores, mass);
	}

	public static HashedString GetAnimForMass(TiersSetType tierType, float mass)
	{
		for (int i = 0; i < TierSets[tierType].Length; i++)
		{
			if (mass <= TierSets[tierType][i].massRequired)
			{
				return TierSets[tierType][i].animName;
			}
		}
		return HashedString.Invalid;
	}

	protected override void OnPrefabInit(HandleVector<int>.Handle handle)
	{
		OreSizeVisualizerData new_data = GetData(handle);
		new_data.absorbHandle = new_data.primaryElement.Subscribe(-2064133523, OnMassChangedDispatcher, handle);
		new_data.splitFromChunkHandle = new_data.primaryElement.Subscribe(1335436905, OnMassChangedDispatcher, handle);
		SetData(handle, new_data);
	}

	protected override void OnSpawn(HandleVector<int>.Handle handle)
	{
		OnMassChanged(handle, null);
	}

	protected override void OnCleanUp(HandleVector<int>.Handle handle)
	{
		OreSizeVisualizerData oreSizeVisualizerData = GetData(handle);
		if (oreSizeVisualizerData.primaryElement != null)
		{
			oreSizeVisualizerData.primaryElement.Unsubscribe(ref oreSizeVisualizerData.absorbHandle);
			oreSizeVisualizerData.primaryElement.Unsubscribe(ref oreSizeVisualizerData.splitFromChunkHandle);
		}
	}

	private static void OnMassChanged(HandleVector<int>.Handle handle, object other_data)
	{
		OreSizeVisualizerData oreSizeVisualizerData = GameComps.OreSizeVisualizers.GetData(handle);
		PrimaryElement primaryElement = oreSizeVisualizerData.primaryElement;
		float mass = primaryElement.Mass;
		MassTier massTier = default(MassTier);
		MassTier[] array = TierSets[oreSizeVisualizerData.tierSetType];
		for (int i = 0; i < array.Length; i++)
		{
			if (mass <= array[i].massRequired)
			{
				massTier = array[i];
				break;
			}
		}
		primaryElement.GetComponent<KBatchedAnimController>().Play(massTier.animName);
		KCircleCollider2D component = primaryElement.GetComponent<KCircleCollider2D>();
		if (component != null)
		{
			component.radius = massTier.colliderRadius;
		}
		primaryElement.Trigger(1807976145);
	}
}
