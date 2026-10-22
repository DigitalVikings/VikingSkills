using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace VikingSkills.Keystones
{
    public static class KeystoneIconManager
    {
        public const string PlaceholderIconName = "keystone_placeholder";

        private static readonly Dictionary<string, Sprite> _spriteCache = new(StringComparer.OrdinalIgnoreCase);

        public static string GetSkillFolderName(Skills.SkillType skillType)
        {
            return skillType switch
            {
                Skills.SkillType.Swords => "swords",
                Skills.SkillType.Axes => "axes",
                Skills.SkillType.Clubs => "clubs",
                Skills.SkillType.Polearms => "polearms",
                Skills.SkillType.Spears => "spears",
                Skills.SkillType.Knives => "knives",
                Skills.SkillType.Unarmed => "unarmed",
                Skills.SkillType.Bows => "bows",
                Skills.SkillType.Crossbows => "crossbows",
                Skills.SkillType.Blocking => "blocking",
                Skills.SkillType.Dodge => "dodge",
                Skills.SkillType.ElementalMagic => "elemental_magic",
                Skills.SkillType.BloodMagic => "blood_magic",
                Skills.SkillType.Run => "run",
                Skills.SkillType.Jump => "jump",
                Skills.SkillType.Sneak => "sneak",
                Skills.SkillType.Swim => "swim",
                Skills.SkillType.Ride => "ride",
                Skills.SkillType.WoodCutting => "woodcutting",
                Skills.SkillType.Pickaxes => "pickaxes",
                Skills.SkillType.Farming => "farming",
                Skills.SkillType.Fishing => "fishing",
                Skills.SkillType.Cooking => "cooking",
                Skills.SkillType.Crafting => "crafting",
                _ => skillType.ToString().ToLowerInvariant()
            };
        }

        public static string GetPredictableIconPath(Skills.SkillType skillType, int milestone, int slotIndex)
        {
            string folder = GetSkillFolderName(skillType);
            char slotChar = (char)('a' + Math.Max(0, slotIndex));
            return $"{folder}/{folder}_{milestone}{slotChar}";
        }

        public static bool HasDedicatedIcon(string keystoneId)
        {
            var def = KeystoneRegistry.GetById(keystoneId);
            if (def == null) return false;
            return HasDedicatedIcon(def);
        }

        public static bool HasDedicatedIcon(KeystoneDefinition keystone)
        {
            if (keystone == null) return false;

            if (!string.IsNullOrEmpty(keystone.IconName))
            {
                if (SpriteExists(keystone.IconName)) return true;
            }

            string predictable = GetPredictableIconPath(keystone.SkillType, keystone.MilestoneLevel, keystone.SlotIndex);
            return SpriteExists(predictable);
        }

        public static bool SpriteExists(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return false;
            if (_spriteCache.ContainsKey(relativePath)) return true;

            try
            {
                string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                string diskPath = Path.Combine(pluginDir, "assets", "keystones", relativePath.Replace('/', Path.DirectorySeparatorChar) + ".png");
                if (File.Exists(diskPath)) return true;

                string currentDir = Directory.GetCurrentDirectory();
                string devPath = Path.Combine(currentDir, "assets", "keystones", relativePath.Replace('/', Path.DirectorySeparatorChar) + ".png");
                if (File.Exists(devPath)) return true;

                var asm = Assembly.GetExecutingAssembly();
                string resName = $"VikingSkills.assets.keystones.{relativePath.Replace('/', '.').Replace('\\', '.')}.png";
                using var stream = asm.GetManifestResourceStream(resName);
                if (stream != null) return true;
            }
            catch
            {
                // Ignored
            }

            return false;
        }

        public static Sprite? GetIcon(KeystoneDefinition keystone, Player? player)
        {
            if (keystone == null) return null;

            // 1. Explicit IconName override if provided
            if (!string.IsNullOrEmpty(keystone.IconName))
            {
                var sprite = LoadSprite(keystone.IconName);
                if (sprite != null) return sprite;
            }

            // 2. Predictable convention path: {skill}/{skill}_{milestone}{slot}
            string predictable = GetPredictableIconPath(keystone.SkillType, keystone.MilestoneLevel, keystone.SlotIndex);
            var bespokeSprite = LoadSprite(predictable);
            if (bespokeSprite != null) return bespokeSprite;

            // 3. Native Valheim ObjectDB Item Prefab (if specified)
            if (!string.IsNullOrEmpty(keystone.IconPrefab) && ObjectDB.instance != null)
            {
                var prefab = ObjectDB.instance.GetItemPrefab(keystone.IconPrefab);
                if (prefab != null)
                {
                    var drop = prefab.GetComponent<ItemDrop>();
                    if (drop != null && drop.m_itemData != null)
                    {
                        var icon = drop.m_itemData.GetIcon();
                        if (icon != null) return icon;
                    }
                }
            }

            // 4. Common placeholder icon indicating artwork is in progress
            var placeholder = LoadSprite(PlaceholderIconName);
            if (placeholder != null) return placeholder;

            // 5. Fallback to native skill icon if placeholder cannot be loaded
            if (player != null)
            {
                var skills = player.GetSkills();
                if (skills != null)
                {
                    foreach (var s in skills.GetSkillList())
                    {
                        if (s.m_info.m_skill == keystone.SkillType && s.m_info.m_icon != null)
                        {
                            return s.m_info.m_icon;
                        }
                    }
                }
            }

            return null;
        }

        public static Sprite? LoadSprite(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;

            if (_spriteCache.TryGetValue(relativePath, out var cached))
            {
                return cached;
            }

            byte[]? data = null;

            // 1. Try file from disk in BepInEx plugins folder or dev folder
            try
            {
                string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                string diskPath = Path.Combine(pluginDir, "assets", "keystones", relativePath.Replace('/', Path.DirectorySeparatorChar) + ".png");
                if (File.Exists(diskPath))
                {
                    data = File.ReadAllBytes(diskPath);
                }
                else
                {
                    string currentDir = Directory.GetCurrentDirectory();
                    string devPath = Path.Combine(currentDir, "assets", "keystones", relativePath.Replace('/', Path.DirectorySeparatorChar) + ".png");
                    if (File.Exists(devPath))
                    {
                        data = File.ReadAllBytes(devPath);
                    }
                }
            }
            catch (Exception)
            {
                // Fall through to embedded resource
            }

            // 2. Try embedded manifest resource
            if (data == null)
            {
                try
                {
                    var asm = Assembly.GetExecutingAssembly();
                    string resName = $"VikingSkills.assets.keystones.{relativePath.Replace('/', '.').Replace('\\', '.')}.png";
                    using var stream = asm.GetManifestResourceStream(resName);
                    if (stream != null)
                    {
                        using var ms = new MemoryStream();
                        stream.CopyTo(ms);
                        data = ms.ToArray();
                    }
                }
                catch (Exception)
                {
                    // Ignored
                }
            }

            if (data != null && data.Length > 0)
            {
                var tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                if (ImageConversion.LoadImage(tex, data))
                {
                    var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    _spriteCache[relativePath] = sprite;
                    return sprite;
                }
            }

            return null;
        }
    }
}
