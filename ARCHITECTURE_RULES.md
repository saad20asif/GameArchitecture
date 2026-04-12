# Architecture Rules
> Unity 6 · ScriptableObject-driven · vContainer DI · Mobile-first · AI-agent-ready

These rules are **law**. Every screen, system, and service built in this project must follow them.
The AI agent uses these rules to validate generated output. Humans use them during code review.

---

## 1. The Three Layers — Never Cross Them

```
State   →  owns WHAT is shown and WHEN
View    →  owns HOW it looks
Service →  owns persistent cross-state data
```

| Layer | Can read from | Can call | Cannot |
|---|---|---|---|
| **State** | Services, ScriptableObject variables | `view.Show(data)`, `view.UpdateX(value)`, FSM transitions | Read from View, touch PlayerPrefs directly |
| **View** | Only data passed into it | Fire UI events (button clicks), animate itself | Read DBInt, access services, call other states |
| **Service** | Its own data store | Nothing UI-related | Touch any MonoBehaviour, reference any View or State |

**The test:** If you find yourself writing `view.coins = economyService.GetBalance()` inside a View — stop. The State reads from the service and calls `view.SetCoins(balance)`.

---

## 2. ScriptableObject Rules

SOs are used for **three things only**:

1. **Events** — `GameEvent`, `GameEventWithParam<T>` — decouple publishers from subscribers
2. **Variables** — `DBInt`, `DBBool`, `Float` — inspector-wirable reactive values
3. **Definitions** — `State`, `Transition`, `ScreenManifest`, `AnimationPreset`, `LevelConfig` — data assets, not runtime logic

### What SOs are NOT
- SOs are **not services**. Do not put `Fetch()`, `Save()`, `Calculate()` methods on SOs.
- SOs are **not singletons**. Do not use `Instance` patterns on SOs.
- SOs are **not runtime state containers**. Do not store mutable runtime state (stacks, lists that grow) on SO assets — it persists between Editor play sessions and breaks with Addressables.

### GameEvent SO rules
```csharp
// CORRECT — named method reference, unsubscribes cleanly
private void OnEnable()  => GoToShopEvent.Subscribe(HandleGoToShop);
private void OnDisable() => GoToShopEvent.UnSubscribe(HandleGoToShop);
private void HandleGoToShop() => GoTo(ShopTransition, UICloseReasons.Home);

// WRONG — lambda leaks a new subscription every OnEnable cycle
private void OnEnable()  => GoToShopEvent.Subscribe(() => GoTo(...));
private void OnDisable() => GoToShopEvent.UnSubscribe(() => GoTo(...)); // does nothing
```

---

## 3. FSM Rules

### When to use a FSM transition
- Changing game mode: Hub → Gameplay, Hub → Onboarding, Gameplay → LevelComplete
- Screens where Android back button should **leave** the screen

### When NOT to use a FSM transition
- Tab switches inside the Hub (Journey → Shop → Leaderboard)
- Opening a liveops overlay/panel on top of the current screen
- Showing a sub-panel, tooltip, or reward popup within a screen

**The back-button test:** If pressing Android back should close the thing without leaving the parent screen — it is internal state management, not an FSM transition.

### FSM state runtime state
The `PausedStates` stack and any other runtime-mutable data must live on a **runtime MonoBehaviour** (`FSMRuntime`), not on the ScriptableObject asset itself. SO assets persist between Editor play sessions.

### Transition guards
Every `Transition` SO can declare a `CanTransition()` condition. The FSM checks this before executing. No transition fires unconditionally.

### State context passing
```csharp
// States declare what context they expect
public class GameplayState : UIViewState<LevelContext>
{
    protected override void OnEnter(LevelContext ctx)
    {
        _view.SetLevelName(ctx.LevelConfig.DisplayName);
    }
}

// Callers pass typed context
fsm.TransitionTo(playTransition, new LevelContext { LevelIndex = 5 });
```

---

## 4. View Hierarchy — Three Types

### `UIBase` — full FSM-managed screens
- Has its own canvas, canvas group, sorting order
- Managed by `UIViewState` — pooled, shown/hidden by state
- Reserved for screens that are proper FSM states
- Examples: `MainMenuView`, `GameplayView`, `LevelCompleteView`, `SettingsView`

### `ISubView` — tab pages and swipeable panels
- Lives inside a parent `UIBase` screen
- Owned entirely by its parent State — not pooled independently
- Implements: `Show(SubViewContext)`, `Hide()`, `OnFocused()`, `OnUnfocused()`
- Examples: `JourneyPage`, `ShopPage`, `LeaderboardPage` inside HubView

### `IOverlayView` — popups, liveops panels, tooltips
- Spawned from pool by the parent State when needed
- Does NOT pause the FSM
- Implements: `Open(OverlayContext)`, `Close()`, fires `OnClosed` callback
- Examples: `LiveOpsPanel`, `RewardOverlay`, `BoosterTooltip`

