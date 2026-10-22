using System.Collections.Generic;
using UnityEngine;

namespace VikingSkills.Data
{
    public static class SkillBonusCalculator
    {
        /// <summary>
        /// Calculates the concise, active accumulated passive bonuses from leveling up the skill (Minor Runes).
        /// Strictly formatted with short, single-line text to prevent awkward wrapping.
        /// </summary>
        public static List<string> GetActiveBonuses(Skills.SkillType skillType, float level)
        {
            var bonuses = new List<string>();
            float lvl = Mathf.Clamp(level, 0f, 100f);
            int autoTens = Mathf.FloorToInt(lvl / 10f);

            switch (skillType)
            {
                case Skills.SkillType.Axes:
                case Skills.SkillType.Clubs:
                case Skills.SkillType.Swords:
                case Skills.SkillType.Spears:
                case Skills.SkillType.Polearms:
                case Skills.SkillType.Knives:
                case Skills.SkillType.Unarmed:
                {
                    float dmg = (lvl * 0.50f) + (autoTens * 2.0f);
                    float stamEff = (lvl * 0.20f) + (autoTens * 2.0f);
                    bonuses.Add($"Damage: +{dmg:F1}%");
                    bonuses.Add($"Stamina Eff: +{stamEff:F1}%");
                    break;
                }

                case Skills.SkillType.Bows:
                {
                    float dmg = (lvl * 0.50f) + (autoTens * 2.0f);
                    float stamEff = (lvl * 0.20f) + (autoTens * 2.0f);
                    float drawSpeed = lvl * 0.30f;
                    bonuses.Add($"Damage: +{dmg:F1}%");
                    bonuses.Add($"Stamina Eff: +{stamEff:F1}%");
                    bonuses.Add($"Draw Speed: +{drawSpeed:F1}%");
                    break;
                }

                case Skills.SkillType.Crossbows:
                {
                    float dmg = (lvl * 0.50f) + (autoTens * 2.0f);
                    float reloadEff = (lvl * 0.20f) + (autoTens * 2.0f);
                    float reloadSpeed = lvl * 0.30f;
                    bonuses.Add($"Damage: +{dmg:F1}%");
                    bonuses.Add($"Reload Eff: +{reloadEff:F1}%");
                    bonuses.Add($"Reload Speed: +{reloadSpeed:F1}%");
                    break;
                }

                case Skills.SkillType.Blocking:
                {
                    float blockPower = (lvl * 0.20f) + (autoTens * 1.0f);
                    float blockEff = (lvl * 0.20f) + (autoTens * 1.0f);
                    bonuses.Add($"Block Power: +{blockPower:F1}%");
                    bonuses.Add($"Block Eff: +{blockEff:F1}%");
                    break;
                }

                case Skills.SkillType.Dodge:
                {
                    float dodgeEff = (lvl * 0.40f) + (autoTens * 2.0f);
                    bonuses.Add($"Dodge Eff: +{dodgeEff:F1}%");
                    break;
                }

                case Skills.SkillType.ElementalMagic:
                {
                    float dmg = (lvl * 0.50f) + (autoTens * 2.0f);
                    float eitrEff = (lvl * 0.20f) + (autoTens * 2.0f);
                    bonuses.Add($"Elemental Dmg: +{dmg:F1}%");
                    bonuses.Add($"Eitr Eff: +{eitrEff:F1}%");
                    break;
                }

                case Skills.SkillType.BloodMagic:
                {
                    float hpEff = (lvl * 0.20f) + (autoTens * 2.0f);
                    float eitrEff = (lvl * 0.20f) + (autoTens * 2.0f);
                    bonuses.Add($"Health Eff: +{hpEff:F1}%");
                    bonuses.Add($"Eitr Eff: +{eitrEff:F1}%");
                    break;
                }

                case Skills.SkillType.Run:
                {
                    float speed = lvl * 0.10f;
                    float sprintEff = (lvl * 0.40f) + (autoTens * 2.0f);
                    bonuses.Add($"Sprint Speed: +{speed:F1}%");
                    bonuses.Add($"Sprint Eff: +{sprintEff:F1}%");
                    break;
                }

                case Skills.SkillType.Jump:
                {
                    float jumpHeight = lvl * 0.003f;
                    float jumpEff = (lvl * 0.40f) + (autoTens * 2.0f);
                    bonuses.Add($"Jump Height: +{jumpHeight:F2}m");
                    bonuses.Add($"Jump Eff: +{jumpEff:F1}%");
                    break;
                }

                case Skills.SkillType.Sneak:
                {
                    float conc = lvl * 0.30f;
                    float sneakEff = (lvl * 0.40f) + (autoTens * 2.0f);
                    bonuses.Add($"Concealment: +{conc:F1}%");
                    bonuses.Add($"Sneak Eff: +{sneakEff:F1}%");
                    break;
                }

                case Skills.SkillType.Swim:
                {
                    float speed = lvl * 0.20f;
                    float swimEff = (lvl * 0.40f) + (autoTens * 2.0f);
                    bonuses.Add($"Swim Speed: +{speed:F1}%");
                    bonuses.Add($"Swim Eff: +{swimEff:F1}%");
                    break;
                }

                case Skills.SkillType.Ride:
                {
                    float speed = lvl * 0.10f;
                    float rideEff = (lvl * 0.40f) + (autoTens * 2.0f);
                    bonuses.Add($"Mount Speed: +{speed:F1}%");
                    bonuses.Add($"Mount Eff: +{rideEff:F1}%");
                    break;
                }

                case Skills.SkillType.WoodCutting:
                {
                    float chop = lvl * 0.30f;
                    float yield = lvl * 0.10f;
                    bonuses.Add($"Chop Dmg: +{chop:F1}%");
                    bonuses.Add($"Wood Yield: +{yield:F1}%");
                    break;
                }

                case Skills.SkillType.Pickaxes:
                {
                    float pick = lvl * 0.30f;
                    float yield = lvl * 0.10f;
                    bonuses.Add($"Pick Dmg: +{pick:F1}%");
                    bonuses.Add($"Mining Yield: +{yield:F1}%");
                    break;
                }

                case Skills.SkillType.Farming:
                {
                    float eff = lvl * 0.20f;
                    float yield = lvl * 0.10f;
                    bonuses.Add($"Tool Eff: +{eff:F1}%");
                    bonuses.Add($"Harvest Yield: +{yield:F1}%");
                    break;
                }

                case Skills.SkillType.Fishing:
                {
                    float eff = lvl * 0.40f;
                    bonuses.Add($"Reel Eff: +{eff:F1}%");
                    break;
                }

                case Skills.SkillType.Cooking:
                {
                    float yield = lvl * 0.10f;
                    float dur = lvl * 0.10f;
                    bonuses.Add($"Cook Yield: +{yield:F1}%");
                    bonuses.Add($"Food Duration: +{dur:F1}%");
                    break;
                }

                case Skills.SkillType.Crafting:
                {
                    float eff = lvl * 0.20f;
                    float yield = lvl * 0.10f;
                    bonuses.Add($"Tool Eff: +{eff:F1}%");
                    bonuses.Add($"Craft Yield: +{yield:F1}%");
                    break;
                }

                default:
                    bonuses.Add($"Mastery: +{lvl * 0.5f:F1}%");
                    break;
            }

            return bonuses;
        }

