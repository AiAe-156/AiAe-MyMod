using System;
using UnityEngine;

public class LargeImpactorVisualizer : KMonoBehaviour
{
	public bool Active;

	private const string SFX_Fold = "HUD_Demolior_LandingZone_close_fx";

	public Vector2I OriginOffset;

	public Vector2 ScreenSpaceNotificationTogglePosition = Vector2.zero;

	public Vector2I RangeMin;

	public Vector2I RangeMax;

	public Vector2I TexSize = new Vector2I(64, 64);

	public bool TestLineOfSight;

	public bool BlockingTileVisible;

	public Func<int, bool> BlockingVisibleCb;

	public Func<int, bool> BlockingCb = Grid.IsSolidCell;

	public bool AllowLineOfSightInvalidCells;

	public bool Visible
	{
		get
		{
			if (Active)
			{
				return !Folded;
			}
			return false;
		}
	}

	public bool Folded { get; private set; } = true;

	public float LastTimeSetToFolded { get; private set; }

	public bool ShouldResetEntryEffect { get; private set; }

	public float EntryEffectDuration { get; private set; } = 3f;

	public float FoldEffectDuration { get; private set; } = 1f;

	public void BeginEntryEffect(float duration)
	{
		EntryEffectDuration = duration;
		SetShouldResetEntryEffect(shouldIt: true);
	}

	public void SetShouldResetEntryEffect(bool shouldIt)
	{
		ShouldResetEntryEffect = shouldIt;
	}

	public void SetFoldedState(bool shouldBeFolded)
	{
		if (!Folded && shouldBeFolded)
		{
			LastTimeSetToFolded = Time.unscaledTime;
			if (Active)
			{
				KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Demolior_LandingZone_close_fx"));
			}
		}
		Folded = shouldBeFolded;
		if (!shouldBeFolded)
		{
			LastTimeSetToFolded = float.MaxValue;
		}
	}
}
