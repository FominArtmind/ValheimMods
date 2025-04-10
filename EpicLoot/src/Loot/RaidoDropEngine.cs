using System;
using System.Collections.Generic;
using System.Linq;
using EpicLoot;
using EpicLoot.Crafting;
using EpicLoot.Data;
using EpicLoot.LegendarySystem;
using EpicLoot.MagicItemEffects;
using UnityEngine;
using static PrivilegeManager;
using Object = UnityEngine.Object;

namespace Raido
{
    public class ResolvedItem
    {
        public string Item;
        public int? Count;
        public string Class;
        public ItemRarity? Rarity;
        public ItemQuality? Quality;
    }

    public static class DropEngine
    {
        public static DropConfig Config;
        public static MagicEffectsConfig EffectsConfig; 
        public static ItemClassesConfig ClassesConfig;
        private static System.Random _random;

       //  private static WeightedRandomCollection<MagicItemEffectDefinition> _weightedEffectTable;

        public static void InitializeDropConfig(DropConfig dropConfig)
        {
            Config = dropConfig;

            _random = new System.Random();
            // _weightedEffectTable = new WeightedRandomCollection<MagicItemEffectDefinition>(_random);
        }

        public static void InitializeEffectsConfig(MagicEffectsConfig effectsConfig)
        {
            EffectsConfig = effectsConfig;
        }

        public static void InitializeClassesConfig(ItemClassesConfig classesConfig)
        {
            ClassesConfig = classesConfig;
            ClassesConfig.Initialize(EffectsConfig);
        }

        private static void _Log(string message)
        {
            EpicLoot.EpicLoot.Log($"Raido.DropEngine -> {message}");
        }

        private static bool _IsMaterial(string itemName)
        {
            string[] materials = new string[]{
                        "RunestoneMagic", "ShardMagic", "DustMagic", "ReagentMagic", "EssenceMagic",
                        "RunestoneRare", "ShardRare", "DustRare", "ReagentRare", "EssenceRare",
                        "RunestoneEpic", "ShardEpic", "DustEpic", "ReagentEpic", "EssenceEpic",
                    };

            return materials.Contains(itemName);
        }

        private static List<DropUnit> _ResolveCreatureItems(string name, int level)
        {
            var drop = Config.Creatures.Find(value => value.Name == name);

            if (drop == null)
            {
                _Log($"Creature {name} not found");
                return new List<DropUnit>();
            }

            CreatureDrop creatureDrop = drop.Drop.Find(value => value.Level == level);
            if (creatureDrop == null)
            {
                _Log($"Creature {name} level {level} drop not found");
                return new List<DropUnit>();
            }

            var defaultRarityRates = Config.DefaultRarityRates;

            var defaultQualityRates = Config.DefaultStarQualityRates.Find(value => value.Level == level);
            if (defaultQualityRates == null)
            {
                _Log($"Default quality rates for level {level} of creature {name} not found");
                return new List<DropUnit>();
            }

            var items = new List<DropUnit>();

            foreach (var item in creatureDrop.Items)
            {
                DropUnit temp = new DropUnit
                {
                    Every = item.Every,
                    Item = item.Item,
                    Class = item.Class,
                    Min = item.Min,
                    Max = item.Max,
                    Rare = item.Rare ?? defaultRarityRates.Rare,
                    Epic = item.Epic ?? defaultRarityRates.Epic,
                    Normal = item.Normal ?? defaultQualityRates.Normal,
                    Exceptional = item.Exceptional ?? defaultQualityRates.Exceptional,
                    Elite = item.Elite ?? defaultQualityRates.Elite,
                    Repeat = item.Repeat
                };

                items.Add(temp);
            }

            return items;
        }

