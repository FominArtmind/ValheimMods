using EpicLoot.LegendarySystem;
using EpicLoot.Data;
using HarmonyLib;
using System.Linq;

namespace EpicLoot
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GiveDefaultItems))]
    public static class Farewell_Patch
    {
        public static void Postfix(Humanoid __instance)
        {
            if (__instance.name == "Player(Clone)")
            {
                var inventory = __instance.m_inventory;

                EpicLoot.Log($"Humanoid is player, trying to give Farewell");

                foreach (var itemData in inventory.m_inventory)
                {
                    if (itemData.m_shared.m_name == "$item_torch")
                    {
                        var itemInfo = UniqueLegendaryHelper.LegendaryInfo.Values.Where(x => x.ID == "Farewell").ToList()[0];

                        if (itemInfo != null)
                        {
                            var magicItem = new MagicItem { Rarity = ItemRarity.Legendary, Quality = ItemQuality.Normal, ItemName = itemData.m_shared.m_name };
                            magicItem.LegendaryID = itemInfo.ID;
                            magicItem.DisplayName = itemInfo.Name;

                            foreach (var guaranteedMagicEffect in itemInfo.GuaranteedMagicEffects)
                            {
                                var effectDef = MagicItemEffectDefinitions.Get(guaranteedMagicEffect.Type);
                                if (effectDef == null)
                                {
                                    EpicLoot.LogError($"Could not find magic effect (Type={guaranteedMagicEffect.Type}) while creating legendary item (ID={itemInfo.ID})");
                                    continue;
                                }

                                var effect = LootRoller.RollEffect(effectDef, magicItem.Rarity, magicItem.Quality, itemData.m_shared.m_name, guaranteedMagicEffect.Values);
                                magicItem.Effects.Add(effect);
                            }

                            var magicItemComponent = itemData.Data().GetOrCreate<MagicItemComponent>();
                            magicItemComponent.SetMagicItem(magicItem);

                            float durability = itemData.GetMaxDurability() * 2.7f;
                            itemData.m_shared.m_maxDurability = durability;
                            itemData.m_durability = durability;

                            EpicLoot.Log($"Farewell given");
                        }
                    }
                }
            }
        }
    }
}
