# VikingSkills — Development Roadmap

A structured plan for evolving **VikingSkills** from a panoramic Nordic HUD replacement into a complete, deeply integrated RPG progression system for Valheim.

---

## 🎯 Guiding Design Principles
1. **Meaningful Specialization**: Shift power from overwhelming passive stat creep into impactful, distinct milestone Keystones.
2. **Immersive Viking Mechanics**: Ground character attunement and respecs in the survival loop (the Rested & Comfort system).
3. **Passive & Unique**: Keystones remain primarily passive or combo-modifying (e.g., combo finishers, stance adjustments, conditional perks) rather than clunky active hotbar abilities.
4. **Ecosystem Friendly**: Provide seamless discovery and configurable power budgets for third-party community skills.

---

## 🗺️ Roadmap Phases

```
┌─────────────────────────────────────────────────────────────┐
│ Phase 1: Hearthside Rested Attunement & Respec Gating   [✓] │
│ • Tie talent socketing/swapping to Rested & Comfort tiers   │
├─────────────────────────────────────────────────────────────┤
│ Phase 2: Power Budget Rebalancing & Unique Mechanics    [~] │
│ • Scale back vanilla passive creep; move power to keystones │
│ • Unique combo modifiers (Axe 360° spin finisher, etc.)     │
├─────────────────────────────────────────────────────────────┤
│ Phase 3: Community & Modded Skill Integration           [~] │
│ • Dynamic discovery of third-party skills (Smoothbrain, etc)│
│ • Configurable power bounds & external JSON schema          │
├─────────────────────────────────────────────────────────────┤
│ Phase 4: Full Keystone Catalog (24 Skills / 240 Nodes)  [✓] │
│ • Complete matrix of 5 milestones × 2 perks across 24 skills│
├─────────────────────────────────────────────────────────────┤
│ Phase 5: Iconography & Visual Polish                    [~] │
│ • 240/240 bespoke runic icons [✓] & hearthside VFX/buffs    │
└─────────────────────────────────────────────────────────────┘
```

---

### Phase 1: Hearthside Rested Attunement & Respec Gating — ✅ Completed
*Objective: Immerse keystone selection into the core Valheim hearth-and-home loop while keeping already-socketed keystones active everywhere.*

- [x] **Configurable Comfort Level Thresholds** (`05 - Hearthside Attunement`, server-authoritative):
  - **Level 20 (Tier I)**: Requires **Rested/Resting** (default Comfort ≥ 1, campfire).
  - **Level 40 (Tier II)**: Requires **Rested/Resting** (default Comfort ≥ 4, basic shelter).
  - **Level 60 (Tier III)**: Requires **Rested/Resting** (default Comfort ≥ 8, cozy homestead).
  - **Level 80 (Tier IV)**: Requires **Rested/Resting** (default Comfort ≥ 12, longhouse).
  - **Level 100 (Tier V)**: Requires **Rested/Resting** (default Comfort ≥ 16, grand mead hall).
  - Master toggle (`EnableHearthsideAttunement`) and `RequireRestedStatus` toggle allow admins to customize or disable hearth gating cleanly.
- [x] **Attunement Verification & Ecosystem Compatibility**:
  - `KeystoneManager.CanAttuneMilestone(Player player, int milestone, out string reason)` checks live `SE_Rested.CalculateComfortLevel(player)` (automatically benefiting from comfort-enhancing mods such as `VikingClimate` and `RestedRites` with zero hard dependency) alongside `Rested`/`Resting` status.
  - Enforced in `KeystoneManager.SelectKeystone` while keeping already-socketed keystones active everywhere even after Rested expires.
- [x] **HUD Comfort Indicator & Diegetic Feedback**:
  - Skill Tree Drawer header displays live Rested/Resting status and current Comfort score.
  - Tooltips, timeline banners, and the Keystone Selection Shelf surface clear lock/ready status and play diegetic hearthside audio + HUD messages upon attunement.

---

### Phase 2: Power Budget Rebalancing & Unique Mechanics — 🚧 In Progress
*Objective: Fix vanilla's excessive passive stat inflation while making keystones genuinely transformative.*

- [x] **Passive Stat Compression**:
  - Neutralized vanilla fractional per-level scaling by default via `VikingLib` (`EnableVanillaSkillScaling` config toggle), channeling character power into milestone Keystones.
- [ ] **Unique Combo & Passive Mechanics**:
  - **Axes (3rd combo finisher)**: Replace standard 3rd attack with a wide 360° spin cleave (similar to Atgeir secondary).
  - **Clubs (3rd combo finisher)**: Ground-slam concussive wave with extended stagger.
  - **Swords (Thrust finisher)**: Armor-piercing critical strike on 3rd combo hit.
  - **Spears**: Forward lunge or distance-scaling puncture.
  - **Bows**: Stamina hold curve or awareness criticals on unaware targets.
  - **Shields / Blocking**: Timed parry riposte counter-attack or stamina refund.

---

### Phase 3: Community & Modded Skill Integration — 🚧 Planned
*Objective: Support popular third-party custom skills without causing power overload.*

- [x] **Dynamic Skill Discovery & Fallback Pillars**:
  - Enumerates `player.GetSkills().GetSkillList()` to render HUD pillar cards and fallback milestone keystones for custom modded skills.
- [ ] **Dedicated Community Category Tab & Power Budget Throttling**:
  - Add an "Other / Community" filter tab and server-authoritative config options to cap or damp community skill bonuses.
- [ ] **Extensibility API / JSON Schema**:
  - Allow other mod authors or modpack creators to define custom keystones and icons for third-party skills via external JSON files.

---

### Phase 4: Full Keystone Catalog — ✅ Completed (240 / 240 Nodes)
*Objective: Complete all milestone options across all 24 vanilla skills.*

- [x] Completed the full **240-node matrix** (24 skills × 5 milestones at Lv. 20 / 40 / 60 / 80 / 100 × 2 choices):
  - **Weapons** (9 skills = 90 keystones): Axes, Clubs, Swords, Spears, Polearms, Knives, Unarmed, Bows, Crossbows
  - **Defense** (2 skills = 20 keystones): Blocking, Dodge
  - **Magic** (2 skills = 20 keystones): Elemental Magic, Blood Magic
  - **Mobility** (5 skills = 50 keystones): Run, Jump, Sneak, Swim, Ride
  - **Resources** (6 skills = 60 keystones): Woodcutting, Pickaxes, Farming, Crafting, Cooking, Fishing
- [ ] Ongoing playtesting and numerical balance tuning across biome tiers (Meadows through Ashlands).

---

### Phase 5: Iconography & Visual Polish — 🚧 Iconography Complete
*Objective: Give every keystone a distinct, high-fidelity Nordic visual identity and atmospheric feedback.*

- [x] **Custom Runic Sprites (240 / 240 Complete)**: Every single keystone across all 24 skills has a dedicated 128×128 hand-finished runic icon composited in the canonical Norse border frame.
- [x] **Hearthside Attunement Audio & UI Polish**: Diegetic attunement chimes, unbinding sounds, and clear hearth-readiness feedback across the Rune Sanctuary and Skill Tree Drawer.
- [ ] **StatusEffect Buff Tray Integration**: Surface conditional/timed keystone procs cleanly in the player's top-right buff tray via `VikingLib`.
- [ ] **Hearthside Attunement Particle VFX**: Optional subtle ember/rune particle burst around the player when attuning at a high-comfort hearth.
