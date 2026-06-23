using System;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/SkyVisibilityVisualizer")]
public class SkyVisibilityVisualizer : KMonoBehaviour
{
	public Vector2I OriginOffset = new Vector2I(0, 0);

	public bool TwoWideOrgin = false;

	public int RangeMin;

	public int RangeMax;

	public int ScanVerticalStep = 0;

	public bool SkipOnModuleInteriors = false;

	public bool AllOrNothingVisibility = false;

	public Func<int, bool> SkyVisibilityCb = HasSkyVisibility;

	private static bool HasSkyVisibility(int cell)
	{
		return Grid.ExposedToSunlight[cell] >= 1;
	}
}
