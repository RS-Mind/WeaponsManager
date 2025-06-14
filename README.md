# Weapons Manager
A library to help mod developers to add additional guns to the player. Guns added by this mod can be swapped between using Q/E or R/L DPad. While inactive, a gun will be frozen, not reloading or refreshing attack cooldown.

## Usage Guide for Developers:
1. Import the included unitypackage into your Unity project.
2. Modify one of the included gun prefabs to met your needs.
	- The ExampleGun is set up to be modified the same way you'd modify a card. WeaponBase is a replica of the vanilla gun. They should be used with the Apply Card Stats bool enabled/disabled respectively.
	- Important: Do not modify the two included objects. The forces applied to keep the gun in place in multiplayer are calculated specifically for the gun's physics.
3. Attach the ManagedWeaponAdder script to your card's object.
4. Attach your gun to the script
5. Set the Apply Card Stats bool
	- Turning it on will make the player's current cards and all future cards apply to the gun
	- Turning it off will make the gun remain as-given forever.