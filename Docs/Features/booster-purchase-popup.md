---
id: booster-purchase-popup
title: Four booster purchase popups
status: Verified
owner: Project
created: 2026-10-19
updated: 2026-10-19
---

# Four booster purchase popups

## Goal
Replace the four zero-stock + buttons' instant refill with the supplied purple/gold purchase dialog.

## User-visible behavior
- + opens the corresponding Extra Slot / Speed Up / Shuffle / Pick Any modal. The game pauses and underlying input is blocked.
- Supplied background/header/X and cream inset; existing game icon with ×3 at its lower right; green button shows supplied coin followed by 1600. Fredoka Bold styling matches existing dialogs.
- Exact descriptions: Add 1 extra slot; Speed up all bee carry speeds; Shuffle all single hive positions; Pick any Hive from queue. Long descriptions may wrap without changing the wording.
- Assumption from price and ×3: purchase spends 1600 game coins and grants 3 uses of that item, closes the dialog and resumes the original game. Purchasing does not use the item or reset ongoing effects. X costs nothing and preserves progress/inventory/effects.
- Insufficient coins disable purchase (price remains 1600); direct/stale callbacks also cannot spend or grant. Duplicate purchase cannot charge again. + remains a zero-stock entry; quantity badges continue to show inventory.

## Out of scope
No real-money IAP/ads, shop navigation, new coin grants, changed booster effects, changed linked-hive shuffle rules, inventory save migration, level content, Foundation package or unrelated UI changes. Existing internal RefillHarvestBoosterCommand remains available for controlled setup/other callers; player + no longer invokes it.

## Ownership and architecture
- Command: Commands/Harvest/PurchaseHarvestBoosterCommand.cs validates index, level generation, paused level, non-Home state, empty inventory and balance; atomically deducts coins/grants 3, saves existing economy and increments revisions.
- Controller: ViewControllers/UI/BoosterPurchasePopupController.cs owns one cached modal via existing UiPopupRouter, label/icon binding, lifecycle and pointer-release/app-background gates.
- HarvestBoosterBar owns one popup instance per scene, created during HUD initialization from direct-reference prefab under the scene Canvas, destroys it with its owner. LevelOneController passes itself through HUD initialization; no runtime global find or repeated dependency lookup.
- Editor: BoosterPurchasePopupSetup builds the prefab and binds it into the shared HarvestBoosterBar prefab using Unity APIs.

## State and persistence
No new model/save schema. Coins use existing EconomyRepository persistence. Booster inventory retains existing session-only lifecycle and survives Retry and Leave reset; active effects are unchanged by purchase. Opening/closing only owns a foreground pause, preserves generation and progress. Cancellation or stale generation cannot charge.

## UI, prefab, scene, and Inspector impact
Direct-reference prefab Assets/Sprites/Prefabs/UI/BoosterPurchasePopup.prefab; owned by HarvestBoosterBar.PurchasePopupPrefab in shared HarvestBoosterBar.prefab used in five scenes. No scene rewrite required for feature binding. Direct art under Assets/Sprites/UI/BoosterPurchase: PurchaseButton.png, Coin.png, static TMP font/material. Reuse PauseDialog Background/Header/Close, LeavingDialog Inset, OkaRefresh20261018 existing four icons. No new Resources key; initialize once, remove listeners and unregister on disable/destroy. Existing page/pause/Leaving dialogs retained.

## Integrations and failure behavior
No network, SDK, consent or hot update. Background/disabled/old pointer callbacks cannot buy or close. Cancel returns only its owned pause and waits for every pointer to release before gameplay input. Replacing with settings/closing during Home, win, reset or scene teardown unregisters the modal and does not unpause Home. Reopening never duplicates handlers/instances. Buy at zero balance is refused without negative coins or inventory grants.

## Performance considerations
One cached view per scene, direct sprites and static font; model/utility references cached during Initialize. Labels updated on open, purchase availability only when economy revision/input gate changes. No per-frame asset loading or repeated architecture resolution.

