using System;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Parser;
using VYaml.Serialization;

namespace ElementData;

[YamlObject(NamingConvention.LowerCamelCase)]
public class ElementEntryCollection
{
	[Preserve]
	public class ElementEntryCollectionGeneratedFormatter : IYamlFormatter<ElementEntryCollection?>, IYamlFormatter
	{
		private static readonly byte[] elementsKeyUtf8Bytes = new byte[8] { 101, 108, 101, 109, 101, 110, 116, 115 };

		[Preserve]
		public void Serialize(ref Utf8YamlEmitter emitter, ElementEntryCollection? value, YamlSerializationContext context)
		{
			if (value == null)
			{
				emitter.WriteNull();
				return;
			}
			emitter.BeginMapping();
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(elementsKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(elementsKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer, out int written);
				emitter.WriteScalar(threadStaticBuffer.AsSpan(0, written));
			}
			context.Serialize(ref emitter, value.elements);
			emitter.EndMapping();
		}

		[Preserve]
		public ElementEntryCollection? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
		{
			if (parser.IsNullScalar())
			{
				parser.Read();
				return null;
			}
			parser.ReadWithVerify(ParseEventType.MappingStart);
			ElementEntry[] elements = null;
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
				int length = span.Length;
				int num = length;
				if (num == 8 && span.SequenceEqual(elementsKeyUtf8Bytes))
				{
					parser.Read();
					elements = context.DeserializeWithAlias<ElementEntry[]>(ref parser);
				}
				else
				{
					parser.Read();
					parser.SkipCurrentNode();
				}
			}
			parser.ReadWithVerify(ParseEventType.MappingEnd);
			return new ElementEntryCollection
			{
				elements = elements
			};
		}
	}

	public ElementEntry[] elements { get; set; }

	[Preserve]
	public static void __RegisterVYamlFormatter()
	{
		GeneratedResolver.Register(new ElementEntryCollectionGeneratedFormatter());
	}
}
