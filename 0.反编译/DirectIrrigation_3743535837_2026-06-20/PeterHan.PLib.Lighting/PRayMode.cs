using System;
using PeterHan.PLib.Detours;
using UnityEngine;

namespace PeterHan.PLib.Lighting;

public class PRayMode : IRayMode
{
	private static readonly IDetouredField<LightBuffer, Camera> LIGHTBUFFER_CAMERA = PDetours.TryDetourField<LightBuffer, Camera>("Camera");

	private static readonly IDetouredField<LightBuffer, Mesh> LIGHTBUFFER_MESH = PDetours.TryDetourField<LightBuffer, Mesh>("Mesh");

	protected readonly LightShape filter;

	protected int layer;

	protected Material material;

	protected readonly Texture2D texture;

	public PRayMode(Texture2D texture, LightShape filter = (LightShape)(-1))
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)texture == (Object)null)
		{
			throw new ArgumentNullException("texture");
		}
		this.filter = filter;
		layer = LayerMask.NameToLayer("Lights");
		material = null;
		this.texture = texture;
	}

	public void DrawCustomRay(Light2D light, LightBuffer lightBuffer)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (FilterDrawing(light))
		{
			Vector2 normalized = ((Vector2)(ref light.Direction)).normalized;
			Vector3 val = ((KMonoBehaviour)light).transform.position + Vector2.op_Implicit(light.Offset);
			float num = Mathf.Atan2(normalized.y, normalized.x) * 57.29578f;
			Matrix4x4 matrix = Matrix4x4.Translate(val) * Matrix4x4.Rotate(Quaternion.AngleAxis(num, Vector3.forward));
			Camera val2 = default(Camera);
			if (LIGHTBUFFER_CAMERA == null)
			{
				((Component)lightBuffer).TryGetComponent<Camera>(ref val2);
			}
			else
			{
				val2 = LIGHTBUFFER_CAMERA.Get(lightBuffer);
			}
			Mesh val3 = LIGHTBUFFER_MESH?.Get(lightBuffer);
			if ((Object)(object)val3 != (Object)null && (Object)(object)val2 != (Object)null)
			{
				GetTransformMatrix(ref matrix);
				Graphics.DrawMesh(val3, matrix, material, layer, val2, 0, light.materialPropertyBlock);
			}
		}
	}

	protected virtual bool FilterDrawing(Light2D light)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)light != (Object)null)
		{
			if ((int)filter >= 0)
			{
				return light.shape == filter;
			}
			return true;
		}
		return false;
	}

	protected virtual void GetTransformMatrix(ref Matrix4x4 matrix)
	{
	}

	public virtual void Prepare(LightBuffer lightBuffer)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		if ((Object)(object)material == (Object)null)
		{
			CameraController instance = CameraController.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				material = new Material(instance.LightCircleOverlay)
				{
					mainTexture = (Texture)(object)texture
				};
			}
		}
		Material obj = material;
		if (obj != null)
		{
			obj.SetTexture("_PropertyWorldLight", lightBuffer.WorldLight);
		}
	}
}
