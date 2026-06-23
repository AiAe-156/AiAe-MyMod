using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class TagFormatter : IYamlFormatter<Tag>, IYamlFormatter
{
	public static readonly TagFormatter Instance = new TagFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, Tag value, YamlSerializationContext context)
	{
		emitter.BeginSequence(SequenceStyle.Flow);
		emitter.WriteString(value.Name);
		emitter.EndSequence();
	}

	public Tag Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return default(Tag);
		}
		parser.ReadWithVerify(ParseEventType.SequenceStart);
		string tag_string = parser.ReadScalarAsString();
		parser.ReadWithVerify(ParseEventType.SequenceEnd);
		return TagManager.Create(tag_string);
	}
}
