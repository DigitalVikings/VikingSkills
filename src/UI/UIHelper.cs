using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VikingSkills.Configuration;

namespace VikingSkills.UI
{
    public static class UIHelper
    {
        private static TMP_FontAsset? _cachedFont;

        // Valheim-Brown / Norse Amber Color Palette
        public static readonly Color AmberGoldConduit = new Color(0.96f, 0.68f, 0.20f, 1.0f);
        public static readonly Color AmberGoldGlow = new Color(1.00f, 0.82f, 0.35f, 0.95f);
        public static readonly Color BrightBlueConduit = AmberGoldConduit; // Redirect old references to warm runic amber
        public static readonly Color BrightBlueGlow = AmberGoldGlow;
        public static readonly Color GoldAccent = new Color(0.95f, 0.78f, 0.35f, 1.0f);
        public static readonly Color MutedText = new Color(0.72f, 0.66f, 0.58f, 1.0f);
        public static readonly Color LightText = new Color(0.95f, 0.92f, 0.88f, 1.0f);

        // Core Valheim Theme Constants
        public static readonly Color ValheimDarkWood = new Color(0.08f, 0.06f, 0.05f, 0.98f);
        public static readonly Color ValheimPanelWood = new Color(0.12f, 0.09f, 0.07f, 0.98f);
        public static readonly Color ValheimCardWood = new Color(0.14f, 0.11f, 0.08f, 0.96f);
        public static readonly Color ValheimBronzeBorder = new Color(0.42f, 0.32f, 0.20f, 0.60f);
        public static readonly Color ValheimGoldTrim = new Color(0.85f, 0.68f, 0.35f, 0.85f);
        public static readonly Color ParchmentText = new Color(0.95f, 0.92f, 0.88f, 1.0f);
        public static readonly Color MutedParchment = new Color(0.72f, 0.66f, 0.58f, 0.90f);
        public static readonly Color NorseGoldText = new Color(0.96f, 0.76f, 0.32f, 1.0f);

        static UIHelper()
        {
            UITextureEngine.EnsureInitialized();
        }

        public static TMP_FontAsset? GetValheimFont()
        {
            if (_cachedFont != null) return _cachedFont;

            // 1. Try finding Averia or Norse font from loaded Resources
            try
            {
                var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                if (fonts != null && fonts.Length > 0)
                {
                    // Prioritize Valheim's standard Averia fonts
                    foreach (var f in fonts)
                    {
                        if (f != null && f.name.IndexOf("Averia", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _cachedFont = f;
                            break;
                        }
                    }

                    // Otherwise take any valid game font that is not LiberationSans
                    if (_cachedFont == null)
                    {
                        foreach (var f in fonts)
                        {
                            if (f != null && !f.name.Contains("LiberationSans"))
                            {
                                _cachedFont = f;
                                break;
                            }
                        }
                    }

                    if (_cachedFont == null && fonts.Length > 0)
                    {
                        _cachedFont = fonts[0];
                    }
                }
            }
            catch
            {
                // Fallback gracefully
            }

            // 2. Try getting font from vanilla SkillsDialog
            if (_cachedFont == null && InventoryGui.instance != null && InventoryGui.instance.m_skillsDialog != null)
            {
                var totalText = InventoryGui.instance.m_skillsDialog.m_totalSkillText;
                if (totalText != null && totalText.font != null)
                {
                    _cachedFont = totalText.font;
                }
            }

            // 3. Fallback to any TMP_Text in InventoryGui
            if (_cachedFont == null && InventoryGui.instance != null)
            {
                var tmp = InventoryGui.instance.GetComponentInChildren<TMP_Text>(true);
                if (tmp != null && tmp.font != null)
                {
                    _cachedFont = tmp.font;
                }
            }

            // Ensure TextMeshPro default fallback doesn't look for LiberationSans SDF
            if (_cachedFont != null)
            {
                try
                {
                    if (TMP_Settings.defaultFontAsset == null)
                    {
                        TMP_Settings.defaultFontAsset = _cachedFont;
                    }
                }
                catch
                {
                }
            }

            return _cachedFont;
        }

        public static Sprite GetWhiteSprite()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.WhitePixel;
        }

        public static Sprite GetHeaderBanner()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.HeaderBanner;
        }

