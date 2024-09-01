# GameArchitecture
Video Link:  https://drive.google.com/file/d/1OkQJ3HKrlkon6Of4p5BwTtCSfPudwRbX/view?usp=sharing
This game architecture combines the power of the State Pattern, scriptable objects, observer patterns, and dependency injection to create a modular, reusable, and developer-friendly environment. It's designed to support collaborative development efforts while ensuring a robust and maintainable codebase.

Architecture Features:
State Machine for states flow(OnEnter, OnExit, OnPause, OnResume).
Scriptable Objects Variables(Persistent and Non-Persistent)(Could be serialized anywhere and work as a dependency injection).
Scriptable Objects Events(With Parameters, With Return Values).
Json Utility(All Json Related functionalities).
Application Flow Controller(All States Transitions happens here using FSM, all you need to invoke is desired state gameevent from anywhere in the game).
Game Hud(Manages All Ui of the game state, thats how we seperate game/controller logic with the ui)
