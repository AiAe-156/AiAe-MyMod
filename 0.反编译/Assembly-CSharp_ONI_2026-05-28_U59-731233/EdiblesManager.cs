using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/EdiblesManager")]
public class EdiblesManager : KMonoBehaviour
{
	public class FoodInfo : IConsumableUIItem, IHasDlcRestrictions
	{
		public string Id;

		public string Name;

		public string Description;

		public float CaloriesPerUnit;

		public float PreserveTemperature;

		public float RotTemperature;

		public float StaleTime;

		public float SpoilTime;

		public bool CanRot;

		public int Quality;

		public List<string> Effects;

		private string[] requiredDlcIds;

		private string[] forbiddenDlcIds;

		public string ConsumableId => Id;

		public string ConsumableName => Name;

		public int MajorOrder => Quality;

		public int MinorOrder => (int)CaloriesPerUnit;

		public bool Display => CaloriesPerUnit != 0f;

		public string[] GetRequiredDlcIds()
		{
			return requiredDlcIds;
		}

		public string[] GetForbiddenDlcIds()
		{
			return forbiddenDlcIds;
		}

		[Obsolete("Use constructor with required/forbidden instead")]
		public FoodInfo(string id, string dlcId, float caloriesPerUnit, int quality, float preserveTemperatue, float rotTemperature, float spoilTime, bool can_rot)
			: this(id, caloriesPerUnit, quality, preserveTemperatue, rotTemperature, spoilTime, can_rot)
		{
			if (dlcId != "")
			{
				requiredDlcIds = new string[1] { dlcId };
			}
		}

		public FoodInfo(string id, float caloriesPerUnit, int quality, float preserveTemperatue, float rotTemperature, float spoilTime, bool can_rot, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
		{
			Id = id;
			this.requiredDlcIds = requiredDlcIds;
			this.forbiddenDlcIds = forbiddenDlcIds;
			CaloriesPerUnit = caloriesPerUnit;
			Quality = quality;
			PreserveTemperature = preserveTemperatue;
			RotTemperature = rotTemperature;
			StaleTime = spoilTime / 2f;
			SpoilTime = spoilTime;
			CanRot = can_rot;
			Name = Strings.Get("STRINGS.ITEMS.FOOD." + id.ToUpper() + ".NAME");
			Description = Strings.Get("STRINGS.ITEMS.FOOD." + id.ToUpper() + ".DESC");
			Effects = new List<string>();
			s_allFoodTypes.Add(this);
			s_allFoodMap[Id] = this;
		}

		public FoodInfo AddEffects(List<string> effects, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
		{
			if (DlcManager.IsCorrectDlcSubscribed(requiredDlcIds, forbiddenDlcIds))
			{
				Effects.AddRange(effects);
			}
			return this;
		}
	}

	private static List<FoodInfo> s_allFoodTypes = new List<FoodInfo>();

	private static Dictionary<string, FoodInfo> s_allFoodMap = new Dictionary<string, FoodInfo>();

	private static List<FoodInfo> s_loadedFoodTypes;

	public static List<FoodInfo> GetAllLoadedFoodTypes()
	{
		return s_allFoodTypes.Where(DlcManager.IsCorrectDlcSubscribed).ToList();
	}

	public static void ClearSaveFoodCache()
	{
		s_loadedFoodTypes = null;
	}

	public static List<FoodInfo> GetAllFoodTypes()
	{
		if (s_loadedFoodTypes == null)
		{
			s_loadedFoodTypes = s_allFoodTypes.Where(Game.IsCorrectDlcActiveForCurrentSave).ToList();
		}
		Debug.Assert(SaveLoader.Instance != null, "Call GetAllLoadedFoodTypes from the frontend");
		return s_loadedFoodTypes;
	}

	public static FoodInfo GetFoodInfo(string foodID)
	{
		string key = foodID.Replace("Compost", "");
		FoodInfo value = null;
		bool flag = s_allFoodMap.TryGetValue(key, out value);
		return value;
	}

	public static bool TryGetFoodInfo(string foodID, out FoodInfo info)
	{
		info = null;
		if (string.IsNullOrEmpty(foodID))
		{
			return false;
		}
		info = GetFoodInfo(foodID);
		return info != null;
	}
}
