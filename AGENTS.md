# AGENTS.md

This file provides guidance to Codex and other coding agents when working with code in this repository.

## Non-Negotiable Gates

Run these gates before code changes. They are the short operational version of the detailed rules below.

1. **Existing project first** - check `AGENTS.md` and current `Assets/_Project` code before using `ecs-survivors` or proposing a new pattern. Existing AtomicConnection code and approved deviations win.
2. **Responsibility gate** - before adding behavior to a runtime `MonoBehaviour`, decide whether it extends the component's current responsibility or adds a new one. New responsibility means a focused sibling/child component by default, not a bigger root object.
3. **Reuse gate** - before creating a new component or object-local logic, search for similar components, interfaces, or duplicated behavior. Reuse, extend, or extract a focused component when the responsibility matches.
4. **Boundary gate** - object-internal behavior belongs to focused components on that object; cross-object coordination, input polling, spawning, cleanup, scene lifecycle, and active loops belong to state-owned services/states.
5. **Root object gate** - runtime root components may gather references, pass config, and expose small object-level APIs. They must not accumulate state, input, visual feedback, targeting, cooldown, attack/effect, and logging behavior.
6. **Event/update gate** - before adding checks to an `Update` loop, decide whether the behavior is truly per-frame. Discrete changes should use events/subscriptions or explicit calls instead of polling every frame.
7. **Lifecycle gate** - states decide when; services know how. Loading states load scenes, enter/setup states prepare runtime objects, active states tick and clean owned runtime services/objects.
8. **Unity asset gate** - keep `.meta` files with scripts/assets, preserve GUIDs when moving assets, update prefabs/scenes for new serialized fields/components, and do not avoid components just to avoid `.meta` files.
9. **Design/docs gate** - if code changes player-facing gameplay/content/UI rules, update `AtomicConnection_GDD.md`; if it changes concrete numbers, update `AtomicConnection_BALANCE.md`.
10. **Validation gate** - run available compile/static checks and report anything Unity Editor/manual validation must cover.

## Project

AtomicConnection is a Unity project built with Unity `6000.4.4f1` + URP. Game design is documented in [AtomicConnection_GDD.md](AtomicConnection_GDD.md), while concrete gameplay numbers and tuning tables live in [AtomicConnection_BALANCE.md](AtomicConnection_BALANCE.md). Infrastructure is still template-derived; `Gameplay/Units` and `Gameplay/Enemies` are minimal example features used to exercise lifecycle and DI patterns until production gameplay replaces them.

## Build & Run

This is a Unity project - there is no CLI build command. Open in Unity `6000.4.4f1` and press Play. Scenes must be loaded in order: `Bootstrap -> MainMenu -> Gameplay` (configured in Build Settings).

No project-owned automated tests exist under `Assets/_Project`. Manual validation is done by running the game in the Editor.

## Repository Rules

- Keep Unity `.meta` files together with their assets and scripts.
- Do not edit generated folders: `Library`, `Temp`, `Obj`, `Logs`, `UserSettings`, `.vs`, `.idea`, or IDE caches.
- Avoid editing generated solution/project files (`*.sln`, `*.csproj`) unless the task explicitly requires it.
- Prefer changes inside `Assets/_Project` for project code. Treat `Assets/Plugins`, `Assets/NaughtyAttributes`, `Assets/TextMesh Pro`, and imported packages as vendor code unless asked otherwise.
- Do not revert unrelated local changes. This repository may contain user, IDE, or Unity-generated changes in progress.

## Architecture

### Lifecycle reference lock

Lifecycle, state machine, DI, scene loading, UI flow, and gameplay loop work is reference-locked to `ecs-survivors`.
Resolve its local path from `REFERENCES.local.md` first when present, otherwise from `REFERENCES.md`.
If neither file contains a valid local path, ask the user for the current `ecs-survivors` checkout path before doing lifecycle work.

Implemented AtomicConnection architecture is the first source of truth. When a rule, pattern, or working example already exists in `AGENTS.md` or `Assets/_Project`, follow this project even if it intentionally differs from `ecs-survivors`.

Use `ecs-survivors` only for new architecture/lifecycle decisions that are not already described or exemplified in AtomicConnection, or when the user explicitly asks to compare with the reference. Do not "correct" user-approved AtomicConnection deviations back to `ecs-survivors` unless the user asks to revisit that decision.

