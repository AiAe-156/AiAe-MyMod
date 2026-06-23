using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class Vector2fFormatter : IYamlFormatter<Vector2f>, IYamlFormatter
{
	public static readonly Vector2fFormatter Instance = new Vector2fFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, Vector2f value, YamlSerializationContext context)
	{
		emitter.BeginMapping();
		emitter.WriteString("X");
		emitter.WriteFloat(value.x);
		emitter.WriteString("Y");
		emitter.WriteFloat(value.y);
		emitter.EndMapping();
	}

	public Vector2f Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return default(Vector2f);
		}
		parser.ReadWithVerify(ParseEventType.MappingStart);
		parser.ReadScalarAsString();
		float a = parser.ReadScalarAsFloat();
		parser.ReadScalarAsString();
		float b = parser.ReadScalarAsFloat();
		parser.ReadWithVerify(ParseEventType.MappingEnd);
		return new Vector2f(a, b);
	}
}
