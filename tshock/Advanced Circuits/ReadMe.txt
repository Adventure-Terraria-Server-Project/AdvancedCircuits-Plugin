=================================================================================
 Advanced Circuits Plugin for Terraria API
   (c) CoderCow 2012-2014
=================================================================================

The Revolutionized Wiring
---------------------------------------------------------------------------------

Advanced Circuits is the long awaited revolution of the Terraria wiring 
mechanism, adding a completely rewritten and extended circuit processing engine 
to TerrariaAPI and TShock driven Terraria servers.

Beside keeping and extending Terraria's vanilla circuits and components, a new 
type of circuit is added called "Advanced Circuit" giving players the freedom of 
building complex systems using binary logic and new types of components.

Several configuration settings give the server's owner control of wiring limits 
and capabilities. This also includes a mighty configuration mechanism of statues 
and their performed actions when powered.

NOTE: Before installing and configuring the plugin, read the "Known Problems" 
section first!

Note: This plugin requires Terraria Server API 1.14 in order to work.

To learn more about Advanced Circuits, have a look on the official wiki:
https://github.com/CoderCow/AdvancedCircuits-Plugin/wiki/

Suggestions? Bugs? File issues here:
https://github.com/CoderCow/AdvancedCircuits-Plugin/issues


Commands
---------------------------------------------------------------------------------
/advancedcircuits|ac [info]
/advancedcircuits|ac switch [+p]
/advancedcircuits|ac commands
/advancedcircuits|ac blocks
/advancedcircuits|ac reloadcfg


Permissions
---------------------------------------------------------------------------------
ac.reloadcfg - to reload the configuration file.
ac.trigger.teleporter - to trigger teleporters. Note that this is a trigger 
  permission, not a usage permission. Players without this permission can still 
  use a teleporter when another player who has this permission triggers it.
ac.wire.teleporter - to wire teleporters.
ac.wire.boulder - to wire boulders.
ac.trigger.blockactivator - to trigger block activators.
ac.wire.sign - to wire signs.
ac.trigger.signcommand - to trigger sign commands.
ac.passivetrigger.sign - to passively trigger signs.
Self defined permissions in the configuration file.


Plugin Cooperation
---------------------------------------------------------------------------------
This plugin will cooperate with Sign Commands (by Scavenger) if installed. Wired 
signs with a sign command can then be triggered by Advanced Circuits to have the 
commands executed for the player who triggered the circuit.
Note that this will not work when Infinite Signs is installed.

If Protector is installed, then the "/ac switch" command will not work on 
protected objects.


Changelog
---------------------------------------------------------------------------------
Version 1.3 Beta [12/31/2013]
  -Please consider donating to support the developer and for faster updates and
   faster new feature implementation.
  -Configuration file structure has changed, your old configuration has to be 
   ported to the new version.
  -Added teleporter wire / trigger permissions.
  -Updated to Terraria 1.2.2.
  -Permission changed: ac_reloadcfg -> ac.reloadcfg
  -Removed the modifiers feature. From now on, components behaviour can be 
   changed by painting them in different colors where no color yields the
   default behaviour.
  -New function for grandfather clock: white - will send 1 on full moon night.
  -New function for switches: white - forward any signal by chance of 50%.
  -Swappers can have up to 6 paints now, where each paint represents a different
   swap count between 2 and 7.
  -New functions for timers: blue and gray - time * 8 and time / 3.
  -Removed some config permission settings and added hardcoded permissions 
   instead.
  -Fixed a bug causing wireless transmitters to work even through not wired 
   properly.
  -Fixed a bug causting meteorite to be dropped when a player placed a 
   wireless transmitter on a wire without having permission to wire it.
  -Updated for Plugin Common Lib 2.5.

Version 1.2 [05/29/2013]
  -Please consider donating to support the developer.
  -Configuration file structure has changed, your old configuration has to be 
   ported to the new version.
  -World metadata file structure has changed, swappers states of previous 
   versions will be discarded.
  -Permission renamed: "advancedcircuits_reloadcfg" -> "ac_reloadcfg".
  -Added full intergration support. AC will fully integrate if you use the 
   provided custom Terraria Server (optional).
  -Added Wireless Transmitter Component (adamantite ore).
  -Pumps can now be configured using 5 different profiles (default + 4 Modifiers).
  -Added SignCommands cooperation. If a Sign Command is written on a sign 
   triggered by a "1" signal in an Advanced Circuit, then this Sign Command will 
   be executed.
  -Added action lists for statues and improved existing actions.
  -Added BuffPlayer statue action.
  -Added Protector cooperation to check whether tiles to change using 
   /ac switch are protected.
  -Doors are now Port Defining Components and will send signals when opened / 
   closed. They also support one Modifier.
  -Added two new Modifier functions to switches / levers.
  -Added some sign configuration possibilities.
  -Added persistent mode to /ac switch.
  -/ac switch might now toggle active stone.
  -/ac switch might now set Logical-Gate port states.
  -Added up to four Modifiers support to Swappers, thus they can now also serve as 
   counting units.
  -Beside acting as XOR-Gate, gold ore may now also be used as a XOR-Port to set 
   the incoming or outgoing signal for / of a Port Defining Component to 0.
  -Block Activators will now work through most Port Defining Components.
  -Tiles affected by gravity will now work properly with Block Activators.
  -Added sand, pearlsand and ebonsand support to Block Activators.
  -Pressure plates support two Modifiers now: One - They can only be triggered by 
   players, Two - Only by npcs.
  -Made some more general block types work with Block Activators 
   (including obsidian).
  -Block Activators with one Modifier will now replace already existing blocks.
  -Pressure Plates will now ignore Input Ports (glass).
  -If a chest is above a block to be modified by a Block Activator, the related 
   block will not be changed anymore.
  -Projectile knockback and angle can now be defined for Dart Traps, also offset 
   and speed are now of type float instead of integer.
  -Fixed projectile lifetime of dart trap configs sometimes not working.
  -Fixed statues being unable to spawn mobs with ids below 0.
  -Fixed a bug with music box switching.
  -Fixed levers sometimes being signaled by advanced circuits even thought they 
   were not wired by a port.
  -Removed the "/aci" command alias.
  -Some performance and stability improvements.
  -Some minor improvements, fixed some typos.
  -Updated for Plugin Common Lib 1.9.