Before recommending, documenting, or implementing a new lifecycle pattern:

1. Check `AGENTS.md` and existing AtomicConnection code first.
2. If the pattern is not already covered locally, find the matching `ecs-survivors` class/file or concrete pattern.
3. Map that pattern to this project without adding extra architectural layers.
4. If there is no matching pattern in `ecs-survivors`, label it as a new proposal and ask before adding it to docs or code.

Reference-backed lifecycle rules:

- UI may call `GameStateMachine.Enter(...)`, as in `HomeHUD` / `GameOverWindow` from `ecs-survivors`.
- UI may query UI-facing gameplay/meta services and static data for display, as in `LevelUpWindow` and `ShopWindow`.
- Current AtomicConnection decision: gameplay/meta choice UI uses MVP direct service calls for now, not request queues. This is a user-approved simplification from the `ecs-survivors` request-entity pattern.
- UI must not call `SceneLoader.LoadScene(...)` or `SceneLoader.ReloadScene(...)` directly.
- UI must not run gameplay cleanup or control gameplay service lifecycle. Opening/closing gameplay menu may toggle the high-level `PauseService`, but UI must not start, stop, tick, or clean individual gameplay services.
- Runtime gameplay/meta changes from UI should go through high-level feature/domain services, not scattered low-level calls across multiple services.
- Loading states own scene loading.
- Enter/setup states prepare a mode after scene load.
- Active states own update, exit, and cleanup.
- `GameStateMachine` resolves states through `IStateFactory`, which resolves concrete state instances from Zenject, matching `ecs-survivors`.
- Bind states in `ProjectInstaller`; do not manually register state instances into the state machine.
- DI creates and wires objects, but does not decide when gameplay starts.
- Scene initializers write concrete scene references into concrete providers/services.
- Do not add flow services, gameplay runtime/session wrappers, generic scene scopes, lifecycle registries, or other lifecycle-owner abstractions unless the user explicitly asks for a separate proposal.
- Do not add presenter/controller/flow layers as lifecycle intermediaries. Existing UI MVP code may stay where it is already part of the project.

Reference map for common lifecycle/UI decisions:
- `HomeHUD` backs scene HUD flow: UI button calls `IGameStateMachine.Enter(...)` or `IWindowService.Open(...)`.
- `LoadingBattleState` backs loading-state ownership: loading state calls scene loader, then enters setup state.
- `BattleEnterState` backs enter/setup ownership: setup state creates gameplay runtime objects, then enters active loop state.
- `BattleLoopState` backs active loop ownership: active state receives ticks and owns exit cleanup.
- `StateFactory` backs state resolution: state machine asks factory, factory resolves concrete states from Zenject.
- `LevelInitializer` and `UIInitializer` back concrete scene references: scene objects write references into concrete providers/services.
- `WindowService`, `WindowFactory`, `BaseWindow`, `WindowsConfig`, `GameOverWindow`, and `GameOverState` back dynamic window flow.
- `LevelUpWindow`, `ShopWindow`, `UpgradeAbilityOnRequestSystem`, and `BuyItemOnRequestSystem` are the reference for request-based gameplay/meta choice UI, but request transport is deferred in this template.

Reference-backed asset and prefab rules:

- Runtime prefab creation from gameplay states/factories is part of lifecycle and DI work. It must be checked against the full `ecs-survivors` chain, not only against the final `IInstantiator` call.
- The reference chain for gameplay views is: state/domain factory creates gameplay data with a resource path, `IAssetProvider` loads from `Resources`, and a view factory instantiates with Zenject `IInstantiator`.
- Do not replace this with serialized gameplay prefab fields on `ProjectInstaller` unless `ecs-survivors` has the same pattern for that case, or the user explicitly approves it as a new proposal.
- A partial API match is not enough. If only `InstantiatePrefabForComponent(...)` matches but asset ownership/loading differs, call it out as a mismatch before changing code or docs.
- Do not copy ECS-only service dependencies such as `GameEntity`, generated contexts, or `ICollisionRegistry` into this template. When a reference service depends on ECS, keep only the lifecycle/DI placement and adapt the service API to Unity-neutral types.

