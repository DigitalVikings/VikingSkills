using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VikingSkills.Data;
using VikingSkills.Keystones;

namespace VikingSkills.UI
{
    public class SkillTreeDrawerUI : MonoBehaviour
    {
        public static SkillTreeDrawerUI? Instance { get; private set; }

        private Skills.SkillType _currentSkill;
        private Player? _player;
        private GameObject? _modalWindow;
        private TMP_Text? _headerTitle;
        private Image? _skillIcon;
        private Sprite? _skillSprite;
        private Action? _onCloseCallback;

        // Left Panel (Pinned Cumulative Overview)
        private TMP_Text? _leftLevelText;
        private Image? _leftXpFill;
        private TMP_Text? _leftPassivesLabels;
        private TMP_Text? _leftPassivesValues;
        private Transform? _leftSocketedRow;

        // Right Timeline (1-100 Left-to-Right)
        private ScrollRect? _timelineScrollRect;
        private RectTransform? _timelineContent;
        private Slider? _timelineSlider;
        private readonly List<GameObject> _timelineNodeObjects = new();

        // Floating Selection Shelf
        private GameObject? _selectionShelf;
        private TMP_Text? _shelfTitle;
        private Transform? _shelfOptionsContainer;
        private int _activeShelfMilestone;

        // Hover Tooltip
        private GameObject? _hoverTooltip;
        private TMP_Text? _tooltipTitle;
        private TMP_Text? _tooltipContent;

        // Layout Constants
        public const float TimelineWidth = 6600f;
        public const float LevelSpacing = 64f;
        public const float StartX = 60f;

        public static SkillTreeDrawerUI Create(Transform parent, Action onClose)
        {
            var go = new GameObject("SkillTreeDrawer", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var drawer = go.AddComponent<SkillTreeDrawerUI>();
            drawer._onCloseCallback = onClose;
            drawer.BuildUI();
            go.SetActive(false);
            Instance = drawer;
            return drawer;
        }

        private void BuildUI()
        {
            var font = UIHelper.GetValheimFont();

            // 1. Semi-transparent backdrop veil
            var backdrop = gameObject.AddComponent<Image>();
            backdrop.sprite = UIHelper.GetWhiteSprite();
            backdrop.color = new Color(0.04f, 0.03f, 0.02f, 0.88f);

            // 2. Centered Modal Window
            _modalWindow = new GameObject("ModalWindow", typeof(RectTransform));
            _modalWindow.transform.SetParent(transform, false);
            var modalRect = _modalWindow.GetComponent<RectTransform>();
            modalRect.anchorMin = new Vector2(0.5f, 0.5f);
            modalRect.anchorMax = new Vector2(0.5f, 0.5f);
            modalRect.pivot = new Vector2(0.5f, 0.5f);
            modalRect.anchoredPosition = Vector2.zero;
            modalRect.sizeDelta = new Vector2(1280f, 620f);

            // Modal Soft Drop Shadow
            var shadowObj = new GameObject("ModalShadow", typeof(RectTransform), typeof(Image));
            shadowObj.transform.SetParent(_modalWindow.transform, false);
            var shadowRect = shadowObj.GetComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(-16f, -16f);
            shadowRect.offsetMax = new Vector2(16f, 16f);
            var shadowImg = shadowObj.GetComponent<Image>();
            shadowImg.sprite = UIHelper.GetSoftShadow();
            shadowImg.type = Image.Type.Sliced;
            shadowImg.color = new Color(0f, 0f, 0f, 0.75f);

            // Modal Glass Body
            var modalBgObj = new GameObject("ModalGlass", typeof(RectTransform), typeof(Image));
            modalBgObj.transform.SetParent(_modalWindow.transform, false);
            var mbRect = modalBgObj.GetComponent<RectTransform>();
            mbRect.anchorMin = Vector2.zero;
            mbRect.anchorMax = Vector2.one;
            mbRect.offsetMin = Vector2.zero;
            mbRect.offsetMax = Vector2.zero;
            var mbImg = modalBgObj.GetComponent<Image>();
            mbImg.sprite = UIHelper.GetGlassPanel();
            mbImg.type = Image.Type.Sliced;
            mbImg.color = new Color(1f, 1f, 1f, 1f);

            // 3. Top Header Bar (56px) - Plain title, no "Skill Tree"
            BuildHeader(modalBgObj.transform, font);

            // 4. Left Pinned Cumulative Overview Panel (280px)
            BuildLeftOverviewPanel(modalBgObj.transform, font);

            // 5. Right Timeline Viewport (Levels 1 to 100)
            BuildTimelineViewport(modalBgObj.transform, font);

            // 6. Floating Selection Shelf (Hidden by default)
            BuildSelectionShelf(modalBgObj.transform, font);

            // 7. Hover Tooltip (Hidden by default)
            BuildHoverTooltip(modalBgObj.transform, font);
        }

        private void BuildHeader(Transform parent, TMP_FontAsset? font)
        {
            var topBarObj = new GameObject("HeaderBar", typeof(RectTransform), typeof(Image));
            topBarObj.transform.SetParent(parent, false);
            var topBarRect = topBarObj.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0f, 1f);
            topBarRect.anchorMax = new Vector2(1f, 1f);
            topBarRect.pivot = new Vector2(0.5f, 1f);
            topBarRect.anchoredPosition = Vector2.zero;
            topBarRect.sizeDelta = new Vector2(0f, 56f);
            var topBarImg = topBarObj.GetComponent<Image>();
            topBarImg.sprite = UIHelper.GetGlassCard();
            topBarImg.type = Image.Type.Sliced;
            topBarImg.color = new Color(0.12f, 0.09f, 0.07f, 0.98f);

            // Icon Crest (Top-Left)
            var iconBackdrop = new GameObject("IconBackdrop", typeof(RectTransform), typeof(Image));
            iconBackdrop.transform.SetParent(topBarObj.transform, false);
            var ibRect = iconBackdrop.GetComponent<RectTransform>();
            ibRect.anchorMin = new Vector2(0f, 0.5f);
            ibRect.anchorMax = new Vector2(0f, 0.5f);
            ibRect.pivot = new Vector2(0f, 0.5f);
            ibRect.anchoredPosition = new Vector2(20f, 0f);
            ibRect.sizeDelta = new Vector2(50f, 50f);
            var ibImg = iconBackdrop.GetComponent<Image>();
            ibImg.sprite = UIHelper.GetNodeCircle();
            ibImg.color = new Color(0.16f, 0.12f, 0.09f, 0.95f);

            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(iconBackdrop.transform, false);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(38f, 38f);
            _skillIcon = iconObj.GetComponent<Image>();
            _skillIcon.preserveAspect = true;

            // Title (Plain title, e.g. "CLUBS", "SWORDS" - NO "Skill Tree")
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(topBarObj.transform, false);
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.5f);
            titleRect.anchorMax = new Vector2(0f, 0.5f);
            titleRect.pivot = new Vector2(0f, 0.5f);
            titleRect.anchoredPosition = new Vector2(80f, 0f);
            titleRect.sizeDelta = new Vector2(400f, 28f);
            _headerTitle = titleObj.GetComponent<TextMeshProUGUI>();
            if (font != null) _headerTitle.font = font;
            _headerTitle.fontSize = 17f;
            _headerTitle.alignment = TextAlignmentOptions.MidlineLeft;
            _headerTitle.color = UIHelper.LightText;