        private static List<DropUnit> _ResolveChestItems(string name, int distanceFromWorldCenter, out int? limit)
        {
            limit = null;
            var drop = Config.Chests.Find(value => value.Name == name);

            if (drop == null)
            {
                _Log($"Chest {name} not found");
                return new List<DropUnit>();
            }

            ChestDrop chestDrop = null;
            int maxDistance = -1;
            foreach (var entity in drop.Drop)
            {
                if (distanceFromWorldCenter >= entity.Distance && entity.Distance > maxDistance)
                {
                    chestDrop = entity;
                    maxDistance = entity.Distance;
                }
            }

            if (chestDrop == null)
            {
                _Log($"Chest {name} distance {distanceFromWorldCenter} drop not found");
                return new List<DropUnit>();
            }

            limit = chestDrop.Limit;
            int level = chestDrop.Level;

            var defaultRarityRates = Config.DefaultRarityRates;

            var defaultQualityRates = Config.DefaultBiomeLevelQualityRates.Find(value => value.Level == level);
            if (defaultQualityRates == null)
            {
                _Log($"Default quality rates for level {level} of chest {name} not found");
                return new List<DropUnit>();
            }

            var items = new List<DropUnit>();

            foreach (var item in chestDrop.Items)
            {
                DropUnit temp = new DropUnit
                {
                    Every = item.Every,
                    Item = item.Item,
                    Class = item.Class,
                    Min = item.Min,
                    Max = item.Max,
                    Rare = item.Rare ?? defaultRarityRates.Rare,
                    Epic = item.Epic ?? defaultRarityRates.Epic,
                    Normal = item.Normal ?? defaultQualityRates.Normal,
                    Exceptional = item.Exceptional ?? defaultQualityRates.Exceptional,
                    Elite = item.Elite ?? defaultQualityRates.Elite,
                    Repeat = item.Repeat
                };

                items.Add(temp);
            }

            return items;
        }

        private static DropUnit _ResolveItemGroup(DropUnit item)
        {
            var result = new DropUnit
            {
                Every = item.Every,
                Item = item.Item,
                Class = item.Class,
                Min = item.Min,
                Max = item.Max,
                Rare = item.Rare,
                Epic = item.Epic,
                Normal = item.Normal,
                Exceptional = item.Exceptional,
                Elite = item.Elite,
                Repeat = item.Repeat
            };

            while (true)
            {
                if (!result.Item.StartsWith("["))
                {
                    return result;
                }

                var group = Config.ItemGroups.Find(value => value.Name == result.Item);

                if (group == null)
                {
                    _Log($"Item group {result.Item} not found");
                    return result;
                }

                var totalWeights = 0;
                foreach (var entity in group.From)
                {
                    totalWeights += entity.Weight;
                }
                var roll = _random.Next(totalWeights);
                var sumWeight = 0;
                foreach (var entity in group.From)
                {
                    sumWeight += entity.Weight;
                    if (roll < sumWeight)
                    {
                        result.Item = entity.Item;
                        result.Class = entity.Class;
                        break;
                    }
                }
            }
        }
        public static float GetLuckFactor(Vector3 fromPoint, out int extraRolls)
        {
            var luckFactor = 0.0f;
            extraRolls = 0;
            var players = new List<Player>();
            Player.GetPlayersInRange(fromPoint, 100f, players);

            if (players.Count > 0)
            {
                var totalLuckFactor = players
                    .Select(x => x.m_nview.GetZDO().GetInt("el-luk") * 0.01f)
                    .DefaultIfEmpty(0)
                    .Sum();
                luckFactor += totalLuckFactor;
            }

            extraRolls = (int)Math.Floor(luckFactor);
            return luckFactor - extraRolls;
        }

        private static string _RollItemClass(DropUnit item)
        {
            if(item.Class != null)
            {
                return item.Class;
            }

            var classes = ClassesConfig.GetAvailableClasses(item.Item);
            var str = "";
            foreach (var c in classes)
            {
                str += $"{c.Key}: {c.Value} ";
            }
            _Log($"Classes for {item.Item} {str}");

            var total = classes.Sum(value => value.Value);
            var roll = _random.Next(total);
            var sumWeight = 0;
            foreach (var entity in classes)
            {
                sumWeight += entity.Value;
                if (roll < sumWeight)
                {
                    return entity.Key;
                }
            }

            _Log($"Class for {item.Item} not found, rolling Forgotten");
            return "Forgotten";
        }

