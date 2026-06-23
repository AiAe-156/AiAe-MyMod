using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.PatchManager;

/// <summary>
/// Manages patches that PLib will conditionally apply.
/// </summary>
public sealed class PPatchManager : PForwardedComponent
{
	/// <summary>
	/// The base flags to use when matching instance or static methods.
	/// </summary>
	internal const BindingFlags FLAGS = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;

	/// <summary>
	/// The flags to use when matching instance and static methods.
	/// </summary>
	internal const BindingFlags FLAGS_EITHER = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	/// <summary>
	/// The version of this component. Uses the running PLib version.
	/// </summary>
	internal static readonly Version VERSION = new Version("4.19.0.0");

	/// <summary>
	/// true if the AfterModsLoad patches have been run, or false otherwise.
	/// </summary>
	private static volatile bool afterModsLoaded = false;

	/// <summary>
	/// The Harmony instance to use for patching.
	/// </summary>
	private readonly Harmony harmony;

	/// <summary>
	/// Patches and delegates to be run at specific points in the runtime. Put the kibosh
	/// on patching Db.Initialize()!
	/// </summary>
	private readonly IDictionary<uint, ICollection<IPatchMethodInstance>> patches;

	/// <summary>
	/// The instantiated copy of this class.
	/// </summary>
	internal static PPatchManager Instance { get; private set; }

	public override Version Version => VERSION;

	private static void Game_DestroyInstances_Postfix()
	{
		Instance?.InvokeAllProcess(6u, null);
	}

	private static void Game_OnPrefabInit_Postfix()
	{
		Instance?.InvokeAllProcess(5u, null);
	}

	private static void Initialize_Prefix()
	{
		Instance?.InvokeAllProcess(2u, null);
	}

	private static void Initialize_Postfix()
	{
		Instance?.InvokeAllProcess(3u, null);
	}

	private static void PostProcess_Prefix()
	{
		Instance?.InvokeAllProcess(8u, null);
	}

	private static void PostProcess_Postfix()
	{
		Instance?.InvokeAllProcess(9u, null);
	}

	private static void Instance_Postfix()
	{
		bool flag = false;
		if (Instance != null)
		{
			lock (VERSION)
			{
				if (!afterModsLoaded)
				{
					flag = (afterModsLoaded = true);
				}
			}
		}
		if (flag)
		{
			Instance.InvokeAllProcess(7u, null);
		}
	}

	private static void MainMenu_OnSpawn_Postfix()
	{
		Instance?.InvokeAllProcess(4u, null);
	}

	private static void DetailsScreen_OnPrefabInit_Postfix()
	{
		Instance?.InvokeAllProcess(10u, null);
	}