        /// <summary>
        /// Calculates the strictly incremental bonus unlocked at an individual level (1-100).
        /// Used for hover tooltips on the left-to-right progression timeline nodes.
        /// </summary>
        public static List<string> GetIncrementalBonuses(Skills.SkillType skillType, int level)
        {
            var gains = new List<string>();
            bool isDecade = level > 0 && level % 10 == 0;
            bool isSocket = level == 20 || level == 40 || level == 60 || level == 80 || level == 100;

            switch (skillType)
            {
                case Skills.SkillType.Axes:
                case Skills.SkillType.Clubs:
                case Skills.SkillType.Swords:
                case Skills.SkillType.Spears:
                case Skills.SkillType.Polearms:
                case Skills.SkillType.Knives:
                case Skills.SkillType.Unarmed:
                    gains.Add(isDecade ? "+2.5% Weapon Damage" : "+0.5% Weapon Damage");
                    gains.Add(isDecade ? "+2.2% Stamina Efficiency" : "+0.2% Stamina Efficiency");
                    break;

                case Skills.SkillType.Bows:
                    gains.Add(isDecade ? "+2.5% Bow Damage" : "+0.5% Bow Damage");
                    gains.Add(isDecade ? "+2.2% Stamina Efficiency" : "+0.2% Stamina Efficiency");
                    gains.Add("+0.3% Draw Speed");
                    break;

                case Skills.SkillType.Crossbows:
                    gains.Add(isDecade ? "+2.5% Crossbow Damage" : "+0.5% Crossbow Damage");
                    gains.Add(isDecade ? "+2.2% Reload Efficiency" : "+0.2% Reload Efficiency");
                    gains.Add("+0.3% Reload Speed");
                    break;

                case Skills.SkillType.Blocking:
                    gains.Add(isDecade ? "+1.2% Block Power" : "+0.2% Block Power");
                    gains.Add(isDecade ? "+1.2% Block Efficiency" : "+0.2% Block Efficiency");
                    break;

                case Skills.SkillType.Dodge:
                    gains.Add(isDecade ? "+2.4% Dodge Efficiency" : "+0.4% Dodge Efficiency");
                    break;

                case Skills.SkillType.ElementalMagic:
                    gains.Add(isDecade ? "+2.5% Elemental Damage" : "+0.5% Elemental Damage");
                    gains.Add(isDecade ? "+2.2% Eitr Efficiency" : "+0.2% Eitr Efficiency");
                    break;

                case Skills.SkillType.BloodMagic:
                    gains.Add(isDecade ? "+2.2% Health Efficiency" : "+0.2% Health Efficiency");
                    gains.Add(isDecade ? "+2.2% Eitr Efficiency" : "+0.2% Eitr Efficiency");
                    break;

                case Skills.SkillType.Run:
                    gains.Add("+0.1% Sprint Speed");
                    gains.Add(isDecade ? "+2.4% Sprint Efficiency" : "+0.4% Sprint Efficiency");
                    break;

                case Skills.SkillType.Jump:
                    gains.Add("+0.003 Jump Height");
                    gains.Add(isDecade ? "+2.4% Jump Efficiency" : "+0.4% Jump Efficiency");
                    break;

                case Skills.SkillType.Sneak:
                    gains.Add("+0.3% Concealment");
                    gains.Add(isDecade ? "+2.4% Sneak Efficiency" : "+0.4% Sneak Efficiency");
                    break;

                case Skills.SkillType.Swim:
                    gains.Add("+0.2% Swim Speed");
                    gains.Add(isDecade ? "+2.4% Swim Efficiency" : "+0.4% Swim Efficiency");
                    break;

                case Skills.SkillType.Ride:
                    gains.Add("+0.1% Riding Speed");
                    gains.Add(isDecade ? "+2.4% Riding Efficiency" : "+0.4% Riding Efficiency");
                    break;

                case Skills.SkillType.WoodCutting:
                    gains.Add("+0.3% Chop Damage");
                    gains.Add("+0.1% Woodcutting Yield");
                    break;

                case Skills.SkillType.Pickaxes:
                    gains.Add("+0.3% Pickaxe Damage");
                    gains.Add("+0.1% Mining Yield");
                    break;

                case Skills.SkillType.Farming:
                    gains.Add("+0.2% Tool Efficiency");
                    gains.Add("+0.1% Foraging Yield");
                    break;

                case Skills.SkillType.Fishing:
                    gains.Add("+0.4% Fishing Stamina Efficiency");
                    break;

                case Skills.SkillType.Cooking:
                    gains.Add("+0.1% Cooking Yield");
                    gains.Add("+0.1% Food Duration");
                    break;

                case Skills.SkillType.Crafting:
                    gains.Add("+0.2% Tool Efficiency");
                    gains.Add("+0.1% Crafting Yield");
                    break;

                default:
                    gains.Add("+0.5% Mastery");
                    break;
            }

            return gains;
        }
    }
}
