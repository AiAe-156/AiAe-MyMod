using System;
using System.Collections.Generic;
using FMOD.Studio;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/Comet")]
public class LargeComet : KMonoBehaviour, ISim33ms
{
	private static HashedString FLYING_SOUND_ID_PARAMETER = "meteorType";

	public string impactSound;

	public string flyingSound;

	public int flyingSoundID;

	public List<KeyValuePair<string, string>> additionalAnimFiles = new List<KeyValuePair<string, string>>();

	public KeyValuePair<string, string> mainAnimFile;

	public bool affectedByDifficulty = true;

	public bool destroyOnExplode = true;

	public bool spawnWithOffset = false;

	public Vector2I stampLocation;

	public Vector2I crashPosition;

	public Dictionary<int, CellOffset> bottomCellsOffsetOfTemplate;

	public TemplateContainer asteroidTemplate;

	public Ref<KPrefabID> ignoreObstacleForDamage = new Ref<KPrefabID>();

	private bool hasExploded = false;

	private float age = 0f;

	private int lowestTemplateYLocalPosition;

	private int templateWidth;

	private int worldID;

	private Vector3 previousVisualPosition;

	private Vector3 initialPosition;

	private Vector2I prevCell;

	public System.Action OnImpact;

	[Serialize]
	protected Vector3 offsetPosition;

	[Serialize]
	protected Vector2 velocity;

	[MyCmpGet]
	private KBatchedAnimController anim;

	[MyCmpGet]
	private KSelectable selectable;

	private LoopingSounds loopingSounds;

	private KBatchedAnimController[] child_controllers;

	private List<KAnimControllerBase> additionalAnimControllers = new List<KAnimControllerBase>();

	private KBatchedAnimController mainChildrenAnimController;

	private Vector2I fromStampToCrashPosition;

	private HashSet<int> cellsCentrePassedThrough = new HashSet<int>();

	private Vector3 activeExplosionPosition = Vector3.zero;

	private Material largeCometMaterial;

	private Sprite largeCometTexture;

	private Sprite explosionTexture;

	private float minSeparationBetweenExplosions = 8f;

	private Vector3 lastExplosionPosition;

	private const string LARGE_COMET_SHADER_NAME = "Klei/DLC4/LargeImpactorCometShader";

	private const int MAX_SHADER_EXPLOSION_COUNT = 30;

	private const float EXPLOSION_ANIMATION_FRAME_COUNT = 37f;

	private const float EXPLOSION_ANIMATION_DURATION = 1.2333333f;

	private Vector4[] ShaderExplosions = new Vector4[30];

	public float LandingProgress { get; private set; } = 0f;

	public Vector3 VisualPosition => base.transform.position + anim.Offset;

	public Vector3 VisualPositionCentredImage => VisualPosition + new Vector3(0f, Mathf.Abs(lowestTemplateYLocalPosition), 0f);

	public Vector2 Velocity
	{
		get
		{
			return velocity;
		}
		set
		{
			velocity = value;
		}
	}

	private float GetVolume(GameObject gameObject)
	{
		float result = 1f;
		if (gameObject != null && selectable != null && selectable.IsSelected)
		{
			result = 1f;
		}
		return result;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		loopingSounds = base.gameObject.GetComponent<LoopingSounds>();
		flyingSound = GlobalAssets.GetSound("Meteor_LP");
		SetVelocity();
	}

	protected override void OnSpawn()
	{
		anim.Offset = offsetPosition;
		SetupOffset();
		child_controllers = GetComponents<KBatchedAnimController>();
		KBatchedAnimController[] array = child_controllers;
		foreach (KBatchedAnimController kBatchedAnimController in array)
		{
			kBatchedAnimController.Offset = anim.Offset;
		}
		base.OnSpawn();
		StartLoopingSound();
		bool flag = offsetPosition.x != 0f || offsetPosition.y != 0f;
		selectable.enabled = !flag;
		Vector3 position = base.gameObject.transform.position;
		foreach (KeyValuePair<string, string> additionalAnimFile in additionalAnimFiles)
		{
			additionalAnimControllers.Add(AddEffectAnim(additionalAnimFile.Key, additionalAnimFile.Value, position));
			position.z -= 0.001f;
		}
		KBatchedAnimController item = AddEffectAnim(mainAnimFile.Key, mainAnimFile.Value, position);
		additionalAnimControllers.Add(item);
		mainChildrenAnimController = item;
		mainChildrenAnimController.materialType = KAnimBatchGroup.MaterialType.Invisible;
		initialPosition = VisualPosition;
		lowestTemplateYLocalPosition = asteroidTemplate.GetTemplateBounds().yMin;
		templateWidth = asteroidTemplate.GetTemplateBounds().width;
		InitializeMaterial();
		CameraController.Instance.RegisterCustomScreenPostProcessingEffect(DrawComet);
		fromStampToCrashPosition = stampLocation - crashPosition;
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		CameraController.Instance.UnregisterCustomScreenPostProcessingEffect(DrawComet);
	}

