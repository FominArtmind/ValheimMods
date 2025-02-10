using HarmonyLib;

namespace EpicLoot
{
    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch(nameof(Inventory.AddItem))]
    [HarmonyPatch(new[] { typeof(ItemDrop.ItemData) })]
    public static class Inventory_AddItem_Patch
    {
        public static void Postfix(Inventory __instance, ItemDrop.ItemData item)
        {
            var player = Player.m_localPlayer;
            if (player != null && player.m_inventory == __instance)
            {
                if (item.IsMagic())
                {
                    var magicItem = item.GetMagicItem();
                    if(magicItem.Quality == ItemQuality.Exceptional || magicItem.Quality == ItemQuality.Elite)
                    {
                        var key = "EpicLoot_PlayerSeen_" + item.m_shared.m_name + magicItem.Quality;
                        if(!player.m_customData.ContainsKey(key))
                        {
                            player.m_customData.Add(key, "1");
                            EpicLoot.Log("Added " + key);
                        }
                        else
                        {
                            EpicLoot.Log("Not added " + key);
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch(nameof(Inventory.AddItem))]
    [HarmonyPatch(new[] { typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int) })]
    public static class Inventory_AddItem2_Patch
    {
        public static void Postfix(Inventory __instance, ItemDrop.ItemData item, int amount, int x, int y)
        {
            var player = Player.m_localPlayer;
            if (player != null && player.m_inventory == __instance)
            {
                if (item.IsMagic())
                {
                    var magicItem = item.GetMagicItem();
                    if (magicItem.Quality == ItemQuality.Exceptional || magicItem.Quality == ItemQuality.Elite)
                    {
                        var key = "EpicLoot_PlayerSeen_" + item.m_shared.m_name + magicItem.Quality;
                        if (!player.m_customData.ContainsKey(key))
                        {
                            player.m_customData.Add(key, "1");
                        }
                    }
                }
            }
        }
    }
}
