using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PeterHan.PLib.Lighting;

/// <summary>
/// Arguments which are passed to lighting callbacks to perform lighting calculations.
///
/// The range is the light radius supplied during the Light2D creation; do not light up
/// tiles outside of this radius (measured by a square around SourceCell)!
///
/// The source cell is the cell nearest to where the Light2D is currently located.
///
/// Use the IDictionary interface to store the relative brightness of cells by their cell
/// location. These values should be between 0 and 1 normally, with the maximum brightness
/// being set by the intensity parameter of the Light2D. The user is responsible for
/// ensuring that cells are valid before lighting them up.
/// </summary>
public sealed class LightingArgs : EventArgs, IDictionary<int, float>, ICollection<KeyValuePair<int, float>>, IEnumerable<KeyValuePair<int, float>>, IEnumerable
{
	/// <summary>
	/// The location where lighting results are stored.
	/// </summary>
	public IDictionary<int, float> Brightness { get; }

	/// <summary>
	/// The maximum range to use for cell lighting. Do not light up cells beyond this
	/// range from SourceCell.
	/// </summary>
	public int Range { get; }

	/// <summary>
	/// The source of the light.
	/// </summary>
	public GameObject Source { get; }

	/// <summary>
	/// The originating cell. Actual lighting can begin elsewhere, but the range limit is
	/// measured from this cell.
	/// </summary>
	public int SourceCell { get; }

	public ICollection<int> Keys => Brightness.Keys;

	public ICollection<float> Values => Brightness.Values;

	public int Count => Brightness.Count;

	public bool IsReadOnly => Brightness.IsReadOnly;

	public float this[int key]
	{
		get
		{
			return Brightness[key];
		}
		set
		{
			Brightness[key] = value;
		}
	}

	internal LightingArgs(GameObject source, int cell, int range, IDictionary<int, float> output)
	{
		if ((Object)(object)source == (Object)null)
		{
			throw new ArgumentNullException("source");
		}
		Brightness = output ?? throw new ArgumentNullException("output");
		Range = range;
		Source = source;
		SourceCell = cell;
	}

	public bool ContainsKey(int key)
	{
		return Brightness.ContainsKey(key);
	}

	public void Add(int key, float value)
	{
		Brightness.Add(key, value);
	}

	public bool Remove(int key)
	{
		return Brightness.Remove(key);
	}

	public bool TryGetValue(int key, out float value)
	{
		return Brightness.TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<int, float> item)
	{
		Brightness.Add(item);
	}

	public void Clear()
	{
		Brightness.Clear();
	}

	public bool Contains(KeyValuePair<int, float> item)
	{
		return Brightness.Contains(item);
	}

	public void CopyTo(KeyValuePair<int, float>[] array, int arrayIndex)
	{
		Brightness.CopyTo(array, arrayIndex);
	}

	public bool Remove(KeyValuePair<int, float> item)
	{
		return Brightness.Remove(item);
	}

	public IEnumerator<KeyValuePair<int, float>> GetEnumerator()
	{
		return Brightness.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Brightness.GetEnumerator();
	}

	public override string ToString()
	{
		return $"LightingArgs[source={SourceCell:D},range={Range:D}]";
	}
}
