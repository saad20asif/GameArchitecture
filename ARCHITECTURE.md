# Game Architecture — Complete Technical Documentation

> A reusable, scene-based, ScriptableObject-driven Unity game architecture built around a Finite State Machine with stacking, pooling, event-driven communication, and a clean UI animation layer.

---

## Table of Contents

1. [High-Level Overview](#1-high-level-overview)
2. [Scene Structure](#2-scene-structure)
3. [Boot & Initialization Flow](#3-boot--initialization-flow)
4. [State Machine System](#4-state-machine-system)
5. [Application Flow Controller](#5-application-flow-controller)
6. [UI System](#6-ui-system)
7. [Pool System](#7-pool-system)
8. [Sound System](#8-sound-system)
9. [Event System](#9-event-system)
10. [Variables System](#10-variables-system)
11. [Coroutine Handler](#11-coroutine-handler)
12. [Time Machine](#12-time-machine)
13. [Analytics System](#13-analytics-system)
14. [Design Patterns Reference](#14-design-patterns-reference)
15. [Full Data Flow Diagram](#15-full-data-flow-diagram)
16. [Adding a New State — Step-by-Step](#16-adding-a-new-state--step-by-step)
17. [Namespace Reference](#17-namespace-reference)

---

## 1. High-Level Overview

The architecture is organized into **layered systems** that communicate exclusively through ScriptableObject events. No system holds a direct reference to another system's MonoBehaviour.

```
┌─────────────────────────────────────────────────────────┐
│                    APPLICATION LAYER                     │
│   ApplicationBase (MonoBehaviour, lives in SplashScene) │
│   ApplicationFlowController (MonoBehaviour, spawned)    │
└──────────────────────┬──────────────────────────────────┘
                       │ initializes
┌──────────────────────▼──────────────────────────────────┐
│                 STATE MACHINE LAYER                      │
│   FiniteStateMachine (ScriptableObject)                  │
│   State / UIViewState / SplashState (ScriptableObjects)  │
│   Transition / TimedTransition (ScriptableObjects)       │
└──────────────────────┬──────────────────────────────────┘
                       │ enters/exits
┌──────────────────────▼──────────────────────────────────┐
│                      UI LAYER                            │
│   UIBase (abstract MonoBehaviour on prefabs)             │
│   UiAnimationSystem (plain C# class)                     │
│   StateAnimationConfig (ScriptableObject)                │
└──────────────────────┬──────────────────────────────────┘
                       │ communicates via
┌──────────────────────▼──────────────────────────────────┐
│                   SERVICE LAYER                          │
│   SoundService (ScriptableObject)                        │
│   PoolManagerSO (ScriptableObject)                       │
│   TimeMachine (ScriptableObject)                         │
└──────────────────────┬──────────────────────────────────┘
                       │ stores data in
┌──────────────────────▼──────────────────────────────────┐
│                    DATA LAYER                            │
│   Variables — Int/Float/Bool/String (ScriptableObjects)  │
│   Persistent — DBInt/DBFloat/DBBool (PlayerPrefs-backed) │
│   Events — GameEvent / GameEventWithParam<T>             │
└─────────────────────────────────────────────────────────┘
```

**Key principle:** Every manager is a ScriptableObject asset. This means they survive scene loads, can be referenced by drag-and-drop across scenes, and are hot-reload friendly in the editor.

---

## 2. Scene Structure

The project uses exactly **two runtime scenes** loaded in sequence.

### SplashScene

**Purpose:** Bootstrap scene. Lives for the entire application lifetime.

| Object | Component | Role |
|--------|-----------|------|
| `ApplicationBase` | `ApplicationBase.cs` | Entry point. Sets frame rate, starts TimeMachine tick, boots FSM. |
| `Loading` | `Loading.cs` | Animates a loading bar slider by reading `SceneLoadingProgress` Float SO. |

**What SplashScene does NOT contain:** Any gameplay UI, cameras for game content, or state prefabs.

### GameScene

**Purpose:** Loaded additively from SplashScene. Hosts all actual gameplay.

**What GameScene contains at start:**
- A root `Camera` set up for UI rendering.
- The scene is otherwise empty — all content is spawned dynamically by states.

**What gets created at runtime inside GameScene:**
- `-------------------STATES-------------------` — a single empty GameObject created by `SplashState` to serve as the parent transform for all spawned UI state prefabs.
- All state UI prefabs (MainMenu, Game HUD, LevelComplete, etc.) are instantiated as children of that root.

---

## 3. Boot & Initialization Flow

This is the exact execution order when the game starts.

```
SplashScene loads
    │
    ▼
ApplicationBase.Start() [coroutine]
    ├── Application.targetFrameRate = 60
    ├── TimeMachine.Tick() coroutine started → emits Tick event every 1 second
    └── FiniteStateMachine.Init()
            │
            ▼
        BootState = SplashState.Enter()
            ├── Instantiate ApplicationFlowController from Resources/
            ├── SceneManager.LoadSceneAsync("GameScene", Additive)
            │     allowSceneActivation = false (held at 90%)
            ├── [WAIT] SceneLoadingProgress Float SO reaches >= 1.0
            │     (Loading.cs animates slider, updates the Float SO)
            ├── allowSceneActivation = true
            ├── [WAIT] asyncLoad.isDone == true
            ├── SceneManager.SetActiveScene("GameScene")
            ├── SetupStateRoots()
            │     └── Creates "STATES" GameObject in GameScene
            │     └── StateRootManager.Initialize(states.transform)
            ├── statesPooler.Initialize(StateRootManager.States)
            │     └── Creates all pool containers under States root
            │     └── Pre-creates pooled GameObjects (inactive)
            └── ApplicationFlowController.Boot()
                    └── BootFlow(MainMenuTransition, UICloseReasons.Home)
                            └── HandleTransition → ClearAll policy
                                    └── FSM.TransitionTo(MainMenuTransition)
                                            └── MainMenuState.Enter()
                                                    └── UIViewState spawns MainMenuView
                                                            └── UIBase.Show() → enter animation plays
                                                                    └── GAME READY
```

### Loading Bar Detail

`Loading.cs` uses DOTween to animate a `Slider` from 0 to 1 over a configured duration. It writes the slider value to a `Float` ScriptableObject (`SceneLoadingProgress`). `SplashState` holds a reference to the same SO and waits with `WaitUntil(() => SceneLoadingProgress.GetValue() >= 1)`. This decouples the visual loading bar from the loading logic completely.

> **Future extension point (noted in code comment):** SDK loading progress can be mapped to this same Float SO. The splash state already has a `WaitUntil` that can wait for real SDK initialization — just update the Float SO from your SDK callbacks.

---

## 4. State Machine System

**Location:** `Saad/Utilities/StateMachine/Scripts/`
**Namespace:** `Blues.Core.StateMachine`

The FSM is the backbone of the entire application. It is a **ScriptableObject**, meaning it persists across scene loads and can be referenced from any Inspector without needing a singleton.

### 4.1 FiniteStateMachine

**File:** `FiniteStateMachine.cs`
**Type:** `SerializedScriptableObject` (Odin) implementing `IState`

#### Fields

| Field | Type | Purpose |
|-------|------|---------|
| `BootState` | `State` | The first state entered when `Init()` is called. |
| `CurrentState` | `State` | The currently active state. Serialized for editor inspection. |
| `PausedStates` | `Stack<State>` | Stack of states that have been paused (overlays, popups). |
| `_pausedStateLookup` | `HashSet<State>` | O(1) check to detect if a state is already paused. |
| `currentStateSortingOrder` | `Int` (SO) | Shared sorting order variable. Increments on pause, decrements on resume. Used by `UIBase` to layer canvases. |
| `_transitionCoroutine` | `Coroutine` | Handle to the active transition coroutine. Cancelled and replaced on new transitions. |

#### Events

```csharp
public event Action<State> OnStateEntered;
public event Action<State> OnStateExited;
public event Action<State> OnStatePaused;
public event Action<State> OnStateResumed;
```

These are C# events (not ScriptableObject events) for observers that want to react to state changes (e.g., analytics, debug overlays).

#### Public API

| Method | Description |
|--------|-------------|
| `IEnumerator Init()` | Boots the FSM. Enters `BootState`. Must be called from a MonoBehaviour coroutine. |
| `void TransitionTo(Transition, bool pauseCurrent)` | Begins a transition. Cancels any in-progress transition first. |
| `IEnumerator ReloadCurrentState()` | Exits then re-enters the current state without touching the paused stack. |
| `IEnumerator ClearPausedStates()` | Exits every paused state from top to bottom. Used by `ClosePolicy.ClearAll`. |
| `IEnumerator PopPausedState()` | Exits current, resumes the top paused state. Used by `ClosePolicy.PopOne`. |
| `IEnumerator JumpTo(State target)` | Navigates to a specific previously-paused state, exiting all states above it. Used by `ClosePolicy.PopUntil`. |

#### Transition Logic (DoTransition)

```
DoTransition(transition, pauseCurrent)
    │
    ├── If CurrentState == nextState → skip (no-op, logs warning)
    ├── If nextState is in pausedStateLookup → skip (use PopPausedState instead)
    │
    ├── if (pauseCurrent || nextState.PausePreviousState)
    │       → PauseCurrentState()
    │             ├── currentStateSortingOrder.Increment(1)
    │             ├── CurrentState.Pause()
    │             └── Push to PausedStates stack + HashSet
    │
    └── else
            → ExitCurrentState()
                  └── CurrentState.Exit()
    │
    ├── transition.Execute()   [optional custom logic in transition]
    ├── CurrentState = nextState
    └── nextState.Enter(this)
```

---

### 4.2 State

**File:** `State.cs`
**Type:** `ScriptableObject`

The minimal base class for all states.

```csharp
public class State : ScriptableObject
{
    protected IState Listener;          // Reference to FSM (via IState interface)
    public bool PausePreviousState;     // If true, always pauses previous state on enter

    public virtual IEnumerator Enter(IState listener) { ... }
    public virtual IEnumerator Pause()  { ... }
    public virtual IEnumerator Resume() { ... }
    public virtual IEnumerator Exit()   { ... }
}
```

**Key design:** `Enter` receives an `IState` (the FSM), not the FSM directly. This limits what the state can do — it can only call `TransitionTo`, not access `PausedStates` or other internal FSM data. This is a controlled interface pattern.

**All lifecycle methods are coroutines.** The FSM `yield return`s each one, meaning:
- The FSM waits for `Exit()` to finish before calling `Enter()` on the next state.
- Exit animations complete before the next state appears.

---

### 4.3 UIViewState

**File:** `UiViewState.cs` (note: class name is `UIViewState`)
**Type:** `State` (ScriptableObject)

This is the standard state for all UI screens. It manages the lifecycle of a UI prefab.

#### Fields

| Field | Type | Purpose |
|-------|------|---------|
| `stateId` | `string` | ID used to Get/Release from the pool OR path in Resources folder. |
| `usePooling` | `bool` | Toggle between pooled and Resources-loaded prefab. |
| `uIStatesPooler` | `PoolManagerSO` | The pool to Get/Release from. Only shown in Inspector if `usePooling = true`. |

#### Enter Flow

```
UIViewState.Enter()
    │
    ├── [if usePooling]   viewObject = uIStatesPooler.Get(stateId)
    └── [if not pooling]  viewObject = Instantiate(Resources.Load(stateId))
                          viewObject.SetParent(StateRootManager.States)
    │
    ├── _uiInstance = viewObject.GetComponent<UIBase>()
    ├── viewObject.SetActive(true)
    └── _uiInstance.Show()          ← plays enter animation
```

#### Exit Flow

```
UIViewState.Exit()
    │
    ├── _uiInstance set to null immediately (prevents double-release)
    ├── yield return instance.Hide()    ← FSM WAITS for exit animation
    │
    ├── [if usePooling]   uIStatesPooler.Release(stateId, instance.gameObject)
    └── [if not pooling]  Destroy(_spawnedInstance)
```

**Important:** The null-before-yield pattern (`_uiInstance = null` then `yield return instance.Hide()`) prevents double-release if `Exit()` is somehow called again mid-coroutine.

#### Pause/Resume

```csharp
public override IEnumerator Pause()  { _uiInstance?.Pause();  ... }
public override IEnumerator Resume() { _uiInstance?.Resume(); ... }
```

Delegates directly to `UIBase.Pause()` / `UIBase.Resume()`.

---

### 4.4 Transition

**File:** `Transition.cs`
**Type:** `ScriptableObject`

```csharp
public class Transition : ScriptableObject
{
    public State ToState;
    public ClosePolicy closePolicy = ClosePolicy.Default;
    public virtual IEnumerator Execute() { yield break; }
}
```

- `ToState` — the state to transition to.
- `closePolicy` — overrides the default policy from `UICloseReasons` if not `Default`.
- `Execute()` — optional coroutine that runs between exit of old state and enter of new state. Subclass `Transition` to add custom logic (e.g., loading data, playing a cutscene).

**TimedTransition** extends `Transition` with a delay before `Execute()` completes.

---

### 4.5 ClosePolicy & UICloseReasons

These two enums work together to determine how the paused state stack is handled during any transition.

#### UICloseReasons

| Value | Meaning | Default Policy |
|-------|---------|----------------|
| `Home` | Going to main menu | `ClearAll` |
| `Game` | Starting a game session | `ClearAll` |
| `SkipLevel` | Skipping the current level | `PopOne` |
| `Revive` | Reviving in a level | `PopOne` |
| `ResumeGame` | Resuming after a pause overlay | `PopOne` |
| `FullScreenPlacement` | Showing an overlay (ad, popup) | `Default` → pauseCurrent = true |
| `ResumeAny` | Jump back to a specific state | `PopUntil` |
| `DailyLogin` | Daily login popup | pauseCurrent = true |
| `ShowFullScreenPlacement` | Special overlay trigger | pauseCurrent = true |

#### ClosePolicy

| Value | What Happens |
|-------|-------------|
| `ClearAll` | `ClearPausedStates()` → exits every paused state, then transitions |
| `PopOne` | `PopPausedState()` → exits current, resumes previous paused state |
| `PopUntil` | `JumpTo(target)` → exits everything until the target paused state is on top |
| `Default` | Use whatever `GetPolicyForReason()` returns for the given reason |

**Resolution order:**
1. Check if `transition.closePolicy != Default` → use transition's explicit policy.
2. Otherwise → use `GetPolicyForReason(reason)` from `BaseApplicationFlowController`.

---

### 4.6 IState Interface

```csharp
public interface IState
{
    void TransitionTo(Transition transition, bool pauseCurrent = false);
}
```

This is the only interface exposed to states. A state can trigger a transition by calling `Listener.TransitionTo(...)`, but cannot directly manipulate the FSM's paused stack or current state.

---

### 4.7 StateRootManager

**File:** `StateRootManager.cs`
**Type:** Static class

```csharp
public static class StateRootManager
{
    public static Transform States { get; }
    public static bool IsInitialized { get; }
    public static void Initialize(Transform statesRoot) { ... }
}
```

Initialized once by `SplashState` after GameScene loads. Provides the single parent transform under which all UI state prefabs are spawned. This keeps the hierarchy clean and organized under one root in GameScene.

---

## 5. Application Flow Controller

**Location:** `Saad/GameFlow/Scripts/`

This is the "router" of the application. It listens to ScriptableObject game events and decides which state to transition to.

### 5.1 BaseApplicationFlowController\<TTransition\>

**File:** `BaseApplicationFlowController.cs`
**Type:** Abstract `MonoBehaviour`

Generic base with `TTransition : Transition`. Handles:
- Back button logic (pops one paused state)
- Flow event registration/unregistration in `OnEnable`/`OnDisable`
- `GetPolicyForReason` mapping of `UICloseReasons` → `ClosePolicy`
- `ShouldPauseCurrent` logic (returns true for overlay-type reasons)
- `HandleTransition` coroutine which applies the policy then calls `FSM.TransitionTo`

```csharp
protected virtual IEnumerator HandleTransition(TTransition transition, ClosePolicy policy, bool pauseCurrent)
{
    // 1. Apply stack policy first
    switch (policy)
    {
        case ClosePolicy.ClearAll:  yield return fsm.ClearPausedStates(); break;
        case ClosePolicy.PopOne:    yield return fsm.PopPausedState();    break;
        case ClosePolicy.PopUntil:  yield return fsm.JumpTo(target);      yield break;
    }
    // 2. Then execute the transition
    fsm.TransitionTo(transition, pauseCurrent);
}
```

**Note:** `PopUntil` uses `yield break` after `JumpTo` because `JumpTo` already handles entering the target state internally.

### 5.2 ApplicationFlowController

**File:** `ApplicationFlowController.cs`
**Type:** Concrete `BaseApplicationFlowController<Transition>` — instantiated from `Resources/ApplicationFlowController`

Wires up all the game's state routing:

| Event SO | Transition | Reason |
|----------|-----------|--------|
| `GoToMainMenuEvent` (int param) | `MainMenuTransition` | `(UICloseReasons)reasonId` |
| `GoToSpinWheelEvent` | `SpinWheelTransition` | `FullScreenPlacement` |
| `GoToGameEvent` | `GameTransition` | `Game` |
| `GoToLevelCompleteEvent` | `LevelCompleteTransition` | `FullScreenPlacement` |
| `GoToLevelFailEvent` | `LevelFailTransition` | `FullScreenPlacement` |
| `GoToRateUsEvent` | `RateUsTransition` | `FullScreenPlacement` |

`GoToMainMenu` takes an int so that callers can pass different `UICloseReasons` — the "Home" button sends `Home` (ClearAll), a "Revive" button would send `Revive` (PopOne), etc.

`Boot()` is called by `SplashState` to start the first transition to `MainMenuState`.

---

## 6. UI System

**Location:** `Saad/UI/Base/Scripts/`
**Namespace:** `Blues.Core.UI`

### 6.1 UIBase

**File:** `UIBase.cs`
**Type:** Abstract `MonoBehaviour` implementing `IShowable`

Every UI screen prefab has a component that extends `UIBase`.

#### Fields

| Field | Type | Purpose |
|-------|------|---------|
| `animationConfig` | `StateAnimationConfig` | Defines enter/exit animation type, duration, easing. |
| `UIPanel` | `RectTransform` | The panel animated by `UiAnimationSystem`. |
| `currentStateSortingOrder` | `Int` (SO) | Shared with FSM; determines canvas layer order. |
| `_canvas` | `Canvas` | Set in Awake. `worldCamera` auto-assigned to `Camera.main`. |
| `_canvasGroup` | `CanvasGroup` | Used for alpha fade and raycast blocking. |
| `_animationSystem` | `UiAnimationSystem` | Created in Awake, handles all tween logic. |
| `_isHiding` | `bool` | Guard flag to prevent double-hide calls. |
| `Paused` | `bool` (property) | Tracks paused state for interactivity control. |

#### Show()

```
Show()
    ├── canvas.sortingOrder = currentStateSortingOrder.GetValue()
    ├── canvas.planeDistance = 5
    ├── gameObject.SetActive(true)
    └── animationSystem.PlayAnimation(Enter, callback: MakeStateInteractable(true))
```

Called by `UIViewState.Enter()`. The UI is not interactable until the enter animation completes.

#### Hide() [Coroutine]

```
Hide()
    ├── Guard: if _isHiding → yield break
    ├── _isHiding = true
    ├── MakeStateInteractable(false)    ← immediately blocks input
    ├── animationSystem.PlayAnimation(Exit, callback: completed = true)
    ├── [WAIT] yield return new WaitUntil(() => completed)
    ├── gameObject.SetActive(false)
    └── _isHiding = false
```

**The FSM waits here.** The coroutine only completes after the exit animation finishes. This guarantees clean visual transitions with no flickering.

#### Pause() / Resume()

- `Pause()`: Blocks raycasts, calls `ForceCompleteCurrentAnimation()` to snap animations to end state.
- `Resume()`: Unblocks raycasts, calls `ForceCompleteCurrentAnimation()` again (resolves any in-progress tween), then re-enables interaction.

**Interactivity:** Uses `_canvasGroup.blocksRaycasts` (not `interactable`). This means paused screens are visible but cannot receive input.

---

### 6.2 UiAnimationSystem

**File:** `UiAnimationSystem.cs`
**Type:** Plain C# class (not MonoBehaviour)

Encapsulates all DOTween animation logic. Created and owned by `UIBase`.

#### Constructor

```csharp
public UiAnimationSystem(MonoBehaviour owner, CanvasGroup cg, RectTransform panel, StateAnimationConfig config)
```

#### PlayAnimation(phase, onComplete)

1. If already animating → `ForceCompleteCurrentAnimation()` first.
2. Set initial state (alpha 0 for fade-in, small scale for scale-in, off-screen for slide-in).
3. Run DOTween tween to target state.
4. Call `onComplete` when tween finishes.

#### ForceCompleteCurrentAnimation()

Calls `tween.Complete()` then `tween.Kill()`. This snaps the UI to its final state immediately. Used by `Pause()` and `Resume()`.

#### Animation Types (`DefaultAnimationType`)

| Type | Enter | Exit |
|------|-------|------|
| `Fade` | alpha 0 → 1 | alpha 1 → 0 |
| `Scale` | `StartScale` → 1 | 1 → `StartScale` |
| `Slide` | off-screen right → center | center → off-screen left |
| `None` | instant | instant |

#### Slide Direction

- Enter: comes from `+Screen.width` (right side).
- Exit: goes to `-Screen.width` (left side).

---

### 6.3 StateAnimationConfig

**File:** `StateAnimationConfig.cs`
**Type:** `ScriptableObject` (`CreateAssetMenu`)

| Field | Type | Purpose |
|-------|------|---------|
| `EnterAnimationType` | `DefaultAnimationType` | Which animation plays on enter. |
| `ExitAnimationType` | `DefaultAnimationType` | Which animation plays on exit. |
| `EnterDuration` | `float` | Duration of enter animation in seconds. |
| `ExitDuration` | `float` | Duration of exit animation in seconds. |
| `EnterEase` | `Ease` (DOTween) | Easing curve for enter. |
| `ExitEase` | `Ease` (DOTween) | Easing curve for exit. |
| `StartScale` | `float` | For Scale type: starting (and ending) scale value. |

One config asset per UI screen, or shared across screens with similar animations.

---

### 6.4 IShowable Interface

```csharp
public interface IShowable
{
    void Show();
    IEnumerator Hide();
    void Pause();
    void Resume();
}
```

`UIBase` implements this. `UIViewState` only works with `IShowable`, not `UIBase` directly — though in practice the field is typed as `UIBase`.

---

### 6.5 Concrete UI States

All follow the same pattern: a `State` (ScriptableObject) paired with a `UIBase` subclass (MonoBehaviour on prefab).

| State ScriptableObject | View MonoBehaviour | Notes |
|------------------------|-------------------|-------|
| `MainMenuState` | `MainMenuView` | Play button raises `GoToGameEvent`, SpinWheel button raises `GoToSpinWheelEvent`. |
| `GameState` / `NormalGameState` | `GameHud` / `NormalGameHud` | Loads both a gameplay prefab and HUD prefab. HUD slides header/footer bars. Subscribes to `TimeMachine.Tick`. |
| `LevelCompleteState` | `LevelCompleteView` | Next button raises `GoToMainMenuEvent` with `Home` reason. |
| `LevelFailState` | `LevelFailView` | Retry or Home buttons raise appropriate events. |
| `SpinWheelState` | `SpinWheelView` | Loads spin data from JSON. Back button pops to MainMenu. |
| `RateUsState` | `RateUsView` | Back button pops. |

---

## 7. Pool System

**Location:** `Saad/Utilities/PoolSystem/Scripts/`
**Namespace:** `Blues.Core.PoolSystem`, `ProjectCore.PoolSystem`

### 7.1 PoolManagerSO

**File:** `PoolManagerSO.cs`
**Type:** `ScriptableObject`

The central pool registry. One instance manages N pools, each identified by a string ID.

#### PoolConfig (nested class)

```csharp
public class PoolConfig
{
    public string PoolID;           // Unique string key
    public GameObject Prefab;       // The prefab to pool
    public int DefaultCapacity;     // Initial pool size (default 10)
    public int MaxSize;             // Hard cap (default 100)
    public bool Prewarm;            // Pre-instantiate DefaultCapacity objects on init
}
```

#### Initialize(Transform poolRoot)

Called by `SplashState` after GameScene loads. Creates one sub-container GameObject per pool under the `poolRoot` transform. Each container is named `{PoolID}_Pool`.

If `Prewarm = true`, the pool immediately instantiates `DefaultCapacity` inactive objects. This eliminates first-time Get latency.

#### Get(string poolId) → GameObject

Returns an active GameObject from the pool. Calls `IPoolable.OnPoolGet()` if the object implements it. Throws `KeyNotFoundException` if pool ID doesn't exist.

#### Release(string poolId, GameObject)

Returns a GameObject to the pool. Calls `IPoolable.OnPoolRelease()`. Re-parents the object back to its pool container. Falls back to `Destroy()` if pool ID not found.

#### ReleaseAfterDelay(string poolId, GameObject, float delay)

Coroutine-based delayed release. Uses `PoolManagerRunner` (a persistent MonoBehaviour) to run the coroutine without needing a scene object.

### 7.2 UnityObjectPool\<T\>

**File:** `UnityObjectPool.cs`
**Type:** Generic class wrapping `Unity.ObjectPool`

Wraps Unity's built-in object pool with automatic activation/deactivation:
- `Get()` → activates object, returns it.
- `Release()` → deactivates object, returns to pool.
- `TryGet()` → safe version, returns bool + out param.
- `Prewarm()` → pre-populates the pool.

### 7.3 IPoolable Interface

```csharp
public interface IPoolable
{
    void OnPoolGet();       // Called when taken from pool
    void OnPoolRelease();   // Called when returned to pool
}
```

Implement on any pooled GameObject component to hook into pool lifecycle events. Useful for resetting state, clearing particle systems, stopping sounds, etc.

### 7.4 Supporting Components

**PoolManagerRunner:** Minimal `MonoBehaviour` created by `PoolManagerSO` when first needed. Marked `DontDestroyOnLoad`. Its only purpose is to own coroutines for `ReleaseAfterDelay`.

**PooledFxAutoRelease:** Attach to particle effect prefabs. Arms delayed auto-release when the effect is done. Also clears particle emission on release to prevent residual particles after pool return.

**PoolMonitor:** Editor debug component that shows pool statistics (active/inactive/total counts) in real-time.

---

## 8. Sound System

**Location:** `Saad/Utilities/SoundSystem/Scripts/`

### 8.1 SoundService

**File:** `SoundService.cs`
**Type:** `ScriptableObject`

#### Sound Data Class

```csharp
public class Sound
{
    public string SoundName;
    public SoundType Type;           // Music, SFX, UI
    public AudioClip[] Clips;        // Random selection if multiple
    public float Volume;
    public float Pitch;
    public bool Loop;
    public bool PlayOnAwake;
    public bool RandomPitch;
    public float PitchVariance;      // +/- range for random pitch
    // Internal:
    public AudioSource ActiveSource; // Set when playing
}
```

#### Core Methods

| Method | Description |
|--------|-------------|
| `Play(soundName)` | Gets AudioSource from pool, configures it, plays clip. Respects mute. |
| `Play(soundName, position)` | Spatial 3D audio version. |
| `Stop(soundName)` | Stops a named sound, releases its AudioSource back to pool. |
| `FadeIn(soundName, duration)` | DOTween fade from 0 to target volume. |
| `FadeOut(soundName, duration)` | DOTween fade to 0, then stop. |
| `StopAllSounds()` | Stops and releases all active AudioSources. |

### 8.2 AudioSourcePool

**File:** `AudioSourcePool.cs`
**Type:** Extends `UnityObjectPool<AudioSource>`

Pool of AudioSource components on GameObjects. When an AudioSource finishes playing (detected via DSP time), it is automatically returned to the pool via the `AutoReturnToPool` component attached to each pooled AudioSource object.

**No GC allocation** on return — uses DSP time comparison, not coroutines.

### 8.3 SoundSettings

**File:** `SoundSettings.cs`
**Type:** `ScriptableObject`

| Field | Purpose |
|-------|---------|
| `MasterVolume` | 0–1 multiplier for all sounds |
| `SFXVolume` | 0–1 multiplier for SFX type |
| `UIVolume` | 0–1 multiplier for UI sounds |
| `MusicVolume` | 0–1 multiplier for music |
| `Muted` | Bool; when true, `SoundService.Play()` skips playback |
| `MixerGroups` | AudioMixer group assignments per type |

---

## 9. Event System

**Location:** `Saad/Utilities/Events/`
**Namespace:** `Blues.Core.Events`

All events are ScriptableObjects. They are drag-and-drop referenced in the Inspector. There are no hardcoded string names or static accessors.

### 9.1 GameEvent (no params)

```csharp
[CreateAssetMenu(...)]
public class GameEvent : ScriptableObject
{
    private Action _onEvent;

    public void Invoke()               { _onEvent?.Invoke(); }
    public void Subscribe(Action cb)   { _onEvent += cb; }
    public void UnSubscribe(Action cb) { _onEvent -= cb; }

    private void OnDisable()           { _onEvent = null; } // Memory safety
}
```

`OnDisable()` clears all subscribers when the SO is unloaded (e.g., domain reload in editor). Prevents stale callbacks.

### 9.2 GameEventWithParam\<T\>

```csharp
public class GameEventWithParam<T> : ScriptableObject
{
    private Action<T> _onEvent;

    public void Invoke(T value) { ... }
    public void Subscribe(Action<T> cb) { ... }
    public void UnSubscribe(Action<T> cb) { ... }
}
```

**Concrete typed subclasses:**
- `GameEventWithInt` → `GameEventWithParam<int>` (serializable, shows in Inspector)
- `GameEventWithFloat`, `GameEventWithBool`, `GameEventWithString`, etc.

### 9.3 Multi-Parameter Events

```csharp
GameEventWithParam<T1, T2>           // Two-param event
GameEventWithParam<T1, T2, T3>       // Three-param event
```

### 9.4 GameEventWithReturn\<T\>

```csharp
public class GameEventWithReturn<T> : ScriptableObject
{
    private Func<T> _onEvent;
    public T Invoke() { return _onEvent != null ? _onEvent() : default; }
    ...
}
```

Used for queries — one system asks another for a value. `GameEventWithReturnInt` is the concrete int version.

---

## 10. Variables System

**Location:** `Saad/Variables/`
**Namespace:** `Blues.Core.Variables`

All variables are ScriptableObjects. They serve as shared, named values that any system can read or write by holding a reference to the SO asset.

### 10.1 Non-Persistent Variables

Reset to default on play-mode entry (via `OnEnable`).

| Type | Methods |
|------|---------|
| `Int` | `GetValue()`, `SetValue(int)`, `Increment(int)`, `Decrement(int)` |
| `Float` | `GetValue()`, `SetValue(float)` |
| `Bool` | `GetValue()`, `SetValue(bool)`, `Toggle()` |
| `String` | `GetValue()`, `SetValue(string)` |
| `Double` | Similar |
| `SharedVector2/3` | Wraps Vector2/3 |

**WithEvent variants** (e.g., `IntWithEvent`, `FloatWithEvent`): Emit a `GameEvent` whenever their value changes. Subscribe to get notified of changes without polling.

### 10.2 Persistent Variables

Backed by `PlayerPrefs`. Automatically saved on `SetValue()`, loaded on `GetValue()`.

| Type | PlayerPrefs Method |
|------|--------------------|
| `DBInt` | `PlayerPrefs.GetInt` / `SetInt` |
| `DBFloat` | `PlayerPrefs.GetFloat` / `SetFloat` |
| `DBBool` | `PlayerPrefs.GetInt` (0/1) |
| `DBString` | `PlayerPrefs.GetString` / `SetString` |

Each requires a unique `Key` string. `KeyValidator` is a helper to enforce key uniqueness across the project.

**WithEvent variants** (`DBIntWithEvent`, etc.) work identically to non-persistent with-event variants.

### 10.3 Collections

| Type | Description |
|------|-------------|
| `SharedDictionary<TKey, TValue>` | ScriptableObject dictionary |
| `SharedDictionaryIntString` | Concrete int→string dictionary |
| `List2D<T>` | 2D list wrapper |

---

## 11. Coroutine Handler

**Location:** `Saad/Utilities/CoroutineHandler/Scripts/`
**Namespace:** `THEBADDEST.Coroutines`

### 11.1 CoroutineHandler (Static)

Provides static coroutine execution without needing a MonoBehaviour reference. The `MonoRunner` is automatically created on first use via `[RuntimeInitializeOnLoadMethod]`.

```csharp
// Static API
CoroutineHandler.StartStaticCoroutine(IEnumerator)   → Coroutine
CoroutineHandler.StopStaticCoroutine(Coroutine)
CoroutineHandler.StopAll()

// Frame updates
CoroutineHandler.DoUpdate(Action)          // Subscribe to Update
CoroutineHandler.RemoveUpdate(Action)      // Unsubscribe

// Delayed callbacks
CoroutineHandler.AfterWait(float, Action)           // Wait seconds
CoroutineHandler.AfterWaitRealtime(float, Action)   // Unscaled time
CoroutineHandler.AfterFrames(int, Action)           // Wait N frames

// Conditional
CoroutineHandler.WaitLoop(Func<bool> condition, Action onComplete)
```

**Used by:** `FiniteStateMachine.TransitionTo()` — the FSM uses `CoroutineHandler` instead of a MonoBehaviour because FSMs are ScriptableObjects.

### 11.2 MonoRunner

A hidden `MonoBehaviour` singleton created automatically. Never visible in the hierarchy (`hideFlags = HideFlags.HideInHierarchy`). Lives in `DontDestroyOnLoad`.

### 11.3 Supporting Types

| Type | Purpose |
|------|---------|
| `CoroutineDelay` | Encapsulates a timed delay coroutine |
| `CoroutineCondition` | Encapsulates a condition-wait coroutine |
| `CoroutineUpdate` | Manages per-frame callback subscriptions |
| `CoroutineSequence` | Chains multiple coroutines sequentially |
| `CoroutineLoop` | Runs a coroutine in a loop |

---

## 12. Time Machine

**Location:** `Saad/TimeMachine/Scripts/`
**Namespace:** `Blues.Core.TheTimeMachine`

### TimeMachine (ScriptableObject)

```csharp
public class TimeMachine : ScriptableObject
{
    [SerializeField] private GameEvent Tick;

    public IEnumerator Tick()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            Tick.Invoke();
        }
    }
}
```

Started by `ApplicationBase.Start()`. Emits the `Tick` `GameEvent` every second. Any system that needs a 1-second timer subscribes to this event (e.g., `GameHud` for displaying elapsed time).

### TimeManager (Static)

```csharp
public static string FormatTime(int totalSeconds)
// Returns "MM:SS" format string
```

Utility for formatting second counts into display strings.

---

## 13. Analytics System

**Location:** `Assets/Analytics/Scripts/`

Built around interfaces so the implementation can be swapped without changing callers.

| Interface | Purpose |
|-----------|---------|
| `IEconomyAnalytics` | Track currency earn/spend events |
| `ILevelAnalytics` | Track level start/complete/fail |
| `IUIInteractionAnalytics` | Track button presses and screen views |

**Concrete loggers:**
- `FirebaseAnalyticsLogger` — sends events to Firebase.
- `GameAnalyticsLogger` — sends events to GameAnalytics SDK.

`AnalyticsSystem` holds references to all active loggers and fans events out to each one.

---

## 14. Design Patterns Reference

| Pattern | Where Used | Why |
|---------|-----------|-----|
| **State** | `FiniteStateMachine`, `State`, `UIViewState` | Clean game flow with enter/exit/pause/resume lifecycle |
| **Observer** | `GameEvent`, `GameEventWithParam<T>` | Decoupled communication between systems |
| **Object Pool** | `PoolManagerSO`, `UnityObjectPool<T>`, `AudioSourcePool` | Reduce GC allocations for frequently created/destroyed objects |
| **ScriptableObject Architecture** | All managers, all events, all variables | Scene-agnostic data, hot-reload, Inspector configuration |
| **Template Method** | `State` base class with virtual Enter/Exit/Pause/Resume | States override only what they need |
| **Strategy** | `UiAnimationSystem` selecting Fade/Scale/Slide | Swappable animation behavior per screen |
| **Generic Constraint** | `BaseApplicationFlowController<TTransition>`, `UnityObjectPool<T>` | Type-safe, reusable base classes |
| **Controlled Interface** | `IState` passed to states | States can trigger transitions but cannot access FSM internals |
| **Coroutine as Async** | All state lifecycle methods | Waitable, cancellable async operations without async/await overhead |
| **Null-Before-Yield** | `UIViewState.Exit()` | Prevents double-release in concurrent coroutine scenarios |

---

## 15. Full Data Flow Diagram

### Example: User taps "Play" on Main Menu

```
[MainMenuView Button click]
    │
    ▼
MainMenuView.OnPlayClicked()
    └── GoToGameEvent.Invoke()                          [GameEvent SO]
            │
            ▼
    ApplicationFlowController.GoToGameEvent.Subscribe callback fires
            └── GoTo(GameTransition, UICloseReasons.Game)
                    └── BaseApplicationFlowController.HandleTransition()
                            ├── policy = ClearAll (Game reason)
                            ├── yield return FSM.ClearPausedStates()   [clears any overlays]
                            └── FSM.TransitionTo(GameTransition, pauseCurrent=false)
                                    └── DoTransition coroutine
                                            ├── yield return MainMenuState.Exit()
                                            │       └── UIViewState.Exit()
                                            │               ├── UIBase.Hide()
                                            │               │       ├── blocks raycasts
                                            │               │       ├── plays exit animation (DOTween)
                                            │               │       ├── [WAIT] animation done
                                            │               │       └── SetActive(false)
                                            │               └── Pool.Release("MainMenu", go)
                                            │
                                            ├── yield return GameTransition.Execute()  [no-op]
                                            │
                                            └── yield return GameState.Enter(fsm)
                                                    ├── Instantiate gameplay prefab
                                                    ├── Instantiate HUD prefab
                                                    └── GameHud.Show()
                                                            ├── canvas.sortingOrder = currentStateSortingOrder
                                                            ├── plays enter animation
                                                            └── [WAIT] animation done → MakeInteractable(true)
                                                                    └── GAME IS NOW PLAYABLE
```

### Example: Overlay (Spin Wheel) on top of Game

```
[Some button raises GoToSpinWheelEvent]
    │
    ▼
ApplicationFlowController
    └── GoTo(SpinWheelTransition, UICloseReasons.FullScreenPlacement)
            ├── ShouldPauseCurrent(FullScreenPlacement) = true
            └── FSM.TransitionTo(SpinWheelTransition, pauseCurrent=true)
                    └── DoTransition
                            ├── pauseCurrent = true → PauseCurrentState()
                            │       ├── currentStateSortingOrder.Increment(1)
                            │       ├── GameState.Pause() → GameHud.Pause()
                            │       │       ├── blocks raycasts
                            │       │       └── ForceCompleteCurrentAnimation()
                            │       └── Push GameState to PausedStates stack
                            │
                            └── SpinWheelState.Enter()
                                    └── UIViewState spawns SpinWheelView
                                            └── Show() with sorting order +1

[User closes Spin Wheel]
    │
    ▼
SpinWheelView back button → GoToMainMenuEvent.Invoke(UICloseReasons.Home as int)
    │
    ▼
ApplicationFlowController.GoToMainMenu(reasonId=0)
    └── GoTo(MainMenuTransition, UICloseReasons.Home)
            ├── policy = ClearAll
            ├── FSM.ClearPausedStates()
            │       └── GameState.Exit() (was paused) → GameHud.Hide() → Pool.Release
            └── FSM.TransitionTo(MainMenuTransition)
                    └── SpinWheelState.Exit() → SpinWheelView.Hide() → Pool.Release
                    └── MainMenuState.Enter() → Pool.Get("MainMenu") → Show()
```

---

## 16. Adding a New State — Step-by-Step

### Step 1: Create the View MonoBehaviour

```csharp
// MyNewView.cs
using Blues.Core.UI;
using Blues.Core.Events;
using UnityEngine;

public class MyNewView : UIBase
{
    [SerializeField] private GameEvent goSomewhereEvent;

    public void OnMyButton()
    {
        goSomewhereEvent.Invoke();
    }
}
```

### Step 2: Create the View Prefab

1. Create a new Canvas prefab in your UI folder.
2. Add `MyNewView` component.
3. Add `Canvas` + `CanvasGroup` components.
4. Assign a `StateAnimationConfig` asset.
5. (Optional) Assign `currentStateSortingOrder` Int SO.
6. Name it clearly (e.g., `MyNewView`).

### Step 3: Register the Prefab in a PoolManagerSO

1. Open your states `PoolManagerSO` asset.
2. Add a new `PoolConfig` entry.
3. Set `PoolID` = `"MyNewView"` (this is the `stateId` in UIViewState).
4. Assign the prefab.
5. Set capacity and whether to prewarm.

### Step 4: Create the State ScriptableObject

1. Right-click in Project → `Create → ProjectCore → State Machine → States`.
   - If `UIViewState` is in the menu, use that. Otherwise create a new State SO asset.
2. If you need custom logic, create a script:

```csharp
// MyNewState.cs
using Blues.Core.StateMachine;
using System.Collections;

[CreateAssetMenu(menuName = "ProjectCore/State Machine/States/MyNewState")]
public class MyNewState : UIViewState
{
    public override IEnumerator Enter(IState listener)
    {
        yield return base.Enter(listener); // spawns and shows the view
        // your custom enter logic here
    }

    public override IEnumerator Exit()
    {
        // custom exit logic here
        yield return base.Exit(); // hides and releases the view
    }
}
```

3. In the Inspector, set `stateId = "MyNewView"`, `usePooling = true`, assign `uIStatesPooler`.

### Step 5: Create a Transition ScriptableObject

1. Right-click → `Create → ProjectCore → State Machine → Transitions → Basic Transition`.
2. Assign `ToState` = your new State SO.
3. Set `closePolicy` if you want to override the default.

### Step 6: Wire into ApplicationFlowController

```csharp
// In ApplicationFlowController.cs
[Header("My New State")]
[SerializeField] private GameEvent GoToMyNewStateEvent;
[SerializeField] private Transition MyNewStateTransition;

protected override void RegisterFlowEvents()
{
    // ... existing registrations ...
    GoToMyNewStateEvent.Subscribe(() => GoTo(MyNewStateTransition, UICloseReasons.FullScreenPlacement));
}

protected override void UnregisterFlowEvents()
{
    // ... existing unregistrations ...
    GoToMyNewStateEvent.UnSubscribe(() => GoTo(MyNewStateTransition, UICloseReasons.FullScreenPlacement));
}
```

### Step 7: Create the Event ScriptableObject

1. Right-click → `Create → ProjectCore → Events → GameEvent`.
2. Name it `GoToMyNewStateEvent`.
3. Assign it in `ApplicationFlowController` Inspector.
4. Assign the same asset to `MyNewView.goSomewhereEvent`.

---

## 17. Namespace Reference

| Namespace | Location | Contents |
|-----------|----------|----------|
| `Blues.Core.StateMachine` | `Saad/Utilities/StateMachine/` | FSM, State, UIViewState, Transition, IState |
| `Blues.Core.Application` | `Saad/GameFlow/Scripts/` | ApplicationBase, SplashState, StateRootManager |
| `Blues.Core.UI` | `Saad/UI/Base/Scripts/` | UIBase, IShowable, UiAnimationSystem, UICloseReasons, ClosePolicy |
| `Blues.Core.Events` | `Saad/Utilities/Events/` | GameEvent, GameEventWithParam<T>, GameEventWithReturn<T> |
| `Blues.Core.Variables` | `Saad/Variables/` | Int, Float, Bool, DBInt, DBFloat, etc. |
| `Blues.Core.PoolSystem` | `Saad/Utilities/PoolSystem/` | PoolManagerSO, UnityObjectPool<T>, IPoolable |
| `Blues.Core.TheTimeMachine` | `Saad/TimeMachine/` | TimeMachine, TimeManager |
| `THEBADDEST.Coroutines` | `Saad/Utilities/CoroutineHandler/` | CoroutineHandler, MonoRunner |

---

*Last updated: 2026-04-12*