### `IGameHud` — gameplay UI companion (fourth type)

`GameHud` is a special category that belongs exclusively to `GameState`. It is **not** a `UIBase` (it doesn't own the canvas), **not** an `ISubView` (it isn't a tab page), and **not** an `IOverlayView` (it lives alongside gameplay, not on top of it). It is the **gameplay UI layer** — header stats, footer boosters, pause button — that `GameState` loads alongside the gameplay prefab.

```
GameState (FSM state)
├── GameplayPrefab           ← the puzzle board / game logic
└── IGameHud (IShowable)     ← the gameplay UI companion
    ├── Header               ← level name, move counter, objectives
    ├── Footer               ← booster bar, pause button
    └── BoosterOverlay       ← IOverlayView, spawned by GameState on booster tap
```

`IGameHud` extends `IShowable` and adds gameplay-specific update methods:

```csharp
public interface IGameHud : IShowable
{
    void SetLevelInfo(string levelName, int levelIndex);
    void SetMoveCount(int movesUsed, int parMoves);
    void SetObjectiveProgress(ObjectiveData[] objectives);
    void SetBoosterCount(BoosterType type, int count);
    void OnBoosterActivated(BoosterType type);

    event Action OnPausePressed;
    event Action<BoosterType> OnBoosterPressed;
}
```

**`GameState` lifecycle with `IGameHud`:**
```csharp
// Enter: load both gameplay prefab AND hud
protected override void OnEnter(LevelContext ctx)
{
    _gameplayInstance = _pool.Get(ctx.LevelConfig.GameplayPrefabId);
    _hud = _pool.Get<IGameHud>(HudPoolId);
    _hud.OnPausePressed += HandlePause;
    _hud.OnBoosterPressed += HandleBooster;
    _hud.Show();
    _hud.SetLevelInfo(ctx.LevelConfig.DisplayName, ctx.LevelIndex);
}

// Pause: hud slides off, gameplay freezes
protected override void OnPause()
{
    _hud.Pause();   // HudAnimations.SlideOutAbove / SlideOutBelow
    _gameLogic.Pause();
}

// Resume: hud slides back in
protected override void OnResume()
{
    _hud.Resume();  // HudAnimations.SlideInFromAbove / SlideInFromBelow
    _gameLogic.Resume();
}

// Exit: hide hud, release both to pool
protected override void OnExit()
{
    _hud.OnPausePressed -= HandlePause;
    _hud.OnBoosterPressed -= HandleBooster;
    _hud.Hide();
    _pool.Release(HudPoolId, _hud);
    _pool.Release(ctx.LevelConfig.GameplayPrefabId, _gameplayInstance);
}
```

**Different game types get different huds — `GameState` doesn't care:**
```csharp
// Normal puzzle game
public class NormalGameHud : MonoBehaviour, IGameHud { ... }

// Tournament game — adds timer, rank display
public class TournamentGameHud : MonoBehaviour, IGameHud { ... }

// Registered in LevelLifetimeScope:
builder.Register<NormalGameHud>(Lifetime.Transient).As<IGameHud>();
```

**`IGameHud` rules:**
- Fired by `GameState` only — never called from `IGameLogic` or any service
- `SetMoveCount()`, `SetObjectiveProgress()` etc. are called by `GameState` in response to `IGameLogic` events
- Booster overlay inside the hud is an `IOverlayView` spawned by `GameState`, not by the hud itself
- The hud never reads from `IEconomyService` or any other service directly

---

### Hub/MainMenu pattern (Magic Sort, Royal Match)
```
HubState (UIViewState)
├── HubView (UIBase)
│   ├── FooterTabBar
│   └── LiveOpsButtonGrid
├── SubViews[]  (ISubView — tab pages)
│   ├── JourneyPage
│   ├── ShopPage
│   └── LeaderboardPage
└── ActiveOverlay  (IOverlayView — spawned on demand)
    ├── LiveOpsEventPanel
    └── RewardOverlay
```

**Swipe navigation = same call as tab press:**
```csharp
// Both swipe and button tap call the same method
SwipeDetector.OnSwipe += dir => NavigateTo(_currentIndex + dir);
tabButtons[i].onClick.AddListener(() => NavigateTo(i));
```

---

## 5. Dependency Injection Rules (vContainer)

### Scope hierarchy
```
ProjectLifetimeScope     (DontDestroyOnLoad — lives forever)
└── GameLifetimeScope    (per game scene — recreated on scene reload)
    └── LevelLifetimeScope  (per level — destroyed on level exit)
```

### What belongs in each scope
| Scope | Contains |
|---|---|
| Project | `ISoundService`, `IPoolService`, `IEventBus`, `IRemoteConfigService`, `ISaveService`, `IDeviceProfileService` |
| Game | `IFiniteStateMachine`, `IApplicationFlowController`, `ILiveOpsService`, `IEconomyService` |
| Level | `IGameLogic`, `ICommandManager`, `IInputHandler`, `ILevelProgressionService` |

