using UnityEngine;

public class PoopData
{
	public bool skipSpawningPoop = false;

	public Storage storage = null;

	public string popupMessage = null;

	public Sprite popupIcon = null;

	public PoopData(bool skipSpawningPoop, Storage storage, string popupMessage = null, Sprite popupIcon = null)
	{
		this.skipSpawningPoop = skipSpawningPoop;
		this.storage = storage;
		this.popupMessage = popupMessage;
		this.popupIcon = popupIcon;
	}
}
