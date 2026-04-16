# State Creator Tool

> AI-assisted state scaffolding for the game architecture.
> **Status:** ✅ Shipped. Tool source: `Assets/Editor/StateCreatorWindow.cs`.
> **Reference output:** `Assets/Game/Screens/GameSettingsX/` — a screen that was built with the tool and hand-polished to match the canonical pattern described in [ARCHITECTURE.md §6.8](./ARCHITECTURE.md#68-gamesettingsx-reference-implementation).

---

## What it does

**Tools > State Creator** — a simplified editor window that generates everything
needed for a new screen state with one click:

### Inputs
| Field | Description |
|-------|-------------|
| **State Type** | UI State (UIViewState + UIBase) or Game State (GameState + GameHud) |
| **Screen Name** | PascalCase name — drives all file/class names |
| **Close Behaviour** | ClosePolicy enum (Default, ClearAll, PopOne, PopUntil) |
| **AI Prompt** | Free-text description of the state's functionality |
| **Mock Image** | Optional reference screenshot / wireframe |

### What CREATE generates

```
Assets/Game/Screens/{Name}/
  ├── Prefabs/    — dummy prefabs (Canvas structure, registered in pooler)
  ├── Art/        — mock image copied here (if provided)
  ├── Scripts/    — minimal scaffolding with // AI agent: TODO markers
  ├── Config/     — GoTo{Name}Event, {Name}State, GoTo{Name}Transition SOs
  └── PROMPT.md   — AI prompt + metadata for the agent to read
```

Plus:
- ApplicationFlowController.cs — fields, subscribe, unsubscribe, handler patched
- ApplicationFlowController.prefab — SO references assigned via YAML
- StatesPooler — pool entries registered for dummy prefabs

### What DELETE removes
- Entire screen folder
- AFC script wiring + prefab references
- Pool entries

---

## AI Agent Workflow

1. User fills in the State Creator form with a description + optional mock
2. Tool creates scaffolding — scripts have `// AI agent:` TODOs
3. User asks Claude to implement the state:
   - Claude reads `PROMPT.md` + mock image
   - Fills in ViewData fields, View UI bindings, State logic
   - Follows existing patterns (named methods, no lambdas in State)
4. After compile, wire components on prefabs

---

## State Types

### UI State (UIViewState + UIBase)
Generates:
- `{Name}State.cs` — extends `UIViewState`, uses `GetView<T>()` pattern
- `{Name}View.cs` — extends `UIBase`, receives `{Name}ViewData`
- `{Name}ViewData.cs` — data struct for the view
- Single dummy Canvas prefab with Background + Content panels

### Game State (GameState + GameHud)
Generates:
- `{Name}State.cs` — extends `GameState`, casts `gameHudInstance` to typed Hud
- `{Name}Hud.cs` — extends `GameHud` (base handles `Paused`, `Pause()`, `Resume()`)
- Dummy HUD prefab (Canvas with Header/Middle/Footer)
- Dummy Gameplay prefab (empty root)

---

## Hard Rules

- Named methods for all State subscriptions — no lambdas in State
- Copy existing code style — do not invent new patterns
- No new base classes, no new systems — use only what exists
- Assets/Editor/ only — not included in builds
- If target folder already exists → warning dialog, do not overwrite
- Dummy prefabs are structural templates — AI agent adds components after compile

---

## Architecture Principles (SOLID)

### Animation is a presentation concern — NOT a state machine concern

The FSM (`FiniteStateMachine`, `State`) has **zero animation awareness**:
- `State.cs` has no animation fields or flags
- `FiniteStateMachine.cs` only manages state lifecycle: Enter, Exit, Pause, Resume
- No `SkipExitAnimation` or similar flags anywhere in the core

Animation decisions live in the **view layer** (`UIBase`, `GameHud`):
- Both have `UseDefaultAnimations` property (set per-state via serialized field)
- Both have `Paused` property — when `Paused == true`, `Hide()` skips all animations
- Custom animation hooks: `OnCustomShow()`, `OnCustomHide()`, `OnCustomPause()`, `OnCustomResume()`

### Multi-state exit ordering

When `ClearAll` fires (e.g., going Home from a deep overlay stack):
1. FSM pauses the current (visible) state — view sets `Paused = true`
2. FSM exits the current state — view sees `Paused`, snaps instantly
3. FSM clears all paused states — all already paused, all snap instantly
4. FSM enters the new state with its enter animation

Method: `FiniteStateMachine.ClearAllAndTransitionTo(Transition)` — pure lifecycle, zero animation coupling.

### IState contract (immutable)
```csharp
public interface IState
{
    void TransitionTo(Transition transition, bool pauseCurrent = false);
    IEnumerator ClearPausedStates();
    IEnumerator ReloadCurrentState();
    int CurrentSortingOrder { get; }
}
```

---

## Close Policies

| Policy | Behaviour | FSM Method |
|--------|-----------|------------|
| **Default** | Exit current, enter next | `TransitionTo(transition, false)` |
| **ClearAll** | Snap-exit ALL states (current + paused), enter next | `ClearAllAndTransitionTo(transition)` |
| **PopOne** | Exit current (animated), resume top of paused stack | `PopPausedState()` |
| **PopUntil** | Exit current (animated), snap-clear intermediates, resume target | `JumpTo(targetState)` |

---

## Key File Map

| File | Role |
|------|------|
| `Assets/Editor/StateCreatorWindow.cs` | State Creator editor tool |
| `Assets/Saad/Utilities/StateMachine/Scripts/State.cs` | Base state (ScriptableObject) |
| `Assets/Saad/Utilities/StateMachine/Scripts/IState.cs` | FSM interface contract |
| `Assets/Saad/Utilities/StateMachine/Scripts/FiniteStateMachine.cs` | FSM controller |
| `Assets/Saad/Utilities/StateMachine/Scripts/Transition.cs` | Transition base class |
| `Assets/Saad/Utilities/StateMachine/Scripts/UiViewState.cs` | UI state base (pooling + UIBase lifecycle) |
| `Assets/Saad/UI/Base/Scripts/UiBase.cs` | UI view base (animation, IShowable) |
| `Assets/Saad/UI/Base/Scripts/UiAnimationSystem.cs` | DOTween animation player |
| `Assets/Saad/UI/Base/Scripts/StateAnimationConfig.cs` | Animation config SO |
| `Assets/Saad/UI/Base/Scripts/IShowable.cs` | Show/Hide/Pause/Resume interface |
| `Assets/Saad/UI/Base/Scripts/UiCloseReasons.cs` | UICloseReasons + ClosePolicy enums |
| `Assets/Saad/UI/GameState/Scripts/GameState.cs` | Game state base (gameplay + HUD lifecycle) |
| `Assets/Saad/UI/GameState/Scripts/GameStateTransition.cs` | Game transition type |
| `Assets/Saad/UI/GameHud/Scripts/GameHud.cs` | GameHud base (header/footer slide animations) |
| `Assets/Saad/GameFlow/Scripts/BaseApplicationFlowController.cs` | AFC base (orchestrates ClosePolicy) |
| `Assets/Saad/GameFlow/Scripts/ApplicationFlowController.cs` | Concrete AFC (screen event wiring) |
| `Assets/Saad/Utilities/PoolSystem/` | Object pooling system (PoolManagerSO) |
| `Assets/Saad/Utilities/CoroutineHandler/` | Static coroutine runner for ScriptableObjects |
| `Assets/Saad/Events/` | GameEvent ScriptableObject event system |

---

## Future Plans

- **More view types**: ISubView, IOverlayView — the State Creator will support additional
  state types beyond UIState and GameState
- **AI-driven prefab generation**: From mock images, auto-generate UI layout and components
- **Sprite assignment**: Drag sprites into Art/ folder, AI agent maps them to UI elements
