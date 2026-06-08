# Game Architecture — Detailed AI/Developer Prompt

Use this prompt when onboarding AI assistants or new developers to the project. It provides a complete mental model of the architecture.

---

## Copy-Paste Prompt for AI Assistants

```
You are working on a Unity game project with the following architecture. Use this as your primary reference when making changes, debugging, or extending the codebase.

## ARCHITECTURE OVERVIEW

This is a **ScriptableObject-driven, state-machine-based** architecture for mobile/casual Unity games. The app bootstraps via SplashState, loads a GameScene additively, initializes pools and state roots, then transitions to MainMenu. All screen/screen flows are managed by a FiniteStateMachine (FSM) with support for pause stacks (e.g., overlay popups on top of MainMenu).

### Core Principles
1. **States are ScriptableObjects** — State and Transition assets define flow, not MonoBehaviours.
2. **Events decouple systems** — GameEvent and GameEventWithParam<T> ScriptableObjects connect UI → FlowController without direct references.
3. **UI follows IShowable** — All showable screens (UIBase, GameHud) implement Show(), Hide(), Pause(), Resume().
4. **Variables are ScriptableObjects** — Int, Float, Bool, DBInt, DBBool, etc. enable data-driven design and persistence.

---

## NAMESPACES

- **Blues.Core.Application** — ApplicationBase, SplashState, Loading, StateRootManager
- **Blues.Core.StateMachine** — FiniteStateMachine, State, Transition, UIViewState, IState
- **Blues.Core.UI** — UIBase, IShowable, UiCloseReasons, ClosePolicy
- **Blues.Core.Events** — GameEvent, GameEventWithParam<T>, GameEventWithInt
- **Blues.Core.Variables** — Int, Float, Bool, DBInt, DBBool, DBBoolWithEvent, etc.
- **Blues.Core.SoundSystem** — SoundService, Sound, AudioSourcePool
- **Blues.Core.PoolSystem** — PoolManagerSO, IPoolable
- **Blues.Core.GameHud** — GameHud, HudAnimations
- **Blues.Core.TheTimeMachine** — TimeMachine
- **Blues.Core.DesignPatterns.CommandPattern** — GameManager, CommandManager, PlayerController, ICommand
- **Blues.Core.Input.SwipeSystem** — ISwipeDetector
- **Blues.Core.Utilities.Json** — IJsonWriter
- **Featrues.SpinWheel** — SpinWheelController (note typo: "Featrues")
- **Core.UI.TextAnimations** — TMPTextAnimator
- **ProjectCore.PoolSystem** — UnityObjectPool (external)

---

## BOOT SEQUENCE (Step by Step)

1. **ApplicationBase** (MonoBehaviour in Bootstrap/Splash scene):
   - Sets Application.targetFrameRate (Android/iOS)
   - Starts TimeMachine.Tick() coroutine (fires GameEvent every second)
   - Yields FiniteStateMachine.Init()

2. **FiniteStateMachine.Init()**:
   - Sets CurrentState = BootState (SplashState)
   - Calls SplashState.Enter(this)
   - Fires OnStateEntered

3. **SplashState.Enter()**:
   - Instantiates ApplicationFlowController from Resources
   - Loads GameScene additively (LoadSceneAsync, allowSceneActivation = false)
   - Waits until Float SceneLoadingProgress >= 1 (updated by Loading component)
   - Sets allowSceneActivation = true, waits for scene load
   - SetGameSceneAsActiveScene()
   - SetupStateRoots(): creates "-------------------STATES-------------------" GameObject, StateRootManager.Initialize(states.transform)
   - InitializerPoolers(): statesPooler.Initialize(StateRootManager.States)
   - ApplicationFlowController.Boot()

4. **ApplicationFlowController.Boot()**:
   - BootFlow(MainMenuTransition, UICloseReasons.Home)
   - HandleTransition: ClearAll policy clears paused stack, then FSM.TransitionTo(MainMenuTransition, false)

5. **MainMenuTransition**:
   - ToState = MainMenuState
   - FSM exits current (SplashState), executes transition (none), enters MainMenuState

6. **MainMenuState.Enter()** (UIViewState):
   - Gets pooled GameObject for stateId from PoolManagerSO
   - Gets UIBase, activates, calls Show()
   - MainMenuView displays with animations

---

## STATE MACHINE DETAILS

### State Types
- **State** — Base. Enter(IState listener), Pause(), Resume(), Exit(). Listener is FSM (IState).
- **UIViewState** — Spawns UI from pool/Resources, gets UIBase, Show/Hide. Pause/Resume forward to UI.
- **GameState** — Loads gameplay prefab + HUD, manages GameHud (IShowable). Fires GameStateEnter/Exit/Paused/Resumed.

### Transition Flow
When FSM.TransitionTo(transition, pauseCurrent):
1. If same state → skip
2. If target is paused → warn, skip (use JumpTo)
3. If pauseCurrent OR nextState.PausePreviousState → PauseCurrentState (push to stack)
4. Else → ExitCurrentState (full exit)
5. transition.Execute() (optional)
6. nextState.Enter(this)
7. CurrentState = nextState, OnStateEntered

### Close Policies (from UICloseReasons)
- **ClearAll** — Clear entire PausedStates stack, then transition
- **PopOne** — Pop top paused state, resume it, discard current
- **PopUntil** — Jump to specific state in stack, exit others
- **Default** — Use policy from GetPolicyForReason(reason)

### UICloseReasons → Policy Mapping
- Home, Game → ClearAll
- ResumeGame, Revive, SkipLevel → PopOne
- ResumeAny → PopUntil
- FullScreenPlacement, ShowFullScreenPlacement, DailyLogin → pause current (push to stack)

---

## NAVIGATION (How Screens Change)

1. **View** (e.g. MainMenuView): Button onClick → MainMenuState.GoToPlayState()
2. **State**: GoToGameEvent.Invoke()
3. **ApplicationFlowController** (subscribed): GoTo(GameTransition, UICloseReasons.Game)
4. **BaseApplicationFlowController**: HandleTransition(GameTransition, ClearAll, false)
5. **FSM**: ClearPausedStates (if any), TransitionTo(GameTransition, false)
6. **FSM**: Exit Splash/MainMenu, Enter NormalGameState
7. **NormalGameState**: Load gameplay + HUD from pool, Show GameHud

Back button: backBtnPressedEvent → OnBackButtonPressed → FSM.PopPausedState()

---

## UI SYSTEM

### UIBase
- Requires: Canvas, CanvasGroup, RectTransform (UIPanel), StateAnimationConfig, Int (currentStateSortingOrder)
- Show(): Set canvas order, activate, play enter animation (Fade/Scale/Slide via UiAnimationSystem), MakeStateInteractable(true)
- Hide(): IEnumerator — block input, play exit animation, wait completion, deactivate
- Pause/Resume: Toggle blocksRaycasts, ForceCompleteCurrentAnimation

### UiAnimationSystem
- Non-MonoBehaviour. Uses DOTween.
- StateAnimationConfig: EnterAnimationType, ExitAnimationType (Fade/Scale/Slide/None), durations, ease, StartScale

### GameHud
- IShowable. Header/Footer RectTransforms slide in/out via HudAnimations (SlideInFromAbove/Below, SlideOutAbove/Below).
- Used inside GameState, not as standalone UIViewState.

---

## EVENTS

- **GameEvent**: Subscribe(Action), UnSubscribe(Action), Invoke(). Clears OnDisable.
- **GameEventWithParam<T>**: Subscribe(Action<T>), Raise(T).
- **GameEventWithInt**: Used for GoToMainMenu(reasonId) — reasonId maps to UICloseReasons.

---

## VARIABLES

- **Int, Float, Bool**: ScriptableObject, Value, DefaultValue, ResetToDefaultOnPlay
- **DBInt, DBFloat, DBBool**: Persistent via PlayerPrefs. Key must be unique (KeyValidator).
- **DBBoolWithEvent**: Invokes GameEvent on SetValue (e.g. mute toggle → UI update).

---

## SOUND

- **SoundService**: Initialize() creates pool, DontDestroyOnLoad parent. Play(name), FadeIn/FadeOut, Stop. Respects DBBoolWithEvent for mute.
- **AudioSourcePool**: ObjectPool<AudioSource>, AutoReturnToPool for non-looping clips.

---

## POOLING

- **PoolManagerSO**: PoolConfig[] with PoolID, Prefab, DefaultCapacity, MaxSize, Prewarm. Get(poolId), Release(poolId, obj), ReleaseAfterDelay.
- **IPoolable**: OnPoolGet(), OnPoolRelease()
- **PooledFxAutoRelease**: Arm(pool, poolId, delay) — auto-release FX after duration.

---

## SPIN WHEEL

- **SpinWheelController**: OnSpinPressed → weighted random slice index → RewardedIndex.SetValue. OnSpinEnded → multiplier, playerCoins.
- **SpinWheelConfigurations**: Config with loadedSpinWheelData, SlicesData, currentCoins.

---

## COMMAND PATTERN (Undo/Redo)

- **GameManager**: Holds CommandManager, PlayerController, PlayerInputHandler. Undo/Redo buttons.
- **CommandManager**: ExecuteCommand (push to history, clear redo), Undo (pop, undo, push to redo), Redo (pop redo, execute, push history).
- **PlayerController**: OnMovementInput → MoveCommand(transform, direction, distance) → ExecuteCommand.
- **PlayerInputHandler**: Bridges Unity Input System (WASD) to OnMovementInput.

---

## COMMON TASKS

### Add new screen
1. Create State (UIViewState or State), assign stateId
2. Create View (UIBase), reference State
3. Create Transition, set ToState
4. Add GameEvent + Transition in ApplicationFlowController, subscribe GoTo(transition, reason)
5. Register prefab in PoolManagerSO if pooled

### Add new event
1. Create GameEvent asset
2. Serialize in publisher, Invoke() on action
3. Subscribe in ApplicationFlowController (or other), UnSubscribe in OnDisable

### Add persistent variable
1. Create DBInt/DBFloat/DBBool asset
2. Set unique key, run KeyValidator
3. Serialize where needed, use GetValue/SetValue

---

## DEPENDENCIES

- Unity (Input System, TextMeshPro, DOTween)
- Sirenix Odin Inspector (optional)
- THEBADDEST.Coroutines (CoroutineHandler)
- ProjectCore.PoolSystem (UnityObjectPool)

---

## FILE LOCATIONS (Key Paths)

- ApplicationBase: Assets/Saad/GameFlow/Scripts/ApplicationBase.cs
- FiniteStateMachine: Assets/Saad/Utilities/StateMachine/Scripts/FiniteStateMachine.cs
- BaseApplicationFlowController: Assets/Saad/GameFlow/Scripts/BaseApplicationFlowController.cs
- ApplicationFlowController: Assets/Saad/GameFlow/Scripts/ApplicationFlowController.cs
- SplashState: Assets/Saad/GameFlow/Scripts/SplashState.cs
- StateRootManager: Assets/Saad/GameFlow/Scripts/StateRootManager.cs
- UIBase: Assets/Saad/UI/Base/Scripts/UiBase.cs
- UIViewState: Assets/Saad/Utilities/StateMachine/Scripts/UiViewState.cs
- GameState: Assets/Saad/UI/GameState/Scripts/GameState.cs
```

