﻿using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements.Collections;

namespace PiggyVarietyMod.Patches
{
    public class RevolverItem : GrabbableObject {
        public static Dictionary<ulong, RuntimeAnimatorController> playerAnimatorDictionary = new Dictionary<ulong, RuntimeAnimatorController>();
        
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

        private Coroutine gunCoroutine;

        public List<AudioClip> gunShootSFX = new List<AudioClip>();

        public AudioClip gunReloadSFX;

        public AudioClip cylinderOpenSFX;
        public AudioClip cylinderCloseSFX;

        public AudioClip gunReloadFinishSFX;

        public AudioClip noAmmoSFX;

        public AudioClip gunSafetySFX;

        public AudioClip switchSafetyOnSFX;

        public AudioClip switchSafetyOffSFX;

        private bool hasHitGroundWithSafetyOff = true;

        private int ammoSlotToUse = -1;

        private bool localClientSendingShootGunRPC;

        private PlayerControllerB previousPlayerHeldBy;

        public ParticleSystem gunShootParticle;

        public Transform revolverRayPoint;

        public List<MeshRenderer> revolverAmmos = new List<MeshRenderer>();

        public MeshRenderer revolverAmmoInHand;

        public Transform revolverAmmoInHandTransform;

        private RaycastHit[] enemyColliders;
        private RaycastHit[] playerColliders;

        private EnemyAI heldByEnemy;

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
            if (Plugin.revolverInfinityAmmo)
            {
                ammosLoaded = 6;
            }
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
            if (playerHeldBy != null)
            { 
                UpdateAnimator(playerHeldBy, playerHeldBy.playerBodyAnimator, false);
            }
            base.EquipItem();
            SyncRevolverAmmoServerRpc(ammosLoaded);
            playerHeldBy.playerBodyAnimator.SetBool("ReloadRevolver", false);
            gunAnimator.SetBool("Reloading", false);
            revolverAmmoInHand.enabled = false;
            previousPlayerHeldBy = playerHeldBy;
            previousPlayerHeldBy.equippedUsableItemQE = true;
            isCylinderMoving = false;
            hasHitGroundWithSafetyOff = false;
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

        public override void GrabItemFromEnemy(EnemyAI enemy)
        {
            base.GrabItemFromEnemy(enemy);
            heldByEnemy = enemy;
            hasHitGroundWithSafetyOff = false;
        }

        public override void DiscardItemFromEnemy()
        {
            base.DiscardItemFromEnemy();
            heldByEnemy = null;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            SyncRevolverAmmoServerRpc(ammosLoaded);
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
            //localClientSendingShootGunRPC = true;
            //ShootGunServerRpc(revolverPosition, forward);
        }

        /*
        [ServerRpc(RequireOwnership = false)]
        public void ShootGunServerRpc(Vector3 revolverPosition, Vector3 revolverForward)
        {
            ShootGunClientRpc(revolverPosition, revolverForward);
        }

        [ClientRpc]
        public void ShootGunClientRpc(Vector3 revolverPosition, Vector3 revolverForward)
        {
            Debug.Log("Shoot gun client rpc received");
            if (localClientSendingShootGunRPC)
            {
                localClientSendingShootGunRPC = false;
                Debug.Log("localClientSendingShootGunRPC was true");
            }
            else
            {
                ShootGun(revolverPosition, revolverForward);
            }
        }
        */

        public IEnumerator FireDelay()
        {
            cantFire = true;
            yield return new WaitForSeconds(0.2f);
            cantFire = false;
        }