        private static ItemRarity _RollItemRarity(DropUnit item)
        {
            if (item.Epic != null && _random.NextDouble() * 100.0 < item.Epic)
            {
                return ItemRarity.Epic;
            }
            if (item.Rare != null && _random.NextDouble() * 100.0 < item.Rare)
            {
                return ItemRarity.Rare;
            }
            return ItemRarity.Magic;
        }

        private static ItemQuality _RollItemQuality(DropUnit item)
        {
            if (item.Elite != null && _random.NextDouble() * 100.0 < item.Elite)
            {
                return ItemQuality.Elite;
            }
            if (item.Exceptional != null && _random.NextDouble() * 100.0 < item.Exceptional)
            {
                return ItemQuality.Exceptional;
            }
            if (item.Normal != null && _random.NextDouble() * 100.0 < item.Normal)
            {
                return ItemQuality.Normal;
            }
            return ItemQuality.Inferior;
        }

        private static int _RollEffectCount(ItemRarity rarity, ItemQuality quality)
        {
            EffectCountConfig qualityConfig;
            switch (quality)
            {
                case ItemQuality.Elite:
                    qualityConfig = EffectsConfig.EffectCountDistribution.Elite;
                    break;
                case ItemQuality.Exceptional:
                    qualityConfig = EffectsConfig.EffectCountDistribution.Exceptional;
                    break;
                case ItemQuality.Normal:
                    qualityConfig = EffectsConfig.EffectCountDistribution.Normal;
                    break;
                default:
                case ItemQuality.Inferior:
                    qualityConfig = EffectsConfig.EffectCountDistribution.Inferior;
                    break;
            }

            List<KeyValuePair<int, int>> config;
            switch (rarity)
            {
                case ItemRarity.Epic:
                    config = qualityConfig.Epic.Select(x => new KeyValuePair<int, int>(x[0], x[1])).ToList();
                    break;
                case ItemRarity.Rare:
                    config = qualityConfig.Rare.Select(x => new KeyValuePair<int, int>(x[0], x[1])).ToList();
                    break;
                default:
                case ItemRarity.Magic:
                    config = qualityConfig.Magic.Select(x => new KeyValuePair<int, int>(x[0], x[1])).ToList();
                    break;
            }

            int total = 0;
            foreach(var item in config)
            {
                total += item.Value;
            }
            
            var roll = _random.Next(total);
            int sum = 0;
            foreach (var item in config)
            {
                sum += item.Value;
                if(sum >= roll)
                {
                    return item.Key;
                }
            }

            return 0;
        }

        private static float _RollEffectValue(ItemResolvedEffect effect)
        {
            var min = effect.Min != null ? effect.Min.Value : 0.0f;
            var max = effect.Max != null ? effect.Max.Value : 0.0f;
            var step = effect.Step != null ? effect.Step.Value : 0.0f;

            var value = min;
            if (step > 0.0f)
            {
                _Log($"RollEffectValue -> Rolling {effect.Type} (min={effect.Min} max={effect.Max} step={effect.Step})");
                var incrementCount = (int)((max - min) / step);

                double v = Math.Pow(_random.NextDouble() * _random.NextDouble(), 0.7);

                value = min + (int)(v * (incrementCount + 1)) * step;
                _Log($"RollEffectValue -> Rolled {value} (incrementCount={incrementCount} v={v})");
            }
            else
            {
                _Log($"RollEffectValue -> SKIPPING {effect.Type} (min={effect.Min} max={effect.Max} step={step}) due to 0 step value");
            }

            return value;
        }

