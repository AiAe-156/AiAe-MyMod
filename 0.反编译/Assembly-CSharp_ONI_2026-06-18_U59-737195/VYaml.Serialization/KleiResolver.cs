using System;
using System.Collections.Generic;

namespace VYaml.Serialization;

public class KleiResolver : IYamlFormatterResolver
{
	private static class FormatterCache<T>
	{
		public static readonly IYamlFormatter<T>? Formatter;

		static FormatterCache()
		{
			if (FormatterMap.TryGetValue(typeof(T), out IYamlFormatter value) && value is IYamlFormatter<T> formatter)
			{
				Formatter = formatter;
			}
			else
			{
				Formatter = null;
			}
		}
	}

	public static readonly KleiResolver Instance = new KleiResolver();

	public static readonly Dictionary<Type, IYamlFormatter> FormatterMap = new Dictionary<Type, IYamlFormatter>
	{
		{
			typeof(Vector2f),
			Vector2fFormatter.Instance
		},
		{
			typeof(SimHashes),
			SimHashesFormatter.Instance
		},
		{
			typeof(Tag),
			TagFormatter.Instance
		},
		{
			typeof(Element.State),
			ElementStateFormatter.Instance
		}
	};

	public IYamlFormatter<T>? GetFormatter<T>()
	{
		return FormatterCache<T>.Formatter;
	}
}
