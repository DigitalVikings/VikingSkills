using HarmonyLib;
using UnityEngine;
using VikingSkills.Configuration;
using VikingSkills.UI;

namespace VikingSkills.Patches
{
    [HarmonyPatch(typeof(InventoryGui))]
    public static class InventoryGuiPatch
    {
        [HarmonyPatch(nameof(InventoryGui.OnOpenSkills))]
        [HarmonyPrefix]
        private static bool OnOpenSkillsPrefix(InventoryGui __instance, bool __runOriginal)
        {
            if (!__runOriginal || !ModConfig.ModEnabled.Value)
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

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static bool UpdatePrefix(InventoryGui __instance, bool __runOriginal)
        {
            if (!__runOriginal) return true;

            if (PanoramicSkillsDialog.IsOpen || PanoramicSkillsDialog.WasClosedThisFrame)
            {
                if (ZInput.GetButtonDown("JoyButtonB") || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
                {
                    if (PanoramicSkillsDialog.IsOpen)
                    {
                        PanoramicSkillsDialog.Hide();
                    }
                    ZInput.ResetButtonStatus("JoyButtonB");
                    return false; // Prevent InventoryGui from closing the entire inventory screen
                }
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
