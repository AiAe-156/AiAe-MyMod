using STRINGS;
using UnityEngine;

public class BackwallSelectionObject : KMonoBehaviour, ICellSelectionProxy
{
	private static BackwallSelectionObject instance;

	private KSelectable mSelectable;

	private KBoxCollider2D mCollider;

	private int selectedCell;

	private float updateTimer;

	public Element element;

	public float Mass;

	public float temperature;

	private static readonly Vector3 offset = new Vector3(0.5f, 0.5f, 0f);

	private float zDepth = Grid.GetLayerZ(Grid.SceneLayer.WorldSelection) + -0.5f;

	private bool isAppFocused = true;

	public static BackwallSelectionObject Instance => instance;

	public int SelectedCell => selectedCell;

	Element ICellSelectionProxy.Element => element;

	protected override void OnPrefabInit()
	{
		instance = this;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		mSelectable = GetComponent<KSelectable>();
		mSelectable.IsSelectable = true;
		mCollider = GetComponent<KBoxCollider2D>();
		mCollider.size = new Vector2(1.1f, 1.1f);
		Subscribe(Game.Instance.gameObject, -1503271301, OnObjectSelected);
	}

	protected override void OnCleanUp()
	{
		instance = null;
		base.OnCleanUp();
	}

	private void OnApplicationFocus(bool focusStatus)
	{
		isAppFocused = focusStatus;
	}

	private void Update()
	{
		if (!isAppFocused || SelectTool.Instance == null || Game.Instance == null || !Game.Instance.GameStarted() || !PlayerController.Instance.IsUsingDefaultTool())
		{
			return;
		}
		if (SelectTool.Instance.selected == mSelectable)
		{
			updateTimer += Time.deltaTime;
			if (updateTimer >= 0.5f)
			{
				updateTimer = 0f;
				UpdateValues();
			}
			return;
		}
		int num = Grid.PosToCell(CameraController.Instance.baseCamera.ScreenToWorldPoint(KInputManager.GetMousePos()));
		bool flag = Grid.IsValidCell(num) && Grid.IsVisible(num) && BackwallManager.HasBackwall(num);
		mCollider.enabled = flag;
		if (flag)
		{
			Vector3 position = Grid.CellToPos(num, 0f, 0f, 0f) + offset;
			position.z = zDepth;
			base.transform.SetPosition(position);
			mSelectable.SetName(BackwallManager.At(num).Element.nameUpperCase + " " + UI.TOOLS.GENERIC.NATURAL_BACKWALL_LABEL);
		}
	}

	public void OnObjectSelected(object o)
	{
		if (!(SelectTool.Instance.selected != mSelectable))
		{
			selectedCell = Grid.PosToCell(base.gameObject);
			updateTimer = 0f;
			UpdateValues();
		}
	}

	public void UpdateValues()
	{
		GameObject gameObject = Grid.Objects[selectedCell, 2];
		if (BackwallManager.HasBackwall(selectedCell))
		{
			element = BackwallManager.At(SelectedCell).Element;
			Mass = BackwallManager.At(SelectedCell).Mass;
			temperature = BackwallManager.At(SelectedCell).Temperature;
		}
		else
		{
			if (!(gameObject != null))
			{
				return;
			}
			PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
			if (component == null)
			{
				return;
			}
			element = component.Element;
			Mass = component.Mass;
			temperature = component.Temperature;
		}
		mSelectable.SetName(element.name + " " + UI.TOOLS.GENERIC.NATURAL_BACKWALL_LABEL_TITLECASE);
		if (!mSelectable.HasStatusItem(Db.Get().MiscStatusItems.BackwallMass))
		{
			mSelectable.AddStatusItem(Db.Get().MiscStatusItems.BackwallMass, this);
		}
		if (!mSelectable.HasStatusItem(Db.Get().MiscStatusItems.BackwallTemperature))
		{
			mSelectable.AddStatusItem(Db.Get().MiscStatusItems.BackwallTemperature, this);
		}
	}

	public static bool IsBackwallSelectionObject(GameObject go)
	{
		return instance != null && go == instance.gameObject;
	}
}
