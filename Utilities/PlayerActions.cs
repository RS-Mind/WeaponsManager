using HarmonyLib;
using InControl;
using Photon.Pun;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using WeaponsManager;

namespace WeaponsManager.Utilities // Adds actions to players
{
    [Serializable]
    public class PlayerActionsAdditionalData
    {
        public PlayerAction nextWeapon;
        public PlayerAction previousWeapon;


        public PlayerActionsAdditionalData()
        {
            nextWeapon = null;
            previousWeapon = null;
        }
    }

    public static class PlayerActionsExtension // Magic
    {
        public static readonly ConditionalWeakTable<PlayerActions, PlayerActionsAdditionalData> data =
            new ConditionalWeakTable<PlayerActions, PlayerActionsAdditionalData>();

        public static PlayerActionsAdditionalData GetAdditionalData(this PlayerActions playerActions)
        {
            return data.GetOrCreateValue(playerActions);
        }

        public static void AddData(this PlayerActions playerActions, PlayerActionsAdditionalData value)
        {
            try
            {
                data.Add(playerActions, value);
            }
            catch (Exception) { }
        }
    }

    [HarmonyPatch(typeof(PlayerActions))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { })]
    class PlayerActionsPatchPlayerActions
    {
        private static void Postfix(PlayerActions __instance) // Voidseer hotkey
        {
            __instance.GetAdditionalData().nextWeapon = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Next Weapon" });

            __instance.GetAdditionalData().previousWeapon = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Previous Weapon" });
        }
    }

    [HarmonyPatch(typeof(PlayerActions), "CreateWithControllerBindings")] // Voidseer for controller
    class PlayerActionsPatchCreateWithControllerBindings
    {
        private static void Postfix(ref PlayerActions __result)
        {
            __result.GetAdditionalData().nextWeapon.AddDefaultBinding(InputControlType.DPadRight);
            __result.GetAdditionalData().previousWeapon.AddDefaultBinding(InputControlType.DPadLeft);
        }
    }
    
    [HarmonyPatch(typeof(PlayerActions), "CreateWithKeyboardBindings")] // Voidseer for keyboard
    class PlayerActionsPatchCreateWithKeyboardBindings
    {
        private static void Postfix(ref PlayerActions __result)
        {
            __result.GetAdditionalData().nextWeapon.AddDefaultBinding(Key.E);
            __result.GetAdditionalData().previousWeapon.AddDefaultBinding(Key.Q);
        }
    }

    [HarmonyPatch(typeof(GeneralInput), "Update")]
    class GeneralInputPatchUpdate // Check if the actions happened
    {
        private static void Postfix(GeneralInput __instance)
        {
            if (!__instance.GetComponent<CharacterData>().view.IsMine) return;
            if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().nextWeapon.WasPressed && __instance.GetComponent<WeaponManager>() is WeaponManager weaponHandler)
            {
                __instance.GetComponent<CharacterData>().view.RPC("RPCNextWeapon", RpcTarget.All);
            }
            if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().previousWeapon.WasPressed && __instance.GetComponent<WeaponManager>() is WeaponManager weaponHandler2)
            {
                __instance.GetComponent<CharacterData>().view.RPC("RPCPreviousWeapon", RpcTarget.All);
            }
        }
    }
}