# Спавн уровней

`Spawn.md` - детальная таблица волн `LevelDefinition`. Если меняются `LevelCatalogConfig` или `LevelXX.asset`, этот документ обновляется в том же проходе.

| Правило | Значение |
|---|---|
| Источник данных | `Assets/_Project/Data/Config/Gameplay/Levels/LevelXX.asset` |
| Отсчет времени | `0` = момент после стартовой задержки спавна врагов |
| `SpawnLimit` | количество срабатываний волны |
| `SpawnCount` | количество врагов за одно срабатывание |
| Boss | есть на каждом уровне и завершает уровень, валюту не дает |

## Сводка

`Level01-Level02` остаются настроенным входом. С `Level03` кампания следует целевой 10-уровневой кривой.

| Level | Standard | Mass | Elite | Ranged | Boss |
|---:|---:|---:|---:|---:|---:|
| 1 | уже настроен | 0 | 0 | 0 | 1 |
| 2 | уже настроен | 0 | 0 | 0 | 1 |
| 3 | 9 | 0 | 1 | 0 | 1 |
| 4 | 10 | 0 | 3 | 0 | 1 |
| 5 | 14 | 8 | 1 | 0 | 1 |
| 6 | 12 | 28 | 2 | 0 | 1 |
| 7 | 14 | 35 | 2 | 0 | 1 |
| 8 | 12 | 14 | 1 | 5 | 1 |
| 9 | 18 | 28 | 4 | 10 | 1 |
| 10 | 22 | 63 | 8 | 18 | 1 |

## Level 1

Оставлен как настроенный входной уровень.

## Level 2

Оставлен как настроенный уровень связей и усиленных Standard.

## Level 3

Роль: первое знакомство с Elite.

| Секунда | Враг | SpawnCount | SpawnLimit | Итого |
|---:|---|---:|---:|---:|
| 0 | Standard HP2 DMG2 | 1 | 6 | 6 |
| 24 | Standard HP2 DMG1 | 3 | 1 | 3 |
| 18 | Elite HP8 DMG3 | 1 | 1 | 1 |
| 35 | Boss HP15 | 1 | 1 | 1 |

## Level 4

Роль: закрепление Elite.

| Секунда | Враг | SpawnCount | SpawnLimit | Итого |
|---:|---|---:|---:|---:|
| 0 | Standard HP2 DMG2 | 1 | 8 | 8 |
| 30 | Standard HP4 DMG3 | 2 | 1 | 2 |
| 12 | Elite HP15 DMG3 | 1 | 3 | 3 |
| 40 | Boss HP60 | 1 | 1 | 1 |

## Level 5

Роль: preview Mass. Mass приходит одной пачкой из 8.

| Секунда | Враг | SpawnCount | SpawnLimit | Итого |
|---:|---|---:|---:|---:|
| 0 | Standard HP2 DMG2 | 1 | 10 | 10 |
| 32 | Standard HP4 DMG3 | 4 | 1 | 4 |
| 20 | Mass HP1 DMG1 | 8 | 1 | 8 |
| 24 | Elite HP15 DMG3 | 1 | 1 | 1 |
| 40 | Boss HP64 | 1 | 1 | 1 |

## Level 6

Роль: Mass становится основной проблемой.

| Секунда | Враг | SpawnCount | SpawnLimit | Итого |
|---:|---|---:|---:|---:|
| 0 | Standard HP3 DMG2 | 1 | 8 | 8 |
| 34 | Standard HP5 DMG3 | 4 | 1 | 4 |
| 10 | Mass HP2 DMG1 | 7 | 4 | 28 |
| 14 | Elite HP18 DMG3 | 1 | 2 | 2 |
| 40 | Boss HP80 | 1 | 1 | 1 |

## Level 7

Роль: Swarm payoff на плотных Mass-пачках.

| Секунда | Враг | SpawnCount | SpawnLimit | Итого |
|---:|---|---:|---:|---:|
| 0 | Standard HP3 DMG2 | 1 | 10 | 10 |
| 28 | Standard HP5 DMG3 | 4 | 1 | 4 |
| 8 | Mass HP2 DMG1 | 7 | 5 | 35 |
| 12 | Elite HP21 DMG3 | 1 | 2 | 2 |
| 40 | Boss HP88 | 1 | 1 | 1 |

## Level 8

Роль: первое знакомство с Ranged. Mass идет двумя пачками по 7.

| Секунда | Враг | SpawnCount | SpawnLimit | Итого |
|---:|---|---:|---:|---:|
| 0 | Standard HP4 DMG3 | 1 | 8 | 8 |
| 36 | Standard HP6 DMG4 | 4 | 1 | 4 |
| 14 | Mass HP2 DMG2 | 7 | 2 | 14 |
| 26 | Elite HP24 DMG4 | 1 | 1 | 1 |
| 18 | Ranged HP4 | 1 | 5 | 5 |
| 45 | Boss HP96 | 1 | 1 | 1 |

## Level 9

Роль: mixed exam.

| Секунда | Враг | SpawnCount | SpawnLimit | Итого |
|---:|---|---:|---:|---:|
| 0 | Standard HP5 DMG3 | 1 | 12 | 12 |
| 36 | Standard HP7 DMG4 | 6 | 1 | 6 |
| 10 | Mass HP3 DMG2 | 7 | 4 | 28 |
| 8 | Elite HP30 DMG4 | 1 | 4 | 4 |
| 14 | Ranged HP5 | 2 | 5 | 10 |
| 50 | Boss HP120 | 1 | 1 | 1 |

## Level 10

Роль: финальный экзамен по Mass + Elite + Ranged + приоритетам.

| Секунда | Враг | SpawnCount | SpawnLimit | Итого |
|---:|---|---:|---:|---:|
| 0 | Standard HP6 DMG4 | 1 | 14 | 14 |
| 44 | Standard HP8 DMG5 | 8 | 1 | 8 |
| 8 | Mass HP3 DMG2 | 7 | 9 | 63 |
| 6 | Elite HP42 DMG5 | 1 | 8 | 8 |
| 12 | Ranged HP6 | 3 | 6 | 18 |
| 60 | Boss HP168 | 1 | 1 | 1 |