        private static EpicLoot.MagicItemEffect _RollEffect(List<ItemResolvedEffect> effects)
        {
            _Log($"Effects to select from: {effects.Count}");

            int total = 0;
            foreach (var item in effects)
            {
                total += item.Weight;
            }

            var roll = _random.Next(total);
            var selected = effects[0];
            int sum = 0;
            foreach (var item in effects)
            {
                sum += item.Weight;
                if (sum >= roll)
                {
                    selected = item;
                    break;
                }
            }

            _Log($"Selected effect {selected.Type} weight {selected.Weight}, is group: {selected.Group != null && selected.Group.Count > 0} Step {selected.Step}");

            ItemResolvedGroupEffect selectedGroupEffect = null;
            if (selected.Group != null && selected.Group.Count > 0)
            {
                int total2 = 0;
                foreach (var item in selected.Group)
                {
                    total2 += item.Weight;
                }

                var roll2 = _random.Next(total2);
                selectedGroupEffect = selected.Group[0];
                _Log($"Changed selected effect to {selectedGroupEffect.Type} weight {selectedGroupEffect.Weight} Step {selectedGroupEffect.Step} as default group 0 effect");
                int sum2 = 0;
                foreach (var item in selected.Group)
                {
                    sum2 += item.Weight;
                    if (sum2 >= roll2)
                    {
                        selectedGroupEffect = item;
                        _Log($"Changed selected effect to {selectedGroupEffect.Type} weight {selectedGroupEffect.Weight} Step {selectedGroupEffect.Step}");
                        break;
                    }
                }
            }

            if(selectedGroupEffect != null)
            {
                selected = new ItemResolvedEffect()
                {
                    Type = selectedGroupEffect.Type,
                    Min = selectedGroupEffect.Min,
                    Max = selectedGroupEffect.Max,
                    Step = selectedGroupEffect.Step
                };
            }

            EpicLoot.MagicItemEffect result = new EpicLoot.MagicItemEffect() { EffectType = selected.Type };
            if(!EffectsConfig.IsValuelessEffect(selected.Type))
            {
                result.EffectValue = _RollEffectValue(selected);
            }

            return result;
        }

        private static List<ResolvedItem> _RollItems(List<DropUnit> items, Vector3 dropPoint, int? limit = null, bool log = true)
        {
            var result = new List<ResolvedItem>();
            var sortedItems = items.OrderByDescending(value => value.Every).ToList();

            var droppedCount = 0;
            foreach (var entity in sortedItems)
            {
                var item = _ResolveItemGroup(entity);
                if (entity.Item != item.Item && log)
                {
                    _Log($"Group {entity.Item} resolved to {item.Item}");
                }

                var rolls = item.Repeat;

                if (!_Creature(item.Item) && !_PlainItem(item.Item))
                {
                    var luckFactor = GetLuckFactor(dropPoint, out int extraRolls);
                    rolls += extraRolls;
                    if (_random.NextDouble() < luckFactor)
                    {
                        rolls++;
                        _Log($"For{item.Item} initial rolls {item.Repeat}, luck increased rolls {rolls} (luck < 100 proc)");
                    }
                    else
                    {
                        _Log($"For{item.Item} initial rolls {item.Repeat}, luck increased rolls {rolls}");
                    }
                }

                for (int n = 0; n < rolls; n++)
                {
                    // since checking for 0 every time results look suspicious
                    var rollBase = _random.Next(item.Every);
                    var roll = _random.Next(item.Every);
                    if (log)
                    {
                        _Log($"For {item.Item} rolled base {rollBase}, roll {roll} of {item.Every}");
                    }
                    if (roll == rollBase)
                    {
                        if (limit != null && droppedCount >= limit)
                        {
                            _Log($"Reaching limit {limit} for drop count");
                            break;
                        }
                        droppedCount++;

                        var resolvedItem = new ResolvedItem
                        {
                            Item = item.Item,
                            Class = _RollItemClass(item),
                            Rarity = _RollItemRarity(item),
                            Quality = _RollItemQuality(item)
                        };

                        var count = _random.Next(item.Min, item.Max + 1);
                        if (log)
                        {
                            _Log($"For {item.Item} rolled count {count} of ({item.Min} - {item.Max})");
                        }
                        resolvedItem.Count = count;

                        result.Add(resolvedItem);
                    }
                }
            }

            return result;
        }

        public static void AddGuaranteedEffects(MagicItem magicItem, List<MagicEffect> guaranteedMagicEffects)
        {
            foreach (var guaranteedMagicEffect in guaranteedMagicEffects)
            {
                var magicItemEffect = new MagicItemEffect() { EffectType = guaranteedMagicEffect.Type };
                if (!EffectsConfig.IsValuelessEffect(guaranteedMagicEffect.Type))
                {
                    magicItemEffect.EffectValue = _RollEffectValue(new ItemResolvedEffect()
                    {
                        Type = guaranteedMagicEffect.Type,
                        Min = guaranteedMagicEffect.Values.MinValue,
                        Max = guaranteedMagicEffect.Values.MaxValue,
                        Step = guaranteedMagicEffect.Values.Increment
                    });
                }

                magicItem.Effects.Add(magicItemEffect);
            }
        }

