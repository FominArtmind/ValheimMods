using EpicLoot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static ClutterSystem;

namespace Raido
{
    public class KnownItemClasses
    {
        public string prefabName;
        public Dictionary<string, ItemQuality?> classes;
    }

    public class KnownItemTierClasses
    {
        public string tier;
        public List<KnownItemClasses> itemsClasses;
    }

    public static class Raido
    {
        // public static Dictionary<string, string> sharedToPrefabNames = new Dictionary<string, string>();

        public static string GetPrefabName(ItemDrop.ItemData item)
        {
            return item.m_dropPrefab.name;
        }

        public static ItemDrop.ItemData GetItemData(string prefabName)
        {
            var itemPrefab = ObjectDB.instance.GetItemPrefab(prefabName);
            var itemDrop = itemPrefab.GetComponent<ItemDrop>();

            return itemDrop.m_itemData;
        }

        private static string _MagicItemKnownKey(string itemName, string itemClass, ItemQuality quality)
        {
            return $"Raido:ItemKnown:{itemName}:{itemClass}:{quality}";
        }

        public static bool PlayerKnowsItem(ItemDrop.ItemData item)
        {
            var player = Player.m_localPlayer;
            var magicItem = item.GetMagicItem();
            var key = _MagicItemKnownKey(item.m_shared.m_name, magicItem.GetClass(), magicItem.Quality);

            return player.m_customData.ContainsKey(key);
        }

        public static bool PlayerKnowsItemClassAndQuality(string prefabName, string itemClass, ItemQuality quality)
        {
            var player = Player.m_localPlayer;
            var key = _MagicItemKnownKey(GetItemData(prefabName).m_shared.m_name, itemClass, quality);

            return player.m_customData.ContainsKey(key);
        }

        public static bool PlayerKnowsItemClassAndQuality(ItemDrop.ItemData item, string itemClass, ItemQuality quality)
        {
            var player = Player.m_localPlayer;
            var key = _MagicItemKnownKey(item.m_shared.m_name, itemClass, quality);

            return player.m_customData.ContainsKey(key);
        }

        public static bool PlayerKnowsItemClass(ItemDrop.ItemData item, string itemClass)
        {
            var player = Player.m_localPlayer;

            var qualities = new List<ItemQuality>() { ItemQuality.Inferior, ItemQuality.Normal, ItemQuality.Exceptional, ItemQuality.Elite };
            foreach(var quality in qualities )
            {
                var key = _MagicItemKnownKey(item.m_shared.m_name, itemClass, quality);
                if(player.m_customData.ContainsKey(key))
                {
                    return true;
                }
            }

            return false;
        }

        public static void SetPlayerKnowsItem(ItemDrop.ItemData item)
        {
            var player = Player.m_localPlayer;
            var magicItem = item.GetMagicItem();
            var key = _MagicItemKnownKey(item.m_shared.m_name, magicItem.GetClass(), magicItem.Quality);

/*            foreach(var record in player.m_customData)
            {
                _Log($"{record.Key}: {record.Value}");
            }*/

            if (!player.m_customData.ContainsKey(key))
            {
                player.m_customData.Add(key, "1");
                // _Log($"Added known item {key}");
            }
            else
            {
                // _Log($"Not added already known item {key}");
            }
        }

        // item, class, quality
        public static Dictionary<string, Dictionary<string, ItemQuality>> PlayerKnownItemsClasses()
        {
            var player = Player.m_localPlayer;

            // Raido:ItemKnown:$item_helmet_bronze:MediumHelmet:Inferior 
            var knownRecords = player.m_customData.Where(value => value.Key.StartsWith("Raido:ItemKnown:")).ToList().Select(value => value.Key).ToList();

            var knownItemClasses = new Dictionary<string, Dictionary<string, ItemQuality>>();
            foreach (var record in knownRecords)
            {
                var parts = record.Split(':');
                var itemName = parts[2];
                var itemClass = parts[3];

                if (Enum.TryParse<ItemQuality>(parts[4], out var quality))
                {
                    if (knownItemClasses.TryGetValue(itemName, out var classes))
                    {
                        if (classes.TryGetValue(itemClass, out var topQuality))
                        {
                            if(quality > topQuality)
                            {
                                classes[itemClass] = quality;
                            }
                        }
                        else
                        {
                            classes[itemClass] = quality;
                        }
                    }
                    else
                    {
                        var itemClasses = new Dictionary<string, ItemQuality>();
                        itemClasses[itemClass] = quality;
                        knownItemClasses[itemName] = itemClasses;
                    }
                }
            }

            return knownItemClasses;
        }

        public static void _Log(string message)
        {
            EpicLoot.EpicLoot.Log($"Raido -> {message}");
        }
    }
}