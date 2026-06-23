using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("KMonoBehaviour/scripts/WaterCubes")]
public class WaterCubes : KMonoBehaviour
{
	public Material material;

	public Texture2D waveTexture;

	public MeshRenderer waterRenderer;

	public LiquidShaderProperties liquidShaderProperties;

	public static Color MOLTEN_METAL_COLOR = Color.white;

	private GameObject cubes;

	public static WaterCubes Instance { get; private set; }

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		Instance = this;
		MOLTEN_METAL_COLOR = material.GetColor("_MoltenMetalColor");
	}

	public void Init()
	{
		cubes = Util.NewGameObject(base.gameObject, "WaterCubes");
		GameObject gameObject = new GameObject();
		gameObject.name = "WaterCubesMesh";
		gameObject.transform.parent = cubes.transform;
		material.renderQueue = RenderQueues.Liquid;
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		waterRenderer = gameObject.AddComponent<MeshRenderer>();
		waterRenderer.sharedMaterial = material;
		waterRenderer.shadowCastingMode = ShadowCastingMode.Off;
		waterRenderer.receiveShadows = false;
		waterRenderer.lightProbeUsage = LightProbeUsage.Off;
		waterRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		waterRenderer.sharedMaterial.SetTexture("_MainTex2", waveTexture);
		meshFilter.sharedMesh = CreateNewMesh();
		waterRenderer.gameObject.layer = LayerMask.NameToLayer("Water");
		waterRenderer.gameObject.transform.parent = base.transform;
		waterRenderer.gameObject.transform.SetPosition(new Vector3(0f, 0f, Grid.GetLayerZ(Grid.SceneLayer.Liquid)));
		if (liquidShaderProperties != null)
		{
			liquidShaderProperties.ApplyToMaterial(material);
		}
	}

	private void LateUpdate()
	{
		Vector3 vector = Camera.main.ScreenToWorldPoint(KInputManager.GetMousePos());
		material.SetVector("_CursorWorldPosition", vector);
		if (liquidShaderProperties != null)
		{
			liquidShaderProperties.ApplyToMaterial(material);
		}
	}

	private Mesh CreateNewMesh()
	{
		Mesh mesh = new Mesh();
		mesh.name = "WaterCubes";
		Vector3[] array = new Vector3[4];
		Vector2[] array2 = new Vector2[4];
		Vector3[] array3 = new Vector3[4];
		Vector4[] array4 = new Vector4[4];
		int[] array5 = new int[6];
		float layerZ = Grid.GetLayerZ(Grid.SceneLayer.Liquid);
		array = new Vector3[4]
		{
			new Vector3(0f, 0f, layerZ),
			new Vector3(Grid.WidthInCells, 0f, layerZ),
			new Vector3(0f, Grid.HeightInMeters, layerZ),
			new Vector3(Grid.WidthInMeters, Grid.HeightInMeters, layerZ)
		};
		array2 = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		array3 = new Vector3[4]
		{
			new Vector3(0f, 0f, -1f),
			new Vector3(0f, 0f, -1f),
			new Vector3(0f, 0f, -1f),
			new Vector3(0f, 0f, -1f)
		};
		array4 = new Vector4[4]
		{
			new Vector4(0f, 1f, 0f, -1f),
			new Vector4(0f, 1f, 0f, -1f),
			new Vector4(0f, 1f, 0f, -1f),
			new Vector4(0f, 1f, 0f, -1f)
		};
		array5 = new int[6] { 0, 2, 1, 1, 2, 3 };
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.uv2 = array2;
		mesh.normals = array3;
		mesh.tangents = array4;
		mesh.triangles = array5;
		mesh.bounds = new Bounds(Vector3.zero, new Vector3(float.MaxValue, float.MaxValue, 0f));
		return mesh;
	}
}
