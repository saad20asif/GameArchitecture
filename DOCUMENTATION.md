# Game Architecture Documentation

A comprehensive review of the Unity game architecture project, covering every major script and system.

---

## Table of Contents
1. [Architecture Overview](#1-architecture-overview)
2. [Project Structure](#2-project-structure)
3. [Core Systems](#3-core-systems)
4. [Script Reference](#4-script-reference)
5. [Data Flow](#5-data-flow)
6. [Extension Guide](#6-extension-guide)

---

## 1. Architecture Overview

This project is a **ScriptableObject-driven, state-machine-based** Unity architecture for mobile/casual games. Key characteristics:

- **State Machine (FSM)**: Central flow control using `FiniteStateMachine` (ScriptableObject) with support for pause/resume stacks
- **Event-Driven Navigation**: `GameEvent` ScriptableObjects decouple UI/systems from flow logic
- **Pooling**: Object pooling for UI states and FX to minimize allocations
- **Persistent Variables**: ScriptableObject variables with optional PlayerPrefs persistence
- **Modular UI**: `UIBase` + `IShowable` pattern for consistent show/hide/pause/resume behavior

### High-Level Flow

```
ApplicationBase (Start)
    → TimeMachine.Tick() (background)
    → FiniteStateMachine.Init() → BootState (SplashState)
        → Load GameScene additively
        → Initialize StateRootManager, Pools
        → ApplicationFlowController.Boot() → MainMenu
    → User interacts → GameEvents fire → FlowController.GoTo() → FSM.TransitionTo()
```

---

## 2. Project Structure

```
Assets/
├── Saad/
│   ├── GameFlow/           # Application bootstrap, splash, flow control
│   ├── UI/                 # UI views, states, HUD
│   │   ├── Base/           # UIBase, animations, interfaces
│   │   ├── MainMenu/
│   │   ├── GameState/
│   │   ├── GameHud/
│   │   ├── LevelComplete/
│   │   ├── LevelFail/
│   │   ├── SpinWheel/      # Legacy spin wheel
│   │   ├── Spin Wheel/     # New spin wheel (Views/Data/Controller)
│   │   └── RateUs/
│   ├── Utilities/
│   │   ├── StateMachine/   # FSM, State, Transition, UIViewState
│   │   ├── Events/         # GameEvent, GameEventWithParam
│   │   ├── SoundSystem/
│   │   ├── PoolSystem/
│   │   ├── UIAnimations/
│   │   ├── TextAnimations/
│   │   ├── SwipeSystem/
│   │   └── Json/
│   ├── Variables/          # Int, Bool, Float, DB* (persistent)
│   ├── TimeMachine/
│   └── Design Patterns/    # Command pattern (undo/redo)
└── Editor/
```

---

## 3. Core Systems

### 3.1 Finite State Machine (FSM)

| Script | Purpose |
|--------|---------|
| `FiniteStateMachine` | ScriptableObject FSM. Manages `CurrentState`, `PausedStates` stack, transitions. Fires `OnStateEntered`, `OnStateExited`, `OnStatePaused`, `OnStateResumed`. |
| `State` | Base ScriptableObject for states. Virtual `Enter`, `Pause`, `Resume`, `Exit`. Stores `Listener` (IState/FSM ref). |
| `Transition` | ScriptableObject with `ToState`, `closePolicy`, optional `Execute()` coroutine. |
| `UIViewState` | State that spawns a UI from pool/Resources, gets `UIBase`, calls Show/Hide. Supports `usePooling`, `stateId`. |
| `IState` | Interface for FSM: `TransitionTo`, `ClearPausedStates`, `ReloadCurrentState`. |

**Close Policies** (driven by `UICloseReasons`):
- `ClearAll`: Clear entire paused stack
- `PopOne`: Pop top paused state and resume
- `PopUntil`: Jump to a specific state in paused stack
- `Default`: Use policy from `GetPolicyForReason`

### 3.2 Application Flow

| Script | Purpose |
|--------|---------|
| `ApplicationBase` | MonoBehaviour entry point. Sets frame rate, starts `TimeMachine.Tick()`, yields `FSM.Init()`. |
| `BaseApplicationFlowController` | Abstract. Subscribes to `backBtnPressedEvent`, implements `BootFlow`, `GoTo`, `HandleTransition`, `GetPolicyForReason`, `ShouldPauseCurrent`. Registers/unregisters flow events. |
| `ApplicationFlowController` | Concrete flow. Maps events: GoToMainMenu, GoToSpinWheel, GoToGame, LevelComplete, LevelFail, RateUs. Calls `BootFlow(MainMenuTransition, UICloseReasons.Home)` on boot. |
| `SplashState` | Boot state. Loads GameScene additively, waits for loading progress, sets active scene, initializes `StateRootManager`, pools, spawns `ApplicationFlowController` from Resources, calls `Boot()`. |
| `StateRootManager` | Static. `States` (Transform), `IsInitialized`. Set by SplashState. |
| `Loading` | Simple loading UI: DOTween slider, updates `Float` load value. |

### 3.3 UI System

| Script | Purpose |
|--------|---------|
| `UIBase` | Abstract MonoBehaviour. `Show()`, `Hide()` (IEnumerator), `Pause()`, `Resume()`. Uses `UiAnimationSystem` for enter/exit. Canvas, CanvasGroup, sorting order from `Int currentStateSortingOrder`. |
| `IShowable` | Interface: `Show`, `Hide`, `Pause`, `Resume`. |
| `UiAnimationSystem` | Non-Mono. Plays Fade/Scale/Slide via DOTween using `StateAnimationConfig` (enter/exit types, durations, ease). |
| `StateAnimationConfig` | ScriptableObject: `EnterAnimationType`, `ExitAnimationType`, durations, ease, `StartScale`. |
| `UiCloseReasons` | Enum: Home, Game, SkipLevel, Revive, ResumeGame, FullScreenPlacement, ResumeAny, DailyLogin, ShowFullScreenPlacement. |
| `ClosePolicy` | Enum: Default, ClearAll, PopUntil, PopOne. |

### 3.4 Events

| Script | Purpose |
|--------|---------|
| `GameEvent` | ScriptableObject. Subscribe/UnSubscribe(void). Invoke(). Clears handlers OnDisable. |
| `GameEventWithParam<T>` | Base for parameterized events. Raise(T). |
| `GameEventWithInt` | GameEventWithParam<int>. Used for GoToMainMenu(reasonId). |

### 3.5 Variables

| Script | Purpose |
|--------|---------|
| `Int` | ScriptableObject. Value, DefaultValue, ResetToDefaultOnPlay. SetValue, Increment, Decrement, GetValue. |
| `DBInt` | Int + PlayerPrefs. Key, Save/Load, overrides SetValue/Increment/Decrement to persist. |
| `DBBool`, `DBBoolWithEvent` | Bool with persistence. DBBoolWithEvent invokes GameEvent on SetValue. |
| `Float` | Similar to Int for floats. |
| `KeyValidator` | Helper to ensure unique keys for DB variables. |

### 3.6 Sound System

| Script | Purpose |
|--------|---------|
| `SoundService` | ScriptableObject. Initialize (creates AudioSourcePool, DontDestroyOnLoad parent). Play(name, delay), Play(name, position), FadeIn/FadeOut, Stop, StopAllSounds. Respects DBBoolWithEvent mute. |
| `Sound` | Serializable. soundName, type (SFX/UI/Music), clip(s), loop, volume, pitch, random pitch. |
| `AudioSourcePool` | ObjectPool<AudioSource>. AutoReturnToPool uses DSP time for allocation-free return. |
| `SoundSettings` | AudioMixer routing. |
| `SoundExtensions` | Extension methods for SoundService. |

### 3.7 Pool System

| Script | Purpose |
|--------|---------|
| `PoolManagerSO` | ScriptableObject. PoolConfig[] (PoolID, Prefab, DefaultCapacity, MaxSize, Prewarm). Uses UnityObjectPool<GameObject>. Get/Release, ReleaseAfterDelay. IPoolable OnPoolGet/OnPoolRelease. |
| `PoolManagerRunner` | DontDestroyOnLoad MonoBehaviour for coroutines. |
| `PooledFxAutoRelease` | Component. Arm(pool, poolId, delay) to auto-release FX after delay. |
| `PoolMonitor` | Editor/debug pool stats. |

### 3.8 Time Machine

| Script | Purpose |
|--------|---------|
| `TimeMachine` | ScriptableObject. Tick() coroutine invokes GameEvent every 1 second. Used for timers, daily rewards, etc. |

### 3.9 Game States & Views

| Script | Purpose |
|--------|---------|
| `GameState` | Abstract State. Loads gameplay prefab + HUD from pool/Resources. Fires GameStateEnter/Paused/Resumed/Exit. GameHud implements IShowable. |
| `NormalGameState` | GameState. GoToLevelCompleteEvent, GoToLeveLFailEvent. |
| `MainMenuState` | UIViewState. GoToGameEvent, GoToSpinWheelEvent. |
| `MainMenuView` | UIBase. Buttons → MainMenuState.GoToPlayState(), GoToSpinWheel(). |
| `LevelCompleteState`, `LevelFailState` | UIViewState. GoToMainMenuEvent (GameEventWithInt) for Home. |
| `SpinWheelState`, `RateUsState` | UIViewState variants. |
| `GameHud` | IShowable. Header/Footer slide in/out via HudAnimations. |
| `NormalGameHud` | Extends GameHud. |
| `HudAnimations` | Static: SlideInFromAbove/Below, SlideOutAbove/Below. |

### 3.10 Spin Wheel (New)

| Script | Purpose |
|--------|---------|
| `SpinWheelController` | Handles spin logic. OnSpinPressed → GetRandomSliceIndex (weighted), set RewardedIndex. OnSpinEnded → multiplier, playerCoins. |
| `SpinWheelConfigurations` | Config ScriptableObject. |
| `SpinWheelData`, `SliceData` | Data models. |
| `SpinWheelView`, `SpinWheelPanelView`, `BaseView` | View layer. |
| `Slice` | Slice behavior. |

### 3.11 Text Animations

| Script | Purpose |
|--------|---------|
| `TMPTextAnimator` | Per-character scale animation on TMP_Text. Configurable delay, overshoot, curvature. IPlay. |
| `TMPTextAnimationConfig` | curveHeight, characterDelay, totalDuration, overshootScale, easeCurve. |
| `UITextSoundPlayer` | Plays sound per character if configured. |

### 3.12 UI Animations

| Script | Purpose |
|--------|---------|
| `AdvancedMoveUI` | IUIAnimation. Multi-phase: anticipation, perpendicular, action to target, rotation. Uses pool for impact particles, SoundService. |
| `AdvancedMoveUIConfig` | Anticipation, hit duration, squash/stretch, reverse distance, etc. |
| `UIJumpAnimation` | Jump-style animation. |
| `IUIAnimation` | Play/Reset contract. |

### 3.13 Design Patterns

| Script | Purpose |
|--------|---------|
| `GameManager` | Entry for Command pattern. PlayerController + PlayerInputHandler, CommandManager. Undo/Redo buttons. |
| `CommandManager` | Stack-based. ExecuteCommand, Undo, Redo. |
| `PlayerController` | Subscribes to input. Creates MoveCommand, executes via CommandManager. |
| `PlayerInputHandler` | Bridges Unity Input System to movement events. |
| `ICommand` | Execute(), Undo(). |
| `MoveCommand` | Moves transform by direction * distance. |

### 3.14 Utilities

| Script | Purpose |
|--------|---------|
| `SwipeSystem.PlayerController` | Swipe input handling. |
| `ISwipeDetector` | OnSwipeDetected, Enable, Disable. |
| `IJsonWriter` | WriteJson<T>(filepath, data). |
| `StudentsJsonHandler` | JSON read/write for student data. |

---

## 4. Script Reference (Detailed)

### ApplicationBase
- **Path**: `Assets/Saad/GameFlow/Scripts/ApplicationBase.cs`
- **Namespace**: Blues.Core.Application
- **Dependencies**: FiniteStateMachine, TimeMachine, Float (SceneLoadingProgress)
- **Flow**: Start() → set targetFrameRate, start TimeMachine.Tick(), yield FSM.Init()

### FiniteStateMachine
- **Path**: `Assets/Saad/Utilities/StateMachine/Scripts/FiniteStateMachine.cs`
- **CreateAssetMenu**: ProjectCore/State Machine/Basic FSM
- **Key Fields**: BootState, CurrentState, PausedStates (Stack<State>), currentStateSortingOrder (Int)
- **Methods**: Init(), TransitionTo(transition, pauseCurrent), ReloadCurrentState(), ClearPausedStates(), PopPausedState(), JumpTo(target)

### BaseApplicationFlowController
- **Path**: `Assets/Saad/GameFlow/Scripts/BaseApplicationFlowController.cs`
- **Abstract**. OnEnable: Subscribe backBtnPressedEvent, RegisterFlowEvents. OnDisable: Unregister.
- **BootFlow(transition, reason)**: HandleTransition with policy from reason.
- **GetPolicyForReason**: Home/Game→ClearAll, ResumeGame/Revive/SkipLevel→PopOne, ResumeAny→PopUntil.
- **ShouldPauseCurrent**: True for FullScreenPlacement, ShowFullScreenPlacement, DailyLogin.

### UIBase
- **Path**: `Assets/Saad/UI/Base/Scripts/UiBase.cs`
- **Show()**: Sets canvas order, planeDistance, activates, plays enter animation, MakeStateInteractable(true).
- **Hide()**: IEnumerator. Blocks input, plays exit animation, waits completion, deactivates.
- **Pause/Resume**: Toggle interactivity, ForceCompleteCurrentAnimation.

### UIViewState
- **Path**: `Assets/Saad/Utilities/StateMachine/Scripts/UiViewState.cs`
- **Enter**: Get from pool (stateId) or Resources, parent to StateRootManager.States, get UIBase, Show().
- **Exit**: Hide() (yield), Release to pool or Destroy.
- **Pause/Resume**: Forward to _uiInstance.

### GameState
- **Path**: `Assets/Saad/UI/GameState/Scripts/GameState.cs`
- **Enter**: Load gameplay prefab (optional), load HUD from pool/Resources, Show HUD, invoke GameStateEnter.
- **Exit**: Hide HUD (yield), release/destroy HUD and gameplay, GameStateExit.
- **Pause/Resume**: Forward to HUD, GameStatePaused/Resumed.

---

## 5. Data Flow

### Navigation Flow
```
View (Button Click) → State.Method() → GameEvent.Invoke()
    → ApplicationFlowController (subscribed) → GoTo(Transition, UICloseReasons)
    → BaseApplicationFlowController.HandleTransition()
    → FSM.ClearPausedStates / PopPausedState / JumpTo (per policy)
    → FSM.TransitionTo(Transition, pauseCurrent)
    → FSM.DoTransition: Exit/Pause current → transition.Execute → nextState.Enter
```

### Back Button
```
backBtnPressedEvent.Invoke() → OnBackButtonPressed() → FSM.PopPausedState()
```

### Variable Persistence
```
DBInt.SetValue(value) → base.SetValue → Save() → PlayerPrefs.SetInt(_key, Value)
DBInt.OnEnable → Load() → PlayerPrefs.GetInt(_key) or DefaultValue
```

---

## 6. Extension Guide

### Adding a New Screen
1. Create `YourState` : UIViewState (or State). Assign stateId (pool ID or Resources path).
2. Create `YourView` : UIBase. Reference YourState, wire buttons to state methods.
3. Create `YourTransition` : Transition. Set ToState = YourState.
4. Add GameEvent for navigation. In ApplicationFlowController: serialize event + transition, subscribe GoTo(YourTransition, reason).
5. Register prefab in PoolManagerSO if using pooling.

### Adding a New GameEvent
1. Create Asset: ProjectCore/Events/Game Event - Basic (or GameEvent - Int).
2. In publisher: serialize GameEvent, call Invoke() or Raise(value).
3. In subscriber (e.g. ApplicationFlowController): Subscribe in RegisterFlowEvents, UnSubscribe in UnregisterFlowEvents.

### Adding a New Variable
1. Non-persistent: Use `Int`, `Float`, `Bool` from Variables/Non-Persistent.
2. Persistent: Use `DBInt`, `DBFloat`, `DBBool`. Set unique key, validate with KeyValidator.

---

## Dependencies

- **Unity** (Input System, TextMeshPro, DOTween)
- **Sirenix Odin Inspector** (optional, for editor enhancements)
- **THEBADDEST.Coroutines** (CoroutineHandler for static coroutines)
