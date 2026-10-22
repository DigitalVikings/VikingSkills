# KeystoneSkills — Development Roadmap

A structured plan for transitioning **KeystoneSkills** from a panoramic HUD replacement into a complete, balanced RPG progression system for Valheim.

---

## 🎯 Guiding Design Principles
1. **Meaningful Specialization**: Shift power from overwhelming passive stat creep into impactful, distinct milestone Keystones.
2. **Immersive Viking Mechanics**: Ground character attunement and respecs in the survival loop (the Rested & Comfort system).
3. **Passive & Unique**: Keystones should remain primarily passive or combo-modifying (e.g., combo finishers, stance adjustments, conditional perks) rather than clunky active hotbar abilities.
4. **Ecosystem Friendly**: Provide seamless discovery and configurable power budgets for third-party community skills.

---

## 🗺️ Roadmap Phases

```
┌─────────────────────────────────────────────────────────────┐
│ Phase 1: Rested Attunement & Respec Gating                 │
│ • Tie talent socketing/swapping to Rested comfort thresholds │
├─────────────────────────────────────────────────────────────┤
│ Phase 2: Power Budget Rebalancing & Unique Mechanics        │
│ • Scale back vanilla passive creep; move power to keystones  │
│ • Unique combo modifiers (Axe 360° spin finisher, etc.)     │
├─────────────────────────────────────────────────────────────┤
│ Phase 3: Community & Modded Skill Integration               │
│ • Dynamic discovery of third-party skills (Smoothbrain, etc)│
│ • Configurable power bounds & clean pillar rendering        │
├─────────────────────────────────────────────────────────────┤
│ Phase 4: Full Keystone Catalog Conceptualization            │
│ • Complete matrix of 4 milestones × 2 perks for all skills  │
├─────────────────────────────────────────────────────────────┤
│ Phase 5: Iconography & Visual Polish                        │
│ • Unique runic iconography & hearthside attunement VFX      │
└─────────────────────────────────────────────────────────────┘
```

---

### Phase 1: Rested Attunement & Respec Gating
*Objective: Immerse keystone selection into the core Valheim hearth-and-home loop.*

- [ ] **Comfort Level Thresholds**:
  - **Milestone 25 (Novice Rite)**: Requires **Rested** (Comfort ≥ 1, basic campfire/bed).
  - **Milestone 50 (Adept Rite)**: Requires **Rested** (Comfort ≥ 4, sheltered shelter with fire).
  - **Milestone 75 (Master Rite)**: Requires **Rested** (Comfort ≥ 8, cozy homestead with hearth & rug).
  - **Milestone 100 (Legendary Rite)**: Requires **Rested** (Comfort ≥ 12, grand mead hall/longhouse).
- [ ] **Attunement Verification**:
  - `KeystoneManager.CanAttune(Player player, int milestone)` inspecting `player.GetSEMan().GetStatusEffect("Rested")` and comfort value.
  - Locked feedback in the modal when attempting to socket in the wilderness:
    `"Requires Rested (Comfort {threshold}+) by a hearth to attune."`
- [ ] **HUD Comfort Indicator**:
  - Sub-skill modal displays player's current Rested status and Comfort score alongside milestone requirements.

---

### Phase 2: Power Budget Rebalancing & Unique Mechanics
*Objective: Fix vanilla's excessive passive stat inflation while making keystones genuinely transformative.*

- [ ] **Passive Stat Compression**:
  - Add configurable multipliers to scale down vanilla passive bonuses (e.g. weapon damage spread, stamina costs, draw times).
  - Shift that shaved power budget into Keystone milestones so that leveling + keystone choice together reaches peak performance.
- [ ] **Unique Combo & Passive Mechanics**:
  - **Axes (3rd combo finisher)**: Replace standard 3rd attack with a wide 360° spin cleave (similar to Atgeir secondary).
  - **Clubs (3rd combo finisher)**: Ground-slam concussive wave with extended stagger.
  - **Swords (Thrust finisher)**: Armor-piercing critical strike on 3rd combo hit.
  - **Spears**: Forward lunge or distance-scaling puncture.
  - **Bows**: Stamina hold curve or awareness criticals on unaware targets.
  - **Shields / Blocking**: Timed parry riposte counter-attack or stamina refund.

---

### Phase 3: Community & Modded Skill Integration
*Objective: Support popular third-party custom skills without causing power overload.*

- [ ] **Dynamic Skill Discovery**:
  - Enumerate `player.GetSkills().GetSkillList()` to discover custom modded skills (e.g., Smoothbrain's Sailing, PackHorse, Mining, Hunting, Evade; KG's skills).
- [ ] **Graceful Fallback Pillars**:
  - Auto-generate clean 235×400 HUD pillar cards for custom skills even if they lack keystones.
  - Assign them dynamically to appropriate categories or an "Other / Community" filter tab.
- [ ] **Power Budget Throttling**:
  - Provide server/client config options to cap or damp community skill bonuses to prevent runaway character power.
- [ ] **Extensibility API / JSON Schema**:
  - Allow other mod authors or pack creators to define keystones for their custom skills via external JSON files.

---

### Phase 4: Full Keystone Catalog Conceptualization
*Objective: Complete all milestone options across all 20 vanilla skills before committing to art assets.*

- [ ] Complete the 160-node matrix (20 skills × 4 milestones × 2 choices):
  - Weapons (8 skills = 64 keystones)
  - Defense (2 skills = 16 keystones)
  - Magic (2 skills = 16 keystones)
  - Mobility (5 skills = 40 keystones)
  - Resources (3 skills = 24 keystones)
- [ ] Playtest and tune gameplay numbers across biome tiers (Meadows through Ashlands).

---

### Phase 5: Iconography & Visual Polish
*Objective: Give every keystone a distinct, high-fidelity visual identity once designs are finalized.*

- [ ] **Custom Runic Sprites**: Replace shared parent skill icons with individual runic / thematic ability glyphs.
- [ ] **StatusEffect Integration**: Register active keystone perks in `ObjectDB` so active effects appear on the player's top-right buff tray.
- [ ] **Attunement VFX / SFX**: Atmospheric sound and flame/rune particles when attuning at a high-comfort hearth.
