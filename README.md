# EnemyHitLog

This mod attempts to post a message in the chat each time a player or ally was hit by an enemy with an attack.

Non-enemy damage such as fall damage and blood shrines are not logged.

![showcase1](https://i.imgur.com/qsieJix.png)

![showcase2](https://i.imgur.com/MMjDDNw.png)

# Features

- Logs damage taken by Players and Player Allies/Drones
- Logs damaging debuffs (Bleed, Burn)
- Colorized character classes and elite enemies for less dull messages
- Toggles and Filters are provided in a configuration file to reduce spam in the chat (see Configuration below)

# Configuration

This mod can become quite noisy. To have control over this, a configuration file is provided by this mod. It is located at `/Risk of Rain 2/BepInEx/config/com.Xay.EnemyHitLog.cfg`. 

## Toggles

You can toggle the following:

- Player logging (turned on by default)
- Ally logging, i.e. Engineer Turrets, Beetle Guard or Aurelionite (turned off by default)
- Utilities logging, i.e. Drones & Turrets (turned off by default)

Setting all toggles to `false` lets the mod become inactive.

If you wish to disable Debuff logging by itself without wanting to deactivate damage logging, simply set `Debuffs` to `false`.

## Filters

You can filter out messages in the following ways:

- `DamageToMaxHealthThreshold`: Do not log any damage which has a lower value than the given percentage of the Player's HP. For example, if this variable is 10, only damage as high as at least 10% of the Player's max. HP (not counting barrier and shield) will be logged to the chat.

# Installation

Drop the `EnemyHitLog.dll` into your `/Risk of Rain 2/BepInEx/plugins` folder.

# Known Bugs/Missing Features

- Some attacks may not be tracked by the RoR2 Hook, such as Clay Dunestriders' regeneration attack when it gets low HP

if you experience any bugs or have suggestions, let me know on GitHub or in the Risk of Rain 2 Modding Discord. 

# Changelog
`v0.2.0`
- Added `DamageToMaxHealthThreshold` to configuration (see Configuration section)
- Added `Debuffs` to configuration (see Configuration section)
- Code refactoring and some optimizations

`v0.1.1`
- README.md update

`v0.1.0`
- First Release
