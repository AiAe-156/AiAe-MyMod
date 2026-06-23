using System;
using System.Collections.Generic;
using System.Reflection;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using STRINGS;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// An abstract parent class containing methods shared by all built-in options handlers.
/// </summary>
public abstract class OptionsEntry : IOptionsEntry, IOptionSpec, IComparable<OptionsEntry>, IUIComponent
{
	private const BindingFlags INSTANCE_PUBLIC = BindingFlags.Instance | BindingFlags.Public;

	/// <summary>
	/// The margins around the control used in each entry.
	/// </summary>
	protected static readonly RectOffset CONTROL_MARGIN = new RectOffset(0, 0, 2, 2);

	/// <summary>
	/// The margins around the label for each entry.
	/// </summary>
	protected static readonly RectOffset LABEL_MARGIN = new RectOffset(0, 5, 2, 2);

	/// <summary>
	/// The category for this entry.
	/// </summary>
	public string Category { get; }

	/// <summary>
	/// The option field name.
	/// </summary>
	public string Field { get; }

	/// <summary>
	/// The format string to use when rendering this option, or null if none was supplied.
	/// </summary>
	public string Format { get; }

	public virtual string Name => "OptionsEntry";

	public bool RestartRequired { get; set; }

	/// <summary>
	/// The option title on screen.
	/// </summary>
	public string Title { get; protected set; }

	/// <summary>
	/// The tool tip to display.
	/// </summary>
	public string Tooltip { get; protected set; }

	/// <summary>
	/// The current value selected by the user.
	/// </summary>
	public abstract object Value { get; set; }

	public event PUIDelegates.OnRealize OnRealize;

	/// <summary>
	/// Adds an options entry to the category list, creating a new category if necessary.
	/// </summary>
	/// <param name="entries">The existing categories.</param>
	/// <param name="entry">The option entry to add.</param>
	internal static void AddToCategory(IDictionary<string, ICollection<IOptionsEntry>> entries, IOptionsEntry entry)
	{
		string key = entry.Category ?? "";
		if (!entries.TryGetValue(key, out var value))
		{
			value = new List<IOptionsEntry>(16);
			entries.Add(key, value);
		}
		value.Add(entry);
	}

	/// <summary>
	/// Builds the options entries from the type.
	/// </summary>
	/// <param name="forType">The type of the options class.</param>
	/// <returns>A list of all public properties annotated for options dialogs.</returns>
	internal static IDictionary<string, ICollection<IOptionsEntry>> BuildOptions(Type forType)
	{
		SortedList<string, ICollection<IOptionsEntry>> sortedList = new SortedList<string, ICollection<IOptionsEntry>>(8);
		OptionsHandlers.InitPredefinedOptions();
		PropertyInfo[] properties = forType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		for (int i = 0; i < properties.Length; i++)
		{
			IOptionsEntry optionsEntry = TryCreateEntry(properties[i], 0);
			if (optionsEntry != null)
			{
				AddToCategory(sortedList, optionsEntry);
			}
		}
		return sortedList;
	}

	/// <summary>
	/// Creates a default UI entry. This entry will have the title and tool tip in the
	/// first column, and the provided UI component in the second column. Only one row is
	/// added by this method.
	/// </summary>
	/// <param name="entry">The options entry to be presented.</param>
	/// <param name="parent">The parent where the components will be added.</param>
	/// <param name="row">The row index where the components will be added.</param>
	/// <param name="presenter">The presenter that can display this option's value.</param>
	public static void CreateDefaultUIEntry(IOptionsEntry entry, PGridPanel parent, int row, IUIComponent presenter)
	{
		parent.AddChild(new PLabel("Label")
		{
			Text = LookInStrings(entry.Title),
			ToolTip = LookInStrings(entry.Tooltip),
			TextStyle = PUITuning.Fonts.TextLightStyle
		}, new GridComponentSpec(row, 0)
		{
			Margin = LABEL_MARGIN,
			Alignment = (TextAnchor)3
		});
		parent.AddChild(presenter, new GridComponentSpec(row, 1)
		{
			Alignment = (TextAnchor)5,
			Margin = CONTROL_MARGIN
		});
	}

	/// <summary>
	/// Creates a dynamic options entry.
	/// </summary>
	/// <param name="prop">The property to be created.</param>
	/// <param name="handler">The type which can handle the property.</param>
	/// <returns>The created entry, or null if no entry could be created.</returns>
	private static IOptionsEntry CreateDynamicOption(PropertyInfo prop, Type handler)
	{
		IOptionsEntry optionsEntry = null;
		ConstructorInfo[] constructors = handler.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
		string name = prop.Name;
		int num = constructors.Length;
		for (int i = 0; i < num; i++)
		{
			if (optionsEntry != null)
			{
				break;
			}
			try
			{
				if (ExecuteConstructor(prop, constructors[i]) is IOptionsEntry optionsEntry2)
				{
					optionsEntry = optionsEntry2;
				}
			}
			catch (TargetInvocationException ex)
			{
				PUtil.LogError("Unable to create option handler for property " + name + ":");
				PUtil.LogException(ex.GetBaseException());
			}
			catch (MemberAccessException)
			{
			}
			catch (AmbiguousMatchException)
			{
			}
			catch (TypeLoadException ex4)
			{
				PUtil.LogError("Unable to instantiate option handler for property " + name + ":");
				PUtil.LogException(ex4.GetBaseException());
			}
		}
		if (optionsEntry == null)
		{
			PUtil.LogWarning("Unable to create option handler for property " + name + ", it must have a public constructor");
		}
		return optionsEntry;
	}

