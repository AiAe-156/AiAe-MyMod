using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("KMonoBehaviour/scripts/PopFX")]
public class PopFX : KMonoBehaviour
{
	public const float Speed = 2f;

	private Sprite mainIcon;

	private Sprite icon;

	private string text;

	private Transform targetTransform;

	private Vector3 offset;

	private Vector3 canvasPaddingMultiplier;

	public RectTransform Pivot;

	public Image bg;

	public Image MainIconDisplay;

	public Image IconDisplay;

	public Image mask;

	public LocText TextDisplay;

	public CanvasGroup canvasGroup;

	private Camera uiCamera;

	private float lifetime;

	private float lifeElapsed = 0f;

	private bool trackTarget = false;

	private bool positionToGroup = true;

	private Vector3 startPos;

	private bool isLive = false;

	private bool isActiveWorld = false;

	private int eventid = -1;

	private static Action<object, object> OnActiveWorldChangedDispatcher = delegate(object context, object data)
	{
		Unsafe.As<PopFX>(context).OnActiveWorldChanged(data);
	};

	public Vector3 StartPos => startPos;

	public void Recycle()
	{
		icon = null;
		mainIcon = null;
		text = "";
		targetTransform = null;
		lifeElapsed = 0f;
		trackTarget = false;
		startPos = Vector3.zero;
		positionToGroup = true;
		canvasPaddingMultiplier = Vector3.zero;
		IconDisplay.color = Color.white;
		TextDisplay.color = Color.white;
		MainIconDisplay.color = Color.white;
		PopFXManager.Instance.RecycleFX(this);
		canvasGroup.alpha = 0f;
		IconDisplay.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: false);
		isLive = false;
		isActiveWorld = false;
		Game.Instance.Unsubscribe(ref eventid);
	}

	public void SetIconTint(Color color)
	{
		MainIconDisplay.color = color;
	}

	public void Run(Vector3 groupSpawnPosition, Vector3 canvasPaddingMultiplier)
	{
		base.gameObject.SetActive(value: true);
		this.canvasPaddingMultiplier = canvasPaddingMultiplier;
		if (positionToGroup && groupSpawnPosition != PopFxGroup.INVALID_SPAWN_POSITION)
		{
			startPos = groupSpawnPosition;
		}
		if (trackTarget && targetTransform != null)
		{
			startPos = targetTransform.GetPosition();
			Grid.PosToXY(startPos, out var _, out var _);
			startPos.x -= 0.5f;
		}
		TextDisplay.text = text;
		IconDisplay.sprite = icon;
		IconDisplay.Opacity(1f);
		MainIconDisplay.Opacity(1f);
		MainIconDisplay.sprite = mainIcon;
		IconDisplay.gameObject.SetActive(icon != null);
		canvasGroup.alpha = 1f;
		isLive = true;
		eventid = Game.Instance.Subscribe(1983128072, OnActiveWorldChangedDispatcher, this);
		SetWorldActive(ClusterManager.Instance.activeWorldId);
		Update();
	}

	public void Setup(Sprite MainIcon, Sprite SecondaryIcon, string Text, Transform TargetTransform, Vector3 Offset, bool PositionToGroup, float LifeTime = 1.5f, bool TrackTarget = false)
	{
		mainIcon = MainIcon;
		icon = SecondaryIcon;
		text = Text;
		targetTransform = TargetTransform;
		trackTarget = TrackTarget;
		lifetime = LifeTime;
		offset = Offset;
		positionToGroup = PositionToGroup;
		if (targetTransform != null)
		{
			startPos = targetTransform.GetPosition();
		}
		Grid.PosToXY(startPos, out var _, out var _);
		startPos.x -= 0.5f;
	}

	private void OnActiveWorldChanged(object data)
	{
		Tuple<int, int> tuple = (Tuple<int, int>)data;
		if (isLive)
		{
			SetWorldActive(tuple.first);
		}
	}

	private void SetWorldActive(int worldId)
	{
		Vector3 pos = ((trackTarget && targetTransform != null) ? targetTransform.position : (startPos + offset));
		int num = Grid.PosToCell(pos);
		isActiveWorld = !Grid.IsValidCell(num) || Grid.WorldIdx[num] == worldId;
	}

	private void Update()
	{
		if (isLive && PopFXManager.Instance.Ready())
		{
			lifeElapsed += Time.unscaledDeltaTime;
			if (lifeElapsed >= lifetime)
			{
				Recycle();
			}
			if (trackTarget && targetTransform != null)
			{
				Vector3 vector = PopFXManager.Instance.WorldToScreen(targetTransform.GetPosition() + offset + Vector3.up * lifeElapsed * (2f * lifeElapsed));
				vector.z = 0f;
				base.gameObject.rectTransform().anchoredPosition = vector;
			}
			else
			{
				Vector3 vector2 = PopFXManager.Instance.WorldToScreen(startPos + offset + Vector3.up * lifeElapsed * (2f * (lifeElapsed / 2f)));
				vector2.z = 0f;
				Vector3 vector3 = Pivot.rect.size;
				vector3.x *= canvasPaddingMultiplier.x;
				vector3.y *= canvasPaddingMultiplier.y;
				vector3.z *= canvasPaddingMultiplier.z;
				vector2 += vector3;
				base.gameObject.rectTransform().anchoredPosition = vector2;
			}
			float num = (CameraController.Instance.FreeCameraEnabled ? TuningData<CameraController.Tuning>.Get().maxOrthographicSizeDebug : 20f);
			float t = (CameraController.Instance.OrthographicSize - CameraController.Instance.minOrthographicSize) / (num - CameraController.Instance.minOrthographicSize);
			base.gameObject.rectTransform().localScale = Vector3.one * Mathf.Lerp(1f, 0.7f, t);
			float num2 = Mathf.Clamp01((lifetime - lifeElapsed) / lifetime);
			float t2 = Mathf.Clamp01((1f - num2) / 0.1f);
			float num3 = Mathf.Clamp01(num2 / 0.2f);
			mask.fillAmount = Mathf.Lerp(0.16f * num3, 1f, t2);
			canvasGroup.alpha = (isActiveWorld ? num3 : 0f);
		}
	}
}
