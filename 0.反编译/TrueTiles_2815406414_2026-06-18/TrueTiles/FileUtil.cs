using System;
using System.IO;
using FUtility;

namespace TrueTiles;

public class FileUtil
{
	public static bool TryReadFile(string path, out string result)
	{
		try
		{
			result = File.ReadAllText(path);
			return result != null;
		}
		catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
		{
			Log.Warning("Tried to read file at " + path + ", but could not be read: ", ex.Message);
			result = null;
			return false;
		}
	}

	public static string GetOrCreateDirectory(string path)
	{
		try
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			return path;
		}
		catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
		{
			Log.Warning("Could not create directory: " + ex.Message);
			return null;
		}
	}
}
