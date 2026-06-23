namespace UtilLibs.UI.FUI.Unity_UI_Extensions.Scripts.Animation;

internal interface ITweenValue
{
	bool ignoreTimeScale { get; }

	float duration { get; }

	void TweenValue(float floatPercentage);

	bool ValidTarget();
}
