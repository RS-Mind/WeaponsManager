using BepInEx;
using HarmonyLib;
using Jotunn.Utils;
using System.Collections;
using System.Reflection;
using UnboundLib;
using UnboundLib.GameModes;
using UnityEngine;

namespace WeaponsManager
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class WeaponsManager : BaseUnityPlugin
    {
        private const string ModId = "com.rsmind.rounds.weaponsmanager";
        private const string ModName = "Weapon Manager";
        public const string Version = "1.3.0";
        public const string ModInitials = "WM";
        internal static AssetBundle assets;
        public static WeaponsManager instance { get; private set; }

        void Awake()
        {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
            assets = AssetUtils.LoadAssetBundleFromResources("weaponsmanager", typeof(WeaponsManager).Assembly);
            if (assets == null)
            {
                UnityEngine.Debug.Log("Failed to load Weapons Manager asset bundle");
            }
        }


        void Start()
        {
            instance = this;
            GameModeManager.AddHook(GameModeHooks.HookGameStart, GameStart);
        }

        public static bool Debug = false;

        IEnumerator GameStart(IGameModeHandler gm)
        {
            foreach (Player player in PlayerManager.instance.players)
                player.gameObject.GetOrAddComponent<WeaponManager>();
            yield break;
        }

    }
}