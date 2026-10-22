using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VikingSkills.Configuration;
using VikingSkills.Data;
using VikingSkills.Keystones;

namespace VikingSkills.UI
{
    public class PanoramicSkillsDialog : MonoBehaviour
    {
        public static PanoramicSkillsDialog? Instance { get; private set; }
        public static bool IsOpen => Instance != null && Instance.gameObject.activeSelf;

        private ScrollRect? _scrollRect;
        private RectTransform? _contentTransform;
        private TMP_Text? _headerTitle;
        private SkillTreeDrawerUI? _drawer;
        private SkillCategoryType? _activeCategoryFilter = null;
        private readonly List<RuneColumnUI> _activeColumns = new();
        private Player? _cachedPlayer;
        private GameObject? _hoverTooltip;
        private TMP_Text? _tooltipTitle;
        private TMP_Text? _tooltipContent;

        public static void EnsureInitialized(Transform parentCanvas)
        {
            if (Instance != null) return;

            var go = new GameObject("VikingSkills_PanoramicDialog", typeof(RectTransform));
            go.transform.SetParent(parentCanvas, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Instance = go.AddComponent<PanoramicSkillsDialog>();
            Instance.BuildInterface();
            go.SetActive(false);

            KeystoneManager.OnKeystonesChanged += Instance.OnKeystonesChangedHandler;
        }

        private void OnDestroy()
        {
            KeystoneManager.OnKeystonesChanged -= OnKeystonesChangedHandler;
        }

        private void OnKeystonesChangedHandler()
        {
            if (IsOpen && _cachedPlayer != null)
            {
                Populate(_cachedPlayer, preserveScroll: true);
            }
        }

        public static void OpenSkillTree(Skills.SkillType skillType)
        {
            if (Instance != null && Player.m_localPlayer != null && Instance._drawer != null)
            {
                Instance.HideHoverTooltip();
                Instance._drawer.Open(Player.m_localPlayer, skillType);
            }
        }

        public static void OpenSkillTreeWithShelf(Skills.SkillType skillType, int milestone)
        {
            if (Instance != null && Player.m_localPlayer != null && Instance._drawer != null)
            {
                Instance.HideHoverTooltip();
                Instance._drawer.Open(Player.m_localPlayer, skillType);
                Instance._drawer.OpenSelectionShelf(milestone);
            }
        }

        public static void Show(Player player)
        {
            if (!ModConfig.ModEnabled.Value) return;

            if (Instance == null && InventoryGui.instance != null)
            {
                EnsureInitialized(InventoryGui.instance.transform);
            }

            if (Instance != null)
            {
                Instance._cachedPlayer = player;
                Instance.Populate(player);
                Instance.gameObject.SetActive(true);
                UIHelper.PlaySfx("sfx_gui_button");
            }
        }

        public static void Hide()
        {
            if (Instance != null && Instance.gameObject.activeSelf)
            {
                Instance.HideHoverTooltip();
                if (Instance._drawer != null && Instance._drawer.gameObject.activeSelf)
                {
                    Instance._drawer.Close();
                    return;
                }

                Instance.gameObject.SetActive(false);
                UIHelper.PlaySfx("sfx_gui_button");
            }
        }

        private void BuildInterface()
        {
            var font = UIHelper.GetValheimFont();
            var whiteSprite = UIHelper.GetWhiteSprite();

            // 1. Full Screen Dark Peat/Timber Hearth Backdrop
            var backdrop = gameObject.AddComponent<Image>();
            backdrop.sprite = whiteSprite;
            backdrop.color = new Color(0.06f, 0.05f, 0.04f, 0.96f);

            // 2. Unified Top Header Banner (Flat top, bronze bottom trim, flush with top of screen)
            var headerObj = new GameObject("HeaderBanner", typeof(RectTransform), typeof(Image));
            headerObj.transform.SetParent(transform, false);
            var headerRect = headerObj.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0f, 82f);
            var headerImg = headerObj.GetComponent<Image>();
            headerImg.sprite = UIHelper.GetHeaderBanner();
            headerImg.type = Image.Type.Sliced;
            headerImg.color = Color.white;

            // Close / Return Button (Top-Right Corner of Header Banner)
            UIHelper.CreateReturnButton(headerObj.transform, new Vector2(-24f, -10f), new Vector2(136f, 36f),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                "RETURN", Hide, font);

            // Main Title (Upper tier of Header Banner)
            var titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(headerObj.transform, false);
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -12f);
            titleRect.sizeDelta = new Vector2(300f, 24f);
            _headerTitle = titleObj.GetComponent<TextMeshProUGUI>();
            if (font != null) _headerTitle.font = font;
            string skillsLabel = Localization.instance != null ? Localization.instance.Localize("$inventory_skills") : "SKILLS";
            _headerTitle.text = $"<b><color=#E5A93C>—  {skillsLabel.ToUpper()}  —</color></b>";
            _headerTitle.fontSize = 17f;
            _headerTitle.alignment = TextAlignmentOptions.Center;

