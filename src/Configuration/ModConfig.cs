using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using VikingSkills.Data;
using UnityEngine;

namespace VikingSkills.Configuration
{
    public enum SkillLossMode
    {
        Percentage,
        FlatLevels,
        None
    }

    public static class ModConfig
    {
        private const string SectionGeneral = "01 - General";
        private const string SectionVisualTheme = "02 - Visual Theme";
        private const string SectionSkillLoss = "03 - Progression - Skill Loss on Death";
        private const string SectionSkillGain = "04 - Progression - Skill Gain Multipliers";
        private const string SectionHearthside = "05 - Hearthside Attunement";

        private const string LegacySectionGeneral = "1 - General";
        private const string LegacySectionVisualTheme = "2 - Visual Theme";
        private const string LegacySectionSkillLoss = "3 - Progression: Skill Loss on Death";
        private const string LegacySectionSkillGain = "4 - Progression: Skill Gain Multipliers";

        private static readonly PropertyInfo? OrphanedEntriesProp =
            typeof(ConfigFile).GetProperty("OrphanedEntries", BindingFlags.Instance | BindingFlags.NonPublic);

        // 01 - General
        public static ConfigEntry<bool> ModEnabled { get; private set; } = null!;
        public static ConfigEntry<bool> EnableVanillaSkillScaling { get; private set; } = null!;
        public static ConfigEntry<bool> EnableAudioEffects { get; private set; } = null!;

        // 02 - Visual Theme
        public static ConfigEntry<string> WeaponsColorHex { get; private set; } = null!;
        public static ConfigEntry<string> DefenseColorHex { get; private set; } = null!;
        public static ConfigEntry<string> MagicColorHex { get; private set; } = null!;
        public static ConfigEntry<string> MobilityColorHex { get; private set; } = null!;
        public static ConfigEntry<string> ResourcesColorHex { get; private set; } = null!;

        // 03 - Progression - Skill Loss on Death
        public static ConfigEntry<SkillLossMode> DeathSkillLossMode { get; private set; } = null!;
        public static ConfigEntry<float> DeathSkillLossPercent { get; private set; } = null!;
        public static ConfigEntry<float> DeathSkillLossFlatLevels { get; private set; } = null!;
        public static ConfigEntry<bool> ProtectUnlockedMilestones { get; private set; } = null!;

        // 04 - Progression - Skill Gain Multipliers
        public static ConfigEntry<float> GlobalSkillGainMultiplier { get; private set; } = null!;
        public static ConfigEntry<float> WeaponsGainMultiplier { get; private set; } = null!;
        public static ConfigEntry<float> DefenseGainMultiplier { get; private set; } = null!;
        public static ConfigEntry<float> MagicGainMultiplier { get; private set; } = null!;
        public static ConfigEntry<float> MobilityGainMultiplier { get; private set; } = null!;
        public static ConfigEntry<float> ResourcesGainMultiplier { get; private set; } = null!;

        // 05 - Hearthside Attunement
        public static ConfigEntry<bool> EnableHearthsideAttunement { get; private set; } = null!;
        public static ConfigEntry<bool> RequireRestedStatus { get; private set; } = null!;
        public static ConfigEntry<int> ComfortThresholdLv20 { get; private set; } = null!;
        public static ConfigEntry<int> ComfortThresholdLv40 { get; private set; } = null!;
        public static ConfigEntry<int> ComfortThresholdLv60 { get; private set; } = null!;
        public static ConfigEntry<int> ComfortThresholdLv80 { get; private set; } = null!;
        public static ConfigEntry<int> ComfortThresholdLv100 { get; private set; } = null!;

