using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FUtility.Components;

public class PrefabSpawner : KMonoBehaviour
{
	[SerializeField]
	public List<(float, Tag)> options = new List<(float, Tag)>();

	[SerializeField]
	public bool yeet = true;

	[SerializeField]
	public bool spawnElementInWorld = true;

	[SerializeField]
	public bool yeetOnlyUp;

	[SerializeField]
	public float yeetSpeed = 1f;

	[SerializeField]
	public int yeetMin = 1;

	[SerializeField]
	public int yeetMax = 3;

	[SerializeField]
	public float minDelay = 0.1f;

	[SerializeField]
	public float volume = 1f;

	[SerializeField]
	public float maxDelay = 0.5f;

	[SerializeField]
	public int minCount = 1;

	[SerializeField]
	public int maxCount;

	[SerializeField]
	public string soundFx;

	[SerializeField]
	public bool rotate;

	[SerializeField]
	public SpawnFXHashes fxHash;

	[SerializeField]
	public Action<GameObject> OnItemSpawned;

	private int itemsSpawned;

	private int itemCount;

	private bool beginSpawning;

	protected override void OnSpawn()
	{
		((KMonoBehaviour)this).OnSpawn();
		itemCount = Random.Range(minCount, maxCount);
		Log.Debuglog($"Spawned Spawner with {options.Count} options", itemCount);
		((MonoBehaviour)this).StartCoroutine(SpawnStuff());
	}

	private IEnumerator SpawnStuff()
	{
		while (itemsSpawned < itemCount)
		{
			if (beginSpawning)
			{
				Log.Debuglog("Spawning started");
				(float, Tag) random = Util.GetRandom<(float, Tag)>(options);
				Tag item = random.Item2;
				var (num, _) = random;
				if (spawnElementInWorld)
				{
					Element element = ElementLoader.GetElement(item);
					if (element != null)
					{
						Log.Debuglog("Spawning an element");
						if (element.IsLiquid)
						{
							FallingWater.instance.AddParticle(Grid.PosToCell((KMonoBehaviour)(object)this), element.idx, num, element.defaultValues.temperature, byte.MaxValue, 0, false, false, false, false);
						}
						else
						{
							SimMessages.AddRemoveSubstance(Grid.PosToCell((KMonoBehaviour)(object)this), element.idx, CellEventLogger.Instance.Dumpable, element.defaultValues.mass / 10f, element.defaultValues.temperature, byte.MaxValue, 0, true, -1);
						}
						goto IL_0194;
					}
				}
				Log.Debuglog("Spawning an item");
				GameObject val = Utils.Spawn(item, ((Component)this).gameObject, (SceneLayer)24);
				val.GetComponent<PrimaryElement>().Mass = num;
				Log.Debuglog(KPrefabIDExtensions.PrefabID(val));
				Utils.YeetRandomly(val, yeetOnlyUp, yeetMin, yeetMax, rotate);
				OnItemSpawned?.Invoke(val);
				goto IL_0194;
			}
			beginSpawning = true;
			goto IL_0200;
			IL_0194:
			if ((int)fxHash != 0)
			{
				Game.Instance.SpawnFX(fxHash, TransformExtensions.GetPosition(((KMonoBehaviour)this).transform), 0f);
			}
			if (!Util.IsNullOrWhiteSpace(soundFx))
			{
				KFMOD.PlayOneShot(soundFx, TransformExtensions.GetPosition(((Component)SoundListenerController.Instance).transform), volume);
			}
			itemsSpawned++;
			goto IL_0200;
			IL_0200:
			yield return (object)new WaitForSeconds(Random.Range(minDelay, maxDelay));
		}
		RemoveSelf();
		yield return null;
	}

	private void RemoveSelf()
	{
		Log.Debuglog("Deleting self");
		((MonoBehaviour)this).StopAllCoroutines();
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