## Acceptance criteria
- [x] Four + controls open the exact corresponding title/description/icon with ×3 and coin1600; no instant refill.
- [x] Cancel preserves coins, progress, effects and stock; modal pauses game and blocks queue, booster and board input.
- [x] 1600 coins grants exactly 3 only once, no immediate use; inventory badge becomes 3; all four items and five scenes work.
- [x] Insufficient balance, stale generation/index, duplicate invocation, old/background touch, disable, scene unload and reopen are safe.
- [x] Native prefab readback, two portrait aspects and no TMP glyph/overflow issues verified.

## Verification matrix
| Boundary | Evidence | Result |
|---|---|---|
| Command | PurchaseHarvestBoosterTests RED/GREEN plus complete EditMode suite | Passed |
| Runtime | BoosterPurchasePopupTests, affected inventory tests and complete PlayMode suite | Passed |
| UI | Five scenes' prefab binding, native validator and screenshots for four variants | Passed |
| Compile/conventions | Console and validate_game_foundation_project.py | Passed |
| Review | Independent requesting-code-review review | Passed |

## Definition of Done
- [x] Ownership/resource/lifecycle conventions followed.
- [x] User-visible acceptance criteria and failure paths verified.
- [x] Compilation, focused/full tests and project/UI validators pass.
- [x] Visual QA and independent review complete, limitations recorded.
- [x] Verified status only after evidence exists.

## Verified evidence (2026-10-19)

Evidence workspace: `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/booster-purchase-popup/`.
- RED: red-edit-results.json has 7 expected missing purchase-command failures; red-play-results.json shows the old + granting 1 immediately instead of asking.
- GREEN: green-edit-results.json has 7/7 purchase command tests passed. Initial runtime focus passed 15/17; the two failures were test setup issues: finding an inactive HUD through FindObjectOfType after Home, and raycasting a newly enabled + before its first rendered frame. Caching the original popup and yielding the UI frame corrected the tests; focused-recheck-results.json passed both.
- Final complete project suites: full-edit-results.json 102/102 passed, full-play-results.json 144/144 passed, zero failures/skips. Covers coins persistence, exact balance/insufficiency, zero-stock packs, duplicate/stale requests, all four items in five scenes, no immediate effects, cancellation, real input, app background, disable, unload, menu navigation and existing level/booster/Leaving/victory behavior.
- audit.json verifies all five scenes inherit the same new PurchasePopupPrefab through HarvestBoosterBar.prefab; four correct Oka icons and new popup pass native prefab validation with zero issues. final-audit.json confirms inactive defaults, cached direct binding, native compile=false/play=false, transparent non-readable/no-mipmap sprites (coin max256, button max1024).
- Eight native renders (4 variants × 1080×1920 and 1080×2340), capture-*.json: all Title/Quantity/Description/Price labels have no missing glyphs and no overflow; compositions and button/icon/frame spacing visually inspected. Screenshots: `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/booster-purchase-popup/`. `four-dialogs-preview.png` is a QA contact sheet cropped from those actual renders.
- Image edits used built-in image_gen: blank green button and transparent user coin. Final raw PNGs live under `Assets/Sprites/UI/BoosterPurchase/`; prompts, inputs and final paths: `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/booster-purchase-popup/art-manifest.json`. Other supplied art and existing game icons reused by direct reference; no scripted pixel edits to supplied assets.
- Preview prepared empty offers temporarily, restored the original four stock values and never purchased/spent player coins. preview-restore.json records restoration.
- Independent requesting-code-review agent review_booster_purchase_popup found no material actionable findings. All subscriptions are paired; one cached popup per scene, static TMP font, no repeated architecture or resource lookups. No SDK, IAP, network or save schema change.
- final-console.json: zero error/warning entries. restored-home.json: MainMenu in Edit Mode. `python3 Scripts/validate_game_foundation_project.py --project .` passed with zero warnings.
- Five gameplay scene files remain byte-identical to baseline. Existing MainMenuSetupTests regenerated MainMenu.unity, MainMenuSettingsPopup.prefab and MainMenuPage.prefab as part of its rebuild regression; menu visuals/behavior remain verified. No pending Inspector bindings. Physical-device testing was not part of this UI task.
