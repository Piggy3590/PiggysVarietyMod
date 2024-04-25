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
using UnityEngine.ParticleSystemJobs;
using static System.Net.Mime.MediaTypeNames;
using System.Collections;

namespace PiggyVarietyMod.Patches
{
    public class CustomTouchInteractTrigger : MonoBehaviour
    {
        public bool isIdleTrigger;
        public bool isKillTrigger;
        public TeslaGate teslaGate;

        void OnTriggerEnter(Collider collider)
        {
            if (collider.transform.parent.GetComponent<PlayerControllerB>() != null)
            {
                PlayerControllerB playerScript = collider.transform.parent.GetComponent<PlayerControllerB>();
                if (isIdleTrigger)
                {
                    teslaGate.activatePlayerList.Add(playerScript);
                    teslaGate.activateList.Add(collider.gameObject);
                }
                else if (!isIdleTrigger && !isKillTrigger)
                {
                    teslaGate.engagingPlayerList.Add(playerScript);
                    teslaGate.engagingList.Add(collider.gameObject);
                }
            }

            if (collider.transform.parent.GetComponent<EnemyAICollisionDetect>() != null)
            {
                if (collider.gameObject.GetComponent<EnemyAICollisionDetect>())
                {
                    EnemyAICollisionDetect enemyDetection = collider.gameObject.GetComponent<EnemyAICollisionDetect>();
                    IHittable hittable;
                    if (enemyDetection.transform.TryGetComponent<IHittable>(out hittable))
                    {
                        Plugin.mls.LogInfo("Tesla gate detected enemy: " + enemyDetection.mainScript.enemyType.enemyName + ", Idle: " + isIdleTrigger + ", Kill: " + isKillTrigger);
                        if (isIdleTrigger)
                        {
                            teslaGate.activateList.Add(collider.gameObject);
                        }
                        if (isKillTrigger)
                        {
                            if (enemyDetection != null && enemyDetection.mainScript != null && enemyDetection.mainScript.IsOwner && enemyDetection.mainScript.enemyType.canDie && !enemyDetection.mainScript.isEnemyDead)
                            {
                                hittable.Hit(5, Vector3.zero, null, true, -1);
                            }
                        }
                        else
                        {
                            teslaGate.engagingList.Add(collider.gameObject);
                        }
                    }
                }
            }
        }

        void OnTriggerStay(Collider collider)
        {
            if (isKillTrigger)
            {
                PlayerControllerB component = collider.gameObject.GetComponent<PlayerControllerB>();
                if (component != null && component == GameNetworkManager.Instance.localPlayerController && !component.isPlayerDead)
                {
                    GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.down * 17f, true, CauseOfDeath.Electrocution, 0);
                    return;
                }
                EnemyAICollisionDetect component3 = collider.gameObject.GetComponent<EnemyAICollisionDetect>();
                if (component3 != null && component3.mainScript != null && component3.mainScript.IsOwner && component3.mainScript.enemyType.canDie && !component3.mainScript.isEnemyDead)
                {
                    component3.mainScript.KillEnemyOnOwnerClient(false);
                }
            }
        }

        void OnTriggerExit(Collider collider)
        {
            if (collider.transform.parent.GetComponent<PlayerControllerB>() != null)
            {
                PlayerControllerB playerScript = collider.transform.parent.GetComponent<PlayerControllerB>();
                teslaGate.engagingPlayerList.Remove(playerScript);
                teslaGate.engagingList.Remove(collider.gameObject);
                if (isIdleTrigger)
                {
                    teslaGate.activatePlayerList.Remove(playerScript);
                    teslaGate.activateList.Remove(collider.gameObject);
                }
            }

            if (collider.transform.parent.GetComponent<EnemyAICollisionDetect>() != null)
            {
                EnemyAICollisionDetect enemyDetection = collider.gameObject.GetComponent<EnemyAICollisionDetect>();
                teslaGate.engagingList.Remove(collider.gameObject);
                if (isIdleTrigger)
                {
                    teslaGate.activateList.Remove(collider.gameObject);
                }
            }
        }
    }
}