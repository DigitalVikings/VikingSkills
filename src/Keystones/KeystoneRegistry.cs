using System;
using System.Collections.Generic;
using VikingLib.Combat;
using VikingLib.Stats;

namespace VikingSkills.Keystones
{
    public static class KeystoneRegistry
    {
        public static readonly int[] Milestones = { 20, 40, 60, 80, 100 };

        private static readonly Dictionary<string, KeystoneDefinition> _byId = new();
        private static readonly Dictionary<Skills.SkillType, Dictionary<int, List<KeystoneDefinition>>> _bySkill = new();

        static KeystoneRegistry()
        {
            RegisterAll();
        }

        public static IReadOnlyList<KeystoneDefinition> GetKeystonesForMilestone(Skills.SkillType skillType, int milestone)
        {
            if (_bySkill.TryGetValue(skillType, out var milestones) && milestones.TryGetValue(milestone, out var list))
            {
                return list;
            }

            // Fallback for custom / modded skills
            return GenerateFallbackKeystones(skillType, milestone);
        }

        public static KeystoneDefinition? GetById(string id)
        {
            _byId.TryGetValue(id, out var keystone);
            return keystone;
        }

        private static void Register(KeystoneDefinition def)
        {
            _byId[def.Id] = def;

            if (!_bySkill.TryGetValue(def.SkillType, out var milestones))
            {
                milestones = new Dictionary<int, List<KeystoneDefinition>>();
                _bySkill[def.SkillType] = milestones;
            }

            if (!milestones.TryGetValue(def.MilestoneLevel, out var list))
            {
                list = new List<KeystoneDefinition>();
                milestones[def.MilestoneLevel] = list;
            }

            list.Add(def);
        }

        private static void RegisterAll()
        {
            RegisterWeapons();
            RegisterDefense();
            RegisterMagic();
            RegisterMobility();
            RegisterResources();
        }

