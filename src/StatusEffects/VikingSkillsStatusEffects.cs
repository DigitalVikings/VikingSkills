using VikingLib.Stats;
using VikingLib.StatusEffects;

namespace VikingSkills.StatusEffects
{
    public static class VikingSkillsStatusEffects
    {
        public const string FlurryId = "SE_VS_Flurry";
        public const string WindrunnerId = "SE_VS_Windrunner";
        public const string AdrenalineId = "SE_VS_Adrenaline";
        public const string ShadowveilId = "SE_VS_Shadowveil";
        public const string StoneGuardId = "SE_VS_StoneGuard";

        private static bool _initialized = false;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            // 1. Flurry: On kill buff granting attack speed & move speed
            StatusEffectBuilder.Create(FlurryId)
                .WithName("Flurry")
                .WithTooltip("Adrenaline surges upon slaying a foe! +15% attack speed and +10% movement speed.")
                .WithDuration(8.0f)
                .WithMaxStacks(1, StackPolicy.RefreshDurationOnly)
                .WithIncreasedModifier(CharacterStats.AttackSpeed, 0.15f)
                .WithIncreasedModifier(CharacterStats.RunSpeed, 0.10f)
                .WithIncreasedModifier(CharacterStats.JogSpeed, 0.10f)
                .BuildAndRegister();

            // 2. Windrunner: Movement speed upon dodging an attack
            StatusEffectBuilder.Create(WindrunnerId)
                .WithName("Windrunner")
                .WithTooltip("Momentum gained from evading an attack! +15% sprint and jog speed.")
                .WithDuration(6.0f)
                .WithMaxStacks(1, StackPolicy.RefreshDurationOnly)
                .WithIncreasedModifier(CharacterStats.RunSpeed, 0.15f)
                .WithIncreasedModifier(CharacterStats.JogSpeed, 0.15f)
                .BuildAndRegister();

            // 3. Adrenaline Surge: Taking damage boosts damage and stamina recovery
            StatusEffectBuilder.Create(AdrenalineId)
                .WithName("Adrenaline Surge")
                .WithTooltip("Wounds ignite fury! +15% melee damage and +25% stamina regeneration.")
                .WithDuration(5.0f)
                .WithMaxStacks(1, StackPolicy.RefreshDurationOnly)
                .WithIncreasedModifier(CharacterStats.DamageMelee, 0.15f)
                .WithIncreasedModifier(CharacterStats.StaminaRegen, 0.25f)
                .BuildAndRegister();

            // 4. Shadowveil: Stealth kill grants concealment and crouch speed
            StatusEffectBuilder.Create(ShadowveilId)
                .WithName("Shadowveil")
                .WithTooltip("Slain quarry masks your presence. +40% concealment and +25% crouch speed.")
                .WithDuration(8.0f)
                .WithMaxStacks(1, StackPolicy.RefreshDurationOnly)
                .WithIncreasedModifier(CharacterStats.Concealment, 0.40f)
                .WithIncreasedModifier(CharacterStats.CrouchSpeed, 0.25f)
                .BuildAndRegister();

            // 5. Stone Guard: Temporary physical resistance after rolling or heavy hits
            StatusEffectBuilder.Create(StoneGuardId)
                .WithName("Stone Guard")
                .WithTooltip("Braced against impact! +15% physical damage resistance.")
                .WithDuration(4.0f)
                .WithMaxStacks(1, StackPolicy.RefreshDurationOnly)
                .WithMultiplierModifier(CharacterStats.ResistancePhysical, 0.85f)
                .BuildAndRegister();
        }
    }
}
