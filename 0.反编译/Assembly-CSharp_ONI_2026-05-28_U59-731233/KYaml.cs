using System;
using System.IO;
using Klei;
using VYaml.Serialization;

public static class KYaml
{
	public struct Error
	{
		public enum Severity
		{
			Fatal,
			Recoverable
		}

		public FileHandle file;

		public string message;

		public Exception inner_exception;

		public string text;

		public Severity severity;
	}

	public delegate void ErrorHandler(string path, Exception exception);

	private static readonly YamlSerializerOptions Options = new YamlSerializerOptions
	{
		Resolver = CompositeResolver.Create(new IYamlFormatter[4]
		{
			new Vector2fFormatter(),
			new TagFormatter(),
			new SimHashesFormatter(),
			new ElementStateFormatter()
		}, new IYamlFormatterResolver[2]
		{
			KleiResolver.Instance,
			StandardResolver.Instance
		})
	};

	public static bool LoadFile<T>(string path, out T result, ErrorHandler errorHandler = null)
	{
		try
		{
			FileHandle fileHandle = FileSystem.FindFileHandle(path);
			if (fileHandle.source == null)
			{
				throw new FileNotFoundException("KYaml tried loading a file that doesn't exist: " + path);
			}
			result = YamlSerializer.Deserialize<T>(fileHandle.source.ReadBytes(fileHandle.full_path), Options);
			return true;
		}
		catch (Exception exception)
		{
			errorHandler?.Invoke(path, exception);
			result = default(T);
			return false;
		}
	}

	public static bool LoadFile<T>(FileHandle file, out T result, ErrorHandler errorHandler = null)
	{
		try
		{
			byte[] array = ((file.source != null) ? file.source.ReadBytes(file.full_path) : File.ReadAllBytes(file.full_path));
			result = YamlSerializer.Deserialize<T>(array, Options);
			return true;
		}
		catch (Exception exception)
		{
			errorHandler?.Invoke(file.full_path, exception);
			result = default(T);
			return false;
		}
	}
}
