using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class MotdDataFetchRequest : IDisposable
{
	private MotdData data;

	private bool isComplete;

	private Action<MotdData> onCompleteFn;

	public void Dispose()
	{
		onCompleteFn = null;
	}

	public void Fetch(string url)
	{
		FetchWebMotdJson(url, delegate(MotdData webMotd)
		{
			data = webMotd;
			if (webMotd == null)
			{
				Debug.LogWarning("MOTD Error: failed to get web motd json");
				CompleteWith(null);
			}
			else
			{
				FetchWebMotdImagesFor(webMotd, delegate(bool isOk)
				{
					foreach (MotdData_Box item in webMotd.boxesLive)
					{
						if (item.resolvedImage.IsNullOrDestroyed())
						{
							isOk = false;
						}
					}
					if (!isOk)
					{
						Debug.LogWarning("MOTD Error: couldn't fetch all web motd images");
						CompleteWith(null);
					}
					else
					{
						WriteCachedMotdImages(webMotd);
						CompleteWith(webMotd);
					}
				});
			}
		});
		void CompleteWith(MotdData data)
		{
			if (!isComplete)
			{
				isComplete = true;
				this.data = data;
				if (onCompleteFn != null)
				{
					onCompleteFn(data);
				}
			}
		}
	}

	public void OnComplete(Action<MotdData> callbackFn)
	{
		if (isComplete)
		{
			callbackFn(data);
		}
		else
		{
			onCompleteFn = (Action<MotdData>)Delegate.Combine(onCompleteFn, callbackFn);
		}
	}

	public static void FetchWebMotdJson(string url, Action<MotdData> onCompleteFn)
	{
		UnityWebRequest webRequest = UnityWebRequest.Get(url);
		webRequest.timeout = 3;
		webRequest.SetRequestHeader("Content-Type", "application/json");
		webRequest.SendWebRequest().completed += delegate
		{
			if (string.IsNullOrEmpty(webRequest.error))
			{
				onCompleteFn(MotdData.Parse(webRequest.downloadHandler.text));
			}
			else
			{
				Debug.LogWarning("MOTD Error: failed to fetch web motd. " + webRequest.error);
				onCompleteFn(null);
			}
			webRequest.Dispose();
		};
	}

	public static void FetchWebMotdImagesFor(MotdData motdData, Action<bool> onCompleteFn)
	{
		foreach (MotdData_Box item in motdData.boxesLive)
		{
			if (item.image == null)
			{
				onCompleteFn(obj: false);
				return;
			}
		}
		int imagesToFetchCount = motdData.boxesLive.Count;
		if (imagesToFetchCount == 0)
		{
			onCompleteFn(obj: false);
			return;
		}
		int imagesValidCount = 0;
		foreach (MotdData_Box box in motdData.boxesLive)
		{
			FetchWebMotdImage(box.image, delegate(Texture2D resolvedImage, bool isFromDisk)
			{
				imagesToFetchCount--;
				box.resolvedImage = resolvedImage;
				box.resolvedImageIsFromDisk = isFromDisk;
				if (box.resolvedImage != null)
				{
					imagesValidCount++;
				}
				if (imagesToFetchCount == 0)
				{
					onCompleteFn((imagesValidCount == motdData.boxesLive.Count) ? true : false);
				}
			});
		}
	}

	public static void FetchWebMotdImage(string url, Action<Texture2D, bool> onCompleteFn)
	{
		Texture2D texture2D = ReadCachedMotdImage(url);
		if (texture2D != null)
		{
			onCompleteFn(texture2D, arg2: true);
			return;
		}
		UnityWebRequest webRequest = UnityWebRequest.Get(url);
		webRequest.timeout = 3;
		webRequest.SendWebRequest().completed += delegate
		{
			if (string.IsNullOrEmpty(webRequest.error))
			{
				onCompleteFn(ParseImage(webRequest.downloadHandler.data), arg2: false);
			}
			else
			{
				Debug.LogWarning("MOTD Error: failed to fetch web image at " + url + ". " + webRequest.error);
				onCompleteFn(null, arg2: false);
			}
			webRequest.Dispose();
		};
	}

	public static string GetCachePath()
	{
		return Path.Combine(Util.CacheFolder(), "motd");
	}

	public static string GetCachedFilePath(string filePath)
	{
		return Path.Combine(Util.CacheFolder(), "motd", Path.GetFileName(filePath));
	}

	public static void WriteCachedMotdImages(MotdData data)
	{
		if (data == null)
		{
			return;
		}
		try
		{
			if (!Directory.Exists(GetCachePath()))
			{
				Directory.CreateDirectory(GetCachePath());
			}
		}
		catch (Exception arg)
		{
			Debug.LogWarning($"MOTD Error: Failed to create image cache directory --- {arg}");
		}
		try
		{
			if (Directory.Exists(GetCachePath()))
			{
				foreach (MotdData_Box item in data.boxesLive)
				{
					if (item.image != null && item.resolvedImage != null && !item.resolvedImageIsFromDisk)
					{
						File.WriteAllBytes(GetCachedFilePath(item.image), item.resolvedImage.EncodeToPNG());
					}
				}
			}
			else
			{
				Debug.LogWarning("MOTD Error: Failed to write cached motd images, couldn't find a valid cache directory");
			}
		}
		catch (Exception arg2)
		{
			Debug.LogWarning($"MOTD Error: Failed to write cached motd images --- {arg2}");
		}
		try
		{
			if (Directory.Exists(GetCachePath()))
			{
				List<string> list = new List<string>(16);
				foreach (MotdData_Box item2 in data.boxesLive)
				{
					if (item2.image != null)
					{
						list.Add(GetCachedFilePath(item2.image));
					}
				}
				string[] files = Directory.GetFiles(GetCachePath());
				foreach (string text in files)
				{
					if (!list.Contains(GetCachedFilePath(text)))
					{
						File.Delete(text);
					}
				}
			}
			else
			{
				Debug.LogWarning("MOTD Error: Failed to clean cached motd images, couldn't find a valid cache directory");
			}
		}
		catch (Exception arg3)
		{
			Debug.LogWarning($"MOTD Error: Failed to clean cached motd images --- {arg3}");
		}
	}

	public static Texture2D ReadCachedMotdImage(string url)
	{
		string fileName = Path.GetFileName(url);
		string cachedFilePath = GetCachedFilePath(fileName);
		if (!File.Exists(cachedFilePath))
		{
			return null;
		}
		try
		{
			return ParseImage(File.ReadAllBytes(cachedFilePath));
		}
		catch (Exception arg)
		{
			Debug.LogWarning($"MOTD Error: Can't load cached motd image \"{fileName}\" --- {arg}");
			return null;
		}
	}

	public static string GetLocaleCode()
	{
		Localization.Locale locale = Localization.GetLocale();
		if (locale != null)
		{
			Localization.Language lang = locale.Lang;
			if (lang == Localization.Language.Chinese || (uint)(lang - 2) <= 1u)
			{
				return locale.Code;
			}
		}
		return null;
	}

	public static Texture2D ParseImage(byte[] buffer)
	{
		if (IsPng(buffer) || IsJpg(buffer))
		{
			Texture2D texture2D = new Texture2D(0, 0);
			texture2D.LoadImage(buffer);
			return texture2D;
		}
		if (IsKleiTex(buffer))
		{
			Debug.LogWarning("MOTD Error: Couldn't load image - KTEX isn't supported yet.");
			return null;
		}
		Debug.LogWarning("MOTD Error: Couldn't load image - Unsupported image file format.");
		return null;
		static bool IsJpg(byte[] array)
		{
			if (array[0] == byte.MaxValue && array[1] == 216 && array[6] == 74 && array[7] == 70 && array[8] == 73)
			{
				return array[9] == 70;
			}
			return false;
		}
		static bool IsKleiTex(byte[] array)
		{
			if (array[0] == 75 && array[1] == 84 && array[2] == 69)
			{
				return array[3] == 88;
			}
			return false;
		}
		static bool IsPng(byte[] array)
		{
			if (array[0] == 137 && array[1] == 80 && array[2] == 78 && array[3] == 71 && array[4] == 13 && array[5] == 10 && array[6] == 26)
			{
				return array[7] == 10;
			}
			return false;
		}
	}

	public static void GetUrlParams(out string platformCode, out string languageCode)
	{
		platformCode = "default";
		Localization.Language? language = ((Localization.GetLocale() == null) ? ((Localization.Language?)null) : new Localization.Language?(Localization.GetLocale().Lang));
		if (language.HasValue && language.GetValueOrDefault() == Localization.Language.Chinese)
		{
			languageCode = "schinese";
		}
		else
		{
			languageCode = "en";
		}
	}

	public static string BuildUrl()
	{
		GetUrlParams(out var platformCode, out var languageCode);
		return "https://motd.klei.com/motd.json/?game=oni&platform=" + platformCode + "&lang=" + languageCode;
	}
}
