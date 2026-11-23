#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VikingLib.UI.Hub;
using VikingLib.UI.Hub.Core;
using VikingLib.UI.Hub.Rendering;
using VikingSkills.Configuration;
using VikingSkills.Data;
using VikingSkills.Keystones;

namespace VikingSkills.UI
{
    /// <summary>
    /// Hosts the Skills &amp; Keystones browser inside <see cref="VikingHubDialog"/>
    /// using <see cref="IStructuredHubTab"/> with a 28/72 Master-Detail split layout.
    /// </summary>
    public sealed class SkillsHubTab : IStructuredHubTab
    {
        public const string TabId = "skills";

        public string Id => TabId;
        public string DisplayName => "Skills";
        public int SortOrder => 20;
        public Sprite? HubIcon => _hubIcon ??= VikingHubTheme.LoadEmbeddedSprite(typeof(SkillsHubTab).Assembly, "hub_skills.png");

        private static Sprite? _hubIcon;
        private Skills.SkillType? _selectedSkill;

        public HubPortalSummary GetPortalSummary()
        {
            Player? player = Player.m_localPlayer;
            if (player == null)
            {
                return new HubPortalSummary(
                    statusLine: "No active character",
                    subtitle: "Skill levels, active passives & milestone keystones.");
            }

            List<Skills.Skill> discovered = GetDiscoveredSkills(player);
            if (discovered.Count == 0)
            {
                return new HubPortalSummary(
                    statusLine: "Train skills to unlock keystones",
                    subtitle: "Skill levels, active passives & milestone keystones.");
            }

            int unattunedCount = 0;
            var catTotals = new Dictionary<SkillCategoryType, (float Sum, int Count)>();

            for (int i = 0; i < discovered.Count; i++)
            {
                Skills.Skill s = discovered[i];
                SkillCategoryType cat = SkillCategory.GetCategory(s.m_info.m_skill);
                catTotals.TryGetValue(cat, out var acc);
                catTotals[cat] = (acc.Sum + s.m_level, acc.Count + 1);

                int level = Mathf.FloorToInt(s.m_level);
                for (int m = 0; m < KeystoneRegistry.Milestones.Length; m++)
                {
                    int milestone = KeystoneRegistry.Milestones[m];
                    if (level >= milestone &&
                        KeystoneRegistry.GetKeystonesForMilestone(s.m_info.m_skill, milestone).Count > 0 &&
                        KeystoneManager.GetSelectedKeystone(player, s.m_info.m_skill, milestone) == null)
                    {
                        unattunedCount++;
                    }
                }
            }

            var catParts = new List<string>(5);
            SkillCategoryType[] orderedCats =
            {
                SkillCategoryType.Weapons,
                SkillCategoryType.Defense,
                SkillCategoryType.Magic,
                SkillCategoryType.Mobility,
                SkillCategoryType.Resources
            };
            for (int i = 0; i < orderedCats.Length; i++)
            {
                SkillCategoryType cat = orderedCats[i];
                if (catTotals.TryGetValue(cat, out var acc) && acc.Count > 0)
                {
                    int avg = Mathf.RoundToInt(acc.Sum / acc.Count);
                    catParts.Add($"{cat} {avg}");
                }
            }

            string statusLine;
            if (catParts.Count >= 4)
            {
                int mid = (catParts.Count + 1) / 2;
                statusLine = $"{string.Join(" • ", catParts.GetRange(0, mid))}\n{string.Join(" • ", catParts.GetRange(mid, catParts.Count - mid))}";
            }
            else if (catParts.Count > 0)
            {
                statusLine = $"Avg: {string.Join(" • ", catParts)}";
            }
            else
            {
                statusLine = $"{discovered.Count} Skills";
            }

            return new HubPortalSummary(
                statusLine: statusLine,
                alertBadge: unattunedCount > 0 ? $"{unattunedCount} Ready" : null,
                subtitle: "Skill levels, active passives & milestone keystones.");
        }

