# EnemyHitLog

This mod attempts to post a message in the chat each time a player or ally was hit by an enemy with an attack.

Non-enemy damage such as fall damage and blood shrines are not logged.

![showcase1](https://i.imgur.com/qsieJix.png)

![showcase2](https://i.imgur.com/MMjDDNw.png)

# Features

- Logs damage taken by Players and Player Allies/Drones _(Configurable)_
- Logs damaging debuffs (Bleed, Burn)
- Colorized character classes and elite enemies for less dull messages

# Configuration

As this mod can become quite noisy, a configuration file located at `/Risk of Rain 2/BepInEx/config/com.Xay.EnemyHitLog.cfg` is provided to make it possible to toggle mixed logging of: 

- Players
- Allies (Engineer Turret, Beetle Guard, Aurelionite, ...) 
- and Utilities (Drones & Turrets)

Setting all toggles to `false` lets the mod become inactive.

# Installation

Drop the `EnemyHitLog.dll` into your `/Risk of Rain 2/BepInEx/plugins` folder.

# Known Bugs/Missing Features

- Some attacks may not be tracked by the RoR2 Hook, such as Clay Dunestriders' regeneration attack when it gets low HP

if you experience any bugs or have suggestions, let me know on GitHub or in the Risk of Rain 2 Modding Discord. 

# Changelog

`v0.1.0`
- First Release