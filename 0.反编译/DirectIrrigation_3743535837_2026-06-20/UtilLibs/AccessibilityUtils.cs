using UnityEngine;

namespace UtilLibs;

public static class AccessibilityUtils
{
	private static Color _logicBad;

	private static Color _logicWarning;

	private static Color _logicGood;

	public static Color LogicBad
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if (_logicBad == default(Color))
			{
				InitializeColors();
			}
			return _logicBad;
		}
	}

	public static Color LogicWarning
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if (_logicWarning == default(Color))
			{
				InitializeColors();
			}
			return _logicWarning;
		}
	}

	public static Color LogicGood
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if (_logicGood == default(Color))
			{
				InitializeColors();
			}
			return _logicGood;
		}
	}

	private static void InitializeColors()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		Color logicGood = Color32.op_Implicit(GlobalAssets.Instance.colorSet.logicOn);
		logicGood.a = 1f;
		_logicGood = logicGood;
		Color logicBad = Color32.op_Implicit(GlobalAssets.Instance.colorSet.logicOff);
		logicBad.a = 1f;
		_logicBad = logicBad;
		Color logicWarning = Color32.op_Implicit(GlobalAssets.Instance.colorSet.cropGrowing);
		logicWarning.a = 1f;
		_logicWarning = logicWarning;
	}
}
