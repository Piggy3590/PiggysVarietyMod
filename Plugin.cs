using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using PiggyVarietyMod.Patches;
using UnityEngine;
using System.IO;
using System.Reflection;
using LethalLib.Modules;

namespace PiggyVarietyMod
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "Piggy.PiggyVarietyMod";
        private const string modName = "PiggyVarietyMod";
        private const string modVersion = "1.3.20";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static Plugin Instance;

        public static ManualLogSource mls;
        public static AssetBundle Bundle;

        internal static PVInputActions InputActionInstance = new PVInputActions();

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

        public static AudioClip flashlightShake;
        public static AudioClip gummylightClick;
        public static AudioClip gummylightOutage;
        public static AudioClip flashFlicker;
        public static Material flashlightBulb;
        public static Material blackRubber;
        public static GameObject gummylightPrefab;
        public static GameObject m4Prefab;
        public static GameObject axePrefab;

        public static Item revolverItem;
        public static Item revolverAmmoItem;
        public static Item gummyFlashlight;
        public static Item arItem;
        public static Item arMagItem;
        public static Item axeItem;

        public static Item bulbItem;
        public static Item chemicalItem;

        public static int revolverRarity;
        public static int revolverAmmoRarity;
        public static int revolverMaxPlayerDamage;
        public static int revolverMaxMonsterDamage;
        public static int rifleMaxPlayerDamage;
        public static int rifleMonsterDamage;
        public static bool customGunInfinityAmmo;

        public static int revolverPrice;
        public static int revolverAmmoPrice;

        public static int riflePrice;
        public static int rifleMagPrice;
        public static int rifleRarity;
        public static int rifleMagRarity;
        public static bool twoHandedRifle;

        public static int bulbRarity;
        public static int chemicalRarity;

        //public static int gummyLightRarity;
        //public static int gummyLightPrice;

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

        public static AudioClip m4FireClip;
        public static AudioClip m4ReloadClip;
        public static AudioClip m4InspectClip;
        public static AudioClip m4TriggerClip;

        public static RuntimeAnimatorController playerAnimator;
        public static RuntimeAnimatorController otherPlayerAnimator;

        public static string PluginDirectory;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            PluginDirectory = Info.Location;

            LoadAssets();

            //harmony.PatchAll();
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("Piggy's Variety Mod is loaded");

            teslaSoundVolume = (float)Config.Bind<float>("Generic", "TeslaGateVolume", 1, "(Default 1) Sets the sound volume for Tesla Gate.").Value;
            teslaShake = (bool)Config.Bind<bool>("Generic", "TeslaGateShake", false, "(Experimental, Default false) Shake the screen when near Tesla Gate.").Value;
            revolverMaxPlayerDamage = (int)Config.Bind<int>("Generic", "RevolverMaxPlayerDamage", 70, "(Default 70) Sets the maximum amount of damage the Revolver can deals on the player.").Value;
            revolverMaxMonsterDamage = (int)Config.Bind<int>("Generic", "RevolverMaxMonsterDamage", 4, "(Default 4) Sets the maximum amount of damage the Revolver can deals on the monster.").Value;
            rifleMaxPlayerDamage = (int)Config.Bind<int>("Generic", "RifleMaxPlayerDamage", 22, "(Default 22) Sets the maximum amount of damage the Rifle can deals on the player.").Value;
            rifleMonsterDamage = (int)Config.Bind<int>("Generic", "RifleMonsterDamage", 1, "(Default 1) Sets the amount of damage the Rifle deals to monsters.").Value;
            customGunInfinityAmmo = (bool)Config.Bind<bool>("Generic", "CustomGunInfinityAmmo", false, "(Default false) If true, reloading custom guns will no longer require ammo.").Value;
            twoHandedRifle = (bool)Config.Bind<bool>("Generic", "TwoHandedRifle", false, "(Default false) If true, changes the rifle to a two-handed item.").Value;

            teslaSpawnWeight = (float)Config.Bind<float>("Spawn", "TeslaGateWeight", 1, "(Default 1) Sets the spawn weight for the Tesla Gate.").Value;

            revolverRarity = (int)Config.Bind<int>("Scrap", "RevolverRarity", 20, "(Default 20) Sets the spawn rarity for the Revolver.").Value;
            revolverAmmoRarity = (int)Config.Bind<int>("Scrap", "RevolverAmmoRarity", 60, "(Default 60) Sets the spawn rarity for the Revolver ammo.").Value;
            //gummyLightRarity = (int)base.Config.Bind<int>("Scrap", "GummylightAmmoRarity", 0, "(Default 0) Sets the spawn rarity for the Gummy flashlight.").Value;

            revolverPrice = (int)Config.Bind<int>("Store", "RevolverPrice", -1, "(Recommended -1 or 550) Set the price of the Revolver. If -1, removes the item from the store list.").Value;
            revolverAmmoPrice = (int)Config.Bind<int>("Store", "RevolverAmmoPrice", -1, "(Recommended -1 or 30) Set the price of the Revolver ammo. If -1, removes the item from the store list.").Value;
            //gummyLightPrice = (int)base.Config.Bind<int>("Store", "GummylightAmmoPrice", 30, "(Recommended 30) Set the price of the Gummy flashlight. If -1, removes the item from the store list.").Value;
            riflePrice = (int)Config.Bind<int>("Store", "RiflePrice", -1, "(Recommended -1 or 1,000~) Set the price of the Rifle (M4A1). If -1, removes the item from the store list.").Value;
            rifleMagPrice = (int)Config.Bind<int>("Store", "RifleMagPrice", -1, "(Recommended -1 or 400~) Set the price of the Rifle magazine. If -1, removes the item from the store list.").Value;
            rifleRarity = (int)Config.Bind<int>("Scrap", "RifleRarity", 20, "(Default 20) Sets the spawn rarity for the Rifle.").Value;
            rifleMagRarity = (int)Config.Bind<int>("Scrap", "RifleMagRarity", 60, "(Default 60) Sets the spawn rarity for the Rifle magazine.").Value;

            bulbRarity = (int)Config.Bind<int>("Scrap", "BulbRarity", 30, "(Default 30) Sets the spawn rarity for the Bulb.").Value;
            chemicalRarity = (int)Config.Bind<int>("Scrap", "ChemicalRarity", 30, "(Default 30) Sets the spawn rarity for the Chemical.").Value;

            translateKorean = (bool)Config.Bind<bool>("Translation", "Enable Korean", false, "Set language to Korean.").Value;

            NetworkPrefabs.RegisterNetworkPrefab(teslaGatePrefab);

            NetworkPrefabs.RegisterNetworkPrefab(revolverItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(revolverAmmoItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(gummyFlashlight.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(arItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(arMagItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(axeItem.spawnPrefab);

            NetworkPrefabs.RegisterNetworkPrefab(bulbItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(chemicalItem.spawnPrefab);
            Utilities.FixMixerGroups(revolverItem.spawnPrefab);
            Utilities.FixMixerGroups(revolverAmmoItem.spawnPrefab);
            Utilities.FixMixerGroups(gummyFlashlight.spawnPrefab);
            Utilities.FixMixerGroups(arItem.spawnPrefab);
            Utilities.FixMixerGroups(arMagItem.spawnPrefab);
            Utilities.FixMixerGroups(axeItem.spawnPrefab);

            Utilities.FixMixerGroups(bulbItem.spawnPrefab);
            Utilities.FixMixerGroups(chemicalItem.spawnPrefab);

            Items.RegisterItem(revolverItem);
            Items.RegisterItem(revolverAmmoItem);
            Items.RegisterItem(gummyFlashlight);
            Items.RegisterItem(arItem);
            Items.RegisterItem(arMagItem);
            Items.RegisterItem(axeItem);

            Items.RegisterItem(bulbItem);
            Items.RegisterItem(chemicalItem);

            if (translateKorean)
            { 
                Translate();
            }
            CreateShopItem();
            Items.RegisterScrap(revolverItem, revolverRarity, Levels.LevelTypes.All);
            Items.RegisterScrap(revolverAmmoItem, revolverAmmoRarity, Levels.LevelTypes.All);

            Items.RegisterScrap(arItem, rifleRarity, Levels.LevelTypes.All);
            Items.RegisterScrap(arMagItem, rifleMagRarity, Levels.LevelTypes.All);

            Items.RegisterScrap(bulbItem, bulbRarity, Levels.LevelTypes.All);
            Items.RegisterScrap(chemicalItem, chemicalRarity, Levels.LevelTypes.All);
            //LethalLib.Modules.Items.RegisterScrap(gummyFlashlight, gummyLightRarity, Levels.LevelTypes.All);

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
                Bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(PluginDirectory), "piggyvarietymod"));
            }
            catch (Exception ex)
            {
                mls.LogError("Couldn't load asset bundle: " + ex.Message);
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

                flashlightShake = Bundle.LoadAsset<AudioClip>("FlashlightShake.wav");
                gummylightClick = Bundle.LoadAsset<AudioClip>("GummyFlashlightClick.wav");
                gummylightOutage = Bundle.LoadAsset<AudioClip>("GummylightBatteryOutage.wav");
                flashFlicker = Bundle.LoadAsset<AudioClip>("FlashlightFlicker.ogg");
                flashlightBulb = Bundle.LoadAsset<Material>("FlashlightBulb1.mat");
                blackRubber = Bundle.LoadAsset<Material>("BlackRubber1.mat");

                revolverItem = Bundle.LoadAsset<Item>("Revolver.asset");
                revolverAmmoItem = Bundle.LoadAsset<Item>("RevolverAmmo.asset");
                arItem = Bundle.LoadAsset<Item>("M4A1.asset");
                arMagItem = Bundle.LoadAsset<Item>("Magazine.asset");
                axeItem = Bundle.LoadAsset<Item>("Axe.asset");
                gummyFlashlight = Bundle.LoadAsset<Item>("GummyFlashlight.asset");

                chemicalItem = Bundle.LoadAsset<Item>("Chemical.asset");
                bulbItem = Bundle.LoadAsset<Item>("Bulb.asset");

                gummylightPrefab = Bundle.LoadAsset<GameObject>("GummylightItem.prefab");
                m4Prefab = Bundle.LoadAsset<GameObject>("M4Item.prefab");
                axePrefab = Bundle.LoadAsset<GameObject>("AxeItem.prefab");

                revolverAmmoInsert = Bundle.LoadAsset<AudioClip>("RevolverReload.wav");
                revolverCylinderOpen = Bundle.LoadAsset<AudioClip>("RevolverCylinderOpen.wav");
                revolverCylinderClose = Bundle.LoadAsset<AudioClip>("RevolverCylinderClose.wav");

                revolverDryFire = Bundle.LoadAsset<AudioClip>("RevolverDryFire.wav");

                revolverBlast1 = Bundle.LoadAsset<AudioClip>("RevolverBlast1.wav");
                revolverBlast2 = Bundle.LoadAsset<AudioClip>("RevolverBlast2.wav");

                m4FireClip = Bundle.LoadAsset<AudioClip>("M4Fire1.wav");
                m4InspectClip = Bundle.LoadAsset<AudioClip>("InspectM4v2.wav");
                m4TriggerClip = Bundle.LoadAsset<AudioClip>("M4Trigger.wav");
                m4ReloadClip = Bundle.LoadAsset<AudioClip>("M4Reload.wav");

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

                arItem.spawnPrefab = m4Prefab;
                M4Item m4Item = arItem.spawnPrefab.AddComponent<M4Item>();
                m4Item.grabbable = true;
                m4Item.isInFactory = true;
                m4Item.itemProperties = arItem;
                m4Item.grabbableToEnemies = true;
                m4Item.gunCompatibleAmmoID = 485;
                m4Item.gunAnimator = m4Item.gameObject.GetComponent<Animator>();
                m4Item.gunAudio = m4Item.gameObject.GetComponent<AudioSource>();
                m4Item.gunShootAudio = m4Item.transform.GetChild(0).GetComponent<AudioSource>();
                m4Item.gunBulletsRicochetAudio = m4Item.transform.GetChild(1).GetComponent<AudioSource>();
                m4Item.gunShootSFX = m4FireClip;
                m4Item.gunReloadSFX = m4ReloadClip;
                m4Item.gunInspectSFX = m4InspectClip;
                m4Item.noAmmoSFX = m4TriggerClip;
                m4Item.gunShootParticle = m4Item.transform.GetChild(5).GetChild(0).GetComponent<ParticleSystem>();
                m4Item.gunRayPoint = m4Item.transform.GetChild(5);

                if (twoHandedRifle)
                {
                    arItem.twoHanded = true;
                }
                gummyFlashlight.spawnPrefab = gummylightPrefab;

                GummylightItem gummylightItem = gummyFlashlight.spawnPrefab.AddComponent<GummylightItem>();
                gummylightItem.useCooldown = 0.12f;
                gummylightItem.itemProperties = gummyFlashlight;
                gummylightItem.mainObjectRenderer = gummylightItem.transform.GetChild(2).GetComponent<MeshRenderer>();
                gummylightItem.insertedBattery = new Battery(false , 1);
                gummylightItem.insertedBattery.charge = 1;
                gummylightItem.grabbableToEnemies = true;
                gummylightItem.flashlightBulb = gummylightItem.transform.GetChild(0).GetComponent<Light>();
                gummylightItem.flashlightBulbGlow = gummylightItem.transform.GetChild(1).GetComponent<Light>();
                gummylightItem.flashlightAudio = gummylightItem.GetComponent<AudioSource>();
                gummylightItem.flashlightClips = new AudioClip[] { gummylightClick };
                gummylightItem.outOfBatteriesClip = gummylightOutage;
                gummylightItem.flashlightFlicker = flashFlicker;
                gummylightItem.bulbLight = flashlightBulb;
                gummylightItem.bulbDark = blackRubber;
                gummylightItem.flashlightMesh = gummylightItem.transform.GetChild(2).GetComponent<MeshRenderer>();
                gummylightItem.changeMaterial = true;
                gummylightItem.isInFactory = true;
                gummylightItem.grabbable = true;
                gummylightItem.flashlightTypeID = 10;
                Destroy(gummyFlashlight.spawnPrefab.GetComponent<FlashlightItem>());

                axeItem.spawnPrefab = axePrefab;
                AxeItem axeScript = axeItem.spawnPrefab.AddComponent<AxeItem>();
                Shovel shovelScript = axeItem.spawnPrefab.GetComponent<Shovel>();
                axeScript.itemProperties = axeItem;
                axeScript.grabbable = true;
                axeScript.isInFactory = true;
                axeScript.grabbableToEnemies = true;
                axeScript.shovelHitForce = 1;
                axeScript.reelUp = shovelScript.reelUp;
                axeScript.swing = shovelScript.swing;
                axeScript.hitSFX = shovelScript.hitSFX;
                axeScript.shovelAudio = shovelScript.shovelAudio;
                Destroy(shovelScript);

                Logger.LogInfo("Successfully loaded assets!");
            }
            catch (Exception ex2)
            {
                Logger.LogError("Couldn't load assets: " + ex2.Message);
            }
        }

        void Translate()
        {
            revolverItem.toolTips[0] = "격발 : [RMB]";
            revolverItem.toolTips[1] = "탄약 삽탄하기 : [E]";
            revolverItem.toolTips[2] = "실린더 열기 : [Q]";

            gummyFlashlight.toolTips[0] = "전등 전환하기 : [RMB]";
            gummyFlashlight.toolTips[1] = "손전등 흔들기 : [Q]";

            arItem.toolTips[0] = "격발 : [RMB]";
            arItem.toolTips[1] = "재장전 : [E]";
            arItem.toolTips[2] = "탄약 확인하기 : [Q]";

            axeItem.toolTips[0] = "도끼 휘두르기 : [RMB]";

            revolverItem.spawnPrefab.GetComponentInChildren<ScanNodeProperties>().headerText = "리볼버";
            revolverAmmoItem.spawnPrefab.GetComponentInChildren<ScanNodeProperties>().headerText = "총알";
            arItem.spawnPrefab.GetComponentInChildren<ScanNodeProperties>().headerText = "소총";
            arMagItem.spawnPrefab.GetComponentInChildren<ScanNodeProperties>().headerText = "탄창";
            bulbItem.spawnPrefab.GetComponentInChildren<ScanNodeProperties>().headerText = "전구";
            chemicalItem.spawnPrefab.GetComponentInChildren<ScanNodeProperties>().headerText = "화학 약품";
        }

        void CreateShopItem()
        {
            if (translateKorean)
            {
                revolverAmmoItem.itemName = "총알";
                revolverItem.itemName = "리볼버";
                gummyFlashlight.itemName = "젤리";
                arMagItem.itemName = "탄창";
                arItem.itemName = "소총";
                axeItem.itemName = "도끼";

                chemicalItem.itemName = "화학 약품";
                bulbItem.itemName = "전구";
            }
            else
            {
                revolverAmmoItem.itemName = "Bullet";
                revolverItem.itemName = "Revolver";
                gummyFlashlight.itemName = "Gummy flashlight";
                arMagItem.itemName = "Magazine";
                arItem.itemName = "Rifle";
                axeItem.itemName = "Axe";
            }

            //Revolver

            TerminalNode revolverItemShopNode = NewTerminalNode(
                "리볼버를 주문하려고 합니다. 수량: [variableAmount]. \r\n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력하세요.\n\n",
                "You have requested to order revolvers. Amount: [variableAmount]. \r\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\n\n");

            TerminalNode revolverItemShopNode2 = NewTerminalNode(
                "[variableAmount]개의 리볼버를 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n",
                "Ordered [variableAmount] revolvers. Your new balance is [playerCredits].\n\nOur contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\n\n");

            TerminalNode revolverItemShopInfo = NewTerminalNode(
                "\n더욱 강력한 자기 보호를 위해!\n실린더를 열고 리볼버 탄약을 삽탄하여 장전하세요.\n\n",
                "\nFor more powerful self-defense!\nOpen the cylinder and insert revolver ammo to load it.\n\n");

            TerminalNode revolverAmmoShopNode = NewTerminalNode(
                "리볼버 탄약을 주문하려고 합니다. 수량: [variableAmount]. \r\n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력하세요.\n\n",
                "You have requested to order revolver ammos. Amount: [variableAmount]. \r\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\n\n");

            TerminalNode revolverAmmoShopNode2 = NewTerminalNode(
                "[variableAmount]개의 리볼버 탄약을 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n",
                "Ordered [variableAmount] revolver ammos. Your new balance is [playerCredits].\n\nOur contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\n\n");

            TerminalNode revolverAmmoShopInfo = NewTerminalNode(
                "\n리볼버에 장전하고 <b>치명적인</b> 순간에 격발하세요!\n\n",
                "\nLoad to your revolver and fire at LETHAL moments!\n\n");

            //Rifle

            TerminalNode rifleItemShopNode = NewTerminalNode(
                "소총을 주문하려고 합니다. 수량: [variableAmount]. \r\n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력하세요.\n\n",
                "You have requested to order rifles. Amount: [variableAmount]. \r\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\n\n");

            TerminalNode rifleItemShopNode2 = NewTerminalNode(
                "[variableAmount]개의 소총을 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n",
                "Ordered [variableAmount] rifles. Your new balance is [playerCredits].\n\nOur contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\n\n");

            TerminalNode rifleItemShopInfo = NewTerminalNode(
                "\n더욱 강력한 자기 보호를 위해!\n탄창을 장전하여 사용하세요.\n\n",
                "\nFor more powerful self-defense!\nload magazine to fire it.\n\n");

            TerminalNode rifleMagShopNode = NewTerminalNode(
                "소총 탄창을 주문하려고 합니다. 수량: [variableAmount]. \r\n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력하세요.\n\n",
                "You have requested to order rifle magazines. Amount: [variableAmount]. \r\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\n\n");

            TerminalNode rifleMagShopNode2 = NewTerminalNode(
                "[variableAmount]개의 소총 탄창을 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n",
                "Ordered [variableAmount] rifle magazines. Your new balance is [playerCredits].\n\nOur contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\n\n");

            TerminalNode rifleMagShopInfo = NewTerminalNode(
                "\n소총에 장전하고 <b>치명적인</b> 순간에 격발하세요!\n\n",
                "\nLoad to your rifle and fire at LETHAL moments!\n\n");

            //GummyFlashlight

            TerminalNode gummylightShopNode = NewTerminalNode(
                "젤리 손전등을 주문하려고 합니다. 수량: [variableAmount]. \r\n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력하세요.\n\n",
                "You have requested to order gummy flashlights. Amount: [variableAmount]. \r\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\n\n");

            TerminalNode gummylightShopNode2 = NewTerminalNode(
                "[variableAmount]개의 젤리 손전등을 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n",
                "Ordered [variableAmount] gummy flashlights. Your new balance is [playerCredits].\n\nOur contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\n\n");

            TerminalNode gummylightShopInfo = NewTerminalNode(
                "\n자가발전 손전등입니다.\n그저 평범한 장난감이지만, 배터리가 다 떨어졌을 때 여러분의 어두운 앞길을 비춰 줄 것입니다!\n\n",
                "\nA self-powered flashlight.\nIt's just a toy, but it'll light up your dark path when the batteries run out!\n\n");

            if (revolverPrice > -1)
            {
                Items.RegisterShopItem(revolverItem, revolverItemShopNode, revolverItemShopNode2, revolverItemShopInfo, revolverPrice);
            }
            if (revolverAmmoPrice > -1)
            {
                Items.RegisterShopItem(revolverAmmoItem, revolverAmmoShopNode, revolverAmmoShopNode2, revolverAmmoShopInfo, revolverAmmoPrice);
            }
            /*
            if (gummyLightPrice > -1)
            {
                LethalLib.Modules.Items.RegisterShopItem(gummyFlashlight, gummylightShopNode, gummylightShopNode2, gummylightShopInfo, gummyLightPrice);
            }
            */
            if (riflePrice > -1)
            {
                Items.RegisterShopItem(arItem, rifleItemShopNode, rifleItemShopNode2, rifleItemShopInfo, riflePrice);
            }
            if (rifleMagPrice > -1)
            {
                Items.RegisterShopItem(arMagItem, rifleMagShopNode, rifleMagShopNode2, rifleMagShopInfo, rifleMagPrice);
            }
        }

        public TerminalNode NewTerminalNode(string korean, string english)
        {
            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            if (translateKorean)
            {
                node.displayText = korean;
            }
            else
            {
                node.displayText = english;
            }
            node.clearPreviousText = true;
            node.maxCharactersToType = 15;
            node.buyRerouteToMoon = -1;
            node.displayPlanetInfo = -1;
            node.shipUnlockableID = -1;
            node.creatureFileID = -1;
            node.storyLogFileID = -1;
            return node;
        }
    }
}
