# State Prompt Template

> Copy this template, fill in the `{placeholders}`, and paste it as your Claude prompt.
> Delete any sections that don't apply (e.g., remove Variables section if there are none).
> The generic rules live in `STATE_IMPLEMENTATION_GUIDE.md` — this prompt personalizes per state.

---

## Template

```
Read STATE_IMPLEMENTATION_GUIDE.md first — it contains all architecture rules and patterns.

Then read the PROMPT.md and mock image for this state:
- Assets/Game/Screens/{Name}/PROMPT.md
- Assets/Game/Screens/{Name}/Art/Mock.png

Then read these reference implementations (pick the closest match):
- For UIState with toggles/multiple buttons: Assets/Game/Screens/GameSettingsX/Scripts/ (all 4 files)
- For UIState with single button: Assets/Saad/UI/MainMenu/Scripts/ (MainMenuView.cs + MainMenuState.cs)
- For GameState: Assets/Saad/UI/GameState/Scripts/NormalGameState.cs + Assets/Saad/UI/GameHud/Scripts/NormalGameHud.cs

Also read these core files:
- Assets/Saad/Utilities/StateMachine/Scripts/UiViewState.cs
- Assets/Saad/GameFlow/Scripts/BaseApplicationFlowController.cs
- Assets/Saad/GameFlow/Scripts/ApplicationFlowController.cs

---

## State: {Name}
- **Type:** {UIState | GameState}
- **Close Behaviour:** {PopOne | ClearAll | Default}
- **Pause Previous State:** {true | false}

## Mock Description
{Describe the mock layout in detail — button positions, colors, icons, text elements,
their arrangement on screen. Don't rely on AI interpreting the image perfectly.}

## Functionality
{Describe what each interactive element does:
- Which buttons navigate (and where)
- Which buttons toggle values (and which SO variables)
- Which elements display data (and from what source)
- Any special behavior (timers, animations, conditional visibility)}

## Variables Needed
{List any ScriptableObject variables this state needs. Format:}
- `v_{VariableName}` — {DBBool | DBInt | Bool | Int} — {purpose}

## Navigation Events
{Describe how this state is entered and exited:}
- **Entry:** GoTo{Name}Event fires from {source state}
- **Exit:** {e_BackBtnPressed (PopOne) | GoTo{Destination}Event (ClearAll) | describe}

---

## Implementation Tasks

### 1. Implement Scripts
Replace all `// AI agent:` comments in:
- Assets/Game/Screens/{Name}/Scripts/{Name}UIView.cs {or {Name}Hud.cs}
- Assets/Game/Screens/{Name}/Scripts/{Name}ViewData.cs
- Assets/Game/Screens/{Name}/Scripts/{Name}State.cs

### 2. After Compile — Build Prefab via MCP
1. Open prefab: Assets/Game/Screens/{Name}/Prefabs/{Name}.prefab
2. Create child GameObjects under Content:
   {List each GO with: name, components, anchor, position, size}
3. {If any GOs need children (e.g., icons inside buttons), list those}
4. Wire all SerializeField references on {Name}UIView using execute_code
   (use GetType().Name matching for component lookup)
5. Save the prefab

### 3. Create & Assign SO Assets via MCP
1. {List any variable assets to create: e.g., "Create DBBool: v_SoundEnabled.asset in Config/"}
2. {Find existing assets: e.g., "Find e_BackBtnPressed.asset in the project"}
3. Assign on {Name}State.asset:
   {List every field to asset mapping}
4. Verify all fields: stateId, usePooling, uIStatesPooler, and all custom refs

### 4. AFC Integration (skip for PopOne states)
{If ClearAll/Default — describe the AFC wiring:
- Add Header + fields to ApplicationFlowController.cs
- Register/unregister in flow events
- Add handler method
- Wire SO refs on AFC prefab}
```

---

## Example: GameSettings (Default + Pause overlay with toggles)

```
Read STATE_IMPLEMENTATION_GUIDE.md first — it contains all architecture rules and patterns.

Then read the PROMPT.md and mock image for this state:
- Assets/Game/Screens/GameSettings/PROMPT.md
- Assets/Game/Screens/GameSettings/Art/Mock.png

Then read these reference implementations:
- Assets/Game/Screens/GameSettingsX/Scripts/ (all 4 files — closest match: toggles + multiple buttons)

