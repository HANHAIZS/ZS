---
id: player-economy
title: Main menu coins and stamina
status: Verified
owner: Project
created: 2026-10-13
updated: 2026-10-13
---

# Main menu coins and stamina

## Goal
Display the supplied coin and heart counters on the main menu and enforce stamina costs for abandoning or retrying an unfinished level.

## User-visible behavior
- New installs start with 1600 coins and 5 stamina. Main menu top row shows stamina on the heart, countdown in its right panel, and coins in the coin panel.
- Coin plus grants 500. Stamina plus grants 1, capped at 5. Both use the existing supplied green badge at the icon's bottom right.
- Below 5 stamina, recover 1 every 600 real seconds. Keep the next recovery deadline when spending again or adding stamina below the cap. Full stamina cancels the deadline and shows `full`.
- Recovery includes time in settings, levels, background and while closed. Counts and next deadline persist locally across launches and scene changes.
- An accepted Home from an unfinished level costs 1, clamped at zero. Duplicate Home after returning cannot spend again.
- An accepted Retry costs 1 and restarts the level. With 1 stamina, Retry is allowed and leaves 0. At 0, Start Game and Retry are unavailable. Home always remains available.
- Successful victory Continue has no cost and preserves existing level progression.

## Out of scope
No coin spending, paid SDK, ads, win rewards, new levels, or cost on application close. Gameplay and level layout stay as implemented.

## Ownership and architecture
| Responsibility | Owner | Type/path | Notes |
| --- | --- | --- | --- |
| Mutation | Command | Commands/Economy/EconomyCommands.cs and existing navigation commands | Refresh, grants, stamina cost, Retry acceptance |
| Persistent state | Model | Models/Economy/EconomyModel.cs | Coins, stamina, recovery deadline, revision |
| Rules | System | Systems/Economy/EconomySystem.cs | UTC clock and countdown calculation |
| Save adapter | Utility | Utilities/Economy/EconomyRepository.cs | Foundation VersionedSaveService, version 1, existing ISaveStore |
| UI | ViewController | ViewControllers/UI/EconomyHeaderView.cs | Cached state, one-second refresh, guarded badge input |
| Retry wiring | ViewController | LevelOneController, SettingsPopupController | Send command, reset presentation only on acceptance |
| Shared infrastructure | Foundation | None modified | Reuse existing save/clock contracts |

## State and persistence
- Economy save schema v1: coins, stamina, nextRecoveryAt (UTC seconds). Slot `BeeHarvest.economy`. Missing save uses defaults; validated backup restores corrupt primary. Invalid numeric fields sanitize; unsupported future versions are not overwritten.
- Commands own mutations and save only actual state changes; UI countdown does not write once per second.
- Tests use isolated memory stores/clocks and restore runtime state to avoid consuming developer balances.

## UI, prefab, scene, and Inspector impact
- Modify Resources prefab `Assets/Resources/Prefabs/UI/MainMenuPage.prefab`, loaded through existing `Prefabs/UI/MainMenuPage` key. Keep all existing references and modal stacking.
- New directly referenced art: `Assets/Sprites/UI/Economy/CoinCounter.png`, `StaminaCounter.png`; badge reuses `Assets/Sprites/UI/Boosters/RefillBadge.png`.
- Header direct prefab child on SafeArea, visible across Home/Shop/Gallery. Settings overlay renders above it; input blocks with existing page guards. All scenes reuse this page.
- Supplied opaque PNGs receive transparent cutouts with built-in imagegen, original files untouched.

## Integrations and failure behavior
- Offline works without network. Save write failure keeps runtime balances and reports warning. No SDK.
- On clock rollback, clamp countdown to at most 600 seconds and rebase stale future deadlines; recovery never creates negative balances or above-cap stamina.
- Plus callbacks reject hidden/modal/background input. Disabled Retry leaves settings open; duplicate button input cannot spend after the modal closes.

## Performance considerations
Cache QFramework dependencies in initialization. Refresh once per real second on visible menu; no asset lookup per tick, no disk write for display-only countdown. Two small sprites and three labels reuse the existing font and badge.

## Acceptance criteria
- [x] Defaults, grants, costs, zero guards, capped 600-second recovery and persistence follow the stated rules.
- [x] Full/counter/countdown labels and badge input render correctly on Home, Shop and Gallery without modal overlap.
- [x] Level 1 and 2 retain gameplay and progression; Continue has no stamina cost.

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Conventions | Foundation validator | Passed, 0 warnings |
| Compilation | Unity console | Passed, 0 errors; consoleFinal.json |
| Logic/save | Focused isolated EditMode tests, including missing/corrupt/backup save, UTC boundaries and costs | 12 economy cases pass in 96/96 full suite; editFullFinal.json |
| Input/navigation | PlayMode actual prefab badges, settings Home/Retry, zero Start and modal guards | Full 75/75, then 9/9 focused after title spacing; playFullFinal.json, uiFinal.json |
| Presentation | Native prefab readback and 1080x1920 screenshots | Passed, nativeFinal.json; home.png, shop.png, gallery.png, countdown.png |
| Regression | Existing EditMode/PlayMode suites | 96/96 EditMode, 75/75 PlayMode; final regeneration 1/1 |

## Definition of Done
- [x] All acceptance criteria, convention, compile, tests and exact prefab/runtime checks have evidence.
- [x] Native Inspector references confirmed and spec marked Verified.
- [x] Device/published-build limits reported in evidence.

## Decision log
- 2026-10-13: User confirms zero blocks Start/Retry; stamina cap 5, background recovery and local persistence. Main menu header is shared by its three tabs. Gameplay Quit cost covers Home/Retry only.

## Completion evidence
Evidence directory: `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/player-economy/`. See verification.md for test job IDs, review outcome and platform limits. MainMenuSetup now invokes EconomyHeaderSetup.Build so regenerated UI preserves its bindings. Shop/Gallery title anchors lowered to fit the header. Native final readback confirms local 1600 coins, 5 stamina, zero recovery deadline and valid prefab references. Restore of the test-generated economy slot was explicitly approved by the user.
