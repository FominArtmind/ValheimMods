using System.Collections.Generic;
using System.Linq;
using Common;
using HarmonyLib;
using JetBrains.Annotations;

namespace EpicLoot.LegendarySystem
{
    public static class UniqueLegendaryHelper
    {
        public static readonly Dictionary<string, LegendaryInfo> LegendaryInfo = new Dictionary<string, LegendaryInfo>();
        public static readonly Dictionary<string, LegendarySetInfo> LegendarySets = new Dictionary<string, LegendarySetInfo>();
        public static readonly Dictionary<string, LegendaryInfo> MythicInfo = new Dictionary<string, LegendaryInfo>();
        public static readonly Dictionary<string, LegendarySetInfo> MythicSets = new Dictionary<string, LegendarySetInfo>();

        private static readonly Dictionary<string, LegendarySetInfo> _legendaryItemsToSetMap = new Dictionary<string, LegendarySetInfo>();
        private static readonly Dictionary<string, LegendarySetInfo> _mythicItemsToSetMap = new Dictionary<string, LegendarySetInfo>();

        public static readonly LegendaryInfo GenericLegendaryInfo = new LegendaryInfo
        {
            Id = nameof(GenericLegendaryInfo)
        };

        public static void Initialize(LegendaryItemConfig config)
        {
            LegendaryInfo.Clear();
            AddLegendaryInfo(config.LegendaryItems);

            LegendarySets.Clear();
            _legendaryItemsToSetMap.Clear();
            AddLegendarySets(config.LegendarySets);

            MythicInfo.Clear();
            AddMythicInfo(config.MythicItems);

            MythicSets.Clear();
            _mythicItemsToSetMap.Clear();
            AddMythicSets(config.MythicSets);
        }

        private static void AddLegendaryInfo([NotNull] IEnumerable<LegendaryInfo> legendaryItems)
        {
            foreach (var legendaryInfo in legendaryItems)
            {
                if (!LegendaryInfo.ContainsKey(legendaryInfo.Id))
                {
                    LegendaryInfo.Add(legendaryInfo.Id, legendaryInfo);
                }
                else
                {
                    EpicLoot.LogWarning($"Duplicate entry found for LegendaryInfo: {legendaryInfo.Id}. " +
                        $"Please fix your configuration.");
                }
            }
        }

        private static void AddMythicInfo([NotNull] IEnumerable<LegendaryInfo> mythicItems)
        {
            foreach (var mythicInfo in mythicItems)
            {
                if (!MythicInfo.ContainsKey(mythicInfo.Id))
                {
                    MythicInfo.Add(mythicInfo.Id, mythicInfo);
                }
                else
                {
                    EpicLoot.LogWarning($"Duplicate entry found for MythicInfo: {mythicInfo.Id}. " +
                        $"Please fix your configuration.");
                }
            }
        }

        private static void AddLegendarySets(List<LegendarySetInfo> legendarySets)
        {
            foreach (var legendarySetInfo in legendarySets)
            {
                if (!LegendarySets.ContainsKey(legendarySetInfo.Id))
                {
                    LegendarySets.Add(legendarySetInfo.Id, legendarySetInfo);
                }
                else
                {
                    EpicLoot.LogWarning($"Duplicate entry found for LegendarySetInfo: {legendarySetInfo.Id}. " +
                        $"Please fix your configuration.");
                    continue;
                }

                foreach (var legendaryID in legendarySetInfo.Items)
                {
                    if (!_legendaryItemsToSetMap.ContainsKey(legendaryID))
                    {
                        _legendaryItemsToSetMap.Add(legendaryID, legendarySetInfo);
                    }
                    else
                    {
                        EpicLoot.LogWarning($"Duplicate entry found for LegendarySet {legendarySetInfo.Id}: {legendaryID}. " +
                            $"Please fix your configuration.");
                    }
                }
            }
        }

        private static void AddMythicSets(List<LegendarySetInfo> mythicSets)
        {
            foreach (var mythicSetInfo in mythicSets)
            {
                if (!MythicSets.ContainsKey(mythicSetInfo.Id))
                {
                    MythicSets.Add(mythicSetInfo.Id, mythicSetInfo);
                }
                else
                {
                    EpicLoot.LogWarning($"Duplicate entry found for MythicSetInfo: {mythicSetInfo.Id}. " +
                        $"Please fix your configuration.");
                    continue;
                }

                foreach (var mythicID in mythicSetInfo.Items)
                {
                    if (!_mythicItemsToSetMap.ContainsKey(mythicID))
                    {
                        _mythicItemsToSetMap.Add(mythicID, mythicSetInfo);
                    }
                    else
                    {
                        EpicLoot.LogWarning($"Duplicate entry found for MythicSet {mythicSetInfo.Id}: {mythicID}. " +
                            $"Please fix your configuration.");
                    }
                }
            }
        }