        private static void RegisterWeapons()
        {
            // 1. Swords
            Register(KeystoneDefinition.Stat("sword_20_eff", Skills.SkillType.Swords, 20, 0, "Fluid Form", "Reduces sword attack stamina drain by 10%.", "ᚠ", CharacterStatType.SwordsEfficiency, 0.10f));
            Register(KeystoneDefinition.Stat("sword_20_melee", Skills.SkillType.Swords, 20, 1, "Honored Blade", "Increases all melee weapon damage by 4%.", "ᚢ", CharacterStatType.DamageMelee, 0.04f));

            Register(KeystoneDefinition.Stat("sword_40_speed", Skills.SkillType.Swords, 40, 0, "Blade Tempo", "Increases sword attack speed by 8%.", "ᚦ", CharacterStatType.AttackSpeed, 0.08f));
            Register(new KeystoneDefinition("sword_40_bleed", Skills.SkillType.Swords, 40, 1, "Severing Gash", "Critical hits with swords inflict bleeding wounds over 10s.", "ᚨ"));

            Register(KeystoneDefinition.Stat("sword_60_dmg", Skills.SkillType.Swords, 60, 0, "Swordmaster", "Increases sword damage by 10%.", "ᚱ", CharacterStatType.DamageSwords, 0.10f));
            Register(KeystoneDefinition.Stat("sword_60_finisher", Skills.SkillType.Swords, 60, 1, "Flurry Finisher", "Increases 3rd combo attack damage by 12%.", "ᚲ", CharacterStatType.DamageComboFinisher, 0.12f));

            Register(KeystoneDefinition.MultiStat("sword_80_master", Skills.SkillType.Swords, 80, 0, "Dancing Steel", "Increases sword damage by 8% and stamina efficiency by 6%.", "ᚷ",
                (CharacterStatType.DamageSwords, 0.08f, ModifierTier.IncreasedPercent),
                (CharacterStatType.SwordsEfficiency, 0.06f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.MultiStat("sword_80_crit", Skills.SkillType.Swords, 80, 1, "Keen Slicer", "Increases critical strike chance by 6% and critical damage by 10%.", "ᚹ",
                (CharacterStatType.CriticalStrikeChance, 0.06f, ModifierTier.IncreasedPercent),
                (CharacterStatType.CriticalStrikeMultiplier, 0.10f, ModifierTier.IncreasedPercent)));

            Register(KeystoneDefinition.MultiStat("sword_100_apex", Skills.SkillType.Swords, 100, 0, "Tyr's Vanguard", "Increases sword damage by 10% and attack speed by 8%.", "ᚺ",
                (CharacterStatType.DamageSwords, 0.10f, ModifierTier.IncreasedPercent),
                (CharacterStatType.AttackSpeed, 0.08f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.MultiStat("sword_100_lethal", Skills.SkillType.Swords, 100, 1, "Lethal Precision", "Increases critical strike chance by 8% and critical damage by 15%.", "ᚾ",
                (CharacterStatType.CriticalStrikeChance, 0.08f, ModifierTier.IncreasedPercent),
                (CharacterStatType.CriticalStrikeMultiplier, 0.15f, ModifierTier.IncreasedPercent)));

            // 2. Axes
            Register(KeystoneDefinition.Stat("axe_20_eff", Skills.SkillType.Axes, 20, 0, "Woodman's Grip", "Reduces axe stamina drain by 10%.", "ᚠ", CharacterStatType.AxesEfficiency, 0.10f));
            Register(KeystoneDefinition.Stat("axe_20_cleave", Skills.SkillType.Axes, 20, 1, "Wide Edge", "Reduces multi-target cleave damage penalty by 6%.", "ᚢ", CharacterStatType.CleaveEfficiency, 0.06f));

            Register(KeystoneDefinition.Stat("axe_40_dmg", Skills.SkillType.Axes, 40, 0, "Heavy Hew", "Increases axe damage by 10%.", "ᚦ", CharacterStatType.DamageAxes, 0.10f));
            Register(KeystoneDefinition.MultiStat("axe_40_stagger", Skills.SkillType.Axes, 40, 1, "Rupturing Impact", "Increases stagger damage by 12% and knockback by 5%.", "ᚨ",
                (CharacterStatType.StaggerDamage, 0.12f, ModifierTier.IncreasedPercent),
                (CharacterStatType.KnockbackDealt, 0.05f, ModifierTier.IncreasedPercent)));

            Register(KeystoneDefinition.MultiStat("axe_60_cleave", Skills.SkillType.Axes, 60, 0, "Sweeping Cleave", "Increases axe damage by 8% and cleave efficiency by 8%.", "ᚱ",
                (CharacterStatType.DamageAxes, 0.08f, ModifierTier.IncreasedPercent),
                (CharacterStatType.CleaveEfficiency, 0.08f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("axe_60_kill", Skills.SkillType.Axes, 60, 1, "Blood Offering", "Restores +5 Health on killing any enemy.", "ᚲ", CharacterStatType.HealthOnKill, 5f, ModifierTier.Flat));

            Register(KeystoneDefinition.MultiStat("axe_80_chop", Skills.SkillType.Axes, 80, 0, "Timber Splitter", "Increases tree chop damage by 15% and wood yield by 10%.", "ᚷ",
                (CharacterStatType.DamageChop, 0.15f, ModifierTier.IncreasedPercent),
                (CharacterStatType.WoodcuttingYield, 0.10f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("axe_80_cleave", Skills.SkillType.Axes, 80, 1, "Scythe of War", "Reduces multi-target cleave damage penalty by 10%.", "ᚹ", CharacterStatType.CleaveEfficiency, 0.10f));

            Register(KeystoneDefinition.Stat("axe_100_fury", Skills.SkillType.Axes, 100, 0, "Berserker's Pace", "Increases axe attack speed by 15%.", "ᚺ", CharacterStatType.AttackSpeed, 0.15f));
            Register(KeystoneDefinition.Stat("axe_100_crit", Skills.SkillType.Axes, 100, 1, "Yggdrasil's Ruin", "Increases critical strike damage multiplier by 15%.", "ᚾ", CharacterStatType.CriticalStrikeMultiplier, 0.15f));

            // 3. Clubs
            Register(KeystoneDefinition.Stat("club_20_eff", Skills.SkillType.Clubs, 20, 0, "Dense Grip", "Reduces club stamina drain by 10%.", "ᚠ", CharacterStatType.ClubsEfficiency, 0.10f));
            Register(KeystoneDefinition.Stat("club_20_stagger", Skills.SkillType.Clubs, 20, 1, "Concussive Force", "Increases stagger damage by 10%.", "ᚢ", CharacterStatType.StaggerDamage, 0.10f));

            Register(KeystoneDefinition.Stat("club_40_staggered", Skills.SkillType.Clubs, 40, 0, "Skullsplitter", "Increases damage against staggered targets by 12%.", "ᚦ", CharacterStatType.DamageAgainstStaggered, 0.12f));
            Register(new KeystoneDefinition("club_40_sunder", Skills.SkillType.Clubs, 40, 1, "Concussive Sunder", "Staggering an enemy increases all physical damage they take by 10% for 6s.", "ᚨ"));

            Register(KeystoneDefinition.Stat("club_60_stam", Skills.SkillType.Clubs, 60, 0, "Thunderous Poise", "Restores +12 stamina whenever you break an enemy's guard.", "ᚱ", CharacterStatType.StaminaOnCauseStagger, 12f, ModifierTier.Flat));
            Register(KeystoneDefinition.Stat("club_60_twohand", Skills.SkillType.Clubs, 60, 1, "Two-Handed Might", "Increases damage with two-handed weapons by 8%.", "ᚲ", CharacterStatType.DamageTwoHanded, 0.08f));

            Register(KeystoneDefinition.MultiStat("club_80_dmg", Skills.SkillType.Clubs, 80, 0, "Heavy Iron", "Increases club damage by 8% and stagger damage by 6%.", "ᚷ",
                (CharacterStatType.DamageClubs, 0.08f, ModifierTier.IncreasedPercent),
                (CharacterStatType.StaggerDamage, 0.06f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("club_80_blunt", Skills.SkillType.Clubs, 80, 1, "Crushing Force", "Increases all blunt damage by 6%.", "ᚹ", CharacterStatType.DamageBlunt, 0.06f));

            Register(KeystoneDefinition.MultiStat("club_100_stagger", Skills.SkillType.Clubs, 100, 0, "Thor's Impact", "Restores +15 stamina on stagger and increases stagger damage by 10%.", "ᚺ",
                (CharacterStatType.StaminaOnCauseStagger, 15f, ModifierTier.Flat),
                (CharacterStatType.StaggerDamage, 0.10f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("club_100_secondary", Skills.SkillType.Clubs, 100, 1, "Titan's Maul", "Increases secondary special attack damage by 12%.", "ᚾ", CharacterStatType.DamageSecondaryAttack, 0.12f));

            // 4. Polearms
            Register(KeystoneDefinition.Stat("polearm_20_eff", Skills.SkillType.Polearms, 20, 0, "Sweeping Form", "Reduces polearm stamina drain by 10%.", "ᚠ", CharacterStatType.PolearmsEfficiency, 0.10f));
            Register(KeystoneDefinition.Stat("polearm_20_reach", Skills.SkillType.Polearms, 20, 1, "Long Reach", "Increases melee attack reach by 4%.", "ᚢ", CharacterStatType.MeleeReach, 0.04f));

            Register(KeystoneDefinition.Stat("polearm_40_pierce", Skills.SkillType.Polearms, 40, 0, "Spearhead Point", "Increases piercing damage by 8%.", "ᚦ", CharacterStatType.DamagePiercing, 0.08f));
            Register(KeystoneDefinition.Stat("polearm_40_finisher", Skills.SkillType.Polearms, 40, 1, "Thrust Finisher", "Increases 3rd combo attack damage by 12%.", "ᚨ", CharacterStatType.DamageComboFinisher, 0.12f));

            Register(KeystoneDefinition.Stat("polearm_60_speed", Skills.SkillType.Polearms, 60, 0, "Whirlwind Cadence", "Increases polearm attack speed by 8%.", "ᚱ", CharacterStatType.AttackSpeed, 0.08f));
            Register(KeystoneDefinition.Stat("polearm_60_air", Skills.SkillType.Polearms, 60, 1, "Skyfall Thrust", "Increases damage against airborne and flying enemies by 12%.", "ᚲ", CharacterStatType.DamageAgainstAirborne, 0.12f));

            Register(KeystoneDefinition.Stat("polearm_80_spin", Skills.SkillType.Polearms, 80, 0, "Whirlwind Master", "Reduces stamina cost of the 360-degree secondary spin by 15%.", "ᚷ", CharacterStatType.SecondaryAttackStaminaEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("polearm_80_dmg", Skills.SkillType.Polearms, 80, 1, "Phalanx Dominance", "Increases polearm damage by 8%.", "ᚹ", CharacterStatType.DamagePolearms, 0.08f));

            Register(KeystoneDefinition.MultiStat("polearm_100_gungnir", Skills.SkillType.Polearms, 100, 0, "Gungnir's Sweep", "Increases polearm damage by 6%, attack speed by 6%, and stamina efficiency by 6%.", "ᚺ",
                (CharacterStatType.DamagePolearms, 0.06f, ModifierTier.IncreasedPercent),
                (CharacterStatType.AttackSpeed, 0.06f, ModifierTier.IncreasedPercent),
                (CharacterStatType.PolearmsEfficiency, 0.06f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.MultiStat("polearm_100_crit", Skills.SkillType.Polearms, 100, 1, "Valkyrie Impaler", "Increases critical strike chance by 6% and critical damage by 10%.", "ᚾ",
                (CharacterStatType.CriticalStrikeChance, 0.06f, ModifierTier.IncreasedPercent),
                (CharacterStatType.CriticalStrikeMultiplier, 0.10f, ModifierTier.IncreasedPercent)));

            // 5. Spears
            Register(KeystoneDefinition.Stat("spear_20_eff", Skills.SkillType.Spears, 20, 0, "Light Haft", "Reduces spear stamina drain by 10%.", "ᚠ", CharacterStatType.SpearsEfficiency, 0.10f));
            Register(KeystoneDefinition.Stat("spear_20_pierce", Skills.SkillType.Spears, 20, 1, "Barbed Tip", "Increases piercing damage by 6%.", "ᚢ", CharacterStatType.DamagePiercing, 0.06f));

            Register(KeystoneDefinition.Stat("spear_40_speed", Skills.SkillType.Spears, 40, 0, "Rapid Thrust", "Increases spear attack speed by 8%.", "ᚦ", CharacterStatType.AttackSpeed, 0.08f));
            Register(KeystoneDefinition.Stat("spear_40_ranged", Skills.SkillType.Spears, 40, 1, "Javelin Force", "Increases ranged projectile damage by 6%.", "ᚨ", CharacterStatType.DamageRanged, 0.06f));

            Register(KeystoneDefinition.MultiStat("spear_60_swift", Skills.SkillType.Spears, 60, 0, "Skirmisher", "Increases jog and sprint speed by 8% while wielding a spear.", "ᚱ",
                (CharacterStatType.JogSpeed, 0.08f, ModifierTier.IncreasedPercent),
                (CharacterStatType.RunSpeed, 0.08f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("spear_60_first", Skills.SkillType.Spears, 60, 1, "Viper Strike", "Increases damage by 12% against enemies above 90% health.", "ᚲ", CharacterStatType.DamageFirstStrike, 0.12f));

            Register(KeystoneDefinition.MultiStat("spear_80_throw", Skills.SkillType.Spears, 80, 0, "Harpooner", "Increases thrown spear secondary damage by 10% and reduces throw stamina by 15%.", "ᚷ",
                (CharacterStatType.DamageSecondaryAttack, 0.10f, ModifierTier.IncreasedPercent),
                (CharacterStatType.SecondaryAttackStaminaEfficiency, 0.15f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("spear_80_melee", Skills.SkillType.Spears, 80, 1, "Martial Prowess", "Increases all melee damage by 6%.", "ᚹ", CharacterStatType.DamageMelee, 0.06f));

            Register(KeystoneDefinition.MultiStat("spear_100_harpoon", Skills.SkillType.Spears, 100, 0, "Odin's Harpoon", "Increases spear damage by 10% and spear stamina efficiency by 15%.", "ᚺ",
                (CharacterStatType.DamageSpears, 0.10f, ModifierTier.IncreasedPercent),
                (CharacterStatType.SpearsEfficiency, 0.15f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("spear_100_speed", Skills.SkillType.Spears, 100, 1, "Lightning Flurry", "Increases attack speed across all weapons by 8%.", "ᚾ", CharacterStatType.AttackSpeed, 0.08f));

            // 6. Knives
            Register(KeystoneDefinition.Stat("knife_20_eff", Skills.SkillType.Knives, 20, 0, "Silent Grip", "Reduces knife stamina drain by 12%.", "ᚠ", CharacterStatType.KnivesEfficiency, 0.12f));
            Register(KeystoneDefinition.Stat("knife_20_sneak", Skills.SkillType.Knives, 20, 1, "Backstab Stalker", "Increases sneak ambush damage by 10%.", "ᚢ", CharacterStatType.DamageSneak, 0.10f));

            Register(KeystoneDefinition.MultiStat("knife_40_crouch", Skills.SkillType.Knives, 40, 0, "Prowler's Stride", "Increases crouch movement speed by 15% and knife damage by 4%.", "ᚦ",
                (CharacterStatType.CrouchSpeed, 0.15f, ModifierTier.IncreasedPercent),
                (CharacterStatType.DamageKnives, 0.04f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("knife_40_crit", Skills.SkillType.Knives, 40, 1, "Throat Slicer", "Increases critical strike chance by 6%.", "ᚨ", CharacterStatType.CriticalStrikeChance, 0.06f));

            Register(KeystoneDefinition.MultiStat("knife_60_tempo", Skills.SkillType.Knives, 60, 0, "Flashing Daggers", "Increases attack speed by 6% and melee damage by 4%.", "ᚱ",
                (CharacterStatType.AttackSpeed, 0.06f, ModifierTier.IncreasedPercent),
                (CharacterStatType.DamageMelee, 0.04f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.MultiStat("knife_60_mobility", Skills.SkillType.Knives, 60, 1, "Shadow Step", "Increases concealment, jog speed, and run speed by 5%.", "ᚲ",
                (CharacterStatType.Concealment, 0.05f, ModifierTier.IncreasedPercent),
                (CharacterStatType.JogSpeed, 0.05f, ModifierTier.IncreasedPercent),
                (CharacterStatType.RunSpeed, 0.05f, ModifierTier.IncreasedPercent)));

            Register(KeystoneDefinition.Stat("knife_80_bleed", Skills.SkillType.Knives, 80, 0, "Arterial Slice", "Increases slash bleed damage by 12%.", "ᚷ", CharacterStatType.SlashBleedPercent, 0.12f));
            Register(KeystoneDefinition.Stat("knife_80_ambush", Skills.SkillType.Knives, 80, 1, "Predator's Eye", "Increases sneak ambush damage by 12%.", "ᚹ", CharacterStatType.DamageSneak, 0.12f));

            Register(new KeystoneDefinition("knife_100_flurry", Skills.SkillType.Knives, 100, 0, "Blood Flurry", "Slaying an enemy grants Flurry (+15% attack speed, +10% move speed for 8s).", "ᚺ"));
            Register(KeystoneDefinition.MultiStat("knife_100_assassin", Skills.SkillType.Knives, 100, 1, "Master Assassin", "Increases sneak attack damage by 15% and first-strike damage by 10%.", "ᚾ",
                (CharacterStatType.DamageSneak, 0.15f, ModifierTier.IncreasedPercent),
                (CharacterStatType.DamageFirstStrike, 0.10f, ModifierTier.IncreasedPercent)));

            // 7. Unarmed
            Register(KeystoneDefinition.Stat("unarmed_20_eff", Skills.SkillType.Unarmed, 20, 0, "Iron Knuckles", "Reduces unarmed stamina drain by 12%.", "ᚠ", CharacterStatType.UnarmedEfficiency, 0.12f));
            Register(KeystoneDefinition.Stat("unarmed_20_stagger", Skills.SkillType.Unarmed, 20, 1, "Haymaker", "Increases stagger damage by 10%.", "ᚢ", CharacterStatType.StaggerDamage, 0.10f));

            Register(KeystoneDefinition.Stat("unarmed_40_speed", Skills.SkillType.Unarmed, 40, 0, "Feral Claws", "Increases unarmed attack speed by 10%.", "ᚦ", CharacterStatType.AttackSpeed, 0.10f));
            Register(KeystoneDefinition.Stat("unarmed_40_kick", Skills.SkillType.Unarmed, 40, 1, "Brutal Kick", "Increases secondary kick damage by 15%.", "ᚨ", CharacterStatType.DamageSecondaryAttack, 0.15f));

            Register(KeystoneDefinition.Stat("unarmed_60_life", Skills.SkillType.Unarmed, 60, 0, "Brawler's Feast", "Restores +8 Health on barehanded or claw kills.", "ᚱ", CharacterStatType.HealthOnKill, 8f, ModifierTier.Flat));
            Register(KeystoneDefinition.Stat("unarmed_60_stam", Skills.SkillType.Unarmed, 60, 1, "Second Wind", "Increases stamina regeneration rate by 6%.", "ᚲ", CharacterStatType.StaminaRegen, 0.06f));

            Register(KeystoneDefinition.Stat("unarmed_80_stagger", Skills.SkillType.Unarmed, 80, 0, "Poise Shatter", "Increases stagger damage by 12%.", "ᚷ", CharacterStatType.StaggerDamage, 0.12f));
            Register(KeystoneDefinition.Stat("unarmed_80_dmg", Skills.SkillType.Unarmed, 80, 1, "Feral Might", "Increases unarmed damage by 8%.", "ᚹ", CharacterStatType.DamageUnarmed, 0.08f));

            Register(KeystoneDefinition.MultiStat("unarmed_100_fenrir", Skills.SkillType.Unarmed, 100, 0, "Fenrir's Bound", "Increases dodge stamina efficiency by 15% and unarmed damage by 10%.", "ᚺ",
                (CharacterStatType.DodgeEfficiency, 0.15f, ModifierTier.IncreasedPercent),
                (CharacterStatType.DamageUnarmed, 0.10f, ModifierTier.IncreasedPercent)));
            Register(new KeystoneDefinition("unarmed_100_adren", Skills.SkillType.Unarmed, 100, 1, "Adrenaline Surge", "Taking damage grants Adrenaline Surge (+15% melee damage, +25% stamina regen for 5s).", "ᚾ"));

            // 8. Bows
            Register(KeystoneDefinition.Stat("bow_20_draw", Skills.SkillType.Bows, 20, 0, "Snap String", "Increases bow draw speed by 15%.", "ᚠ", CharacterStatType.BowDrawSpeed, 0.15f));
            Register(KeystoneDefinition.Stat("bow_20_hold", Skills.SkillType.Bows, 20, 1, "Hunter's Stance", "Reduces bow draw hold stamina drain by 15%.", "ᚢ", CharacterStatType.BowDrawStaminaEfficiency, 0.15f));

            Register(KeystoneDefinition.Stat("bow_40_dmg", Skills.SkillType.Bows, 40, 0, "Barbed Flight", "Increases bow damage by 10%.", "ᚦ", CharacterStatType.DamageBows, 0.10f));
            Register(KeystoneDefinition.Stat("bow_40_prec", Skills.SkillType.Bows, 40, 1, "Eagle Eye", "Increases arrow precision by 25% (tighter spread and flatter trajectory).", "ᚨ", CharacterStatType.ProjectilePrecision, 0.25f));

            Register(KeystoneDefinition.Stat("bow_60_draw", Skills.SkillType.Bows, 60, 0, "Rapid Quiver", "Increases bow draw speed by 15%.", "ᚱ", CharacterStatType.BowDrawSpeed, 0.15f));
            Register(KeystoneDefinition.Stat("bow_60_sneak", Skills.SkillType.Bows, 60, 1, "Silent Marksman", "Increases sneak ambush damage with bows by 15%.", "ᚲ", CharacterStatType.DamageSneak, 0.15f));

            Register(KeystoneDefinition.Stat("bow_80_stamina", Skills.SkillType.Bows, 80, 0, "Iron Arm", "Reduces aiming hold stamina drain by 30%.", "ᚷ", CharacterStatType.BowDrawStaminaEfficiency, 0.30f));
            Register(KeystoneDefinition.Stat("bow_80_dmg", Skills.SkillType.Bows, 80, 1, "Heavy Arrows", "Increases bow damage by 8%.", "ᚹ", CharacterStatType.DamageBows, 0.08f));

            Register(KeystoneDefinition.MultiStat("bow_100_sniper", Skills.SkillType.Bows, 100, 0, "Ullr's Precision", "Increases bow damage by 12% and draw speed by 15%.", "ᚺ",
                (CharacterStatType.DamageBows, 0.12f, ModifierTier.IncreasedPercent),
                (CharacterStatType.BowDrawSpeed, 0.15f, ModifierTier.IncreasedPercent)));
            Register(new KeystoneDefinition("bow_100_cripple", Skills.SkillType.Bows, 100, 1, "Pinning Arrow", "Arrow hits inflict Crippled (reduces target movement speed by 15% for 6s).", "ᚾ"));

            // 9. Crossbows
            Register(KeystoneDefinition.Stat("crossbow_20_reload", Skills.SkillType.Crossbows, 20, 0, "Greased Windlass", "Increases crossbow reload speed by 15%.", "ᚠ", CharacterStatType.CrossbowReloadSpeed, 0.15f));
            Register(KeystoneDefinition.MultiStat("crossbow_20_endure", Skills.SkillType.Crossbows, 20, 1, "Scout's Agility", "Increases sprint efficiency by 10% and stamina regen by 4%.", "ᚢ",
                (CharacterStatType.SprintEfficiency, 0.10f, ModifierTier.IncreasedPercent),
                (CharacterStatType.StaminaRegen, 0.04f, ModifierTier.IncreasedPercent)));

            Register(KeystoneDefinition.Stat("crossbow_40_mobile", Skills.SkillType.Crossbows, 40, 0, "Mobile Loader", "Increases walk speed while reloading by 15%.", "ᚦ", CharacterStatType.WalkSpeed, 0.15f));
            Register(new KeystoneDefinition("crossbow_40_bleed", Skills.SkillType.Crossbows, 40, 1, "Barbed Quarrel", "Crossbow critical hits inflict bleeding wounds over 8s.", "ᚨ"));

            Register(KeystoneDefinition.Stat("crossbow_60_dmg", Skills.SkillType.Crossbows, 60, 0, "Arbalest Power", "Increases crossbow damage by 10%.", "ᚱ", CharacterStatType.DamageCrossbows, 0.10f));
            Register(KeystoneDefinition.Stat("crossbow_60_ranged", Skills.SkillType.Crossbows, 60, 1, "Heavy Ballistics", "Increases all ranged damage by 8%.", "ᚲ", CharacterStatType.DamageRanged, 0.08f));

            Register(KeystoneDefinition.Stat("crossbow_80_eff", Skills.SkillType.Crossbows, 80, 0, "Clockwork Crank", "Reduces reload stamina cost by 25%.", "ᚷ", CharacterStatType.CrossbowReloadStaminaEfficiency, 0.25f));
            Register(KeystoneDefinition.Stat("crossbow_80_dmg", Skills.SkillType.Crossbows, 80, 1, "Dwarven Mechanism", "Increases crossbow damage by 8%.", "ᚹ", CharacterStatType.DamageCrossbows, 0.08f));

            Register(KeystoneDefinition.Stat("crossbow_100_siege", Skills.SkillType.Crossbows, 100, 0, "Siege Ballista", "Increases bolt knockback force by 35%.", "ᚺ", CharacterStatType.KnockbackDealt, 0.35f));
            Register(KeystoneDefinition.Stat("crossbow_100_haste", Skills.SkillType.Crossbows, 100, 1, "Rapid Ratchet", "Increases general weapon attack speed by 6%.", "ᚾ", CharacterStatType.AttackSpeed, 0.06f));
        }

        private static void RegisterDefense()
        {
            // 10. Blocking
            Register(KeystoneDefinition.Stat("block_20_eff", Skills.SkillType.Blocking, 20, 0, "Solid Stance", "Reduces stamina consumed when blocking by 15%.", "ᚠ", CharacterStatType.BlockStaminaEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("block_20_armor", Skills.SkillType.Blocking, 20, 1, "Reinforced Rim", "Increases physical armor efficiency by 5%.", "ᚢ", CharacterStatType.ArmorEfficiency, 0.05f));

            Register(new KeystoneDefinition("block_40_riposte", Skills.SkillType.Blocking, 40, 0, "Riposte Stance", "A successful parry empowers your next melee attack by +25% damage.", "ᚦ"));
            Register(KeystoneDefinition.MultiStat("block_40_window", Skills.SkillType.Blocking, 40, 1, "Perfect Timing", "Extends parry window by 25ms and increases parry bonus by 12%.", "ᚨ",
                (CharacterStatType.ParryWindow, 0.025f, ModifierTier.Flat),
                (CharacterStatType.ParryMultiplier, 0.12f, ModifierTier.IncreasedPercent)));

            Register(KeystoneDefinition.Stat("block_60_power", Skills.SkillType.Blocking, 60, 0, "Iron Bulwark", "Increases block power by 12%.", "ᚱ", CharacterStatType.BlockPowerMultiplier, 0.12f));
            Register(new KeystoneDefinition("block_60_fort", Skills.SkillType.Blocking, 60, 1, "Fortified Guard", "Blocking an attack grants Fortified (+20% armor efficiency on next hit taken).", "ᚲ"));

            Register(KeystoneDefinition.Stat("block_80_eff", Skills.SkillType.Blocking, 80, 0, "Unyielding Stance", "Reduces block stamina cost by 20%.", "ᚷ", CharacterStatType.BlockStaminaEfficiency, 0.20f));
            Register(KeystoneDefinition.Stat("block_80_power", Skills.SkillType.Blocking, 80, 1, "Tower Aegis", "Increases block power by 10%.", "ᚹ", CharacterStatType.BlockPowerMultiplier, 0.10f));

            Register(KeystoneDefinition.MultiStat("block_100_bastion", Skills.SkillType.Blocking, 100, 0, "Titan's Bastion", "Increases block power by 12% and block stamina efficiency by 12%.", "ᚺ",
                (CharacterStatType.BlockPowerMultiplier, 0.12f, ModifierTier.IncreasedPercent),
                (CharacterStatType.BlockStaminaEfficiency, 0.12f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("block_100_vitality", Skills.SkillType.Blocking, 100, 1, "Heart of Stone", "Increases maximum health by +10.", "ᚾ", CharacterStatType.MaxHealth, 10f, ModifierTier.Flat));

            // 11. Dodge
            Register(new KeystoneDefinition("dodge_20_counter", Skills.SkillType.Dodge, 20, 0, "Counter Roll", "Dodging through an attack empowers your next attack by +15% damage.", "ᚠ"));
            Register(KeystoneDefinition.Stat("dodge_20_eff", Skills.SkillType.Dodge, 20, 1, "Nimble Form", "Reduces dodge stamina cost by 12%.", "ᚢ", CharacterStatType.DodgeEfficiency, 0.12f));

            Register(new KeystoneDefinition("dodge_40_wind", Skills.SkillType.Dodge, 40, 0, "Windrunner", "Dodging an attack grants Windrunner (+15% movement speed for 6s).", "ᚦ"));
            Register(KeystoneDefinition.MultiStat("dodge_40_speed", Skills.SkillType.Dodge, 40, 1, "Evasive Agility", "Increases jog speed and sprint speed by 8%.", "ᚨ",
                (CharacterStatType.JogSpeed, 0.08f, ModifierTier.IncreasedPercent),
                (CharacterStatType.RunSpeed, 0.08f, ModifierTier.IncreasedPercent)));

            Register(KeystoneDefinition.Stat("dodge_60_stam", Skills.SkillType.Dodge, 60, 0, "Adrenaline Recovery", "Restores +10 stamina when taking damage.", "ᚱ", CharacterStatType.StaminaOnDamageTaken, 10f, ModifierTier.Flat));
            Register(KeystoneDefinition.Stat("dodge_60_eff", Skills.SkillType.Dodge, 60, 1, "Fluid Leap", "Reduces dodge stamina cost by 15%.", "ᚲ", CharacterStatType.DodgeEfficiency, 0.15f));

            Register(KeystoneDefinition.Stat("dodge_80_eff", Skills.SkillType.Dodge, 80, 0, "Acrobat's Grace", "Reduces dodge stamina cost by 15%.", "ᚷ", CharacterStatType.DodgeEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("dodge_80_speed", Skills.SkillType.Dodge, 80, 1, "Fleet of Foot", "Increases sprint speed by 5%.", "ᚹ", CharacterStatType.RunSpeed, 0.05f));

            Register(new KeystoneDefinition("dodge_100_stone", Skills.SkillType.Dodge, 100, 0, "Stone Guard", "Dodging grants Stone Guard (+15% physical damage resistance for 4s).", "ᚺ"));
            Register(KeystoneDefinition.Stat("dodge_100_master", Skills.SkillType.Dodge, 100, 1, "Mirage Dancer", "Reduces dodge stamina cost by 25%.", "ᚾ", CharacterStatType.DodgeEfficiency, 0.25f));
        }

        private static void RegisterMagic()
        {
            // 12. Elemental Magic
            Register(KeystoneDefinition.Stat("elemental_20_eff", Skills.SkillType.ElementalMagic, 20, 0, "Eitr Flow", "Reduces elemental spell Eitr cost by 12%.", "ᚠ", CharacterStatType.ElementalMagicEitrEfficiency, 0.12f));
            Register(KeystoneDefinition.MultiStat("elemental_20_spark", Skills.SkillType.ElementalMagic, 20, 1, "Primal Spark", "Increases fire and frost damage by 5%.", "ᚢ",
                (CharacterStatType.DamageFire, 0.05f, ModifierTier.IncreasedPercent),
                (CharacterStatType.DamageFrost, 0.05f, ModifierTier.IncreasedPercent)));

            Register(KeystoneDefinition.Stat("elemental_40_dmg", Skills.SkillType.ElementalMagic, 40, 0, "Spellweaver", "Increases elemental spell damage by 10%.", "ᚦ", CharacterStatType.DamageElementalMagic, 0.10f));
            Register(KeystoneDefinition.Stat("elemental_40_health", Skills.SkillType.ElementalMagic, 40, 1, "Arcane Vitality", "Increases maximum health by +8.", "ᚨ", CharacterStatType.MaxHealth, 8f, ModifierTier.Flat));

            Register(KeystoneDefinition.Stat("elemental_60_dmg", Skills.SkillType.ElementalMagic, 60, 0, "Astral Surge", "Increases elemental spell damage by 10%.", "ᚱ", CharacterStatType.DamageElementalMagic, 0.10f));
            Register(KeystoneDefinition.Stat("elemental_60_regen", Skills.SkillType.ElementalMagic, 60, 1, "Deep Well", "Increases Eitr regeneration rate by 6%.", "ᚲ", CharacterStatType.EitrRegen, 0.06f));

            Register(KeystoneDefinition.Stat("elemental_80_eff", Skills.SkillType.ElementalMagic, 80, 0, "Conduit Mastery", "Reduces elemental spell Eitr cost by 15%.", "ᚷ", CharacterStatType.ElementalMagicEitrEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("elemental_80_speed", Skills.SkillType.ElementalMagic, 80, 1, "Spellstride", "Increases jog speed by 5%.", "ᚹ", CharacterStatType.JogSpeed, 0.05f));

            Register(KeystoneDefinition.MultiStat("elemental_100_archmage", Skills.SkillType.ElementalMagic, 100, 0, "Archmage Ascendance", "Increases elemental spell damage by 12% and Eitr efficiency by 10%.", "ᚺ",
                (CharacterStatType.DamageElementalMagic, 0.12f, ModifierTier.IncreasedPercent),
                (CharacterStatType.ElementalMagicEitrEfficiency, 0.10f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("elemental_100_crit", Skills.SkillType.ElementalMagic, 100, 1, "Arcane Overload", "Increases critical strike chance with magic spells by 8%.", "ᚾ", CharacterStatType.CriticalStrikeChance, 0.08f));

            // 13. Blood Magic
            Register(KeystoneDefinition.Stat("blood_20_health", Skills.SkillType.BloodMagic, 20, 0, "Vital Spark", "Reduces health cost of blood spells by 12%.", "ᚠ", CharacterStatType.BloodMagicHealthEfficiency, 0.12f));
            Register(KeystoneDefinition.Stat("blood_20_eitr", Skills.SkillType.BloodMagic, 20, 1, "Blood Channeling", "Reduces Eitr cost of blood spells by 10%.", "ᚢ", CharacterStatType.BloodMagicEitrEfficiency, 0.10f));

            Register(KeystoneDefinition.Stat("blood_40_dmg", Skills.SkillType.BloodMagic, 40, 0, "Sanguine Pact", "Increases blood spell damage by 10%.", "ᚦ", CharacterStatType.DamageBloodMagic, 0.10f));
            Register(KeystoneDefinition.Stat("blood_40_magic", Skills.SkillType.BloodMagic, 40, 1, "Universal Arcana", "Increases all magic damage by 10%.", "ᚨ", CharacterStatType.DamageMagic, 0.10f));

            Register(KeystoneDefinition.Stat("blood_60_health", Skills.SkillType.BloodMagic, 60, 0, "Grim Bargain", "Reduces health cost of blood spells by 12%.", "ᚱ", CharacterStatType.BloodMagicHealthEfficiency, 0.12f));
            Register(KeystoneDefinition.Stat("blood_60_regen", Skills.SkillType.BloodMagic, 60, 1, "Vampiric Pulse", "Increases health regeneration rate by 5%.", "ᚲ", CharacterStatType.HealthRegen, 0.05f));

            Register(KeystoneDefinition.Stat("blood_80_health", Skills.SkillType.BloodMagic, 80, 0, "Blood Tithe", "Reduces health cost of blood spells by 15%.", "ᚷ", CharacterStatType.BloodMagicHealthEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("blood_80_armor", Skills.SkillType.BloodMagic, 80, 1, "Bone Fortitude", "Increases physical damage resistance by 5%.", "ᚹ", CharacterStatType.ResistancePhysical, 0.95f, ModifierTier.Multiplier));

            Register(KeystoneDefinition.Stat("blood_100_sanguine", Skills.SkillType.BloodMagic, 100, 0, "Sanguine Ward", "Restores +8 Health whenever you take damage.", "ᚺ", CharacterStatType.HealthOnDamageTaken, 8f, ModifierTier.Flat));
            Register(KeystoneDefinition.MultiStat("blood_100_pool", Skills.SkillType.BloodMagic, 100, 1, "Deep Bloodwell", "Increases blood magic Eitr efficiency by 12% and maximum Eitr by +10.", "ᚾ",
                (CharacterStatType.BloodMagicEitrEfficiency, 0.12f, ModifierTier.IncreasedPercent),
                (CharacterStatType.MaxEitr, 10f, ModifierTier.Flat)));
        }

        private static void RegisterMobility()
        {
            // 14. Run
            Register(KeystoneDefinition.Stat("run_20_eff", Skills.SkillType.Run, 20, 0, "Fleetfoot", "Reduces sprint stamina drain by 12%.", "ᚠ", CharacterStatType.SprintEfficiency, 0.12f));
            Register(KeystoneDefinition.Stat("run_20_speed", Skills.SkillType.Run, 20, 1, "Long Stride", "Increases sprint movement speed by 4%.", "ᚢ", CharacterStatType.RunSpeed, 0.04f));

            Register(KeystoneDefinition.Stat("run_40_eff", Skills.SkillType.Run, 40, 0, "Marathoner", "Reduces sprint stamina drain by 15%.", "ᚦ", CharacterStatType.SprintEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("run_40_speed", Skills.SkillType.Run, 40, 1, "Windrunner", "Increases sprint movement speed by 5%.", "ᚨ", CharacterStatType.RunSpeed, 0.05f));

            Register(KeystoneDefinition.Stat("run_60_eff", Skills.SkillType.Run, 60, 0, "Endurance Pace", "Reduces sprint stamina drain by 15%.", "ᚱ", CharacterStatType.SprintEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("run_60_delay", Skills.SkillType.Run, 60, 1, "Quick Recovery", "Stamina begins regenerating 15% faster after sprinting.", "ᚲ", CharacterStatType.StaminaRegenDelay, -0.15f, ModifierTier.IncreasedPercent));

            Register(KeystoneDefinition.Stat("run_80_eff", Skills.SkillType.Run, 80, 0, "Unstoppable Gale", "Reduces sprint stamina drain by 15%.", "ᚷ", CharacterStatType.SprintEfficiency, 0.15f));
            Register(KeystoneDefinition.MultiStat("run_80_speed", Skills.SkillType.Run, 80, 1, "Athletic Gait", "Increases jog speed and walk speed by 5%.", "ᚹ",
                (CharacterStatType.JogSpeed, 0.05f, ModifierTier.IncreasedPercent),
                (CharacterStatType.WalkSpeed, 0.05f, ModifierTier.IncreasedPercent)));

            Register(KeystoneDefinition.MultiStat("run_100_apex", Skills.SkillType.Run, 100, 0, "Apex Strider", "Increases sprint speed by 8% and sprint stamina efficiency by 15%.", "ᚺ",
                (CharacterStatType.RunSpeed, 0.08f, ModifierTier.IncreasedPercent),
                (CharacterStatType.SprintEfficiency, 0.15f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("run_100_regen", Skills.SkillType.Run, 100, 1, "Hermod's Vigor", "Increases general stamina regeneration rate by 8%.", "ᚾ", CharacterStatType.StaminaRegen, 0.08f));

            // 15. Jump
            Register(KeystoneDefinition.Stat("jump_20_eff", Skills.SkillType.Jump, 20, 0, "Spring Step", "Reduces jump stamina cost by 12%.", "ᚠ", CharacterStatType.JumpEfficiency, 0.12f));
            Register(KeystoneDefinition.Stat("jump_20_height", Skills.SkillType.Jump, 20, 1, "High Vault", "Increases jump apex height by 0.12m.", "ᚢ", CharacterStatType.JumpHeight, 0.12f, ModifierTier.Flat));

            Register(KeystoneDefinition.Stat("jump_40_fall", Skills.SkillType.Jump, 40, 0, "Feather Landing", "Reduces fall damage received by 15%.", "ᚦ", CharacterStatType.ResistanceFall, 0.85f, ModifierTier.Multiplier));
            Register(KeystoneDefinition.Stat("jump_40_height", Skills.SkillType.Jump, 40, 1, "Acrobatic Leap", "Increases jump apex height by 0.10m.", "ᚨ", CharacterStatType.JumpHeight, 0.10f, ModifierTier.Flat));

            Register(KeystoneDefinition.Stat("jump_60_height", Skills.SkillType.Jump, 60, 0, "Sky Strider", "Increases jump apex height by 0.15m.", "ᚱ", CharacterStatType.JumpHeight, 0.15f, ModifierTier.Flat));
            Register(KeystoneDefinition.Stat("jump_60_fall", Skills.SkillType.Jump, 60, 1, "Cat's Grace", "Reduces fall damage received by 20%.", "ᚲ", CharacterStatType.ResistanceFall, 0.80f, ModifierTier.Multiplier));

            Register(KeystoneDefinition.Stat("jump_80_eff", Skills.SkillType.Jump, 80, 0, "Effortless Bound", "Reduces jump stamina cost by 15%.", "ᚷ", CharacterStatType.JumpEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("jump_80_height", Skills.SkillType.Jump, 80, 1, "Soaring Vault", "Increases jump apex height by 0.15m.", "ᚹ", CharacterStatType.JumpHeight, 0.15f, ModifierTier.Flat));

            Register(KeystoneDefinition.MultiStat("jump_100_bound", Skills.SkillType.Jump, 100, 0, "Valkyrie Flight", "Increases jump apex height by 0.20m and cuts jump stamina by 15%.", "ᚺ",
                (CharacterStatType.JumpHeight, 0.20f, ModifierTier.Flat),
                (CharacterStatType.JumpEfficiency, 0.15f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("jump_100_stagger", Skills.SkillType.Jump, 100, 1, "Comet Impact", "Increases weapon stagger damage dealt by 10%.", "ᚾ", CharacterStatType.StaggerDamage, 0.10f));

            // 16. Sneak
            Register(KeystoneDefinition.Stat("sneak_20_eff", Skills.SkillType.Sneak, 20, 0, "Quiet Footsteps", "Reduces sneak stamina cost by 12%.", "ᚠ", CharacterStatType.SneakEfficiency, 0.12f));
            Register(KeystoneDefinition.Stat("sneak_20_conc", Skills.SkillType.Sneak, 20, 1, "Dark Camouflage", "Reduces enemy detection visibility by 10%.", "ᚢ", CharacterStatType.Concealment, 0.10f));

            Register(KeystoneDefinition.Stat("sneak_40_crouch", Skills.SkillType.Sneak, 40, 0, "Prowling Stride", "Increases crouch movement speed by 15%.", "ᚦ", CharacterStatType.CrouchSpeed, 0.15f));
            Register(KeystoneDefinition.Stat("sneak_40_dmg", Skills.SkillType.Sneak, 40, 1, "Stalker's Eye", "Increases sneak ambush damage by 12%.", "ᚨ", CharacterStatType.DamageSneak, 0.12f));

            Register(KeystoneDefinition.Stat("sneak_60_eff", Skills.SkillType.Sneak, 60, 0, "Ghost Tread", "Reduces sneak stamina cost by 15%.", "ᚱ", CharacterStatType.SneakEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("sneak_60_conc", Skills.SkillType.Sneak, 60, 1, "Silent Shadow", "Increases concealment rating by 15%.", "ᚲ", CharacterStatType.Concealment, 0.15f));

            Register(KeystoneDefinition.Stat("sneak_80_conc", Skills.SkillType.Sneak, 80, 0, "Veil of Shadows", "Increases concealment rating by 15%.", "ᚷ", CharacterStatType.Concealment, 0.15f));
            Register(KeystoneDefinition.Stat("sneak_80_dmg", Skills.SkillType.Sneak, 80, 1, "Lethal Strike", "Increases sneak ambush damage by 15%.", "ᚹ", CharacterStatType.DamageSneak, 0.15f));

            Register(new KeystoneDefinition("sneak_100_shadow", Skills.SkillType.Sneak, 100, 0, "Shadowveil", "Slaying an enemy from sneak grants Shadowveil (+40% concealment, +25% crouch speed for 8s).", "ᚺ"));
            Register(KeystoneDefinition.Stat("sneak_100_assassin", Skills.SkillType.Sneak, 100, 1, "Master Executioner", "Increases sneak attack bonus damage by 20%.", "ᚾ", CharacterStatType.DamageSneak, 0.20f));

            // 17. Swim
            Register(KeystoneDefinition.Stat("swim_20_eff", Skills.SkillType.Swim, 20, 0, "Aquatic Form", "Reduces swim stamina drain by 15%.", "ᚠ", CharacterStatType.SwimEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("swim_20_speed", Skills.SkillType.Swim, 20, 1, "Strong Stroke", "Increases swimming speed by 8%.", "ᚢ", CharacterStatType.SwimSpeed, 0.08f));

            Register(KeystoneDefinition.Stat("swim_40_speed", Skills.SkillType.Swim, 40, 0, "Tide Rider", "Increases swimming speed by 10%.", "ᚦ", CharacterStatType.SwimSpeed, 0.10f));
            Register(KeystoneDefinition.Stat("swim_40_eff", Skills.SkillType.Swim, 40, 1, "Deep Lungs", "Reduces swim stamina drain by 15%.", "ᚨ", CharacterStatType.SwimEfficiency, 0.15f));

            Register(KeystoneDefinition.Stat("swim_60_eff", Skills.SkillType.Swim, 60, 0, "Ocean's Kin", "Reduces swim stamina drain by 20%.", "ᚱ", CharacterStatType.SwimEfficiency, 0.20f));
            Register(KeystoneDefinition.Stat("swim_60_speed", Skills.SkillType.Swim, 60, 1, "Tidal Surge", "Increases swimming speed by 10%.", "ᚲ", CharacterStatType.SwimSpeed, 0.10f));

            Register(KeystoneDefinition.Stat("swim_80_eff", Skills.SkillType.Swim, 80, 0, "Buoyancy Mastery", "Reduces swim stamina drain by 20%.", "ᚷ", CharacterStatType.SwimEfficiency, 0.20f));
            Register(KeystoneDefinition.Stat("swim_80_speed", Skills.SkillType.Swim, 80, 1, "Wave Crest", "Increases swimming speed by 10%.", "ᚹ", CharacterStatType.SwimSpeed, 0.10f));

            Register(KeystoneDefinition.MultiStat("swim_100_aegir", Skills.SkillType.Swim, 100, 0, "Aegir's Blessing", "Reduces swim stamina drain by 25% and boosts swim speed by 12%.", "ᚺ",
                (CharacterStatType.SwimEfficiency, 0.25f, ModifierTier.IncreasedPercent),
                (CharacterStatType.SwimSpeed, 0.12f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("swim_100_vital", Skills.SkillType.Swim, 100, 1, "Sea Serpent Grace", "Increases natural health regeneration rate by 8%.", "ᚾ", CharacterStatType.HealthRegen, 0.08f));

            // 18. Ride
            Register(KeystoneDefinition.Stat("ride_20_eff", Skills.SkillType.Ride, 20, 0, "Comfortable Saddle", "Reduces mount sprint stamina drain by 15%.", "ᚠ", CharacterStatType.RidingStaminaEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("ride_20_speed", Skills.SkillType.Ride, 20, 1, "Spur of Speed", "Increases mount sprint speed by 6%.", "ᚢ", CharacterStatType.RidingSpeed, 0.06f));

            Register(KeystoneDefinition.Stat("ride_40_control", Skills.SkillType.Ride, 40, 0, "Obedient Reins", "Increases mount turning and acceleration responsiveness by 10%.", "ᚦ", CharacterStatType.RidingControl, 0.10f));
            Register(KeystoneDefinition.Stat("ride_40_eff", Skills.SkillType.Ride, 40, 1, "Symbiotic Bond", "Reduces mount stamina drain by 10%.", "ᚨ", CharacterStatType.RidingStaminaEfficiency, 0.10f));

            Register(KeystoneDefinition.Stat("ride_60_eff", Skills.SkillType.Ride, 60, 0, "War Charger", "Reduces mount sprint stamina drain by 15%.", "ᚱ", CharacterStatType.RidingStaminaEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("ride_60_dmg", Skills.SkillType.Ride, 60, 1, "Cavalier Strike", "Increases weapon damage with one-handed weapons by 8%.", "ᚲ", CharacterStatType.DamageOneHanded, 0.08f));

            Register(KeystoneDefinition.Stat("ride_80_control", Skills.SkillType.Ride, 80, 0, "Equestrian Mastery", "Increases mount handling control by 10%.", "ᚷ", CharacterStatType.RidingControl, 0.10f));
            Register(KeystoneDefinition.Stat("ride_80_speed", Skills.SkillType.Ride, 80, 1, "Swift Gallop", "Increases mount sprint speed by 6%.", "ᚹ", CharacterStatType.RidingSpeed, 0.06f));

            Register(KeystoneDefinition.MultiStat("ride_100_cavalry", Skills.SkillType.Ride, 100, 0, "Sleipnir's Kin", "Increases mount sprint speed by 10% and cuts mount stamina drain by 15%.", "ᚺ",
                (CharacterStatType.RidingSpeed, 0.10f, ModifierTier.IncreasedPercent),
                (CharacterStatType.RidingStaminaEfficiency, 0.15f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("ride_100_armor", Skills.SkillType.Ride, 100, 1, "Armored Barding", "Increases physical armor efficiency by 10%.", "ᚾ", CharacterStatType.ArmorEfficiency, 0.10f));
        }

        private static void RegisterResources()
        {
            // 19. Woodcutting
            Register(KeystoneDefinition.Stat("wood_20_eff", Skills.SkillType.WoodCutting, 20, 0, "Deep Bite", "Increases chop damage against trees by 8%.", "ᚠ", CharacterStatType.DamageChop, 0.08f));
            Register(KeystoneDefinition.Stat("wood_20_yield", Skills.SkillType.WoodCutting, 20, 1, "Clean Cut", "Increases woodcutting duplicate drop yield by 5%.", "ᚢ", CharacterStatType.WoodcuttingYield, 0.05f));

            Register(KeystoneDefinition.Stat("wood_40_chop", Skills.SkillType.WoodCutting, 40, 0, "Rapid Chop", "Increases chop damage against trees by 10%.", "ᚦ", CharacterStatType.DamageChop, 0.10f));
            Register(KeystoneDefinition.Stat("wood_40_dur", Skills.SkillType.WoodCutting, 40, 1, "Hardened Axe", "Reduces axe tool durability decay by 12%.", "ᚨ", CharacterStatType.DurabilityEfficiency, 0.12f));

            Register(KeystoneDefinition.Stat("wood_60_yield", Skills.SkillType.WoodCutting, 60, 0, "Bountiful Timber", "Increases woodcutting drop yield by 6%.", "ᚱ", CharacterStatType.WoodcuttingYield, 0.06f));
            Register(KeystoneDefinition.Stat("wood_60_chop", Skills.SkillType.WoodCutting, 60, 1, "Log Splitter", "Increases chop damage against logs by 15%.", "ᚲ", CharacterStatType.DamageChop, 0.15f));

            Register(KeystoneDefinition.Stat("wood_80_eff", Skills.SkillType.WoodCutting, 80, 0, "Heartwood Cleaver", "Increases chop damage by 12%.", "ᚷ", CharacterStatType.DamageChop, 0.12f));
            Register(KeystoneDefinition.Stat("wood_80_dur", Skills.SkillType.WoodCutting, 80, 1, "Reinforced Edge", "Reduces tool durability loss by 12%.", "ᚹ", CharacterStatType.DurabilityEfficiency, 0.12f));

            Register(KeystoneDefinition.MultiStat("wood_100_master", Skills.SkillType.WoodCutting, 100, 0, "Yggdrasil's Forester", "Increases wood yield by 10% and durability efficiency by 15%.", "ᚺ",
                (CharacterStatType.WoodcuttingYield, 0.10f, ModifierTier.IncreasedPercent),
                (CharacterStatType.DurabilityEfficiency, 0.15f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("wood_100_chop", Skills.SkillType.WoodCutting, 100, 1, "Felling Strike", "Increases chop damage by 15%.", "ᚾ", CharacterStatType.DamageChop, 0.15f));

            // 20. Pickaxes
            Register(KeystoneDefinition.Stat("pick_20_eff", Skills.SkillType.Pickaxes, 20, 0, "Pointed Impact", "Increases pickaxe damage against deposits by 8%.", "ᚠ", CharacterStatType.DamagePickaxe, 0.08f));
            Register(KeystoneDefinition.Stat("pick_20_yield", Skills.SkillType.Pickaxes, 20, 1, "Ore Seeker", "Increases mining duplicate drop yield by 5%.", "ᚢ", CharacterStatType.MiningYield, 0.05f));

            Register(KeystoneDefinition.Stat("pick_40_pick", Skills.SkillType.Pickaxes, 40, 0, "Heavy Pick", "Increases pickaxe damage against deposits by 10%.", "ᚦ", CharacterStatType.DamagePickaxe, 0.10f));
            Register(KeystoneDefinition.Stat("pick_40_dur", Skills.SkillType.Pickaxes, 40, 1, "Reinforced Tip", "Reduces pickaxe durability decay by 12%.", "ᚨ", CharacterStatType.DurabilityEfficiency, 0.12f));

            Register(KeystoneDefinition.Stat("pick_60_yield", Skills.SkillType.Pickaxes, 60, 0, "Rich Vein", "Increases mining drop yield by 6%.", "ᚱ", CharacterStatType.MiningYield, 0.06f));
            Register(KeystoneDefinition.Stat("pick_60_pick", Skills.SkillType.Pickaxes, 60, 1, "Fracture Point", "Increases pickaxe damage by 15%.", "ᚲ", CharacterStatType.DamagePickaxe, 0.15f));

            Register(KeystoneDefinition.Stat("pick_80_eff", Skills.SkillType.Pickaxes, 80, 0, "Mountain Sunderer", "Increases pickaxe damage by 12%.", "ᚷ", CharacterStatType.DamagePickaxe, 0.12f));
            Register(KeystoneDefinition.Stat("pick_80_dur", Skills.SkillType.Pickaxes, 80, 1, "Tempered Steel", "Reduces tool durability decay by 12%.", "ᚹ", CharacterStatType.DurabilityEfficiency, 0.12f));

            Register(KeystoneDefinition.MultiStat("pick_100_master", Skills.SkillType.Pickaxes, 100, 0, "Dwarven Delver", "Increases mining yield by 10% and durability efficiency by 15%.", "ᚺ",
                (CharacterStatType.MiningYield, 0.10f, ModifierTier.IncreasedPercent),
                (CharacterStatType.DurabilityEfficiency, 0.15f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("pick_100_pick", Skills.SkillType.Pickaxes, 100, 1, "Bedrock Breaker", "Increases pickaxe damage by 15%.", "ᚾ", CharacterStatType.DamagePickaxe, 0.15f));

            // 21. Farming
            Register(KeystoneDefinition.Stat("farm_20_eff", Skills.SkillType.Farming, 20, 0, "Swift Cultivator", "Reduces cultivator stamina cost by 12%.", "ᚠ", CharacterStatType.ToolStaminaEfficiency, 0.12f));
            Register(KeystoneDefinition.Stat("farm_20_yield", Skills.SkillType.Farming, 20, 1, "Abundant Soil", "Increases harvested crop duplicate yield by 5%.", "ᚢ", CharacterStatType.ForagingYield, 0.05f));

            Register(KeystoneDefinition.Stat("farm_40_rad", Skills.SkillType.Farming, 40, 0, "Wide Scythe", "Increases scythe and cultivator harvesting radius by 15%.", "ᚦ", CharacterStatType.HarvestRadius, 0.15f));
            Register(KeystoneDefinition.Stat("farm_40_dur", Skills.SkillType.Farming, 40, 1, "Sturdy Cultivator", "Reduces farming tool durability loss by 12%.", "ᚨ", CharacterStatType.DurabilityEfficiency, 0.12f));

            Register(KeystoneDefinition.Stat("farm_60_yield", Skills.SkillType.Farming, 60, 0, "Golden Sheaf", "Increases harvested crop yield by 6%.", "ᚱ", CharacterStatType.ForagingYield, 0.06f));
            Register(KeystoneDefinition.Stat("farm_60_eff", Skills.SkillType.Farming, 60, 1, "Effortless Fieldwork", "Reduces cultivator stamina drain by 10%.", "ᚲ", CharacterStatType.ToolStaminaEfficiency, 0.10f));

            Register(KeystoneDefinition.Stat("farm_80_eff", Skills.SkillType.Farming, 80, 0, "Harvester's Cadence", "Reduces cultivator stamina cost by 15%.", "ᚷ", CharacterStatType.ToolStaminaEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("farm_80_rad", Skills.SkillType.Farming, 80, 1, "Reaping Sweep", "Increases crop harvesting radius by 15%.", "ᚹ", CharacterStatType.HarvestRadius, 0.15f));

            Register(KeystoneDefinition.MultiStat("farm_100_freyr", Skills.SkillType.Farming, 100, 0, "Freyr's Abundance", "Increases crop yield by 10% and harvest radius by 15%.", "ᚺ",
                (CharacterStatType.ForagingYield, 0.10f, ModifierTier.IncreasedPercent),
                (CharacterStatType.HarvestRadius, 0.15f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("farm_100_dur", Skills.SkillType.Farming, 100, 1, "Indestructible Hoe", "Reduces farming tool durability loss by 15%.", "ᚾ", CharacterStatType.DurabilityEfficiency, 0.15f));

            // 22. Fishing
            Register(KeystoneDefinition.Stat("fish_20_eff", Skills.SkillType.Fishing, 20, 0, "Smooth Reel", "Reduces fishing rod reeling stamina drain by 15%.", "ᚠ", CharacterStatType.FishingStaminaEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("fish_20_swim", Skills.SkillType.Fishing, 20, 1, "Wader's Agility", "Reduces swimming stamina drain by 5%.", "ᚢ", CharacterStatType.SwimEfficiency, 0.05f));

            Register(KeystoneDefinition.Stat("fish_40_eff", Skills.SkillType.Fishing, 40, 0, "Deep Angler", "Reduces reeling stamina drain by 15%.", "ᚦ", CharacterStatType.FishingStaminaEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("fish_40_speed", Skills.SkillType.Fishing, 40, 1, "Water Strider", "Increases swimming speed by 5%.", "ᚨ", CharacterStatType.SwimSpeed, 0.05f));

            Register(KeystoneDefinition.Stat("fish_60_eff", Skills.SkillType.Fishing, 60, 0, "Master of Tides", "Reduces reeling stamina drain by 15%.", "ᚱ", CharacterStatType.FishingStaminaEfficiency, 0.15f));
            Register(KeystoneDefinition.Stat("fish_60_regen", Skills.SkillType.Fishing, 60, 1, "Angler's Rest", "Increases stamina regeneration rate by 8%.", "ᚲ", CharacterStatType.StaminaRegen, 0.08f));

            Register(KeystoneDefinition.Stat("fish_80_eff", Skills.SkillType.Fishing, 80, 0, "Braided Line", "Reduces reeling stamina drain by 20%.", "ᚷ", CharacterStatType.FishingStaminaEfficiency, 0.20f));
            Register(KeystoneDefinition.Stat("fish_80_stam", Skills.SkillType.Fishing, 80, 1, "Deep Reserves", "Increases maximum stamina by +10.", "ᚹ", CharacterStatType.MaxStamina, 10f, ModifierTier.Flat));

            Register(KeystoneDefinition.Stat("fish_100_njord", Skills.SkillType.Fishing, 100, 0, "Njord's Catch", "Reduces fishing reeling stamina drain by 25%.", "ᚺ", CharacterStatType.FishingStaminaEfficiency, 0.25f));
            Register(KeystoneDefinition.Stat("fish_100_weight", Skills.SkillType.Fishing, 100, 1, "Ocean Hauler", "Increases maximum carry weight by +15kg.", "ᚾ", CharacterStatType.MaxCarryWeight, 15f, ModifierTier.Flat));

            // 23. Cooking
            Register(KeystoneDefinition.Stat("cook_20_yield", Skills.SkillType.Cooking, 20, 0, "Thrifty Chef", "Increases cooked food duplicate yield by 6%.", "ᚠ", CharacterStatType.CookingYield, 0.06f));
            Register(KeystoneDefinition.Stat("cook_20_dur", Skills.SkillType.Cooking, 20, 1, "Hearty Meal", "Increases food duration by 5% (food burns slower).", "ᚢ", CharacterStatType.FoodDurationMultiplier, 0.05f));

            Register(KeystoneDefinition.Stat("cook_40_health", Skills.SkillType.Cooking, 40, 0, "Savory Stews", "Increases food health effectiveness by 6%.", "ᚦ", CharacterStatType.FoodHealthEffectiveness, 0.06f));
            Register(KeystoneDefinition.Stat("cook_40_dur", Skills.SkillType.Cooking, 40, 1, "Slow Roasted", "Increases food duration by 5%.", "ᚨ", CharacterStatType.FoodDurationMultiplier, 0.05f));

            Register(KeystoneDefinition.Stat("cook_60_yield", Skills.SkillType.Cooking, 60, 0, "Master Feast", "Increases cooked food duplicate yield by 8%.", "ᚱ", CharacterStatType.CookingYield, 0.08f));
            Register(KeystoneDefinition.Stat("cook_60_stam", Skills.SkillType.Cooking, 60, 1, "Energizing Diet", "Increases food stamina effectiveness by 6%.", "ᚲ", CharacterStatType.FoodStaminaEffectiveness, 0.06f));

            Register(KeystoneDefinition.Stat("cook_80_dur", Skills.SkillType.Cooking, 80, 0, "Sustained Vitality", "Increases food duration by 8%.", "ᚷ", CharacterStatType.FoodDurationMultiplier, 0.08f));
            Register(KeystoneDefinition.Stat("cook_80_yield", Skills.SkillType.Cooking, 80, 1, "Bountiful Skillet", "Increases cooked food duplicate yield by 8%.", "ᚹ", CharacterStatType.CookingYield, 0.08f));

            Register(KeystoneDefinition.MultiStat("cook_100_feast", Skills.SkillType.Cooking, 100, 0, "Feast of Valhalla", "Increases food duration by 10% and food cooking yield by 6%.", "ᚺ",
                (CharacterStatType.FoodDurationMultiplier, 0.10f, ModifierTier.IncreasedPercent),
                (CharacterStatType.CookingYield, 0.06f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.MultiStat("cook_100_regen", Skills.SkillType.Cooking, 100, 1, "Nourished Body", "Increases health and stamina regeneration rates by 5%.", "ᚾ",
                (CharacterStatType.HealthRegen, 0.05f, ModifierTier.IncreasedPercent),
                (CharacterStatType.StaminaRegen, 0.05f, ModifierTier.IncreasedPercent)));

            // 24. Crafting
            Register(KeystoneDefinition.Stat("craft_20_eff", Skills.SkillType.Crafting, 20, 0, "Efficient Hammer", "Reduces building hammer stamina cost by 12%.", "ᚠ", CharacterStatType.ToolStaminaEfficiency, 0.12f));
            Register(KeystoneDefinition.Stat("craft_20_yield", Skills.SkillType.Crafting, 20, 1, "Apprentice Smith", "Increases crafting duplicate yield for ammo and consumables by 5%.", "ᚢ", CharacterStatType.CraftingYield, 0.05f));

            Register(KeystoneDefinition.Stat("craft_40_dur", Skills.SkillType.Crafting, 40, 0, "Master Smith", "Reduces equipment durability wear by 12%.", "ᚦ", CharacterStatType.DurabilityEfficiency, 0.12f));
            Register(KeystoneDefinition.Stat("craft_40_yield", Skills.SkillType.Crafting, 40, 1, "Resourceful Artisan", "Increases crafting duplicate yield by 5%.", "ᚨ", CharacterStatType.CraftingYield, 0.05f));

            Register(KeystoneDefinition.Stat("craft_60_eff", Skills.SkillType.Crafting, 60, 0, "Agile Builder", "Reduces building hammer stamina cost by 12%.", "ᚱ", CharacterStatType.ToolStaminaEfficiency, 0.12f));
            Register(KeystoneDefinition.Stat("craft_60_yield", Skills.SkillType.Crafting, 60, 1, "Efficient Workshop", "Increases crafting duplicate yield by 6%.", "ᚲ", CharacterStatType.CraftingYield, 0.06f));

            Register(KeystoneDefinition.Stat("craft_80_dur", Skills.SkillType.Crafting, 80, 0, "Tempered Alloy", "Reduces equipment durability decay by 12%.", "ᚷ", CharacterStatType.DurabilityEfficiency, 0.12f));
            Register(KeystoneDefinition.Stat("craft_80_weight", Skills.SkillType.Crafting, 80, 1, "Pack Mule", "Increases maximum carry weight by +15kg.", "ᚹ", CharacterStatType.MaxCarryWeight, 15f, ModifierTier.Flat));

            Register(KeystoneDefinition.MultiStat("craft_100_master", Skills.SkillType.Crafting, 100, 0, "Wayland's Art", "Increases crafting yield by 10% and durability efficiency by 15%.", "ᚺ",
                (CharacterStatType.CraftingYield, 0.10f, ModifierTier.IncreasedPercent),
                (CharacterStatType.DurabilityEfficiency, 0.15f, ModifierTier.IncreasedPercent)));
            Register(KeystoneDefinition.Stat("craft_100_eff", Skills.SkillType.Crafting, 100, 1, "Grand Architect", "Reduces building hammer stamina cost by 10%.", "ᚾ", CharacterStatType.ToolStaminaEfficiency, 0.10f));
        }

        private static List<KeystoneDefinition> GenerateFallbackKeystones(Skills.SkillType skillType, int milestone)
        {
            string prefix = skillType.ToString().ToLower();
            return new List<KeystoneDefinition>
            {
                KeystoneDefinition.Stat($"{prefix}_{milestone}_a", skillType, milestone, 0, $"Rite of Mastery {milestone}", $"Increases mastery efficiency for {skillType} by 10%.", "ᚠ", CharacterStatType.AttackStaminaEfficiency, 0.10f),
                KeystoneDefinition.Stat($"{prefix}_{milestone}_b", skillType, milestone, 1, $"Rite of Power {milestone}", $"Increases melee damage by 5%.", "ᚢ", CharacterStatType.DamageMelee, 0.05f)
            };
        }
    }
}
