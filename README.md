# Superplay

Superplay is a C#/.NET project designed as a modular multiplayer game server framework.  
It features player management, in-memory state, resource handling, and extensible message routing—ideal for turn-based or casual multiplayer games.

---

## What Can Superplay Do?

### 1. Player Management
- **Login/Logout:**  
  Players can log in and out using a unique identifier. Duplicate logins are prevented.
- **Session Tracking:**  
  The server keeps track of which players are currently online.
- **Player State:**  
  Each player has a state object that tracks their resources, IDs, and other game-related data.

### 2. Resource & Economy Management
- **Resource Updates:**  
  Players can earn or spend resources (such as coins, rolls, or other in-game currencies).
- **Transfers:**  
  Players can transfer resources to each other, with server-side balance checks to prevent overdrafts.

### 3. Extensible WebSocket Messaging
- **Dynamic Routing:**  
  The server routes incoming messages to the appropriate handler using a registry-based dispatch system (no lengthy switch/case blocks).
- **Adding New Features:**  
  Developers can add new message types by implementing a handler interface and registering it with the server—no need to modify core logic.
- **Shared Models:**  
  Both server and client use shared message and model definitions, making it easy to keep communication in sync.

### 4. Testing & Reliability
- **Unit Tests:**  
  The core player and resource logic are covered by NUnit test cases, ensuring correctness and supporting refactoring.

### 5. Logging & Observability
- **Structured Logging:**  
  All server actions (logins, errors, resource changes, etc.) are logged via Serilog for easier debugging and monitoring.

---

## Example Use Cases

- **Turn-Based Games:**  
  Manage turns, player actions, and in-game resources for casual or social games.
- **Social Features:**  
  Add friend systems, gifting, or in-game economies.
- **Prototyping:**  
  Quickly implement and test new multiplayer game ideas by adding custom message handlers.

---

## Extending the App

To add a new feature:
1. Define a new message type in the Shared project.
2. Implement `IMessageHandler` for your message in the server.
3. Register your handler in the server's registry during startup.

---

## Quick Start

### Requirements
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Run the Server
```bash
dotnet run --project GameServer
```

### Run Tests
```bash
dotnet test GameServer.Tests
```

---

## Project Structure Overview

```
GameServer/        # Main server logic and message handlers
Shared/            # Shared models and message definitions
GameServer.Tests/  # NUnit-based tests for server logic
```

---

## License

MIT

---

*For architecture diagrams, message format details, or more advanced customization, see the source code or open an issue!*