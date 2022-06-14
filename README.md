# DDoorDebugMod
![GitHub release (latest by date)](https://img.shields.io/github/v/release/PrestonRooker/DDoorDebugMod)
![GitHub issues](https://img.shields.io/github/issues/PrestonRooker/DDoorDebugMod)

Debug mod for Death's Door designed to help find glitches and assist in speedrun practice.

## Installation
Follow these steps to download the mod for the first time.

1. Download the latest release of format DDoorDebugMod_vX.X.X.zip from https://github.com/PrestonRooker/DDoorDebugMod/releases
2. Open your steam library and right click on Death's Door then click Manage>Browse Local Files
3. This should open a folder containing DeathsDoor.exe, move the DDoorDebugMod_vX.X.X.zip into this folder
4. Click into the .zip file and extract it here
5. Move all the contents of the extracted folder into the same directory containing DeathsDoor.exe
6. After this is done you should have the files winhttp.dll, BepInEx, doorstop_config.ini, and README.TXT in the same directory as DeathsDoor.exe
7. Now the mod is installed, the next time you open Death's Door it will be running

If you ever want to uninstall the mod, simply rename or delete the file named winhttp.dll

## Update
Follow these steps to update the mod if you already have a previous version installed.

1. Download the latest plugin of format DDoorDebug.dll from https://github.com/PrestonRooker/DDoorDebugMod/releases
2. Find Death's Door in your steam library, right click and go to Manage>Browse Local Files
3. You should be in a directory containing a folder named BepInEx
4. Go to BepInEx/plugins
5. Delete the existing DDoorDebug.dll
6. Move your new DDoorDebug.dll into the same folder, where the old DDoorDebug.dll was
7. Now the mod is updated, the next time you open Death's Door it will be running the new version

## Usage
```
F1 :: info pane
F2 :: enemy HP overlay
F3 :: Scene travel
F4 :: auto heal
F5 :: boss reset (you still need to travel to "OldCrowVoid" scene manually for Crow)
Shift+F5 :: resets boss intro flags as well
F6 :: grants 20k souls and resets stats to 0
F8 :: set teleport checkpoint in scene
F7 :: get all weapons
F9 :: teleport to saved cp (only within same scene)
F10 :: toggle state of collider visibility (box->mesh->capsule->sphere > cycle back)
Ctrl+F10 :: populate collider cache (populated with objects camera can see)
Insert :: Timescale Down
PgUp :: Timescale UP
Home :: Timescale back to default
F11 :: freecam
Ctrl+Mouse1 :: teleport to mouse position on screen
Delete :: rotate fixed camera left 45d
PgDown :: rotate fixed camera right 45d
+/- :: FOV change
Backspace :: enable Velocity graph. Only Active when Info Pane is active
0 :: enable line of sight
P :: enable path history trail
```
