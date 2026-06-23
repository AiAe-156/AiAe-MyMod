using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FUtility.Components;
using HarmonyLib;
using Klei.CustomSettings;
using STRINGS;
using UnityEngine;

namespace FUtility;

public class Utils
{
	public const char CENTER = 'O';

	public const char FILLED = 'X';

	private static FieldRef<KBatchedAnimController, KAnimLayering> kAnimLayering;

	private static FieldRef<KAnimLayering, KAnimControllerBase> foregroundController;

	public static string ModPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

	public static string AssetsPath => Path.Combine(ModPath, "assets");

	public static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

	public static float GetSFXVolume()
	{
		return KPlayerPrefs.GetFloat("Volume_SFX") * KPlayerPrefs.GetFloat("Volume_Master");
	}

	public static string ConfigPath(string modId)
	{
		return Path.Combine(Util.RootFolder(), "mods", "config", modId.ToLowerInvariant());
	}

	public static string ReplaceLastOccurrence(string source, string find, string replace)
	{
		int num = source.LastIndexOf(find);
		if (num == -1)
		{
			return source;
		}
		return source.Remove(num, find.Length).Insert(num, replace);
	}

	public static bool IsDlcMixedIn(string dlcId)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)CustomGameSettings.Instance == (Object)null)
		{
			Log.Debug("CustomGameSettings.Instance is null");
		}
		if (CustomGameSettings.Instance.MixingSettings.TryGetValue(dlcId, out var value))
		{
			return CustomGameSettings.Instance.GetCurrentMixingSettingLevel(dlcId) == ((ToggleSettingConfig)(DlcMixingSettingConfig)value).on_level;
		}
		if (!(dlcId == "EXPANSION1_ID"))
		{
			if (dlcId != null && dlcId.Length == 0)
			{
				return true;
			}
			return false;
		}
		return DlcManager.IsExpansion1Active();
	}

	public static float Bias(float x, float bias)
	{
		float num = Mathf.Pow(1f - bias, 3f);
		return x * num / (x * num - x + 1f);
	}

	public static float GetClampedGaussian(float stdDev, float mean)
	{
		return Mathf.Clamp(Util.GaussianRandom(0f, 1f) * stdDev / 3f + mean, 0f - stdDev, stdDev);
	}

	public static float GetClampedAssymetricGaussian(float stvDev, float mean)
	{
		return Mathf.Abs(GetClampedGaussian(stvDev, mean));
	}

	public static void FixFacadeLayers(GameObject go)
	{
		KBatchedAnimController component = go.GetComponent<KBatchedAnimController>();
		if (kAnimLayering == null)
		{
			kAnimLayering = AccessTools.FieldRefAccess<KBatchedAnimController, KAnimLayering>("layering");
		}
		if (foregroundController == null)
		{
			AccessTools.FieldRefAccess<KAnimLayering, KAnimControllerBase>("foregroundController");
		}
		if (kAnimLayering != null && foregroundController != null)
		{
			KAnimLayering val = kAnimLayering.Invoke(component);
			if (val != null)
			{
				KAnimControllerBase obj = foregroundController.Invoke(val);
				((KBatchedAnimController)((obj is KBatchedAnimController) ? obj : null)).SwapAnims(((KAnimControllerBase)component).AnimFiles);
				val.HideSymbols();
			}
		}
	}

	public static List<CellOffset> MakeCellOffsetsFromMap(bool fillCenter, params string[] pattern)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		List<CellOffset> list = new List<CellOffset>();
		for (int i = 0; i < pattern.Length; i++)
		{
			string text = pattern[i];
			for (int j = 0; j < text.Length; j++)
			{
				if (text[j] == 'O')
				{
					num = j;
					num2 = i;
					break;
				}
			}
		}
		for (int k = 0; k < pattern.Length; k++)
		{
			string text2 = pattern[k];
			for (int l = 0; l < text2.Length; l++)
			{
				if (text2[l] == 'X' || (fillCenter && text2[l] == 'O'))
				{
					list.Add(new CellOffset(l - num, k - num2));
				}
			}
		}
		return list;
	}

	public static CellOffset[] MakeCellOffsets(int width, int height, int offsetX = 0, int offsetY = 0)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		CellOffset[] array = (CellOffset[])(object)new CellOffset[width * height];
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				array[i * j + j] = new CellOffset(i + offsetX, j + offsetY);
			}
		}
		return array;
	}

	public static string FormatAsLink(string text, string id = null)
	{
		text = UI.StripLinkFormatting(text);
		if (Util.IsNullOrWhiteSpace(id))
		{
			id = text;
			id = id.Replace(" ", "");
		}
		id = id.ToUpperInvariant();
		id = id.Replace("_", "");
		return "<link=\"" + id + "\">" + text + "</link>";
	}

	public static string GetLinkAppropiateFormat(string link)
	{
		return UI.StripLinkFormatting(link).Replace(" ", "").Replace("_", "")
			.ToUpperInvariant();
	}

	public static GameObject Spawn(Tag tag, Vector3 position, SceneLayer sceneLayer = (SceneLayer)24, bool setActive = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Assets.GetPrefab(tag) == (Object)null)
		{
			return null;
		}
		GameObject obj = GameUtil.KInstantiate(Assets.GetPrefab(tag), position, sceneLayer, (string)null, 0);
		obj.SetActive(setActive);
		return obj;
	}

	public static GameObject Spawn(Tag tag, GameObject atGO, SceneLayer sceneLayer = (SceneLayer)24, bool setActive = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return Spawn(tag, atGO.transform.position, sceneLayer, setActive);
	}

	public static void YeetRandomly(GameObject go, bool onlyUp, float minDistance, float maxDistance, bool rotate, bool stopOnLand = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Vector2 insideUnitCircle = Random.insideUnitCircle;
		Vector2 normalized = ((Vector2)(ref insideUnitCircle)).normalized;
		if (onlyUp)
		{
			normalized.y = Mathf.Abs(normalized.y);
		}
		normalized += new Vector2(0f, Random.Range(0f, 1f));
		normalized *= Random.Range(minDistance, maxDistance);
		Yeet(go, minDistance, rotate, normalized, stopOnLand);
	}

	public static void YeetAtAngle(GameObject go, float angle, float distance, bool rotate, bool stopOnLand = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector2 vec = DegreeToVector2(angle) * distance;
		Yeet(go, distance, rotate, vec, stopOnLand);
	}

	private static void Yeet(GameObject go, float distance, bool rotate, Vector2 vec, bool stopOnLand = true)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (((KComponentManager<FallerComponent>)(object)GameComps.Fallers).Has((object)go))
		{
			((KGameObjectComponentManager<FallerComponent>)(object)GameComps.Fallers).Remove(go);
		}
		GameComps.Fallers.Add(go, vec);
		if (rotate)
		{
			Rotator rotator = EntityTemplateExtensions.AddOrGet<Rotator>(go);
			rotator.minDistance = distance;
			rotator.SetVec(vec);
			rotator.stopOnLand = stopOnLand;
		}
	}

	public static Vector2 RadianToVector2(float radian)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
	}

	public static Vector2 DegreeToVector2(float degree)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return RadianToVector2(degree * (MathF.PI / 180f));
	}

	public static ComplexRecipe AddRecipe(string fabricatorID, RecipeElement[] input, RecipeElement[] output, string desc, int sortOrder = 0, float time = 40f)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		return new ComplexRecipe(ComplexRecipeManager.MakeRecipeID(fabricatorID, (IList<RecipeElement>)input, (IList<RecipeElement>)output), input, output)
		{
			time = time,
			description = desc,
			nameDisplay = (RecipeNameDisplay)2,
			fabricators = new List<Tag> { TagManager.Create(fabricatorID) }
		};
	}

	public static ComplexRecipe AddRecipe(string fabricatorID, RecipeElement input, RecipeElement output, string desc, int sortOrder = 0, float time = 40f)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		RecipeElement[] array = (RecipeElement[])(object)new RecipeElement[1] { input };
		RecipeElement[] array2 = (RecipeElement[])(object)new RecipeElement[1] { output };
		return new ComplexRecipe(ComplexRecipeManager.MakeRecipeID(fabricatorID, (IList<RecipeElement>)array, (IList<RecipeElement>)array2), array, array2)
		{
			time = time,
			description = desc,
			nameDisplay = (RecipeNameDisplay)2,
			fabricators = new List<Tag> { TagManager.Create(fabricatorID) }
		};
	}

	public static void RegisterBatchTag(KAnimGroupFile kAnimGroupFile, int taghash, HashSet<HashedString> swaps)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		List<Group> data = kAnimGroupFile.GetData();
		Group val = KAnimGroupFile.GetGroup(new HashedString(taghash));
		data.RemoveAll((Group g) => swaps.Contains(g.animNames[0]));
		foreach (HashedString swap in swaps)
		{
			KAnimFile anim = Assets.GetAnim(swap);
			val.animFiles.Add(anim);
			val.animNames.Add(HashedString.op_Implicit(((Object)anim).name));
		}
	}
}
