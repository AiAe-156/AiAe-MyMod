using System;
using System.Collections;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace PeterHan.PLib.Core;

/// <summary>
/// Extension methods to make life easier!
/// </summary>
public static class ExtensionMethods
{
	/// <summary>
	/// Shorthand for string.Format() which can be invoked directly on the message.
	/// </summary>
	/// <param name="message">The format template message.</param>
	/// <param name="args">The substitutions to be included.</param>
	/// <returns>The formatted string.</returns>
	public static string F(this string message, params object[] args)
	{
		return string.Format(message, args);
	}

	/// <summary>
	/// Retrieves a component, but returns null if the GameObject is disposed.
	/// </summary>
	/// <typeparam name="T">The component type to retrieve.</typeparam>
	/// <param name="obj">The GameObject that hosts the component.</param>
	/// <returns>The requested component, or null if it does not exist</returns>
	public static T GetComponentSafe<T>(this GameObject obj) where T : Component
	{
		if (!((Object)(object)obj == (Object)null))
		{
			return obj.GetComponent<T>();
		}
		return default(T);
	}

	/// <summary>
	/// Gets the assembly name of an assembly.
	/// </summary>
	/// <param name="assembly">The assembly to query.</param>
	/// <returns>The assembly name, or null if assembly is null.</returns>
	public static string GetNameSafe(this Assembly assembly)
	{
		return assembly?.GetName()?.Name;
	}

