using System;
using System.Reflection;
using HarmonyLib;

namespace PeterHan.PLib.Core;

/// <summary>
/// Delegates calls to forwarded components in other assemblies.
/// </summary>
internal sealed class PRemoteComponent : PForwardedComponent
{
	/// <summary>
	/// The prototype used for delegates to remote Initialize.
	/// </summary>
	private delegate void InitializeDelegate(Harmony instance);

	/// <summary>
	/// The prototype used for delegates to remote Process.
	/// </summary>
	private delegate void ProcessDelegate(uint operation, object args);

	/// <summary>
	/// Points to the component's version of Bootstrap.
	/// </summary>
	private readonly InitializeDelegate doBootstrap;

	/// <summary>
	/// Points to the component's version of Initialize.
	/// </summary>
	private readonly InitializeDelegate doInitialize;

	/// <summary>
	/// Points to the component's version of PostInitialize.
	/// </summary>
	private readonly InitializeDelegate doPostInitialize;

	/// <summary>
	/// Gets the component's data.
	/// </summary>
	private readonly Func<object> getData;

	/// <summary>
	/// Runs the processing method of the component.
	/// </summary>
	private readonly ProcessDelegate process;

	/// <summary>
	/// Sets the component's data.
	/// </summary>
	private readonly Action<object> setData;

	/// <summary>
	/// The component's version.
	/// </summary>
	private readonly Version version;

	/// <summary>
	/// The wrapped instance from the other mod.
	/// </summary>
	private readonly object wrapped;

	protected override object InstanceData
	{
		get
		{
			return getData?.Invoke();
		}
		set
		{
			setData?.Invoke(value);
		}
	}

	public override Version Version => version;

	internal PRemoteComponent(object wrapped)
	{
		this.wrapped = wrapped ?? throw new ArgumentNullException("wrapped");
		if (!PPatchTools.TryGetPropertyValue<Version>(wrapped, "Version", out var value))
		{
			throw new ArgumentException("Remote component missing Version property");
		}
		version = value;
		Type type = wrapped.GetType();
		doInitialize = type.CreateDelegate<InitializeDelegate>("Initialize", wrapped, new Type[1] { typeof(Harmony) });
		if (doInitialize == null)
		{
			throw new ArgumentException("Remote component missing Initialize");
		}
		doBootstrap = type.CreateDelegate<InitializeDelegate>("Bootstrap", wrapped, new Type[1] { typeof(Harmony) });
		doPostInitialize = type.CreateDelegate<InitializeDelegate>("PostInitialize", wrapped, new Type[1] { typeof(Harmony) });
		getData = type.CreateGetDelegate<object>("InstanceData", wrapped);
		setData = type.CreateSetDelegate<object>("InstanceData", wrapped);
		process = type.CreateDelegate<ProcessDelegate>("Process", wrapped, new Type[2]
		{
			typeof(uint),
			typeof(object)
		});
	}

	public override void Bootstrap(Harmony plibInstance)
	{
		doBootstrap?.Invoke(plibInstance);
	}

	internal override object DoInitialize(Harmony plibInstance)
	{
		doInitialize(plibInstance);
		return wrapped;
	}

	public override Assembly GetOwningAssembly()
	{
		return wrapped.GetType().Assembly;
	}

	public override void Initialize(Harmony plibInstance)
	{
		DoInitialize(plibInstance);
	}

	public override void PostInitialize(Harmony plibInstance)
	{
		doPostInitialize?.Invoke(plibInstance);
	}

	public override void Process(uint operation, object args)
	{
		process?.Invoke(operation, args);
	}

	public override string ToString()
	{
		return "PRemoteComponent[ID={0},TargetType={1}]".F(base.ID, wrapped.GetType().AssemblyQualifiedName);
	}
}
