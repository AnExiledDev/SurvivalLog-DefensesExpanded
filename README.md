# Defenses Expanded

Adds five new defenses to build against the siege in Survival Log. Each has its own model and icon, and its recipe sits at the workbench next to the game's defense it is based on.

- **Concrete Barrier**: a heavy wall of cast concrete. It blocks zombies like Sandbags, with 1500 health against their 600, and each Repair Parts puts back 300.
- **Barbed Wire**: zombies can walk through it, but it cuts every one in reach (up to 6) for 1 damage every 2 seconds and slows them by 60% for 6 seconds.
- **Spike Plate**: laid flat on the ground. Zombies walk over it and take 4 damage every 1.5 seconds, up to 6 at once.
- **Nail Gun Turret**: needs power (4). It fires at one zombie up to 3.5 m away for 10 damage every 1.2 seconds, and stops working below half health until it is repaired.
- **Tesla Coil**: needs power (6). Every 4 seconds it shocks up to 8 zombies within 2.5 m for 6 damage and slows them by 20% for 2 seconds.

Zombies go for them the way they go for the game's own defenses, Repair Parts fix them, and they count toward the size of the next siege like the game's own.

## Getting them

- The Concrete Barrier's recipe shows once you have the Electric Trap's. Barbed Wire and the Spike Plate come with the Spike Barrier's, and the Nail Gun Turret and Tesla Coil with the Mechanical Chainsaw's. Each is at the same workbench level as the game's defense it sits next to.
- Concrete Barrier: 3 Scrap Metal and 2 Iron Parts.
- Barbed Wire: 2 Scrap Metal and 3 Wire.
- Spike Plate: 2 Scrap Metal and 2 Iron Rod.
- Nail Gun Turret: 2 Electronic Components, 2 Iron Parts and 1 Iron Rod.
- Tesla Coil: 3 Electronic Components, 2 Wire and 1 Scrap Metal.
- **Test buttons**: with Cheat Menu installed, its Mod tests tab shows each defense's numbers, or shows all five recipes right away.

Every number above (health, materials, repairs, siege size, damage, timing, range, targets, slowing and power) is a setting (see Settings). Changes apply at once, to the defenses already standing too.

This is an early test build.

## Settings

Settings live in `BepInEx\config\com.dev.survivallog.defensesexpanded.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. See [CONFIG.md](CONFIG.md) for every setting. Without Exiled's Toolkit installed, changes made on the Mods screen take effect at the next game start instead.

## Save safety

The game saves each defense as the game's defense it is based on (the Concrete Barrier as Sandbags, Barbed Wire and the Spike Plate as the Spike Barrier, the Nail Gun Turret as the Mechanical Chainsaw, the Tesla Coil as the Electric Trap), and the mod turns it back when the save loads. Without the mod they become those defenses and the save still loads.

## Uninstall

A defense package in a bag or in storage would be lost without the mod, so do this first:

1. Turn on `UninstallPrep` (on the Mods screen, or in the config file while the game is closed).
2. Load your save. Every defense package, wherever it is, becomes the package of the game's defense it is based on. `BepInEx\LogOutput.log` says what was changed.
3. Save the game and quit.

Then delete `DefensesExpanded.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `com.dev.survivallog.defensesexpanded.cfg` from `BepInEx\config`. Do this for each save you want to keep.

## Requirements

- Survival Log 1.1.20151.
- BepInEx for Survival Log.
- Optional: [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.4.0 or newer. With it, settings changed on the Mod Manager's Mods screen take effect without restarting the game. Without it, they take effect at the next game start.

## Install

Copy `DefensesExpanded.dll` into `BepInEx/plugins`.