Service lifecycle rule:

```text
State decides when.
Service knows how.
```

If a service is passive, do not add lifecycle methods to it. If a service owns subscriptions, timers, spawn loops, input modes, async tasks, or update work, expose explicit methods such as `Start`, `Stop`, `Enable`, `Disable`, `Update`, or `Cleanup`, and call them from the owning state.

Event/update rule:

```text
Update is for continuous work.
Events are for discrete changes.
```

Do not add per-frame checks to a state or service when the behavior can be triggered by an event, subscription, callback, or explicit method call. Use `Update` for truly continuous work such as input polling, timers, movement, cooldown progression, spawn loops, and active ticking. Use events/subscriptions for one-off or edge-triggered changes such as shot requests, death, charge completion, resource changes, object creation, UI selections, or state notifications. Services that subscribe to runtime-object events must expose lifecycle methods such as `Start`/`Cleanup`, subscribe only while their owning state is active, and unsubscribe during cleanup. If polling is intentionally chosen for a discrete change, document the reason in the code or architecture notes.

### Architecture workflow

- `AGENTS.md` is the source of truth for current architecture rules.
- Keep new or undocumented lifecycle decisions reference-backed by `ecs-survivors`; if a needed decision is neither covered locally nor reference-backed, stop and present it as a separate proposal.
- After each architecture/lifecycle/UI/DI slice, check whether `AGENTS.md` still matches the implemented architecture. Update it before the final response if current flow, state registration, lifecycle rules, or project conventions changed.
- Every time code adds gameplay, content, UI behavior, player-facing rules, or other design-relevant behavior that is not already described in `AtomicConnection_GDD.md`, update the GDD in the same task before the final response. Keep concrete gameplay numbers out of the GDD; if the change adds or changes balance values, update `AtomicConnection_BALANCE.md` instead or alongside the GDD. Pure infrastructure refactors that do not change design or player-facing behavior do not require GDD or balance-document updates.

### Layer structure

```
Assets/_Project/Scripts/
├── Gameplay/         # Game features, one subfolder per feature
│   ├── Cameras/      # Camera provider and camera-facing gameplay helpers
│   ├── Common/       # Small common gameplay services: time, random, physics
│   ├── Enemies/      # Minimal enemy feature: view, factory, spawner service
│   ├── Level/        # Concrete scene references and providers
│   ├── Talents/      # Talent tree config, progress service, and test/window UI
│   ├── Units/        # Atom core, free atom, and related unit feature folders
│   └── Windows/      # Dynamic window infrastructure and window configs
├── Infrastructure/   # App lifecycle: GameRunner, state machine, states, scene loading
│   ├── GameStates/   # Runtime state machine area
│   │   ├── Factory/              # Zenject-backed state resolution
│   │   ├── StateInfrastructure/  # State interfaces and base state helpers
│   │   ├── StateMachine/         # State machine implementations and APIs
│   │   └── States/               # Concrete lifecycle states
│   └── SceneManagement/          # Scene loading and scene initialization
├── GameplayData/     # ScriptableObject repositories and base Definitions
├── Audio/            # Audio subsystem: Domain/, Data/, View/
├── Localization/     # EN/RU via XML
├── MainMenu/         # Main menu scene UI entry scripts
└── Utils/            # ContentUi helper, Pause service, Editor tools
```

### Core patterns

**State Machine** controls game flow. State machine implementations live in `Infrastructure/GameStates/StateMachine/`, state interfaces and base helpers live in `Infrastructure/GameStates/StateInfrastructure/`, state resolution lives in `Infrastructure/GameStates/Factory/`, and concrete lifecycle states live in `Infrastructure/GameStates/States/`. Current flow is `BootstrapState -> LoadMainMenuState -> MainMenuState -> LoadGameplayState -> GameplayEnterState -> GameplayLoopState`. `GameplayLoopState` transitions to `GameOverOrParagonState` when the atom core dies, and `GameOverOrParagonState` opens `WindowId.GameOverWindow`. `GameplayPauseState` is registered for later transitions. Do not enter `GameplayPauseState` for the gear menu until `GameplayLoopState` has explicit suspend/resume semantics; a normal state transition out of `GameplayLoopState` runs cleanup.

