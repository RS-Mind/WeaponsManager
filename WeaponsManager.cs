using BepInEx;
using HarmonyLib;

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
        public const string Version = "1.0.0";
        public const string ModInitials = "WM";
        public static WeaponsManager instance { get; private set; }

        void Awake()
        {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }


        void Start()
        {
            instance = this;
        }

        public static bool Debug = false;
    }
}