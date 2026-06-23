using System;
using UnityEngine;

namespace UtilLibs.YeetUtils;

internal class Rotator : KMonoBehaviour
{
	[SerializeField]
	public float baseDegreesPerSec = 360f;

	[SerializeField]
	private Vector2 direction;

	[SerializeField]
	private float scale;

	[SerializeField]
	public float minDistance;

	[SerializeField]
	public bool stopOnLand = true;

	[MyCmpGet]
	private KBatchedAnimController animController;

	public override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		if (stopOnLand)
		{
			((KMonoBehaviour)this).Subscribe(1188683690, (Action<object>)delegate
			{
				StopRotation();
			});
		}
	}

	public void SetVec(Vector2 vec)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		direction = vec;
		scale = (0f - Mathf.Sign(direction.x)) * (direction.y / minDistance);
		((Behaviour)this).enabled = true;
	}

	private void Update()
	{
		if ((Object)(object)animController != (Object)null)
		{
			KBatchedAnimController obj = animController;
			((KAnimControllerBase)obj).Rotation = ((KAnimControllerBase)obj).Rotation + scale * baseDegreesPerSec * Time.deltaTime % 360f;
		}
	}

	private void StopRotation()
	{
		if ((Object)(object)animController != (Object)null)
		{
			((KAnimControllerBase)animController).Rotation = 0f;
		}
		((Behaviour)this).enabled = false;
	}
}
