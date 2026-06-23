using UnityEngine;

public class KAnimActivePostProcessingEffects : KMonoBehaviour
{
	private KAnimConverter.PostProcessingEffects currentActiveEffects;

	public void EnableEffect(KAnimConverter.PostProcessingEffects effect_flag)
	{
		currentActiveEffects |= effect_flag;
	}

	public void DisableEffect(KAnimConverter.PostProcessingEffects effect_flag)
	{
		if (IsEffectActive(effect_flag))
		{
			currentActiveEffects ^= effect_flag;
		}
	}

	public bool IsEffectActive(KAnimConverter.PostProcessingEffects effect_flag)
	{
		return (currentActiveEffects & effect_flag) != 0;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination);
		if (currentActiveEffects != 0)
		{
			KAnimBatchManager.Instance().RenderKAnimPostProcessingEffects(currentActiveEffects);
		}
	}
}
