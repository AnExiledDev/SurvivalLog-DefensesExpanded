# Defenses Expanded settings

Settings live in `BepInEx\config\com.dev.survivallog.defensesexpanded.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, where they take effect at once (on the defenses already standing too), or edit the file while the game is closed. Delete the file to get the defaults back. Without Exiled's Toolkit installed, changes made on the Mods screen take effect at the next game start instead.

Each defense has its own section: `[ConcreteBarrier]`, `[BarbedWire]`, `[SpikePlate]`, `[NailGunTurret]` and `[TeslaCoil]`. A section holds only the settings that defense uses.

## Every defense

| Setting | Barrier | Wire | Spikes | Turret | Tesla | What it does |
| --- | --- | --- | --- | --- | --- | --- |
| `Health` | `1500` | `250` | `300` | `250` | `250` | How much damage it takes before it breaks. The game's Sandbags have 600 and its other defenses 200. A change keeps each placed defense at the same share of its health. Any value from `1` to `20000`. |
| `Materials` | `20004:3, 20362:2` | `20004:2, 20104:3` | `20004:2, 20361:2` | `20210:2, 20362:2, 20361:1` | `20210:3, 20104:2, 20004:1` | What crafting its package takes, written as item id:count pairs split by commas. 20004 is Scrap Metal, 20104 Wire, 20210 Electronic Components, 20361 Iron Rod and 20362 Iron Parts. Up to 7 different items and up to 99 of each. A list the game cannot use (one it cannot read, an item that does not exist, or the same materials as another recipe, the other defenses' included) falls back to the default, with a line in `BepInEx\LogOutput.log` saying why. |
| `RepairPerPart` | `300` | `50` | `60` | `40` | `40` | How much health one Repair Parts puts back. The game's Sandbags get 200 and its Spike Barrier 50. Any value from `1` to `5000`. |
| `RepairCostRatio` | `0.5` | `0.65` | `0.65` | `0.8` | `0.65` | How many Repair Parts fixing it from broken takes, as a share of what crafting it takes: `0.5` is half. The game uses 0.5 for its Sandbags and 0.8 for its Mechanical Chainsaw. Any value from `0` to `2`. |
| `SiegePoints` | `2` | `3` | `3` | `6` | `6` | How much each one adds to the size of the next siege. The game counts its Sandbags as 1 and its Mechanical Chainsaw as 6. Any value from `0` to `50`. |

## Defenses that strike (Wire, Spikes, Turret, Tesla)

| Setting | Wire | Spikes | Turret | Tesla | What it does |
| --- | --- | --- | --- | --- | --- |
| `Damage` | `1` | `4` | `10` | `6` | Damage to each zombie it strikes. The game's Electric Trap does 2 and its Mechanical Chainsaw 12. Any value from `0` to `500`. |
| `AttackInterval` | `2` | `1.5` | `1.2` | `4` | Seconds between strikes. Any value from `0.1` to `60`. |
| `AttackRange` | `1.2` | `0.8` | `3.5` | `2.5` | How far away a zombie can be and still be struck, in meters. Any value from `0.1` to `10`. |
| `MaxTargets` | `6` | `6` | `1` | `8` | How many zombies it strikes at once. Any value from `1` to `50`. |

## Defenses that slow (Wire, Tesla)

| Setting | Wire | Tesla | What it does |
| --- | --- | --- | --- |
| `SlowPercent` | `60` | `20` | How much slower a zombie it strikes walks, in percent. `0` does not slow them. The game's Electric Trap slows by 50. Any value from `0` to `90`. |
| `SlowSeconds` | `6` | `2` | How long the slow lasts, in seconds. Any value from `0` to `60`. |

## Defenses that need power (Turret, Tesla)

| Setting | Turret | Tesla | What it does |
| --- | --- | --- | --- |
| `PowerCost` | `4` | `6` | Power it draws. Without power it does not strike. `0` lets it work with no power at all. The game's Electric Trap draws 3 and its Mechanical Chainsaw 5. Any value from `0` to `50`. |
| `PowerPerHit` | `1` | `2` | Extra power each strike uses up. Any value from `0` to `50`. |
| `ShutsDownBelow` | `0.5` | `0` | Below this share of its health it stops working until it is repaired: `0.5` is half. `0` keeps it working until it breaks. The game's Mechanical Chainsaw uses 0.5. Any value from `0` to `0.95`. |

## Uninstall

| Setting | Default | What it does |
| --- | --- | --- |
| `UninstallPrep` | `false` | Turn this on before you remove the mod, then load your save. Every defense package, in bags, in storage, on the floor and in every place the save holds, becomes the package of the game's defense it is based on. Placed and packed defenses are always saved as those defenses, so they need nothing. Save the game, quit, and remove `DefensesExpanded.dll`. `BepInEx\LogOutput.log` says what was changed. Turning it off again does not turn the packages back. |
