# Quick Reference
> Keep this open while building. Every common decision answered in one line.

---

## When to use what

| Situation | Use |
|---|---|
| New full screen (has own back button behavior) | `UIBase` + `UIViewState` + `ScreenManifest` |
| Tab page inside hub (journey, shop, leaderboard) | `ISubView` — owned by parent state |
| Popup / liveops panel on top of current screen | `IOverlayView` — spawned by parent state, no FSM |
| Gameplay UI — moves, objectives, booster bar, pause | `IGameHud` — loaded by `GameState` alongside gameplay prefab |
| Sub-panel swap within one screen | `enum SubPanel` inside State — no new class |
| Swiping between tabs | `SwipeDetector` → `HubState.NavigateTo(i)` — same as tab press |
| Persistent simple value (coins, level index) | `DBInt` / `DBBool` ScriptableObject |
| Complex persistent data (level progress, inventory) | `ISaveService.Save<T>()` |
| Tunable game value (reward amounts, difficulty) | `IRemoteConfigService.Get(key, default)` |
| Timer / countdown | `ITimeProvider.UtcNow` — never `DateTime.UtcNow` directly |
| Navigation between screens | Raise a `GameEvent` SO — never call FSM directly from View |
| Passing data to a new screen | `IStateContext` subclass passed to `TransitionTo()` |
| Reacting to currency change | Subscribe to `IEconomyService.OnBalanceChanged` in State |
| Playing a sound | `ISoundService.Play(soundName)` |
| Getting a pooled object | `IPoolService.Get(poolId)` — null check the result |
| Custom enter/exit animation for a screen | `useDefaultAnimations = false` on `UIViewState` + override `OnCustomShow()` / `OnCustomHide()` (coroutine) on the View |
| Custom HUD animations per game mode | `useDefaultHudAnimations = false` on `GameState` + override HUD's `OnCustomShow/Hide/Pause/Resume` |
| Scaffold a new screen | **Tools → State Creator** (Editor window). Reference: `Assets/Game/Screens/GameSettingsX/` |

---

## FSM transition vs internal switch

| Question | Answer |
|---|---|
| Does Android back leave this screen? | FSM transition |
| Does Android back close this without leaving the parent? | Internal / IOverlayView |
| Does this change the game mode (menu → gameplay)? | FSM transition |
| Is this a tab change or panel swap within one screen? | Internal ISubView switch |
| Does this pause the game behind it? | Only if `ShouldPauseCurrent` returns true in FlowController |

---

## Subscription pattern (always named methods)

```csharp
// OnEnable — subscribe
private void OnEnable() => MyEvent.Subscribe(HandleMyEvent);

// OnDisable — unsubscribe same method
private void OnDisable() => MyEvent.UnSubscribe(HandleMyEvent);

// Handler — named method
private void HandleMyEvent() => DoSomething();
```

---

## Screen file checklist

Use **Tools → State Creator** (`StateCreatorWindow`) to scaffold all of this. Reference implementation: `Assets/Game/Screens/GameSettingsX/`.

Every screen lives in `Assets/Game/Screens/[Name]/` and contains:
- [ ] `Scripts/[Name]State.cs` — extends `UIViewState` (use `GetView<T>()`; named handlers only)
- [ ] `Scripts/[Name]UIView.cs` — extends `UIBase` (emits events, no logic)
- [ ] `Scripts/[Name]ViewData.cs` — plain struct, no Unity deps
- [ ] `Scripts/[Name]Transition.cs` — extends `Transition` (optional if no custom Execute)
- [ ] `Config/[Name]State.asset`, `GoTo[Name]Event.asset`, `GoTo[Name]Transition.asset`
- [ ] `Prefabs/[Name].prefab`
- [ ] (When `ScreenManifest` adoption rolls out in later phases) `[Name]Manifest.asset`

---

## ViewData rules (3 rules)

1. Only primitives, enums, other ViewData structs
2. No `MonoBehaviour`, `Sprite`, `GameObject` — use `string` addresses
3. Must compile with only `using System;` and `using System.Collections.Generic;`

---

## vContainer scope — what lives where

```
ProjectLifetimeScope (forever)
  ISoundService, IPoolService, ISaveService,
  IRemoteConfigService, ITimeProvider, IDeviceProfileService

GameLifetimeScope (per game scene)
  IEconomyService, ILiveOpsService, IApplicationFlowController

LevelLifetimeScope (per level — destroyed on exit)
  IGameLogic, ICommandManager, IInputHandler
```

---

## Performance — the 4 always-do rules

1. `private readonly WaitForSeconds _wait = new(1f);` — never `new` inside loops
2. `pool.Get(id)` — always null-check the result
3. `DOTween` recycler enabled — `DOTween.SetTweensCapacity(200, 50)` on boot
4. Pool prewarm is spread across frames — never block main thread

---

## Phase 0 bugs — fixed?

- [x] Lambda subscriptions → named methods in `ApplicationFlowController`
- [x] `WaitForSeconds` cached in `TimeMachine`
- [x] `PoolManagerSO.Get()` returns null + logs error instead of throwing
- [x] Sorting order counter moved off SO — now a plain `int` on `FiniteStateMachine`, exposed via `IState.CurrentSortingOrder`, written to `UIBase.SetSortingOrder(int)`. (`FSMRuntime` MonoBehaviour split still deferred; note `GameHud` still uses an Int SO — legacy tech-debt.)
- [ ] Legacy `SpinWheel/` folder deleted (manual, Editor-only)

---

## Template build checklist (Phase 5)

| Screen | Manifest | State | View | ViewData | Prefab tagged |
|---|---|---|---|---|---|
| SplashScreen | | | | | |
| HubScreen | | | | | |
| JourneyPage (subview) | | | | | |
| ShopPage (subview) | | | | | |
| LeaderboardPage (subview) | | | | | |
| LiveOpsPanel (overlay) | | | | | |
| RewardOverlay (overlay) | | | | | |
| GameplayScreen | | | | | |
| LevelCompleteScreen | | | | | |
| LevelFailScreen | | | | | |
| SettingsScreen (GameSettingsX) | N/A | ✅ | ✅ | ✅ | ✅ |
| SpinWheelScreen | | | | | |
| DailyRewardScreen | | | | | |

---

## Glossary

| Term | Meaning |
|---|---|
| `ScreenManifest` | SO that declares everything about one screen — the agent's output contract |
| `ScreenRegistry` | SO array of all manifests — SplashState reads this to warm pools |
| `FlowGraph` | SO graph of states + navigation edges — replaces hardcoded FlowController |
| `IStateContext` | Typed data passed when transitioning to a state |
| `IGameHud` | Gameplay UI companion loaded by GameState alongside the puzzle board — HUD, moves, objectives, boosters |
| `ISubView` | A tab page or swipeable panel inside a UIBase screen |
| `IOverlayView` | A popup or liveops panel spawned on top — no FSM involvement |
| `ComponentTag` | Enum tag on every interactive prefab component — agent's semantic anchor |
| `ViewData` | Plain C# struct passed from State to View — serializable, no Unity deps |
| `LevelScope` | vContainer scope destroyed on level exit — prevents state leaking between levels |
| `IRemoteConfigService` | Interface for all remote-tunable values — Firebase plugs in here |
| `ITimeProvider` | Interface for all time reads — server time plugs in here |
