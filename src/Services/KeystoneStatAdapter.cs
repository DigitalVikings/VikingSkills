using System;
using System.Collections.Generic;
using UnityEngine;
using VikingLib.Combat;
using VikingLib.SkillSystem;
using VikingLib.Stats;
using VikingLib.StatusEffects;
using VikingLib.UI;
using VikingLib.UI.Tooltip;
using VikingSkills.Configuration;
using VikingSkills.Keystones;
using VikingSkills.StatusEffects;

namespace VikingSkills.Services
{
    public static class KeystoneStatAdapter
    {
        public const string OwnerKey = "VikingSkills";
        private static bool _initialized = false;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            // Initialize custom status effects
            VikingSkillsStatusEffects.Init();

            // 1. Listen for keystone socket changes
            KeystoneManager.OnKeystonesChanged += () =>
            {
                if (Player.m_localPlayer != null)
                {
                    SyncPlayer(Player.m_localPlayer);
                }
            };

            // 2. Listen for skill level changes to update minor runes and milestone availability
            SkillEvents.OnSkillLevelChanged += (player, skill, level) =>
            {
                if (player != null && player == Player.m_localPlayer)
                {
                    SyncPlayer(player);
                }
            };

            // 3. Combat Procs: OnKill
            DamagePipeline.OnKill += (attacker, victim, ctx) =>
            {
                if (attacker is Player player && player == Player.m_localPlayer)
                {
                    if (KeystoneManager.HasKeystone(player, "knife_100_flurry"))
                    {
                        StatusEffectRegistry.Apply(player, VikingSkillsStatusEffects.FlurryId);
                    }
                    if (KeystoneManager.HasKeystone(player, "sneak_100_shadow"))
                    {
                        if (ctx.Flags.HasFlag(DamageFlags.Backstab))
                        {
                            StatusEffectRegistry.Apply(player, VikingSkillsStatusEffects.ShadowveilId);
                        }
                    }
                }
            };

            // 4. Combat Procs: OnParry
            DamagePipeline.OnParry += (defender, attacker, hit) =>
            {
                if (defender is Player player && player == Player.m_localPlayer)
                {
                    if (KeystoneManager.HasKeystone(player, "block_40_riposte"))
                    {
                        StatusEffectRegistry.Apply(player, CanonicalStatusEffects.EmpoweredStrikeId);
                    }
                }
            };

            // 5. Combat Procs: OnDodge
            DamagePipeline.OnDodge += (defender, attacker, hit) =>
            {
                if (defender is Player player && player == Player.m_localPlayer)
                {
                    if (KeystoneManager.HasKeystone(player, "dodge_20_counter"))
                    {
                        if (ProcCooldownManager.TryTrigger(player, "VS_DodgeCounter", 3.0f))
                        {
                            StatusEffectRegistry.Apply(player, CanonicalStatusEffects.EmpoweredStrikeId);
                        }
                    }
                    if (KeystoneManager.HasKeystone(player, "dodge_40_wind"))
                    {
                        StatusEffectRegistry.Apply(player, VikingSkillsStatusEffects.WindrunnerId);
                    }
                    if (KeystoneManager.HasKeystone(player, "dodge_100_stone"))
                    {
                        StatusEffectRegistry.Apply(player, VikingSkillsStatusEffects.StoneGuardId);
                    }
                }
            };

            // 6. Combat Procs: OnCriticalHit
            DamagePipeline.OnCriticalHit += (attacker, target, ctx) =>
            {
                if (attacker is Player player && player == Player.m_localPlayer)
                {
                    if (KeystoneManager.HasKeystone(player, "sword_40_bleed"))
                    {
                        DamagePipeline.ApplyOnHitStatus(attacker, target, CanonicalStatusEffects.BleedId, 25.0f, 10.0f);
                    }
                    if (KeystoneManager.HasKeystone(player, "crossbow_40_bleed"))
                    {
                        DamagePipeline.ApplyOnHitStatus(attacker, target, CanonicalStatusEffects.BleedId, 20.0f, 8.0f);
                    }
                }
            };

            // 7. Combat Procs: OnCauseStagger
            DamagePipeline.OnCauseStagger += (attacker, target, ctx) =>
            {
                if (attacker is Player player && player == Player.m_localPlayer)
                {
                    if (KeystoneManager.HasKeystone(player, "club_40_sunder"))
                    {
                        DamagePipeline.ApplyOnHitStatus(attacker, target, CanonicalStatusEffects.VulnerableId, 0.10f, 6.0f);
                    }
                }
            };

            // 8. Combat Procs: OnHitTaken
            DamagePipeline.OnHitTaken += (ctx) =>
            {
                if (ctx.Target is Player player && player == Player.m_localPlayer)
                {
                    if (KeystoneManager.HasKeystone(player, "unarmed_100_adren"))
                    {
                        if (ProcCooldownManager.TryTrigger(player, "VS_AdrenalineSurge", 15.0f))
                        {
                            StatusEffectRegistry.Apply(player, VikingSkillsStatusEffects.AdrenalineId);
                        }
                    }
                    if (KeystoneManager.HasKeystone(player, "block_60_fort"))
                    {
                        if (ProcCooldownManager.TryTrigger(player, "VS_FortressGuard", 10.0f))
                        {
                            StatusEffectRegistry.Apply(player, CanonicalStatusEffects.FortifiedId);
                        }
                    }
                }
            };

