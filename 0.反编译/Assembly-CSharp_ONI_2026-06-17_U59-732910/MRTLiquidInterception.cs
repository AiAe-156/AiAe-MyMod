using System;
using UnityEngine;
using UnityEngine.Rendering;

public class MRTLiquidInterception : MonoBehaviour
{
	private MultipleRenderTargetProxy mrt;

	private CommandBuffer cb;

	private CameraEvent interceptionTime = CameraEvent.BeforeForwardOpaque;

	private RenderTargetIdentifier[] mrts_identifier = new RenderTargetIdentifier[3];

	private RenderTargetIdentifier[] mrts_identifier_2 = new RenderTargetIdentifier[2];

	private void Start()
	{
		mrt = GetComponent<MultipleRenderTargetProxy>();
		MultipleRenderTargetProxy multipleRenderTargetProxy = mrt;
		multipleRenderTargetProxy.OnTexturesRecreated = (System.Action)Delegate.Combine(multipleRenderTargetProxy.OnTexturesRecreated, new System.Action(RecreateCommandBuffer));
		RecreateCommandBuffer();
	}

	private void RecreateCommandBuffer()
	{
		if (cb != null)
		{
			CameraController.Instance.baseCamera.RemoveCommandBuffer(interceptionTime, cb);
			cb.Clear();
		}
		else
		{
			cb = new CommandBuffer();
			cb.name = "Get MRT1 before rendering liquid";
		}
		cb.Blit(mrt.Textures[0], mrt.TexturesCopies[0]);
		cb.Blit(mrt.Textures[1], mrt.TexturesCopies[1]);
		cb.SetGlobalTexture(mrt.TexturesCopies[0].name, mrt.TexturesCopies[0]);
		cb.SetGlobalTexture(mrt.TexturesCopies[1].name, mrt.TexturesCopies[1]);
		mrts_identifier[0] = mrt.Textures[0];
		mrts_identifier[1] = mrt.Textures[1];
		mrts_identifier[2] = mrt.Textures[2];
		mrts_identifier_2[0] = mrt.Textures[0];
		mrts_identifier_2[1] = mrt.Textures[1];
		cb.SetRenderTarget(mrt.IsColouredOverlayBufferEnabled ? mrts_identifier : mrts_identifier_2, mrt.Textures[0].depthBuffer);
		cb.DrawRenderer(WaterCubes.Instance.waterRenderer, WaterCubes.Instance.material);
		CameraController.Instance.baseCamera.AddCommandBuffer(interceptionTime, cb);
	}
}
