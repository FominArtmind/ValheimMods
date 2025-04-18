using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using EpicLoot;
using JetBrains.Annotations;
using static ClutterSystem;

namespace Raido
{
    [Serializable]
    public class ItemClassGroupEffectConfig
    {
        public int Weight = 1;
        public string Type;
        public bool Core = false; 
        public float Power = 1.0f;
        public List<ItemQuality> Qualities = new List<ItemQuality>();
        public List<ItemRarity> Rarities = new List<ItemRarity>();
    }

    [Serializable]
    public class ItemClassEffectConfig : ItemClassGroupEffectConfig
    {
        public List<ItemClassGroupEffectConfig> Group = new List<ItemClassGroupEffectConfig>();
    }

    [Serializable]
    public class ItemClassConfig
    {
        public string Id;
        public string Name;
        public string Description = "";
        public List<ItemClassEffectConfig> Effects = new List<ItemClassEffectConfig>();
    }

    [Serializable]
    public class ItemAssignedClassConfig
    {
        public string Id;
        public int Weight;
    }

    [Serializable]
    public class ItemGroupAssignedClassesConfig
    {
        public List<string> Names = new List<string>();
        public List<ItemAssignedClassConfig> Classes = new List<ItemAssignedClassConfig>();
    }

    [Serializable]
    public class ItemResolvedGroupEffect
    {
        public int Weight = 1;
        public string Type;
        public bool Core = false;
        public float? Min;
        public float? Max;
        public float? Step;
    }

    public class ItemResolvedEffect : ItemResolvedGroupEffect
    {
        public List<ItemResolvedGroupEffect> Group = new List<ItemResolvedGroupEffect>();
    }

    [Serializable]
    public class ItemClassesConfig
    {
        public List<ItemClassConfig> Classes = new List<ItemClassConfig>();
        public List<ItemGroupAssignedClassesConfig> Items = new List<ItemGroupAssignedClassesConfig>();
        public MagicEffectsConfig EffectsConfig;

        private Dictionary<string, float[]> _effectRangeCache = new Dictionary<string, float[]>();
        private Dictionary<string, ItemResolvedGroupEffect> _resolvedEffectCache = new Dictionary<string, ItemResolvedGroupEffect>();

        public void Initialize(MagicEffectsConfig effectsConfig)
        {
            EffectsConfig = effectsConfig;
        }

        public string GetClassName(string itemClass)
        {
            var item = Classes.Find(value => value.Id == itemClass);
            return item.Name;
        }

        public List<KeyValuePair<string, int>> GetAvailableClasses(string prefabName)
        {
            var item = Items.Find(value => value.Names.Contains(prefabName));
            if(item == null)
            {
                return new List<KeyValuePair<string, int>>();
            }
            return item.Classes.Select(value => new KeyValuePair<string, int>(value.Id, value.Weight)).ToList();
        }

        public float[] GetEffectRangeForItem(string effectType, string prefabName, string itemClass, ItemQuality quality, ItemRarity rarity)
        {
            var _cacheKey = $"{effectType}{prefabName}{itemClass}{quality}{rarity}";
            if(_effectRangeCache.ContainsKey(_cacheKey))
            {
                // _Log("Getting effect range from cache");
                return _effectRangeCache[_cacheKey];
            }

            var c = Classes.Find(value => value.Id == itemClass);
            if(c != null)
            {
                var effect = c.Effects.Find(value => value.Type == effectType);
                if(effect != null)
                {
                    _effectRangeCache[_cacheKey] = EffectsConfig.GetEffectRangeForItem(effectType, prefabName, quality, rarity, effect.Power);
                    return _effectRangeCache[_cacheKey];
                }

                var groups = c.Effects.Where(value => value.Group != null && value.Group.Count > 0).ToList();
                foreach (var group in groups)
                {
                    var groupEffect = group.Group.Find(value => value.Type == effectType);
                    if (groupEffect != null)
                    {
                        _effectRangeCache[_cacheKey] = EffectsConfig.GetEffectRangeForItem(effectType, prefabName, quality, rarity, groupEffect.Power);
                        return _effectRangeCache[_cacheKey];
                    }
                }
            }

            _effectRangeCache[_cacheKey] = new float[] { 0.0f, 0.0f, 1.0f };
            return _effectRangeCache[_cacheKey];
        }

