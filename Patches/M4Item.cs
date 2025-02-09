using System.Collections;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PiggyVarietyMod.Patches
{
    public class M4Item : GrabbableObject
    {
        private bool isCrouching;
        private bool isJumping;
        private bool isWalking;
        private bool isSprinting;
        private AnimatorStateInfo currentStateInfo;
        private float currentAnimationTime;
        public int gunCompatibleAmmoID = 1410;

        public bool isReloading;
        public bool isInspecting;

        public bool cantFire;

        public bool isFiring;

        public int ammosLoaded;

        public Animator gunAnimator;

        public AudioSource gunAudio;

        public AudioSource gunShootAudio;

        public AudioSource gunBulletsRicochetAudio;

        public AudioClip gunShootSFX;

        public AudioClip gunReloadSFX;
        public AudioClip gunInspectSFX;

        public AudioClip noAmmoSFX;

        private int ammoSlotToUse = -1;

        private bool localClientSendingShootGunRPC;

        private PlayerControllerB previousPlayerHeldBy;

        public ParticleSystem gunShootParticle;

        public Transform gunRayPoint;

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
            if (playerHeldBy == null)
            {
                return;
            }

            if (!isInspecting && isFiring && !isReloading && !cantFire && !playerHeldBy.playerBodyAnimator.GetBool("ReloadM4"))
            {
                if (ammosLoaded > 0)
                {
                    ShootGunAndSync(true);
                }
            }
            if (Plugin.InputActionInstance.RifleReloadKey.triggered && !isReloading && ammosLoaded < 30)
            {
                StartReloadGun();
            }
        }

        public override void EquipItem()
        {
            base.EquipItem();
            previousPlayerHeldBy = playerHeldBy;
            previousPlayerHeldBy.equippedUsableItemQE = true;
            if (playerHeldBy != null)
            {
                UpdateAnimator(playerHeldBy, playerHeldBy.playerBodyAnimator, false);
            }
            if (Plugin.translateKorean)
            {
                KR_SetAmmoControlTip(false);
            }
            else
            {
                SetAmmoControlTip(false);
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

        public override void ItemActivate(bool used, bool buttonDown = false)
        {
            Debug.Log(buttonDown);
            base.ItemActivate(used, buttonDown);
            isFiring = buttonDown;
            if (!isInspecting && !isReloading && !cantFire && !playerHeldBy.playerBodyAnimator.GetBool("ReloadM4"))
            {
                if (ammosLoaded <= 0)
                {
                    gunAudio.PlayOneShot(noAmmoSFX);
                }
            }
        }

        public void ShootGunAndSync(bool heldByPlayer)
        {
            Vector3 gunPosition;
            Vector3 forward;
            if (!heldByPlayer)
            {
                gunPosition = gunRayPoint.position;
                forward = gunRayPoint.forward;
            }
            else
            {
                gunPosition = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position - GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.up * 0.45f;
                forward = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward;
            }
            ShootGun(gunPosition, forward); ;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ShootGunServerRpc(Vector3 gunPosition, Vector3 gunForward)
        {
            ShootGunClientRpc(gunPosition, gunForward);
        }

        [ClientRpc]
        public void ShootGunClientRpc(Vector3 gunPosition, Vector3 gunForward)
        {
            Debug.Log("Shoot gun client rpc received");
            if (localClientSendingShootGunRPC)
            {
                localClientSendingShootGunRPC = false;
                Debug.Log("localClientSendingShootGunRPC was true");
            }
            else
            {
                ShootGun(gunPosition, gunForward);
            }
        }

        public IEnumerator FireDelay()
        {
            cantFire = true;
            yield return new WaitForSeconds(0.07f);
            cantFire = false;
        }

        public void ShootGun(Vector3 gunPosition, Vector3 gunForward)
        {
            if (Plugin.translateKorean)
            {
                KR_SetAmmoControlTip(false);
            }
            else
            {
                SetAmmoControlTip(false);
            }
            CentipedeAI[] array = GameObject.FindObjectsByType<CentipedeAI>(FindObjectsSortMode.None);
            FlowerSnakeEnemy[] array2 = GameObject.FindObjectsByType<FlowerSnakeEnemy>(FindObjectsSortMode.None);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].clingingToPlayer == playerHeldBy)
                {
                    array[i].HitEnemy(2, playerHeldBy, true, -1);
                }
            }
            for (int i = 0; i < array2.Length; i++)
            {
                if (array2[i].clingingToPlayer == playerHeldBy)
                {
                    array2[i].HitEnemy(2, playerHeldBy, true, -1);
                }
            }
            StartCoroutine(FireDelay());
            if (isHeld && playerHeldBy != null && playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                playerHeldBy.playerBodyAnimator.SetTrigger("ShootM4");
            }
            gunAnimator.SetTrigger("Fire");
            gunShootAudio.PlayOneShot(gunShootSFX);
            WalkieTalkie.TransmitOneShotAudio(gunShootAudio, gunShootSFX);
            gunShootParticle.Play(withChildren: true);
            ammosLoaded = Mathf.Clamp(ammosLoaded - 1, 0, 30);
            PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
            if (localPlayerController == null)
            {
                return;
            }
            float gunPlayerDistance = Vector3.Distance(localPlayerController.transform.position, gunRayPoint.transform.position);
            Vector3 vector = localPlayerController.playerCollider.ClosestPoint(gunPosition);
            if (gunPlayerDistance < 12f)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
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
                            Debug.Log("Rifle is held by enemy, skipping enemy raycast");
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
                                Plugin.mls.LogInfo("Damage to enemy, damage: " + Plugin.rifleMonsterDamage + ", distance:" + distance);
                                hittable.Hit(Plugin.rifleMonsterDamage, hitDirection, playerHeldBy, true, -1);
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
                        playerColliders[i].transform.GetComponent<PlayerControllerB>().DamagePlayer(Plugin.rifleMaxPlayerDamage, true, true, CauseOfDeath.Gunshots, 0, false, playerHeldBy.gameplayCamera.transform.forward * 30f);
                    }
                    else if (distance < 25)
                    {
                        playerColliders[i].transform.GetComponent<PlayerControllerB>().DamagePlayer(Mathf.RoundToInt(Plugin.rifleMaxPlayerDamage - (Plugin.rifleMaxPlayerDamage / 3)), true, true, CauseOfDeath.Gunshots, 0, false, playerHeldBy.gameplayCamera.transform.forward * 30f);
                    }
                    else
                    {
                        playerColliders[i].transform.GetComponent<PlayerControllerB>().DamagePlayer(Mathf.RoundToInt(Plugin.rifleMaxPlayerDamage / 3), true, true, CauseOfDeath.Gunshots, 0, false, playerHeldBy.gameplayCamera.transform.forward * 30f);
                    }
                    Debug.Log("Player ouch");
                }
            }
        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);
            if (playerHeldBy == null)
            {
                return;
            }
            Debug.Log($"r/l activate: {right}");
            if (!right)
            {
                StartCheckMagazine();
            }
        }

        private void StartReloadGun()
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

        private void StartCheckMagazine()
        {
            if (!isInspecting)
            {
                StartCoroutine(CheckAmmoGunAnimation());
            }
            else
            {
                gunAudio.PlayOneShot(noAmmoSFX);
            }
        }

        private IEnumerator CheckAmmoGunAnimation()
        {
            playerHeldBy.playerBodyAnimator.SetTrigger("InspectM4");
            isInspecting = true;
            gunAudio.PlayOneShot(gunInspectSFX);

            if (playerHeldBy == StartOfRound.Instance.localPlayerController)
            {
                WalkieTalkie.TransmitOneShotAudio(gunAudio, gunInspectSFX);
            }

            yield return new WaitForSeconds(1f);

            if (Plugin.translateKorean)
            {
                KR_SetAmmoControlTip(true);
            }
            else
            {
                SetAmmoControlTip(true);
            }
            yield return new WaitForSeconds(1f);
            isInspecting = false;
        }
        private void SetAmmoControlTip(bool isChecking)
        {
            string changeTo = "Check ammo : [Q]";
            if (ammosLoaded == 30)
            {
                changeTo = (isChecking) ? ("Check ammo : [Q] [Full]") : ("Check ammo : [Q] [??]");
            }
            else if (ammosLoaded >= 20 && 30 > ammosLoaded)
            {
                changeTo = (isChecking) ? ("Check ammo : [Q] [<30]") : ("Check ammo : [Q] [??]");
            }
            else if (ammosLoaded >= 10 && 20 > ammosLoaded)
            {
                changeTo = (isChecking) ? ("Check ammo : [Q] [<20]") : ("Check ammo : [Q] [??]");
            }
            else if (ammosLoaded >= 1 && 10 > ammosLoaded)
            {
                changeTo = (isChecking) ? ("Check ammo : [Q] [<10]") : ("Check ammo : [Q] [??]");
            }
            else if (ammosLoaded <= 0)
            {
                changeTo = (isChecking) ? ("Check ammo : [Q] [Empty]") : ("Check ammo : [Q] [??]");
            }

            if (base.IsOwner)
            {
                HUDManager.Instance.ChangeControlTip(3, changeTo);
                string text = Plugin.InputActionInstance.RifleReloadKey.GetBindingDisplayString().Replace(" | ", "");
                HUDManager.Instance.ChangeControlTip(2, "Reload : [" + text + "]");
            }
        }

        private void KR_SetAmmoControlTip(bool isChecking)
        {
            string changeTo = "탄약 확인하기 : [Q]";
            if (ammosLoaded == 30)
            {
                changeTo = (isChecking) ? ("탄약 확인하기 : [Q] [가득 참]") : ("탄약 확인하기 : [Q] [??]");
            }
            else if (ammosLoaded >= 20 && 30 > ammosLoaded)
            {
                changeTo = (isChecking) ? ("탄약 확인하기 : [Q] [<30]") : ("탄약 확인하기 : [Q] [??]");
            }
            else if (ammosLoaded >= 10 && 20 > ammosLoaded)
            {
                changeTo = (isChecking) ? ("탄약 확인하기 : [Q] [<20]") : ("탄약 확인하기 : [Q] [??]");
            }
            else if (ammosLoaded >= 1 && 10 > ammosLoaded)
            {
                changeTo = (isChecking) ? ("탄약 확인하기 : [Q] [<10]") : ("탄약 확인하기 : [Q] [??]");
            }
            else if (ammosLoaded <= 0)
            {
                changeTo = (isChecking) ? ("탄약 확인하기 : [Q] [비어 있음]") : ("탄약 확인하기 : [Q] [??]");
            }

            if (base.IsOwner)
            {
                HUDManager.Instance.ChangeControlTip(3, changeTo);
                string text = Plugin.InputActionInstance.RifleReloadKey.GetBindingDisplayString().Replace(" | ", "");
                HUDManager.Instance.ChangeControlTip(2, "재장전 : [" + text + "]");
            }
        }

        private IEnumerator ReloadGunAnimation()
        {
            playerHeldBy.playerBodyAnimator.SetTrigger("ReloadM4");
            gunAnimator.SetTrigger("Reloading");
            isReloading = true;
            gunAudio.PlayOneShot(gunReloadSFX);

            if (playerHeldBy == StartOfRound.Instance.localPlayerController)
            {
                WalkieTalkie.TransmitOneShotAudio(gunAudio, gunReloadSFX);
            }

            yield return new WaitForSeconds(2f);
            if (isReloading)
            {
                if (playerHeldBy == StartOfRound.Instance.localPlayerController && !Plugin.customGunInfinityAmmo)
                {
                    playerHeldBy.DestroyItemInSlotAndSync(ammoSlotToUse);
                }
                ammoSlotToUse = -1;
                ammosLoaded = 30;
                if (Plugin.translateKorean)
                {
                    KR_SetAmmoControlTip(true);
                }
                else
                {
                    SetAmmoControlTip(true);
                }
            }
            yield return new WaitForSeconds(0.55f);
            isReloading = false;
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

        private void StopUsingGun()
        {
            previousPlayerHeldBy.equippedUsableItemQE = false;
            gunAudio.Stop();
            isReloading = false;
            previousPlayerHeldBy.playerBodyAnimator.SetTrigger("SwitchHoldAnimation");
            gunAnimator.SetTrigger("Reset");
            isInspecting = false;
        }

        void UpdateAnimator(PlayerControllerB player, Animator playerBodyAnimator, bool restore)
        {
            if (!restore)
            {
                if (playerBodyAnimator.runtimeAnimatorController != Plugin.playerAnimator && playerBodyAnimator.runtimeAnimatorController != Plugin.otherPlayerAnimator)
                {
                    if (originalPlayerAnimator != null)
                    {
                        Destroy(originalPlayerAnimator);
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
                Plugin.mls.LogInfo("Replace Other Player Animator!");
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