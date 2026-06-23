using System;
using System.Reflection;
using System.Reflection.Emit;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.Detours;

/// <summary>
/// Efficiently detours around many changes in the game by creating detour methods and
/// accessors which are resilient against many types of source compatible but binary
/// incompatible changes.
/// </summary>
public static class PDetours
{
	/// <summary>
	/// Stores information about a delegate.
	/// </summary>
	private sealed class DelegateInfo
	{
		/// <summary>
		/// The delegate's type.
		/// </summary>
		private readonly Type delegateType;

		/// <summary>
		/// The delegate's parameter types.
		/// </summary>
		public readonly Type[] parameterTypes;

		/// <summary>
		/// The delegate's return types.
		/// </summary>
		public readonly Type returnType;

		/// <summary>
		/// Creates delegate information on the specified delegate type.
		/// </summary>
		/// <param name="delegateType">The delegate type to wrap.</param>
		/// <returns>Information about that delegate's return and parameter types.</returns>
		public static DelegateInfo Create(Type delegateType)
		{
			if (delegateType == null)
			{
				throw new ArgumentNullException("delegateType");
			}
			MethodInfo methodSafe = delegateType.GetMethodSafe("Invoke", isStatic: false, PPatchTools.AnyArguments);
			if (methodSafe == null)
			{
				throw new ArgumentException("Invalid delegate type: " + delegateType);
			}
			return new DelegateInfo(delegateType, methodSafe.GetParameterTypes(), methodSafe.ReturnType);
		}

		private DelegateInfo(Type delegateType, Type[] parameterTypes, Type returnType)
		{
			this.delegateType = delegateType;
			this.parameterTypes = parameterTypes;
			this.returnType = returnType;
		}

		public override string ToString()
		{
			return "DelegateInfo[delegate={0},return={1},parameters={2}]".F(delegateType, returnType, parameterTypes.Join());
		}
	}

	/// <summary>
	/// Creates a dynamic detour method of the specified delegate type to wrap a base game
	/// method with the same name as the delegate type. The dynamic method will
	/// automatically adapt if optional parameters are added, filling in their default
	/// values.
	/// </summary>
	/// <typeparam name="D">The delegate type to be used to call the detour.</typeparam>
	/// <param name="type">The target type.</param>
	/// <returns>The detour that will call the method with the name of the delegate type.</returns>
	/// <exception cref="T:PeterHan.PLib.Detours.DetourException">If the delegate does not match any valid target method.</exception>
	public static D Detour<D>(this Type type) where D : Delegate
	{
		return type.Detour<D>(typeof(D).Name);
	}

