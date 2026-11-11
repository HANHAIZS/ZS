---
id: playtest-level-selector
title: Playtest level selector
status: Verified
owner: Project
created: 2026-11-10
updated: 2026-11-10
---

# Playtest level selector

## Goal
Allow invited testers to open any authored level without unlocking or stamina costs.

## User-visible behavior
- Editor and explicitly flagged test builds show Level Select next to Home's existing start flow.
- Five-column scrollable grids in groups of 30, numeric jump, and most recently played highlight. Return restores the group and scroll position.
- Pause offers Level Select. Victory offers Replay, Level Select and Next, with no Next on the final level.
- Every explicit selection starts a fresh attempt. Returning to the normal path restores its level number, balances and booster stock; its unfinished board is restarted.

## Out of scope
No new game scenes, level assets, navigation tabs, thumbnails, network features, real purchases, uploads or automatic distribution.

## Ownership and architecture
QFramework navigation Commands own chooser and trial state; the existing Game scene loads the selected number. MainMenuNavigationController owns the scene-local selector instance; pause and victory use existing modal input gates.

## State and persistence
Snapshot normal economy and booster counts on first selection; restore on exit and Editor session reset. EconomyRepository skips trial writes. Trial mode and selector state are transient. Settings preferences remain independent.

## UI, prefab, scene, and Inspector impact
Scene-owned runtime uGUI/TMP under the existing Home safe area. Reuse directly referenced pause art and Home font; no serialized scene, prefab or Inspector binding changes. Ordinary player builds require no configuration. Set BEEHARVEST_PLAYTEST in the test target's Scripting Define Symbols for a shareable player build.

## Lifecycle, failure, and performance
Reject invalid level numbers or duplicate transition requests. Scene navigation owns input gates; panel creates 30 reusable buttons, one set of group tabs and no model instances. No continuous asset lookups or extra scenes.

## Acceptance criteria
- [x] Authored level numbers from 1 to LastLevel are selectable and each selection starts a fresh attempt. PlayMode samples include levels 106, 118 and 119.
- [x] Return, retry, same-level selection and final-level completion retain correct navigation.
- [x] Test economy cannot write to the formal save; exit restores the normal state.
- [x] Home, pause and victory provide access to the selector. The entry is compile-gated to Editor and flagged player builds.

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| Commands | PlaytestNavigationTests in Unity 2022.3.62f3c1 | 2 passed |
| UI | PlaytestLevelSelectorTests, MainMenuFlowTests and VictorySettlementTests in Unity 2022.3.62f3c1 | 16 passed |
| Foundation | Foundation validator, git diff check and Unity test compilation | Passed |

## Definition of Done
- [x] Record test evidence and export instructions. A packaged player and physical-device input were outside this verification run.

## Export a playtest build

In Unity, select the target platform and add `BEEHARVEST_PLAYTEST` under Player Settings → Other Settings → Scripting Define Symbols. Apply the change and build to a separate output folder. Remove the symbol for a regular distribution build. The Editor shows Level Select without the symbol.
