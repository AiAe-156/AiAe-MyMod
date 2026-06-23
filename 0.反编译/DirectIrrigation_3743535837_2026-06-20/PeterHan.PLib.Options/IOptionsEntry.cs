using PeterHan.PLib.UI;

namespace PeterHan.PLib.Options;

public interface IOptionsEntry : IOptionSpec
{
	bool RestartRequired { get; set; }

	void CreateUIEntry(PGridPanel parent, ref int row);

	void ReadFrom(object settings);

	bool WriteTo(object settings);
}
