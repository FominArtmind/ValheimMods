using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Linq;
using Common;
using EpicLoot;
using EpicLoot.Adventure.Feature;
using EpicLoot.Crafting;
using EpicLoot.Data;
using EpicLoot.GatedItemType;
using EpicLoot.LegendarySystem;
using EpicLoot.MagicItemEffects;
using EpicLoot_UnityLib;
using JetBrains.Annotations;
using UnityEngine;
using static CharacterDrop;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Raido
{
    public class ResolvedItem
    {
        public string Item;
        public int? Count;
        public ItemRarity? Rarity;
        public ItemQuality? Quality;
    }

    public static class DropEngine
    {
        public static DropConfig Config;
        private static System.Random _random;

        private static WeightedRandomCollection<MagicItemEffectDefinition> _weightedEffectTable;

        public static void Initialize(DropConfig dropConfig)
        {
            Config = dropConfig;

            _random = new System.Random();
            _weightedEffectTable = new WeightedRandomCollection<MagicItemEffectDefinition>(_random);

            /*            for(int i = 1; i < 170; i++)
                        {
                            int[] distrib = new int[i];

                            int tries = 10000;
                            for (int j = 0; j < tries; j++)
                            {
                                var roll = _random.Next(i);
                                distrib[roll]++;
                            }

                            _Log($"Expected chance for {i} is {1.0 / i}, got {distrib[0] * 1.0 / tries}");
                        }*/
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
                    // From = item.From,
                    Item = item.Item,
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
                    // From = item.From,
                    Item = item.Item,
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
                // From = item.From,
                Item = item.Item,
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

                if (!_PlainItem(item.Item))
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

        public static MagicItem RollMagicItem(string itemName, ItemRarity rarity, ItemQuality quality, ItemDrop.ItemData baseItem, float luckFactor)
        {
            var magicItem = new MagicItem { Rarity = rarity, Quality = quality, ItemName = baseItem.m_shared.m_name };

            if (rarity == ItemRarity.Legendary)
            {
                LegendaryInfo itemInfo = UniqueLegendaryHelper.GetItem(itemName);

                if (itemInfo.IsSetItem)
                {
                    var setID = UniqueLegendaryHelper.GetSetForLegendaryItem(itemInfo);
                    magicItem.SetID = setID;
                }

                magicItem.LegendaryID = itemInfo.ID;
                magicItem.DisplayName = itemInfo.Name;

                List<GuaranteedMagicEffect> guaranteedMagicEffects;
                if (quality == ItemQuality.Elite && itemInfo.GuaranteedMagicEffectsElite.Count() > 0)
                {
                    guaranteedMagicEffects = itemInfo.GuaranteedMagicEffectsElite;
                }
                else if (quality == ItemQuality.Exceptional && itemInfo.GuaranteedMagicEffectsExceptional.Count() > 0)
                {
                    guaranteedMagicEffects = itemInfo.GuaranteedMagicEffectsExceptional;
                }
                else
                {
                    guaranteedMagicEffects = itemInfo.GuaranteedMagicEffects;
                }

                foreach (var guaranteedMagicEffect in guaranteedMagicEffects)
                {
                    var effectDef = MagicItemEffectDefinitions.Get(guaranteedMagicEffect.Type);
                    if (effectDef == null)
                    {
                        _Log($"Could not find magic effect (Type={guaranteedMagicEffect.Type}) while creating legendary item (ID={itemInfo.ID})");
                        continue;
                    }

                    var effect = LootRoller.RollEffect(effectDef, rarity, magicItem.Quality, baseItem.m_shared.m_name, guaranteedMagicEffect.Values);
                    magicItem.Effects.Add(effect);
                }
            }
            else
            {
                var effectCount = LootRoller.RollEffectCountPerRarity(magicItem.Rarity, magicItem.Quality);

                for (var i = 0; i < effectCount; i++)
                {
                    var availableEffects = MagicItemEffectDefinitions.GetAvailableEffects(baseItem, magicItem);
                    if (availableEffects.Count == 0)
                    {
                        _Log($"Tried to add more effects to magic item ({baseItem.m_shared.m_name}) but there were no more available effects. " +
                                            $"Current Effects: {(string.Join(", ", magicItem.Effects.Select(x => x.EffectType.ToString())))}");
                        break;
                    }

                    _weightedEffectTable.Setup(availableEffects, x => x.SelectionWeight);
                    var effectDef = _weightedEffectTable.Roll();

                    var effect = LootRoller.RollEffect(effectDef, magicItem.Rarity, magicItem.Quality, baseItem.m_shared.m_name);
                    magicItem.Effects.Add(effect);
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
            }

            var itemDrop = itemPrefab.GetComponent<ItemDrop>();
            if (!itemDrop)
            {
                _Log($"Item drop for {prefabName} not found!");
            }

            return itemDrop;
        }

        private static bool _PlainItem(string itemName)
        {
            string legendaryBasePrefabName = null;
            if (UniqueLegendaryHelper.IsLegendaryItem(itemName, out legendaryBasePrefabName))
            {
                return false;
            }

            return !EpicLoot.EpicLoot.CanBeMagicItem(_ItemDrop(itemName).m_itemData);
        }
        private static bool _PlayerKnowsItem(string itemName)
        {
            var itemDrop = _ItemDrop(itemName);

            return Player.m_localPlayer != null && Player.m_localPlayer.m_knownMaterial.Contains(itemDrop.m_itemData.m_shared.m_name);
        }

        private static bool _ItemAllowed(string itemName)
        {
            if (_PlayerKnowsItem(itemName))
            {
                return true;
            }

            var exceptions = LootRoller.Config.ItemDropLimitsExceptions;

            if (EpicLoot.EpicLoot.AllowItemDropLimitsExceptions.Value && exceptions != null && exceptions.Contains(itemName))
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
                _Log($"Item {item.Item}, Rarity {item.Rarity}, Quality {item.Quality}, Count {item.Count}");
            }

            foreach (var rolledItem in rolledItems)
            {
                var itemName = rolledItem.Item;

                ItemRarity rarity = ItemRarity.Magic;
                ItemQuality quality = ItemQuality.Inferior;

                if (_PlainItem(itemName))
                {
                    // nothing to do yet
                }
                else
                {
                    rarity = rolledItem.Rarity ?? ItemRarity.Magic;
                    quality = rolledItem.Quality ?? ItemQuality.Inferior;

                    var lootFilterForcedSacrifice = false;

                    string legendaryBasePrefabName = null;
                    if (UniqueLegendaryHelper.IsLegendaryItem(itemName, out legendaryBasePrefabName))
                    {
                        itemName = legendaryBasePrefabName;
                        rarity = ItemRarity.Legendary;

                        if(quality == ItemQuality.Inferior)
                        {
                            quality = ItemQuality.Normal;
                        }
                    }
                    else
                    {
                        if (LootFilterDefinitions.FilteredOut(itemName, rarity, quality, distanceFromWorldCenter, out lootFilterForcedSacrifice))
                        {
                            if (!lootFilterForcedSacrifice)
                            {
                                _Log($"Item filtered {itemName} out due to loot filters");
                                continue;
                            }
                        }
                    }

                    bool ReplaceWithMats()
                    {
                        if (itemName == null)
                        {
                            _Log($"Item replaced with materials due to its id being null");
                            return true;
                        }

                        if (lootFilterForcedSacrifice)
                        {
                            _Log($"Item replaced with materials due to loot filters");
                            return true;
                        }

                        if (!_ItemAllowed(itemName))
                        {
                            _Log($"Item replaced with materials due to being not allowed yet");
                            return true;
                        }

                        if (rarity != ItemRarity.Legendary)
                        {
                            if (quality != ItemQuality.Elite)
                            {
                                var player = Player.m_localPlayer;

                                var eliteKey = "EpicLoot_PlayerSeen_" + itemName + ItemQuality.Elite;
                                if (player.m_customData.ContainsKey(eliteKey))
                                {
                                    _Log($"Item replaced with materials due to player has already seen Elity quality of it");
                                    return true;
                                }

                                if (quality != ItemQuality.Exceptional)
                                {
                                    var exceptionalKey = "EpicLoot_PlayerSeen_" + itemName + ItemQuality.Exceptional;
                                    if (player.m_customData.ContainsKey(exceptionalKey))
                                    {
                                        _Log($"Item replaced with materials due to player has already seen Exceptional quality of it");
                                        return true;
                                    }

                                    if (quality != ItemQuality.Normal)
                                    {
                                        var normalKey = "EpicLoot_PlayerSeen_" + itemName + ItemQuality.Normal;
                                        if (player.m_customData.ContainsKey(normalKey))
                                        {
                                            _Log($"Item replaced with materials due to player has already seen Normal quality of it");
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
                            prefab = ObjectDB.instance.GetItemPrefab(itemName);
                        }
                        catch (Exception e)
                        {
                            _Log($"Unable to get Prefab for [{itemName}]. Continuing.");
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

                GameObject itemPrefab = null;

                itemPrefab = ObjectDB.instance.GetItemPrefab(itemName);
                var item = LootRoller.SpawnLootForDrop(itemPrefab, dropPoint, initializeObject);
                var itemDrop = item.GetComponent<ItemDrop>();

                if (_PlainItem(itemName))
                {
                    itemDrop.m_itemData.m_stack = rolledItem.Count ?? 1;
                }
                else
                {
                    var itemData = itemDrop.m_itemData;
                    var magicItemComponent = itemData.Data().GetOrCreate<MagicItemComponent>();
                    var magicItem = RollMagicItem(rolledItem.Item, rarity, quality, itemData, 0.0f);

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
    }
}