	private KBatchedAnimController AddEffectAnim(string anim_file, string anim_name, Vector3 startPosition)
	{
		KBatchedAnimController kBatchedAnimController = FXHelpers.CreateEffect(anim_file, startPosition);
		kBatchedAnimController.Play(anim_name, KAnim.PlayMode.Loop);
		kBatchedAnimController.visibilityType = KAnimControllerBase.VisibilityType.OffscreenUpdate;
		kBatchedAnimController.animScale = 0.1f;
		kBatchedAnimController.isMovable = true;
		kBatchedAnimController.Offset = anim.Offset;
		return kBatchedAnimController;
	}

	protected void SetupOffset()
	{
		Vector3 position = base.transform.GetPosition();
		Vector3 position2 = base.transform.GetPosition();
		position2.z = 0f;
		Vector3 vector = new Vector3(velocity.x, velocity.y, 0f);
		WorldContainer myWorld = base.gameObject.GetMyWorld();
		float num = (float)(myWorld.WorldOffset.y + myWorld.Height + MissileLauncher.Def.launchRange.y) * Grid.CellSizeInMeters - position2.y;
		float f = Vector3.Angle(Vector3.up, -vector) * (MathF.PI / 180f);
		float num2 = Mathf.Abs(num / Mathf.Cos(f));
		Vector3 vector2 = position2 - vector.normalized * num2;
		float num3 = (float)(myWorld.WorldOffset.x + myWorld.Width) * Grid.CellSizeInMeters;
		if (!(vector2.x >= (float)myWorld.WorldOffset.x * Grid.CellSizeInMeters) || !(vector2.x <= num3))
		{
			float num4 = ((vector.x < 0f) ? (num3 - position2.x) : (position2.x - (float)myWorld.WorldOffset.x * Grid.CellSizeInMeters));
			f = Vector3.Angle((vector.x < 0f) ? Vector3.right : Vector3.left, -vector) * (MathF.PI / 180f);
			num2 = Mathf.Abs(num4 / Mathf.Cos(f));
		}
		Vector3 vector3 = -vector.normalized * num2;
		Vector3 vector4 = position2 + vector3;
		vector4.z = position.z;
		offsetPosition = vector3;
		anim.Offset = offsetPosition;
		worldID = myWorld.id;
		previousVisualPosition = VisualPosition;
	}

	public void SetVelocity()
	{
		int num = -90;
		float f = (float)num * MathF.PI / 180f;
		int num2 = 12;
		velocity = new Vector2((0f - Mathf.Cos(f)) * (float)num2, Mathf.Sin(f) * (float)num2);
		KBatchedAnimController component = GetComponent<KBatchedAnimController>();
		component.Rotation = (float)(-num) - 90f;
	}

	private void Explode(Vector3 pos)
	{
		PlayImpactSound(pos);
		if (OnImpact != null)
		{
			OnImpact();
		}
		foreach (KAnimControllerBase additionalAnimController in additionalAnimControllers)
		{
			Util.KDestroyGameObject(additionalAnimController);
		}
		Util.KDestroyGameObject(base.gameObject);
	}

