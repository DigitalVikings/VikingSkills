# Changelog

All notable changes to **VikingSkills** will be documented in this file.

## [0.2.0] - 2026-11-23

### Added
- **Unified VikingHub Integration**: Added a dedicated **Skills** tab directly inside the centralized VikingHub window alongside the panoramic Skill Tree HUD.
- **Custom Keystone Artwork**: Added bespoke icon artwork across all skill pillars and milestone keystones.

### Changed
- **Skill Tree & HUD Polish**: Refined panoramic skill tree layout, keystone status tooltips, and Project Auga compatibility.
- **VikingLib 0.4.0 Alignment**: Updated configuration layout and synchronized mod integration with VikingLib v0.4.0.

## [0.1.3] - 2026-11-03

### Added
- **Death Skill Loss Configuration**: Added comprehensive settings to customize death skill loss behavior (None, Percentage, or FlatLevels).

### Changed
- **Default Skill Loss Tuning**: Synchronized default death skill loss mode to None for forgiving exploration.

## [0.1.2] - 2026-10-29

### Added
- **Project Auga Compatibility**: Integrated the panoramic skill interface with Auga's custom inventory tabs via soft dependency, restoring rich skill tree access.

### Changed
- **Panoramic UI Architecture**: Refactored the skill tree drawer and dialog views with fluent UI builder helpers, optimizing layout hierarchy and rendering overhead.
- **Event-Driven Skill Progression**: Subscribed progression logic directly to VikingLib's canonical SkillEvents channels, eliminating redundant native method patches.
- **Localized Compendium Display**: Dynamically localizes skill names in Hugin's Compendium using native Valheim translation keys across all supported client languages.

## [0.1.1] - 2026-10-25

### Changed
- **Norse Keystone Iconography Overhaul**: Refreshed all 48 milestone keystone ability icons with high-fidelity handcrafted Norse artwork and master border frames.
- **HUD Performance & Layout**: Optimized panoramic tree layout scrolling and tooltip responsiveness across ultra-wide resolutions.

## [0.1.0] - 2026-10-22

### Added
- **Panoramic Skill Tree HUD**: Full-screen panoramic interface overhauling Valheim's skills view with fluid horizontal scrolling and category filters.
- **Dedicated Skill Pillars**: Architectural stone pillars for all 20 vanilla skills displaying level progress, milestone sockets, and active specialization benefits.
- **Milestone Keystone Progression**: Impactful milestone choices at skill levels 20, 40, 60, 80, and 100 granting specialized passives and combat techniques.
- **Standardized Norse Iconography**: Handcrafted runic ability icons featuring uniform master Norse border frames across all skill categories.