            // 3. Category Filter Tabs Row (Sleek text tabs seamlessly seated on the bottom rail)
            var filterRowObj = new GameObject("FilterRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            filterRowObj.transform.SetParent(headerObj.transform, false);
            var filterRowRect = filterRowObj.GetComponent<RectTransform>();
            filterRowRect.anchorMin = new Vector2(0.5f, 0f);
            filterRowRect.anchorMax = new Vector2(0.5f, 0f);
            filterRowRect.pivot = new Vector2(0.5f, 0f);
            filterRowRect.anchoredPosition = new Vector2(0f, 0f);
            filterRowRect.sizeDelta = new Vector2(800f, 28f);

            var filterLayout = filterRowObj.GetComponent<HorizontalLayoutGroup>();
            filterLayout.spacing = 16f;
            filterLayout.childAlignment = TextAnchor.MiddleCenter;
            filterLayout.childControlWidth = true;
            filterLayout.childControlHeight = true;
            filterLayout.childForceExpandWidth = false;
            filterLayout.childForceExpandHeight = false;

            CreateFilterButton(filterRowObj.transform, font, whiteSprite, "ALL", null);
            CreateFilterButton(filterRowObj.transform, font, whiteSprite, "WEAPONS", SkillCategoryType.Weapons);
            CreateFilterButton(filterRowObj.transform, font, whiteSprite, "DEFENSE", SkillCategoryType.Defense);
            CreateFilterButton(filterRowObj.transform, font, whiteSprite, "MAGIC", SkillCategoryType.Magic);
            CreateFilterButton(filterRowObj.transform, font, whiteSprite, "MOBILITY", SkillCategoryType.Mobility);
            CreateFilterButton(filterRowObj.transform, font, whiteSprite, "RESOURCES", SkillCategoryType.Resources);

            // 4. Horizontal Panoramic Scroll View (Begins cleanly below the header banner)
            var scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollObj.transform.SetParent(transform, false);
            var scrollRectTransform = scrollObj.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0f, 0f);
            scrollRectTransform.anchorMax = new Vector2(1f, 1f);
            scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
            scrollRectTransform.offsetMin = new Vector2(20f, 20f);
            scrollRectTransform.offsetMax = new Vector2(-20f, -88f);

            _scrollRect = scrollObj.GetComponent<ScrollRect>();
            _scrollRect.horizontal = true;
            _scrollRect.vertical = false;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _scrollRect.scrollSensitivity = 25f;

            // Viewport
            var viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportObj.transform.SetParent(scrollObj.transform, false);
            var viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            _scrollRect.viewport = viewportRect;

            // Content Container
            var contentObj = new GameObject("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            contentObj.transform.SetParent(viewportObj.transform, false);
            _contentTransform = contentObj.GetComponent<RectTransform>();
            _contentTransform.anchorMin = new Vector2(0f, 0f);
            _contentTransform.anchorMax = new Vector2(1f, 1f);
            _contentTransform.pivot = new Vector2(0.5f, 0.5f);
            _contentTransform.offsetMin = Vector2.zero;
            _contentTransform.offsetMax = Vector2.zero;
            _contentTransform.anchoredPosition = Vector2.zero;

            var layout = contentObj.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 18f;
            layout.padding = new RectOffset(20, 20, 10, 10);
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;

            _scrollRect.content = _contentTransform;

            // 5. Skill Tree Detail Drawer Component
            _drawer = SkillTreeDrawerUI.Create(transform, () =>
            {
                if (_cachedPlayer != null) Populate(_cachedPlayer, preserveScroll: true);
            });

            // 7. Hover Tooltip Overlay (Top-level)
            BuildHoverTooltip(transform, font);
        }