Each state implements `IState, IGameState` directly or inherits a base state that does. Bind every lifecycle state in `ProjectInstaller` with self binding, resolve states through `IStateFactory`, transition with `_stateMachine.Enter<SomeState>()`, and keep scene loading inside loading states.

States are lifecycle orchestrators, not gameplay logic containers. A state may
load scenes, create initial runtime objects through factories, start services,
call service `Update`/`Cleanup`/`Cancel` methods, gate those calls with pause or
state-transition conditions, and transition to another state. A state must not
contain gameplay mechanics or algorithms: no input-processing branches, drag/drop
rules, click/spawn progress, target selection, timers/cooldowns, physics queries,
damage/resource/score mutation, or UI choice application logic. Put that logic
inside the owning service or object component according to the object-boundary
rules below, then call that owner from the state. Keep this rule even when the
code looks small enough for a private state helper method.

`GameplayEnterState` owns the current gameplay setup: it reads `ILevelStartPointProvider.StartPoint`, calls `IAtomCoreFactory.Create(...)`, creates the battle molecule, then enters `GameplayLoopState`. `AtomCoreFactory`, `FreeAtomFactory`, and `BattleMoleculeFactory` follow the reference-backed prefab flow: `Resources` paths under `Gameplay/Units/...` -> `IAssetProvider` -> Zenject `IInstantiator`, mirroring `HeroFactory.AddViewPath("Gameplay/Hero/hero")` and `EntityViewFactory.CreateViewForEntity(...)` in `ecs-survivors`. Because gameplay factories are project-bound services, instantiated runtime gameplay objects must enter the active gameplay scene hierarchy: top-level runtime objects go under the scene-owned `GameplayRuntime` hierarchy, while owned child objects such as generated atoms are parented under their current owner. Repeat-spawned runtime objects such as enemies and free atoms may be pooled by their owning factories during the active gameplay session; use `PooledFactory<T>` from `Gameplay/Common/Pooling` for shared active/pool ownership, strict return-to-pool validation, and cleanup. Factory `Cleanup()` must still destroy both active and pooled instances before the runtime hierarchy is cleaned. They must not remain in `DontDestroyOnLoad` / `ProjectContext` and must not clutter the gameplay scene root. `GameplayLoopState` inherits `EndOfFrameExitState`; it starts/ticks/cleans active gameplay services such as `IEnemySpawner`, `IAtomCoreService`, and `IBattleMoleculeService`, skips gameplay ticks while `PauseService.IsPaused`, and its `ExitOnEndOfFrame()` calls cleanup for state-owned runtime objects. `IAtomCoreService` owns input hit detection, free atom creation through `FreeAtomFactory`, and lifecycle calls into the current `AtomCore`; `AtomCore` owns click progress, core-owned atom ownership, and core atom orbiting through local components. `IBattleMoleculeService` ticks created battle molecules and subscribes to their shot-request events during `Start()`; each `BattleMolecule` owns its accepted atoms, charge, fire request, and molecule atom orbiting through local components. Enemy spawning, atom core behavior, and battle molecule setup read tunable numeric values from config assets assigned in `GlobalConfigInstaller`. Do not use serialized gameplay prefab fields on `ProjectInstaller` for runtime gameplay prefabs unless explicitly approved as a new proposal. `GameplaySceneInitializer` writes the `MainCamera` and `GameplayStartPoint` scene references into `CameraProvider` and `LevelStartPointProvider`.

Scene initializers that implement Zenject interfaces must be listed in `SceneInitializationInstaller` on the scene `SceneContext`, matching the `ecs-survivors` pattern.

