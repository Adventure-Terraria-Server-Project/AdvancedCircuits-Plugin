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

Beside that, note that this plugin had never been tested under 
Linux and Mono before. 
Please report any success or problem - I'm curious about this.


A Quickstart to Advanced Circuits
--------------------------------------------------------------
Because this stuff is far easier explained and understood 
if you see it in action, you should download this pre-configured 
testserver. Just extract it, run "Run Server.bat" and use the
game to connect to your localhost: 127.0.0.1

I'll try to write a detailed textual guide on how to use 
Advanced Circuits later, though I'm too busy with plugin 
development right now. If they can spare some free-time I would 
very appreciate a community member writing a guide instead.


Changelog
--------------------------------------------------------------
Version 1.0.2 Beta [12.09.2012]
  -Triggering players are now notified if a circuit exceeds the
   maximum amount of Pumps, Dart Traps or Statues.
  -World Metadata are now written in JSON format.
  -Grandfather Clock will no longer signalize a Vanilla Circuit
   and thus works only in Advanced Circuits.
  -Fixed a bug causing wrong NPCs to be moved by statues - this
   could also have lead to exceptions.
  -Fixed a bug causing some configuration settings not to be
   applied when reloaded using /ac reloadcfg.
  -Fixed a bug causing no entries being written into the TShock 
   log file.

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
