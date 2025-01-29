using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace EpicLoot.Adventure
{
    public enum MinimapPinQueueTask
    {
        AddTreasurePin,
        AddBountyPin,
        RemoveTreasurePin,
        RemoveBountyPin,
        RefreshAll
    }
    public class PinJob
    {
        public MinimapPinQueueTask Task { get; set; }
        public KeyValuePair<Tuple<int, Heightmap.Biome>, AreaPinInfo> TreasurePin { get; set; }
        public KeyValuePair<string, AreaPinInfo> BountyPin { get; set; }
        public bool DebugMode { get; set; }
    }
    public class AreaPinInfo
    {
        public Minimap.PinData Pin { get; set; }
        public Minimap.PinData Area { get; set; }
        public Minimap.PinData DebugPin { get; set; }
        
        //Pin Data
        public Vector3 Position { get; set; }
        public Minimap.PinType Type { get; set; }
        public string Name { get; set; }
        public bool Save { get; set; }
        public bool Checked { get; set; }
        public long OwnerId { get; set; }

        public AreaPinInfo()
        {
            Name = string.Empty;
            Save = false;
            Checked = false;
            OwnerId = 0L;
        }
    }
    
    [RequireComponent(typeof(Minimap))]
    public class MinimapController : MonoBehaviour
    {
        private static readonly Queue<PinJob> MinimapPinQueue = new();
        
        private Minimap _minimap;
        private static Player _player;
        
        public const float AreaScale = 2.1f;

        public static readonly Dictionary<Tuple<int, Heightmap.Biome>, AreaPinInfo> TreasureMapPins = new();
        public static readonly Dictionary<string, AreaPinInfo> BountyPins = new();
        public static bool DebugMode;
        private static bool _enabled;
        private static bool _drawingOfCirclesRequired = true;

        public virtual void Awake()
        {
            _minimap = GetComponent<Minimap>();
            
            if (!_minimap.m_icons.Exists(x => x.m_name == EpicLoot.TreasureMapPinType))
            {
                _minimap.m_icons.Add(new Minimap.SpriteData { m_name = EpicLoot.TreasureMapPinType, m_icon = EpicLoot.Assets.MapIconTreasureMap });
            }
            if (!_minimap.m_icons.Exists(x => x.m_name == EpicLoot.BountyPinType))
            {
                _minimap.m_icons.Add(new Minimap.SpriteData { m_name = EpicLoot.BountyPinType, m_icon = EpicLoot.Assets.MapIconBounty });
            }

            // DrawDistanceLevelCircles();
        }

        private void Start()
        {
            TreasureMapPins.Clear();
            BountyPins.Clear();
            
            if (_minimap.m_visibleIconTypes.Length < (int)EpicLoot.TreasureMapPinType + 1)
            {
                _minimap.m_visibleIconTypes = new bool[(int)EpicLoot.TreasureMapPinType + 1];
                for (var index = 0; index < _minimap.m_visibleIconTypes.Length; ++index)
                {
                    _minimap.m_visibleIconTypes[index] = true;
                }
            }
            
            var pinJob = new PinJob
            {
                Task = MinimapPinQueueTask.RefreshAll
            };
            AddPinJobToQueue(pinJob);

            // DrawDistanceLevelCircles();
        }

        public virtual void Update()
        {
            if(_drawingOfCirclesRequired)
            {
                DrawDistanceLevelCircles();
                _drawingOfCirclesRequired = false;
            }

            if (!_enabled)
                return;

            while (MinimapPinQueue.Any())
            {
                ProcessMinimapPinTask(MinimapPinQueue.Dequeue());
            }
        }

        private void OnDestroy()
        {
            TreasureMapPins.Clear();
            BountyPins.Clear();

            _enabled = false;
        }
        
        //Static Methods
        public static void AddPinJobToQueue(PinJob pinJob)
        {
            if (pinJob != null)
            {
                MinimapPinQueue.Enqueue(pinJob);
            }
        }

        public static void Enable(Player player)
        {
            _player = player;
            _enabled = true;
        }
        
        private void ProcessMinimapPinTask(PinJob pinJob)
        {
            switch (pinJob.Task)
            {
                case MinimapPinQueueTask.AddBountyPin:
                case MinimapPinQueueTask.AddTreasurePin:
                    AddPin(pinJob);
                    break;
                case MinimapPinQueueTask.RemoveTreasurePin:
                    RemovePin(pinJob.TreasurePin.Value);
                    TreasureMapPins.Remove(pinJob.TreasurePin.Key);
                    break;
                case MinimapPinQueueTask.RemoveBountyPin:
                    RemovePin(pinJob.BountyPin.Value);
                    BountyPins.Remove(pinJob.BountyPin.Key);
                    break;
                case MinimapPinQueueTask.RefreshAll:
                    RefreshPins();
                    break;
            }
        }

        private void AddPin(PinJob pinJob)
        {
            AreaPinInfo newPin = null;
            switch (pinJob.Task)
            {
                case MinimapPinQueueTask.AddBountyPin:
                    newPin = pinJob.BountyPin.Value;
                    break;
                case MinimapPinQueueTask.AddTreasurePin:
                    newPin = pinJob.TreasurePin.Value;
                    break;
            }

            if (newPin == null) return;
            
            //Add Area Pin
            newPin.Area = _minimap.AddPin(newPin.Position, Minimap.PinType.EventArea, string.Empty, false, false);
            newPin.Area.m_worldSize = AdventureDataManager.Config.TreasureMap.MinimapAreaRadius * AreaScale;
                    
            //Add Pin
            newPin.Pin = _minimap.AddPin(newPin.Position, newPin.Type, newPin.Name, false, false);
                    
            //Add Debug Pin
            if (pinJob.DebugMode)
            {
                newPin.DebugPin = _minimap.AddPin(newPin.Position, Minimap.PinType.Icon3,
                    $"{newPin.Position.x:0.0}, {newPin.Position.z:0.0}", false, false);
            }
            
            switch (pinJob.Task)
            {
                    
                case MinimapPinQueueTask.AddBountyPin:
                    BountyPins[pinJob.BountyPin.Key] = pinJob.BountyPin.Value;
                    break;
                case MinimapPinQueueTask.AddTreasurePin:
                    TreasureMapPins[pinJob.TreasurePin.Key] = pinJob.TreasurePin.Value;
                    break;
            }
        }

        private void RemovePin(AreaPinInfo pinEntry)
        {
            _minimap.RemovePin(pinEntry.Pin);
            _minimap.RemovePin(pinEntry.Area);
            if (pinEntry.DebugPin != null)
            {
                _minimap.RemovePin(pinEntry.DebugPin);
            }
        }

        private void RefreshPins()
        {
            if (_player == null)
                return;

            var adventureSaveData = _player.GetAdventureSaveData();
            if (adventureSaveData == null)
                return;

            var unfoundTreasureChests = adventureSaveData.GetUnfoundTreasureChests();
            var oldPins = TreasureMapPins.Where(pinEntry => !unfoundTreasureChests
                .Exists(x => x.Interval == pinEntry.Key.Item1 && x.Biome == pinEntry.Key.Item2)).ToList();
            foreach (var pinEntry in oldPins)
            {
                var pinJob = new PinJob
                {
                    Task = MinimapPinQueueTask.RemoveTreasurePin,
                    DebugMode = DebugMode,
                    TreasurePin = new KeyValuePair<Tuple<int, Heightmap.Biome>, AreaPinInfo>(pinEntry.Key, pinEntry.Value)
                };

                AddPinJobToQueue(pinJob);
            }

            foreach (var chestInfo in unfoundTreasureChests)
            {
                var key = new Tuple<int, Heightmap.Biome>(chestInfo.Interval, chestInfo.Biome);
                if (!TreasureMapPins.ContainsKey(key))
                {
                    var pinInfo = new AreaPinInfo
                    {
                        Position = chestInfo.Position + chestInfo.MinimapCircleOffset,
                        Type = EpicLoot.TreasureMapPinType,
                        Name = Localization.instance.Localize("$mod_epicloot_treasurechest_minimappin",
                            Localization.instance.Localize($"$biome_{chestInfo.Biome.ToString().ToLowerInvariant()}"),
                            (chestInfo.Interval + 1).ToString())
                    };

                    var pinJob = new PinJob
                    {
                        Task = MinimapPinQueueTask.AddTreasurePin,
                        DebugMode = DebugMode,
                        TreasurePin = new KeyValuePair<Tuple<int, Heightmap.Biome>, AreaPinInfo>(key, pinInfo)
                    };

                    AddPinJobToQueue(pinJob);
                }
            }

            var currentBounties = adventureSaveData.GetInProgressBounties();
            var oldBountyPins = BountyPins.Where(pinEntry => !currentBounties.Exists(x => x.ID == pinEntry.Key)).ToList();
            foreach (var pinEntry in oldBountyPins)
            {
                var pinJob = new PinJob
                {
                    Task = MinimapPinQueueTask.RemoveBountyPin,
                    DebugMode = DebugMode,
                    BountyPin = new KeyValuePair<string, AreaPinInfo>(pinEntry.Key, pinEntry.Value)
                };

                AddPinJobToQueue(pinJob);
            }

            foreach (var bounty in currentBounties)
            {
                var key = bounty.ID;
                if (!BountyPins.ContainsKey(key))
                {
                    var pinInfo = new AreaPinInfo
                    {
                        Position = bounty.Position + bounty.MinimapCircleOffset,
                        Type = EpicLoot.BountyPinType,
                        Name = Localization.instance.Localize("$mod_epicloot_bounties_minimappin", AdventureDataManager.GetBountyName(bounty))
                    };

                    var pinJob = new PinJob
                    {
                        Task = MinimapPinQueueTask.AddBountyPin,
                        DebugMode = DebugMode,
                        BountyPin = new KeyValuePair<string, AreaPinInfo>(key, pinInfo)
                    };

                    AddPinJobToQueue(pinJob);
                }
            }
        }

        private void DrawDistanceLevelCircles()
        {
            if (!_minimap)
            {
                EpicLoot.LogWarning("Minimap pointer is null");
                return;
            }
            if (!_minimap.m_mapTexture || !_minimap.m_forestMaskTexture || !_minimap.m_heightTexture)
            {
                EpicLoot.LogWarning("Minimap textures are null");
                return;
            }
            if (!ZoneSystem.instance)
            {
                EpicLoot.LogWarning("Zone instance is null");
                return;
            }
            if (!Game.instance)
            {
                EpicLoot.LogWarning("Game instance is null");
                return;
            }

            Color[] originalMapColors = _minimap.m_mapTexture.GetPixels();
            Color[] originalForestColors = _minimap.m_forestMaskTexture.GetPixels();
            Color[] originalHeightColors = _minimap.m_heightTexture.GetPixels();

            Color[] mapColors = new Color[originalMapColors.Length];
            Array.Copy(originalMapColors, mapColors, originalMapColors.Length);
            Color[] forestColors = _minimap.m_forestMaskTexture.GetPixels();
            Array.Copy(originalForestColors, forestColors, originalForestColors.Length);
            Color[] heightColors = _minimap.m_heightTexture.GetPixels();
            Array.Copy(originalHeightColors, heightColors, originalHeightColors.Length);

            int[] levels = new int[6] { 500, 2000, 3500, 5000, 6500, 8000 };
            foreach (int item in levels)
            {
                ZoneSystem.instance.GetLocationIcon(Game.instance.m_StartLocation, out var pos);
                Vector2 vector = (float)_minimap.m_textureSize / 2f * Vector2.one + new Vector2(pos.x, pos.z) / _minimap.m_pixelSize;
                float num = (float)item / _minimap.m_pixelSize;
                int num2 = Mathf.CeilToInt(num * 2f * (float)Math.PI);
                int i;
                for (i = 0; i < num2; i++)
                {
                    float num3 = vector.x + num * Mathf.Sin((float)Math.PI * 2f * (float)i / (float)num2);
                    float num4 = vector.y + num * Mathf.Cos((float)Math.PI * 2f * (float)i / (float)num2);
                    int num5 = ((!(num3 % 1f > 0.5f)) ? 1 : (-1));
                    int num6 = ((!(num4 % 1f > 0.5f)) ? 1 : (-1));
                    apply(num3, num4, 1f - Mathf.Abs(0.5f - num3 % 1f) * Mathf.Abs(0.5f - num4 % 1f) * 2f);
                    // apply(num3 + (float)num5, num4, 0.5f - (0.5f - Mathf.Abs(0.5f - num3 % 1f)) * Mathf.Abs(0.5f - num4 % 1f) * 2f);
                    // apply(num3, num4 + (float)num6, 0.5f - (0.5f - Mathf.Abs(0.5f - num4 % 1f)) * Mathf.Abs(0.5f - num3 % 1f) * 2f);
                    // apply(num3 + (float)num5, num4 + (float)num6, 0.5f - (0.5f - Mathf.Abs(0.5f - num3 % 1f)) * (0.5f - Mathf.Abs(0.5f - num4 % 1f)) * 2f);
                }

                void apply(float x, float y, float intensity)
                {
                    intensity = 1.0f;

                    if (!(x < 0f) && Mathf.RoundToInt(x) < _minimap.m_textureSize && !(y < 0f) && Mathf.RoundToInt(y) < _minimap.m_textureSize)
                    {
                        int num7 = Mathf.RoundToInt(y) * _minimap.m_textureSize + Mathf.RoundToInt(x);
                        Color color = Color.white; // ((i % 20 < 10) ? Color.red : Color.blue);
                        mapColors[num7] = Color.white; // ((mapColors[num7] == Color.white) ? color : Color.Lerp(mapColors[num7], color, intensity));
                        if (intensity > 0.2f)
                        {
                            forestColors[num7] = _minimap.noForest;
                            heightColors[num7] = new Color(Mathf.Clamp(heightColors[num7].r, ZoneSystem.instance.m_waterLevel + 4f, 89f), 0f, 0f);
                        }
                    }
                }
            }

            _minimap.m_mapTexture.SetPixels(mapColors);
            _minimap.m_mapTexture.Apply();
            _minimap.m_forestMaskTexture.SetPixels(forestColors);
            _minimap.m_forestMaskTexture.Apply();
            _minimap.m_heightTexture.SetPixels(heightColors);
            _minimap.m_heightTexture.Apply();
        }

    }

    // TO DO : check if this works for CreatureLevelControl map changes to work properly
     [HarmonyPatch(typeof(Minimap))]
     public static class MinimapPatch
     {
         [HarmonyPatch(nameof(Minimap.Awake))]
         [UsedImplicitly]
         public static void Postfix(Minimap __instance)
         {
             __instance.gameObject.AddComponent<MinimapController>();
         }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
     public static class Player_SetLocalPlayer_Patch
     {
         [UsedImplicitly]
         public static void Postfix(Player __instance)
         {
             MinimapController.Enable(__instance);
         }
    }
}