**Window Flow** follows the reference-backed dynamic window path:
`WindowService.Open(WindowId)` -> `WindowFactory.CreateWindow(...)` ->
`WindowsConfig` prefab lookup at `Resources/Configs/Windows/windowConfig` ->
Zenject `IInstantiator` under the scene `UIRoot`. `UIInitializer` writes the
scene `Canvas`/`UIRoot` into `WindowFactory` through `SceneInitializationInstaller`.
`SettingsView` is now a `BaseWindow` opened by `MainMenuHud` through
`WindowService.Open(WindowId.SettingsWindow)`. In `Gameplay`, the scene-owned
HUD `GearButton` sets `PauseService.SetPaused(true)` and opens the dynamic
`WindowId.GameplayMenuWindow`; `GameplayMenuWindow` backs that gameplay menu modal and
unpauses before close/restart/main-menu actions. This is not a result/game-over
window, and `GameOverOrParagonState` must not open it unless explicitly
redesigned. `MainMenuHud` opens `WindowId.TalentTreeWindow` from the main-menu
`Update` button while the result screen is deferred, and exposes `Reset` for
clearing saved PlayerPrefs-backed data through `TalentService.ResetProgress()`.
Do not inject concrete windows such as `SettingsView` directly into menu UI.

**Gameplay Menu Pause** is not a `GameplayPauseState` transition yet.
`GearButton -> PauseService.SetPaused(true) -> WindowService.Open(WindowId.GameplayMenuWindow)`.
`Close`, `Restart`, and `MainMenu` actions in `GameplayMenuWindow` must unpause first; loading states
also reset pause as a safety measure. `GameplayLoopState` skips active gameplay
ticks while `PauseService.IsPaused`, but keeps state-owned runtime objects alive.
No gameplay-changing path may bypass this pause gate. Input polling, timers,
cooldowns, spawning, resource changes, score/progress changes, and other runtime
gameplay mutations must run from services ticked by `GameplayLoopState`, not from
independent `MonoBehaviour.Update()` methods.

**Game Over Flow** is a normal state transition, not a gameplay menu action.
`AtomCoreService` subscribes to the current core death event during `Start()` and
raises a service-level notification. `GameplayLoopState` owns the transition from
that notification to `GameOverOrParagonState`; exiting `GameplayLoopState` runs
normal active gameplay cleanup. `GameOverOrParagonState` opens the dynamic
`WindowId.GameOverWindow`, and `GameOverWindow` may restart gameplay or return to
main menu through `GameStateMachine.Enter(...)`.

**Gameplay Input and Interaction** belongs to the active gameplay loop when it
can change runtime state. Input polling, hit detection, cross-object targeting,
spawn requests, and drag/drop coordination should be modeled as state-owned
services with explicit `Start`, `Update`, `Cancel`/`Stop`, and `Cleanup` methods
as needed. Object-local interaction state such as click progress, charge,
owned-object lists, and local visual feedback belongs on the runtime object or
its focused components. Keep simple interaction coordination inside the existing
owning service instead of adding wrapper services. Current drag/drop ownership is
`DragService`: it reads input and camera data, starts/moves/ends drag, handles raw
release for active drags, and is ticked from `GameplayLoopState`. Do not put drag
mechanics into `GameplayLoopState`, and do not add a separate drag interaction
service unless `DragService` has become genuinely too broad and the user approves
the split.
`MonoBehaviour` runtime components may expose passive methods such as
`SetProgress`, `OnDragMove`, or collision/view callbacks, but they should not
poll input or spawn/clean gameplay objects on their own. UI-filtered input is
appropriate for starting clicks/drags; release/cancel for an already-started
drag must use raw input so the interaction cannot get stuck when the pointer is
over UI. Any `EventSystem.current` access must be null-checked.

**Zenject DI** wires dependencies. No `new SomeService()` for DI-owned services - bind them in installers and inject them. `IInitializable` is allowed for local setup, UI presenters, settings, cached references, and other non-flow initialization. Do not use `IInitializable` to enter gameplay states, load gameplay scenes, or start active gameplay loops.

**Config Assets vs Prefab Registries** are intentionally different. Value/config
assets with gameplay or settings numbers should be assigned in installers,
bound with `FromInstance(...)`, and injected; `EnemySpawnerConfig` and
`TalentConfig` are current examples. Dynamic prefab registries that map ids to prefabs may stay
resource-backed, matching `ecs-survivors` `WindowsConfig`/`StaticDataService`
lookup. `WindowsConfig` is a prefab registry, not a numeric settings config.
When adding a new value config, create the asset under `Assets/_Project/Data/Config`,
add a serialized auto-property to `GlobalConfigInstaller`, bind it with
`FromInstance(...)`, and inject the config into the owning service.

