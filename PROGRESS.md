# Progress Tracker
> Updated every time something is completed. This is the single source of truth for what's done and what's next.
> See `ROADMAP.md` for full specs of each item. See `ARCHITECTURE_RULES.md` for rules that govern all work.

---

## How to Use This File

- When you finish something → change `[ ]` to `[x]` and add the date
- When you start something → add `🔄` and the date next to it
- Never delete completed items — the history matters
- Each section links back to its phase in `ROADMAP.md`

---

## Documentation (Pre-Work)

| # | Item | Status | Date |
|---|------|--------|------|
| D-01 | Deep architecture exploration and code audit | ✅ Done | 2026-04-12 |
| D-02 | `ARCHITECTURE.md` — full technical deep-dive of existing codebase | ✅ Done | 2026-04-12 |
| D-03 | `ARCHITECTURE_RULES.md` — law document (layers, SO rules, FSM, DI, LiveOps) | ✅ Done | 2026-04-12 |
| D-04 | `ROADMAP.md` — phased improvement plan with full specs | ✅ Done | 2026-04-12 |
| D-05 | `VISION_AND_DIRECTION.md` — 3-horizon vision + AI agent goals | ✅ Done | 2026-04-12 |
| D-06 | `AI_AGENT_CONTEXT.md` — agent operating manual (screen gen + game design → skeleton) | ✅ Done | 2026-04-12 |
| D-07 | `QUICK_REFERENCE.md` — one-line decisions, checklists, glossary | ✅ Done | 2026-04-12 |
| D-08 | `SCRIPTS_INDEX.md` — every script with one-line description | ✅ Done | 2026-04-12 |
| D-09 | `ARCHITECTURE_PROMPT.md` — AI onboarding prompt with boot sequence | ✅ Done | 2026-04-12 |
| D-10 | `PROGRESS.md` — this file | ✅ Done | 2026-04-12 |

---

## Phase 0 — Critical Bug Fixes
> **Must complete before any other phase.** See `ROADMAP.md → Phase 0` for full specs.
> Risk if skipped: silent memory leaks, GC hitches, unrecoverable crashes on low-end devices.

| # | Bug | File | Status | Date |
|---|-----|------|--------|------|
| BUG-01 | Lambda subscription leak — `ApplicationFlowController` subscribes with lambdas that can never be unsubscribed | `Assets/Saad/GameFlow/Scripts/ApplicationFlowController.cs` | ✅ Done | 2026-04-12 |
| BUG-02 | `WaitForSeconds` GC allocation every second in `TimeMachine` | `Assets/Saad/TimeMachine/Scripts/TimeMachine.cs` | ✅ Already fixed (readonly field was already present) | 2026-04-12 |
| BUG-03 | `PoolManagerSO.Get()` throws `KeyNotFoundException` instead of null + error log | `Assets/Saad/Utilities/PoolSystem/Scripts/PoolManagerSO.cs` | ✅ Done | 2026-04-12 |
| BUG-04 | Canvas sorting order race condition — counter lives on SO asset (persists between sessions) | `IState.cs`, `FiniteStateMachine.cs`, `UIBase.cs`, `UIViewState.cs` | ✅ Done | 2026-04-12 |
| BUG-05 | Delete legacy `Assets/Saad/UI/SpinWheel/` folder — duplicate system confuses agent | `Assets/Saad/UI/SpinWheel/` | ⚠️ Manual — delete in Unity Editor | — |

---

## Phase 1 — Screen Contract
> **Unlocks:** AI agent screen generation, consistent screen structure, pool auto-registration.
> **Prerequisite:** Phase 0 complete.

| # | Item | Status | Date |
|---|------|--------|------|
| P1-01 | `ScreenManifest` ScriptableObject (ScreenId, Prefab, AnimPreset, SortGroup, Layout, Events) | ⬜ Pending | — |
| P1-02 | `ScreenRegistry` ScriptableObject (array of manifests, auto-warms pools on boot) | ⬜ Pending | — |
| P1-03 | `Assets/Game/Screens/[Name]/` folder convention + Editor warning for violations | ⬜ Pending | — |
| P1-04 | `AnimationPreset` library — named SO assets replacing inline `StateAnimationConfig` | ⬜ Pending | — |
| P1-05 | `ComponentTag` enum + `TaggedComponent` MonoBehaviour (semantic anchors for AI agent) | ⬜ Pending | — |
| P1-06 | `NavigationRequest` value object replacing raw `GoTo(Transition, UICloseReasons)` calls | ⬜ Pending | — |

---

## Phase 2 — vContainer Dependency Injection
> **Unlocks:** Testable systems, injectable services, clean LevelScope for puzzle games.
> **Prerequisite:** Phase 1 complete.

