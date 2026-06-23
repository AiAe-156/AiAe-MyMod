using System;
using System.Collections.Generic;
using System.Reflection;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using STRINGS;
using UnityEngine;

namespace PeterHan.PLib.Options;

public abstract class OptionsEntry : IOptionsEntry, IOptionSpec, IComparable<OptionsEntry>, IUIComponent
{
	private const BindingFlags INSTANCE_PUBLIC = BindingFlags.Instance | BindingFlags.Public;

	protected static readonly RectOffset CONTROL_MARGIN = new RectOffset(0, 0, 2, 2);

	protected static readonly RectOffset LABEL_MARGIN = new RectOffset(0, 5, 2, 2);

	public string Category { get; }

	public string Field { get; }

	public string Format { get; }

	public virtual string Name => "OptionsEntry";

	public bool RestartRequired { get; set; }

	public string Title { get; protected set; }

	public string Tooltip { get; protected set; }

	public abstract object Value { get; set; }

	public event PUIDelegates.OnRealize OnRealize;

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

	internal static IOptionsEntry TryCreateEntry(PropertyInfo prop, int depth)
	{
		IOptionsEntry optionsEntry = null;
		if (prop.GetIndexParameters().Length < 1)
		{
			bool flag = true;
			bool restart = false;
			object[] customAttributes = prop.GetCustomAttributes(inherit: false);
			int num = customAttributes.Length;
			for (int i = 0; i < num; i++)
			{
				object obj = customAttributes[i];
				if (obj is IRequireFilter requireFilter && !requireFilter.Filter())
				{
					flag = false;
				}
				else if (obj is RestartRequiredAttribute)
				{
					restart = true;
				}
			}
			if (flag)
			{
				for (int j = 0; j < num; j++)
				{
					optionsEntry = TryCreateEntry(customAttributes[j], prop, depth, restart);
					if (optionsEntry != null)
					{
						break;
					}
				}
			}
		}
		return optionsEntry;
	}

	private static IOptionsEntry TryCreateEntry(object attribute, PropertyInfo prop, int depth, bool restart)
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

	public virtual void CreateUIEntry(PGridPanel parent, ref int row)
	{
		CreateDefaultUIEntry(this, parent, row, this);
	}

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
