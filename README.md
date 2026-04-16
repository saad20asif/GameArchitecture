# GameArchitecture
Video Link:  https://drive.google.com/file/d/1OkQJ3HKrlkon6Of4p5BwTtCSfPudwRbX/view?usp=sharing
This game architecture combines the power of the State Pattern, scriptable objects, observer patterns, and dependency injection to create a modular, reusable, and developer-friendly environment. It's designed to support collaborative development efforts while ensuring a robust and maintainable codebase.

FEATURES:

State Machine for states flow (OnEnter, OnExit, OnPause, OnResume). The FSM is animation-agnostic — views own their own presentation via `UseDefaultAnimations` + `OnCustomShow/Hide/Pause/Resume` hooks on `UIBase` and `GameHud`.

ScriptableObject Variables (Persistent and Non-Persistent; serializable anywhere, work as dependency-injection-style shared state).

ScriptableObject Events (With Parameters, With Return Values).

Json Utility (All Json related functionalities).

Application Flow Controller (All state transitions happen here via the FSM; anywhere in the game you just invoke the corresponding `GameEvent` SO). All subscriptions use named methods, never lambdas.

Game Hud (Manages gameplay UI separately from game/controller logic).

State Creator Tool (**Tools → State Creator**) — an Editor window that scaffolds a new screen (State SO, View MB, ViewData POCO, Transition SO, GameEvent SO, prefab, folder) in one form. Canonical reference output: `Assets/Game/Screens/GameSettingsX/`.

DOCS INDEX:

- `ARCHITECTURE.md` — full technical deep-dive (state machine, UI system, pool, events, data flow, GameSettingsX reference)
- `ARCHITECTURE_RULES.md` — non-negotiable rules (layers, SO usage, FSM, animation independence, DI, LiveOps, **docs-sync rule §11**)
- `QUICK_REFERENCE.md` — one-line answers for common decisions
- `SCRIPTS_INDEX.md` — every script with a one-line description
- `BUILD_STATE_CREATOR_TOOL.md` — the State Creator Editor tool
- `PROGRESS.md` — current phase status
- `ROADMAP.md` — phased improvement plan
- `VISION_AND_DIRECTION.md` — long-term direction
- `AI_AGENT_CONTEXT.md` / `ARCHITECTURE_PROMPT.md` — onboarding context for AI agents
