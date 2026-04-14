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
/// State Creator — Tools ▸ State Creator
///
/// Simplified creation flow:
///   1. Pick state type (UI / Game) and close behaviour
///   2. Optionally describe the state in the AI Prompt + attach a mock image
///   3. Hit CREATE — tool generates clean folder structure, dummy prefabs,
///      minimal scaffolding scripts, SO assets, and wires ApplicationFlowController.
///   4. AI agent reads PROMPT.md (+ mock image) and implements the real functionality.
///
/// Folder layout per screen:
///   Assets/Game/Screens/{Name}/
///     ├── Prefabs/        — dummy view / hud / gameplay prefabs
///     ├── Art/            — mock image, sprites (added later)
///     ├── Scripts/        — State, View/Hud, ViewData
///     └── Config/         — SO assets (Event, State, Transition, variables)
/// </summary>
public class StateCreatorWindow : EditorWindow
{
    // ── Enums ──────────────────────────────────────────────────────────────
    private enum StateType { UIState, GameState }

    // ── Input fields ───────────────────────────────────────────────────────
    private string      _screenName   = "";
    private StateType   _stateType    = StateType.UIState;
    private ClosePolicy _closePolicy  = ClosePolicy.Default;
    private bool        _pausePreviousState = false;
    private bool        _useDefaultAnimations = true;
    private string      _aiPrompt     = "";
    private Texture2D   _mockImage;

    // ── AI Generation ────────────────────────────────────────────────────
    private static readonly HttpClient _httpClient = new HttpClient();
    private bool _isGenerating;
    private string _generationLog = "";
    private Vector2 _logScroll;
    private float _genProgress;

    // ── Delete ─────────────────────────────────────────────────────────────
    private string _deleteStateName = "";

    // ── Constants ──────────────────────────────────────────────────────────
    private const string PoolerPath = "Assets/Saad/Utilities/PoolSystem/Config/StatesPooler.asset";
    private const string AfcPath    = "Assets/Saad/GameFlow/Scripts/ApplicationFlowController.cs";
    private const string AfcPrefab  = "Assets/Saad/GameFlow/Prefabs/Resources/ApplicationFlowController.prefab";

    private Vector2 _scrollPos;
    private bool _showDeleteSection;

    // ── Cached Styles ───────────────────────────────────────────────────
    private GUIStyle _headerStyle;
    private GUIStyle _sectionBoxStyle;
    private GUIStyle _titleStyle;
    private GUIStyle _subtitleStyle;
    private GUIStyle _promptTextStyle;
    private GUIStyle _logTextStyle;
    private GUIStyle _bigButtonStyle;
    private GUIStyle _tagStyle;
    private bool _stylesInitialized;

    // ── Theme Colors ────────────────────────────────────────────────────
    private static readonly Color AccentBlue      = new Color(0.30f, 0.55f, 0.95f);
    private static readonly Color AccentGreen     = new Color(0.25f, 0.75f, 0.40f);
    private static readonly Color AccentRed       = new Color(0.90f, 0.30f, 0.30f);
    private static readonly Color AccentPurple    = new Color(0.60f, 0.40f, 0.90f);
    private static readonly Color SectionBg       = new Color(0.22f, 0.22f, 0.22f, 0.60f);
    private static readonly Color SectionBgLight  = new Color(0.80f, 0.80f, 0.80f, 0.15f);
    private static readonly Color SeparatorColor  = new Color(0.45f, 0.45f, 0.45f, 0.35f);
    private static readonly Color HeaderBarColor  = new Color(0.18f, 0.18f, 0.22f, 0.90f);

