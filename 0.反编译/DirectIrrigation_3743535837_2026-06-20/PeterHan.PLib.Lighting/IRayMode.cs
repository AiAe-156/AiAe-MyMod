namespace PeterHan.PLib.Lighting;

public interface IRayMode
{
	void DrawCustomRay(Light2D light, LightBuffer lightBuffer);

	void Prepare(LightBuffer lightBuffer);
}
