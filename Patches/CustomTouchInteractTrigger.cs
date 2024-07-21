using GameNetcodeStuff;
using UnityEngine;

namespace PiggyVarietyMod.Patches
{
    public class CustomTouchInteractTrigger : MonoBehaviour
    {
        public bool isIdleTrigger;
        public bool isKillTrigger;
        public TeslaGate teslaGate;

        void OnTriggerEnter(Collider collider)
        {
            PlayerControllerB playerScript = collider.GetComponent<PlayerControllerB>();
            if (playerScript != null)
            {
                Plugin.mls.LogInfo("Tesla gate detected player: " + playerScript.playerUsername + ", Idle: " + isIdleTrigger + ", Kill: " + isKillTrigger);
                if (!playerScript.isPlayerDead)
                {
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
            }

            EnemyAICollisionDetect enemyDetection = collider.GetComponent<EnemyAICollisionDetect>();
            if (enemyDetection != null)
            {
                IHittable hittable;
                if (enemyDetection.transform.TryGetComponent<IHittable>(out hittable))
                {
                    Plugin.mls.LogInfo("Tesla gate detected enemy: " + enemyDetection.mainScript.enemyType.enemyName + ", Idle: " + isIdleTrigger + ", Kill: " + isKillTrigger);
                    if (isIdleTrigger)
                    {
                        /*
                        teslaGate.activateList.Add(collider.gameObject);
                        */
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
                        /*
                        teslaGate.engagingList.Add(collider.gameObject);
                        */
                    }
                }
            }
        }

        void OnTriggerStay(Collider collider)
        {
            if (isKillTrigger)
            {
                PlayerControllerB player = collider.GetComponent<PlayerControllerB>();
                if (player != null && player == GameNetworkManager.Instance.localPlayerController && !player.isPlayerDead)
                {
                    GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.down * 17f, true, CauseOfDeath.Electrocution, 0);
                    return;
                }
                EnemyAICollisionDetect enemyCollision = collider.GetComponent<EnemyAICollisionDetect>();
                if (enemyCollision != null && enemyCollision.mainScript != null && enemyCollision.mainScript.IsOwner &&
                    enemyCollision.mainScript.enemyType.canDie && !enemyCollision.mainScript.isEnemyDead)
                {
                    enemyCollision.mainScript.KillEnemyOnOwnerClient(false);
                }
            }
        }

        void OnTriggerExit(Collider collider)
        {
            PlayerControllerB playerScript = collider.GetComponent<PlayerControllerB>();
            if (playerScript != null)
            {
                teslaGate.engagingPlayerList.Remove(playerScript);
                teslaGate.engagingList.Remove(collider.gameObject);
                if (isIdleTrigger)
                {
                    teslaGate.activatePlayerList.Remove(playerScript);
                    teslaGate.activateList.Remove(collider.gameObject);
                }
            }

            /*
            if (collider.transform.parent.GetComponent<EnemyAICollisionDetect>() != null)
            {
                EnemyAICollisionDetect enemyDetection = collider.gameObject.GetComponent<EnemyAICollisionDetect>();
                teslaGate.engagingList.Remove(collider.gameObject);
                if (isIdleTrigger)
                {
                    teslaGate.activateList.Remove(collider.gameObject);
                }
            }
            */
        }
    }
}