using System;
using System.Collections.Generic;
using System.IO;
using ImGuiNET;
using Klei;
using STRINGS;
using UnityEngine;

public class DevToolManager
{
	public const string SHOW_DEVTOOLS = "ShowDevtools";

	public static DevToolManager Instance;

	private bool toggleKeyWasDown = false;

	private bool showImGui = false;

	private bool prevShowImGui = false;

	private bool doesImGuiWantInput = false;

	private bool prevDoesImGuiWantInput = false;

	private bool showImguiState = false;

	private bool showImguiDemo = false;

	public bool UserAcceptedWarning;

	private DevToolWarning warning = new DevToolWarning();

	private DevToolMenuFontSize menuFontSize = new DevToolMenuFontSize();

	public DevPanelList panels = new DevPanelList();

	public DevToolMenuNodeList menuNodes = new DevToolMenuNodeList();

	public Dictionary<Type, string> devToolNameDict = new Dictionary<Type, string>();

	private HashSet<Type> dontAutomaticallyRegisterTypes = new HashSet<Type>();

	public bool Show => showImGui;

	private bool quickDevEnabled => DebugHandler.enabled && GenericGameSettings.instance.quickDevTools;

	public DevToolManager()
	{
		Instance = this;
		RegisterDevTool<DevToolSimDebug>("Debuggers/Sim Debug");
		RegisterDevTool<DevToolStateMachineDebug>("Debuggers/State Machine");
		RegisterDevTool<DevToolSaveGameInfo>("Debuggers/Save Game Info");
		RegisterDevTool<DevToolPerformanceInfo>("Debuggers/Performance Info");
		RegisterDevTool<DevToolPrintingPodDebug>("Debuggers/Printing Pod Debug");
		RegisterDevTool<DevToolBigBaseMutations>("Debuggers/Big Base Mutation Utilities");
		RegisterDevTool<DevToolNavGrid>("Debuggers/Nav Grid");
		RegisterDevTool<DevToolResearchDebugger>("Debuggers/Research");
		RegisterDevTool<DevToolStatusItems>("Debuggers/StatusItems");
		RegisterDevTool<DevToolUI>("Debuggers/UI");
		RegisterDevTool<DevToolUnlockedIds>("Debuggers/UnlockedIds List");
		RegisterDevTool<DevToolStringsTable>("Debuggers/StringsTable");
		RegisterDevTool<DevToolChoreDebugger>("Debuggers/Chore");
		RegisterDevTool<DevToolAllThingsCritter>("Debuggers/Critter Info");
		RegisterDevTool<DevToolBatchedAnimDebug>("Debuggers/Batched Anim");
		RegisterDevTool<DevTool_StoryTraits_Reveal>("Debuggers/Story Traits Reveal");
		RegisterDevTool<DevTool_StoryTrait_CritterManipulator>("Debuggers/Story Trait - Critter Manipulator");
		RegisterDevTool<DevToolAnimEventManager>("Debuggers/Anim Event Manager");
		RegisterDevTool<DevToolDebugModeToggle>("Debuggers/Debug Mode Toggle");
		RegisterDevTool<DevToolSceneBrowser>("Scene/Browser");
		RegisterDevTool<DevToolSceneInspector>("Scene/Inspector");
		menuNodes.AddAction("Help/" + UI.FRONTEND.DEVTOOLS.TITLE.text, delegate
		{
			warning.ShouldDrawWindow = true;
		});
		RegisterDevTool<DevToolCommandPalette>("Help/Command Palette");
		RegisterAdditionalDevToolsByReflection();
	}

	public void Init()
	{
		UserAcceptedWarning = KPlayerPrefs.GetInt("ShowDevtools", 0) == 1;
	}

	private void RegisterDevTool<T>(string location) where T : DevTool, new()
	{
		menuNodes.AddAction(location, delegate
		{
			panels.AddPanelFor<T>();
		});
		dontAutomaticallyRegisterTypes.Add(typeof(T));
		devToolNameDict[typeof(T)] = Path.GetFileName(location);
	}

