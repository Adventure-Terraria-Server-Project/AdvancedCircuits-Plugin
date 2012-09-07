==============================================================
 Advanced Circuits Plugin for Terraria API and TShock
   by CoderCow
==============================================================

The Revolutionized Wiring
--------------------------------------------------------------

Advanced Circuits is the long awaited revolution of the 
Terraria wiring mechanism, adding a completely rewritten 
and extended wiring engine to TerrariaAPI and TShock driven 
Terraria servers. 

Beside keeping and extending Terraria's Vanilla Circuits 
and components, a new type of circuit is added called 
"Advanced Circuit" giving players the freedom of building 
complex systems using binary logic a new types of 
components.

Several configuration settings give the server's owner
control of wiring limits and capabilities. This also 
includes a mighty configuration mechanism of Statues and 
their performed actions when powered.

NOTE: Before installing the plugin, read the "Known Problems"
section first!


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
npcs will still behave as usual. They will neither trigger an 
Advanced Circuit nor will any configuration options or 
extensions take any effect on the Vanilla Circuit they execute.


A Quickstart to Advanced Circuits
--------------------------------------------------------------
Because this stuff is far easier explained and understood if
you see it in action, you should download the testworld.

I'll try to write a detailed textual guide on how to use 
Advanced Circuits later, though I'm too busy with plugin 
development right now. If they can spare some free-time
I would very appreciate a community member writing a guide 
instead though.

Changelog
--------------------------------------------------------------

Version 1.0 [07.09.2012]
  -Initial release by CoderCow
