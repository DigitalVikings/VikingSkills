using System;
using System.Collections.Generic;
using UnityEngine;
using VikingLib.SkillSystem;
using VikingSkills.Configuration;
using VikingSkills.Data;
using VikingSkills.Keystones;

namespace VikingSkills.Services
{
    /// <summary>
    /// Manages custom skill progression curves, milestone threshold floor protections,
    /// authoritative skill gain multipliers, and configurable death penalties.
    /// </summary>
    public static class SkillProgressionManager
    {
        private static bool _initialized;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            // Intercept native skill gain and death penalty from VikingLib
            SkillEvents.OnRaiseSkill += HandleRaiseSkill;
            SkillEvents.OnDeathPenalty += HandleDeathPenalty;
        }

        private static void HandleRaiseSkill(Character character, Skills.SkillType skillType, SkillRaiseEventArgs args)
        {
            if (!ModConfig.ModEnabled.Value || skillType == Skills.SkillType.None) return;
            float mult = CalculateSkillGainMultiplier(skillType);
            args.Factor *= mult;
        }

        /// <summary>
        /// Calculates the effective skill gain multiplier for a given skill type,
        /// combining the global multiplier, category-specific multiplier,
        /// and normalizing against Game.m_skillGainRate to override world modifiers authoritatively.
        /// </summary>
        public static float CalculateSkillGainMultiplier(Skills.SkillType skillType)
        {
            float globalMult = ModConfig.GlobalSkillGainMultiplier?.Value ?? 1.0f;
            float catMult = GetCategoryMultiplier(skillType);
            float totalModMult = Mathf.Max(0.0001f, globalMult * catMult);

            // Neutralize Game.m_skillGainRate so VikingSkills configuration is exact and authoritative
            float worldRate = Game.m_skillGainRate;
            if (worldRate <= 0.0001f)
            {
                // If world has 0 skill gain set by a custom server or zero cheat, respect zero gain
                return 0f;
            }

            return totalModMult / worldRate;
        }

        private static float GetCategoryMultiplier(Skills.SkillType skillType)
        {
            var category = SkillCategory.GetCategory(skillType);
            return category switch
            {
                SkillCategoryType.Weapons => ModConfig.WeaponsGainMultiplier?.Value ?? 1.0f,
                SkillCategoryType.Defense => ModConfig.DefenseGainMultiplier?.Value ?? 1.0f,
                SkillCategoryType.Magic => ModConfig.MagicGainMultiplier?.Value ?? 1.0f,
                SkillCategoryType.Mobility => ModConfig.MobilityGainMultiplier?.Value ?? 1.0f,
                SkillCategoryType.Resources => ModConfig.ResourcesGainMultiplier?.Value ?? 1.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Intercepts native death penalty, applying custom loss mode and milestone floor clamping.
        /// </summary>
        private static void HandleDeathPenalty(Player player, SkillCancelEventArgs args)
        {
            // Cancel native Skills.LowerAllSkills execution
            args.Cancel = true;

            var skills = player?.GetSkills();
            if (skills == null || player == null) return;

            var lossMode = ModConfig.DeathSkillLossMode.Value;
            if (lossMode == SkillLossMode.None)
            {
                // Skill loss disabled; do not modify skill levels
                return;
            }

            float percentLoss = Mathf.Clamp01(ModConfig.DeathSkillLossPercent.Value);
            float flatLoss = Mathf.Max(0f, ModConfig.DeathSkillLossFlatLevels.Value);
            bool protectMilestones = ModConfig.ProtectUnlockedMilestones.Value;

            bool anySkillLowered = false;
            var skillList = skills.GetSkillList();

            foreach (var s in skillList)
            {
                if (s == null) continue;

                float currentLevel = s.m_level;
                if (currentLevel <= 0f) continue;

                float loss = lossMode switch
                {
                    SkillLossMode.Percentage => currentLevel * percentLoss,
                    SkillLossMode.FlatLevels => flatLoss,
                    _ => 0f
                };

                float targetLevel = currentLevel - loss;

                if (protectMilestones)
                {
                    float floor = GetMilestoneFloor(currentLevel);
                    targetLevel = Mathf.Max(floor, targetLevel);
                }

                targetLevel = Mathf.Clamp(targetLevel, 0f, 100f);

                if (targetLevel < currentLevel || s.m_accumulator > 0f)
                {
                    s.m_level = targetLevel;
                    s.m_accumulator = 0f;
                    anySkillLowered = true;
                }
            }

            if (anySkillLowered)
            {
                player.Message(MessageHud.MessageType.TopLeft, "$msg_skills_lowered");
            }

            // Sync equipment and keystone passives
            VikingLib.Stats.VanillaEquipmentAdapter.SyncEquipment(player);
            KeystoneStatAdapter.SyncPlayer(player);
        }

        /// <summary>
        /// Finds the highest milestone floor (20, 40, 60, 80, 100) that the skill has already achieved.
        /// </summary>
        public static float GetMilestoneFloor(float currentLevel)
        {
            float floor = 0f;
            foreach (int m in KeystoneRegistry.Milestones)
            {
                if (currentLevel >= m)
                {
                    floor = m;
                }
                else
                {
                    break;
                }
            }
            return floor;
        }
    }
}
