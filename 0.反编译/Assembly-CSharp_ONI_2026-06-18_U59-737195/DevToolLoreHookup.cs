using System.Collections.Generic;
using ImGuiNET;
using UnityEngine;

public class DevToolLoreHookup : DevTool
{
	private string cachedUnlockId;

	private string unlockIdStatus;

	private Vector4 unlockIdStatusColor;

	private string cachedDisplayText;

	private string displayTextPreview;

	private bool displayTextValid;

	private string cachedNextCollectionId;

	private string nextCollectionIdStatus;

	private Vector4 nextCollectionIdStatusColor;

	public DevToolLoreHookup()
	{
		RequiresGameRunning = true;
	}

	private void ValidateUnlockId(string unlockId)
	{
		cachedUnlockId = unlockId;
		if (unlockId.IsNullOrWhiteSpace())
		{
			unlockIdStatus = null;
			return;
		}
		foreach (KeyValuePair<string, string[]> lockCollection in Game.Instance.unlocks.lockCollections)
		{
			string[] value = lockCollection.Value;
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i] == unlockId)
				{
					unlockIdStatus = "[" + lockCollection.Key + "]";
					unlockIdStatusColor = new Vector4(0.5f, 1f, 0.5f, 1f);
					return;
				}
			}
		}
		if (CodexCache.GetEntryForLock(unlockId) != null)
		{
			unlockIdStatus = "[codex]";
			unlockIdStatusColor = new Vector4(0.5f, 0.8f, 1f, 1f);
		}
		else
		{
			unlockIdStatus = "[unknown ID]";
			unlockIdStatusColor = new Vector4(1f, 0.4f, 0.4f, 1f);
		}
	}

	private void ValidateNextCollectionId(string collectionId)
	{
		cachedNextCollectionId = collectionId;
		string[] value;
		if (collectionId.IsNullOrWhiteSpace())
		{
			nextCollectionIdStatus = null;
		}
		else if (Game.Instance.unlocks.lockCollections.TryGetValue(collectionId, out value))
		{
			int num = value.Length;
			nextCollectionIdStatus = $"[{num} entries]";
			nextCollectionIdStatusColor = new Vector4(0.5f, 1f, 0.5f, 1f);
		}
		else
		{
			nextCollectionIdStatus = "[unknown collection]";
			nextCollectionIdStatusColor = new Vector4(1f, 0.4f, 0.4f, 1f);
		}
	}

	private void ValidateDisplayText(string displayText)
	{
		cachedDisplayText = displayText;
		StringEntry result;
		if (displayText.IsNullOrWhiteSpace())
		{
			displayTextPreview = null;
		}
		else if (Strings.TryGet(displayText, out result))
		{
			displayTextValid = true;
			displayTextPreview = result.String;
		}
		else
		{
			displayTextValid = false;
			displayTextPreview = "[string key not found]";
		}
	}

	protected override void RenderTo(DevPanel panel)
	{
		if (SelectTool.Instance == null || SelectTool.Instance.selected == null)
		{
			ImGui.Text("Select an entity in-game.");
			return;
		}
		GameObject gameObject = SelectTool.Instance.selected.gameObject;
		LoreBearer component = gameObject.GetComponent<LoreBearer>();
		ImGui.Text("Selected: " + gameObject.name);
		ImGui.Separator();
		if (component == null)
		{
			ImGui.Text("No LoreBearer component on this entity.");
			return;
		}
		ImGui.Text("LoreBearer Fields");
		ImGui.Separator();
		string input = component.poiOverrideLoreUnlockId ?? "";
		bool flag = false;
		if (ImGui.InputText("Override Unlock ID", ref input, 256u))
		{
			component.poiOverrideLoreUnlockId = (string.IsNullOrWhiteSpace(input) ? null : input);
			flag = true;
		}
		if (input != cachedUnlockId)
		{
			ValidateUnlockId(input);
		}
		if (unlockIdStatus != null)
		{
			ImGui.SameLine();
			ImGui.TextColored(unlockIdStatusColor, unlockIdStatus);
		}
		string input2 = component.poiOverrideLoreDisplayText ?? "";
		if (ImGui.InputText("Override Display Text (string key)", ref input2, 1024u))
		{
			component.poiOverrideLoreDisplayText = (string.IsNullOrWhiteSpace(input2) ? null : input2);
			flag = true;
		}
		if (input2 != cachedDisplayText)
		{
			ValidateDisplayText(input2);
		}
		if (displayTextPreview != null)
		{
			if (displayTextValid)
			{
				ImGui.TextWrapped(displayTextPreview);
			}
			else
			{
				ImGui.SameLine();
				ImGui.TextColored(new Vector4(1f, 0.4f, 0.4f, 1f), displayTextPreview);
			}
		}
		string input3 = component.poiOverrideNextCollectionId ?? "";
		if (ImGui.InputText("Override Next Collection ID (Optional)", ref input3, 256u))
		{
			component.poiOverrideNextCollectionId = (string.IsNullOrWhiteSpace(input3) ? null : input3);
			flag = true;
		}
		if (input3 != cachedNextCollectionId)
		{
			ValidateNextCollectionId(input3);
		}
		if (nextCollectionIdStatus != null)
		{
			ImGui.SameLine();
			ImGui.TextColored(nextCollectionIdStatusColor, nextCollectionIdStatus);
		}
		if (flag)
		{
			if (!string.IsNullOrEmpty(component.poiOverrideLoreUnlockId))
			{
				if (!string.IsNullOrEmpty(component.poiOverrideNextCollectionId))
				{
					component.Internal_SetContent(LoreBearerUtil.UnlockSpecificEntryThenNext(component.poiOverrideLoreUnlockId, component.poiOverrideLoreDisplayText, LoreBearerUtil.GetUnlockActionForCollection(component.poiOverrideNextCollectionId)));
				}
				else
				{
					component.Internal_SetContent(LoreBearerUtil.UnlockSpecificEntry(component.poiOverrideLoreUnlockId, Strings.Get(component.poiOverrideLoreDisplayText)));
				}
			}
			else
			{
				component.Internal_SetContent(null);
			}
		}
		ImGui.Separator();
		if (ImGui.Button("Make Uninspected"))
		{
			component.Debug_ResetSearched();
		}
		ImGui.SameLine();
		if (ImGui.Button("Clear Override"))
		{
			component.poiOverrideLoreUnlockId = null;
			component.poiOverrideLoreDisplayText = null;
			component.poiOverrideNextCollectionId = null;
			component.Internal_SetContent(null);
		}
	}
}