	public void Sim33ms(float dt)
	{
		if (hasExploded)
		{
			return;
		}
		if (offsetPosition.y > 0f)
		{
			Vector3 vector = new Vector3(velocity.x * dt, velocity.y * dt, 0f);
			Vector3 vector2 = offsetPosition + vector;
			offsetPosition = vector2;
			anim.Offset = offsetPosition;
			foreach (KAnimControllerBase additionalAnimController in additionalAnimControllers)
			{
				additionalAnimController.Offset = offsetPosition;
			}
		}
		else
		{
			if (anim.Offset != Vector3.zero)
			{
				anim.Offset = Vector3.zero;
				foreach (KAnimControllerBase additionalAnimController2 in additionalAnimControllers)
				{
					additionalAnimController2.Offset = anim.Offset;
				}
			}
			Vector3 position = base.transform.GetPosition();
			Vector3 vector3 = position + new Vector3(velocity.x * dt, velocity.y * dt, 0f);
			loopingSounds.UpdateVelocity(flyingSound, vector3 - position);
			base.transform.SetPosition(vector3);
			Vector3 position2 = vector3;
			foreach (KAnimControllerBase additionalAnimController3 in additionalAnimControllers)
			{
				additionalAnimController3.transform.SetPosition(position2);
				position2.z -= 0.001f;
			}
			if (vector3.y < (float)crashPosition.y)
			{
				Explode(vector3);
			}
		}
		Vector2I vector2I = Grid.PosToXY(previousVisualPosition);
		Vector2I vector2I2 = Grid.PosToXY(VisualPosition);
		vector2I.y = Mathf.Clamp(vector2I.y, crashPosition.y, int.MaxValue);
		vector2I2.y = Mathf.Clamp(vector2I2.y, crashPosition.y, int.MaxValue);
		if (vector2I2.y != vector2I.y)
		{
			Grid.CollectCellsInLine(Grid.XYToCell(vector2I.x, vector2I.y), Grid.XYToCell(vector2I2.x, vector2I2.y), cellsCentrePassedThrough);
			bool flag = false;
			Vector3 position3 = Vector3.zero;
			foreach (int item in cellsCentrePassedThrough)
			{
				foreach (CellOffset value in bottomCellsOffsetOfTemplate.Values)
				{
					int cell = Grid.OffsetCell(item, 0, Mathf.Abs(lowestTemplateYLocalPosition));
					int cell2 = Grid.OffsetCell(cell, value.x, value.y);
					if (Grid.IsValidCellInWorld(cell2, worldID) && DestroyCell(cell2) && !flag)
					{
						Vector3 position4 = Grid.CellToPos(cell2);
						if (IsPositionFarAwayFromOtherExplosions(position4))
						{
							flag = true;
							position3 = Grid.CellToPos(cell2);
						}
					}
				}
			}
			if (flag)
			{
				PlayExplosionEffectOnPosition(position3);
			}
		}
		float num = Mathf.Clamp(1f - (VisualPosition.y - (float)crashPosition.y) / (initialPosition.y - (float)crashPosition.y), 0f, 1f);
		mainChildrenAnimController.postProcessingParameters = Mathf.Clamp(Mathf.Ceil(num * (Mathf.Pow(10f, 3f) - 1f)), 0f, float.MaxValue);
		LandingProgress = num;
		previousVisualPosition = VisualPosition;
		age += dt;
	}

