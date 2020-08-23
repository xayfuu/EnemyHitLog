# EnemyHitLog

Log damage taken in the chat.

# Features

- Logs damage taken by players and allied entities (supports friendly fire)
- Logs damage-on-tick (DoT) debuffs
- Colorized labels for less dull messages
- Toggles and Filters to reduce spam in the chat

![showcase1](https://i.imgur.com/qsieJix.png)

![showcase2](https://i.imgur.com/MMjDDNw.png)

# Configuration

This mod can become quite noisy. To have control over this, a configuration file is provided by this mod. It is located at `/Risk of Rain 2/BepInEx/config/com.Xay.EnemyHitLog.cfg`, generated after Risk of Rain 2 has been launched once with BepInEx with the mod installed. 

## Toggles

You can toggle the following:

- Player logging `([true]|false)`
- Ally logging, i.e. Engineer Turrets, Beetle Guard or Aurelionite `(true|[false])`
- Utilities logging, i.e. Drones & Turrets `(true|[false])`

Setting the above toggles to `false` lets the mod become inactive.

- Fall damage logging `([true]|false)`
- Shrine of Blood logging `(true|[false])`
- Debuff logging `([true]|false)`

## Filters

You can filter out messages in the following ways:

- `DamageToMaxHealthThreshold`: Do not log any damage which has a lower value than the given percentage of the Player's HP. For example, if this variable is `10`, only damage as high as at least 10% of the Player's max. HP (not counting barrier and shield) will be logged to the chat. Default value is `5`.

# Installation

Drop the `EnemyHitLog.dll` into your `/Risk of Rain 2/BepInEx/plugins` folder.

# Known Bugs/Missing Features

- Some attacks may not be tracked by the RoR2 Hook, such as Clay Dunestriders' regeneration attack when it gets low HP

If you experience any bugs or have suggestions, let me know on GitHub by creating an [Issue](https://github.com/xayfuu/EnemyHitLog/issues) or bugging me in the Risk of Rain 2 Modding Discord. 

# To-Do

- Log timed Debuffs like Slow, ClayGoo, Nullified
- Summarize Splash damage into one damage message

# Changelog

`0.3.0`

- Added fall damage
- Added Shrine of Blood damage
- Added Poison, Blight, and Burn tick debuffs
- Added friendly fire handling
- Added `FallDamage` toggle to Config file (default is `true`)
- Added `ShrinesOfBlood` toggle to Config file (default is `false`)
- Added proper Captain label color
- Updated some Survivor label colors
- Updated mod logo
- Updated some logging text
- Updated/Fixed `DamageToMaxHealthThreshold` such that its filter functionality applies to any damage event
- Fixed typos in Config descriptions and entity labels

`0.2.0`
- Added `DamageToMaxHealthThreshold` to configuration (see Configuration section)
- Added `Debuffs` to configuration (see Configuration section)
- Code refactoring and some optimizations

`0.1.1`
- ReadMe update

`0.1.0`
- First Release
