using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Database;
using HarmonyLib;
using UnityEngine;

namespace UtilLibs;

public static class SupplyClosetUtils
{
	public class SkinCollection
	{
		public class SkinEntry
		{
			public string ID;

			public string Name;

			public string Description;

			public string KanimFile;

			public Dictionary<string, string> Workables;

			public SkinEntry(string id, string name, string desc, string kanim, Dictionary<string, string> workables = null)
			{
				ID = id;
				Name = name;
				Description = desc;
				KanimFile = kanim;
				Workables = workables;
			}
		}

		public static List<SkinCollection> SkinSets = new List<SkinCollection>();

		public static Dictionary<string, List<string>> SkinIds = new Dictionary<string, List<string>>();

		private bool isMainCategory = false;

		private string subcategoryID;

		private string mainCategoryID;

		private string buildingId;

		private Sprite newCategoryIcon = null;

		private int sortkey = -1;

		private List<SkinEntry> skins;

		public List<SkinEntry> Skins => skins;

		public static SkinCollection Create(string buildingID, string _subcategoryId)
		{
			return new SkinCollection(buildingID, _subcategoryId);
		}

		public static SkinCollection CategoryInit(string _mainCategory, string _subcategoryID, Sprite icon, int _sortkey)
		{
			return new SkinCollection("", _subcategoryID).NewCategory(_mainCategory, icon, _sortkey);
		}

		public SkinCollection(string buildingID, string _subcategoryID)
		{
			buildingId = buildingID;
			subcategoryID = _subcategoryID;
			skins = new List<SkinEntry>();
			SkinSets.Add(this);
			if (buildingId.Any())
			{
				SkinIds.Add(buildingId, new List<string>());
			}
		}

		public SkinCollection NewCategory(string _mainCategory, Sprite icon, int _sortkey)
		{
			mainCategoryID = _mainCategory;
			newCategoryIcon = icon;
			sortkey = _sortkey;
			isMainCategory = true;
			return this;
		}

		public SkinCollection Skin(string Id, string name, string description, string kanimFile, Dictionary<string, string> workables = null)
		{
			skins.Add(new SkinEntry(Id, name, description, kanimFile, workables));
			SkinIds[buildingId].Add(Id);
			return this;
		}

		public void RegisterCategory()
		{
			string[] permitIDs = skins.Select((SkinEntry entry) => entry.ID).ToArray();
			if (isMainCategory)
			{
				AddSubcategory(mainCategoryID, subcategoryID, newCategoryIcon, sortkey, permitIDs);
			}
			else
			{
				AddItemsToSubcategory(subcategoryID, permitIDs);
			}
		}

		public void RegisterSkins(ResourceSet<BuildingFacadeResource> set)
		{
			foreach (SkinEntry skin in skins)
			{
			}
		}

		public static void RegisterAllSkins()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected O, but got Unknown
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Expected O, but got Unknown
			SgtLogger.l("Patching Skin Injection..");
			Harmony val = new Harmony("SgtImalas_SupplyClosetUtils");
			Type type = AccessTools.TypeByName("Database.BuildingFacades");
			ConstructorInfo constructorInfo = AccessTools.Constructor(type, new Type[1] { typeof(ResourceSet) }, false);
			MethodInfo methodInfo = AccessTools.Method(typeof(SkinCollection), "BuildingFacades_Postfix", (Type[])null, (Type[])null);
			val.Patch((MethodBase)constructorInfo, (HarmonyMethod)null, new HarmonyMethod(methodInfo), (HarmonyMethod)null, (HarmonyMethod)null);
			Type type2 = AccessTools.TypeByName("InventoryOrganization");
			MethodInfo methodInfo2 = AccessTools.Method(type2, "GenerateSubcategories", (Type[])null, (Type[])null);
			MethodInfo methodInfo3 = AccessTools.Method(typeof(SkinCollection), "Subcategories_Postfix", (Type[])null, (Type[])null);
			val.Patch((MethodBase)methodInfo2, (HarmonyMethod)null, new HarmonyMethod(methodInfo3), (HarmonyMethod)null, (HarmonyMethod)null);
		}

		public static void BuildingFacades_Postfix(object __instance)
		{
			ResourceSet<BuildingFacadeResource> set = (ResourceSet<BuildingFacadeResource>)__instance;
			SgtLogger.l("Registering " + SkinSets.Count + " skin collections");
			foreach (SkinCollection skinSet in SkinSets)
			{
				SgtLogger.l("Registering skins for " + skinSet.buildingId);
				skinSet.RegisterSkins(set);
			}
		}

		public static void Subcategories_Postfix()
		{
			foreach (SkinCollection skinSet in SkinSets)
			{
				skinSet.RegisterCategory();
			}
		}
	}

	public static bool TryGetCollectionFor(string buildingID, out List<string> collection)
	{
		return SkinCollection.SkinIds.TryGetValue(buildingID, out collection);
	}

	public static List<string> AddOrGetSubCategory(string subCategory, string mainCategory = null, Sprite icon = null, int sortkey = 850)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!InventoryOrganization.subcategoryIdToPermitIdsMap.ContainsKey(subCategory))
		{
			if (mainCategory == null)
			{
				mainCategory = "UNCATEGORIZED";
			}
			if ((Object)(object)icon == (Object)null)
			{
				icon = Assets.GetSprite(HashedString.op_Implicit("unknown"));
			}
			InventoryOrganization.AddSubcategory(subCategory, icon, sortkey, Array.Empty<string>());
			InventoryOrganization.categoryIdToSubcategoryIdsMap[mainCategory].Add(subCategory);
		}
		return InventoryOrganization.subcategoryIdToPermitIdsMap[subCategory];
	}

	public static void AddItemsToSubcategory(string subcategoryID, string[] permitIDs)
	{
		List<string> list = AddOrGetSubCategory(subcategoryID);
		for (int i = 0; i < permitIDs.Length; i++)
		{
			list.Add(permitIDs[i]);
		}
	}

	public static void AddSubcategory(string mainCategory, string subcategoryID, Sprite icon, int sortkey, string[] permitIDs)
	{
		List<string> list = AddOrGetSubCategory(subcategoryID, mainCategory, icon, sortkey);
		for (int i = 0; i < permitIDs.Length; i++)
		{
			list.Add(permitIDs[i]);
		}
	}
}
