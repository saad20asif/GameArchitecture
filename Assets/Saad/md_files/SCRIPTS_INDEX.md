# Script Index — Quick Reference

Every C# script in the architecture with a one-line description. Use for quick lookup when navigating the codebase.

---

## GameFlow
| Script | Description |
|--------|-------------|
| `ApplicationBase` | Entry point: frame rate, TimeMachine.Tick, FSM.Init |
| `BaseApplicationFlowController` | Abstract flow: BootFlow, GoTo, HandleTransition, back button, policy mapping |
| `ApplicationFlowController` | Concrete flow: MainMenu, SpinWheel, Game, LevelComplete, LevelFail, RateUs, **GameSettingsX** events. All subs use named methods (no lambdas). |
| `SplashState` | Boot state: load GameScene additively, init StateRootManager, pools, ApplicationFlowController.Boot |
| `StateRootManager` | Static States Transform, IsInitialized |
| `Loading` | DOTween slider, updates Float load value |

---

## StateMachine
| Script | Description |
|--------|-------------|
| `FiniteStateMachine` | ScriptableObject FSM: Init, TransitionTo, **ClearAllAndTransitionTo**, PausedStates stack, ClearAll/PopOne/JumpTo. Sorting order is a plain int (resets each session). |
| `State` | Base ScriptableObject state: Enter, Pause, Resume, Exit. **No animation flags.** |
| `Transition` | ScriptableObject: ToState, closePolicy, Execute |
| `UIViewState` | State that spawns UI from pool/Resources, Show/Hide via UIBase. Adds `useDefaultAnimations` flag + `GetView<T>()` helper. |
| `IState` | Interface: TransitionTo, ClearPausedStates, ReloadCurrentState, **CurrentSortingOrder** |
| `StateLogger` | Debug logging for state changes |

---

## UI Base
| Script | Description |
|--------|-------------|
| `UIBase` | Abstract: Show, Hide (IEnumerator), Pause, Resume. Has `UseDefaultAnimations` property + `OnCustomShow/Hide/Pause/Resume` hooks. `SetSortingOrder(int)` replaces Int SO. |
| `IShowable` | Interface: Show, Hide, Pause, Resume |
| `UiAnimationSystem` | DOTween enter/exit: Fade, Scale, Slide |
| `StateAnimationConfig` | ScriptableObject: Enter/Exit types, durations, ease |
| `UiCloseReasons` | Enum: Home, Game, SkipLevel, Revive, FullScreenPlacement, ResumeGame, etc. |
| `UiAnimations` | Legacy/alternate animation helpers |
| `ClosePolicy` | Enum: Default, ClearAll, PopUntil, PopOne |

---

## UI Screens (States)
| Script | Description |
|--------|-------------|
| `MainMenuState` | UIViewState: GoToGameEvent, GoToSpinWheelEvent |
| `MainMenuView` | UIBase: Play, SpinWheel buttons → MainMenuState |
| `MainMenuTransition` | Transition to MainMenuState |
| `LevelCompleteState` | UIViewState: GoToMainMenuEvent (GameEventWithInt) |
| `LevelCompleteView` | UIBase for level complete screen |
| `LevelCompleteTransition` | Transition to LevelCompleteState |
| `LevelFailState` | UIViewState: GoToMainMenuEvent |
| `LevelFailView` | UIBase for level fail screen |
| `LevelFailTransition` | Transition to LevelFailState |
| `SpinWheelState` | UIViewState for spin wheel |
| `SpinWheelView` | UIBase for spin wheel (legacy) |
| `SpinWheelTransition` | Transition to SpinWheelState |
| `RateUsState` | UIViewState for rate us popup |
| `RateUsView` | UIBase for rate us |
| `RateUsTransition` | Transition to RateUsState |
| `GameSettingsXState` | UIViewState reference implementation — toggles DBBool SoundEnabled / HapticsEnabled via view events; `GetView<T>()` pattern |
| `GameSettingsXUIView` | UIBase — emits `OnSettingsPressed / OnSoundToggled / OnHapticsToggled / OnRestorePressed / OnExitPressed` events |
| `GameSettingsXViewData` | Plain POCO pushed by the state to the view (`SoundEnabled`, `HapticsEnabled`) |
| `GameSettingsXTransition` | Transition to GameSettingsXState |

---

## GameState & HUD
| Script | Description |
|--------|-------------|
| `GameState` | Abstract: load gameplay + HUD, GameStateEnter/Exit/Paused/Resumed. Has `useDefaultHudAnimations` flag written to HUD before Show(). |
| `NormalGameState` | GameState: GoToLevelComplete, GoToLevelFail, **GoToSettings** events. Wires HUD `OnSettingsPressed` → `GoToGameSettingsXEvent`. |
| `GameStateTransition` | Transition to NormalGameState |
| `GameHud` | IShowable: Header/Footer slide animations via HudAnimations. Has `UseDefaultAnimations` + `OnCustomShow/Hide/Pause/Resume` hooks mirroring UIBase. |
| `NormalGameHud` | Extends GameHud. Exposes `OnLevelCompletePressed`, `OnLevelFailPressed`, `OnSettingsPressed` events. |
| `HudAnimations` | Static: SlideInFromAbove/Below, SlideOutAbove/Below |

