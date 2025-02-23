## 1.3.19

- Added Gummy Flashlight
- Animator now reverts to original state when you swap or drop the weapons.
- Fixed the error that occurs when loading the mod.
- Removed most/all of the code regarding compatibility with BetterEmotes as it was not being used.

## 1.3.18

- Added [LethalCompany InputUtils](https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils) dependency.
- Changed the default reload key for the rifle to R.

## 1.3.17

- Changed READMD.md

## 1.3.16

- Fixed null check in playerHeldBy for dropped Rifle.

## 1.3.15

- Fixed some player animation issues.

## 1.3.14

- Made v60 animation compatible.

## 1.3.13

- Fixed an issue where mod wouldn't load properly.

## 1.3.12

- Fixed Chamical bottle's ScanNode... again.

## 1.3.11

- Fixed Chamical bottle's ScanNode.

## 1.3.10

- Lowered the default spawn weight of Bulb and Chemical to 30 (requires initialization of existing config to reapply!)

## 1.3.9

- Added ScanNode to Rifle, Magazine.
- Fixed client desync issue with rifle.

## 1.3.8

- Reverted changes from v1.3.7

## 1.3.7

* Changed item names to fix an issue where items had the same name(adding spaces):
 * "Bullet" → "Bulletㅤ"
 * "Revolver" → "Revolverㅤ"
 * "Magazine" → "Magazineㅤ"
 * "Rifle" → "Rifleㅤ"
 * "Axe" → "Axeㅤ"

## 1.3.6

* Fixed an issue where revolver damage was applied to rifle.

## 1.3.5

* Fixed Tesla gate's light.

## 1.3.4

* Added two scraps: Chemical bottle and Bulb.
* Added a config to change the rifle to two-handed item.

## 1.3.3

* Tesla Gate now works! (Special thanks to TestAccount666!)

* Rifle is no longer two-handed.
* Fixed an issue where dropping a rifle while reloading would cause it to reload without consuming the magazine.

## 1.3.2

* Fixed an issue where rifle animations would not work in multiplayer.

## 1.3.1

* Edited manifest.json.
* Edited README.md

## 1.3.0

* (Feature) Added Rifle.
* (Feature) Added Magazine.

* (Feature) Unlimited revolver ammo config has been changed to a config that does not consume ammo when reloading all guns in PVM.

## 1.2.1

* Improved compatibility with mods that change some animations by changing the animator only when necessary.

## 1.2.0

* Updated v55 compatibility.
* Tesla Gate did not work when tested. Will be fixed later

## 1.1.30

* Minor tweaks to revolver animations.

## 1.1.29

* Disabled Tesla Gate screen shake config by default.

## 1.1.28

* Changed the config that disables the screen shake option to one that enables it (to avoid confusion).

## 1.1.27

* Added config to disable camera shake for Tesla Gate.

## 1.1.26

* Changed icon.png

## 1.1.25

* Added infinite ammo config to Revolver.
* Added player/monster damage config to Revolver.

## 1.1.24

* Added KlutzyBubbles' Better Emotes mod to temporary compatibility.

## 1.1.23

* Fixed minor bugs.

## 1.1.22

* (Feature) Added config to adjust volume of Tesla Gate.

## 1.1.21

* (Nerf) Tesla Gate no longer detects enemies (but it still kills enemies it touches).

## 1.1.20

* (Bugfix) Fixed attempting to trigger while the cylinder was open would actually trigger in the other client's game.

## 1.1.19

* (Bugfix) Added ammo sync code for Revolver.

## 1.1.18

* (Bugfix) Fixed Tesla Gate was detecting dead players.

## 1.1.17

* Tesla Gate is no longer experimental.
* (Feature) Changed to require the price to be set to -1 to not add revolvers and ammo to the store.

* (Bugfix) Fixed Tesla Gate could not kill anything.

## 1.1.16

* Tesla Gate is now temporarily “experimental” and is disabled by default.
* (Feature) Tesla Gate spawn weights can now be decimal.

* (Bugfix) Improved Korean translation config.
* (Buxfix) Fixed bullet could be duplicated if the revolver was dropped while reloading.

## 1.1.15

* (Buxfix) Improved compatibility with More Emotes, except for sign animations.

## 1.1.14

* (Buxfix) Fixed a bug when using the terminal while holding a revolver and then exiting would cause the revolver to always have 6 rounds of ammo.

## 1.1.13

* (Buxfix) Fixed player's revolver animation would not work.

## 1.1.12

* (Nerf?) Reduced the hearing range of Tesla Gate.
* (Buxfix) Fixed Tesla Gate's sound could be heard at any distance.
* (Buxfix) Fixed Tesla Gate would only spawn on hosts.

## 1.1.11

* Edited README.md

## 1.1.10

* Tesla Gate has been re-added, with spawn weight config.

* Enemies can now trigger Tesla Gates.
* (Buff) Enemies can now be killed by Tesla Gate.

* Fixed knife animations did not work.

## 1.1.9

* Tesla gate temporarily removed due to a critical issue!

## 1.1.8

* Added dependency.

## 1.1.7

* Added config to add revolvers and bullets to the store.

## 1.1.6

* Edited README.md

## 1.1.5

* Edited README.md

## 1.1.4

* (Buff) Reduced the firing delay of the revolver. (0.3s -> 0.2s)
* Revolver's hammer now plays an animation when dry-fired.

## 1.1.3

* (Buff) Increased the size of revolver hit detection.
* (Buff) Improved revolver hit detection. Now you can deal damage from any distance! (I recommend installing a mod like Crosshair to hit from a distance * and long-range shooting is not recommended due to the severe damage reduction)
* (Feature) Added a revolver icon.

## 1.1.2

* (Feature) Added a config that allows you to adjust the rarity of revolver and revolver ammo.

## 1.1.1

* (Buxfix) Fixed reload sequence would continue even if you switched to a different slot while reloading the revolver.

## 1.1.0

* (Feature) Added Revolver.

## 1.0.3

* (Bugfix) Fixed Tesla Gate could not kill players.

## 1.0.2

* (Nerf) Increased the interval between Tesla Gate triggers.

## 1.0.1

* edited README.md

## 1.0.0

* initial release.
