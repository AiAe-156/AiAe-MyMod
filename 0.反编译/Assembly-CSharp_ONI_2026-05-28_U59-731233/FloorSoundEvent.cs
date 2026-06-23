using System.Diagnostics;
using FMOD.Studio;
using UnityEngine;

[DebuggerDisplay("{Name}")]
public class FloorSoundEvent : SoundEvent
{
	public static float IDLE_WALKING_VOLUME_REDUCTION = 0.55f;

	public FloorSoundEvent(string file_name, string sound_name, int frame)
		: base(file_name, sound_name, frame, do_load: false, is_looping: false, SoundEvent.IGNORE_INTERVAL, is_dynamic: true)
	{
		base.noiseValues = SoundEventVolumeCache.instance.GetVolume("FloorSoundEvent", sound_name);
	}

	public override void PlaySound(AnimEventManager.EventPlayerData behaviour)
	{
		Vector3 pos = behaviour.position;
		KBatchedAnimController controller = behaviour.controller;
		if (controller != null)
		{
			pos = controller.GetPivotSymbolPosition();
		}
		int num = Grid.PosToCell(pos);
		int cell = Grid.CellBelow(num);
		if (!Grid.IsValidCell(cell))
		{
			return;
		}
		string audioCategory = GetAudioCategory(cell);
		string text = StringFormatter.Combine(audioCategory, "_", base.name);
		string text2 = GlobalAssets.GetSound(text, force_no_warning: true);
		if (text2 == null)
		{
			text = StringFormatter.Combine("Rock_", base.name);
			text2 = GlobalAssets.GetSound(text, force_no_warning: true);
			if (text2 == null)
			{
				text = base.name;
				text2 = GlobalAssets.GetSound(text, force_no_warning: true);
			}
		}
		GameObject gameObject = behaviour.controller.gameObject;
		MinionIdentity minionIdentity = null;
		minionIdentity = gameObject.GetComponent<MinionIdentity>();
		base.objectIsSelectedAndVisible = SoundEvent.ObjectIsSelectedAndVisible(gameObject);
		if (SoundEvent.IsLowPrioritySound(text2) && !base.objectIsSelectedAndVisible)
		{
			return;
		}
		pos = SoundEvent.GetCameraScaledPosition(pos);
		pos.z = 0f;
		if (base.objectIsSelectedAndVisible)
		{
			pos = SoundEvent.AudioHighlightListenerPosition(pos);
		}
		if (Grid.Element == null)
		{
			return;
		}
		bool isLiquid = Grid.Element[num].IsLiquid;
		float num2 = 0f;
		bool flag = Grid.IsSubstantialLiquid(Grid.CellAbove(num));
		if (isLiquid)
		{
			num2 = SoundUtil.GetLiquidDepth(num);
			string text3 = GlobalAssets.GetSound(flag ? "uw_footstep" : "Liquid_footstep");
			if (text3 != null && (base.objectIsSelectedAndVisible || SoundEvent.ShouldPlaySound(behaviour.controller, text3, base.looping, isDynamic)))
			{
				FMOD.Studio.EventInstance instance = SoundEvent.BeginOneShot(text3, pos, SoundEvent.GetVolume(base.objectIsSelectedAndVisible));
				if (num2 > 0f && !flag)
				{
					instance.setParameterByName("liquidDepth", num2);
				}
				SoundEvent.EndOneShot(instance);
			}
		}
		if (minionIdentity != null && minionIdentity.model == BionicMinionConfig.MODEL)
		{
			string text4 = GlobalAssets.GetSound("Bionic_move", force_no_warning: true);
			if (text4 != null && (base.objectIsSelectedAndVisible || SoundEvent.ShouldPlaySound(behaviour.controller, text4, base.looping, isDynamic)))
			{
				FMOD.Studio.EventInstance instance2 = SoundEvent.BeginOneShot(text4, pos, SoundEvent.GetVolume(base.objectIsSelectedAndVisible));
				SoundEvent.EndOneShot(instance2);
			}
		}
		if (text2 == null || (!base.objectIsSelectedAndVisible && !SoundEvent.ShouldPlaySound(behaviour.controller, text2, base.looping, isDynamic)))
		{
			return;
		}
		FMOD.Studio.EventInstance instance3 = SoundEvent.BeginOneShot(text2, pos);
		if (instance3.isValid())
		{
			if (num2 > 0f)
			{
				instance3.setParameterByName("liquidDepth", num2);
			}
			if (behaviour.controller.HasAnimationFile("anim_loco_walk_kanim"))
			{
				instance3.setVolume(IDLE_WALKING_VOLUME_REDUCTION);
			}
			SoundEvent.EndOneShot(instance3);
		}
	}

	private static string GetAudioCategory(int cell)
	{
		if (!Grid.IsValidCell(cell))
		{
			return "Rock";
		}
		Element element = Grid.Element[cell];
		if (Grid.Foundation[cell])
		{
			BuildingDef buildingDef = null;
			GameObject gameObject = Grid.Objects[cell, 1];
			if (gameObject != null)
			{
				Building component = gameObject.GetComponent<BuildingComplete>();
				if (component != null)
				{
					buildingDef = component.Def;
				}
			}
			string result = "";
			if (buildingDef != null)
			{
				result = buildingDef.PrefabID switch
				{
					"PlasticTile" => "TilePlastic", 
					"GlassTile" => "TileGlass", 
					"BunkerTile" => "TileBunker", 
					"MetalTile" => "TileMetal", 
					"CarpetTile" => "Carpet", 
					"SnowTile" => "TileSnow", 
					"WoodTile" => "TileWood", 
					_ => "Tile", 
				};
			}
			return result;
		}
		string floorEventAudioCategory = element.substance.GetFloorEventAudioCategory();
		if (floorEventAudioCategory != null)
		{
			return floorEventAudioCategory;
		}
		if (element.HasTag(GameTags.RefinedMetal))
		{
			return "RefinedMetal";
		}
		if (element.HasTag(GameTags.Metal))
		{
			return "RawMetal";
		}
		return "Rock";
	}
}
