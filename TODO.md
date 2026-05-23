# TODO

## 1 — Вынести префабы эффектов в сервис управления эффектами

Константы с путями к префабам эффектов сейчас зашиты в компонентах.
Нужно вынести в отдельный сервис (например, `EffectService`) для централизованного управления.

- [`EnemyDieEffectorBehaviour.cs`](Assets/_Project/Scripts/Gameplay/Effects/EnemyDieEffectorBehaviour.cs#L9-L10) — `DIE_EFFECT_PREFAB_PATH`
- [`EnemyCoreCollision.cs`](Assets/_Project/Scripts/Gameplay/Enemies/Components/EnemyCoreCollision.cs#L11-L12) — `CONTACT_EFFECT_PREFAB_PATH`
