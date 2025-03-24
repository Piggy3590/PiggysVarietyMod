using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace PiggyVarietyMod.Patches
{
    public class GummylightItem : GrabbableObject
    {
        [Space(15f)]
        public bool usingPlayerHelmetLight;

        public int flashlightInterferenceLevel;

        public static int globalFlashlightInterferenceLevel;

        public Light flashlightBulb;

        public Light flashlightBulbGlow;

        public AudioSource flashlightAudio;

        public AudioClip[] flashlightClips;

        public AudioClip outOfBatteriesClip;

        public AudioClip flashlightFlicker;

        public Material bulbLight;

        public Material bulbDark;

        public MeshRenderer flashlightMesh;

        public int flashlightTypeID;

        public bool changeMaterial = true;

        private float initialIntensity;

        private PlayerControllerB previousPlayerHeldBy;

        public override void Start()
        {
            useCooldown = 0.12f;
            itemProperties = Plugin.gummyFlashlight;
            mainObjectRenderer = transform.GetChild(2).GetComponent<MeshRenderer>();
            insertedBattery.charge = 1;
            grabbableToEnemies = true;
            flashlightBulb = transform.GetChild(0).GetComponent<Light>();
            flashlightBulbGlow = transform.GetChild(1).GetComponent<Light>();
            flashlightAudio = GetComponent<AudioSource>();
            flashlightClips = new AudioClip[] { Plugin.gummylightClick };
            outOfBatteriesClip = Plugin.gummylightOutage;
            flashlightFlicker = Plugin.flashFlicker;
            bulbLight = Plugin.flashlightBulb;
            bulbDark = Plugin.blackRubber;
            flashlightMesh = transform.GetChild(2).GetComponent<MeshRenderer>();  
            changeMaterial = true;
            isInFactory = true;
            grabbable = true;
            flashlightTypeID = 0;
            itemProperties.batteryUsage = 60;
            Destroy(GetComponent<FlashlightItem>());

            base.Start();
            initialIntensity = flashlightBulb.intensity;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (flashlightInterferenceLevel < 2)
            {
                SwitchFlashlight(used);
            }
            flashlightAudio.PlayOneShot(flashlightClips[Random.Range(0, flashlightClips.Length)]);
            RoundManager.Instance.PlayAudibleNoise(transform.position, 7f, 0.4f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
        }

        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            SwitchFlashlight(on: false);
            flashlightAudio.PlayOneShot(outOfBatteriesClip, 1f);
            RoundManager.Instance.PlayAudibleNoise(transform.position, 13f, 0.65f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
        }

        public override void PocketItem()
        {
            previousPlayerHeldBy.equippedUsableItemQE = false;
            if (!IsOwner)
            {
                base.PocketItem();
                return;
            }
            if (previousPlayerHeldBy != null)
            {
                flashlightBulb.enabled = false;
                flashlightBulbGlow.enabled = false;
                if (isBeingUsed && (previousPlayerHeldBy.ItemSlots[previousPlayerHeldBy.currentItemSlot] == null || previousPlayerHeldBy.ItemSlots[previousPlayerHeldBy.currentItemSlot].itemProperties.itemId != 1 || previousPlayerHeldBy.ItemSlots[previousPlayerHeldBy.currentItemSlot].itemProperties.itemId != 6))
                {
                    previousPlayerHeldBy.helmetLight.enabled = true;
                    previousPlayerHeldBy.pocketedFlashlight = this;
                    usingPlayerHelmetLight = true;
                    PocketFlashlightServerRpc(stillUsingFlashlight: true);
                }
                else
                {
                    isBeingUsed = false;
                    usingPlayerHelmetLight = false;
                    flashlightBulbGlow.enabled = false;
                    SwitchFlashlight(on: false);
                    PocketFlashlightServerRpc();
                }
            }
            else
            {
                Debug.Log("Could not find what player was holding this flashlight item");
            }

            if (IsOwner && playerHeldBy != null)
            {
                playerHeldBy.equippedUsableItemQE = false;
            }
            base.PocketItem();
        }

        [ServerRpc]
        public void PocketFlashlightServerRpc(bool stillUsingFlashlight = false)
        {
            {
                PocketFlashlightClientRpc(stillUsingFlashlight);
            }
        }
        [ClientRpc]
        public void PocketFlashlightClientRpc(bool stillUsingFlashlight)
        {
            if (IsOwner)
            {
                return;
            }
            flashlightBulb.enabled = false;
            flashlightBulbGlow.enabled = false;
            if (stillUsingFlashlight)
            {
                if (!(previousPlayerHeldBy == null))
                {
                    previousPlayerHeldBy.helmetLight.enabled = true;
                    previousPlayerHeldBy.pocketedFlashlight = this;
                    usingPlayerHelmetLight = true;
                }
            }
            else
            {
                isBeingUsed = false;
                usingPlayerHelmetLight = false;
                flashlightBulbGlow.enabled = false;
                SwitchFlashlight(on: false);
            }
        }
        public override void DiscardItem()
        {
            if (playerHeldBy != null)
            {
                playerHeldBy.equippedUsableItemQE = false;
            }
            if (previousPlayerHeldBy != null)
            {
                previousPlayerHeldBy.helmetLight.enabled = false;
                flashlightBulb.enabled = isBeingUsed;
                flashlightBulbGlow.enabled = isBeingUsed;
            }
            base.DiscardItem();
        }

        public override void EquipItem()
        {
            previousPlayerHeldBy = playerHeldBy;
            playerHeldBy.equippedUsableItemQE = true;
            playerHeldBy.ChangeHelmetLight(flashlightTypeID);
            playerHeldBy.helmetLight.enabled = false;
            usingPlayerHelmetLight = false;
            if (isBeingUsed)
            {
                SwitchFlashlight(on: true);
            }
            base.EquipItem();
        }

        public void SwitchFlashlight(bool on)
        {
            isBeingUsed = on;
            if (!IsOwner)
            {
                Debug.Log($"Flashlight click. playerheldby null?: {playerHeldBy != null}");
                Debug.Log($"Flashlight being disabled or enabled: {on}");
                if (playerHeldBy != null)
                {
                    playerHeldBy.ChangeHelmetLight(flashlightTypeID, on);
                }
                flashlightBulb.enabled = false;
                flashlightBulbGlow.enabled = false;
            }
            else
            {
                flashlightBulb.enabled = on;
                flashlightBulbGlow.enabled = on;
            }
            if (usingPlayerHelmetLight && playerHeldBy != null)
            {
                playerHeldBy.helmetLight.enabled = on;
            }
            if (changeMaterial)
            {
                Material[] sharedMaterials = flashlightMesh.sharedMaterials;
                if (on)
                {
                    sharedMaterials[1] = bulbLight;
                }
                else
                {
                    sharedMaterials[1] = bulbDark;
                }
                flashlightMesh.sharedMaterials = sharedMaterials;
            }
        }

        public override void Update()
        {
            if (currentUseCooldown >= 0f)
            {
                currentUseCooldown -= Time.deltaTime;
            }
            if (IsOwner)
            {
                if (isBeingUsed && itemProperties.requiresBattery || flashlightBulb.enabled)
                {
                    isBeingUsed = true;
                    if (insertedBattery.charge > 0f)
                    {
                        if (!itemProperties.itemIsTrigger)
                        {
                            insertedBattery.charge -= Time.deltaTime / itemProperties.batteryUsage;
                        }
                    }
                    else if (!insertedBattery.empty)
                    {
                        insertedBattery.empty = true;
                        if (isBeingUsed)
                        {
                            Debug.Log("Use up batteries local");
                            isBeingUsed = false;
                            UseUpBatteries();
                            isSendingItemRPC++;
                            UseUpItemBatteriesServerRpc();
                        }
                    }
                }
                if (!wasOwnerLastFrame)
                {
                    wasOwnerLastFrame = true;
                }
            }
            else if (wasOwnerLastFrame)
            {
                wasOwnerLastFrame = false;
            }
            if (!isHeld && parentObject == null)
            {
                if (fallTime >= 1f)
                {
                    if (!reachedFloorTarget)
                    {
                        if (!hasHitGround)
                        {
                            PlayDropSFX();
                            OnHitGround();
                        }
                        reachedFloorTarget = true;
                        if (floorYRot == -1)
                        {
                            transform.rotation = Quaternion.Euler(itemProperties.restingRotation.x, transform.eulerAngles.y, itemProperties.restingRotation.z);
                        }
                        else
                        {
                            transform.rotation = Quaternion.Euler(itemProperties.restingRotation.x, (float)(floorYRot + itemProperties.floorYOffset) + 90f, itemProperties.restingRotation.z);
                        }
                    }
                    transform.localPosition = targetFloorPosition;
                    return;
                }
                reachedFloorTarget = false;
                FallWithCurve();
                if (transform.localPosition.y - targetFloorPosition.y < 0.05f && !hasHitGround)
                {
                    PlayDropSFX();
                    OnHitGround();
                    return;
                }
            }
            else if (isHeld || isHeldByEnemy)
            {
                reachedFloorTarget = false;
            }

            int flashlightIntensity = ((flashlightInterferenceLevel <= globalFlashlightInterferenceLevel) ? globalFlashlightInterferenceLevel : flashlightInterferenceLevel);
            if (flashlightIntensity >= 2)
            {
                flashlightBulb.intensity = 0f;
            }
            else if (flashlightIntensity == 1)
            {
                flashlightBulb.intensity = Random.Range(0f, 200f);
            }
            else
            {
                flashlightBulb.intensity = initialIntensity;
            }
        }

        public override void ItemInteractLeftRight(bool right)
        {
            Debug.Log($"r/l activate: {right}");
            if (!right && playerHeldBy != null)
            {
                flashlightAudio.PlayOneShot(Plugin.flashlightShake);
                WalkieTalkie.TransmitOneShotAudio(flashlightAudio, Plugin.flashlightShake, 1f);
                playerHeldBy.playerBodyAnimator.SetTrigger("shakeItem");
                float newBattery = (insertedBattery.charge * 100) + 8;
                if (newBattery <= 100)
                {
                    SyncBatteryServerRpc((int)newBattery);
                }
                else
                {
                    SyncBatteryServerRpc(100);
                }
                if (IsOwner)
                {
                    RoundManager.Instance.PlayAudibleNoise(transform.position, 5f, 0.2f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed, 941);
                }
                isBeingUsed = false;
            }
        }
    }
}
