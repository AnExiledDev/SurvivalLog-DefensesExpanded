# Defenses Expanded settings

The settings are in `BepInEx\config\com.dev.survivallog.defensesexpanded.cfg`, which is made the first time you start the game with the mod. You can change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. A change on the Mods screen applies right away, to the defenses you've already placed too. Delete the file to get the defaults back.

Each defense has its own section: `[ConcreteBarrier]`, `[BarbedWire]`, `[SpikePlate]`, `[NailGunTurret]` and `[TeslaCoil]`. A section holds only the settings that defense uses.

## Every defense

| Setting | Barrier | Wire | Spikes | Turret | Tesla | What it does |
| --- | --- | --- | --- | --- | --- | --- |
| `Health` | `1500` | `250` | `300` | `250` | `250` | How much damage it takes before it breaks. The game's Sandbags have 600 and its other defenses 200. A change keeps each placed defense at the same share of its health. Any value from `1` to `20000`. |
| `Materials` | `20004:4, 20361:1` | `20004:3, 20104:2` | `20004:2, 20361:1` | `20210:1, 20361:1, 20004:3` | `20210:2, 20104:3, 20004:2` | What it takes to craft its package, written as item id:count pairs separated by commas. 20004 is Scrap Metal, 20104 Wire, 20210 Electronic Components, 20361 Iron Rod and 20362 Iron Parts. Up to 7 different items and up to 99 of each. A list the game cannot use (one it cannot read, an item that does not exist, or the same materials as another recipe, the other defenses' included) falls back to the default, with a line in `BepInEx\LogOutput.log` saying why. |
| `RepairPerPart` | `300` | `50` | `60` | `40` | `40` | How much health one Repair Parts puts back. The game's Sandbags get 200 and its Spike Barrier 50. Any value from `1` to `5000`. |
| `RepairCostRatio` | `0.5` | `0.65` | `0.65` | `0.8` | `0.65` | How many Repair Parts it takes to fix it from broken, as a share of what it costs to craft. `0.5` is half. The game uses 0.5 for its Sandbags and 0.8 for its Mechanical Chainsaw. Any value from `0` to `2`. |
| `SiegePoints` | `2` | `3` | `3` | `6` | `6` | How much each one adds to the size of the next siege. The game counts its Sandbags as 1 and its Mechanical Chainsaw as 6. Any value from `0` to `50`. |
| `UnlockLevel` | `2` | `2` | `2` | `3` | `5` | The Defense skill level you need before you can craft it. `0` needs no level. The game's Electric Trap needs level 3 and its Mechanical Chainsaw level 4. Any value from `0` to `5`. |

## Defenses that strike (Wire, Spikes, Turret, Tesla)

| Setting | Wire | Spikes | Turret | Tesla | What it does |
| --- | --- | --- | --- | --- | --- |
| `Damage` | `1` | `3` | `8` | `18` | How much damage it does to each zombie it strikes. The game's Electric Trap does 2 and its Mechanical Chainsaw 12. Any value from `0` to `500`. |
| `AttackInterval` | `2` | `2` | `1.5` | `3` | Seconds between strikes. Any value from `0.1` to `60`. |
| `AttackRange` | `1.2` | `0.8` | `4` | `3` | How close a zombie has to be to get struck, in meters. Any value from `0.1` to `10`. |
| `MaxTargets` | `6` | `6` | `1` | `8` | How many zombies it strikes at once. Any value from `1` to `50`. |

## Defenses that slow (Wire, Tesla)

| Setting | Wire | Tesla | What it does |
| --- | --- | --- | --- |
| `SlowPercent` | `40` | `20` | How much it slows each zombie it strikes, in percent. `0` means no slowing. The game's Electric Trap slows by 50. Any value from `0` to `90`. |
| `SlowSeconds` | `4` | `2` | How long the slow lasts, in seconds. Any value from `0` to `60`. |

## Defenses that need power (Turret, Tesla)

| Setting | Turret | Tesla | What it does |
| --- | --- | --- | --- |
| `PowerCost` | `4` | `6` | How much power it draws. Without power it does not strike. `0` means it works with no power at all. The game's Electric Trap draws 3 and its Mechanical Chainsaw 5. Any value from `0` to `50`. |
| `PowerPerHit` | `1` | `2` | How much extra power it uses on each strike. Any value from `0` to `50`. |
| `ShutsDownBelow` | `0.5` | `0` | When its health falls below this share, it stops working until you repair it. `0.5` is half. `0` keeps it working until it breaks. The game's Mechanical Chainsaw uses 0.5. Any value from `0` to `0.95`. |

## Uninstall

| Setting | Default | What it does |
| --- | --- | --- |
| `UninstallPrep` | `false` | Turn this on before you remove the mod, then load your save. Every defense package turns into the package of the game defense it is based on. That covers your bags, storage, the floor and every other place in the save. Placed defenses are always saved as those game defenses, so they need nothing. Then save the game, quit, and delete `DefensesExpanded.dll`. `BepInEx\LogOutput.log` lists what was changed. Turning this off again does not change the packages back. |