        ItemResolvedGroupEffect ResolveEffect(string type, int weight, bool core, float power, List<ItemQuality> qualities, List<ItemRarity> rarities, string prefabName, ItemQuality quality, ItemRarity rarity)
        {
            var _cacheKey = $"{type}{prefabName}{quality}{rarity}{power}";
            if (_resolvedEffectCache.ContainsKey(_cacheKey))
            {
                _Log($"Getting resolved effect {_cacheKey} from cache");
                return _resolvedEffectCache[_cacheKey];
            }

            var effectMetadata = EffectsConfig.EffectMetadata.Find(value => value.Type == type);
            if (effectMetadata != null && effectMetadata.AllowedOnItem(prefabName))
            {
                if ((qualities == null || qualities.Count == 0 || qualities.Contains(quality)) && (rarities == null || rarities.Count == 0 || rarities.Contains(rarity)))
                {
                    var resolvedEffect = new ItemResolvedGroupEffect() { Weight = weight, Type = type, Core = core };
                    if (!EffectsConfig.IsValuelessEffect(type))
                    {
                        var range = EffectsConfig.GetEffectRangeForItem(type, prefabName, quality, rarity, power, true);
                        resolvedEffect.Min = range[0];
                        resolvedEffect.Max = range[1];
                        resolvedEffect.Step = range[2];
                        _Log($"Generating resolved effect Min={resolvedEffect.Min} Max={resolvedEffect.Max} Step={resolvedEffect.Step}");
                    }

                    _resolvedEffectCache[_cacheKey] = resolvedEffect;
                    return resolvedEffect;
                }
            }

            _resolvedEffectCache[_cacheKey] = null;
            return null;
        }

        ItemResolvedGroupEffect ResolveEffect(ItemClassGroupEffectConfig effect, string prefabName, ItemQuality quality, ItemRarity rarity)
        {
            return ResolveEffect(effect.Type, effect.Weight, effect.Core, effect.Power, effect.Qualities, effect.Rarities, prefabName, quality, rarity);
        }

        ItemResolvedGroupEffect ResolveEffect(ItemClassEffectConfig effect, string prefabName, ItemQuality quality, ItemRarity rarity)
        {
            return ResolveEffect(effect.Type, effect.Weight, effect.Core, effect.Power, effect.Qualities, effect.Rarities, prefabName, quality, rarity);
        }

        public List<ItemResolvedEffect> GetAvailableEnchantEffects(string prefabName, string itemClass, ItemQuality quality, ItemRarity rarity, List<string> skippedEffectsNames = null)
        {
            var result = new List<ItemResolvedEffect>();

            var classData = Classes.Find(value => value.Id == itemClass);
            if(classData == null)
            {
                return result;
            }

            foreach(var effect in classData.Effects) {
                if (effect.Group != null && effect.Group.Count > 0)
                {
                    if (skippedEffectsNames != null)
                    {
                        bool skipGroupEffect = false;
                        foreach (var groupEffect in effect.Group)
                        {
                            if (skippedEffectsNames.Contains(groupEffect.Type))
                            {
                                skipGroupEffect = true;
                                break;
                            }
                        }
                        if (skipGroupEffect)
                        {
                            continue;
                        }
                    }

                    var group = new ItemResolvedEffect() { Weight = effect.Weight, Core = effect.Core };
                    foreach (var groupEffect in effect.Group)
                    {
                        var eff = ResolveEffect(groupEffect, prefabName, quality, rarity);
                        if (eff != null)
                        {
                            group.Group.Add(eff);
                        }
                    }
                    if(group.Group.Count > 0)
                    {
                        if (group.Group.Count == 1)
                        {
                            var eff = group.Group[0];
                            result.Add(new ItemResolvedEffect() { Weight = group.Weight, Type = eff.Type, Core = group.Core, Min = eff.Min, Max = eff.Max, Step = eff.Step });
                        }
                        else
                        {
                            result.Add(group);
                        }
                    }
                }
                else
                {
                    if (skippedEffectsNames == null || !skippedEffectsNames.Contains(effect.Type))
                    {
                        var eff = ResolveEffect(effect, prefabName, quality, rarity);
                        if (eff != null)
                        {
                            result.Add(new ItemResolvedEffect() { Weight = eff.Weight, Type = eff.Type, Core = eff.Core, Min = eff.Min, Max = eff.Max, Step = eff.Step });
                        }
                    }
                }
            }

            return result;
        }

