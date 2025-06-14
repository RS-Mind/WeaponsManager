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
        private static void Postfix(ApplyCardStats __instance, Player ___playerToUpgrade, Gun ___myGunStats)
        {
            WeaponManager weaponManager = ___playerToUpgrade.gameObject.GetComponent<WeaponManager>();
            if (weaponManager != null && ___myGunStats)
            {
                for (int i = 0; i < weaponManager.weapons.Count; i++)
                {
                    if (i == weaponManager.activeWeapon) continue; // apply stats to guns that aren't active, exist, and are supposed to have their stats updated.
                    if (!weaponManager.weapons[i]) continue;
                    if (!weaponManager.updateStats[i]) continue;

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
                    if (___myGunStats.projectiles.Length != 0)
                    {
                        weaponManager.weapons[i].projectiles[0].objectToSpawn = ___myGunStats.projectiles[0].objectToSpawn;
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
}
