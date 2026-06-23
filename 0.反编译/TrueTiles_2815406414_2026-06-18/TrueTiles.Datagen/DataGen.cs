using System;
using System.Collections.Generic;
using System.IO;
using FUtility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace TrueTiles.Datagen;

public class DataGen
{
	public class ElementOverrides
	{
		public string id;

		public Dictionary<string, TileData> items;

		public ElementOverrides(string buildingDef)
		{
			id = buildingDef;
			items = new Dictionary<string, TileData>();
		}

		public unsafe ElementOverrides Add(SimHashes element, TileData data)
		{
			items.Add(((object)(*(SimHashes*)(&element))/*cast due to .constrained prefix*/).ToString(), data);
			return this;
		}

		public ElementOverrides Add(string element, TileData data)
		{
			items.Add(element, data);
			return this;
		}

		public ElementOverrides AddSimpleTile(string prefix, string element, bool top = true)
		{
			Add(element, new TileDataBuilder().MainTex(prefix + "_" + element.ToLower() + "_main", top ? (prefix + "_" + element.ToLower() + "_top") : null).Build());
			return this;
		}

		public ElementOverrides AddShinyTile(string prefix, string element, bool specularTop = true, bool top = true)
		{
			Add(element, new TileDataBuilder().MainTex(prefix + "_" + element.ToLower() + "_main", top ? (prefix + "_" + element.ToLower() + "_top") : null).Specular(prefix + "_" + element.ToLower() + "_spec", specularTop ? (prefix + "_" + element.ToLower() + "_spec_top") : null).Build());
			return this;
		}
	}

	public class TileDataBuilder
	{
		private string main;

		private string top;

		private string spec;

		private string topSpec;

		private string normalTex;

		private float[] specColor;

		private float[] topSpecColor;

		private float frequency;

		private bool transparent;

		public TileDataBuilder()
		{
		}

		public TileDataBuilder(string prefix, string element, bool top = true)
		{
			MainTex(prefix + "_" + element.ToLower() + "_main", top ? (prefix + "_" + element.ToLower() + "_top") : null);
		}

		public unsafe TileDataBuilder(string prefix, SimHashes element, bool top = true)
			: this(prefix, ((object)(*(SimHashes*)(&element))/*cast due to .constrained prefix*/).ToString(), top)
		{
		}

		public TileDataBuilder MainTex(string texture, string top = null)
		{
			main = texture;
			this.top = top;
			return this;
		}

		public TileDataBuilder Specular(string texture, string top = null)
		{
			spec = texture;
			topSpec = top;
			return this;
		}

		public TileDataBuilder SpecularColor(Color color)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			specColor = new float[4] { color.r, color.g, color.b, color.a };
			return this;
		}

		public TileDataBuilder TopSpecularColor(Color color)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			topSpecColor = new float[4] { color.r, color.g, color.b, color.a };
			return this;
		}

		public TileDataBuilder Frequency(float value)
		{
			frequency = value;
			return this;
		}

		public TileDataBuilder Transparent()
		{
			transparent = true;
			return this;
		}

		public TileDataBuilder Normal(string texture)
		{
			normalTex = texture;
			return this;
		}

		public TileData Build()
		{
			return new TileData
			{
				MainTex = main,
				TopTex = top,
				MainSpecular = spec,
				TopSpecular = topSpec,
				TopSpecularColor = topSpecColor,
				MainSpecularColor = specColor,
				NormalTex = normalTex,
				Transparent = transparent,
				Frequency = frequency
			};
		}
	}

	protected string path;

	public DataGen(string path)
	{
		this.path = path;
	}

	public static void Write<T>(string path, string filename, T data)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0050: Expected O, but got Unknown
		try
		{
			path = FileUtil.GetOrCreateDirectory(path);
			path = Path.Combine(path, filename + ".json");
			string contents = JsonConvert.SerializeObject((object)data, (Formatting)1, new JsonSerializerSettings
			{
				NullValueHandling = (NullValueHandling)1,
				DefaultValueHandling = (DefaultValueHandling)1,
				Converters = new List<JsonConverter> { (JsonConverter)new StringEnumConverter() }
			});
			File.WriteAllText(path, contents);
		}
		catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
		{
			Log.Warning("Datagen: Could not write bundle file: " + ex.Message);
		}
	}
}
