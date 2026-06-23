using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class CodexEntry : IHasDlcRestrictions
{
	public EntryDevLog log = new EntryDevLog();

	private List<ContentContainer> _contentContainers = new List<ContentContainer>();

	private string _id;

	private string _parentId;

	private string _category;

	private string _title;

	private string _name;

	private string _subtitle;

	private List<SubEntry> _subEntries = new List<SubEntry>();

	private List<CodexEntry_MadeAndUsed> _contentMadeAndUsed = new List<CodexEntry_MadeAndUsed>();

	private Sprite _icon;

	private Color _iconColor = Color.white;

	private string _iconPrefabID;

	private string _iconLockID;

	private string _iconAssetName;

	private bool _disabled;

	private bool _searchOnly = false;

	private int _customContentLength = 0;

	private string _sortString;

	private bool _showBeforeGeneratedCategoryLinks;

	private bool _insertMergeContentAtBottom;

	public List<ContentContainer> contentContainers
	{
		get
		{
			return _contentContainers;
		}
		private set
		{
			_contentContainers = value;
		}
	}

	public string[] requiredAtLeastOneDlcIds { get; set; }

	public string[] requiredDlcIds { get; set; }

	public string[] forbiddenDlcIds { get; set; }

	public string id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	public string parentId
	{
		get
		{
			return _parentId;
		}
		set
		{
			_parentId = value;
		}
	}

	public string category
	{
		get
		{
			return _category;
		}
		set
		{
			_category = value;
		}
	}

	public string title
	{
		get
		{
			return _title;
		}
		set
		{
			_title = value;
		}
	}

	public string name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public string subtitle
	{
		get
		{
			return _subtitle;
		}
		set
		{
			_subtitle = value;
		}
	}

	public List<SubEntry> subEntries
	{
		get
		{
			return _subEntries;
		}
		set
		{
			_subEntries = value;
		}
	}

	public List<CodexEntry_MadeAndUsed> contentMadeAndUsed
	{
		get
		{
			return _contentMadeAndUsed;
		}
		set
		{
			_contentMadeAndUsed = value;
		}
	}

	public Sprite icon
	{
		get
		{
			return _icon;
		}
		set
		{
			_icon = value;
		}
	}

	public Color iconColor
	{
		get
		{
			return _iconColor;
		}
		set
		{
			_iconColor = value;
		}
	}

	public string iconPrefabID
	{
		get
		{
			return _iconPrefabID;
		}
		set
		{
			_iconPrefabID = value;
		}
	}

	public string iconLockID
	{
		get
		{
			return _iconLockID;
		}
		set
		{
			_iconLockID = value;
		}
	}

	public string iconAssetName
	{
		get
		{
			return _iconAssetName;
		}
		set
		{
			_iconAssetName = value;
		}
	}

	public bool disabled
	{
		get
		{
			return _disabled;
		}
		set
		{
			_disabled = value;
		}
	}

	public bool searchOnly
	{
		get
		{
			return _searchOnly;
		}
		set
		{
			_searchOnly = value;
		}
	}

	public int customContentLength
	{
		get
		{
			return _customContentLength;
		}
		set
		{
			_customContentLength = value;
		}
	}

	public string sortString
	{
		get
		{
			return _sortString;
		}
		set
		{
			_sortString = value;
		}
	}

	public bool showBeforeGeneratedCategoryLinks
	{
		get
		{
			return _showBeforeGeneratedCategoryLinks;
		}
		set
		{
			_showBeforeGeneratedCategoryLinks = value;
		}
	}

	public bool insertMergeContentAtBottom
	{
		get
		{
			return _insertMergeContentAtBottom;
		}
		set
		{
			_insertMergeContentAtBottom = value;
		}
	}

	public CodexEntry()
	{
	}

	public CodexEntry(string category, List<ContentContainer> contentContainers, string name)
	{
		this.category = category;
		this.name = name;
		this.contentContainers = contentContainers;
		if (string.IsNullOrEmpty(sortString))
		{
			sortString = UI.StripLinkFormatting(name);
		}
	}

	public CodexEntry(string category, string titleKey, List<ContentContainer> contentContainers)
	{
		this.category = category;
		title = titleKey;
		this.contentContainers = contentContainers;
		if (string.IsNullOrEmpty(sortString))
		{
			sortString = UI.StripLinkFormatting(title);
		}
	}

	public static List<string> ContentContainerDebug(List<ContentContainer> _contentContainers)
	{
		List<string> list = new List<string>();
		foreach (ContentContainer _contentContainer in _contentContainers)
		{
			if (_contentContainer != null)
			{
				string text = "<b>" + _contentContainer.contentLayout.ToString() + " container: " + ((_contentContainer.content != null) ? _contentContainer.content.Count : 0) + " items</b>";
				if (_contentContainer.content != null)
				{
					text += "\n";
					for (int i = 0; i < _contentContainer.content.Count; i++)
					{
						text = text + "    • " + _contentContainer.content[i].ToString() + ": " + GetContentWidgetDebugString(_contentContainer.content[i]) + "\n";
					}
				}
				list.Add(text);
			}
			else
			{
				list.Add("null container");
			}
		}
		return list;
	}

	private static string GetContentWidgetDebugString(ICodexWidget widget)
	{
		if (widget is CodexText { text: var text })
		{
			return text;
		}
		if (widget is CodexLabelWithIcon codexLabelWithIcon)
		{
			return codexLabelWithIcon.label.text + " / " + codexLabelWithIcon.icon.spriteName;
		}
		if (widget is CodexImage { spriteName: var spriteName })
		{
			return spriteName;
		}
		if (!(widget is CodexVideo { name: var result }))
		{
			if (widget is CodexIndentedLabelWithIcon codexIndentedLabelWithIcon)
			{
				return codexIndentedLabelWithIcon.label.text + " / " + codexIndentedLabelWithIcon.icon.spriteName;
			}
			return "";
		}
		return result;
	}

	public void CreateContentContainerCollection()
	{
		contentContainers = new List<ContentContainer>();
	}

	public void InsertContentContainer(int index, ContentContainer container)
	{
		contentContainers.Insert(index, container);
	}

	public void RemoveContentContainerAt(int index)
	{
		contentContainers.RemoveAt(index);
	}

	public void AddContentContainer(ContentContainer container)
	{
		contentContainers.Add(container);
	}

	public void AddContentContainerRange(IEnumerable<ContentContainer> containers)
	{
		contentContainers.AddRange(containers);
	}

	public void RemoveContentContainer(ContentContainer container)
	{
		contentContainers.Remove(container);
	}

	public ICodexWidget GetFirstWidget()
	{
		for (int i = 0; i < contentContainers.Count; i++)
		{
			if (contentContainers[i].content == null)
			{
				continue;
			}
			for (int j = 0; j < contentContainers[i].content.Count; j++)
			{
				if (contentContainers[i].content[j] != null && Game.IsCorrectDlcActiveForCurrentSave(contentContainers[i].content[j] as IHasDlcRestrictions))
				{
					return contentContainers[i].content[j];
				}
			}
		}
		return null;
	}

	public string[] GetRequiredDlcIds()
	{
		return requiredDlcIds;
	}

	public string[] GetForbiddenDlcIds()
	{
		return forbiddenDlcIds;
	}

	public string[] GetAnyRequiredDlcIds()
	{
		return requiredAtLeastOneDlcIds;
	}
}
