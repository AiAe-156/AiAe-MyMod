using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Klei;
using UnityEngine;

namespace KMod;

internal struct Directory : IFileSource
{
	private struct CopyDirectoryResult
	{
		public enum Error
		{
			None,
			Read,
			Write
		}

		public Error error;

		public int fileCount;
	}

	private AliasDirectory file_system;

	private string root;

	public Directory(string root)
	{
		this.root = root;
		file_system = new AliasDirectory(root, root, Application.streamingAssetsPath, isModded: true);
	}

	public string GetRoot()
	{
		return root;
	}

	public bool Exists()
	{
		return System.IO.Directory.Exists(GetRoot());
	}

	public bool Exists(string relative_path)
	{
		if (!Exists())
		{
			return false;
		}
		return new DirectoryInfo(FileSystem.Normalize(Path.Combine(root, relative_path))).Exists;
	}

	public void GetTopLevelItems(List<FileSystemItem> file_system_items, string relative_root)
	{
		relative_root = relative_root ?? "";
		string text = FileSystem.Normalize(Path.Combine(root, relative_root));
		DirectoryInfo directoryInfo = new DirectoryInfo(text);
		if (!directoryInfo.Exists)
		{
			Debug.LogError("Cannot iterate over $" + text + ", this directory does not exist");
			return;
		}
		FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
		foreach (FileSystemInfo fileSystemInfo in fileSystemInfos)
		{
			file_system_items.Add(new FileSystemItem
			{
				name = fileSystemInfo.Name,
				type = ((!(fileSystemInfo is DirectoryInfo)) ? FileSystemItem.ItemType.File : FileSystemItem.ItemType.Directory)
			});
		}
	}

	public IFileDirectory GetFileSystem()
	{
		return file_system;
	}

	public bool TryCopyTo(string path, List<string> extensions = null)
	{
		try
		{
			return CopyDirectory(root, path, extensions).error == CopyDirectoryResult.Error.None;
		}
		catch (UnauthorizedAccessException)
		{
			FileUtil.ErrorDialog(FileUtil.ErrorType.UnauthorizedAccess, path, null, null);
			return false;
		}
		catch (IOException)
		{
			FileUtil.ErrorDialog(FileUtil.ErrorType.IOError, path, null, null);
			return false;
		}
		catch
		{
			throw;
		}
	}

	public string Read(string relative_path)
	{
		try
		{
			using FileStream fileStream = File.OpenRead(Path.Combine(root, relative_path));
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, (int)fileStream.Length);
			return Encoding.UTF8.GetString(array);
		}
		catch
		{
			return string.Empty;
		}
	}

	private static CopyDirectoryResult CopyDirectory(string sourceDirName, string destDirName, List<string> extensions)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		if (!directoryInfo.Exists)
		{
			return new CopyDirectoryResult
			{
				error = CopyDirectoryResult.Error.Read,
				fileCount = 0
			};
		}
		if (!FileUtil.CreateDirectory(destDirName))
		{
			return new CopyDirectoryResult
			{
				error = CopyDirectoryResult.Error.Write,
				fileCount = 0
			};
		}
		FileInfo[] files = directoryInfo.GetFiles();
		CopyDirectoryResult result = new CopyDirectoryResult
		{
			error = CopyDirectoryResult.Error.None,
			fileCount = 0
		};
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			bool flag = extensions == null || extensions.Count == 0;
			if (extensions != null)
			{
				foreach (string extension in extensions)
				{
					if (extension == Path.GetExtension(fileInfo.Name).ToLower())
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				string destFileName = Path.Combine(destDirName, fileInfo.Name);
				if (fileInfo.CopyTo(destFileName, overwrite: false) == null)
				{
					result.error = CopyDirectoryResult.Error.Write;
					return result;
				}
				result.fileCount++;
			}
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		foreach (DirectoryInfo directoryInfo2 in directories)
		{
			string destDirName2 = Path.Combine(destDirName, directoryInfo2.Name);
			CopyDirectoryResult copyDirectoryResult = CopyDirectory(directoryInfo2.FullName, destDirName2, extensions);
			result.fileCount += copyDirectoryResult.fileCount;
			if (copyDirectoryResult.error != CopyDirectoryResult.Error.None)
			{
				result.error = copyDirectoryResult.error;
				return result;
			}
		}
		if (result.fileCount == 0)
		{
			FileUtil.DeleteDirectory(destDirName);
		}
		return result;
	}

	public void Dispose()
	{
	}
}
