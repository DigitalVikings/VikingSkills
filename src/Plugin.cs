using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using VikingSkills.Configuration;

namespace VikingSkills
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency("com.jotunn.jotunn", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(VikingLib.VikingLibPlugin.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
    public class VikingSkillsPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.digitalvikings.vikingskills";
        public const string ModName = "VikingSkills";
        public const string ModVersion = "0.1.1";

        internal static ManualLogSource Log = null!;
        private Harmony _harmony = null!;

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

            // Apply Harmony patches
            _harmony = new Harmony(ModGuid);
            _harmony.PatchAll();

            Log.LogInfo($"{ModName} v{ModVersion} loaded successfully.");

            // Listen for server synchronization events if Jotunn is active
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.jotunn.jotunn"))
            {
                HookJotunnSync();
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
