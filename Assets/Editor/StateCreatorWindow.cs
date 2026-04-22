using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blues.Core.Events;
using Blues.Core.PoolSystem;
using Blues.Core.StateMachine;
using Blues.Core.UI;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// State Creator — Tools > State Creator
///
/// TWO-STEP FLOW (eliminates all script-timing bugs):
///
///   STEP 1 — "Create Scripts"
///     Creates folder structure + .cs files only.
///     No SO assets yet — scripts haven't compiled.
///     Unity compiles after this step.
///
///   STEP 2 — "Create Assets"  (enabled only after compilation)
///     Scripts are now compiled. Uses ScriptableObject.CreateInstance(compiledType)
///     so the asset is born with the correct script — no YAML tricks, no delayCall chains.
///     Creates SOs, prefab, registers pool, wires ApplicationFlowController.
/// </summary>
public class StateCreatorWindow : EditorWindow
{
    private enum StateType { UIState, GameState }

    // ── Fields ─────────────────────────────────────────────────────────────
    private string      _screenName           = "";
    private StateType   _stateType            = StateType.UIState;
    private ClosePolicy _closePolicy          = ClosePolicy.Default;
    private bool        _pausePreviousState   = false;
    private bool        _useDefaultAnimations = true;
    private string      _aiPrompt             = "";
    private Texture2D   _mockImage;

    private static readonly HttpClient _httpClient = new HttpClient();
    private bool    _isGenerating;
    private string  _generationLog = "";
    private Vector2 _logScroll;
    private float   _genProgress;

    private string _deleteStateName = "";

    private const string AfcPath   = "Assets/Saad/GameFlow/Scripts/ApplicationFlowController.cs";
    private const string AfcPrefab = "Assets/Saad/GameFlow/Prefabs/Resources/ApplicationFlowController.prefab";

    private Vector2 _scrollPos;
    private bool    _showDeleteSection;

    // ── Styles ──────────────────────────────────────────────────────────────
    private GUIStyle _titleStyle;
    private GUIStyle _subtitleStyle;
    private GUIStyle _headerStyle;
    private GUIStyle _promptTextStyle;
    private GUIStyle _logTextStyle;
    private GUIStyle _bigButtonStyle;
    private GUIStyle _tagStyle;
    private bool     _stylesInitialized;

    // ── Colors ──────────────────────────────────────────────────────────────
    private static readonly Color AccentBlue     = new Color(0.30f, 0.55f, 0.95f);
    private static readonly Color AccentGreen    = new Color(0.25f, 0.75f, 0.40f);
    private static readonly Color AccentOrange   = new Color(0.95f, 0.60f, 0.15f);
    private static readonly Color AccentRed      = new Color(0.90f, 0.30f, 0.30f);
    private static readonly Color AccentPurple   = new Color(0.60f, 0.40f, 0.90f);
    private static readonly Color SeparatorColor = new Color(0.45f, 0.45f, 0.45f, 0.35f);
    private static readonly Color HeaderBarColor = new Color(0.18f, 0.18f, 0.22f, 0.90f);

