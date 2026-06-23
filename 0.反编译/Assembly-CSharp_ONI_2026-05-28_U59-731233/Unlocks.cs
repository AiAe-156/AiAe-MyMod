using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using ProcGen;
using STRINGS;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/Unlocks")]
public class Unlocks : KMonoBehaviour
{
	private class MetaUnlockCategory
	{
		public string metaCollectionID;

		public string mesaCollectionID;

		public int mesaUnlockCount;

		public MetaUnlockCategory(string metaCollectionID, string mesaCollectionID, int mesaUnlockCount)
		{
			this.metaCollectionID = metaCollectionID;
			this.mesaCollectionID = mesaCollectionID;
			this.mesaUnlockCount = mesaUnlockCount;
		}
	}

	private const int FILE_IO_RETRY_ATTEMPTS = 5;

	private List<string> unlocked = new List<string>();

	private List<MetaUnlockCategory> MetaUnlockCategories = new List<MetaUnlockCategory>
	{
		new MetaUnlockCategory("dimensionalloreMeta", "dimensionallore", 4)
	};

	public Dictionary<string, string[]> lockCollections = new Dictionary<string, string[]>
	{
		{
			"emails",
			new string[25]
			{
				"email_thermodynamiclaws", "email_security2", "email_pens2", "email_atomiconrecruitment", "email_devonsblog", "email_researchgiant", "email_thejanitor", "email_newemployee", "email_timeoffapproved", "email_security3",
				"email_preliminarycalculations", "email_hollandsdog", "email_temporalbowupdate", "email_retemporalbowupdate", "email_memorychip", "email_arthistoryrequest", "email_AIcontrol", "email_AIcontrol2", "email_friendlyemail", "email_AIcontrol3",
				"email_AIcontrol4", "email_engineeringcandidate", "email_missingnotes", "email_journalistrequest", "email_journalistrequest2"
			}
		},
		{
			"dlc2emails",
			new string[5] { "email_newbaby", "email_cerestourism1", "email_cerestourism2", "email_voicemail", "email_expelled" }
		},
		{
			"dlc3emails",
			new string[1] { "email_ulti" }
		},
		{
			"dlc4emails",
			new string[2] { "notices_foreword", "notes_HigbySong" }
		},
		{
			"dlc5archivebuilding",
			new string[1] { "notes_marinea" }
		},
		{
			"dlc5emails",
			new string[1] { "email_ceasedesist" }
		},
		{
			"journals",
			new string[35]
			{
				"journal_timesarrowthoughts", "journal_A046_1", "journal_B835_1", "journal_sunflowerseeds", "journal_B327_1", "journal_B556_1", "journal_employeeprocessing", "journal_B327_2", "journal_A046_2", "journal_elliesbirthday1",
				"journal_B835_2", "journal_ants", "journal_pipedream", "journal_B556_2", "journal_movedrats", "journal_B835_3", "journal_A046_3", "journal_B556_3", "journal_B327_3", "journal_B835_4",
				"journal_cleanup", "journal_A046_4", "journal_B327_4", "journal_revisitednumbers", "journal_B556_4", "journal_B835_5", "journal_elliesbirthday2", "journal_B111_1", "journal_revisitednumbers2", "journal_timemusings",
				"journal_evil", "journal_timesorder", "journal_inspace", "journal_mysteryaward", "journal_courier"
			}
		},
		{
			"dlc3journals",
			new string[3] { "journal_potatobattery1", "journal_potatobattery2", "journal_potatobattery3" }
		},
		{
			"dlc4journals",
			new string[5] { "journal_expedition1", "journal_expedition2", "journal_expedition3", "journal_B824", "journal_incoming" }
		},
		{
			"dlc5journals",
			new string[4] { "journal_cakeandtea", "journal_hoax", "journal_printanomaly", "journal_fridgenote" }
		},
		{
			"researchnotes",
			new string[25]
			{
				"notes_clonedrats", "misc_dishbot", "notes_agriculture1", "notes_husbandry1", "notes_hibiscus3", "misc_newsecurity", "notes_husbandry2", "notes_agriculture2", "notes_geneticooze", "notes_agriculture3",
				"notes_husbandry3", "misc_casualfriday", "notes_memoryimplantation", "notes_husbandry4", "notes_agriculture4", "notes_neutronium", "misc_mailroometiquette", "notes_firstsuccess", "misc_reminder", "notes_neutroniumapplications",
				"notes_teleportation", "notes_AI", "misc_politerequest", "cryotank_warning", "misc_unattendedcultures"
			}
		},
		{
			"dlc2researchnotes",
			new string[1] { "notes_cleanup" }
		},
		{
			"dlc3researchnotes",
			new string[2] { "notes_talkshow", "notes_remoteworkstation" }
		},
		{
			"dlc4researchnotes",
			new string[1] { "notes_seepage" }
		},
		{
			"dimensionallore",
			new string[6] { "notes_clonedrabbits", "notes_clonedraccoons", "journal_movedrabbits", "journal_movedraccoons", "journal_strawberries", "journal_shrimp" }
		},
		{
			"dimensionalloreMeta",
			new string[1] { "log9" }
		},
		{
			"dlc2dimensionallore",
			new string[3] { "notes_tragicnews", "notes_tragicnews2", "notes_tragicnews3" }
		},
		{
			"dlc2archivebuilding",
			new string[1] { "notes_welcometoceres" }
		},
		{
			"dlc2geoplantinput",
			new string[1] { "notes_geoinputs" }
		},
		{
			"dlc2geoplantcomplete",
			new string[1] { "notes_earthquake" }
		},
		{
			"dlc4surfacepoi",
			new string[1] { "notice_surfacepoi" }
		},
		{
			"space",
			new string[4] { "display_spaceprop1", "notice_pilot", "journal_inspace", "notes_firstcolony" }
		},
		{
			"storytraits",
			new string[20]
			{
				"story_trait_critter_manipulator_initial", "story_trait_critter_manipulator_complete", "storytrait_crittermanipulator_workiversary", "story_trait_mega_brain_tank_initial", "story_trait_mega_brain_tank_competed", "story_trait_fossilhunt_initial", "story_trait_fossilhunt_poi1", "story_trait_fossilhunt_poi2", "story_trait_fossilhunt_poi3", "story_trait_fossilhunt_complete",
				"story_trait_morbrover_initial", "story_trait_morbrover_reveal", "story_trait_morbrover_reveal_lore", "story_trait_morbrover_complete", "story_trait_morbrover_complete_lore", "story_trait_morbrover_biobot", "story_trait_morbrover_locker", "story_trait_hijackheadquarters_mirror", "story_trait_hijackheadquarters_complete", "story_trait_hijackheadquarters_initial"
			}
		}
	};

