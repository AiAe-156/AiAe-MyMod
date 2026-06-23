using System;
using System.Collections.Generic;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/ConsumerManager")]
public class ConsumerManager : KMonoBehaviour, ISaveLoadable
{
	public static ConsumerManager instance;

	[Serialize]
	private List<Tag> undiscoveredConsumableTags = new List<Tag>();

	[Serialize]
	private List<Tag> defaultForbiddenTagsList = new List<Tag>();

	public static string OXYGEN_TANK_ID = ClosestOxygenCanisterSensor.GenericBreathableGassesTankTag.ToString();

	public List<Tag> DefaultForbiddenTagsList => defaultForbiddenTagsList;

	public List<Tag> StandardDuplicantDietaryRestrictions
	{
		get
		{
			List<Tag> list = new List<Tag>();
			foreach (GameObject item in Assets.GetPrefabsWithTag(GameTags.ChargedPortableBattery))
			{
				list.Add(item.PrefabID());
			}
			list.Add(OXYGEN_TANK_ID);
			return list;
		}
	}

	public List<Tag> BionicDuplicantDietaryRestrictions
	{
		get
		{
			List<Tag> list = new List<Tag>();
			foreach (GameObject item2 in Assets.GetPrefabsWithTag(GameTags.Edible))
			{
				list.Add(item2.PrefabID());
			}
			Tag[] array = new Tag[GameTags.BionicIncompatibleBatteries.Count];
			GameTags.BionicIncompatibleBatteries.CopyTo(array, 0);
			Tag[] array2 = array;
			foreach (Tag item in array2)
			{
				list.Add(item);
			}
			return list;
		}
	}

	public event Action<Tag> OnDiscover;

	public static void DestroyInstance()
	{
		instance = null;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		instance = this;
		RefreshDiscovered();
		DiscoveredResources.Instance.OnDiscover += OnWorldInventoryDiscover;
		Game.Instance.Subscribe(-107300940, RefreshDiscovered);
	}

	public bool isDiscovered(Tag id)
	{
		return !undiscoveredConsumableTags.Contains(id);
	}

	private void OnWorldInventoryDiscover(Tag category_tag, Tag tag)
	{
		if (undiscoveredConsumableTags.Contains(tag))
		{
			RefreshDiscovered();
		}
	}

	public void RefreshDiscovered(object data = null)
	{
		foreach (EdiblesManager.FoodInfo allFoodType in EdiblesManager.GetAllFoodTypes())
		{
			if (!ShouldBeDiscovered(allFoodType.Id.ToTag()) && !undiscoveredConsumableTags.Contains(allFoodType.Id.ToTag()))
			{
				undiscoveredConsumableTags.Add(allFoodType.Id.ToTag());
				if (this.OnDiscover != null)
				{
					this.OnDiscover("UndiscoveredSomething".ToTag());
				}
			}
			else
			{
				if (!undiscoveredConsumableTags.Contains(allFoodType.Id.ToTag()) || !ShouldBeDiscovered(allFoodType.Id.ToTag()))
				{
					continue;
				}
				undiscoveredConsumableTags.Remove(allFoodType.Id.ToTag());
				if (this.OnDiscover != null)
				{
					this.OnDiscover(allFoodType.Id.ToTag());
				}
				if (!DiscoveredResources.Instance.IsDiscovered(allFoodType.Id.ToTag()))
				{
					if (allFoodType.CaloriesPerUnit == 0f)
					{
						DiscoveredResources.Instance.Discover(allFoodType.Id.ToTag(), GameTags.CookingIngredient);
					}
					else
					{
						DiscoveredResources.Instance.Discover(allFoodType.Id.ToTag(), GameTags.Edible);
					}
				}
			}
		}
	}

	private bool ShouldBeDiscovered(Tag food_id)
	{
		if (DiscoveredResources.Instance.IsDiscovered(food_id))
		{
			return true;
		}
		foreach (Recipe recipe in RecipeManager.Get().recipes)
		{
			if (!(recipe.Result == food_id))
			{
				continue;
			}
			string[] fabricators = recipe.fabricators;
			foreach (string id in fabricators)
			{
				if (Db.Get().TechItems.IsTechItemComplete(id))
				{
					return true;
				}
			}
		}
		foreach (Crop item in Components.Crops.Items)
		{
			if (Grid.IsVisible(Grid.PosToCell(item.gameObject)) && item.cropId == food_id.Name)
			{
				return true;
			}
		}
		return false;
	}
}