        public List<ItemResolvedEffect> GetAvailableAugmentEffects(ItemDrop.ItemData item, int replacedEffectIndex)
        {
            var result = new List<ItemResolvedEffect>();

            var magicItem = item.GetMagicItem();
            var classData = Classes.Find(value => value.Id == magicItem.GetClass());
            if (classData == null)
            {
                return result;
            }

            var prefabName = Raido.GetPrefabName(item);
            var quality = magicItem.Quality;
            var rarity = magicItem.Rarity;

            string replacedType = null;
            if (replacedEffectIndex >= 0)
            {
                replacedType = magicItem.Effects[replacedEffectIndex].EffectType;
                foreach (var effect in classData.Effects)
                {
                    if (effect.Type == replacedType && (effect.Core || effect.Weight == 0))
                    {
                        var eff = ResolveEffect(effect, prefabName, quality, rarity);
                        if (eff != null)
                        {
                            result.Add(new ItemResolvedEffect() { Weight = eff.Weight, Type = eff.Type, Core = eff.Core, Min = eff.Min, Max = eff.Max, Step = eff.Step });
                        }
                        return result;
                    }

                    if ((effect.Core || effect.Weight == 0) && effect.Group != null && effect.Group.Count > 0)
                    {
                        var group = new ItemResolvedEffect() { Weight = effect.Weight, Core = effect.Core };
                        bool found = false;
                        foreach (var groupEffect in effect.Group)
                        {
                            var eff = ResolveEffect(groupEffect, prefabName, quality, rarity);
                            if (eff != null)
                            {
                                group.Group.Add(eff);
                            }
                            if (groupEffect.Type == replacedType)
                            {
                                found = true;
                            }
                        }

                        if (found)
                        {
                            if (group.Group.Count > 0)
                            {
                                if (group.Group.Count == 1)
                                {
                                    var eff = group.Group[0];
                                    result.Add(new ItemResolvedEffect() { Weight = group.Weight, Type = eff.Type, Core = group.Core, Min = eff.Min, Max = eff.Max, Step = eff.Step });
                                }
                                else
                                {
                                    result.Add(group);
                                }
                            }
                            return result;
                        }
                    }
                }
            }

            var itemRemainingEffects = magicItem.Effects.Select(value => value.EffectType).ToList();
            if(replacedType != null)
            {
                itemRemainingEffects.Remove(replacedType);
            }

            foreach (var effect in classData.Effects)
            {
                if (effect.Group != null && effect.Group.Count > 0)
                {
                    bool foundInGroup = false;
                    foreach (var remainingEffect in itemRemainingEffects)
                    {
                        if (effect.Group.Find(value => value.Type == remainingEffect) != null)
                        {
                            foundInGroup = true;
                            break;
                        }
                    }

                    if(!foundInGroup)
                    {
                        var group = new ItemResolvedEffect() { Weight = effect.Weight, Core = effect.Core };
                        foreach (var groupEffect in effect.Group)
                        {
                            var eff = ResolveEffect(groupEffect, prefabName, quality, rarity);
                            if (eff != null)
                            {
                                group.Group.Add(eff);
                            }
                        }

                        if (group.Group.Count > 0)
                        {
                            if (group.Group.Count == 1)
                            {
                                var eff = group.Group[0];
                                result.Add(new ItemResolvedEffect() { Weight = group.Weight, Type = eff.Type, Core = group.Core, Min = eff.Min, Max = eff.Max, Step = eff.Step });
                            }
                            else
                            {
                                result.Add(group);
                            }
                        }
                    }
                }
                else
                {
                    if (!itemRemainingEffects.Contains(effect.Type))
                    {
                        var eff = ResolveEffect(effect, prefabName, quality, rarity);
                        if (eff != null)
                        {
                            result.Add(new ItemResolvedEffect() { Weight = eff.Weight, Type = eff.Type, Core = eff.Core, Min = eff.Min, Max = eff.Max, Step = eff.Step });
                        }
                    }
                }
            }

            return result;
        }

        public void _Log(string message)
        {
            EpicLoot.EpicLoot.Log($"RaidoItemClasses -> {message}");
        }
    }
}
