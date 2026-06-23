using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class BionicUpgradeComponentConfig : IMultiEntityConfig
{
	public enum BoosterType
	{
		Basic,
		Intermediate,
		Advanced,
		Sleep,
		Space,
		Special
	}

	public class BionicUpgradeData
	{
		private const string DEFAULT_ANIM_STATE_NAME = "object";

		public string stateMachineDescription;

		public string animStateName = "object";

		public string[] skillPerks = new string[0];

		public float WattageCost { get; private set; }

		public Func<StateMachine.Instance, StateMachine.Instance> stateMachine { get; private set; }

		public string uiAnimName => (animStateName == "object") ? "ui" : ("ui_" + animStateName);

		public string relatedTrait { get; private set; } = null;

		public BoosterType Booster { get; private set; }

		public bool isCarePackage { get; private set; }

		public BionicUpgradeData(float cost, string animStateName, string relatedTrait, BoosterType booster, Func<StateMachine.Instance, StateMachine.Instance> smi, string stateMachineDescription, bool isCarePackage, string[] skillPerkIds = null)
		{
			WattageCost = cost;
			stateMachine = smi;
			this.stateMachineDescription = stateMachineDescription;
			this.animStateName = animStateName;
			this.relatedTrait = relatedTrait;
			Booster = booster;
			this.isCarePackage = isCarePackage;
			skillPerks = skillPerkIds;
		}
	}

	public const string DEFAULT_ANIM_FILE_NAME = "upgrade_disc_kanim";

	public const string STARTING_TRAIT_PREFIX = "StartWith";

	public const string Booster_Dig1 = "Booster_Dig1";

	public const string Booster_Construct1 = "Booster_Construct1";

	public const string Booster_Dig2 = "Booster_Dig2";

	public const string Booster_Farm1 = "Booster_Farm1";

	public const string Booster_Ranch1 = "Booster_Ranch1";

	public const string Booster_Cook1 = "Booster_Cook1";

	public const string Booster_Art1 = "Booster_Art1";

	public const string Booster_Research1 = "Booster_Research1";

	public const string Booster_Research2 = "Booster_Research2";

	public const string Booster_Research3 = "Booster_Research3";

	public const string Booster_Pilot1 = "Booster_Pilot1";

	public const string Booster_PilotVanilla1 = "Booster_PilotVanilla1";

	public const string Booster_Suits1 = "Booster_Suits1";

	public const string Booster_Carry1 = "Booster_Carry1";

	public const string Booster_Op1 = "Booster_Op1";

	public const string Booster_Op2 = "Booster_Op2";

	public const string Booster_Medicine1 = "Booster_Medicine1";

	public const string Booster_Tidy1 = "Booster_Tidy1";

	public static List<string> BASIC_BOOSTERS = new List<string> { "Booster_Dig1", "Booster_Construct1", "Booster_Carry1", "Booster_Research1", "Booster_Medicine1" };

	public static Dictionary<Tag, BionicUpgradeData> UpgradesData = new Dictionary<Tag, BionicUpgradeData>();

	public static string GenerateTooltipForBooster(BionicUpgradeComponent booster)
	{
		string text = "<b>" + booster.GetProperName() + "</b>";
		InfoDescription component = booster.gameObject.GetComponent<InfoDescription>();
		if (component != null)
		{
			text = text + "\n" + component.description;
		}
		return text + "\n\n" + UpgradesData[booster.PrefabID()].stateMachineDescription;
	}

	public static Tag[] GetBoostersWithSkillPerk(string perkID)
	{
		return (from kvp in UpgradesData
			where kvp.Value.skillPerks.Contains(perkID)
			select kvp.Key).ToArray();
	}

	public AttributeModifier[] CreateBoosterModifiers(string name, Dictionary<string, float> attributes)
	{
		AttributeModifier[] array = new AttributeModifier[attributes.Count];
		string description = Strings.Get("STRINGS.ITEMS.BIONIC_BOOSTERS." + name.ToUpper() + ".NAME");
		int num = 0;
		foreach (KeyValuePair<string, float> attribute2 in attributes)
		{
			Klei.AI.Attribute attribute = Db.Get().Attributes.Get(attribute2.Key);
			array[num] = new AttributeModifier(attribute.Id, attribute2.Value, description);
			num++;
		}
		return array;
	}

	public List<GameObject> CreatePrefabs()
	{
		List<GameObject> list = new List<GameObject>();
		if (!DlcManager.IsContentSubscribed("DLC3_ID"))
		{
			return list;
		}
		string text = "Booster_Dig1";
		AttributeModifier[] array = CreateBoosterModifiers(text, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Digging.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		SkillPerk[] skillPerks = new SkillPerk[1] { Db.Get().SkillPerks.CanDigVeryFirm };
		AttributeModifier[] modifiers = array;
		BionicUpgrade_SkilledWorker.Def skill_worker_def = new BionicUpgrade_SkilledWorker.Def(text, Db.Get().Attributes.Digging.Id, modifiers, skillPerks, new string[2] { "hat_role_mining1", "hat_role_mining2" });
		list.Add(CreateNewUpgradeComponent(text, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def), skill_worker_def.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "basic_excavation_0", SimHashes.Creature, null, BoosterType.Basic, isStartingBooster: true, isCarePackage: true, skillPerks));
		string text2 = "Booster_Construct1";
		AttributeModifier[] array2 = CreateBoosterModifiers(text2, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Construction.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		SkillPerk[] skillPerks2 = new SkillPerk[1] { Db.Get().SkillPerks.CanDemolish };
		modifiers = array2;
		BionicUpgrade_SkilledWorker.Def skill_worker_def2 = new BionicUpgrade_SkilledWorker.Def(text2, Db.Get().Attributes.Construction.Id, modifiers, skillPerks2, new string[3] { "hat_role_building1", "hat_role_building2", "hat_role_building3" });
		list.Add(CreateNewUpgradeComponent(text2, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def2), skill_worker_def2.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "basic_construction_0", SimHashes.Creature, null, BoosterType.Basic, isStartingBooster: true, isCarePackage: true, skillPerks2));
		string text3 = "Booster_Carry1";
		AttributeModifier[] array3 = CreateBoosterModifiers(text3, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Strength.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		SkillPerk[] skillPerks3 = new SkillPerk[1] { Db.Get().SkillPerks.IncreasedCarryBionics };
		modifiers = array3;
		BionicUpgrade_SkilledWorker.Def skill_worker_def3 = new BionicUpgrade_SkilledWorker.Def(text3, Db.Get().Attributes.Athletics.Id, modifiers, skillPerks3, new string[2] { "hat_role_hauling1", "hat_role_hauling2" });
		list.Add(CreateNewUpgradeComponent(text3, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def3), skill_worker_def3.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "basic_strength_0", SimHashes.Creature, null, BoosterType.Basic, isStartingBooster: false, isCarePackage: true, skillPerks3));
		string text4 = "Booster_Research1";
		AttributeModifier[] array4 = CreateBoosterModifiers(text4, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Learning.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		SkillPerk[] skillPerks4 = new SkillPerk[4]
		{
			Db.Get().SkillPerks.AllowAdvancedResearch,
			Db.Get().SkillPerks.CanStudyWorldObjects,
			Db.Get().SkillPerks.AllowGeyserTuning,
			Db.Get().SkillPerks.AllowChemistry
		};
		modifiers = array4;
		BionicUpgrade_SkilledWorker.Def skill_worker_def4 = new BionicUpgrade_SkilledWorker.Def(text4, Db.Get().Attributes.Learning.Id, modifiers, skillPerks4, new string[2] { "hat_role_research1", "hat_role_research2" });
		list.Add(CreateNewUpgradeComponent(text4, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def4), skill_worker_def4.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "science_4", SimHashes.Creature, null, BoosterType.Basic, isStartingBooster: false, isCarePackage: true, skillPerks4));
		string text5 = "Booster_Medicine1";
		AttributeModifier[] array5 = CreateBoosterModifiers(text5, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Caring.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		SkillPerk[] skillPerks5 = new SkillPerk[3]
		{
			Db.Get().SkillPerks.CanCompound,
			Db.Get().SkillPerks.CanDoctor,
			Db.Get().SkillPerks.CanAdvancedMedicine
		};
		modifiers = array5;
		BionicUpgrade_SkilledWorker.Def skill_worker_def5 = new BionicUpgrade_SkilledWorker.Def(text5, Db.Get().Attributes.DoctoredLevel.Id, modifiers, skillPerks5, new string[3] { "hat_role_medicalaid1", "hat_role_medicalaid2", "hat_role_medicalaid3" });
		list.Add(CreateNewUpgradeComponent(text5, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def5), skill_worker_def5.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "medicine_0", SimHashes.Creature, null, BoosterType.Basic, isStartingBooster: true, isCarePackage: true, skillPerks5));
		string text6 = "Booster_Dig2";
		SkillPerk[] skillPerks6 = ((!DlcManager.IsExpansion1Active()) ? new SkillPerk[2]
		{
			Db.Get().SkillPerks.CanDigNearlyImpenetrable,
			Db.Get().SkillPerks.CanDigSuperDuperHard
		} : new SkillPerk[3]
		{
			Db.Get().SkillPerks.CanDigNearlyImpenetrable,
			Db.Get().SkillPerks.CanDigSuperDuperHard,
			Db.Get().SkillPerks.CanDigRadioactiveMaterials
		});
		AttributeModifier[] array6 = CreateBoosterModifiers(text6, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Digging.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		string[] hats = ((!DlcManager.IsExpansion1Active()) ? new string[1] { "hat_role_mining3" } : new string[2] { "hat_role_mining3", "hat_role_mining4" });
		modifiers = array6;
		BionicUpgrade_SkilledWorker.Def skill_worker_def6 = new BionicUpgrade_SkilledWorker.Def(text6, Db.Get().Attributes.Digging.Id, modifiers, skillPerks6, hats);
		list.Add(CreateNewUpgradeComponent(text6, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def6), skill_worker_def6.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "excavation_1", SimHashes.Creature, null, BoosterType.Intermediate, isStartingBooster: true, isCarePackage: true, skillPerks6));
		string text7 = "Booster_Farm1";
		List<SkillPerk> list2 = new List<SkillPerk>
		{
			Db.Get().SkillPerks.CanFarmTinker,
			Db.Get().SkillPerks.CanFarmStation,
			Db.Get().SkillPerks.CanSalvagePlantFiber
		};
		if (DlcManager.IsContentSubscribed("DLC5_ID"))
		{
			list2.Add(Db.Get().SkillPerks.CanFarmClams);
		}
		if (DlcManager.IsExpansion1Active())
		{
			list2.Add(Db.Get().SkillPerks.CanIdentifyMutantSeeds);
		}
		AttributeModifier[] array7 = CreateBoosterModifiers(text7, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Botanist.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		modifiers = array7;
		BionicUpgrade_SkilledWorker.Def skill_worker_def7 = new BionicUpgrade_SkilledWorker.Def(text7, Db.Get().Attributes.Botanist.Id, modifiers, list2.ToArray(), new string[3] { "hat_role_farming1", "hat_role_farming2", "hat_role_farming3" });
		list.Add(CreateNewUpgradeComponent(text7, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def7), skill_worker_def7.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "agriculture_0", SimHashes.Creature, null, BoosterType.Intermediate, isStartingBooster: true, isCarePackage: false, list2.ToArray()));
		string text8 = "Booster_Ranch1";
		AttributeModifier[] array8 = CreateBoosterModifiers(text8, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Ranching.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		SkillPerk[] skillPerks7 = new SkillPerk[3]
		{
			Db.Get().SkillPerks.CanWrangleCreatures,
			Db.Get().SkillPerks.CanUseRanchStation,
			Db.Get().SkillPerks.CanUseMilkingStation
		};
		modifiers = array8;
		BionicUpgrade_SkilledWorker.Def skill_worker_def8 = new BionicUpgrade_SkilledWorker.Def(text8, Db.Get().Attributes.Ranching.Id, modifiers, skillPerks7, new string[2] { "hat_role_rancher1", "hat_role_rancher2" });
		list.Add(CreateNewUpgradeComponent(text8, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def8), skill_worker_def8.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "ranching_0", SimHashes.Creature, null, BoosterType.Intermediate, isStartingBooster: true, isCarePackage: false, skillPerks7));
		string text9 = "Booster_Cook1";
		AttributeModifier[] array9 = CreateBoosterModifiers(text9, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Cooking.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		SkillPerk[] skillPerks8 = new SkillPerk[5]
		{
			Db.Get().SkillPerks.CanElectricGrill,
			Db.Get().SkillPerks.CanDeepFry,
			Db.Get().SkillPerks.CanGasRange,
			Db.Get().SkillPerks.CanSushiBar,
			Db.Get().SkillPerks.CanSpiceGrinder
		};
		modifiers = array9;
		BionicUpgrade_SkilledWorker.Def skill_worker_def9 = new BionicUpgrade_SkilledWorker.Def(text9, Db.Get().Attributes.Cooking.Id, modifiers, skillPerks8, new string[2] { "hat_role_cooking1", "hat_role_cooking2" });
		list.Add(CreateNewUpgradeComponent(text9, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def9), skill_worker_def9.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "cooking_0", SimHashes.Creature, null, BoosterType.Intermediate, isStartingBooster: true, isCarePackage: true, skillPerks8));
		string text10 = "Booster_Art1";
		List<SkillPerk> list3 = new List<SkillPerk>
		{
			Db.Get().SkillPerks.CanArt,
			Db.Get().SkillPerks.CanClothingAlteration,
			Db.Get().SkillPerks.CanArtGreat
		};
		if (DlcManager.FeatureClusterSpaceEnabled())
		{
			list3.Add(Db.Get().SkillPerks.CanStudyArtifact);
		}
		AttributeModifier[] array10 = CreateBoosterModifiers(text10, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Art.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		modifiers = array10;
		BionicUpgrade_SkilledWorker.Def skill_worker_def10 = new BionicUpgrade_SkilledWorker.Def(text10, Db.Get().Attributes.Art.Id, modifiers, list3.ToArray(), new string[3] { "hat_role_art1", "hat_role_art2", "hat_role_art3" });
		list.Add(CreateNewUpgradeComponent(text10, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def10), skill_worker_def10.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "creativity_0", SimHashes.Creature, null, BoosterType.Intermediate, isStartingBooster: true, isCarePackage: false, list3.ToArray()));
		string text11 = "Booster_Research2";
		List<SkillPerk> list4 = new List<SkillPerk> { Db.Get().SkillPerks.CanMissionControl };
		if (DlcManager.FeatureClusterSpaceEnabled())
		{
			list4.Add(Db.Get().SkillPerks.CanUseClusterTelescope);
			list4.Add(Db.Get().SkillPerks.AllowOrbitalResearch);
		}
		else
		{
			list4.Add(Db.Get().SkillPerks.AllowInterstellarResearch);
		}
		string[] hats2 = ((!DlcManager.IsExpansion1Active()) ? new string[1] { "hat_role_research3" } : new string[2] { "hat_role_research3", "hat_role_research4" });
		AttributeModifier[] array11 = CreateBoosterModifiers(text11, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Learning.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		modifiers = array11;
		BionicUpgrade_SkilledWorker.Def skill_worker_def11 = new BionicUpgrade_SkilledWorker.Def(text11, Db.Get().Attributes.Learning.Id, modifiers, list4.ToArray(), hats2);
		list.Add(CreateNewUpgradeComponent(text11, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def11), skill_worker_def11.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "science_2", SimHashes.Creature, null, BoosterType.Intermediate, isStartingBooster: false, isCarePackage: false, list4.ToArray()));
		if (DlcManager.IsExpansion1Active())
		{
			string text12 = "Booster_Research3";
			AttributeModifier[] array12 = CreateBoosterModifiers(text12, new Dictionary<string, float>
			{
				{
					Db.Get().Attributes.Learning.Id,
					5f
				},
				{
					Db.Get().Attributes.Athletics.Id,
					2f
				}
			});
			SkillPerk[] skillPerks9 = new SkillPerk[1] { Db.Get().SkillPerks.AllowNuclearResearch };
			modifiers = array12;
			BionicUpgrade_SkilledWorker.Def skill_worker_def12 = new BionicUpgrade_SkilledWorker.Def(text12, Db.Get().Attributes.Learning.Id, modifiers, skillPerks9, new string[1] { "hat_role_research5" });
			list.Add(CreateNewUpgradeComponent(text12, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def12), skill_worker_def12.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "science_3", SimHashes.Creature, null, BoosterType.Intermediate, isStartingBooster: false, isCarePackage: false, skillPerks9));
		}
		if (DlcManager.IsExpansion1Active())
		{
			string text13 = "Booster_Pilot1";
			AttributeModifier[] array13 = CreateBoosterModifiers(text13, new Dictionary<string, float>
			{
				{
					Db.Get().Attributes.SpaceNavigation.Id,
					5f
				},
				{
					Db.Get().Attributes.Athletics.Id,
					2f
				}
			});
			SkillPerk[] skillPerks10 = new SkillPerk[1] { Db.Get().SkillPerks.CanUseRocketControlStation };
			modifiers = array13;
			BionicUpgrade_SkilledWorker.Def skill_worker_def13 = new BionicUpgrade_SkilledWorker.Def(text13, Db.Get().Attributes.SpaceNavigation.Id, modifiers, skillPerks10, new string[2] { "hat_role_astronaut1", "hat_role_astronaut2" });
			list.Add(CreateNewUpgradeComponent(text13, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def13), skill_worker_def13.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "piloting_0", SimHashes.Creature, null, BoosterType.Intermediate, isStartingBooster: false, isCarePackage: false, skillPerks10));
		}
		if (DlcManager.IsPureVanilla())
		{
			string text14 = "Booster_PilotVanilla1";
			AttributeModifier[] modifiers2 = CreateBoosterModifiers(text14, new Dictionary<string, float> { 
			{
				Db.Get().Attributes.Athletics.Id,
				3f
			} });
			SkillPerk[] skillPerks11 = new SkillPerk[1] { Db.Get().SkillPerks.CanUseRockets };
			BionicUpgrade_SkilledWorker.Def skill_worker_def14 = new BionicUpgrade_SkilledWorker.Def(text14, null, modifiers2, skillPerks11, new string[2] { "hat_role_astronaut1", "hat_role_astronaut2" });
			list.Add(CreateNewUpgradeComponent(text14, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def14), skill_worker_def14.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "piloting_vanilla_0", SimHashes.Creature, null, BoosterType.Intermediate, isStartingBooster: false, isCarePackage: false, skillPerks11));
		}
		string text15 = "Booster_Suits1";
		AttributeModifier[] array14 = CreateBoosterModifiers(text15, new Dictionary<string, float> { 
		{
			Db.Get().Attributes.Athletics.Id,
			5f
		} });
		SkillPerk[] skillPerks12 = new SkillPerk[2]
		{
			Db.Get().SkillPerks.ExosuitDurability,
			Db.Get().SkillPerks.ExosuitExpertise
		};
		modifiers = array14;
		BionicUpgrade_SkilledWorker.Def skill_worker_def15 = new BionicUpgrade_SkilledWorker.Def(text15, Db.Get().Attributes.Athletics.Id, modifiers, skillPerks12, new string[2] { "hat_role_suits1", "hat_role_suits2" });
		list.Add(CreateNewUpgradeComponent(text15, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def15), skill_worker_def15.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "suits_0", SimHashes.Creature, null, BoosterType.Intermediate, isStartingBooster: true, isCarePackage: false, skillPerks12));
		string text16 = "Booster_Tidy1";
		AttributeModifier[] array15 = CreateBoosterModifiers(text16, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Strength.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		SkillPerk[] skillPerks13 = new SkillPerk[2]
		{
			Db.Get().SkillPerks.CanDoPlumbing,
			Db.Get().SkillPerks.CanMakeMissiles
		};
		modifiers = array15;
		BionicUpgrade_SkilledWorker.Def skill_worker_def16 = new BionicUpgrade_SkilledWorker.Def(text16, Db.Get().Attributes.Strength.Id, modifiers, skillPerks13, new string[3] { "hat_role_basekeeping1", "hat_role_basekeeping2", "hat_role_pyrotechnics" });
		list.Add(CreateNewUpgradeComponent(text16, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def16), skill_worker_def16.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "tidy_0", SimHashes.Creature, null, BoosterType.Intermediate, isStartingBooster: false, isCarePackage: false, skillPerks13));
		string text17 = "Booster_Op1";
		AttributeModifier[] array16 = CreateBoosterModifiers(text17, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Machinery.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		SkillPerk[] skillPerks14 = new SkillPerk[2]
		{
			Db.Get().SkillPerks.CanPowerTinker,
			Db.Get().SkillPerks.CanCraftElectronics
		};
		modifiers = array16;
		BionicUpgrade_SkilledWorker.Def skill_worker_def17 = new BionicUpgrade_SkilledWorker.Def(text17, Db.Get().Attributes.Machinery.Id, modifiers, skillPerks14, new string[2] { "hat_role_technicals1", "hat_role_technicals2" });
		list.Add(CreateNewUpgradeComponent(text17, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def17), skill_worker_def17.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "machinery_0", SimHashes.Creature, null, BoosterType.Intermediate, isStartingBooster: true, isCarePackage: false, skillPerks14));
		string text18 = "Booster_Op2";
		AttributeModifier[] array17 = CreateBoosterModifiers(text18, new Dictionary<string, float>
		{
			{
				Db.Get().Attributes.Machinery.Id,
				5f
			},
			{
				Db.Get().Attributes.Athletics.Id,
				2f
			}
		});
		SkillPerk[] skillPerks15 = new SkillPerk[1] { Db.Get().SkillPerks.ConveyorBuild };
		modifiers = array17;
		BionicUpgrade_SkilledWorker.Def skill_worker_def18 = new BionicUpgrade_SkilledWorker.Def(text18, Db.Get().Attributes.Machinery.Id, modifiers, skillPerks15, new string[1] { "hat_role_engineering1" });
		list.Add(CreateNewUpgradeComponent(text18, null, null, 0f, (StateMachine.Instance smi) => new BionicUpgrade_SkilledWorker.Instance(smi.GetMaster(), skill_worker_def18), skill_worker_def18.GetDescription() + "\n\n" + string.Format(STRINGS.ITEMS.BIONIC_BOOSTERS.FABRICATION_SOURCE, STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.NAME), DlcManager.DLC3, "upgrade_disc_kanim", "machinery_1", SimHashes.Creature, null, BoosterType.Advanced, isStartingBooster: false, isCarePackage: false, skillPerks15));
		list.RemoveAll((GameObject t) => t == null);
		return list;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}

	public static Tag GetBionicUpgradePrefabIDWithTraitID(string traitID)
	{
		foreach (Tag key in UpgradesData.Keys)
		{
			BionicUpgradeData bionicUpgradeData = UpgradesData[key];
			if (bionicUpgradeData.relatedTrait != null && bionicUpgradeData.relatedTrait == traitID)
			{
				return key;
			}
		}
		return Tag.Invalid;
	}

	public static GameObject CreateNewUpgradeComponent(string id, string name = null, string desc = null, float wattageCost = 0f, Func<StateMachine.Instance, StateMachine.Instance> stateMachine = null, string sm_description = "", string[] dlcIDs = null, string animFile = "upgrade_disc_kanim", string animStateName = "object", SimHashes element = SimHashes.Creature, string craftTechUnlockID = null, BoosterType booster = BoosterType.Basic, bool isStartingBooster = false, bool isCarePackage = false, SkillPerk[] skillPerks = null)
	{
		if (!DlcManager.IsAllContentSubscribed(dlcIDs))
		{
			return null;
		}
		if (name == null)
		{
			name = Strings.Get("STRINGS.ITEMS.BIONIC_BOOSTERS." + id.ToUpper() + ".NAME");
		}
		if (desc == null)
		{
			desc = Strings.Get("STRINGS.ITEMS.BIONIC_BOOSTERS." + id.ToUpper() + ".DESC");
		}
		string ID = id;
		TechItem techItem = new TechItem(ID, Db.Get().TechItems, Strings.Get("STRINGS.RESEARCH.OTHER_TECH_ITEMS." + id.ToUpper() + ".NAME"), Strings.Get("STRINGS.RESEARCH.OTHER_TECH_ITEMS." + id.ToUpper() + ".DESC"), (string a, bool b) => Def.GetUISprite(Assets.GetPrefab(ID)).first, craftTechUnlockID, DlcManager.DLC3);
		if (!craftTechUnlockID.IsNullOrWhiteSpace())
		{
			Db.Get().Techs.Get(craftTechUnlockID).AddUnlockedItemIDs(techItem.Id);
		}
		GameObject gameObject = EntityTemplates.CreateLooseEntity(ID, name, desc, 25f, unitMass: true, Assets.GetAnim(animFile), animStateName, Grid.SceneLayer.Ore, EntityTemplates.CollisionShape.RECTANGLE, 0.6f, 0.45f, isPickupable: true, SORTORDER.ARTIFACTS, element, new List<Tag>
		{
			GameTags.BionicUpgrade,
			GameTags.MiscPickupable,
			GameTags.NotRoomAssignable
		});
		OccupyArea occupyArea = gameObject.AddOrGet<OccupyArea>();
		occupyArea.SetCellOffsets(EntityTemplates.GenerateOffsets(1, 1));
		DecorProvider decorProvider = gameObject.AddOrGet<DecorProvider>();
		decorProvider.SetValues(DECOR.NONE);
		decorProvider.overrideName = gameObject.GetProperName();
		BionicUpgradeComponent bionicUpgradeComponent = gameObject.AddOrGet<BionicUpgradeComponent>();
		bionicUpgradeComponent.slotID = Db.Get().AssignableSlots.BionicUpgrade.Id;
		gameObject.AddOrGet<KSelectable>();
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.PedestalDisplayable);
		component.requiredDlcIds = dlcIDs;
		component.SetUnityEditorConfigOverride("BionicUpgradeComponentConfig");
		string text = null;
		if (isStartingBooster)
		{
			text = "StartWith" + id;
			DUPLICANTSTATS.BIONICUPGRADETRAITS.Add(new DUPLICANTSTATS.TraitVal
			{
				id = text,
				requiredDlcIds = DlcManager.DLC3
			});
			TraitUtil.CreateBionicUpgradeTrait(text, sm_description)();
		}
		UpgradesData.Add(component.PrefabTag, new BionicUpgradeData(wattageCost, animStateName, text, booster, stateMachine, sm_description, isCarePackage, skillPerks.Select((SkillPerk perk) => perk.Id).ToArray()));
		if (!BASIC_BOOSTERS.Contains(ID))
		{
			ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement("PowerStationTools", 8f)
			};
			ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement(ID.ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
			};
			string id2 = ComplexRecipeManager.MakeRecipeID(ID, array, array2);
			ComplexRecipe complexRecipe = new ComplexRecipe(id2, array, array2)
			{
				time = INDUSTRIAL.RECIPES.STANDARD_FABRICATION_TIME,
				description = string.Format(STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.BIONIC_COMPONENT_RECIPE_DESC, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.POWER_STATION_TOOLS.NAME, name) + "\n\n" + UpgradesData[ID].stateMachineDescription,
				nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
				fabricators = new List<Tag> { "AdvancedCraftingTable" },
				requiredTech = craftTechUnlockID,
				sortOrder = 3
			};
			complexRecipe.runTimeDescription = () => GetColonyBoosterAssignmentString(ID);
		}
		else
		{
			ComplexRecipe.RecipeElement[] array3 = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement("PowerStationTools", (booster == BoosterType.Basic) ? 2 : 4, inheritElement: true)
			};
			ComplexRecipe.RecipeElement[] array4 = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement(ID, 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
			};
			string id3 = ComplexRecipeManager.MakeRecipeID(ID, array3, array4);
			ComplexRecipe complexRecipe2 = new ComplexRecipe(id3, array3, array4, DlcManager.DLC3)
			{
				time = INDUSTRIAL.RECIPES.STANDARD_FABRICATION_TIME * 2f,
				description = string.Format(STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.BIONIC_COMPONENT_RECIPE_DESC, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.POWER_STATION_TOOLS.NAME, name) + "\n\n" + UpgradesData[ID].stateMachineDescription,
				nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
				fabricators = new List<Tag> { "CraftingTable" },
				sortOrder = 1
			};
			complexRecipe2.runTimeDescription = () => GetColonyBoosterAssignmentString(ID);
		}
		return gameObject;
	}

	public static string GetColonyBoosterAssignmentString(string boosterID)
	{
		int num = 0;
		foreach (MinionIdentity worldItem in Components.LiveMinionIdentities.GetWorldItems(ClusterManager.Instance.activeWorldId))
		{
			if (!worldItem.HasTag(GameTags.Minions.Models.Bionic))
			{
				continue;
			}
			BionicUpgradesMonitor.Instance sMI = worldItem.GetSMI<BionicUpgradesMonitor.Instance>();
			if (sMI == null || sMI.upgradeComponentSlots == null)
			{
				continue;
			}
			BionicUpgradesMonitor.UpgradeComponentSlot[] upgradeComponentSlots = sMI.upgradeComponentSlots;
			foreach (BionicUpgradesMonitor.UpgradeComponentSlot upgradeComponentSlot in upgradeComponentSlots)
			{
				if (upgradeComponentSlot.HasUpgradeComponentAssigned && upgradeComponentSlot.assignedUpgradeComponent.PrefabID() == boosterID)
				{
					num++;
					break;
				}
			}
		}
		if (num == 0)
		{
			return string.Format(STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.COLONY_HAS_BOOSTER_ASSIGNED_NONE);
		}
		return string.Format(STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.COLONY_HAS_BOOSTER_ASSIGNED_COUNT, num);
	}
}
