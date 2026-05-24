# Пейсинг прогрессии

`Pacing.md` - целевое состояние кампании, экономики и дерева талантов. Конкретные волны живут в [Spawn.md](Spawn.md), численные конфиги - в [AtomicConnection_BALANCE.md](AtomicConnection_BALANCE.md).

## Коротко

- Кампания состоит из 10 уровней.
- `Level01-Level02` остаются входом; `Level03-Level10` задают основную кривую кампании.
- Кампания должна проходиться по target path без обязательного фарма, но полное закрытие дерева допускает 1-3 фарм-забега после `Level10`.
- Elite дает `5 DNA + 1 Radical`.
- Radicals используются как полноценная вторая валюта: 10 radical-талантов, первый spend после `Level4`.
- Поздние DNA-таланты являются фарм-синками, потому что к `Level10` игрок получает тысячи DNA.
- AutoLoad-таланты исключены из активного дерева.

## Уровни

| Level | Роль | Standard | Mass | Elite | Ranged |
|---:|---|---:|---:|---:|---:|
| 1 | Входной | уже настроен | 0 | 0 | 0 |
| 2 | Связи / усиленные Standard | уже настроен | 0 | 0 | 0 |
| 3 | Elite intro | 9 | 0 | 1 | 0 |
| 4 | Elite закрепление | 10 | 0 | 3 | 0 |
| 5 | Mass preview | 14 | 6 | 1 | 0 |
| 6 | Mass problem | 12 | 20 | 2 | 0 |
| 7 | Swarm payoff | 14 | 25 | 2 | 0 |
| 8 | Ranged intro | 12 | 10 | 1 | 5 |
| 9 | Mixed exam | 18 | 20 | 4 | 10 |
| 10 | Final exam | 22 | 45 | 8 | 18 |

Правила:
- Boss есть на каждом уровне, но валюту не дает.
- `Level5` mass - одна пачка из 6.
- `Level8` mass - две пачки по 5.
- `Level10` намеренно плотнее остальных уровней.

## Экономика

Модель прохождения: 3 неудачных попытки + 1 успешная. Доход с врагов считается как:

```text
AttemptMultiplier = 2.65
30% + 55% + 80% + 100% = 2.65x
```

Базовые награды:

| Источник | Награда |
|---|---:|
| Standard | `1 DNA` |
| Mass | `1 DNA` |
| Ranged | `1 DNA` |
| Elite | `5 DNA + 1 Radical` |
| Boss | `0` |
| First-clear любого уровня | `+1 Isotope` |

Формула:

```text
NonBossCount = Standard + Mass + Elite + Ranged

RawDnaDrops =
  Standard + Mass + Ranged + Elite * 5

DnaKillBonus =
  NonBossCount * DnaFlatKillRewardLevel

DnaCollected =
  (RawDnaDrops + DnaKillBonus) * (1 + DnaPickupAmountBonusLevel)

RawRadicalDrops =
  Elite

RadicalChanceDrops =
  NonBossCount * RadicalDropChance

RadicalsCollected =
  (RawRadicalDrops + RadicalChanceDrops) * (1 + RadicalPickupAmountBonusLevel)

ExpectedLevelDna =
  floor(AttemptMultiplier * DnaCollected)

ExpectedLevelRadicals =
  floor(AttemptMultiplier * RadicalsCollected)
```

Экономические таланты применяются со следующего уровня после покупки:

| Talent | Целевой момент |
|---|---|
| `RadicalDropChance 1` | после `Level4` |
| `RadicalDropChance 2` | после `Level5` |
| `RadicalPickupAmountBonus 1` | после `Level6` |
| `RadicalPickupAmountBonus 2` | после `Level7` |
| `DnaPickupAmountBonus 1` | после `Level8` |
| `DnaPickupAmountBonus 2` | после `Level9` |

Ожидаемый first-clear итог при текущих `Level01-Level02` и целевой экономике `Level03-Level10`: примерно `6.3k-6.5k DNA` и `360-390 Radicals`. Полная цена дерева выше first-clear дохода, поэтому остаток закрывается фармом.

## Изотопы

Всего в активном дереве тратится ровно 9 изотопов. Кампания при first-clear всех 10 уровней выдает 10 изотопов; финальный уровень не имеет отдельной валютной логики, а отличается только финальным экраном.

| Talent | Isotopes |
|---|---:|
| `AutoClick` | 1 |
| `NeedleMoleculeAim` | 1 |
| `StingerMolecule` | 1 |
| `SwarmMolecule` | 1 |
| `SwarmMoleculeShotCount 1` | 1 |
| `SwarmMoleculeShotCount 2` | 1 |
| `MembraneMolecule` | 1 |
| `MembraneMoleculeDuration` | 2 |

Не стоят изотопы:
- `StingerMoleculeAim`
- `StingerMoleculePierce`
- `StingerMoleculeChargeReduction`

## DNA-таланты

