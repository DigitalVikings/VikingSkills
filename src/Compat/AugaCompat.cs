using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using VikingSkills.Configuration;
using VikingSkills.UI;

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
                // Exclude VikingSkills from Auga's aggressive AutoRestyle so our handcrafted UI layout, fonts, and materials are preserved
                Type apiType = AccessTools.TypeByName("Auga.API");
                if (apiType != null)
                {
                    var excludeMethod = AccessTools.Method(apiType, "AutoRestyle_Exclude", new[] { typeof(string) });
                    excludeMethod?.Invoke(null, new object[] { VikingSkillsPlugin.ModGuid });
                    VikingSkillsPlugin.Log.LogInfo("Excluded VikingSkills from Auga AutoRestyle.");
                }

                // Patch AugaTabController.SelectTab to intercept the Skills tab
                Type tabControllerType = AccessTools.TypeByName("AugaUnity.AugaTabController");
                if (tabControllerType != null)
                {
                    var selectTabMethod = AccessTools.Method(tabControllerType, "SelectTab", new[] { typeof(int) });
                    if (selectTabMethod != null)
                    {
                        var prefix = AccessTools.Method(typeof(AugaCompat), nameof(SelectTabPrefix));
                        harmony.Patch(selectTabMethod, prefix: new HarmonyMethod(prefix));
                        VikingSkillsPlugin.Log.LogInfo("Auga AugaTabController.SelectTab successfully patched to hijack Skills tab.");
                    }
                    else
                    {
                        VikingSkillsPlugin.Log.LogWarning("Auga AugaTabController.SelectTab method not found.");
                    }
                }
                else
                {
                    VikingSkillsPlugin.Log.LogWarning("Auga AugaUnity.AugaTabController type not found.");
                }
            }
            catch (Exception ex)
            {
                VikingSkillsPlugin.Log.LogError($"Failed to initialize Auga compatibility: {ex}");
            }
        }

        private static bool SelectTabPrefix(object __instance, int index, bool __runOriginal)
        {
            if (!__runOriginal || !ModConfig.ModEnabled.Value || Player.m_localPlayer == null)
            {
                return true;
            }

            try
            {
                var traverse = Traverse.Create(__instance);
                var tabContents = traverse.Field("TabContents").GetValue<List<GameObject>>();
                if (tabContents == null || index < 0 || index >= tabContents.Count)
                {
                    return true;
                }

                GameObject content = tabContents[index];
                bool isSkillsTab = false;

                if (content != null)
                {
                    string contentName = content.name;
                    if (contentName.IndexOf("skill", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        isSkillsTab = true;
                    }
                    else if (content.GetComponent("SkillsPanelController") != null || content.GetComponent<SkillsDialog>() != null)
                    {
                        isSkillsTab = true;
                    }
                }

                if (!isSkillsTab)
                {
                    var tabButtons = traverse.Field("TabButtons").GetValue<IList>();
                    if (tabButtons != null && index >= 0 && index < tabButtons.Count)
                    {
                        if (tabButtons[index] is Component btnComp && btnComp.gameObject.name.IndexOf("skill", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            isSkillsTab = true;
                        }
                    }
                }

                if (!isSkillsTab)
                {
                    var tabTitles = traverse.Field("TabTitles").GetValue<IList>();
                    if (tabTitles != null && index >= 0 && index < tabTitles.Count)
                    {
                        if (tabTitles[index] is TMP_Text tmp)
                        {
                            string titleText = tmp.text ?? "";
                            if (titleText.IndexOf("skill", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                titleText.IndexOf("$inventory_skills", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                isSkillsTab = true;
                            }
                        }
                    }
                }

                if (isSkillsTab)
                {
                    PanoramicSkillsDialog.Show(Player.m_localPlayer);
                    return false; // Suppress Auga's native small skill panel
                }
            }
            catch (Exception ex)
            {
                VikingSkillsPlugin.Log.LogError($"Error in AugaCompat.SelectTabPrefix: {ex}");
            }

            return true;
        }
    }
}
