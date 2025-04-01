using System;
using System.Collections.Generic;
using Common;

namespace EpicLoot.LegendarySystem
{
    [Serializable]
    public class MagicEffect
    {
        public string Type;
        public MagicItemEffectDefinition.ValueDef Values;
    }

    [Serializable]
    public class TextureReplacement
    {
        public string ItemID;
        public string MainTexture;
        public string ChestTex;
        public string LegsTex;
    }

    [Serializable]
    public class LegendaryInfo
    {
        public string Id;
        public string Name;
        public string Item;
        public string Description;
        public MagicItemEffectRequirements Requirements;
        public List<MagicEffect> Normal = new List<MagicEffect>();
        public List<MagicEffect> Exceptional = new List<MagicEffect>();
        public List<MagicEffect> Elite = new List<MagicEffect>();
        public string EquipFx;
        public FxAttachMode EquipFxMode = FxAttachMode.Player;
        public List<TextureReplacement> TextureReplacements = new List<TextureReplacement>();
        public bool IsSetItem;
        public bool Enchantable;
        public List<RecipeRequirementConfig> EnchantCost = new List<RecipeRequirementConfig>();
    }

    [Serializable]
    public class SetBonusInfo
    {
        public int Count;
        public MagicEffect Effect;
    }

    [Serializable]
    public class LegendarySetInfo
    {
        public string Id;
        public string Name;
        public List<string> Items = new List<string>();
        public List<SetBonusInfo> SetBonuses = new List<SetBonusInfo>();
        public List<SetBonusInfo> SetBonusesExceptional = new List<SetBonusInfo>();
        public List<SetBonusInfo> SetBonusesElite = new List<SetBonusInfo>();
    }

    [Serializable]
    public class LegendaryItemConfig
    {
        public List<LegendaryInfo> LegendaryItems = new List<LegendaryInfo>();
        public List<LegendarySetInfo> LegendarySets = new List<LegendarySetInfo>();
        public List<LegendaryInfo> MythicItems = new List<LegendaryInfo>();
        public List<LegendarySetInfo> MythicSets = new List<LegendarySetInfo>();
    }
}