	/// <summary>
	/// Runs a dynamic option constructor.
	/// </summary>
	/// <param name="prop">The property to be created.</param>
	/// <param name="cons">The constructor to run.</param>
	/// <returns>The constructed dynamic option.</returns>
	private static object ExecuteConstructor(PropertyInfo prop, ConstructorInfo cons)
	{
		ParameterInfo[] parameters = cons.GetParameters();
		int num = parameters.Length;
		if (num == 0)
		{
			return cons.Invoke(null);
		}
		object[] array = new object[num];
		for (int i = 0; i < num; i++)
		{
			Type parameterType = parameters[i].ParameterType;
			if (typeof(Attribute).IsAssignableFrom(parameterType))
			{
				array[i] = prop.GetCustomAttribute(parameterType);
			}
			else if (parameterType == typeof(IOptionSpec))
			{
				array[i] = prop.GetCustomAttribute<OptionAttribute>();
			}
			else if (parameterType == typeof(string))
			{
				array[i] = prop.Name;
			}
			else
			{
				PUtil.LogWarning("DynamicOption cannot handle constructor parameter of type " + parameterType.FullName);
			}
		}
		return cons.Invoke(array);
	}

	/// <summary>
	/// Substitutes default strings for an options entry with an empty title.
	/// </summary>
	/// <param name="spec">The option attribute supplied (Format is still accepted!)</param>
	/// <param name="member">The item declaring the attribute.</param>
	/// <returns>A substitute attribute with default values from STRINGS.</returns>
	internal static IOptionSpec HandleDefaults(IOptionSpec spec, MemberInfo member)
	{
		string text = "STRINGS.{0}.OPTIONS.{1}.".F(member.DeclaringType?.Namespace?.ToUpperInvariant(), member.Name.ToUpperInvariant());
		string category = "";
		StringEntry val = default(StringEntry);
		if (Strings.TryGet(text + "CATEGORY", ref val))
		{
			category = val.String;
		}
		return new OptionAttribute(text + "NAME", text + "TOOLTIP", category)
		{
			Format = spec.Format
		};
	}

	/// <summary>
	/// First looks to see if the string exists in the string database; if it does, returns
	/// the localized value, otherwise returns the string unmodified.
	///
	/// This method is somewhat slow. Cache the result if possible.
	/// </summary>
	/// <param name="keyOrValue">The string key to check.</param>
	/// <returns>The string value with that key, or the key if there is no such localized
	/// string value.</returns>
	public static string LookInStrings(string keyOrValue)
	{
		string result = keyOrValue;
		StringEntry val = default(StringEntry);
		if (!string.IsNullOrEmpty(keyOrValue) && Strings.TryGet(keyOrValue, ref val))
		{
			result = UI.StripLinkFormatting(val.String);
		}
		return result;
	}

	/// <summary>
	/// Shared code to create an options entry if an [Option] attribute is found on a
	/// property.
	/// </summary>
	/// <param name="prop">The property to inspect.</param>
	/// <param name="depth">The current depth of iteration to avoid infinite loops.</param>
	/// <returns>The OptionsEntry created, or null if none was.</returns>
	internal static IOptionsEntry TryCreateEntry(PropertyInfo prop, int depth)
	{
		IOptionsEntry optionsEntry = null;
		if (prop.GetIndexParameters().Length < 1)
		{
			PooledList<Attribute, OptionsEntry> val = ListPool<Attribute, OptionsEntry>.Allocate();
			bool flag = true;
			bool restart = false;
			((List<Attribute>)(object)val).AddRange(prop.GetCustomAttributes());
			int count = ((List<Attribute>)(object)val).Count;
			for (int i = 0; i < count; i++)
			{
				Attribute attribute = ((List<Attribute>)(object)val)[i];
				if (attribute is RequireDLCAttribute requireDLCAttribute && PGameUtils.IsDLCOwned(requireDLCAttribute.DlcID) != requireDLCAttribute.Required)
				{
					flag = false;
				}
				else if (attribute is RestartRequiredAttribute)
				{
					restart = true;
				}
			}
			if (flag)
			{
				for (int j = 0; j < count; j++)
				{
					optionsEntry = TryCreateEntry(((List<Attribute>)(object)val)[j], prop, depth, restart);
					if (optionsEntry != null)
					{
						break;
					}
				}
			}
			val.Recycle();
		}
		return optionsEntry;
	}

