# AtomicConnection Architecture Notes

This document keeps detailed architecture and flow notes. `AGENTS.md` is the
short operational rule file; update both when architecture changes.

## State Machine

State machine implementations live in `Infrastructure/GameStates/StateMachine/`,
state interfaces and base helpers in `StateInfrastructure/`, state resolution in
`Factory/`, and concrete lifecycle states in `States/`.

Current flow:

```text
BootstrapState
-> LoadMainMenuState
-> MainMenuState
-> LoadGameplayState
-> GameplayEnterState
-> GameplayLoopState
```

Terminal gameplay transitions:
- core death: `GameplayLoopState -> GameOverOrParagonState -> GameOverWindow`
- level complete: `GameplayLoopState -> LevelCompleteState -> LevelCompleteWindow`

`GameplayPauseState` is registered for later transitions. Do not enter it for
the gear menu until `GameplayLoopState` has explicit suspend/resume semantics.

States are lifecycle orchestrators. They may load scenes, ask services/factories
to create runtime objects, start/tick/clean services, gate calls with pause or
transition conditions, and transition to another state. They must not contain
gameplay mechanics such as input-processing branches, drag/drop rules, click or
spawn progress, target selection, timers, cooldowns, physics queries, damage,
resources, score mutation, or UI choice application logic.

## Runtime Prefab Flow

Runtime prefab flow:

```text
Resources path -> IAssetProvider -> Zenject IInstantiator
```

Runtime gameplay objects must enter the active gameplay scene hierarchy.
Top-level runtime objects go under scene-owned `GameplayRuntime`; owned children
such as generated atoms are parented under their current owner.

Repeat-spawned runtime objects such as enemies and free atoms may be pooled by
their owning factories. Use `PooledFactory<T>` for shared active/pool ownership,
strict return-to-pool validation, and cleanup. Factory `Cleanup()` must destroy
both active and pooled instances before runtime hierarchy cleanup.

## Enemy Runtime

Enemy prefab paths, stats, level duration, completion reward, and parallel spawn
tracks live in `LevelCatalogConfig`. Enemy pooling is separated by `EnemyId` so
each type keeps its own prefab.

Spawn tracks are independent:
- each starts at `StartTimeSeconds`
- each optionally stops at `EndTimeSeconds`
- each may stop after `SpawnLimit`
- `0` spawn limit means unlimited while active

Runtime ownership:
- `EnemyService` owns wave timing, active enemy tracking, per-frame enemy
  ticking, death subscriptions, non-boss nucleotide pickup spawn requests, and
  boss-kill notification.
- `EnemySpawner` is only the spawn helper: choose offscreen position, ask
  `EnemyFactory` to create the unit, apply spawn-time object setup, and keep
  multi-enemy wave spawns clustered in one offscreen sector.
- `CurrencyPickupService` owns physical currency pickup spawning, cursor-hover
  collection checks, currency grant on collection, and pickup cleanup.
- Enemy object-internal behavior stays on focused components.
- `EnemyMovement` moves enemies directly toward the core; prefab-specific
  movement variants such as `MassEnemyArcMovement` own enemy-local path shapes.
- Enemy-local runtime behaviors implement `IEnemyRuntimeBehavior`; `EnemyUnit`
  configures and ticks them while `EnemyService` owns the active enemy loop.
- Ranged enemies use prefab variants: `RangedEnemyStopMovement` owns stopping
  near the core, `RangedEnemyAttack` owns telegraph, projectile spawn, projectile
  ticking, and projectile cleanup.
- `EnemyCoreCollision` resolves normal overlap damage with the atom core while
  ticked through `EnemyUnit` by `EnemyService`.
- Boss one-shot core collision is a prefab component variant,
  `BossCoreCollision`, not a definition flag.

## Active Gameplay Loop

`GameplayEnterState` creates the core and battle molecule, then enters
`GameplayLoopState`.

`GameplayLoopState` inherits `EndOfFrameExitState`. It starts, ticks,
fixed-ticks, and cleans active gameplay services such as `IEnemyService`, `IAtomCoreService`,
`IBattleMoleculeService`, `ICurrencyPickupService`, and
`ILevelProgressService`. It skips gameplay ticks while `PauseService.IsPaused`;
`ExitOnEndOfFrame()` cleans state-owned runtime services and objects.

## Core, Molecule, And Progress

