using DunGen;
using DunGen.Graph;
using HarmonyLib;
using UnityEngine;

namespace PiggyVarietyMod.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void Start_Postfix(ref DungeonFlow[] ___dungeonFlowTypes)
        {
            if (Plugin.translateKorean)
            {
                Plugin.gummyFlashlight.itemName = "젤리 손전등";
            }
        }

        static void FindBigDoorRoomInTileSet(TileSet tileSet)
        {
            Plugin.mls.LogInfo("[TESLA GATE] Found Tileset: " + tileSet.name);
            if (tileSet.TileWeights.Weights != null)
            {
                foreach (GameObjectChance weight in tileSet.TileWeights.Weights)
                {
                    Plugin.mls.LogInfo("[TESLA GATE] Found weight in Tileset: " + tileSet.name);
                    if (weight.Value.transform != null)
                    {
                        foreach (Transform child in weight.Value.transform)
                        {
                            if (child.GetComponent<Doorway>())
                            {
                                Plugin.mls.LogInfo("[TESLA GATE] Find Doorway: " + child.gameObject.name);
                                Doorway doorway = child.GetComponent<Doorway>();
                                foreach (GameObjectWeight gameObjectWeight in doorway.ConnectorPrefabWeights)
                                {
                                    if (gameObjectWeight.GameObject.name == "BigDoorSpawn")
                                    {
                                        if (gameObjectWeight.GameObject.name == "TeslaGateSpawn")
                                        {
                                            return;
                                        }
                                        GameObjectWeight newWeight = new GameObjectWeight();
                                        newWeight.Weight = 100f;
                                        newWeight.GameObject = Plugin.teslaGateSpawn;
                                        doorway.ConnectorPrefabWeights.Add(newWeight);
                                        Plugin.mls.LogInfo("added tesla to " + child.gameObject.name);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}