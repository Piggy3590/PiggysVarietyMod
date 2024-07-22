using GameNetcodeStuff;
using HarmonyLib;
using MoreEmotes.Patch;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PiggyVarietyMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        public static AnimationClip middleFinger;
        public static AnimationClip middleFinger_D;
        public static AnimationClip clap;
        public static AnimationClip clap_D;
        public static AnimationClip shy;
        public static AnimationClip griddy;
        public static AnimationClip twerk;
        public static AnimationClip salute;
        public static AnimationClip prisyadka;
        public static AnimationClip sign;
        public static AnimationClip sign_D;

        private static bool pv_isPlayerFirstFrame;

        /*
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        private static void Update_Postfix(PlayerControllerB __instance, ref Animator ___playerBodyAnimator)
        {
            if (___playerBodyAnimator.runtimeAnimatorController != Plugin.playerAnimator && ___playerBodyAnimator.runtimeAnimatorController != Plugin.otherPlayerAnimator)
            {
                if (Plugin.foundMoreEmotes)
                {
                    UpdateMoreEmotesAnimator(__instance, ___playerBodyAnimator);
                }
                else
                {
                    UpdateAnimator(__instance, ___playerBodyAnimator);
                }
            }
            if (__instance == StartOfRound.Instance.localPlayerController)
            {
                if (pv_isPlayerFirstFrame)
                {
                    OnFirstLocalPlayerFrameWithNewAnimator(__instance);
                }
                if (pv_isPlayerFirstFrame)
                {
                    __instance.SpawnPlayerAnimation();
                    pv_isPlayerFirstFrame = false;
                }
            }
        }
        */

        static void UpdateAnimator(PlayerControllerB __instance, Animator ___playerBodyAnimator)
        {
            /*
            if (___playerBodyAnimator.runtimeAnimatorController != Plugin.playerAnimator && ___playerBodyAnimator.runtimeAnimatorController != Plugin.otherPlayerAnimator)
            {
                if (__instance == StartOfRound.Instance.localPlayerController)
                {
                    ___playerBodyAnimator.runtimeAnimatorController = Plugin.playerAnimator;
                    Plugin.mls.LogInfo("Replace Player Animator!");
                }
                else
                {
                    ___playerBodyAnimator.runtimeAnimatorController = Plugin.otherPlayerAnimator;
                    Plugin.mls.LogInfo("Replace Other Player Animator!");
                }
            }
            */
        }

        static void UpdateMoreEmotesAnimator(PlayerControllerB __instance, Animator ___playerBodyAnimator)
        {
            if (middleFinger == null || middleFinger_D == null || clap == null | clap_D == null || shy == null | griddy == null || twerk == null
                    || salute == null || prisyadka == null || sign == null || sign_D == null)
            {
                GetMoreEmotes(___playerBodyAnimator.runtimeAnimatorController);
            }

            if (___playerBodyAnimator.runtimeAnimatorController != Plugin.playerAnimator && ___playerBodyAnimator.runtimeAnimatorController != Plugin.otherPlayerAnimator)
            {
                if (__instance == StartOfRound.Instance.localPlayerController)
                {
                    if (middleFinger != null && middleFinger_D != null && clap != null && clap_D != null && shy != null && griddy != null && twerk != null
                    && salute != null && prisyadka != null && sign != null && sign_D != null)
                    {
                        bool flag = __instance.playerBodyAnimator.runtimeAnimatorController is AnimatorOverrideController;
                        if (!flag)
                        {
                            __instance.playerBodyAnimator.runtimeAnimatorController = new AnimatorOverrideController(__instance.playerBodyAnimator.runtimeAnimatorController);
                        }

                        __instance.SpawnPlayerAnimation();

                        UpdateAnimatorVariable(Plugin.playerAnimator);
                        EmotePatch.local = Plugin.playerAnimator;
                        Plugin.mls.LogInfo("Replace More Emotes Animator!");
                    }
                }
                else
                {
                    if (middleFinger != null && middleFinger_D != null && clap != null && clap_D != null && shy != null && griddy != null && twerk != null
                    && salute != null && prisyadka != null && sign != null && sign_D != null)
                    {
                        UpdateAnimatorVariable(Plugin.otherPlayerAnimator);
                        EmotePatch.others = Plugin.otherPlayerAnimator;
                        Plugin.mls.LogInfo("Replace More Emotes Other Animator!");
                        bool flag = __instance.playerBodyAnimator.runtimeAnimatorController is AnimatorOverrideController;
                        if (!flag)
                        {
                            __instance.playerBodyAnimator.runtimeAnimatorController = new AnimatorOverrideController(__instance.playerBodyAnimator.runtimeAnimatorController);
                        }
                    }
                }
            }
        }

        private static void OnFirstLocalPlayerFrameWithNewAnimator(PlayerControllerB __instance)
        {
            pv_isPlayerFirstFrame = false;
            __instance.SpawnPlayerAnimation();
        }

        static void GetMoreEmotes(RuntimeAnimatorController animator)
        {
            foreach (AnimationClip clip in animator.animationClips)
            {
                if (middleFinger == null && clip.name == "Middle_Finger")
                {
                    middleFinger = clip;
                }
                if (middleFinger_D == null && clip.name == "D_Middle_Finger")
                {
                    middleFinger_D = clip;
                }
                if (clap == null && clip.name == "Clap")
                {
                    clap = clip;
                }
                if (clap_D == null && clip.name == "D_Clap")
                {
                    clap_D = clip;
                }
                if (shy == null && clip.name == "Shy")
                {
                    shy = clip;
                }
                if (griddy == null && clip.name == "The_Griddy")
                {
                    griddy = clip;
                }
                if (twerk == null && clip.name == "Twerk")
                {
                    twerk = clip;
                }
                if (salute == null && clip.name == "Salute")
                {
                    salute = clip;
                }
                if (prisyadka == null && clip.name == "Prisyadka")
                {
                    prisyadka = clip;
                }
                if (sign == null && clip.name == "Sign")
                {
                    sign = clip;
                }
                if (sign_D == null && clip.name == "D_Sign")
                {
                    sign_D = clip;
                }
            }
        }

        static void UpdateAnimatorVariable(RuntimeAnimatorController animator)
        {
            AnimationClip[] clips = animator.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                switch (clips[i].name)
                {
                    case "Middle_Finger":
                        clips[i] = middleFinger;
                        break;
                    case "D_Middle_Finger":
                        clips[i] = middleFinger_D;
                        break;
                    case "Clap":
                        clips[i] = clap;
                        break;
                    case "D_Clap":
                        clips[i] = clap_D;
                        break;
                    case "Shy":
                        clips[i] = shy;
                        break;
                    case "The_Griddy":
                        clips[i] = griddy;
                        break;
                    case "Twerk":
                        clips[i] = twerk;
                        break;
                    case "Salute":
                        clips[i] = salute;
                        break;
                    case "Prisyadka":
                        clips[i] = prisyadka;
                        break;
                    case "Sign":
                        clips[i] = sign;
                        break;
                    case "D_Sign":
                        clips[i] = sign_D;
                        break;
                }
            }
        }
    }
}