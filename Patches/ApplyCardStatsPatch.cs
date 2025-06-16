using HarmonyLib;
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
        private static void Prefix(ApplyCardStats __instance, Player ___playerToUpgrade, Gun ___myGunStats, out int __state)
        {
            __state = -1;
            WeaponManager weaponManager = ___playerToUpgrade.gameObject.GetComponent<WeaponManager>();
            if (weaponManager != null && !weaponManager.shouldUpdateStats[weaponManager.activeWeapon])
            {
                GunAmmo gunAmmo = weaponManager.weapons[weaponManager.activeWeapon].gameObject.GetComponentInChildren<GunAmmo>();

                if (gunAmmo)
                {
                    gunAmmo.ammoReg -= ___myGunStats.ammoReg;
                    gunAmmo.maxAmmo -= ___myGunStats.ammo;
                    __state = gunAmmo.maxAmmo;
                    if (___myGunStats.reloadTime != 0)
                        gunAmmo.reloadTimeMultiplier /= ___myGunStats.reloadTime;
                    gunAmmo.reloadTimeAdd -= ___myGunStats.reloadTimeAdd;
                }
            }
        }

        private static void Postfix(ApplyCardStats __instance, Player ___playerToUpgrade, Gun ___myGunStats, int __state)
        {
            WeaponManager weaponManager = ___playerToUpgrade.gameObject.GetComponent<WeaponManager>();
            if (weaponManager != null && ___myGunStats)
            {
                if (__state != -1)
                {
                    GunAmmo gunAmmo = weaponManager.weapons[weaponManager.activeWeapon].gameObject.GetComponentInChildren<GunAmmo>();
                    gunAmmo.maxAmmo = __state;
                }

                for (int i = 0; i < weaponManager.weapons.Count; i++)
                {
                    if (i == weaponManager.activeWeapon) continue; // apply stats to guns that aren't active, exist, and are supposed to have their stats updated.
                    if (!weaponManager.weapons[i]) continue;
                    if (!weaponManager.shouldUpdateStats[i]) continue;

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