    [MenuItem("Tools/State Creator")]
    public static void ShowWindow()
    {
        var w = GetWindow<StateCreatorWindow>("State Creator");
        w.minSize = new Vector2(400, 560);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  STYLES
    // ════════════════════════════════════════════════════════════════════════

    private void InitStyles()
    {
        if (_stylesInitialized) return;
        _titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18, alignment = TextAnchor.MiddleLeft,
            padding  = new RectOffset(6, 0, 0, 0),
            normal   = { textColor = Color.white }
        };
        _subtitleStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 10, alignment = TextAnchor.MiddleLeft,
            padding  = new RectOffset(8, 0, 0, 0),
            normal   = { textColor = new Color(0.65f, 0.65f, 0.70f) }
        };
        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12, padding = new RectOffset(4, 0, 2, 2),
            normal   = { textColor = new Color(0.85f, 0.85f, 0.90f) }
        };
        _promptTextStyle = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true, fontSize = 12,
            padding  = new RectOffset(8, 8, 8, 8)
        };
        _logTextStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
        {
            fontSize = 10, richText = true,
            padding  = new RectOffset(6, 6, 4, 4),
            normal   = { textColor = new Color(0.70f, 0.80f, 0.70f) }
        };
        _bigButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13, fontStyle = FontStyle.Bold,
            fixedHeight = 40, margin = new RectOffset(4, 4, 4, 4)
        };
        _tagStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 9, alignment = TextAnchor.MiddleCenter,
            padding  = new RectOffset(6, 6, 2, 2),
            fontStyle = FontStyle.Bold,
            normal   = { textColor = Color.white }
        };
        _stylesInitialized = true;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GUI
    // ════════════════════════════════════════════════════════════════════════

    private void OnGUI()
    {
        InitStyles();
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        EditorGUILayout.Space(6);

        DrawTitleBar();
        EditorGUILayout.Space(8);

        // ── Section 1: Configuration ──────────────────────────────────────
        BeginSection();
        DrawSectionHeader("\u2699  Configuration", AccentBlue);
        EditorGUILayout.Space(6);

        EditorGUILayout.LabelField("State Type", _headerStyle);
        var prevBg = GUI.backgroundColor;
        GUI.backgroundColor = _stateType == StateType.UIState ? AccentBlue : AccentPurple;
        _stateType = (StateType)GUILayout.Toolbar((int)_stateType,
            new[] { "\u25A3  UI State", "\u25B6  Game State" }, GUILayout.Height(28));
        GUI.backgroundColor = prevBg;
        EditorGUILayout.Space(4);

        DrawInfoTag(
            _stateType == StateType.UIState
                ? "UIViewState + UIView + ViewData + Transition + Prefab"
                : "GameState + GameHud + Transition + Gameplay & HUD Prefabs",
            _stateType == StateType.UIState ? AccentBlue : AccentPurple);
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Screen Name", _headerStyle);
        _screenName = EditorGUILayout.TextField(_screenName);
        string trimmedName = _screenName.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedName))
            DrawPathPreview(trimmedName);
        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField("Behaviour", _headerStyle);
        _closePolicy = (ClosePolicy)EditorGUILayout.EnumPopup("Close Policy", _closePolicy);
        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        _pausePreviousState   = EditorGUILayout.ToggleLeft("  Pause Previous",     _pausePreviousState,   GUILayout.Width(140));
        _useDefaultAnimations = EditorGUILayout.ToggleLeft("  Default Animations", _useDefaultAnimations);
        EditorGUILayout.EndHorizontal();
        EndSection();
        EditorGUILayout.Space(6);

        // ── Section 2: AI Prompt ──────────────────────────────────────────
        BeginSection();
        DrawSectionHeader("\u2728  AI Agent Prompt", AccentPurple);
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Describe buttons, data, transitions, behaviour.", _subtitleStyle);
        EditorGUILayout.Space(4);
        _aiPrompt = EditorGUILayout.TextArea(_aiPrompt, _promptTextStyle, GUILayout.MinHeight(70));
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Mock / Reference Image", _headerStyle);
        _mockImage = (Texture2D)EditorGUILayout.ObjectField(_mockImage, typeof(Texture2D), false, GUILayout.Height(44));
        if (_mockImage != null)
            DrawInfoTag("Copied to Art/ and referenced in PROMPT.md", new Color(0.5f, 0.5f, 0.5f));
        EndSection();
        EditorGUILayout.Space(10);

        // ── Section 3: TWO-STEP BUTTONS ───────────────────────────────────
        BeginSection();
        DrawSectionHeader("\u25B6  Create", AccentGreen);
        EditorGUILayout.Space(6);

        // Determine current state of this screen
        string folder         = !string.IsNullOrWhiteSpace(trimmedName) ? $"Assets/Game/Screens/{trimmedName}" : null;
        bool   hasFolder      = folder != null && AssetDatabase.IsValidFolder(folder);
        bool   hasScripts     = hasFolder && AssetDatabase.IsValidFolder($"{folder}/Scripts") &&
                                Directory.GetFiles($"{folder}/Scripts", "*.cs").Length > 0;
        bool   isCompiled     = hasScripts && FindTypeByName($"{trimmedName}State") != null;
        bool   hasAssets      = hasFolder  && AssetDatabase.IsValidFolder($"{folder}/Config") &&
                                File.Exists($"{folder}/Config/{trimmedName}State.asset");

        // Status indicator
        if (hasFolder)
        {
            string status;
            Color  statusColor;
            if (hasAssets)
            {
                status = "\u2713  Complete — scripts + assets created";
                statusColor = AccentGreen;
            }
            else if (isCompiled)
            {
                status = "\u25B6  Scripts compiled — ready for Step 2";
                statusColor = AccentOrange;
            }
            else if (hasScripts)
            {
                status = "\u231B  Scripts created — waiting for compilation...";
                statusColor = AccentOrange;
            }
            else
            {
                status = "\u25CB  Folder exists but no scripts found";
                statusColor = AccentOrange;
            }
            DrawInfoTag(status, statusColor);
            EditorGUILayout.Space(6);
        }

        // ── STEP 1: Create Scripts ──
        bool step1Enabled = !string.IsNullOrWhiteSpace(trimmedName) && !hasScripts;
        EditorGUI.BeginDisabledGroup(!step1Enabled);
        GUI.backgroundColor = step1Enabled ? AccentGreen : new Color(0.4f, 0.4f, 0.4f);
        if (GUILayout.Button("  STEP 1 — Create Scripts  (then wait for compile)", _bigButtonStyle))
        {
            if (_stateType == StateType.UIState) CreateScripts_UI();
            else CreateScripts_Game();
        }
        GUI.backgroundColor = prevBg;
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField(
            hasScripts ? (isCompiled ? "\u2713 Scripts compiled" : "\u231B Compiling...") : "Creates folder + .cs files. Unity compiles after this.",
            _subtitleStyle);

        EditorGUILayout.Space(8);

        // ── STEP 2: Create Assets ──
        bool step2Enabled = isCompiled && !hasAssets;
        EditorGUI.BeginDisabledGroup(!step2Enabled);
        GUI.backgroundColor = step2Enabled ? AccentOrange : new Color(0.4f, 0.4f, 0.4f);
        if (GUILayout.Button("  STEP 2 — Create Assets  (SO assets + prefab + wiring)", _bigButtonStyle))
        {
            if (_stateType == StateType.UIState) CreateAssets_UI();
            else CreateAssets_Game();
        }
        GUI.backgroundColor = prevBg;
        EditorGUI.EndDisabledGroup();

        if (!step2Enabled && hasScripts && !isCompiled)
            EditorGUILayout.LabelField("Waiting for Unity to finish compiling...", _subtitleStyle);
        else if (!step2Enabled && hasAssets)
            EditorGUILayout.LabelField("\u2713 Assets already created", _subtitleStyle);
        else
            EditorGUILayout.LabelField("Creates SOs with correct types, prefab, pool, AFC wiring.", _subtitleStyle);

        EditorGUILayout.Space(8);

        // ── Implement with AI ──
        if (hasScripts)
        {
            EditorGUI.BeginDisabledGroup(_isGenerating);
            GUI.backgroundColor = AccentPurple;
            if (GUILayout.Button("\u2726  Implement with AI", _bigButtonStyle))
                _ = RunAIImplementation();
            GUI.backgroundColor = prevBg;
            EditorGUI.EndDisabledGroup();
        }

        EndSection();

        // Repaint so the status updates after compilation
        if (hasScripts && !isCompiled)
            Repaint();

        // ── Progress + Log ────────────────────────────────────────────────
        if (_isGenerating)
        {
            EditorGUILayout.Space(6);
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 22), _genProgress, "Generating...");
        }
        if (!string.IsNullOrEmpty(_generationLog))
        {
            EditorGUILayout.Space(6);
            BeginSection();
            DrawSectionHeader("\u2263  Log", new Color(0.5f, 0.7f, 0.5f));
            _logScroll = EditorGUILayout.BeginScrollView(_logScroll, GUILayout.MaxHeight(160));
            EditorGUILayout.LabelField(_generationLog, _logTextStyle);
            EditorGUILayout.EndScrollView();
            EndSection();
        }

        EditorGUILayout.Space(16);

        // ── Danger Zone ───────────────────────────────────────────────────
        DrawSeparator();
        _showDeleteSection = EditorGUILayout.Foldout(_showDeleteSection, "\u26A0  Danger Zone", true,
            new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                normal    = { textColor = AccentRed },
                onNormal  = { textColor = AccentRed }
            });
        if (_showDeleteSection)
        {
            BeginSection();
            EditorGUILayout.LabelField("Removes folder, AFC wiring, pool entries. Cannot be undone.", _subtitleStyle);
            EditorGUILayout.Space(4);
            _deleteStateName = EditorGUILayout.TextField("State Name", _deleteStateName);
            EditorGUILayout.Space(4);
            GUI.backgroundColor = AccentRed;
            if (GUILayout.Button("\u2716  DELETE STATE", _bigButtonStyle)) DeleteState();
            GUI.backgroundColor = prevBg;
            EndSection();
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.EndScrollView();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  STEP 1 — CREATE SCRIPTS (UI STATE)
    // ════════════════════════════════════════════════════════════════════════

    private void CreateScripts_UI()
    {
        if (!ValidateName(out string name, out string folder)) return;

        CreateFolderStructure(folder, name);

        string scriptsDir = $"{folder}/Scripts";
        File.WriteAllText($"{scriptsDir}/{name}ViewData.cs",   BuildViewData(name));
        File.WriteAllText($"{scriptsDir}/{name}UIView.cs",     BuildView(name));
        File.WriteAllText($"{scriptsDir}/{name}State.cs",      BuildUIStateScript(name));
        File.WriteAllText($"{scriptsDir}/{name}Transition.cs", BuildTransition(name));

        SavePromptFile(folder, name, "UIState", _pausePreviousState);
        CopyImplementationPrompt(name, folder);

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Step 1 Done \u2713",
            $"Scripts created:\n" +
            $"  {name}State.cs\n  {name}UIView.cs\n  {name}ViewData.cs\n  {name}Transition.cs\n\n" +
            "Unity is compiling. When complete, click STEP 2.",
            "OK");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  STEP 1 — CREATE SCRIPTS (GAME STATE)
    // ════════════════════════════════════════════════════════════════════════

    private void CreateScripts_Game()
    {
        if (!ValidateName(out string name, out string folder)) return;

        CreateFolderStructure(folder, name);

        string scriptsDir = $"{folder}/Scripts";
        File.WriteAllText($"{scriptsDir}/{name}Hud.cs",          BuildGameHud(name));
        File.WriteAllText($"{scriptsDir}/{name}State.cs",        BuildGameStateScript(name));
        File.WriteAllText($"{scriptsDir}/{name}Transition.cs",   BuildTransition(name));

        SavePromptFile(folder, name, "GameState", _pausePreviousState);
        CopyImplementationPrompt(name, folder);

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Step 1 Done \u2713",
            $"Scripts created:\n" +
            $"  {name}State.cs\n  {name}Hud.cs\n  {name}Transition.cs\n\n" +
            "Unity is compiling. When complete, click STEP 2.",
            "OK");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  STEP 2 — CREATE ASSETS (UI STATE)
    //
    //  KEY INSIGHT: Scripts are compiled by the time this runs.
    //  FindTypeByName() returns the actual C# type.
    //  ScriptableObject.CreateInstance(compiledType) creates an asset that
    //  is born with the correct script — no YAML hacks, no delayCall chains.
    // ════════════════════════════════════════════════════════════════════════

    private void CreateAssets_UI()
    {
        string name   = _screenName.Trim();
        string folder = $"Assets/Game/Screens/{name}";

        // Guard: compiled type must exist
        Type stateType = FindTypeByName($"{name}State");
        if (stateType == null)
        {
            EditorUtility.DisplayDialog("Not Compiled",
                $"{name}State is not compiled yet.\nWait for Unity to finish, then try again.", "OK");
            return;
        }

        string configDir = $"{folder}/Config";
        string prefabDir = $"{folder}/Prefabs";

        // ── GoToEvent (GameEvent — no custom type needed)
        var goToEvent = ScriptableObject.CreateInstance<GameEvent>();
        AssetDatabase.CreateAsset(goToEvent, $"{configDir}/GoTo{name}Event.asset");

        // ── State SO — created as the REAL compiled type
        var stateInstance = ScriptableObject.CreateInstance(stateType);
        AssetDatabase.CreateAsset(stateInstance, $"{configDir}/{name}State.asset");

        // Set all properties — no timing issue, type is correct from birth
        var pooler   = FindStatesPooler();
        var stateSO  = new SerializedObject(stateInstance);
        SafeSet(stateSO, "stateId",              name);
        SafeSet(stateSO, "usePooling",           true);
        SafeSet(stateSO, "PausePreviousState",   _pausePreviousState);
        SafeSet(stateSO, "useDefaultAnimations", _useDefaultAnimations);
        if (pooler != null) SafeSet(stateSO, "uIStatesPooler", pooler);
        stateSO.ApplyModifiedPropertiesWithoutUndo();

        // ── Transition SO — use custom type if compiled, else base Transition
        Type transType    = FindTypeByName($"{name}Transition");
        var  transInstance = transType != null
            ? ScriptableObject.CreateInstance(transType)
            : ScriptableObject.CreateInstance<Transition>();
        AssetDatabase.CreateAsset(transInstance, $"{configDir}/GoTo{name}Transition.asset");

        if (transInstance is Transition trans)
        {
            trans.ToState     = stateInstance as State;
            trans.closePolicy = _closePolicy;
            EditorUtility.SetDirty(transInstance);
        }

        // ── Prefab
        GameObject dummyPrefab = CreateUIDummyPrefab(name, $"{prefabDir}/{name}.prefab");

        // ── Pool
        bool poolWired = pooler != null && dummyPrefab != null;
        if (poolWired) AddPoolEntry(pooler, name, dummyPrefab);

        // ── AFC wiring
        bool flowUpdated = PatchApplicationFlowController(name, _closePolicy, _pausePreviousState);
        bool prefabWired = PatchAfcPrefab(name,
            $"{configDir}/GoTo{name}Event.asset",
            $"{configDir}/GoTo{name}Transition.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── Verify result
        var verify = AssetDatabase.LoadAssetAtPath<ScriptableObject>($"{configDir}/{name}State.asset");
        string typeConfirm = verify != null ? verify.GetType().Name : "LOAD FAILED";

        var sb = new StringBuilder();
        sb.AppendLine($"UI State '{name}' — Step 2 complete!\n");
        sb.AppendLine($"State asset type:  {typeConfirm}");
        sb.AppendLine($"stateId:           {name}");
        sb.AppendLine(poolWired    ? "StatesPooler: wired \u2713" : "\u26A0 StatesPooler: not found");
        sb.AppendLine(flowUpdated  ? "AFC script: wired \u2713"   : "\u26A0 AFC script: not wired");
        sb.AppendLine(prefabWired  ? "AFC prefab: wired \u2713"   : "\u26A0 AFC prefab: manual");
        sb.AppendLine();
        sb.AppendLine("Remaining manual steps:");
        sb.AppendLine("  1. Assign Script field on State SO (if still needed)");
        sb.AppendLine("  2. Add View component to prefab + wire refs");
        sb.AppendLine("  3. Assign DBBool / GameEvent SOs in State inspector");
        sb.AppendLine();
        sb.AppendLine("Prompt copied to clipboard \u2014 paste into Claude Code.");
        EditorUtility.DisplayDialog("Step 2 Done \u2713", sb.ToString(), "OK");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  STEP 2 — CREATE ASSETS (GAME STATE)
    // ════════════════════════════════════════════════════════════════════════

    private void CreateAssets_Game()
    {
        string name   = _screenName.Trim();
        string folder = $"Assets/Game/Screens/{name}";

        Type stateType = FindTypeByName($"{name}State");
        if (stateType == null)
        {
            EditorUtility.DisplayDialog("Not Compiled",
                $"{name}State is not compiled yet.\nWait for Unity to finish, then try again.", "OK");
            return;
        }

        string configDir = $"{folder}/Config";
        string prefabDir = $"{folder}/Prefabs";

        var goToEvent = ScriptableObject.CreateInstance<GameEvent>();
        AssetDatabase.CreateAsset(goToEvent, $"{configDir}/GoTo{name}Event.asset");

        var stateInstance = ScriptableObject.CreateInstance(stateType);
        AssetDatabase.CreateAsset(stateInstance, $"{configDir}/{name}State.asset");

        var stateSO = new SerializedObject(stateInstance);
        SafeSet(stateSO, "PausePreviousState", _pausePreviousState);
        stateSO.ApplyModifiedPropertiesWithoutUndo();

        Type transType     = FindTypeByName($"{name}Transition");
        var  transInstance = transType != null
            ? ScriptableObject.CreateInstance(transType)
            : ScriptableObject.CreateInstance<Transition>();
        AssetDatabase.CreateAsset(transInstance, $"{configDir}/GoTo{name}Transition.asset");

        if (transInstance is Transition trans)
        {
            trans.ToState     = stateInstance as State;
            trans.closePolicy = _closePolicy;
            EditorUtility.SetDirty(transInstance);
        }

        GameObject hudPrefab      = CreateGameHudDummyPrefab(name,  $"{prefabDir}/{name}Hud.prefab");
        GameObject gameplayPrefab = CreateGameplayDummyPrefab(name, $"{prefabDir}/{name}Gameplay.prefab");

        var pooler = FindStatesPooler();
        bool gpWired = false, hudWired = false;
        if (pooler != null)
        {
            if (gameplayPrefab != null) { AddPoolEntry(pooler, name,         gameplayPrefab); gpWired  = true; }
            if (hudPrefab != null)      { AddPoolEntry(pooler, $"{name}Hud", hudPrefab);      hudWired = true; }
        }

        bool flowUpdated = PatchApplicationFlowController(name, _closePolicy, _pausePreviousState);
        bool prefabWired = PatchAfcPrefab(name,
            $"{configDir}/GoTo{name}Event.asset",
            $"{configDir}/GoTo{name}Transition.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var verify     = AssetDatabase.LoadAssetAtPath<ScriptableObject>($"{configDir}/{name}State.asset");
        string typeConfirm = verify != null ? verify.GetType().Name : "LOAD FAILED";

        var sb = new StringBuilder();
        sb.AppendLine($"Game State '{name}' — Step 2 complete!\n");
        sb.AppendLine($"State asset type: {typeConfirm}");
        sb.AppendLine(gpWired      ? "Gameplay pool: wired \u2713" : "\u26A0 Gameplay pool: not wired");
        sb.AppendLine(hudWired     ? "HUD pool: wired \u2713"      : "\u26A0 HUD pool: not wired");
        sb.AppendLine(flowUpdated  ? "AFC script: wired \u2713"    : "\u26A0 AFC script: not wired");
        sb.AppendLine(prefabWired  ? "AFC prefab: wired \u2713"    : "\u26A0 AFC prefab: manual");
        sb.AppendLine();
        sb.AppendLine("Prompt copied to clipboard \u2014 paste into Claude Code.");
        EditorUtility.DisplayDialog("Step 2 Done \u2713", sb.ToString(), "OK");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  SAFE PROPERTY SETTER
    // ════════════════════════════════════════════════════════════════════════

    private static void SafeSet(SerializedObject so, string prop, object value)
    {
        var p = so.FindProperty(prop);
        if (p == null)
        {
            Debug.LogWarning($"[StateCreator] Property '{prop}' not found on {so.targetObject?.GetType().Name}");
            return;
        }
        switch (value)
        {
            case string s:               p.stringValue            = s;   break;
            case bool b:                 p.boolValue              = b;   break;
            case UnityEngine.Object obj: p.objectReferenceValue   = obj; break;
            default:
                Debug.LogWarning($"[StateCreator] Unsupported type for '{prop}': {value?.GetType()}");
                break;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CLIPBOARD PROMPT
    // ════════════════════════════════════════════════════════════════════════

    private static void CopyImplementationPrompt(string name, string folder)
    {
        EditorGUIUtility.systemCopyBuffer =
            $"Read Assets/Game/Screens/{name}/PROMPT.md and the mock image at " +
            $"Assets/Game/Screens/{name}/Art/Mock.png\n\n" +
            "Then read these files for patterns:\n" +
            "- Assets/Saad/Utilities/StateMachine/Scripts/UiViewState.cs\n" +
            "- Assets/Saad/UI/MainMenu/Scripts/MainMenuView.cs\n" +
            "- Assets/Saad/UI/MainMenu/States&Transition/ (all files)\n" +
            "- Assets/Saad/Variables/DBBool.cs\n\n" +
            $"Implement these three files completely, replacing all // AI agent: comments:\n" +
            $"- Assets/Game/Screens/{name}/Scripts/{name}UIView.cs\n" +
            $"- Assets/Game/Screens/{name}/Scripts/{name}ViewData.cs\n" +
            $"- Assets/Game/Screens/{name}/Scripts/{name}State.cs\n\n" +
            "Follow MainMenuView.cs style exactly. Named methods only, never lambdas.\n" +
            "After writing scripts, use MCP to open the prefab at " +
            $"Assets/Game/Screens/{name}/Prefabs/{name}.prefab, add the {name}UIView " +
            "component, create child GameObjects matching the mock layout, wire all " +
            "SerializeField references, save the prefab.";
    }

    // ════════════════════════════════════════════════════════════════════════
    //  FOLDER STRUCTURE
    // ════════════════════════════════════════════════════════════════════════

    private void CreateFolderStructure(string folder, string name)
    {
        EnsureFolder("Assets/Game");
        EnsureFolder("Assets/Game/Screens");
        AssetDatabase.CreateFolder("Assets/Game/Screens", name);
        AssetDatabase.CreateFolder(folder, "Prefabs");
        AssetDatabase.CreateFolder(folder, "Art");
        AssetDatabase.CreateFolder(folder, "Scripts");
        AssetDatabase.CreateFolder(folder, "Config");

        if (_mockImage != null)
        {
            string srcPath = AssetDatabase.GetAssetPath(_mockImage);
            if (!string.IsNullOrEmpty(srcPath))
                AssetDatabase.CopyAsset(srcPath, $"{folder}/Art/Mock{Path.GetExtension(srcPath)}");
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  DUMMY PREFAB CREATION
    // ════════════════════════════════════════════════════════════════════════

    private static GameObject CreateUIDummyPrefab(string name, string path)
    {
        var root = new GameObject(name);
        root.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();
        root.AddComponent<CanvasGroup>();

        var bg = new GameObject("Background");
        bg.transform.SetParent(root.transform, false);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        var content = new GameObject("Content");
        content.transform.SetParent(root.transform, false);
        var cr = content.AddComponent<RectTransform>();
        cr.anchorMin = Vector2.zero; cr.anchorMax = Vector2.one;
        cr.offsetMin = Vector2.zero; cr.offsetMax = Vector2.zero;

        // Add the View component if already compiled (second+ run)
        var viewType = FindTypeByName($"{name}UIView");
        if (viewType != null) root.AddComponent(viewType);

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateGameHudDummyPrefab(string name, string path)
    {
        var root = new GameObject($"{name}Hud");
        root.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();
        root.AddComponent<CanvasGroup>();
        CreateAnchoredChild("Header", root.transform, new Vector2(0,1), new Vector2(1,1), new Vector2(0,-200), Vector2.zero);
        CreateAnchoredChild("Middle", root.transform, Vector2.zero,     Vector2.one,      Vector2.zero,         Vector2.zero);
        CreateAnchoredChild("Footer", root.transform, Vector2.zero,     new Vector2(1,0), Vector2.zero,         new Vector2(0, 200));
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateGameplayDummyPrefab(string name, string path)
    {
        var root   = new GameObject($"{name}Gameplay");
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateAnchoredChild(string childName, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var child = new GameObject(childName);
        child.transform.SetParent(parent, false);
        var r = child.AddComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax;
        r.offsetMin = offsetMin; r.offsetMax = offsetMax;
        return child;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CODE GENERATORS
    // ════════════════════════════════════════════════════════════════════════

    private static string BuildTransition(string name) =>
        "using UnityEngine;\nusing Blues.Core.StateMachine;\n\n" +
        $"[CreateAssetMenu(fileName = \"{name}Transition\", menuName = \"ProjectCore/State Machine/Transitions/{name}Transition\")]\n" +
        $"public class {name}Transition : Transition\n{{\n}}\n";

    private static string BuildViewData(string name) =>
        "using System;\n\n[Serializable]\n" +
        $"public struct {name}ViewData\n{{\n    // AI agent: add fields from PROMPT.md\n}}\n";

    private static string BuildView(string name)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using Blues.Core.UI;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using UnityEngine.UI;");
        sb.AppendLine("using TMPro;");
        sb.AppendLine();
        sb.AppendLine($"public class {name}UIView : UIBase");
        sb.AppendLine("{");
        sb.AppendLine("    // AI agent: add [SerializeField] references from PROMPT.md");
        sb.AppendLine();
        sb.AppendLine($"    public void SetData({name}ViewData data)");
        sb.AppendLine("    {");
        sb.AppendLine("        // AI agent: bind data to view");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected override void Awake()");
        sb.AppendLine("    {");
        sb.AppendLine("        base.Awake();");
        sb.AppendLine("        // AI agent: wire button listeners");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildUIStateScript(string name)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using Blues.Core.Events;");
        sb.AppendLine("using Blues.Core.StateMachine;");
        sb.AppendLine("using System.Collections;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine();
        sb.AppendLine($"[CreateAssetMenu(fileName = \"{name}State\", menuName = \"ProjectCore/State Machine/States/{name}State\")]");
        sb.AppendLine($"public class {name}State : UIViewState");
        sb.AppendLine("{");
        sb.AppendLine("    // AI agent: add [SerializeField] GameEvent / variable fields");
        sb.AppendLine();
        sb.AppendLine($"    private {name}UIView _view;");
        sb.AppendLine();
        sb.AppendLine("    public override IEnumerator Enter(IState previous)");
        sb.AppendLine("    {");
        sb.AppendLine("        yield return base.Enter(previous);");
        sb.AppendLine($"        _view = GetView<{name}UIView>();");
        sb.AppendLine("        if (_view == null) yield break;");
        sb.AppendLine("        // AI agent: subscribe events + refresh view");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public override IEnumerator Exit()");
        sb.AppendLine("    {");
        sb.AppendLine("        // AI agent: unsubscribe events");
        sb.AppendLine("        _view = null;");
        sb.AppendLine("        yield return base.Exit();");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildGameHud(string name)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using Blues.Core.GameHud;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using UnityEngine.UI;");
        sb.AppendLine();
        sb.AppendLine($"public class {name}Hud : GameHud");
        sb.AppendLine("{");
        sb.AppendLine("    // AI agent: add SerializeField buttons / text");
        sb.AppendLine("    // AI agent: add public event Action fields");
        sb.AppendLine();
        sb.AppendLine("    protected override void Awake()");
        sb.AppendLine("    {");
        sb.AppendLine("        base.Awake();");
        sb.AppendLine("        // AI agent: wire button listeners");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildGameStateScript(string name)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using Blues.Core.Events;");
        sb.AppendLine("using System.Collections;");
        sb.AppendLine("using Blues.Core.StateMachine;");
        sb.AppendLine();
        sb.AppendLine($"[CreateAssetMenu(fileName = \"{name}State\", menuName = \"ProjectCore/State Machine/States/{name} Game State\")]");
        sb.AppendLine($"public class {name}State : GameState");
        sb.AppendLine("{");
        sb.AppendLine("    // AI agent: add [SerializeField] GameEvent fields");
        sb.AppendLine();
        sb.AppendLine($"    private {name}Hud _hud;");
        sb.AppendLine();
        sb.AppendLine("    public override IEnumerator Enter(IState previous)");
        sb.AppendLine("    {");
        sb.AppendLine("        yield return base.Enter(previous);");
        sb.AppendLine($"        _hud = gameHudInstance as {name}Hud;");
        sb.AppendLine("        if (_hud == null)");
        sb.AppendLine("        {");
        sb.AppendLine($"            Debug.LogError(\"[{name}State] HUD is not {name}Hud.\");");
        sb.AppendLine("            yield break;");
        sb.AppendLine("        }");
        sb.AppendLine("        // AI agent: subscribe HUD events");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public override IEnumerator Exit()");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_hud != null) { /* AI agent: unsubscribe */ _hud = null; }");
        sb.AppendLine("        yield return base.Exit();");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PROMPT FILE
    // ════════════════════════════════════════════════════════════════════════

    private void SavePromptFile(string folder, string name, string type, bool pausePrevious)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {name}");
        sb.AppendLine();
        sb.AppendLine($"- **Type:** {type}");
        sb.AppendLine($"- **Close Behaviour:** {_closePolicy}");
        sb.AppendLine($"- **Pause Previous:** {pausePrevious}");
        sb.AppendLine();
        if (_mockImage != null)
        {
            string ext = Path.GetExtension(AssetDatabase.GetAssetPath(_mockImage));
            sb.AppendLine("## Mock Reference");
            sb.AppendLine($"![Mock](Art/Mock{ext})");
            sb.AppendLine();
        }
        sb.AppendLine("## Description");
        sb.AppendLine(string.IsNullOrWhiteSpace(_aiPrompt) ? "_No prompt provided._" : _aiPrompt.Trim());
        sb.AppendLine();
        sb.AppendLine("## AI Agent Instructions");
        sb.AppendLine("1. Read this prompt and the mock image.");
        sb.AppendLine("2. Implement all `// AI agent:` TODOs in Scripts/.");
        sb.AppendLine("3. Named methods only — never lambdas in State.");
        sb.AppendLine("4. Wire prefab components after compilation.");
        File.WriteAllText($"{folder}/PROMPT.md", sb.ToString());
    }

    // ════════════════════════════════════════════════════════════════════════
    //  DELETE STATE
    // ════════════════════════════════════════════════════════════════════════

    private void DeleteState()
    {
        if (string.IsNullOrWhiteSpace(_deleteStateName))
        {
            EditorUtility.DisplayDialog("State Creator", "Enter the state name to delete.", "OK");
            return;
        }
        string name   = _deleteStateName.Trim();
        string folder = $"Assets/Game/Screens/{name}";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            EditorUtility.DisplayDialog("State Creator", $"Not found:\n{folder}", "OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("Confirm Delete",
            $"Delete {folder}/ and all AFC wiring + pool entries?\nCannot be undone.",
            "Delete", "Cancel")) return;

        var report = new StringBuilder();
        report.AppendLine(UnpatchApplicationFlowController(name) ? "AFC script \u2713" : "AFC script — not found");
        report.AppendLine(UnpatchAfcPrefab(name) ? "AFC prefab \u2713" : "AFC prefab — not found");
        var pooler = FindStatesPooler();
        if (pooler != null)
        {
            int n = RemovePoolEntries(pooler, name);
            report.AppendLine(n > 0 ? $"Pool: {n} entries removed \u2713" : "Pool: none found");
        }
        AssetDatabase.DeleteAsset(folder);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        report.AppendLine($"\n{folder}/ deleted \u2713");
        EditorUtility.DisplayDialog("Deleted", report.ToString(), "OK");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  AFC — SCRIPT PATCHER
    // ════════════════════════════════════════════════════════════════════════

    private static bool PatchApplicationFlowController(string name,
        ClosePolicy closePolicy = ClosePolicy.Default, bool pausePrevious = false)
    {
        if (!File.Exists(AfcPath)) return false;
        string src = File.ReadAllText(AfcPath);
        if (src.Contains($"_goTo{name}Event")) return true;

        const string bootMarker = "    public void Boot()";
        if (!src.Contains(bootMarker)) return false;

        string fields =
            $"\n    [Header(\"{name}\")]\n" +
            $"    [SerializeField] private GameEvent _goTo{name}Event;\n" +
            $"    [SerializeField] private Transition _goTo{name}Transition;\n";
        src = src.Replace(bootMarker, fields + bootMarker);
        src = InsertBeforeMethodClose(src, "protected override void RegisterFlowEvents()",
            $"        _goTo{name}Event.Subscribe(HandleGoTo{name});\n");
        src = InsertBeforeMethodClose(src, "protected override void UnregisterFlowEvents()",
            $"        _goTo{name}Event.UnSubscribe(HandleGoTo{name});\n");

        // Derive the correct UICloseReasons from Close Policy + Pause Previous:
        //   ClearAll                    → Home  (clears entire stack)
        //   PopOne                      → ResumeGame (exits current, resumes previous)
        //   Default + pausePrevious     → FullScreenPlacement (pushes on top, pauses previous)
        //   Default + no pause          → FullScreenPlacement without pause flag — but closest safe default
        string reason;
        if (closePolicy == ClosePolicy.ClearAll)
            reason = "Home";
        else if (closePolicy == ClosePolicy.PopOne)
            reason = "ResumeGame";
        else if (pausePrevious)
            reason = "FullScreenPlacement";
        else
            reason = "FullScreenPlacement";

        int lb = src.LastIndexOf('}');
        if (lb < 0) return false;
        src = src.Substring(0, lb) +
              $"\n    private void HandleGoTo{name}() =>\n" +
              $"        GoTo(_goTo{name}Transition, UICloseReasons.{reason});\n" +
              "}\n";
        File.WriteAllText(AfcPath, src);
        return true;
    }

    private static bool UnpatchApplicationFlowController(string name)
    {
        if (!File.Exists(AfcPath)) return false;
        string src = File.ReadAllText(AfcPath), before = src;
        src = Regex.Replace(src,
            $@"\r?\n\s*\[Header\(""{Regex.Escape(name)}""\)\]\s*\r?\n" +
            $@"\s*\[SerializeField\][^\r\n]*_goTo{Regex.Escape(name)}Event;[^\r\n]*\r?\n" +
            $@"\s*\[SerializeField\][^\r\n]*_goTo{Regex.Escape(name)}Transition;[^\r\n]*\r?\n?", "\n");
        src = Regex.Replace(src, $@"[^\S\r\n]*_goTo{Regex.Escape(name)}Event\.Subscribe[^\r\n]*\r?\n",   "");
        src = Regex.Replace(src, $@"[^\S\r\n]*_goTo{Regex.Escape(name)}Event\.UnSubscribe[^\r\n]*\r?\n", "");
        src = Regex.Replace(src, $@"\r?\n\s*private void HandleGoTo{Regex.Escape(name)}\(\)[^}}]*?;\s*\r?\n", "\n");
        if (src == before) return false;
        File.WriteAllText(AfcPath, src);
        return true;
    }

    private static string InsertBeforeMethodClose(string src, string methodSig, string insertion)
    {
        int idx = src.IndexOf(methodSig, StringComparison.Ordinal);
        if (idx < 0) return src;
        int open = src.IndexOf('{', idx);
        if (open < 0) return src;
        int depth = 1, pos = open + 1;
        while (pos < src.Length && depth > 0)
        {
            if (src[pos] == '{') depth++;
            else if (src[pos] == '}') depth--;
            pos++;
        }
        return src.Substring(0, pos - 1) + insertion + src.Substring(pos - 1);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  AFC — PREFAB YAML PATCHER
    // ════════════════════════════════════════════════════════════════════════

    private static bool PatchAfcPrefab(string name, string eventPath, string transPath)
    {
        if (!File.Exists(AfcPrefab)) return false;
        string eg = AssetDatabase.AssetPathToGUID(eventPath);
        string tg = AssetDatabase.AssetPathToGUID(transPath);
        if (string.IsNullOrEmpty(eg) || string.IsNullOrEmpty(tg)) return false;
        string yaml = File.ReadAllText(AfcPrefab);
        if (yaml.Contains($"_goTo{name}Event:")) return true;
        yaml  = yaml.TrimEnd();
        yaml += $"\n  _goTo{name}Event: {{fileID: 11400000, guid: {eg}, type: 2}}";
        yaml += $"\n  _goTo{name}Transition: {{fileID: 11400000, guid: {tg}, type: 2}}\n";
        File.WriteAllText(AfcPrefab, yaml);
        return true;
    }

    private static bool UnpatchAfcPrefab(string name)
    {
        if (!File.Exists(AfcPrefab)) return false;
        string yaml = File.ReadAllText(AfcPrefab), before = yaml;
        yaml = Regex.Replace(yaml, $@"\r?\n\s*_goTo{Regex.Escape(name)}Event:[^\r\n]*",      "");
        yaml = Regex.Replace(yaml, $@"\r?\n\s*_goTo{Regex.Escape(name)}Transition:[^\r\n]*", "");
        if (yaml == before) return false;
        File.WriteAllText(AfcPrefab, yaml);
        return true;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  POOL HELPERS
    // ════════════════════════════════════════════════════════════════════════

    private static void AddPoolEntry(PoolManagerSO pooler, string id, GameObject prefab)
    {
        var so      = new SerializedObject(pooler);
        var configs = so.FindProperty("_poolConfigs");
        configs.InsertArrayElementAtIndex(configs.arraySize);
        var e = configs.GetArrayElementAtIndex(configs.arraySize - 1);
        e.FindPropertyRelative("PoolID").stringValue          = id;
        e.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
        e.FindPropertyRelative("DefaultCapacity").intValue    = 1;
        e.FindPropertyRelative("MaxSize").intValue            = 1;
        e.FindPropertyRelative("Prewarm").boolValue           = false;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(pooler);
    }

    private static int RemovePoolEntries(PoolManagerSO pooler, string name)
    {
        var so      = new SerializedObject(pooler);
        var configs = so.FindProperty("_poolConfigs");
        int removed = 0;
        for (int i = configs.arraySize - 1; i >= 0; i--)
        {
            string id = configs.GetArrayElementAtIndex(i).FindPropertyRelative("PoolID").stringValue;
            if (id == name || id == $"{name}Hud") { configs.DeleteArrayElementAtIndex(i); removed++; }
        }
        if (removed > 0) { so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(pooler); }
        return removed;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  AI IMPLEMENTATION
    // ════════════════════════════════════════════════════════════════════════

    private async Task RunAIImplementation()
    {
        string name       = _screenName.Trim();
        string folder     = $"Assets/Game/Screens/{name}";
        string scriptsDir = $"{folder}/Scripts";

        _isGenerating = true; _generationLog = ""; _genProgress = 0f;
        Repaint();

        try
        {
            string apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            if (string.IsNullOrEmpty(apiKey)) apiKey = EditorPrefs.GetString("AnthropicApiKey", "");
            if (string.IsNullOrEmpty(apiKey))
            {
                AppendLog("ERROR: No API key. Set ANTHROPIC_API_KEY env var or EditorPrefs 'AnthropicApiKey'.");
                return;
            }

            AppendLog("Reading stubs...");
            _genProgress = 0.1f;
            var stubs = new StringBuilder();
            if (Directory.Exists(scriptsDir))
                foreach (var f in Directory.GetFiles(scriptsDir, "*.cs"))
                {
                    stubs.AppendLine($"// --- {Path.GetFileName(f)} ---");
                    stubs.AppendLine(File.ReadAllText(f));
                    stubs.AppendLine();
                }

            string promptContent = File.Exists($"{folder}/PROMPT.md")
                ? File.ReadAllText($"{folder}/PROMPT.md") : "(No PROMPT.md)";

            string mockB64 = null, mockMime = null;
            if (Directory.Exists($"{folder}/Art"))
                foreach (var img in Directory.GetFiles($"{folder}/Art"))
                {
                    string ext = Path.GetExtension(img).ToLowerInvariant();
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                    {
                        AppendLog($"Encoding: {Path.GetFileName(img)}");
                        mockB64  = Convert.ToBase64String(File.ReadAllBytes(img));
                        mockMime = ext == ".png" ? "image/png" : "image/jpeg";
                        break;
                    }
                }
            _genProgress = 0.2f; Repaint();

            var msgContent = new JArray();
            msgContent.Add(new JObject { ["type"] = "text", ["text"] =
                $"Screen: {name}\n\nDescription:\n{promptContent}\n\nStubs:\n{stubs}" });
            if (mockB64 != null)
                msgContent.Add(new JObject
                {
                    ["type"]   = "image",
                    ["source"] = new JObject { ["type"] = "base64", ["media_type"] = mockMime, ["data"] = mockB64 }
                });
            msgContent.Add(new JObject { ["type"] = "text", ["text"] =
                "Implement all three files. Replace every // AI agent: comment with real code." });

            var body = new JObject
            {
                ["model"]      = "claude-sonnet-4-6",
                ["max_tokens"] = 8192,
                ["system"]     = BuildAISystemPrompt(name),
                ["messages"]   = new JArray { new JObject { ["role"] = "user", ["content"] = msgContent } }
            };

            AppendLog("Calling Claude API...");
            _genProgress = 0.3f; Repaint();

            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            req.Headers.Add("x-api-key", apiKey);
            req.Headers.Add("anthropic-version", "2023-06-01");
            req.Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");

            var resp     = await _httpClient.SendAsync(req);
            string rBody = await resp.Content.ReadAsStringAsync();
            _genProgress = 0.7f;

            if (!resp.IsSuccessStatusCode) { AppendLog($"ERROR {(int)resp.StatusCode}:\n{rBody}"); return; }
            AppendLog($"Response: {rBody.Length} chars");

            var sb2 = new StringBuilder();
            foreach (var block in JObject.Parse(rBody)["content"] as JArray ?? new JArray())
                if (block["type"]?.ToString() == "text") sb2.Append(block["text"]);

            var files = ParseGeneratedFiles(sb2.ToString());
            if (files.Count == 0)
            {
                AppendLog("No files parsed. Preview:\n" + sb2.ToString().Substring(0, Math.Min(500, sb2.Length)));
                return;
            }

            foreach (var kvp in files)
            {
                File.WriteAllText($"{scriptsDir}/{kvp.Key}", kvp.Value);
                AppendLog($"Wrote {kvp.Key}");
            }

            _genProgress = 1f;
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("AI Done", $"{files.Count} file(s) written.\nRecompile then wire prefab.", "OK");
            };
            AppendLog("Done!");
        }
        catch (Exception ex) { AppendLog($"ERROR: {ex.Message}"); }
        finally { _isGenerating = false; Repaint(); }
    }

    private void AppendLog(string msg) { _generationLog += $"[{DateTime.Now:HH:mm:ss}] {msg}\n"; Repaint(); }

    private static Dictionary<string, string> ParseGeneratedFiles(string response)
    {
        var files   = new Dictionary<string, string>();
        var matches = Regex.Matches(response,
            @"//\s*FILE:\s*(\S+\.cs)\s*\n(.*?)//\s*END\s*FILE", RegexOptions.Singleline);
        foreach (Match m in matches)
            files[m.Groups[1].Value.Trim()] = m.Groups[2].Value.Trim() + "\n";
        return files;
    }

    private static string BuildAISystemPrompt(string name) =>
        "You are a Unity C# code generator for a ScriptableObject state machine architecture.\n\n" +
        "Patterns:\n" +
        "- UIViewState.Enter(): base.Enter() then GetView<TView>()\n" +
        "- UIBase: [SerializeField] refs, public event Actions, SetData() + Awake()\n" +
        "- State: ALWAYS named methods, NEVER lambdas. SubscribeEvents()/UnsubscribeEvents()\n" +
        "- ViewData: plain [Serializable] struct — only primitives/enums, NO Unity types\n" +
        "- Namespaces: Blues.Core.UI, Blues.Core.Events, Blues.Core.StateMachine\n\n" +
        "Return ONLY:\n" +
        $"// FILE: {name}UIView.cs\n<code>\n// END FILE\n\n" +
        $"// FILE: {name}ViewData.cs\n<code>\n// END FILE\n\n" +
        $"// FILE: {name}State.cs\n<code>\n// END FILE";

    // ════════════════════════════════════════════════════════════════════════
    //  VALIDATION
    // ════════════════════════════════════════════════════════════════════════

    private bool ValidateName(out string name, out string folder)
    {
        name = ""; folder = "";
        if (string.IsNullOrWhiteSpace(_screenName))
        {
            EditorUtility.DisplayDialog("State Creator", "Screen Name cannot be empty.", "OK");
            return false;
        }
        if (!Regex.IsMatch(_screenName.Trim(), @"^[A-Z][a-zA-Z0-9]*$"))
        {
            EditorUtility.DisplayDialog("State Creator",
                $"'{_screenName}' is not valid PascalCase.", "OK");
            return false;
        }
        name   = _screenName.Trim();
        folder = $"Assets/Game/Screens/{name}";
        if (AssetDatabase.IsValidFolder(folder))
        {
            EditorUtility.DisplayDialog("State Creator",
                $"Folder already exists:\n{folder}\n\nDelete it first.", "OK");
            return false;
        }
        return true;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GUI HELPERS
    // ════════════════════════════════════════════════════════════════════════

    private void DrawTitleBar()
    {
        Rect r = EditorGUILayout.GetControlRect(false, 48);
        EditorGUI.DrawRect(r, HeaderBarColor);
        EditorGUI.DrawRect(new Rect(r.x, r.y, 4, r.height), AccentBlue);
        GUI.Label(new Rect(r.x+14, r.y+4,  r.width-20, 26), "State Creator",                                       _titleStyle);
        GUI.Label(new Rect(r.x+14, r.y+26, r.width-20, 18), "AI-assisted state scaffolding for your game architecture", _subtitleStyle);
    }

    private static void DrawSectionHeader(string label, Color accent)
    {
        Rect r = EditorGUILayout.GetControlRect(false, 22);
        EditorGUI.DrawRect(new Rect(r.x, r.y+2, 3, r.height-4), accent);
        GUI.Label(new Rect(r.x+10, r.y, r.width-10, r.height), label,
            new GUIStyle(EditorStyles.boldLabel) { fontSize = 12, normal = { textColor = accent } });
    }

    private void DrawInfoTag(string text, Color bg)
    {
        Rect r = EditorGUILayout.GetControlRect(false, 18);
        Rect t = new Rect(r.x+4, r.y, r.width-8, r.height);
        EditorGUI.DrawRect(t, new Color(bg.r, bg.g, bg.b, 0.18f));
        GUI.Label(t, text, _tagStyle);
    }

    private void DrawPathPreview(string name) =>
        EditorGUILayout.LabelField($"\u2192  Assets/Game/Screens/{name}/",
            new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize  = 10, fontStyle = FontStyle.Italic,
                normal    = { textColor = new Color(0.55f, 0.75f, 0.55f) }
            });

    private static void BeginSection()  => EditorGUILayout.BeginVertical("box");
    private static void EndSection()    => EditorGUILayout.EndVertical();
    private static void DrawSeparator()
    {
        EditorGUILayout.Space(4);
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), SeparatorColor);
        EditorGUILayout.Space(4);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  TYPE / ASSET RESOLVERS
    // ════════════════════════════════════════════════════════════════════════

    private static Type FindTypeByName(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(typeName);
            if (t != null) return t;
        }
        return null;
    }

    private static PoolManagerSO FindStatesPooler()
    {
        foreach (string guid in AssetDatabase.FindAssets("StatesPooler t:PoolManagerSO"))
        {
            var p = AssetDatabase.LoadAssetAtPath<PoolManagerSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (p != null) return p;
        }
        var guids = AssetDatabase.FindAssets("t:PoolManagerSO");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var p = AssetDatabase.LoadAssetAtPath<PoolManagerSO>(path);
            if (p != null) { Debug.LogWarning($"[StateCreator] Using fallback pooler: {path}"); return p; }
        }
        Debug.LogError("[StateCreator] No PoolManagerSO found. Assign uIStatesPooler manually.");
        return null;
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(
                Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "Assets",
                Path.GetFileName(path));
    }
}