        public static MagicItem RollMagicItem(string itemName, string itemClass, ItemRarity rarity, ItemQuality quality, ItemDrop.ItemData baseItem)
        {
            var magicItem = new MagicItem { ItemName = baseItem.m_shared.m_name, Class = itemClass, Rarity = rarity, Quality = quality };

            if (rarity == ItemRarity.Legendary)
            {
                LegendaryInfo itemInfo = UniqueLegendaryHelper.GetItem(itemName);

                if (itemInfo.IsSetItem)
                {
                    var setID = UniqueLegendaryHelper.GetSetForLegendaryItem(itemInfo);
                    magicItem.SetID = setID;
                }

                var legendaryNameFormat = Localization.instance.Localize("$mod_epicloot_basiclegendarynameformat");
                var qualityStr = "";
                switch (magicItem.Quality)
                {
                    case ItemQuality.Inferior:
                        qualityStr = Localization.instance.Localize("$mod_epicloot_inferior");
                        break;
                    case ItemQuality.Exceptional:
                        qualityStr = Localization.instance.Localize("$mod_epicloot_exceptional");
                        break;
                    case ItemQuality.Elite:
                        qualityStr = Localization.instance.Localize("$mod_epicloot_elite");
                        break;
                    default:
                        break;
                }

                magicItem.LegendaryID = itemInfo.Id;
                magicItem.DisplayName = string.Format(legendaryNameFormat, qualityStr, itemInfo.Name).Trim();

                List<MagicEffect> guaranteedMagicEffects;
                if (quality == ItemQuality.Elite && itemInfo.Elite.Count() > 0)
                {
                    guaranteedMagicEffects = itemInfo.Elite;
                }
                else if (quality == ItemQuality.Exceptional && itemInfo.Exceptional.Count() > 0)
                {
                    guaranteedMagicEffects = itemInfo.Exceptional;
                }
                else
                {
                    guaranteedMagicEffects = itemInfo.Normal;
                }

                AddGuaranteedEffects(magicItem, guaranteedMagicEffects);
            }
            else
            {
                var effectCount = _RollEffectCount(magicItem.Rarity, magicItem.Quality);

                var skippedEffectsNames = new List<string>();
                var availableEffects = ClassesConfig.GetAvailableEnchantEffects(itemName, magicItem.Class, magicItem.Quality, magicItem.Rarity, skippedEffectsNames);
                var coreOrRequiredEffects = availableEffects.Where(value => value.Core || value.Weight == 0).ToList();
                foreach (var effect in coreOrRequiredEffects)
                {
                    var magicItemEffect = _RollEffect(new List<ItemResolvedEffect>() { effect });

                    magicItem.Effects.Add(magicItemEffect);
                    skippedEffectsNames.Add(magicItemEffect.EffectType);

                    if(!effect.Core)
                    {
                        effectCount--;
                    }
                }

                for (var i = 0; i < effectCount; i++)
                {
                    var remainingEffects = ClassesConfig.GetAvailableEnchantEffects(itemName, magicItem.Class, magicItem.Quality, magicItem.Rarity, skippedEffectsNames);
                    if (remainingEffects.Count == 0)
                    {
                        _Log($"Tried to add more effects to magic item ({baseItem.m_shared.m_name}) but there were no more available effects. " +
                                            $"Current Effects: {(string.Join(", ", magicItem.Effects.Select(x => x.EffectType.ToString())))}");
                        break;
                    }

                    var magicItemEffect = _RollEffect(remainingEffects);

                    magicItem.Effects.Add(magicItemEffect);
                    skippedEffectsNames.Add(magicItemEffect.EffectType);
                }
            }

            if (string.IsNullOrEmpty(magicItem.DisplayName))
            {
                magicItem.DisplayName = MagicItemNames.GetNameForItem(baseItem, magicItem);
            }
            if (magicItem.Rarity == ItemRarity.Epic && string.IsNullOrEmpty(magicItem.KnowAsName))
            {
                magicItem.KnowAsName = MagicItemNames.BuildEpicName(baseItem, magicItem);
            }

            return magicItem;
        }