---

## Quick Reference Tables

### State Hierarchy
| Class | Base | Use Case |
|-------|------|----------|
| State | ScriptableObject | Base for any state |
| UIViewState | State | Full-screen UI with pool/Resources |
| GameState | State | Gameplay + HUD |
| MainMenuState | UIViewState | Main menu |
| LevelCompleteState | UIViewState | Level complete popup |
| NormalGameState | GameState | Normal gameplay |

### Transition Types
| Transition | ToState |
|------------|---------|
| MainMenuTransition | MainMenuState |
| GameTransition | NormalGameState |
| LevelCompleteTransition | LevelCompleteState |
| LevelFailTransition | LevelFailState |
| SpinWheelTransition | SpinWheelState |
| RateUsTransition | RateUsState |

### Event → Transition Mapping
| Event | Transition | Reason |
|-------|------------|--------|
| GoToMainMenuEvent (int) | MainMenuTransition | (reasonId) |
| GoToSpinWheelEvent | SpinWheelTransition | FullScreenPlacement |
| GoToGameEvent | GameTransition | Game |
| GoToLevelCompleteEvent | LevelCompleteTransition | FullScreenPlacement |
| GoToLevelFailEvent | LevelFailTransition | FullScreenPlacement |
| GoToRateUsEvent | RateUsTransition | FullScreenPlacement |
