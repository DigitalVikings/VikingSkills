using System;
using System.Collections.Generic;
using UnityEngine;

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
            if (player == null || player.m_customData == null) return false;

            // Check if player meets the milestone requirement
            if (!IsMilestoneUnlocked(player, skillType, milestone))
            {
                return false;
            }

            var def = KeystoneRegistry.GetById(keystoneId);
            if (def == null || def.SkillType != skillType || def.MilestoneLevel != milestone)
            {
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
