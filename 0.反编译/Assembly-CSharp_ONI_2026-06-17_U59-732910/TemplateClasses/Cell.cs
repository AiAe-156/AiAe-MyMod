using System;
using System.ComponentModel;

namespace TemplateClasses;

[Serializable]
public class Cell
{
	public SimHashes element { get; set; }

	public float mass { get; set; }

	public float temperature { get; set; }

	public string diseaseName { get; set; }

	public int diseaseCount { get; set; }

	public int location_x { get; set; }

	public int location_y { get; set; }

	public bool preventFoWReveal { get; set; }

	[DefaultValue(SimHashes.Vacuum)]
	public SimHashes backwallElement { get; set; }

	public float backwallTemperature { get; set; }

	public float backwallMass { get; set; }

	public Cell()
	{
	}

	public Cell(int loc_x, int loc_y, int gameCell)
	{
		location_x = loc_x;
		location_y = loc_y;
		element = Grid.Element[gameCell].id;
		temperature = Grid.Temperature[gameCell];
		mass = Grid.Mass[gameCell];
		diseaseName = ((Grid.DiseaseIdx[gameCell] != byte.MaxValue) ? Db.Get().Diseases[Grid.DiseaseIdx[gameCell]].Id : null);
		diseaseCount = Grid.DiseaseCount[gameCell];
		preventFoWReveal = Grid.PreventFogOfWarReveal[gameCell];
		if (BackwallManager.HasBackwall(gameCell))
		{
			backwallElement = BackwallManager.At(gameCell).Element.id;
			backwallTemperature = BackwallManager.At(gameCell).Temperature;
			backwallMass = BackwallManager.At(gameCell).Mass;
		}
		else
		{
			backwallElement = SimHashes.Vacuum;
			backwallTemperature = 0f;
			backwallMass = 0f;
		}
	}

	public Cell(int loc_x, int loc_y, SimHashes _element, float _temperature, float _mass, string _diseaseName, int _diseaseCount, bool _preventFoWReveal = false, SimHashes _backwallElement = SimHashes.Vacuum, float _backwallMass = 0f, float _backwallTemperature = 0f)
	{
		location_x = loc_x;
		location_y = loc_y;
		element = _element;
		temperature = _temperature;
		mass = _mass;
		diseaseName = _diseaseName;
		diseaseCount = _diseaseCount;
		preventFoWReveal = _preventFoWReveal;
		backwallElement = _backwallElement;
		backwallMass = _backwallMass;
		backwallTemperature = _backwallTemperature;
	}
}