	/// <summary>
	/// Creates a dynamic detour method of the specified delegate type to wrap a base game
	/// method with the specified name. The dynamic method will automatically adapt if
	/// optional parameters are added, filling in their default values.
	/// </summary>
	/// <typeparam name="D">The delegate type to be used to call the detour.</typeparam>
	/// <param name="type">The target type.</param>
	/// <param name="name">The method name.</param>
	/// <returns>The detour that will call that method.</returns>
	/// <exception cref="T:PeterHan.PLib.Detours.DetourException">If the delegate does not match any valid target method.</exception>
	public static D Detour<D>(this Type type, string name) where D : Delegate
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		DelegateInfo expected = DelegateInfo.Create(typeof(D));
		MethodInfo methodInfo = null;
		int num = int.MaxValue;
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo2 in array)
		{
			if (!(methodInfo2.Name == name))
			{
				continue;
			}
			try
			{
				int num2 = ValidateDelegate(expected, methodInfo2, methodInfo2.ReturnType).Length;
				if (num2 < num)
				{
					num = num2;
					methodInfo = methodInfo2;
				}
			}
			catch (DetourException)
			{
			}
		}
		if (methodInfo == null)
		{
			throw new DetourException("No match found for {1}.{0}".F(name, type.FullName));
		}
		return methodInfo.Detour<D>();
	}

	/// <summary>
	/// Creates a dynamic detour method of the specified delegate type to wrap a base game
	/// constructor. The dynamic method will automatically adapt if optional parameters
	/// are added, filling in their default values.
	/// </summary>
	/// <typeparam name="D">The delegate type to be used to call the detour.</typeparam>
	/// <param name="type">The target type.</param>
	/// <returns>The detour that will call that type's constructor.</returns>
	/// <exception cref="T:PeterHan.PLib.Detours.DetourException">If the delegate does not match any valid target method.</exception>
	public static D DetourConstructor<D>(this Type type) where D : Delegate
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		DelegateInfo expected = DelegateInfo.Create(typeof(D));
		ConstructorInfo constructorInfo = null;
		int num = int.MaxValue;
		ConstructorInfo[] array = constructors;
		foreach (ConstructorInfo constructorInfo2 in array)
		{
			try
			{
				int num2 = ValidateDelegate(expected, constructorInfo2, type).Length;
				if (num2 < num)
				{
					num = num2;
					constructorInfo = constructorInfo2;
				}
			}
			catch (DetourException)
			{
			}
		}
		if (constructorInfo == null)
		{
			throw new DetourException("No match found for {0} constructor".F(type.FullName));
		}
		return constructorInfo.Detour<D>();
	}

	/// <summary>
	/// Creates a dynamic detour method of the specified delegate type to wrap a base game
	/// method with the specified name. The dynamic method will automatically adapt if
	/// optional parameters are added, filling in their default values.
	///
	/// This overload creates a lazy detour that only performs the expensive reflection
	/// when it is first used.
	/// </summary>
	/// <typeparam name="D">The delegate type to be used to call the detour.</typeparam>
	/// <param name="type">The target type.</param>
	/// <param name="name">The method name.</param>
	/// <returns>The detour that will call that method.</returns>
	/// <exception cref="T:PeterHan.PLib.Detours.DetourException">If the delegate does not match any valid target method.</exception>
	public static DetouredMethod<D> DetourLazy<D>(this Type type, string name) where D : Delegate
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		return new DetouredMethod<D>(type, name);
	}

	/// <summary>
	/// Creates a dynamic detour method of the specified delegate type to wrap a base game
	/// method with the specified name. The dynamic method will automatically adapt if
	/// optional parameters are added, filling in their default values.
	/// </summary>
	/// <typeparam name="D">The delegate type to be used to call the detour.</typeparam>
	/// <param name="target">The target method to be called.</param>
	/// <returns>The detour that will call that method.</returns>
	/// <exception cref="T:PeterHan.PLib.Detours.DetourException">If the delegate does not match the target.</exception>
	public static D Detour<D>(this MethodInfo target) where D : Delegate
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (target.ContainsGenericParameters)
		{
			throw new ArgumentException("Generic types must have all parameters defined");
		}
		DelegateInfo delegateInfo = DelegateInfo.Create(typeof(D));
		Type declaringType = target.DeclaringType;
		Type[] parameterTypes = delegateInfo.parameterTypes;
		ParameterInfo[] actualParams = ValidateDelegate(delegateInfo, target, target.ReturnType);
		int offset = ((!target.IsStatic) ? 1 : 0);
		if (declaringType == null)
		{
			throw new ArgumentException("Method is not declared by an actual type");
		}
		DynamicMethod dynamicMethod = new DynamicMethod(target.Name + "_Detour", delegateInfo.returnType, parameterTypes, declaringType, skipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		LoadParameters(iLGenerator, actualParams, parameterTypes, offset);
		if (declaringType.IsValueType || target.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Call, target);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Callvirt, target);
		}
		iLGenerator.Emit(OpCodes.Ret);
		FinishDynamicMethod(dynamicMethod, actualParams, parameterTypes, offset);
		return dynamicMethod.CreateDelegate(typeof(D)) as D;
	}

	/// <summary>
	/// Creates a dynamic detour method of the specified delegate type to wrap a base game
	/// constructor. The dynamic method will automatically adapt if optional parameters
	/// are added, filling in their default values.
	/// </summary>
	/// <typeparam name="D">The delegate type to be used to call the detour.</typeparam>
	/// <param name="target">The target constructor to be called.</param>
	/// <returns>The detour that will call that constructor.</returns>
	/// <exception cref="T:PeterHan.PLib.Detours.DetourException">If the delegate does not match the target.</exception>
	public static D Detour<D>(this ConstructorInfo target) where D : Delegate
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (target.ContainsGenericParameters)
		{
			throw new ArgumentException("Generic types must have all parameters defined");
		}
		if (target.IsStatic)
		{
			throw new ArgumentException("Static constructors cannot be called manually");
		}
		DelegateInfo delegateInfo = DelegateInfo.Create(typeof(D));
		Type declaringType = target.DeclaringType;
		Type[] parameterTypes = delegateInfo.parameterTypes;
		ParameterInfo[] actualParams = ValidateDelegate(delegateInfo, target, declaringType);
		if (declaringType == null)
		{
			throw new ArgumentException("Method is not declared by an actual type");
		}
		DynamicMethod dynamicMethod = new DynamicMethod("Constructor_Detour", delegateInfo.returnType, parameterTypes, declaringType, skipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		LoadParameters(iLGenerator, actualParams, parameterTypes, 0);
		iLGenerator.Emit(OpCodes.Newobj, target);
		iLGenerator.Emit(OpCodes.Ret);
		FinishDynamicMethod(dynamicMethod, actualParams, parameterTypes, 0);
		return dynamicMethod.CreateDelegate(typeof(D)) as D;
	}

	/// <summary>
	/// Creates dynamic detour methods to wrap a base game field or property with the
	/// specified name. The detour will still work even if the field is converted to a
	/// source compatible property and vice versa.
	/// </summary>
	/// <typeparam name="P">The type of the parent class.</typeparam>
	/// <typeparam name="T">The type of the field or property element.</typeparam>
	/// <param name="name">The name of the field or property to be accessed.</param>
	/// <returns>A detour element that wraps the field or property with common getter and
	/// setter delegates which will work on both types.</returns>
	public static IDetouredField<P, T> DetourField<P, T>(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		Type typeFromHandle = typeof(P);
		FieldInfo field = typeFromHandle.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (field == null)
		{
			try
			{
				PropertyInfo property = typeFromHandle.GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (property == null)
				{
					throw new DetourException("Unable to find {0} on type {1}".F(name, typeof(P).FullName));
				}
				return DetourProperty<P, T>(property);
			}
			catch (AmbiguousMatchException)
			{
				throw new DetourException("Unable to find {0} on type {1}".F(name, typeof(P).FullName));
			}
		}
		if (!typeFromHandle.IsValueType)
		{
			if (typeFromHandle.IsByRef)
			{
				Type elementType = typeFromHandle.GetElementType();
				if ((object)elementType != null && elementType.IsValueType)
				{
					goto IL_00ca;
				}
			}
			return DetourField<P, T>(field);
		}
		goto IL_00ca;
		IL_00ca:
		throw new ArgumentException("For accessing struct fields, use DetourStructField");
	}

	/// <summary>
	/// Creates dynamic detour methods to wrap a base game field or property with the
	/// specified name. The detour will still work even if the field is converted to a
	/// source compatible property and vice versa.
	///
	/// This overload creates a lazy detour that only performs the expensive reflection
	/// when it is first used.
	/// </summary>
	/// <typeparam name="P">The type of the parent class.</typeparam>
	/// <typeparam name="T">The type of the field or property element.</typeparam>
	/// <param name="name">The name of the field or property to be accessed.</param>
	/// <returns>A detour element that wraps the field or property with common getter and
	/// setter delegates which will work on both types.</returns>
	public static IDetouredField<P, T> DetourFieldLazy<P, T>(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		return new LazyDetouredField<P, T>(typeof(P), name);
	}

	/// <summary>
	/// Creates dynamic detour methods to wrap a base game field with the specified name.
	/// </summary>
	/// <typeparam name="P">The type of the parent class.</typeparam>
	/// <typeparam name="T">The type of the field element.</typeparam>
	/// <param name="target">The field which will be accessed.</param>
	/// <returns>A detour element that wraps the field with a common interface matching
	/// that of a detoured property.</returns>
	private static IDetouredField<P, T> DetourField<P, T>(FieldInfo target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		Type declaringType = target.DeclaringType;
		string name = target.Name;
		if (declaringType != typeof(P))
		{
			throw new ArgumentException("Parent type does not match delegate to be created");
		}
		DynamicMethod dynamicMethod = new DynamicMethod(name + "_Detour_Get", typeof(T), new Type[1] { typeof(P) }, restrictedSkipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		if (target.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Ldsfld, target);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldfld, target);
		}
		iLGenerator.Emit(OpCodes.Ret);
		DynamicMethod dynamicMethod2;
		if (target.IsInitOnly)
		{
			dynamicMethod2 = null;
		}
		else
		{
			dynamicMethod2 = new DynamicMethod(name + "_Detour_Set", null, new Type[2]
			{
				typeof(P),
				typeof(T)
			}, restrictedSkipVisibility: true);
			iLGenerator = dynamicMethod2.GetILGenerator();
			if (target.IsStatic)
			{
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Stsfld, target);
			}
			else
			{
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Stfld, target);
			}
			iLGenerator.Emit(OpCodes.Ret);
		}
		return new DetouredField<P, T>(name, dynamicMethod.CreateDelegate(typeof(Func<P, T>)) as Func<P, T>, dynamicMethod2?.CreateDelegate(typeof(Action<P, T>)) as Action<P, T>);
	}

	/// <summary>
	/// Creates dynamic detour methods to wrap a base game property with the specified name.
	/// </summary>
	/// <typeparam name="P">The type of the parent class.</typeparam>
	/// <typeparam name="T">The type of the property element.</typeparam>
	/// <param name="target">The property which will be accessed.</param>
	/// <returns>A detour element that wraps the property with a common interface matching
	/// that of a detoured field.</returns>
	/// <exception cref="T:PeterHan.PLib.Detours.DetourException">If the property has indexers.</exception>
	private static IDetouredField<P, T> DetourProperty<P, T>(PropertyInfo target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		Type declaringType = target.DeclaringType;
		string name = target.Name;
		if (declaringType != typeof(P))
		{
			throw new ArgumentException("Parent type does not match delegate to be created");
		}
		ParameterInfo[] indexParameters = target.GetIndexParameters();
		if (indexParameters != null && indexParameters.Length != 0)
		{
			throw new DetourException("Cannot detour on properties with index arguments");
		}
		MethodInfo getMethod = target.GetGetMethod(nonPublic: true);
		DynamicMethod dynamicMethod;
		if (target.CanRead && getMethod != null)
		{
			dynamicMethod = new DynamicMethod(name + "_Detour_Get", typeof(T), new Type[1] { typeof(P) }, restrictedSkipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			if (!getMethod.IsStatic)
			{
				iLGenerator.Emit(OpCodes.Ldarg_0);
			}
			iLGenerator.Emit(OpCodes.Call, getMethod);
			iLGenerator.Emit(OpCodes.Ret);
		}
		else
		{
			dynamicMethod = null;
		}
		MethodInfo setMethod = target.GetSetMethod(nonPublic: true);
		DynamicMethod dynamicMethod2;
		if (target.CanWrite && setMethod != null)
		{
			dynamicMethod2 = new DynamicMethod(name + "_Detour_Set", null, new Type[2]
			{
				typeof(P),
				typeof(T)
			}, restrictedSkipVisibility: true);
			ILGenerator iLGenerator2 = dynamicMethod2.GetILGenerator();
			if (!setMethod.IsStatic)
			{
				iLGenerator2.Emit(OpCodes.Ldarg_0);
			}
			iLGenerator2.Emit(OpCodes.Ldarg_1);
			iLGenerator2.Emit(OpCodes.Call, setMethod);
			iLGenerator2.Emit(OpCodes.Ret);
		}
		else
		{
			dynamicMethod2 = null;
		}
		return new DetouredField<P, T>(name, dynamicMethod?.CreateDelegate(typeof(Func<P, T>)) as Func<P, T>, dynamicMethod2?.CreateDelegate(typeof(Action<P, T>)) as Action<P, T>);
	}

	/// <summary>
	/// Creates dynamic detour methods to wrap a base game struct field with the specified
	/// name. For static struct fields, use the regular DetourField.
	/// </summary>
	/// <typeparam name="T">The type of the field element.</typeparam>
	/// <param name="parentType">The struct type which will be accessed.</param>
	/// <param name="name">The name of the struct field to be accessed.</param>
	/// <returns>A detour element that wraps the field with a common interface matching
	/// that of a detoured property.</returns>
	public static IDetouredField<object, T> DetourStructField<T>(this Type parentType, string name)
	{
		if (parentType == null)
		{
			throw new ArgumentNullException("parentType");
		}
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		FieldInfo field = parentType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field == null)
		{
			throw new DetourException("Unable to find {0} on type {1}".F(name, parentType.FullName));
		}
		DynamicMethod dynamicMethod = new DynamicMethod(name + "_Detour_Get", typeof(T), new Type[1] { typeof(object) }, restrictedSkipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Unbox, parentType);
		iLGenerator.Emit(OpCodes.Ldfld, field);
		iLGenerator.Emit(OpCodes.Ret);
		DynamicMethod dynamicMethod2;
		if (field.IsInitOnly)
		{
			dynamicMethod2 = null;
		}
		else
		{
			dynamicMethod2 = new DynamicMethod(name + "_Detour_Set", null, new Type[2]
			{
				typeof(object),
				typeof(T)
			}, restrictedSkipVisibility: true);
			ILGenerator iLGenerator2 = dynamicMethod2.GetILGenerator();
			iLGenerator2.Emit(OpCodes.Ldarg_0);
			iLGenerator2.Emit(OpCodes.Unbox, parentType);
			iLGenerator2.Emit(OpCodes.Ldarg_1);
			iLGenerator2.Emit(OpCodes.Stfld, field);
			iLGenerator2.Emit(OpCodes.Ret);
		}
		return new DetouredField<object, T>(name, dynamicMethod.CreateDelegate(typeof(Func<object, T>)) as Func<object, T>, dynamicMethod2?.CreateDelegate(typeof(Action<object, T>)) as Action<object, T>);
	}

	/// <summary>
	/// Generates the required method parameters for the dynamic detour method.
	/// </summary>
	/// <param name="caller">The method where the parameters will be defined.</param>
	/// <param name="actualParams">The actual parameters required.</param>
	/// <param name="expectedParams">The parameters provided.</param>
	/// <param name="offset">The offset to start loading (0 = static, 1 = instance).</param>
	private static void FinishDynamicMethod(DynamicMethod caller, ParameterInfo[] actualParams, Type[] expectedParams, int offset)
	{
		int num = expectedParams.Length;
		if (offset > 0)
		{
			caller.DefineParameter(1, ParameterAttributes.None, "this");
		}
		for (int i = offset; i < num; i++)
		{
			ParameterInfo parameterInfo = actualParams[i - offset];
			caller.DefineParameter(i + 1, parameterInfo.Attributes, parameterInfo.Name);
		}
	}

	/// <summary>
	/// Generates instructions to load arguments or default values onto the stack in a
	/// detour method.
	/// </summary>
	/// <param name="generator">The method where the calls will be added.</param>
	/// <param name="actualParams">The actual parameters required.</param>
	/// <param name="expectedParams">The parameters provided.</param>
	/// <param name="offset">The offset to start loading (0 = static, 1 = instance).</param>
	private static void LoadParameters(ILGenerator generator, ParameterInfo[] actualParams, Type[] expectedParams, int offset)
	{
		int num = expectedParams.Length;
		int num2 = actualParams.Length + offset;
		if (num > 0)
		{
			generator.Emit(OpCodes.Ldarg_0);
		}
		if (num > 1)
		{
			generator.Emit(OpCodes.Ldarg_1);
		}
		if (num > 2)
		{
			generator.Emit(OpCodes.Ldarg_2);
		}
		if (num > 3)
		{
			generator.Emit(OpCodes.Ldarg_3);
		}
		for (int i = 4; i < num; i++)
		{
			generator.Emit(OpCodes.Ldarg_S, i);
		}
		for (int j = num; j < num2; j++)
		{
			ParameterInfo parameterInfo = actualParams[j - offset];
			PTranspilerTools.GenerateDefaultLoad(generator, parameterInfo.ParameterType, parameterInfo.DefaultValue);
		}
	}

	/// <summary>
	/// Creates a dynamic detour method of the specified delegate type to wrap a base game
	/// method with the specified name. The dynamic method will automatically adapt if
	/// optional parameters are added, filling in their default values.
	/// </summary>
	/// <typeparam name="D">The delegate type to be used to call the detour.</typeparam>
	/// <param name="type">The target type.</param>
	/// <param name="name">The method name.</param>
	/// <returns>The detour that will call that method.</returns>
	/// <exception cref="T:PeterHan.PLib.Detours.DetourException">If the delegate does not match any valid target method.</exception>
	public static D TryDetour<D>(this Type type, string name) where D : Delegate
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		try
		{
			return type.Detour<D>(name);
		}
		catch (DetourException)
		{
			return null;
		}
	}

	/// <summary>
	/// Creates a dynamic detour method of the specified delegate type to wrap a base game
	/// constructor. The dynamic method will automatically adapt if optional parameters
	/// are added, filling in their default values.
	/// </summary>
	/// <typeparam name="D">The delegate type to be used to call the detour.</typeparam>
	/// <param name="type">The target type.</param>
	/// <returns>The detour that will call that type's constructor, or null if the
	/// delegate does not match any valid target method.</returns>
	public static D TryDetourConstructor<D>(this Type type) where D : Delegate
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		try
		{
			return type.DetourConstructor<D>();
		}
		catch (DetourException)
		{
			return null;
		}
	}

	/// <summary>
	/// Creates a dynamic detour method of the specified delegate type to wrap a base game
	/// method with the specified name. The dynamic method will automatically adapt if
	/// optional parameters are added, filling in their default values.
	/// </summary>
	/// <typeparam name="D">The delegate type to be used to call the detour.</typeparam>
	/// <param name="target">The target method to be called.</param>
	/// <returns>The detour that will call that method, or null if the delegate does
	/// not match the target.</returns>
	public static D TryDetour<D>(this MethodInfo target) where D : Delegate
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		try
		{
			return target.Detour<D>();
		}
		catch (DetourException)
		{
			return null;
		}
	}

	/// <summary>
	/// Creates a dynamic detour method of the specified delegate type to wrap a base game
	/// constructor. The dynamic method will automatically adapt if optional parameters
	/// are added, filling in their default values.
	/// </summary>
	/// <typeparam name="D">The delegate type to be used to call the detour.</typeparam>
	/// <param name="target">The target constructor to be called.</param>
	/// <returns>The detour that will call that constructor, or null if the delegate does
	/// not match the target.</returns>
	public static D TryDetour<D>(this ConstructorInfo target) where D : Delegate
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		try
		{
			return target.Detour<D>();
		}
		catch (DetourException)
		{
			return null;
		}
	}

	/// <summary>
	/// Creates dynamic detour methods to wrap a base game field or property with the
	/// specified name. The detour will still work even if the field is converted to a
	/// source compatible property and vice versa.
	/// </summary>
	/// <typeparam name="P">The type of the parent class.</typeparam>
	/// <typeparam name="T">The type of the field or property element.</typeparam>
	/// <param name="name">The name of the field or property to be accessed.</param>
	/// <returns>A detour element that wraps the field or property with common getter and
	/// setter delegates, or null if the field cannot be found.</returns>
	public static IDetouredField<P, T> TryDetourField<P, T>(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		try
		{
			return DetourField<P, T>(name);
		}
		catch (DetourException)
		{
			return null;
		}
	}

	/// <summary>
	/// Creates dynamic detour methods to wrap a base game struct field with the specified
	/// name. For static struct fields, use the regular DetourField.
	/// </summary>
	/// <typeparam name="T">The type of the field element.</typeparam>
	/// <param name="parentType">The struct type which will be accessed.</param>
	/// <param name="name">The name of the struct field to be accessed.</param>
	/// <returns>A detour element that wraps the field, or null if the field cannot be found.</returns>
	public static IDetouredField<object, T> TryDetourStructField<T>(this Type parentType, string name)
	{
		if (parentType == null)
		{
			throw new ArgumentNullException("parentType");
		}
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		try
		{
			return parentType.DetourStructField<T>(name);
		}
		catch (DetourException)
		{
			return null;
		}
	}

	/// <summary>
	/// Verifies that the delegate signature provided in dst can be dynamically mapped to
	/// the method provided by src, with the possible addition of optional parameters set
	/// to their default values.
	/// </summary>
	/// <param name="expected">The method return type and parameter types expected.</param>
	/// <param name="actual">The method to be called.</param>
	/// <param name="actualReturn">The type of the method or constructor's return value.</param>
	/// <returns>The parameters used in the call to the actual method.</returns>
	/// <exception cref="T:PeterHan.PLib.Detours.DetourException">If the delegate does not match the target.</exception>
	private static ParameterInfo[] ValidateDelegate(DelegateInfo expected, MethodBase actual, Type actualReturn)
	{
		Type[] parameterTypes = expected.parameterTypes;
		Type returnType = expected.returnType;
		if (!returnType.IsAssignableFrom(actualReturn))
		{
			throw new DetourException("Return type {0} cannot be converted to type {1}".F(actualReturn.FullName, returnType.FullName));
		}
		Type declaringType = actual.DeclaringType;
		if (declaringType == null)
		{
			throw new ArgumentException("Method is not declared by an actual type");
		}
		if (declaringType.ContainsGenericParameters)
		{
			throw new DetourException("Method parent type {0} must have all generic parameters defined".F(declaringType.FullName));
		}
		string text = declaringType.FullName + "." + actual.Name;
		ParameterInfo[] parameters = actual.GetParameters();
		int num = parameters.Length;
		int num2 = parameterTypes.Length;
		Type[] array = new Type[num];
		bool flag = actual.IsStatic || actual.IsConstructor;
		for (int i = 0; i < num; i++)
		{
			array[i] = parameters[i].ParameterType;
		}
		Type[] array2;
		if (flag)
		{
			array2 = array;
		}
		else
		{
			array2 = PTranspilerTools.PushDeclaringType(array, declaringType);
			num++;
		}
		if (num2 > num)
		{
			throw new DetourException("Method {0} has only {1:D} parameters, but {2:D} were supplied".F(actual.ToString(), num, num2));
		}
		for (int j = 0; j < num2; j++)
		{
			Type type = array2[j];
			Type type2 = parameterTypes[j];
			if (!type.IsAssignableFrom(type2))
			{
				throw new DetourException("Argument {0:D} for method {3} cannot be converted from {1} to {2}".F(j, type.FullName, type2.FullName, text));
			}
		}
		int num3 = ((!flag) ? 1 : 0);
		for (int k = num2; k < num; k++)
		{
			ParameterInfo parameterInfo = parameters[k - num3];
			if (!parameterInfo.IsOptional)
			{
				throw new DetourException("New argument {0:D} for method {1} ({2}) is not optional".F(k, text, parameterInfo.ParameterType.FullName));
			}
		}
		return parameters;
	}
}