        private static ItemDrop _ItemDrop(string prefabName)
        {
            var itemPrefab = ObjectDB.instance.GetItemPrefab(prefabName);
            if (!itemPrefab)
            {
                _Log($"Item prefab {prefabName} not found!");
                return null;
            }

            var itemDrop = itemPrefab.GetComponent<ItemDrop>();
            if (!itemDrop)
            {
                _Log($"Item drop for {prefabName} not found!");
                return null;
            }

            return itemDrop;
        }

        private static bool _Creature(string prefabName)
        {
            var creaturePrefab = ZNetScene.instance.GetPrefab(prefabName);
            if (creaturePrefab != null)
            {
                var character = creaturePrefab.GetComponent<Character>();
                if (character)
                {
                    _Log($"Is creature: {prefabName}");
                    return true;
                }
            }

            return false;
        }

        private static bool _PlainItem(string prefabName)
        {
            if(prefabName == "Wishbone") {
                return true;
            }

            string legendaryBasePrefabName = null;
            if (UniqueLegendaryHelper.IsLegendaryItem(prefabName, out legendaryBasePrefabName))
            {
                return false;
            }

            return !EpicLoot.EpicLoot.CanBeMagicItem(_ItemDrop(prefabName).m_itemData);
        }
        private static bool _PlayerKnowsItem(string prefabName)
        {
            var itemDrop = _ItemDrop(prefabName);

            return itemDrop != null && Player.m_localPlayer != null && Player.m_localPlayer.m_knownMaterial.Contains(itemDrop.m_itemData.m_shared.m_name);
        }

        private static bool _ItemAllowed(string prefabName)
        {
            if (_PlayerKnowsItem(prefabName))
            {
                return true;
            }

            var exceptions = LootRoller.Config.ItemDropLimitsExceptions;

            if (EpicLoot.EpicLoot.AllowItemDropLimitsExceptions.Value && exceptions != null && exceptions.Contains(prefabName))
            {
                return true;
            }
            return false;
        }

