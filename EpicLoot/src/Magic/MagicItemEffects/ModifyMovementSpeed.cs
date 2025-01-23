using HarmonyLib;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateMovementModifier))]
    public static class RemoveSpeedPenalty_Player_UpdateMovementModifier_Patch
    {
        public static void Postfix(Player __instance)
        {
            RemoveSpeedPenalties(__instance);

            float bonus = 0.0f;
                
            ModifyWithLowHealth.Apply(__instance, MagicEffectType.ModifyMovementSpeed, effect =>
            {
                bonus += __instance.GetTotalActiveMagicEffectValue(effect, 0.01f);
            });

            __instance.m_equipmentMovementModifier += bonus;
        }

        public static void RemoveSpeedPenalties(Player __instance)
        {
            foreach (var itemData in __instance.GetEquipment())
            {
                if (itemData != null && itemData.HasMagicEffect(MagicEffectType.RemoveSpeedPenalty))
                {
                    __instance.m_equipmentMovementModifier -= itemData.m_shared.m_movementModifier;
                }
            }
        }
    }
}