`AtomCoreService` implements both setup-facing `IAtomCoreCreator` and
active-loop-facing `IAtomCoreService`. It creates/destroys the core, applies core
HP and atom click count, exposes current core transform for active targeting
services, subscribes to core death during `Start()`, and ticks current
`AtomCore`.

`AtomCore` is the root facade. `AtomCoreClickInteraction` owns hit detection,
manual/auto click progress, and generated free atom creation through
`FreeAtomFactory`.

`IBattleMoleculeService` ticks created battle molecules, including transform-based
core orbit movement, and auto-loads core atoms into molecules when unlocked.
Each `BattleMolecule` owns its accepted atoms, charge, fire request, collision
collider, attack resolution, shot feedback, and molecule atom orbiting through
local components. Attack-capable molecule prefabs use focused attack components
such as `StingerMoleculeAttack` and `SwarmMoleculeAttack` to resolve
local shot requests into raycasts, enemy damage, and shot-line feedback.

`LevelProgressService` owns completion reward/unlock, marks the selected level
complete through `ILevelSelectionService`, grants first-clear completion reward
through `CurrencyService`, and raises completion for state transition.

## UI And Windows

Dynamic window flow:

```text
WindowService.Open(WindowId)
-> WindowFactory.CreateWindow(...)
-> WindowsConfig prefab lookup at Resources/Configs/Windows/windowConfig
-> modal root with raycast-blocking backdrop under scene UIRoot
-> Zenject IInstantiator under modal root
```

UI may call `GameStateMachine.Enter(...)` or `IWindowService.Open(...)`, but must
not load scenes or control gameplay service lifecycle.

Every dynamic window gets a shared modal backdrop. The backdrop blocks clicks to
UI behind the current window and forwards backdrop clicks to the opened
`BaseWindow`. Its color is configured in `WindowsConfig`. Windows opt into
dismiss-on-backdrop by overriding `OnBackdropClicked()`; result windows keep the
default no-op and require explicit button actions.

The gameplay menu is not a `GameplayPauseState` transition yet:

```text
GearButton
-> PauseService.SetPaused(true)
-> WindowService.Open(WindowId.GameplayMenuWindow)
```

`GameplayMenuWindow` must unpause before close/restart/main-menu actions.

## Result Flows

Game over is a normal state transition. `AtomCoreService` raises current core
death, `GameplayLoopState` transitions to `GameOverOrParagonState`, active
gameplay cleanup runs, and `GameOverOrParagonState` opens `GameOverWindow`.

Level complete is also a normal state transition. In the current MVP, the level
completes when `EnemyService` raises `BossKilled`. `GameplayLoopState` calls
`LevelProgressService.Complete()`, then transitions to `LevelCompleteState`,
which opens `LevelCompleteWindow`.

## Gameplay Input And Interaction

Gameplay input that can change runtime state belongs to the active gameplay loop.
Input polling, hit detection, cross-object targeting, spawn requests, and
drag/drop coordination should be state-owned services with explicit lifecycle
methods.

Object-local interaction state such as click progress, charge, owned-object
lists, and local visual feedback belongs on the runtime object or its focused
components.

Current drag/drop ownership is `DragService`: it reads input and camera data,
starts/moves/ends drag, handles raw release for active drags, and is ticked from
`GameplayLoopState`.

## Data, Config, And Meta

Value/config assets with gameplay or settings numbers should be assigned in
installers, bound with `FromInstance(...)`, and injected.

Dynamic prefab registries map ids to prefabs. `WindowsConfig` is a prefab
registry, not a numeric settings config.

`AtomCoreConfig` owns core setup values: base core HP, clicks required to
generate a free atom, generated atom spawn radius, and free atom orbit speed.

Talent-adjusted runtime values are applied by the current owner:
- `AtomCoreService` applies core HP and atom click count
- `BattleMoleculeFactory` applies atom charge count
- `AtomCoreClickInteraction` applies AutoClick
- molecule-local attack components resolve shot damage and Pierce
- `BattleMoleculeService` applies AutoLoad

Talent tree uses `TalentConfig`. `TalentService` owns talent progress and
buying; `CurrencyService` owns saved meta-currencies. Talent progress and
currencies currently use `PlayerPrefs` as MVP persistence. `ProgressData`
contains a save/progression version; if the stored version does not match the
current code version, `SaveLoadService` clears saved data and creates fresh
progress instead of migrating.
