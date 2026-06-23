using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Klei;

namespace FUtility.FLocalization;

public class TemplateGenerator
{
	private class TextInfo
	{
		public string text;

		public string translatorsNote;
	}

	public static void GenerateStringsTemplate(Type locStringTreeRoot, string outputFolder)
	{
		outputFolder = FileSystem.Normalize(outputFolder);
		if (FileUtil.CreateDirectory(outputFolder, 5))
		{
			GenerateStringsTemplate(locStringTreeRoot.Namespace, Assembly.GetAssembly(locStringTreeRoot), FileSystem.Normalize(Path.Combine(outputFolder, locStringTreeRoot.Namespace.ToLower() + "_template.pot")), null);
		}
	}

	private static void GenerateStringsTemplate(string locStringsNamespace, Assembly assembly, string outputFilename, Dictionary<string, object> runtimeForest)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		MethodInfo method = typeof(Localization).GetMethod("CollectLocStringTreeRoots", BindingFlags.Static | BindingFlags.NonPublic);
		if (method == null)
		{
			Log.Warning("Could not find method Localization.CollectLocStringTreeRoots.");
			return;
		}
		foreach (Type item in method.Invoke(null, new object[2] { locStringsNamespace, assembly }) as IEnumerable<Type>)
		{
			Dictionary<string, object> dictionary2 = MakeRuntimeLocStringTree(item);
			if (dictionary2.Count > 0)
			{
				dictionary[item.Name] = dictionary2;
			}
		}
		if (runtimeForest != null)
		{
			dictionary.Concat(runtimeForest);
		}
		using (StreamWriter streamWriter = new StreamWriter(outputFilename, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
		{
			streamWriter.WriteLine("msgid \"\"");
			streamWriter.WriteLine("msgstr \"\"");
			streamWriter.WriteLine("\"Application: Oxygen Not Included\"");
			streamWriter.WriteLine("\"Generated with FUtility\"");
			streamWriter.WriteLine("\"POT Version: 2.0\"");
			WriteStringsTemplate(locStringsNamespace, streamWriter, dictionary);
		}
		Log.Info("Generated " + outputFilename);
	}

	private static Dictionary<string, object> MakeRuntimeLocStringTree(Type locStringTreeRoot)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		FieldInfo[] fields = locStringTreeRoot.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.FieldType != typeof(LocString))
			{
				continue;
			}
			if (!fieldInfo.IsStatic)
			{
				DebugUtil.DevLogError("LocString fields must be static, skipping. " + fieldInfo.Name);
				continue;
			}
			LocString val = (LocString)fieldInfo.GetValue(null);
			if (val == null)
			{
				Debug.LogError((object)("Tried to generate LocString for " + fieldInfo.Name + " but it is null so skipping"));
				continue;
			}
			dictionary[fieldInfo.Name] = new TextInfo
			{
				text = val.text,
				translatorsNote = GetNote(fieldInfo)
			};
		}
		Type[] nestedTypes = locStringTreeRoot.GetNestedTypes();
		foreach (Type type in nestedTypes)
		{
			Dictionary<string, object> dictionary2 = MakeRuntimeLocStringTree(type);
			if (dictionary2.Count > 0)
			{
				dictionary[type.Name] = dictionary2;
			}
		}
		return dictionary;
	}

	private static string GetNote(FieldInfo fieldInfo)
	{
		return fieldInfo.GetCustomAttribute<NoteAttribute>()?.message;
	}

	private static void WriteStringsTemplate(string path, StreamWriter writer, Dictionary<string, object> runtimeTree)
	{
		if (writer == null)
		{
			Log.Warning("writer is null");
		}
		if (runtimeTree == null)
		{
			Log.Warning("runtimeTree is null");
		}
		List<string> list = new List<string>(runtimeTree.Keys);
		list.Sort();
		foreach (string item in list)
		{
			string text = path + "." + item;
			object obj = runtimeTree[item];
			if (obj == null)
			{
				Log.Warning("tree is null");
			}
			Type type = obj.GetType();
			if (type != typeof(string) && type != typeof(TextInfo))
			{
				WriteStringsTemplate(text, writer, obj as Dictionary<string, object>);
				continue;
			}
			TextInfo textInfo = obj as TextInfo;
			if (textInfo == null)
			{
				Log.Warning("info is null");
			}
			string text2 = ((textInfo == null) ? (obj as string) : textInfo.text);
			text2 = text2.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n")
				.Replace("’", "'")
				.Replace("“", "\\\"")
				.Replace("”", "\\\"")
				.Replace("…", "...");
			string value = ((textInfo == null) ? ("#. " + text) : ("# " + textInfo.translatorsNote));
			writer.WriteLine(value);
			writer.WriteLine("msgctxt \"{0}\"", text);
			writer.WriteLine("msgid \"" + text2 + "\"");
			writer.WriteLine("msgstr \"\"");
			writer.WriteLine("");
		}
	}
}