            // 9. Weapon Tooltip Keystone Traits
            TooltipEngine.RegisterLineProvider((item, player) =>
            {
                if (item?.m_shared == null || player == null) return null;

                var skillType = item.m_shared.m_skillType;
                var activeKeystones = KeystoneManager.GetActiveKeystones(player, skillType);
                if (activeKeystones.Count == 0) return null;

                // Only show keystones that do something else than modify stats already visible on the UI
                var specialKeystones = activeKeystones.FindAll(k => k.IsSpecialMechanic);
                if (specialKeystones.Count == 0) return null;

                var lines = new List<string>(specialKeystones.Count);
                foreach (var k in specialKeystones)
                {
                    lines.Add($"<color=#FFD700>{k.Name}</color>: <color=#CCCCCC>{k.Description}</color>");
                }
                return lines;
            }, priority: 10);

            // 10. Hugin's Compendium Category
            CharacterStatsGlossaryPanel.RegisterCustomCategory("Socketed Runic Keystones", player =>
            {
                var lines = new List<string>();
                bool firstSkill = true;
                foreach (Skills.SkillType skill in Enum.GetValues(typeof(Skills.SkillType)))
                {
                    var keystones = KeystoneManager.GetActiveKeystones(player, skill);
                    if (keystones.Count == 0) continue;

                    string headerPrefix = firstSkill ? "" : "\n";
                    lines.Add($"{headerPrefix}• <color=#E0E0E0><b>{GetSkillDisplayName(skill)}</b>:</color>");
                    firstSkill = false;

                    foreach (var k in keystones)
                    {
                        string desc = string.IsNullOrWhiteSpace(k.Description)
                            ? string.Empty
                            : $": <color=#CCCCCC>{k.Description}</color>";
                        lines.Add($"    <color=#FFD700>{k.Name}</color> <color=#888888>(Lv {k.MilestoneLevel})</color>{desc}");
                    }
                }
                return lines;
            }, priority: 25);
        }

        public static string GetSkillDisplayName(Skills.SkillType skill)
        {
            return skill switch
            {
                Skills.SkillType.ElementalMagic => "Elemental Magic",
                Skills.SkillType.BloodMagic => "Blood Magic",
                Skills.SkillType.WoodCutting => "Woodcutting",
                _ => skill.ToString()
            };
        }

        public static void SyncPlayer(Player player)
        {
            if (player == null || !ModConfig.ModEnabled.Value) return;

            var finalModifiers = new List<StatModifier>(64);
            var skills = player.GetSkills();
            if (skills == null) return;

            var skillList = skills.GetSkillList();
            if (skillList == null) return;

            foreach (Skills.SkillType skill in Enum.GetValues(typeof(Skills.SkillType)))
            {
                if (skill is Skills.SkillType.None or Skills.SkillType.All) continue;
                if (skillList.Find(sd => sd.m_info != null && sd.m_info.m_skill == skill) == null) continue;

                float level = skills.GetSkillLevel(skill);
                var activeKeystones = KeystoneManager.GetActiveKeystones(player, skill);

                if (level <= 0.001f && activeKeystones.Count == 0) continue;

                var rawModifiers = new List<StatModifier>();

                // 1. Evaluate Minor Runes (1-100) and Automatic 10s
                if (level > 0.001f)
                {
                    int autoTenTicks = Mathf.FloorToInt(level / 10f);
                    CollectMinorRunes(skill, level, autoTenTicks, rawModifiers);
                }

                // 2. Evaluate Active Sockets (20, 40, 60, 80, 100)
                foreach (var keystone in activeKeystones)
                {
                    if (keystone.Modifiers != null)
                    {
                        var mods = keystone.Modifiers(player, OwnerKey);
                        if (mods != null)
                        {
                            rawModifiers.AddRange(mods);
                        }
                    }
                }

                // 3. Accumulate benefits by (StatType, Tier) under expressive skill source (e.g. "Axes - VikingSkills")
                string sourceName = $"{GetSkillDisplayName(skill)} - VikingSkills";
                AccumulateAndAddModifiers(rawModifiers, sourceName, finalModifiers);
            }

            // 4. Atomically update CharacterStatManager
            CharacterStatManager.ReplaceModifiersByOwner(player, OwnerKey, finalModifiers);
        }

