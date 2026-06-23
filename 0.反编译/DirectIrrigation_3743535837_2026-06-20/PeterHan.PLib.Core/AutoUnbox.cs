namespace PeterHan.PLib.Core;

public static class AutoUnbox<T> where T : struct
{
	public static object Box(T data)
	{
		return Boxed<T>.Get(data);
	}

	public static bool Unbox(object data, out T result)
	{
		bool result2 = true;
		if (data is Boxed<T> val)
		{
			result = val.value;
		}
		else if (data is T val2)
		{
			result = val2;
		}
		else
		{
			result = default(T);
			result2 = false;
		}
		return result2;
	}
}