        public static void Init(ConfigFile config)
        {
            var orphaned = OrphanedEntriesProp?.GetValue(config) as Dictionary<ConfigDefinition, string>;
            bool migrated = false;

            // 01 - General
            ModEnabled = Bind(
                config,
                SectionGeneral,
                "Enabled",
                true,
                "Master switch to enable or disable all features of VikingSkills.",
                order: 100,
                isAdminOnly: false,
                legacySection: LegacySectionGeneral,
                orphaned: orphaned,
                migrated: ref migrated
            );

            EnableVanillaSkillScaling = Bind(
                config,
                SectionGeneral,
                "EnableVanillaSkillScaling",
                false,
                "When false (default), individual skill levels grant no passive scaling; all power derives from socketed Keystones. Set to true to re-enable vanilla fractional scaling alongside Keystones.",
                order: 90,
                isAdminOnly: true,
                legacySection: LegacySectionGeneral,
                orphaned: orphaned,
                migrated: ref migrated
            );

            EnableAudioEffects = Bind(
                config,
                SectionGeneral,
                "EnableAudioEffects",
                true,
                "Play native Valheim UI sound effects when opening the window, hovering over rune columns, and attuning Keystones.",
                order: 80,
                isAdminOnly: false,
                legacySection: LegacySectionGeneral,
                orphaned: orphaned,
                migrated: ref migrated
            );

            // 02 - Visual Theme
            WeaponsColorHex = Bind(
                config,
                SectionVisualTheme,
                "WeaponsColor",
                "#EB5757",
                "Hex color code for Weapons category columns and rune glow.",
                order: 70,
                isAdminOnly: false,
                legacySection: LegacySectionVisualTheme,
                orphaned: orphaned,
                migrated: ref migrated
            );

            DefenseColorHex = Bind(
                config,
                SectionVisualTheme,
                "DefenseColor",
                "#4EA8DE",
                "Hex color code for Defense category columns and rune glow.",
                order: 60,
                isAdminOnly: false,
                legacySection: LegacySectionVisualTheme,
                orphaned: orphaned,
                migrated: ref migrated
            );

            MagicColorHex = Bind(
                config,
                SectionVisualTheme,
                "MagicColor",
                "#9B5DE5",
                "Hex color code for Magic category columns and rune glow.",
                order: 50,
                isAdminOnly: false,
                legacySection: LegacySectionVisualTheme,
                orphaned: orphaned,
                migrated: ref migrated
            );

            MobilityColorHex = Bind(
                config,
                SectionVisualTheme,
                "MobilityColor",
                "#F4A261",
                "Hex color code for Mobility category columns and rune glow.",
                order: 40,
                isAdminOnly: false,
                legacySection: LegacySectionVisualTheme,
                orphaned: orphaned,
                migrated: ref migrated
            );

            ResourcesColorHex = Bind(
                config,
                SectionVisualTheme,
                "ResourcesColor",
                "#2EC4B6",
                "Hex color code for Resources & Crafting category columns and rune glow.",
                order: 30,
                isAdminOnly: false,
                legacySection: LegacySectionVisualTheme,
                orphaned: orphaned,
                migrated: ref migrated
            );

            // 03 - Progression - Skill Loss on Death
            DeathSkillLossMode = Bind(
                config,
                SectionSkillLoss,
                "DeathSkillLossMode",
                SkillLossMode.None,
                "Determines how skill loss is calculated upon death. Overrides vanilla world modifiers so values are exact.\n" +
                "Percentage: Loses a fraction of current level (Vanilla default is 5%).\n" +
                "FlatLevels: Loses a fixed number of levels regardless of current level.\n" +
                "None: Completely disables skill loss on death.",
                order: 100,
                isAdminOnly: true,
                legacySection: LegacySectionSkillLoss,
                orphaned: orphaned,
                migrated: ref migrated
            );

            ProtectUnlockedMilestones = Bind(
                config,
                SectionSkillLoss,
                "ProtectUnlockedMilestones",
                true,
                "When enabled, death penalty cannot reduce a skill below its highest unlocked milestone tier (20, 40, 60, 80, 100). Prevents losing socketed Keystones upon death.",
                order: 90,
                isAdminOnly: true,
                legacySection: LegacySectionSkillLoss,
                orphaned: orphaned,
                migrated: ref migrated
            );

            DeathSkillLossPercent = Bind(
                config,
                SectionSkillLoss,
                "DeathSkillLossPercent",
                0.05f,
                "Fraction of current skill level lost on death when DeathSkillLossMode is set to Percentage (0.05 = 5%).",
                order: 70,
                new AcceptableValueRange<float>(0f, 1f),
                isAdminOnly: true,
                legacySection: LegacySectionSkillLoss,
                orphaned: orphaned,
                migrated: ref migrated
            );

            DeathSkillLossFlatLevels = Bind(
                config,
                SectionSkillLoss,
                "DeathSkillLossFlatLevels",
                1.0f,
                "Number of skill levels lost on death when DeathSkillLossMode is set to FlatLevels (units: levels).",
                order: 60,
                new AcceptableValueRange<float>(0f, 20f),
                isAdminOnly: true,
                legacySection: LegacySectionSkillLoss,
                orphaned: orphaned,
                migrated: ref migrated
            );

            // 04 - Progression - Skill Gain Multipliers
            GlobalSkillGainMultiplier = Bind(
                config,
                SectionSkillGain,
                "GlobalSkillGainMultiplier",
                1.0f,
                "Global multiplier applied to all skill XP gains (units: x multiplier). Overrides world modifiers so value is exact.",
                order: 100,
                new AcceptableValueRange<float>(0.01f, 20f),
                isAdminOnly: true,
                legacySection: LegacySectionSkillGain,
                orphaned: orphaned,
                migrated: ref migrated
            );

            WeaponsGainMultiplier = Bind(
                config,
                SectionSkillGain,
                "WeaponsGainMultiplier",
                1.0f,
                "Multiplier for Weapons category skill XP gains (units: x multiplier, stacks with Global multiplier).",
                order: 70,
                new AcceptableValueRange<float>(0.01f, 10f),
                isAdminOnly: true,
                legacySection: LegacySectionSkillGain,
                orphaned: orphaned,
                migrated: ref migrated
            );

            DefenseGainMultiplier = Bind(
                config,
                SectionSkillGain,
                "DefenseGainMultiplier",
                1.0f,
                "Multiplier for Defense category skill XP gains (Blocking, Dodge; units: x multiplier, stacks with Global multiplier).",
                order: 60,
                new AcceptableValueRange<float>(0.01f, 10f),
                isAdminOnly: true,
                legacySection: LegacySectionSkillGain,
                orphaned: orphaned,
                migrated: ref migrated
            );

            MagicGainMultiplier = Bind(
                config,
                SectionSkillGain,
                "MagicGainMultiplier",
                1.0f,
                "Multiplier for Magic category skill XP gains (Elemental, Blood; units: x multiplier, stacks with Global multiplier).",
                order: 50,
                new AcceptableValueRange<float>(0.01f, 10f),
                isAdminOnly: true,
                legacySection: LegacySectionSkillGain,
                orphaned: orphaned,
                migrated: ref migrated
            );

            MobilityGainMultiplier = Bind(
                config,
                SectionSkillGain,
                "MobilityGainMultiplier",
                1.0f,
                "Multiplier for Mobility category skill XP gains (Run, Jump, Sneak, Swim, Ride; units: x multiplier, stacks with Global multiplier).",
                order: 40,
                new AcceptableValueRange<float>(0.01f, 10f),
                isAdminOnly: true,
                legacySection: LegacySectionSkillGain,
                orphaned: orphaned,
                migrated: ref migrated
            );

            ResourcesGainMultiplier = Bind(
                config,
                SectionSkillGain,
                "ResourcesGainMultiplier",
                1.0f,
                "Multiplier for Resources category skill XP gains (Woodcutting, Pickaxes, Farming, Crafting, Cooking, Fishing; units: x multiplier, stacks with Global multiplier).",
                order: 30,
                new AcceptableValueRange<float>(0.01f, 10f),
                isAdminOnly: true,
                legacySection: LegacySectionSkillGain,
                orphaned: orphaned,
                migrated: ref migrated
            );

            // 05 - Hearthside Attunement
            EnableHearthsideAttunement = Bind(
                config,
                SectionHearthside,
                "EnableHearthsideAttunement",
                true,
                "Master switch for Hearthside Attunement. When enabled, socketing or swapping Keystones requires meeting Rested and Comfort thresholds by a hearth. Already-socketed Keystones remain active everywhere even after Rested expires. Disable to allow swapping Keystones anywhere.",
                order: 100,
                isAdminOnly: true
            );

            RequireRestedStatus = Bind(
                config,
                SectionHearthside,
                "RequireRestedStatus",
                true,
                "When enabled, players must have an active Rested buff or be actively Resting near a warm fire and shelter to attune or swap Keystones.",
                order: 90,
                isAdminOnly: true
            );

            ComfortThresholdLv20 = Bind(
                config,
                SectionHearthside,
                "ComfortThresholdLv20",
                1,
                "Minimum Comfort required to attune or swap Level 20 Keystones (units: Comfort). Set to 0 to disable the Comfort requirement for this tier.",
                order: 70,
                new AcceptableValueRange<int>(0, 30),
                isAdminOnly: true
            );

            ComfortThresholdLv40 = Bind(
                config,
                SectionHearthside,
                "ComfortThresholdLv40",
                4,
                "Minimum Comfort required to attune or swap Level 40 Keystones (units: Comfort). Set to 0 to disable the Comfort requirement for this tier.",
                order: 60,
                new AcceptableValueRange<int>(0, 30),
                isAdminOnly: true
            );

            ComfortThresholdLv60 = Bind(
                config,
                SectionHearthside,
                "ComfortThresholdLv60",
                8,
                "Minimum Comfort required to attune or swap Level 60 Keystones (units: Comfort). Set to 0 to disable the Comfort requirement for this tier.",
                order: 50,
                new AcceptableValueRange<int>(0, 30),
                isAdminOnly: true
            );

            ComfortThresholdLv80 = Bind(
                config,
                SectionHearthside,
                "ComfortThresholdLv80",
                12,
                "Minimum Comfort required to attune or swap Level 80 Keystones (units: Comfort). Set to 0 to disable the Comfort requirement for this tier.",
                order: 40,
                new AcceptableValueRange<int>(0, 30),
                isAdminOnly: true
            );

            ComfortThresholdLv100 = Bind(
                config,
                SectionHearthside,
                "ComfortThresholdLv100",
                16,
                "Minimum Comfort required to attune or swap Level 100 Keystones (units: Comfort). Set to 0 to disable the Comfort requirement for this tier.",
                order: 30,
                new AcceptableValueRange<int>(0, 30),
                isAdminOnly: true
            );

            if (migrated)
            {
                config.Save();
            }
        }

