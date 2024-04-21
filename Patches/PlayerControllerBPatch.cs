using BepInEx.Logging;
using DunGen;
using DunGen.Graph;
using GameNetcodeStuff;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace PiggyVarietyMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        private static void Update_Postfix(PlayerControllerB __instance, ref Animator ___playerBodyAnimator)
        {
            if (___playerBodyAnimator.runtimeAnimatorController != Plugin.playerAnimator && ___playerBodyAnimator.runtimeAnimatorController != Plugin.otherPlayerAnimator)
            {
                if (__instance == StartOfRound.Instance.localPlayerController)
                {
                    ___playerBodyAnimator.runtimeAnimatorController = Plugin.playerAnimator;
                }else
                {
                    ___playerBodyAnimator.runtimeAnimatorController = Plugin.otherPlayerAnimator;
                }
            }
        }
    }
}