---
id: popup-motion
title: Shared elastic popup transitions
status: Verified
owner: Project
created: 2026-10-19
updated: 2026-10-19
---

## Goal
Apply the user-specified animation to existing main-menu settings, level Paused/Leaving, four booster purchase variants and victory settlement. Keep existing artwork, layout and business rules.

## Ownership and architecture
Project-owned PopupMotion in ViewControllers/UI caches CanvasGroups and authored/fitted scales once; existing controllers drive it with unscaled time. Opening .25 seconds: .85 at start, 1.03 at .175 seconds, 1 at .25 seconds, synchronized panel fade-in and original dim-strength fade-in. Closing .15 seconds: fade and shrink toward .85 from the currently displayed pose, then hide. Settings view changes animate the arriving panel without flashing the dim layer. PausedDialogFit composes its responsive fitted scale with the motion multiplier.

Existing controllers retain Commands, router ownership, pause and pointer gates. User-driven dismissal remains modal through the closing animation; forced router replacement/disable/scene teardown closes synchronously and cancels pending work. Retry/Leave/victory Continue commit at the end of the closing animation, once, with generation/background validation; purchase retains the existing immediate transaction and closes visually before releasing pause/input. Backgrounding freezes opening; pending navigation/spend actions are cancelled, while ordinary closing completes safely. Public Close remains a synchronous force-close for the existing router contract. No Foundation, model, save, resource paths, images or authored hierarchy changes. Runtime CanvasGroups are cached with their existing prefab instance and destroyed with it.

## Acceptance criteria
- [x] Deterministic open/overshoot/settle/close timing, opacity, interruption and scale reuse.
- [x] All existing popup types visibly use the shared motion with authored/fitted scale intact.
- [x] Closing blocks duplicate actions and underlying input; force close, old pointers, background, generation change and unload remain safe.
- [x] Actual runtime frames, responsive fit, focused/full tests, compile/console and project/native prefab validators pass.

## Verification
Editor PopupMotionTests and real-scene popup animation tests plus existing settings, leaving, purchasing, victory and menu regressions. Snapshot and evidence: work/popup-motion; screenshots: outputs/popup-motion. No per-frame allocation or dependency lookup; no new art or Resources keys. Physical device timing/profiling is out of scope.


## User-visible behavior
All existing popup compositions keep their artwork and sizes at rest. Their panel, title and buttons enter together with a small 3% overshoot; the existing dim reaches its original opacity. Normal user close shrinks/fades over .15 seconds; closing remains modal until completion. Opening can accept the existing valid fresh-button events, while victory keeps its established entry-pointer gate. Existing close calls made by the router/scene teardown remain immediate.

## Out of scope
No new purchases, rewards, rules, save formats, SDKs, popup art, layout redesign, Resources keys or Foundation changes. No physical-device profiling.

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Exact timing/presentation | Four Editor PopupMotionTests, .175 peak, .25 settle, .15 close | 147/147 full EditMode passed |
| Actual modal/input flow | Existing runtime suites plus five PopupMotionUiTests and pending Continue interruption | 152/152 full PlayMode passed |
| Native UI preservation | Exact prefab/scene hashes, validators, actual Unity frames | All original prefabs/scenes byte-identical; four native popup validators valid/zero issues |
| Performance | Cached references/CanvasGroups, no per-frame loading, callbacks or collection allocation | Code review passed |
| Conventions/compile | Project validator and fresh console | Zero warnings; zero Console errors/warnings |

## Definition of Done
- [x] Presentation stays in project ViewControllers; existing Commands retain gameplay/economy mutation.
- [x] Deterministic timing and complete close interruption coverage are implemented.
- [x] Full runtime tests, compile, project and native prefab checks pass.
- [x] All original art/layout/serialized bindings are preserved and actual frames are inspected.
- [x] Review findings and manual-device limitations are recorded.


## Final evidence
- Full EditMode job `286500df45ce4f738219b93860e49c7d`: 147 passed, no failures/skips. Full PlayMode job `6667266d2b5644abb8e9f57026c128a0`: 152 passed, no failures/skips. Four new Editor tests proved RED before implementation; five new PopupMotionUiTests and one pending-Continue scenario cover real runtime integration. Existing transaction/progression/pointer assertions remain, with condition waits replacing assumptions of instant closure. Diagnostics use the new motion state rather than the removed legacy victory elapsed field.
- Independent review found no implementation defect. Requested pending-close interruption coverage was added for Leave background cancellation/fresh single charge, Retry replacement/new generation and Continue disable/background/new generation; reviewer confirmed the gap resolved.
- Exact rendered readback in `work/popup-motion/capture.json`: panel/dim alpha advance together; t=.175 normalized scale=1.03; t=.25 normalized scale=1.0. `outputs/popup-motion/frame-00.png` through `frame-16.png` record open/close; [actual animation GIF](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/popup-motion/popup-motion.gif). Holds are added between transitions in the GIF for readability; motion frames retain .25/.15 second timing.
- Static actual Unity presentation captures: Paused, Leaving, Purchase-0/1/2/3 and Victory at 1080×1920 and 1080×2340. [All popup preview](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/popup-motion/all-popup-preview.png). Victory is rendered through its existing presentation Show for the screenshot; actual win-trigger and Continue behavior are exercised in the runtime tests. Both portrait layouts and the peak/settled frames were inspected.
- Native popup validators: SettingsPopup, MainMenuSettingsPopup, BoosterPurchasePopup and VictoryPopup all Valid=True, Issues=0. `final-console.json`: no errors/warnings. Python project validation passed, zero warnings. `final-scope.json`: every original scene/prefab byte-identical; Editor rebuild test side effects were restored exactly with Unity FileUtil before final runtime validation. Original art/layout/bindings stay intact; no Inspector action required.

## Final implementation and limits
`Assets/Scripts/Game/ViewControllers/UI/PopupMotion.cs` owns cached transient presentation only. SettingsPopupController, BoosterPurchasePopupController and VictoryPopupController drive it through their existing updates and own all action completion, pause and pointer logic. PausedDialogFit composes fitted scale and motion multiplier, including resize. No Foundation/model/save/Command files changed. Runtime CanvasGroups are created once per scene-cached view, with no frame allocations, coroutines, callbacks or dependency resolution in the animation path. Test support lives in Tests only. No physical-device run or allocation profiler capture; timing verified with deterministic steps and actual unscaled-time runtime tests. Unity restored to MainMenu edit mode.