Also read these core files:
- Assets/Saad/Utilities/StateMachine/Scripts/UiViewState.cs
- Assets/Saad/GameFlow/Scripts/BaseApplicationFlowController.cs
- Assets/Saad/GameFlow/Scripts/ApplicationFlowController.cs

---

## State: GameSettings
- **Type:** UIState
- **Close Behaviour:** Default
- **Pause Previous State:** true

## Mock Description
5 round buttons stacked vertically on the right edge of the screen.
Top = Settings gear button (blue, exits state).
Then Sound toggle button (green when enabled, grey when disabled).
Then Haptics toggle button (green when enabled, grey when disabled).
Then Restore purchases button (green).
Bottom = Exit button (red, exits state).
Both Settings and Exit buttons fire the same exit event.
Sound and Haptics buttons have child icon Images that change color based on enabled state.

## Functionality
- **SettingsBtn:** Fires exit event (e_BackBtnPressed → PopOne back to previous state)
- **SoundBtn:** Toggles v_SoundEnabled DBBool, refreshes view to update icon color
- **HapticsBtn:** Toggles v_HapticsEnabled DBBool, refreshes view to update icon color
- **RestoreBtn:** Logs restore purchases requested (placeholder for IAP integration)
- **ExitBtn:** Fires same exit event as SettingsBtn

## Variables Needed
- `v_SoundEnabled` — DBBool — persistent sound on/off toggle
- `v_HapticsEnabled` — DBBool — persistent haptics on/off toggle

## Navigation Events
- **Entry:** GoToGameSettingsEvent fires from NormalGameState (settings button on HUD)
  - AFC handler: `GoTo(GameSettingsTransition, UICloseReasons.FullScreenPlacement)`
  - FullScreenPlacement → Default policy + pauses previous state (game stays alive behind overlay)
- **Exit:** e_BackBtnPressed (PopOne) — resumes the previous state (gameplay)

---

## Implementation Tasks

### 1. Implement Scripts
Replace all `// AI agent:` comments in:
- Assets/Game/Screens/GameSettings/Scripts/GameSettingsUIView.cs
- Assets/Game/Screens/GameSettings/Scripts/GameSettingsViewData.cs
- Assets/Game/Screens/GameSettings/Scripts/GameSettingsState.cs

### 2. After Compile — Build Prefab via MCP
1. Open prefab: Assets/Game/Screens/GameSettings/Prefabs/GameSettings.prefab
2. Create child GameObjects under Content:
   - SettingsBtn: Button + Image, anchor top-right, pos (-60, -120), size 80x80
   - SoundBtn: Button + Image, anchor top-right, pos (-60, -210), size 80x80
   - HapticsBtn: Button + Image, anchor top-right, pos (-60, -300), size 80x80
   - RestoreBtn: Button + Image, anchor top-right, pos (-60, -390), size 80x80
   - ExitBtn: Button + Image, anchor top-right, pos (-60, -480), size 80x80
3. Create icon children:
   - SoundIcon as child of SoundBtn: Image, anchored stretch with 15px inset
   - HapticsIcon as child of HapticsBtn: Image, anchored stretch with 15px inset
4. Wire all SerializeField references on GameSettingsUIView using execute_code
   (use GetType().Name matching for component lookup)
5. Save the prefab

### 3. Create & Assign SO Assets via MCP
1. Create DBBool assets: v_SoundEnabled.asset and v_HapticsEnabled.asset in Config/
2. Find e_BackBtnPressed.asset in the project (exit event for PopOne states)
3. Assign on GameSettingsState.asset:
   - GoToHomeEvent = e_BackBtnPressed
   - SoundEnabled = v_SoundEnabled
   - HapticsEnabled = v_HapticsEnabled
4. Verify all fields: stateId, usePooling, uIStatesPooler, and the above refs

### 4. AFC Integration
The State Creator tool already patches AFC with the GoToGameSettingsEvent + Transition fields.
Verify the handler uses the correct UICloseReasons:
```csharp
// CORRECT — Default policy + pauses previous (overlay on top of game)
private void HandleGoToGameSettings() =>
    GoTo(GameSettingsTransition, UICloseReasons.FullScreenPlacement);

// WRONG — Home triggers ClearAll which destroys the game state behind it
// private void HandleGoToGameSettings() =>
//     GoTo(GameSettingsTransition, UICloseReasons.Home);
```
```
