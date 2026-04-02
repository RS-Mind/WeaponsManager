using HarmonyLib;
using SoundImplementation;
using System.Collections;
using System.Collections.Generic;
using UnboundLib;
using UnityEngine;
using WeaponsManager;

namespace WeaponsManager.Patches
{
    [HarmonyPatch(typeof(ApplyCardStats), "ApplyStats")]
    [HarmonyPriority(Priority.High)]
    class ApplyCardStatsPatch
    {
        static List<SoundShotModifier> soundShotModifier = null;
        static List<SoundImpactModifier> soundImpactModifier = null;
        private static void Prefix(ApplyCardStats __instance, Player ___playerToUpgrade, Gun ___myGunStats, out int __state)
        {
            soundShotModifier = null;
            soundImpactModifier = null;
            __state = -1;
            WeaponManager weaponManager = ___playerToUpgrade.gameObject.GetComponent<WeaponManager>();
            if (weaponManager != null)
            {
                if (weaponManager.shouldIgnoreSounds[weaponManager.activeWeapon])
                {
                    soundShotModifier = new List<SoundShotModifier>((List<SoundShotModifier>)weaponManager.weapons[weaponManager.activeWeapon].soundGun.GetFieldValue("soundShotModifierAllList"));
                    soundImpactModifier = new List<SoundImpactModifier>((List<SoundImpactModifier>)weaponManager.weapons[weaponManager.activeWeapon].soundGun.GetFieldValue("soundImpactModifierAllList"));
                }
                if (!weaponManager.shouldUpdateStats[weaponManager.activeWeapon])
                {
                    GunAmmo gunAmmo = weaponManager.weapons[weaponManager.activeWeapon].gameObject.GetComponentInChildren<GunAmmo>();

                    if (gunAmmo)
                    {
                        __state = gunAmmo.maxAmmo;
                    }
                }
            }
        }

        private static void Postfix(ApplyCardStats __instance, Player ___playerToUpgrade, Gun ___myGunStats, int __state)
        {
            WeaponManager weaponManager = ___playerToUpgrade.gameObject.GetComponent<WeaponManager>();
            if (weaponManager != null && ___myGunStats)
            {
                if (!weaponManager.shouldUpdateStats[weaponManager.activeWeapon])
                {
                    GunAmmo gunAmmo = weaponManager.weapons[weaponManager.activeWeapon].gameObject.GetComponentInChildren<GunAmmo>();

                    if (gunAmmo)
                    {
                        gunAmmo.ammoReg -= ___myGunStats.ammoReg;
                        gunAmmo.maxAmmo -= ___myGunStats.ammo;
                        gunAmmo.maxAmmo = __state;
                        if (___myGunStats.reloadTime != 0)
                            gunAmmo.reloadTimeMultiplier /= ___myGunStats.reloadTime;
                        gunAmmo.reloadTimeAdd -= ___myGunStats.reloadTimeAdd;
                    }
                }
                if (weaponManager.shouldIgnoreSounds[weaponManager.activeWeapon])
                {
                    if (soundShotModifier != null)
                        weaponManager.weapons[weaponManager.activeWeapon].soundGun.SetFieldValue("soundShotModifierAllList", soundShotModifier);
                    if (soundImpactModifier != null)
                        weaponManager.weapons[weaponManager.activeWeapon].soundGun.SetFieldValue("soundImpactModifierAllList", soundImpactModifier);
                    try
                    {
                        weaponManager.weapons[weaponManager.activeWeapon].soundGun.RefreshSoundModifiers();
                    }
                    catch { }
                }

                for (int i = 0; i < weaponManager.weapons.Count; i++)
                {
                    if (i == weaponManager.activeWeapon) continue; // apply stats to guns that aren't active, exist, and are supposed to have their stats updated.
                    if (!weaponManager.weapons[i]) continue;
                    if (!weaponManager.shouldUpdateStats[i]) continue;

                    if (weaponManager.shouldIgnoreSounds[i])
                    {
                        soundShotModifier = new List<SoundShotModifier>((List<SoundShotModifier>)weaponManager.weapons[i].soundGun.GetFieldValue("soundShotModifierAllList"));
                        soundImpactModifier = new List<SoundImpactModifier>((List<SoundImpactModifier>)weaponManager.weapons[i].soundGun.GetFieldValue("soundImpactModifierAllList"));
                    }

                    GunAmmo gunAmmo = weaponManager.weapons[i].gameObject.GetComponentInChildren<GunAmmo>();

                    if (gunAmmo)
                    {
                        gunAmmo.ammoReg += ___myGunStats.ammoReg;
                        gunAmmo.maxAmmo += ___myGunStats.ammo;
                        gunAmmo.maxAmmo = Mathf.Clamp(gunAmmo.maxAmmo, 1, 90);
                        gunAmmo.reloadTimeMultiplier *= ___myGunStats.reloadTime;
                        gunAmmo.reloadTimeAdd += ___myGunStats.reloadTimeAdd;
                    }

                    if (___myGunStats.lockGunToDefault)
                    {
                        weaponManager.weapons[i].defaultCooldown = ___myGunStats.forceSpecificAttackSpeed;
                        weaponManager.weapons[i].lockGunToDefault = ___myGunStats.lockGunToDefault;
                    }
                    try
                    {
                        ApplyCardStats.CopyGunStats(___myGunStats, weaponManager.weapons[i]);
                    }
                    catch { }
                    if (weaponManager.shouldIgnoreSounds[i])
                    {
                        if (soundShotModifier != null)
                            weaponManager.weapons[i].soundGun.SetFieldValue("soundShotModifierAllList", soundShotModifier);
                        if (soundImpactModifier != null)
                            weaponManager.weapons[i].soundGun.SetFieldValue("soundImpactModifierAllList", soundImpactModifier);
                        try
                        {
                            weaponManager.weapons[i].soundGun.RefreshSoundModifiers();
                        } catch { }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ApplyCardStats), "CopyGunStats")]
    [HarmonyPriority(Priority.High)]
    class CopyGunStatsPatch
    {
        static bool Prefix(ApplyCardStats __instance, Gun copyToGun)
        {
            if (copyToGun.player)
                if (copyToGun.player.GetComponent<WeaponManager>() is WeaponManager weaponManager)
                {
                    if (weaponManager.weapons[weaponManager.activeWeapon] == copyToGun && !weaponManager.shouldUpdateStats[weaponManager.activeWeapon])
                    {
                        return false;
                    }
                }
            return true;
        }
    }
}