            // Return Button (Standardized shared UI element on top-right)
            UIHelper.CreateReturnButton(topBarObj.transform, new Vector2(-20f, 0f), new Vector2(146f, 44f),
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                "RETURN", Close, font);
        }

        private void BuildLeftOverviewPanel(Transform parent, TMP_FontAsset? font)
        {
            var leftPanelObj = new GameObject("LeftOverviewPanel", typeof(RectTransform), typeof(Image));
            leftPanelObj.transform.SetParent(parent, false);
            var lpRect = leftPanelObj.GetComponent<RectTransform>();
            lpRect.anchorMin = new Vector2(0f, 0f);
            lpRect.anchorMax = new Vector2(0f, 1f);
            lpRect.pivot = new Vector2(0f, 0.5f);
            lpRect.anchoredPosition = new Vector2(14f, -28f);
            lpRect.sizeDelta = new Vector2(340f, -70f);

            var lpImg = leftPanelObj.GetComponent<Image>();
            lpImg.sprite = UIHelper.GetGlassCard();
            lpImg.type = Image.Type.Sliced;
            lpImg.color = new Color(0.09f, 0.07f, 0.05f, 0.96f);

            // 1. Level Display
            var levelObj = new GameObject("LevelText", typeof(RectTransform), typeof(TextMeshProUGUI));
            levelObj.transform.SetParent(leftPanelObj.transform, false);
            var levelRect = levelObj.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.5f, 1f);
            levelRect.anchorMax = new Vector2(0.5f, 1f);
            levelRect.pivot = new Vector2(0.5f, 1f);
            levelRect.anchoredPosition = new Vector2(0f, -14f);
            levelRect.sizeDelta = new Vector2(300f, 32f);
            _leftLevelText = levelObj.GetComponent<TextMeshProUGUI>();
            if (font != null) _leftLevelText.font = font;
            _leftLevelText.fontSize = 20f;
            _leftLevelText.alignment = TextAlignmentOptions.Center;

            // 2. XP Progress Track (No "XP to next level" text)
            var xpTrackObj = new GameObject("XpTrack", typeof(RectTransform), typeof(Image));
            xpTrackObj.transform.SetParent(leftPanelObj.transform, false);
            var xpTrackRect = xpTrackObj.GetComponent<RectTransform>();
            xpTrackRect.anchorMin = new Vector2(0.5f, 1f);
            xpTrackRect.anchorMax = new Vector2(0.5f, 1f);
            xpTrackRect.pivot = new Vector2(0.5f, 1f);
            xpTrackRect.anchoredPosition = new Vector2(0f, -50f);
            xpTrackRect.sizeDelta = new Vector2(300f, 6f);
            var xpTrackImg = xpTrackObj.GetComponent<Image>();
            xpTrackImg.sprite = UIHelper.GetConduitTrack();
            xpTrackImg.type = Image.Type.Sliced;
            xpTrackImg.color = new Color(0.08f, 0.06f, 0.05f, 0.95f);

            var xpFillObj = new GameObject("XpFill", typeof(RectTransform), typeof(Image));
            xpFillObj.transform.SetParent(xpTrackObj.transform, false);
            var xpFillRect = xpFillObj.GetComponent<RectTransform>();
            xpFillRect.anchorMin = Vector2.zero;
            xpFillRect.anchorMax = Vector2.one;
            xpFillRect.offsetMin = Vector2.zero;
            xpFillRect.offsetMax = Vector2.zero;
            _leftXpFill = xpFillObj.GetComponent<Image>();
            _leftXpFill.sprite = UIHelper.GetWhiteSprite();
            _leftXpFill.type = Image.Type.Filled;
            _leftXpFill.fillMethod = Image.FillMethod.Horizontal;
            _leftXpFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            _leftXpFill.color = UIHelper.BrightBlueConduit;

            // 3. Divider
            CreateHorizontalLine(leftPanelObj.transform, -68f);

