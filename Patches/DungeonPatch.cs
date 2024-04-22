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
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace PiggyVarietyMod.Patches
{
    [HarmonyPatch(typeof(Dungeon))]
    internal class DungeonPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SpawnDoorPrefab")]
        private static void SpawnDoorPrefab_Prefix(Doorway a, Doorway b, RandomStream randomStream)
        {
            for (int i = 0; i < a.ConnectorPrefabWeights.Count; i++)
            {
                GameObjectWeight gameObjectWeight = a.ConnectorPrefabWeights[i];
                if (gameObjectWeight.GameObject.name == "TeslaGateSpawn")
                {
                    return;
                }
                if (gameObjectWeight.GameObject.name == "BigDoorSpawn")
                {
                    GameObjectWeight newWeight = new GameObjectWeight();
                    a.ConnectorPrefabWeights.Add(newWeight);
                    newWeight.Weight = 0.05f * Plugin.teslaSpawnWeight;
                    newWeight.GameObject = Plugin.teslaGateSpawn;
                    Plugin.mls.LogInfo("added tesla to " + a.gameObject.name);
                }
            }
            for (int i = 0; i < b.ConnectorPrefabWeights.Count; i++)
            {
                GameObjectWeight gameObjectWeight = b.ConnectorPrefabWeights[i];
                if (gameObjectWeight.GameObject.name == "TeslaGateSpawn")
                {
                    return;
                }
                if (gameObjectWeight.GameObject.name == "BigDoorSpawn")
                {
                    GameObjectWeight newWeight = new GameObjectWeight();
                    b.ConnectorPrefabWeights.Add(newWeight);
                    newWeight.Weight = 0.07f * Plugin.teslaSpawnWeight;
                    newWeight.GameObject = Plugin.teslaGateSpawn;
                    Plugin.mls.LogInfo("added tesla to " + b.gameObject.name);
                }
            }
        }
    }
}