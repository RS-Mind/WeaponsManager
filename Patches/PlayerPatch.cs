using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnboundLib;
using UnityEngine;
using WeaponsManager;

namespace WeaponsManager.Patches
{
    [HarmonyPatch(typeof(Player), "Start")]
    class PlayerPatch
    {
        private static void Postfix(Player __instance)
        {
            __instance.gameObject.GetOrAddComponent<WeaponManager>();
        }
    }
}
