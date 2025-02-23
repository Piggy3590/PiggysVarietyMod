using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace PiggyVarietyMod.Patches
{
    public class RevolverItem : GrabbableObject
    {
        private bool isCrouching;
        private bool isJumping;
        private bool isWalking;
        private bool isSprinting;
        private AnimatorStateInfo currentStateInfo;
        private float currentAnimationTime;

        public int gunCompatibleAmmoID = 1410;

        public bool isReloading;

        public bool cantFire;

        public Transform cylinderTransform;

        public bool isCylinderMoving;

        public int ammosLoaded;

        public Animator gunAnimator;

        public AudioSource gunAudio;

        public AudioSource gunShootAudio;

        public AudioSource gunBulletsRicochetAudio;

        public List<AudioClip> gunShootSFX = new List<AudioClip>();

        public AudioClip gunReloadSFX;

        public AudioClip cylinderOpenSFX;
        public AudioClip cylinderCloseSFX;

        public AudioClip gunReloadFinishSFX;

        public AudioClip noAmmoSFX;

        public AudioClip gunSafetySFX;

        public AudioClip switchSafetyOnSFX;

        public AudioClip switchSafetyOffSFX;

        private int ammoSlotToUse = -1;

        private PlayerControllerB previousPlayerHeldBy;

        public ParticleSystem gunShootParticle;

        public Transform revolverRayPoint;

        public List<MeshRenderer> revolverAmmos = new List<MeshRenderer>();

        public MeshRenderer revolverAmmoInHand;

        public Transform revolverAmmoInHandTransform;

        private RaycastHit[] enemyColliders;
        private RaycastHit[] playerColliders;

        private EnemyAI heldByEnemy;

        private static RuntimeAnimatorController originalPlayerAnimator;

        public override void Start()
        {
            base.Start();
        }

        public override int GetItemDataToSave()
        {
            base.GetItemDataToSave();
            return ammosLoaded;
        }

        public override void LoadItemSaveData(int saveData)
        {
            base.LoadItemSaveData(saveData);
            ammosLoaded = saveData;
        }

        public override void Update()
        {
            base.Update();
            if (!isReloading)
            {
                switch (ammosLoaded)
                {
                    case 0:
                        cylinderTransform.localRotation = Quaternion.Lerp(cylinderTransform.localRotation, Quaternion.Euler(new Vector3(0, 0, 0)), Time.deltaTime * 25);
                        break;
                    case 1:
                        cylinderTransform.localRotation = Quaternion.Lerp(cylinderTransform.localRotation, Quaternion.Euler(new Vector3(0, 60, 0)), Time.deltaTime * 25);
                        break;
                    case 2:
                        cylinderTransform.localRotation = Quaternion.Lerp(cylinderTransform.localRotation, Quaternion.Euler(new Vector3(0, 120, 0)), Time.deltaTime * 25);
                        break;
                    case 3:
                        cylinderTransform.localRotation = Quaternion.Lerp(cylinderTransform.localRotation, Quaternion.Euler(new Vector3(0, 180, 0)), Time.deltaTime * 25);
                        break;
                    case 4:
                        cylinderTransform.localRotation = Quaternion.Lerp(cylinderTransform.localRotation, Quaternion.Euler(new Vector3(0, 240, 0)), Time.deltaTime * 25);
                        break;
                    case 5:
                        cylinderTransform.localRotation = Quaternion.Lerp(cylinderTransform.localRotation, Quaternion.Euler(new Vector3(0, 300, 0)), Time.deltaTime * 25);
                        break;
                    case 6:
                        cylinderTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        break;
                }
            }
        }

        public override void EquipItem()
        {
            base.EquipItem();
            //SyncRevolverAmmoServerRpc(ammosLoaded);
            if (playerHeldBy != null)
            {
                UpdateAnimator(playerHeldBy, playerHeldBy.playerBodyAnimator, false);
            }
            playerHeldBy.playerBodyAnimator.SetBool("ReloadRevolver", false);
            gunAnimator.SetBool("Reloading", false);
            revolverAmmoInHand.enabled = false;
            previousPlayerHeldBy = playerHeldBy;
            previousPlayerHeldBy.equippedUsableItemQE = true;
            isCylinderMoving = false;
            foreach (MeshRenderer ammo in revolverAmmos)
            {
                ammo.enabled = false;
            }
            if (ammosLoaded > 0)
            {
                for (int i = 0; i <= ammosLoaded - 1; i++)
                {
                    revolverAmmos[i].enabled = true;
                }
            }
        }
        public override void GrabItem()
        {
            if (playerHeldBy != null)
            {
                UpdateAnimator(playerHeldBy, playerHeldBy.playerBodyAnimator, false);
            }
            base.GrabItem();
        }

        public override void GrabItemFromEnemy(EnemyAI enemy)
        {
            base.GrabItemFromEnemy(enemy);
            heldByEnemy = enemy;
        }

        public override void DiscardItemFromEnemy()
        {
            base.DiscardItemFromEnemy();
            heldByEnemy = null;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            //SyncRevolverAmmoServerRpc(ammosLoaded);
            base.ItemActivate(used, buttonDown);
            if (!isReloading && !cantFire && !gunAnimator.GetBool("Reloading"))
            {
                if (ammosLoaded > 0)
                {
                    gunAnimator.SetBool("Reloading", false);
                    ShootGunAndSync(true);
                }
                else
                {
                    StartCoroutine(FireDelay());
                    gunAnimator.SetTrigger("Fire");
                    gunAudio.PlayOneShot(noAmmoSFX);
                }
            }
        }

        public void ShootGunAndSync(bool heldByPlayer)
        {
            Vector3 revolverPosition;
            Vector3 forward;
            if (!heldByPlayer)
            {
                revolverPosition = revolverRayPoint.position;
                forward = revolverRayPoint.forward;
            }
            else
            {
                revolverPosition = playerHeldBy.gameplayCamera.transform.position - playerHeldBy.gameplayCamera.transform.up * 0.45f;
                forward = playerHeldBy.gameplayCamera.transform.forward;
            }
            ShootGun(revolverPosition, forward);
        }
        public IEnumerator FireDelay()
        {
            cantFire = true;
            yield return new WaitForSeconds(0.2f);
            cantFire = false;
        }

        public void ShootGun(Vector3 revolverPosition, Vector3 revolverForward)
        {
            CentipedeAI[] array = FindObjectsByType<CentipedeAI>(FindObjectsSortMode.None);
            FlowerSnakeEnemy[] array2 = FindObjectsByType<FlowerSnakeEnemy>(FindObjectsSortMode.None);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].clingingToPlayer == playerHeldBy)
                {
                    array[i].HitEnemy(5, playerHeldBy, true, -1);
                }
            }
            for (int i = 0; i < array2.Length; i++)
            {
                if (array2[i].clingingToPlayer == playerHeldBy)
                {
                    array2[i].HitEnemy(5, playerHeldBy, true, -1);
                }
            }
            StartCoroutine(FireDelay());
            if (isHeld && playerHeldBy != null && playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                playerHeldBy.playerBodyAnimator.SetTrigger("ShootRevolver");
            }
            gunAnimator.SetTrigger("Fire");
            RoundManager.PlayRandomClip(gunShootAudio, gunShootSFX.ToArray(), randomize: true, 1f, 1840);
            WalkieTalkie.TransmitOneShotAudio(gunShootAudio, gunShootSFX[0]);
            gunShootParticle.Play(withChildren: true);
            ammosLoaded = Mathf.Clamp(ammosLoaded - 1, 0, 6);
            foreach (MeshRenderer ammo in revolverAmmos)
            {
                ammo.enabled = false;
            }
            if (ammosLoaded > 0)
            {
                for (int i = 0; i <= ammosLoaded - 1; i++)
                {
                    revolverAmmos[i].enabled = true;
                }
            }
            PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
            if (localPlayerController == null)
            {
                return;
            }
            float gunPlayerDistance = Vector3.Distance(localPlayerController.transform.position, revolverRayPoint.transform.position);
            Vector3 vector = localPlayerController.playerCollider.ClosestPoint(revolverPosition);
            float gunAudioDelay = 0f;
            if (gunPlayerDistance < 5f)
            {
                gunAudioDelay = 0.25f;
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            }
            if (gunPlayerDistance < 15f)
            {
                gunAudioDelay = 0.15f;
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
            }
            if (gunAudioDelay > 0f && SoundManager.Instance.timeSinceEarsStartedRinging > 16f)
            {
                StartCoroutine(delayedEarsRinging(gunAudioDelay));
            }

            Ray ray = new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward);
            RaycastHit hit;

            if (enemyColliders == null)
            {
                enemyColliders = new RaycastHit[35];
            }

            if (Physics.Raycast(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward, out hit, Mathf.Infinity, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                gunBulletsRicochetAudio.transform.position = ray.GetPoint(hit.distance - 0.5f);
                gunBulletsRicochetAudio.Play();
            }

            if (playerHeldBy == StartOfRound.Instance.localPlayerController)
            {
                int enemyHitCount = Physics.SphereCastNonAlloc(ray, 0.25f, enemyColliders, Mathf.Infinity, 524288, QueryTriggerInteraction.Collide);

                for (int i = 0; i < enemyHitCount; i++)
                {
                    Debug.Log("Raycasting enemy");
                    if (enemyColliders[i].transform.GetComponent<EnemyAICollisionDetect>())
                    {
                        EnemyAI mainScript = enemyColliders[i].transform.GetComponent<EnemyAICollisionDetect>().mainScript;
                        if (heldByEnemy != null && heldByEnemy == mainScript)
                        {
                            Debug.Log("Shotgun is held by enemy, skipping enemy raycast");
                        }
                        else
                        {
                            Debug.Log("Hit enemy " + mainScript.enemyType.enemyName);
                            IHittable hittable;
                            if (enemyColliders[i].distance == 0f)
                            {
                                Debug.Log("Spherecast started inside enemy collider");
                            }
                            else if (Physics.Linecast(playerHeldBy.gameplayCamera.transform.position, enemyColliders[i].point, out hit, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                            {
                                Debug.DrawRay(hit.point, Vector3.up, Color.red, 15f);
                                Debug.DrawLine(playerHeldBy.gameplayCamera.transform.position, enemyColliders[i].point, Color.cyan, 15f);
                                Plugin.mls.LogInfo("Raycast hit wall: " + hit.collider.gameObject.name + ", distance: " + hit.distance);
                            }
                            else if (enemyColliders[i].transform.TryGetComponent<IHittable>(out hittable))
                            {
                                Vector3 hitDirection = playerHeldBy.gameplayCamera.transform.forward;
                                float distance = Vector3.Distance(playerHeldBy.gameplayCamera.transform.position, enemyColliders[i].point);
                                int damage;
                                if (distance < 3f)
                                {
                                    damage = Plugin.revolverMaxMonsterDamage;
                                }
                                else if (distance < 12f)
                                {
                                    damage = Mathf.RoundToInt(Plugin.revolverMaxMonsterDamage / 2);
                                }
                                else if (distance < 26f)
                                {
                                    damage = Mathf.RoundToInt(Plugin.revolverMaxMonsterDamage / 3);
                                }
                                else
                                {
                                    damage = Mathf.RoundToInt(Plugin.revolverMaxMonsterDamage / 4);
                                }

                                Plugin.mls.LogInfo("Damage to enemy, damage: " + damage + ", distance:" + distance);
                                hittable.Hit(damage, hitDirection, playerHeldBy, true, -1);
                            }
                            else
                            {
                                Plugin.mls.LogInfo("Could not get hittable script from collider, transform: " + enemyColliders[i].transform.name);
                            }
                        }
                    }
                }
            }

            if (playerColliders == null)
            {
                playerColliders = new RaycastHit[10];
            }

            int playerCheck = Physics.SphereCastNonAlloc(ray, 0.3f, playerColliders, Mathf.Infinity, StartOfRound.Instance.collidersRoomMaskDefaultAndPlayers, QueryTriggerInteraction.Collide);

            for (int i = 0; i < playerCheck; i++)
            {
                if (playerColliders[i].transform.GetComponent<PlayerControllerB>() != null && playerColliders[i].transform.GetComponent<PlayerControllerB>() != playerHeldBy)
                {
                    float distance = Vector3.Distance(playerHeldBy.gameplayCamera.transform.position, playerColliders[i].point);
                    if (distance < 10)
                    {
                        playerColliders[i].transform.GetComponent<PlayerControllerB>().DamagePlayer(Plugin.revolverMaxPlayerDamage, true, true, CauseOfDeath.Gunshots, 0, false, playerHeldBy.gameplayCamera.transform.forward * 30f);
                    }
                    else if (distance < 25)
                    {
                        playerColliders[i].transform.GetComponent<PlayerControllerB>().DamagePlayer(Mathf.RoundToInt(Plugin.revolverMaxPlayerDamage - (Plugin.revolverMaxPlayerDamage / 3)), true, true, CauseOfDeath.Gunshots, 0, false, playerHeldBy.gameplayCamera.transform.forward * 30f);
                    }
                    else
                    {
                        playerColliders[i].transform.GetComponent<PlayerControllerB>().DamagePlayer(Mathf.RoundToInt(Plugin.revolverMaxPlayerDamage / 3), true, true, CauseOfDeath.Gunshots, 0, false, playerHeldBy.gameplayCamera.transform.forward * 30f);
                    }
                    Debug.Log("Revolver vs Player wtf lmao");
                }
            }
        }

        private IEnumerator delayedEarsRinging(float effectSeverity)
        {
            yield return new WaitForSeconds(0.25f);
            SoundManager.Instance.earsRingingTimer = effectSeverity;
        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);
            foreach (MeshRenderer ammo in revolverAmmos)
            {
                ammo.enabled = false;
            }
            if (ammosLoaded > 0)
            {
                for (int i = 0; i <= ammosLoaded - 1; i++)
                {
                    revolverAmmos[i].enabled = true;
                }
            }
            if (playerHeldBy == null)
            {
                return;
            }
            Debug.Log($"r/l activate: {right}");
            if (!right)
            {
                //StartOpenGunServerRpc();
            }
            else if (!isCylinderMoving && !isReloading && ammosLoaded < 6 && gunAnimator.GetBool("Reloading"))
            {
                //StartReloadGunServerRpc();
            }
        }

        public override void SetControlTipsForItem()
        {
            string[] toolTips = itemProperties.toolTips;
            if (toolTips.Length <= 2)
            {
                Debug.LogError("Shotgun control tips array length is too short to set tips!");
                return;
            }
            if (playerHeldBy.playerBodyAnimator.GetBool("ReloadRevolver"))
            {
                if (Plugin.translateKorean)
                {
                    toolTips[2] = "실린더 닫기: [Q]";
                }else
                {
                    toolTips[2] = "Close cylinder: [Q]";
                }
            }
            else
            {
                if (Plugin.translateKorean)
                {
                    toolTips[2] = "실린더 열기: [Q]";
                }
                else
                {
                    toolTips[2] = "Open cylinder: [Q]";
                }
            }
            HUDManager.Instance.ChangeControlTipMultiple(toolTips, holdingItem: true, itemProperties);
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartReloadGunServerRpc()
        {
            StartReloadGunClientRpc();
        }

        [ClientRpc]
        public void StartReloadGunClientRpc()
        {
            if ((Plugin.customGunInfinityAmmo || ReloadedGun()) && !isReloading)
            {
                StartCoroutine(ReloadGunAnimation());
            }
            else
            {
                gunAudio.PlayOneShot(noAmmoSFX);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartOpenGunServerRpc()
        {
            StartOpenGunClientRpc();
        }

        [ClientRpc]
        public void StartOpenGunClientRpc()
        {
            if (!isCylinderMoving && !isReloading)
            {
                StartCoroutine(RevolverMoveCylinder());
            }
            else
            {
                gunAudio.PlayOneShot(noAmmoSFX);
            }
        }

        private IEnumerator ReloadGunAnimation()
        {
            if (isCylinderMoving && !gunAnimator.GetBool("Reloading"))
            {
                yield break;
            }
            yield return new WaitForSeconds(0.05f);
            playerHeldBy.playerBodyAnimator.SetTrigger("RevolverInsertBullet");
            isReloading = true;
            gunAudio.PlayOneShot(gunReloadSFX);

            if (playerHeldBy == StartOfRound.Instance.localPlayerController)
            {
                WalkieTalkie.TransmitOneShotAudio(gunAudio, gunReloadSFX);

                revolverAmmoInHand.enabled = true;
                revolverAmmoInHandTransform.SetParent(playerHeldBy.leftHandItemTarget);
                revolverAmmoInHandTransform.localPosition = new Vector3(0.0033f, 0.0732f, -0.0762f);
                revolverAmmoInHandTransform.localEulerAngles = new Vector3(6.533f, 106.232f, -12.891f);
            }

            switch (ammosLoaded)
            {
                case 0:
                    cylinderTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                    break;
                case 1:
                    cylinderTransform.localRotation = Quaternion.Euler(new Vector3(0, 60, 0));
                    break;
                case 2:
                    cylinderTransform.localRotation = Quaternion.Euler(new Vector3(0, 120, 0));
                    break;
                case 3:
                    cylinderTransform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
                case 4:
                    cylinderTransform.localRotation = Quaternion.Euler(new Vector3(0, 240, 0));
                    break;
                case 5:
                    cylinderTransform.localRotation = Quaternion.Euler(new Vector3(0, 300, 0));
                    break;
                case 6:
                    cylinderTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                    break;
            }

            gunAudio.PlayOneShot(gunReloadSFX);

            yield return new WaitForSeconds(0.75f);
            if (playerHeldBy != null)
            {
                if (playerHeldBy == StartOfRound.Instance.localPlayerController && !Plugin.customGunInfinityAmmo)
                {
                    playerHeldBy.DestroyItemInSlotAndSync(ammoSlotToUse);
                }
                ammoSlotToUse = -1;
                ammosLoaded = Mathf.Clamp(ammosLoaded + 1, 0, 6);
            }
            yield return new WaitForSeconds(0.1f);

            if (playerHeldBy == StartOfRound.Instance.localPlayerController)
            {
                foreach (MeshRenderer ammo in revolverAmmos)
                {
                    ammo.enabled = false;
                }
                if (ammosLoaded > 0)
                {
                    for (int i = 0; i <= ammosLoaded - 1; i++)
                    {
                        revolverAmmos[i].enabled = true;
                    }
                }
                revolverAmmoInHand.enabled = false;
            }
            revolverAmmoInHandTransform.SetParent(transform);
            yield return new WaitForSeconds(1f);
            gunAudio.PlayOneShot(gunReloadFinishSFX);
            WalkieTalkie.TransmitOneShotAudio(gunAudio, gunReloadFinishSFX);
            isReloading = false;
        }

        private IEnumerator RevolverMoveCylinder()
        {
            isCylinderMoving = true;
            foreach (MeshRenderer ammo in revolverAmmos)
            {
                ammo.enabled = false;
            }
            if (ammosLoaded > 0)
            {
                for (int i = 0; i <= ammosLoaded - 1; i++)
                {
                    revolverAmmos[i].enabled = true;
                }
            }

            playerHeldBy.playerBodyAnimator.SetBool("ReloadRevolver", !playerHeldBy.playerBodyAnimator.GetBool("ReloadRevolver"));
            gunAnimator.SetBool("Reloading", !gunAnimator.GetBool("Reloading"));

            if (playerHeldBy == StartOfRound.Instance.localPlayerController)
            {
                if (playerHeldBy.playerBodyAnimator.GetBool("ReloadRevolver"))
                {
                    gunAudio.PlayOneShot(cylinderOpenSFX);
                }
                else
                {
                    gunAudio.PlayOneShot(cylinderCloseSFX);
                }
                SetControlTipsForItem();
            }else
            {
                if (gunAnimator.GetBool("Reloading"))
                {
                    gunAudio.PlayOneShot(cylinderOpenSFX);
                    WalkieTalkie.TransmitOneShotAudio(gunAudio, cylinderOpenSFX);
                    gunAnimator.SetBool("Reloading", true);
                }
                else
                {
                    gunAudio.PlayOneShot(cylinderCloseSFX);
                    WalkieTalkie.TransmitOneShotAudio(gunAudio, cylinderCloseSFX);
                    gunAnimator.SetBool("Reloading", false);
                }
            }
            yield return new WaitForSeconds(0.45f);
            isCylinderMoving = false;
        }

        private bool ReloadedGun()
        {
            int ammoInv = FindAmmoInInventory();
            if (ammoInv == -1)
            {
                Debug.Log("not reloading");
                return false;
            }
            Debug.Log("reloading!");
            ammoSlotToUse = ammoInv;
            return true;
        }

        private int FindAmmoInInventory()
        {
            for (int i = 0; i < playerHeldBy.ItemSlots.Length; i++)
            {
                if (!(playerHeldBy.ItemSlots[i] == null))
                {
                    GunAmmo gunAmmo = playerHeldBy.ItemSlots[i] as GunAmmo;
                    Debug.Log($"Ammo null in slot #{i}?: {gunAmmo == null}");
                    if (gunAmmo != null)
                    {
                        Debug.Log($"Ammo in slot #{i} id: {gunAmmo.ammoType}");
                    }
                    if (gunAmmo != null && gunAmmo.ammoType == gunCompatibleAmmoID)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public override void PocketItem()
        {
            if (playerHeldBy != null)
            {
                UpdateAnimator(playerHeldBy, playerHeldBy.playerBodyAnimator, true);
            }
            base.PocketItem();
            StopUsingGun();
        }

        public override void DiscardItem()
        {
            if (playerHeldBy != null)
            {
                UpdateAnimator(playerHeldBy, playerHeldBy.playerBodyAnimator, true);
            }
            base.DiscardItem();
            StopUsingGun();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncRevolverAmmoServerRpc(int ammoCount)
        {
            if (IsOwner)
            {
                SyncRevolverAmmoClientRpc(ammoCount);
            }
        }
        [ClientRpc]
        public void SyncRevolverAmmoClientRpc(int ammoCount)
        {
            ammosLoaded = ammoCount;
        }

        private void StopUsingGun()
        {
            //SyncRevolverAmmoServerRpc(ammosLoaded);
            isCylinderMoving = false;
            previousPlayerHeldBy.equippedUsableItemQE = false;
            if (isReloading)
            {
                StopCoroutine(ReloadGunAnimation());
                isReloading = false;
            }
            if (previousPlayerHeldBy != null)
            {
                previousPlayerHeldBy.playerBodyAnimator.SetBool("ReloadRevolver", false);
            }
            if (gunAnimator.GetBool("Reloading"))
            {
                gunAnimator.SetBool("Reloading", false);
                gunAudio.Stop();
                revolverAmmoInHandTransform.SetParent(transform);
                revolverAmmoInHand.enabled = false;
                isReloading = false;
            }
        }

        void UpdateAnimator(PlayerControllerB player, Animator playerBodyAnimator, bool restore)
        {
            if (!restore)
            {
                if (playerBodyAnimator.runtimeAnimatorController != Plugin.playerAnimator && playerBodyAnimator.runtimeAnimatorController != Plugin.otherPlayerAnimator)
                {
                    if (originalPlayerAnimator != null)
                    {
                        Destroy(originalPlayerAnimator, 1f);
                    }
                    originalPlayerAnimator = Instantiate(player.playerBodyAnimator.runtimeAnimatorController);
                    originalPlayerAnimator.name = "DefaultPlayerAnimator";
                    if (player == StartOfRound.Instance.localPlayerController)
                    {
                        SaveAnimatorStates(playerBodyAnimator);
                        playerBodyAnimator.runtimeAnimatorController = Plugin.playerAnimator;
                        RestoreAnimatorStates(playerBodyAnimator);
                        Plugin.mls.LogInfo("Replace Player Animator!");
                    }
                    else
                    {
                        SaveAnimatorStates(playerBodyAnimator);
                        playerBodyAnimator.runtimeAnimatorController = Plugin.otherPlayerAnimator;
                        RestoreAnimatorStates(playerBodyAnimator);
                        Plugin.mls.LogInfo("Replace Other Player Animator!");
                    }
                }
            }
            else
            {
                SaveAnimatorStates(playerBodyAnimator);
                playerBodyAnimator.runtimeAnimatorController = originalPlayerAnimator;
                RestoreAnimatorStates(playerBodyAnimator);
                Plugin.mls.LogInfo("Restored Player Animator!");
            }
        }

        void SaveAnimatorStates(Animator animator)
        {
            isCrouching = animator.GetBool("crouching");
            isJumping = animator.GetBool("Jumping");
            isWalking = animator.GetBool("Walking");
            isSprinting = animator.GetBool("Sprinting");
            currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            currentAnimationTime = currentStateInfo.normalizedTime;
        }


        public void RestoreAnimatorStates(Animator animator)
        {
            animator.Play(currentStateInfo.fullPathHash, 0, currentAnimationTime);
            animator.SetBool("crouching", isCrouching);
            animator.SetBool("Jumping", isJumping);
            animator.SetBool("Walking", isWalking);
            animator.SetBool("Sprinting", isSprinting);
        }
    }
}
