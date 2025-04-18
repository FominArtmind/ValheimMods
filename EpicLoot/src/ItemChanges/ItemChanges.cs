using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raido
{
    public static class ItemChanges
    {
        public static void ChangeItems()
        {
            var itemPrefab = ObjectDB.instance.GetItemPrefab("CrossbowArbalest");
            if (itemPrefab != null)
            {
                EpicLoot.EpicLoot.Log($"Found {itemPrefab}");
                var itemDrop = itemPrefab.GetComponent<ItemDrop>();
                var damages = itemDrop.m_itemData.m_shared.m_damages;
                EpicLoot.EpicLoot.Log($"Initial damages {damages}");
                damages.m_pierce = 100;
                EpicLoot.EpicLoot.Log($"Updated damages {damages}");
                var damagePerLevel = itemDrop.m_itemData.m_shared.m_damagesPerLevel;
                damagePerLevel.m_pierce = 25;
                itemDrop.m_itemData.m_shared.m_damages = damages;
                EpicLoot.EpicLoot.Log($"Updated damages 2 {itemDrop.m_itemData.m_shared.m_damages}");
                itemDrop.m_itemData.m_shared.m_damagesPerLevel = damagePerLevel;

                var itemPrefab2 = ObjectDB.instance.GetItemPrefab("CrossbowArbalest");
                var itemDrop2 = itemPrefab2.GetComponent<ItemDrop>();
                var damages2 = itemDrop2.m_itemData.m_shared.m_damages;
                EpicLoot.EpicLoot.Log($"Found {itemPrefab}, new damage: {damages2.m_pierce}");
            }
            else
            {
                EpicLoot.EpicLoot.Log($"Not found CrossbowArbalest");
            }

/*            var itemPrefab3 = ObjectDB.instance.GetItemPrefab("Torch");
            if (itemPrefab3 != null)
            {
                EpicLoot.EpicLoot.Log($"Found {itemPrefab3}");
                var itemDrop = itemPrefab.GetComponent<ItemDrop>();
                itemDrop.m_itemData.m_durability = 100;
                itemDrop.m_itemData.m_shared.m_maxDurability = 100;
            }*/
        }
    }
}
