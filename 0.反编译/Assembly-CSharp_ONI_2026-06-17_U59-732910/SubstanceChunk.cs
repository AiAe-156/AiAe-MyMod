using KSerialization;
using STRINGS;
using UnityEngine;

[SkipSaveFileSerialization]
[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/SubstanceChunk")]
public class SubstanceChunk : KMonoBehaviour, ISaveLoadable
{
	private const string symbolName = "substance_tinter";

	private static readonly KAnimHashedString symbolToTint = new KAnimHashedString("substance_tinter");

	private static readonly KAnimHashedString symbolToTint2 = new KAnimHashedString("substance_tinter_cap");

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Element element = GetComponent<PrimaryElement>().Element;
		KBatchedAnimController component = GetComponent<KBatchedAnimController>();
		if (element.IsLiquid)
		{
			GameUtil.TintLiquidSymbolOnBuilding("substance_tinter", component, element);
			return;
		}
		Color color = element.substance.colour;
		color.a = 1f;
		component.SetSymbolTint(symbolToTint, color);
		component.SetSymbolTint(symbolToTint2, color);
	}

	private void OnRefreshUserMenu(object data)
	{
		Game.Instance.userMenu.AddButton(base.gameObject, new KIconButtonMenu.ButtonInfo("action_deconstruct", UI.USERMENUACTIONS.RELEASEELEMENT.NAME, OnRelease, Action.NumActions, null, null, null, UI.USERMENUACTIONS.RELEASEELEMENT.TOOLTIP));
	}

	private void OnRelease()
	{
		int gameCell = Grid.PosToCell(base.transform.GetPosition());
		PrimaryElement component = GetComponent<PrimaryElement>();
		if (component.Mass > 0f)
		{
			SimMessages.AddRemoveSubstance(gameCell, component.ElementID, CellEventLogger.Instance.ExhaustSimUpdate, component.Mass, component.Temperature, component.DiseaseIdx, component.DiseaseCount);
		}
		base.gameObject.DeleteObject();
	}
}