	public Dictionary<int, string> cycleLocked = new Dictionary<int, string>
	{
		{ 0, "log1" },
		{ 3, "log2" },
		{ 15, "log3" },
		{ 1000, "log4" },
		{ 1500, "log4b" },
		{ 2000, "log5" },
		{ 2500, "log5b" },
		{ 3000, "log6" },
		{ 3500, "log6b" },
		{ 4000, "log7" },
		{ 4001, "log8" }
	};

	private static readonly EventSystem.IntraObjectHandler<Unlocks> OnLaunchRocketDelegate = new EventSystem.IntraObjectHandler<Unlocks>(delegate(Unlocks component, object data)
	{
		component.OnLaunchRocket(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Unlocks> OnDuplicantDiedDelegate = new EventSystem.IntraObjectHandler<Unlocks>(delegate(Unlocks component, object data)
	{
		component.OnDuplicantDied(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Unlocks> OnDiscoveredSpaceDelegate = new EventSystem.IntraObjectHandler<Unlocks>(delegate(Unlocks component, object data)
	{
		component.OnDiscoveredSpace(data);
	});

	private static string UnlocksFilename => System.IO.Path.Combine(Util.RootFolder(), "unlocks.json");

	protected override void OnPrefabInit()
	{
		LoadUnlocks();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		UnlockCycleCodexes();
		GameClock.Instance.Subscribe(631075836, OnNewDay);
		Subscribe(-1277991738, OnLaunchRocketDelegate);
		Subscribe(282337316, OnDuplicantDiedDelegate);
		Subscribe(-818188514, OnDiscoveredSpaceDelegate);
		Components.LiveMinionIdentities.OnAdd += OnNewDupe;
	}

	public bool IsUnlocked(string unlockID)
	{
		if (string.IsNullOrEmpty(unlockID))
		{
			return false;
		}
		if (DebugHandler.InstantBuildMode)
		{
			return true;
		}
		return unlocked.Contains(unlockID);
	}

	public IReadOnlyList<string> GetAllUnlockedIds()
	{
		return unlocked;
	}

	public void Lock(string unlockID)
	{
		if (unlocked.Contains(unlockID))
		{
			unlocked.Remove(unlockID);
			SaveUnlocks();
			Game.Instance.Trigger(1594320620, (object)unlockID);
		}
	}

	public void Unlock(string unlockID, bool shouldTryShowCodexNotification = true)
	{
		if (string.IsNullOrEmpty(unlockID))
		{
			DebugUtil.DevAssert(test: false, "Unlock called with null or empty string");
			return;
		}
		if (!unlocked.Contains(unlockID))
		{
			unlocked.Add(unlockID);
			SaveUnlocks();
			Game.Instance.Trigger(1594320620, (object)unlockID);
			if (shouldTryShowCodexNotification)
			{
				MessageNotification messageNotification = GenerateCodexUnlockNotification(unlockID);
				if (messageNotification != null)
				{
					GetComponent<Notifier>().Add(messageNotification);
				}
			}
		}
		EvalMetaCategories();
	}

	private void EvalMetaCategories()
	{
		foreach (MetaUnlockCategory metaUnlockCategory in MetaUnlockCategories)
		{
			string metaCollectionID = metaUnlockCategory.metaCollectionID;
			string mesaCollectionID = metaUnlockCategory.mesaCollectionID;
			int mesaUnlockCount = metaUnlockCategory.mesaUnlockCount;
			int count = 0;
			bool isCollectionReplaced = false;
			if (SaveLoader.Instance != null)
			{
				foreach (LoreCollectionOverride clusterUnlock in SaveLoader.Instance.ClusterLayout.clusterUnlocks)
				{
					if (EvaluateCollection(clusterUnlock))
					{
						break;
					}
				}
				foreach (string currentDlcMixingId in CustomGameSettings.Instance.GetCurrentDlcMixingIds())
				{
					DlcMixingSettings cachedDlcMixingSettings = SettingsCache.GetCachedDlcMixingSettings(currentDlcMixingId);
					if (cachedDlcMixingSettings == null)
					{
						continue;
					}
					foreach (LoreCollectionOverride globalLoreUnlock in cachedDlcMixingSettings.globalLoreUnlocks)
					{
						if (EvaluateCollection(globalLoreUnlock))
						{
							break;
						}
					}
				}
			}
			if (!isCollectionReplaced)
			{
				string[] array = lockCollections[mesaCollectionID];
				foreach (string unlockID in array)
				{
					if (IsUnlocked(unlockID))
					{
						count++;
					}
				}
			}
			if (count >= mesaUnlockCount)
			{
				UnlockNext(metaCollectionID);
			}
			bool EvaluateCollection(LoreCollectionOverride loreUnlock)
			{
				if (loreUnlock.id == mesaCollectionID)
				{
					string[] array2 = lockCollections[loreUnlock.collection];
					foreach (string unlockID2 in array2)
					{
						if (IsUnlocked(unlockID2))
						{
							count++;
						}
					}
					if (loreUnlock.orderRule == LoreCollectionOverride.OrderRule.Replace)
					{
						isCollectionReplaced = true;
						return true;
					}
				}
				return false;
			}
		}
	}

	private void SaveUnlocks()
	{
		if (!Directory.Exists(Util.RootFolder()))
		{
			Directory.CreateDirectory(Util.RootFolder());
		}
		string s = JsonConvert.SerializeObject(unlocked);
		bool flag = false;
		int num = 0;
		while (!flag && num < 5)
		{
			try
			{
				Thread.Sleep(num * 100);
				using FileStream fileStream = File.Open(UnlocksFilename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
				flag = true;
				ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
				byte[] bytes = aSCIIEncoding.GetBytes(s);
				fileStream.Write(bytes, 0, bytes.Length);
			}
			catch (Exception ex)
			{
				Debug.LogWarningFormat("Failed to save Unlocks attempt {0}: {1}", num + 1, ex.ToString());
			}
			num++;
		}
	}

	public void LoadUnlocks()
	{
		unlocked.Clear();
		if (!File.Exists(UnlocksFilename))
		{
			return;
		}
		string text = "";
		bool flag = false;
		int num = 0;
		while (!flag && num < 5)
		{
			try
			{
				Thread.Sleep(num * 100);
				using FileStream fileStream = File.Open(UnlocksFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				flag = true;
				ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
				byte[] array = new byte[fileStream.Length];
				if (fileStream.Read(array, 0, array.Length) == fileStream.Length)
				{
					text += aSCIIEncoding.GetString(array);
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarningFormat("Failed to load Unlocks attempt {0}: {1}", num + 1, ex.ToString());
			}
			num++;
		}
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		try
		{
			string[] array2 = JsonConvert.DeserializeObject<string[]>(text);
			string[] array3 = array2;
			foreach (string text2 in array3)
			{
				if (!string.IsNullOrEmpty(text2) && !unlocked.Contains(text2))
				{
					unlocked.Add(text2);
				}
			}
		}
		catch (Exception ex2)
		{
			Debug.LogErrorFormat("Error parsing unlocks file [{0}]: {1}", UnlocksFilename, ex2.ToString());
		}
	}

	private string GetNextClusterUnlock(string collectionID, out LoreCollectionOverride.OrderRule orderRule, bool randomize)
	{
		foreach (LoreCollectionOverride clusterUnlock in SaveLoader.Instance.ClusterLayout.clusterUnlocks)
		{
			if (clusterUnlock.id != collectionID)
			{
				continue;
			}
			if (!lockCollections.ContainsKey(collectionID))
			{
				DebugUtil.DevLogError("Lore collection '" + collectionID + "' is missing");
				orderRule = LoreCollectionOverride.OrderRule.Invalid;
				return null;
			}
			if (!lockCollections.ContainsKey(clusterUnlock.collection))
			{
				DebugUtil.DevLogError("Lore collection '" + clusterUnlock.collection + "' is missing but defined in the cluster file.");
				continue;
			}
			string[] array = lockCollections[clusterUnlock.collection];
			if (randomize)
			{
				array.Shuffle();
			}
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!IsUnlocked(text))
				{
					orderRule = clusterUnlock.orderRule;
					return text;
				}
			}
			if (clusterUnlock.orderRule != LoreCollectionOverride.OrderRule.Replace)
			{
				continue;
			}
			orderRule = clusterUnlock.orderRule;
			return null;
		}
		orderRule = LoreCollectionOverride.OrderRule.Invalid;
		return null;
	}

	private string GetNextGlobalDlcUnlock(string collectionID, out LoreCollectionOverride.OrderRule orderRule, bool randomize)
	{
		foreach (string currentDlcMixingId in CustomGameSettings.Instance.GetCurrentDlcMixingIds())
		{
			DlcMixingSettings cachedDlcMixingSettings = SettingsCache.GetCachedDlcMixingSettings(currentDlcMixingId);
			if (cachedDlcMixingSettings == null)
			{
				continue;
			}
			foreach (LoreCollectionOverride globalLoreUnlock in cachedDlcMixingSettings.globalLoreUnlocks)
			{
				if (globalLoreUnlock.id != collectionID)
				{
					continue;
				}
				if (!lockCollections.ContainsKey(collectionID))
				{
					DebugUtil.DevLogError("Lore collection '" + collectionID + "' is missing");
					orderRule = LoreCollectionOverride.OrderRule.Invalid;
					return null;
				}
				string[] array = lockCollections[globalLoreUnlock.collection];
				if (randomize)
				{
					array.Shuffle();
				}
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (!IsUnlocked(text))
					{
						orderRule = globalLoreUnlock.orderRule;
						return text;
					}
				}
				if (globalLoreUnlock.orderRule != LoreCollectionOverride.OrderRule.Replace)
				{
					continue;
				}
				orderRule = globalLoreUnlock.orderRule;
				return null;
			}
		}
		orderRule = LoreCollectionOverride.OrderRule.Invalid;
		return null;
	}

	public string UnlockNext(string collectionID, bool randomize = false)
	{
		if (SaveLoader.Instance != null)
		{
			string nextClusterUnlock = GetNextClusterUnlock(collectionID, out var orderRule, randomize);
			if (nextClusterUnlock != null && (orderRule == LoreCollectionOverride.OrderRule.Prepend || orderRule == LoreCollectionOverride.OrderRule.Replace))
			{
				Unlock(nextClusterUnlock);
				return nextClusterUnlock;
			}
			nextClusterUnlock = GetNextGlobalDlcUnlock(collectionID, out var orderRule2, randomize);
			if (nextClusterUnlock != null && (orderRule2 == LoreCollectionOverride.OrderRule.Prepend || orderRule2 == LoreCollectionOverride.OrderRule.Replace))
			{
				Unlock(nextClusterUnlock);
				return nextClusterUnlock;
			}
			if (orderRule == LoreCollectionOverride.OrderRule.Replace || orderRule2 == LoreCollectionOverride.OrderRule.Replace)
			{
				return null;
			}
		}
		string[] array = lockCollections[collectionID];
		if (randomize)
		{
			array.Shuffle();
		}
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (string.IsNullOrEmpty(text))
			{
				DebugUtil.DevAssertArgs(false, "Found null/empty string in Unlocks collection: ", collectionID);
			}
			else if (!IsUnlocked(text))
			{
				Unlock(text);
				return text;
			}
		}
		if (SaveLoader.Instance != null)
		{
			string nextClusterUnlock2 = GetNextClusterUnlock(collectionID, out var orderRule3, randomize);
			if (nextClusterUnlock2 != null && orderRule3 == LoreCollectionOverride.OrderRule.Append)
			{
				Unlock(nextClusterUnlock2);
				return nextClusterUnlock2;
			}
			nextClusterUnlock2 = GetNextGlobalDlcUnlock(collectionID, out orderRule3, randomize);
			if (nextClusterUnlock2 != null && orderRule3 == LoreCollectionOverride.OrderRule.Append)
			{
				Unlock(nextClusterUnlock2);
				return nextClusterUnlock2;
			}
		}
		return null;
	}

	private MessageNotification GenerateCodexUnlockNotification(string lockID)
	{
		string entryForLock = CodexCache.GetEntryForLock(lockID);
		if (string.IsNullOrEmpty(entryForLock))
		{
			return null;
		}
		string text = null;
		if (CodexCache.FindSubEntry(lockID) != null)
		{
			text = CodexCache.FindSubEntry(lockID).title;
		}
		else if (CodexCache.FindSubEntry(entryForLock) != null)
		{
			text = CodexCache.FindSubEntry(entryForLock).title;
		}
		else if (CodexCache.FindEntry(entryForLock) != null)
		{
			text = CodexCache.FindEntry(entryForLock).title;
		}
		MessageNotification messageNotification = null;
		string text2 = UI.FormatAsLink(Strings.Get(text), entryForLock);
		if (!string.IsNullOrEmpty(text))
		{
			ContentContainer contentContainer = CodexCache.FindEntry(entryForLock).contentContainers.Find((ContentContainer match) => match.lockID == lockID);
			if (contentContainer != null)
			{
				foreach (ICodexWidget item in contentContainer.content)
				{
					if (item is CodexText codexText)
					{
						text2 = text2 + "\n\n" + codexText.text;
					}
				}
			}
			CodexUnlockedMessage m = new CodexUnlockedMessage(lockID, text2);
			return new MessageNotification(m);
		}
		return null;
	}

	private void UnlockCycleCodexes()
	{
		foreach (KeyValuePair<int, string> item in cycleLocked)
		{
			if (GameClock.Instance.GetCycle() + 1 >= item.Key)
			{
				Unlock(item.Value);
			}
		}
	}

	private void OnNewDay(object data)
	{
		UnlockCycleCodexes();
	}

	private void OnLaunchRocket(object data)
	{
		Unlock("surfacebreach");
		Unlock("firstrocketlaunch");
	}

	private void OnDuplicantDied(object data)
	{
		Unlock("duplicantdeath");
		if (Components.LiveMinionIdentities.Count == 1)
		{
			Unlock("onedupeleft");
		}
	}

	private void OnNewDupe(MinionIdentity minion_identity)
	{
		if (Components.LiveMinionIdentities.Count >= Db.Get().Personalities.GetAll(onlyEnabledMinions: true, onlyStartingMinions: false).Count)
		{
			Unlock("fulldupecolony");
		}
	}

	private void OnDiscoveredSpace(object data)
	{
		Unlock("surfacebreach");
	}
}
