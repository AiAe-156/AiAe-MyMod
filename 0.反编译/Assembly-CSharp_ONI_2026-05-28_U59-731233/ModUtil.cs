using System;
using System.Collections.Generic;
using System.IO;
using KMod;
using TUNING;
using UnityEngine;

public static class ModUtil
{
	public enum BuildingOrdering
	{
		Before,
		After
	}

	public static void AddBuildingToPlanScreen(HashedString category, string building_id)
	{
		AddBuildingToPlanScreen(category, building_id, "uncategorized");
	}

	public static void AddBuildingToPlanScreen(HashedString category, string building_id, string subcategoryID)
	{
		AddBuildingToPlanScreen(category, building_id, subcategoryID, null);
	}

	public static void AddBuildingToPlanScreen(HashedString category, string building_id, string subcategoryID, string relativeBuildingId, BuildingOrdering ordering = BuildingOrdering.After)
	{
		int num = BUILDINGS.PLANORDER.FindIndex((PlanScreen.PlanInfo x) => x.category == category);
		if (num < 0)
		{
			Debug.LogWarning($"Mod: Unable to add '{building_id}' as category '{category}' does not exist");
			return;
		}
		List<KeyValuePair<string, string>> buildingAndSubcategoryData = BUILDINGS.PLANORDER[num].buildingAndSubcategoryData;
		KeyValuePair<string, string> item = new KeyValuePair<string, string>(building_id, subcategoryID);
		if (relativeBuildingId == null)
		{
			buildingAndSubcategoryData.Add(item);
			return;
		}
		int num2 = buildingAndSubcategoryData.FindIndex((KeyValuePair<string, string> x) => x.Key == relativeBuildingId);
		if (num2 == -1)
		{
			buildingAndSubcategoryData.Add(item);
			Debug.LogWarning("Mod: Building '" + relativeBuildingId + "' doesn't exist, inserting '" + building_id + "' at the end of the list instead");
		}
		else
		{
			int index = ((ordering == BuildingOrdering.After) ? (num2 + 1) : Mathf.Max(num2, 0));
			buildingAndSubcategoryData.Insert(index, item);
		}
	}

	[Obsolete("Use PlanScreen instead")]
	public static void AddBuildingToHotkeyBuildMenu(HashedString category, string building_id, Action hotkey)
	{
		BuildMenu.DisplayInfo info = BuildMenu.OrderedBuildings.GetInfo(category);
		if (!(info.category != category))
		{
			IList<BuildMenu.BuildingInfo> list = info.data as IList<BuildMenu.BuildingInfo>;
			list.Add(new BuildMenu.BuildingInfo(building_id, hotkey));
		}
	}

	public static KAnimFile AddKAnimMod(string name, KAnimFile.Mod anim_mod)
	{
		KAnimFile kAnimFile = ScriptableObject.CreateInstance<KAnimFile>();
		kAnimFile.mod = anim_mod;
		kAnimFile.name = name;
		AnimCommandFile animCommandFile = new AnimCommandFile();
		KAnimGroupFile.GroupFile groupFile = new KAnimGroupFile.GroupFile();
		groupFile.groupID = animCommandFile.GetGroupName(kAnimFile);
		groupFile.commandDirectory = "assets/" + name;
		animCommandFile.AddGroupFile(groupFile);
		KAnimGroupFile groupFile2 = KAnimGroupFile.GetGroupFile();
		if (groupFile2.AddAnimMod(groupFile, animCommandFile, kAnimFile) == KAnimGroupFile.AddModResult.Added)
		{
			Assets.ModLoadedKAnims.Add(kAnimFile);
		}
		return kAnimFile;
	}

	public static KAnimFile AddKAnim(string name, TextAsset anim_file, TextAsset build_file, IList<Texture2D> textures)
	{
		KAnimFile kAnimFile = ScriptableObject.CreateInstance<KAnimFile>();
		kAnimFile.Initialize(anim_file, build_file, textures);
		kAnimFile.name = name;
		AnimCommandFile animCommandFile = new AnimCommandFile();
		KAnimGroupFile.GroupFile groupFile = new KAnimGroupFile.GroupFile();
		groupFile.groupID = animCommandFile.GetGroupName(kAnimFile);
		groupFile.commandDirectory = "assets/" + name;
		animCommandFile.AddGroupFile(groupFile);
		KAnimGroupFile groupFile2 = KAnimGroupFile.GetGroupFile();
		groupFile2.AddAnimFile(groupFile, animCommandFile, kAnimFile);
		Assets.ModLoadedKAnims.Add(kAnimFile);
		return kAnimFile;
	}

	public static KAnimFile AddKAnim(string name, TextAsset anim_file, TextAsset build_file, Texture2D texture)
	{
		List<Texture2D> list = new List<Texture2D>();
		list.Add(texture);
		return AddKAnim(name, anim_file, build_file, list);
	}

	public static Substance CreateSubstance(string name, Element.State state, KAnimFile kanim, Material material, Color32 colour, Color32 ui_colour, Color32 conduit_colour)
	{
		Substance substance = new Substance();
		substance.name = name;
		substance.nameTag = TagManager.Create(name);
		substance.elementID = (SimHashes)Hash.SDBMLower(name);
		substance.anim = kanim;
		substance.colour = colour;
		substance.uiColour = ui_colour;
		substance.conduitColour = conduit_colour;
		substance.material = material;
		substance.renderedByWorld = (state & Element.State.Solid) == Element.State.Solid;
		return substance;
	}

	public static void RegisterForTranslation(Type locstring_tree_root)
	{
		Localization.RegisterForTranslation(locstring_tree_root);
		Localization.GenerateStringsTemplate(locstring_tree_root, Path.Combine(Manager.GetDirectory(), "strings_templates"));
	}

	public static Texture2D LoadTexture(string path)
	{
		Texture2D texture2D = null;
		if (File.Exists(path))
		{
			byte[] data = File.ReadAllBytes(path);
			texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(data);
		}
		else
		{
			Debug.LogWarning("ModUtil: Texture file '" + path + "' not found");
		}
		return texture2D;
	}
}