Three installer types:
- `MonoInstaller` - scene-bound, serialized fields for Unity references
- `ScriptableObjectInstaller` - asset-based config (e.g. `GlobalConfigInstaller`)
- Installers in `ProjectContext` apply project-wide; scene `GameObjectContext`/`SceneContext` are local

**MVP** used for UI: `Model` (data + PlayerPrefs), `Presenter` (`IInitializable`, UI/local logic), `View` (MonoBehaviour, UI only). See `Audio/` for the canonical example. Presenters must not become lifecycle intermediaries for gameplay flow.

**Gameplay choice UI** uses MVP direct service calls for now. Presenters/windows may read display data from services/static data and may call high-level feature/domain methods such as `BuyItem(id)`, `SelectUpgrade(id)`, or `ApplyChoice(id)` directly. Keep the operation encapsulated in one owning service; do not spread one UI click across low-level calls such as `RemoveGold`, `AddPurchasedItem`, `ApplyBoost`, and `SaveProgress` from the presenter. The `ecs-survivors` request pattern (`UpgradeRequest`/`BuyRequest` entities processed later by systems) is documented as a deferred option, not the current default.

**Talent Tree** currently uses `TalentConfig` for the list of talents, graph
positions, costs, prerequisites, and numeric effects. Talent tree UI is
prefab-driven: `TalentTreeWindow` receives `TalentNodeView` and
`TalentConnectionView` prefabs through serialized fields and must not build UI
views manually in code. The talent graph is zoomable with mouse wheel; keep
long descriptions in the serialized tooltip UI instead of always-visible node
text. `TalentService` is the single high-level service for reading
gold/progress and buying talents; `TalentTreeWindow` may call
`TalentService.Buy(id)` directly. Talent progress is currently saved through
`PlayerPrefs` as an MVP persistence path until the real meta-progress save model
exists. `TalentConfig.ClearSavedProgressOnStartup` is a temporary testing flag
for wiping talent `PlayerPrefs` on service initialization. The temporary test
entry point is the main-menu `Update` button; the intended future entry point is
the victory/defeat result flow.

**Deferred request transport** may be reconsidered later if direct service calls cause real problems: multiple UI entry points apply the same effect differently, strict ordering inside the active gameplay/meta loop matters, close/pause/state transitions conflict with direct calls, or choices need to be logged/tested separately from their effects. If this returns, do not add an abstract event bus by default; model it as a named request transport owned by the feature, then process it from the owning gameplay/meta loop.

**Repositories** - inherit `Repository<T> : ScriptableObject` where `T : Definition` for any game data. Bind with `FromInstance()` in an installer.

**UniTask** for all async and time-based operations. Coroutines are **never** used, including Unity yield instructions such as `WaitForSeconds`, `UnityEngine.WaitUntil`, and similar. Use `UniTask.Delay`, `UniTask.WaitUntil`, `async UniTaskVoid` instead. This applies to all code: MonoBehaviour components, services, states.

**Component-based approach** - runtime Unity objects are built from small `MonoBehaviour` components. One responsibility equals one component. Dependencies between gameplay components should use `[SerializeField]` or `GetComponent`, not Zenject. State-owned lifecycle helpers such as factories, spawners, input handlers, and cross-object coordinators can be plain DI services that are started, updated, cancelled/stopped, and cleaned by states. Do not inject gameplay services, factories, config, or input into a runtime `MonoBehaviour` just so that component can own active gameplay flow; inject those dependencies into the owning state/service instead and keep the component passive.

Gameplay logic ownership is split by object boundary. If behavior describes how one runtime object works internally, implement it as focused components on that object. A root component may keep the object's existing root responsibility, gather local component references, pass config into local components, and expose small object-level APIs, but it must not become the place where every behavior of that object accumulates. This includes movement, local stats, local progress, local owned-object containers, local charge, local firing behavior, local visual feedback, and other behavior that belongs to one object. If behavior coordinates several runtime objects, external input, factories, scene lifecycle, registration, spawning, cleanup, target selection across objects, or state transitions, implement it in a service or state. Feature services should not micromanage reusable components when the root object can own that behavior; services should contain logic that cannot naturally live inside one runtime object.

