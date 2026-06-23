using UnityEngine;

public class PoopData
{
	public bool skipSpawningPoop;

	public Storage storage;

	public string popupMessage;

	public Sprite popupIcon;

	public PoopData(bool skipSpawningPoop, Storage storage, string popupMessage = null, Sprite popupIcon = null)
	{
		this.skipSpawningPoop = skipSpawningPoop;
		this.storage = storage;
		this.popupMessage = popupMessage;
		this.popupIcon = popupIcon;
	}
}
