using System;
using System.Collections.Generic;
using Raido;
using EpicLoot;
using JetBrains.Annotations;
using UnityEngine;

namespace Raido
{
    [Serializable]
    public class EffectCountConfig
    {
        public int[][] Magic;
        public int[][] Rare;
        public int[][] Epic;
    }

    [Serializable]
    public class EffectCountDistributionConfig
    {
        public EffectCountConfig Inferior;
        public EffectCountConfig Normal;
        public EffectCountConfig Exceptional;
        public EffectCountConfig Elite;
    }

    [Serializable]
    public class EffectPowerConfig
    {
        public float[] Magic;
        public float[] Rare;
        public float[] Epic;
    }

    public class EffectPowerDistributionConfig
    {
        public EffectPowerConfig Inferior;
        public EffectPowerConfig Normal;
        public EffectPowerConfig Exceptional;
        public EffectPowerConfig Elite;
    }

    [Serializable]
    public class EffectLevelScalingConfig
    {
        // Effect Resulting Value = Value * (1.0 + EffectMetadata.Scaling * (EffectLevelScaling[Level -> ItemLevel].Power - 1.0))
        public int Level;
        public string Tier;
        public float Power;
        public List<string> Items = new List<string>();
    }

    [Serializable]
    public class EffectMetadataConfig
    {
        public string Type;
        public string DisplayText = "";
        public string Description = "";
        public float Scaling = 0.0f;
        public float Rounding = 1.0f;
        public bool CanBeAugmented = true;
        public bool CanBeDisenchanted = true;
        public List<Skills.SkillType> AllowedSkillTypes = new List<Skills.SkillType>();
        public List<Skills.SkillType> ExcludedSkillTypes = new List<Skills.SkillType>();
        public bool? ItemHasPhysicalDamage;
        public bool? ItemHasElementalDamage;
        public bool? ItemHasChopDamage;
        public bool? ItemUsesDurability;
        public bool? ItemHasNegativeMovementSpeedModifier;
        public bool? ItemHasBlockPower;
        public bool? ItemHasParryPower;
        public bool? ItemHasNoParryPower;
        public bool? ItemHasArmor;
        public bool? ItemHasBackstabBonus;
        public bool? ItemUsesStaminaOnAttack;
        public bool? ItemUsesEitrOnAttack;
        public bool? ItemUsesHealthOnAttack;
        public bool? ItemUsesDrawStaminaOnAttack;
        public string Ability;

        public bool AllowedOnItem(string prefabName)
        {
            var itemData = Raido.GetItemData(prefabName);

            if (AllowedSkillTypes?.Count > 0 && !AllowedSkillTypes.Contains(itemData.m_shared.m_skillType))
            {
                return false;
            }

            if (ExcludedSkillTypes?.Count > 0 && ExcludedSkillTypes.Contains(itemData.m_shared.m_skillType))
            {
                return false;
            }

            if (ItemHasPhysicalDamage != null &&
                (ItemHasPhysicalDamage == itemData.m_shared.m_damages.GetTotalPhysicalDamage() <= 0))
            {
                return false;
            }

            if (ItemHasElementalDamage != null &&
                (ItemHasElementalDamage == itemData.m_shared.m_damages.GetTotalElementalDamage() <= 0))
            {
                return false;
            }

            if (ItemHasChopDamage != null &&
                (ItemHasChopDamage == itemData.m_shared.m_damages.m_chop <= 0))
            {
                return false;
            }

            if (ItemUsesDurability != null &&
                (ItemUsesDurability == !itemData.m_shared.m_useDurability))
            {
                return false;
            }

            if (ItemHasNegativeMovementSpeedModifier != null &&
                (ItemHasNegativeMovementSpeedModifier == itemData.m_shared.m_movementModifier >= 0))
            {
                return false;
            }

            if (ItemHasBlockPower != null && (ItemHasBlockPower == itemData.m_shared.m_blockPower <= 0))
            {
                return false;
            }

            if (ItemHasParryPower != null && (ItemHasParryPower == itemData.m_shared.m_timedBlockBonus <= 0))
            {
                return false;
            }

            if (ItemHasNoParryPower != null && (ItemHasNoParryPower == itemData.m_shared.m_timedBlockBonus > 0))
            {
                return false;
            }

            if (ItemHasArmor != null && (ItemHasArmor == itemData.m_shared.m_armor <= 0))
            {
                return false;
            }

            if (ItemHasBackstabBonus != null && (ItemHasBackstabBonus == itemData.m_shared.m_backstabBonus <= 0))
            {
                return false;
            }

            if (ItemUsesStaminaOnAttack != null)
            {
                bool hasStamina = itemData.m_shared.m_attack.m_attackStamina > 0 ||
                    itemData.m_shared.m_secondaryAttack.m_attackStamina > 0;
                if (ItemUsesStaminaOnAttack.Value != hasStamina)
                {
                    return false;
                }
            }

            if (ItemUsesEitrOnAttack != null)
            {
                bool hasEitr = itemData.m_shared.m_attack.m_attackEitr > 0 ||
                    itemData.m_shared.m_attack.m_drawEitrDrain > 0 ||
                    itemData.m_shared.m_attack.m_reloadEitrDrain > 0 ||
                    itemData.m_shared.m_secondaryAttack.m_attackEitr > 0 ||
                    itemData.m_shared.m_secondaryAttack.m_drawEitrDrain > 0 ||
                    itemData.m_shared.m_secondaryAttack.m_reloadEitrDrain > 0;

                if (ItemUsesEitrOnAttack.Value != hasEitr)
                {
                    return false;
                }
            }

            if (ItemUsesHealthOnAttack != null)
            {
                bool usesHealth = itemData.m_shared.m_attack.m_attackHealth > 0 ||
                    itemData.m_shared.m_secondaryAttack.m_attackHealth > 0;

                if (ItemUsesHealthOnAttack.Value != usesHealth)
                {
                    return false;
                }
            }

            if (ItemUsesDrawStaminaOnAttack != null)
            {
                bool drawStamina = itemData.m_shared.m_attack.m_drawStaminaDrain > 0 ||
                    itemData.m_shared.m_secondaryAttack.m_drawStaminaDrain > 0;

                if (ItemUsesDrawStaminaOnAttack.Value != drawStamina)
                {
                    return false;
                }
            }

            return true;
        }
    }

