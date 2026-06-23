using UnityEngine;

public class Infrared : MonoBehaviour
{
	public enum Mode
	{
		Disabled,
		Infrared,
		Disease
	}

	public class TemperatureOverlayInfraredData
	{
		public GridArea bounds;
	}

	private RenderTexture minionTexture;

	private RenderTexture cameraTexture;

	private Mode mode;

	public static int temperatureParametersId;

	public static Infrared Instance;

	private static TemperatureOverlayInfraredData temperatureOvelayInfraredParams = new TemperatureOverlayInfraredData();

	public static void DestroyInstance()
	{
		Instance = null;
	}

	private void Awake()
	{
		temperatureParametersId = Shader.PropertyToID("_TemperatureParameters");
		Instance = this;
		OnResize();
		UpdateState();
	}

	private void OnRenderImage(RenderTexture source, RenderTexture dest)
	{
		Graphics.Blit(source, minionTexture);
		Graphics.Blit(source, dest);
	}

	private void OnResize()
	{
		if (minionTexture != null)
		{
			minionTexture.DestroyRenderTexture();
		}
		if (cameraTexture != null)
		{
			cameraTexture.DestroyRenderTexture();
		}
		int num = 2;
		minionTexture = new RenderTexture(Screen.width / num, Screen.height / num, 0, RenderTextureFormat.ARGB32);
		cameraTexture = new RenderTexture(Screen.width / num, Screen.height / num, 0, RenderTextureFormat.ARGB32);
		GetComponent<Camera>().targetTexture = cameraTexture;
	}

	public void SetMode(Mode mode)
	{
		Vector4 value;
		switch (mode)
		{
		case Mode.Disabled:
			value = Vector4.zero;
			break;
		case Mode.Disease:
			value = new Vector4(1f, 0f, 0f, 0f);
			Game.Instance.Trigger(972756592);
			break;
		default:
			value = new Vector4(1f, 0f, 0f, 0f);
			break;
		}
		Shader.SetGlobalVector("_ColouredOverlayParameters", value);
		this.mode = mode;
		UpdateState();
	}

	private void UpdateState()
	{
		base.gameObject.SetActive(mode != Mode.Disabled);
		if (base.gameObject.activeSelf)
		{
			Update();
		}
	}

	private void Update()
	{
		switch (mode)
		{
		case Mode.Infrared:
		{
			GridArea visibleArea = GridVisibleArea.GetVisibleArea();
			temperatureOvelayInfraredParams.bounds = visibleArea;
			Game.Instance.Trigger(-880408538, (object)temperatureOvelayInfraredParams);
			break;
		}
		case Mode.Disease:
			GameComps.DiseaseContainers.UpdateOverlayColours();
			break;
		case Mode.Disabled:
			break;
		}
	}
}