        public static Sprite GetTabPlaque()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.TabPlaque;
        }

        public static Sprite GetGlassPanel()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.GlassPanel;
        }

        public static Sprite GetGlassCard()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.GlassCard;
        }

        public static Sprite GetSoftShadow()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.SoftShadow;
        }

        public static Sprite GetSocketRing()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.SocketRing;
        }

        public static Sprite GetSocketEmpty()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.SocketInnerEmpty;
        }

        public static Sprite GetRadialGlow()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.RadialGlow;
        }

        public static Sprite GetConduitTrack()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.ConduitTrack;
        }

        public static Sprite GetConduitFill()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.ConduitFill;
        }

        public static Sprite GetNodeCircle()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.NodeCircle;
        }

        public static Sprite GetNodeDecade()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.NodeDecade;
        }

        public static Sprite GetCircleMask()
        {
            UITextureEngine.EnsureInitialized();
            return UITextureEngine.CircleMask;
        }

        private static Sprite? _cachedReturnArrow;

        public static Sprite GetRunicReturnArrow()
        {
            if (_cachedReturnArrow != null) return _cachedReturnArrow;

            try
            {
                byte[]? data = null;
                string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                string diskPath = Path.Combine(pluginDir, "assets", "ui", "runic_return_arrow.png");
                if (File.Exists(diskPath))
                {
                    data = File.ReadAllBytes(diskPath);
                }
                else
                {
                    string currentDir = Directory.GetCurrentDirectory();
                    string devPath = Path.Combine(currentDir, "assets", "ui", "runic_return_arrow.png");
                    if (File.Exists(devPath))
                    {
                        data = File.ReadAllBytes(devPath);
                    }
                    else
                    {
                        var asm = Assembly.GetExecutingAssembly();
                        using var stream = asm.GetManifestResourceStream("VikingSkills.assets.ui.runic_return_arrow.png");
                        if (stream != null)
                        {
                            using var ms = new MemoryStream();
                            stream.CopyTo(ms);
                            data = ms.ToArray();
                        }
                    }
                }

                if (data != null && data.Length > 0)
                {
                    var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(data))
                    {
                        tex.name = "RunicReturnArrow";
                        _cachedReturnArrow = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        return _cachedReturnArrow;
                    }
                }
            }
            catch (Exception ex)
            {
                VikingSkillsPlugin.Log?.LogWarning($"[UIHelper] Failed loading runic return arrow asset: {ex.Message}");
            }

            // Fallback to procedural texture
            UITextureEngine.EnsureInitialized();
            _cachedReturnArrow = UITextureEngine.RunicReturnArrow;
            return _cachedReturnArrow;
        }

        /// <summary>
        /// Creates a standardized Dark RPG return / go-back button with the runic return arrow icon and localized label.
        /// </summary>
        public static GameObject CreateReturnButton(Transform parent, Vector2 anchoredPosition, Vector2 sizeDelta,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, string label, Action onClick, TMP_FontAsset? font)
        {
            var btnObj = new GameObject("ReturnButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var img = btnObj.GetComponent<Image>();
            img.sprite = GetGlassCard();
            img.type = Image.Type.Sliced;
            img.color = new Color(0.16f, 0.12f, 0.09f, 0.96f);

            var btn = btnObj.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.16f, 0.12f, 0.09f, 0.96f);
            colors.highlightedColor = new Color(0.26f, 0.20f, 0.14f, 1f);
            colors.pressedColor = new Color(0.12f, 0.09f, 0.07f, 1f);
            btn.colors = colors;

            // Runic Return Arrow Icon (Masked inside circle)
            var maskObj = new GameObject("IconMask", typeof(RectTransform), typeof(Image), typeof(Mask));
            maskObj.transform.SetParent(btnObj.transform, false);
            var mRect = maskObj.GetComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0f, 0.5f);
            mRect.anchorMax = new Vector2(0f, 0.5f);
            mRect.pivot = new Vector2(0f, 0.5f);
            mRect.anchoredPosition = new Vector2(10f, 0f);
            mRect.sizeDelta = new Vector2(30f, 30f);
            var mImg = maskObj.GetComponent<Image>();
            mImg.sprite = GetCircleMask();
            var mask = maskObj.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(maskObj.transform, false);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            var iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = GetRunicReturnArrow();
            iconImg.preserveAspect = true;

            // Label Text (Warm Ivory Parchment)
            var textObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);
            var tRect = textObj.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = new Vector2(48f, 0f);
            tRect.offsetMax = new Vector2(-10f, 0f);
            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = $"<b><color=#F4E8D4>{label}</color></b>";
            tmp.fontSize = 13.5f;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            btn.onClick.AddListener(() =>
            {
                onClick();
                PlaySfx("sfx_gui_button");
            });

            return btnObj;
        }

        /// <summary>
        /// Creates a compact circular or square icon-only button with the runic return arrow.
        /// </summary>
        public static GameObject CreateRunicIconButton(Transform parent, Vector2 anchoredPosition, Vector2 sizeDelta,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Action onClick)
        {
            var btnObj = new GameObject("RunicIconBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var img = btnObj.GetComponent<Image>();
            img.sprite = GetGlassCard();
            img.type = Image.Type.Sliced;
            img.color = new Color(0.16f, 0.12f, 0.09f, 0.96f);

            var btn = btnObj.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.16f, 0.12f, 0.09f, 0.96f);
            colors.highlightedColor = new Color(0.26f, 0.20f, 0.14f, 1f);
            colors.pressedColor = new Color(0.12f, 0.09f, 0.07f, 1f);
            btn.colors = colors;

            // Runic Return Arrow Icon (Masked inside circle)
            var maskObj = new GameObject("IconMask", typeof(RectTransform), typeof(Image), typeof(Mask));
            maskObj.transform.SetParent(btnObj.transform, false);
            var mRect = maskObj.GetComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0.5f, 0.5f);
            mRect.anchorMax = new Vector2(0.5f, 0.5f);
            mRect.pivot = new Vector2(0.5f, 0.5f);
            mRect.sizeDelta = new Vector2(sizeDelta.x - 6f, sizeDelta.y - 6f);
            var mImg = maskObj.GetComponent<Image>();
            mImg.sprite = GetCircleMask();
            var mask = maskObj.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(maskObj.transform, false);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            var iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = GetRunicReturnArrow();
            iconImg.preserveAspect = true;

            btn.onClick.AddListener(() =>
            {
                onClick();
                PlaySfx("sfx_gui_button");
            });

            return btnObj;
        }

        public static void PlaySfx(string sfxName)
        {
            if (!ModConfig.EnableAudioEffects.Value) return;

            try
            {
                if (ZNetScene.instance != null && Player.m_localPlayer != null)
                {
                    var prefab = ZNetScene.instance.GetPrefab(sfxName);
                    if (prefab != null)
                    {
                        UnityEngine.Object.Instantiate(prefab, Player.m_localPlayer.transform.position, Quaternion.identity);
                    }
                }
            }
            catch
            {
                // Ignore audio playback failures gracefully
            }
        }
    }
}
