# Weapons Manager
A library to help mod developers to add additional guns to the player. Guns added by this mod can be swapped between using Q/E or R/L DPad. While inactive, a gun will reload slowly.

## Usage Guide for Developers:
1. Import the included unitypackage into your Unity project.
2. Modify one of the included gun prefabs to met your needs.
	- The ExampleGun is set up to be modified the same way you'd modify a card. WeaponBase is a replica of the vanilla gun. They should be used with the Apply Card Stats bool enabled/disabled respectively.
	- If you're using asymmetrical textures, you can use the included LeftRightScale script to cause them to mirror properly.
	- Shoot Position determines the angle of the bullet fired, not the position.
	- Important: Do not modify the two included objects. The forces applied to keep the gun in place in multiplayer are calculated specifically for the gun's physics.
3. Attach the ManagedWeaponAdder script to your card's object.
4. Attach your gun to the script
5. Set the parameters
	- Turning the apply card stats toggle on will make the player's current cards and all future cards apply to the gun
	- The Inactive Reload Time Multiplier changes how quickly your gun reloads while inactive
6. Add an icon for your weapon
	- Use the included template for reference.
	- This icon can easily be the same as a FancyCardBar icon
7. Add a name for your weapon
	- This is used in the UI to help identify your gun.