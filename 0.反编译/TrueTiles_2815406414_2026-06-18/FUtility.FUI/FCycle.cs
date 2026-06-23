using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FUtility.FUI;

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

	private int currentIndex;

	[SerializeField]
	public List<Option> Options;

	private bool HasOptions => Options.Count > 0;

	public string Value
	{
		get
		{
			if (Options.Count < currentIndex)
			{
				return null;
			}
			return Options[currentIndex].id;
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
					Log.Warning("Invalid option ID given \"" + value + "\"");
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

	protected override void OnSpawn()
	{
		((KMonoBehaviour)this).OnSpawn();
		UpdateLabel();
	}

	public void CycleLeft()
	{
		if (HasOptions)
		{
			currentIndex = (currentIndex + Options.Count - 1) % Options.Count;
			UpdateLabel();
			this.OnChange?.Invoke();
		}
	}

	public void CycleRight()
	{
		if (HasOptions)
		{
			currentIndex = (currentIndex + 1) % Options.Count;
			UpdateLabel();
			this.OnChange?.Invoke();
		}
	}

	public void UpdateLabel()
	{
		if (Options.Count >= currentIndex)
		{
			Value = Options[currentIndex].id;
			((TMP_Text)label).SetText(Options[currentIndex].title);
			if ((Object)(object)description != (Object)null)
			{
				((TMP_Text)description).SetText(Options[currentIndex].description);
			}
		}
	}
}
