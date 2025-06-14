using Photon.Pun;
using System.Collections.Generic;
using UnboundLib;
using UnityEngine;

namespace WeaponsManager
{
    class WeaponManager : MonoBehaviour
    {
        internal List<Gun> weapons = new List<Gun>();
        internal List<bool> updateStats = new List<bool>();
        private Player player;
        internal int activeWeapon = 1;
        private WeaponHandler weaponHandler;
        private Holding holding;
        public void Start()
        {
            player = GetComponentInParent<Player>();
            weaponHandler = GetComponentInParent<WeaponHandler>();
            holding = GetComponentInParent<Holding>();
            Gun gun = player.data.weaponHandler.gun;
            weapons.Add(gun);
            updateStats.Add(true);
        }

        public void AddWeapon(Gun weapon, bool applyCardStats)
        {
            Gun newWeapon = Instantiate(weapon);
            weapons.Add(newWeapon);
            updateStats.Add(applyCardStats);
            newWeapon.gameObject.SetActive(false);
            if (applyCardStats)
            { // Apply the default gun's stats to the new gun
                ApplyCardStats.CopyGunStats(weapons[0], newWeapon);
                // Undo default stats
                newWeapon.attackSpeed /= 0.3f;

                // Update ammo stats
                GunAmmo playerAmmo = weapons[0].GetComponentInChildren<GunAmmo>();
                GunAmmo newAmmo = newWeapon.GetComponentInChildren<GunAmmo>();
                newAmmo.ammoReg += playerAmmo.ammoReg;
                newAmmo.maxAmmo += playerAmmo.maxAmmo - 3;
                newAmmo.maxAmmo = Mathf.Clamp(newAmmo.maxAmmo, 1, 90);
                newAmmo.reloadTimeMultiplier *= playerAmmo.reloadTime;
                newAmmo.reloadTimeAdd += playerAmmo.reloadTimeAdd;
            }
        }

        public void RemoveWeapon(System.Predicate<Gun> weapon)
        {
            int i = weapons.FindIndex(weapon);
            weapons.RemoveAt(i);
            updateStats.RemoveAt(i);
        }

        public void NextWeapon()
        {
            SetActiveWeapon((activeWeapon + 1) % weapons.Count);
        }

        public void PreviousWeapon()
        {
            SetActiveWeapon((activeWeapon - 1 + weapons.Count) % weapons.Count);
        }
        private void SetActiveWeapon(int index)
        {
            // Disable previous weapon
            weapons[activeWeapon].gameObject.SetActive(false);

            // Enable new weapon
            activeWeapon = index;
            weapons[activeWeapon].gameObject.SetActive(true);
            weaponHandler.gun = weapons[activeWeapon];
            holding.SetFieldValue("gun", weapons[activeWeapon]);
            holding.holdable = weapons[activeWeapon].GetComponentInParent<Holdable>();

            // Ensure current ammo/reload ring is properly rendered
            GunAmmo ammo = weapons[activeWeapon].gameObject.GetComponentInChildren<GunAmmo>();

            if ((int)ammo.GetFieldValue("currentAmmo") <= 0)
            {
                ammo.reloadAnim.PlayIn();
            }
            else
            {
                ammo.reloadAnim.PlayOut();
            }

            for (int i = 1; i < ammo.populate.transform.childCount; i++)
            {
                if (i <= (int)ammo.GetFieldValue("currentAmmo"))
                {
                    ammo.populate.transform.GetChild(i).GetComponent<CurveAnimation>().PlayIn();
                }
                else
                {
                    ammo.populate.transform.GetChild(i).GetComponent<CurveAnimation>().PlayOut();
                }
            }
        }

        [PunRPC]
        public void RPCNextWeapon()
        {
            NextWeapon();
        }

        [PunRPC]
        public void RPCPreviousWeapon()
        {
            PreviousWeapon();
        }
    }
}