        public void ShootGun(Vector3 revolverPosition, Vector3 revolverForward)
        {
            CentipedeAI[] array = GameObject.FindObjectsByType<CentipedeAI>(FindObjectsSortMode.None);
            FlowerSnakeEnemy[] array2 = GameObject.FindObjectsByType<FlowerSnakeEnemy>(FindObjectsSortMode.None);
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
            bool flag = false;
            if (isHeld && playerHeldBy != null && playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                playerHeldBy.playerBodyAnimator.SetTrigger("ShootRevolver");
                flag = true;
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
            float num = Vector3.Distance(localPlayerController.transform.position, revolverRayPoint.transform.position);
            bool flag2 = false;
            int num2 = 0;
            float num3 = 0f;
            Vector3 vector = localPlayerController.playerCollider.ClosestPoint(revolverPosition);
            if (!flag && !Physics.Linecast(revolverPosition, vector, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) && Vector3.Angle(revolverForward, vector - revolverPosition) < 30f)
            {
                flag2 = true;
            }
            if (num < 5f)
            {
                num3 = 0.25f;
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                num2 = 100;
            }
            if (num < 15f)
            {
                num3 = 0.15f;
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
                num2 = 100;
            }
            else if (num < 23f)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
                num2 = 40;
            }
            else if (num < 30f)
            {
                num2 = 20;
            }
            if (num3 > 0f && SoundManager.Instance.timeSinceEarsStartedRinging > 16f)
            {
                StartCoroutine(delayedEarsRinging(num3));
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

            /*
            RaycastHit hitTest;
            if (playerHeldBy == StartOfRound.Instance.localPlayerController)
            {
                if (Physics.Raycast(playerHeldBy.cameraContainerTransform.transform.position, playerHeldBy.cameraContainerTransform.transform.forward, out hitTest, Mathf.Infinity))
                {
                    if (hitTest.collider.GetComponent<EnemyAICollisionDetect>() != null)
                    {
                        EnemyAI mainScript = hitTest.collider.GetComponent<EnemyAICollisionDetect>().mainScript;
                        IHittable hittable;
                        if (hitTest.collider.transform.TryGetComponent<IHittable>(out hittable))
                        {
                            Plugin.mls.LogWarning("HIT ENEMY! RANGE: " + hitTest.distance);
                            hittable.Hit(9999, playerHeldBy.cameraContainerTransform.forward, this.playerHeldBy, true, -1);
                        }    
                    }
                }
            }
            */

            if (playerHeldBy == StartOfRound.Instance.localPlayerController)
            {
                int num4 = Physics.SphereCastNonAlloc(ray, 0.25f, enemyColliders, Mathf.Infinity, 524288, QueryTriggerInteraction.Collide);

                for (int i = 0; i < num4; i++)
                {
                    Debug.Log("Raycasting enemy");
                    if (this.enemyColliders[i].transform.GetComponent<EnemyAICollisionDetect>())
                    {
                        EnemyAI mainScript = this.enemyColliders[i].transform.GetComponent<EnemyAICollisionDetect>().mainScript;
                        if (this.heldByEnemy != null && this.heldByEnemy == mainScript)
                        {
                            Debug.Log("Shotgun is held by enemy, skipping enemy raycast");
                        }
                        else
                        {
                            Debug.Log("Hit enemy " + mainScript.enemyType.enemyName);
                            IHittable hittable;
                            if (this.enemyColliders[i].distance == 0f)
                            {
                                Debug.Log("Spherecast started inside enemy collider");
                            }
                            else if (Physics.Linecast(playerHeldBy.gameplayCamera.transform.position, this.enemyColliders[i].point, out hit, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                            {
                                Debug.DrawRay(hit.point, Vector3.up, Color.red, 15f);
                                Debug.DrawLine(playerHeldBy.gameplayCamera.transform.position, this.enemyColliders[i].point, Color.cyan, 15f);
                                Plugin.mls.LogInfo("Raycast hit wall: " + hit.collider.gameObject.name + ", distance: " + hit.distance);
                            }
                            else if (this.enemyColliders[i].transform.TryGetComponent<IHittable>(out hittable))
                            {
                                Vector3 hitDirection = playerHeldBy.gameplayCamera.transform.forward;
                                float distance = Vector3.Distance(playerHeldBy.gameplayCamera.transform.position, this.enemyColliders[i].point);
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
                                Plugin.mls.LogInfo("Could not get hittable script from collider, transform: " + this.enemyColliders[i].transform.name);
                            }
                        }
                    }
                    /*
                    else if (enemyColliders[i].transform.GetComponent<EnemyAI>())
                    {
                        EnemyAI mainScript = this.enemyColliders[i].transform.GetComponent<EnemyAI>();

                        IHittable hittable;
                        if (this.enemyColliders[i].transform.GetComponentInChildren<EnemyAICollisionDetect>().TryGetComponent<IHittable>(out hittable))
                        {
                            Vector3 hitDirection = playerHeldBy.cameraContainerTransform.forward;
                            float num5 = Vector3.Distance(playerHeldBy.cameraContainerTransform.position, this.enemyColliders[i].point);
                            int num6;
                            Debug.Log("Damage to enemy: " + num5);
                            if (num5 < 10f)
                            {
                                num6 = 3;
                            }
                            else if (num5 < 20f)
                            {
                                num6 = 2;
                            }
                            else
                            {
                                num6 = 1;
                            }
                            Debug.Log(string.Format("Hit enemy, hitDamage: {0}", num6));
                            hittable.Hit(num6, hitDirection, this.playerHeldBy, true, -1);
                        }
                    }
                    */
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

            /*
            if (Physics.Raycast(playerHeldBy.cameraContainerTransform.position, playerHeldBy.cameraContainerTransform.forward, out hit, Mathf.Infinity, StartOfRound.Instance.collidersRoomMaskDefaultAndPlayers))
            {
                Debug.Log("Revolver hit distance: " + hit.distance);
                if (hit.collider.GetComponent<PlayerControllerB>() != null)
                {
                    if (hit.distance < 10)
                    {
                        hit.collider.GetComponent<PlayerControllerB>().DamagePlayer(85, true, true, CauseOfDeath.Gunshots, 0, false, this.playerHeldBy.cameraContainerTransform.forward * 30f);
                    }
                    else if (hit.distance < 25)
                    {
                        hit.collider.GetComponent<PlayerControllerB>().DamagePlayer(55, true, true, CauseOfDeath.Gunshots, 0, false, this.playerHeldBy.cameraContainerTransform.forward * 30f);
                    }
                    else
                    {
                        hit.collider.GetComponent<PlayerControllerB>().DamagePlayer(35, true, true, CauseOfDeath.Gunshots, 0, false, this.playerHeldBy.cameraContainerTransform.forward * 30f);
                    }
                    Debug.Log("Revolver vs Player wtf lmao");
                }
            }
            */
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
                StartOpenGunServerRpc();
            }
            else if (!isCylinderMoving && !isReloading && ammosLoaded < 6 && gunAnimator.GetBool("Reloading"))
            {
                StartReloadGunServerRpc();
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

        private void SetSafetyControlTip()
        {
            string changeTo;
            if (Plugin.translateKorean)
            {
                changeTo = ((!playerHeldBy.playerBodyAnimator.GetBool("ReloadRevolver")) ? "실린더 열기: [Q]" : "실린더 닫기: [Q]");
            }else
            {
                changeTo = ((!playerHeldBy.playerBodyAnimator.GetBool("ReloadRevolver")) ? "Open cylinder: [Q]" : "Close cylinder: [Q]");
            }

            if (base.IsOwner)
            {
                HUDManager.Instance.ChangeControlTip(3, changeTo);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartReloadGunServerRpc()
        {
            StartReloadGunClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartReloadGunClientRpc()
        {
            if (ReloadedGun() && !isReloading)
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
                if (playerHeldBy == StartOfRound.Instance.localPlayerController)
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
            revolverAmmoInHandTransform.SetParent(base.transform);
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
            int num = FindAmmoInInventory();
            if (num == -1)
            {
                Debug.Log("not reloading");
                return false;
            }
            Debug.Log("reloading!");
            ammoSlotToUse = num;
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
        
        

        public override void GrabItem() {
            if (playerHeldBy != null)
            { 
                UpdateAnimator(playerHeldBy, playerHeldBy.playerBodyAnimator, false);
            }
            base.GrabItem();
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
            if (base.IsOwner)
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
            SyncRevolverAmmoServerRpc(ammosLoaded);
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
                if (gunCoroutine != null)
                {
                    StopCoroutine(gunCoroutine);
                }   
                gunAnimator.SetBool("Reloading", false);
                gunAudio.Stop();
                revolverAmmoInHandTransform.SetParent(base.transform);
                revolverAmmoInHand.enabled = false;
                isReloading = false;
            }
        }
        
        static void UpdateAnimator(PlayerControllerB player, Animator playerBodyAnimator, bool restore)
        {
            if (!restore)
            {
                if (playerBodyAnimator.runtimeAnimatorController != Plugin.playerAnimator && playerBodyAnimator.runtimeAnimatorController != Plugin.otherPlayerAnimator)
                {
                    if (player == StartOfRound.Instance.localPlayerController)
                    {
                        playerBodyAnimator.runtimeAnimatorController = Plugin.playerAnimator;
                        Plugin.mls.LogInfo("Replace Player Animator!");
                    }
                    else
                    {
                        playerBodyAnimator.runtimeAnimatorController = Plugin.otherPlayerAnimator;
                        Plugin.mls.LogInfo("Replace Other Player Animator!");
                    }
                }
            }
            else
            {
                if (playerAnimatorDictionary.ContainsKey(player.playerClientId))
                {
                    playerBodyAnimator.runtimeAnimatorController = playerAnimatorDictionary.Get(player.playerClientId);
                    playerAnimatorDictionary.Remove(player.playerClientId);
                    Plugin.mls.LogInfo("Restored Player Animator!");
                }    
            }
        }
    }
}