---

## Spin Wheel (New)
| Script | Description |
|--------|-------------|
| `SpinWheelController` | Spin logic: weighted random, RewardedIndex, multiplier, playerCoins |
| `SpinWheelConfigurations` | Config ScriptableObject |
| `SpinWheelData` | Data model |
| `SliceData` | Slice data |
| `SpinWheelView` | View for spin wheel panel |
| `SpinWheelPanelView` | Panel view |
| `BaseView` | Base view class |
| `Slice` | Slice behavior |
| `SpinWheelViewRefs` | View references |
| `SpinWheelRefs` | Refs model |
| `BaseRefs` | Base refs |

---

## Events
| Script | Description |
|--------|-------------|
| `GameEvent` | ScriptableObject: Subscribe, UnSubscribe, Invoke |
| `GameEventWithParam<T>` | Base: Raise(T) |
| `GameEventWithInt` | GameEventWithParam<int> |
| `GameEventWithReturn` | Event with return value |
| `GameEventWithReturnInt` | Int return variant |

---

## Variables
| Script | Description |
|--------|-------------|
| `Int` | ScriptableObject: Value, SetValue, Increment, Decrement |
| `Float` | ScriptableObject float |
| `Bool` | ScriptableObject bool |
| `DBInt` | Int + PlayerPrefs persistence |
| `DBFloat` | Float + persistence |
| `DBBool` | Bool + persistence |
| `DBBoolWithEvent` | DBBool + GameEvent on SetValue |
| `DBIntWithEvent` | DBInt + event |
| `IntWithEvent` | Int + event |
| `KeyValidator` | Ensures unique keys for DB variables |

---

## Sound System
| Script | Description |
|--------|-------------|
| `SoundService` | ScriptableObject: Play, FadeIn/FadeOut, Stop, pool-based |
| `Sound` | Serializable: name, clip(s), volume, pitch, loop |
| `AudioSourcePool` | ObjectPool<AudioSource>, AutoReturnToPool |
| `SoundSettings` | AudioMixer groups |
| `SoundExtensions` | Extension methods |

---

## Pool System
| Script | Description |
|--------|-------------|
| `PoolManagerSO` | ScriptableObject: Get/Release, PoolConfig[], ReleaseAfterDelay |
| `PoolManagerRunner` | DontDestroyOnLoad coroutine runner |
| `PooledFxAutoRelease` | Arm(pool, poolId, delay) for FX auto-release |
| `PoolMonitor` | Editor pool stats |

---

## Time Machine
| Script | Description |
|--------|-------------|
| `TimeMachine` | ScriptableObject: Tick() invokes GameEvent every second |

---

## Text Animations
| Script | Description |
|--------|-------------|
| `TMPTextAnimator` | Per-character scale animation on TMP_Text |
| `TMPTextAnimationConfig` | curveHeight, characterDelay, overshootScale |
| `UITextSoundPlayer` | Sound per character |
| `IPlay` | Play() interface |

---

## UI Animations
| Script | Description |
|--------|-------------|
| `AdvancedMoveUI` | Multi-phase: anticipation → action → target, particles |
| `AdvancedMoveUIConfig` | Anticipation, hit duration, squash/stretch |
| `UIJumpAnimation` | Jump animation |
| `IUIAnimation` | Play, Reset interface |

---

## Design Patterns (Command)
| Script | Description |
|--------|-------------|
| `GameManager` | Entry: CommandManager, PlayerController, Undo/Redo |
| `CommandManager` | ExecuteCommand, Undo, Redo stacks |
| `PlayerController` | Movement → MoveCommand → CommandManager |
| `PlayerInputHandler` | Bridges Input System to movement |
| `ICommand` | Execute(), Undo() |
| `MoveCommand` | Moves transform |
| `PlayerInput` | Auto-generated Input System actions (WASD) |

---

## Swipe System
| Script | Description |
|--------|-------------|
| `PlayerController` | Swipe-based player control |
| `ISwipeDetector` | OnSwipeDetected, Enable, Disable |

---

## JSON
| Script | Description |
|--------|-------------|
| `IJsonWriter` | WriteJson<T>(filepath, data) |
| `StudentsJsonHandler` | Student data JSON read/write |

---

## Editor
| Script | Description |
|--------|-------------|
| `ScriptableObjectCreator` | Editor utility for creating SO assets |
| `StateCreatorWindow` | **Tools → State Creator** editor window. Scaffolds a complete new screen (State SO, View MB, ViewData POCO, Transition SO, GameEvent SO, prefab, folder layout) from one form. Matches the GameSettingsX pattern. |

---

## Learning / Misc
| Script | Description |
|--------|-------------|
| `Delegations` | Delegation pattern examples |
