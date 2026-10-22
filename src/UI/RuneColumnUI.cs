using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VikingSkills.Configuration;
using VikingSkills.Data;
using VikingSkills.Keystones;

namespace VikingSkills.UI
{
    public class RuneColumnUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Skills.SkillType SkillType { get; private set; }
        public float SkillLevel { get; private set; }
        public SkillCategoryType Category { get; private set; }

        public const float ColumnWidth = 340f;
        public const float ColumnHeight = 440f;

        private Player? _player;
        private Image? _cardBg;
        private Image? _fillImage;
        private Image? _subLevelFill;
        private Image? _ambientGlow;
        private TMP_Text? _levelText;
        private TMP_Text? _bonusesLabels;
        private TMP_Text? _bonusesValues;
        private Color _categoryColor;

        public static RuneColumnUI Create(
            Transform parent,
            Player player,
            Skills.Skill skill,
            TMP_FontAsset? font)
        {
            var go = new GameObject($"RuneColumn_{skill.m_info.m_skill}", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(ColumnWidth, ColumnHeight);

            var col = go.AddComponent<RuneColumnUI>();
            col.Initialize(player, skill, font);
            return col;
        }

        private void Initialize(Player player, Skills.Skill skill, TMP_FontAsset? font)
        {
            _player = player;
            var fontToUse = font ?? UIHelper.GetValheimFont();
            SkillType = skill.m_info.m_skill;
            SkillLevel = skill.m_level;
            Category = SkillCategory.GetCategory(SkillType);
            _categoryColor = ModConfig.GetCategoryColor(Category);

            // 1. Soft Drop Shadow (Gives 3D depth over game viewport)
            var shadowObj = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            shadowObj.transform.SetParent(transform, false);
            var shadowRect = shadowObj.GetComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(-12f, -12f);
            shadowRect.offsetMax = new Vector2(12f, 12f);
            var shadowImg = shadowObj.GetComponent<Image>();
            shadowImg.sprite = UIHelper.GetSoftShadow();
            shadowImg.type = Image.Type.Sliced;
            shadowImg.color = new Color(0f, 0f, 0f, 0.70f);

            // 2. High-Res 9-Slice Glass Card Background
            var bgObj = new GameObject("GlassCard", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(transform, false);
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            _cardBg = bgObj.GetComponent<Image>();
            _cardBg.sprite = UIHelper.GetGlassCard();
            _cardBg.type = Image.Type.Sliced;
            _cardBg.color = new Color(1f, 1f, 1f, 1f);

            // 3. Top Ambient Edge Glow (Category themed)
            var glowObj = new GameObject("AmbientGlow", typeof(RectTransform), typeof(Image));
            glowObj.transform.SetParent(bgObj.transform, false);
            var glowRect = glowObj.GetComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 1f);
            glowRect.anchorMax = new Vector2(0.5f, 1f);
            glowRect.pivot = new Vector2(0.5f, 1f);
            glowRect.anchoredPosition = new Vector2(0f, 8f);
            glowRect.sizeDelta = new Vector2(190f, 85f);
            _ambientGlow = glowObj.GetComponent<Image>();
            _ambientGlow.sprite = UIHelper.GetRadialGlow();
            _ambientGlow.color = new Color(_categoryColor.r, _categoryColor.g, _categoryColor.b, 0.22f);

            // 4. Header: Circular Icon Crest
            var iconBackdropObj = new GameObject("IconBackdrop", typeof(RectTransform), typeof(Image));
            iconBackdropObj.transform.SetParent(transform, false);
            var ibRect = iconBackdropObj.GetComponent<RectTransform>();
            ibRect.anchorMin = new Vector2(0.5f, 1f);
            ibRect.anchorMax = new Vector2(0.5f, 1f);
            ibRect.pivot = new Vector2(0.5f, 1f);
            ibRect.anchoredPosition = new Vector2(0f, -10f);
            ibRect.sizeDelta = new Vector2(60f, 60f);
            var ibImg = iconBackdropObj.GetComponent<Image>();
            ibImg.sprite = UIHelper.GetNodeCircle();
            ibImg.color = new Color(0.14f, 0.10f, 0.08f, 0.95f);

            var iconObj = new GameObject("SkillIcon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(iconBackdropObj.transform, false);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(46f, 46f);
            var iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = skill.m_info.m_icon;
            iconImg.preserveAspect = true;

            // 5. Header: Skill Name
            var nameObj = new GameObject("SkillName", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(transform, false);
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1f);
            nameRect.anchorMax = new Vector2(0.5f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.anchoredPosition = new Vector2(0f, -74f);
            nameRect.sizeDelta = new Vector2(310f, 20f);
            var nameText = nameObj.GetComponent<TextMeshProUGUI>();
            if (fontToUse != null) nameText.font = fontToUse;
            string locName = Localization.instance != null
                ? Localization.instance.Localize("$skill_" + SkillType.ToString().ToLower())
                : SkillType.ToString();
            nameText.text = $"<b>{locName}</b>";
            nameText.fontSize = 15f;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = 12f;
            nameText.fontSizeMax = 16f;
            nameText.textWrappingMode = TextWrappingModes.NoWrap;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = UIHelper.LightText;

            // 6. Header: Level Display
            var levelObj = new GameObject("LevelText", typeof(RectTransform), typeof(TextMeshProUGUI));
            levelObj.transform.SetParent(transform, false);
            var levelRect = levelObj.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.5f, 1f);
            levelRect.anchorMax = new Vector2(0.5f, 1f);
            levelRect.pivot = new Vector2(0.5f, 1f);
            levelRect.anchoredPosition = new Vector2(0f, -96f);
            levelRect.sizeDelta = new Vector2(300f, 18f);
            _levelText = levelObj.GetComponent<TextMeshProUGUI>();
            if (fontToUse != null) _levelText.font = fontToUse;
            _levelText.fontSize = 13.5f;
            _levelText.alignment = TextAlignmentOptions.Center;
            _levelText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(_categoryColor)}><b>Lv. {Mathf.FloorToInt(SkillLevel)}</b></color>";

            // 6b. Horizontal Sub-Level XP Progress Bar (to next level)
            var subTrackObj = new GameObject("SubLevelTrack", typeof(RectTransform), typeof(Image));
            subTrackObj.transform.SetParent(transform, false);
            var subTrackRect = subTrackObj.GetComponent<RectTransform>();
            subTrackRect.anchorMin = new Vector2(0.5f, 1f);
            subTrackRect.anchorMax = new Vector2(0.5f, 1f);
            subTrackRect.pivot = new Vector2(0.5f, 1f);
            subTrackRect.anchoredPosition = new Vector2(0f, -118f);
            subTrackRect.sizeDelta = new Vector2(180f, 4f);
            var subTrackImg = subTrackObj.GetComponent<Image>();
            subTrackImg.sprite = UIHelper.GetConduitTrack();
            subTrackImg.type = Image.Type.Sliced;
            subTrackImg.color = new Color(0.08f, 0.06f, 0.05f, 0.95f);

            var subFillObj = new GameObject("SubLevelFill", typeof(RectTransform), typeof(Image));
            subFillObj.transform.SetParent(subTrackObj.transform, false);
            var subFillRect = subFillObj.GetComponent<RectTransform>();
            subFillRect.anchorMin = Vector2.zero;
            subFillRect.anchorMax = Vector2.one;
            subFillRect.offsetMin = Vector2.zero;
            subFillRect.offsetMax = Vector2.zero;
            _subLevelFill = subFillObj.GetComponent<Image>();
            _subLevelFill.sprite = UIHelper.GetWhiteSprite();
            _subLevelFill.type = Image.Type.Filled;
            _subLevelFill.fillMethod = Image.FillMethod.Horizontal;
            _subLevelFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            _subLevelFill.fillAmount = Mathf.Clamp01(skill.GetLevelPercentage());
            _subLevelFill.color = UIHelper.BrightBlueConduit;

            // 7. Vertical Clean Energy Conduit Bar (Centered, no tick marks, overall progress 0-100)
            var conduitTrackObj = new GameObject("ConduitTrack", typeof(RectTransform), typeof(Image));
            conduitTrackObj.transform.SetParent(transform, false);
            var conduitTrackRect = conduitTrackObj.GetComponent<RectTransform>();
            conduitTrackRect.anchorMin = new Vector2(0.5f, 1f);
            conduitTrackRect.anchorMax = new Vector2(0.5f, 1f);
            conduitTrackRect.pivot = new Vector2(0.5f, 1f);
            conduitTrackRect.anchoredPosition = new Vector2(0f, -128f);
            conduitTrackRect.sizeDelta = new Vector2(14f, 66f);
            var conduitTrackImg = conduitTrackObj.GetComponent<Image>();
            conduitTrackImg.sprite = UIHelper.GetConduitTrack();
            conduitTrackImg.type = Image.Type.Sliced;
            conduitTrackImg.color = new Color(0.08f, 0.06f, 0.05f, 0.95f);

            // Glowing Fill Inside Conduit
            var fillObj = new GameObject("ConduitFill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(conduitTrackObj.transform, false);
            var fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            _fillImage = fillObj.GetComponent<Image>();
            _fillImage.sprite = UIHelper.GetConduitFill();
            _fillImage.type = Image.Type.Filled;
            _fillImage.fillMethod = Image.FillMethod.Vertical;
            _fillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            _fillImage.fillAmount = Mathf.Clamp01(SkillLevel / 100f);
            _fillImage.color = UIHelper.BrightBlueConduit;

            // 8. Active Bonuses Glass Sub-Container (Middle)
            var bonusBoxObj = new GameObject("BonusBox", typeof(RectTransform), typeof(Image));
            bonusBoxObj.transform.SetParent(transform, false);
            var bbRect = bonusBoxObj.GetComponent<RectTransform>();
            bbRect.anchorMin = new Vector2(0f, 0f);
            bbRect.anchorMax = new Vector2(1f, 1f);
            bbRect.offsetMin = new Vector2(12f, 98f);
            bbRect.offsetMax = new Vector2(-12f, -202f);
            var bbImg = bonusBoxObj.GetComponent<Image>();
            bbImg.sprite = UIHelper.GetGlassCard();
            bbImg.type = Image.Type.Sliced;
            bbImg.color = new Color(0.09f, 0.07f, 0.05f, 0.90f);

            // 9. Horizontal Row of 5 Keystones (Going left-to-right at the bottom of each pillar)
            var socketRowObj = new GameObject("KeystonesRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            socketRowObj.transform.SetParent(transform, false);
            var srRect = socketRowObj.GetComponent<RectTransform>();
            srRect.anchorMin = new Vector2(0.5f, 0f);
            srRect.anchorMax = new Vector2(0.5f, 0f);
            srRect.pivot = new Vector2(0.5f, 0f);
            srRect.anchoredPosition = new Vector2(0f, 10f);
            srRect.sizeDelta = new Vector2(320f, 66f);

            var hlg = socketRowObj.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            CreateBottomKeystoneSocket(socketRowObj.transform, fontToUse, 20);
            CreateBottomKeystoneSocket(socketRowObj.transform, fontToUse, 40);
            CreateBottomKeystoneSocket(socketRowObj.transform, fontToUse, 60);
            CreateBottomKeystoneSocket(socketRowObj.transform, fontToUse, 80);
            CreateBottomKeystoneSocket(socketRowObj.transform, fontToUse, 100);

            var bonusTitleObj = new GameObject("BonusTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            bonusTitleObj.transform.SetParent(bonusBoxObj.transform, false);
            var btRect = bonusTitleObj.GetComponent<RectTransform>();
            btRect.anchorMin = new Vector2(0f, 1f);
            btRect.anchorMax = new Vector2(1f, 1f);
            btRect.pivot = new Vector2(0.5f, 1f);
            btRect.anchoredPosition = new Vector2(0f, -6f);
            btRect.sizeDelta = new Vector2(-16f, 14f);
            var btTmp = bonusTitleObj.GetComponent<TextMeshProUGUI>();
            if (fontToUse != null) btTmp.font = fontToUse;
            btTmp.text = "<color=#D5C7B5>ACTIVE PASSIVES</color>";
            btTmp.fontSize = 10f;
            btTmp.alignment = TextAlignmentOptions.Center;

            // 2-Column Active Bonuses Text Area (Left-aligned descriptions, Right-aligned values)
            var labelsObj = new GameObject("BonusesLabels", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelsObj.transform.SetParent(bonusBoxObj.transform, false);
            var lRect = labelsObj.GetComponent<RectTransform>();
            lRect.anchorMin = Vector2.zero;
            lRect.anchorMax = Vector2.one;
            lRect.offsetMin = new Vector2(8f, 4f);
            lRect.offsetMax = new Vector2(-58f, -22f);
            _bonusesLabels = labelsObj.GetComponent<TextMeshProUGUI>();
            if (fontToUse != null) _bonusesLabels.font = fontToUse;
            _bonusesLabels.fontSize = 9.5f;
            _bonusesLabels.lineSpacing = 1.1f;
            _bonusesLabels.textWrappingMode = TextWrappingModes.NoWrap;
            _bonusesLabels.alignment = TextAlignmentOptions.TopLeft;

            var valsObj = new GameObject("BonusesValues", typeof(RectTransform), typeof(TextMeshProUGUI));
            valsObj.transform.SetParent(bonusBoxObj.transform, false);
            var vRect = valsObj.GetComponent<RectTransform>();
            vRect.anchorMin = Vector2.zero;
            vRect.anchorMax = Vector2.one;
            vRect.offsetMin = new Vector2(136f, 4f);
            vRect.offsetMax = new Vector2(-8f, -22f);
            _bonusesValues = valsObj.GetComponent<TextMeshProUGUI>();
            if (fontToUse != null) _bonusesValues.font = fontToUse;
            _bonusesValues.fontSize = 9.5f;
            _bonusesValues.lineSpacing = 1.1f;
            _bonusesValues.textWrappingMode = TextWrappingModes.NoWrap;
            _bonusesValues.alignment = TextAlignmentOptions.TopRight;

            PopulateBonuses();
        }

        private void CreateBottomKeystoneSocket(Transform parent, TMP_FontAsset? fontToUse, int milestone)
        {
            bool reached = SkillLevel >= milestone;
            var active = _player != null ? KeystoneManager.GetSelectedKeystone(_player, SkillType, milestone) : null;

            var socketObj = new GameObject($"KeystoneSocket_{milestone}", typeof(RectTransform), typeof(Image), typeof(Button));
            socketObj.transform.SetParent(parent, false);
            var sRect = socketObj.GetComponent<RectTransform>();
            sRect.sizeDelta = new Vector2(58f, 58f);

            var ringImg = socketObj.GetComponent<Image>();
            ringImg.sprite = UIHelper.GetSocketRing();
            ringImg.color = reached
                ? (active != null ? UIHelper.GoldAccent : new Color(0.95f, 0.80f, 0.45f, 1f))
                : new Color(0.45f, 0.35f, 0.25f, 0.60f);

            // Level number badge above socket
            var numObj = new GameObject("Num", typeof(RectTransform), typeof(TextMeshProUGUI));
            numObj.transform.SetParent(socketObj.transform, false);
            var nRect = numObj.GetComponent<RectTransform>();
            nRect.anchorMin = new Vector2(0.5f, 1f);
            nRect.anchorMax = new Vector2(0.5f, 1f);
            nRect.pivot = new Vector2(0.5f, 0f);
            nRect.anchoredPosition = new Vector2(0f, 2f);
            nRect.sizeDelta = new Vector2(58f, 14f);
            var nTmp = numObj.GetComponent<TextMeshProUGUI>();
            if (fontToUse != null) nTmp.font = fontToUse;
            nTmp.text = $"{milestone}";
            nTmp.fontSize = 10.5f;
            nTmp.alignment = TextAlignmentOptions.Center;
            nTmp.color = reached ? (active != null ? UIHelper.GoldAccent : UIHelper.AmberGoldGlow) : UIHelper.MutedParchment;

            // Circular Mask clipping the icon inside the circular slot
            var maskObj = new GameObject("IconMask", typeof(RectTransform), typeof(Image), typeof(Mask));
            maskObj.transform.SetParent(socketObj.transform, false);
            var mRect = maskObj.GetComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0.5f, 0.5f);
            mRect.anchorMax = new Vector2(0.5f, 0.5f);
            mRect.pivot = new Vector2(0.5f, 0.5f);
            mRect.sizeDelta = new Vector2(48f, 48f);
            var mImg = maskObj.GetComponent<Image>();
            mImg.sprite = UIHelper.GetCircleMask();
            var mask = maskObj.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            // Inner Keystone Icon
            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(maskObj.transform, false);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            var iconImg = iconObj.GetComponent<Image>();
            iconImg.preserveAspect = true;

            if (active != null)
            {
                iconImg.sprite = active.GetIcon(_player) ?? UIHelper.GetWhiteSprite();
                iconImg.color = Color.white;
            }
            else if (reached)
            {
                iconImg.sprite = UIHelper.GetSocketEmpty();
                iconImg.color = UIHelper.AmberGoldGlow;
            }
            else
            {
                iconImg.sprite = UIHelper.GetSocketEmpty();
                iconImg.color = new Color(0.40f, 0.30f, 0.20f, 0.45f);
            }

            int mCaptured = milestone;
            socketObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                PanoramicSkillsDialog.OpenSkillTreeWithShelf(SkillType, mCaptured);
                UIHelper.PlaySfx("sfx_gui_button");
            });

            var hover = socketObj.AddComponent<OverviewKeystoneHoverTrigger>();
            hover.Init(active, milestone, reached);
        }

        private void PopulateBonuses()
        {
            if (_bonusesLabels == null || _bonusesValues == null) return;

            var lines = SkillBonusCalculator.GetActiveBonuses(SkillType, SkillLevel);
            var labelLines = new List<string>();
            var valueLines = new List<string>();

            foreach (var b in lines)
            {
                int colon = b.IndexOf(':');
                if (colon >= 0)
                {
                    string desc = b.Substring(0, colon).Trim();
                    string val = b.Substring(colon + 1).Trim();
                    labelLines.Add($"<color=#F59E0B>•</color> <color=#EDE4D8>{desc}</color>");
                    valueLines.Add($"<color=#FAF6F0><b>{val}</b></color>");
                }
                else
                {
                    labelLines.Add($"<color=#F59E0B>•</color> <color=#EDE4D8>{b}</color>");
                    valueLines.Add("");
                }
            }

            _bonusesLabels.text = string.Join("\n", labelLines);
            _bonusesValues.text = string.Join("\n", valueLines);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_cardBg != null)
            {
                _cardBg.color = new Color(1.15f, 1.10f, 1.05f, 1f);
            }
            if (_ambientGlow != null)
            {
                _ambientGlow.color = new Color(_categoryColor.r, _categoryColor.g, _categoryColor.b, 0.40f);
            }

            UIHelper.PlaySfx("sfx_gui_button");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_cardBg != null)
            {
                _cardBg.color = Color.white;
            }
            if (_ambientGlow != null)
            {
                _ambientGlow.color = new Color(_categoryColor.r, _categoryColor.g, _categoryColor.b, 0.22f);
            }
            PanoramicSkillsDialog.Instance?.HideHoverTooltip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PanoramicSkillsDialog.OpenSkillTree(SkillType);
        }
    }

    public class OverviewKeystoneHoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private KeystoneDefinition? _keystone;
        private int _milestone;
        private bool _isUnlocked;

        public void Init(KeystoneDefinition? keystone, int milestone, bool isUnlocked)
        {
            _keystone = keystone;
            _milestone = milestone;
            _isUnlocked = isUnlocked;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PanoramicSkillsDialog.Instance?.ShowKeystoneTooltip(_keystone, _milestone, _isUnlocked, transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PanoramicSkillsDialog.Instance?.HideHoverTooltip();
        }
    }
}
