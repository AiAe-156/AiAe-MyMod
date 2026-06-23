using System.Collections.Generic;
using Database;

namespace UtilLibs;

public class ArtHelper
{
	public const string READY = "AwaitingArting";

	public const string UGLY = "LookingUgly";

	public const string OKAY = "LookingOkay";

	public const string GREAT = "LookingGreat";

	public static string AddStatueStage(ArtableStages __instance, string buildingId, string statueId, string name, string description, string kanim, ArtableStatusType level, string animNameOverride = null, bool? cheer = null)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected I4, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Invalid comparison between Unknown and I4
		GetDefaultDecors(__instance, buildingId, out var ugly, out var mediocre, out var great);
		int decorValue = (level - 1) switch
		{
			1 => mediocre, 
			2 => ugly, 
			_ => great, 
		};
		if (!cheer.HasValue)
		{
			cheer = (int)level == 3;
		}
		return AddStatueStage(__instance, buildingId, statueId, name, description, kanim, level, decorValue, animNameOverride, cheer.Value);
	}

	public unsafe static string AddStatueStage(ArtableStages __instance, string buildingId, string statueId, string name, string description, string kanim, ArtableStatusType level, int decorValue, string animNameOverride = null, bool? cheer = null)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		if (!cheer.HasValue)
		{
			cheer = (int)level == 3;
		}
		if (Util.IsNullOrWhiteSpace(animNameOverride))
		{
			animNameOverride = statueId;
		}
		SgtLogger.l(statueId + ": anim name: " + animNameOverride);
		string text = buildingId + "_" + statueId;
		__instance.Add(text, name, description, (PermitRarity)1, kanim, animNameOverride, decorValue, cheer.Value, ((object)(*(ArtableStatusType*)(&level))/*cast due to .constrained prefix*/).ToString(), buildingId, "ui", (string[])null, (string[])null);
		return text;
	}

	public static void GetDefaultDecors(ArtableStages artableStages, string id, out int ugly, out int mediocre, out int great, int fallbackUgly = 5, int fallbackMediocre = 10, int fallbackGreat = 15)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		List<ArtableStage> prefabStages = artableStages.GetPrefabStages(Tag.op_Implicit(id));
		ugly = GetDefaultDecor("LookingGreat", prefabStages, fallbackUgly);
		mediocre = GetDefaultDecor("LookingOkay", prefabStages, fallbackMediocre);
		great = GetDefaultDecor("LookingUgly", prefabStages, fallbackGreat);
	}

	private static int GetDefaultDecor(string status, List<ArtableStage> stages, int defaultIfNotFound)
	{
		return stages.Find((ArtableStage s) => ((Resource)(s.statusItem?)).Id == status)?.decor ?? defaultIfNotFound;
	}

	public static void MoveStages(List<ArtableStage> stages, Dictionary<string, string> targetStates, int uglyDecor, int okayDecor, int greatDecor)
	{
		SgtLogger.l("moving stages");
		if (stages == null)
		{
			SgtLogger.l("Invalid artable.");
		}
		if (targetStates == null || targetStates.Count == 0)
		{
			SgtLogger.l("no targetstates");
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
			SgtLogger.l(stage.id);
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
					SgtLogger.warning("Invalid quality tier");
					break;
				}
				SgtLogger.l("rearranged sculpture " + stage.id + " to " + ((Resource)stage.statusItem).Id);
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
