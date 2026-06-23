using System;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.PatchManager;

/// <summary>
/// Refers to a single instance of the annotation, with its annotated method.
/// </summary>
internal sealed class PLibMethodInstance : IPatchMethodInstance
{
	/// <summary>
	/// The attribute describing the method.
	/// </summary>
	public PLibMethodAttribute Descriptor { get; }

	/// <summary>
	/// The method to run.
	/// </summary>
	public MethodInfo Method { get; }

	public PLibMethodInstance(PLibMethodAttribute attribute, MethodInfo method)
	{
		Descriptor = attribute ?? throw new ArgumentNullException("attribute");
		Method = method ?? throw new ArgumentNullException("method");
	}

	/// <summary>
	/// Runs the method, passing the required parameters if any.
	/// </summary>
	/// <param name="instance">The Harmony instance to use if the method wants to
	/// perform a patch.</param>
	public void Run(Harmony instance)
	{
		if (!PPatchManager.CheckConditions(Descriptor.RequireAssembly, Descriptor.RequireType, out var requiredType))
		{
			return;
		}
		Type[] parameterTypes = Method.GetParameterTypes();
		int num = parameterTypes.Length;
		if (num <= 0)
		{
			Method.Invoke(null, null);
		}
		else if (parameterTypes[0] == typeof(Harmony))
		{
			switch (num)
			{
			case 1:
				Method.Invoke(null, new object[1] { instance });
				break;
			case 2:
				if (parameterTypes[1] == typeof(Type))
				{
					Method.Invoke(null, new object[2] { instance, requiredType });
				}
				break;
			}
		}
		else
		{
			PUtil.LogWarning("Invalid signature for PLibMethod - must have (), (Harmony), or (Harmony, Type)");
		}
	}
}