            // 4. "CUMULATIVE PASSIVES" Title & Content
            var passTitleObj = new GameObject("PassivesTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            passTitleObj.transform.SetParent(leftPanelObj.transform, false);
            var ptRect = passTitleObj.GetComponent<RectTransform>();
            ptRect.anchorMin = new Vector2(0f, 1f);
            ptRect.anchorMax = new Vector2(1f, 1f);
            ptRect.pivot = new Vector2(0.5f, 1f);
            ptRect.anchoredPosition = new Vector2(0f, -76f);
            ptRect.sizeDelta = new Vector2(-28f, 16f);
            var ptTmp = passTitleObj.GetComponent<TextMeshProUGUI>();
            if (font != null) ptTmp.font = font;
            ptTmp.text = "<color=#E5A93C><b>CUMULATIVE PASSIVES</b></color>";
            ptTmp.fontSize = 11f;
            ptTmp.alignment = TextAlignmentOptions.Center;

            // Passives Two-Column Container (Left aligned labels, Right aligned values)
            var passContainer = new GameObject("PassivesContainer", typeof(RectTransform));
            passContainer.transform.SetParent(leftPanelObj.transform, false);
            var pcRect = passContainer.GetComponent<RectTransform>();
            pcRect.anchorMin = new Vector2(0f, 1f);
            pcRect.anchorMax = new Vector2(1f, 1f);
            pcRect.pivot = new Vector2(0.5f, 1f);
            pcRect.anchoredPosition = new Vector2(0f, -96f);
            pcRect.sizeDelta = new Vector2(-28f, 160f);

            var passLabelsObj = new GameObject("PassivesLabels", typeof(RectTransform), typeof(TextMeshProUGUI));
            passLabelsObj.transform.SetParent(passContainer.transform, false);
            var plRect = passLabelsObj.GetComponent<RectTransform>();
            plRect.anchorMin = Vector2.zero;
            plRect.anchorMax = Vector2.one;
            plRect.offsetMin = Vector2.zero;
            plRect.offsetMax = new Vector2(-65f, 0f);
            _leftPassivesLabels = passLabelsObj.GetComponent<TextMeshProUGUI>();
            if (font != null) _leftPassivesLabels.font = font;
            _leftPassivesLabels.fontSize = 10.5f;
            _leftPassivesLabels.lineSpacing = 1.15f;
            _leftPassivesLabels.alignment = TextAlignmentOptions.TopLeft;

            var passValsObj = new GameObject("PassivesValues", typeof(RectTransform), typeof(TextMeshProUGUI));
            passValsObj.transform.SetParent(passContainer.transform, false);
            var pvRect = passValsObj.GetComponent<RectTransform>();
            pvRect.anchorMin = Vector2.zero;
            pvRect.anchorMax = Vector2.one;
            pvRect.offsetMin = new Vector2(170f, 0f);
            pvRect.offsetMax = Vector2.zero;
            _leftPassivesValues = passValsObj.GetComponent<TextMeshProUGUI>();
            if (font != null) _leftPassivesValues.font = font;
            _leftPassivesValues.fontSize = 10.5f;
            _leftPassivesValues.lineSpacing = 1.15f;
            _leftPassivesValues.alignment = TextAlignmentOptions.TopRight;

            // 5. Divider
            CreateHorizontalLine(leftPanelObj.transform, -270f);

            // 6. "KEYSTONES" Section Header
            var sockTitleObj = new GameObject("SocketTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            sockTitleObj.transform.SetParent(leftPanelObj.transform, false);
            var stRect = sockTitleObj.GetComponent<RectTransform>();
            stRect.anchorMin = new Vector2(0f, 1f);
            stRect.anchorMax = new Vector2(1f, 1f);
            stRect.pivot = new Vector2(0.5f, 1f);
            stRect.anchoredPosition = new Vector2(0f, -280f);
            stRect.sizeDelta = new Vector2(-28f, 16f);
            var stTmp = sockTitleObj.GetComponent<TextMeshProUGUI>();
            if (font != null) stTmp.font = font;
            stTmp.text = "<color=#E5A93C><b>KEYSTONE SOCKETS</b></color>";
            stTmp.fontSize = 11f;
            stTmp.alignment = TextAlignmentOptions.Center;

            // 7. Horizontal Row of 5 Circular Keystone Socket Icons (Hover for details)
            var sockRowObj = new GameObject("SocketRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            sockRowObj.transform.SetParent(leftPanelObj.transform, false);
            var scRect = sockRowObj.GetComponent<RectTransform>();
            scRect.anchorMin = new Vector2(0.5f, 0f);
            scRect.anchorMax = new Vector2(0.5f, 0f);
            scRect.pivot = new Vector2(0.5f, 0f);
            scRect.anchoredPosition = new Vector2(0f, 14f);
            scRect.sizeDelta = new Vector2(320f, 66f);
            var hlg = sockRowObj.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            _leftSocketedRow = sockRowObj.transform;
        }

        private void CreateHorizontalLine(Transform parent, float yOffset)
        {
            var lineObj = new GameObject("Line", typeof(RectTransform), typeof(Image));
            lineObj.transform.SetParent(parent, false);
            var lRect = lineObj.GetComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0f, 1f);
            lRect.anchorMax = new Vector2(1f, 1f);
            lRect.pivot = new Vector2(0.5f, 1f);
            lRect.anchoredPosition = new Vector2(0f, yOffset);
            lRect.sizeDelta = new Vector2(-24f, 1.5f);
            var lImg = lineObj.GetComponent<Image>();
            lImg.sprite = UIHelper.GetWhiteSprite();
            lImg.color = new Color(0.40f, 0.30f, 0.20f, 0.50f);
        }

        private void BuildTimelineViewport(Transform parent, TMP_FontAsset? font)
        {
            // Viewport bounds
            var scrollObj = new GameObject("TimelineScroll", typeof(RectTransform), typeof(ScrollRect));
            scrollObj.transform.SetParent(parent, false);
            var scrollRect = scrollObj.GetComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(368f, 52f);
            scrollRect.offsetMax = new Vector2(-16f, -64f);

            _timelineScrollRect = scrollObj.GetComponent<ScrollRect>();
            _timelineScrollRect.horizontal = true;
            _timelineScrollRect.vertical = false;
            _timelineScrollRect.movementType = ScrollRect.MovementType.Clamped;
            _timelineScrollRect.scrollSensitivity = 40f;

            // Viewport Mask
            var vpObj = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            vpObj.transform.SetParent(scrollObj.transform, false);
            var vpRect = vpObj.GetComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = Vector2.zero;
            vpRect.offsetMax = Vector2.zero;
            _timelineScrollRect.viewport = vpRect;

            // Content Track
            var contentObj = new GameObject("Content", typeof(RectTransform));
            contentObj.transform.SetParent(vpObj.transform, false);
            _timelineContent = contentObj.GetComponent<RectTransform>();
            _timelineContent.anchorMin = new Vector2(0f, 0.5f);
            _timelineContent.anchorMax = new Vector2(0f, 0.5f);
            _timelineContent.pivot = new Vector2(0f, 0.5f);
            _timelineContent.anchoredPosition = Vector2.zero;
            _timelineContent.sizeDelta = new Vector2(TimelineWidth, 460f);
            _timelineScrollRect.content = _timelineContent;

            // Draggable Timeline Slider at bottom
            BuildTimelineSlider(parent, font);
        }

        private void BuildTimelineSlider(Transform parent, TMP_FontAsset? font)
        {
            var sliderRootObj = new GameObject("TimelineSliderRoot", typeof(RectTransform));
            sliderRootObj.transform.SetParent(parent, false);
            var rootRect = sliderRootObj.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 0f);
            rootRect.anchorMax = new Vector2(1f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0f);
            rootRect.anchoredPosition = new Vector2(150f, 16f);
            rootRect.sizeDelta = new Vector2(-400f, 22f);

            // Left Label: "Lv. 1"
            var leftLbl = new GameObject("LeftLbl", typeof(RectTransform), typeof(TextMeshProUGUI));
            leftLbl.transform.SetParent(sliderRootObj.transform, false);
            var llRect = leftLbl.GetComponent<RectTransform>();
            llRect.anchorMin = new Vector2(0f, 0.5f);
            llRect.anchorMax = new Vector2(0f, 0.5f);
            llRect.pivot = new Vector2(1f, 0.5f);
            llRect.anchoredPosition = new Vector2(-12f, 0f);
            llRect.sizeDelta = new Vector2(50f, 20f);
            var llTmp = leftLbl.GetComponent<TextMeshProUGUI>();
            if (font != null) llTmp.font = font;
            llTmp.text = "<b>Lv. 1</b>";
            llTmp.fontSize = 11f;
            llTmp.alignment = TextAlignmentOptions.Right;
            llTmp.color = UIHelper.MutedText;

            // Right Label: "Lv. 100"
            var rightLbl = new GameObject("RightLbl", typeof(RectTransform), typeof(TextMeshProUGUI));
            rightLbl.transform.SetParent(sliderRootObj.transform, false);
            var rlRect = rightLbl.GetComponent<RectTransform>();
            rlRect.anchorMin = new Vector2(1f, 0.5f);
            rlRect.anchorMax = new Vector2(1f, 0.5f);
            rlRect.pivot = new Vector2(0f, 0.5f);
            rlRect.anchoredPosition = new Vector2(12f, 0f);
            rlRect.sizeDelta = new Vector2(60f, 20f);
            var rlTmp = rightLbl.GetComponent<TextMeshProUGUI>();
            if (font != null) rlTmp.font = font;
            rlTmp.text = "<b>Lv. 100</b>";
            rlTmp.fontSize = 11f;
            rlTmp.alignment = TextAlignmentOptions.Left;
            rlTmp.color = UIHelper.MutedText;

            // Slider Component
            var sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(sliderRootObj.transform, false);
            var sRect = sliderObj.GetComponent<RectTransform>();
            sRect.anchorMin = Vector2.zero;
            sRect.anchorMax = Vector2.one;
            sRect.offsetMin = Vector2.zero;
            sRect.offsetMax = Vector2.zero;

            _timelineSlider = sliderObj.GetComponent<Slider>();
            _timelineSlider.direction = Slider.Direction.LeftToRight;
            _timelineSlider.minValue = 0f;
            _timelineSlider.maxValue = 1f;

            // Background Track
            var trackObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            trackObj.transform.SetParent(sliderObj.transform, false);
            var tRect = trackObj.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0.5f);
            tRect.anchorMax = new Vector2(1f, 0.5f);
            tRect.pivot = new Vector2(0.5f, 0.5f);
            tRect.anchoredPosition = Vector2.zero;
            tRect.sizeDelta = new Vector2(0f, 6f);
            var tImg = trackObj.GetComponent<Image>();
            tImg.sprite = UIHelper.GetConduitTrack();
            tImg.type = Image.Type.Sliced;
            tImg.color = new Color(0.08f, 0.10f, 0.15f, 0.95f);

