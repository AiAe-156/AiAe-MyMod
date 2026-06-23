using System;
using System.Collections.Generic;
using KMod;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.AVC;

/// <summary>
/// Represents a "task" to check a particular mod for updates.
/// </summary>
internal sealed class VersionCheckTask
{
	/// <summary>
	/// The method which will be used to check.
	/// </summary>
	private readonly IModVersionChecker method;

	/// <summary>
	/// The mod whose version will be checked.
	/// </summary>
	private readonly Mod mod;

	/// <summary>
	/// The location where the outcome of mod version checking will be stored.
	/// </summary>
	private readonly ICollection<ModVersionCheckResults> results;

	/// <summary>
	/// The next task to run when the check completes, or null to not run any task.
	/// </summary>
	internal Action Next { get; set; }

	internal VersionCheckTask(Mod mod, IModVersionChecker method, ICollection<ModVersionCheckResults> results)
	{
		this.mod = mod ?? throw new ArgumentNullException("mod");
		this.method = method ?? throw new ArgumentNullException("method");
		this.results = results ?? throw new ArgumentNullException("results");
		Next = null;
	}

	/// <summary>
	/// Records the result of the mod version check, and runs the next checker in
	/// line, from this mod or a different one.
	/// </summary>
	/// <param name="result">The results from the version check.</param>
	private void OnComplete(ModVersionCheckResults result)
	{
		method.OnVersionCheckCompleted -= OnComplete;
		if (result != null)
		{
			results.Add(result);
			if (!result.IsUpToDate)
			{
				PUtil.LogWarning("Mod {0} is out of date! New version: {1}".F(result.ModChecked, result.NewVersion ?? "unknown"));
			}
		}
		RunNext();
	}

	/// <summary>
	/// Runs the version check, and registers a callback to run the next one if
	/// it is not null.
	/// </summary>
	internal void Run()
	{
		bool flag = false;
		foreach (ModVersionCheckResults result in results)
		{
			if (result.ModChecked == mod.staticID)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			method.OnVersionCheckCompleted += OnComplete;
			bool flag2;
			try
			{
				flag2 = method.CheckVersion(mod);
			}
			catch (Exception thrown)
			{
				PUtil.LogWarning("Unable to check version for mod " + mod.label.title + ":");
				PUtil.LogExcWarn(thrown);
				flag2 = false;
			}
			if (!flag2)
			{
				method.OnVersionCheckCompleted -= OnComplete;
				RunNext();
			}
		}
	}

	/// <summary>
	/// Runs the next version check.
	/// </summary>
	private void RunNext()
	{
		Next?.Invoke();
	}
}