        // level == -1 for chests
        private static List<GameObject> RollDropInternal(string objectName, int level, Vector3 dropPoint, bool initializeObject)
        {
            var results = new List<GameObject>();

            int distanceFromWorldCenter = (int)new Vector3(dropPoint.x, 0, dropPoint.z).magnitude;

            List<DropUnit> items;
            int? limit = null;
            if (level == -1)
            {
                _Log($"Distance from world center for chest {objectName}: {distanceFromWorldCenter}");
                items = _ResolveChestItems(objectName, distanceFromWorldCenter, out limit);
            }
            else
            {
                items = _ResolveCreatureItems(objectName, level);
            }

            _Log($"\r\nResolved items for {objectName} level {level}:");
            foreach (var item in items)
            {
                _Log($"Item {item.Item}, Every {item.Every}, Rare {item.Rare} Epic {item.Epic} Ex {item.Exceptional} El {item.Elite}, Min {item.Min} Max {item.Max}");
            }

            List<ResolvedItem> rolledItems = _RollItems(items, dropPoint, limit);

            if (rolledItems.Count == 0)
            {
                _Log($"No items have been rolled");
                return new List<GameObject>();
            }

            _Log($"\r\nRolled items for {objectName} level {level}:");
            foreach (var item in rolledItems)
            {
                _Log($"Item {item.Item}, Class {item.Class} Rarity {item.Rarity}, Quality {item.Quality}, Count {item.Count}");
            }

            foreach (var rolledItem in rolledItems)
            {
                var prefabName = rolledItem.Item;

                string itemClass = "Forgotten";
                ItemRarity rarity = ItemRarity.Magic;
                ItemQuality quality = ItemQuality.Inferior;

                if (_Creature(prefabName) || _PlainItem(prefabName))
                {
                    // nothing to do yet
                }
                else
                {
                    itemClass = rolledItem.Class;
                    rarity = rolledItem.Rarity ?? ItemRarity.Magic;
                    quality = rolledItem.Quality ?? ItemQuality.Inferior;

                    var lootFilterForcedSacrifice = false;

                    string legendaryBasePrefabName = null;
                    if (UniqueLegendaryHelper.IsLegendaryItem(prefabName, out legendaryBasePrefabName))
                    {
                        prefabName = legendaryBasePrefabName;
                        rarity = ItemRarity.Legendary;

                        if(quality == ItemQuality.Inferior)
                        {
                            quality = ItemQuality.Normal;
                        }
                    }
                    else
                    {
                        if (LootFilterDefinitions.FilteredOut(prefabName, rarity, quality, distanceFromWorldCenter, out lootFilterForcedSacrifice))
                        {
                            if (!lootFilterForcedSacrifice)
                            {
                                _Log($"Item filtered {prefabName} out due to loot filters");
                                continue;
                            }
                        }
                    }

                    bool ReplaceWithMats()
                    {
                        if (prefabName == null)
                        {
                            _Log($"Item replaced with materials due to its id being null");
                            return true;
                        }

                        // TO DO : other special items if needed
                        if (prefabName == "Wishbone") {
                            return false;
                        }

                        if (lootFilterForcedSacrifice)
                        {
                            _Log($"Item replaced with materials due to loot filters");
                            return true;
                        }

                        if (!_ItemAllowed(prefabName))
                        {
                            _Log($"Item replaced with materials due to being not allowed yet");
                            return true;
                        }

                        if (rarity != ItemRarity.Legendary)
                        {
                            if (quality != ItemQuality.Elite)
                            {
                                if (Raido.PlayerKnowsItemClassAndQuality(prefabName, itemClass, ItemQuality.Elite))
                                {
                                    _Log($"Item replaced with materials due to player has already seen Elity quality of such base and class");
                                    return true;
                                }

                                if (quality != ItemQuality.Exceptional)
                                {
                                    if (Raido.PlayerKnowsItemClassAndQuality(prefabName, itemClass, ItemQuality.Exceptional))
                                    {
                                        _Log($"Item replaced with materials due to player has already seen Exceptional quality of such base and class");
                                        return true;
                                    }

                                    if (quality != ItemQuality.Normal)
                                    {
                                        if (Raido.PlayerKnowsItemClassAndQuality(prefabName, itemClass, ItemQuality.Normal))
                                        {
                                            _Log($"Item replaced with materials due to player has already seen Normal quality of such base and class");
                                            return true;
                                        }
                                    }
                                }
                            }
                        }

                        return false;
                    };

                    if (ReplaceWithMats())
                    {
                        GameObject prefab = null;

                        try
                        {
                            prefab = ObjectDB.instance.GetItemPrefab(prefabName);
                        }
                        catch (Exception e)
                        {
                            _Log($"Unable to get Prefab for [{prefabName}]. Continuing.");
                            _Log($"Error: {e.Message}");
                        }

                        if (prefab != null)
                        {
                            var itemType = prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_itemType;
                            var disenchantProducts = EnchantCostsHelper.GetSacrificeProducts(true, itemType, rarity, quality);
                            if (disenchantProducts != null)
                            {
                                foreach (var itemAmountConfig in disenchantProducts)
                                {
                                    GameObject materialPrefab = null;
                                    try
                                    {
                                        materialPrefab = ObjectDB.instance.GetItemPrefab(itemAmountConfig.Item);
                                    }
                                    catch (Exception e)
                                    {
                                        _Log($"Unable to get Disenchant Product Prefab for [{itemAmountConfig?.Item ?? "Invalid Item"}]. Continuing.");
                                        _Log($"Error: {e.Message}");
                                    }

                                    if (materialPrefab == null)
                                    {
                                        continue;
                                    }
                                    var materialItem = LootRoller.SpawnLootForDrop(materialPrefab, dropPoint, initializeObject);
                                    var materialItemDrop = materialItem.GetComponent<ItemDrop>();
                                    materialItemDrop.m_itemData.m_stack = itemAmountConfig.Amount;
                                    if (materialItemDrop.m_itemData.IsMagicCraftingMaterial())
                                    {
                                        // TO DO: looks like as fixing bug thing
                                        // materialItemDrop.m_itemData.m_variant = EpicLoot.GetRarityIconIndex(rarity);
                                        materialItemDrop.m_itemData.m_variant = EpicLoot.EpicLoot.GetRarityIconIndex(ItemRarity.Magic);
                                    }
                                    results.Add(materialItem);
                                }
                            }
                        }

                        continue;
                    }
                }

                if (_Creature(prefabName))
                {
                    // creating creature right here since there is no sense to put it into container etc
                    var creaturePrefab = ZNetScene.instance.GetPrefab(prefabName);

                    for (int i = 0; i < rolledItem.Count; i++)
                    {
                        var spawnPoint = new Vector3(dropPoint.x, dropPoint.y, dropPoint.z);
                        var randomSpacing = UnityEngine.Random.insideUnitSphere * 2;
                        spawnPoint += randomSpacing;
                        ZoneSystem.instance.GetSolidHeight(spawnPoint, out var height);
                        spawnPoint.y = height;

                        var creature = Object.Instantiate(creaturePrefab, spawnPoint, Quaternion.identity);
                    }
                }
                else
                {
                    GameObject itemPrefab = null;

                    itemPrefab = ObjectDB.instance.GetItemPrefab(prefabName);

                    if (itemPrefab != null)
                    {
                        var item = LootRoller.SpawnLootForDrop(itemPrefab, dropPoint, initializeObject);
                        var itemDrop = item.GetComponent<ItemDrop>();

                        if (_PlainItem(prefabName))
                        {
                            itemDrop.m_itemData.m_stack = rolledItem.Count ?? 1;
                        }
                        else
                        {
                            var itemData = itemDrop.m_itemData;
                            var magicItemComponent = itemData.Data().GetOrCreate<MagicItemComponent>();
                            var magicItem = RollMagicItem(rolledItem.Item, itemClass, rarity, quality, itemData);

                            magicItemComponent.SetMagicItem(magicItem);
                            itemDrop.m_itemData = itemData;
                            itemDrop.Save();

                            Indestructible.MakeItemIndestructible(itemData);
                            if (itemData.m_shared.m_useDurability)
                            {
                                itemData.m_durability = itemData.GetMaxDurability();
                                // Random.Range(0.2f, 1.0f) * itemData.GetMaxDurability();
                            }
                        }

                        results.Add(item);
                    }
                }
            }

            return results;
        }

