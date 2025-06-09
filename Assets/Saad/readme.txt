Game Architecture Overview
Introduction
This project utilizes a robust and flexible game architecture designed to manage the flow of game states, variables,
and events through the use of ScriptableObjects. The architecture includes a state machine that handles game states,
including pausing and resuming functionality, with a stack to manage paused states. Additionally, utility scripts like
JSON handling are incorporated to streamline data management.

Key Components
1. ScriptableObject Variables
Description: These are reusable variables that can be shared across different components of the game. By leveraging
ScriptableObjects, these variables can be easily modified and tested without directly altering the codebase.
Examples: Health, Score, PlayerPosition.

2. ScriptableObject Events
Description: ScriptableObject-based events allow for a decoupled and flexible event-driven architecture. These events
facilitate communication between different game systems without tight coupling.
Examples: GameOverEvent, LevelCompletedEvent.

3. Utilities
JSON Handling: Utilities include custom JSON handlers for saving and loading game data. This ensures that data
serialization is handled efficiently, making it easier to maintain game state across sessions.
Other Utilities: Additional utility scripts are included to support common tasks such as logging, configuration management,
and more.

4. State Machine for Game Flow
Description: The state machine is the core of the game flow management. It manages transitions between different states
such as menus, gameplay, and pause screens.

State Management:
Paused States Stack: When a state is paused, it is pushed onto the paused states stack. This allows for easy resumption
of the state when required.
State Transitions: If a state is on the top of paused states stack(last paused) during a transition, the state is resumed. If the state is not
in the stack or not on the top, the stack is cleared, and the new state is activated.

5. State Handling
Pause and Resume: States can be paused and resumed seamlessly. The paused states stack ensures that states are correctly
managed, allowing for a smooth user experience.
Stack Management: The architecture ensures that states are only resumed if they exist within the stack. Otherwise, a fresh
state is loaded, and the stack is reset.

Conclusion
This architecture is designed to be flexible, scalable, and easy to maintain. By using ScriptableObjects, event-driven programming,
and a robust state machine, the architecture provides a solid foundation for any game development project. The inclusion of utility
scripts and the management of paused states further enhances the system, ensuring that the game flow is smooth and intuitive.

Feel free to explore the components mentioned above, and refer to the codebase for detailed implementation. Happy coding!