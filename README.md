# Defenses Expanded

Adds five new defenses to build against the siege in Survival Log. Each has its own model and icon, and unlocks at a Defense skill level.

- **Concrete Barrier**: a heavy wall of cast concrete. It blocks zombies like Sandbags, with 1500 health against their 600, and each Repair Parts puts back 300.
- **Barbed Wire**: zombies can walk through it, but it cuts every one in reach (up to 6) for 1 damage every 2 seconds and slows them by 40% for 4 seconds.
- **Spike Plate**: laid flat on the ground. Zombies walk over it and take 3 damage every 2 seconds, up to 6 at once.
- **Nail Gun Turret**: needs power (4). It turns to face one zombie up to 4 m away and fires for 8 damage every 1.5 seconds, and stops working below half health until it is repaired.
- **Tesla Coil**: needs power (6). Every 3 seconds it shocks up to 8 zombies within 3 m for 18 damage and slows them by 20% for 2 seconds.

Zombies attack them just like Sandbags and the rest, Repair Parts fix them, and they count toward the size of the next siege.

## Getting them

- The recipes show at the workbench once you have any of the game's defense recipes, and each one unlocks at a Defense skill level: the Concrete Barrier, Barbed Wire and Spike Plate at level 2, the Nail Gun Turret at level 3 and the Tesla Coil at level 5. The game's Electric Trap needs level 3 and its Mechanical Chainsaw level 4.
- Concrete Barrier: 4 Scrap Metal and 1 Iron Rod.
- Barbed Wire: 3 Scrap Metal and 2 Wire.
- Spike Plate: 2 Scrap Metal and 1 Iron Rod.
- Nail Gun Turret: 1 Electronic Components, 1 Iron Rod and 3 Scrap Metal.
- Tesla Coil: 2 Electronic Components, 3 Wire and 2 Scrap Metal.
- **Test buttons**: with Cheat Menu installed, its Mod tests tab shows each defense's numbers, shows all five recipes right away, gives you one of each defense, or starts a large test wave.

Every number above (health, materials, unlock levels, repairs, siege size, damage, timing, range, targets, slowing and power) is a setting (see Settings). Changes apply at once, to the defenses already standing too.

## Settings

Settings live in `BepInEx\config\com.dev.survivallog.defensesexpanded.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. See [CONFIG.md](CONFIG.md) for every setting.

## Save safety

The game saves each defense as the game's defense it is based on (the Concrete Barrier as Sandbags, Barbed Wire and the Spike Plate as the Spike Barrier, the Nail Gun Turret as the Mechanical Chainsaw, the Tesla Coil as the Electric Trap), and the mod turns it back when the save loads. Without the mod they become those defenses and the save still loads.

## Uninstall

A defense package in a bag or in storage would be lost without the mod, so do this first:

1. Turn on `UninstallPrep` (on the Mods screen, or in the config file while the game is closed).
2. Load your save. Every defense package, wherever it is, becomes the package of the game's defense it is based on. `BepInEx\LogOutput.log` says what was changed.
3. Save the game and quit.

Then delete `DefensesExpanded.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `com.dev.survivallog.defensesexpanded.cfg` from `BepInEx\config`. Do this for each save you want to keep.

## Requirements

- Survival Log 1.1.20719.
- BepInEx for Survival Log.
- [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.19.0 or newer. Defenses Expanded does not load without it. It lets settings changed on the Mod Manager's Mods screen take effect without restarting the game.
- Optional: [Mod Manager](https://www.nexusmods.com/survivallog/mods/73). With it, you can change every setting in game on its Mods screen and see what each one does, without opening the config file. Each defense's materials show as item cards with the item's picture and name: pick an item from a list and set how many, with no item ids to look up.

## Install

Download `DefensesExpanded-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-DefensesExpanded/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/139). Extract it into the game folder (the folder with `SurvivalLog.exe`), so `DefensesExpanded.dll` lands in `BepInEx\plugins`. Install BepInEx and Exiled's Toolkit first.

## Known issues

The new defenses can't be dyed. Each one uses the sounds of the game's defense it is based on.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log`.

To reach me directly, message `exiled.dev` on Discord, you can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
