==============================================================
 Advanced Circuits Plugin for Terraria API and TShock
   by CoderCow
==============================================================

The Revolutionized Wiring
--------------------------------------------------------------

Advanced Circuits is the long awaited revolution of the 
Terraria wiring mechanism, adding a completely rewritten and 
extended wiring engine to TerrariaAPI and TShock driven 
Terraria servers.

Beside keeping and extending Terraria's Vanilla Circuits and 
components, a new type of circuit is added called 
"Advanced Circuit" giving players the freedom of building 
complex systems using binary logic and new types of components.

Several configuration settings give the server's owner control 
of wiring limits and capabilities. This also includes a mighty 
configuration mechanism of Statues and their performed actions
when powered.

NOTE: Before installing and configuring the plugin, read the 
"Known Problems" section first!


Features
--------------------------------------------------------------
-Adds "Advanced Circuits", a more powerful wiring mechanism.
-Improves the existing Terraria wiring mechanics.
-Enables the server's owner to have more control over wiring.


Commands
--------------------------------------------------------------
/advancedcircuits|aci|ac (info)
/advancedcircuits|aci|ac commands
/advancedcircuits|aci|ac blocks
/advancedcircuits|aci|ac reloadcfg


Permissions
--------------------------------------------------------------
advancedcircuits_reloadcfg - to reload the configuration file.
self declared permissions for statues.


Known Problems
--------------------------------------------------------------
The Plugin is currently unable to fully override Terraria's 
wiring mechanism because I couldn't find a way to hook into 
the handling of npc / mob movement using the current ServerAPI. 
This means that Pressure Plates which are triggered by mobs or 
npcs will still behave as usual even if you enabled the 
configuration setting OverrideVanillaCircuits. They will 
neither trigger an Advanced Circuit nor will any configuration 
options or extensions take any effect on the Vanilla Circuit 
they execute.

1.0 to 1.1 Upgrade Notes
--------------------------------------------------------------
Delete your "tshock/Advanced Circuits/World Data" folder.


A Quickstart to Advanced Circuits
--------------------------------------------------------------
Have a look in the official wiki:
https://github.com/CoderCow/AdvancedCircuits-Plugin/wiki/


Changelog
--------------------------------------------------------------
Version 1.1.1 [xx.xx.xxxx]
  -Improved metadata handling.
Version 1.1 [19.11.2012]
  -Updated for TShock 4.0.
  -Configuration file structure has changed, your old configuration
   will have to be ported to the new version.
  -Components in circuits might now be signaled multiple times during 
   a single circuit execution.
  -Wires of circuits might now be processed multiple times during a 
   single circuit execution.
  -Added the new Port Defining Component "Block Activator" 
   (Active Stone).
  -Beside acting as NOT-Gate, Obsidian may now also be used as a
   NOT-Port to invert the incoming or outgoing signal for / of a 
   Port Defining Component.
  -Signs will now state their text to the circuit triggering player
   when signaled with "1" in an Advanced Circuit.
  -Implemented modifiers, special blocks (Cobalt Ore) changing the
   behaviour of certain components.
  -Timers support 4 modifiers now to modify their time intervals.
  -Dart Traps may have up to 5 different configurations now (one
   default + 4 for the modifiers).
  -Switches and Levers will now forward received signals if they
   got toggled by them.
  -Added AC sub-command "toggle" / "switch", useful to easily toggle 
   the state of a Torch or some other component.
  -Boulders will now start rolling if they receive a "1" signal in an
   Advanced Circuit.
  -Grandfather Clocks can now be used with one or two modifiers.
  -Components will now be dropped if they were placed onto a wire by
   a player who didn't have the permission to wire them up.
  -Huge performance improvements in matter of circuit processing.
  -Fixed a bug allowing Statues to be placed on wires even if that
   player had not the permission to do so.
  -Improved configuration parsing, validation and error detection.
  -Some general stability improvements.

Version 1.0.4 [23.10.2012]
  -Fixed a bug causing Grandfather Clock to not work anymore.
  -Fixed a bug causing World Metadata sometimes not be validated 
   correctly for Timers.
  -Improved World Metadata validation.

Version 1.0.3 [03.10.2012]
  -The version tree 1.0 is from now on considered stable.
  -Added some unit tests to ensure that no further bugs are 
   introduced with new updates.
  -Improved general plugin stability.
  -Gate states are now property deleted from the world metadata 
   if their tile were destroyed or an older version of the world
   was restored.
  -Timers that were enabled during a world save will now be 
   re-enabled when that world is loaded again.
  -Fixed a bug causing Port Defining Components sometimes not 
   to be signalized by their Input Ports if their circuit met
   special conditions.
  -Fixed a bug causing Pumps to malfunction or not work at all.
  -Fixed a bug causing Doors not to always try to open to the 
   opposite direction if blocked.
  -Fixed a bug causing Timers in an AC to be signalized when
   connected directly.
  -Fixed the default ProjectileOffset value of DartTraps. It did 
   not reflect the Terraria default value (2), causing objects 
   right infront of them not to be hit.
  -Fixed a very rare occuring bug causing a NullReferenceException 
   related to Timers.
  -Fixed a rare occuring bug causing "random exceptions" in 
   circuits meeting special conditions.
  -Updated for Plugins Common Lib 1.3.
  -The Plugin Common Library will now carry its version number 
   in its file name. The old "CCPluginsCommon.dll" can be 
   safely removed.

Version 1.0.2 Beta [12.09.2012]
  -Triggering players are now notified if a circuit exceeds the
   maximum amount of Pumps, Dart Traps or Statues.
  -World Metadata are now written in JSON format.
  -Grandfather Clock will no longer signalize a Vanilla Circuit
   and thus works only in Advanced Circuits.
  -Fixed a bug introduced with 1.0.1 where a Music Box could not
   be switched by signals.
  -Fixed a bug causing wrong NPCs to be moved by statues - this
   could also have lead to exceptions.
  -Fixed a bug causing some configuration settings not to be
   applied when reloaded using /ac reloadcfg.
  -Fixed a bug causing no entries being written into the TShock 
   log file.
  -Updated for Plugins Common Lib 1.2.

Version 1.0.1 Beta [10.09.2012]
  -Recommended maximum circuit length is now 400.
  -Overriding of Vanilla Circuits is now enabled by default.
  -Players triggering a circuit which exceeds the maximum 
   length are now notified about that.
  -Timers which became invalid due to a server crash will no 
   longer throw exceptions.
  -Fixed Timers turning themselfes off in overridden vanilla 
   circuits.
  -Fixed Timers sometimes not updating their state for other 
   players when being directly switched.
  -Fixed Dart Traps doing no damage when no configuration was present.
  -Fixed Dart Traps not being read properly from the configuration.
  -Implemented a work-around of a Mono configuration issue 
   causing configuration files not to be reloaded successfully.
  -Updated for Plugins Common Lib 1.1

Version 1.0 Beta [07.09.2012]
  -Initial release by CoderCow
