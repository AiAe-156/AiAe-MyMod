using System;
using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class ElementStateFormatter : IYamlFormatter<Element.State>, IYamlFormatter
{
	public static readonly ElementStateFormatter Instance = new ElementStateFormatter();

	private static readonly (Element.State flag, string name)[] Flags = new(Element.State, string)[3]
	{
		(Element.State.TemperatureInsulated, "TemperatureInsulated"),
		(Element.State.Unstable, "Unstable"),
		(Element.State.Unbreakable, "Unbreakable")
	};

	public void Serialize(ref Utf8YamlEmitter emitter, Element.State value, YamlSerializationContext context)
	{
		Element.State state = value & Element.State.Solid;
		List<string> list = new List<string> { state.ToString() };
		(Element.State, string)[] flags = Flags;
		for (int i = 0; i < flags.Length; i++)
		{
			var (state2, item) = flags[i];
			if ((value & state2) != Element.State.Vacuum)
			{
				list.Add(item);
			}
		}
		emitter.WriteString(string.Join(", ", list));
	}

	public Element.State Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return Element.State.Vacuum;
		}
		string text = parser.ReadScalarAsString();
		if (text == null)
		{
			return Element.State.Vacuum;
		}
		Element.State state = Element.State.Vacuum;
		string[] array = text.Split(',');
		foreach (string text2 in array)
		{
			string text3 = text2.Trim();
			if (Enum.TryParse<Element.State>(text3, ignoreCase: true, out var result))
			{
				state |= result;
			}
			else
			{
				Debug.LogWarning("ElementStateFormatter: Unknown state flag '" + text3 + "'");
			}
		}
		return state;
	}
}
