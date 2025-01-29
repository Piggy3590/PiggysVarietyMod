using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Unity.Netcode;

namespace PiggyVarietyMod.Patches
{
    public class TeslaGate : MonoBehaviour
    {
        public ParticleSystem telegraphParticle;
        public ParticleSystem idleParticle;

        public GameObject killTrigger;

        public CustomTouchInteractTrigger idleCTI;
        public CustomTouchInteractTrigger crackCTI;

        public AudioSource windUpSource;
        public AudioSource idleSource;
        public AudioSource crackSource;

        public List<PlayerControllerB> activatePlayerList = new List<PlayerControllerB>();
        public List<GameObject> activateList = new List<GameObject>();

        public List<PlayerControllerB> engagingPlayerList = new List<PlayerControllerB>();
        public List<GameObject> engagingList = new List<GameObject>();

        public bool coroutineRunning;
        public bool idleIntroPlayed;

        void Start()
        {
            //this.GetComponent<TerminalAccessibleObject>().terminalCodeEvent.AddListener(ForceTeslaTrigger);
            telegraphParticle = this.transform.GetChild(0).GetComponent<ParticleSystem>();
            idleParticle = this.transform.GetChild(1).GetComponent<ParticleSystem>();

            windUpSource = this.transform.GetChild(4).GetChild(0).GetComponent<AudioSource>();
            idleSource = this.transform.GetChild(4).GetChild(1).GetComponent<AudioSource>();
            crackSource = this.transform.GetChild(4).GetChild(2).GetComponent<AudioSource>();

            killTrigger = this.transform.GetChild(5).gameObject;
            CustomTouchInteractTrigger killTriggerScript = killTrigger.AddComponent<CustomTouchInteractTrigger>();
            killTriggerScript.isKillTrigger = true;

            crackCTI = this.transform.GetChild(2).gameObject.AddComponent<CustomTouchInteractTrigger>();
            crackCTI.teslaGate = this;

            idleCTI = this.transform.GetChild(3).gameObject.AddComponent<CustomTouchInteractTrigger>();
            idleCTI.teslaGate = this;
            idleCTI.isIdleTrigger = true;
            killTrigger.SetActive(false);

            windUpSource.volume = 0.45f * Plugin.teslaSoundVolume;
            idleSource.volume = 0.45f * Plugin.teslaSoundVolume;
            crackSource.volume = 0.45f * Plugin.teslaSoundVolume;

            StartCoroutine(ActivateLights(this.transform));
        }

        private static IEnumerator ActivateLights(Transform parent)
        {
            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (child == null)
                {
                    continue;
                }

                Light[] lights = child.GetComponents<Light>();

                foreach (Light light in lights)
                {
                    if (light == null)
                    {
                        continue;
                    }

                    light.gameObject.SetActive(true);
                }

                yield return new WaitForEndOfFrame();
                yield return ActivateLights(child);
            }
        }

        void Update()
        {
            if (activateList.Count > 0)
            {
                if (!idleIntroPlayed)
                {
                    idleSource.clip = Plugin.teslaIdleStart;
                    idleSource.Play();
                    idleSource.loop = false;
                    idleIntroPlayed = true;
                    idleParticle.Play();
                }
                else
                {
                    if (idleIntroPlayed && !idleSource.isPlaying)
                    {
                        idleSource.clip = Plugin.teslaIdle;
                        idleSource.loop = true;
                        idleSource.Play();
                        idleIntroPlayed = true;
                    }
                }
            }
            else
            {
                if (idleSource.isPlaying && idleIntroPlayed)
                {
                    idleSource.loop = false;
                    idleSource.Stop();
                    idleParticle.Stop();
                    idleSource.PlayOneShot(Plugin.teslaIdleEnd);
                    idleIntroPlayed = false;
                }
            }
            
            if (engagingList.Count > 0)
            {
                if (!coroutineRunning)
                {
                    StartCoroutine(EngageTeslaCoroutine());
                }
            }else
            {
                StopCoroutine(EngageTeslaCoroutine());
            }
        }

        private IEnumerator EngageTeslaCoroutine()
        {
            coroutineRunning = true;
            killTrigger.SetActive(false);
            windUpSource.PlayOneShot(Plugin.teslaBeep);
            windUpSource.PlayOneShot(Plugin.teslaWindUp);
            windUpSource.PlayOneShot(Plugin.teslaUnderbass);
            windUpSource.PlayOneShot(Plugin.teslaClimax);
            yield return new WaitForSeconds(0.75f);
            killTrigger.SetActive(true);

            foreach (PlayerControllerB player in engagingPlayerList)
            {
                if (player == StartOfRound.Instance.localPlayerController && Plugin.teslaShake)
                {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
                }
            }
            telegraphParticle.Play();
            windUpSource.Stop();
            crackSource.PlayOneShot(Plugin.teslaCrack);
            yield return new WaitForSeconds(0.65f);
            killTrigger.SetActive(false);
            yield return new WaitForSeconds(0.6f);
            coroutineRunning = false;
            yield break;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ForceTeslaTriggerServerRpc()
        {
            ForceTeslaTriggerClientRpc();
        }

        [ClientRpc]
        public void ForceTeslaTriggerClientRpc()
        {
            StartCoroutine(InstantTeslaCoroutine());
        }

        public void ForceTeslaTrigger(PlayerControllerB player)
        {
            ForceTeslaTriggerServerRpc();
        }

        private IEnumerator InstantTeslaCoroutine()
        {
            killTrigger.SetActive(true);
            foreach (PlayerControllerB player in engagingPlayerList)
            {
                if (player == StartOfRound.Instance.localPlayerController && Plugin.teslaShake)
                {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
                }
            }
            crackSource.PlayOneShot(Plugin.teslaCrack);
            yield return new WaitForSeconds(0.65f);
            killTrigger.SetActive(false);
            yield break;
        }
    }
}