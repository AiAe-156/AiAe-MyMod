using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TrueTiles.Datagen;

public class TileDataGen : DataGen
{
	private const string TILE = "tile";

	private const string CARPET = "carpet";

	private const string INSULATION = "insulation";

	private const string MESH = "mesh";

	private const string AIRFLOW = "airflow";

	private const string METAL = "metal";

	private const string WINDOW = "window";

	private const string WOOD = "wood";

	private const string PLASTIC = "plastic";

	public TileDataGen(string path)
		: base(path)
	{
		WriteTiles("Default", ConfigureDefaultTiles());
		WriteTiles("CutesyCarpet", ConfigureCutesyCarpet());
		WriteTiles("AltAirflow", ConfigureAltAirflow());
	}

	private Dictionary<string, Dictionary<string, TileData>> ConfigureAltAirflow()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, Dictionary<string, TileData>> dictionary = new Dictionary<string, Dictionary<string, TileData>>();
		AddTiles(dictionary, "GasPermeableMembrane").AddSimpleTile("airflow", ((object)(SimHashes)167973730/*cast due to .constrained prefix*/).ToString(), top: false).AddSimpleTile("airflow", ((object)(SimHashes)1875790680/*cast due to .constrained prefix*/).ToString(), top: false).AddSimpleTile("airflow", ((object)(SimHashes)(-1411918265)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)(-1736594426)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)28407099/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)361868060/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)1608833498/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)(-1779895821)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)(-899253461)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)1387581016/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)(-198894563)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)1559722904/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)134298891/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)(-1208854194)/*cast due to .constrained prefix*/).ToString(), top: false)
			.Add((SimHashes)2108244480, new TileDataBuilder("airflow", (SimHashes)167973730, top: false).Build())
			.Add((SimHashes)108179667, new TileDataBuilder("airflow", (SimHashes)(-1411918265), top: false).Build())
			.Add((SimHashes)(-1725038055), new TileDataBuilder("airflow", (SimHashes)(-1736594426), top: false).Build())
			.Add((SimHashes)(-279785280), new TileDataBuilder("airflow", (SimHashes)361868060, top: false).Build())
			.Add((SimHashes)2059777261, new TileDataBuilder("airflow", (SimHashes)361868060, top: false).Build())
			.Add((SimHashes)(-1774383478), new TileDataBuilder("airflow", (SimHashes)1387581016, top: false).Build())
			.Add((SimHashes)1306370440, new TileDataBuilder("airflow", (SimHashes)1608833498, top: false).Build())
			.Add((SimHashes)(-537625624), new TileDataBuilder("airflow", (SimHashes)1875790680, top: false).Build())
			.AddShinyTile("airflow", ((object)(SimHashes)(-755153220)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.Add((SimHashes)1064294988, new TileDataBuilder("airflow", (SimHashes)134298891, top: false).Build())
			.Add((SimHashes)(-348942381), new TileDataBuilder("airflow", (SimHashes)134298891, top: false).SpecularColor(Color.green).Build())
			.Add((SimHashes)(-1058835580), new TileDataBuilder("airflow", (SimHashes)(-1208854194), top: false).Build());
		return dictionary;
	}

	private Dictionary<string, Dictionary<string, TileData>> ConfigureDefaultTiles()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_050e: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_056b: Unknown result type (might be due to invalid IL or missing references)
		//IL_058a: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0606: Unknown result type (might be due to invalid IL or missing references)
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_0663: Unknown result type (might be due to invalid IL or missing references)
		//IL_0682: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_081b: Unknown result type (might be due to invalid IL or missing references)
		//IL_089a: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0912: Unknown result type (might be due to invalid IL or missing references)
		//IL_0930: Unknown result type (might be due to invalid IL or missing references)
		//IL_094e: Unknown result type (might be due to invalid IL or missing references)
		//IL_096c: Unknown result type (might be due to invalid IL or missing references)
		//IL_098a: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a02: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a20: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b36: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0be8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c07: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c26: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c45: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c64: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c83: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ca2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d1e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d3d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d5c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d88: Unknown result type (might be due to invalid IL or missing references)
		//IL_0da7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0de5: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, Dictionary<string, TileData>> dictionary = new Dictionary<string, Dictionary<string, TileData>>();
		AddTiles(dictionary, "Tile").AddSimpleTile("tile", ((object)(SimHashes)(-1467370314)/*cast due to .constrained prefix*/).ToString(), top: false).AddSimpleTile("tile", ((object)(SimHashes)1757792140/*cast due to .constrained prefix*/).ToString()).AddSimpleTile("tile", ((object)(SimHashes)(-105943486)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("tile", ((object)(SimHashes)878995724/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("tile", ((object)(SimHashes)(-355957251)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("tile", ((object)(SimHashes)(-2008682336)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("tile", ((object)(SimHashes)1071649902/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("tile", ((object)(SimHashes)1282846257/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("tile", ((object)(SimHashes)(-474151749)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("tile", ((object)(SimHashes)493438017/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("tile", ((object)(SimHashes)183408504/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("tile", ((object)(SimHashes)(-1708952091)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("tile", ((object)(SimHashes)(-1713958528)/*cast due to .constrained prefix*/).ToString());
		AddTiles(dictionary, "WoodTile").AddSimpleTile("wood", ((object)(SimHashes)663200452/*cast due to .constrained prefix*/).ToString());
		AddTiles(dictionary, "CarpetTile").AddSimpleTile("carpet", ((object)(SimHashes)(-1467370314)/*cast due to .constrained prefix*/).ToString()).AddSimpleTile("carpet", ((object)(SimHashes)1757792140/*cast due to .constrained prefix*/).ToString()).AddSimpleTile("carpet", ((object)(SimHashes)(-105943486)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)878995724/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)(-355957251)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)(-2008682336)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)1071649902/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)1282846257/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)(-1708952091)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)(-474151749)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)183408504/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)(-1713958528)/*cast due to .constrained prefix*/).ToString());
		AddTiles(dictionary, "InsulationTile").AddSimpleTile("insulation", ((object)(SimHashes)(-1467370314)/*cast due to .constrained prefix*/).ToString(), top: false).AddSimpleTile("insulation", ((object)(SimHashes)1757792140/*cast due to .constrained prefix*/).ToString(), top: false).AddSimpleTile("insulation", ((object)(SimHashes)(-105943486)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("insulation", ((object)(SimHashes)878995724/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("insulation", ((object)(SimHashes)(-355957251)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("insulation", ((object)(SimHashes)(-2008682336)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("insulation", ((object)(SimHashes)1282846257/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("insulation", ((object)(SimHashes)(-474151749)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("insulation", ((object)(SimHashes)493438017/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("insulation", ((object)(SimHashes)183408504/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("insulation", ((object)(SimHashes)(-1708952091)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("insulation", ((object)(SimHashes)(-1713958528)/*cast due to .constrained prefix*/).ToString(), top: false);
		AddTiles(dictionary, "PlasticTile").AddSimpleTile("plastic", ((object)(SimHashes)1220285359/*cast due to .constrained prefix*/).ToString());
		AddTiles(dictionary, "MeshTile").AddShinyTile("mesh", ((object)(SimHashes)167973730/*cast due to .constrained prefix*/).ToString(), specularTop: false).AddShinyTile("mesh", ((object)(SimHashes)1387581016/*cast due to .constrained prefix*/).ToString(), specularTop: false).AddShinyTile("mesh", ((object)(SimHashes)(-198894563)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("mesh", ((object)(SimHashes)1875790680/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("mesh", ((object)(SimHashes)(-1411918265)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("mesh", ((object)(SimHashes)(-1736594426)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("mesh", ((object)(SimHashes)28407099/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("mesh", ((object)(SimHashes)361868060/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.Add((SimHashes)1608833498, new TileDataBuilder("mesh", (SimHashes)1608833498, top: false).Build())
			.AddShinyTile("mesh", ((object)(SimHashes)(-1779895821)/*cast due to .constrained prefix*/).ToString(), specularTop: false, top: false)
			.AddShinyTile("mesh", ((object)(SimHashes)(-899253461)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("mesh", ((object)(SimHashes)1559722904/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("mesh", ((object)(SimHashes)134298891/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("mesh", ((object)(SimHashes)(-1208854194)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.Add((SimHashes)(-1774383478), new TileDataBuilder("mesh", (SimHashes)1387581016, top: false).Specular("mesh_nickelore_spec").Build())
			.Add((SimHashes)2108244480, new TileDataBuilder("mesh", (SimHashes)167973730, top: false).Specular("mesh_aluminumore_spec").Build())
			.Add((SimHashes)108179667, new TileDataBuilder("mesh", (SimHashes)(-1411918265), top: false).Specular("mesh_cobaltite_spec").Build())
			.Add((SimHashes)(-1725038055), new TileDataBuilder("mesh", (SimHashes)(-1736594426), top: false).Specular("mesh_cuprite_spec").Build())
			.Add((SimHashes)(-279785280), new TileDataBuilder("mesh", (SimHashes)361868060, top: false).Specular("mesh_goldamalgam_spec").Build())
			.Add((SimHashes)2059777261, new TileDataBuilder("mesh", (SimHashes)361868060, top: false).Specular("mesh_goldamalgam_spec").Build())
			.Add((SimHashes)1306370440, new TileDataBuilder("mesh", (SimHashes)1608833498, top: false).Build())
			.AddShinyTile("mesh", ((object)(SimHashes)(-755153220)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.Add((SimHashes)1064294988, new TileDataBuilder("mesh", (SimHashes)134298891, top: false).Specular("mesh_uraniumore_spec").Build())
			.Add((SimHashes)(-348942381), new TileDataBuilder("mesh", (SimHashes)134298891, top: false).Specular("mesh_uraniumore_spec").SpecularColor(Color.green).Build())
			.Add((SimHashes)(-537625624), new TileDataBuilder("mesh", (SimHashes)1875790680, top: false).Specular("mesh_cinnabar_spec").Build())
			.Add((SimHashes)(-1058835580), new TileDataBuilder("mesh", (SimHashes)(-1208854194), top: false).Specular("mesh_wolframite_spec").Build());
		AddTiles(dictionary, "GasPermeableMembrane").AddSimpleTile("airflow", ((object)(SimHashes)167973730/*cast due to .constrained prefix*/).ToString(), top: false).AddSimpleTile("airflow", ((object)(SimHashes)1875790680/*cast due to .constrained prefix*/).ToString(), top: false).AddSimpleTile("airflow", ((object)(SimHashes)(-1411918265)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)1387581016/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)(-198894563)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)(-1736594426)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)28407099/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)361868060/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)1608833498/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)(-1779895821)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)(-899253461)/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)1559722904/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)134298891/*cast due to .constrained prefix*/).ToString(), top: false)
			.AddSimpleTile("airflow", ((object)(SimHashes)(-1208854194)/*cast due to .constrained prefix*/).ToString(), top: false)
			.Add((SimHashes)2108244480, new TileDataBuilder("airflow", (SimHashes)167973730, top: false).Build())
			.Add((SimHashes)(-1774383478), new TileDataBuilder("airflow", (SimHashes)1387581016, top: false).Build())
			.Add((SimHashes)(-537625624), new TileDataBuilder("airflow", (SimHashes)1875790680, top: false).Build())
			.Add((SimHashes)108179667, new TileDataBuilder("airflow", (SimHashes)(-1411918265), top: false).Build())
			.Add((SimHashes)(-1725038055), new TileDataBuilder("airflow", (SimHashes)(-1736594426), top: false).Build())
			.Add((SimHashes)(-279785280), new TileDataBuilder("airflow", (SimHashes)361868060, top: false).Build())
			.Add((SimHashes)2059777261, new TileDataBuilder("airflow", (SimHashes)361868060, top: false).Build())
			.Add((SimHashes)1306370440, new TileDataBuilder("airflow", (SimHashes)1608833498, top: false).Build())
			.AddShinyTile("airflow", ((object)(SimHashes)(-755153220)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.Add((SimHashes)1064294988, new TileDataBuilder("airflow", (SimHashes)134298891, top: false).Build())
			.Add((SimHashes)(-348942381), new TileDataBuilder("airflow", (SimHashes)134298891, top: false).SpecularColor(Color.green).Build())
			.Add((SimHashes)(-1058835580), new TileDataBuilder("airflow", (SimHashes)(-1208854194), top: false).Build());
		AddTiles(dictionary, "MetalTile").AddShinyTile("metal", ((object)(SimHashes)2108244480/*cast due to .constrained prefix*/).ToString(), specularTop: false, top: false).AddShinyTile("metal", ((object)(SimHashes)108179667/*cast due to .constrained prefix*/).ToString(), specularTop: false).AddShinyTile("metal", ((object)(SimHashes)(-1725038055)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("metal", ((object)(SimHashes)1064294988/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("metal", ((object)(SimHashes)(-279785280)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("metal", ((object)(SimHashes)(-1774383478)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("metal", ((object)(SimHashes)(-198894563)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("metal", ((object)(SimHashes)1306370440/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("metal", ((object)(SimHashes)(-755153220)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("metal", ((object)(SimHashes)(-1779895821)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("metal", ((object)(SimHashes)(-899253461)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("metal", ((object)(SimHashes)1559722904/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("metal", ((object)(SimHashes)(-537625624)/*cast due to .constrained prefix*/).ToString(), specularTop: false)
			.AddShinyTile("metal", ((object)(SimHashes)(-1058835580)/*cast due to .constrained prefix*/).ToString(), specularTop: false);
		AddTiles(dictionary, "GlassTile").AddShinyTile("window", ((object)(SimHashes)(-2079931820)/*cast due to .constrained prefix*/).ToString()).AddShinyTile("window", ((object)(SimHashes)1376267226/*cast due to .constrained prefix*/).ToString()).Add((SimHashes)1254083875, new TileDataBuilder("window", (SimHashes)1376267226).Build())
			.AddShinyTile("window", ((object)(SimHashes)(-1130501789)/*cast due to .constrained prefix*/).ToString());
		return dictionary;
	}

	private Dictionary<string, Dictionary<string, TileData>> ConfigureCutesyCarpet()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, Dictionary<string, TileData>> dictionary = new Dictionary<string, Dictionary<string, TileData>>();
		AddTiles(dictionary, "CarpetTile").AddSimpleTile("carpet", ((object)(SimHashes)(-1467370314)/*cast due to .constrained prefix*/).ToString()).AddSimpleTile("carpet", ((object)(SimHashes)1757792140/*cast due to .constrained prefix*/).ToString()).AddSimpleTile("carpet", ((object)(SimHashes)(-105943486)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)878995724/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)(-355957251)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)(-2008682336)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)1282846257/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)(-1708952091)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)(-474151749)/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)493438017/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)183408504/*cast due to .constrained prefix*/).ToString())
			.AddSimpleTile("carpet", ((object)(SimHashes)(-1713958528)/*cast due to .constrained prefix*/).ToString());
		return dictionary;
	}

	private ElementOverrides AddTiles(Dictionary<string, Dictionary<string, TileData>> tiles, string id)
	{
		ElementOverrides elementOverrides = new ElementOverrides(id);
		tiles.Add(id, elementOverrides.items);
		return elementOverrides;
	}

	private void WriteTiles(string id, Dictionary<string, Dictionary<string, TileData>> data)
	{
		DataGen.Write(FileUtil.GetOrCreateDirectory(Path.Combine(path, id)), "tiles", data);
	}
}