| Talent | MaxLevel | CostsByLevel |
|---|---:|---|
| `CoreHealthSmall` | 5 | `2 / 8 / 25 / 90 / 240` |
| `CoreClickReduction` | 4 | `8 / 30 / 120 / 420` |
| `NeedleMoleculeDamage` | 5 | `8 / 25 / 80 / 250 / 750` |
| `NeedleMoleculeChargeReduction` | 2 | `4 / 80` |
| `StingerMoleculeDamageSmall` | 3 | `14 / 45 / 160` |
| `StingerMoleculeAim` | 1 | `15` |
| `StingerMoleculeCriticalChance` | 3 | `30 / 110 / 420` |
| `SwarmMoleculeDamageSmall` | 4 | `30 / 100 / 360 / 1100` |
| `CurrencyPickupArea` | 4 | `20 / 80 / 300 / 900` |
| `ConnectionAtomSpeed` | 3 | `20 / 80 / 280` |
| `DnaFlatKillReward` | 5 | `40 / 150 / 500 / 1300 / 3000` |
| `RadicalDropChance` | 3 | `60 / 220 / 700` |
| `MembraneMoleculeChargeReduction` | 1 | `350` |
| `MembraneMoleculeCooldownReduction` | 3 | `250 / 750 / 1800` |

## Radical-таланты

| Talent | MaxLevel | CostsByLevel |
|---|---:|---|
| `StingerMoleculePierce` | 2 | `4 / 10` |
| `StingerMoleculeCriticalReward` | 3 | `5 / 15 / 45` |
| `SwarmMoleculeAttackRange` | 2 | `5 / 16` |
| `CoreHealthLarge` | 2 | `8 / 24` |
| `RadicalPickupAmountBonus` | 2 | `10 / 28` |
| `DnaPickupAmountBonus` | 2 | `12 / 40` |
| `StingerMoleculeChargeReduction` | 2 | `9 / 30` |
| `StingerMoleculeDamageLarge` | 3 | `10 / 26 / 80` |
| `SwarmMoleculeDamageLarge` | 2 | `8 / 26` |
| `MembraneMoleculeIntegrity` | 3 | `8 / 30 / 100` |

## Таймлайн покупок

| После | Целевой bundle |
|---:|---|
| 1 | `CoreHealthSmall 1`, `NeedleMoleculeChargeReduction 1`, `NeedleMoleculeDamage 1`, `AutoClick` |
| 2 | `NeedleMoleculeAim`, `CoreClickReduction 1`, `NeedleMoleculeDamage 2`, `CoreHealthSmall 2` |
| 3 | `StingerMolecule`, `StingerMoleculeDamageSmall 1`, `StingerMoleculeAim`, `DnaFlatKillReward 1` |
| 4 | `StingerMoleculePierce 1`, `StingerMoleculeCriticalChance 1`, `DnaFlatKillReward 2`, `RadicalDropChance 1`, `StingerMoleculeCriticalReward 1` |
| 5 | `SwarmMolecule`, `SwarmMoleculeDamageSmall 1`, `CurrencyPickupArea 1`, `ConnectionAtomSpeed 1`, `RadicalDropChance 2`, `SwarmMoleculeAttackRange 1` |
| 6 | `SwarmMoleculeShotCount 1`, `SwarmMoleculeDamageSmall 2`, `StingerMoleculePierce 2`, `RadicalPickupAmountBonus 1` |
| 7 | `SwarmMoleculeShotCount 2`, `MembraneMolecule`, `MembraneMoleculeChargeReduction`, `RadicalPickupAmountBonus 2`, `CoreHealthLarge 1`, `RadicalDropChance 3` |
| 8 | `CoreHealthLarge 2`, `DnaFlatKillReward 3`, `CurrencyPickupArea 2`, `ConnectionAtomSpeed 2`, `StingerMoleculeCriticalChance 2`, `StingerMoleculeDamageSmall 2-3`, `DnaPickupAmountBonus 1` |
| 9 | `MembraneMoleculeDuration`, `DnaPickupAmountBonus 2`, `DnaFlatKillReward 4`, `StingerMoleculeChargeReduction 1-2`, `MembraneMoleculeIntegrity 1`, `MembraneMoleculeCooldownReduction 1`, `StingerMoleculeDamageLarge 1`, `SwarmMoleculeDamageLarge 1` |
| 10 first-clear | `DnaFlatKillReward 5`, `CurrencyPickupArea 3-4`, `ConnectionAtomSpeed 3`, `CoreClickReduction 2-3`, `NeedleMoleculeDamage 3-4`, `StingerMoleculeCriticalChance 3`, `SwarmMoleculeDamageSmall 3-4`, `StingerMoleculeDamageLarge 2`, `SwarmMoleculeDamageLarge 2` |
| 10 фарм | Остаток дерева: `CoreHealthSmall 3-5`, `CoreClickReduction 4`, `NeedleMoleculeDamage 5`, `NeedleMoleculeChargeReduction 2`, `MembraneMoleculeCooldownReduction 2-3`, `MembraneMoleculeIntegrity 2-3`, `StingerMoleculeDamageLarge 3`, `StingerMoleculeCriticalReward 2-3`, оставшиеся late DNA-грейды |

## Проверка

- Суммарная isotope-стоимость активного дерева равна `9`.
- Radical-талантов в активном дереве ровно `10`.
- Первый radical-spend доступен после `Level4`.
- `StingerMoleculeAim`, `StingerMoleculePierce`, `StingerMoleculeChargeReduction` не стоят изотопы.
- AutoLoad-таланты не участвуют в активном `TalentConfig`.
- Target path доступен без фарма до прохождения `Level10`.
- Full tree требует примерно `1-3` фарм-забега после `Level10`.