Version 1.1.1 [11/25/2012]
  -Fixed a wire processing bug causing some wires, depending on their position 
   and alignment, not to be signaled in Advanced Circuits.
  -Fixed a bug causing Block Activators not to properly check whether the maximum 
   of changeable blocks is reached or not.
  -Improved metadata handling.

Version 1.1 [11/19/2012]
  -Updated for TShock 4.0.
  -Configuration file structure has changed, your old configuration will have to 
   be ported to the new version.
  -Components in circuits might now be signaled multiple times during a single 
   circuit execution.
  -Wires of circuits might now be processed multiple times during a single 
   circuit execution.
  -Added the new Port Defining Component "Block Activator" (active stone).
  -Beside acting as NOT-Gate, obsidian may now also be used as a NOT-Port to 
   invert the incoming or outgoing signal for / of a Port Defining Component.
  -Signs will now state their text to the circuit triggering player when 
   signaled with "1" in an Advanced Circuit.
  -Implemented modifiers, special blocks (Cobalt Ore) changing the behaviour of 
   certain components.
  -Timers support 4 modifiers now to modify their time intervals.
  -Dart Traps may have up to 5 different configurations now (one default + 4 
   for the modifiers).
  -Switches and Levers will now forward received signals if they got toggled by 
   them.
  -Added AC sub-command "toggle" / "switch", useful to easily toggle the state 
   of a torch or some other component.
  -Boulders will now start rolling if they receive a "1" signal in an Advanced 
   Circuit.
  -Grandfather clocks can now be used with one or two modifiers.
  -Components will now be dropped if they were placed onto a wire by a player 
   who didn't have the permission to wire them up.
  -Huge performance improvements in matter of circuit processing.
  -Fixed a bug allowing statues to be placed on wires even if that player had 
   not the permission to do so.
  -Improved configuration parsing, validation and error detection.
  -Some general stability improvements.

Version 1.0.4 [10/23/2012]
  -Fixed a bug causing grandfather clock to not work anymore.
  -Fixed a bug causing world metadata sometimes not be validated correctly for 
   Timers.
  -Improved world metadata validation.

Version 1.0.3 [10/03/2012]
  -The version tree 1.0 is from now on considered stable.
  -Added some unit tests to ensure that no further bugs are introduced with new 
   updates.
  -Improved general plugin stability.
  -Gate states are now property deleted from the world metadata if their tile 
   were destroyed or an older version of the world was restored.
  -Timers that were enabled during a world save will now be re-enabled when that 
   world is loaded again.
  -Fixed a bug causing Port Defining Components sometimes not to be signalized by 
   their Input Ports if their circuit met special conditions.
  -Fixed a bug causing pumps to malfunction or not work at all.
  -Fixed a bug causing doors not to always try to open to the opposite direction 
   if blocked.
  -Fixed a bug causing timers in an AC to be signalized when connected directly.
  -Fixed the default ProjectileOffset value of dart traps. It did not reflect the 
   Terraria default value (2), causing objects right infront of them not to be hit.
  -Fixed a very rare occuring bug causing a NullReferenceException related to 
   timers.
  -Fixed a rare occuring bug causing "random exceptions" in circuits meeting 
   special conditions.
  -Updated for Plugins Common Lib 1.3.
  -The Plugin Common Library will now carry its version number in its file name. 
   The old "CCPluginsCommon.dll" can be safely removed.

Version 1.0.2 Beta [09/12/2012]
  -Triggering players are now notified if a circuit exceeds the maximum amount of 
   pumps, dart traps or statues.
  -World Metadata are now written in JSON format.
  -Grandfather clock will no longer signalize a Vanilla Circuit and thus works only 
   in Advanced Circuits.
  -Fixed a bug introduced with 1.0.1 where a music box could not be switched by 
   signals.
  -Fixed a bug causing wrong NPCs to be moved by statues - this could also have 
   lead to exceptions.
  -Fixed a bug causing some configuration settings not to be applied when reloaded 
   using /ac reloadcfg.
  -Fixed a bug causing no entries being written into the TShock log file.
  -Updated for Plugins Common Lib 1.2.

Version 1.0.1 Beta [09/10/2012]
  -Recommended maximum circuit length is now 400.
  -Overriding of Vanilla Circuits is now enabled by default.
  -Players triggering a circuit which exceeds the maximum length are now notified 
   about that.
  -Timers which became invalid due to a server crash will no longer throw exceptions.
  -Fixed timers turning themselfes off in overridden vanilla circuits.
  -Fixed timers sometimes not updating their state for other players when being 
   directly switched.
  -Fixed dart traps doing no damage when no configuration was present.
  -Fixed dart traps not being read properly from the configuration.
  -Implemented a work-around of a Mono configuration issue causing configuration 
   files not to be reloaded successfully.
  -Updated for Plugins Common Lib 1.1

Version 1.0 Beta [09/07/2012]
  -Initial release by CoderCow.
