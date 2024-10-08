# Tangerine
Tangerine is a mod manager and modding framework for **MEGA MAN X DiVE Offline**.

## Mod Manager
As a **mod manager**, it removes all of the complexities of installing mods and provides great features, such as:
- Loading mods without overwriting original files
- Customizing the load order of mods
- Hot-reloading of mods, without the need to restart the game
- (Planned) An in-game menu for managing your mods!

## Modding Framework
As a **modding framework**, it makes developing mods a much smoother process, by allowing:
- Loading new asset bundles or updating existing bundles without replacing original files
  - Asset bundles can be loaded with their hashed name or from their full unhashed file path (example: `MyMod/model/character/ch140_000`)
  - Asset bundles do not have to be encrypted. In order to load unencrypted bundles, use a CRC value of 0 in `AssetBundleConfig.json`
  - Assets can be remapped from an asset bundle to another.
    - For example, you can make the game load a specific sprite from your modded asset bundle, and load the rest from the original file
    - This allows mods that modify the same asset bundle to be compatible with each other!

- Patching table data and adding new entries
  - Patch tables from code by using the `TangerineDataManager` class
  - Or patch them without even writing a plugin, by providing the tables as JSON files 

- Adding new character controller classes, for customizing the behavior of skills/animations of a character