	private bool IsPositionFarAwayFromOtherExplosions(Vector3 position)
	{
		activeExplosionPosition.z = position.z;
		for (int i = 0; i < 30; i++)
		{
			if (ShaderExplosions[i].z >= 0f && Time.timeSinceLevelLoad - ShaderExplosions[i].z < 1.2333333f)
			{
				activeExplosionPosition.x = ShaderExplosions[i].x;
				activeExplosionPosition.y = ShaderExplosions[i].y;
				if ((activeExplosionPosition - position).magnitude < minSeparationBetweenExplosions)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void PlayExplosionEffectOnPosition(Vector3 position)
	{
		for (int i = 0; i < 30; i++)
		{
			if (ShaderExplosions[i].z < 0f || Time.timeSinceLevelLoad - ShaderExplosions[i].z > 1.2333333f)
			{
				ShaderExplosions[i].x = position.x;
				ShaderExplosions[i].y = position.y;
				ShaderExplosions[i].z = Time.timeSinceLevelLoad;
				KFMOD.PlayOneShot(GlobalAssets.GetSound("Battery_explode"), position);
				lastExplosionPosition = position;
				break;
			}
		}
	}

	private void PlayImpactSound(Vector3 pos)
	{
		if (impactSound == null)
		{
			impactSound = "Meteor_Large_Impact";
		}
		loopingSounds.StopSound(flyingSound);
		string sound = GlobalAssets.GetSound(impactSound);
		int num = Grid.PosToCell(pos);
		if (Grid.IsValidCell(num) && Grid.WorldIdx[num] == ClusterManager.Instance.activeWorldId)
		{
			float volume = GetVolume(base.gameObject);
			pos.z = 0f;
			EventInstance instance = KFMOD.BeginOneShot(sound, pos, volume);
			instance.setParameterByName("userVolume_SFX", KPlayerPrefs.GetFloat("Volume_SFX"));
			KFMOD.EndOneShot(instance);
		}
	}

	public bool DestroyCell(int cell)
	{
		bool flag = false;
		ListPool<GameObject, LargeComet>.PooledList pooledList = ListPool<GameObject, LargeComet>.Allocate();
		GameObject gameObject = Grid.Objects[cell, 1];
		flag = gameObject != null;
		pooledList.Add(gameObject);
		pooledList.Add(Grid.Objects[cell, 2]);
		pooledList.Add(Grid.Objects[cell, 12]);
		pooledList.Add(Grid.Objects[cell, 15]);
		pooledList.Add(Grid.Objects[cell, 16]);
		pooledList.Add(Grid.Objects[cell, 19]);
		pooledList.Add(Grid.Objects[cell, 20]);
		pooledList.Add(Grid.Objects[cell, 23]);
		pooledList.Add(Grid.Objects[cell, 26]);
		pooledList.Add(Grid.Objects[cell, 29]);
		pooledList.Add(Grid.Objects[cell, 31]);
		pooledList.Add(Grid.Objects[cell, 30]);
		foreach (MinionIdentity item in Components.LiveMinionIdentities.Items)
		{
			int num = Grid.PosToCell(item);
			if (num == cell)
			{
				pooledList.Add(item.gameObject);
				SaveGame.Instance.ColonyAchievementTracker.deadDupeCounter++;
			}
		}
		foreach (GameObject item2 in pooledList)
		{
			if (item2 != null)
			{
				Util.KDestroyGameObject(item2);
			}
		}
		ClearCellPickupables(cell);
		Element element = ElementLoader.elements[Grid.ElementIdx[cell]];
		if (element.id == SimHashes.Void)
		{
			SimMessages.ReplaceElement(cell, SimHashes.Void, CellEventLogger.Instance.DebugTool, 0f, 0f);
		}
		else
		{
			SimMessages.ReplaceElement(cell, SimHashes.Vacuum, CellEventLogger.Instance.DebugTool, 0f, 0f);
		}
		flag = flag || element.IsSolid;
		pooledList.Recycle();
		return flag;
	}

	public void ClearCellPickupables(int cell)
	{
		GameObject gameObject = Grid.Objects[cell, 3];
		if (!(gameObject != null))
		{
			return;
		}
		ObjectLayerListItem objectLayerListItem = gameObject.GetComponent<Pickupable>().objectLayerListItem;
		while (objectLayerListItem != null)
		{
			GameObject gameObject2 = objectLayerListItem.gameObject;
			objectLayerListItem = objectLayerListItem.nextItem;
			if (!(gameObject2 == null))
			{
				Util.KDestroyGameObject(gameObject2);
			}
		}
	}

	private void InitializeMaterial()
	{
		largeCometMaterial = new Material(Shader.Find("Klei/DLC4/LargeImpactorCometShader"));
		largeCometTexture = Assets.GetSprite("Demolior_final_broken");
		explosionTexture = Assets.GetSprite("contact_explode_fx_animationSheet");
		for (int i = 0; i < 30; i++)
		{
			ShaderExplosions[i] = Vector4.one * -1f;
			ShaderExplosions[i].w = (minSeparationBetweenExplosions - 1f) * 2f;
		}
	}

	private Material DrawComet(RenderTexture source)
	{
		largeCometMaterial.SetTexture("_CometTex", largeCometTexture.texture);
		largeCometMaterial.SetTexture("_ExplosionTex", explosionTexture.texture);
		largeCometMaterial.SetVector("_CometWorldPosition", VisualPositionCentredImage);
		largeCometMaterial.SetFloat("_LandingProgress", LandingProgress);
		largeCometMaterial.SetFloat("_CometWidth", templateWidth);
		largeCometMaterial.SetFloat("_CometRatio", (float)largeCometTexture.texture.height / (float)largeCometTexture.texture.width);
		largeCometMaterial.SetFloat("_UnscaledTime", Time.unscaledTime);
		largeCometMaterial.SetVectorArray("_ExplosionLocations", ShaderExplosions);
		return largeCometMaterial;
	}

	private void StartLoopingSound()
	{
		loopingSounds.StartSound(flyingSound);
		loopingSounds.UpdateFirstParameter(flyingSound, FLYING_SOUND_ID_PARAMETER, flyingSoundID);
	}
}
