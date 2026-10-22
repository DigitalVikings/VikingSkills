using System;
using System.Collections.Generic;
using UnityEngine;
using VikingLib.Combat;
using VikingLib.Stats;

namespace VikingSkills.Keystones
{
    public delegate IEnumerable<StatModifier> ModifierFactory(Player player, object owner);

    public class KeystoneDefinition
    {
        public string Id { get; }
        public Skills.SkillType SkillType { get; }
        public int MilestoneLevel { get; }
        public int SlotIndex { get; } // 0 or 1
        public string Name { get; }
        public string Description { get; }
        public string RuneSymbol { get; }
        public string RuneGlyph => RuneSymbol;
        public string? IconPrefab { get; set; }
        public string? IconName { get; set; }
        public bool IsSpecialMechanic { get; }

        public ModifierFactory? Modifiers { get; }

        private Sprite? _cachedIcon;

        public KeystoneDefinition(
            string id,
            Skills.SkillType skillType,
            int milestoneLevel,
            int slotIndex,
            string name,
            string description,
            string runeSymbol,
            ModifierFactory? modifiers = null,
            string? iconPrefab = null,
            string? iconName = null,
            bool isSpecialMechanic = false)
        {
            Id = id;
            SkillType = skillType;
            MilestoneLevel = milestoneLevel;
            SlotIndex = slotIndex;
            Name = name;
            Description = description;
            RuneSymbol = runeSymbol;
            Modifiers = modifiers;
            IconPrefab = iconPrefab;
            IconName = iconName;
            IsSpecialMechanic = isSpecialMechanic || (modifiers == null);
        }

        public Sprite? GetIcon(Player? player)
        {
            if (_cachedIcon != null) return _cachedIcon;

            _cachedIcon = KeystoneIconManager.GetIcon(this, player);
            return _cachedIcon;
        }

        public static KeystoneDefinition Stat(
            string id,
            Skills.SkillType skillType,
            int milestoneLevel,
            int slotIndex,
            string name,
            string description,
            string runeSymbol,
            CharacterStatType stat,
            float value,
            ModifierTier tier = ModifierTier.IncreasedPercent)
        {
            return new KeystoneDefinition(id, skillType, milestoneLevel, slotIndex, name, description, runeSymbol,
                (p, owner) =>
                {
                    StatModifier mod = tier switch
                    {
                        ModifierTier.Multiplier => StatModifier.Multiplier(stat, value, name, null, owner),
                        ModifierTier.Flat => new StatModifier(stat, ModifierTier.Flat, value, name, null, owner),
                        _ => StatModifier.Increased(stat, value, name, null, owner)
                    };
                    return new[] { mod };
                });
        }

        public static KeystoneDefinition MultiStat(
            string id,
            Skills.SkillType skillType,
            int milestoneLevel,
            int slotIndex,
            string name,
            string description,
            string runeSymbol,
            params (CharacterStatType stat, float value, ModifierTier tier)[] entries)
        {
            return new KeystoneDefinition(id, skillType, milestoneLevel, slotIndex, name, description, runeSymbol,
                (p, owner) =>
                {
                    var list = new List<StatModifier>(entries.Length);
                    foreach (var (stat, value, tier) in entries)
                    {
                        StatModifier mod = tier switch
                        {
                            ModifierTier.Multiplier => StatModifier.Multiplier(stat, value, name, null, owner),
                            ModifierTier.Flat => new StatModifier(stat, ModifierTier.Flat, value, name, null, owner),
                            _ => StatModifier.Increased(stat, value, name, null, owner)
                        };
                        list.Add(mod);
                    }
                    return list;
                });
        }
    }
}