        private void BuildHoverTooltip(Transform parent, TMP_FontAsset? font)
        {
            _hoverTooltip = new GameObject("HoverTooltip", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            _hoverTooltip.transform.SetParent(parent, false);
            var rect = _hoverTooltip.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(380f, 0f);

            var img = _hoverTooltip.GetComponent<Image>();
            img.sprite = UIHelper.GetGlassCard();
            img.type = Image.Type.Sliced;
            img.color = new Color(0.04f, 0.06f, 0.09f, 0.98f);

            var vlg = _hoverTooltip.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 12, 12);
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

        public void ShowKeystoneTooltip(VikingSkills.Keystones.KeystoneDefinition? keystone, int milestone, bool isUnlocked, Vector3 worldPos)
        {
            if (_hoverTooltip == null || _tooltipTitle == null || _tooltipContent == null) return;

            if (keystone != null)
            {
                string artNotice = KeystoneIconManager.HasDedicatedIcon(keystone.Id) ? "" : " <color=#B8A99A><size=80%>(Artwork In Progress)</size></color>";
                _tooltipTitle.text = $"<color=#F59E0B><b>{keystone.Name}</b></color> <size=80%><color=#B8A99A>(Level {milestone} Talent)</color></size>{artNotice}";
                _tooltipContent.text = $"<color=#F3EBE1>{keystone.Description}</color>\n<size=85%><color=#ECC267>Click socket to modify talent.</color></size>";
            }
            else if (isUnlocked)
            {
                _tooltipTitle.text = $"<color=#F59E0B><b>Level {milestone} Keystone Socket</b></color>";
                _tooltipContent.text = "<color=#F3EBE1>Talent slot available!</color>\n<size=85%><color=#ECC267>Click to choose a talent.</color></size>";
            }
            else
            {
                _tooltipTitle.text = $"<color=#B8A99A>Level {milestone} Keystone Socket</color>";
                _tooltipContent.text = $"<color=#8C7B6E>Unlocks at Skill Level {milestone}.</color>\n<size=85%><color=#ECC267>Click to preview talents.</color></size>";
            }

            PositionTooltip(worldPos, 20f);
            _hoverTooltip.transform.SetAsLastSibling();
            _hoverTooltip.SetActive(true);
        }

        public void HideHoverTooltip()
        {
            if (_hoverTooltip != null)
            {
                _hoverTooltip.SetActive(false);
            }
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

        private readonly List<(GameObject btnObj, Image img, Image indicator, TMP_Text text, SkillCategoryType? cat)> _filterBtnList = new();

        private void CreateFilterButton(
            Transform parent,
            TMP_FontAsset? font,
            Sprite whiteSprite,
            string label,
            SkillCategoryType? filterCategory)
        {
            var btnObj = new GameObject($"Filter_{label}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(110f, 28f);

            var le = btnObj.GetComponent<LayoutElement>();
            le.preferredWidth = 110f;
            le.preferredHeight = 28f;

            // Transparent hit-target image (No dark boxes or borders!)
            var img = btnObj.GetComponent<Image>();
            img.sprite = whiteSprite;
            img.color = Color.clear;

            bool isActive = _activeCategoryFilter == filterCategory;

            // Active golden underline indicator bar flush along the bottom of the banner
            var indicatorObj = new GameObject("ActiveIndicator", typeof(RectTransform), typeof(Image));
            indicatorObj.transform.SetParent(btnObj.transform, false);
            var indRect = indicatorObj.GetComponent<RectTransform>();
            indRect.anchorMin = new Vector2(0.10f, 0f);
            indRect.anchorMax = new Vector2(0.90f, 0f);
            indRect.pivot = new Vector2(0.5f, 0f);
            indRect.anchoredPosition = Vector2.zero;
            indRect.sizeDelta = new Vector2(0f, 3f);
            var indImg = indicatorObj.GetComponent<Image>();
            indImg.sprite = whiteSprite;
            indImg.color = isActive ? UIHelper.AmberGoldConduit : Color.clear;

            var textObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            var fontToUse = font ?? UIHelper.GetValheimFont();
            if (fontToUse != null) tmp.font = fontToUse;
            tmp.text = $"<b>{label}</b>";
            tmp.fontSize = 12f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.color = isActive ? UIHelper.NorseGoldText : UIHelper.MutedParchment;

            var btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = tmp;
            var colors = btn.colors;
            colors.normalColor = isActive ? UIHelper.NorseGoldText : UIHelper.MutedParchment;
            colors.highlightedColor = UIHelper.ParchmentText;
            colors.pressedColor = UIHelper.AmberGoldConduit;
            colors.selectedColor = isActive ? UIHelper.NorseGoldText : UIHelper.MutedParchment;
            btn.colors = colors;

            _filterBtnList.Add((btnObj, img, indImg, tmp, filterCategory));

            btn.onClick.AddListener(() =>
            {
                _activeCategoryFilter = filterCategory;
                UpdateFilterButtonVisuals();
                UIHelper.PlaySfx("sfx_gui_button");
                if (_cachedPlayer != null) Populate(_cachedPlayer);
            });
        }

        private void UpdateFilterButtonVisuals()
        {
            foreach (var (btnObj, img, indicator, text, cat) in _filterBtnList)
            {
                bool isActive = _activeCategoryFilter == cat;
                if (img != null) img.color = Color.clear;
                if (indicator != null) indicator.color = isActive ? UIHelper.AmberGoldConduit : Color.clear;
                text.color = isActive ? UIHelper.NorseGoldText : UIHelper.MutedParchment;

                var btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    var colors = btn.colors;
                    colors.normalColor = isActive ? UIHelper.NorseGoldText : UIHelper.MutedParchment;
                    colors.highlightedColor = UIHelper.ParchmentText;
                    colors.pressedColor = UIHelper.AmberGoldConduit;
                    colors.selectedColor = isActive ? UIHelper.NorseGoldText : UIHelper.MutedParchment;
                    btn.colors = colors;
                }
            }
        }

        public void Populate(Player player, bool preserveScroll = false)
        {
            _cachedPlayer = player;
            if (_contentTransform == null) return;

            float savedScroll = (preserveScroll && _scrollRect != null) ? _scrollRect.horizontalNormalizedPosition : 0f;

            // Clear previous column instances
            foreach (var col in _activeColumns)
            {
                if (col != null) Destroy(col.gameObject);
            }
            _activeColumns.Clear();

            var skillsManager = player.GetSkills();
            if (skillsManager == null) return;

            var font = UIHelper.GetValheimFont();

            // 1. Get all discovered skills (level > 0 or has progression accumulator)
            var allSkills = skillsManager.GetSkillList();
            var allDiscoveredSkills = allSkills
                .Where(s => s.m_level > 0f || s.m_accumulator > 0f)
                .ToList();

            // 2. Discover active categories that contain at least one experienced skill
            var discoveredCategories = new HashSet<SkillCategoryType>(
                allDiscoveredSkills.Select(s => SkillCategory.GetCategory(s.m_info.m_skill)));

            // If selected filter category is no longer discovered (or player has no experience), revert to ALL
            if (_activeCategoryFilter != null && !discoveredCategories.Contains(_activeCategoryFilter.Value))
            {
                _activeCategoryFilter = null;
            }

            // 3. Show/hide filter buttons dynamically based on discovered skills
            foreach (var (btnObj, img, indicator, text, cat) in _filterBtnList)
            {
                if (cat == null)
                {
                    // "ALL" filter tab is always visible
                    btnObj.SetActive(true);
                }
                else
                {
                    // Category filter items only show up once the player has experience in at least one skill within the category
                    bool isDiscovered = discoveredCategories.Contains(cat.Value);
                    btnObj.SetActive(isDiscovered);
                }

                bool isActive = _activeCategoryFilter == cat;
                if (img != null) img.color = Color.clear;
                if (indicator != null) indicator.color = isActive ? UIHelper.AmberGoldConduit : Color.clear;
                text.color = isActive ? UIHelper.NorseGoldText : UIHelper.MutedParchment;

                var btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    var colors = btn.colors;
                    colors.normalColor = isActive ? UIHelper.NorseGoldText : UIHelper.MutedParchment;
                    colors.highlightedColor = UIHelper.ParchmentText;
                    colors.pressedColor = UIHelper.AmberGoldConduit;
                    colors.selectedColor = isActive ? UIHelper.NorseGoldText : UIHelper.MutedParchment;
                    btn.colors = colors;
                }
            }

            // 4. Filter columns by active selection
            var filteredSkills = allDiscoveredSkills
                .Where(s => _activeCategoryFilter == null || SkillCategory.GetCategory(s.m_info.m_skill) == _activeCategoryFilter.Value)
                .OrderBy(s => (int)SkillCategory.GetCategory(s.m_info.m_skill))
                .ThenBy(s => SkillCategory.GetSkillOrder(s.m_info.m_skill))
                .ThenBy(s => s.m_info.m_skill.ToString())
                .ToList();

            foreach (var skill in filteredSkills)
            {
                var column = RuneColumnUI.Create(_contentTransform, player, skill, font);
                _activeColumns.Add(column);
            }

            UpdateContentLayout(filteredSkills.Count, preserveScroll ? savedScroll : 0f);
        }

        private void UpdateContentLayout(int columnCount, float targetScrollPos = 0f)
        {
            if (_contentTransform == null || _scrollRect == null) return;

            float colWidth = RuneColumnUI.ColumnWidth;
            float spacing = 18f;
            float padX = 40f; // 20 left + 20 right
            float neededWidth = columnCount > 0 ? (columnCount * colWidth + (columnCount - 1) * spacing + padX) : 0f;

            float viewportWidth = _scrollRect.viewport != null && _scrollRect.viewport.rect.width > 0f
                ? _scrollRect.viewport.rect.width
                : (Screen.width - 40f);

            if (neededWidth <= viewportWidth)
            {
                // Columns fit on page: Stretch Content to viewport and Center-align
                _contentTransform.anchorMin = new Vector2(0f, 0f);
                _contentTransform.anchorMax = new Vector2(1f, 1f);
                _contentTransform.pivot = new Vector2(0.5f, 0.5f);
                _contentTransform.offsetMin = Vector2.zero;
                _contentTransform.offsetMax = Vector2.zero;
                _contentTransform.sizeDelta = Vector2.zero;
                _contentTransform.anchoredPosition = Vector2.zero;

                _scrollRect.horizontal = false;
            }
            else
            {
                // Columns exceed viewport: Left-align and enable horizontal scrolling
                _contentTransform.anchorMin = new Vector2(0f, 0f);
                _contentTransform.anchorMax = new Vector2(0f, 1f);
                _contentTransform.pivot = new Vector2(0f, 0.5f);
                _contentTransform.offsetMin = Vector2.zero;
                _contentTransform.offsetMax = Vector2.zero;
                _contentTransform.sizeDelta = new Vector2(neededWidth, 0f);
                _contentTransform.anchoredPosition = Vector2.zero;

                _scrollRect.horizontal = true;
                Canvas.ForceUpdateCanvases();
                float clampedPos = Mathf.Clamp01(targetScrollPos);
                _scrollRect.horizontalNormalizedPosition = clampedPos;
            }
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;

            // Escape or Tab closes drawer if open, otherwise closes the dialog
            if (ZInput.GetButtonDown("JoyButtonB") || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
            {
                if (_drawer != null && _drawer.gameObject.activeSelf)
                {
                    _drawer.Close();
                    return;
                }

                Hide();
                return;
            }

            // Mouse wheel horizontal smooth panning
            float mouseScroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(mouseScroll) > 0.01f && _scrollRect != null)
            {
                _scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(
                    _scrollRect.horizontalNormalizedPosition - (mouseScroll * 0.08f)
                );
            }
        }
    }
}