            // Handle Slide Area
            var handleAreaObj = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleAreaObj.transform.SetParent(sliderObj.transform, false);
            var haRect = handleAreaObj.GetComponent<RectTransform>();
            haRect.anchorMin = Vector2.zero;
            haRect.anchorMax = Vector2.one;
            haRect.offsetMin = new Vector2(24f, 0f);
            haRect.offsetMax = new Vector2(-24f, 0f);

            // Handle
            var handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleObj.transform.SetParent(handleAreaObj.transform, false);
            var hRect = handleObj.GetComponent<RectTransform>();
            hRect.sizeDelta = new Vector2(48f, 18f);
            var hImg = handleObj.GetComponent<Image>();
            hImg.sprite = UIHelper.GetGlassCard();
            hImg.type = Image.Type.Sliced;
            hImg.color = new Color(0.22f, 0.32f, 0.50f, 0.98f);

            _timelineSlider.targetGraphic = hImg;
            _timelineSlider.handleRect = hRect;

            _timelineSlider.onValueChanged.AddListener(val =>
            {
                if (_timelineScrollRect != null)
                {
                    _timelineScrollRect.horizontalNormalizedPosition = val;
                }
            });

            if (_timelineScrollRect != null)
            {
                _timelineScrollRect.onValueChanged.AddListener(pos =>
                {
                    _timelineSlider.SetValueWithoutNotify(pos.x);
                });
            }
        }

        public void ScrollToLevel(int level)
        {
            if (_timelineScrollRect == null) return;
            float clamped = Mathf.Clamp(level, 1, 100);
            float targetX = (clamped - 1) * LevelSpacing;
            float maxScroll = TimelineWidth - 900f;
            float norm = Mathf.Clamp01(targetX / Mathf.Max(1f, maxScroll));
            _timelineScrollRect.horizontalNormalizedPosition = norm;
            _timelineSlider?.SetValueWithoutNotify(norm);
        }

        private void BuildSelectionShelf(Transform parent, TMP_FontAsset? font)
        {
            _selectionShelf = new GameObject("SelectionShelf", typeof(RectTransform), typeof(Image));
            _selectionShelf.transform.SetParent(parent, false);
            var rect = _selectionShelf.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(140f, 0f);
            rect.sizeDelta = new Vector2(820f, 500f);

            var img = _selectionShelf.GetComponent<Image>();
            img.sprite = UIHelper.GetGlassPanel();
            img.type = Image.Type.Sliced;
            img.color = new Color(0.11f, 0.08f, 0.06f, 0.98f);

            // Single Return Button in Top-Right Corner
            UIHelper.CreateReturnButton(_selectionShelf.transform, new Vector2(-16f, -14f), new Vector2(136f, 36f),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                "RETURN", () => _selectionShelf.SetActive(false), font);

            // Shelf Header (Centered, inset from edges)
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(_selectionShelf.transform, false);
            var tRect = titleObj.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.pivot = new Vector2(0.5f, 1f);
            tRect.anchoredPosition = new Vector2(0f, -16f);
            tRect.sizeDelta = new Vector2(-320f, 32f);
            _shelfTitle = titleObj.GetComponent<TextMeshProUGUI>();
            if (font != null) _shelfTitle.font = font;
            _shelfTitle.fontSize = 16f;
            _shelfTitle.alignment = TextAlignmentOptions.Center;

            // Options Container (Horizontal Layout supporting 2, 3, or N choices)
            var optionsObj = new GameObject("OptionsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            optionsObj.transform.SetParent(_selectionShelf.transform, false);
            var optRect = optionsObj.GetComponent<RectTransform>();
            optRect.anchorMin = Vector2.zero;
            optRect.anchorMax = Vector2.one;
            optRect.offsetMin = new Vector2(24f, 24f);
            optRect.offsetMax = new Vector2(-24f, -58f);
            var hlg = optionsObj.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            _shelfOptionsContainer = optionsObj.transform;

            _selectionShelf.SetActive(false);
        }

        private void BuildHoverTooltip(Transform parent, TMP_FontAsset? font)
        {
            _hoverTooltip = new GameObject("HoverTooltip", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            _hoverTooltip.transform.SetParent(parent, false);
            var rect = _hoverTooltip.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(480f, 0f);

            var img = _hoverTooltip.GetComponent<Image>();
            img.sprite = UIHelper.GetGlassCard();
            img.type = Image.Type.Sliced;
            img.color = new Color(0.09f, 0.07f, 0.05f, 0.98f);

            var vlg = _hoverTooltip.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(18, 18, 12, 14);
            vlg.spacing = 6f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = _hoverTooltip.GetComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(_hoverTooltip.transform, false);
            _tooltipTitle = titleObj.GetComponent<TextMeshProUGUI>();
            if (font != null) _tooltipTitle.font = font;
            _tooltipTitle.fontSize = 13f;
            _tooltipTitle.alignment = TextAlignmentOptions.Center;

            var contentObj = new GameObject("Content", typeof(RectTransform), typeof(TextMeshProUGUI));
            contentObj.transform.SetParent(_hoverTooltip.transform, false);
            _tooltipContent = contentObj.GetComponent<TextMeshProUGUI>();
            if (font != null) _tooltipContent.font = font;
            _tooltipContent.fontSize = 11f;
            _tooltipContent.lineSpacing = 1.15f;
            _tooltipContent.alignment = TextAlignmentOptions.Center;

            var cg = _hoverTooltip.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            _hoverTooltip.SetActive(false);
        }

        private void PositionTooltip(Vector3 worldPos, float yOffset)
        {
            if (_hoverTooltip == null) return;
            var rect = _hoverTooltip.GetComponent<RectTransform>();
            rect.position = worldPos + new Vector3(0f, yOffset, 0f);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            float width = corners[2].x - corners[0].x;
            float halfWidth = width * 0.5f;

            float screenW = Screen.width;
            float currentX = rect.position.x;
            float minX = halfWidth + 14f;
            float maxX = screenW - halfWidth - 14f;

            float clampedX = Mathf.Clamp(currentX, minX, maxX);
            rect.position = new Vector3(clampedX, rect.position.y, rect.position.z);
        }

        public void ShowIncrementalTooltip(int level, bool isUnlocked, Vector3 worldPos)
        {
            if (_hoverTooltip == null || _tooltipTitle == null || _tooltipContent == null) return;

            // Clean title without locked/unlocked tags
            _tooltipTitle.alignment = TextAlignmentOptions.Center;
            _tooltipTitle.text = $"<b>Level {level}</b>";

            var gains = SkillBonusCalculator.GetIncrementalBonuses(_currentSkill, level);
            var formatted = new List<string>();
            foreach (var g in gains)
            {
                formatted.Add($"<color=#38BDF8>•</color> {g}");
            }
            _tooltipContent.alignment = TextAlignmentOptions.TopLeft;
            _tooltipContent.text = string.Join("\n", formatted);

            PositionTooltip(worldPos, 40f);
            _hoverTooltip.SetActive(true);
        }

        public void ShowKeystoneTooltip(KeystoneDefinition? keystone, int milestone, bool isUnlocked, Vector3 worldPos)
        {
            if (_hoverTooltip == null || _tooltipTitle == null || _tooltipContent == null) return;

            _tooltipTitle.alignment = TextAlignmentOptions.Center;
            _tooltipContent.alignment = TextAlignmentOptions.Center; // Always center keystone descriptions

            if (keystone != null)
            {
                string artNotice = KeystoneIconManager.HasDedicatedIcon(keystone.Id) ? "" : " <color=#B8A99A><size=80%>(Artwork In Progress)</size></color>";
                _tooltipTitle.text = $"<b><color=#F59E0B>{keystone.Name}</color></b> <size=80%>(Milestone {milestone})</size>{artNotice}";
                _tooltipContent.text = $"<color=#F3EBE1>{keystone.Description}</color>";
            }
            else if (isUnlocked)
            {
                _tooltipTitle.text = $"<b><color=#F59E0B>Milestone {milestone} Socket</color></b>";
                _tooltipContent.text = "<color=#ECC267>Click to socket a keystone talent.</color>";
            }
            else
            {
                _tooltipTitle.text = $"<b><color=#B8A99A>Milestone {milestone} Socket</color></b>";
                _tooltipContent.text = $"<color=#8C7B6E>Requires Level {milestone}.</color>\n<size=85%><color=#ECC267>Click to preview talents.</color></size>";
            }

            PositionTooltip(worldPos, 45f);
            _hoverTooltip.SetActive(true);
        }

        public void HideHoverTooltip()
        {
            if (_hoverTooltip != null)
            {
                _hoverTooltip.SetActive(false);
            }
        }

        public void Open(Player player, Skills.SkillType skillType)
        {
            _player = player;
            _currentSkill = skillType;

            var skill = player.GetSkills()?.GetSkillList().FirstOrDefault(s => s.m_info.m_skill == skillType);
            _skillSprite = skill?.m_info.m_icon;
            if (_skillIcon != null && _skillSprite != null)
            {
                _skillIcon.sprite = _skillSprite;
            }

            string locName = Localization.instance != null
                ? Localization.instance.Localize("$skill_" + skillType.ToString().ToLower())
                : skillType.ToString();

            if (_headerTitle != null)
            {
                _headerTitle.text = $"<b><color=#E5A93C>{locName.ToUpper()}</color></b>";
            }

            if (_selectionShelf != null) _selectionShelf.SetActive(false);
            HideHoverTooltip();

            RebuildAll();
            gameObject.SetActive(true);
            UIHelper.PlaySfx("sfx_gui_button");

            float currentLvl = player.GetSkills()?.GetSkillLevel(skillType) ?? 1f;
            ScrollToLevel(Mathf.FloorToInt(currentLvl));
        }

        public void Close()
        {
            HideHoverTooltip();
            if (_selectionShelf != null) _selectionShelf.SetActive(false);
            gameObject.SetActive(false);
            UIHelper.PlaySfx("sfx_gui_button");
            _onCloseCallback?.Invoke();
        }

        private void RebuildAll()
        {
            if (_player == null) return;

            float savedNorm = _timelineScrollRect != null ? _timelineScrollRect.horizontalNormalizedPosition : 0f;

            float currentLevel = _player.GetSkills()?.GetSkillLevel(_currentSkill) ?? 0f;
            var skill = _player.GetSkills()?.GetSkillList().FirstOrDefault(s => s.m_info.m_skill == _currentSkill);

            // 1. Update Left Overview Panel
            if (_leftLevelText != null)
            {
                _leftLevelText.text = $"<b><color=#E5A93C>Level {Mathf.FloorToInt(currentLevel)}</color></b>";
            }
            if (_leftXpFill != null && skill != null)
            {
                _leftXpFill.fillAmount = Mathf.Clamp01(skill.GetLevelPercentage());
            }
            if (_leftPassivesLabels != null && _leftPassivesValues != null)
            {
                var bonuses = SkillBonusCalculator.GetActiveBonuses(_currentSkill, currentLevel);
                var labelLines = new List<string>();
                var valueLines = new List<string>();

                foreach (var b in bonuses)
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

                _leftPassivesLabels.text = string.Join("\n", labelLines);
                _leftPassivesValues.text = string.Join("\n", valueLines);
            }

            RebuildLeftSocketRow();

            // 2. Rebuild Timeline Nodes & Line Segments (No lines through nodes)
            RebuildTimeline(currentLevel);

            if (_timelineScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _timelineScrollRect.horizontalNormalizedPosition = savedNorm;
                _timelineSlider?.SetValueWithoutNotify(savedNorm);
            }
        }

        private void RebuildLeftSocketRow()
        {
            if (_leftSocketedRow == null || _player == null) return;

            for (int i = _leftSocketedRow.childCount - 1; i >= 0; i--)
            {
                Destroy(_leftSocketedRow.GetChild(i).gameObject);
            }

            var font = UIHelper.GetValheimFont();

            foreach (int m in KeystoneRegistry.Milestones)
            {
                bool isUnlocked = (_player.GetSkills()?.GetSkillLevel(_currentSkill) ?? 0f) >= m;
                var active = KeystoneManager.GetSelectedKeystone(_player, _currentSkill, m);

                var iconBtnObj = new GameObject($"SocketIconBtn_{m}", typeof(RectTransform), typeof(Image), typeof(Button));
                iconBtnObj.transform.SetParent(_leftSocketedRow, false);
                var ibRect = iconBtnObj.GetComponent<RectTransform>();
                ibRect.sizeDelta = new Vector2(58f, 58f);

                var ringImg = iconBtnObj.GetComponent<Image>();
                ringImg.sprite = UIHelper.GetSocketRing();
                ringImg.color = isUnlocked
                    ? (active != null ? UIHelper.GoldAccent : Color.white)
                    : new Color(0.45f, 0.35f, 0.25f, 0.65f);

                // Circular Mask to ensure icon is strictly clipped inside circular slot
                var maskObj = new GameObject("IconMask", typeof(RectTransform), typeof(Image), typeof(Mask));
                maskObj.transform.SetParent(iconBtnObj.transform, false);
                var mRect = maskObj.GetComponent<RectTransform>();
                mRect.anchorMin = new Vector2(0.5f, 0.5f);
                mRect.anchorMax = new Vector2(0.5f, 0.5f);
                mRect.pivot = new Vector2(0.5f, 0.5f);
                mRect.sizeDelta = new Vector2(48f, 48f);
                var mImg = maskObj.GetComponent<Image>();
                mImg.sprite = UIHelper.GetCircleMask();
                var mask = maskObj.GetComponent<Mask>();
                mask.showMaskGraphic = false;

                // Inner Graphic (Icon if socketed, or empty frame / label)
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
                    var sprite = active.GetIcon(_player) ?? _skillSprite ?? UIHelper.GetWhiteSprite();
                    iconImg.sprite = sprite;
                    iconImg.color = Color.white;
                }
                else if (isUnlocked)
                {
                    iconImg.sprite = UIHelper.GetSocketEmpty();
                    iconImg.color = UIHelper.AmberGoldGlow;
                }
                else
                {
                    // Locked milestone level text
                    iconImg.color = Color.clear;
                    var numObj = new GameObject("Num", typeof(RectTransform), typeof(TextMeshProUGUI));
                    numObj.transform.SetParent(iconBtnObj.transform, false);
                    var nRect = numObj.GetComponent<RectTransform>();
                    nRect.anchorMin = Vector2.zero;
                    nRect.anchorMax = Vector2.one;
                    nRect.offsetMin = Vector2.zero;
                    nRect.offsetMax = Vector2.zero;
                    var nTmp = numObj.GetComponent<TextMeshProUGUI>();
                    if (font != null) nTmp.font = font;
                    nTmp.text = $"{m}";
                    nTmp.fontSize = 11f;
                    nTmp.alignment = TextAlignmentOptions.Center;
                    nTmp.color = UIHelper.MutedParchment;
                }

                // Hover trigger to show description on hover
                var hoverTrigger = iconBtnObj.AddComponent<KeystoneIconHoverTrigger>();
                hoverTrigger.Init(active, m, isUnlocked, this);

                int milestoneCaptured = m;
                iconBtnObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OpenSelectionShelf(milestoneCaptured);
                    UIHelper.PlaySfx("sfx_gui_button");
                });
            }
        }

        private void RebuildTimeline(float currentLevel)
        {
            if (_timelineContent == null || _player == null) return;

            foreach (var go in _timelineNodeObjects)
            {
                if (go != null) Destroy(go);
            }
            _timelineNodeObjects.Clear();

            var font = UIHelper.GetValheimFont();

            // 1. Draw segmented conduit lines between nodes (physically stopping at node perimeters)
            for (int lvl = 1; lvl < 100; lvl++)
            {
                float xA = StartX + (lvl - 1) * LevelSpacing;
                float xB = StartX + lvl * LevelSpacing;

                float rA = GetNodeRadius(lvl);
                float rB = GetNodeRadius(lvl + 1);

                float startX = xA + rA + 1f;
                float endX = xB - rB - 1f;
                float segWidth = Mathf.Max(2f, endX - startX);
                float centerX = (startX + endX) * 0.5f;

                CreateConduitSegment(lvl, centerX, segWidth, currentLevel);
            }

            // 2. Draw nodes on top of the conduit line ends
            for (int lvl = 1; lvl <= 100; lvl++)
            {
                bool isUnlocked = currentLevel >= lvl;
                bool isDecade = lvl % 10 == 0 && lvl != 20 && lvl != 40 && lvl != 60 && lvl != 80 && lvl != 100;
                bool isSocket = lvl == 20 || lvl == 40 || lvl == 60 || lvl == 80 || lvl == 100;

                float xPos = StartX + (lvl - 1) * LevelSpacing;

                if (isSocket)
                {
                    CreateSocketNode(lvl, xPos, isUnlocked, font);
                }
                else if (isDecade)
                {
                    CreateDecadeNode(lvl, xPos, isUnlocked, font);
                }
                else
                {
                    CreateMinorNode(lvl, xPos, isUnlocked, font);
                }
            }
        }

        private float GetNodeRadius(int level)
        {
            if (level == 20 || level == 40 || level == 60 || level == 80 || level == 100)
            {
                return 44f; // Socket radius (88px diameter)
            }
            if (level % 10 == 0)
            {
                return 14f; // Decade radius (28px diameter)
            }
            return 8f; // Minor node radius (16px diameter)
        }

        private void CreateConduitSegment(int fromLevel, float centerX, float width, float currentLevel)
        {
            var segObj = new GameObject($"Seg_{fromLevel}_{fromLevel + 1}", typeof(RectTransform), typeof(Image));
            segObj.transform.SetParent(_timelineContent!.transform, false);
            var rect = segObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(centerX, 0f);
            rect.sizeDelta = new Vector2(width, 6f);

            var bgImg = segObj.GetComponent<Image>();
            bgImg.sprite = UIHelper.GetConduitTrack();
            bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0.08f, 0.06f, 0.05f, 0.95f);

            // Active blue fill
            var fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(segObj.transform, false);
            var fRect = fillObj.GetComponent<RectTransform>();
            fRect.anchorMin = Vector2.zero;
            fRect.anchorMax = Vector2.one;
            fRect.offsetMin = Vector2.zero;
            fRect.offsetMax = Vector2.zero;

            var fillImg = fillObj.GetComponent<Image>();
            fillImg.sprite = UIHelper.GetConduitFill();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.color = UIHelper.BrightBlueConduit;

            if (currentLevel >= fromLevel + 1)
            {
                fillImg.fillAmount = 1.0f;
            }
            else if (currentLevel > fromLevel)
            {
                fillImg.fillAmount = Mathf.Clamp01(currentLevel - fromLevel);
            }
            else
            {
                fillImg.fillAmount = 0.0f;
            }

            _timelineNodeObjects.Add(segObj);
        }

        private void CreateMinorNode(int level, float xPos, bool isUnlocked, TMP_FontAsset? font)
        {
            var nodeObj = new GameObject($"MinorNode_{level}", typeof(RectTransform), typeof(Image));
            nodeObj.transform.SetParent(_timelineContent!.transform, false);
            var rect = nodeObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(xPos, 0f);
            rect.sizeDelta = new Vector2(16f, 16f);

            var img = nodeObj.GetComponent<Image>();
            img.sprite = UIHelper.GetNodeCircle();
            img.color = isUnlocked ? UIHelper.AmberGoldGlow : new Color(0.24f, 0.18f, 0.12f, 0.90f);

            var trigger = nodeObj.AddComponent<TimelineNodeHoverTrigger>();
            trigger.Init(level, isUnlocked, this);

            _timelineNodeObjects.Add(nodeObj);
        }

        private void CreateDecadeNode(int level, float xPos, bool isUnlocked, TMP_FontAsset? font)
        {
            var nodeObj = new GameObject($"DecadeNode_{level}", typeof(RectTransform), typeof(Image));
            nodeObj.transform.SetParent(_timelineContent!.transform, false);
            var rect = nodeObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(xPos, 0f);
            rect.sizeDelta = new Vector2(28f, 28f);

            var img = nodeObj.GetComponent<Image>();
            img.sprite = UIHelper.GetNodeDecade();
            img.color = isUnlocked ? new Color(1f, 1f, 1f, 1f) : new Color(0.45f, 0.35f, 0.25f, 0.85f);

            // Label above
            var lblObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(nodeObj.transform, false);
            var lRect = lblObj.GetComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0.5f, 1f);
            lRect.anchorMax = new Vector2(0.5f, 1f);
            lRect.pivot = new Vector2(0.5f, 0f);
            lRect.anchoredPosition = new Vector2(0f, 4f);
            lRect.sizeDelta = new Vector2(40f, 16f);
            var tmp = lblObj.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = $"{level}";
            tmp.fontSize = 10f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = isUnlocked ? UIHelper.GoldAccent : UIHelper.MutedParchment;

            var trigger = nodeObj.AddComponent<TimelineNodeHoverTrigger>();
            trigger.Init(level, isUnlocked, this);

            _timelineNodeObjects.Add(nodeObj);
        }

        private void CreateSocketNode(int milestone, float xPos, bool isUnlocked, TMP_FontAsset? font)
        {
            var socketObj = new GameObject($"SocketNode_{milestone}", typeof(RectTransform), typeof(Image), typeof(Button));
            socketObj.transform.SetParent(_timelineContent!.transform, false);
            var rect = socketObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(xPos, 0f);
            rect.sizeDelta = new Vector2(88f, 88f);

            var active = _player != null ? KeystoneManager.GetSelectedKeystone(_player, _currentSkill, milestone) : null;

            // Outer Metallic Socket Ring
            var ringImg = socketObj.GetComponent<Image>();
            ringImg.sprite = UIHelper.GetSocketRing();
            ringImg.color = isUnlocked
                ? (active != null ? UIHelper.GoldAccent : Color.white)
                : new Color(0.45f, 0.35f, 0.25f, 0.70f);

            // Number above socket node (Identical to decade nodes, no "Lv." prefix)
            var lblObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(socketObj.transform, false);
            var lRect = lblObj.GetComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0.5f, 1f);
            lRect.anchorMax = new Vector2(0.5f, 1f);
            lRect.pivot = new Vector2(0.5f, 0f);
            lRect.anchoredPosition = new Vector2(0f, 4f);
            lRect.sizeDelta = new Vector2(40f, 16f);
            var tmp = lblObj.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = $"{milestone}";
            tmp.fontSize = 11f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = isUnlocked ? UIHelper.GoldAccent : UIHelper.MutedParchment;

            // Circular Mask clipping the icon inside the circular socket
            var maskObj = new GameObject("IconMask", typeof(RectTransform), typeof(Image), typeof(Mask));
            maskObj.transform.SetParent(socketObj.transform, false);
            var mRect = maskObj.GetComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0.5f, 0.5f);
            mRect.anchorMax = new Vector2(0.5f, 0.5f);
            mRect.pivot = new Vector2(0.5f, 0.5f);
            mRect.sizeDelta = new Vector2(72f, 72f);
            var mImg = maskObj.GetComponent<Image>();
            mImg.sprite = UIHelper.GetCircleMask();
            var mask = maskObj.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            // Inner Keystone Icon
            var iconObj = new GameObject("KeystoneIcon", typeof(RectTransform), typeof(Image));
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
                var sprite = active.GetIcon(_player) ?? _skillSprite ?? UIHelper.GetWhiteSprite();
                iconImg.sprite = sprite;
                iconImg.color = Color.white;
            }
            else if (isUnlocked)
            {
                iconImg.sprite = UIHelper.GetSocketEmpty();
                iconImg.color = UIHelper.BrightBlueGlow;
            }
            else
            {
                // Muted empty socket graphic, no locked text
                iconImg.sprite = UIHelper.GetSocketEmpty();
                iconImg.color = new Color(0.35f, 0.40f, 0.50f, 0.45f);
            }

            // Milestone Banner Below Socket
            var bannerObj = new GameObject("Banner", typeof(RectTransform), typeof(TextMeshProUGUI));
            bannerObj.transform.SetParent(socketObj.transform, false);
            var bRect = bannerObj.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0f);
            bRect.anchorMax = new Vector2(0.5f, 0f);
            bRect.pivot = new Vector2(0.5f, 1f);
            bRect.anchoredPosition = new Vector2(0f, -4f);
            bRect.sizeDelta = new Vector2(110f, 24f);
            var bTmp = bannerObj.GetComponent<TextMeshProUGUI>();
            if (font != null) bTmp.font = font;
            bTmp.alignment = TextAlignmentOptions.Top;
            bTmp.fontSize = 10f;

            if (active != null)
            {
                bTmp.text = $"<b><color=#E2E8F0>{active.Name}</color></b>";
            }
            else if (isUnlocked)
            {
                bTmp.text = "<b><color=#38BDF8>Choose Talent</color></b>";
            }
            else
            {
                bTmp.text = ""; // Drop locked text completely
            }

            int mCaptured = milestone;
            socketObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                OpenSelectionShelf(mCaptured);
                UIHelper.PlaySfx("sfx_gui_button");
            });

            var trigger = socketObj.AddComponent<KeystoneIconHoverTrigger>();
            trigger.Init(active, milestone, isUnlocked, this);

            _timelineNodeObjects.Add(socketObj);
        }

        public void OpenSelectionShelf(int milestone)
        {
            if (_selectionShelf == null || _shelfOptionsContainer == null || _player == null) return;

            _activeShelfMilestone = milestone;
            ScrollToLevel(milestone);

            for (int i = _shelfOptionsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_shelfOptionsContainer.GetChild(i).gameObject);
            }

            bool isUnlocked = KeystoneManager.IsMilestoneUnlocked(_player, _currentSkill, milestone);

            if (_shelfTitle != null)
            {
                if (isUnlocked)
                {
                    _shelfTitle.text = $"<b><color=#F59E0B>LEVEL {milestone} KEYSTONES</color></b>\n<size=75%><color=#94A3B8>Click an icon to socket or switch your talent.</color></size>";
                }
                else
                {
                    _shelfTitle.text = $"<b><color=#94A3B8>LEVEL {milestone} KEYSTONES</color></b>\n<size=75%><color=#EF4444>🔒 Locked (Requires Skill Level {milestone})</color></size>";
                }
            }

            var keystones = KeystoneRegistry.GetKeystonesForMilestone(_currentSkill, milestone);
            var active = KeystoneManager.GetSelectedKeystone(_player, _currentSkill, milestone);
            var font = UIHelper.GetValheimFont();

            foreach (var k in keystones)
            {
                bool isSelected = active != null && active.Id == k.Id;

                var cardObj = new GameObject($"Card_{k.Id}", typeof(RectTransform), typeof(Image));
                cardObj.transform.SetParent(_shelfOptionsContainer, false);
                var cImg = cardObj.GetComponent<Image>();
                cImg.sprite = UIHelper.GetGlassCard();
                cImg.type = Image.Type.Sliced;
                if (isSelected)
                {
                    cImg.color = new Color(0.24f, 0.17f, 0.11f, 0.98f);
                }
                else if (!isUnlocked)
                {
                    cImg.color = new Color(0.10f, 0.08f, 0.06f, 0.95f);
                }
                else
                {
                    cImg.color = new Color(0.14f, 0.10f, 0.08f, 0.96f);
                }

                // Keystone Icon Crest Button (Click icon to select keystone) - Scaled up for prominence
                var iconBtnObj = new GameObject("IconCrestBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                iconBtnObj.transform.SetParent(cardObj.transform, false);
                var ibRect = iconBtnObj.GetComponent<RectTransform>();
                ibRect.anchorMin = new Vector2(0.5f, 1f);
                ibRect.anchorMax = new Vector2(0.5f, 1f);
                ibRect.pivot = new Vector2(0.5f, 1f);
                ibRect.anchoredPosition = new Vector2(0f, -16f);
                ibRect.sizeDelta = new Vector2(116f, 116f);

                var ibImg = iconBtnObj.GetComponent<Image>();
                ibImg.sprite = UIHelper.GetSocketRing();
                ibImg.color = isSelected ? UIHelper.GoldAccent : (isUnlocked ? Color.white : new Color(0.55f, 0.45f, 0.35f, 0.75f));

                var iconBtn = iconBtnObj.GetComponent<Button>();
                var colors = iconBtn.colors;
                if (isSelected)
                {
                    colors.normalColor = UIHelper.GoldAccent;
                    colors.highlightedColor = new Color(1.3f, 1.3f, 1.4f, 1f);
                    colors.pressedColor = UIHelper.GoldAccent;
                }
                else if (isUnlocked)
                {
                    colors.normalColor = Color.white;
                    colors.highlightedColor = new Color(1.3f, 1.3f, 1.4f, 1f);
                    colors.pressedColor = UIHelper.GoldAccent;
                }
                else
                {
                    colors.normalColor = new Color(0.65f, 0.55f, 0.45f, 0.9f);
                    colors.highlightedColor = new Color(0.85f, 0.75f, 0.65f, 1f);
                    colors.pressedColor = new Color(0.50f, 0.40f, 0.30f, 1f);
                }
                iconBtn.colors = colors;

                // Circular Mask clipping the icon inside the circular slot
                var maskObj = new GameObject("IconMask", typeof(RectTransform), typeof(Image), typeof(Mask));
                maskObj.transform.SetParent(iconBtnObj.transform, false);
                var mRect = maskObj.GetComponent<RectTransform>();
                mRect.anchorMin = new Vector2(0.5f, 0.5f);
                mRect.anchorMax = new Vector2(0.5f, 0.5f);
                mRect.pivot = new Vector2(0.5f, 0.5f);
                mRect.sizeDelta = new Vector2(98f, 98f);
                var mImg = maskObj.GetComponent<Image>();
                mImg.sprite = UIHelper.GetCircleMask();
                var mask = maskObj.GetComponent<Mask>();
                mask.showMaskGraphic = false;

                var iconImgObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconImgObj.transform.SetParent(maskObj.transform, false);
                var iiRect = iconImgObj.GetComponent<RectTransform>();
                iiRect.anchorMin = Vector2.zero;
                iiRect.anchorMax = Vector2.one;
                iiRect.offsetMin = Vector2.zero;
                iiRect.offsetMax = Vector2.zero;
                var iiImg = iconImgObj.GetComponent<Image>();
                iiImg.preserveAspect = true;
                iiImg.sprite = k.GetIcon(_player) ?? _skillSprite ?? UIHelper.GetWhiteSprite();
                if (!isUnlocked && !isSelected)
                {
                    iiImg.color = new Color(0.85f, 0.88f, 0.92f, 0.90f);
                }

                // Badge indicator (Active if selected, Locked if milestone not yet reached)
                bool hasBadge = isSelected || !isUnlocked;
                if (isSelected)
                {
                    var selBadge = new GameObject("SelectedBadge", typeof(RectTransform), typeof(TextMeshProUGUI));
                    selBadge.transform.SetParent(cardObj.transform, false);
                    var sbRect = selBadge.GetComponent<RectTransform>();
                    sbRect.anchorMin = new Vector2(0.5f, 1f);
                    sbRect.anchorMax = new Vector2(0.5f, 1f);
                    sbRect.pivot = new Vector2(0.5f, 1f);
                    sbRect.anchoredPosition = new Vector2(0f, -138f);
                    sbRect.sizeDelta = new Vector2(160f, 18f);
                    var sbTmp = selBadge.GetComponent<TextMeshProUGUI>();
                    if (font != null) sbTmp.font = font;
                    sbTmp.fontSize = 10.5f;
                    sbTmp.alignment = TextAlignmentOptions.Center;
                    sbTmp.text = "<color=#F59E0B><b>★ ACTIVE</b></color>";
                }
                else if (!isUnlocked)
                {
                    var lockBadge = new GameObject("LockedBadge", typeof(RectTransform), typeof(TextMeshProUGUI));
                    lockBadge.transform.SetParent(cardObj.transform, false);
                    var lbRect = lockBadge.GetComponent<RectTransform>();
                    lbRect.anchorMin = new Vector2(0.5f, 1f);
                    lbRect.anchorMax = new Vector2(0.5f, 1f);
                    lbRect.pivot = new Vector2(0.5f, 1f);
                    lbRect.anchoredPosition = new Vector2(0f, -138f);
                    lbRect.sizeDelta = new Vector2(170f, 18f);
                    var lbTmp = lockBadge.GetComponent<TextMeshProUGUI>();
                    if (font != null) lbTmp.font = font;
                    lbTmp.fontSize = 10.5f;
                    lbTmp.alignment = TextAlignmentOptions.Center;
                    lbTmp.text = $"<color=#EF4444><b>🔒 REQUIRES LV. {milestone}</b></color>";
                }

                // Keystone Name Header
                var rHeadObj = new GameObject("KeystoneName", typeof(RectTransform), typeof(TextMeshProUGUI));
                rHeadObj.transform.SetParent(cardObj.transform, false);
                var rhRect = rHeadObj.GetComponent<RectTransform>();
                rhRect.anchorMin = new Vector2(0f, 1f);
                rhRect.anchorMax = new Vector2(1f, 1f);
                rhRect.pivot = new Vector2(0.5f, 1f);
                rhRect.anchoredPosition = new Vector2(0f, hasBadge ? -162f : -140f);
                rhRect.sizeDelta = new Vector2(-16f, 26f);
                var rhTmp = rHeadObj.GetComponent<TextMeshProUGUI>();
                if (font != null) rhTmp.font = font;
                rhTmp.fontSize = 14.5f;
                rhTmp.alignment = TextAlignmentOptions.Center;
                string nameColor = isSelected ? "#F59E0B" : (isUnlocked ? "#F3EBE1" : "#B8A99A");
                rhTmp.text = $"<b><color={nameColor}>{k.Name}</color></b>";

                // Description Box (Extended area, fills remaining card height since socket talent button is removed)
                var descObj = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
                descObj.transform.SetParent(cardObj.transform, false);
                var dRect = descObj.GetComponent<RectTransform>();
                dRect.anchorMin = Vector2.zero;
                dRect.anchorMax = Vector2.one;
                dRect.offsetMin = new Vector2(16f, 16f);
                dRect.offsetMax = new Vector2(-16f, hasBadge ? -194f : -172f);
                var dTmp = descObj.GetComponent<TextMeshProUGUI>();
                if (font != null) dTmp.font = font;
                dTmp.fontSize = 11.5f;
                dTmp.lineSpacing = 1.15f;
                dTmp.text = $"<color=#EDE4D8>{k.Description}</color>";
                dTmp.alignment = TextAlignmentOptions.Center;

                // Selection by pressing icon
                string kId = k.Id;
                iconBtn.onClick.AddListener(() =>
                {
                    if (!isUnlocked)
                    {
                        UIHelper.PlaySfx("sfx_gui_cancel");
                        return;
                    }

                    if (_player != null)
                    {
                        KeystoneManager.SelectKeystone(_player, _currentSkill, milestone, kId);
                        UIHelper.PlaySfx("sfx_build_hammer");
                        _selectionShelf.SetActive(false);
                        RebuildAll();
                    }
                });
            }

            _selectionShelf.SetActive(true);
        }
    }

    public class TimelineNodeHoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private int _level;
        private bool _isUnlocked;
        private SkillTreeDrawerUI? _drawer;

        public void Init(int level, bool isUnlocked, SkillTreeDrawerUI drawer)
        {
            _level = level;
            _isUnlocked = isUnlocked;
            _drawer = drawer;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _drawer?.ShowIncrementalTooltip(_level, _isUnlocked, transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _drawer?.HideHoverTooltip();
        }
    }

    public class KeystoneIconHoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private KeystoneDefinition? _keystone;
        private int _milestone;
        private bool _isUnlocked;
        private SkillTreeDrawerUI? _drawer;

        public void Init(KeystoneDefinition? keystone, int milestone, bool isUnlocked, SkillTreeDrawerUI drawer)
        {
            _keystone = keystone;
            _milestone = milestone;
            _isUnlocked = isUnlocked;
            _drawer = drawer;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _drawer?.ShowKeystoneTooltip(_keystone, _milestone, _isUnlocked, transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _drawer?.HideHoverTooltip();
        }
    }
}