	/// <summary>
	/// Creates an options entry if an attribute is a valid IOptionSpec or
	/// DynamicOptionAttribute.
	/// </summary>
	/// <param name="attribute">The attribute to parse.</param>
	/// <param name="prop">The property to inspect.</param>
	/// <param name="depth">The current depth of iteration to avoid infinite loops.</param>
	/// <param name="restart">true if the options class requests a restart for value changes.</param>
	/// <returns>The OptionsEntry created from the attribute, or null if none was.</returns>
	private static IOptionsEntry TryCreateEntry(Attribute attribute, PropertyInfo prop, int depth, bool restart)
	{
		IOptionsEntry optionsEntry = null;
		if (prop == null)
		{
			throw new ArgumentNullException("prop");
		}
		IOptionSpec optionSpec = attribute as IOptionSpec;
		if (optionSpec != null)
		{
			if (string.IsNullOrEmpty(optionSpec.Title))
			{
				optionSpec = HandleDefaults(optionSpec, prop);
			}
			Type propertyType = prop.PropertyType;
			optionsEntry = OptionsHandlers.FindOptionClass(optionSpec, prop);
			if (optionsEntry == null && !propertyType.IsValueType && depth < 16 && propertyType != prop.DeclaringType)
			{
				optionsEntry = CompositeOptionsEntry.Create(optionSpec, prop, depth);
			}
		}
		else if (attribute is DynamicOptionAttribute dynamicOptionAttribute && typeof(IOptionsEntry).IsAssignableFrom(dynamicOptionAttribute.Handler))
		{
			optionsEntry = CreateDynamicOption(prop, dynamicOptionAttribute.Handler);
		}
		if (optionsEntry != null)
		{
			optionsEntry.RestartRequired = restart;
		}
		return optionsEntry;
	}

	protected OptionsEntry(string field, IOptionSpec attr)
	{
		if (attr == null)
		{
			throw new ArgumentNullException("attr");
		}
		Field = field;
		Format = attr.Format;
		RestartRequired = false;
		Title = attr.Title ?? throw new ArgumentException("attr.Title is null");
		Tooltip = attr.Tooltip;
		Category = attr.Category;
	}

	public GameObject Build()
	{
		GameObject uIComponent = GetUIComponent();
		this.OnRealize?.Invoke(uIComponent);
		return uIComponent;
	}

	public int CompareTo(OptionsEntry other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return string.Compare(Category, other.Category, StringComparison.CurrentCultureIgnoreCase);
	}

	/// <summary>
	/// Adds the line item entry for this options entry.
	/// </summary>
	/// <param name="parent">The location to add this entry.</param>
	/// <param name="row">The layout row index to use. If updated, the row index will
	/// continue to count up from the new value.</param>
	public virtual void CreateUIEntry(PGridPanel parent, ref int row)
	{
		CreateDefaultUIEntry(this, parent, row, this);
	}

	/// <summary>
	/// Retrieves the UI component which can alter this setting. It should be sized
	/// properly to display any of the valid settings. The actual value will be set after
	/// the component is realized.
	/// </summary>
	/// <returns>The UI component to display.</returns>
	public abstract GameObject GetUIComponent();

	public virtual void ReadFrom(object settings)
	{
		if (Field == null || settings == null)
		{
			return;
		}
		try
		{
			PropertyInfo property = settings.GetType().GetProperty(Field, BindingFlags.Instance | BindingFlags.Public);
			if (property != null && property.CanRead)
			{
				Value = property.GetValue(settings, null);
			}
		}
		catch (TargetInvocationException thrown)
		{
			PUtil.LogException(thrown);
		}
		catch (AmbiguousMatchException thrown2)
		{
			PUtil.LogException(thrown2);
		}
		catch (InvalidCastException thrown3)
		{
			PUtil.LogException(thrown3);
		}
	}

	public override string ToString()
	{
		return "{1}[field={0},title={2}]".F(Field, GetType().Name, Title);
	}

	public virtual bool WriteTo(object settings)
	{
		bool result = false;
		if (Field != null && settings != null)
		{
			try
			{
				PropertyInfo property = settings.GetType().GetProperty(Field, BindingFlags.Instance | BindingFlags.Public);
				if (property != null && property.CanWrite)
				{
					object value = Value;
					bool flag = false;
					if (property.CanRead)
					{
						object value2 = property.GetValue(settings, null);
						flag = ((value2 == null) ? (value != null) : (!value2.Equals(value)));
					}
					property.SetValue(settings, value, null);
					result = flag;
				}
			}
			catch (TargetInvocationException thrown)
			{
				PUtil.LogException(thrown);
			}
			catch (AmbiguousMatchException thrown2)
			{
				PUtil.LogException(thrown2);
			}
			catch (InvalidCastException thrown3)
			{
				PUtil.LogException(thrown3);
			}
		}
		return result;
	}
}