        public HubView BuildView(HubViewContext ctx)
        {
            var view = new HubView
            {
                ViewTitle = "Skills",
                ShowBackToHub = true
            };

            Player? player = Player.m_localPlayer;
            if (player == null)
            {
                view.Root = new TextNode("No active character.", TextStyle.Muted, TextAlign.Center);
                return view;
            }

            List<Skills.Skill> discovered = GetDiscoveredSkills(player);
            if (discovered.Count == 0)
            {
                view.Root = new TextNode("Train any skill in the world to begin unlocking passives and keystones.", TextStyle.Muted, TextAlign.Center);
                return view;
            }

            // Register skill icons with HubSpriteAtlas
            var discoveredCategories = new HashSet<SkillCategoryType>();
            for (int i = 0; i < discovered.Count; i++)
            {
                Skills.Skill s = discovered[i];
                Skills.SkillType st = s.m_info.m_skill;
                discoveredCategories.Add(SkillCategory.GetCategory(st));
                if (s.m_info.m_icon != null)
                {
                    HubSpriteAtlas.RegisterExternalSprite($"vs_skill_{st}", s.m_info.m_icon);
                }
            }

            // Filter bar (category chips only, no search)
            var filterBar = new FilterBarNode
            {
                Id = "SkillsFilterBar",
                ShowSearch = false,
                Placeholder = string.Empty
            };

            SkillCategoryType? activeCat = ParseCategory(ctx.ActiveCategory);
            if (activeCat.HasValue && !discoveredCategories.Contains(activeCat.Value))
            {
                activeCat = null;
                ctx.ActiveCategory = "all";
            }

            filterBar.Chips.Add(new FilterChip("all", "All", !activeCat.HasValue));
            AddCategoryChipIfDiscovered(filterBar, discoveredCategories, SkillCategoryType.Weapons, "Weapons", activeCat);
            AddCategoryChipIfDiscovered(filterBar, discoveredCategories, SkillCategoryType.Defense, "Defense", activeCat);
            AddCategoryChipIfDiscovered(filterBar, discoveredCategories, SkillCategoryType.Magic, "Magic", activeCat);
            AddCategoryChipIfDiscovered(filterBar, discoveredCategories, SkillCategoryType.Mobility, "Mobility", activeCat);
            AddCategoryChipIfDiscovered(filterBar, discoveredCategories, SkillCategoryType.Resources, "Resources", activeCat);

            List<Skills.Skill> filtered = activeCat.HasValue
                ? discovered.Where(s => SkillCategory.GetCategory(s.m_info.m_skill) == activeCat.Value).ToList()
                : discovered;

            if (filtered.Count == 0) filtered = discovered;

            // Ensure a valid selected skill
            Skills.Skill selectedSkill = filtered[0];
            if (_selectedSkill.HasValue)
            {
                for (int i = 0; i < filtered.Count; i++)
                {
                    if (filtered[i].m_info.m_skill == _selectedSkill.Value)
                    {
                        selectedSkill = filtered[i];
                        break;
                    }
                }
            }
            _selectedSkill = selectedSkill.m_info.m_skill;

            // Left Master Pane: FilterBar + Scrollable Skill List
            var masterScroll = new ScrollColumnNode { Id = "SkillsMasterScroll", ItemGap = 8f };
            for (int i = 0; i < filtered.Count; i++)
            {
                Skills.Skill skill = filtered[i];
                Skills.SkillType st = skill.m_info.m_skill;
                bool isSelected = st == _selectedSkill.Value;

                masterScroll.Children.Add(new SelectableCardNode
                {
                    Id = $"SkillItem_{st}",
                    IconKey = $"vs_skill_{st}",
                    Title = GetLocalizedSkillName(st),
                    AlertBadge = $"Lv {Mathf.FloorToInt(skill.m_level)}",
                    IsSelected = isSelected,
                    OnClick = () =>
                    {
                        _selectedSkill = st;
                        UIHelper.PlaySfx("sfx_gui_button");
                        ctx.RequestRebuild();
                    }
                });
            }

            var masterPane = new SplitPaneNode
            {
                Id = "SkillsMasterPane",
                Horizontal = false,
                SplitRatio = 0.001f,
                First = filterBar,
                Second = masterScroll
            };

            // Right Detail Pane: HeroHeader + row-aligned Keystone pairs (no bottom passives strip)
            var detailScroll = new ScrollColumnNode { Id = "SkillsDetailScroll", ItemGap = 12f };
            detailScroll.Children.Add(BuildSkillHeroHeader(selectedSkill));
            AppendKeystoneRows(detailScroll, player, selectedSkill, ctx);

            view.Root = new MasterDetailNode
            {
                Id = "SkillsMasterDetail",
                MasterRatio = 0.28f,
                Master = masterPane,
                Detail = detailScroll
            };

            return view;
        }

