using System.Collections.Generic;
using Database;

namespace FUtility;

public class ArtableUtil
{
	public const string READY = "AwaitingArting";

	public const string UGLY = "LookingUgly";

	public const string OKAY = "LookingOkay";

	public const string GREAT = "LookingGreat";

	public static void GetDefaultDecors(ArtableStages artableStages, string id, out int ugly, out int mediocre, out int great, int fallbackUgly = 5, int fallbackMediocre = 10, int fallbackGreat = 15)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		List<ArtableStage> prefabStages = artableStages.GetPrefabStages(Tag.op_Implicit(id));
		ugly = GetDefaultDecor("LookingGreat", prefabStages, fallbackUgly);
		mediocre = GetDefaultDecor("LookingOkay", prefabStages, fallbackMediocre);
		great = GetDefaultDecor("LookingUgly", prefabStages, fallbackGreat);
	}

	private static int GetDefaultDecor(string status, List<ArtableStage> stages, int defaultIfNotFound)
	{
		return stages.Find((ArtableStage s) => ((Resource)(s.statusItem?)).Id == status)?.decor ?? defaultIfNotFound;
	}

	public unsafe static string AddStage(ArtableStages stages, string buildingID, string ID, string anim, int decorBonus, ArtableStatusType status, string defaultAnim = "idle")
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Invalid comparison between Unknown and I4
		string modName = Log.modName;
		string text = modName + ".STRINGS.BUILDINGS.PREFABS." + buildingID.ToUpperInvariant() + ".VARIANT." + ID.ToUpperInvariant();
		string result = modName + "_" + buildingID + "_" + ID;
		StringEntry val = Strings.Get(text + ".NAME");
		StringEntry val2 = Strings.Get(text + ".DESCRIPTION");
		stages.Add(modName + "_" + buildingID + "_" + ID, StringEntry.op_Implicit(val), StringEntry.op_Implicit(val2), (PermitRarity)1, anim, defaultAnim, decorBonus, (int)status == 3, ((object)(*(ArtableStatusType*)(&status))/*cast due to .constrained prefix*/).ToString(), buildingID, "???", (string[])null, (string[])null);
		return result;
	}

	public unsafe static string AddStageLegacy(ArtableStages __instance, string buildingId, string kanimPrefix, string name, string description, string id, int decor, ArtableStatusType status, string animName = null)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		string text = buildingId + "_" + id;
		animName = animName ?? id;
		__instance.Add(text, name, description, (PermitRarity)1, kanimPrefix + "_" + id + "_kanim", animName, decor, (int)status > 1, ((object)(*(ArtableStatusType*)(&status))/*cast due to .constrained prefix*/).ToString(), buildingId, string.Empty, (string[])null, (string[])null);
		return text;
	}

	public static void MoveStages(List<ArtableStage> stages, Dictionary<string, string> targetStates, int uglyDecor, int okayDecor, int greatDecor)
	{
		if (stages == null)
		{
			Log.Warning("Invalid artable.");
		}
		if (targetStates == null || targetStates.Count == 0)
		{
			Log.Debug("no targetstates");
			return;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "Bad", "MarbleSculpture_Bad" },
			{ "Average", "MarbleSculpture_Average" },
			{ "Good1", "MarbleSculpture_Good1" },
			{ "Good2", "MarbleSculpture_Good2" },
			{ "Good3", "MarbleSculpture_Good3" }
		};
		Dictionary<string, ArtableStatusItem> dictionary2 = new Dictionary<string, ArtableStatusItem>();
		foreach (KeyValuePair<string, string> targetState in targetStates)
		{
			string value;
			string key = (dictionary.TryGetValue(targetState.Key, out value) ? value : targetState.Key);
			dictionary2[key] = GetStatusItem(targetState.Value);
		}
		foreach (ArtableStage stage in stages)
		{
			if (dictionary2.TryGetValue(stage.id, out var value2))
			{
				stage.statusItem = value2;
				switch (((Resource)value2).Id)
				{
				case "LookingUgly":
					stage.decor = uglyDecor;
					stage.cheerOnComplete = false;
					break;
				case "LookingOkay":
					stage.decor = okayDecor;
					stage.cheerOnComplete = false;
					break;
				case "LookingGreat":
					stage.decor = greatDecor;
					stage.cheerOnComplete = true;
					break;
				default:
					Log.Warning("Invalid quality tier");
					break;
				}
				Log.Debug("rearranged sculpture " + stage.id + " to " + ((Resource)stage.statusItem).Id);
			}
		}
	}

	private static ArtableStatusItem GetStatusItem(string value)
	{
		ArtableStatuses artableStatuses = Db.Get().ArtableStatuses;
		switch (value)
		{
		case "Bad":
		case "LookingUgly":
			return artableStatuses.LookingUgly;
		case "Okay":
		case "LookingOkay":
			return artableStatuses.LookingOkay;
		default:
			return artableStatuses.LookingGreat;
		}
	}
}