	private void RegisterAdditionalDevToolsByReflection()
	{
		List<Type> list = ReflectionUtil.CollectTypesThatInheritOrImplement<DevTool>();
		foreach (Type type in list)
		{
			if (!type.IsAbstract && !dontAutomaticallyRegisterTypes.Contains(type) && ReflectionUtil.HasDefaultConstructor(type))
			{
				menuNodes.AddAction("Debuggers/" + DevToolUtil.GenerateDevToolName(type), delegate
				{
					panels.AddPanelFor((DevTool)Activator.CreateInstance(type));
				});
			}
		}
	}

	public void UpdateShouldShowTools()
	{
		if (!DebugHandler.enabled)
		{
			showImGui = false;
			return;
		}
		bool flag = Input.GetKeyDown(KeyCode.BackQuote) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl));
		if (!toggleKeyWasDown && flag)
		{
			showImGui = !showImGui;
		}
		toggleKeyWasDown = flag;
	}

	public void UpdateTools()
	{
		if (!DebugHandler.enabled)
		{
			return;
		}
		if (showImGui)
		{
			if (warning.ShouldDrawWindow)
			{
				warning.DrawWindow(out warning.ShouldDrawWindow);
			}
			if (!UserAcceptedWarning)
			{
				warning.DrawMenuBar();
			}
			else
			{
				DrawMenu();
				panels.Render();
				if (showImguiState)
				{
					if (ImGui.Begin("ImGui state", ref showImguiState))
					{
						ImGui.Checkbox("ImGui.GetIO().WantCaptureMouse", ref ImGui.GetIO().WantCaptureMouse);
						ImGui.Checkbox("ImGui.GetIO().WantCaptureKeyboard", ref ImGui.GetIO().WantCaptureKeyboard);
					}
					ImGui.End();
				}
				if (showImguiDemo)
				{
					ImGui.ShowDemoWindow(ref showImguiDemo);
				}
			}
		}
		UpdateConsumingGameInputs();
		UpdateShortcuts();
	}

	private void UpdateShortcuts()
	{
		if ((showImGui || quickDevEnabled) && UserAcceptedWarning)
		{
			DoUpdate();
		}
		void DoUpdate()
		{
			if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Space))
			{
				DevToolCommandPalette.Init();
				showImGui = true;
			}
			if (Input.GetKeyDown(KeyCode.Comma))
			{
				DevToolUI.PingHoveredObject();
				showImGui = true;
			}
		}
	}

	private void DrawMenu()
	{
		menuFontSize.InitializeIfNeeded();
		if (ImGui.BeginMainMenuBar())
		{
			menuNodes.Draw();
			menuFontSize.DrawMenu();
			if (ImGui.BeginMenu("IMGUI"))
			{
				ImGui.Checkbox("ImGui state", ref showImguiState);
				ImGui.Checkbox("ImGui Demo", ref showImguiDemo);
				ImGui.EndMenu();
			}
			ImGui.EndMainMenuBar();
		}
	}

	private void UpdateConsumingGameInputs()
	{
		doesImGuiWantInput = false;
		if (showImGui)
		{
			doesImGuiWantInput = ImGui.GetIO().WantCaptureMouse || ImGui.GetIO().WantCaptureKeyboard;
			if (!prevDoesImGuiWantInput && doesImGuiWantInput)
			{
				OnInputEnterImGui();
			}
			if (prevDoesImGuiWantInput && !doesImGuiWantInput)
			{
				OnInputExitImGui();
			}
		}
		if (prevShowImGui && prevDoesImGuiWantInput && !showImGui)
		{
			OnInputExitImGui();
		}
		prevShowImGui = showImGui;
		prevDoesImGuiWantInput = doesImGuiWantInput;
		KInputManager.devToolFocus = showImGui && doesImGuiWantInput;
		static void OnInputEnterImGui()
		{
			UnityMouseCatcherUI.SetEnabled(is_enabled: true);
			GameInputManager inputManager = Global.GetInputManager();
			for (int i = 0; i < inputManager.GetControllerCount(); i++)
			{
				KInputController controller = inputManager.GetController(i);
				controller.HandleCancelInput();
			}
		}
		static void OnInputExitImGui()
		{
			UnityMouseCatcherUI.SetEnabled(is_enabled: false);
		}
	}
}
