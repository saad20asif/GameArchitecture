# Improvement Roadmap
> Ordered by dependency. Each phase must be complete before the next begins.

---

## Phase 0 — Critical Bug Fixes
**Timeline: Day 1 — do before anything else**
**Risk if skipped: Silent memory leaks, GC hitches, unrecoverable crashes on low-end devices**

---

### BUG-01 · Lambda subscription leak in ApplicationFlowController

**File:** `Assets/Saad/GameFlow/Scripts/ApplicationFlowController.cs`

**Problem:** Every `OnEnable/OnDisable` cycle registers a new lambda that can never be unsubscribed. After 10 minutes of play (ads firing, OS interrupts), dozens of phantom subscribers accumulate.

```csharp
// BROKEN — UnSubscribe creates a NEW lambda, the original is never removed
GoToSpinWheelEvent.Subscribe(() => GoTo(SpinWheelTransition, UICloseReasons.Home));
GoToSpinWheelEvent.UnSubscribe(() => GoTo(SpinWheelTransition, UICloseReasons.Home));
```

**Fix:** Convert every lambda subscription to a named private method.

```csharp
// CORRECT
private void OnEnable()
{
    GoToMainMenuEvent.Subscribe(HandleGoToMainMenu);
    GoToSpinWheelEvent.Subscribe(HandleGoToSpinWheel);
    GoToGameEvent.Subscribe(HandleGoToGame);
    LevelCompleteEvent.Subscribe(HandleLevelComplete);
    LevelFailEvent.Subscribe(HandleLevelFail);
}

private void OnDisable()
{
    GoToMainMenuEvent.UnSubscribe(HandleGoToMainMenu);
    GoToSpinWheelEvent.UnSubscribe(HandleGoToSpinWheel);
    GoToGameEvent.UnSubscribe(HandleGoToGame);
    LevelCompleteEvent.UnSubscribe(HandleLevelComplete);
    LevelFailEvent.UnSubscribe(HandleLevelFail);
}

private void HandleGoToMainMenu()    => GoTo(MainMenuTransition, UICloseReasons.Home);
private void HandleGoToSpinWheel()   => GoTo(SpinWheelTransition, UICloseReasons.Home);
private void HandleGoToGame()        => GoTo(GameTransition, UICloseReasons.Game);
private void HandleLevelComplete()   => GoTo(LevelCompleteTransition, UICloseReasons.Home);
private void HandleLevelFail()       => GoTo(LevelFailTransition, UICloseReasons.Home);
```

**Apply this same pattern to every class that subscribes to GameEvents.**

---

### BUG-02 · WaitForSeconds GC allocation in TimeMachine

**File:** `Assets/Saad/TimeMachine/Scripts/TimeMachine.cs`

**Problem:** `yield return new WaitForSeconds(1f)` allocates a new object every second. On a 1-hour session = 3,600 allocations that trigger GC pauses.

```csharp
// BROKEN
private IEnumerator Tick()
{
    while (true)
    {
        yield return new WaitForSeconds(1f); // allocates every tick
        OnTick?.Invoke();
    }
}
```

**Fix:**

```csharp
// CORRECT
private readonly WaitForSeconds _wait = new(1f);

private IEnumerator Tick()
{
    while (true)
    {
        yield return _wait;
        OnTick?.Invoke();
    }
}
```

---

### BUG-03 · PoolManagerSO.Get() throws KeyNotFoundException

**File:** `Assets/Saad/Utilities/PoolSystem/Scripts/PoolManagerSO.cs`

**Problem:** If a pool ID is missing or misspelled, `Get()` throws an unhandled exception. On low-end Android, this crashes the game with no recovery path.

```csharp
// BROKEN — throws if key missing
public GameObject Get(string poolId) => _pools[poolId].Get();
```

**Fix:**

```csharp
// CORRECT
public GameObject Get(string poolId)
{
    if (!_pools.TryGetValue(poolId, out var pool))
    {
        Debug.LogError($"[Pool] No pool registered for ID '{poolId}'. Check PoolManagerSO config.");
        return null;
    }
    return pool.Get();
}

// All callers must null-check the result:
var obj = _pool.Get(poolId);
if (obj == null) return;
```

---

### BUG-04 · Canvas sorting order race condition

**File:** `Assets/Saad/Utilities/StateMachine/Scripts/FiniteStateMachine.cs`

**Problem:** `currentStateSortingOrder` is a shared ScriptableObject `Int`. During rapid transitions (e.g., boot sequence), two coroutines can read/increment it concurrently, producing incorrect canvas ordering.