| # | Item | Status | Date |
|---|------|--------|------|
| P2-01 | `ProjectLifetimeScope` — registers `ISoundService`, `IPoolService`, `ISaveService`, `IRemoteConfigService`, `ITimeProvider`, `IDeviceProfileService` | ⬜ Pending | — |
| P2-02 | `GameLifetimeScope` — registers `IEconomyService`, `ILiveOpsService`, `IApplicationFlowController` | ⬜ Pending | — |
| P2-03 | `LevelLifetimeScope` — registers `IGameLogic`, `ICommandManager`, `IInputHandler` | ⬜ Pending | — |
| P2-04 | Replace `Resources.Load<ApplicationFlowController>` in `SplashState` with container resolution | ⬜ Pending | — |
| P2-05 | Replace static `StateRootManager` with injectable `IStateRoot` service | ⬜ Pending | — |
| P2-06 | States receive dependencies via `[Inject]` — remove all serialized service references from States | ⬜ Pending | — |

---

## Phase 3 — Modular Flow & State Context
> **Unlocks:** New screens without code changes, LiveOps dynamic state injection, typed data passing between states.
> **Prerequisite:** Phase 2 complete.

| # | Item | Status | Date |
|---|------|--------|------|
| P3-01 | `IStateContext` interface + concrete contexts: `LevelContext`, `RewardContext` | ⬜ Pending | — |
| P3-02 | `FSM.TransitionTo()` updated to accept optional `IStateContext` | ⬜ Pending | — |
| P3-03 | `UIViewState<TContext>` generic base — states declare their expected context type | ⬜ Pending | — |
| P3-04 | `FlowGraph` ScriptableObject — nodes (State + Edges) replace hardcoded `ApplicationFlowController` mappings | ⬜ Pending | — |
| P3-05 | `FlowGraph` Editor window — visual node + edge editor | ⬜ Pending | — |
| P3-06 | Transition guards — `CanTransition(State currentState)` virtual method on `Transition` | ⬜ Pending | — |
| P3-07 | `ISubView` interface — tab pages and swipeable panels inside a UIBase screen | ⬜ Pending | — |
| P3-08 | `IOverlayView` interface — popups and liveops panels spawned on top (no FSM involvement) | ⬜ Pending | — |
| P3-09 | `IGameHud` interface — gameplay UI companion with `SetLevelInfo`, `SetMoveCount`, `SetObjectiveProgress`, `SetBoosterCount` | ⬜ Pending | — |
| P3-10 | `FSMRuntime` MonoBehaviour — moves `PausedStates` stack and sorting order counter off SO asset | ⬜ Pending | — |

---

## Phase 4 — Service Interfaces (Remote-Ready Stubs)
> **Unlocks:** All LiveOps features can be built locally now. Firebase / backend plugs in later with zero refactor.
> **Prerequisite:** Phase 2 complete (can run in parallel with Phase 3).

| # | Item | Status | Date |
|---|------|--------|------|
| P4-01 | `IRemoteConfigService` + `LocalRemoteConfigService` (reads `StreamingAssets/remote_config.json`) | ⬜ Pending | — |
| P4-02 | `ISaveService` + `JsonFileSaveService` (local encrypted JSON, async on low-end) | ⬜ Pending | — |
| P4-03 | `ITimeProvider` + `LocalTimeProvider` (wraps `DateTime.UtcNow`) | ⬜ Pending | — |
| P4-04 | `IEconomyService` + `LocalEconomyService` (Coins, Gems, Lives, Energy — all earn/spend analytics-wired) | ⬜ Pending | — |
| P4-05 | `IDeviceProfileService` + `DeviceProfileService` (detects Low/Mid/High tier on boot) | ⬜ Pending | — |
| P4-06 | `ISoundService` interface wrapping existing `SoundService` | ⬜ Pending | — |
| P4-07 | `IPoolService` interface wrapping existing `PoolManagerSO` | ⬜ Pending | — |

---

## Phase 5 — Magic Sort Template Build
> **This is the AI agent's training material — every screen built perfectly following all rules.**
> **Prerequisite:** Phases 1–4 complete.
> Each screen needs: Manifest + State + View + ViewData + tagged Prefab in `Assets/Game/Screens/[Name]/`

| Screen | Type | Manifest | State | View | ViewData | Prefab Tagged | Status | Date |
|--------|------|----------|-------|------|----------|---------------|--------|------|
| `SplashScreen` | FSM State | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ Pending | — |
| `HubScreen` | FSM State + tabs | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ Pending | — |
| `JourneyPage` | ISubView | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ Pending | — |
| `ShopPage` | ISubView | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ Pending | — |
| `LeaderboardPage` | ISubView | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ Pending | — |
| `LiveOpsPanel` | IOverlayView | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ Pending | — |
| `RewardOverlay` | IOverlayView | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ Pending | — |
| `GameplayScreen` | FSM State + IGameHud | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ Pending | — |
| `LevelCompleteScreen` | FSM State | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ Pending | — |
| `LevelFailScreen` | FSM State | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ Pending | — |
| `SettingsScreen` | FSM State | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ Pending | — |
| `SpinWheelScreen` | FSM State | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ Pending | — |
| `DailyRewardScreen` | FSM State | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ Pending | — |

---

## Phase 6 — Editor Validation Tool
> **Unlocks:** AI agent self-check, enforces screen contract, catches integration errors before runtime.
> **Prerequisite:** Phase 1 complete. Can build alongside Phase 5.

