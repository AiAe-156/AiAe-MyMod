using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomActiveScreenPostProcessingEffects : KMonoBehaviour
{
	private List<Func<RenderTexture, Material>> ActiveEffects = new List<Func<RenderTexture, Material>>();

	private RenderTexture previousSource = null;

	private RenderTexture previousDestination = null;

	public void RegisterEffect(Func<RenderTexture, Material> effectFn)
	{
		ActiveEffects.Add(effectFn);
	}

	public void UnregisterEffect(Func<RenderTexture, Material> effectFn)
	{
		ActiveEffects.Remove(effectFn);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ActiveEffects.Count > 0)
		{
			CheckTemporaryRenderTextureValidity(ref previousSource, source);
			CheckTemporaryRenderTextureValidity(ref previousDestination, source);
			Graphics.Blit(source, previousSource);
			foreach (Func<RenderTexture, Material> activeEffect in ActiveEffects)
			{
				Graphics.Blit(previousSource, previousDestination, activeEffect(source));
				previousSource.DiscardContents();
				Graphics.Blit(previousDestination, previousSource);
			}
			Graphics.Blit(previousSource, destination);
		}
		else
		{
			Graphics.Blit(source, destination);
		}
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		previousSource.Release();
		previousDestination.Release();
	}

	private void CheckTemporaryRenderTextureValidity(ref RenderTexture temporaryTexture, RenderTexture source)
	{
		if (temporaryTexture == null || temporaryTexture.width != source.width || temporaryTexture.height != source.height || temporaryTexture.depth != source.depth || temporaryTexture.format != source.format)
		{
			if (temporaryTexture != null)
			{
				temporaryTexture.Release();
			}
			temporaryTexture = RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format);
		}
	}
}