**Fix:** Move the sorting order counter to the FSM's runtime MonoBehaviour, not the SO asset.

```csharp
// In FSMRuntime.cs (new MonoBehaviour, not SO)
private int _sortingOrderCounter = 0;

public int NextSortingOrder() => ++_sortingOrderCounter;

public void ResetSortingOrder() => _sortingOrderCounter = 0;
```

Remove `currentStateSortingOrder` Int SO reference from `FiniteStateMachine`. Pass `FSMRuntime.NextSortingOrder()` to `UIBase.Show()`.

---

### BUG-05 · Delete legacy SpinWheel folder

**Path:** `Assets/Saad/UI/SpinWheel/` (legacy) vs `Assets/Saad/UI/Spin Wheel/` (current MVC)

**Problem:** Two implementations of the same system exist. The AI agent will treat both as valid patterns and generate inconsistent output.

**Fix:** Delete `Assets/Saad/UI/SpinWheel/` entirely. Keep only `Assets/Saad/UI/Spin Wheel/`.

---

## Phase 1 — Screen Contract
**Timeline: Week 1**
**Unlocks: AI agent screen generation, consistent screen structure, pool auto-registration**

---

### P1-01 · ScreenManifest ScriptableObject

Create `Assets/Game/Core/Screens/ScreenManifest.cs`:

```csharp
[CreateAssetMenu(menuName = "Game/Screen Manifest")]
public class ScreenManifest : ScriptableObject
{
    [Header("Identity")]
    public string ScreenId;
    public string DisplayName;

    [Header("Prefab")]
    public GameObject Prefab;
    public bool UsePooling = true;

    [Header("Animation")]
    public AnimationPreset EnterPreset;
    public AnimationPreset ExitPreset;

    [Header("Sorting")]
    public CanvasSortGroup SortGroup; // enum: Base, Overlay, Modal, System

    [Header("Layout")]
    public LayoutDescriptor Layout;

    [Header("Navigation")]
    public GameEvent[] PublishedEvents;
}
```

**One manifest per screen. No exceptions.**

---

### P1-02 · ScreenRegistry ScriptableObject

```csharp
[CreateAssetMenu(menuName = "Game/Screen Registry")]
public class ScreenRegistry : ScriptableObject
{
    public ScreenManifest[] Screens;

    public ScreenManifest Get(string screenId)
    {
        foreach (var s in Screens)
            if (s.ScreenId == screenId) return s;
        Debug.LogError($"[ScreenRegistry] No manifest for '{screenId}'");
        return null;
    }
}
```

`SplashState` reads `ScreenRegistry` on boot and warms all pools automatically. No manual pool registration per screen.

---

### P1-03 · Screens/ folder convention (enforced)

```
Assets/Game/Screens/
└── [ScreenName]/
    ├── [Name]Manifest.asset
    ├── [Name]State.cs
    ├── [Name]View.cs
    ├── [Name]ViewData.cs
    └── [Name]Prefab.prefab
```

Add an Editor script that warns when screen files exist outside this structure.

---

### P1-04 · AnimationPreset library

Move all DOTween configs out of `StateAnimationConfig` and into named presets:

```
Assets/Game/Core/AnimationPresets/
├── PopIn.asset
├── PopOut.asset
├── SlideFromBottom.asset
├── SlideToBottom.asset
├── FadeIn.asset
├── FadeOut.asset
└── SlideFromRight.asset
```

`ScreenManifest` references presets by asset, not inline config. The AI agent selects a preset by name based on screen type.

---

### P1-05 · ComponentTag system

```csharp
public enum ComponentTag
{
    // Layout
    Header, Body, Footer,
    // Currency displays
    CoinDisplay, GemDisplay, LivesDisplay, EnergyDisplay,
    // Buttons
    PlayButton, CloseButton, BackButton, SettingsButton, ShopButton,
    // Content
    ScrollList, ItemGrid, ProgressBar, TimerDisplay,
    LevelCell, RewardItem, BoosterSlot,
    // Navigation
    TabBar, TabButton
}

public class TaggedComponent : MonoBehaviour
{
    public ComponentTag Tag;
}
```

Every interactive MonoBehaviour in every prefab must have a `TaggedComponent`. The AI agent uses tags to wire events.

---

### P1-06 · NavigationRequest value object

Replace `GoTo(Transition, UICloseReasons)` raw calls with:

```csharp
public struct NavigationRequest
{
    public string TargetScreenId;
    public UICloseReasons Reason;
    public IStateContext Context;
}

// FlowController receives requests:
public void Navigate(NavigationRequest request)
```

Clean, serializable, loggable. The agent emits `NavigationRequest` structs.

