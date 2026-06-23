using System.Collections.Generic;
using System.Linq;
using STRINGS;

namespace UtilLibs.MarkdownExport;

public class MD_Localization
{
	public class MD_Loc_Entry
	{
		public Dictionary<string, string> strings = new Dictionary<string, string>();

		public MD_Loc_Entry(string code)
		{
			if (!string.IsNullOrEmpty(code) && !(code == Localization.DEFAULT_LANGUAGE_CODE))
			{
			}
		}

		public MD_Loc_Entry Add(string key, string value)
		{
			strings.Add(key, value);
			return this;
		}

		public bool TryGet(string key, out string value)
		{
			return strings.TryGetValue(key, out value);
		}
	}

	private static Dictionary<string, string> EnglishKeyLookupTable = null;

	public static Dictionary<string, MD_Loc_Entry> Values = new Dictionary<string, MD_Loc_Entry>
	{
		{
			Localization.DEFAULT_LANGUAGE_CODE,
			new MD_Loc_Entry("").Add("BUILDINGS", "Buildings").Add("BUILDING_INFO_HEADER", "Info").Add("BUILDING_DIMENSIONS_LABEL", "Dimensions:")
				.Add("BUILDING_DIMENSIONS_INFO", "{0} wide x {1} high")
				.Add("BUILDING_POWER_CONSUMPTION", "Power Consumption:")
				.Add("BUILDING_POWER_GENERATION", "Power Generation:")
				.Add("BUILDING_RESEARCH_REQUIREMENT", "Research Required:")
				.Add("BUILDING_STORAGE_CAPACITY", "Storage Capacity:")
				.Add("BUILDING_PORTS_HEADER", "Building Ports")
				.Add("INPUTS_HEADER", "Inputs:")
				.Add("OUTPUTS_HEADER", "Outputs:")
				.Add("BUILDING_MATERIAL_COST_HEADER", "Material Costs:")
				.Add("SEPARATOR_OR", "or")
				.Add("CONVERSION_GENERATOR_HEADER", "Fuel Conversion")
				.Add("CONVERSION_ELEMENT_HEADER", "Element Conversion")
				.Add("RECIPES_HEADER", "Recipes")
				.Add("AT_TEMPERATURE", "at {0}°C")
				.Add("RECIPE_INGREDIENTS", "Ingredients:")
				.Add("RECIPE_TIME", "Time:")
				.Add("RECIPE_PRODUCTS", "Products:")
				.Add("RECIPE_PRODUCTS_RANDOM", "Random Products:")
				.Add("RECIPE_RANDOM_OCCURENCE", "Random Occurences:")
				.Add("PIPE_INPUT", "{0} Input Pipe")
				.Add("RAIL_INPUT", "{0} Input Rail")
				.Add("PIPE_OUTPUT", "{0} Output Pipe")
				.Add("RAIL_OUTPUT", "{0} Output Rail")
				.Add("ELEMENT_ID", "ID")
				.Add("ELEMENT_NAME", "Name")
				.Add("ELEMENT_ICON", "Icon")
				.Add("ELEMENT_STATE", "State")
				.Add("ELEMENT_PROPERTIES", "Material Properties")
				.Add("MODIFIED_SUFFIX", "(Modified)")
				.Add("NEW_ITEM", "New {0}")
				.Add("REENABLED_ITEM", "Reenabled {0}")
		},
		{
			"zh",
			new MD_Loc_Entry("zh").Add("BUILDINGS", "建筑").Add("BUILDING_INFO_HEADER", "信息").Add("BUILDING_DIMENSIONS_LABEL", "尺寸:")
				.Add("BUILDING_DIMENSIONS_INFO", "宽 {0} x 高 {1}")
				.Add("BUILDING_POWER_CONSUMPTION", "耗电:")
				.Add("BUILDING_POWER_GENERATION", "发电:")
				.Add("BUILDING_RESEARCH_REQUIREMENT", "科技:")
				.Add("BUILDING_STORAGE_CAPACITY", "存储容量:")
				.Add("BUILDING_PORTS_HEADER", "建筑接口")
				.Add("INPUTS_HEADER", "输入:")
				.Add("OUTPUTS_HEADER", "输出:")
				.Add("BUILDING_MATERIAL_COST_HEADER", "建造材料:")
				.Add("SEPARATOR_OR", "或")
				.Add("CONVERSION_GENERATOR_HEADER", "燃料转换")
				.Add("CONVERSION_ELEMENT_HEADER", "元素转换")
				.Add("RECIPES_HEADER", "配方")
				.Add("AT_TEMPERATURE", "于 {0}°C")
				.Add("RECIPE_INGREDIENTS", "材料:")
				.Add("RECIPE_TIME", "耗时:")
				.Add("RECIPE_PRODUCTS", "产物:")
				.Add("RECIPE_PRODUCTS_RANDOM", "随机产物:")
				.Add("RECIPE_RANDOM_OCCURENCE", "随机概率:")
				.Add("PIPE_INPUT", "{0} 输入管道")
				.Add("RAIL_INPUT", "{0} 输入轨道")
				.Add("PIPE_OUTPUT", "{0} 输出管道")
				.Add("RAIL_OUTPUT", "{0} 输出轨道")
				.Add("ELEMENT_ID", "ID")
				.Add("ELEMENT_NAME", "名称")
				.Add("ELEMENT_ICON", "图标")
				.Add("ELEMENT_STATE", "状态")
				.Add("ELEMENT_PROPERTIES", "材料属性")
				.Add("MODIFIED_SUFFIX", "(修改)")
				.Add("NEW_ITEM", "新 {0}")
				.Add("REENABLED_ITEM", "重新启用的 {0}")
		}
	};

