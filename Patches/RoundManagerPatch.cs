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
            /*
            Plugin.mls.LogInfo("TESTING flow: " + ___dungeonFlowTypes[0]);
            Plugin.mls.LogInfo("TESTING archetype: " + ___dungeonFlowTypes[0].Lines[0].DungeonArchetypes[0]);
            Plugin.mls.LogInfo("TESTING archetype name: " + ___dungeonFlowTypes[0].Lines[0].DungeonArchetypes[0].name);
            Plugin.mls.LogInfo("TESTING flow namne: " + ___dungeonFlowTypes[0].name);
            foreach (DungeonFlow dungeonFlow in ___dungeonFlowTypes)
            {
                Plugin.mls.LogInfo("Found DungeonFlow: " + dungeonFlow.ToString());
                if (dungeonFlow.Lines != null)
                {
                    foreach (GraphLine line in dungeonFlow.Lines)
                    {
                        Plugin.mls.LogInfo("[TESLA GATE] Finding Dungeon Archetype in " + dungeonFlow.name);
                        if (line.DungeonArchetypes != null)
                        {
                            foreach (DungeonArchetype archetype in line.DungeonArchetypes)
                            {
                                if (archetype.TileSets != null)
                                {
                                    foreach (TileSet tileSet in archetype.TileSets)
                                    {
                                        FindBigDoorRoomInTileSet(tileSet);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            */
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