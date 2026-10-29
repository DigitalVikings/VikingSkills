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
            _modalWindow = transform.CreateChild("ModalWindow", typeof(RectTransform));
            var modalRect = _modalWindow.GetComponent<RectTransform>();
            modalRect.SetAnchors(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1280f, 620f));

            // Modal Soft Drop Shadow
            var shadowImg = _modalWindow.transform.AddImage("ModalShadow", UIHelper.GetSoftShadow(), new Color(0f, 0f, 0f, 0.75f), Image.Type.Sliced);
            shadowImg.rectTransform.Fill(left: -16f, bottom: -16f, right: -16f, top: -16f);

            // Modal Glass Body
            var modalBg = _modalWindow.transform.AddImage("ModalGlass", UIHelper.GetGlassPanel(), Color.white, Image.Type.Sliced);
            modalBg.rectTransform.Fill();

            // 3. Top Header Bar (56px) - Plain title, no "Skill Tree"
            BuildHeader(modalBg.transform, font);

            // 4. Left Pinned Cumulative Overview Panel (280px)
            BuildLeftOverviewPanel(modalBg.transform, font);

            // 5. Right Timeline Viewport (Levels 1 to 100)
            BuildTimelineViewport(modalBg.transform, font);

            // 6. Floating Selection Shelf (Hidden by default)
            BuildSelectionShelf(modalBg.transform, font);

            // 7. Hover Tooltip (Hidden by default)
            BuildHoverTooltip(modalBg.transform, font);
        }

        private void BuildHeader(Transform parent, TMP_FontAsset? font)
        {
            var topBar = parent.AddImage("HeaderBar", UIHelper.GetGlassCard(), new Color(0.12f, 0.09f, 0.07f, 0.98f), Image.Type.Sliced);
            topBar.rectTransform.SetAnchors(new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 56f));

            // Icon Crest (Top-Left)
            var iconBackdrop = topBar.transform.AddImage("IconBackdrop", UIHelper.GetNodeCircle(), new Color(0.16f, 0.12f, 0.09f, 0.95f));
            iconBackdrop.rectTransform.SetAnchors(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(50f, 50f));

            _skillIcon = iconBackdrop.transform.AddImage("Icon", null, Color.white);
            _skillIcon.rectTransform.SetAnchors(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(38f, 38f));
            _skillIcon.preserveAspect = true;

            // Title (Plain title, e.g. "CLUBS", "SWORDS" - NO "Skill Tree")
            _headerTitle = topBar.transform.AddText("Title", "", font, 17f, UIHelper.LightText, TextAlignmentOptions.MidlineLeft);
            _headerTitle.rectTransform.SetAnchors(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(80f, 0f), new Vector2(400f, 28f));

            // Return Button (Standardized shared UI element on top-right)
            UIHelper.CreateReturnButton(topBar.transform, new Vector2(-20f, 0f), new Vector2(146f, 44f),
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                "RETURN", Close, font);
        }

        private void BuildLeftOverviewPanel(Transform parent, TMP_FontAsset? font)
        {
            var leftPanel = parent.AddImage("LeftOverviewPanel", UIHelper.GetGlassCard(), new Color(0.09f, 0.07f, 0.05f, 0.96f), Image.Type.Sliced);
            leftPanel.rectTransform.SetAnchors(new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(14f, -28f), new Vector2(340f, -70f));

            // 1. Level Display
            _leftLevelText = leftPanel.transform.AddText("LevelText", "", font, 20f, Color.white, TextAlignmentOptions.Center);
            _leftLevelText.rectTransform.SetAnchors(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(300f, 32f));

            // 2. XP Progress Track
            var xpTrack = leftPanel.transform.AddImage("XpTrack", UIHelper.GetConduitTrack(), new Color(0.08f, 0.06f, 0.05f, 0.95f), Image.Type.Sliced);
            xpTrack.rectTransform.SetAnchors(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(300f, 6f));

            _leftXpFill = xpTrack.transform.AddImage("XpFill", UIHelper.GetWhiteSprite(), UIHelper.BrightBlueConduit, Image.Type.Filled);
            _leftXpFill.rectTransform.Fill();
            _leftXpFill.fillMethod = Image.FillMethod.Horizontal;
            _leftXpFill.fillOrigin = (int)Image.OriginHorizontal.Left;

            // 3. Divider
            CreateHorizontalLine(leftPanel.transform, -68f);

            // 4. "CUMULATIVE PASSIVES" Title & Content
            var ptTmp = leftPanel.transform.AddText("PassivesTitle", "<color=#E5A93C><b>CUMULATIVE PASSIVES</b></color>", font, 11f, Color.white, TextAlignmentOptions.Center);
            ptTmp.rectTransform.SetAnchors(new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -76f), new Vector2(-28f, 16f));

            // Passives Two-Column Container
            var passContainer = leftPanel.transform.CreateChild("PassivesContainer", typeof(RectTransform));
            var pcRect = passContainer.GetComponent<RectTransform>();
            pcRect.SetAnchors(new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(-28f, 160f));

            _leftPassivesLabels = passContainer.transform.AddText("PassivesLabels", "", font, 10.5f, Color.white, TextAlignmentOptions.TopLeft);
            _leftPassivesLabels.rectTransform.Fill(right: 65f);
            _leftPassivesLabels.lineSpacing = 1.15f;

            _leftPassivesValues = passContainer.transform.AddText("PassivesValues", "", font, 10.5f, Color.white, TextAlignmentOptions.TopRight);
            _leftPassivesValues.rectTransform.Fill(left: 170f);
            _leftPassivesValues.lineSpacing = 1.15f;

            // 5. Divider
            CreateHorizontalLine(leftPanel.transform, -270f);

            // 6. "KEYSTONES" Section Header
            var stTmp = leftPanel.transform.AddText("SocketTitle", "<color=#E5A93C><b>KEYSTONE SOCKETS</b></color>", font, 11f, Color.white, TextAlignmentOptions.Center);
            stTmp.rectTransform.SetAnchors(new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -280f), new Vector2(-28f, 16f));

            // 7. Horizontal Row of 5 Circular Keystone Socket Icons
            var sockRowObj = leftPanel.transform.CreateChild("SocketRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var scRect = sockRowObj.GetComponent<RectTransform>();
            scRect.SetAnchors(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(320f, 66f));
            var hlg = sockRowObj.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            _leftSocketedRow = sockRowObj.transform;
        }

        private void CreateHorizontalLine(Transform parent, float yOffset)
        {
            var line = parent.AddImage("Line", UIHelper.GetWhiteSprite(), new Color(0.40f, 0.30f, 0.20f, 0.50f));
            line.rectTransform.SetAnchors(new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, yOffset), new Vector2(-24f, 1.5f));
        }

        private void BuildTimelineViewport(Transform parent, TMP_FontAsset? font)
        {
            // Viewport bounds
            var scrollObj = parent.CreateChild("TimelineScroll", typeof(RectTransform), typeof(ScrollRect));
            var scrollRect = scrollObj.GetComponent<RectTransform>();
            scrollRect.SetAnchors(Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            scrollRect.offsetMin = new Vector2(368f, 52f);
            scrollRect.offsetMax = new Vector2(-16f, -64f);

            _timelineScrollRect = scrollObj.GetComponent<ScrollRect>();
            _timelineScrollRect.horizontal = true;
            _timelineScrollRect.vertical = false;
            _timelineScrollRect.movementType = ScrollRect.MovementType.Clamped;
            _timelineScrollRect.scrollSensitivity = 40f;

            // Viewport Mask
            var vpObj = scrollObj.transform.CreateChild("Viewport", typeof(RectTransform), typeof(RectMask2D));
            var vpRect = vpObj.GetComponent<RectTransform>();
            vpRect.Fill();
            _timelineScrollRect.viewport = vpRect;

            // Content Track
            var contentObj = vpObj.transform.CreateChild("Content", typeof(RectTransform));
            _timelineContent = contentObj.GetComponent<RectTransform>();
            _timelineContent.SetAnchors(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(TimelineWidth, 460f));
            _timelineScrollRect.content = _timelineContent;

            // Draggable Timeline Slider at bottom
            BuildTimelineSlider(parent, font);
        }

        private void BuildTimelineSlider(Transform parent, TMP_FontAsset? font)
        {
            var sliderRootObj = parent.CreateChild("TimelineSliderRoot", typeof(RectTransform));
            var rootRect = sliderRootObj.GetComponent<RectTransform>();
            rootRect.SetAnchors(new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(150f, 16f), new Vector2(-400f, 22f));

            // Left Label: "Lv. 1"
            var llTmp = sliderRootObj.transform.AddText("LeftLbl", "<b>Lv. 1</b>", font, 11f, UIHelper.MutedText, TextAlignmentOptions.Right);
            llTmp.rectTransform.SetAnchors(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(50f, 20f));

            // Right Label: "Lv. 100"
            var rlTmp = sliderRootObj.transform.AddText("RightLbl", "<b>Lv. 100</b>", font, 11f, UIHelper.MutedText, TextAlignmentOptions.Left);
            rlTmp.rectTransform.SetAnchors(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(60f, 20f));

            // Slider Component
            var sliderObj = sliderRootObj.transform.CreateChild("Slider", typeof(RectTransform), typeof(Slider));
            var sRect = sliderObj.GetComponent<RectTransform>();
            sRect.Fill();

            _timelineSlider = sliderObj.GetComponent<Slider>();
            _timelineSlider.direction = Slider.Direction.LeftToRight;
            _timelineSlider.minValue = 0f;
            _timelineSlider.maxValue = 1f;

            // Background Track
            var tImg = sliderObj.transform.AddImage("Background", UIHelper.GetConduitTrack(), new Color(0.08f, 0.10f, 0.15f, 0.95f), Image.Type.Sliced);
            tImg.rectTransform.SetAnchors(new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 6f));

            // Handle Slide Area
            var handleAreaObj = sliderObj.transform.CreateChild("Handle Slide Area", typeof(RectTransform));
            var haRect = handleAreaObj.GetComponent<RectTransform>();
            haRect.Fill(left: 24f, right: 24f);

            // Handle
            var hImg = handleAreaObj.transform.AddImage("Handle", UIHelper.GetGlassCard(), new Color(0.22f, 0.32f, 0.50f, 0.98f), Image.Type.Sliced);
            hImg.rectTransform.sizeDelta = new Vector2(48f, 18f);

            _timelineSlider.targetGraphic = hImg;
            _timelineSlider.handleRect = hImg.rectTransform;

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
            var shelfImg = parent.AddImage("SelectionShelf", UIHelper.GetGlassPanel(), new Color(0.11f, 0.08f, 0.06f, 0.98f), Image.Type.Sliced);
            _selectionShelf = shelfImg.gameObject;
            shelfImg.rectTransform.SetAnchors(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(140f, 0f), new Vector2(820f, 500f));

            // Single Return Button in Top-Right Corner
            UIHelper.CreateReturnButton(_selectionShelf.transform, new Vector2(-16f, -14f), new Vector2(136f, 36f),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                "RETURN", () => _selectionShelf.SetActive(false), font);

            // Shelf Header (Centered, inset from edges)
            _shelfTitle = _selectionShelf.transform.AddText("Title", "", font, 16f, Color.white, TextAlignmentOptions.Center);
            _shelfTitle.rectTransform.SetAnchors(new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(-320f, 32f));

            // Options Container (Horizontal Layout supporting 2, 3, or N choices)
            var optionsObj = _selectionShelf.transform.CreateChild("OptionsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var optRect = optionsObj.GetComponent<RectTransform>();
            optRect.Fill(left: 24f, bottom: 24f, right: 24f, top: 58f);
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
            _hoverTooltip = parent.CreateChild("HoverTooltip", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
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

            _tooltipTitle = _hoverTooltip.transform.AddText("Title", "", font, 13f, Color.white, TextAlignmentOptions.Center);

            _tooltipContent = _hoverTooltip.transform.AddText("Content", "", font, 11f, Color.white, TextAlignmentOptions.Center);
            _tooltipContent.lineSpacing = 1.15f;

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