        // Legacy IVikingHubTab fallback (unused by VikingHubDialog for IStructuredHubTab)
        public void BuildContent(RectTransform container) { }
        public void OnShow() { }
        public void OnHide() { }
        public void OnRefresh() { }
        public bool HandleBack() => false;

        private static HeroHeaderNode BuildSkillHeroHeader(Skills.Skill skill)
        {
            Skills.SkillType st = skill.m_info.m_skill;
            int level = Mathf.FloorToInt(skill.m_level);
            float pct = Mathf.Clamp01(skill.GetLevelPercentage());

            var hero = new HeroHeaderNode
            {
                Id = $"SkillHero_{st}",
                IconKey = $"vs_skill_{st}",
                Title = $"{GetLocalizedSkillName(st)} — Lv {level}",
                Subtitle = SkillCategory.GetCategory(st).ToString(),
                Progress = new ProgressBarNode
                {
                    Id = $"SkillXP_{st}",
                    Label = "Experience",
                    Value = pct,
                    Max = 1f,
                    ValueText = level >= 100 ? "MAX" : $"{Mathf.RoundToInt(pct * 100f)}% to Lv {level + 1}",
                    ToneKey = "stamina"
                }
            };

            List<string> bonuses = SkillBonusCalculator.GetActiveBonuses(st, skill.m_level);
            for (int i = 0; i < bonuses.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(bonuses[i]))
                {
                    hero.PassiveBadges.Add(new BadgeNode(bonuses[i], "accent"));
                }
            }

            return hero;
        }

        private static void AppendKeystoneRows(ScrollColumnNode detailScroll, Player player, Skills.Skill skill, HubViewContext ctx)
        {
            Skills.SkillType st = skill.m_info.m_skill;
            float skillLevel = skill.m_level;
            int addedRows = 0;

            for (int mIdx = 0; mIdx < KeystoneRegistry.Milestones.Length; mIdx++)
            {
                int milestone = KeystoneRegistry.Milestones[mIdx];
                var choices = KeystoneRegistry.GetKeystonesForMilestone(st, milestone);
                if (choices == null || choices.Count == 0) continue;

                KeystoneDefinition? active = KeystoneManager.GetSelectedKeystone(player, st, milestone);
                bool isUnlocked = skillLevel >= milestone;

                var rowCards = new List<SelectableCardNode>(choices.Count);
                for (int kIdx = 0; kIdx < choices.Count; kIdx++)
                {
                    KeystoneDefinition k = choices[kIdx];
                    bool isEquipped = active != null && string.Equals(active.Id, k.Id, StringComparison.Ordinal);

                    string statusBadge = isEquipped
                        ? "Equipped"
                        : (isUnlocked ? $"Lv {milestone} • Select" : $"Lv {milestone}");

                    var card = new SelectableCardNode
                    {
                        Id = $"Keystone_{st}_{milestone}_{k.Id}",
                        IconKey = null,
                        Title = k.Name,
                        AlertBadge = statusBadge,
                        IsSelected = isEquipped,
                        IsDimmed = !isUnlocked,
                        MinHeight = 84f,
                        OnClick = () => HandleKeystoneClick(player, st, milestone, k.Id, k.Name, isUnlocked, isEquipped, ctx)
                    };
                    card.Children.Add(new TextNode(k.Description, TextStyle.Muted));
                    rowCards.Add(card);
                }

                for (int i = 0; i < rowCards.Count; i += 2)
                {
                    HubNode first = rowCards[i];
                    HubNode second = (i + 1 < rowCards.Count)
                        ? rowCards[i + 1]
                        : new TextNode(string.Empty);

                    detailScroll.Children.Add(new SplitPaneNode
                    {
                        Id = $"KeystoneRow_{st}_{milestone}_{i / 2}",
                        Horizontal = true,
                        SplitRatio = 0.5f,
                        First = first,
                        Second = second
                    });
                    addedRows++;
                }
            }

            if (addedRows == 0)
            {
                detailScroll.Children.Add(new TextNode("No milestone keystones are registered for this skill.", TextStyle.Muted));
            }
        }

