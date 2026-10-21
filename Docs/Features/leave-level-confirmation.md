---
id: leave-level-confirmation
title: Leaving confirmation and current-level reset
status: Verified
owner: Project
created: 2026-10-19
updated: 2026-10-19
---

# Leaving confirmation and current-level reset

## Goal

Add the supplied Leaving confirmation to in-level Quit.

## User-visible behavior
Quit in the level pause dialog opens a second view in the same modal session. It uses the supplied purple/gold background and header, cream inset, broken heart with -1, Leaving title, You will lose 1 life message and red Leave button. X returns to Paused with progress and stamina unchanged. Leave spends one stamina, resets the current level to its initial layout and returns Home. The next Start enters that same level at its beginning. Repeated or stale confirmations cannot charge again. At zero stamina leaving remains possible and stamina clamps at zero; Start remains unavailable until recovery.

## Out of scope

No shop, level content, reward, IAP, inventory refund, Foundation package or save schema change.

## Ownership and architecture
- Command: Assets/Scripts/Game/Commands/Navigation/LeaveLevelCommand.cs validates level/generation, resets through StartLevelCommand, spends via SpendStaminaCommand, navigates Home and publishes the existing navigation event.
- Presentation: SettingsPopupController owns the confirmation view within its existing UiPopupRouter registration and foreground pause. LevelOneController uses cached level assets, resets visual rotation after acceptance. Dependencies are cached at Start.
- Editor: LeavingDialogSetup builds the direct-reference prefab descendants and imports art. LevelPauseSetup invokes it on authoring rebuild. SettingsPopupStyleSetup removes the level-only confirmation when deriving the menu template.
- Existing ReturnHomeCommand remains available to internal flows that intentionally preserve the live session. Foundation packages unchanged.

## State and persistence
No new model, save schema, save version or IAP. Existing stamina save/recovery is reused. Current level number and booster inventory survive; transient level state, active booster effects, cubes, hive/queue state, flights, slots, rotation and win/deadlock state reset. Consumed inventory is not refunded.

## UI, prefab, scene, and Inspector impact
Direct owner: Assets/Sprites/Prefabs/UI/SettingsPopup.prefab shared by all five level scenes. Add sibling LeavingContent with LeaveButton and CancelLeaveButton serialized bindings. Project UI convention profile declares LeavingContent as an optional popup root child. Existing pause descendants and scene overrides preserved; no scene rewrite required. Background/Header/Close reused from Assets/Sprites/UI/PauseDialog. New direct art under Assets/Sprites/UI/LeavingDialog: Inset.png, BrokenHeart.png, LeaveButton.png; native TMP font/material assets under same folder using Fredoka-Bold. No new Resources keys, runtime loads or per-open instantiation. SettingsPopupController releases listeners and modal state on disable/destroy.

## Failure and lifecycle
Quit and confirmation do nothing when disabled, backgrounded or while waiting for old pointers to release. Cancel costs nothing and retains original manual pause ownership. Hiding/unloading closes both views and unregisters modal. Invalid reset input must not spend or navigate; generation/level mismatch rejects stale confirmation. Same-level navigation does not reload or advance scenes. No SDK/network dependency.

## Performance
Cached prefab UI and dependencies; no per-frame asset/model lookups. Existing once-per-second economy refresh remains. Reset performs the same bounded rebuild as Restart.

## Acceptance criteria
- [x] Supplied composition appears on Quit across five levels, blocks underlying input, displays Leaving/-1/message/Leave with matching Fredoka styling.
- [x] X restores pause view without cost/reset, closing pause resumes only its owned pause.
- [x] Leave spends once, returns Home, clears current level progress and effects, retains current level and inventory, next Start is fresh and playable.
- [x] Double clicks, stale generation/level, invalid data, zero stamina, application background and disable are handled safely.
- [x] Existing pause/retry/victory, navigation and inventory behavior remains compatible apart from intentional Quit reset.

## Verification matrix
| Boundary | Evidence | Result |
|---|---|---|
| Command | LeaveLevelTests red/green, economy/navigation suite | Passed |
| Runtime | LeavingDialogTests and affected PlayMode suite | Passed |
| Assets | Native prefab readback and five scene bindings; portrait screenshots | Passed |
| Conventions | validate_game_foundation_project.py, UI prefab validator, compilation/console | Passed |
| Review | Independent review per requesting-code-review skill | Passed |

## Definition of Done
- [x] Project-owned Commands/presentation and resource conventions followed.
- [x] Acceptance criteria implemented and tested.
- [x] Compile, project validator, prefab validator and relevant tests pass.
- [x] Visual composition verified at two portrait aspects.
- [x] Evidence and any limitations recorded; status Verified only after checks pass.

## Decision log
- 2026-10-19: User supplied exact art/layout and specified that reset means the current level starts over. Confirmation is a view of the existing pause modal so cancel preserves modal pause/input ownership.

## Verified evidence (2026-10-19)

Workspace evidence root: `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/leave-level-confirmation/`.
- RED: red-results.json reports 4 missing-command failures; ui-red-results.json shows Quit immediately navigating instead of asking.
- Final EditMode: final-edit-results.json, 95/95 passed, zero failures/skips.
- Final PlayMode: final-play-results.json, 138/138 passed, zero failures/skips. New LeavingDialogTests covers cancel/progress/pause ownership, five actual levels and fresh Start, duplicate confirm, disable/background and real touch raycasts with stale-pointer rejection. New LeaveLevelTests covers duplicate/stale generation/level, invalid data atomicity and zero stamina.
- The first full runtime run found two test synchronization issues: newly enabled graphics had not received render depth in the same frame; a fixed .65s wall wait could finish before a frame-delta-capped victory animation. Tests now yield the presentation frame and wait for actual animation completion with a bounded deadline. Both focused retest and final complete suite passed.
- audit.json: all five scenes share SettingsPopup.prefab; Leaving/Leave/-1/message use Fredoka with no missing glyphs; Leave/X bindings valid, inactive by default; existing victory SettingsPopup references retained; UI prefab validator valid, zero issues.
- final-assets.json: menu template has no Leaving controls; art has alpha enabled, no mipmaps and no CPU-readable copies; compile=false, play=false. final-console.json: zero error/warning entries. restored-home.json: MainMenu edit state.
- Native 1080×1920 and 1080×2340 screenshots: `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/leave-level-confirmation/portrait.png` and `tall.png`; composition, margins, complete text and button placement visually checked.
- `python3 Scripts/validate_game_foundation_project.py --project .`: passed, zero warnings.
- Independent requesting-code-review initial and follow-up reviews: no actionable findings. No runtime allocation/asset/model lookup introduced in click refresh paths; static TMP fonts and cached prefab views reuse resources.
- Existing MainMenuSetupTests regenerates MainMenu.unity, MainMenuPage.prefab and MainMenuSettingsPopup.prefab. The menu builder explicitly removes the level-only confirmation. The five gameplay scene files remain byte-identical to baseline; original pause descendants and Inspector bindings are preserved. New direct art and optional popup profile rule are project-owned.
- No external service, IAP, save version, Foundation package or device build involved. Portrait verification used native Unity renders; physical-device testing was not part of this UI task. No pending Inspector binding work.
- Generated art/provenance/prompt set and final paths: `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/leave-level-confirmation/art-manifest.json`; built-in image_gen, no scripted pixel edits.
