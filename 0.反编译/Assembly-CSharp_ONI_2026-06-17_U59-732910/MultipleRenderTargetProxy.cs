using System;
using UnityEngine;

public class MultipleRenderTargetProxy : MonoBehaviour
{
	public RenderTexture[] Textures = new RenderTexture[3];

	public RenderTexture[] TexturesCopies = new RenderTexture[3];

	private bool colouredOverlayBufferEnabled;

	public System.Action OnTexturesRecreated;

	public bool IsColouredOverlayBufferEnabled => colouredOverlayBufferEnabled;

	private void Start()
	{
		if (ScreenResize.Instance != null)
		{
			ScreenResize instance = ScreenResize.Instance;
			instance.OnResize = (System.Action)Delegate.Combine(instance.OnResize, new System.Action(OnResize));
		}
		CreateRenderTarget();
		ShaderReloader.Register(OnShadersReloaded);
	}

	public void ToggleColouredOverlayView(bool enabled)
	{
		colouredOverlayBufferEnabled = enabled;
		CreateRenderTarget();
	}

	private void CreateRenderTarget()
	{
		RenderBuffer[] array = new RenderBuffer[colouredOverlayBufferEnabled ? 3 : 2];
		Textures[0] = RecreateRT(Textures[0], 24, RenderTextureFormat.ARGB32);
		Textures[0].filterMode = FilterMode.Point;
		Textures[0].name = "MRT0";
		TexturesCopies[0] = new RenderTexture(Textures[0]);
		TexturesCopies[0].name = "MRT0_Copy";
		Textures[1] = RecreateRT(Textures[1], 0, RenderTextureFormat.ARGB32);
		Textures[1].filterMode = FilterMode.Point;
		Textures[1].name = "MRT1";
		TexturesCopies[1] = new RenderTexture(Textures[1]);
		TexturesCopies[1].name = "MRT1_Copy";
		array[0] = Textures[0].colorBuffer;
		array[1] = Textures[1].colorBuffer;
		if (colouredOverlayBufferEnabled)
		{
			Textures[2] = RecreateRT(Textures[2], 0, RenderTextureFormat.ARGB32);
			Textures[2].filterMode = FilterMode.Bilinear;
			Textures[2].name = "MRT2";
			TexturesCopies[2] = new RenderTexture(Textures[2]);
			TexturesCopies[2].name = "MRT2_Copy";
			array[2] = Textures[2].colorBuffer;
		}
		GetComponent<Camera>().SetTargetBuffers(array, Textures[0].depthBuffer);
		OnShadersReloaded();
		OnTexturesRecreated?.Invoke();
	}

	private RenderTexture RecreateRT(RenderTexture rt, int depth, RenderTextureFormat format)
	{
		RenderTexture result = rt;
		if (rt == null || rt.width != Screen.width || rt.height != Screen.height || rt.format != format)
		{
			if (rt != null)
			{
				rt.DestroyRenderTexture();
			}
			result = new RenderTexture(Screen.width, Screen.height, depth, format);
		}
		return result;
	}

	private void OnResize()
	{
		CreateRenderTarget();
	}

	private void Update()
	{
		if (!Textures[0].IsCreated())
		{
			CreateRenderTarget();
		}
	}

	private void OnShadersReloaded()
	{
		Shader.SetGlobalTexture("_MRT0", Textures[0]);
		Shader.SetGlobalTexture("_MRT1", Textures[1]);
		if (colouredOverlayBufferEnabled)
		{
			Shader.SetGlobalTexture("_MRT2", Textures[2]);
		}
	}
}