        public static List<ItemDrop.ItemData> RollContainerDrop(string objectName, Vector3 dropPoint)
        {
            var results = new List<ItemDrop.ItemData>();
            var gameObjects = RollDropInternal(objectName, -1, dropPoint, false);
            foreach (var itemObject in gameObjects)
            {
                results.Add(itemObject.GetComponent<ItemDrop>().m_itemData.Clone());
                ZNetScene.instance.Destroy(itemObject);
            }

            return results;
        }

        public static List<GameObject> RollCreatureDrop(string objectName, int level, Vector3 dropPoint)
        {
            return RollDropInternal(objectName, level - 1, dropPoint, true);
        }

        public static List<ResolvedItem> RollResolvedItems(string objectName, int level, Vector3 dropPoint)
        {
            int distanceFromWorldCenter = (int)new Vector3(dropPoint.x, 0, dropPoint.z).magnitude;

            List<DropUnit> items;
            int? limit = null;
            if (level == -1)
            {
                items = _ResolveChestItems(objectName, distanceFromWorldCenter, out limit);
            }
            else
            {
                items = _ResolveCreatureItems(objectName, level);
            }

            List<ResolvedItem> rolledItems = _RollItems(items, dropPoint, limit, false);

            return rolledItems;
        }

        public static List<MagicItemEffect> RollAugmentEffects(int count, ItemDrop.ItemData item, int replacedEffectIndex)
        {
            var availableEffects = ClassesConfig.GetAvailableAugmentEffects(item, replacedEffectIndex);

            List<MagicItemEffect> result = new List<MagicItemEffect>();

            for (var i = 0; i < count; i++)
            {
                var effect = _RollEffect(availableEffects);
                result.Add(effect);
            }

            return result;
        }
    }
}
