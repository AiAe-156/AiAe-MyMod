using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UtilLibs.UIcmp;

public class FCycle : KMonoBehaviour
{
	[Serializable]
	public class Option
	{
		public string id;

		public string title;

		public string description;

		public Option(string id, string title, string description = null)
		{
			this.id = id;
			this.title = title;
			this.description = description;
		}
	}

	[SerializeField]
	public FButton leftArrow;

	[SerializeField]
	public FButton rightArrow;

	[SerializeField]
	public LocText label;

	[SerializeField]
	public LocText description;

	private int currentIndex = 0;

	[SerializeField]
	public List<Option> Options;

	[SerializeField]
	public Func<string, string> DescriptionFormatter = null;

	[SerializeField]
	public Func<string, string> NameFormatter = null;

	private bool _isInteractable = true;

	public bool IsInteractable
	{
		get
		{
			return _isInteractable;
		}
		set
		{
			_isInteractable = value;
			leftArrow.SetInteractable(value);
			rightArrow.SetInteractable(value);
		}
	}

	private bool HasOptions => Options.Count > 0;

	public string Value
	{
		get
		{
			return (Options.Count >= currentIndex) ? Options[currentIndex].id : null;
		}
		set
		{
			int num = Options.FindIndex((Option x) => x.id == value);
			if (currentIndex != num)
			{
				if (num != -1)
				{
					currentIndex = num;
				}
				else
				{
					SgtLogger.warning("Invalid option ID given \"" + value + "\"");
					currentIndex = 0;
				}
				UpdateLabel();
			}
		}
	}

	public event Action OnChange;

	public void Initialize(FButton leftButton, FButton rightButton, LocText label, LocText description = null)
	{
		leftArrow = leftButton;
		rightArrow = rightButton;
		this.label = label;
		this.description = description;
		leftArrow.OnClick += CycleLeft;
		rightArrow.OnClick += CycleRight;
	}

	public override void OnSpawn()
	{
		((KMonoBehaviour)this).OnSpawn();
		UpdateLabel();
	}

	public void SetInteractable(bool interactable)
	{
		IsInteractable = interactable;
	}

	public void CycleLeft()
	{
		if (HasOptions && IsInteractable)
		{
			currentIndex = (currentIndex + Options.Count - 1) % Options.Count;
			UpdateLabel();
			this.OnChange?.Invoke();
		}
	}

	public void CycleRight()
	{
		if (HasOptions && IsInteractable)
		{
			currentIndex = (currentIndex + 1) % Options.Count;
			UpdateLabel();
			this.OnChange?.Invoke();
		}
	}

	public void UpdateLabel()
	{
		if (Options.Count < currentIndex)
		{
			return;
		}
		Value = Options[currentIndex].id;
		string text = Options[currentIndex].title;
		if (NameFormatter != null)
		{
			text = NameFormatter(text);
		}
		((TMP_Text)label).SetText(text);
		if ((Object)(object)description != (Object)null)
		{
			string text2 = Options[currentIndex].description;
			if (DescriptionFormatter != null)
			{
				text2 = DescriptionFormatter(text2);
			}
			((TMP_Text)description).SetText(text2);
		}
	}

	public void SetDescriptionFormatter(Func<string, string> formatDescription)
	{
		DescriptionFormatter = formatDescription;
	}

	public void SetNameFormatter(Func<string, string> formatName)
	{
		NameFormatter = formatName;
	}
}