	/// <summary>
	/// Gets the file version of the specified assembly.
	/// </summary>
	/// <param name="assembly">The assembly to query</param>
	/// <returns>The AssemblyFileVersion of that assembly, or null if it could not be determined.</returns>
	public static string GetFileVersion(this Assembly assembly)
	{
		object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), inherit: true);
		string result = null;
		if (customAttributes != null && customAttributes.Length != 0)
		{
			AssemblyFileVersionAttribute assemblyFileVersionAttribute = (AssemblyFileVersionAttribute)customAttributes[0];
			if (assemblyFileVersionAttribute != null)
			{
				result = assemblyFileVersionAttribute.Version;
			}
		}
		return result;
	}

	/// <summary>
	/// Coerces a floating point number into the specified range.
	/// </summary>
	/// <param name="value">The original number.</param>
	/// <param name="min">The minimum value (inclusive).</param>
	/// <param name="max">The maximum value (inclusive).</param>
	/// <returns>The nearest value between minimum and maximum inclusive to value.</returns>
	public static double InRange(this double value, double min, double max)
	{
		double num = value;
		if (num < min)
		{
			num = min;
		}
		if (num > max)
		{
			num = max;
		}
		return num;
	}

	/// <summary>
	/// Coerces a floating point number into the specified range.
	/// </summary>
	/// <param name="value">The original number.</param>
	/// <param name="min">The minimum value (inclusive).</param>
	/// <param name="max">The maximum value (inclusive).</param>
	/// <returns>The nearest value between minimum and maximum inclusive to value.</returns>
	public static float InRange(this float value, float min, float max)
	{
		float num = value;
		if (num < min)
		{
			num = min;
		}
		if (num > max)
		{
			num = max;
		}
		return num;
	}

	/// <summary>
	/// Coerces an integer into the specified range.
	/// </summary>
	/// <param name="value">The original number.</param>
	/// <param name="min">The minimum value (inclusive).</param>
	/// <param name="max">The maximum value (inclusive).</param>
	/// <returns>The nearest value between minimum and maximum inclusive to value.</returns>
	public static int InRange(this int value, int min, int max)
	{
		int num = value;
		if (num < min)
		{
			num = min;
		}
		if (num > max)
		{
			num = max;
		}
		return num;
	}

	/// <summary>
	/// Checks to see if an object is falling.
	/// </summary>
	/// <param name="obj">The object to check.</param>
	/// <returns>true if it is falling, or false otherwise.</returns>
	public static bool IsFalling(this GameObject obj)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		int num = Grid.PosToCell(obj);
		Navigator val = default(Navigator);
		if (obj.TryGetComponent<Navigator>(ref val) && !val.IsMoving() && Grid.IsValidCell(num) && Grid.IsValidCell(Grid.CellBelow(num)))
		{
			return !val.NavGrid.NavTable.IsValid(num, val.CurrentNavType);
		}
		return false;
	}

	/// <summary>
	/// Checks to see if a floating point value is NaN or infinite.
	/// </summary>
	/// <param name="value">The value to check.</param>
	/// <returns>true if it is NaN, PositiveInfinity, or NegativeInfinity, or false otherwise.</returns>
	public static bool IsNaNOrInfinity(this double value)
	{
		if (!double.IsNaN(value))
		{
			return double.IsInfinity(value);
		}
		return true;
	}

	/// <summary>
	/// Checks to see if a floating point value is NaN or infinite.
	/// </summary>
	/// <param name="value">The value to check.</param>
	/// <returns>true if it is NaN, PositiveInfinity, or NegativeInfinity, or false otherwise.</returns>
	public static bool IsNaNOrInfinity(this float value)
	{
		if (!float.IsNaN(value))
		{
			return float.IsInfinity(value);
		}
		return true;
	}

	/// <summary>
	/// Checks to see if a building is usable.
	/// </summary>
	/// <param name="building">The building component to check.</param>
	/// <returns>true if it is usable (enabled, not broken, not overheated), or false otherwise.</returns>
	public static bool IsUsable(this GameObject building)
	{
		Operational val = default(Operational);
		if (building.TryGetComponent<Operational>(ref val))
		{
			return val.IsFunctional;
		}
		return false;
	}

	/// <summary>
	/// Creates a string joining the members of an enumerable.
	/// </summary>
	/// <param name="values">The values to join.</param>
	/// <param name="delimiter">The delimiter to use between values.</param>
	/// <returns>A string consisting of each value in order, with the delimiter in between.</returns>
	public static string Join(this IEnumerable values, string delimiter = ",")
	{
		StringBuilder stringBuilder = new StringBuilder(128);
		bool flag = true;
		foreach (object value in values)
		{
			if (!flag)
			{
				stringBuilder.Append(delimiter);
			}
			stringBuilder.Append(value);
			flag = false;
		}
		return stringBuilder.ToString();
	}

	/// <summary>
	/// Patches a method manually.
	/// </summary>
	/// <param name="instance">The Harmony instance.</param>
	/// <param name="type">The class to modify.</param>
	/// <param name="methodName">The method to patch.</param>
	/// <param name="prefix">The prefix to apply, or null if none.</param>
	/// <param name="postfix">The postfix to apply, or null if none.</param>
	public static void Patch(this Harmony instance, Type type, string methodName, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(methodName))
		{
			throw new ArgumentNullException("methodName");
		}
		try
		{
			MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				instance.Patch((MethodBase)method, prefix, postfix, (HarmonyMethod)null, (HarmonyMethod)null);
				return;
			}
			PUtil.LogWarning("Unable to find method {0} on type {1}".F(methodName, type.FullName));
		}
		catch (AmbiguousMatchException thrown)
		{
			PUtil.LogException(thrown);
		}
	}

	/// <summary>
	/// Patches a constructor manually.
	/// </summary>
	/// <param name="instance">The Harmony instance.</param>
	/// <param name="type">The class to modify.</param>
	/// <param name="arguments">The constructor's argument types.</param>
	/// <param name="prefix">The prefix to apply, or null if none.</param>
	/// <param name="postfix">The postfix to apply, or null if none.</param>
	public static void PatchConstructor(this Harmony instance, Type type, Type[] arguments, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		try
		{
			ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, arguments, null);
			if (constructor != null)
			{
				instance.Patch((MethodBase)constructor, prefix, postfix, (HarmonyMethod)null, (HarmonyMethod)null);
				return;
			}
			PUtil.LogWarning("Unable to find constructor on type {0}".F(type.FullName));
		}
		catch (ArgumentException thrown)
		{
			PUtil.LogException(thrown);
		}
	}

	/// <summary>
	/// Patches a method manually with a transpiler.
	/// </summary>
	/// <param name="instance">The Harmony instance.</param>
	/// <param name="type">The class to modify.</param>
	/// <param name="methodName">The method to patch.</param>
	/// <param name="transpiler">The transpiler to apply.</param>
	public static void PatchTranspile(this Harmony instance, Type type, string methodName, HarmonyMethod transpiler)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(methodName))
		{
			throw new ArgumentNullException("methodName");
		}
		try
		{
			MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				instance.Patch((MethodBase)method, (HarmonyMethod)null, (HarmonyMethod)null, transpiler, (HarmonyMethod)null);
				return;
			}
			PUtil.LogWarning("Unable to find method {0} on type {1}".F(methodName, type.FullName));
		}
		catch (AmbiguousMatchException thrown)
		{
			PUtil.LogException(thrown);
		}
		catch (FormatException ex)
		{
			PUtil.LogWarning("Unable to transpile method {0}: {1}".F(methodName, ex.Message));
		}
	}

	/// <summary>
	/// Sets a game object's parent.
	/// </summary>
	/// <param name="child">The game object to modify.</param>
	/// <param name="parent">The new parent object.</param>
	/// <returns>The game object, for call chaining.</returns>
	public static GameObject SetParent(this GameObject child, GameObject parent)
	{
		if ((Object)(object)child == (Object)null)
		{
			throw new ArgumentNullException("child");
		}
		child.transform.SetParent(((Object)(object)parent == (Object)null) ? null : parent.transform, false);
		return child;
	}
}
