# Defenses Expanded

Adds five new defenses to build against the siege in Survival Log. Each has its own model and icon, and unlocks at a Defense skill level. The numbers below are the defaults, and you can change every one of them in the settings.

- **Concrete Barrier**: a heavy wall of cast concrete. It blocks zombies like Sandbags do, but with 1500 health instead of 600, and each Repair Parts puts back 300 health.
- **Barbed Wire**: zombies can walk through it, but it cuts up to 6 of them at a time for 1 damage every 2 seconds and slows them by 40% for 4 seconds.
- **Spike Plate**: it lies flat on the ground. Zombies walk over it and take 3 damage every 2 seconds, up to 6 of them at once.
- **Nail Gun Turret**: runs on 4 power. It turns to face one zombie up to 4 m away and fires for 8 damage every 1.5 seconds. Below half health it stops working until you repair it.
- **Tesla Coil**: runs on 6 power. Every 3 seconds it shocks up to 8 zombies within 3 m for 18 damage and slows them by 20% for 2 seconds.

Zombies attack them like any other defense, and you fix them with Repair Parts. Each one you build also makes the next siege a bit bigger.

## Getting them

The recipes show up at the workbench once you have any of the game's defense recipes. Each one unlocks at a Defense skill level: the Concrete Barrier, Barbed Wire and Spike Plate at level 2, the Nail Gun Turret at level 3 and the Tesla Coil at level 5. For comparison, the game's Electric Trap needs level 3 and its Mechanical Chainsaw level 4.

- Concrete Barrier: 4 Scrap Metal and 1 Iron Rod.
- Barbed Wire: 3 Scrap Metal and 2 Wire.
- Spike Plate: 2 Scrap Metal and 1 Iron Rod.
- Nail Gun Turret: 1 Electronic Components, 1 Iron Rod and 3 Scrap Metal.
- Tesla Coil: 2 Electronic Components, 3 Wire and 2 Scrap Metal.
With Cheat Menu installed, its Mod tests tab has test buttons that show each defense's numbers, show all five recipes right away, give you one of each defense and start a large test wave.

## Settings

The settings are in `BepInEx\config\com.dev.survivallog.defensesexpanded.cfg`, which is made the first time you start the game with the mod. You can change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. A change on the Mods screen applies right away, to the defenses you've already placed too. See [CONFIG.md](CONFIG.md) for every setting.

## Save safety

Each defense is saved as the game defense it is based on: the Concrete Barrier as Sandbags, Barbed Wire and the Spike Plate as the Spike Barrier, the Nail Gun Turret as the Mechanical Chainsaw and the Tesla Coil as the Electric Trap. When the save loads, the mod turns them back. If you load the save without the mod, they show up as those game defenses and the save still loads.

## Uninstall

Defense packages in your bags or storage would be lost without the mod, so do this first:

1. Turn on `UninstallPrep` (on the Mods screen, or in the config file while the game is closed).
2. Load your save. Every defense package, wherever it is, turns into the package of the game defense it is based on. `BepInEx\LogOutput.log` lists what was changed.
3. Save the game and quit. Do steps 2 and 3 for every save you want to keep.

Then delete `DefensesExpanded.dll` from the `BepInEx\plugins` folder. To remove the settings too, also delete `com.dev.survivallog.defensesexpanded.cfg` from `BepInEx\config`.

## Requirements

- Survival Log 1.1.20719.
- BepInEx for Survival Log.
- [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.19.0 or newer. Defenses Expanded does not load without it. It lets settings changed on the Mod Manager's Mods screen take effect without restarting the game.
- Optional: [Mod Manager](https://www.nexusmods.com/survivallog/mods/73). With it, you can change every setting in game on its Mods screen and see what each one does, without opening the config file. Each defense's materials show as item cards with the item's picture and name: pick an item from a list and set how many, with no item ids to look up.

## Install

Download `DefensesExpanded-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-DefensesExpanded/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/139). Extract it into the game folder (the folder with `SurvivalLog.exe`), so `DefensesExpanded.dll` lands in `BepInEx\plugins`. Install BepInEx and Exiled's Toolkit first.

## Known issues

The new defenses can't be dyed. Each one uses the sounds of the game defense it is based on.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log`.

To reach me directly, message `exiled.dev` on Discord, you can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
