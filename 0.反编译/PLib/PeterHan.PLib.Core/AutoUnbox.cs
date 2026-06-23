using System;

namespace PeterHan.PLib.Core;

/// <summary>
/// A helper class for migrating to boxed event types.
/// </summary>
/// <typeparam name="T">The value type to be unboxed.</typeparam>
public static class AutoUnbox<T> where T : struct
{
	private delegate object BoxData(T raw);

	private delegate T UnboxData(object boxed);

	/// <summary>
	/// If boxing is required, gets the data from the object.
	/// </summary>
	private static readonly BoxData DATA_BOX;

	/// <summary>
	/// If unboxing is required, gets the data from the object.
	/// </summary>
	private static readonly UnboxData DATA_UNBOX;

	static AutoUnbox()
	{
		Type type;
		try
		{
			type = typeof(Tag).Assembly.GetType("Boxed`1");
		}
		catch
		{
			type = null;
		}
		if (type != null && type.ContainsGenericParameters)
		{
			Type type2 = type.MakeGenericType(typeof(T));
			DATA_UNBOX = type2.CreateStaticDelegate<UnboxData>("Unbox", new Type[1] { typeof(object) });
			DATA_BOX = type2.CreateStaticDelegate<BoxData>("Get", new Type[1] { typeof(T) });
		}
		else
		{
			DATA_BOX = null;
			DATA_UNBOX = null;
		}
	}

	/// <summary>
	/// Boxes the specified object.
	/// </summary>
	/// <param name="data">The data to box.</param>
	/// <returns>The boxed data for the Trigger method.</returns>
	public static object Box(T data)
	{
		if (DATA_BOX != null)
		{
			return DATA_BOX(data);
		}
		return data;
	}

	/// <summary>
	/// Unboxes the specified object.
	/// </summary>
	/// <param name="data">The data or boxed data.</param>
	/// <param name="result">The result of unboxing. Only valid if true is returned.</param>
	/// <returns>true if the data could be unboxed or directly converted, or false otherwise.</returns>
	public static bool Unbox(object data, out T result)
	{
		bool result2 = true;
		if (DATA_UNBOX != null)
		{
			result = DATA_UNBOX(data);
		}
		else if (data is T val)
		{
			result = val;
		}
		else
		{
			result = default(T);
			result2 = false;
		}
		return result2;
	}
}
