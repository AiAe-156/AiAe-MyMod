using System;
using System.Collections.Generic;
using System.Reflection;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// An options entry that encapsulates other options. The category annotation on those
/// objects will be ignored, and the category of the Option attribute on the property
/// that declared those options (to avoid infinite loops) will be used instead.
///
/// <b>This object is not in the scene graph.</b> Any events in OnRealize will never be
/// invoked, and it is never "built".
/// </summary>
internal class CompositeOptionsEntry : OptionsEntry
{
	/// <summary>
	/// The options encapsulated in this object.
	/// </summary>
	protected readonly IDictionary<PropertyInfo, IOptionsEntry> subOptions;

	/// <summary>
	/// The type of the encapsulated object.
	/// </summary>
	protected readonly Type targetType;

	/// <summary>
	/// The object thus wrapped.
	/// </summary>
	protected object value;

	/// <summary>
	/// Reports the number of options contained inside this one.
	/// </summary>
	public int ChildCount => subOptions.Count;

	public override object Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value != null && targetType.IsAssignableFrom(value.GetType()))
			{
				this.value = value;
			}
		}
	}

	/// <summary>
	/// Creates an options entry wrapper for the specified property, iterating its internal
	/// fields to create sub-options if needed (recursively).
	/// </summary>
	/// <param name="info">The property to wrap.</param>
	/// <param name="spec">The option title and tool tip.</param>
	/// <param name="depth">The current depth of iteration to avoid infinite loops.</param>
	/// <returns>An options wrapper, or null if no inner properties are themselves options.</returns>
	internal static CompositeOptionsEntry Create(IOptionSpec spec, PropertyInfo info, int depth)
	{
		Type propertyType = info.PropertyType;
		CompositeOptionsEntry compositeOptionsEntry = new CompositeOptionsEntry(info.Name, spec, propertyType);
		PropertyInfo[] properties = propertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			IOptionsEntry optionsEntry = OptionsEntry.TryCreateEntry(propertyInfo, depth + 1);
			if (optionsEntry != null)
			{
				compositeOptionsEntry.AddField(propertyInfo, optionsEntry);
			}
		}
		if (compositeOptionsEntry.ChildCount <= 0)
		{
			return null;
		}
		return compositeOptionsEntry;
	}

	public CompositeOptionsEntry(string field, IOptionSpec spec, Type fieldType)
		: base(field, spec)
	{
		subOptions = new Dictionary<PropertyInfo, IOptionsEntry>(16);
		targetType = fieldType ?? throw new ArgumentNullException("fieldType");
		value = OptionsDialog.CreateOptions(fieldType);
	}

	/// <summary>
	/// Adds an options entry object that operates on Option fields of the encapsulated
	/// object.
	/// </summary>
	/// <param name="info">The property that is wrapped.</param>
	/// <param name="entry">The entry to add.</param>
	public void AddField(PropertyInfo info, IOptionsEntry entry)
	{
		if (entry == null)
		{
			throw new ArgumentNullException("entry");
		}
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		subOptions.Add(info, entry);
	}

	public override void CreateUIEntry(PGridPanel parent, ref int row)
	{
		int row2 = row;
		bool flag = true;
		parent.AddOnRealize(WhenRealized);
		foreach (KeyValuePair<PropertyInfo, IOptionsEntry> subOption in subOptions)
		{
			if (!flag)
			{
				row2++;
				parent.AddRow(new GridRowSpec());
			}
			subOption.Value.CreateUIEntry(parent, ref row2);
			flag = false;
		}
		row = row2;
	}

	public override GameObject GetUIComponent()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		return new GameObject("Empty");
	}

	public override void ReadFrom(object settings)
	{
		base.ReadFrom(settings);
		foreach (KeyValuePair<PropertyInfo, IOptionsEntry> subOption in subOptions)
		{
			subOption.Value.ReadFrom(value);
		}
	}

	public override string ToString()
	{
		return "{1}[field={0},title={2},children=[{3}]]".F(base.Field, GetType().Name, base.Title, subOptions.Join());
	}

	/// <summary>
	/// Updates the child objects for the first time when the panel is realized.
	/// </summary>
	private void WhenRealized(GameObject _)
	{
		foreach (KeyValuePair<PropertyInfo, IOptionsEntry> subOption in subOptions)
		{
			subOption.Value.ReadFrom(value);
		}
	}

	public override bool WriteTo(object settings)
	{
		bool flag = false;
		foreach (KeyValuePair<PropertyInfo, IOptionsEntry> subOption in subOptions)
		{
			flag |= subOption.Value.WriteTo(value);
		}
		return flag | base.WriteTo(settings);
	}
}
