using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using PiggyVarietyMod.Patches;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using LethalLib.Modules;
using BepInEx.Bootstrap;

namespace PiggyVarietyMod
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "Piggy.PiggyVarietyMod";
        private const string modName = "PiggyVarietyMod";
        private const string modVersion = "1.1.31";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static Plugin Instance;

        public static ManualLogSource mls;
        public static AssetBundle Bundle;

        public static GameObject teslaGateSpawn;
        public static GameObject teslaGatePrefab;

        public static AudioClip teslaIdleStart;
        public static AudioClip teslaIdle;
        public static AudioClip teslaIdleEnd;

        public static AudioClip teslaCrack;
        public static AudioClip teslaBeep;
        public static AudioClip teslaWindUp;
        public static AudioClip teslaUnderbass;
        public static AudioClip teslaClimax;

        public static Item revolverItem;
        public static Item revolverAmmoItem;

        public static int revolverRarity;
        public static int revolverAmmoRarity;
        public static int revolverMaxPlayerDamage;
        public static int revolverMaxMonsterDamage;
        public static bool revolverInfinityAmmo;

        public static int revolverPrice;
        public static int revolverAmmoPrice;

        public static float teslaSpawnWeight;
        public static float teslaSoundVolume;
        public static bool teslaShake;

        public static bool translateKorean;

        
        /*
        public static GameObject revolverPrefab;
        public static GameObject revolverAmmoPrefab;
        */

        public static AudioClip revolverAmmoInsert;
        public static AudioClip revolverCylinderOpen;
        public static AudioClip revolverCylinderClose;
        public static AudioClip revolverDryFire;

        public static AudioClip revolverBlast1;
        public static AudioClip revolverBlast2;

        public static RuntimeAnimatorController playerAnimator;
        public static RuntimeAnimatorController otherPlayerAnimator;

        public static bool foundMoreEmotes;

        public static string PluginDirectory;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            Plugin.PluginDirectory = base.Info.Location;

            LoadAssets();

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("Piggy's Variety Mod is loaded");

            teslaSoundVolume = (float)base.Config.Bind<float>("Generic", "TeslaGateVolume", 1, "(Default 1) Sets the sound volume for Tesla Gate.").Value;
            teslaShake = (bool)base.Config.Bind<bool>("Generic", "TeslaGateShake", false, "(Experimental, Default false) Shake the screen when near Tesla Gate.").Value;
            revolverMaxPlayerDamage = (int)base.Config.Bind<int>("Generic", "RevolverMaxPlayerDamage", 70, "(Default 70) Sets the maximum amount of damage the revolver can inflict on the player.").Value;
            revolverMaxMonsterDamage = (int)base.Config.Bind<int>("Generic", "RevolverMaxMonsterDamage", 4, "(Default 4) Sets the maximum amount of damage the revolver can inflict on the monster.").Value;
            revolverInfinityAmmo = (bool)base.Config.Bind<bool>("Generic", "RevolverInfinityAmmo", false, "(Default false) If true, the revolver will not consume ammo.").Value;
            teslaSpawnWeight = (float)base.Config.Bind<float>("Spawn", "TeslaGateWeight", 1, "(Default 1) Sets the spawn weight for the Tesla Gate.").Value;

            revolverRarity = (int)base.Config.Bind<int>("Scrap", "RevolverRarity", 20, "(Default 20) Sets the spawn rarity for the revolver.").Value;
            revolverAmmoRarity = (int)base.Config.Bind<int>("Scrap", "RevolverAmmoRarity", 60, "(Default 60) Sets the spawn rarity for the revolver ammo.").Value;

            revolverPrice = (int)base.Config.Bind<int>("Store", "RevolverPrice", -1, "(Recommended -1 or 550) Set the price of the revolver. If -1, removes the item from the store list.").Value;
            revolverAmmoPrice = (int)base.Config.Bind<int>("Store", "RevolverAmmoPrice", -1, "(Recommended -1 or 30) Set the price of the revolver ammo. If -1, removes the item from the store list.").Value;

            translateKorean = (bool)base.Config.Bind<bool>("Translation", "Enable Korean", false, "Set language to Korean.").Value;

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(teslaGatePrefab);

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(revolverItem.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(revolverAmmoItem.spawnPrefab);
            Utilities.FixMixerGroups(revolverItem.spawnPrefab);
            Utilities.FixMixerGroups(revolverAmmoItem.spawnPrefab);

            LethalLib.Modules.Items.RegisterItem(revolverItem);
            LethalLib.Modules.Items.RegisterItem(revolverAmmoItem);

            foreach (KeyValuePair<string, PluginInfo> pluginInfo in Chainloader.PluginInfos)
            {
                BepInPlugin metadata = pluginInfo.Value.Metadata;
                if (metadata.GUID.Equals("MoreEmotes", StringComparison.OrdinalIgnoreCase) || metadata.GUID.Equals("BetterEmotes", StringComparison.OrdinalIgnoreCase))
                {
                    foundMoreEmotes = true;
                    mls.LogInfo("[Piggys Variety Mod] Detected More Emotes / Better Emotes!");
                    mls.LogInfo("[Piggys Variety Mod] More Emotes / Better Emotes may not be compatible!");
                }
            }

            if (translateKorean)
            { 
                Translate();
            }
            CreateShopItem();
            LethalLib.Modules.Items.RegisterScrap(revolverItem, revolverRarity, Levels.LevelTypes.All);
            LethalLib.Modules.Items.RegisterScrap(revolverAmmoItem, revolverAmmoRarity, Levels.LevelTypes.All);

            /*
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(revolverPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(revolverAmmoPrefab);
            */

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void LoadAssets()
        {
            try
            {
                Plugin.Bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Plugin.PluginDirectory), "piggyvarietymod"));
            }
            catch (Exception ex)
            {
                Plugin.mls.LogError("Couldn't load asset bundle: " + ex.Message);
                return;
            }
            try
            {
                teslaGateSpawn = Bundle.LoadAsset<GameObject>("TeslaGateSpawn.prefab");
                teslaGatePrefab = Bundle.LoadAsset<GameObject>("TeslaGate.prefab");
                teslaGatePrefab.AddComponent<TeslaGate>();

                teslaCrack = Bundle.LoadAsset<AudioClip>("Tesla_Crack.ogg");
                teslaBeep = Bundle.LoadAsset<AudioClip>("Tesla_Beeps.ogg");
                teslaWindUp = Bundle.LoadAsset<AudioClip>("Tesla_WindUp.ogg");
                teslaUnderbass = Bundle.LoadAsset<AudioClip>("Tesla_Underbass.ogg");
                teslaClimax = Bundle.LoadAsset<AudioClip>("Tesla_Climax.ogg");

                teslaIdleStart = Bundle.LoadAsset<AudioClip>("Tesla_IdleStarts.ogg");
                teslaIdle = Bundle.LoadAsset<AudioClip>("Tesla_IdleLoop.ogg");
                teslaIdleEnd = Bundle.LoadAsset<AudioClip>("Tesla_IdleEnd.ogg");

                revolverItem = Bundle.LoadAsset<Item>("Revolver.asset");
                revolverAmmoItem = Bundle.LoadAsset<Item>("RevolverAmmo.asset");

                /*
                revolverPrefab = Bundle.LoadAsset<GameObject>("RevolverItem.prefab");
                revolverAmmoPrefab = Bundle.LoadAsset<GameObject>("RevolverAmmo.prefab");
                */

                revolverAmmoInsert = Bundle.LoadAsset<AudioClip>("RevolverReload.wav");
                revolverCylinderOpen = Bundle.LoadAsset<AudioClip>("RevolverCylinderOpen.wav");
                revolverCylinderClose = Bundle.LoadAsset<AudioClip>("RevolverCylinderClose.wav");

                revolverDryFire = Bundle.LoadAsset<AudioClip>("RevolverDryFire.wav");

                revolverBlast1 = Bundle.LoadAsset<AudioClip>("RevolverBlast1.wav");
                revolverBlast2 = Bundle.LoadAsset<AudioClip>("RevolverBlast2.wav");

                playerAnimator = Bundle.LoadAsset<RuntimeAnimatorController>("PlayerAnimator.controller");
                otherPlayerAnimator = Bundle.LoadAsset<RuntimeAnimatorController>("OtherPlayerAnimator.controller");

                RevolverItem revolverScript = revolverItem.spawnPrefab.AddComponent<RevolverItem>();

                revolverScript.grabbable = true;
                revolverScript.grabbableToEnemies = true;
                revolverScript.gunReloadSFX = revolverAmmoInsert;
                revolverScript.cylinderOpenSFX = revolverCylinderOpen;
                revolverScript.cylinderCloseSFX = revolverCylinderClose;

                revolverScript.gunShootSFX.Add(revolverBlast1);
                revolverScript.gunShootSFX.Add(revolverBlast2);

                revolverScript.noAmmoSFX = revolverDryFire;
                revolverScript.gunSafetySFX = revolverDryFire;
                revolverScript.switchSafetyOffSFX = revolverDryFire;
                revolverScript.switchSafetyOnSFX = revolverDryFire;

                revolverScript.gunAudio = revolverScript.gameObject.GetComponent<AudioSource>();
                revolverScript.gunShootAudio = revolverScript.gameObject.transform.GetChild(1).GetComponent<AudioSource>();
                revolverScript.gunBulletsRicochetAudio = revolverScript.gameObject.transform.GetChild(2).GetComponent<AudioSource>();

                revolverScript.gunAnimator = revolverScript.gameObject.GetComponent<Animator>();

                revolverScript.revolverRayPoint = revolverScript.gameObject.transform.GetChild(3);
                revolverScript.gunShootParticle = revolverScript.gameObject.transform.GetChild(3).GetChild(0).GetComponent<ParticleSystem>();

                revolverScript.cylinderTransform = revolverScript.gameObject.transform.GetChild(5).GetChild(0).GetChild(0);

                revolverScript.revolverAmmos.Add(revolverScript.gameObject.transform.GetChild(5).GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshRenderer>());
                revolverScript.revolverAmmos.Add(revolverScript.gameObject.transform.GetChild(5).GetChild(0).GetChild(0).GetChild(1).GetComponent<MeshRenderer>());
                revolverScript.revolverAmmos.Add(revolverScript.gameObject.transform.GetChild(5).GetChild(0).GetChild(0).GetChild(2).GetComponent<MeshRenderer>());
                revolverScript.revolverAmmos.Add(revolverScript.gameObject.transform.GetChild(5).GetChild(0).GetChild(0).GetChild(3).GetComponent<MeshRenderer>());
                revolverScript.revolverAmmos.Add(revolverScript.gameObject.transform.GetChild(5).GetChild(0).GetChild(0).GetChild(4).GetComponent<MeshRenderer>());
                revolverScript.revolverAmmos.Add(revolverScript.gameObject.transform.GetChild(5).GetChild(0).GetChild(0).GetChild(5).GetComponent<MeshRenderer>());

                revolverScript.revolverAmmoInHandTransform = revolverScript.gameObject.transform.GetChild(0);
                revolverScript.revolverAmmoInHand = revolverScript.gameObject.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
                revolverScript.gunCompatibleAmmoID = 500;

                revolverScript.itemProperties = revolverItem;

                base.Logger.LogInfo("Successfully loaded assets!");
            }
            catch (Exception ex2)
            {
                base.Logger.LogError("Couldn't load assets: " + ex2.Message);
            }
        }

        void Translate()
        {
            revolverItem.toolTips[0] = "격발 : [RMB]";
            revolverItem.toolTips[1] = "탄약 삽탄하기 : [E]";
            revolverItem.toolTips[2] = "실린더 열기 : [Q]";

            revolverItem.spawnPrefab.GetComponentInChildren<ScanNodeProperties>().headerText = "리볼버";

            revolverAmmoItem.spawnPrefab.GetComponentInChildren<ScanNodeProperties>().headerText = "총알";
        }

        void CreateShopItem()
        {
            if (translateKorean)
            {
                revolverAmmoItem.itemName = "총알";
                revolverItem.itemName = "리볼버";
            }
            else
            {
                revolverAmmoItem.itemName = "Bullet";
                revolverItem.itemName = "Revolver";
            }

            TerminalNode revolverItemShopNode = ScriptableObject.CreateInstance<TerminalNode>();
            if (translateKorean)
            {
                revolverItemShopNode.displayText = "리볼버를 주문하려고 합니다. 수량: [variableAmount]. \r\n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력하세요.\n\n";
            }else
            {
                revolverItemShopNode.displayText = "You have requested to order revolvers. Amount: [variableAmount]. \r\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\n\n";
            }
            revolverItemShopNode.clearPreviousText = true;
            revolverItemShopNode.isConfirmationNode = true;
            revolverItemShopNode.maxCharactersToType = 15;
            revolverItemShopNode.buyRerouteToMoon = -1;
            revolverItemShopNode.displayPlanetInfo = -1;
            revolverItemShopNode.shipUnlockableID = -1;
            revolverItemShopNode.creatureFileID = -1;
            revolverItemShopNode.storyLogFileID = -1;

            TerminalNode revolverItemShopNode2 = ScriptableObject.CreateInstance<TerminalNode>();
            if (translateKorean)
            {
                revolverItemShopNode2.displayText = "[variableAmount]개의 리볼버를 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다..\n\n";
            }else
            {
                revolverItemShopNode2.displayText = "Ordered [variableAmount] revolvers. Your new balance is [playerCredits].\n\nOur contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\n\n";
            }
            revolverItemShopNode2.clearPreviousText = true;
            revolverItemShopNode2.maxCharactersToType = 15;
            revolverItemShopNode2.buyRerouteToMoon = -1;
            revolverItemShopNode2.displayPlanetInfo = -1;
            revolverItemShopNode2.shipUnlockableID = -1;
            revolverItemShopNode2.creatureFileID = -1;
            revolverItemShopNode2.storyLogFileID = -1;

            TerminalNode revolverItemShopInfo = ScriptableObject.CreateInstance<TerminalNode>();
            if (translateKorean)
            {
                revolverItemShopInfo.displayText = "\n더욱 강력한 자기 보호를 위해!\n실린더를 열고 리볼버 탄약을 삽탄하여 장전하세요.\n\n";
            }else
            {
                revolverItemShopInfo.displayText = "\nFor more powerful self-defense!\nOpen the cylinder and insert revolver ammo to load it.\n\n";
            }
            revolverItemShopInfo.clearPreviousText = true;
            revolverItemShopInfo.maxCharactersToType = 15;
            revolverItemShopInfo.buyRerouteToMoon = -1;
            revolverItemShopInfo.displayPlanetInfo = -1;
            revolverItemShopInfo.shipUnlockableID = -1;
            revolverItemShopInfo.creatureFileID = -1;
            revolverItemShopInfo.storyLogFileID = -1;

            TerminalNode revolverAmmoShopNode = ScriptableObject.CreateInstance<TerminalNode>();
            if (translateKorean)
            {
                revolverAmmoShopNode.displayText = "리볼버 탄약을 주문하려고 합니다. 수량: [variableAmount]. \r\n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력하세요.\n\n";
            }
            else
            {
                revolverAmmoShopNode.displayText = "You have requested to order revolver ammos. Amount: [variableAmount]. \r\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\n\n";
            }
            revolverAmmoShopNode.clearPreviousText = true;
            revolverAmmoShopNode.isConfirmationNode = true;
            revolverAmmoShopNode.maxCharactersToType = 15;
            revolverAmmoShopNode.buyRerouteToMoon = -1;
            revolverAmmoShopNode.displayPlanetInfo = -1;
            revolverAmmoShopNode.shipUnlockableID = -1;
            revolverAmmoShopNode.creatureFileID = -1;
            revolverAmmoShopNode.storyLogFileID = -1;

            TerminalNode revolverAmmoShopNode2 = ScriptableObject.CreateInstance<TerminalNode>();
            if (translateKorean)
            {
                revolverAmmoShopNode2.displayText = "[variableAmount]개의 리볼버 탄약을 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다..\n\n";
            }else
            {
                revolverAmmoShopNode2.displayText = "Ordered [variableAmount] revolver ammos. Your new balance is [playerCredits].\n\nOur contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\n\n";
            }
            revolverAmmoShopNode2.clearPreviousText = true;
            revolverAmmoShopNode2.maxCharactersToType = 15;
            revolverAmmoShopNode2.buyRerouteToMoon = -1;
            revolverAmmoShopNode2.displayPlanetInfo = -1;
            revolverAmmoShopNode2.shipUnlockableID = -1;
            revolverAmmoShopNode2.creatureFileID = -1;
            revolverAmmoShopNode2.storyLogFileID = -1;

            TerminalNode revolverAmmoShopInfo = ScriptableObject.CreateInstance<TerminalNode>();
            if (translateKorean)
            {
                revolverAmmoShopInfo.displayText = "\n리볼버에 장전하고 <b>치명적인</b> 순간에 격발하세요!\n\n";
            }
            else
            {
                revolverAmmoShopInfo.displayText = "\nLoad to your revolver and fire at LETHAL moments!\n\n";
            }
            revolverAmmoShopInfo.clearPreviousText = true;
            revolverAmmoShopInfo.maxCharactersToType = 15;
            revolverAmmoShopInfo.buyRerouteToMoon = -1;
            revolverAmmoShopInfo.displayPlanetInfo = -1;
            revolverAmmoShopInfo.shipUnlockableID = -1;
            revolverAmmoShopInfo.creatureFileID = -1;
            revolverAmmoShopInfo.storyLogFileID = -1;

            if (revolverPrice > -1)
            {
                LethalLib.Modules.Items.RegisterShopItem(revolverItem, revolverItemShopNode, revolverItemShopNode2, revolverItemShopInfo, revolverPrice);
            }
            if (revolverAmmoPrice > -1)
            {
                LethalLib.Modules.Items.RegisterShopItem(revolverAmmoItem, revolverAmmoShopNode, revolverAmmoShopNode2, revolverAmmoShopInfo, revolverAmmoPrice);
            }
        }
    }
}