        private static void AccumulateAndAddModifiers(
            List<StatModifier> rawModifiers,
            string sourceName,
            List<StatModifier> outputList)
        {
            if (rawModifiers.Count == 0) return;

            var grouped = new Dictionary<(CharacterStatType stat, ModifierTier tier), float>();

            foreach (var mod in rawModifiers)
            {
                var key = (mod.StatType, mod.Tier);
                if (!grouped.TryGetValue(key, out float currentVal))
                {
                    currentVal = (mod.Tier == ModifierTier.Multiplier) ? 1.0f : 0.0f;
                }

                if (mod.Tier == ModifierTier.Multiplier)
                {
                    currentVal *= mod.Value;
                }
                else
                {
                    currentVal += mod.Value;
                }

                grouped[key] = currentVal;
            }

            foreach (var kvp in grouped)
            {
                var (stat, tier) = kvp.Key;
                float val = kvp.Value;

                if (tier == ModifierTier.Multiplier && Mathf.Abs(val - 1.0f) <= 0.0001f) continue;
                if (tier != ModifierTier.Multiplier && Mathf.Abs(val) <= 0.0001f) continue;

                StatModifier combined = tier switch
                {
                    ModifierTier.Flat => new StatModifier(stat, ModifierTier.Flat, val, sourceName, null, OwnerKey),
                    ModifierTier.IncreasedPercent => StatModifier.Increased(stat, val, sourceName, null, OwnerKey),
                    ModifierTier.Multiplier => StatModifier.Multiplier(stat, val, sourceName, null, OwnerKey),
                    _ => StatModifier.Increased(stat, val, sourceName, null, OwnerKey)
                };

                outputList.Add(combined);
            }
        }

        private static void CollectMinorRunes(Skills.SkillType skill, float level, int autoTenTicks, List<StatModifier> modifiers)
        {
            switch (skill)
            {
                // --- Weapons: +0.50% Dmg, +0.20% Stamina Efficiency per level ---
                case Skills.SkillType.Swords:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.DamageSwords, (level * 0.0050f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.SwordsEfficiency, (level * 0.0020f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Axes:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.DamageAxes, (level * 0.0050f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.AxesEfficiency, (level * 0.0020f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Clubs:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.DamageClubs, (level * 0.0050f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.ClubsEfficiency, (level * 0.0020f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Polearms:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.DamagePolearms, (level * 0.0050f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.PolearmsEfficiency, (level * 0.0020f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Spears:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.DamageSpears, (level * 0.0050f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.SpearsEfficiency, (level * 0.0020f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Knives:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.DamageKnives, (level * 0.0050f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.KnivesEfficiency, (level * 0.0020f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Unarmed:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.DamageUnarmed, (level * 0.0050f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.UnarmedEfficiency, (level * 0.0020f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Bows:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.DamageBows, (level * 0.0050f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.BowsEfficiency, (level * 0.0020f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.BowDrawSpeed, level * 0.0030f, "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Crossbows:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.DamageCrossbows, (level * 0.0050f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.CrossbowReloadStaminaEfficiency, (level * 0.0020f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.CrossbowReloadSpeed, level * 0.0030f, "Minor Runes", null, OwnerKey));
                    break;

                // --- Defense ---
                case Skills.SkillType.Blocking:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.BlockPowerMultiplier, (level * 0.0020f) + (autoTenTicks * 0.01f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.BlockStaminaEfficiency, (level * 0.0020f) + (autoTenTicks * 0.01f), "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Dodge:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.DodgeEfficiency, (level * 0.0040f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                // --- Magic ---
                case Skills.SkillType.ElementalMagic:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.DamageElementalMagic, (level * 0.0050f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.ElementalMagicEitrEfficiency, (level * 0.0020f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.BloodMagic:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.BloodMagicHealthEfficiency, (level * 0.0020f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.BloodMagicEitrEfficiency, (level * 0.0020f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                // --- Mobility ---
                case Skills.SkillType.Run:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.RunSpeed, level * 0.0010f, "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.SprintEfficiency, (level * 0.0040f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Jump:
                    modifiers.Add(new StatModifier(CharacterStatType.JumpHeight, ModifierTier.Flat, level * 0.0030f, "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.JumpEfficiency, (level * 0.0040f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Sneak:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.Concealment, level * 0.0030f, "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.SneakEfficiency, (level * 0.0040f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Swim:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.SwimSpeed, level * 0.0020f, "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.SwimEfficiency, (level * 0.0040f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Ride:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.RidingSpeed, level * 0.0010f, "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.RidingStaminaEfficiency, (level * 0.0040f) + (autoTenTicks * 0.02f), "Minor Runes", null, OwnerKey));
                    break;

                // --- Resources ---
                case Skills.SkillType.WoodCutting:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.DamageChop, level * 0.0030f, "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.WoodcuttingYield, level * 0.0010f, "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Pickaxes:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.DamagePickaxe, level * 0.0030f, "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.MiningYield, level * 0.0010f, "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Farming:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.ToolStaminaEfficiency, level * 0.0020f, "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.ForagingYield, level * 0.0010f, "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Fishing:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.FishingStaminaEfficiency, level * 0.0040f, "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Cooking:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.CookingYield, level * 0.0010f, "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.FoodDurationMultiplier, level * 0.0010f, "Minor Runes", null, OwnerKey));
                    break;

                case Skills.SkillType.Crafting:
                    modifiers.Add(StatModifier.Increased(CharacterStatType.ToolStaminaEfficiency, level * 0.0020f, "Minor Runes", null, OwnerKey));
                    modifiers.Add(StatModifier.Increased(CharacterStatType.CraftingYield, level * 0.0010f, "Minor Runes", null, OwnerKey));
                    break;
            }
        }
    }
}
