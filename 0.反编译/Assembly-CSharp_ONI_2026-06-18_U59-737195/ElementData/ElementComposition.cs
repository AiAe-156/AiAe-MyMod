using System;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Parser;
using VYaml.Serialization;

namespace ElementData;

[YamlObject(NamingConvention.LowerCamelCase)]
public class ElementComposition
{
	[Preserve]
	public class ElementCompositionGeneratedFormatter : IYamlFormatter<ElementComposition?>, IYamlFormatter
	{
		private static readonly byte[] elementIDKeyUtf8Bytes = new byte[9] { 101, 108, 101, 109, 101, 110, 116, 73, 68 };

		private static readonly byte[] percentageKeyUtf8Bytes = new byte[10] { 112, 101, 114, 99, 101, 110, 116, 97, 103, 101 };

		[Preserve]
		public void Serialize(ref Utf8YamlEmitter emitter, ElementComposition? value, YamlSerializationContext context)
		{
			if (value == null)
			{
				emitter.WriteNull();
				return;
			}
			emitter.BeginMapping();
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(elementIDKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(elementIDKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer, out int written);
				emitter.WriteScalar(threadStaticBuffer.AsSpan(0, written));
			}
			context.Serialize(ref emitter, value.elementID);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(percentageKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(percentageKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer2, out int written2);
				emitter.WriteScalar(threadStaticBuffer2.AsSpan(0, written2));
			}
			context.Serialize(ref emitter, value.percentage);
			emitter.EndMapping();
		}

		[Preserve]
		public ElementComposition? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
		{
			if (parser.IsNullScalar())
			{
				parser.Read();
				return null;
			}
			parser.ReadWithVerify(ParseEventType.MappingStart);
			string elementID = null;
			float percentage = 0f;
			while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
			{
				if (parser.CurrentEventType != ParseEventType.Scalar)
				{
					throw new YamlSerializerException(parser.CurrentMark, "Custom type deserialization supports only string key");
				}
				if (!parser.TryGetScalarAsSpan(out var span))
				{
					throw new YamlSerializerException(parser.CurrentMark, "Custom type deserialization supports only string key");
				}
				if (context.Options.NamingConvention != NamingConvention.LowerCamelCase)
				{
					NamingConventionMutator.MutateToThreadStaticBufferUtf8(span, NamingConvention.LowerCamelCase, out byte[] threadStaticBuffer, out int written);
					span = threadStaticBuffer.AsSpan(0, written);
				}
				switch (span.Length)
				{
				case 9:
					if (span.SequenceEqual(elementIDKeyUtf8Bytes))
					{
						parser.Read();
						elementID = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					break;
				case 10:
					if (span.SequenceEqual(percentageKeyUtf8Bytes))
					{
						parser.Read();
						percentage = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				}
				parser.Read();
				parser.SkipCurrentNode();
			}
			parser.ReadWithVerify(ParseEventType.MappingEnd);
			return new ElementComposition
			{
				elementID = elementID,
				percentage = percentage
			};
		}
	}

	public string elementID { get; set; }

	public float percentage { get; set; }

	[Preserve]
	public static void __RegisterVYamlFormatter()
	{
		GeneratedResolver.Register(new ElementCompositionGeneratedFormatter());
	}
}
