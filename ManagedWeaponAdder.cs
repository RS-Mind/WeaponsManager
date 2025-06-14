using HarmonyLib;
using UnboundLib;
using UnityEngine;

namespace WeaponsManager
{
    public class ManagedWeaponAdder : MonoBehaviour
    {
        public Gun weapon; // The weapon to be added to the player

        public bool applyCardStats;

        public void Apply(Player player)
        {
            WeaponManager weaponManager = player.gameObject.GetOrAddComponent<WeaponManager>();
            SetTeamColor[] setTeamColors = weapon.GetComponentsInChildren<SetTeamColor>();
            int layerID = player.gameObject.GetComponentInChildren<SpriteMask>().frontSortingLayerID;
            foreach (SetTeamColor color in setTeamColors)
            { // Find and fix team-colored elements
                var mask = color.gameObject.GetComponent<SpriteMask>();
                mask.frontSortingLayerID = layerID;
                mask.backSortingLayerID = layerID;
            }
            try
            {
                weaponManager.AddWeapon(weapon, applyCardStats);
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(ApplyCardStats), "ApplyStats")]
    [HarmonyPriority(Priority.Low)]
    public class ApplyPlayerStatsPatch
    {
        static void Postfix(ApplyCardStats __instance, Player ___playerToUpgrade)
        {
            if (__instance.GetComponent<ManagedWeaponAdder>() is ManagedWeaponAdder adder)
            {
                adder.Apply(___playerToUpgrade);
            }
        }
    }
}