        private static void HandleKeystoneClick(
            Player player,
            Skills.SkillType skillType,
            int milestone,
            string keystoneId,
            string keystoneName,
            bool isUnlocked,
            bool wasEquipped,
            HubViewContext ctx)
        {
            if (!isUnlocked)
            {
                UIHelper.PlaySfx("sfx_gui_button");
                player.Message(MessageHud.MessageType.TopLeft, $"Requires Skill Level {milestone}.");
                return;
            }

            if (!KeystoneManager.SelectKeystone(player, skillType, milestone, keystoneId, out string failReason))
            {
                UIHelper.PlaySfx("sfx_gui_button");
                if (!string.IsNullOrEmpty(failReason))
                {
                    player.Message(MessageHud.MessageType.TopLeft, failReason);
                }
                return;
            }

            if (wasEquipped)
            {
                UIHelper.PlaySfx("sfx_gui_moveitem");
                player.Message(MessageHud.MessageType.TopLeft, $"Unbound {keystoneName}");
            }
            else
            {
                UIHelper.PlaySfx("sfx_gui_craftitem_forge_end");
                string msg = (ModConfig.EnableHearthsideAttunement != null && ModConfig.EnableHearthsideAttunement.Value)
                    ? $"Attuned {keystoneName} at the hearth"
                    : $"Attuned {keystoneName}";
                player.Message(MessageHud.MessageType.TopLeft, msg);
            }

            ctx.RequestRebuild();
        }

        private static List<Skills.Skill> GetDiscoveredSkills(Player player)
        {
            var skillsManager = player.GetSkills();
            if (skillsManager == null) return new List<Skills.Skill>();

            return skillsManager.GetSkillList()
                .Where(s => s.m_level > 0f || s.m_accumulator > 0f)
                .OrderBy(s => (int)SkillCategory.GetCategory(s.m_info.m_skill))
                .ThenBy(s => SkillCategory.GetSkillOrder(s.m_info.m_skill))
                .ThenBy(s => s.m_info.m_skill.ToString(), StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static void AddCategoryChipIfDiscovered(
            FilterBarNode bar,
            HashSet<SkillCategoryType> discovered,
            SkillCategoryType cat,
            string label,
            SkillCategoryType? activeCat)
        {
            if (discovered.Contains(cat))
            {
                bar.Chips.Add(new FilterChip(cat.ToString().ToLowerInvariant(), label, activeCat == cat));
            }
        }

        private static SkillCategoryType? ParseCategory(string active)
        {
            if (string.IsNullOrWhiteSpace(active) || string.Equals(active, "all", StringComparison.OrdinalIgnoreCase))
                return null;

            return Enum.TryParse(active, ignoreCase: true, out SkillCategoryType parsed) ? parsed : null;
        }

        private static string GetLocalizedSkillName(Skills.SkillType skillType)
        {
            if (Localization.instance != null)
            {
                string localized = Localization.instance.Localize("$skill_" + skillType.ToString().ToLowerInvariant());
                if (!string.IsNullOrEmpty(localized) && !localized.StartsWith("$"))
                {
                    return localized;
                }
            }
            return skillType.ToString();
        }
    }
}
