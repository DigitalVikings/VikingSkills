using UnityEngine;

namespace VikingSkills.Data
{
    public enum SkillCategoryType
    {
        Weapons = 0,
        Defense = 1,
        Magic = 2,
        Mobility = 3,
        Resources = 4,
        Custom = 5
    }

    public static class SkillCategory
    {
        public static SkillCategoryType GetCategory(Skills.SkillType skillType)
        {
            switch (skillType)
            {
                case Skills.SkillType.Axes:
                case Skills.SkillType.Clubs:
                case Skills.SkillType.Swords:
                case Skills.SkillType.Spears:
                case Skills.SkillType.Polearms:
                case Skills.SkillType.Knives:
                case Skills.SkillType.Unarmed:
                case Skills.SkillType.Bows:
                case Skills.SkillType.Crossbows:
                    return SkillCategoryType.Weapons;

                case Skills.SkillType.Blocking:
                case Skills.SkillType.Dodge:
                    return SkillCategoryType.Defense;

                case Skills.SkillType.ElementalMagic:
                case Skills.SkillType.BloodMagic:
                    return SkillCategoryType.Magic;

                case Skills.SkillType.Run:
                case Skills.SkillType.Jump:
                case Skills.SkillType.Sneak:
                case Skills.SkillType.Swim:
                case Skills.SkillType.Ride:
                    return SkillCategoryType.Mobility;

                case Skills.SkillType.WoodCutting:
                case Skills.SkillType.Pickaxes:
                case Skills.SkillType.Farming:
                case Skills.SkillType.Crafting:
                case Skills.SkillType.Cooking:
                case Skills.SkillType.Fishing:
                    return SkillCategoryType.Resources;

                default:
                    return SkillCategoryType.Custom;
            }
        }

        public static int GetSkillOrder(Skills.SkillType skillType)
        {
            return skillType switch
            {
                // Weapons: Axe, Club, Sword, Spear, Polearm, Fist, Bow, Crossbow
                Skills.SkillType.Axes => 0,
                Skills.SkillType.Clubs => 1,
                Skills.SkillType.Swords => 2,
                Skills.SkillType.Spears => 3,
                Skills.SkillType.Polearms => 4,
                Skills.SkillType.Knives => 5,
                Skills.SkillType.Unarmed => 6,
                Skills.SkillType.Bows => 7,
                Skills.SkillType.Crossbows => 8,

                // Defense: Blocking, Dodging
                Skills.SkillType.Blocking => 0,
                Skills.SkillType.Dodge => 1,

                // Magic: Elemental, Blood
                Skills.SkillType.ElementalMagic => 0,
                Skills.SkillType.BloodMagic => 1,

                // Mobility: Run, Jump, Sneak, Swim, Ride
                Skills.SkillType.Run => 0,
                Skills.SkillType.Jump => 1,
                Skills.SkillType.Sneak => 2,
                Skills.SkillType.Swim => 3,
                Skills.SkillType.Ride => 4,

                // Resources: Woodcutting, Pickaxes, Farming, Crafting, Cooking, Fishing
                Skills.SkillType.WoodCutting => 0,
                Skills.SkillType.Pickaxes => 1,
                Skills.SkillType.Farming => 2,
                Skills.SkillType.Crafting => 3,
                Skills.SkillType.Cooking => 4,
                Skills.SkillType.Fishing => 5,

                _ => 99
            };
        }

        public static string GetCategoryDisplayName(SkillCategoryType category)
        {
            return category switch
            {
                SkillCategoryType.Weapons => "Weapons",
                SkillCategoryType.Defense => "Defense",
                SkillCategoryType.Magic => "Magic",
                SkillCategoryType.Mobility => "Mobility",
                SkillCategoryType.Resources => "Resources",
                _ => "Other"
            };
        }

        public static Color GetCategoryColor(SkillCategoryType category)
        {
            return category switch
            {
                SkillCategoryType.Weapons => new Color(0.92f, 0.35f, 0.30f, 1f),   // Crimson Red
                SkillCategoryType.Defense => new Color(0.31f, 0.66f, 0.87f, 1f),   // Steel Azure
                SkillCategoryType.Magic => new Color(0.61f, 0.36f, 0.90f, 1f),     // Mystic Purple
                SkillCategoryType.Mobility => new Color(0.95f, 0.65f, 0.25f, 1f),  // Sun Amber
                SkillCategoryType.Resources => new Color(0.18f, 0.77f, 0.71f, 1f), // Forest Jade
                _ => new Color(0.70f, 0.75f, 0.80f, 1f)                           // Pale Rune Stone
            };
        }
    }
}
