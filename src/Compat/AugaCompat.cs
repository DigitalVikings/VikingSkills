using System;
using HarmonyLib;

namespace VikingSkills.Compat
{
    public static class AugaCompat
    {
        public const string AugaGuid1 = "randyknapp.mods.auga";
        public const string AugaGuid2 = "org.bepinex.plugins.auga";

        public static bool IsAugaLoaded =>
            BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(AugaGuid1) ||
            BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(AugaGuid2) ||
            Type.GetType("Auga.API, Auga") != null;

        public static void Initialize(Harmony harmony)
        {
            if (!IsAugaLoaded) return;

            try
            {
                // Exclude VikingSkills from Auga's aggressive AutoRestyle so our handcrafted UI layout, fonts, and materials are preserved.
                // Auga Skills tab interception is handled centrally by VikingLib's AugaUIBridge -> VikingHubRegistry.
                Type apiType = AccessTools.TypeByName("Auga.API");
                if (apiType != null)
                {
                    var excludeMethod = AccessTools.Method(apiType, "AutoRestyle_Exclude", new[] { typeof(string) });
                    excludeMethod?.Invoke(null, new object[] { VikingSkillsPlugin.ModGuid });
                    VikingSkillsPlugin.Log.LogInfo("Excluded VikingSkills from Auga AutoRestyle.");
                }
            }
            catch (Exception ex)
            {
                VikingSkillsPlugin.Log.LogError($"Failed to initialize Auga compatibility: {ex}");
            }
        }
    }
}
