using System;
using System.Collections.Generic;

namespace Raido
{
    [Serializable]
    public class DefaultRarityRatesConfig
    {
        public float Magic;
        public float Rare;
        public float Epic;
    }

    [Serializable]
    public class DefaultStarQualityRatesConfig
    {
        public int Level;
        public float Inferior;
        public float Normal;
        public float Exceptional;
        public float Elite;
    }

    [Serializable]
    public class DefaultBiomeLevelQualityRatesConfig
    {
        public int Level;
        public float Inferior;
        public float Normal;
        public float Exceptional;
        public float Elite;
    }

    [Serializable]
    public class ItemGroupUnit
    {
        public string Item;
        // public string From;
        public int Weight;
        public string Class;
    }

    [Serializable]
    public class ItemGroup
    {
        public string Name;
        public ItemGroupUnit[] From;
    }

    [Serializable]
    public class DropUnit
    {
        public string Item;
        public string Class;
        public int Every = 1;
        public int Min = 1;
        public int Max = 1;
        public float? Rare;
        public float? Epic;
        public float? Normal;
        public float? Exceptional;
        public float? Elite;
        public int Repeat = 1;
    }

    [Serializable]
    public class CreatureDrop
    {
        public int Level;
        public List<DropUnit> Items;
    }

    [Serializable]
    public class CreatureDropTable
    {
        public string Name;
        public List<CreatureDrop> Drop;
    }

    [Serializable]
    public class ChestDrop
    {
        public int Distance;
        public int Level;
        public int Limit;
        public List<DropUnit> Items;
    }

    [Serializable]
    public class ChestDropTable
    {
        public string Name;
        public List<ChestDrop> Drop;
    }

    [Serializable]
    public class DropConfig
    {
        public DefaultRarityRatesConfig DefaultRarityRates;
        public List<DefaultStarQualityRatesConfig> DefaultStarQualityRates = new List<DefaultStarQualityRatesConfig>();
        public List<DefaultBiomeLevelQualityRatesConfig> DefaultBiomeLevelQualityRates = new List<DefaultBiomeLevelQualityRatesConfig>();

        public List<ItemGroup> ItemGroups = new List<ItemGroup>();

        public List<CreatureDropTable> Creatures = new List<CreatureDropTable>();
        public List<ChestDropTable> Chests = new List<ChestDropTable>();
    }
}
