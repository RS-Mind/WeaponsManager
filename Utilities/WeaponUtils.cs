using UnboundLib;
using WeaponsManager;

namespace Assets._WeaponManager.Utilities
{
    class WeaponUtils
    {
        void AddWeaponToPlayer(Player player, Gun weapon, bool applyCardStats)
        {
            WeaponManager weaponManager = player.gameObject.GetOrAddComponent<WeaponManager>();
            weaponManager.AddWeapon(weapon, applyCardStats);
        }
        void RemoveWeaponFromPlayer(Player player, System.Predicate<Gun> weapon)
        {
            WeaponManager weaponManager = player.gameObject.GetOrAddComponent<WeaponManager>();
            weaponManager.RemoveWeapon(weapon);
        }
    }
}
