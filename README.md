# DDoorDebugMod
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/PrestonRooker/DDoorDebugMod)](https://github.com/PrestonRooker/DDoorDebugMod/releases/latest)
[![GitHub issues](https://img.shields.io/github/issues/PrestonRooker/DDoorDebugMod)](https://github.com/PrestonRooker/DDoorDebugMod/issues)

Debug mod for Death's Door designed to help find glitches and assist in speedrun practice.

The mod can be installed through the raven mod installer https://github.com/dpinela/Raven

## Manual Installation
Make sure you have an appropriate bepinex version installed with your game.

Download the DDoorDebug4.8.dll and wfshader.bundle files from the latest release: https://github.com/PrestonRooker/DDoorDebugMod/releases  
Put the files in game/BepInEx/plugins.  
If you are playing on 1.0 or this version doesn't happen to work, you should use the 4.5.2 version.  

## Usage
```
Tab (by default) opens a binding menu where you can see all features. How to use:
Clicking on feature name will activate it.
Clicking on the key name will allow you to bind the key by clicking the desired key, pressing shift + esc will make the key "None" meaning it isn't bound. (you need to hold shift before clicking or it will bind shift)
The s, c and a are toggleable buttons that decide what modifiers, shift, ctrl and alt respectively, need to be pressed to activate set feature. Selecting multiple means pressing them all at the same time.
If the "extra" toggleable button is pressed you can press other modifiers than the ones specified and it'll still activate and the other way around.

Change Stat Upgrades: Open pause menu and you can see the window on the left. Click + to add attribute, - to remove attribute. They will wrap around but you can bypass this by holding shift.

list of all features and what they do (if this is out of date remind wijo in dd discord, very possible I forget in the future):
open bind menu: opens the bind menu
info menu: opens info gui to the right that gives you a lot of different info
show hp: shows hp of everything that can be damaged. sometimes you need to re-enable to get it to show up for certain enemies.
warp menu: menu that allows you to warp to any scene. names taken directly from code and often don't match the catual name.
warp to selected: equivalent to pressing the "travel to" button in the warp menu, but bindable to a key.
heal to full: heals you to full once.
auto heal: keeps you at full hp but you can still take dmg.
inf magic: you get your magic points back instantly.
toggle godmode: toggles a state where you can't get hit.
boss reset: brings back to life all bosses
boss reset with cuts: same as above but also re-enables the pre-boss cutscenes
give soul: gives 100 soul (sorry reset stats we all loved you :c)
unlock weapons: gives you all the weapons
unlock spells: gives you all the spells
save pos: saves your coordinates for the current map. does not include anything else eg. rotation.
load pos: loads saved coordinates for the current map.
save gpos: saves over the global position, which is shared across maps.
save gpos: loads the global position, which is shared across maps.
show colliders: cycles between what colliders are shown.
load visible colliders: loads all visible colliders into memory to then be shown accrding to the above option.
freecam: allows you to look with the mouse. (og creator intended this to be a full on fps fly but it's non-functional)
pos history: shows your position trajectory within the world
velocity graph: shows a graph of your velocity on the info menu, though seems to be broken for some mysterious reason.
timescale down: makes the game run slower
timescale up: makes the game run faster (up to 5x)
reset timescale: returns timescale to default
rotate cam right: rotates the camera right, fps dependent (sry)
rotate cam left: rotates the camera left, fps dependent (sry)
reset cam: resets both cam rotation and zoom
mouse tele: when holding this key input is disabled and you can click to teleport anywhere.
zoom in: zooms in
zoom out: zooms out
toggle noclip: toggles noclip + fly
tele up: teleports you up by a bit, intended to be used with the noclip but also useful for climbing ladders faster
tele down: teleports you down by a bit, intended to be used with the noclip
toggle night: toggles the night state
save file: saves the file as if you walked through a door
reload file: identical to doing an sq (except if you are indoors it can sometimes bug a bit sry)
get gp: gives you glide potential
instant textskip: while the key is held, spams through text as fast as possible. note: for fastest possible skipping it is not recommended to put your textskipper key here and use the ahk since that'd mean it's only held for about half the time and thus being significantly slower.
```