	private static Dictionary<string, Tuple<string, List<string>>> RegisteredStringGetters = new Dictionary<string, Tuple<string, List<string>>>();

	public static string CurrentLocalization { get; private set; } = Localization.DEFAULT_LANGUAGE_CODE;

	public static string GetSuffix()
	{
		return "." + CurrentLocalization;
	}

	public static void SetLocalization(string key)
	{
		if (key == null)
		{
			CurrentLocalization = Localization.DEFAULT_LANGUAGE_CODE;
		}
		else
		{
			CurrentLocalization = key;
		}
		SgtLogger.l("Setting language of wiki gen to loc key: " + key);
	}

	public static string FindStringKey(string value)
	{
		if (EnglishKeyLookupTable == null)
		{
			EnglishKeyLookupTable = new Dictionary<string, string>();
			foreach (KeyValuePair<int, string> keyName in Strings.RootTable.KeyNames)
			{
				if (!Strings.RootTable.Entries.TryGetValue(keyName.Key, out var value2))
				{
					continue;
				}
				string value3 = keyName.Value;
				int num = value3.IndexOf("STRINGS");
				if (num >= 0)
				{
					string value4 = value3.Substring(num);
					if (!EnglishKeyLookupTable.ContainsKey(value2.String))
					{
						EnglishKeyLookupTable.Add(value2.String, value4);
					}
				}
			}
		}
		if (EnglishKeyLookupTable.TryGetValue(value, out var value5))
		{
			return value5;
		}
		return "MISSING." + value;
	}

	public static string FormatLineBreaks(string input)
	{
		return input.Replace("\n", "<br/>");
	}

	public static string Strip(string input)
	{
		return FormatLineBreaks(UI.StripLinkFormatting(input));
	}

	public static string L(string key)
	{
		if (Util.IsNullOrWhiteSpace(key))
		{
			return "MISSING!!!";
		}
		if (TryGetManuallyRegistered(key, out var val))
		{
			return Strip(val);
		}
		if (!Values.TryGetValue(CurrentLocalization, out var value))
		{
			value = Values[Localization.DEFAULT_LANGUAGE_CODE];
		}
		if (value.TryGet(key, out var value2))
		{
			return Strip(value2);
		}
		if (CurrentLocalization == Localization.DEFAULT_LANGUAGE_CODE)
		{
			return Strip(StringEntry.op_Implicit(Strings.Get(key)));
		}
		if (LocalisationUtil.TryGetTranslatedString(CurrentLocalization, key, out var translatedString))
		{
			return Strip(translatedString);
		}
		return "MISSING." + key;
	}

	public static bool HasKey(string key)
	{
		return !L(key).Contains("MISSING.");
	}

	public static bool TryGetManuallyRegistered(string key, out string val)
	{
		val = null;
		if (!RegisteredStringGetters.TryGetValue(key, out var value))
		{
			return false;
		}
		string text = L(value.first);
		string[] array = value.second.Select((string x) => L(x)).ToArray();
		if (text.Contains("MISSING") || array.Any((string entry) => entry.Contains("Missing")))
		{
			return false;
		}
		object[] args = array;
		val = string.Format(text, args);
		return true;
	}

	public static void Add(string Key, string KeyToFormat, List<string> KeyFormatValues)
	{
		if (!RegisteredStringGetters.ContainsKey(Key))
		{
			Key = Key.ToUpperInvariant();
			RegisteredStringGetters.Add(Key, new Tuple<string, List<string>>(KeyToFormat, KeyFormatValues));
		}
	}
}
