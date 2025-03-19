using System.Collections.Specialized;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot
{
    [HarmonyPatch(typeof(SpawnArea), nameof(SpawnArea.Awake))]
    public static class SpawnArea_Awake_Patch
    {
        public static void Prefix(SpawnArea __instance)
        {
            EpicLoot.Log($"SpawnArea AWAKE!");

            if(__instance != null)
            {
                /*                EpicLoot.Log($"SpawnArea m_farRadius {__instance.m_farRadius}");
                                EpicLoot.Log($"SpawnArea m_maxNear {__instance.m_maxNear}");
                                EpicLoot.Log($"SpawnArea m_maxTotal {__instance.m_maxTotal}");
                                EpicLoot.Log($"SpawnArea m_spawnIntervalSec {__instance.m_spawnIntervalSec}");
                                EpicLoot.Log($"SpawnArea spawnRadius {__instance.m_spawnRadius}");
                                EpicLoot.Log($"SpawnArea m_spawnTimer {__instance.m_spawnTimer}");
                                EpicLoot.Log($"SpawnArea SpawnArea.m_triggerDistance {__instance.m_triggerDistance}");*/

                if (__instance.name == "Spawner_GreydwarfNest(Clone)")
                {
                    var destructibleComponent = __instance.GetComponentInParent<Destructible>();
                    if (destructibleComponent != null)
                    {
                        // EpicLoot.Log($"Local Spawner_GreydwarfNest(Clone) health before! {destructibleComponent.m_health}");

                        if (__instance.FindSpawnPoint(ZNetScene.instance.GetPrefab("Spawner_GreydwarfNest"), out UnityEngine.Vector3 point))
                        {
                            var plainPos = new Vector3(point.x, 0, point.z);
                            // EpicLoot.Log($"Local spawn point! {point}, plain point {plainPos}");
                            var distance = plainPos.magnitude;
                            // EpicLoot.Log($"Local distance! {distance}");
                            if (distance > 6500)
                            {
                                destructibleComponent.m_health = 3650;
                                __instance.gameObject.transform.localScale = new Vector3(2.25f, 2.25f, 2.25f);

                                __instance.m_nearRadius = 30;
                                __instance.m_spawnRadius = 9;
                                __instance.m_maxNear = 5;
                                __instance.m_maxTotal = 200;
                                __instance.m_spawnIntervalSec = 10;
                                __instance.m_triggerDistance = 180;
                            }
                            else if (distance > 3500)
                            {
                                destructibleComponent.m_health = 1350;
                                __instance.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                                __instance.m_nearRadius = 30;
                                __instance.m_spawnRadius = 6;
                                __instance.m_maxNear = 5;
                                __instance.m_maxTotal = 200;
                                __instance.m_spawnIntervalSec = 10;
                                __instance.m_triggerDistance = 135;
                            }
                            else
                            {
                                destructibleComponent.m_health = 500;

                                __instance.m_nearRadius = 30;
                                __instance.m_spawnRadius = 4;
                                __instance.m_maxNear = 5;
                                __instance.m_maxTotal = 200;
                                __instance.m_spawnIntervalSec = 10;
                                __instance.m_triggerDistance = 90;
                            }

/*                            EpicLoot.Log($"Local Spawner_GreydwarfNest(Clone) health after! {destructibleComponent.m_health}");
                            EpicLoot.Log($"SpawnArea m_farRadius {__instance.m_farRadius}");
                            EpicLoot.Log($"SpawnArea m_maxNear {__instance.m_maxNear}");
                            EpicLoot.Log($"SpawnArea m_maxTotal {__instance.m_maxTotal}");
                            EpicLoot.Log($"SpawnArea m_spawnIntervalSec {__instance.m_spawnIntervalSec}");
                            EpicLoot.Log($"SpawnArea spawnRadius {__instance.m_spawnRadius}");
                            EpicLoot.Log($"SpawnArea m_spawnTimer {__instance.m_spawnTimer}");
                            EpicLoot.Log($"SpawnArea SpawnArea.m_triggerDistance {__instance.m_triggerDistance}");*/
                        }
                    }
                }
                else if(__instance.name == "BonePileSpawner(Clone)")
                {
                    var destructibleComponent = __instance.GetComponentInParent<Destructible>();
                    if (destructibleComponent != null)
                    {
                        // EpicLoot.Log($"Local BonePileSpawner(Clone) health before! {destructibleComponent.m_health}");

                        if (__instance.FindSpawnPoint(ZNetScene.instance.GetPrefab("BonePileSpawner"), out UnityEngine.Vector3 point))
                        {
                            ZoneSystem.instance.GetGroundData(ref point, out UnityEngine.Vector3 normal, out Heightmap.Biome biome, out Heightmap.BiomeArea biomeArea, out Heightmap hmap);
                            /*                     None = 0,
                                                 Meadows = 1,
                                                 Swamp = 2,
                                                 Mountain = 4,
                                                 BlackForest = 8,
                                                 Plains = 0x10,
                                                 AshLands = 0x20,
                                                 DeepNorth = 0x40,
                                                 Ocean = 0x100,
                                                 Mistlands = 0x200,
                                                 All = 0x37F*/

                            var plainPos = new Vector3(point.x, 0, point.z);
                            // EpicLoot.Log($"Local spawn point! {point}, plain point {plainPos}");
                            var distance = plainPos.magnitude;
                            // EpicLoot.Log($"Local distance! {distance}");

                            if (biome == Heightmap.Biome.Mountain)
                            {
                                EpicLoot.Log($"Biome mountain!");
                                if (distance > 6500)
                                {
                                    destructibleComponent.m_health = 13300;
                                    __instance.gameObject.transform.localScale = new Vector3(2.8561f, 2.8561f, 2.8561f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 2.28f;
                                    __instance.m_maxNear = 6;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 5;
                                    __instance.m_triggerDistance = 90;
                                }
                                else if (distance > 3500)
                                {
                                    destructibleComponent.m_health = 4925;
                                    __instance.gameObject.transform.localScale = new Vector3(2.197f, 2.197f, 2.197f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 2.28f;
                                    __instance.m_maxNear = 6;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 5;
                                    __instance.m_triggerDistance = 75;
                                }
                                else
                                {
                                    destructibleComponent.m_health = 1825;
                                    __instance.gameObject.transform.localScale = new Vector3(1.69f, 1.69f, 1.69f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 2.28f;
                                    __instance.m_maxNear = 6;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 5;
                                    __instance.m_triggerDistance = 60;
                                }
                            }
                            else if (biome == Heightmap.Biome.Swamp)
                            {
                                EpicLoot.Log($"Biome swamp!");
                                if (distance > 5000)
                                {
                                    destructibleComponent.m_health = 4925;
                                    __instance.gameObject.transform.localScale = new Vector3(2.197f, 2.197f, 2.197f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 2.28f;
                                    __instance.m_maxNear = 3;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 15;
                                    __instance.m_triggerDistance = 180;
                                }
                                else if (distance > 3500)
                                {
                                    destructibleComponent.m_health = 1825;
                                    __instance.gameObject.transform.localScale = new Vector3(1.69f, 1.69f, 1.69f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 2.28f;
                                    __instance.m_maxNear = 3;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 15;
                                    __instance.m_triggerDistance = 135;
                                }
                                else
                                {
                                    destructibleComponent.m_health = 675;
                                    __instance.gameObject.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 2.28f;
                                    __instance.m_maxNear = 3;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 15;
                                    __instance.m_triggerDistance = 90;
                                }
                            }
                            else
                            {
                                EpicLoot.Log($"Biome BF or other!");
                                if (distance > 6500)
                                {
                                    destructibleComponent.m_health = 1825;
                                    __instance.gameObject.transform.localScale = new Vector3(1.69f, 1.69f, 1.69f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 2.28f;
                                    __instance.m_maxNear = 3;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 5;
                                    __instance.m_triggerDistance = 60;
                                }
                                else if (distance > 3500)
                                {
                                    destructibleComponent.m_health = 675;
                                    __instance.gameObject.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 2.28f;
                                    __instance.m_maxNear = 3;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 5;
                                    __instance.m_triggerDistance = 45;
                                }
                                else
                                {
                                    destructibleComponent.m_health = 250;

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 2.28f;
                                    __instance.m_maxNear = 3;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 5;
                                    __instance.m_triggerDistance = 30;
                                }
                            }

/*                            EpicLoot.Log($"Local BonePileSpawner(Clone) health after! {destructibleComponent.m_health}");
                            EpicLoot.Log($"SpawnArea m_farRadius {__instance.m_farRadius}");
                            EpicLoot.Log($"SpawnArea m_maxNear {__instance.m_maxNear}");
                            EpicLoot.Log($"SpawnArea m_maxTotal {__instance.m_maxTotal}");
                            EpicLoot.Log($"SpawnArea m_spawnIntervalSec {__instance.m_spawnIntervalSec}");
                            EpicLoot.Log($"SpawnArea spawnRadius {__instance.m_spawnRadius}");
                            EpicLoot.Log($"SpawnArea m_spawnTimer {__instance.m_spawnTimer}");
                            EpicLoot.Log($"SpawnArea SpawnArea.m_triggerDistance {__instance.m_triggerDistance}");*/
                        }
                    }
                }
                else if(__instance.name == "Spawner_DraugrPile(Clone)")
                {
                    var destructibleComponent = __instance.GetComponentInParent<Destructible>();
                    if (destructibleComponent != null)
                    {
                        // EpicLoot.Log($"Local Spawner_GreydwarfNest(Clone) health before! {destructibleComponent.m_health}");

                        if (__instance.FindSpawnPoint(ZNetScene.instance.GetPrefab("Spawner_DraugrPile"), out UnityEngine.Vector3 point))
                        {
                            ZoneSystem.instance.GetGroundData(ref point, out UnityEngine.Vector3 normal, out Heightmap.Biome biome, out Heightmap.BiomeArea biomeArea, out Heightmap hmap);
                            /*                     None = 0,
                                                 Meadows = 1,
                                                 Swamp = 2,
                                                 Mountain = 4,
                                                 BlackForest = 8,
                                                 Plains = 0x10,
                                                 AshLands = 0x20,
                                                 DeepNorth = 0x40,
                                                 Ocean = 0x100,
                                                 Mistlands = 0x200,
                                                 All = 0x37F*/

                            var plainPos = new Vector3(point.x, 0, point.z);
                            // EpicLoot.Log($"Local spawn point! {point}, plain point {plainPos}");
                            var distance = plainPos.magnitude;
                            // EpicLoot.Log($"Local distance! {distance}");

                            if (biome == Heightmap.Biome.Mountain)
                            {
                                EpicLoot.Log($"Biome mountain!");
                                if (distance > 6500)
                                {
                                    destructibleComponent.m_health = 13300;
                                    __instance.gameObject.transform.localScale = new Vector3(2.8561f, 2.8561f, 2.8561f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 3;
                                    __instance.m_maxNear = 4;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 8;
                                    __instance.m_triggerDistance = 90;
                                }
                                else if (distance > 3500)
                                {
                                    destructibleComponent.m_health = 4925;
                                    __instance.gameObject.transform.localScale = new Vector3(2.197f, 2.197f, 2.197f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 3;
                                    __instance.m_maxNear = 4;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 8;
                                    __instance.m_triggerDistance = 75;
                                }
                                else
                                {
                                    destructibleComponent.m_health = 1825;
                                    __instance.gameObject.transform.localScale = new Vector3(1.69f, 1.69f, 1.69f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 3;
                                    __instance.m_maxNear = 4;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 8;
                                    __instance.m_triggerDistance = 60;
                                }
                            }
                            else if (biome == Heightmap.Biome.Swamp)
                            {
                                EpicLoot.Log($"Biome swamp!");
                                if (distance > 5000)
                                {
                                    destructibleComponent.m_health = 4925;
                                    __instance.gameObject.transform.localScale = new Vector3(2.197f, 2.197f, 2.197f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 3;
                                    __instance.m_maxNear = 2;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 8;
                                    __instance.m_triggerDistance = 180;
                                }
                                else if (distance > 3500)
                                {
                                    destructibleComponent.m_health = 1825;
                                    __instance.gameObject.transform.localScale = new Vector3(1.69f, 1.69f, 1.69f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 3;
                                    __instance.m_maxNear = 2;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 8;
                                    __instance.m_triggerDistance = 135;
                                }
                                else
                                {
                                    destructibleComponent.m_health = 675;
                                    __instance.gameObject.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 3;
                                    __instance.m_maxNear = 2;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 8;
                                    __instance.m_triggerDistance = 90;
                                }
                            }
                            else
                            {
                                EpicLoot.Log($"Biome Meadows or other!");
                                if (distance > 3500)
                                {
                                    destructibleComponent.m_health = 675;
                                    __instance.gameObject.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 3;
                                    __instance.m_maxNear = 3;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 8;
                                    __instance.m_triggerDistance = 45;
                                }
                                else
                                {
                                    destructibleComponent.m_health = 250;

                                    __instance.m_nearRadius = 15;
                                    __instance.m_spawnRadius = 3;
                                    __instance.m_maxNear = 3;
                                    __instance.m_maxTotal = 200;
                                    __instance.m_spawnIntervalSec = 8;
                                    __instance.m_triggerDistance = 30;
                                }
                            }

                            /*                            EpicLoot.Log($"Local Spawner_GreydwarfNest(Clone) health after! {destructibleComponent.m_health}");
                                                        EpicLoot.Log($"SpawnArea m_farRadius {__instance.m_farRadius}");
                                                        EpicLoot.Log($"SpawnArea m_maxNear {__instance.m_maxNear}");
                                                        EpicLoot.Log($"SpawnArea m_maxTotal {__instance.m_maxTotal}");
                                                        EpicLoot.Log($"SpawnArea m_spawnIntervalSec {__instance.m_spawnIntervalSec}");
                                                        EpicLoot.Log($"SpawnArea spawnRadius {__instance.m_spawnRadius}");
                                                        EpicLoot.Log($"SpawnArea m_spawnTimer {__instance.m_spawnTimer}");
                                                        EpicLoot.Log($"SpawnArea SpawnArea.m_triggerDistance {__instance.m_triggerDistance}");*/
                        }
                    }
                }
                else
                {
                    EpicLoot.Log($"Other Spawn type! {__instance.name}");
                }
            }

/*            var m_prefab = ZNetScene.instance.GetPrefab("Spawner_GreydwarfNest");
            EpicLoot.Log($"SpawnArea Spawner_GreydwarfNest! {m_prefab}");
            if(m_prefab != null)
            {
                var destructible = m_prefab.GetComponent<Destructible>();
                if(destructible != null)
                {
                    EpicLoot.Log($"SpawnArea Spawner_GreydwarfNest Destructible! {destructible}");
                    EpicLoot.Log($"SpawnArea Spawner_GreydwarfNest Destructible health! {destructible.m_health}");
                    destructible.m_health = 2000;
                }

                m_prefab.transform.localScale = new Vector3(2, 2, 2);
            }*/
        }
    }
}
