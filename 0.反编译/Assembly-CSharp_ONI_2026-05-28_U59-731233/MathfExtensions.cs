public static class MathfExtensions
{
	public static long Max(this long a, long b)
	{
		return (a >= b) ? a : b;
	}

	public static long Min(this long a, long b)
	{
		return (a <= b) ? a : b;
	}
}
