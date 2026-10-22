using BepInEx.Configuration;
using VikingSkills.Data;
using UnityEngine;

namespace VikingSkills.Configuration
{
    public static class ModConfig
    {
        public static ConfigEntry<bool> ModEnabled { get; private set; } = null!;
        public static ConfigEntry<bool> EnableVanillaSkillScaling { get; private set; } = null!;
        public static ConfigEntry<bool> EnableAudioEffects { get; private set; } = null!;
        public static ConfigEntry<string> WeaponsColorHex { get; private set; } = null!;
        public static ConfigEntry<string> DefenseColorHex { get; private set; } = null!;
        public static ConfigEntry<string> MagicColorHex { get; private set; } = null!;
        public static ConfigEntry<string> MobilityColorHex { get; private set; } = null!;
        public static ConfigEntry<string> ResourcesColorHex { get; private set; } = null!;

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