For feature folders that contain a runtime object root, keep object-local focused components in a `Components/` subfolder beside the root, factory, service, config, and interface files. The root component may stay at the feature folder level. Use a `.Components` namespace segment for scripts inside that folder. Preserve script `.meta` files when moving components so prefab references keep their GUIDs.

Before adding behavior to any existing runtime `MonoBehaviour`, run a responsibility gate: identify whether the change only extends the component's current responsibility or adds a new responsibility. If it adds a new responsibility, do not add it to the existing component by default; create a focused sibling or child component and let the existing component coordinate it only through a small object-level API when coordination is truly needed. This gate applies to all runtime objects and all prototype work. Temporary `Debug.Log` placeholders, visual feedback, input adapters, targeting, aiming, cooldowns, timers, state indicators, and future-facing scaffolding are not exempt from component boundaries.

Before creating a new runtime component or adding new object-local logic, search the existing project for similar components, interfaces, or duplicated behavior. Reuse or extend an existing focused component when its responsibility already matches the new need. If similar logic exists inside another object-specific component, consider extracting a reusable component instead of copying the logic or creating a parallel one-off implementation. Keep the extracted component focused on the shared responsibility; do not introduce a global service just because two objects use the same local mechanic.

When a change touches more than one responsibility category, split it before editing: state/progress, input handling, visual feedback, physics/collision callbacks, ownership/containers, movement/orbiting, targeting/aiming, attack/effect execution, cooldown/timing, and logging/prototype event output. If you are about to add fields and methods for multiple categories to one component, stop and split first. Do not avoid creating new Unity scripts/components just to avoid `.meta` files; correct component boundaries are more important than minimizing asset count. When adding a new script, create or preserve its `.meta` file according to the Unity asset rules.

Reusable gameplay components do not define service ownership. A shared component such as movement, orbiting, health, targeting, or visual feedback may be reused by multiple features, but do not create a global lifecycle service merely because several runtime objects use that component. The owning runtime object remains responsible for its own internal behavior, while feature services coordinate behavior that spans object boundaries. If two objects need the same mechanical behavior, extract or reuse a component and let each owning object drive it according to that object's rules.

Reusable ownership components are valid when multiple runtime owners keep the same kind of child object. For atoms, use the `OwnedAtoms` component on the owning runtime object (for example core or battle molecule) instead of duplicating atom lists in each owner or storing domain ownership in a factory. `OwnedAtoms` tracks the owner's current atoms, parents owned atoms under the current owner in the Unity hierarchy, removes them on owner change or destruction, can request release for owned atoms, and can apply owner-local component ticks such as atom orbit movement. Factories may track created instances for lifecycle cleanup and pooling, but they must not be the source of truth for gameplay ownership. Services may trigger ownership transfers when the transfer comes from cross-object interactions such as drag/drop, spawning, cleanup, or scene lifecycle, but the owner component stores the ownership state.

Runtime ownership and pool collections must not contain `null` entries as a tolerated state. Do not write read-side loops that skip destroyed Unity objects or `null` collection items, such as repeatedly popping a pool until a non-null object is found. Instead, prevent those paths: only the owning factory/service/component may add, remove, release, pool, or destroy objects in its collection; external code must use that owner API. If an untracked object is released, a pooled object was destroyed externally, or a collection ownership invariant is broken, fail loudly or fix the ownership path rather than filtering the bad entry at use time.

### Adding new things

**New gameplay feature:**
1. Create a `Gameplay/FeatureName/` folder.
2. Put runtime object behavior into focused `MonoBehaviour` components.
3. If the feature polls input, coordinates several runtime objects, advances feature/global timers, spawns or cleans objects, or changes resources/progress outside one object's local state, put that logic in a plain DI service ticked by the owning state. If the progress, timer, cooldown, movement, ownership, or visual feedback belongs to one runtime object, keep it on that object or its components.
4. Put factories/spawners/lifecycle helpers in the same feature folder as plain DI services when a state owns their lifecycle.
5. If the feature needs ScriptableObject data, create a `GameplayData/Definitions/FeatureName/` folder.

**New service:**
1. Define the interface beside the feature/service implementation, or in the relevant `Domain/` folder for subsystems that already use one.
2. Implement the class.
3. Bind in an installer with `Container.Bind<IMyService>().To<MyService>().AsSingle()` or `Container.BindInterfacesAndSelfTo<MyService>().AsSingle()` when self binding is also needed.
4. If the service has active lifecycle, call its explicit lifecycle methods from a state.
5. Inject via `[Inject]`.

