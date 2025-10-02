using HarmonyLib;
using UnboundLib;
using UnityEngine;

namespace WeaponsManager
{
    public class ManagedWeaponAdder : MonoBehaviour
    {
        public Gun weapon; // The weapon to be added to the player
        public bool applyCardStats; // Should the weapon inherit stats from other cards?
        public float inactiveReloadTimeMultiplier; // Modifies the reload time while the weapon is inactive. Set to 0 to disable passive reload.
        public GameObject icon; // The icon to be displayed in the weapon UI
        public string weaponName; // The name of the weapon for the UI

        internal void Apply(Player player, WeaponManager weaponManager)
        {
            SetTeamColor[] setTeamColors = weapon.GetComponentsInChildren<SetTeamColor>();
            int layerID = player.gameObject.GetComponentInChildren<SpriteMask>().frontSortingLayerID;
            foreach (SetTeamColor color in setTeamColors)
            { // Find and fix team-colored elements
                var mask = color.gameObject.GetComponent<SpriteMask>();
                mask.frontSortingLayerID = layerID;
                mask.backSortingLayerID = layerID;
            }
            weaponManager.AddWeapon(weapon, applyCardStats, icon, weaponName, inactiveReloadTimeMultiplier);
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
                WeaponManager weaponManager = ___playerToUpgrade.gameObject.GetOrAddComponent<WeaponManager>();
                adder.Apply(___playerToUpgrade, weaponManager);
            }
        }
    }
}