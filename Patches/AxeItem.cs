using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

public class AxeItem : GrabbableObject
{
    public int shovelHitForce = 1;

    public bool reelingUp;

    public bool isHoldingButton;

    private Coroutine reelingUpCoroutine;

    private RaycastHit[] objectsHitByShovel;

    private List<RaycastHit> objectsHitByShovelList = new List<RaycastHit>();

    public AudioClip reelUp;

    public AudioClip swing;

    public AudioClip[] hitSFX;

    public AudioSource shovelAudio;

    private PlayerControllerB previousPlayerHeldBy;

    private int shovelMask = 11012424;

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        if (playerHeldBy == null)
        {
            return;
        }
        Debug.Log($"Is player pressing down button?: {buttonDown}");
        isHoldingButton = buttonDown;
        Debug.Log("PLAYER ACTIVATED ITEM TO HIT WITH SHOVEL. Who sent this log: " + GameNetworkManager.Instance.localPlayerController.gameObject.name);
        if (!reelingUp && buttonDown)
        {
            reelingUp = true;
            previousPlayerHeldBy = playerHeldBy;
            Debug.Log($"Set previousPlayerHeldBy: {previousPlayerHeldBy}");
            if (reelingUpCoroutine != null)
            {
                StopCoroutine(reelingUpCoroutine);
            }
            reelingUpCoroutine = StartCoroutine(reelUpShovel());
        }
    }

    private IEnumerator reelUpShovel()
    {
        playerHeldBy.activatingItem = true;
        playerHeldBy.twoHanded = true;
        playerHeldBy.playerBodyAnimator.ResetTrigger("shovelHit");
        playerHeldBy.playerBodyAnimator.SetBool("reelingUp", value: true);
        shovelAudio.PlayOneShot(reelUp);
        ReelUpSFXServerRpc();
        yield return new WaitForSeconds(0.35f);
        yield return new WaitUntil(() => !isHoldingButton || !isHeld);
        SwingShovel(!isHeld);
        yield return new WaitForSeconds(0.13f);
        yield return new WaitForEndOfFrame();
        HitShovel(!isHeld);
        yield return new WaitForSeconds(0.3f);
        reelingUp = false;
        reelingUpCoroutine = null;
    }

    [ServerRpc]
    public void ReelUpSFXServerRpc()
    {
        {
            ReelUpSFXClientRpc();
        }
    }
    [ClientRpc]
    public void ReelUpSFXClientRpc()
    {
        if (!IsOwner)
        {
            shovelAudio.PlayOneShot(reelUp);
        }
    }

    public override void DiscardItem()
    {
        if (playerHeldBy != null)
        {
            playerHeldBy.activatingItem = false;
        }
        base.DiscardItem();
    }

    public void SwingShovel(bool cancel = false)
    {
        previousPlayerHeldBy.playerBodyAnimator.SetBool("reelingUp", value: false);
        if (!cancel)
        {
            shovelAudio.PlayOneShot(swing);
            previousPlayerHeldBy.UpdateSpecialAnimationValue(specialAnimation: true, (short)previousPlayerHeldBy.transform.localEulerAngles.y, 0.4f);
        }
    }

    public void HitShovel(bool cancel = false)
    {
        if (this.previousPlayerHeldBy == null)
        {
            Debug.LogError("Previousplayerheldby is null on this client when HitShovel is called.");
            return;
        }
        this.previousPlayerHeldBy.activatingItem = false;
        bool flag = false;
        bool flag2 = false;
        bool flag3 = false;
        int num = -1;
        if (!cancel)
        {
            this.previousPlayerHeldBy.twoHanded = false;
            this.objectsHitByShovel = Physics.SphereCastAll(this.previousPlayerHeldBy.gameplayCamera.transform.position + this.previousPlayerHeldBy.gameplayCamera.transform.right * -0.35f, 0.8f, this.previousPlayerHeldBy.gameplayCamera.transform.forward, 1.5f, this.shovelMask, QueryTriggerInteraction.Collide);
            this.objectsHitByShovelList = this.objectsHitByShovel.OrderBy((RaycastHit x) => x.distance).ToList<RaycastHit>();

            List<EnemyAI> list = new List<EnemyAI>();
            for (int i = 0; i < this.objectsHitByShovelList.Count; i++)
            {
                IHittable hittable;
                RaycastHit raycastHit;
                Vector3 forward = this.previousPlayerHeldBy.gameplayCamera.transform.forward;

                if (this.objectsHitByShovelList[i].transform.TryGetComponent<IHittable>(out hittable) && !(this.objectsHitByShovelList[i].transform == this.previousPlayerHeldBy.transform) && (this.objectsHitByShovelList[i].point == Vector3.zero || !Physics.Linecast(this.previousPlayerHeldBy.gameplayCamera.transform.position, this.objectsHitByShovelList[i].point, out raycastHit, StartOfRound.Instance.walkableSurfacesMask, QueryTriggerInteraction.Ignore)))
                {
                    TerrainObstacleTrigger terrainObstacleTrigger = objectsHitByShovelList[i].collider.GetComponentInChildren<TerrainObstacleTrigger>();
                    if (terrainObstacleTrigger != null && IsOwner)
                    {
                        RoundManager.Instance.DestroyTreeOnLocalClient(terrainObstacleTrigger.transform.position);
                    }
                }

                if (this.objectsHitByShovelList[i].transform.gameObject.layer == 8 || this.objectsHitByShovelList[i].transform.gameObject.layer == 11)
                {
                    if (!this.objectsHitByShovelList[i].collider.isTrigger)
                    {
                        flag = true;
                        string tag = this.objectsHitByShovelList[i].collider.gameObject.tag;
                        for (int j = 0; j < StartOfRound.Instance.footstepSurfaces.Length; j++)
                        {
                            if (StartOfRound.Instance.footstepSurfaces[j].surfaceTag == tag)
                            {
                                num = j;
                                break;
                            }
                        }
                    }
                }
                else if (this.objectsHitByShovelList[i].transform.TryGetComponent<IHittable>(out hittable) && !(this.objectsHitByShovelList[i].transform == this.previousPlayerHeldBy.transform) && (this.objectsHitByShovelList[i].point == Vector3.zero || !Physics.Linecast(this.previousPlayerHeldBy.gameplayCamera.transform.position, this.objectsHitByShovelList[i].point, out raycastHit, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore)))
                {
                    flag = true;
                    try
                    {
                        EnemyAICollisionDetect component = this.objectsHitByShovelList[i].collider.GetComponent<EnemyAICollisionDetect>();
                        if (component != null)
                        {
                            if (component.mainScript == null || list.Contains(component.mainScript))
                            {
                                goto IL_361;
                            }
                        }
                        else if (this.objectsHitByShovelList[i].transform.GetComponent<PlayerControllerB>() != null)
                        {
                            if (flag3)
                            {
                                goto IL_361;
                            }
                            flag3 = true;
                        }
                        bool flag4 = hittable.Hit(this.shovelHitForce, forward, this.previousPlayerHeldBy, true, 1);
                        if (flag4 && component != null)
                        {
                            list.Add(component.mainScript);
                        }
                        if (!flag2)
                        {
                            flag2 = flag4;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(string.Format("Exception caught when hitting object with shovel from player #{0}: {1}", this.previousPlayerHeldBy.playerClientId, ex));
                    }
                }
            IL_361:;
            }
        }
        if (flag)
        {
            RoundManager.PlayRandomClip(this.shovelAudio, this.hitSFX, true, 1f, 0, 1000);
            FindObjectOfType<RoundManager>().PlayAudibleNoise(transform.position, 17f, 0.8f, 0, false, 0);
            if (!flag2 && num != -1)
            {
                this.shovelAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[num].hitSurfaceSFX);
                WalkieTalkie.TransmitOneShotAudio(this.shovelAudio, StartOfRound.Instance.footstepSurfaces[num].hitSurfaceSFX, 1f);
            }
            this.playerHeldBy.playerBodyAnimator.SetTrigger("shovelHit");
            this.HitShovelServerRpc(num);
        }
    }

    [ServerRpc]
    public void HitShovelServerRpc(int hitSurfaceID)
    {
        {
            HitShovelClientRpc(hitSurfaceID);
        }
    }
    [ClientRpc]
    public void HitShovelClientRpc(int hitSurfaceID)
    {
        if (!IsOwner)
        {
            RoundManager.PlayRandomClip(shovelAudio, hitSFX);
            if (hitSurfaceID != -1)
            {
                HitSurfaceWithShovel(hitSurfaceID);
            }
        }
    }
    private void HitSurfaceWithShovel(int hitSurfaceID)
    {
        shovelAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[hitSurfaceID].hitSurfaceSFX);
        WalkieTalkie.TransmitOneShotAudio(shovelAudio, StartOfRound.Instance.footstepSurfaces[hitSurfaceID].hitSurfaceSFX);
    }
}
