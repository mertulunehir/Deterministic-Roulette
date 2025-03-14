# Unity Roulette Game

A fully functional 3D roulette game implementation in Unity featuring realistic wheel physics, comprehensive betting system, and game history tracking.

## Game Overview

This roulette game provides players with an authentic casino experience, complete with:
- Full 3D roulette wheel with realistic ball physics
- Multiple camera angles that switch dynamically during gameplay
- Comprehensive betting system supporting all standard roulette bet types
- Chip placement and removal with intuitive controls
- Game history and statistics tracking
- Save system to preserve player progress

## Controls and Gameplay Instructions

### Placing Bets
1. **Select Chip Value**: Use the buttons at the bottom of the screen to select your bet amount (10, 50, 100, or 200).
2. **Place Chips**: Tap/click on any betting position on the table to place a chip.
3. **Remove Chips**: Long press on a chip to pick it up. You can place it somewhere else.
4. **Cancel All Bets**: Press the "Cancel" button to remove all chips from the table.

### Spinning the Wheel
1. After placing your bets, press the "Spin" button to start the wheel.
2. The camera will automatically switch to follow the action (table view → spin view → ball view → back to table).
3. After the ball lands, your winnings (if any) will be calculated and added to your balance.

### Deterministic Mode
The game includes a development option to force specific outcomes:
- Set `isDeterministic` to true in the `RouletteWheelController` and specify `currentTargetNumber` for testing specific scenarios.

### Statistics and History
- Tap the Stats button to view your game history and statistics.
- The history panel shows your recent games, including bet types, amounts, and outcomes.
- Statistics include total wins, losses, and win rate.

## Technical Implementation

### Core Systems

#### Betting System
- Supports all standard roulette bets: straight, split, street, corner, six line, column, dozen, and even/odd/red/black/high/low.
- Each bet type has appropriate payout multipliers.
- The system validates bets against the player's balance and prevents over-betting.

#### Wheel Physics
- The wheel rotation and ball movement use physics-based calculations.
- The ball's trajectory is influenced by realistic forces like gravity, tangential force, and downward force.
- When in deterministic mode, the ball is guided toward the target number pocket using targeted forces.

#### Camera System
- Multiple camera positions track different game phases.
- Smooth transitions between camera views enhance the visual experience.
- Cameras automatically switch based on game events.

#### Object Pooling
- Chip objects are pooled for better performance.
- The system dynamically expands the pool when needed.

#### Event System
- Uses an event-driven architecture for decoupled communication between components.
- Events include game actions like chip placement, spin initiation, and win calculations.

#### Save System
- Saves player balance, game history, and betting statistics.
- Persists data between game sessions using PlayerPrefs.

### OOP Principles Applied

#### Encapsulation
- Properties and methods are properly scoped with public, private, and protected modifiers.
- Data is accessed through getters and setters, particularly in the `SaveManager` and `MoneyCanvasController` classes.
- Example: `PlaceBetType` and `ConnectedNumbers` in `TableNumberPlace` expose read-only access to internal data.

#### Single Responsibility Principle
- Classes have clear, focused responsibilities:
  - `ChipPool` manages the object pooling system
  - `RouletteWheelController` handles wheel physics and rotation
  - `CameraController` manages camera transitions
  - `SaveManager` handles data persistence
  - `RouletteBetController` processes bet logic and calculations

#### Open/Closed Principle
- The event system allows extending functionality without modifying existing code.
- New bet types can be added to the `BetTypes` enum without changing core bet processing logic.

#### Dependency Inversion
- High-level modules (like `GameManager`) don't depend on low-level modules but on abstractions.
- Components communicate through the `EventManager` rather than direct references.

#### Interface Segregation
- Components expose only what's necessary through public methods and properties.
- Clean interfaces between systems allow for modular development.

#### Composition Over Inheritance
- Game objects are composed of multiple components rather than using deep inheritance hierarchies.
- For example, the roulette wheel combines `RouletteWheelController` and `RouletteWheelNumberController`.

## Known Issues and Future Improvements

### Known Issues
- Ball physics can occasionally be unpredictable when using very high spin speeds.
- Some edge cases in bet validation may allow placing chips with insufficient funds.

### Planned Improvements
- Implement different roulette variations (American, European, French).
- Add chip stacking animations and improved visual feedback.
- Enhance sound effects and music integration.
- Add VFX for winning bets and major wins.


## Demo Video

https://drive.google.com/file/d/1LX77mmwbZ4uZ4PN3Hr1KQK18yRJCroTH/view?usp=sharing

Note: Due to a Unity editor recording issue, the demo video may show some pixelation during camera transitions. This is caused by the screen recorder and does not occur during actual gameplay in the Unity editor.

## Requirements

- Unity 2022.3 or higher

## Installation

1. Clone this repository
2. Open the project in Unity
3. Open the game scene from the Scenes folder
4. Press Play to run the game in the editor
