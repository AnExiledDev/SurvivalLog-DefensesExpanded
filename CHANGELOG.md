# Changelog

## 0.2.3

- Clearer wording in the settings, the Cheat Menu test buttons and the log.
- Made for Survival Log 1.1.20719.

## 0.2.2

- If a game update stops the mod from hooking into the game, the Mod Manager now shows that the mod may need an update.
- Made for Survival Log 1.1.20719.

## 0.2.1

- Nail Gun Turrets are lighter on performance while they're firing.
- Fixed: if the defenses failed to load into the game, they could stay missing until you changed a setting. The mod now tries again on its own.
- The `Materials` description now says you can use up to 99 of each item.
- Made for Survival Log 1.1.20719.

## 0.2.0

- Each defense now unlocks at a Defense skill level: the Concrete Barrier, Barbed Wire and Spike Plate at level 2, the Nail Gun Turret at level 3 and the Tesla Coil at level 5. Each defense has a new `UnlockLevel` setting to change its level.
- The early defenses cost less to make and the late ones cost more, and a defense that unlocks later now does more damage. See Settings for the new numbers.
- The Nail Gun Turret turns its head to face the zombie it is shooting and kicks back on each shot.
- The Nail Gun Turret and Tesla Coil now face the right way.
- Placing a defense no longer freezes the game for a moment. The models now load a few seconds after a run starts instead of the first time you place one.
- Two new Cheat Menu test buttons: one gives you one of each defense, and one starts a large test wave.
- Made for Survival Log 1.1.20719.

## 0.1.4

- With Mod Manager, each defense's `Materials` setting shows as item cards you pick from a list. The defense's package shows beside them with its pieces, weight and value.
- Made for Survival Log 1.1.20719.

## 0.1.3

- Now needs Exiled's Toolkit 0.19.0 or newer, because some of the mod's code moved there.
- Made for Survival Log 1.1.20719.

## 0.1.2

- Setting a Nail Gun Turret's or Tesla Coil's `PowerCost` to `0`, or from `0` back up, now applies right away. The defense joins or leaves the power grid without reloading the save.

## 0.1.1

- Fixed: changing any setting could take health from a damaged defense when a talent had raised its maximum health. Now only a change to `Health` moves a defense's health, and the defense keeps the same share of its health.
- With `UninstallPrep` on, a placed Concrete Barrier is saved with no more health than Sandbags can hold.
- Fixed: a defense could show up as the game defense it is based on if its attack effect failed to play.

## 0.1.0

- First build: the Concrete Barrier, Barbed Wire, Spike Plate, Nail Gun Turret and Tesla Coil, each with its own model and icon.
- Made for Survival Log 1.1.20151.