    [Serializable]
    public class MagicEffectsConfig
    {
        public EffectCountDistributionConfig EffectCountDistribution;
        public EffectPowerDistributionConfig EffectPowerDistribution;
        public List<EffectLevelScalingConfig> EffectLevelScaling = new List<EffectLevelScalingConfig>();
        public Dictionary<string, float> EffectValues = new Dictionary<string, float>();
        public List<EffectMetadataConfig> EffectMetadata = new List<EffectMetadataConfig>();

        public EffectMetadataConfig GetEffectMetadata(string effectType)
        {
            return EffectMetadata.Find(value => value.Type == effectType);
        }

        public bool IsValuelessEffect(string effectType)
        {
            return !EffectValues.ContainsKey(effectType);
        }

        public float[] GetEffectRangeForItem(string effectType, string prefabName, ItemQuality quality, ItemRarity rarity, float power = 1.0f, bool logging = false)
        {
            if (IsValuelessEffect(effectType))
            {
                if(logging)
                    _Log($"Valueless effect {effectType}, setting values to 1.0f");
                return new float[] { 1.0f, 1.0f, 1.0f };
            }

            if (logging)
                _Log($"Getting effect range for {effectType}");
            var metadata = EffectMetadata.Find(value => value.Type == effectType);
            if (logging)
                _Log($"Power {power}");
            var scaling = power;
            if (metadata.Scaling != 0.0f)
            {
                var levelData = EffectLevelScaling.Find(value => value.Items.Contains(prefabName));
                if (levelData != null)
                {
                    scaling *= (1.0f + metadata.Scaling * (levelData.Power - 1.0f));
                    if (logging)
                        _Log($"Scaled power {scaling}");
                }
            }

            EffectPowerConfig dist = null;
            switch (quality)
            {
                case ItemQuality.Elite:
                    dist = EffectPowerDistribution.Elite;
                    break;
                case ItemQuality.Exceptional:
                    dist = EffectPowerDistribution.Exceptional;
                    break;
                case ItemQuality.Normal:
                    dist = EffectPowerDistribution.Normal;
                    break;
                default:
                case ItemQuality.Inferior:
                    dist = EffectPowerDistribution.Inferior;
                    break;
            }
            float[] qrPower;
            switch (rarity)
            {
                case ItemRarity.Epic:
                    qrPower = dist.Epic;
                    break;
                case ItemRarity.Rare:
                    qrPower = dist.Rare;
                    break;
                default:
                case ItemRarity.Magic:
                    qrPower = dist.Magic;
                    break;
            }

            if (logging)
                _Log($"QR Power distribution {qrPower[0]} {qrPower[1]}");

            var baseValue = EffectValues[effectType];

            if (logging)
                _Log($"Effect base value {baseValue}");

            float[] RoundRange(float[] range, float rounding)
            {
                float RoundToNearest(float value)
                {
                    return (float)(Math.Round(value / rounding) * rounding);
                }

                float min = RoundToNearest(range[0]);
                float max = RoundToNearest(range[1]);

                min = Math.Max(min, rounding);
                max = Math.Max(min, max);

                if (logging)
                    _Log($"Rounded range {min} - {max}, rounding {rounding}");
                return new float[] { min, max, rounding };
            }

            return RoundRange(new float[] { baseValue * qrPower[0] * scaling, baseValue * qrPower[1] * scaling }, metadata.Rounding);
        }

        private static void _Log(string message)
        {
            EpicLoot.EpicLoot.Log($"Raido.MagicEffectsConfig -> {message}");
        }
    }
}
