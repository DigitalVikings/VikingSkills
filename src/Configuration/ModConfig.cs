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
        public static ConfigEntry<bool> ModEnabled { get; private set; } = null!;
        public static ConfigEntry<bool> EnableVanillaSkillScaling { get; private set; } = null!;
        public static ConfigEntry<bool> EnableAudioEffects { get; private set; } = null!;

        // Visual Theme
        public static ConfigEntry<string> WeaponsColorHex { get; private set; } = null!;
        public static ConfigEntry<string> DefenseColorHex { get; private set; } = null!;
        public static ConfigEntry<string> MagicColorHex { get; private set; } = null!;
        public static ConfigEntry<string> MobilityColorHex { get; private set; } = null!;
        public static ConfigEntry<string> ResourcesColorHex { get; private set; } = null!;

        // Progression: Skill Loss
        public static ConfigEntry<SkillLossMode> DeathSkillLossMode { get; private set; } = null!;
        public static ConfigEntry<float> DeathSkillLossPercent { get; private set; } = null!;
        public static ConfigEntry<float> DeathSkillLossFlatLevels { get; private set; } = null!;
        public static ConfigEntry<bool> ProtectUnlockedMilestones { get; private set; } = null!;

        // Progression: Skill Gain
        public static ConfigEntry<float> GlobalSkillGainMultiplier { get; private set; } = null!;
        public static ConfigEntry<float> WeaponsGainMultiplier { get; private set; } = null!;
        public static ConfigEntry<float> DefenseGainMultiplier { get; private set; } = null!;
        public static ConfigEntry<float> MagicGainMultiplier { get; private set; } = null!;
        public static ConfigEntry<float> MobilityGainMultiplier { get; private set; } = null!;
        public static ConfigEntry<float> ResourcesGainMultiplier { get; private set; } = null!;

        public static void Init(ConfigFile config)
        {
            ModEnabled = Bind(
                config,
                "1 - General",
                "Enabled",
                true,
                "Master switch to enable or disable all features of VikingSkills.",
                order: 100,
                isAdminOnly: false
            );

            EnableVanillaSkillScaling = Bind(
                config,
                "1 - General",
                "EnableVanillaSkillScaling",
                false,
                "When false (default), individual skill levels grant no bonuses; all power derives from socketed Keystones. Set to true to re-enable vanilla fractional scaling alongside Keystones.",
                order: 95,
                isAdminOnly: false
            );

            EnableAudioEffects = Bind(
                config,
                "1 - General",
                "EnableAudioEffects",
                true,
                "Play native Valheim UI sound effects when opening the window and hovering over rune columns.",
                order: 90,
                isAdminOnly: false
            );

            WeaponsColorHex = Bind(
                config,
                "2 - Visual Theme",
                "WeaponsColor",
                "#EB5757",
                "Hex color code for Weapons category columns and rune glow.",
                order: 80,
                isAdminOnly: false
            );

            DefenseColorHex = Bind(
                config,
                "2 - Visual Theme",
                "DefenseColor",
                "#4EA8DE",
                "Hex color code for Defense category columns and rune glow.",
                order: 75,
                isAdminOnly: false
            );

            MagicColorHex = Bind(
                config,
                "2 - Visual Theme",
                "MagicColor",
                "#9B5DE5",
                "Hex color code for Magic category columns and rune glow.",
                order: 70,
                isAdminOnly: false
            );

            MobilityColorHex = Bind(
                config,
                "2 - Visual Theme",
                "MobilityColor",
                "#F4A261",
                "Hex color code for Mobility category columns and rune glow.",
                order: 60,
                isAdminOnly: false
            );

            ResourcesColorHex = Bind(
                config,
                "2 - Visual Theme",
                "ResourcesColor",
                "#2EC4B6",
                "Hex color code for Resources & Crafting category columns and rune glow.",
                order: 50,
                isAdminOnly: false
            );

            // 3 - Progression: Skill Loss on Death
            DeathSkillLossMode = Bind(
                config,
                "3 - Progression: Skill Loss on Death",
                "DeathSkillLossMode",
                SkillLossMode.None,
                "Determines how skill loss is calculated upon death. Overrides vanilla world modifiers so values are exact.\n" +
                "Percentage: Loses a fraction of current level (Vanilla default is 5%).\n" +
                "FlatLevels: Loses a fixed number of levels regardless of current level.\n" +
                "None: Completely disables skill loss on death.",
                order: 40,
                isAdminOnly: true
            );

            DeathSkillLossPercent = Bind(
                config,
                "3 - Progression: Skill Loss on Death",
                "DeathSkillLossPercent",
                0.05f,
                "Fraction of current skill level lost on death when DeathSkillLossMode is set to Percentage. Vanilla default is 0.05 (5%).",
                order: 38,
                new AcceptableValueRange<float>(0f, 1f),
                isAdminOnly: true
            );

            DeathSkillLossFlatLevels = Bind(
                config,
                "3 - Progression: Skill Loss on Death",
                "DeathSkillLossFlatLevels",
                1.0f,
                "Number of skill levels lost on death when DeathSkillLossMode is set to FlatLevels.",
                order: 36,
                new AcceptableValueRange<float>(0f, 20f),
                isAdminOnly: true
            );

            ProtectUnlockedMilestones = Bind(
                config,
                "3 - Progression: Skill Loss on Death",
                "ProtectUnlockedMilestones",
                true,
                "When enabled, death penalty cannot reduce a skill below its highest unlocked milestone tier (20, 40, 60, 80). Prevents losing socketed Keystones upon death.",
                order: 34,
                isAdminOnly: true
            );

            // 4 - Progression: Skill Gain Multipliers
            GlobalSkillGainMultiplier = Bind(
                config,
                "4 - Progression: Skill Gain Multipliers",
                "GlobalSkillGainMultiplier",
                1.0f,
                "Global multiplier applied to all skill XP gains. Overrides world modifiers so value is exact.",
                order: 25,
                new AcceptableValueRange<float>(0.01f, 20f),
                isAdminOnly: true
            );

            WeaponsGainMultiplier = Bind(
                config,
                "4 - Progression: Skill Gain Multipliers",
                "WeaponsGainMultiplier",
                1.0f,
                "Multiplier for Weapons category skill XP gains (stacks with Global multiplier).",
                order: 24,
                new AcceptableValueRange<float>(0.01f, 10f),
                isAdminOnly: true
            );

            DefenseGainMultiplier = Bind(
                config,
                "4 - Progression: Skill Gain Multipliers",
                "DefenseGainMultiplier",
                1.0f,
                "Multiplier for Defense category skill XP gains (Blocking, Dodge). Stacks with Global multiplier.",
                order: 23,
                new AcceptableValueRange<float>(0.01f, 10f),
                isAdminOnly: true
            );

            MagicGainMultiplier = Bind(
                config,
                "4 - Progression: Skill Gain Multipliers",
                "MagicGainMultiplier",
                1.0f,
                "Multiplier for Magic category skill XP gains (Elemental, Blood). Stacks with Global multiplier.",
                order: 22,
                new AcceptableValueRange<float>(0.01f, 10f),
                isAdminOnly: true
            );

            MobilityGainMultiplier = Bind(
                config,
                "4 - Progression: Skill Gain Multipliers",
                "MobilityGainMultiplier",
                1.0f,
                "Multiplier for Mobility category skill XP gains (Run, Jump, Sneak, Swim, Ride). Stacks with Global multiplier.",
                order: 21,
                new AcceptableValueRange<float>(0.01f, 10f),
                isAdminOnly: true
            );

            ResourcesGainMultiplier = Bind(
                config,
                "4 - Progression: Skill Gain Multipliers",
                "ResourcesGainMultiplier",
                1.0f,
                "Multiplier for Resources category skill XP gains (Woodcutting, Pickaxes, Farming, Crafting, Cooking, Fishing). Stacks with Global multiplier.",
                order: 20,
                new AcceptableValueRange<float>(0.01f, 10f),
                isAdminOnly: true
            );
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
            var tags = new ConfigurationManagerAttributes
            {
                Order = order,
                Category = section,
                IsAdminOnly = isAdminOnly
            };
            var configDesc = new ConfigDescription(description, acceptableValues, tags);
            return config.Bind(section, key, defaultValue, configDesc);
        }
    }
}