### Rules
- **Services are interfaces.** Never inject a concrete class — always inject an interface.
- **SOs are registered as instances.** `builder.RegisterInstance(myGameEventSO)` — never construct SOs via DI.
- **Views do not receive [Inject].** Views are pooled objects. Pooled objects and constructor injection conflict. Views receive data only through `Show(ScreenContext)` or explicit setter calls from their State.
- **States receive [Inject].** States are resolved by the container at construction time.

---

## 6. Screen Contract — ScreenManifest

Every screen is defined by a `ScreenManifest` ScriptableObject. This is the single file the AI agent generates and validates.

```
Assets/Game/Screens/
└── [ScreenName]/
    ├── [Name]Manifest.asset       ← ScreenManifest SO — one per screen
    ├── [Name]State.cs             ← extends UIViewState<TContext>
    ├── [Name]View.cs              ← extends UIBase
    ├── [Name]ViewData.cs          ← plain C# struct, no Unity deps
    └── [Name]Prefab.prefab        ← tagged components inside
```

**No exceptions.** If a screen's files are not in this structure, it is not a valid screen.

### ScreenManifest declares
- Pool ID and prefab reference
- Animation preset (enter + exit)
- Sort order group
- Required `ViewData` type
- `LayoutDescriptor` — named UI slots (Header, Body, Footer, CTA)
- Navigation events it publishes

### ComponentTag — every interactive element is tagged
```csharp
public enum ComponentTag
{
    Header, Footer, CoinDisplay, GemDisplay, LivesDisplay,
    PlayButton, CloseButton, BackButton, SettingsButton,
    ScrollList, ItemGrid, ProgressBar, TimerDisplay,
    RewardItem, BoosterSlot, LevelCell
}
```
The AI agent uses tags to wire events without parsing prefab hierarchy.

---

## 7. LiveOps Rules

### ILiveOpsService is always a service, never a state
The service tracks active events, their timers, and reward configs. It lives in `GameLifetimeScope` and is always running.

### LiveOps UI is always an overlay, never an FSM state
A liveops event renders as an `IOverlayView` opened by the `HubState`. Only when a liveop has its own **gameplay** (e.g., a tournament level) does it trigger an FSM transition.

### All timers use ITimeProvider
```csharp
// Never use DateTime.UtcNow directly
// Always use injected ITimeProvider.UtcNow
// Swap LocalTimeProvider → ServerTimeProvider without changing callers
```

### All tunable values use IRemoteConfigService
```csharp
// Never hardcode reward amounts, difficulty values, event durations
int spinReward = _remoteConfig.Get("spin_wheel_coin_reward", defaultValue: 100);
```

---

## 8. Economy Rules

- No component ever touches PlayerPrefs directly for currency.
- All earn/spend goes through `IEconomyService`.
- Every transaction automatically fires an analytics event.
- Currency types: `Coins`, `Gems`, `Lives`, `Energy` — defined in `CurrencyType` enum.

---

## 9. Performance Rules

- Cache all `WaitForSeconds` as `private readonly` fields — never `new WaitForSeconds()` inside a loop.
- Enable DOTween's built-in recycler: `DOTween.SetTweensCapacity(200, 50)` + `tween.SetAutoKill(true)`.
- Pool prewarm is spread across frames — never block the main thread.
- `FindObjectsOfType` and `GetComponent` are banned in hot paths (Update, coroutine loops).
- Device tier is detected on boot. Low-tier devices get reduced pool sizes, simpler animations, lower particle counts.

---

## 10. Save System Rules

- No raw PlayerPrefs for complex data (level progress, inventory, event state).
- `ISaveService` is the only save interface. Current implementation: `JsonFileSaveService` (local, encrypted).
- Future: swap to `CloudSaveService` (Unity Gaming Services) without changing callers.
- All saves on low-end devices use `SaveAsync` to avoid main-thread spikes.

---

## Quick Reference — Decision Tree

```
Need to show something new?
├── Changes game mode (hub → gameplay)?          → FSM transition
├── Tab switch / swipe within hub?               → ISubView switch (no FSM)
├── Popup / liveops panel on top?                → IOverlayView (no FSM)
├── Gameplay UI (HUD, moves, objectives)?        → IGameHud — loaded by GameState alongside gameplay prefab
└── Sub-panel swap inside a screen?              → Internal state enum in State class

Need to store data?
├── Per-session, non-persistent?                 → ScriptableObject Variable (Int, Bool)
├── Persistent simple values?                    → DBInt, DBBool (PlayerPrefs)
├── Persistent complex data?                     → ISaveService (JSON file)
└── Server-authoritative?                        → CloudSaveService

Need to tune a value?
└── Always                                       → IRemoteConfigService.Get(key, default)
```
