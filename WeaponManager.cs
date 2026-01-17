using HarmonyLib;
using InControl.NativeProfile;
using Photon.Pun;
using SoundImplementation;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnboundLib;
using UnboundLib.GameModes;
using UnityEngine;
using UnityEngine.Audio;

namespace WeaponsManager
{
    public class WeaponManager : MonoBehaviour
    {
        public List<Gun> weapons = new List<Gun>();
        public List<bool> shouldUpdateStats = new List<bool>();
        public List<bool> willReload = new List<bool>();
        public List<GameObject> icons = new List<GameObject>();
        public List<string> names = new List<string>();
        public List<float> reloadTimers = new List<float>();
        public List<float> reloadMultipliers = new List<float>();
        private Player player;
        public int activeWeapon { get; private set; } = 0;
        private WeaponHandler weaponHandler;
        private Holding holding;

        private GameObject visualizer;
        private Transform activeSlot;
        private Transform prevSlot;
        private Transform nextSlot;
        private TextMeshProUGUI activeName;
        private TextMeshProUGUI prevName;
        private TextMeshProUGUI nextName;

        public void Awake()
        {
            player = GetComponentInParent<Player>();
            weaponHandler = GetComponentInParent<WeaponHandler>();
            holding = GetComponentInParent<Holding>();
            Gun gun = player.data.weaponHandler.gun;
            weapons.Add(gun);
            shouldUpdateStats.Add(true);
            willReload.Add(false);
            reloadTimers.Add(0f);
            reloadMultipliers.Add(0.25f);
            if (player.data.view.IsMine)
            {
                visualizer = Instantiate(WeaponsManager.assets.LoadAsset<GameObject>("SlotVisualizer"), GameObject.Find("Game/UI/UI_Game/Canvas").transform);
                visualizer.GetComponent<Canvas>().sortingLayerName = "MostFront";
                activeSlot = visualizer.transform.GetChild(0);
                prevSlot = visualizer.transform.GetChild(1);
                nextSlot = visualizer.transform.GetChild(2);
                activeName = activeSlot.GetComponentInChildren<TextMeshProUGUI>();
                prevName = prevSlot.GetComponentInChildren<TextMeshProUGUI>();
                nextName = nextSlot.GetComponentInChildren<TextMeshProUGUI>();
                names.Add("Pistol");
                icons.Add(Instantiate(WeaponsManager.assets.LoadAsset<GameObject>("PistolIcon"), visualizer.transform));
            }
        }

        public void Start()
        {
            if (player.data.view.IsMine)
            {
                visualizer.SetActive(false);
            }
            GameModeManager.AddHook(GameModeHooks.HookRoundStart, RoundStart);
        }

