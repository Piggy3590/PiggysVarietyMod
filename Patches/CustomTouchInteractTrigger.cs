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
                if (isKillTrigger)
                {
                    if (playerScript == StartOfRound.Instance.localPlayerController && !playerScript.isPlayerDead)
                    {
                        playerScript.KillPlayer(Vector3.zero, true, CauseOfDeath.Electrocution);
                    }
                }
                else
                {
                    teslaGate.engagingPlayerList.Add(playerScript);
                    teslaGate.engagingList.Add(collider.gameObject);
                }
            }

            if (collider.transform.parent.GetComponent<EnemyAICollisionDetect>() != null)
            {
                EnemyAICollisionDetect enemyDetection = collider.gameObject.GetComponent<EnemyAICollisionDetect>();
                if (isIdleTrigger)
                {
                    teslaGate.activateList.Add(collider.gameObject);
                }
                if (isKillTrigger)
                {
                    if (enemyDetection != null && enemyDetection.mainScript != null && enemyDetection.mainScript.IsOwner && enemyDetection.mainScript.enemyType.canDie && !enemyDetection.mainScript.isEnemyDead)
                    {
                        enemyDetection.mainScript.KillEnemyOnOwnerClient(false);
                    }
                }
                else
                {
                    teslaGate.engagingList.Add(collider.gameObject);
                }
            }
        }

        void OnTriggerStay(Collider collider)
        {
            if (collider.transform.parent.GetComponent<EnemyAICollisionDetect>() != null)
            {
                EnemyAICollisionDetect enemyDetection = collider.gameObject.GetComponent<EnemyAICollisionDetect>();
                if (isKillTrigger)
                {
                    if (enemyDetection != null && enemyDetection.mainScript != null && enemyDetection.mainScript.IsOwner && enemyDetection.mainScript.enemyType.canDie && !enemyDetection.mainScript.isEnemyDead)
                    {
                        enemyDetection.mainScript.KillEnemyOnOwnerClient(false);
                    }
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