        public static int GetRequiredComfort(int milestone)
        {
            return milestone switch
            {
                <= 20 => ComfortThresholdLv20.Value,
                <= 40 => ComfortThresholdLv40.Value,
                <= 60 => ComfortThresholdLv60.Value,
                <= 80 => ComfortThresholdLv80.Value,
                _ => ComfortThresholdLv100.Value
            };
        }

        public static Color GetCategoryColor(SkillCategoryType category)
        {
            string hex = category switch
            {
                SkillCategoryType.Weapons => WeaponsColorHex.Value,
                SkillCategoryType.Defense => DefenseColorHex.Value,
                SkillCategoryType.Magic => MagicColorHex.Value,
                SkillCategoryType.Mobility => MobilityColorHex.Value,
                SkillCategoryType.Resources => ResourcesColorHex.Value,
                _ => "#A8DADC"
            };

            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }

            return SkillCategory.GetCategoryColor(category);
        }

        private static ConfigEntry<T> Bind<T>(
            ConfigFile config,
            string section,
            string key,
            T defaultValue,
            string description,
            int order = 0,
            AcceptableValueBase? acceptableValues = null,
            bool isAdminOnly = false)
        {
            bool unused = false;
            return Bind(config, section, key, defaultValue, description, order, acceptableValues, isAdminOnly, null, null, ref unused);
        }