        public void FixedUpdate()
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                if (i == activeWeapon) continue;
                reloadTimers[i] += Time.fixedDeltaTime * reloadMultipliers[i];
            }
        }

        public void AddWeapon(Gun weapon, bool applyCardStats, GameObject icon, string name, float reloadMultiplier = 0.25f)
        {
            Gun newWeapon = Instantiate(weapon);
            weapons.Add(newWeapon);
            shouldUpdateStats.Add(applyCardStats);
            willReload.Add(false);
            reloadTimers.Add(0f);
            reloadMultipliers.Add(reloadMultiplier);
            if (player.data.view.IsMine)
            {
                icons.Add(Instantiate(icon, visualizer.transform));
                names.Add(name);
                UpdateIcons();
            }
            newWeapon.gameObject.SetActive(false);

            if (applyCardStats)
            {
                // Apply ammo stats
                GunAmmo playerAmmo = weapons[0].GetComponentInChildren<GunAmmo>();
                GunAmmo newAmmo = newWeapon.GetComponentInChildren<GunAmmo>();
                newAmmo.ammoReg += playerAmmo.ammoReg;
                newAmmo.maxAmmo += playerAmmo.maxAmmo - 3;
                newAmmo.maxAmmo = Mathf.Clamp(newAmmo.maxAmmo, 1, 90);
                newAmmo.reloadTimeMultiplier *= playerAmmo.reloadTime;
                newAmmo.reloadTimeAdd += playerAmmo.reloadTimeAdd;

                // Apply the default gun's stats to the new gun
                try
                {
                    ApplyCardStats.CopyGunStats(weapons[0], newWeapon);
                } catch { }
            }


            FixNewGunSound(newWeapon);
            if (player.data.view.IsMine)
            {
                newWeapon.AddAttackAction(new Action(player.GetComponent<SyncPlayerMovement>().SendShoot));
                visualizer.SetActive(true);
            }
        }

        private void FixNewGunSound(Gun gun) 
        {
            GunAmmo newAmmo = gun.GetComponentInChildren<GunAmmo>();
            SoundGun soundGun = gun.soundGun;

            AudioMixerGroup sdxAudioGroup = SoundVolumeManager.Instance.audioMixer.FindMatchingGroups("SFX")[0];

            newAmmo.soundReloadComplete.variables.audioMixerGroup = sdxAudioGroup;
            newAmmo.soundReloadInProgressLoop.variables.audioMixerGroup = sdxAudioGroup;

            // Also apply the audio mixer group to all sound events in the reload loop
            foreach (var soundEvent in newAmmo.soundReloadInProgressLoop.variables.triggerOnPlaySoundEvents)
            {
                soundEvent.variables.audioMixerGroup = sdxAudioGroup;
            }

            foreach (var soundEvent in newAmmo.soundReloadInProgressLoop.variables.triggerOnStopSoundEvents)
            {
                soundEvent.variables.audioMixerGroup = sdxAudioGroup;
            }

            // Apply the audio mixer group to all sound events in the gun
            soundGun.soundShotModifierBasic.single.variables.audioMixerGroup = sdxAudioGroup;
            soundGun.soundShotModifierBasic.singleAutoLoop.variables.audioMixerGroup = sdxAudioGroup;
            soundGun.soundShotModifierBasic.singleAutoTail.variables.audioMixerGroup = sdxAudioGroup;
            soundGun.soundShotModifierBasic.shotgun.variables.audioMixerGroup = sdxAudioGroup;
            soundGun.soundShotModifierBasic.shotgunAutoLoop.variables.audioMixerGroup = sdxAudioGroup;
            soundGun.soundShotModifierBasic.shotgunAutoTail.variables.audioMixerGroup = sdxAudioGroup;

            soundGun.soundImpactModifierBasic.impactCharacter.variables.audioMixerGroup = sdxAudioGroup;
            soundGun.soundImpactModifierBasic.impactEnvironment.variables.audioMixerGroup = sdxAudioGroup;

            soundGun.soundImpactModifierDamageToExplosionHuge.impactCharacter.variables.audioMixerGroup = sdxAudioGroup;
            soundGun.soundImpactModifierDamageToExplosionHuge.impactEnvironment.variables.audioMixerGroup = sdxAudioGroup;

            soundGun.soundImpactModifierDamageToExplosionMedium.impactCharacter.variables.audioMixerGroup = sdxAudioGroup;
            soundGun.soundImpactModifierDamageToExplosionMedium.impactEnvironment.variables.audioMixerGroup = sdxAudioGroup;

            soundGun.soundImpactBounce.variables.audioMixerGroup = sdxAudioGroup;
            soundGun.soundImpactBullet.variables.audioMixerGroup = sdxAudioGroup;
        }

        public void UpdateIcons()
        {
            foreach (GameObject icon in icons)
                icon.transform.SetYPosition(-1000); // Get all icons offscreen 

            if (weapons.Count > 2) // Set the icons properly
            {
                icons[((activeWeapon + icons.Count - 1) % icons.Count)].transform.SetPositionAndRotation(prevSlot.position, prevSlot.rotation);
                icons[((activeWeapon + icons.Count - 1) % icons.Count)].transform.localScale = new Vector3(2, 2, 2);
                prevName.text = names[((activeWeapon + icons.Count - 1) % icons.Count)];
            }
            else
                prevName.text = "";

            if (weapons.Count > 1)
            {
                icons[(activeWeapon + 1) % icons.Count].transform.SetPositionAndRotation(nextSlot.position, nextSlot.rotation);
                icons[(activeWeapon + 1) % icons.Count].transform.localScale = new Vector3(2, 2, 2);
                nextName.text = names[(activeWeapon + 1) % icons.Count];
            }
            else
                nextName.text = "";

            icons[activeWeapon].transform.SetPositionAndRotation(activeSlot.position, activeSlot.rotation);
            icons[activeWeapon].transform.localScale = new Vector3(3, 3, 3);
            activeName.text = names[activeWeapon];
        }

        public void RemoveWeapon(System.Predicate<Gun> weapon)
        {
            int i = weapons.FindIndex(weapon);
            weapons.RemoveAt(i);
            shouldUpdateStats.RemoveAt(i);
            icons.RemoveAt(i);
            names.RemoveAt(i);
            willReload.RemoveAt(i);
            reloadTimers.RemoveAt(i);
            reloadMultipliers.RemoveAt(i);
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
            weapons[index].transform.SetPositionAndRotation(weapons[activeWeapon].transform.position, weapons[activeWeapon].transform.rotation);

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
                try
                {
                    ammo.reloadAnim.PlayIn();
                }
                catch { }
            }
            else
            {
                try
                {
                    ammo.reloadAnim.PlayOut();
                }
                catch { }
            }

            for (int i = 1; i < ammo.populate.transform.childCount; i++)
            {
                if (i <= (int)ammo.GetFieldValue("currentAmmo"))
                {
                    try
                    {
                        ammo.populate.transform.GetChild(i).GetComponent<CurveAnimation>().PlayIn();
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        ammo.populate.transform.GetChild(i).GetComponent<CurveAnimation>().PlayOut();
                    }
                    catch { }
                }
            }

            ammo.SetFieldValue("reloadCounter", (float)ammo.GetFieldValue("reloadCounter") - reloadTimers[index]);
            reloadTimers[index] = 0f;

            if (willReload[index])
            {
                weapons[index].sinceAttack = 100f;
                try { weapons[index].gameObject.GetComponentInChildren<GunAmmo>()?.ReloadAmmo(); } catch { }
                willReload[index] = false;
            }

            if (player.data.view.IsMine)
                UpdateIcons();
        }

        public void Reset()
        {
            SetActiveWeapon(0);
            for (int i = weapons.Count - 1; i > 0; i++)
            {
                Destroy(weapons[i]);
            }
            Destroy(this);
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

        private IEnumerator RoundStart(IGameModeHandler gm)
        {
            for (int i = 0; i < willReload.Count; i++)
            {
                if (i != activeWeapon)
                willReload[i] = true;
            }
            yield break;
        }

        private void OnDestroy()
        {
            GameModeManager.RemoveHook(GameModeHooks.HookRoundStart, RoundStart);
        }
    }

    [HarmonyPatch(typeof(Player), "FullReset")]
    public class FullResetPatch
    {
        static void Prefix(Player __instance)
        {
            WeaponManager weaponManager = __instance.gameObject.GetComponent<WeaponManager>();
            if (weaponManager)
            {
                weaponManager.Reset();
            }
        }
    }
}
