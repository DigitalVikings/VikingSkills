using HarmonyLib;
using VikingSkills.Configuration;
using VikingSkills.UI;

namespace VikingSkills.Patches
{
    [HarmonyPatch(typeof(InventoryGui))]
    public static class InventoryGuiPatch
    {
        [HarmonyPatch(nameof(InventoryGui.OnOpenSkills))]
        [HarmonyPrefix]
        private static bool OnOpenSkillsPrefix(InventoryGui __instance)
        {
            if (!ModConfig.ModEnabled.Value)
            {
                return true;
            }

            if (Player.m_localPlayer != null)
            {
                PanoramicSkillsDialog.Show(Player.m_localPlayer);
                return false; // Suppress vanilla small dialog
            }

            return true;
        }

        [HarmonyPatch(nameof(InventoryGui.Hide))]
        [HarmonyPostfix]
        private static void OnHidePostfix()
        {
            PanoramicSkillsDialog.Hide();
        }

        [HarmonyPatch(nameof(InventoryGui.IsSkillsPanelOpen), MethodType.Getter)]
        [HarmonyPostfix]
        private static void IsSkillsPanelOpenPostfix(ref bool __result)
        {
            if (PanoramicSkillsDialog.IsOpen)
            {
                __result = true;
            }
        }
    }
}