        public static bool TryGetLegendaryInfo(string legendaryID, out LegendaryInfo legendaryInfo)
        {
            if (MythicInfo.TryGetValue(legendaryID, out legendaryInfo))
            {
                return true;
            }

            return LegendaryInfo.TryGetValue(legendaryID, out legendaryInfo);
        }

        public static bool IsGenericLegendary(LegendaryInfo legendaryInfo)
        {
            return legendaryInfo == GenericLegendaryInfo;
        }

        public static bool IsLegendaryItem(string name, out string allowedPrefabName)
        {
            allowedPrefabName = "";

            if (!LegendaryInfo.ContainsKey(name))
            {
                return false;
            }

            allowedPrefabName = LegendaryInfo[name].Item;
            return true;
        }

        public static LegendaryInfo GetItem(string name)
        {
            return LegendaryInfo[name];
        }

        public static IList<LegendaryInfo> GetAvailableLegendaries(ItemDrop.ItemData baseItem, MagicItem magicItem, bool rollSetItem)
        {
            var availableLegendaries = LegendaryInfo.Values.Where(x => (x.IsSetItem == rollSetItem || EpicLoot.AlwaysDropUniqueLegendaries.Value) && x.Requirements.CheckRequirements(baseItem, magicItem)).AddItem(GenericLegendaryInfo).ToList();
            if ((rollSetItem || EpicLoot.AlwaysDropUniqueLegendaries.Value) && availableLegendaries.Count > 1)
            {
                availableLegendaries.Remove(UniqueLegendaryHelper.GenericLegendaryInfo);
            }

            return availableLegendaries;
        }

        public static IList<LegendaryInfo> GetAvailableMythics(ItemDrop.ItemData baseItem, MagicItem magicItem, bool rollSetItem)
        {
            var availableMythics = MythicInfo.Values
                .Where(x => x.IsSetItem == rollSetItem && x.Requirements.CheckRequirements(baseItem, magicItem))
                .AddItem(GenericLegendaryInfo).ToList();
            if (rollSetItem && availableMythics.Count > 1)
            {
                availableMythics.Remove(UniqueLegendaryHelper.GenericLegendaryInfo);
            }

            return availableMythics;
        }

        public static MagicItemEffectDefinition.ValueDef GetLegendaryEffectValues(string legendaryID, string effectType, ItemQuality quality)
        {
            if (MythicInfo.TryGetValue(legendaryID, out var mythicInfo))
            {
                if (mythicInfo.Normal.TryFind(x => x.Type == effectType, out var guaranteedMagicEffect))
                {
                    return guaranteedMagicEffect.Values;
                }
            }
            else 
            if (LegendaryInfo.TryGetValue(legendaryID, out var legendaryInfo))
            {
                if (quality == ItemQuality.Elite)
                {
                    if (legendaryInfo.Elite.Count() > 0 && legendaryInfo.Elite.TryFind(x => x.Type == effectType, out var guaranteedMagicEffectElite))
                    {
                        return guaranteedMagicEffectElite.Values;
                    }
                }
                else if (quality == ItemQuality.Exceptional)
                {
                    if (legendaryInfo.Exceptional.Count() > 0 && legendaryInfo.Exceptional.TryFind(x => x.Type == effectType, out var guaranteedMagicEffectExceptional))
                    {
                        return guaranteedMagicEffectExceptional.Values;
                    }
                }

                if (legendaryInfo.Normal.TryFind(x => x.Type == effectType, out var guaranteedMagicEffect))
                {
                    return guaranteedMagicEffect.Values;
                }
            }
            return null;
        }

        public static bool TryGetLegendarySetInfo(string setID, out LegendarySetInfo legendarySetInfo, out ItemRarity rarity)
        {
            if (string.IsNullOrEmpty(setID))
            {
                legendarySetInfo = null;
                rarity = ItemRarity.Magic;
                return false;
            }

            if (MythicSets.TryGetValue(setID, out legendarySetInfo))
            {
                rarity = ItemRarity.Mythic;
                return true;
            }

            rarity = ItemRarity.Legendary;
            return LegendarySets.TryGetValue(setID, out legendarySetInfo);
        }

        public static string GetSetForLegendaryItem(LegendaryInfo legendary)
        {
            if (legendary != null && legendary.IsSetItem && !string.IsNullOrEmpty(legendary.Id))
            {
                if (_mythicItemsToSetMap.TryGetValue(legendary.Id, out var mythicSetInfo))
                {
                    return mythicSetInfo.Id;
                }
                else if (_legendaryItemsToSetMap.TryGetValue(legendary.Id, out var setInfo))
                {
                    return setInfo.Id;
                }
            }

            return null;
        }
    }
}
