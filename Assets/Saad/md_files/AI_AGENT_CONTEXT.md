# AI Agent Context
> Everything the agent needs to generate, validate, and integrate a screen

This document is the agent's operating manual. It defines exactly what to generate, in what format, and how to validate the output. The agent must read `ARCHITECTURE_RULES.md` before reading this.

---

## What the Agent Can Do

### Capability 1 · Mockup → Working Screen

**Input:**
- A mockup image (PNG/JPG)
- A screen name (PascalCase, e.g. `ShopScreen`)
- Optional: screen type hint (`fullscreen` | `subview` | `overlay`)

**Output:** A complete `Assets/Game/Screens/[Name]/` folder containing 5 files.

**Output is valid when:** `ScreenManifestValidator` shows all green.

---

### Capability 2 · Game Design → Skeleton

**Input:** A `game_design.json` file (see schema below)

**Output:** Game logic interfaces, LevelConfig schema, GameState, InputHandler, CommandManager — all wired and compilable. Developer fills in only the core mechanic.

---

## Screen Generation — Step by Step

### Step 1 · Analyze the mockup

Identify these regions in the mockup image:

| Region | Description | Maps to LayoutDescriptor slot |
|---|---|---|
| Top bar | Currency displays, settings button | `Header` |
| Main content area | Grid, list, game board | `Body` |
| Bottom bar / CTA | Primary action button, navigation | `Footer` |
| Floating elements | Close button, back button | `Overlay` |
| Reward/feedback area | Stars, coins burst, progress | `FeedbackLayer` |

### Step 2 · Assign ComponentTags

For every interactive element detected, assign a tag from this list:

```
// Currency displays
CoinDisplay      → TMP text showing coin balance
GemDisplay       → TMP text showing gem balance  
LivesDisplay     → TMP text or heart icons showing lives
EnergyDisplay    → TMP text showing energy

// Buttons
PlayButton       → primary play/start CTA
CloseButton      → X button, closes this screen
BackButton       → back arrow, returns to previous
SettingsButton   → gear icon
ShopButton       → cart/shop icon
CollectButton    → collect reward CTA
RetryButton      → retry level CTA
ReviveButton     → watch ad to revive CTA

// Content
ScrollList       → vertical scrollable list
ItemGrid         → grid of purchasable/selectable items
LevelCell        → individual level button in a grid
ProgressBar      → fill bar (XP, level progress, event)
TimerDisplay     → countdown timer text
RewardItem       → a reward icon + amount pair
BoosterSlot      → a booster button with count

// Navigation
TabBar           → container of tab buttons
TabButton        → individual tab (Journey, Shop, etc.)
```

### Step 3 · Generate ViewData struct

`[Name]ViewData.cs` — plain C# struct, no Unity dependencies, no `using UnityEngine`.

```csharp
// GENERATED — DO NOT ADD UNITY DEPS HERE
public struct ShopViewData
{
    public int Coins;
    public int Gems;
    public ShopItemData[] Items;
}

public struct ShopItemData
{
    public string ItemId;
    public string DisplayName;
    public string IconAddress;    // Addressables key, not Sprite ref
    public int Price;
    public CurrencyType PriceCurrency;
    public bool IsOwned;
}
```

Rules for ViewData:
- Only primitive types, enums, and other ViewData structs
- No `MonoBehaviour`, `ScriptableObject`, `Sprite`, `GameObject`
- Use `string` for asset addresses — views load from Addressables
- Must be JSON-serializable (`[Serializable]` attribute)

### Step 4 · Generate View script

`[Name]View.cs` — extends `UIBase`.

```csharp
public class ShopView : UIBase
{
    // Serialized component refs — named to match ComponentTags
    [SerializeField] private TMP_Text _coinDisplay;      // tag: CoinDisplay
    [SerializeField] private TMP_Text _gemDisplay;       // tag: GemDisplay
    [SerializeField] private Button _closeButton;        // tag: CloseButton
    [SerializeField] private Transform _itemGridRoot;    // tag: ItemGrid
    [SerializeField] private ShopItemCell _itemCellPrefab;

    // Events the State listens to
    public event Action OnClosePressed;
    public event Action<string> OnItemPressed; // itemId

    // Called by State — view only renders, never reads services
    public void SetData(ShopViewData data)
    {
        _coinDisplay.text = data.Coins.ToString("N0");
        _gemDisplay.text = data.Gems.ToString("N0");
        PopulateGrid(data.Items);
    }

    private void Awake()
    {
        _closeButton.onClick.AddListener(() => OnClosePressed?.Invoke());
    }

    private void PopulateGrid(ShopItemData[] items) { ... }
}
```

Rules for View:
- No service access (`IEconomyService`, `ISaveService`, etc.)
- No `DBInt`, `DBBool`, `GameEvent` SO references
- Fires events upward (Action delegates) — State subscribes
- Only receives data through `SetData(ViewData)` or explicit setter calls
- `using` only: `UnityEngine`, `UnityEngine.UI`, `TMPro`, `System`

### Step 5 · Generate State script

`[Name]State.cs` — extends `UIViewState<TContext>`.

