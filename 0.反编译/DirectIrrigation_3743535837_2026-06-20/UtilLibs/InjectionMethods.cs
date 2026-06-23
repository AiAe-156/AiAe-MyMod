using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Database;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using STRINGS;
using TUNING;
using UnityEngine;

namespace UtilLibs;

public static class InjectionMethods
{
	public class BATCH_TAGS
	{
		public const int SWAPS = -77805842;

		public const int INTERACTS = -1371425853;
	}

	public class IncludePrivateContractResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			IEnumerable<JsonProperty> first = from f in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				select _003C_003En__0(f, memberSerialization);
			IEnumerable<JsonProperty> second = from p in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				select _003C_003En__0(p, memberSerialization);
			List<JsonProperty> list = first.Concat(second).ToList();
			foreach (JsonProperty item in list)
			{
				item.Readable = true;
				item.Writable = true;
			}
			return list;
		}

		protected override JsonContract CreateContract(Type objectType)
		{
			return ((DefaultContractResolver)this).CreateContract(objectType);
		}

		[CompilerGenerated]
		[DebuggerHidden]
		private JsonProperty _003C_003En__0(MemberInfo member, MemberSerialization memberSerialization)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((DefaultContractResolver)this).CreateProperty(member, memberSerialization);
		}
	}

	private static HashSet<string> ResearchablesFromMod = new HashSet<string>();

	public static void DumpBatchGroups(KAnimGroupFile __instance)
	{
		SgtLogger.l("Dumping all group ids of KanimGroupFile");
		foreach (Group group in __instance.groups)
		{
			SgtLogger.l("KanimGroup: " + ((object)Unsafe.As<HashedString, HashedString>(ref group.id)/*cast due to .constrained prefix*/).ToString() + ", dir: " + group.commandDirectory);
			foreach (KAnimFile animFile in group.animFiles)
			{
				SgtLogger.l(((Object)animFile).name);
			}
		}
	}

	public static void RegisterCustomSwapAnim(KAnimGroupFile kAnimGroupFile, HashedString swap)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		RegisterCustomSwapAnims(kAnimGroupFile, new HashSet<HashedString> { swap });
	}

	public static void RegisterCustomSwapAnims(KAnimGroupFile kAnimGroupFile, HashSet<HashedString> swaps)
	{
		MoveKanimsToNewGroup(kAnimGroupFile, -77805842, swaps);
	}

	public static void RegisterCustomInteractAnim(KAnimGroupFile kAnimGroupFile, HashedString swap)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		RegisterCustomInteractAnims(kAnimGroupFile, new HashSet<HashedString> { swap });
	}

	public static void RegisterCustomInteractAnims(KAnimGroupFile kAnimGroupFile, HashSet<HashedString> swaps)
	{
		MoveKanimsToNewGroup(kAnimGroupFile, -1371425853, swaps);
	}

	public static void MoveKanimsToBatchGroupOf(KAnimGroupFile kAnimGroupFile, HashSet<HashedString> swaps, string animInTargetGroupId)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		List<Group> data = kAnimGroupFile.GetData();
		KAnimFile item = default(KAnimFile);
		if (!Assets.TryGetAnim(HashedString.op_Implicit(animInTargetGroupId), ref item))
		{
			Debug.LogWarning((object)("Could not find anim " + animInTargetGroupId + " in asset list!"));
			return;
		}
		Group val = null;
		foreach (Group item2 in data)
		{
			if (item2.animFiles.Contains(item))
			{
				val = item2;
				break;
			}
		}
		if (val == null)
		{
			Debug.LogWarning((object)("Could not find anim group for " + animInTargetGroupId + "!"));
		}
		else
		{
			MoveKanimsToNewGroup(kAnimGroupFile, val.id.hash, swaps);
		}
	}

	public unsafe static void MoveKanimsToNewGroup(KAnimGroupFile kAnimGroupFile, int groupIdHash, HashSet<HashedString> swaps)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		List<Group> data = kAnimGroupFile.GetData();
		Group val = KAnimGroupFile.GetGroup(new HashedString(groupIdHash));
		data.RemoveAll((Group g) => swaps.Contains(g.animNames[0]));
		foreach (HashedString swap in swaps)
		{
			KAnimFile anim = Assets.GetAnim(swap);
			if ((Object)(object)anim == (Object)null)
			{
				HashedString val2 = swap;
				SgtLogger.warning("anim " + ((object)(*(HashedString*)(&val2))/*cast due to .constrained prefix*/).ToString() + " not found");
				continue;
			}
			if (val.animFiles.Contains(anim) || val.animNames.Contains(HashedString.op_Implicit(((Object)anim).name)))
			{
				HashedString val2 = swap;
				SgtLogger.warning("anim " + ((object)(*(HashedString*)(&val2))/*cast due to .constrained prefix*/).ToString() + " already in group");
				continue;
			}
			val.animFiles.Add(anim);
			val.animNames.Add(HashedString.op_Implicit(((Object)anim).name));
			Group val3 = val;
			SgtLogger.l(((object)anim)?.ToString() + "; " + ((Object)anim).name + " added to group");
		}
	}

	public static Func<S, T> CreateGetter<S, T>(FieldInfo field)
	{
		string name = field.ReflectedType.FullName + ".get_" + field.Name;
		DynamicMethod dynamicMethod = new DynamicMethod(name, typeof(T), new Type[1] { typeof(S) }, restrictedSkipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		if (field.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Ldsfld, field);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldfld, field);
		}
		iLGenerator.Emit(OpCodes.Ret);
		return (Func<S, T>)dynamicMethod.CreateDelegate(typeof(Func<S, T>));
	}

	public static Action<S, T> CreateSetter<S, T>(FieldInfo field)
	{
		string name = field.ReflectedType.FullName + ".set_" + field.Name;
		DynamicMethod dynamicMethod = new DynamicMethod(name, null, new Type[2]
		{
			typeof(S),
			typeof(T)
		}, restrictedSkipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		if (field.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(OpCodes.Stsfld, field);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(OpCodes.Stfld, field);
		}
		iLGenerator.Emit(OpCodes.Ret);
		return (Action<S, T>)dynamicMethod.CreateDelegate(typeof(Action<S, T>));
	}

	public static void AddStatusItem(string status_id, string category, string name, string desc)
	{
		status_id = status_id.ToUpperInvariant();
		category = category.ToUpperInvariant();
		Strings.Add(new string[2]
		{
			"STRINGS." + category + ".STATUSITEMS." + status_id + ".NAME",
			name
		});
		Strings.Add(new string[2]
		{
			"STRINGS." + category + ".STATUSITEMS." + status_id + ".TOOLTIP",
			desc
		});
	}

	public static bool IsFromThisMod(string id)
	{
		return ResearchablesFromMod.Contains(id);
	}

	public static TechItem AddItemToTechnologySprite(string techItemId, string targetTechId, string name, string description, string spriteName, string[] requiredDLcs = null, string[] forbiddenDlc = null, bool isPoiUnlock = false)
	{
		AddBuildingToTechnology(targetTechId, techItemId);
		return Db.Get().TechItems.AddTechItem(techItemId, name, description, GetSpriteFnBuilder(spriteName), requiredDLcs, forbiddenDlc, isPoiUnlock);
	}

	public static TechItem AddItemToTechnologyKanim(string techItemId, string targetTechId, string name, string description, string kanimName, string uiAnim = "ui", string[] requiredDLcs = null, string[] forbiddenDlc = null, bool isPoiUnlock = false)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Sprite sprite = Def.GetUISpriteFromMultiObjectAnim(Assets.GetAnim(HashedString.op_Implicit(kanimName)), uiAnim, false, "");
		AddBuildingToTechnology(targetTechId, techItemId);
		return Db.Get().TechItems.AddTechItem(techItemId, name, description, (Func<string, bool, Sprite>)((string anim, bool centered) => sprite), requiredDLcs, forbiddenDlc, isPoiUnlock);
	}

	public static void MoveExistingBuildingToNewPlanscreen(HashedString category, string building_id, string subcategoryID = "uncategorized", BuildingOrdering ordering = (BuildingOrdering)1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		MoveExistingBuildingToNewCategory(category, building_id, string.Empty, subcategoryID, ordering);
	}

	public static void MoveExistingBuildingToNewCategory(HashedString category, string building_id, string relativeBuildingId = "", string subcategoryID = "uncategorized", BuildingOrdering ordering = (BuildingOrdering)1)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		if (subcategoryID == string.Empty || subcategoryID == null)
		{
			subcategoryID = "uncategorized";
		}
		bool flag = false;
		foreach (PlanInfo item in BUILDINGS.PLANORDER)
		{
			int num = item.buildingAndSubcategoryData.FindIndex((KeyValuePair<string, string> dat) => dat.Key == building_id);
			if (num > -1)
			{
				SgtLogger.l($"Building {building_id} found in category {item.category}, moving it to {category}");
				item.buildingAndSubcategoryData.RemoveAt(num);
				flag = true;
				break;
			}
		}
		if (BUILDINGS.PLANSUBCATEGORYSORTING.ContainsKey(building_id))
		{
			BUILDINGS.PLANSUBCATEGORYSORTING.Remove(building_id);
		}
		if (!flag)
		{
			SgtLogger.l($"Building {building_id} had no previous category defined, adding it to {category}");
		}
		AddBuildingToPlanScreenBehindNext(category, building_id, relativeBuildingId, subcategoryID, ordering);
	}

	public static void AddBuildingToPlanScreen(HashedString category, string building_id, string subcategoryID = "uncategorized", BuildingOrdering ordering = (BuildingOrdering)1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		AddBuildingToPlanScreenBehindNext(category, building_id, string.Empty, subcategoryID, ordering);
	}

	public static void AddBuildingToPlanScreenBehindNext(HashedString category, string building_id, string relativeBuildingId = null, string subcategoryID = "uncategorized", BuildingOrdering ordering = (BuildingOrdering)1)
	{
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (relativeBuildingId != null)
		{
			if (subcategoryID == "uncategorized" && BUILDINGS.PLANSUBCATEGORYSORTING.ContainsKey(relativeBuildingId))
			{
				subcategoryID = BUILDINGS.PLANSUBCATEGORYSORTING[relativeBuildingId];
			}
			if (BUILDINGS.PLANSUBCATEGORYSORTING.ContainsKey(building_id))
			{
				BUILDINGS.PLANSUBCATEGORYSORTING[building_id] = subcategoryID;
			}
			else
			{
				BUILDINGS.PLANSUBCATEGORYSORTING.Add(building_id, subcategoryID);
			}
			ModUtil.AddBuildingToPlanScreen(category, building_id, subcategoryID, relativeBuildingId, ordering);
		}
		else if (relativeBuildingId == string.Empty && subcategoryID != "uncategorized")
		{
			if (BUILDINGS.PLANSUBCATEGORYSORTING.ContainsKey(building_id))
			{
				BUILDINGS.PLANSUBCATEGORYSORTING[building_id] = subcategoryID;
			}
			else
			{
				BUILDINGS.PLANSUBCATEGORYSORTING.Add(building_id, subcategoryID);
			}
			ModUtil.AddBuildingToPlanScreen(category, building_id, subcategoryID, relativeBuildingId, ordering);
		}
		else
		{
			BUILDINGS.PLANSUBCATEGORYSORTING[building_id] = subcategoryID;
			ModUtil.AddBuildingToPlanScreen(category, building_id, subcategoryID, relativeBuildingId, ordering);
		}
	}

	private static Func<string, bool, Sprite> GetSpriteFnBuilder(string spriteName)
	{
		return (string anim, bool centered) => Assets.GetSprite(HashedString.op_Implicit(spriteName));
	}

	public static void MoveItemToNewTech(string buildingId, string oldTechId, string newTechId)
	{
		Techs techs = Db.Get().Techs;
		if (((ResourceSet<Tech>)(object)techs).Exists(oldTechId) && ((ResourceSet<Tech>)(object)techs).Exists(newTechId) && ((ResourceSet<Tech>)(object)techs).Get(oldTechId).unlockedItemIDs.Contains(buildingId))
		{
			((ResourceSet<Tech>)(object)techs).Get(oldTechId).unlockedItemIDs.Remove(buildingId);
			((ResourceSet<Tech>)(object)techs).Get(newTechId).unlockedItemIDs.Add(buildingId);
		}
	}

	public static void AddBuildingToTechnologyOfOther(string buildingId, string otherBuildingId)
	{
		ResearchablesFromMod.Add(buildingId);
		Techs techs = Db.Get().Techs;
		foreach (Tech resource in ((ResourceSet<Tech>)(object)techs).resources)
		{
			if (resource.unlockedItemIDs.Contains(otherBuildingId))
			{
				resource.unlockedItemIDs.Add(buildingId);
				return;
			}
		}
		SgtLogger.error("Could not add " + buildingId + " to tech as " + otherBuildingId + " was not found in any existing tech");
	}

	public static void AddBuildingToTechnology(string techId, string buildingId)
	{
		ResearchablesFromMod.Add(buildingId);
		((ResourceSet<Tech>)(object)Db.Get().Techs).Get(techId).unlockedItemIDs.Add(buildingId);
	}

	public static Sprite AddSpriteToAssets(Assets instance, string spriteid, bool overrideExisting = false)
	{
		return AssetUtils.AddSpriteToAssets(instance, spriteid, overrideExisting, (TextureWrapMode)0);
	}

	public static void AddBuildingStrings(string buildingId, string name, string description = "", string effect = "")
	{
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDINGS.PREFABS." + buildingId.ToUpperInvariant() + ".NAME",
			UI.FormatAsLink(name, buildingId)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDINGS.PREFABS." + buildingId.ToUpperInvariant() + ".DESC",
			description
		});
		Strings.Add(new string[2]
		{
			"STRINGS.BUILDINGS.PREFABS." + buildingId.ToUpperInvariant() + ".EFFECT",
			effect
		});
	}

	public static void AddLaserEffect(string ID, HashedString context, KBatchedAnimEventToggler kbatchedAnimEventToggler, KBatchedAnimController kbac, string animFile, string defaultAnimation = "loop")
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		LaserEffect val = new LaserEffect
		{
			id = ID,
			animFile = animFile,
			anim = defaultAnimation,
			context = context
		};
		GameObject val2 = new GameObject(val.id);
		val2.transform.parent = ((KMonoBehaviour)kbatchedAnimEventToggler).transform;
		EntityTemplateExtensions.AddOrGet<KPrefabID>(val2).PrefabTag = new Tag(val.id);
		KBatchedAnimTracker val3 = EntityTemplateExtensions.AddOrGet<KBatchedAnimTracker>(val2);
		val3.controller = kbac;
		val3.symbol = new HashedString("snapTo_rgtHand");
		val3.offset = new Vector3(195f, -35f, 0f);
		val3.useTargetPoint = true;
		KBatchedAnimController val4 = EntityTemplateExtensions.AddOrGet<KBatchedAnimController>(val2);
		((KAnimControllerBase)val4).AnimFiles = (KAnimFile[])(object)new KAnimFile[1] { Assets.GetAnim(HashedString.op_Implicit(val.animFile)) };
		Entry item = new Entry
		{
			anim = val.anim,
			context = val.context,
			controller = val4
		};
		kbatchedAnimEventToggler.entries.Add(item);
		EntityTemplateExtensions.AddOrGet<LoopingSounds>(val2);
	}

	public static void AddCreatureStrings(string creatureId, string name)
	{
		Strings.Add(new string[2]
		{
			"STRINGS.CREATURES.FAMILY." + creatureId.ToUpperInvariant(),
			UI.FormatAsLink(name, creatureId)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.CREATURES.FAMILY_PLURAL." + creatureId.ToUpperInvariant(),
			UI.FormatAsLink(name + "s", creatureId)
		});
	}

	public static void AddPlantStrings(string plantId, string name, string description, string domesticatedDescription)
	{
		Strings.Add(new string[2]
		{
			"STRINGS.CREATURES.SPECIES." + plantId.ToUpperInvariant() + ".NAME",
			UI.FormatAsLink(name, plantId)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.CREATURES.SPECIES." + plantId.ToUpperInvariant() + ".DESC",
			description
		});
		Strings.Add(new string[2]
		{
			"STRINGS.CREATURES.SPECIES." + plantId.ToUpperInvariant() + ".DOMESTICATEDDESC",
			domesticatedDescription
		});
	}

	public static void AddPlantSeedStrings(string plantId, string name, string description)
	{
		Strings.Add(new string[2]
		{
			"STRINGS.CREATURES.SPECIES.SEEDS." + plantId.ToUpperInvariant() + ".NAME",
			UI.FormatAsLink(name, plantId)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.CREATURES.SPECIES.SEEDS." + plantId.ToUpperInvariant() + ".DESC",
			description
		});
	}

	public static void AddFoodStrings(string foodId, string name, string description, string recipeDescription = null)
	{
		Strings.Add(new string[2]
		{
			"STRINGS.ITEMS.FOOD." + foodId.ToUpperInvariant() + ".NAME",
			UI.FormatAsLink(name, foodId)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.ITEMS.FOOD." + foodId.ToUpperInvariant() + ".DESC",
			description
		});
		if (recipeDescription != null)
		{
			Strings.Add(new string[2]
			{
				"STRINGS.ITEMS.FOOD." + foodId.ToUpperInvariant() + ".RECIPEDESC",
				recipeDescription
			});
		}
	}

	public static void AddDiseaseStrings(string id, string name, string symptomps, string description)
	{
		Strings.Add(new string[2]
		{
			"STRINGS.DUPLICANTS.DISEASES." + id.ToUpperInvariant() + ".NAME",
			UI.FormatAsLink(name, id)
		});
		Strings.Add(new string[2]
		{
			"STRINGS.DUPLICANTS.DISEASES." + id.ToUpperInvariant() + ".DESCRIPTIVE_SYMPTOMS",
			symptomps
		});
		Strings.Add(new string[2]
		{
			"STRINGS.DUPLICANTS.DISEASES." + id.ToUpperInvariant() + ".DESC",
			description
		});
	}

	public unsafe static void Action(Tag speciesTag, string name, Dictionary<string, CodexEntry> results)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Expected O, but got Unknown
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Expected O, but got Unknown
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Expected O, but got Unknown
		List<GameObject> prefabsWithComponent = Assets.GetPrefabsWithComponent<CreatureBrain>();
		CodexEntry val = new CodexEntry("CREATURES", new List<ContentContainer>
		{
			new ContentContainer(new List<ICodexWidget>
			{
				(ICodexWidget)new CodexSpacer(),
				(ICodexWidget)new CodexSpacer()
			}, (ContentLayout)0)
		}, name);
		val.parentId = "CREATURES";
		CodexCache.AddEntry(((object)(*(Tag*)(&speciesTag))/*cast due to .constrained prefix*/).ToString(), val, (List<CategoryEntry>)null);
		results.Add(((object)(*(Tag*)(&speciesTag))/*cast due to .constrained prefix*/).ToString(), val);
		foreach (GameObject item in prefabsWithComponent)
		{
			if (StateMachineControllerExtensions.GetDef<Def>(item) != null)
			{
				continue;
			}
			Sprite val2 = null;
			GameObject val3 = Assets.TryGetPrefab(Tag.op_Implicit(((object)KPrefabIDExtensions.PrefabID(item)/*cast due to .constrained prefix*/).ToString() + "Baby"));
			if ((Object)(object)val3 != (Object)null)
			{
				val2 = Def.GetUISprite((object)val3, "ui", false).first;
			}
			CreatureBrain component = item.GetComponent<CreatureBrain>();
			if (component.species == speciesTag)
			{
				List<ContentContainer> list = new List<ContentContainer>();
				string symbolPrefix = component.symbolPrefix;
				Sprite first = Def.GetUISprite((object)item, symbolPrefix + "ui", false).first;
				if (Object.op_Implicit((Object)(object)val2))
				{
					Traverse.Create(typeof(CodexEntryGenerator)).Method("GenerateImageContainers", new Type[3]
					{
						typeof(Sprite[]),
						typeof(List<ContentContainer>),
						typeof(ContentLayout)
					}, (object[])null).GetValue(new object[3]
					{
						new Sprite[2] { first, val2 },
						list,
						(object)(ContentLayout)1
					});
				}
				else
				{
					list.Add(new ContentContainer(new List<ICodexWidget> { (ICodexWidget)new CodexImage(128, 128, first) }, (ContentLayout)0));
				}
				Traverse.Create(typeof(CodexEntryGenerator)).Method("GenerateCreatureDescriptionContainers", new Type[2]
				{
					typeof(GameObject),
					typeof(List<ContentContainer>)
				}, (object[])null).GetValue(new object[2] { item, list });
				val.subEntries.Add(new SubEntry(((object)KPrefabIDExtensions.PrefabID((Component)(object)component)/*cast due to .constrained prefix*/).ToString(), ((object)(*(Tag*)(&speciesTag))/*cast due to .constrained prefix*/).ToString(), list, KSelectableExtensions.GetProperName((Component)(object)component))
				{
					icon = first,
					iconColor = Color.white
				});
			}
		}
	}

	public static void LogKanimFrameInfo(string kanim)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		SgtLogger.l(kanim + " Symbols:");
		KAnimFileData data = Assets.GetAnim(HashedString.op_Implicit(kanim)).GetData();
		Symbol[] symbols = data.build.symbols;
		foreach (Symbol val in symbols)
		{
			SgtLogger.l($"Symbol: {val.path}, framecount: {val.numFrames}, lookupframes: {val.numLookupFrames}");
			if (val.frameLookup == null)
			{
				SgtLogger.l("Framelookup was null!");
			}
			else
			{
				SgtLogger.l("FrameLookup values: " + GeneralExtensions.Join<int>((IEnumerable<int>)val.frameLookup, (Func<int, string>)null, ", "));
			}
		}
	}

	public static void AddItemToTechnologyKanim(object tECH_ID, string spaceStationTechID, string name, string description, string kanim, string[] requiredDLcs)
	{
		throw new NotImplementedException();
	}
}
