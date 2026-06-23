using Unity.Collections;
using UnityEngine;

public class LargeImpactorVisualizerEffect : KMonoBehaviour
{
	private Material material;

	private Camera myCamera;

	public Color highlightColor = new Color(1f, 0.7f, 0.3f, 1f);

	private Texture2D OcclusionTex;

	private LargeImpactorVisualizer rangeVisualizer;

	private Sprite icon;

	protected override void OnSpawn()
	{
		GameplayEventInstance gameplayEventInstance = GameplayEventManager.Instance.GetGameplayEventInstance(Db.Get().GameplayEvents.LargeImpactor.Id);
		material = new Material(Shader.Find("Klei/PostFX/LargeImpactorVisualizerShader"));
		if (!SetLargeImpactObjectFromEventInstance(gameplayEventInstance))
		{
			GameplayEventManager.Instance.Subscribe(1491341646, SetupOnGameplayEventStart);
		}
		icon = Assets.GetSprite("iconWarning");
	}

	private bool SetLargeImpactObjectFromEventInstance(GameplayEventInstance eventInstance)
	{
		if (eventInstance != null)
		{
			LargeImpactorEvent.StatesInstance statesInstance = (LargeImpactorEvent.StatesInstance)eventInstance.smi;
			rangeVisualizer = statesInstance.impactorInstance.GetComponent<LargeImpactorVisualizer>();
			LargeImpactorNotificationMonitor.Instance sMI = statesInstance.impactorInstance.GetSMI<LargeImpactorNotificationMonitor.Instance>();
			if (rangeVisualizer != null)
			{
				material.SetFloat("_EntryStartTime", -1f);
				material.SetFloat("_ZoneWasRevealed", sMI.HasRevealSequencePlayed ? 1 : 0);
			}
			statesInstance.impactorInstance.Subscribe(-467702038, OnAnySequenceRelatedToImpactorCompleted);
			return true;
		}
		return false;
	}

	private void OnAnySequenceRelatedToImpactorCompleted(object o)
	{
		material.SetFloat("_ZoneWasRevealed", 1f);
	}

	private void SetupOnGameplayEventStart(object data)
	{
		GameplayEventInstance gameplayEventInstance = (GameplayEventInstance)data;
		if (gameplayEventInstance.eventID == Db.Get().GameplayEvents.LargeImpactor.Id)
		{
			SetLargeImpactObjectFromEventInstance(gameplayEventInstance);
		}
		GameplayEventManager.Instance.Unsubscribe(1491341646, SetupOnGameplayEventStart);
	}

