# DDoorDebugMod
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/PrestonRooker/DDoorDebugMod)](https://github.com/PrestonRooker/DDoorDebugMod/releases/latest)
[![GitHub issues](https://img.shields.io/github/issues/PrestonRooker/DDoorDebugMod)](https://github.com/PrestonRooker/DDoorDebugMod/issues)

Debug mod for Death's Door designed to help find glitches and assist in speedrun practice.

## Installation
Follow these steps to download the mod for the first time.

1. Download the latest release of format DDoorDebugMod_vX.X.X.zip from https://github.com/PrestonRooker/DDoorDebugMod/releases
2. Extract the DDoorDebugMod_vX.X.X.zip folder (make sure you don't lose any of the extracted files)
3. Open your steam library and right click on Death's Door. Then click Manage -> Browse Local Files
4. Move all the contents of the extracted folder into the same folder containing DeathsDoor.exe
5. After this is done you should have the files winhttp.dll, BepInEx, doorstop_config.ini, and README.TXT in the same directory as DeathsDoor.exe
6. Now the mod is installed, the next time you open Death's Door it will be running

7. If the mod doesn't appear to be functioning, try this dll instead (install using the update instructions starting from step 2): https://drive.google.com/file/d/1-XPTtx3V8XRR-AbYEeXwtyY5UrRUn69s/view?usp=sharing (this is mod version 0.3.3,.net 4.8 dll should be added to the next release.)

If you ever want to uninstall the mod, simply rename or delete the file named winhttp.dll

## Update
Follow these steps to update the mod if you already have a previous version installed.

1. Download the latest plugin of format DDoorDebug.dll from https://github.com/PrestonRooker/DDoorDebugMod/releases
2. Find Death's Door in your steam library, right click and go to Manage -> Browse Local Files
3. You should be in a directory containing a folder named BepInEx
4. Go to BepInEx/plugins
5. Move your new DDoorDebug.dll into the same folder, replacing the old DDoorDebug.dll
6. Now the mod is updated, the next time you open Death's Door it will be running the new version

7. If the mod doesn't appear to be functioning, try this dll instead (install using the update instructions starting from step 2): https://drive.google.com/file/d/1-XPTtx3V8XRR-AbYEeXwtyY5UrRUn69s/view?usp=sharing (this is mod version 0.3.3,.net 4.8 dll should be added to the next release.)

## Usage
```
Tab (by default) opens a binding menu where you can see all features. How to use:
Clicking on feature name will activate it.
Clicking on the key name will allow you to bind the key by clicking the desired key, pressing shift + esc will make the key "None" meaning it isn't bound.
The s, c and a are toggleable buttons that decide what modifiers, shift, ctrl and alt respectively, need to be pressed to activate set feature. Selecting multiple means pressing them all at the same time.
If the "extra" toggleable button is pressed you can press other modifiers than the ones specified and it'll still activate and the other way around.

Change Stat Upgrades: Open pause menu and you can see the window on the left. Click + to add attribute, - to remove attribute. They will wrap around but you can bypass this by holding shift.
```
