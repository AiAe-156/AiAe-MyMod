public class DelayedUprootable : Uprootable
{
	public HashedString deathAnimation;

	public override void Uproot()
	{
		if (deathAnimation.IsValid && TryGetComponent<KBatchedAnimController>(out var component))
		{
			component.Play(deathAnimation);
			component.onAnimComplete += delegate
			{
				FinalizeUproot();
			};
		}
		else
		{
			FinalizeUproot();
		}
	}

	private void FinalizeUproot()
	{
		base.Uproot();
	}
}
