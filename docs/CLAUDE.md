# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Single-project .NET 8 console application (`UMLIoT.csproj` / `IOT.sln`) that simulates an IoT device manager. The whole app is the entry point in `Program.cs` — a menu loop that delegates every operation to `IoTFacade`. There are no tests.

## Commands

Run from the repo root (where `IOT.sln` lives):

```powershell
dotnet build                  # compile
dotnet run                    # build + launch the interactive console menu
dotnet run --project UMLIoT.csproj
dotnet build -c Release
```

There is no test project, no linter config, and no CI. The "test" loop is exercising the menu manually.

## Architecture

The codebase is organized by **layer first, then pattern** — not by feature. Two top-level folders:

- `Core/` — domain (Devices, Users) and the orchestrator (`Controllers/ControladorIOT`).
- `Patterns/` — one folder per GoF pattern used (Facade, Factory, Command, Observer, State).

### Request flow

`Program.cs` only knows about `IoTFacade`. Every menu option calls a facade method.

```
Program.cs (menu)
   └─> IoTFacade  ──────────────────────────────────────┐
        ├─ AuthService + UserRepository  (login state)  │
        └─ ControladorIOT  (device registry)            │
              ├─ DeviceCreator (Factory Method) ────────┤
              ├─ ICommand.execute()  (Command)          │
              └─ DeviceEventManager.notifyObservers() ──┘
                    └─> MobileNotifier / SecuritySystem / EventLogger
```

Key consequence: **`Program.cs` should not import anything under `Core.Controllers`, `Patterns.Factory`, `Patterns.Command`, etc. directly.** New user-facing operations belong on `IoTFacade`. The facade is the seam.

### Device model

- `IDevice` is the minimal contract (`connect` / `disconnect` / `getStatus`). Capabilities are split into separate interfaces — `ISwitchable`, `IMonitorable`, `IAlarm` — and the facade narrows by `is` checks before dispatching commands. When adding a new capability, prefer a new small interface over widening `IDevice`.
- `Device` (abstract) holds `id`, `name`, `ipAddress`, and the current `DeviceStatus`. Concrete devices (`Camera`, `Smartlight`, `Alarm`) extend it.
- `id` and `ipAddress` are **assigned by `ControladorIOT.addDevice`**, not by the caller or the creator. The factory call passes placeholders; `ControladorIOT` overwrites them with the next sequential id and `192.168.1.{id}`. Don't try to set ids in the menu / facade — they will be overwritten.

### Adding a new device type

1. Add a class under `Core/Devices/` extending `Device` and implementing the relevant capability interfaces (`ISwitchable`, `IMonitorable`, `IAlarm` as needed).
2. Add a `XCreator : DeviceCreator` under `Patterns/Factory/Devices/` whose `DeviceCreatorMethod()` returns an instance.
3. Wire a new `case` into the `switch` inside `IoTFacade.registerDevice` — that switch is the single place type strings are mapped to creators.
4. If the device needs extra registration parameters (like `Smartlight`'s `color` / `schedule`), prompt for them in `Program.cs` case `"3"` and pass them through the `config` dictionary.

### State pattern

`DeviceStatus` is an abstract base with concrete `OnlineStatus`, `OfflineStatus`, `ErrorStatus`. Devices start in `OfflineStatus`; `connect()`/`disconnect()` swap the instance. `Device.handleStatus()` delegates to the current status. Don't add status-dependent `if`/`switch` blocks in `Device` or commands — push the behavior into a `DeviceStatus` subclass.

### Observer pattern

`DeviceEventManager` is wired up once in `Program.Main` with three observers (`MobileNotifier`, `SecuritySystem`, `EventLogger`) and injected into the facade via `setEventManager`. `ControladorIOT` fires `notifyObservers(device, "Device Added")` when a device is registered. To emit a new event, call `eventManager?.notifyObservers(...)` from the controller — observers branch on the `eventType` string.

### Auth

`AuthService` keeps a single `currentUser` field — this is global state per process, not per session. `IoTFacade.isUserLoggedIn()` is the gate that menu option 3 checks before allowing device registration. Other options currently do **not** enforce login (intentional or not, that's the current behavior).

## Conventions in this codebase

- Method names are camelCase (`registerUser`, `getStatus`, `turnOnDevice`) — not the usual C# PascalCase. Match the existing style when adding methods to existing types.
- Namespaces follow folder layout under the root `UMLIoT` namespace (e.g., `UMLIoT.Patterns.Factory.Devices`). File-scoped namespace declarations are used.
- Persistence is in-memory only (`List<>` fields in `UserRepository` and `ControladorIOT`). State is lost when the process exits.
- `Nullable` and `ImplicitUsings` are both enabled — use `?` annotations and rely on the implicit `using`s rather than re-importing `System`/`System.Collections.Generic`.

## Git hygiene note

`obj/` is in `.gitignore` but the build artifacts inside it were committed before the ignore rule was added, so they keep showing up as modified in `git status`. If asked to "clean up" the repo, the fix is `git rm -r --cached obj/ bin/` followed by a commit — not deleting the local files.
