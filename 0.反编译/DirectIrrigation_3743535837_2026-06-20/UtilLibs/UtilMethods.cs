using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UtilLibs;

public static class UtilMethods
{
	public static string ModPath => IO_Utils.ModPath;

	public static void ListAllTypesWithAssemblies()
	{
		IEnumerable<Type> source = AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly t) => t.GetTypes());
		source.ToList().ForEach(delegate(Type t)
		{
			SgtLogger.l(t.Name + ", AQN: " + t.AssemblyQualifiedName, t.Namespace);
		});
	}

	public static void ListAllPropertyValues(object s, Func<string, bool> exclude = null)
	{
		SgtLogger.l("Listing all properties of: " + s.ToString());
		foreach (PropertyInfo item in from p in s.GetType().GetProperties()
			where !p.GetGetMethod().GetParameters().Any()
			select p)
		{
			if (exclude == null || !exclude(item.ToString()))
			{
				Console.WriteLine(item?.ToString() + ": " + item.GetValue(s, null));
			}
		}
	}

	public static void ListAllFieldValues(object s)
	{
		SgtLogger.l("Listing all fields of: " + s.ToString());
		FieldInfo[] fields = s.GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			Console.WriteLine(fieldInfo?.ToString() + ": " + fieldInfo.GetValue(s));
		}
	}

	public static void ListAllComponents(GameObject s)
	{
		SgtLogger.l("Listing all Components of: " + ((object)s).ToString());
		Component[] components = s.GetComponents(typeof(Object));
		foreach (Component val in components)
		{
			if ((Object)(object)val != (Object)null)
			{
				Console.WriteLine("Type: " + ((object)val).GetType().ToString() + ", Name ->" + ((Object)val).name);
			}
		}
	}

	public static float GetCFromKelvin(float degreeK)
	{
		return degreeK - 273.15f;
	}

	public static float GetKelvinFromC(float degreeC)
	{
		return degreeC + 273.15f;
	}

	public static bool IsCellInSpaceAndVacuum(int _cell, int root)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		if (!Grid.IsValidCell(_cell) || !Grid.AreCellsInSameWorld(_cell, root))
		{
			return true;
		}
		return (Grid.IsCellOpenToSpace(_cell) || IsCellInRocket(_cell)) && ((int)Grid.Element[_cell].id == 758759285 || (int)Grid.Element[_cell].id == 1838482828);
	}

	private static bool IsCellInRocket(int _cell)
	{
		WorldContainer val = ((Grid.IsValidCell(_cell) && Grid.WorldIdx[_cell] != byte.MaxValue) ? ClusterManager.Instance.GetWorld((int)Grid.WorldIdx[_cell]) : null);
		return val.IsModuleInterior;
	}
}
