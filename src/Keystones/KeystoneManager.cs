using System;
using System.Collections.Generic;
using UnityEngine;
using VikingSkills.Configuration;

namespace VikingSkills.Keystones
{
    public static class KeystoneManager
    {
        public static event Action? OnKeystonesChanged;

        private const string CustomDataPrefix = "KS_";

        public static string GetKey(Skills.SkillType skillType, int milestone)
        {
            return $"{CustomDataPrefix}{(int)skillType}_{milestone}";
        }

        public static bool IsMilestoneUnlocked(Player player, Skills.SkillType skillType, int milestone)
        {
            if (player == null || player.GetSkills() == null) return false;
            return player.GetSkills().GetSkillLevel(skillType) >= milestone;
        }

        public static bool HasRestedBuff(Player player)
        {
            var seMan = player != null ? player.GetSEMan() : null;
            return seMan != null && seMan.HaveStatusEffect(SEMan.s_statusEffectRested);
        }

        public static bool IsActivelyResting(Player player)
        {
            var seMan = player != null ? player.GetSEMan() : null;
            return seMan != null && seMan.HaveStatusEffect(SEMan.s_statusEffectResting);
        }

        public static bool IsPlayerRestedOrResting(Player player)
        {
            var seMan = player != null ? player.GetSEMan() : null;
            if (seMan == null) return false;
            return seMan.HaveStatusEffect(SEMan.s_statusEffectRested)
                || seMan.HaveStatusEffect(SEMan.s_statusEffectResting);
        }

        public static int GetEffectiveComfort(Player player)
        {
            if (player == null) return 0;
            return Mathf.Max(0, SE_Rested.CalculateComfortLevel(player));
        }

        public static bool CanAttuneMilestone(Player player, int milestone, out string reason)
        {
            if (player == null)
            {
                reason = "No active character.";
                return false;
            }

            if (ModConfig.EnableHearthsideAttunement == null || !ModConfig.EnableHearthsideAttunement.Value)
            {
                reason = string.Empty;
                return true;
            }

            bool requireRested = ModConfig.RequireRestedStatus != null && ModConfig.RequireRestedStatus.Value;
            bool restedOrResting = IsPlayerRestedOrResting(player);
            int requiredComfort = Mathf.Max(0, ModConfig.GetRequiredComfort(milestone));
            int currentComfort = GetEffectiveComfort(player);

            bool missingRested = requireRested && !restedOrResting;
            bool missingComfort = requiredComfort > 0 && currentComfort < requiredComfort;

            if (missingRested && missingComfort)
            {
                reason = $"Requires Rested & Comfort {requiredComfort}+ by a hearth (current: Comfort {currentComfort}).";
                return false;
            }

            if (missingRested)
            {
                reason = requiredComfort > 0
                    ? $"Requires Rested (Comfort {requiredComfort}+) by a hearth to attune."
                    : "Requires Rested or Resting by a hearth to attune.";
                return false;
            }

            if (missingComfort)
            {
                reason = $"Requires Comfort {requiredComfort}+ by a hearth to attune (current: Comfort {currentComfort}).";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public static KeystoneDefinition? GetSelectedKeystone(Player player, Skills.SkillType skillType, int milestone)
        {
            if (player == null || player.m_customData == null) return null;

            string key = GetKey(skillType, milestone);
            if (player.m_customData.TryGetValue(key, out string id) && !string.IsNullOrEmpty(id))
            {
                return KeystoneRegistry.GetById(id);
            }

            return null;
        }

        public static bool HasKeystone(Player player, string keystoneId)
        {
            if (player == null || player.m_customData == null) return false;
            foreach (var pair in player.m_customData)
            {
                if (pair.Key.StartsWith(CustomDataPrefix) && pair.Value == keystoneId)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasKeystone(Player player, Skills.SkillType skillType, int milestone, string keystoneId)
        {
            if (player == null || player.m_customData == null) return false;
            string key = GetKey(skillType, milestone);
            return player.m_customData.TryGetValue(key, out string currentId) && currentId == keystoneId;
        }

        public static bool SelectKeystone(Player player, Skills.SkillType skillType, int milestone, string keystoneId)
        {
            return SelectKeystone(player, skillType, milestone, keystoneId, out _);
        }

        public static bool SelectKeystone(Player player, Skills.SkillType skillType, int milestone, string keystoneId, out string failureReason)
        {
            if (player == null || player.m_customData == null)
            {
                failureReason = "No active character.";
                return false;
            }

            // Check if player meets the milestone requirement
            if (!IsMilestoneUnlocked(player, skillType, milestone))
            {
                failureReason = $"Requires Skill Level {milestone}.";
                return false;
            }

            // Check Hearthside Rested & Comfort attunement gate
            if (!CanAttuneMilestone(player, milestone, out failureReason))
            {
                return false;
            }

            var def = KeystoneRegistry.GetById(keystoneId);
            if (def == null || def.SkillType != skillType || def.MilestoneLevel != milestone)
            {
                failureReason = "Invalid keystone selection.";
                return false;
            }

            string key = GetKey(skillType, milestone);

            // Toggle off if already selected
            if (player.m_customData.TryGetValue(key, out string currentId) && currentId == keystoneId)
            {
                player.m_customData.Remove(key);
            }
            else
            {
                player.m_customData[key] = keystoneId;
            }

            failureReason = string.Empty;
            OnKeystonesChanged?.Invoke();
            return true;
        }

        public static List<KeystoneDefinition> GetActiveKeystones(Player player, Skills.SkillType skillType)
        {
            var list = new List<KeystoneDefinition>();
            if (player == null || player.m_customData == null) return list;

            foreach (int m in KeystoneRegistry.Milestones)
            {
                if (IsMilestoneUnlocked(player, skillType, m))
                {
                    var k = GetSelectedKeystone(player, skillType, m);
                    if (k != null)
                    {
                        list.Add(k);
                    }
                }
            }

            return list;
        }
    }
}