---

## Phase 2 — Dependency Injection (vContainer)
**Timeline: Week 2**
**Unlocks: Testable systems, injectable services, clean LevelScope for puzzle games**

---

### P2-01 · ProjectLifetimeScope

```csharp
public class ProjectLifetimeScope : LifetimeScope
{
    [SerializeField] private SoundService _soundService;
    [SerializeField] private PoolManagerSO _poolManager;
    [SerializeField] private ScreenRegistry _screenRegistry;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_soundService).As<ISoundService>();
        builder.RegisterInstance(_poolManager).As<IPoolService>();
        builder.RegisterInstance(_screenRegistry);
        builder.Register<LocalSaveService>(Lifetime.Singleton).As<ISaveService>();
        builder.Register<LocalRemoteConfigService>(Lifetime.Singleton).As<IRemoteConfigService>();
        builder.Register<LocalTimeProvider>(Lifetime.Singleton).As<ITimeProvider>();
        builder.Register<DeviceProfileService>(Lifetime.Singleton).As<IDeviceProfileService>();
    }
}
```

---

### P2-02 · GameLifetimeScope

```csharp
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<EconomyService>(Lifetime.Singleton).As<IEconomyService>();
        builder.Register<LiveOpsService>(Lifetime.Singleton).As<ILiveOpsService>();
        builder.RegisterEntryPoint<ApplicationFlowController>();
    }
}
```

---

### P2-03 · LevelLifetimeScope

Created when a level loads. Disposed when a level exits.

```csharp
public class LevelLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<CommandManager>(Lifetime.Singleton).As<ICommandManager>();
        // IGameLogic registered by the specific game type (SortGameLogic, etc.)
    }
}
```

---

### P2-04 · Remove Resources.Load and static StateRootManager

- `SplashState` uses `IObjectResolver.Instantiate()` instead of `Resources.Load + Instantiate`
- `StateRootManager` becomes injectable `IStateRoot` service registered in `GameLifetimeScope`
- States receive dependencies via `[Inject]` constructor parameters

---

## Phase 3 — Modular Flow & State Context
**Timeline: Week 3**
**Unlocks: New screens without code changes, LiveOps dynamic state injection**

---

### P3-01 · IStateContext for typed data passing

```csharp
public interface IStateContext { }

public class LevelContext : IStateContext
{
    public LevelConfig LevelConfig;
    public int LevelIndex;
    public string PackId;
}

public class RewardContext : IStateContext
{
    public RewardBundle Reward;
    public string SourceEvent;
}
```

FSM signature becomes:
```csharp
void TransitionTo(Transition transition, IStateContext context = null, bool pauseCurrent = false);
```

---

### P3-02 · FlowGraph ScriptableObject

Replace hardcoded `ApplicationFlowController` mappings with data:

```csharp
[CreateAssetMenu(menuName = "Game/Flow Graph")]
public class FlowGraph : ScriptableObject
{
    public FlowNode[] Nodes;
}

[Serializable]
public class FlowNode
{
    public string NodeId;
    public State State;
    public FlowEdge[] Edges;
}

[Serializable]
public class FlowEdge
{
    public GameEvent TriggerEvent;
    public string TargetNodeId;
    public UICloseReasons CloseReason;
}
```

Adding a new screen = adding a node + edges to this asset. Zero code change.

---

### P3-03 · Transition guards

```csharp
public abstract class Transition : ScriptableObject
{
    public State ToState;
    public ClosePolicy ClosePolicy;

    // Override to add conditions
    public virtual bool CanTransition(State currentState) => true;

    public virtual IEnumerator Execute() { yield break; }
}
```

---

### P3-04 · ISubView and IOverlayView interfaces

```csharp
public interface ISubView
{
    void Show(SubViewContext context);
    void Hide();
    void OnFocused();
    void OnUnfocused();
}

public interface IOverlayView
{
    void Open(OverlayContext context);
    void Close();
    event Action OnClosed;
}
```

---

## Phase 4 — Service Stubs (Remote-Ready Interfaces)
**Timeline: Week 4**
**Unlocks: LiveOps features can be built locally now, Firebase plugs in later with zero refactor**

---

### P4-01 · IRemoteConfigService + local stub

```csharp
public interface IRemoteConfigService
{
    T Get<T>(string key, T defaultValue);
    Task FetchAsync();
    bool IsFetched { get; }
}

// Reads from StreamingAssets/remote_config.json
public class LocalRemoteConfigService : IRemoteConfigService { ... }

// Later: FirebaseRemoteConfigService : IRemoteConfigService (one new class)
```

---

