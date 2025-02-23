using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace PiggyVarietyMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        public static RuntimeAnimatorController originalPlayerAnimator;

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void Start_Postfix(PlayerControllerB __instance) 
        {
            if (originalPlayerAnimator == null)
            {
                originalPlayerAnimator = GameObject.Instantiate(__instance.playerBodyAnimator.runtimeAnimatorController);
                originalPlayerAnimator.name = "DefaultPlayerAnimator";
            }
        }
    }
}