**New value config:**
1. Create a `ScriptableObject` config with serialized auto-properties.
2. Place the asset in `Assets/_Project/Data/Config`.
3. Add it to `GlobalConfigInstaller` as a serialized auto-property.
4. Bind it with `FromInstance(...)` through the shared config binding helper.
5. Inject it into the service that owns the behavior.

**New prefab registry:**
1. Use this only for dynamic prefab lookup by id/path, not for gameplay tuning numbers.
2. Keep it resource-backed when it mirrors `WindowsConfig`/`StaticDataService` from `ecs-survivors`.
3. Document the lookup path and keep every enum id mapped to a prefab.

**New game state:**
1. `public class MyState : IState, IGameState`
2. Implement `Enter()` and `Exit()`
3. Implement update only when the state owns an active loop
4. Add `Container.BindInterfacesAndSelfTo<MyState>().AsSingle()` in `ProjectInstaller`
5. Transition: `_stateMachine.Enter<MyState>()`

**New gameplay data:**
1. `public class MyDef : Definition { }`
2. `public class MyRepo : Repository<MyDef> { }` with `[CreateAssetMenu]`
3. Bind in `RepositoryInstaller`

## Coding conventions

| Element | Convention |
|---|---|
| Private fields | `_camelCase` |
| Constants | `UPPER_SNAKE_CASE` |
| Interfaces | `IPascalCase` |
| Properties | `PascalCase`, `private set` |
| Namespaces | Mirror directory path |

- Use field injection for Zenject dependencies. Prefer `[Inject] private SomeService _service;` over constructor injection.
- Use serialized auto-properties for inspector-exposed fields: `[field: SerializeField] private GameObject Obj { get; set; }`. Do not add new `[SerializeField] private GameObject _obj;` fields.
- When converting existing serialized fields to serialized auto-properties, update scene/prefab YAML references to the backing field name, for example `<Obj>k__BackingField`.
- Prefer `List<T>` over arrays (`T[]`) where possible, including `[field: SerializeField]` collections
- For new files, prefer usings grouped as System -> UnityEngine -> third-party -> project. In existing files, keep the surrounding order unless the file is already being cleaned up.

## Validation

- When possible, validate Unity changes by opening the project in Unity `6000.4.4f1`.
- For code-only changes, at minimum check affected C# files for compile-time issues and keep scene/prefab references in sync.
- If adding or moving Unity assets, ensure corresponding `.meta` files are present and Unity-valid. Script `.cs.meta` files should contain a `MonoImporter` block, folder `.meta` files should contain `folderAsset: yes` and `DefaultImporter`, and new/moved prefabs or assets must keep stable GUID references. Prefer letting Unity generate or refresh these files when possible.
- Current manual lifecycle/UI validation should include `MainMenu -> Settings -> Apply/Cancel`, `MainMenu -> Update -> TalentTreeWindow`, `MainMenu -> Reset`, `MainMenu -> Gameplay`, `GearButton -> GameplayMenuWindow -> Close`, `GearButton -> GameplayMenuWindow -> Restart`, and `GearButton -> GameplayMenuWindow -> MainMenu`.
- Current gameplay validation should include `Bootstrap -> LoadMainMenuState -> MainMenuState -> LoadGameplayState -> GameplayEnterState -> GameplayLoopState`, `AtomCore` creation at `GameplayStartPoint`, enemy spawning after first gameplay click, and cleanup when restarting or returning to main menu.

## Git Notes

- Git may report `dubious ownership` in sandboxed environments. Use a per-command safe directory override when inspecting status:
  `git -c safe.directory=F:/unity_personal/Jams/gamedevtv-jam-2026-atomic_connection status --short --branch`
- Do not create commits, branches, stage files, or rewrite history unless the user asks for that explicitly.

## Key dependencies

- **Zenject** - DI container (in `Plugins/`)
- **UniTask** - async/await (Cysharp)
- **DOTween** - tweening (loading curtain fade)
- **Cinemachine 2.10.7**, **Input System 1.19.0**, **URP 17.4.0**
