using EpicLoot;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Raido
{
    public static class Raido
    {
        // public static Dictionary<string, string> sharedToPrefabNames = new Dictionary<string, string>();

        public static string GetPrefabName(ItemDrop.ItemData item)
        {
            return item.m_dropPrefab.name;
/*            if (sharedToPrefabNames.Count == 0)
            {
                List<string> itemNames = new List<string>();
                foreach(var items in DropEngine.EffectsConfig.EffectLevelScaling)
                {
                    foreach(var item in items.Items)
                    {
                        var prefab = ObjectDB.instance.GetItemPrefab(item);
                        if(prefab != null)
                        {
                            var itemDrop = prefab.GetComponent<ItemDrop>();
                            if(itemDrop != null)
                            {
                                var name = itemDrop?.m_itemData?.m_shared?.m_name;
                                if(name != null)
                                {
                                    sharedToPrefabNames[name] = item;
                                    _Log($"{name} -> {item}");
                                }
                            }
                        }
                    }
                }

                return sharedToPrefabNames[sharedName];
            }

            return sharedToPrefabNames[sharedName];*/
        }

        public static ItemDrop.ItemData GetItemData(string itemName)
        {
            var itemPrefab = ObjectDB.instance.GetItemPrefab(itemName);
            var itemDrop = itemPrefab.GetComponent<ItemDrop>();

            return itemDrop.m_itemData;
        }

        private static string _MagicItemKnownKey(string itemName, string itemClass, ItemQuality quality)
        {
            return $"Raido_PlayerKnows_{itemName}_{itemClass}_{quality}";
        }

        public static bool PlayerKnowsItem(ItemDrop.ItemData item)
        {
            var player = Player.m_localPlayer;
            var magicItem = item.GetMagicItem();
            var key = _MagicItemKnownKey(item.m_shared.m_name, magicItem.GetClass(), magicItem.Quality);

            return player.m_customData.ContainsKey(key);
        }

        public static bool PlayerKnowsItemClassAndQuality(string itemName, string itemClass, ItemQuality quality)
        {
            var player = Player.m_localPlayer;
            var key = _MagicItemKnownKey(GetItemData(itemName).m_shared.m_name, itemClass, quality);

            return player.m_customData.ContainsKey(key);
        }

        public static bool PlayerKnowsItemClassAndQuality(ItemDrop.ItemData item, string itemClass, ItemQuality quality)
        {
            var player = Player.m_localPlayer;
            var key = _MagicItemKnownKey(item.m_shared.m_name, itemClass, quality);

            return player.m_customData.ContainsKey(key);
        }

        public static void SetPlayerKnowsItem(ItemDrop.ItemData item)
        {
            var player = Player.m_localPlayer;
            var magicItem = item.GetMagicItem();
            var key = _MagicItemKnownKey(item.m_shared.m_name, magicItem.GetClass(), magicItem.Quality);

            if (!player.m_customData.ContainsKey(key))
            {
                player.m_customData.Add(key, "1");
                _Log($"Added known item {key}");
            }
            else
            {
                _Log($"Not added already known item {key}");
            }
        }

        public static void _Log(string message)
        {
            EpicLoot.EpicLoot.Log($"Raido -> {message}");
        }
    }
}