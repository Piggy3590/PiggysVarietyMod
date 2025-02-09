using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

public class AxeItem : GrabbableObject
{
    public int axeHitForce = 1;

    public bool reelingUp;

    public bool isHoldingButton;

    private Coroutine reelingUpCoroutine;

    private RaycastHit[] objectsHitByAxe;

    private List<RaycastHit> objectsHitByAxeList = new List<RaycastHit>();

    public AudioClip reelUp;

    public AudioClip swing;

    public AudioClip[] hitSFX;

    public AudioSource axeAudio;

    private PlayerControllerB previousPlayerHeldBy;

    private int axeMask = 11012424;

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        if (playerHeldBy == null)
        {
            return;
        }
        Debug.Log($"Is player pressing down button?: {buttonDown}");
        isHoldingButton = buttonDown;
        Debug.Log("PLAYER ACTIVATED ITEM TO HIT WITH AXE. Who sent this log: " + GameNetworkManager.Instance.localPlayerController.gameObject.name);
        if (!reelingUp && buttonDown)
        {
            reelingUp = true;
            previousPlayerHeldBy = playerHeldBy;
            Debug.Log($"Set previousPlayerHeldBy: {previousPlayerHeldBy}");
            if (reelingUpCoroutine != null)
            {
                StopCoroutine(reelingUpCoroutine);
            }
            reelingUpCoroutine = StartCoroutine(reelUpAxe());
        }
    }

    private IEnumerator reelUpAxe()
    {
        playerHeldBy.activatingItem = true;
        playerHeldBy.twoHanded = true;
        playerHeldBy.playerBodyAnimator.ResetTrigger("axeHit");
        playerHeldBy.playerBodyAnimator.SetBool("reelingUp", value: true);
        axeAudio.PlayOneShot(reelUp);
        ReelUpSFXServerRpc();
        yield return new WaitForSeconds(0.35f);
        yield return new WaitUntil(() => !isHoldingButton || !isHeld);
        SwingAxe(!isHeld);
        yield return new WaitForSeconds(0.13f);
        yield return new WaitForEndOfFrame();
        HitAxe(!isHeld);
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
            axeAudio.PlayOneShot(reelUp);
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

    public void SwingAxe(bool cancel = false)
    {
        previousPlayerHeldBy.playerBodyAnimator.SetBool("reelingUp", value: false);
        if (!cancel)
        {
            axeAudio.PlayOneShot(swing);
            previousPlayerHeldBy.UpdateSpecialAnimationValue(specialAnimation: true, (short)previousPlayerHeldBy.transform.localEulerAngles.y, 0.4f);
        }
    }

    public void HitAxe(bool cancel = false)
    {
        
        if (previousPlayerHeldBy == null)
        {
            Debug.LogError("Previousplayerheldby is null on this client when HitAxe is called.");
            return;
        }
        previousPlayerHeldBy.activatingItem = false;
        bool flag = false;
        bool flag2 = false;
        bool flag3 = false;
        int num = -1;
        if (!cancel)
        {
            previousPlayerHeldBy.twoHanded = false;
            objectsHitByAxe = Physics.SphereCastAll(previousPlayerHeldBy.gameplayCamera.transform.position + previousPlayerHeldBy.gameplayCamera.transform.right * -0.35f, 0.8f, previousPlayerHeldBy.gameplayCamera.transform.forward, 1.5f, axeMask, QueryTriggerInteraction.Collide);
            objectsHitByAxeList = objectsHitByAxe.OrderBy((RaycastHit x) => x.distance).ToList<RaycastHit>();

            List<EnemyAI> list = new List<EnemyAI>();
            for (int i = 0; i < objectsHitByAxeList.Count; i++)
            {
                IHittable hittable;
                RaycastHit raycastHit;
                Vector3 forward = previousPlayerHeldBy.gameplayCamera.transform.forward;

                if (objectsHitByAxeList[i].transform.TryGetComponent<IHittable>(out hittable) && !(objectsHitByAxeList[i].transform == previousPlayerHeldBy.transform) && (objectsHitByAxeList[i].point == Vector3.zero || !Physics.Linecast(previousPlayerHeldBy.gameplayCamera.transform.position, objectsHitByAxeList[i].point, out raycastHit, StartOfRound.Instance.walkableSurfacesMask, QueryTriggerInteraction.Ignore)))
                {
                    TerrainObstacleTrigger terrainObstacleTrigger = objectsHitByAxeList[i].collider.GetComponentInChildren<TerrainObstacleTrigger>();
                    if (terrainObstacleTrigger != null && IsOwner)
                    {
                        RoundManager.Instance.DestroyTreeOnLocalClient(terrainObstacleTrigger.transform.position);
                    }
                }

                if (objectsHitByAxeList[i].transform.gameObject.layer == 8 || objectsHitByAxeList[i].transform.gameObject.layer == 11)
                {
                    if (!objectsHitByAxeList[i].collider.isTrigger)
                    {
                        flag = true;
                        string tag = objectsHitByAxeList[i].collider.gameObject.tag;
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
                else if (objectsHitByAxeList[i].transform.TryGetComponent<IHittable>(out hittable) && !(objectsHitByAxeList[i].transform == previousPlayerHeldBy.transform) && (objectsHitByAxeList[i].point == Vector3.zero || !Physics.Linecast(previousPlayerHeldBy.gameplayCamera.transform.position, objectsHitByAxeList[i].point, out raycastHit, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore)))
                {
                    flag = true;
                    try
                    {
                        EnemyAICollisionDetect component = objectsHitByAxeList[i].collider.GetComponent<EnemyAICollisionDetect>();
                        if (component != null)
                        {
                            if (component.mainScript == null || list.Contains(component.mainScript))
                            {
                                goto IL_361;
                            }
                        }
                        else if (objectsHitByAxeList[i].transform.GetComponent<PlayerControllerB>() != null)
                        {
                            if (flag3)
                            {
                                goto IL_361;
                            }
                            flag3 = true;
                        }
                        bool flag4 = hittable.Hit(axeHitForce, forward, previousPlayerHeldBy, true, 1);
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
                        Debug.Log(string.Format("Exception caught when hitting object with axe from player #{0}: {1}", previousPlayerHeldBy.playerClientId, ex));
                    }
                }
            IL_361:;
            }
        }
        if (flag)
        {
            RoundManager.PlayRandomClip(axeAudio, hitSFX, true, 1f, 0, 1000);
            FindObjectOfType<RoundManager>().PlayAudibleNoise(transform.position, 17f, 0.8f, 0, false, 0);
            if (!flag2 && num != -1)
            {
                axeAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[num].hitSurfaceSFX);
                WalkieTalkie.TransmitOneShotAudio(axeAudio, StartOfRound.Instance.footstepSurfaces[num].hitSurfaceSFX, 1f);
            }
            playerHeldBy.playerBodyAnimator.SetTrigger("axeHit");
            HitAxeServerRpc(num);
        }
    }

    [ServerRpc]
    public void HitAxeServerRpc(int hitSurfaceID)
    {
        {
            HitAxeClientRpc(hitSurfaceID);
        }
    }
    [ClientRpc]
    public void HitAxeClientRpc(int hitSurfaceID)
    {
        if (!IsOwner)
        {
            RoundManager.PlayRandomClip(axeAudio, hitSFX);
            if (hitSurfaceID != -1)
            {
                HitSurfaceWithAxe(hitSurfaceID);
            }
        }
    }
    private void HitSurfaceWithAxe(int hitSurfaceID)
    {
        axeAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[hitSurfaceID].hitSurfaceSFX);
        WalkieTalkie.TransmitOneShotAudio(axeAudio, StartOfRound.Instance.footstepSurfaces[hitSurfaceID].hitSurfaceSFX);
    }
}