	private void OnPostRender()
	{
		if (rangeVisualizer == null || !rangeVisualizer.Active || (rangeVisualizer.Folded && Time.unscaledTime - rangeVisualizer.LastTimeSetToFolded > rangeVisualizer.FoldEffectDuration + 1f))
		{
			return;
		}
		Vector2I vector2I = Grid.PosToXY(rangeVisualizer.transform.position);
		bool flag = false;
		if (OcclusionTex == null || OcclusionTex.width != rangeVisualizer.TexSize.X || OcclusionTex.height != rangeVisualizer.TexSize.Y)
		{
			OcclusionTex = new Texture2D(rangeVisualizer.TexSize.X, rangeVisualizer.TexSize.Y, TextureFormat.Alpha8, mipChain: false);
			OcclusionTex.filterMode = FilterMode.Point;
			OcclusionTex.wrapMode = TextureWrapMode.Clamp;
			flag = true;
		}
		FindWorldBounds(out var world_min, out var world_max);
		Vector2I rangeMin = rangeVisualizer.RangeMin;
		Vector2I rangeMax = rangeVisualizer.RangeMax;
		Vector2I originOffset = rangeVisualizer.OriginOffset;
		Vector2I vector2I2 = vector2I + originOffset;
		if (flag)
		{
			int width = OcclusionTex.width;
			NativeArray<byte> pixelData = OcclusionTex.GetPixelData<byte>(0);
			for (int i = 0; i <= rangeMax.y - rangeMin.y; i++)
			{
				int num = vector2I2.y + rangeMin.y + i;
				for (int j = 0; j <= rangeMax.x - rangeMin.x; j++)
				{
					int num2 = vector2I2.x + rangeMin.x + j;
					int arg = Grid.XYToCell(num2, num);
					bool flag2 = num2 > world_min.x && num2 < world_max.x && num > world_min.y && num < world_max.y && rangeVisualizer.BlockingCb(arg);
					pixelData[i * width + j] = (byte)((!flag2) ? 255u : 0u);
				}
			}
		}
		OcclusionTex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
		Vector2I vector2I3 = rangeMin + vector2I2;
		Vector2I vector2I4 = rangeMax + vector2I2;
		if (myCamera == null)
		{
			myCamera = GetComponent<Camera>();
			if (myCamera == null)
			{
				return;
			}
		}
		Ray ray = myCamera.ViewportPointToRay(Vector3.zero);
		float distance = Mathf.Abs(ray.origin.z / ray.direction.z);
		Vector3 point = ray.GetPoint(distance);
		Vector4 value = default(Vector4);
		value.x = point.x;
		value.y = point.y;
		ray = myCamera.ViewportPointToRay(Vector3.one);
		distance = Mathf.Abs(ray.origin.z / ray.direction.z);
		point = ray.GetPoint(distance);
		value.z = point.x - value.x;
		value.w = point.y - value.y;
		material.SetVector("_UVOffsetScale", value);
		Vector4 value2 = default(Vector4);
		value2.x = vector2I3.x;
		value2.y = vector2I3.y;
		value2.z = vector2I4.x + 1;
		value2.w = vector2I4.y + 1;
		material.SetVector("_RangeParams", value2);
		material.SetColor("_HighlightColor", highlightColor);
		material.SetTexture("_Icon", icon.texture);
		Vector4 value3 = default(Vector4);
		value3.x = 1f / (float)OcclusionTex.width;
		value3.y = 1f / (float)OcclusionTex.height;
		value3.z = 0f;
		value3.w = 0f;
		material.SetVector("_OcclusionParams", value3);
		material.SetTexture("_OcclusionTex", OcclusionTex);
		Vector4 value4 = default(Vector4);
		value4.x = Grid.WidthInCells;
		value4.y = Grid.HeightInCells;
		value4.z = 1f / (float)Grid.WidthInCells;
		value4.w = 1f / (float)Grid.HeightInCells;
		material.SetVector("_WorldParams", value4);
		if (rangeVisualizer.ShouldResetEntryEffect)
		{
			material.SetFloat("_EntryStartTime", Time.unscaledTime);
			rangeVisualizer.SetShouldResetEntryEffect(shouldIt: false);
		}
		material.SetFloat("_EntryEffectDuration", rangeVisualizer.EntryEffectDuration);
		material.SetFloat("_FoldDuration", rangeVisualizer.FoldEffectDuration);
		material.SetFloat("_UnscaledTime", Time.unscaledTime);
		material.SetVector("_UIToggleScreenPosition", rangeVisualizer.ScreenSpaceNotificationTogglePosition);
		material.SetFloat("_LastTimeSetToFold", rangeVisualizer.LastTimeSetToFolded);
		GL.PushMatrix();
		material.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(5);
		GL.Color(Color.white);
		GL.Vertex3(0f, 0f, 0f);
		GL.Vertex3(0f, 1f, 0f);
		GL.Vertex3(1f, 0f, 0f);
		GL.Vertex3(1f, 1f, 0f);
		GL.End();
		GL.PopMatrix();
	}

	private void FindWorldBounds(out Vector2I world_min, out Vector2I world_max)
	{
		if (ClusterManager.Instance != null)
		{
			WorldContainer activeWorld = ClusterManager.Instance.activeWorld;
			world_min = activeWorld.WorldOffset;
			world_max = activeWorld.WorldOffset + activeWorld.WorldSize;
		}
		else
		{
			world_min.x = 0;
			world_min.y = 0;
			world_max.x = Grid.WidthInCells;
			world_max.y = Grid.HeightInCells;
		}
	}
}