### P4-02 · ISaveService + JSON implementation

```csharp
public interface ISaveService
{
    void Save<T>(string key, T data);
    T Load<T>(string key, T defaultValue = default);
    Task SaveAsync<T>(string key, T data);
    void Delete(string key);
}
```

---

### P4-03 · ITimeProvider + local stub

```csharp
public interface ITimeProvider
{
    DateTime UtcNow { get; }
    long UnixTimestamp { get; }
}

public class LocalTimeProvider : ITimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public long UnixTimestamp => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
// Later: ServerTimeProvider fetches offset from backend on boot
```

---

### P4-04 · IEconomyService

```csharp
public interface IEconomyService
{
    int GetBalance(CurrencyType currency);
    bool Spend(CurrencyType currency, int amount, string reason);
    void Earn(CurrencyType currency, int amount, EarnSource source);
    event Action<CurrencyType, int> OnBalanceChanged;
}
```

---

### P4-05 · IDeviceProfileService

```csharp
public enum DeviceTier { Low, Mid, High }

public interface IDeviceProfileService
{
    DeviceTier Tier { get; }
    int MaxParticles { get; }
    bool UseHighResTextures { get; }
    int PoolPrewarmBudget { get; } // objects per frame
}
```

Detected once on boot. Used by pool system, animation system, and texture loading.

---

## Phase 5 — Magic Sort Template Build
**Timeline: Month 2**
**This is the AI agent's training material — build it perfectly**

Build every screen following all Phase 1–4 rules exactly:

| Screen | Type | Notes |
|---|---|---|
| `SplashScreen` | UIBase / FSM state | Boot, load, transition to hub |
| `HubScreen` | UIBase / FSM state | Footer tabs, liveops buttons, swipe |
| `JourneyPage` | ISubView | Level select grid, locked/unlocked cells |
| `ShopPage` | ISubView | Currency packs, boosters |
| `LeaderboardPage` | ISubView | Tournament rankings |
| `LiveOpsPanel` | IOverlayView | Event detail, enter button |
| `RewardOverlay` | IOverlayView | Coin/gem reward animation |
| `GameplayScreen` | UIBase + GameState | Level, HUD, booster bar |
| `LevelCompleteScreen` | UIBase / FSM state | Stars, reward, next level |
| `LevelFailScreen` | UIBase / FSM state | Retry, revive, quit |
| `SettingsScreen` | UIBase / FSM state | Sound, notifications |
| `SpinWheelScreen` | UIBase / FSM state | Weighted spin, reward |
| `DailyRewardScreen` | UIBase / FSM state | Login streak calendar |

Each screen must have: Manifest, State, View, ViewData, Prefab with ComponentTags.

---

## Phase 6 — Editor Validation Tool
**Timeline: Month 2 (alongside template build)**

Build `ScreenManifestValidator` Editor window:

- Lists all `ScreenManifest` assets in project
- For each: checks pool registration, prefab exists, required ComponentTags present, ViewData type exists, animation presets valid
- Green/red checklist per manifest
- "Fix All" button for auto-correctable issues

This is the AI agent's self-check tool. After generating a screen, the agent runs validation. If green — the screen is integrated.

---

## Phase 7 — AI Agent Integration
**Timeline: Month 3**
**Prerequisite: Phases 1–6 complete + template build complete**

Using `com.coplaydev.unity-mcp` (already in packages):

**Agent capability 1: Mockup → Screen**
```
Input: mockup.png + screen_name
Agent: detects regions → maps to LayoutDescriptor slots → assigns ComponentTags
       → generates ViewData struct → generates View + State scripts
       → creates Manifest asset → adds FlowGraph node
       → runs ScreenManifestValidator
Output: working screen, integrated
```

**Agent capability 2: Game design → skeleton**
```
Input: game_design.json
Agent: generates IGameLogic interface → LevelConfig schema
       → GameState with correct IStateContext → InputHandler
       → CommandManager with game-specific commands
Output: compilable skeleton, developer fills in core mechanic
```

---

## Ongoing — Performance Hardening
**Timeline: Continuous from Phase 1 onward**

- [ ] All `WaitForSeconds` cached as `readonly` fields
- [ ] DOTween recycler enabled on boot
- [ ] Pool prewarm spread across frames using `IDeviceProfileService.PoolPrewarmBudget`
- [ ] Audit all `FindObjectsOfType` / `GetComponent` calls in hot paths
- [ ] Low-tier device: simplified animations, reduced particle counts, smaller pool sizes
- [ ] All saves on low-tier devices use `SaveAsync`
- [ ] Frame budget system for level loading and analytics flush
