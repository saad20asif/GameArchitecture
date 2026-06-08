# State Implementation Guide — AI Agent Reference

> This document defines **every rule and pattern** an AI agent must follow when implementing a new screen/state in this game architecture. It is the single source of truth — the per-state PROMPT.md personalizes *what* to build, this guide defines *how*.

---

## Table of Contents

1. [Architecture Mental Model](#1-architecture-mental-model)
2. [State Types](#2-state-types)
3. [File Structure](#3-file-structure)
4. [Script Patterns](#4-script-patterns)
5. [ScriptableObject Assets](#5-scriptableobject-assets)
6. [Prefab Construction](#6-prefab-construction)
7. [Application Flow Integration](#7-application-flow-integration)
8. [MCP Workflow](#8-mcp-workflow)
9. [Common Pitfalls](#9-common-pitfalls)
10. [Reference File Locations](#10-reference-file-locations)

---

## 1. Architecture Mental Model

```
State (SO)  ─── "the brain" ─── owns WHAT happens and WHEN
  │
  ├── View (MonoBehaviour on prefab) ─── owns HOW it looks, nothing else
  │     └── fires event Actions upward (never calls State)
  │
  └── ViewData (plain struct) ─── data flows DOWN from State → View.SetData()
```

### Core Principles

- **State is a ScriptableObject** — it lives as a `.asset` file, not on a GameObject
- **View is a MonoBehaviour** — it lives on a prefab, managed by the pool system
- **One State can drive multiple Views** — State is the brain, Views are limbs
- **Communication is one-directional**: View → State via `event Action`, State → View via `SetData(ViewData)`
- **Navigation uses GameEvent SOs** — State never calls FSM directly, it fires a GameEvent that the ApplicationFlowController listens to
- **All subscriptions are named methods** — never lambdas in State scripts
- **Button listeners wire once in `Awake()`** — the GO is pooled, not destroyed between uses

---

## 2. State Types

### 2a. UI State (`UIViewState` + `UIBase`)

For screens/overlays/popups — anything that is purely UI.

| Base Class | Role |
|------------|------|
| `UIViewState` (extends `State`) | Handles pooling, spawning, Show/Hide lifecycle |
| `UIBase` (extends `MonoBehaviour`) | Animation system, canvas management, IShowable |

**State gets the view via:** `_view = GetView<MyView>()` (called after `base.Enter()`)

### 2b. Game State (`GameState` + `GameHud`)

For gameplay screens with a HUD overlay and optional gameplay prefab.

| Base Class | Role |
|------------|------|
| `GameState` (extends `State`) | Spawns gameplay prefab + HUD, manages both lifecycles |
| `GameHud` (extends `MonoBehaviour`) | Header/footer slide animations, IShowable |

**State gets the hud via:** `_hud = gameHudInstance as MyHud` (called after `base.Enter()`)

---

## 3. File Structure

Every state lives in `Assets/Game/Screens/{Name}/`:

```
Assets/Game/Screens/{Name}/
  ├── Art/
  │   └── Mock.png              ← reference screenshot/wireframe
  ├── Config/
  │   ├── {Name}State.asset     ← the State ScriptableObject
  │   ├── GoTo{Name}Event.asset ← GameEvent SO (if this state needs its own navigation event)
  │   ├── GoTo{Name}Transition.asset ← Transition SO
  │   └── v_*.asset             ← any DBBool/DBInt/etc. variables this state owns
  ├── Prefabs/
  │   └── {Name}.prefab         ← the UI prefab (registered in pool)
  ├── Scripts/
  │   ├── {Name}State.cs        ← brain — extends UIViewState or GameState
  │   ├── {Name}UIView.cs       ← view — extends UIBase (or {Name}Hud.cs extending GameHud)
  │   ├── {Name}ViewData.cs     ← plain struct with primitives only
  │   └── {Name}Transition.cs   ← usually empty, extends Transition
  └── PROMPT.md                 ← per-state AI instructions + mock reference
```

---

## 4. Script Patterns

### 4a. ViewData — data struct

```csharp
using System;

[Serializable]
public struct {Name}ViewData
{
    // ONLY primitives: bool, int, float, string, enum
    // NO UnityEngine.Object references, NO collections
    // State builds this and passes it to View.SetData()
}
```

### 4b. View — presentation layer (UIBase)

```csharp
using System;
using Blues.Core.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// {Name}View — owns HOW the screen looks, nothing else.
///
/// Rules:
///   - Never holds a reference to {Name}State or any State/Service
///   - Fires events upward; {Name}State subscribes in Enter(), unsubscribes in Exit()
///   - Button listeners wired once in Awake() — GO is pooled, not destroyed
/// </summary>
public class {Name}UIView : UIBase
{
    // ── SerializeField references (buttons, images, text, etc.) ──
    [SerializeField] private Button SomeBtn;

    // ── Events fired upward to State ──
    public event Action OnSomePressed;

    // ── SetData: State pushes data down to refresh the view ──
    public void SetData({Name}ViewData data)
    {
        // Bind data fields to UI elements
    }

    // ── Awake: wire button listeners ONCE (pooled GO survives) ──
    protected override void Awake()
    {
        base.Awake();
        SomeBtn.onClick.AddListener(OnSomeBtnClicked);
    }

    // ── Private click handlers → fire events ──
    private void OnSomeBtnClicked() => OnSomePressed?.Invoke();
}
```

**Key rules:**
- Every button gets its own `event Action` — the View never knows what the action *does*
- `SetData()` is the only way State talks to View (one-way data flow)
- Colors, sprites, text changes — all done inside `SetData()` based on ViewData primitives
- For toggle buttons: View fires `OnXToggled`, State toggles the variable and calls `RefreshView()` which calls `SetData()` again
- If the view needs icons that change state (enabled/disabled), add `[SerializeField] private Image XIcon` and set color in `SetData()`

### 4c. View — presentation layer (GameHud)

```csharp
using System;
using UnityEngine;
using UnityEngine.UI;
using Blues.Core.GameHud;

/// <summary>
/// {Name}Hud — owns HOW the gameplay HUD looks, nothing else.
/// Same rules as UIBase views.
/// </summary>
public class {Name}Hud : GameHud
{
    [SerializeField] private Button SomeBtn;

    public event Action OnSomePressed;

    protected override void Awake()
    {
        base.Awake();
        SomeBtn.onClick.AddListener(OnSomeBtnClicked);
    }

    private void OnSomeBtnClicked() => OnSomePressed?.Invoke();
}
```

### 4d. State — the brain (UIViewState)

```csharp
using Blues.Core.Events;
using Blues.Core.StateMachine;
using System.Collections;
using UnityEngine;

/// <summary>
/// {Name}State — owns WHAT happens and WHEN.
///
/// Rules:
///   - Subscribes to view events in Enter(), unsubscribes by name in Exit()
///   - Navigation fires GameEvent SOs — never calls FSM directly
///   - No public methods exposed to the View
///   - Named methods only — NEVER lambdas
/// </summary>
[CreateAssetMenu(fileName = "{Name}State", menuName = "ProjectCore/State Machine/States/{Name}State")]
public class {Name}State : UIViewState
{
    // ── GameEvent SOs for navigation ──
    [SerializeField] private GameEvent GoToSomeEvent;

    // ── ScriptableObject variables (DBBool, DBInt, etc.) ──
    [Header("Variables")]
    [SerializeField] private DBBool SomeToggle;

    // ── Cached view reference ──
    private {Name}UIView _view;

    // ── Enter: get view, subscribe, refresh ──
    public override IEnumerator Enter(IState previous)
    {
        yield return base.Enter(previous);

        _view = GetView<{Name}UIView>();
        if (_view == null) yield break;

        SubscribeEvents();
        RefreshView();
    }

    // ── Exit: unsubscribe, null view ──
    public override IEnumerator Exit()
    {
        if (_view != null)
        {
            UnsubscribeEvents();
            _view = null;
        }

        yield return base.Exit();
    }

    // ── Named subscribe/unsubscribe methods ──
    private void SubscribeEvents()
    {
        _view.OnSomePressed += HandleSomePressed;
    }

    private void UnsubscribeEvents()
    {
        _view.OnSomePressed -= HandleSomePressed;
    }

    // ── Named handlers ──
    private void HandleSomePressed()
    {
        GoToSomeEvent.Invoke();
    }

    // ── RefreshView: build ViewData and push to view ──
    private void RefreshView()
    {
        _view.SetData(new {Name}ViewData
        {
            // populate from SO variables
        });
    }
}
```

### 4e. State — the brain (GameState)

```csharp
using Blues.Core.Events;
using Blues.Core.StateMachine;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "{Name}State", menuName = "ProjectCore/State Machine/States/{Name}State")]
public class {Name}State : GameState
{
    [SerializeField] private GameEvent GoToSomeEvent;

    private {Name}Hud _hud;

    public override IEnumerator Enter(IState previous)
    {
        yield return base.Enter(previous);

        _hud = gameHudInstance as {Name}Hud;
        if (_hud == null)
        {
            Debug.LogError("[{Name}State] gameHudInstance is not a {Name}Hud.");
            yield break;
        }

        _hud.OnSomePressed += HandleSomePressed;
    }

    public override IEnumerator Exit()
    {
        if (_hud != null)
        {
            _hud.OnSomePressed -= HandleSomePressed;
            _hud = null;
        }

        yield return base.Exit();
    }

    private void HandleSomePressed() => GoToSomeEvent.Invoke();
}
```

---

## 5. ScriptableObject Assets

### Exit / Navigation Events

| Close Behaviour | Which GameEvent to use | How it works |
|-----------------|----------------------|--------------|
| **PopOne** | `e_BackBtnPressed.asset` (find it in project) | BaseAFC.OnBackButtonPressed → `PopPausedState()` — exits current, resumes previous |
| **ClearAll** | Custom `GoTo{Name}Event.asset` | AFC handler calls `GoTo(transition, UICloseReasons.Home)` → `ClearAllAndTransitionTo()` |
| **Default** | Custom `GoTo{Name}Event.asset` | AFC handler calls `GoTo(transition, reason)` — exits current, enters next |

**Rule:** If the state uses PopOne (overlay/popup that goes back), it should fire `e_BackBtnPressed` to exit. Do NOT create a new event for "going back" — reuse the existing back event.

### UICloseReasons → ClosePolicy + PauseCurrent Mapping

This is **critical** — the `UICloseReasons` you pass in the AFC handler determines both the ClosePolicy AND whether the previous state gets paused. Get this wrong and you'll either destroy states that should stay alive or fail to pause them.

```
BaseApplicationFlowController.GetPolicyForReason():
  Home                    → ClearAll
  Game                    → ClearAll
  ResumeGame              → PopOne
  Revive                  → PopOne
  SkipLevel               → PopOne
  ResumeAny               → PopUntil
  Everything else         → Default

BaseApplicationFlowController.ShouldPauseCurrent():
  ShowFullScreenPlacement → true
  FullScreenPlacement     → true
  DailyLogin              → true
  Everything else         → false
```

**How to pick the right UICloseReasons for your state:**

| PROMPT says | Pause Previous? | Use UICloseReasons | Result |
|-------------|-----------------|-------------------|--------|
| Close Behaviour: ClearAll | N/A | `Home` or `Game` | Clears entire stack, enters new state |
| Close Behaviour: PopOne | Yes (implicit) | N/A — use `e_BackBtnPressed` instead | Pops current, resumes previous |
| Close Behaviour: Default, Pause: **true** | Yes | `FullScreenPlacement` | Pushes on top, pauses previous |
| Close Behaviour: Default, Pause: **false** | No | Any non-special reason (or custom) | Exits current, enters next, no pause |

**Common mistake:** Using `UICloseReasons.Home` for an overlay/popup. `Home` triggers `ClearAll` which **destroys all states in the stack** — use `FullScreenPlacement` for overlays that should keep the previous state alive and paused.

### Variable Assets

| Type | Asset Menu | Use Case |
|------|------------|----------|
| `DBBool` | `ProjectCore/Variables/Persistent/DBBool` | Persistent on/off settings (Sound, Haptics, etc.) |
| `DBInt` | `ProjectCore/Variables/Persistent/DBInt` | Persistent counters/values |
| `Bool` | `ProjectCore/Variables/Non-Persistent/Bool` | Runtime-only flags |
| `Int` | `ProjectCore/Variables/Non-Persistent/Int` | Runtime-only values |

### State Asset Fields to Verify

After creating a `{Name}State.asset`, always verify these inherited fields:

| Field | Source | What to set |
|-------|--------|-------------|
| `stateId` | `UIViewState` | Must match the pool key / prefab name (e.g., `"GameSettings"`) |
| `usePooling` | `UIViewState` | `true` (default) |
| `uIStatesPooler` | `UIViewState` | Reference to `StatesPooler.asset` |
| `useDefaultAnimations` | `UIViewState` | `true` unless custom animations needed |
| Custom fields | Your State class | GameEvents, Variables, etc. |

---

## 6. Prefab Construction

### UI State Prefab Structure

```
{Name} (Canvas + CanvasScaler + GraphicRaycaster + CanvasGroup + {Name}UIView)
  ├── Background (Image — full screen, raycast target for blocking input)
  └── Content (RectTransform — parent for all UI elements)
      ├── Button1 (Button + Image)
      │   └── Icon (Image, optional)
      ├── Button2 (Button + Image)
      ├── TextElement (TextMeshProUGUI)
      └── ...
```

### MCP Prefab Wiring Rules

1. **Always use `GetType().Name` matching** when looking up components via `execute_code` — never use generic `GetComponent<T>()` which can return null in MCP context
2. **Wire SerializeField references** using `execute_code` after all GameObjects and components are created
3. **Save the prefab** after all wiring is complete
4. **Anchor and position** elements precisely as described in the mock

### Button Layout Conventions

- Standard button size: `80x80` for icon buttons, `200x60` for text buttons
- Spacing between stacked buttons: `10px`
- Edge margins: `20px` from screen edge
- Buttons anchored to their visual position (top-right, bottom-center, etc.)

---

## 7. Application Flow Integration

When a state needs its **own navigation event** (not PopOne), the AFC must be updated:

### ApplicationFlowController.cs Changes

```csharp
// 1. Add fields
[Header("{Name}")]
[SerializeField] private GameEvent GoTo{Name}Event;
[SerializeField] private Transition {Name}Transition;

// 2. Register in RegisterFlowEvents()
GoTo{Name}Event.Subscribe(HandleGoTo{Name});

// 3. Unregister in UnregisterFlowEvents()
GoTo{Name}Event.UnSubscribe(HandleGoTo{Name});

// 4. Handler method — pick UICloseReasons from the mapping table in §5
//    Close Behaviour: ClearAll          → UICloseReasons.Home (or .Game)
//    Close Behaviour: Default + Pause   → UICloseReasons.FullScreenPlacement
//    Close Behaviour: Default, no Pause → UICloseReasons.Default (or other non-special)
private void HandleGoTo{Name}() => GoTo({Name}Transition, UICloseReasons.{CorrectReason});
```

### ApplicationFlowController.prefab Changes

Wire the new SO references on the prefab via MCP `execute_code`.

**PopOne states do NOT need AFC changes** — they use `e_BackBtnPressed` which is already wired.

---

## 8. MCP Workflow

After scripts compile, use Unity MCP tools in this exact order:

### Step 1: Verify Compilation
```
read_console → check for errors
```

### Step 2: Build Prefab
```
manage_prefabs(action="load") → open the prefab
manage_gameobject(action="create") → create child GameObjects under Content
manage_components(action="add") → add Button, Image, TextMeshProUGUI components
manage_gameobject(action="modify") → set anchors, position, size
manage_prefabs(action="save") → save the prefab
```

### Step 3: Wire View References
```
execute_code → find the View component using GetType().Name
execute_code → assign SerializeField references using reflection or serialized property
manage_prefabs(action="save") → save again
```

### Step 4: Create SO Assets
```
execute_code → ScriptableObject.CreateInstance<DBBool>() + AssetDatabase.CreateAsset()
execute_code → find existing assets (e.g., e_BackBtnPressed) with AssetDatabase.FindAssets()
```

### Step 5: Wire State Asset
```
execute_code → load {Name}State.asset, assign all SerializeField references
execute_code → verify all fields are set (stateId, pooler, events, variables)
```

### Step 6: Register in Pool
```
execute_code → add pool entry to StatesPooler.asset if not already done by State Creator
```

---

## 9. Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| `GetComponent<T>()` returns null in MCP | Use `GetType().Name == "TypeName"` matching instead |
| Lambdas in State subscribe/unsubscribe | Always use named methods — lambdas can't be unsubscribed |
| View references State directly | View must NEVER know about State — fire events upward |
| Creating a new "back" event for PopOne | Reuse `e_BackBtnPressed.asset` — it already triggers `PopPausedState()` |
| ViewData with UnityEngine.Object refs | ViewData is `[Serializable] struct` with primitives ONLY |
| Forgetting `base.Awake()` in View | UIBase.Awake sets up Canvas, CanvasGroup, animation system |
| Forgetting `base.Enter()`/`base.Exit()` | UIViewState.Enter spawns the view, Exit releases it — MUST call base |
| Forgetting to null `_view` in Exit | Always null the cached view ref to prevent stale access |
| Button listeners added in OnEnable | Add in `Awake()` — the GO is pooled, Awake runs once |
| Multiple views: subscribing to wrong one | Each view has its own events — State manages each independently |

---

## 10. Reference File Locations

### Core Architecture
| File | Path |
|------|------|
| State base | `Assets/Saad/Utilities/StateMachine/Scripts/State.cs` |
| IState interface | `Assets/Saad/Utilities/StateMachine/Scripts/IState.cs` |
| FSM | `Assets/Saad/Utilities/StateMachine/Scripts/FiniteStateMachine.cs` |
| Transition | `Assets/Saad/Utilities/StateMachine/Scripts/Transition.cs` |
| UIViewState | `Assets/Saad/Utilities/StateMachine/Scripts/UiViewState.cs` |
| UIBase | `Assets/Saad/UI/Base/Scripts/UiBase.cs` |
| GameState | `Assets/Saad/UI/GameState/Scripts/GameState.cs` |
| GameHud | `Assets/Saad/UI/GameHud/Scripts/GameHud.cs` |
| GameEvent | `Assets/Saad/Utilities/Events/GameEvent.cs` |
| BaseAFC | `Assets/Saad/GameFlow/Scripts/BaseApplicationFlowController.cs` |
| AFC | `Assets/Saad/GameFlow/Scripts/ApplicationFlowController.cs` |

### Variable System
| File | Path |
|------|------|
| Bool (non-persistent) | `Assets/Saad/Variables/Non-Persistent/Bool.cs` |
| DBBool (persistent) | `Assets/Saad/Variables/Persistent/DBBool.cs` |

### Reference Implementations
| Screen | Path | Type |
|--------|------|------|
| MainMenu | `Assets/Saad/UI/MainMenu/` | UIState (simple, single button) |
| NormalGame | `Assets/Saad/UI/GameState/` + `Assets/Saad/UI/GameHud/` | GameState (gameplay + HUD) |
| GameSettingsX | `Assets/Game/Screens/GameSettingsX/` | UIState (toggles, multiple buttons, ViewData) |
| SpinWheel | `Assets/Saad/UI/SpinWheel/` | UIState (single button, PopOne) |
| LevelComplete | `Assets/Saad/UI/LevelComplete/` | UIState (single button) |
| LevelFail | `Assets/Saad/UI/LevelFail/` | UIState (single button) |
| RateUs | `Assets/Saad/UI/RateUs/` | UIState (single button, PopOne) |

### Pool & Flow
| Asset | Path |
|-------|------|
| StatesPooler | `Assets/Saad/Utilities/PoolSystem/Config/StatesPooler.asset` |
| AFC Prefab | `Assets/Saad/GameFlow/Prefabs/Resources/ApplicationFlowController.prefab` |
| e_BackBtnPressed | Search project — used for all PopOne exits |

---

## Appendix: Close Policy Quick Reference

| Policy | When to use | FSM method | Pauses previous? |
|--------|-------------|------------|-------------------|
| **Default** | Normal forward navigation | `TransitionTo(t, false)` | No |
| **ClearAll** | Going Home, starting Game | `ClearAllAndTransitionTo(t)` | N/A — clears all |
| **PopOne** | Overlays, popups, settings | `PopPausedState()` | Yes (previous state stays paused) |
| **PopUntil** | Jump back to a specific state | `JumpTo(targetState)` | N/A — clears intermediates |
