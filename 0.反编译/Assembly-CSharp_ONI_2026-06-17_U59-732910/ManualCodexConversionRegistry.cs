using System;
using System.Collections.Generic;

public class ManualCodexConversionRegistry
{
	public class ManualConversionEntry
	{
		public string headerDescription;

		public Tuple<Tag, float> converter;

		public Tuple<Tag, float> input;

		public Tuple<Tag, float> output;

		public Func<Tag, float, bool, string> inputCustomFormating;

		public Func<Tag, float, bool, string> converterCustomFormating;

		public Func<Tag, float, bool, string> outputCustomFormating;

		public ManualConversionEntry(Tuple<Tag, float> converter, Tuple<Tag, float> input, Tuple<Tag, float> output, string headerDescription, Func<Tag, float, bool, string> inputCustomFormating = null, Func<Tag, float, bool, string> converterCustomFormating = null, Func<Tag, float, bool, string> outputCustomFormating = null)
		{
			this.converter = converter;
			this.input = input;
			this.output = output;
			this.headerDescription = headerDescription;
			this.inputCustomFormating = inputCustomFormating;
			this.converterCustomFormating = converterCustomFormating;
			this.outputCustomFormating = outputCustomFormating;
		}
	}

	public static Dictionary<Tag, List<ManualConversionEntry>> conversionsByTag = new Dictionary<Tag, List<ManualConversionEntry>>();

	public static List<ManualConversionEntry> GetConversionsForGivenConverter(Tag converter)
	{
		if (!conversionsByTag.ContainsKey(converter))
		{
			return null;
		}
		List<ManualConversionEntry> list = new List<ManualConversionEntry>();
		foreach (ManualConversionEntry item in conversionsByTag[converter])
		{
			if (item.converter != null && item.converter.first == converter)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<ManualConversionEntry> GetProducersForGivenOutput(Tag output)
	{
		if (!conversionsByTag.ContainsKey(output))
		{
			return null;
		}
		List<ManualConversionEntry> list = new List<ManualConversionEntry>();
		foreach (ManualConversionEntry item in conversionsByTag[output])
		{
			if (item.output != null && item.output.first == output)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<ManualConversionEntry> GetConsumersForGivenInput(Tag input)
	{
		if (!conversionsByTag.ContainsKey(input))
		{
			return null;
		}
		List<ManualConversionEntry> list = new List<ManualConversionEntry>();
		foreach (ManualConversionEntry item in conversionsByTag[input])
		{
			if (item.input != null && item.input.first == input)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static void AddConversion(Tag inputTag, float inputAmount, Tag converterTag, float converterAmount, Tag outputTag, float outputAmount, string headerDescription, Func<Tag, float, bool, string> inputCustomFormating = null, Func<Tag, float, bool, string> converterCustomFormating = null, Func<Tag, float, bool, string> outputCustomFormating = null)
	{
		ManualConversionEntry item = new ManualConversionEntry(new Tuple<Tag, float>(converterTag, converterAmount), new Tuple<Tag, float>(inputTag, inputAmount), new Tuple<Tag, float>(outputTag, outputAmount), headerDescription, inputCustomFormating, converterCustomFormating, outputCustomFormating);
		if (converterTag != null)
		{
			if (!conversionsByTag.TryGetValue(converterTag, out var value))
			{
				value = new List<ManualConversionEntry>();
				conversionsByTag[converterTag] = value;
			}
			conversionsByTag[converterTag].Add(item);
		}
		if (inputTag != null)
		{
			if (!conversionsByTag.TryGetValue(inputTag, out var value2))
			{
				value2 = new List<ManualConversionEntry>();
				conversionsByTag[inputTag] = value2;
			}
			conversionsByTag[inputTag].Add(item);
		}
		if (outputTag != null)
		{
			if (!conversionsByTag.TryGetValue(outputTag, out var value3))
			{
				value3 = new List<ManualConversionEntry>();
				conversionsByTag[outputTag] = value3;
			}
			conversionsByTag[outputTag].Add(item);
		}
	}
}
