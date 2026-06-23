using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;

namespace PeterHan.PLib.Core;

/// <summary>
/// Contains tools to aid with patching.
/// </summary>
public static class PPatchTools
{
	/// <summary>
	/// The base binding flags for all reflection methods.
	/// </summary>
	public const BindingFlags BASE_FLAGS = BindingFlags.Public | BindingFlags.NonPublic;

	/// <summary>
	/// A placeholder flag to ReplaceMethodCallSafe to remove the method call.
	/// </summary>
	public static readonly MethodInfo RemoveCall = typeof(PPatchTools).GetMethodSafe("RemoveMethodCallPrivate", true);

	/// <summary>
	/// Passed to GetMethodSafe to match any method arguments.
	/// </summary>
	public static Type[] AnyArguments => new Type[1];

	/// <summary>
	/// Creates a delegate for a private instance method. This delegate is over ten times
	/// faster than reflection, so useful if called frequently on the same object.
	/// </summary>
	/// <typeparam name="T">A delegate type which matches the method signature.</typeparam>
	/// <param name="type">The declaring type of the target method.</param>
	/// <param name="method">The target method name.</param>
	/// <param name="caller">The object on which to call the method.</param>
	/// <param name="argumentTypes">The types of the target method arguments, or PPatchTools.
	/// AnyArguments (not recommended, type safety is good) to match any method with
	/// that name.</param>
	/// <returns>A delegate which calls this method, or null if the method could not be
	/// found or did not match the types.</returns>
	public static T CreateDelegate<T>(this Type type, string method, object caller, params Type[] argumentTypes) where T : Delegate
	{
		T result = null;
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(method))
		{
			throw new ArgumentNullException("method");
		}
		MethodInfo methodSafe = type.GetMethodSafe(method, isStatic: false, argumentTypes);
		if (methodSafe != null)
		{
			return Delegate.CreateDelegate(typeof(T), caller, methodSafe, throwOnBindFailure: false) as T;
		}
		return result;
	}

	/// <summary>
	/// Creates a delegate for a private instance method. This delegate is over ten times
	/// faster than reflection, so useful if called frequently on the same object.
	/// </summary>
	/// <typeparam name="T">A delegate type which matches the method signature.</typeparam>
	/// <param name="method">The target method.</param>
	/// <param name="caller">The object on which to call the method.</param>
	/// <returns>A delegate which calls this method, or null if the method was null or did
	/// not match the delegate type.</returns>
	public static T CreateDelegate<T>(this MethodInfo method, object caller) where T : Delegate
	{
		T result = null;
		if (method != null)
		{
			return Delegate.CreateDelegate(typeof(T), caller, method, throwOnBindFailure: false) as T;
		}
		return result;
	}

	/// <summary>
	/// Creates a delegate for a private instance property getter. This delegate is over
	/// ten times faster than reflection, so useful if called frequently on the same object.
	///
	/// This method does not work on indexed properties.
	/// </summary>
	/// <typeparam name="T">The property's type.</typeparam>
	/// <param name="type">The declaring type of the target property.</param>
	/// <param name="property">The target property name.</param>
	/// <param name="caller">The object on which to call the property getter.</param>
	/// <returns>A delegate which calls this property's getter, or null if the property
	/// could not be found or did not match the type.</returns>
	public static Func<T> CreateGetDelegate<T>(this Type type, string property, object caller)
	{
		Func<T> result = null;
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(property))
		{
			throw new ArgumentNullException("property");
		}
		MethodInfo methodInfo = type.GetPropertySafe<T>(property, isStatic: false)?.GetGetMethod(nonPublic: true);
		if (methodInfo != null)
		{
			result = Delegate.CreateDelegate(typeof(Func<T>), caller, methodInfo, throwOnBindFailure: false) as Func<T>;
		}
		return result;
	}

	/// <summary>
	/// Creates a delegate for a private instance property getter. This delegate is over
	/// ten times faster than reflection, so useful if called frequently on the same object.
	///
	/// This method does not work on indexed properties.
	/// </summary>
	/// <typeparam name="T">The property's type.</typeparam>
	/// <param name="property">The target property.</param>
	/// <param name="caller">The object on which to call the property getter.</param>
	/// <returns>A delegate which calls this property's getter, or null if the property
	/// was null or did not match the type.</returns>
	public static Func<T> CreateGetDelegate<T>(this PropertyInfo property, object caller)
	{
		Func<T> result = null;
		MethodInfo methodInfo = property?.GetGetMethod(nonPublic: true);
		if (methodInfo != null && typeof(T).IsAssignableFrom(property.PropertyType))
		{
			result = Delegate.CreateDelegate(typeof(Func<T>), caller, methodInfo, throwOnBindFailure: false) as Func<T>;
		}
		return result;
	}

	/// <summary>
	/// Creates a delegate for a private instance property setter. This delegate is over
	/// ten times faster than reflection, so useful if called frequently on the same object.
	///
	/// This method does not work on indexed properties.
	/// </summary>
	/// <typeparam name="T">The property's type.</typeparam>
	/// <param name="type">The declaring type of the target property.</param>
	/// <param name="property">The target property name.</param>
	/// <param name="caller">The object on which to call the property setter.</param>
	/// <returns>A delegate which calls this property's setter, or null if the property
	/// could not be found or did not match the type.</returns>
	public static Action<T> CreateSetDelegate<T>(this Type type, string property, object caller)
	{
		Action<T> result = null;
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(property))
		{
			throw new ArgumentNullException("property");
		}
		MethodInfo methodInfo = type.GetPropertySafe<T>(property, isStatic: false)?.GetSetMethod(nonPublic: true);
		if (methodInfo != null)
		{
			result = Delegate.CreateDelegate(typeof(Action<T>), caller, methodInfo, throwOnBindFailure: false) as Action<T>;
		}
		return result;
	}

	/// <summary>
	/// Creates a delegate for a private instance property setter. This delegate is over
	/// ten times faster than reflection, so useful if called frequently on the same object.
	///
	/// This method does not work on indexed properties.
	/// </summary>
	/// <typeparam name="T">The property's type.</typeparam>
	/// <param name="property">The target property.</param>
	/// <param name="caller">The object on which to call the property setter.</param>
	/// <returns>A delegate which calls this property's setter, or null if the property
	/// was null or did not match the type.</returns>
	public static Action<T> CreateSetDelegate<T>(this PropertyInfo property, object caller)
	{
		Action<T> result = null;
		MethodInfo methodInfo = property?.GetSetMethod(nonPublic: true);
		if (methodInfo != null && property.PropertyType.IsAssignableFrom(typeof(T)))
		{
			result = Delegate.CreateDelegate(typeof(Action<T>), caller, methodInfo, throwOnBindFailure: false) as Action<T>;
		}
		return result;
	}

	/// <summary>
	/// Creates a delegate for a private static method. This delegate is over ten times
	/// faster than reflection, so useful if called frequently.
	/// </summary>
	/// <typeparam name="T">A delegate type which matches the method signature.</typeparam>
	/// <param name="type">The declaring type of the target method.</param>
	/// <param name="method">The target method name.</param>
	/// <param name="argumentTypes">The types of the target method arguments, or PPatchTools.
	/// AnyArguments (not recommended, type safety is good) to match any static method with
	/// that name.</param>
	/// <returns>A delegate which calls this method, or null if the method could not be
	/// found or did not match the types.</returns>
	public static T CreateStaticDelegate<T>(this Type type, string method, params Type[] argumentTypes) where T : Delegate
	{
		T result = null;
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(method))
		{
			throw new ArgumentNullException("method");
		}
		MethodInfo methodSafe = type.GetMethodSafe(method, isStatic: true, argumentTypes);
		if (methodSafe != null)
		{
			return Delegate.CreateDelegate(typeof(T), methodSafe, throwOnBindFailure: false) as T;
		}
		return result;
	}

	/// <summary>
	/// Replaces method calls in a transpiled method.
	/// </summary>
	/// <param name="method">The method to patch.</param>
	/// <param name="translation">A mapping from the old method calls to replace, to the
	/// new method calls to use instead.</param>
	/// <returns>A transpiled version of that method that replaces or removes all calls
	/// to the specified methods.</returns>
	private static IEnumerable<CodeInstruction> DoReplaceMethodCalls(IEnumerable<CodeInstruction> method, IDictionary<MethodInfo, MethodInfo> translation)
	{
		MethodInfo remove = RemoveCall;
		int replaced = 0;
		foreach (CodeInstruction item in method)
		{
			OpCode opcode = item.opcode;
			if ((opcode == OpCodes.Call || opcode == OpCodes.Calli || opcode == OpCodes.Callvirt) && item.operand is MethodInfo methodInfo && translation.TryGetValue(methodInfo, out var value))
			{
				if (value != null && value != remove)
				{
					item.opcode = (value.IsStatic ? OpCodes.Call : OpCodes.Callvirt);
					item.operand = value;
					yield return item;
				}
				else
				{
					int n = methodInfo.GetParameters().Length;
					if (!methodInfo.IsStatic)
					{
						n++;
					}
					item.opcode = ((n == 0) ? OpCodes.Nop : OpCodes.Pop);
					item.operand = null;
					yield return item;
					for (int i = 0; i < n - 1; i++)
					{
						yield return new CodeInstruction(OpCodes.Pop, (object)null);
					}
				}
				replaced++;
			}
			else
			{
				yield return item;
			}
		}
	}

	/// <summary>
	/// Dumps the IL body of the method to the debug log.
	///
	/// Only to be used for debugging purposes.
	/// </summary>
	/// <param name="opcodes">The IL instructions to log.</param>
	public static void DumpMethodBody(IEnumerable<CodeInstruction> opcodes)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder(1024);
		stringBuilder.AppendLine("METHOD BODY:");
		foreach (CodeInstruction opcode in opcodes)
		{
			foreach (ExceptionBlock block in opcode.blocks)
			{
				ExceptionBlockType blockType = block.blockType;
				if ((int)blockType == 5)
				{
					stringBuilder.AppendLine("}");
					continue;
				}
				if ((int)blockType != 0)
				{
					stringBuilder.Append("} ");
				}
				stringBuilder.Append(block.blockType);
				stringBuilder.AppendLine(" {");
			}
			foreach (Label label in opcode.labels)
			{
				stringBuilder.Append(label.GetHashCode());
				stringBuilder.Append(": ");
			}
			stringBuilder.Append('\t');
			stringBuilder.Append(opcode.opcode);
			object operand = opcode.operand;
			if (operand != null)
			{
				stringBuilder.Append('\t');
				if (operand is Label)
				{
					stringBuilder.Append(operand.GetHashCode());
				}
				else if (operand is MethodBase method)
				{
					FormatMethodCall(stringBuilder, method);
				}
				else
				{
					stringBuilder.Append(FormatArgument(operand));
				}
			}
			stringBuilder.AppendLine();
		}
		PUtil.LogDebug(stringBuilder.ToString());
	}

	/// <summary>
	/// This method was taken directly from Harmony (https://github.com/pardeike/Harmony)
	/// which is also available under the MIT License.
	/// </summary>
	/// <param name="argument">The argument to format.</param>
	/// <returns>The IL argument in string form.</returns>
	private static string FormatArgument(object argument)
	{
		if (argument == null)
		{
			return "NULL";
		}
		if (argument is MethodBase methodBase)
		{
			return GeneralExtensions.FullDescription(methodBase);
		}
		if (argument is FieldInfo fieldInfo)
		{
			return GeneralExtensions.FullDescription(fieldInfo.FieldType) + " " + GeneralExtensions.FullDescription(fieldInfo.DeclaringType) + "::" + fieldInfo.Name;
		}
		if (argument is Label label)
		{
			return $"Label{label.GetHashCode()}";
		}
		if (argument is Label[] array)
		{
			int num = array.Length;
			string[] array2 = new string[num];
			for (int i = 0; i < num; i++)
			{
				array2[i] = array[i].GetHashCode().ToString();
			}
			return "Labels" + array2.Join();
		}
		if (argument is LocalBuilder localBuilder)
		{
			return $"{localBuilder.LocalIndex} ({localBuilder.LocalType})";
		}
		if (argument is string text)
		{
			return GeneralExtensions.ToLiteral(text, "\"");
		}
		return argument.ToString().Trim();
	}

	/// <summary>
	/// Formats a method call for logging.
	/// </summary>
	/// <param name="result">The location where the log is stored.</param>
	/// <param name="method">The method that is called.</param>
	private static void FormatMethodCall(StringBuilder result, MethodBase method)
	{
		bool flag = true;
		Type declaringType = method.DeclaringType;
		if (method is MethodInfo methodInfo)
		{
			result.Append(methodInfo.ReturnType.Name);
			result.Append(' ');
		}
		if (declaringType != null)
		{
			result.Append(declaringType.Name);
			result.Append('.');
		}
		result.Append(method.Name);
		result.Append('(');
		ParameterInfo[] parameters = method.GetParameters();
		foreach (ParameterInfo parameterInfo in parameters)
		{
			string name = parameterInfo.Name;
			if (!flag)
			{
				result.Append(", ");
			}
			result.Append(parameterInfo.ParameterType.Name);
			if (!string.IsNullOrEmpty(name))
			{
				result.Append(' ');
				result.Append(name);
			}
			if (parameterInfo.IsOptional)
			{
				result.Append(" = ");
				result.Append(parameterInfo.DefaultValue);
			}
			flag = false;
		}
		result.Append(')');
	}

	/// <summary>
	/// Retrieves a field using reflection, or returns null if it does not exist.
	/// </summary>
	/// <param name="type">The base type.</param>
	/// <param name="fieldName">The field name.</param>
	/// <param name="isStatic">true to find static fields, or false to find instance
	/// fields.</param>
	/// <returns>The field, or null if no such field could be found.</returns>
	public static FieldInfo GetFieldSafe(this Type type, string fieldName, bool isStatic)
	{
		FieldInfo result = null;
		if (type != null && !string.IsNullOrEmpty(fieldName))
		{
			try
			{
				BindingFlags bindingFlags = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
				result = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags);
			}
			catch (AmbiguousMatchException thrown)
			{
				PUtil.LogException(thrown);
			}
		}
		return result;
	}

	/// <summary>
	/// Creates a store instruction to the same local as the specified load instruction.
	/// </summary>
	/// <param name="load">The initial load instruction.</param>
	/// <returns>The counterbalancing store instruction.</returns>
	public static CodeInstruction GetMatchingStoreInstruction(CodeInstruction load)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		OpCode opcode = load.opcode;
		if (opcode == OpCodes.Ldloc)
		{
			return new CodeInstruction(OpCodes.Stloc, load.operand);
		}
		if (opcode == OpCodes.Ldloc_S)
		{
			return new CodeInstruction(OpCodes.Stloc_S, load.operand);
		}
		if (opcode == OpCodes.Ldloc_0)
		{
			return new CodeInstruction(OpCodes.Stloc_0, (object)null);
		}
		if (opcode == OpCodes.Ldloc_1)
		{
			return new CodeInstruction(OpCodes.Stloc_1, (object)null);
		}
		if (opcode == OpCodes.Ldloc_2)
		{
			return new CodeInstruction(OpCodes.Stloc_2, (object)null);
		}
		if (opcode == OpCodes.Ldloc_3)
		{
			return new CodeInstruction(OpCodes.Stloc_3, (object)null);
		}
		return new CodeInstruction(OpCodes.Pop, (object)null);
	}

	/// <summary>
	/// Retrieves a method using reflection, or returns null if it does not exist.
	/// </summary>
	/// <param name="type">The base type.</param>
	/// <param name="methodName">The method name.</param>
	/// <param name="isStatic">true to find static methods, or false to find instance
	/// methods.</param>
	/// <param name="arguments">The method argument types. If null is provided, any
	/// argument types are matched, whereas no arguments match only void methods.</param>
	/// <returns>The method, or null if no such method could be found.</returns>
	public static MethodInfo GetMethodSafe(this Type type, string methodName, bool isStatic, params Type[] arguments)
	{
		MethodInfo result = null;
		if (type != null && arguments != null && !string.IsNullOrEmpty(methodName))
		{
			try
			{
				BindingFlags bindingFlags = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
				result = ((arguments.Length != 1 || !(arguments[0] == null)) ? type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags, null, arguments, new ParameterModifier[arguments.Length]) : type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags));
			}
			catch (AmbiguousMatchException thrown)
			{
				PUtil.LogException(thrown);
			}
		}
		return result;
	}

	/// <summary>
	/// Retrieves the method matching the criteria that has the most arguments. Useful when
	/// patching a base game overload is added with extra parameters (for binary
	/// compatibility) where the shorter methods just call the longer one.
	/// </summary>
	/// <param name="type">The base type.</param>
	/// <param name="methodName">The netgh</param>
	/// <param name="isStatic">true to find static methods, or false to find instance
	/// methods.</param>
	/// <param name="arguments">The method argument types. If null is provided, any
	/// argument types are matched, whereas no arguments match only void methods.</param>
	/// <returns>The method, or null if no such method could be found.</returns>
	public static MethodInfo GetOverloadWithMostArguments(this Type type, string methodName, bool isStatic, params Type[] arguments)
	{
		MethodInfo result = null;
		if (type != null && arguments != null && !string.IsNullOrEmpty(methodName))
		{
			MethodInfo[] methods = type.GetMethods((BindingFlags)(0x30 | (isStatic ? 8 : 4)));
			int num = methods.Length;
			int num2 = -1;
			for (int i = 0; i < num; i++)
			{
				MethodInfo methodInfo = methods[i];
				int num3;
				if (methodInfo.Name == methodName && (num3 = ParametersMatch(methodInfo, arguments)) > num2)
				{
					num2 = num3;
					result = methodInfo;
				}
			}
		}
		return result;
	}

	/// <summary>
	/// Retrieves a property using reflection, or returns null if it does not exist.
	/// </summary>
	/// <param name="type">The base type.</param>
	/// <param name="propName">The property name.</param>
	/// <param name="isStatic">true to find static properties, or false to find instance
	/// properties.</param>
	/// <typeparam name="T">The property field type.</typeparam>
	/// <returns>The property, or null if no such property could be found.</returns>
	public static PropertyInfo GetPropertySafe<T>(this Type type, string propName, bool isStatic)
	{
		PropertyInfo result = null;
		if (type != null && !string.IsNullOrEmpty(propName))
		{
			try
			{
				BindingFlags bindingFlags = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
				result = type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags, null, typeof(T), Type.EmptyTypes, null);
			}
			catch (AmbiguousMatchException thrown)
			{
				PUtil.LogException(thrown);
			}
		}
		return result;
	}

	/// <summary>
	/// Retrieves an indexed property using reflection, or returns null if it does not
	/// exist.
	/// </summary>
	/// <param name="type">The base type.</param>
	/// <param name="propName">The property name.</param>
	/// <param name="isStatic">true to find static properties, or false to find instance
	/// properties.</param>
	/// <param name="arguments">The property indexer's arguments.</param>
	/// <typeparam name="T">The property field type.</typeparam>
	/// <returns>The property, or null if no such property could be found.</returns>
	public static PropertyInfo GetPropertyIndexedSafe<T>(this Type type, string propName, bool isStatic, params Type[] arguments)
	{
		PropertyInfo result = null;
		if (type != null && arguments != null && !string.IsNullOrEmpty(propName))
		{
			try
			{
				BindingFlags bindingFlags = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
				result = type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags, null, typeof(T), arguments, new ParameterModifier[arguments.Length]);
			}
			catch (AmbiguousMatchException thrown)
			{
				PUtil.LogException(thrown);
			}
		}
		return result;
	}

	/// <summary>
	/// Retrieves a type using its full name (including namespace). However, the assembly
	/// name is optional, as this method searches all assemblies in the current
	/// AppDomain if it is null or empty.
	/// </summary>
	/// <param name="name">The type name to retrieve.</param>
	/// <param name="assemblyName">If specified, the name of the assembly that contains
	/// the type. No other assembly name will be searched if this parameter is not null
	/// or empty. The assembly name might not match the DLL name, use a decompiler to
	/// make sure.</param>
	/// <returns>The type, or null if the type is not found or cannot be loaded.</returns>
	public static Type GetTypeSafe(string name, string assemblyName = null)
	{
		Type type = null;
		if (string.IsNullOrEmpty(assemblyName))
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				try
				{
					type = assembly.GetType(name, throwOnError: false);
				}
				catch (IOException)
				{
				}
				catch (BadImageFormatException)
				{
				}
				if (type != null)
				{
					break;
				}
			}
		}
		else
		{
			try
			{
				type = Type.GetType(name + ", " + assemblyName, throwOnError: false);
			}
			catch (TargetInvocationException thrown)
			{
				PUtil.LogWarning("Unable to load type {0} from assembly {1}:".F(name, assemblyName));
				PUtil.LogExcWarn(thrown);
			}
			catch (ArgumentException thrown2)
			{
				PUtil.LogWarning("Unable to load type {0} from assembly {1}:".F(name, assemblyName));
				PUtil.LogExcWarn(thrown2);
			}
			catch (ReflectionTypeLoadException ex3)
			{
				PUtil.LogWarning("Unable to load type {0} from assembly {1}:".F(name, assemblyName));
				Exception[] loaderExceptions = ex3.LoaderExceptions;
				foreach (Exception ex4 in loaderExceptions)
				{
					if (ex4 != null)
					{
						PUtil.LogExcWarn(ex4);
					}
				}
			}
			catch (IOException)
			{
			}
			catch (BadImageFormatException)
			{
			}
		}
		return type;
	}

	/// <summary>
	/// Checks to see if a patch with the specified method name (the method used in the
	/// patch class) and type is defined.
	/// </summary>
	/// <param name="instance">The Harmony instance to query for patches. Unused.</param>
	/// <param name="target">The target method to search for patches.</param>
	/// <param name="type">The patch type to look up.</param>
	/// <param name="name">The patch method name to look up (name as declared by patch owner).</param>
	/// <returns>true if such a patch was found, or false otherwise</returns>
	public static bool HasPatchWithMethodName(Harmony instance, MethodBase target, HarmonyPatchType type, string name)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected I4, but got Unknown
		bool flag = false;
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		Patches patchInfo = Harmony.GetPatchInfo(target);
		if (patchInfo != null)
		{
			ICollection<Patch> collection;
			switch ((int)type)
			{
			case 1:
				collection = patchInfo.Prefixes;
				break;
			case 2:
				collection = patchInfo.Postfixes;
				break;
			case 3:
				collection = patchInfo.Transpilers;
				break;
			default:
				if (patchInfo.Transpilers != null)
				{
					flag = HasPatchWithMethodName(patchInfo.Transpilers, name);
				}
				if (patchInfo.Prefixes != null)
				{
					flag = flag || HasPatchWithMethodName(patchInfo.Prefixes, name);
				}
				collection = patchInfo.Postfixes;
				break;
			}
			if (collection != null)
			{
				flag = flag || HasPatchWithMethodName(collection, name);
			}
		}
		return flag;
	}

	/// <summary>
	/// Checks to see if the patch list has a method with the specified name.
	/// </summary>
	/// <param name="patchList">The patch list to search.</param>
	/// <param name="name">The declaring method name to look up.</param>
	/// <returns>true if a patch matches that name, or false otherwise</returns>
	private static bool HasPatchWithMethodName(IEnumerable<Patch> patchList, string name)
	{
		bool result = false;
		foreach (Patch patch in patchList)
		{
			if (patch.PatchMethod.Name == name)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	/// <summary>
	/// Checks to see if an instruction opcode is a branch instruction.
	/// </summary>
	/// <param name="opcode">The opcode to check.</param>
	/// <returns>true if it is a branch, or false otherwise.</returns>
	public static bool IsConditionalBranchInstruction(this OpCode opcode)
	{
		return PTranspilerTools.IsConditionalBranchInstruction(opcode);
	}

	/// <summary>
	/// Adds a logger to all unhandled exceptions.
	/// </summary>
	[Obsolete("Do not use this method in production code. Make sure to remove it in release builds, or disable it with #if DEBUG.")]
	public static void LogAllExceptions()
	{
		PUtil.LogWarning("PLib in mod " + Assembly.GetCallingAssembly().GetName()?.Name + " is logging ALL unhandled exceptions!");
		PTranspilerTools.LogAllExceptions();
	}

	/// <summary>
	/// Adds a logger to all failed assertions. The assertions will still fail, but a stack
	/// trace will be printed for each failed assertion.
	/// </summary>
	[Obsolete("Do not use this method in production code. Make sure to remove it in release builds, or disable it with #if DEBUG.")]
	public static void LogAllFailedAsserts()
	{
		PUtil.LogWarning("PLib in mod " + Assembly.GetCallingAssembly().GetName()?.Name + " is logging ALL failed assertions!");
		PTranspilerTools.LogAllFailedAsserts();
	}

	/// <summary>
	/// Compares the method parameter list to the required parameters.
	/// </summary>
	/// <param name="method">The method to check.</param>
	/// <param name="required">The types its parameters need to have.</param>
	/// <returns>Negative if the method did not match, otherwise the total number of method parameters.</returns>
	private static int ParametersMatch(MethodBase method, Type[] required)
	{
		int result = -1;
		ParameterInfo[] parameters = method.GetParameters();
		int num = parameters.Length;
		int num2 = required.Length;
		if (num2 == 1 && required[0] == null)
		{
			result = num;
		}
		else if (num >= num2)
		{
			bool flag = true;
			for (int i = 0; i < num2 && flag; i++)
			{
				flag = parameters[i].ParameterType == required[i];
			}
			if (flag)
			{
				result = num;
			}
		}
		return result;
	}

	/// <summary>
	/// Transpiles a method to replace instances of one constant value with another.
	/// </summary>
	/// <param name="method">The method to patch.</param>
	/// <param name="oldValue">The old constant to remove.</param>
	/// <param name="newValue">The new constant to replace.</param>
	/// <param name="all">true to replace all instances, or false to replace the first
	/// instance (default).</param>
	/// <returns>A transpiled version of that method which replaces instances of the first
	/// constant with that of the second.</returns>
	public static IEnumerable<CodeInstruction> ReplaceConstant(IEnumerable<CodeInstruction> method, double oldValue, double newValue, bool all = false)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		int replaced = 0;
		foreach (CodeInstruction item in method)
		{
			if (item.opcode == OpCodes.Ldc_R8 && item.operand is double num && num == oldValue)
			{
				if (all || replaced == 0)
				{
					item.operand = newValue;
				}
				replaced++;
			}
			yield return item;
		}
	}

	/// <summary>
	/// Transpiles a method to replace instances of one constant value with another.
	/// </summary>
	/// <param name="method">The method to patch.</param>
	/// <param name="oldValue">The old constant to remove.</param>
	/// <param name="newValue">The new constant to replace.</param>
	/// <param name="all">true to replace all instances, or false to replace the first
	/// instance (default).</param>
	/// <returns>A transpiled version of that method which replaces instances of the first
	/// constant with that of the second.</returns>
	public static IEnumerable<CodeInstruction> ReplaceConstant(IEnumerable<CodeInstruction> method, float oldValue, float newValue, bool all = false)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		int replaced = 0;
		foreach (CodeInstruction item in method)
		{
			if (item.opcode == OpCodes.Ldc_R4 && item.operand is float num && num == oldValue)
			{
				if (all || replaced == 0)
				{
					item.operand = newValue;
				}
				replaced++;
			}
			yield return item;
		}
	}

	/// <summary>
	/// Transpiles a method to replace instances of one constant value with another.
	///
	/// Note that values of type byte, short, char, and bool are also represented with "i4"
	/// constants which can be targeted by this method.
	/// </summary>
	/// <param name="method">The method to patch.</param>
	/// <param name="oldValue">The old constant to remove.</param>
	/// <param name="newValue">The new constant to replace.</param>
	/// <param name="all">true to replace all instances, or false to replace the first
	/// instance (default).</param>
	/// <returns>A transpiled version of that method which replaces instances of the first
	/// constant with that of the second.</returns>
	public static IEnumerable<CodeInstruction> ReplaceConstant(IEnumerable<CodeInstruction> method, int oldValue, int newValue, bool all = false)
	{
		int replaced = 0;
		bool quickCode = oldValue >= -1 && oldValue <= 8;
		OpCode qc = OpCodes.Nop;
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		if (quickCode)
		{
			qc = PTranspilerTools.LOAD_INT[oldValue + 1];
		}
		foreach (CodeInstruction item in method)
		{
			OpCode opcode = item.opcode;
			object operand = item.operand;
			if ((opcode == OpCodes.Ldc_I4 && operand is int num && num == oldValue) || (opcode == OpCodes.Ldc_I4_S && ((operand is byte b && b == oldValue) || (operand is sbyte b2 && b2 == oldValue))) || (quickCode && qc == opcode))
			{
				if (all || replaced == 0)
				{
					PTranspilerTools.ModifyLoadI4(item, newValue);
				}
				replaced++;
			}
			yield return item;
		}
	}

	/// <summary>
	/// Transpiles a method to replace instances of one constant value with another.
	/// </summary>
	/// <param name="method">The method to patch.</param>
	/// <param name="oldValue">The old constant to remove.</param>
	/// <param name="newValue">The new constant to replace.</param>
	/// <param name="all">true to replace all instances, or false to replace the first
	/// instance (default).</param>
	/// <returns>A transpiled version of that method which replaces instances of the first
	/// constant with that of the second.</returns>
	public static IEnumerable<CodeInstruction> ReplaceConstant(IEnumerable<CodeInstruction> method, long oldValue, long newValue, bool all = false)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		int replaced = 0;
		foreach (CodeInstruction item in method)
		{
			if (item.opcode == OpCodes.Ldc_I8 && item.operand is long num && num == oldValue)
			{
				if (all || replaced == 0)
				{
					item.operand = newValue;
				}
				replaced++;
			}
			yield return item;
		}
	}

	/// <summary>
	/// Transpiles a method to remove all calls to the specified victim method.
	/// </summary>
	/// <param name="method">The method to patch.</param>
	/// <param name="victim">The old method calls to remove.</param>
	/// <returns>A transpiled version of that method that removes all calls to method.</returns>
	/// <exception cref="T:System.ArgumentException">If the method being removed had a return value
	/// (with what would it be replaced?).</exception>
	public static IEnumerable<CodeInstruction> RemoveMethodCall(IEnumerable<CodeInstruction> method, MethodInfo victim)
	{
		return ReplaceMethodCallSafe(method, new Dictionary<MethodInfo, MethodInfo> { { victim, RemoveCall } });
	}

	/// <summary>
	/// A placeholder method for signaling call removal. Not actually called.
	/// </summary>
	private static void RemoveMethodCallPrivate()
	{
	}

	/// <summary>
	/// Transpiles a method to replace all calls to the specified victim method with
	/// another method, altering the call type if necessary. The argument types and return
	/// type must match exactly, including in/out/ref parameters.
	///
	/// If replacing an instance method call with a static method, the first argument
	/// will receive the "this" which the old method would have received.
	///
	/// If newMethod is null, the calls will all be removed silently instead. This will
	/// fail if the method call being removed had a return type (what would it be replaced
	/// with?); in those cases, declare an empty method with the same signature and
	/// replace it instead.
	/// </summary>
	/// <param name="method">The method to patch.</param>
	/// <param name="victim">The old method calls to remove.</param>
	/// <param name="newMethod">The new method to replace, or null to delete the calls.</param>
	/// <returns>A transpiled version of that method that replaces or removes all calls
	/// to method.</returns>
	/// <exception cref="T:System.ArgumentException">If the new method's argument types do not
	/// exactly match the old method's argument types.</exception>
	[Obsolete("This method is unsafe. Use the RemoveMethodCall or ReplaceMethodCallSafe versions instead.")]
	public static IEnumerable<CodeInstruction> ReplaceMethodCall(IEnumerable<CodeInstruction> method, MethodInfo victim, MethodInfo newMethod = null)
	{
		if (newMethod == null)
		{
			newMethod = RemoveCall;
		}
		return ReplaceMethodCallSafe(method, new Dictionary<MethodInfo, MethodInfo> { { victim, newMethod } });
	}

	/// <summary>
	/// Transpiles a method to replace all calls to the specified victim method with
	/// another method, altering the call type if necessary. The argument types and return
	/// type must match exactly, including in/out/ref parameters.
	///
	/// If replacing an instance method call with a static method, the first argument
	/// will receive the "this" which the old method would have received.
	/// </summary>
	/// <param name="method">The method to patch.</param>
	/// <param name="victim">The old method calls to remove.</param>
	/// <param name="newMethod">The new method to replace.</param>
	/// <returns>A transpiled version of that method that replaces all calls to method.</returns>
	/// <exception cref="T:System.ArgumentException">If the new method's argument types do not
	/// exactly match the old method's argument types.</exception>
	public static IEnumerable<CodeInstruction> ReplaceMethodCallSafe(IEnumerable<CodeInstruction> method, MethodInfo victim, MethodInfo newMethod)
	{
		if (newMethod == null)
		{
			throw new ArgumentNullException("newMethod");
		}
		return ReplaceMethodCallSafe(method, new Dictionary<MethodInfo, MethodInfo> { { victim, newMethod } });
	}

	/// <summary>
	/// Transpiles a method to replace calls to the specified victim methods with
	/// replacement methods, altering the call type if necessary.
	///
	/// Each key to value pair must meet the criteria defined in
	/// ReplaceMethodCall(TranspiledMethod, MethodInfo, MethodInfo).
	/// </summary>
	/// <param name="method">The method to patch.</param>
	/// <param name="translation">A mapping from the old method calls to replace, to the
	/// new method calls to use instead.</param>
	/// <returns>A transpiled version of that method that replaces or removes all calls
	/// to the specified methods.</returns>
	/// <exception cref="T:System.ArgumentException">If any of the new methods' argument types do
	/// not exactly match the old methods' argument types.</exception>
	[Obsolete("This method is unsafe. Use ReplaceMethodCallSafe instead.")]
	public static IEnumerable<CodeInstruction> ReplaceMethodCall(IEnumerable<CodeInstruction> method, IDictionary<MethodInfo, MethodInfo> translation)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		if (translation == null)
		{
			throw new ArgumentNullException("translation");
		}
		foreach (KeyValuePair<MethodInfo, MethodInfo> item in translation)
		{
			MethodInfo key = item.Key;
			MethodInfo value = item.Value;
			if (key == null)
			{
				throw new ArgumentNullException("victim");
			}
			if (value != null)
			{
				PTranspilerTools.CompareMethodParams(key, key.GetParameterTypes(), value);
			}
			else if (key.ReturnType != typeof(void))
			{
				throw new ArgumentException("Cannot remove method {0} with a return value".F(key.Name));
			}
		}
		return DoReplaceMethodCalls(method, translation);
	}

	/// <summary>
	/// Transpiles a method to replace calls to the specified victim methods with
	/// replacement methods, altering the call type if necessary.
	///
	/// Each key to value pair must meet the criteria defined in
	/// ReplaceMethodCallSafe(TranspiledMethod, MethodInfo, MethodInfo).
	/// </summary>
	/// <param name="method">The method to patch.</param>
	/// <param name="translation">A mapping from the old method calls to replace, to the
	/// new method calls to use instead.</param>
	/// <returns>A transpiled version of that method that replaces or removes all calls
	/// to the specified methods.</returns>
	/// <exception cref="T:System.ArgumentException">If any of the new methods' argument types do
	/// not exactly match the old methods' argument types.</exception>
	public static IEnumerable<CodeInstruction> ReplaceMethodCallSafe(IEnumerable<CodeInstruction> method, IDictionary<MethodInfo, MethodInfo> translation)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		if (translation == null)
		{
			throw new ArgumentNullException("translation");
		}
		MethodInfo removeCall = RemoveCall;
		foreach (KeyValuePair<MethodInfo, MethodInfo> item in translation)
		{
			MethodInfo key = item.Key;
			MethodInfo value = item.Value;
			if (key == null)
			{
				throw new ArgumentNullException("victim");
			}
			if (value == null)
			{
				throw new ArgumentNullException("newMethod");
			}
			if (value == removeCall)
			{
				if (key.ReturnType != typeof(void))
				{
					throw new ArgumentException("Cannot remove method {0} with a return value".F(key.Name));
				}
			}
			else
			{
				PTranspilerTools.CompareMethodParams(key, key.GetParameterTypes(), value);
			}
		}
		return DoReplaceMethodCalls(method, translation);
	}

	/// <summary>
	/// Attempts to read a static field value from an object of a type not in this assembly.
	///
	/// If this operation is expected to be performed more than once on the same object,
	/// use a delegate. If the type of the object is known, use Detours.
	/// </summary>
	/// <typeparam name="T">The type of the value to read.</typeparam>
	/// <param name="type">The type whose static field should be read.</param>
	/// <param name="name">The field name.</param>
	/// <param name="value">The location where the field value will be stored.</param>
	/// <returns>true if the field was read, or false if the field was not found or
	/// has the wrong type.</returns>
	public static bool TryGetFieldValue<T>(Type type, string name, out T value)
	{
		bool result = false;
		if (type != null && !string.IsNullOrEmpty(name))
		{
			FieldInfo fieldSafe = type.GetFieldSafe(name, isStatic: true);
			if (fieldSafe != null && fieldSafe.GetValue(null) is T val)
			{
				result = true;
				value = val;
			}
			else
			{
				value = default(T);
			}
		}
		else
		{
			value = default(T);
		}
		return result;
	}

	/// <summary>
	/// Attempts to read a non-static field value from an object of a type not in this
	/// assembly.
	///
	/// If this operation is expected to be performed more than once on the same object,
	/// use a delegate. If the type of the object is known, use Detours.
	/// </summary>
	/// <typeparam name="T">The type of the value to read.</typeparam>
	/// <param name="source">The source object.</param>
	/// <param name="name">The field name.</param>
	/// <param name="value">The location where the field value will be stored.</param>
	/// <returns>true if the field was read, or false if the field was not found or
	/// has the wrong type.</returns>
	public static bool TryGetFieldValue<T>(object source, string name, out T value)
	{
		if (source != null && !string.IsNullOrEmpty(name))
		{
			FieldInfo fieldSafe = source.GetType().GetFieldSafe(name, isStatic: false);
			if (fieldSafe != null && fieldSafe.GetValue(source) is T val)
			{
				value = val;
				return true;
			}
		}
		value = default(T);
		return false;
	}

	/// <summary>
	/// Attempts to read a property value from an object of a type not in this assembly.
	///
	/// If this operation is expected to be performed more than once on the same object,
	/// use a delegate. If the type of the object is known, use Detours.
	/// </summary>
	/// <typeparam name="T">The type of the value to read.</typeparam>
	/// <param name="source">The source object.</param>
	/// <param name="name">The property name.</param>
	/// <param name="value">The location where the property value will be stored.</param>
	/// <returns>true if the property was read, or false if the property was not found or
	/// has the wrong type.</returns>
	public static bool TryGetPropertyValue<T>(object source, string name, out T value)
	{
		if (source != null && !string.IsNullOrEmpty(name))
		{
			PropertyInfo propertySafe = source.GetType().GetPropertySafe<T>(name, isStatic: false);
			if (propertySafe != null && propertySafe.GetIndexParameters().Length < 1 && propertySafe.GetValue(source, null) is T val)
			{
				value = val;
				return true;
			}
		}
		value = default(T);
		return false;
	}

	/// <summary>
	/// Transpiles a method to wrap it with a try/catch that logs and rethrows all
	/// exceptions.
	/// </summary>
	/// <param name="method">The method body to patch.</param>
	/// <param name="generator">The IL generator to make labels.</param>
	/// <returns>A transpiled version of that method that is wrapped with an error
	/// logger.</returns>
	public static IEnumerable<CodeInstruction> WrapWithErrorLogger(IEnumerable<CodeInstruction> method, ILGenerator generator)
	{
		MethodInfo logger = typeof(PUtil).GetMethodSafe("LogException", true, typeof(Exception));
		IEnumerator<CodeInstruction> ee = method.GetEnumerator();
		if (ee.MoveNext())
		{
			bool isFirst = true;
			Label endMethod = generator.DefineLabel();
			CodeInstruction last;
			bool hasNext;
			do
			{
				last = ee.Current;
				if (isFirst)
				{
					last.blocks.Add(new ExceptionBlock((ExceptionBlockType)0, (Type)null));
				}
				hasNext = ee.MoveNext();
				isFirst = false;
				if (hasNext)
				{
					yield return last;
				}
			}
			while (hasNext);
			last.opcode = OpCodes.Nop;
			last.operand = null;
			yield return last;
			yield return new CodeInstruction(OpCodes.Leave, (object)endMethod);
			CodeInstruction val = ((logger != null) ? new CodeInstruction(OpCodes.Call, (object)logger) : new CodeInstruction(OpCodes.Pop, (object)null));
			val.blocks.Add(new ExceptionBlock((ExceptionBlockType)1, typeof(Exception)));
			yield return val;
			yield return new CodeInstruction(OpCodes.Rethrow, (object)null);
			CodeInstruction val2 = new CodeInstruction(OpCodes.Leave, (object)endMethod);
			val2.blocks.Add(new ExceptionBlock((ExceptionBlockType)5, (Type)null));
			yield return val2;
			CodeInstruction val3 = new CodeInstruction(OpCodes.Ret, (object)null);
			val3.labels.Add(endMethod);
			yield return val3;
		}
		ee.Dispose();
	}
}
