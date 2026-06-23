public interface IShearable
{
	bool IsFullyGrown();

	void Shear();

	Tuple<Tag, float> GetItemDroppedOnShear();
}