	/// <summary>
	/// Checks to see if the conditions for a method running are met.
	/// </summary>
	/// <param name="assemblyName">The assembly name that must be present, or null if none is required.</param>
	/// <param name="typeName">The type full name that must be present, or null if none is required.</param>
	/// <param name="requiredType">The type that was required, if typeName was not null or empty.</param>
	/// <returns>true if the requirements are met, or false otherwise.</returns>
	internal static bool CheckConditions(string assemblyName, string typeName, out Type requiredType)
	{
		bool result = false;
		bool flag = string.IsNullOrEmpty(typeName);
		if (string.IsNullOrEmpty(assemblyName))
		{
			if (flag)
			{
				requiredType = null;
				result = true;
			}
			else
			{
				requiredType = PPatchTools.GetTypeSafe(typeName);
				result = requiredType != null;
			}
		}
		else if (flag)
		{
			requiredType = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				if (assemblies[i].GetName().Name == assemblyName)
				{
					result = true;
					break;
				}
			}
		}
		else
		{
			requiredType = PPatchTools.GetTypeSafe(typeName, assemblyName);
			result = requiredType != null;
		}
		return result;
	}

	/// <summary>
	/// Creates a patch manager to execute patches at specific times.
	///
	/// Create this instance in OnLoad() and use RegisterPatchClass to register a
	/// patch class.
	/// </summary>
	/// <param name="harmony">The Harmony instance to use for patching.</param>
	public PPatchManager(Harmony harmony)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (harmony == null)
		{
			PUtil.LogWarning("Use the Harmony instance from OnLoad to create PPatchManager");
			harmony = new Harmony("PLib.PostLoad." + Assembly.GetExecutingAssembly().GetNameSafe());
		}
		this.harmony = harmony;
		patches = new Dictionary<uint, ICollection<IPatchMethodInstance>>(11);
		InstanceData = patches;
	}

	/// <summary>
	/// Schedules a patch method instance to be run.
	/// </summary>
	/// <param name="when">When to run the patch.</param>
	/// <param name="instance">The patch method instance to run.</param>
	private void AddHandler(uint when, IPatchMethodInstance instance)
	{
		if (!patches.TryGetValue(when, out var value))
		{
			patches.Add(when, value = new List<IPatchMethodInstance>(16));
		}
		value.Add(instance);
	}

	public override void Initialize(Harmony plibInstance)
	{
		Instance = this;
		plibInstance.Patch(typeof(Db), "Initialize", PatchMethod("Initialize_Prefix"), PatchMethod("Initialize_Postfix"));
		plibInstance.Patch(typeof(Db), "PostProcess", PatchMethod("PostProcess_Prefix"), PatchMethod("PostProcess_Postfix"));
		plibInstance.Patch(typeof(Game), "DestroyInstances", null, PatchMethod("Game_DestroyInstances_Postfix"));
		plibInstance.Patch(typeof(Game), "OnPrefabInit", null, PatchMethod("Game_OnPrefabInit_Postfix"));
		plibInstance.Patch(typeof(GlobalResources), "Instance", null, PatchMethod("Instance_Postfix"));
		plibInstance.Patch(typeof(MainMenu), "OnSpawn", null, PatchMethod("MainMenu_OnSpawn_Postfix"));
		plibInstance.Patch(typeof(DetailsScreen), "OnPrefabInit", null, PatchMethod("DetailsScreen_OnPrefabInit_Postfix"));
	}

	public override void PostInitialize(Harmony plibInstance)
	{
		InvokeAllProcess(1u, null);
	}

	public override void Process(uint when, object _)
	{
		if (!patches.TryGetValue(when, out var value) || value == null || value.Count <= 0)
		{
			return;
		}
		string text = RunAt.ToString(when);
		foreach (IPatchMethodInstance item in value)
		{
			try
			{
				item.Run(harmony);
			}
			catch (TargetInvocationException ex)
			{
				PUtil.LogError("Error running patches for stage " + text + ":");
				PUtil.LogException(ex.GetBaseException());
			}
			catch (Exception thrown)
			{
				PUtil.LogError("Error running patches for stage " + text + ":");
				PUtil.LogException(thrown);
			}
		}
	}

	/// <summary>
	/// Registers a single patch to be run by Patch Manager. Obviously, the patch must be
	/// registered before the time that it is used.
	/// </summary>
	/// <param name="when">The time when the method should be run.</param>
	/// <param name="patch">The patch to execute.</param>
	public void RegisterPatch(uint when, IPatchMethodInstance patch)
	{
		RegisterForForwarding();
		if (patch == null)
		{
			throw new ArgumentNullException("patch");
		}
		if (when == 0)
		{
			patch.Run(harmony);
		}
		else
		{
			AddHandler(when, patch);
		}
	}

	/// <summary>
	/// Registers a class containing methods for [PLibPatch] and [PLibMethod] handlers.
	/// All methods, public and private, of the type will be searched for annotations.
	/// However, nested and derived types will not be searched, nor will inherited methods.
	///
	/// This method cannot be used to register a class from another mod, as the annotations
	/// on those methods would have a different assembly qualified name and would thus
	/// not be recognized.
	/// </summary>
	/// <param name="type">The type to register.</param>
	public void RegisterPatchClass(Type type)
	{
		int num = 0;
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		RegisterForForwarding();
		MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			object[] customAttributes = methodInfo.GetCustomAttributes(inherit: true);
			for (int j = 0; j < customAttributes.Length; j++)
			{
				if (customAttributes[j] is IPLibAnnotation iPLibAnnotation)
				{
					uint runtime = iPLibAnnotation.Runtime;
					IPatchMethodInstance patchMethodInstance = iPLibAnnotation.CreateInstance(methodInfo);
					if (runtime == 0)
					{
						patchMethodInstance.Run(harmony);
					}
					else
					{
						AddHandler(iPLibAnnotation.Runtime, patchMethodInstance);
					}
					num++;
				}
			}
		}
		if (num > 0)
		{
			PRegistry.LogPatchDebug("Registered {0:D} handler(s) for {1}".F(num, Assembly.GetCallingAssembly().GetNameSafe() ?? "?"));
		}
		else
		{
			PRegistry.LogPatchWarning("RegisterPatchClass could not find any handlers!");
		}
	}
}
