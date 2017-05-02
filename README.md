Advanced Circuits Plugin
================================

### The Revolutionized Wiring

Advanced Circuits is the long awaited revolution of the Terraria wiring mechanism, adding a completely rewritten and extended wiring engine to TerrariaAPI and TShock driven Terraria servers.

Beside keeping and extending Terraria's Vanilla Circuits and components, a new type of circuit is added called "Advanced Circuit" giving players the freedom of building complex systems using binary logic and new types of components.

Several configuration settings give the server's owner control of wiring limits and capabilities. This also includes a mighty configuration mechanism of Statues and their performed actions when powered.

To learn more about how to use Advanced Circuits, have a look on this more or less up-to-date guide [here](https://docs.google.com/document/d/16bM21SvoxrumdX1ZUnVoutRlK8w8QWEtqLnCJ96Piak) or the older guide in the wiki.

Note that this plugin doesn't support all vanilla Terraria wiring features. It doesn't currently work with Terraria's logic gates and some other objects.

### How to Install

Note: This plugin requires [TerrariaAPI-Server](https://github.com/NyxStudios/TerrariaAPI-Server) and [TShock](https://github.com/NyxStudios/TShock) in order to work. You can't use this with a vanilla Terraria server.

Grab the latest release and put the _.dll_ files into your server's _ServerPlugins_ directory. Also put the contents of the _tshock/_ folder into your server's _tshock_ folder. You may change the configuration options to your needs by editing the _tshock/Advanced Circuits/Config.xml_ file.

### Plugin Compatibility

This plugin will cooperate with [Sign Commands](https://github.com/CoderCow/Essentials-SignCommands-1) if installed. Wired signs with a sign command can then be triggered by Advanced Circuits to have the commands executed for the player who triggered the circuit.
Note that this will not work when Infinite Signs is installed.

If [Protector](https://github.com/CoderCow/Protector-Plugin) is installed, then the `/ac switch` command will not work on protected objects.

### Commands

* `/advancedcircuits|ac [info]`
* `/advancedcircuits|ac switch [+p]`
* `/advancedcircuits|ac commands`
* `/advancedcircuits|ac blocks`
* `/advancedcircuits|ac reloadcfg`

To get more information about a command type `/<command> help` ingame.

### Permissions

* **ac.reloadcfg** - to reload the configuration file.
* **ac.trigger.teleporter** - to trigger teleporters. Note that this is a trigger 
  permission, not a usage permission. Players without this permission can still 
  use a teleporter when another player who has this permission triggers it.
* **ac.wire.teleporter**  - to wire teleporters.
* **ac.wire.boulder** - to wire boulders.
* **ac.trigger.blockactivator** - to trigger block activators.
* **ac.wire.sign** - to wire signs.
* **ac.trigger.signcommand** - to trigger sign commands.
* **ac.passivetrigger.sign** - to passively trigger signs.

Aswell as self defined permissions in the configuration file.

### Credits

Icon made by [freepik](http://www.freepik.com/)