        private static ConfigEntry<T> Bind<T>(
            ConfigFile config,
            string section,
            string key,
            T defaultValue,
            string description,
            int order,
            bool isAdminOnly,
            string? legacySection,
            Dictionary<ConfigDefinition, string>? orphaned,
            ref bool migrated)
        {
            return Bind(config, section, key, defaultValue, description, order, null, isAdminOnly, legacySection, orphaned, ref migrated);
        }

        private static ConfigEntry<T> Bind<T>(
            ConfigFile config,
            string section,
            string key,
            T defaultValue,
            string description,
            int order,
            AcceptableValueBase? acceptableValues,
            bool isAdminOnly,
            string? legacySection,
            Dictionary<ConfigDefinition, string>? orphaned,
            ref bool migrated)
        {
            var newDef = new ConfigDefinition(section, key);
            bool hadExistingNewEntry = orphaned != null && orphaned.ContainsKey(newDef);

            var tags = new ConfigurationManagerAttributes
            {
                Order = order,
                Category = section,
                IsAdminOnly = isAdminOnly
            };
            var configDesc = new ConfigDescription(description, acceptableValues, tags);
            var entry = config.Bind(section, key, defaultValue, configDesc);

            if (!string.IsNullOrEmpty(legacySection) && orphaned != null)
            {
                var legacyDef = new ConfigDefinition(legacySection, key);
                if (orphaned.TryGetValue(legacyDef, out string legacyValue))
                {
                    if (!hadExistingNewEntry)
                    {
                        try
                        {
                            entry.SetSerializedValue(legacyValue);
                        }
                        catch (Exception ex)
                        {
                            VikingSkillsPlugin.Log?.LogWarning($"[ModConfig] Failed migrating [{legacySection}]/{key} -> [{section}]/{key}: {ex.Message}");
                        }
                    }

                    orphaned.Remove(legacyDef);
                    migrated = true;
                }
            }

            return entry;
        }
    }
}
