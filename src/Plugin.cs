using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using VikingLib.UI.Hub;
using VikingSkills.Configuration;
using VikingSkills.UI;

namespace VikingSkills
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency("com.jotunn.jotunn", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(Compat.AugaCompat.AugaGuid1, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(Compat.AugaCompat.AugaGuid2, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(VikingLib.VikingLibPlugin.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
    public class VikingSkillsPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.digitalvikings.vikingskills";
        public const string ModName = "VikingSkills";
        public const string ModVersion = "0.2.0";

        internal static ManualLogSource Log = null!;
        private Harmony _harmony = null!;
        private SkillsHubTab? _skillsHubTab;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{ModName} v{ModVersion} initializing...");

            // Initialize configuration
            ModConfig.Init(Config);

            // Configure Vanilla Skill Scaling
            if (!ModConfig.EnableVanillaSkillScaling.Value)
            {
                VikingLib.SkillSystem.VanillaSkillIntegration.DisableAll();
                Log.LogInfo("Vanilla skill scaling neutralized. Pure milestone & Keystone progression active.");
            }
            else
            {
                VikingLib.SkillSystem.VanillaSkillIntegration.ResetToDefaults();
                Log.LogInfo("Vanilla skill scaling active alongside Keystones.");
            }

            // Initialize KeystoneStatAdapter & status effects
            Services.KeystoneStatAdapter.Init();

            // Initialize SkillProgressionManager for death penalties and progression curves
            Services.SkillProgressionManager.Init();

            // Register Skills tab into VikingLib's unified VikingHub Character UI
            _skillsHubTab = new SkillsHubTab();
            SyncHubTabRegistration();
            ModConfig.ModEnabled.SettingChanged += OnModEnabledChanged;

            // Apply Harmony patches
            _harmony = new Harmony(ModGuid);
            _harmony.PatchAll();

            // Initialize Auga compatibility if Auga is present
            if (Compat.AugaCompat.IsAugaLoaded)
            {
                Compat.AugaCompat.Initialize(_harmony);
            }

            Log.LogInfo($"{ModName} v{ModVersion} loaded successfully.");

            // Listen for server synchronization events if Jotunn is active
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.jotunn.jotunn"))
            {
                HookJotunnSync();
            }
        }

        private void OnModEnabledChanged(object sender, EventArgs args)
        {
            SyncHubTabRegistration();
        }

        private void SyncHubTabRegistration()
        {
            if (_skillsHubTab == null) return;

            if (ModConfig.ModEnabled.Value)
            {
                VikingHubRegistry.RegisterTab(_skillsHubTab);
            }
            else
            {
                VikingHubRegistry.UnregisterTab(SkillsHubTab.TabId);
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void HookJotunnSync()
        {
            Jotunn.Managers.SynchronizationManager.OnConfigurationSynchronized += (sender, args) =>
            {
                Log.LogInfo("Server configuration synchronized successfully.");
            };
        }

        private void OnDestroy()
        {
            if (ModConfig.ModEnabled != null)
            {
                ModConfig.ModEnabled.SettingChanged -= OnModEnabledChanged;
            }

            VikingHubRegistry.UnregisterTab(SkillsHubTab.TabId);
            _harmony?.UnpatchSelf();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    public static class PlayerSpawnPatch
    {
        [HarmonyPostfix]
        private static void OnPlayerSpawned(Player __instance)
        {
            if (__instance == Player.m_localPlayer)
            {
                Services.KeystoneStatAdapter.SyncPlayer(__instance);
            }
        }
    }
}
