using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using KMod;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace UtilLibs;

public static class IO_Utils
{
	public sealed class Vector2IConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Vector2I);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			Vector2I val = (Vector2I)value;
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(((Vector2I)(ref val)).X);
			writer.WritePropertyName("y");
			writer.WriteValue(((Vector2I)(ref val)).Y);
			writer.WriteEndObject();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			JObject val = JObject.Load(reader);
			return (object)new Vector2I(Extensions.Value<int>((IEnumerable<JToken>)val["x"]), Extensions.Value<int>((IEnumerable<JToken>)val["y"]));
		}
	}

	private static string _modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

	public static string ModID => Assembly.GetExecutingAssembly().GetName().Name;

	public static string ModPath => _modPath;

	public static string ModsFolder => Manager.GetDirectory();

	public static string ConfigsFolder => Path.Combine(ModsFolder, "config");

	public static void OverrideModPath(string newPath)
	{
		_modPath = newPath;
	}

	public static void PutToClipboard(string toPut)
	{
		Type type = Type.GetType("UnityEngine.TextEditor, UnityEngine");
		if (type != null)
		{
			object obj = Activator.CreateInstance(type);
			Traverse val = Traverse.Create(obj);
			val.Property("text", (object[])null).SetValue((object)toPut);
			val.Method("SelectAll", Array.Empty<object>()).GetValue();
			val.Method("Copy", Array.Empty<object>()).GetValue();
		}
	}

	public static bool TryGetStringFromClipboard(out string clipboardText)
	{
		clipboardText = string.Empty;
		Type type = Type.GetType("UnityEngine.TextEditor, UnityEngine");
		if (type != null)
		{
			object obj = Activator.CreateInstance(type);
			Traverse val = Traverse.Create(obj);
			val.Property("text", (object[])null).SetValue((object)string.Empty);
			val.Method("Paste", Array.Empty<object>()).GetValue();
			clipboardText = (string)val.Property("text", (object[])null).GetValue();
		}
		return !Util.IsNullOrWhiteSpace(clipboardText);
	}

	public static bool ReadFromFile<T>(FileInfo filePath, out T output, string forceExtensionTo = "", JsonSerializerSettings converterSettings = null)
	{
		try
		{
			if (!filePath.Exists || (forceExtensionTo != string.Empty && filePath.Extension != forceExtensionTo && !filePath.Name.StartsWith("._")))
			{
				SgtLogger.logwarning(filePath.FullName, "File does not exist!");
				output = default(T);
				return false;
			}
			FileStream stream = filePath.OpenRead();
			using StreamReader streamReader = new StreamReader(stream);
			string text = streamReader.ReadToEnd();
			output = JsonConvert.DeserializeObject<T>(text, converterSettings);
			return true;
		}
		catch (Exception ex)
		{
			SgtLogger.warning("failed reading " + filePath.FullName + ":\n\n" + ex.Message);
			output = default(T);
			return false;
		}
	}

	public static bool ReadFromFile<T>(string FileOrigin, out T output, string forceExtensionTo = "", JsonSerializerSettings converterSettings = null)
	{
		return ReadFromFile<T>(new FileInfo(FileOrigin), out output, forceExtensionTo, converterSettings);
	}

	public static bool WriteToFile<T>(T DataObject, string filePath, JsonSerializerSettings converterSettings = null)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(filePath);
			FileStream stream = fileInfo.Open(FileMode.Create);
			string value = JsonConvert.SerializeObject((object)DataObject, (Formatting)1, converterSettings);
			using (StreamWriter streamWriter = new StreamWriter(stream))
			{
				streamWriter.Write(value);
			}
			return true;
		}
		catch (Exception ex)
		{
			SgtLogger.logError("Could not write file, Exception: " + ex);
			return false;
		}
	}

	public static bool DeleteFile(string filePath)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(filePath);
			fileInfo.Delete();
			return true;
		}
		catch (Exception ex)
		{
			SgtLogger.logError("Could not delete file, Exception: " + ex);
			return false;
		}
	}

	public static void DumpToFile(object data, string path, bool useCustomConverter = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		JsonSerializerSettings val = new JsonSerializerSettings
		{
			ReferenceLoopHandling = (ReferenceLoopHandling)1,
			TypeNameHandling = (TypeNameHandling)1,
			Formatting = (Formatting)1,
			ConstructorHandling = (ConstructorHandling)1,
			ContractResolver = (IContractResolver)(object)new InjectionMethods.IncludePrivateContractResolver(),
			Converters = new List<JsonConverter>(1) { (JsonConverter)(object)new Vector2IConverter() }
		};
		WriteToFile(data, path, useCustomConverter ? val : null);
	}
}