    // ── Menu item ──────────────────────────────────────────────────────────
    [MenuItem("Tools/State Creator")]
    public static void ShowWindow()
    {
        var w = GetWindow<StateCreatorWindow>("State Creator");
        w.minSize = new Vector2(380, 520);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  STYLE INITIALIZATION
    // ════════════════════════════════════════════════════════════════════════

    private void InitStyles()
    {
        if (_stylesInitialized) return;

        _titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 18,
            alignment = TextAnchor.MiddleLeft,
            padding   = new RectOffset(6, 0, 0, 0),
            normal    = { textColor = Color.white }
        };

        _subtitleStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize  = 10,
            alignment = TextAnchor.MiddleLeft,
            padding   = new RectOffset(8, 0, 0, 0),
            normal    = { textColor = new Color(0.65f, 0.65f, 0.70f) }
        };

        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 12,
            padding   = new RectOffset(4, 0, 2, 2),
            normal    = { textColor = new Color(0.85f, 0.85f, 0.90f) }
        };

        _sectionBoxStyle = new GUIStyle("box")
        {
            padding = new RectOffset(12, 12, 10, 10),
            margin  = new RectOffset(4, 4, 2, 2)
        };

        _promptTextStyle = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap  = true,
            fontSize  = 12,
            padding   = new RectOffset(8, 8, 8, 8)
        };

        _logTextStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
        {
            fontSize  = 10,
            richText  = true,
            padding   = new RectOffset(6, 6, 4, 4),
            normal    = { textColor = new Color(0.70f, 0.80f, 0.70f) }
        };

        _bigButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize  = 13,
            fontStyle = FontStyle.Bold,
            fixedHeight = 38,
            margin    = new RectOffset(4, 4, 4, 4)
        };

        _tagStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize  = 9,
            alignment = TextAnchor.MiddleCenter,
            padding   = new RectOffset(6, 6, 2, 2),
            fontStyle = FontStyle.Bold,
            normal    = { textColor = Color.white }
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

        // ╔══════════════════════════════════════╗
        //  TITLE BAR
        // ╚══════════════════════════════════════╝
        DrawTitleBar();
        EditorGUILayout.Space(8);

        // ╔══════════════════════════════════════╗
        //  SECTION 1 — STATE CONFIGURATION
        // ╚══════════════════════════════════════╝
        BeginSection();
        DrawSectionHeader("\u2699  Configuration", AccentBlue);
        EditorGUILayout.Space(6);

        // State Type Toolbar
        EditorGUILayout.LabelField("State Type", _headerStyle);
        EditorGUILayout.Space(2);

        var prevBg = GUI.backgroundColor;
        GUI.backgroundColor = _stateType == StateType.UIState ? AccentBlue : AccentPurple;
        _stateType = (StateType)GUILayout.Toolbar((int)_stateType,
            new[] { "\u25A3  UI State", "\u25B6  Game State" },
            GUILayout.Height(28));
        GUI.backgroundColor = prevBg;
        EditorGUILayout.Space(4);

        // Type description tag
        string typeDesc = _stateType == StateType.UIState
            ? "UIViewState + UIView + ViewData + Transition + Prefab"
            : "GameState + GameHud + Transition + Gameplay & HUD Prefabs";
        DrawInfoTag(typeDesc, _stateType == StateType.UIState ? AccentBlue : AccentPurple);
        EditorGUILayout.Space(10);

        // Screen Name
        EditorGUILayout.LabelField("Screen Name", _headerStyle);
        EditorGUILayout.Space(2);
        _screenName = EditorGUILayout.TextField(_screenName);
        if (!string.IsNullOrWhiteSpace(_screenName))
        {
            string preview = _screenName.Trim();
            EditorGUILayout.Space(2);
            DrawPathPreview(preview);
        }
        EditorGUILayout.Space(8);

        // Close Behaviour + Options in two columns
        EditorGUILayout.LabelField("Behaviour", _headerStyle);
        EditorGUILayout.Space(2);
        _closePolicy = (ClosePolicy)EditorGUILayout.EnumPopup("Close Policy", _closePolicy);
        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();
        _pausePreviousState   = EditorGUILayout.ToggleLeft("  Pause Previous", _pausePreviousState, GUILayout.Width(140));
        _useDefaultAnimations = EditorGUILayout.ToggleLeft("  Default Animations", _useDefaultAnimations);
        EditorGUILayout.EndHorizontal();

        EndSection();
        EditorGUILayout.Space(6);

        // ╔══════════════════════════════════════╗
        //  SECTION 2 — AI PROMPT
        // ╚══════════════════════════════════════╝
        BeginSection();
        DrawSectionHeader("\u2728  AI Agent Prompt", AccentPurple);
        EditorGUILayout.Space(4);

        EditorGUILayout.LabelField(
            "Describe buttons, data, transitions, behaviour. Saved as PROMPT.md.",
            _subtitleStyle);
        EditorGUILayout.Space(4);
        _aiPrompt = EditorGUILayout.TextArea(_aiPrompt, _promptTextStyle, GUILayout.MinHeight(80));
        EditorGUILayout.Space(8);

        // Mock Image
        EditorGUILayout.LabelField("Mock / Reference Image", _headerStyle);
        EditorGUILayout.Space(2);
        _mockImage = (Texture2D)EditorGUILayout.ObjectField(
            _mockImage, typeof(Texture2D), false, GUILayout.Height(48));
        if (_mockImage != null)
        {
            EditorGUILayout.Space(2);
            DrawInfoTag("Will be copied to Art/ and referenced in PROMPT.md", new Color(0.5f, 0.5f, 0.5f));
        }
        EndSection();
        EditorGUILayout.Space(10);

        // ╔══════════════════════════════════════╗
        //  ACTION BUTTONS
        // ╚══════════════════════════════════════╝
        string btnLabel = _stateType == StateType.UIState
            ? "\u25B6  CREATE UI STATE"
            : "\u25B6  CREATE GAME STATE";

        GUI.backgroundColor = AccentGreen;
        if (GUILayout.Button(btnLabel, _bigButtonStyle))
        {
            if (_stateType == StateType.UIState) CreateUIState();
            else CreateGameState();
        }
        GUI.backgroundColor = prevBg;

        // Implement with AI — only if folder exists
        string aiCheckName   = _screenName?.Trim();
        string aiCheckFolder = !string.IsNullOrWhiteSpace(aiCheckName)
            ? $"Assets/Game/Screens/{aiCheckName}" : null;

        if (aiCheckFolder != null && AssetDatabase.IsValidFolder(aiCheckFolder))
        {
            EditorGUILayout.Space(4);
            GUI.backgroundColor = AccentPurple;
            EditorGUI.BeginDisabledGroup(_isGenerating);
            if (GUILayout.Button("\u2726  Implement with AI", _bigButtonStyle))
            {
                _ = RunAIImplementation();
            }
            EditorGUI.EndDisabledGroup();
            GUI.backgroundColor = prevBg;
        }

        // Progress Bar
        if (_isGenerating)
        {
            EditorGUILayout.Space(6);
            Rect progressRect = EditorGUILayout.GetControlRect(false, 22);
            EditorGUI.ProgressBar(progressRect, _genProgress, "Generating...");
        }

        // AI Log
        if (!string.IsNullOrEmpty(_generationLog))
        {
            EditorGUILayout.Space(6);
            BeginSection();
            DrawSectionHeader("\u2263  Generation Log", new Color(0.5f, 0.7f, 0.5f));
            EditorGUILayout.Space(2);
            _logScroll = EditorGUILayout.BeginScrollView(_logScroll, GUILayout.MaxHeight(180));
            EditorGUILayout.LabelField(_generationLog, _logTextStyle);
            EditorGUILayout.EndScrollView();
            EndSection();
        }

        EditorGUILayout.Space(16);

        // ╔══════════════════════════════════════╗
        //  SECTION 3 — DANGER ZONE
        // ╚══════════════════════════════════════╝
        DrawSeparator();
        _showDeleteSection = EditorGUILayout.Foldout(_showDeleteSection, "\u26A0  Danger Zone", true,
            new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold,
                normal = { textColor = AccentRed }, onNormal = { textColor = AccentRed } });

        if (_showDeleteSection)
        {
            BeginSection();
            EditorGUILayout.LabelField(
                "Removes folder, AFC wiring, and pool entries. This cannot be undone.",
                _subtitleStyle);
            EditorGUILayout.Space(6);
            _deleteStateName = EditorGUILayout.TextField("State Name", _deleteStateName);
            EditorGUILayout.Space(6);
            GUI.backgroundColor = AccentRed;
            if (GUILayout.Button("\u2716  DELETE STATE", _bigButtonStyle))
                DeleteState();
            GUI.backgroundColor = prevBg;
            EndSection();
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.EndScrollView();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GUI HELPERS
    // ════════════════════════════════════════════════════════════════════════

    private void DrawTitleBar()
    {
        Rect titleRect = EditorGUILayout.GetControlRect(false, 48);
        EditorGUI.DrawRect(titleRect, HeaderBarColor);

        // Accent strip on left
        Rect strip = new Rect(titleRect.x, titleRect.y, 4, titleRect.height);
        EditorGUI.DrawRect(strip, AccentBlue);

        // Title text
        Rect titleTextRect = new Rect(titleRect.x + 14, titleRect.y + 4, titleRect.width - 20, 26);
        GUI.Label(titleTextRect, "State Creator", _titleStyle);

        // Subtitle
        Rect subtitleRect = new Rect(titleRect.x + 14, titleRect.y + 26, titleRect.width - 20, 18);
        GUI.Label(subtitleRect, "AI-assisted state scaffolding for your game architecture", _subtitleStyle);
    }

    private static void DrawSectionHeader(string label, Color accentColor)
    {
        Rect r = EditorGUILayout.GetControlRect(false, 22);

        // Accent bar
        Rect bar = new Rect(r.x, r.y + 2, 3, r.height - 4);
        EditorGUI.DrawRect(bar, accentColor);

        // Label
        Rect labelRect = new Rect(r.x + 10, r.y, r.width - 10, r.height);
        var style = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            normal = { textColor = accentColor }
        };
        GUI.Label(labelRect, label, style);
    }

    private void DrawInfoTag(string text, Color bgColor)
    {
        Rect r = EditorGUILayout.GetControlRect(false, 18);
        Rect tagRect = new Rect(r.x + 4, r.y, r.width - 8, r.height);
        Color dimmed = new Color(bgColor.r, bgColor.g, bgColor.b, 0.15f);
        EditorGUI.DrawRect(tagRect, dimmed);
        GUI.Label(tagRect, text, _tagStyle);
    }

    private void DrawPathPreview(string name)
    {
        string path = $"Assets/Game/Screens/{name}/";
        var style = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize  = 10,
            fontStyle = FontStyle.Italic,
            normal    = { textColor = new Color(0.55f, 0.75f, 0.55f) }
        };
        EditorGUILayout.LabelField($"\u2192  {path}", style);
    }

    private static void BeginSection()
    {
        EditorGUILayout.BeginVertical("box");
    }

    private static void EndSection()
    {
        EditorGUILayout.EndVertical();
    }

    private static void DrawSeparator()
    {
        EditorGUILayout.Space(4);
        Rect r = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(r, SeparatorColor);
        EditorGUILayout.Space(4);
    }

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
                $"'{_screenName}' is not valid PascalCase.\nUse letters/digits only, start with uppercase.", "OK");
            return false;
        }

        name   = _screenName.Trim();
        folder = $"Assets/Game/Screens/{name}";

        if (AssetDatabase.IsValidFolder(folder))
        {
            EditorUtility.DisplayDialog("State Creator",
                $"Folder already exists:\n{folder}\n\nDelete it first or choose a different name.", "OK");
            return false;
        }

        return true;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  UI STATE CREATION
    // ════════════════════════════════════════════════════════════════════════

    private void CreateUIState()
    {
        if (!ValidateName(out string name, out string folder)) return;

        // 1. Create folder structure
        CreateFolderStructure(folder, name);

        // 2. Scripts
        string scriptsDir = $"{folder}/Scripts";
        File.WriteAllText($"{scriptsDir}/{name}ViewData.cs",    BuildViewData(name));
        File.WriteAllText($"{scriptsDir}/{name}UIView.cs",      BuildView(name));
        File.WriteAllText($"{scriptsDir}/{name}State.cs",       BuildUIStateScript(name));
        File.WriteAllText($"{scriptsDir}/{name}Transition.cs",  BuildTransition(name));

        AssetDatabase.Refresh();

        // 3. Dummy prefab
        string prefabPath = $"{folder}/Prefabs/{name}.prefab";
        GameObject dummyPrefab = CreateUIDummyPrefab(name, prefabPath);

        // 4. SO assets (in Config/)
        string configDir = $"{folder}/Config";

        var goToEvent = ScriptableObject.CreateInstance<GameEvent>();
        AssetDatabase.CreateAsset(goToEvent, $"{configDir}/GoTo{name}Event.asset");

        var stateAsset = ScriptableObject.CreateInstance<UIViewState>();
        AssetDatabase.CreateAsset(stateAsset, $"{configDir}/{name}State.asset");

        var pooler = AssetDatabase.LoadAssetAtPath<PoolManagerSO>(PoolerPath);
        var stateSO = new SerializedObject(stateAsset);
        stateSO.FindProperty("stateId").stringValue              = name;
        stateSO.FindProperty("usePooling").boolValue             = true;
        stateSO.FindProperty("PausePreviousState").boolValue     = _pausePreviousState;
        stateSO.FindProperty("useDefaultAnimations").boolValue   = _useDefaultAnimations;
        if (pooler != null)
            stateSO.FindProperty("uIStatesPooler").objectReferenceValue = pooler;
        stateSO.ApplyModifiedPropertiesWithoutUndo();

        // Try typed transition first; fall back to base Transition if not yet compiled
        var transitionType = FindTypeByName($"{name}Transition");
        Transition transition;
        if (transitionType != null && typeof(Transition).IsAssignableFrom(transitionType))
            transition = (Transition)ScriptableObject.CreateInstance(transitionType);
        else
            transition = ScriptableObject.CreateInstance<Transition>();
        transition.ToState     = stateAsset;
        transition.closePolicy = _closePolicy;
        AssetDatabase.CreateAsset(transition, $"{configDir}/GoTo{name}Transition.asset");

        // 5. Pool registration
        bool poolWired = false;
        if (pooler != null && dummyPrefab != null)
        {
            AddPoolEntry(pooler, name, dummyPrefab);
            poolWired = true;
        }

        // 6. Wire ApplicationFlowController (script + prefab)
        bool flowUpdated  = PatchApplicationFlowController(name);
        bool prefabWired  = PatchAfcPrefab(name,
            $"{configDir}/GoTo{name}Event.asset",
            $"{configDir}/GoTo{name}Transition.asset");

        // 7. Save prompt
        SavePromptFile(folder, name, "UIState", _pausePreviousState);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 8. Report
        var sb = new StringBuilder();
        sb.AppendLine($"UI State '{name}' created!\n");
        sb.AppendLine("Folder structure:");
        sb.AppendLine($"  {folder}/");
        sb.AppendLine($"    Prefabs/{name}.prefab (dummy)");
        sb.AppendLine($"    Scripts/{name}State.cs, {name}UIView.cs, {name}ViewData.cs, {name}Transition.cs");
        sb.AppendLine($"    Config/GoTo{name}Event, {name}State, GoTo{name}Transition");
        sb.AppendLine($"    Art/");
        sb.AppendLine($"    PROMPT.md");
        sb.AppendLine();
        sb.AppendLine(poolWired  ? "StatesPooler: wired ✓" : "⚠ StatesPooler: not wired");
        sb.AppendLine(flowUpdated ? "AFC script: wired ✓" : "⚠ AFC script: not wired");
        sb.AppendLine(prefabWired ? "AFC prefab: references assigned ✓" : "⚠ AFC prefab: assign manually");
        sb.AppendLine();
        sb.AppendLine("After recompile:");
        sb.AppendLine($"  1. Set Script field on {name}State.asset → {name}State");
        sb.AppendLine($"  2. Add {name}UIView component to the dummy prefab");
        if (!string.IsNullOrWhiteSpace(_aiPrompt))
            sb.AppendLine($"  3. Ask AI agent to implement (reads PROMPT.md)");
        sb.AppendLine();
        sb.AppendLine("Implementation prompt copied to clipboard — paste into Claude Code");
        CopyImplementationPrompt(name, folder);
        EditorUtility.DisplayDialog("State Creator — Done!", sb.ToString(), "OK");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GAME STATE CREATION
    // ════════════════════════════════════════════════════════════════════════

    private void CreateGameState()
    {
        if (!ValidateName(out string name, out string folder)) return;

        // 1. Folder structure
        CreateFolderStructure(folder, name);

        // 2. Scripts
        string scriptsDir = $"{folder}/Scripts";
        File.WriteAllText($"{scriptsDir}/{name}Hud.cs",          BuildGameHud(name));
        File.WriteAllText($"{scriptsDir}/{name}State.cs",        BuildGameStateScript(name));
        File.WriteAllText($"{scriptsDir}/{name}Transition.cs",   BuildTransition(name));

        AssetDatabase.Refresh();

        // 3. Dummy prefabs
        string hudPrefabPath      = $"{folder}/Prefabs/{name}Hud.prefab";
        string gameplayPrefabPath = $"{folder}/Prefabs/{name}Gameplay.prefab";
        GameObject hudPrefab      = CreateGameHudDummyPrefab(name, hudPrefabPath);
        GameObject gameplayPrefab = CreateGameplayDummyPrefab(name, gameplayPrefabPath);

        // 4. SO assets
        string configDir = $"{folder}/Config";

        var goToEvent = ScriptableObject.CreateInstance<GameEvent>();
        AssetDatabase.CreateAsset(goToEvent, $"{configDir}/GoTo{name}Event.asset");

        var stateAsset = ScriptableObject.CreateInstance<State>();
        AssetDatabase.CreateAsset(stateAsset, $"{configDir}/{name}State.asset");

        var stateSO = new SerializedObject(stateAsset);
        stateSO.FindProperty("PausePreviousState").boolValue = _pausePreviousState;
        stateSO.ApplyModifiedPropertiesWithoutUndo();

        // Try typed transition first; fall back to base Transition if not yet compiled
        var transitionType = FindTypeByName($"{name}Transition");
        Transition transition;
        if (transitionType != null && typeof(Transition).IsAssignableFrom(transitionType))
            transition = (Transition)ScriptableObject.CreateInstance(transitionType);
        else
            transition = ScriptableObject.CreateInstance<Transition>();
        transition.ToState     = stateAsset;
        transition.closePolicy = _closePolicy;
        AssetDatabase.CreateAsset(transition, $"{configDir}/GoTo{name}Transition.asset");

        // 5. Pool registration
        var pooler = AssetDatabase.LoadAssetAtPath<PoolManagerSO>(PoolerPath);
        bool gameplayPoolWired = false, hudPoolWired = false;
        if (pooler != null)
        {
            if (gameplayPrefab != null) { AddPoolEntry(pooler, name,           gameplayPrefab); gameplayPoolWired = true; }
            if (hudPrefab != null)      { AddPoolEntry(pooler, $"{name}Hud",   hudPrefab);      hudPoolWired = true; }
        }

        // 6. Wire ApplicationFlowController
        bool flowUpdated = PatchApplicationFlowController(name);
        bool prefabWired = PatchAfcPrefab(name,
            $"{configDir}/GoTo{name}Event.asset",
            $"{configDir}/GoTo{name}Transition.asset");

        // 7. Save prompt
        SavePromptFile(folder, name, "GameState", _pausePreviousState);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 8. Report
        var sb = new StringBuilder();
        sb.AppendLine($"Game State '{name}' created!\n");
        sb.AppendLine("Folder structure:");
        sb.AppendLine($"  {folder}/");
        sb.AppendLine($"    Prefabs/{name}Gameplay.prefab, {name}Hud.prefab (dummies)");
        sb.AppendLine($"    Scripts/{name}State.cs, {name}Hud.cs, {name}Transition.cs");
        sb.AppendLine($"    Config/GoTo{name}Event, {name}State, GoTo{name}Transition");
        sb.AppendLine($"    Art/");
        sb.AppendLine($"    PROMPT.md");
        sb.AppendLine();
        sb.AppendLine(gameplayPoolWired ? "Gameplay pool: wired ✓" : "⚠ Gameplay pool: not wired");
        sb.AppendLine(hudPoolWired      ? "HUD pool: wired ✓"     : "⚠ HUD pool: not wired");
        sb.AppendLine(flowUpdated       ? "AFC script: wired ✓"   : "⚠ AFC script: not wired");
        sb.AppendLine(prefabWired       ? "AFC prefab: wired ✓"   : "⚠ AFC prefab: assign manually");
        sb.AppendLine();
        sb.AppendLine("After recompile:");
        sb.AppendLine($"  1. Set Script field on {name}State.asset → {name}State");
        sb.AppendLine($"  2. Configure gameplayPrefabId=\"{name}\" hudPrefabId=\"{name}Hud\"");
        sb.AppendLine($"  3. Add {name}Hud component to the HUD prefab");
        if (!string.IsNullOrWhiteSpace(_aiPrompt))
            sb.AppendLine($"  4. Ask AI agent to implement (reads PROMPT.md)");
        sb.AppendLine();
        sb.AppendLine("Implementation prompt copied to clipboard — paste into Claude Code");
        CopyImplementationPrompt(name, folder);
        EditorUtility.DisplayDialog("State Creator — Done!", sb.ToString(), "OK");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CLIPBOARD PROMPT
    // ════════════════════════════════════════════════════════════════════════

    private static void CopyImplementationPrompt(string name, string folder)
    {
        string prompt =
            $"Read Assets/Game/Screens/{name}/PROMPT.md and the mock image at " +
            $"Assets/Game/Screens/{name}/Art/Mock.png\n\n" +
            "Then read these files for patterns:\n" +
            "- Assets/Saad/Utilities/StateMachine/Scripts/UiViewState.cs\n" +
            "- Assets/Saad/UI/MainMenu/Scripts/MainMenuView.cs\n" +
            "- Assets/Saad/UI/MainMenu/States&Transition/ (all files)\n" +
            "- Assets/Saad/Variables/DBBool.cs\n\n" +
            "Implement these three files completely, replacing all // AI agent: comments:\n" +
            $"- Assets/Game/Screens/{name}/Scripts/{name}UIView.cs\n" +
            $"- Assets/Game/Screens/{name}/Scripts/{name}ViewData.cs\n" +
            $"- Assets/Game/Screens/{name}/Scripts/{name}State.cs\n\n" +
            "Follow MainMenuView.cs style exactly. Named methods only, never lambdas.\n" +
            "After writing scripts, use MCP to open the prefab at " +
            $"Assets/Game/Screens/{name}/Prefabs/{name}.prefab, add the {name}UIView " +
            "component, create child GameObjects matching the mock layout, wire all " +
            "SerializeField references, save the prefab.";

        EditorGUIUtility.systemCopyBuffer = prompt;
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

        // Copy mock image to Art/ if provided
        if (_mockImage != null)
        {
            string srcPath = AssetDatabase.GetAssetPath(_mockImage);
            if (!string.IsNullOrEmpty(srcPath))
            {
                string ext  = Path.GetExtension(srcPath);
                string dest = $"{folder}/Art/Mock{ext}";
                AssetDatabase.CopyAsset(srcPath, dest);
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  DUMMY PREFAB CREATION
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Creates a Canvas prefab ready for a UIBase-derived View component.</summary>
    private static GameObject CreateUIDummyPrefab(string name, string path)
    {
        var root = new GameObject(name);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();
        root.AddComponent<CanvasGroup>();

        // Background panel
        var bg = new GameObject("Background");
        bg.transform.SetParent(root.transform, false);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.6f);

        // Content area
        var content = new GameObject("Content");
        content.transform.SetParent(root.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        // Best-effort: add view component if type is already compiled (e.g., second create cycle)
        var viewType = FindTypeByName($"{name}UIView");
        if (viewType != null)
        {
            root.AddComponent(viewType);
            Debug.Log($"[StateCreator] Added {name}UIView component to prefab.");
        }
        else
        {
            Debug.Log($"[StateCreator] {name}UIView type not compiled yet — add component after recompile.");
        }

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab;
    }

    /// <summary>Creates a Canvas prefab with Header / Middle / Footer for GameHud.</summary>
    private static GameObject CreateGameHudDummyPrefab(string name, string path)
    {
        var root = new GameObject($"{name}Hud");
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();
        root.AddComponent<CanvasGroup>();

        // Header — anchored to top
        var header = CreateAnchoredChild("Header", root.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -200), Vector2.zero);

        // Middle — stretch fill
        var middle = CreateAnchoredChild("Middle", root.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // Footer — anchored to bottom
        var footer = CreateAnchoredChild("Footer", root.transform,
            Vector2.zero, new Vector2(1, 0), Vector2.zero, new Vector2(0, 200));

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab;
    }

    /// <summary>Creates an empty gameplay root prefab.</summary>
    private static GameObject CreateGameplayDummyPrefab(string name, string path)
    {
        var root = new GameObject($"{name}Gameplay");
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateAnchoredChild(string childName, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var child = new GameObject(childName);
        child.transform.SetParent(parent, false);
        var rect = child.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return child;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CODE GENERATORS — SHARED
    // ════════════════════════════════════════════════════════════════════════

    private static string BuildTransition(string name)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using Blues.Core.StateMachine;");
        sb.AppendLine();
        sb.AppendLine($"[CreateAssetMenu(fileName = \"{name}Transition\", menuName = \"ProjectCore/State Machine/Transitions/{name}Transition\")]");
        sb.AppendLine($"public class {name}Transition : Transition");
        sb.AppendLine("{");
        sb.AppendLine("    ");
        sb.AppendLine("}");
        return sb.ToString();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CODE GENERATORS — UI STATE (minimal scaffolding)
    // ════════════════════════════════════════════════════════════════════════

    private static string BuildViewData(string name)
    {
        return
            $"using System;\n\n" +
            $"[Serializable]\n" +
            $"public struct {name}ViewData\n" +
            $"{{\n" +
            $"    // AI agent: populate from PROMPT.md\n" +
            $"}}\n";
    }

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
        sb.AppendLine($"    // AI agent: add SerializeField references from PROMPT.md");
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
        sb.AppendLine($"    // AI agent: add [SerializeField] GameEvent / variable fields from PROMPT.md");
        sb.AppendLine();
        sb.AppendLine($"    private {name}UIView _view;");
        sb.AppendLine();
        sb.AppendLine("    public override IEnumerator Enter(IState previous)");
        sb.AppendLine("    {");
        sb.AppendLine("        yield return base.Enter(previous);");
        sb.AppendLine();
        sb.AppendLine($"        _view = GetView<{name}UIView>();");
        sb.AppendLine("        if (_view == null) yield break;");
        sb.AppendLine();
        sb.AppendLine("        // AI agent: subscribe events and refresh view");
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

    // ════════════════════════════════════════════════════════════════════════
    //  CODE GENERATORS — GAME STATE (minimal scaffolding)
    // ════════════════════════════════════════════════════════════════════════

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
        sb.AppendLine("    // AI agent: add SerializeField buttons / text from PROMPT.md");
        sb.AppendLine();
        sb.AppendLine("    // AI agent: add public event Action fields for each button");
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
        sb.AppendLine("    // AI agent: add [SerializeField] GameEvent fields from PROMPT.md");
        sb.AppendLine();
        sb.AppendLine($"    private {name}Hud _hud;");
        sb.AppendLine();
        sb.AppendLine("    public override IEnumerator Enter(IState previous)");
        sb.AppendLine("    {");
        sb.AppendLine("        yield return base.Enter(previous);");
        sb.AppendLine();
        sb.AppendLine($"        _hud = gameHudInstance as {name}Hud;");
        sb.AppendLine("        if (_hud == null)");
        sb.AppendLine("        {");
        sb.AppendLine($"            Debug.LogError(\"[{name}State] gameHudInstance is not a {name}Hud.\");");
        sb.AppendLine("            yield break;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // AI agent: subscribe to HUD events");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public override IEnumerator Exit()");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_hud != null)");
        sb.AppendLine("        {");
        sb.AppendLine("            // AI agent: unsubscribe from HUD events");
        sb.AppendLine("            _hud = null;");
        sb.AppendLine("        }");
        sb.AppendLine();
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

        // Mock reference
        if (_mockImage != null)
        {
            string srcPath = AssetDatabase.GetAssetPath(_mockImage);
            string ext     = Path.GetExtension(srcPath);
            sb.AppendLine("## Mock Reference");
            sb.AppendLine($"![Mock](Art/Mock{ext})");
            sb.AppendLine();
        }

        sb.AppendLine("## Description");
        sb.AppendLine(string.IsNullOrWhiteSpace(_aiPrompt)
            ? "_No prompt provided — add description here for the AI agent._"
            : _aiPrompt.Trim());
        sb.AppendLine();
        sb.AppendLine("## Folder Structure");
        sb.AppendLine("```");
        sb.AppendLine($"{folder}/");
        sb.AppendLine("  Prefabs/   — dummy prefabs (AI agent wires components after compile)");
        sb.AppendLine("  Art/       — mock images, sprites");
        sb.AppendLine("  Scripts/   — State, View/Hud, ViewData");
        sb.AppendLine("  Config/    — SO assets (Event, State, Transition)");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## AI Agent Instructions");
        sb.AppendLine("1. Read this prompt and the mock image (if present).");
        sb.AppendLine("2. Implement the `// AI agent:` TODOs in the Scripts/ folder.");
        sb.AppendLine("3. Follow existing patterns — named methods, no lambdas in State.");
        sb.AppendLine("4. Wire prefab components in Unity after compilation.");

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
            EditorUtility.DisplayDialog("State Creator",
                $"Folder not found:\n{folder}\n\nNothing to delete.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("State Creator — Confirm Delete",
            $"This will permanently delete:\n" +
            $"  - {folder}/ (all files)\n" +
            $"  - ApplicationFlowController wiring for '{name}'\n" +
            $"  - StatesPooler entries: '{name}', '{name}Hud'\n\n" +
            $"This cannot be undone. Continue?",
            "Delete", "Cancel"))
            return;

        var report = new StringBuilder();

        // 1. AFC script
        bool afcCleaned = UnpatchApplicationFlowController(name);
        report.AppendLine(afcCleaned
            ? "AFC script — wiring removed ✓"
            : "AFC script — no wiring found");

        // 2. AFC prefab
        bool prefabCleaned = UnpatchAfcPrefab(name);
        report.AppendLine(prefabCleaned
            ? "AFC prefab — references removed ✓"
            : "AFC prefab — no references found");

        // 3. Pool entries
        var pooler = AssetDatabase.LoadAssetAtPath<PoolManagerSO>(PoolerPath);
        if (pooler != null)
        {
            int removed = RemovePoolEntries(pooler, name);
            report.AppendLine(removed > 0
                ? $"StatesPooler — {removed} entry(s) removed ✓"
                : "StatesPooler — no matching entries");
        }
        else
        {
            report.AppendLine("StatesPooler — asset not found, skipped");
        }

        // 4. Delete folder
        AssetDatabase.DeleteAsset(folder);
        report.AppendLine($"\n{folder}/ — deleted ✓");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("State Creator — Deleted", report.ToString(), "OK");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  APPLICATION FLOW CONTROLLER — SCRIPT PATCHER
    // ════════════════════════════════════════════════════════════════════════

    private static bool PatchApplicationFlowController(string name)
    {
        if (!File.Exists(AfcPath)) return false;

        string src = File.ReadAllText(AfcPath);

        if (src.Contains($"_goTo{name}Event"))
            return true;

        const string bootMarker = "    public void Boot()";
        if (!src.Contains(bootMarker)) return false;

        string fieldBlock =
            $"\n    [Header(\"{name}\")]\n" +
            $"    [SerializeField] private GameEvent _goTo{name}Event;\n" +
            $"    [SerializeField] private Transition _goTo{name}Transition;\n";

        src = src.Replace(bootMarker, fieldBlock + bootMarker);

        src = InsertBeforeMethodClose(src, "protected override void RegisterFlowEvents()",
            $"        _goTo{name}Event.Subscribe(HandleGoTo{name});\n");

        src = InsertBeforeMethodClose(src, "protected override void UnregisterFlowEvents()",
            $"        _goTo{name}Event.UnSubscribe(HandleGoTo{name});\n");

        string handler =
            $"\n    private void HandleGoTo{name}() =>\n" +
            $"        GoTo(_goTo{name}Transition, UICloseReasons.Home);\n";

        int lastBrace = src.LastIndexOf('}');
        if (lastBrace < 0) return false;
        src = src.Substring(0, lastBrace) + handler + "}\n";

        File.WriteAllText(AfcPath, src);
        return true;
    }

    private static bool UnpatchApplicationFlowController(string name)
    {
        if (!File.Exists(AfcPath)) return false;

        string src    = File.ReadAllText(AfcPath);
        string before = src;

        string headerPattern = $@"\r?\n\s*\[Header\(""{Regex.Escape(name)}""\)\]\s*\r?\n" +
                               $@"\s*\[SerializeField\][^\r\n]*_goTo{Regex.Escape(name)}Event;[^\r\n]*\r?\n" +
                               $@"\s*\[SerializeField\][^\r\n]*_goTo{Regex.Escape(name)}Transition;[^\r\n]*\r?\n?";
        src = Regex.Replace(src, headerPattern, "\n");

        string subPattern = $@"[^\S\r\n]*_goTo{Regex.Escape(name)}Event\.Subscribe\(HandleGoTo{Regex.Escape(name)}\);\s*\r?\n";
        src = Regex.Replace(src, subPattern, "");

        string unsubPattern = $@"[^\S\r\n]*_goTo{Regex.Escape(name)}Event\.UnSubscribe\(HandleGoTo{Regex.Escape(name)}\);\s*\r?\n";
        src = Regex.Replace(src, unsubPattern, "");

        string handlerPattern = $@"\r?\n\s*private void HandleGoTo{Regex.Escape(name)}\(\)[^}}]*?;\s*\r?\n";
        src = Regex.Replace(src, handlerPattern, "\n");

        if (src == before) return false;
        File.WriteAllText(AfcPath, src);
        return true;
    }

    private static string InsertBeforeMethodClose(string src, string methodSig, string insertion)
    {
        int sigIdx = src.IndexOf(methodSig, StringComparison.Ordinal);
        if (sigIdx < 0) return src;

        int openBrace = src.IndexOf('{', sigIdx);
        if (openBrace < 0) return src;

        int depth = 1, pos = openBrace + 1;
        while (pos < src.Length && depth > 0)
        {
            if (src[pos] == '{') depth++;
            else if (src[pos] == '}') depth--;
            pos++;
        }

        int closingBrace = pos - 1;
        return src.Substring(0, closingBrace) + insertion + src.Substring(closingBrace);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  APPLICATION FLOW CONTROLLER — PREFAB YAML PATCHER
    // ════════════════════════════════════════════════════════════════════════

    private static bool PatchAfcPrefab(string name, string eventAssetPath, string transitionAssetPath)
    {
        if (!File.Exists(AfcPrefab)) return false;

        string eventGuid      = AssetDatabase.AssetPathToGUID(eventAssetPath);
        string transitionGuid = AssetDatabase.AssetPathToGUID(transitionAssetPath);
        if (string.IsNullOrEmpty(eventGuid) || string.IsNullOrEmpty(transitionGuid)) return false;

        string yaml = File.ReadAllText(AfcPrefab);
        if (yaml.Contains($"_goTo{name}Event:")) return true;

        string eventLine      = $"  _goTo{name}Event: {{fileID: 11400000, guid: {eventGuid}, type: 2}}";
        string transitionLine = $"  _goTo{name}Transition: {{fileID: 11400000, guid: {transitionGuid}, type: 2}}";

        yaml = yaml.TrimEnd();
        yaml += "\n" + eventLine + "\n" + transitionLine + "\n";

        File.WriteAllText(AfcPrefab, yaml);
        return true;
    }

    private static bool UnpatchAfcPrefab(string name)
    {
        if (!File.Exists(AfcPrefab)) return false;

        string yaml   = File.ReadAllText(AfcPrefab);
        string before = yaml;

        string eventPattern      = $@"\r?\n\s*_goTo{Regex.Escape(name)}Event:[^\r\n]*";
        string transitionPattern = $@"\r?\n\s*_goTo{Regex.Escape(name)}Transition:[^\r\n]*";
        yaml = Regex.Replace(yaml, eventPattern, "");
        yaml = Regex.Replace(yaml, transitionPattern, "");

        if (yaml == before) return false;
        File.WriteAllText(AfcPrefab, yaml);
        return true;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  POOL HELPERS
    // ════════════════════════════════════════════════════════════════════════

    private static void AddPoolEntry(PoolManagerSO pooler, string id, GameObject prefab)
    {
        var poolerSO = new SerializedObject(pooler);
        var configs  = poolerSO.FindProperty("_poolConfigs");
        int idx      = configs.arraySize;
        configs.InsertArrayElementAtIndex(idx);
        var entry = configs.GetArrayElementAtIndex(idx);
        entry.FindPropertyRelative("PoolID").stringValue          = id;
        entry.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
        entry.FindPropertyRelative("DefaultCapacity").intValue    = 1;
        entry.FindPropertyRelative("MaxSize").intValue            = 1;
        entry.FindPropertyRelative("Prewarm").boolValue           = false;
        poolerSO.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(pooler);
    }

    private static int RemovePoolEntries(PoolManagerSO pooler, string name)
    {
        var poolerSO = new SerializedObject(pooler);
        var configs  = poolerSO.FindProperty("_poolConfigs");
        int removed  = 0;

        for (int i = configs.arraySize - 1; i >= 0; i--)
        {
            string poolId = configs.GetArrayElementAtIndex(i)
                                   .FindPropertyRelative("PoolID").stringValue;
            if (poolId == name || poolId == $"{name}Hud")
            {
                configs.DeleteArrayElementAtIndex(i);
                removed++;
            }
        }

        if (removed > 0)
        {
            poolerSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pooler);
        }

        return removed;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  AI IMPLEMENTATION
    // ════════════════════════════════════════════════════════════════════════

    private async Task RunAIImplementation()
    {
        string name = _screenName.Trim();
        string folder = $"Assets/Game/Screens/{name}";
        string scriptsDir = $"{folder}/Scripts";

        _isGenerating = true;
        _generationLog = "";
        _genProgress = 0f;
        Repaint();

        try
        {
            // 1. API key — env var > EditorPrefs
            string apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                apiKey = EditorPrefs.GetString("AnthropicApiKey", "");
            if (string.IsNullOrEmpty(apiKey))
            {
                AppendLog("ERROR: No API key found.\nSet ANTHROPIC_API_KEY env var or EditorPrefs key 'AnthropicApiKey'.");
                return;
            }

            // 2. Read stub files
            AppendLog("Reading stub files...");
            _genProgress = 0.1f;
            var stubBuilder = new StringBuilder();
            if (Directory.Exists(scriptsDir))
            {
                foreach (var file in Directory.GetFiles(scriptsDir, "*.cs"))
                {
                    stubBuilder.AppendLine($"// --- {Path.GetFileName(file)} ---");
                    stubBuilder.AppendLine(File.ReadAllText(file));
                    stubBuilder.AppendLine();
                }
            }

            // 3. Read PROMPT.md
            AppendLog("Reading PROMPT.md...");
            _genProgress = 0.15f;
            string promptPath = $"{folder}/PROMPT.md";
            string promptContent = File.Exists(promptPath) ? File.ReadAllText(promptPath) : "(No PROMPT.md found)";

            // 4. Check for mock image
            string mockImageBase64 = null;
            string mockMediaType = null;
            string artDir = $"{folder}/Art";
            if (Directory.Exists(artDir))
            {
                foreach (var img in Directory.GetFiles(artDir))
                {
                    string ext = Path.GetExtension(img).ToLowerInvariant();
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                    {
                        AppendLog($"Encoding mock image: {Path.GetFileName(img)}...");
                        byte[] bytes = File.ReadAllBytes(img);
                        mockImageBase64 = Convert.ToBase64String(bytes);
                        mockMediaType = ext == ".png" ? "image/png" : "image/jpeg";
                        break;
                    }
                }
            }
            _genProgress = 0.2f;
            Repaint();

            // 5. Build request
            string systemPrompt = BuildAISystemPrompt(name);

            string userText1 = $"Screen name: {name}\n\nDescription:\n{promptContent}\n\nExisting stubs:\n{stubBuilder}";
            string userText2 = "Implement all three files completely based on the description and mockup. " +
                               "Every // AI agent: comment must be replaced with real code.";

            var contentArray = new JArray();
            contentArray.Add(new JObject { ["type"] = "text", ["text"] = userText1 });

            if (mockImageBase64 != null)
            {
                contentArray.Add(new JObject
                {
                    ["type"] = "image",
                    ["source"] = new JObject
                    {
                        ["type"] = "base64",
                        ["media_type"] = mockMediaType,
                        ["data"] = mockImageBase64
                    }
                });
            }

            contentArray.Add(new JObject { ["type"] = "text", ["text"] = userText2 });

            var requestBody = new JObject
            {
                ["model"] = "claude-sonnet-4-6",
                ["max_tokens"] = 8192,
                ["system"] = systemPrompt,
                ["messages"] = new JArray
                {
                    new JObject
                    {
                        ["role"] = "user",
                        ["content"] = contentArray
                    }
                }
            };

            // 6. Send request
            AppendLog("Calling Claude API (claude-sonnet-4-6)...");
            _genProgress = 0.3f;
            Repaint();

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();
            _genProgress = 0.7f;

            if (!response.IsSuccessStatusCode)
            {
                AppendLog($"ERROR: API returned {(int)response.StatusCode}\n{responseBody}");
                return;
            }

            AppendLog($"Response received ({responseBody.Length} chars)");

            // 7. Extract text from response
            var responseJson = JObject.Parse(responseBody);
            var fullTextBuilder = new StringBuilder();
            var contentBlocks = responseJson["content"] as JArray;
            if (contentBlocks != null)
            {
                foreach (var block in contentBlocks)
                {
                    if (block["type"]?.ToString() == "text")
                        fullTextBuilder.Append(block["text"]?.ToString());
                }
            }
            string fullText = fullTextBuilder.ToString();

            // 8. Parse files
            AppendLog("Parsing generated files...");
            _genProgress = 0.8f;
            var files = ParseGeneratedFiles(fullText);

            if (files.Count == 0)
            {
                AppendLog("WARNING: No files parsed from response. Raw output (first 500 chars):\n" +
                          fullText.Substring(0, Math.Min(500, fullText.Length)));
                return;
            }

            // 9. Write files
            foreach (var kvp in files)
            {
                string filePath = $"{scriptsDir}/{kvp.Key}";
                File.WriteAllText(filePath, kvp.Value);
                int lineCount = kvp.Value.Split('\n').Length;
                AppendLog($"Wrote {kvp.Key} ({lineCount} lines)");
            }
            _genProgress = 0.95f;

            // 10. Refresh on main thread
            AppendLog("Refreshing AssetDatabase...");
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("State Creator — AI Implementation",
                    $"AI implementation complete — {files.Count} file(s) written.\n" +
                    "Recompile then wire prefab components.", "OK");
            };

            _genProgress = 1f;
            AppendLog("Done!");
        }
        catch (Exception ex)
        {
            AppendLog($"ERROR: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            _isGenerating = false;
            Repaint();
        }
    }

    private void AppendLog(string message)
    {
        _generationLog += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
        Repaint();
    }

    private static Dictionary<string, string> ParseGeneratedFiles(string response)
    {
        var files = new Dictionary<string, string>();
        var matches = Regex.Matches(response,
            @"//\s*FILE:\s*(\S+\.cs)\s*\n(.*?)//\s*END\s*FILE",
            RegexOptions.Singleline);

        foreach (Match m in matches)
        {
            string fileName = m.Groups[1].Value.Trim();
            string content = m.Groups[2].Value.Trim() + "\n";
            files[fileName] = content;
        }

        return files;
    }

    private static string BuildAISystemPrompt(string name)
    {
        return
            "You are a Unity C# code generator. You will implement three files for a Unity game " +
            "that uses this architecture:\n\n" +
            "Read these key patterns from the codebase before generating:\n" +
            "- UIViewState: base class for UI states. Enter() calls base.Enter() then GetView<TView>() " +
            "to get the pooled view instance\n" +
            "- UIBase: base class for views. Has SetData(), Awake() for wiring button listeners. " +
            "Events fire upward via Action delegates\n" +
            "- State subscriptions ALWAYS use named methods — never lambdas\n" +
            "- ViewData is a plain C# struct, Serializable, no Unity dependencies, only primitives and enums\n\n" +
            "Generate exactly three files in this format and nothing else:\n\n" +
            $"// FILE: {name}View.cs\n<complete implementation>\n// END FILE\n\n" +
            $"// FILE: {name}ViewData.cs\n<complete implementation>\n// END FILE\n\n" +
            $"// FILE: {name}State.cs\n<complete implementation>\n// END FILE\n\n" +
            "Rules:\n" +
            "- View: [SerializeField] for every UI element. public event Action for each button. " +
            "SetData() binds all data. Awake() wires all listeners with named method references\n" +
            "- ViewData: only bool, int, string, float, enums. No MonoBehaviour, no Sprite, no GameObject\n" +
            "- State: [SerializeField] GameEvent for each navigation. private void SubscribeEvents() and " +
            "UnsubscribeEvents() called in Enter/Exit. private void HandleX() named handlers only. " +
            "private void RefreshView() calls _view.SetData(new ViewData{})\n" +
            "- Match the exact namespaces: Blues.Core.UI for UIBase, Blues.Core.Events for GameEvent, " +
            "Blues.Core.StateMachine for UIViewState\n" +
            "- Keep IEnumerator Enter(IState previous) and Exit() signatures exactly as in the stubs";
    }

    // ════════════════════════════════════════════════════════════════════════
    //  VIEW TYPE RESOLVER
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Searches all loaded assemblies for a type by name.
    /// Used for best-effort component addition on prefabs after a previous compile cycle.
    /// </summary>
    private static Type FindTypeByName(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type != null) return type;
        }
        return null;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GENERAL HELPERS
    // ════════════════════════════════════════════════════════════════════════

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "Assets";
            string child  = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, child);
        }
    }

}