```csharp
public class ShopState : UIViewState<ShopContext>
{
    // Injected by vContainer
    [Inject] private IEconomyService _economy;
    [Inject] private IShopService _shop;

    // Navigation events — wired in FlowGraph
    [SerializeField] private GameEvent _closeEvent;

    private ShopView _view;

    protected override void OnEnter(ShopContext ctx)
    {
        _view = GetView<ShopView>();
        _view.OnClosePressed += HandleClose;
        _view.OnItemPressed += HandleItemPressed;

        RefreshView();
        _economy.OnBalanceChanged += HandleBalanceChanged;
    }

    protected override void OnExit()
    {
        _view.OnClosePressed -= HandleClose;
        _view.OnItemPressed -= HandleItemPressed;
        _economy.OnBalanceChanged -= HandleBalanceChanged;
    }

    private void RefreshView()
    {
        _view.SetData(new ShopViewData
        {
            Coins = _economy.GetBalance(CurrencyType.Coins),
            Gems  = _economy.GetBalance(CurrencyType.Gems),
            Items = _shop.GetAvailableItems()
        });
    }

    private void HandleClose()          => _closeEvent.Invoke();
    private void HandleItemPressed(string id) => _shop.Purchase(id);
    private void HandleBalanceChanged(CurrencyType _, int __) => RefreshView();
}
```

Rules for State:
- Reads from services, writes to view — never the reverse
- All event subscriptions use named methods, unsubscribed in `OnExit`
- Navigation fires a `GameEvent` SO — never calls FSM directly
- Context type is the minimum data needed to enter this screen

### Step 6 · Create ScreenManifest asset

`[Name]Manifest.asset` YAML:

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_Script: {fileID: [ScreenManifest GUID]}
  ScreenId: ShopScreen
  DisplayName: Shop
  UsePooling: 1
  SortGroup: 1        # 0=Base, 1=Overlay, 2=Modal, 3=System
  EnterPreset: {fileID: [SlideFromBottom.asset GUID]}
  ExitPreset: {fileID: [SlideToBottom.asset GUID]}
```

### Step 7 · Add node to FlowGraph

In `Assets/Game/Core/MainFlowGraph.asset`, add:

```yaml
- NodeId: ShopScreen
  State: {fileID: [ShopState.asset GUID]}
  Edges:
  - TriggerEvent: {fileID: [GoToShopEvent.asset GUID]}
    TargetNodeId: ShopScreen
    CloseReason: 7   # Home
```

### Step 8 · Run validation

Open `Window > Game > Screen Manifest Validator`. Every row for the new screen must be green before the screen is considered integrated.

Validation checks:
- [ ] Manifest asset exists in correct folder
- [ ] Prefab reference is assigned
- [ ] Prefab is registered in ScreenRegistry
- [ ] All ComponentTags declared in manifest exist in prefab
- [ ] ViewData type compiles and is JSON-serializable
- [ ] State script compiles
- [ ] Enter + Exit animation presets assigned
- [ ] FlowGraph node exists for this screen

---

## Folder Structure — Generated Output

```
Assets/Game/Screens/ShopScreen/
├── ShopScreenManifest.asset     ← generated step 6
├── ShopScreenState.cs           ← generated step 5
├── ShopScreenView.cs            ← generated step 4
├── ShopScreenViewData.cs        ← generated step 3
└── ShopScreenPrefab.prefab      ← agent creates layout, designer polishes
```

**Never generate files outside this folder.**

---

## Game Design JSON Schema

For Capability 2 (game design → skeleton):

```json
{
  "gameId": "magic_sort",
  "displayName": "Magic Sort",
  "gameType": "sort",
  "grid": {
    "width": 5,
    "height": 5,
    "cellTypes": ["tube", "free"]
  },
  "pieces": [
    { "id": "red",   "color": "#E24B4A" },
    { "id": "blue",  "color": "#378ADD" },
    { "id": "green", "color": "#639922" }
  ],
  "winCondition": "all_sorted",
  "mechanics": ["move", "undo", "booster_wildcard"],
  "maxMoves": 30,
  "boosters": ["wildcard", "shuffle", "extraMove"]
}
```

Agent output from this JSON:
- `ISortGameLogic.cs` — interface with `MakeMove(from, to)`, `IsComplete()`, `GetValidMoves()`
- `SortLevelConfig.cs` — ScriptableObject extending `LevelConfig`
- `SortGameState.cs` — extends `GameState`, uses `LevelContext`
- `SortInputHandler.cs` — implements `IInputHandler`, uses `SwipeSystem`
- `SortCommandManager.cs` — registers `MoveCommand`, wires undo button
- `LevelConfig_001.asset` through `LevelConfig_010.asset` — starter levels

---

## What the Agent Must NOT Do

- Do not write core game logic (the sort algorithm, physics solver, win condition evaluation)
- Do not write level content (which pieces go where in specific levels)
- Do not modify any file outside `Assets/Game/Screens/[Name]/` and `MainFlowGraph.asset`
- Do not use `Resources.Load` — use pool IDs and Addressables keys
- Do not add any `using` statement to `ViewData` files beyond `System` and `System.Collections.Generic`
- Do not hardcode any numeric values that should come from `IRemoteConfigService`
- Do not create a screen without first running the ScreenManifest validator

---

## Error Recovery

If validation fails, the agent checks in this order:

1. **Missing ComponentTag in prefab** → Add `TaggedComponent` to the correct GameObject
2. **ViewData not JSON-serializable** → Remove any non-primitive type, replace with string address
3. **State doesn't compile** → Check all injected interfaces exist in `GameLifetimeScope`
4. **FlowGraph node missing** → Add node manually to `MainFlowGraph.asset`
5. **Animation preset missing** → Create preset asset in `Assets/Game/Core/AnimationPresets/`

If all validation passes but the screen doesn't appear in-game, check:
- Screen is registered in `ScreenRegistry.asset`
- A `GameEvent` that targets this screen is being raised by some other state
- The FlowGraph edge has the correct `TriggerEvent` reference
