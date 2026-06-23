using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeterHan.PLib.Lighting;

/// <summary>
/// Represents a light shape which can be used by mods.
/// </summary>
internal sealed class PLightShape : ILightShape
{
	/// <summary>
	/// The handler for this light shape.
	/// </summary>
	private readonly PLightManager.CastLightDelegate handler;

	public string Identifier { get; }

	public LightShape KleiLightShape => (LightShape)(ShapeID + 1);

	public LightShape RayMode { get; }

	/// <summary>
	/// The light shape ID.
	/// </summary>
	internal int ShapeID { get; }

	internal PLightShape(int id, string identifier, PLightManager.CastLightDelegate handler, LightShape rayMode)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		this.handler = handler ?? throw new ArgumentNullException("handler");
		Identifier = identifier ?? throw new ArgumentNullException("identifier");
		RayMode = rayMode;
		ShapeID = id;
	}

	public override bool Equals(object obj)
	{
		if (obj is PLightShape pLightShape)
		{
			return pLightShape.Identifier == Identifier;
		}
		return false;
	}

	/// <summary>
	/// Invokes the light handler with the provided light information.
	/// </summary>
	/// <param name="source">The source of the light.</param>
	/// <param name="cell">The origin cell.</param>
	/// <param name="range">The range to fill.</param>
	/// <param name="brightness">The location where lit points will be stored.</param>
	internal void DoFillLight(GameObject source, int cell, int range, IDictionary<int, float> brightness)
	{
		handler(new LightingArgs(source, cell, range, brightness));
	}

	public void FillLight(LightingArgs args)
	{
		handler(args);
	}

	public override int GetHashCode()
	{
		return Identifier.GetHashCode();
	}

	public override string ToString()
	{
		return "PLightShape[ID=" + Identifier + "]";
	}
}