| # | Item | Status | Date |
|---|------|--------|------|
| P6-01 | `ScreenManifestValidator` Editor window — lists all manifests, green/red checklist per screen | ⬜ Pending | — |
| P6-02 | Validation checks: pool registered, prefab exists, ComponentTags present, ViewData serializable, presets assigned, FlowGraph node exists | ⬜ Pending | — |
| P6-03 | "Fix All" button for auto-correctable issues | ⬜ Pending | — |
| P6-04 | Folder convention enforcer — Editor warning when screen files exist outside `Assets/Game/Screens/[Name]/` | ⬜ Pending | — |

---

## Phase 7 — AI Agent Integration
> **Unlocks:** Mockup → working screen with zero manual wiring. Game design JSON → compilable skeleton.
> **Prerequisite:** Phases 1–6 complete + Magic Sort template complete.

| # | Item | Status | Date |
|---|------|--------|------|
| P7-01 | `unity-mcp` bridge configured (already in packages — needs activation) | ⬜ Pending | — |
| P7-02 | Agent Capability 1: mockup image + screen name → full `Assets/Game/Screens/[Name]/` folder | ⬜ Pending | — |
| P7-03 | Agent Capability 2: `game_design.json` → `IGameLogic`, `LevelConfig`, `GameState`, `InputHandler`, `CommandManager` skeleton | ⬜ Pending | — |
| P7-04 | Agent self-validation: runs `ScreenManifestValidator` after generation, retries on failure | ⬜ Pending | — |
| P7-05 | Agent adds generated screen node to `FlowGraph` automatically | ⬜ Pending | — |

---

## Ongoing — Performance Hardening
> These run continuously from Phase 1 onward — not a blocker for any phase.

| # | Item | Status | Date |
|---|------|--------|------|
| PERF-01 | All `WaitForSeconds` cached as `private readonly` fields (see BUG-02 for TimeMachine) | ⬜ Pending | — |
| PERF-02 | DOTween recycler enabled on boot: `DOTween.SetTweensCapacity(200, 50)` | ⬜ Pending | — |
| PERF-03 | Pool prewarm spread across frames using `IDeviceProfileService.PoolPrewarmBudget` | ⬜ Pending | — |
| PERF-04 | Audit all `FindObjectsOfType` / `GetComponent` calls in hot paths (Update, coroutine loops) | ⬜ Pending | — |
| PERF-05 | Low-tier devices: simplified animations, reduced particle counts, smaller pool sizes | ⬜ Pending | — |
| PERF-06 | All saves on low-tier devices route through `SaveAsync` | ⬜ Pending | — |
| PERF-07 | Frame budget system for level loading and analytics batch flush | ⬜ Pending | — |

---

## LiveOps Features — Target Parity with Royal Match
> These are delivered through Phase 5 (template) and require Phase 4 service interfaces first.

| Feature | Phase | Status | Date |
|---------|-------|--------|------|
| Daily login reward | 5 | ⬜ Pending | — |
| Spin wheel (re-wire existing system to new architecture) | 5 | ⬜ Pending | — |
| Remote config — Firebase integration | Post-Phase 4 | ⬜ Pending | — |
| Timed limited events | Post-Phase 4 | ⬜ Pending | — |
| Season pass / battle pass | Post-Phase 5 | ⬜ Pending | — |
| Tournament with leaderboard | Phase 7+ | ⬜ Pending | — |
| Push notifications | Post-Phase 4 | ⬜ Pending | — |
| Downloadable level packs | Phase 7+ | ⬜ Pending | — |
| Force update / maintenance gate | Post-Phase 4 | ⬜ Pending | — |
| A/B test framework | Post-Phase 4 | ⬜ Pending | — |
| Cloud save / cross-device | Phase 7+ | ⬜ Pending | — |

---

## Overall Status

```
Phase 0 — Critical Bug Fixes      ████████░░  4 / 5   (80%) — BUG-05 manual
Phase 1 — Screen Contract         ░░░░░░░░░░  0 / 6   (0%)
Phase 2 — vContainer DI           ░░░░░░░░░░  0 / 6   (0%)
Phase 3 — Modular Flow            ░░░░░░░░░░  0 / 10  (0%)
Phase 4 — Service Interfaces      ░░░░░░░░░░  0 / 7   (0%)
Phase 5 — Magic Sort Template     ░░░░░░░░░░  0 / 13  (0%)
Phase 6 — Editor Validation       ░░░░░░░░░░  0 / 4   (0%)
Phase 7 — AI Agent Integration    ░░░░░░░░░░  0 / 5   (0%)
Performance Hardening             ░░░░░░░░░░  0 / 7   (0%)
Documentation                     ██████████  10 / 10 (100%)
```

---

## What's Next

**Immediate next action: BUG-05 (manual), then Phase 1.**

BUG-05: In the Unity Editor, delete `Assets/Saad/UI/SpinWheel/` (the legacy folder with 3 scripts). The new system lives in `Assets/Saad/UI/Spin Wheel/`. Do this in the Editor so Unity removes the `.meta` files and cleans up references properly.

Once BUG-05 is done, Phase 0 is complete. Move to Phase 1 — ScreenManifest. This is the unlock that makes everything else systematic.
