# Vision & Direction
> Where we are, where we're going, and why every architectural decision points there

---

## The Goal

Build a **Unity 6 game-making platform** — not just a game. The end state is a system where:

1. A designer drops a mockup image into a tool
2. An AI agent reads the mockup, applies architecture rules, and generates a fully wired, playable screen
3. That screen is integrated into the game with zero manual wiring
4. LiveOps content (events, rewards, difficulty) is configured remotely without new builds
5. New puzzle game variants can be scaffolded from a game design document in hours, not weeks

The reference bar is **Royal Match and Grand Games** — polished, LiveOps-driven casual puzzle games with deep meta-game loops, high retention mechanics, and content that updates independently of the app binary.

---

## Where We Are Now

A well-structured Unity architecture template with:
- ScriptableObject-driven FSM (state machine)
- GameEvent SOs for decoupled navigation
- DBInt/DBBool for persistent variables
- Object pooling for UI and FX
- Command pattern for undo/redo
- Spin wheel, basic game states, HUD system

**What it is:** A solid foundation with correct instincts.
**What it is not:** A complete game or an automatable system yet.

---

## The Three Horizons

### Horizon 1 — Solid Template (Now → Month 2)
*Goal: Build a complete Magic Sort-style game manually using the improved architecture. Every screen, every system, every liveop feature built by hand — following all rules precisely. This becomes the AI agent's training material.*

Deliverables:
- All critical bugs fixed
- ScreenManifest + ScreenRegistry system live
- vContainer DI integrated (Project + Game + Level scopes)
- FlowGraph replacing hardcoded FlowController
- ISubView + IOverlayView pattern for hub
- ISaveService, IRemoteConfigService, IEconomyService (local stubs)
- ITimeProvider abstraction
- Full Magic Sort template: hub, levels, liveops panels, economy, spin wheel, daily reward
- All screens in `Assets/Game/Screens/[Name]/` folder convention
- Every component tagged with ComponentTag

**Why hand-build first:** The agent learns from examples. 6 well-built screens following all rules exactly are worth more than 60 screens built inconsistently.

### Horizon 2 — AI-Assisted Screens (Month 2 → Month 4)
*Goal: The AI agent can take a mockup image + screen name and generate a working screen that passes ScreenManifest validation.*

The agent pipeline:
```
Input: mockup.png + "ShopScreen"
  ↓
Region detection (header, body items, footer CTA)
  ↓
Map regions to LayoutDescriptor slots
  ↓
Assign ComponentTags to interactive elements
  ↓
Generate:
  - ShopViewData.cs (data struct matching detected elements)
  - ShopView.cs (UIBase with tagged component refs)
  - ShopState.cs (UIViewState<ShopContext>)
  - ShopManifest.asset (ScreenManifest SO)
  - ShopPrefab.prefab (layout matching mockup)
  ↓
Run ScreenManifest validator
  ↓
Add node to FlowGraph
  ↓
Output: working screen, integrated, ready to fill with game logic
```

Tools required:
- `unity-mcp` bridge (already in packages)
- ScreenManifest validator Editor window
- ComponentTag registry
- FlowGraph editor with node add/remove

### Horizon 3 — Full Game Scaffolding (Month 4+)
*Goal: From a game design document describing a new puzzle mechanic, scaffold the entire game skeleton — states, level config schema, game logic interfaces, input wiring — ready for a developer to fill in the core mechanic only.*

```
Input: game_design.json
{
  "gameType": "sort",
  "gridSize": "5x5",
  "pieceTypes": ["red", "blue", "green", "yellow"],
  "winCondition": "all_sorted",
  "mechanics": ["swap", "undo", "booster_wildcard"]
}
  ↓
Agent generates:
  - ISortGameLogic interface
  - SortLevelConfig ScriptableObject schema
  - SortGameState with correct IStateContext
  - SortInputHandler wired to SwipeSystem
  - SortCommandManager with SwapCommand, UndoCommand
  - LevelConfig assets for levels 1-10 (procedural)
  - All wired into FlowGraph
```

---

## Why This Architecture Enables Automation

### SOs are machine-writable assets
An agent can create a `.asset` file for a `ScreenManifest` SO by writing YAML — no code compilation needed. It can set pool ID, animation preset, prefab reference, sort order, all from a text file.

### ComponentTags are semantic anchors
When the agent sees a button in a mockup that's tagged `PlayButton`, it knows: wire to `GoToGameplay` event. When it sees `CoinDisplay`, it knows: bind to `ViewData.Coins`. No ambiguity, no guessing from component names.

### FlowGraph is a data asset
Adding a new screen to the game is adding a node + edges to the FlowGraph SO. The agent does this in YAML, not C#. No code change, no recompile.

### The Screens/ folder is a generation target
The agent has one output location with a deterministic structure. Every file it needs to generate is in one folder. It can validate its own output by checking the folder is complete.

### ViewData is JSON-serializable
`[Name]ViewData.cs` contains only plain C# types — strings, ints, sprite addresses. The agent generates this struct directly from the detected elements in a mockup. No Unity knowledge required for this file.

---

## Non-Negotiable Principles

**Remote-first.** Every tunable value in the game reads from `IRemoteConfigService` with a local default. No hardcoded numbers in gameplay code. When Firebase is added, zero code changes.

**Interface-first.** Every service is an interface. Local stub → real implementation is always a one-class swap, never a refactor.

**Folder convention is enforced.** One screen = one folder in `Assets/Game/Screens/`. The CI validator rejects PRs that break this.

**Agent-readable = human-readable.** If a system is hard for a human to understand in 30 seconds, it's too complex for an agent to generate reliably. Simplicity is a feature.

**The template is the documentation.** The Magic Sort template game is not a demo — it is the authoritative example of how every system is used. When rules conflict, the template wins.

---

## LiveOps Target Features (Royal Match parity)

| Feature | Horizon |
|---|---|
| Daily login reward | 1 |
| Spin wheel | 1 (exists, needs wiring) |
| Remote config (Firebase) | 2 |
| Timed limited events | 2 |
| Season pass / battle pass | 2 |
| Tournament with leaderboard | 3 |
| Push notifications | 2 |
| Downloadable level packs | 3 |
| Force update / maintenance gate | 2 |
| A/B test framework | 2 |
| Cloud save / cross-device | 3 |

---

## What the AI Agent Is Not

- It does not write game logic (the sort algorithm, physics, win conditions) — a developer does that
- It does not design levels — a designer does that in the Level Editor
- It does not make product decisions (what features to add) — the product team does that
- It does not replace code review — every generated screen is reviewed before merging

The agent handles **boilerplate and wiring**. Humans handle **logic and decisions**.
