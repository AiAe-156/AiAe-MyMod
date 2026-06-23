using KSerialization;
using UnityEngine;

public class ParallaxBackgroundObject : KMonoBehaviour
{
	public interface IMotion
	{
		float GetETA();

		float GetDuration();

		void OnNormalizedDistanceChanged(float normalizedDistance);
	}

	private static Mesh mesh;

	private static int? layer;

	private static float? depth;

	[SerializeField]
	private Sprite sprite;

	[SerializeField]
	private float parallaxFactor = 1f;

	[Range(0f, 5f)]
	public float scaleMin = 0.25f;

	[Range(0f, 5f)]
	public float scaleMax = 3f;

	[Serialize]
	private bool visible = true;

	private const string SHADER_DAMAGED_TIME_VARIABLE_NAME = "_LastTimeDamaged";

	private const string SHADER_PLAYER_CLICKED_TIME_VARIABLE_NAME = "_LastTimePlayerClickedNotification";

	private const string SHADER_SIZE_PROGRESS_VARIABLE_NAME = "_SizeProgress";

	private const string SHADER_EXPLOSION_START_TIME_VARIABLE_NAME = "_LastTimeExploding";

	[SerializeField]
	private Material material;

	[SerializeField]
	[Range(0f, 1f)]
	private float normalizedDistance;

	[SerializeField]
	private bool distanceUpdate;

	[SerializeField]
	private Vector2 startOffset;

	[SerializeField]
	private Vector2 endOffset;

	[Serialize]
	public int? worldId;

	public IMotion motion;

	public static Mesh Mesh
	{
		get
		{
			if (mesh == null)
			{
				mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
			}
			return mesh;
		}
	}

	public static int Layer
	{
		get
		{
			int valueOrDefault = layer.GetValueOrDefault();
			if (!layer.HasValue)
			{
				valueOrDefault = LayerMask.NameToLayer("Default");
				layer = valueOrDefault;
			}
			return layer.Value;
		}
	}

	public static float Depth
	{
		get
		{
			float valueOrDefault = depth.GetValueOrDefault();
			if (!depth.HasValue)
			{
				valueOrDefault = Grid.GetLayerZ(Grid.SceneLayer.Background) + 0.8f;
				depth = valueOrDefault;
			}
			return depth.Value;
		}
	}

	public float lastScaleUsed { get; private set; } = 1f;

	private void OnActiveWorldChanged(object data)
	{
		if (worldId.HasValue)
		{
			int first = ((Tuple<int, int>)data).first;
			visible = first == worldId.Value;
		}
	}

	public void Initialize(string texture)
	{
		sprite = Assets.GetSprite(texture);
	}

	public void SetVisibilityState(bool visible)
	{
		this.visible = visible;
	}

	public void PlayPlayerClickFeedback()
	{
		material.SetFloat("_LastTimePlayerClickedNotification", Time.unscaledTime);
	}

	public void PlayExplosion()
	{
		material.SetFloat("_LastTimeExploding", Time.unscaledTime);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Game.Instance.Subscribe(1983128072, OnActiveWorldChanged);
		distanceUpdate = true;
		startOffset = new Vector2(0f, 0f);
		endOffset = new Vector2(0.5f, 0.2f);
		Material source = Assets.GetMaterial("BGPlanet");
		material = new Material(source);
		material.SetTexture("_MainTex", sprite.texture);
		material.SetFloat("_LastTimeDamaged", float.MinValue);
		material.SetFloat("_LastTimePlayerClickedNotification", float.MinValue);
		material.SetFloat("_SizeProgress", 0f);
		material.renderQueue = RenderQueues.Stars;
	}

	public void TriggerShaderDamagedEffect(int _)
	{
		material.SetFloat("_LastTimeDamaged", Time.unscaledTime);
	}

	private void LateUpdate()
	{
		if (motion != null && visible)
		{
			if (distanceUpdate)
			{
				float duration = motion.GetDuration();
				normalizedDistance = ((duration == 0f) ? 1f : (1f - Mathf.Pow(motion.GetETA() / duration, 4f)));
				motion.OnNormalizedDistanceChanged(normalizedDistance);
			}
			Color a = new Color(0.16862746f, 0.22745098f, 0.36078432f, 0f);
			material.color = Color.Lerp(a, Color.white, normalizedDistance);
			float num = (lastScaleUsed = Mathf.Lerp(scaleMin, scaleMax, normalizedDistance));
			Vector2 vector = Vector2.Lerp(startOffset, endOffset, normalizedDistance);
			Vector3 position = CameraController.Instance.baseCamera.transform.position;
			Vector3 vector2 = new Vector3(position.x * parallaxFactor, position.y * parallaxFactor, Depth);
			float num3 = CameraController.Instance.baseCamera.orthographicSize / 1f;
			Vector3 vector3 = vector2 + (Vector3)vector * num3;
			Vector3 vector4 = num * num3 * Vector3.one;
			Quaternion q = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0f, 0f, -20f), normalizedDistance);
			material.SetFloat("_UnscaledTime", Time.unscaledTime);
			material.SetVector("_Random", new Vector4(Random.value, Random.value));
			material.SetFloat("_SizeProgress", normalizedDistance);
			Matrix4x4 matrix = Matrix4x4.Translate(vector3) * Matrix4x4.Scale(vector4) * Matrix4x4.Rotate(q);
			Graphics.DrawMesh(Mesh, matrix, material, Layer);
		}
	}
}
