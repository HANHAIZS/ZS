---
id: navigation-bar
title: Purple Bottom Navigation
status: Verified
owner: Project
created: 2026-10-15
updated: 2026-10-19
---

# Purple Bottom Navigation

## Goal
Replace only main-menu bottom navigation presentation with supplied purple/gold bar and Shop, Home, Setting icons. Preserve all three tab actions, selection model, settings preferences, economy, input/lifecycle gates, and resource paths. Level pause dialog sizing is tracked separately in paused-dialog.md.

## Ownership and architecture
MainMenuTabsController remains the input/event owner; MainMenuNavigationBarView renders cached references and owns only transient presentation state, never navigation or persistent state. MainMenuPage.prefab directly references Assets/Sprites/UI/NavigationBar art. Existing ShopButton, HomeButton, GalleryButton objects/references and labels remain. Native editor builder imports/crops supplied strip/rim into sprite subassets and builds the bar; MainMenuTabsSetup calls it last so rebuilds preserve presentation. No new Resources keys, loaders, saves, commands, dependencies or Foundation edits.

## Acceptance criteria
- Left Shop, center Home, right Setting use supplied icons, with purple background removed through built-in imagegen background-extraction; original files preserved.
- Three equal columns; selected column rises with gold curved rim, larger raised icon and cream outlined label; other columns show icons alone.
- Selected plate and rim follow all three tabs. No purple rectangle around an icon, no top-rim seam or cropped icon.
- Existing tab navigation, touch release, background/foreground, popup blocking, settings persistence and economy remain unchanged.
- Existing menu, economy and settings PlayMode regression tests pass; compile and prefab/convention validators pass; three real runtime selected-state captures verified.

## Failure and lifecycle
Serialized missing navigation references fail at bind; no per-frame lookups, polling, or runtime image allocation. Selection rerenders on existing tab events/reopen and survives scene flow through existing model. New art lives with prefab lifetime.

## Asset prompts
Built-in image_gen, one background-extraction call per supplied Settings/Shop/Home icon: remove ONLY purple background and external backdrop shadow; preserve exact object, silhouette, colors, proportions, glossy 3D details, centered square true alpha, no added text/objects/redesign.

## Evidence
Native Apply and repeated Apply passed; serialized sprite IDs are stable nonzero IDs, one navigation view and three original button bindings. Final runtime screenshots: outputs/navigation-bar/home-1080.png, shop-1080.png, setting-1080.png. Independent review passed. MainMenu/Level01/Level02 scenes, shared Fredoka font and main-menu popup hashes unchanged. Five NavigationBar PNGs match workspace final assets byte for byte. NavigationLabel.mat is a dedicated purple-outline material. Full PlayMode job 44be070f664a411dbf34b03910134e6f: 89 passed, zero failed/skipped; convention validator passed with zero warnings; fresh preview Console has zero errors/warnings. No real-device test or performance profiling was run.

## User-visible behavior
Shop / Home / Setting are three equal columns. Selected tab uses raised purple plate, gold outline, larger icon and its own label. Other two tabs show only icons.

## Out of scope
No new purchases, tab actions, settings changes, game rules, save keys or Foundation changes.

## Verification matrix
| Boundary | Evidence |
| --- | --- |
| Navigation and input | Existing MainMenuTabsTests, MainMenuFlowTests and settings/economy regression |
| Native prefab/art | Three runtime selected-state captures, exact serialized binding readback |
| Conventions and compilation | Project and prefab validators, Unity Console |

## Definition of Done
- [x] Native asset/runtime readback and all three visual states inspected.
- [x] Relevant regression tests and compile/convention validation pass.
- [x] Preserve actual settings/economy and document verification limitations.

## Final asset paths and prompt set
Final project-bound cutouts: work/navigation-bar/assets/{Shop,Home,Setting}.png (copies also at Assets/Sprites/UI/NavigationBar). Bar.png and Rim.png are byte-identical copies of supplied strips; native sprite regions compose the states. Tool: built-in image_gen, transparent_background=true, referenced_image_paths was the matching supplied source.

Final prompt for each icon:
> Use case: background-extraction. Edit target: the attached [cream and orange gear with hot pink center / pink and golden shop stall with honeycomb blocks and honey jar / cream pink and gold bee house with the bee above its roof] navigation icon. Remove ONLY the purple background and background shadow outside the object, leaving a clean true transparent alpha background. Preserve the exact object, silhouette, all details, colors, proportions and glossy 3D materials. Keep the whole object intact centered on a square canvas, no text, no new objects, no redesign. This is an exact asset cutout for a Unity game navigation bar.

## 2026-10-19 Video-reference animation

User approved the proposed reference-video treatment: retain purple/gold art, slide a single raised plate, lift/enlarge the arriving icon while lowering/shrinking the departing one, fade labels, use roughly .25 seconds with a fast-start/slow-finish curve, retarget continuous clicks from the current pose. The latest user instruction explicitly preserves the current purple/gold UI, icons and layout: retain equal thirds, existing plate width, icon positions, sizes, fonts and materials. Button hit regions remain fixed during transitions and contain each corresponding icon/label; no moving raycast target.

Ownership: MainMenuNavigationBarView owns only cached presentation weights and normalized position, drives transitions with unscaled UI time, and exposes its frame Tick used by Update. Existing tab commands/events and page input gates remain owners of navigation. MainMenuNavigationBarSetup preserves all existing anchors and calls immediate rendering; MainMenuPage.Show snaps to the actual selected tab. First display, disable/reopen and app-background interruption settle immediately. Repeated renders of the same target do not restart animation. No new art, loaders, saves, commands, dependencies, packages or gameplay changes.

Additional acceptance criteria:
- [x] A real tab click starts from the displayed pose rather than snapping. Plate, icons and label opacity progress together and settle in .25 seconds of unscaled time.
- [x] A second target starts at the current intermediate pose; same-target renders do not extend duration or jump. Invalid/zero frame deltas do not corrupt the UI.
- [x] Initial display, Hide/Show, disable and background resume show a valid selected state; original page switches, pointer release, modal and economy behavior remain intact.
- [x] Native prefab readback, two portrait layouts, actual transition frame sequence, focused/new and full project tests, console/project/prefab validators and independent review pass.

Verification: four NavigationBarAnimationTests cover continuous retargeting, shared plate/icon/label progress, repeated targets, invalid deltas and app interruption; two NavigationBarAnimationUiTests cover actual button events, unscaled time, preserved art/hit regions, Hide/Show and background input gates. RED had four expected missing-animation failures. Final full EditMode job `449ea539f035485bae53799ad421bfa9`: 143/143 passed. Final full PlayMode job `e7c30d504ce642f6a1e5cd491c706396`: 146/146 passed. An earlier PlayMode run exposed a test waiting for near-final plate position before the exact icon endpoint; the waiter now waits for final label opacity too. No production change was needed for that test timing issue. Native prefab validator: valid, zero issues; final Console: zero errors/warnings; project validator: zero warnings. Independent code review found no concrete defects. Evidence root: `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/navigation-bar-animation/`.


Final implementation and visual evidence:
- Changed scripts: `ViewControllers/UI/MainMenuNavigationBarView.cs` (cached .25s cubic ease-out using unscaled time); `MainMenuTabsController.cs` (forward immediate presentation), `MainMenuPage.cs` (snap on Show), and `Editor/MainMenuNavigationBarSetup.cs` (immediate authoring). Tests add a TextMeshPro reference to the Editor test assembly. All are under `Assets/Scripts/Game`.
- Existing `Assets/Resources/Prefabs/UI/MainMenuPage.prefab`, every scene and all baseline UI prefabs are byte-identical to the starting snapshot. Full Editor tests regenerate the menu asset as a side effect; both menu assets were restored exactly through Unity FileUtil and the final full PlayMode suite ran on the originals. `final-preservation.json` records the final hashes. No Inspector changes, new art, Resources keys or asset lifecycle changes.
- Actual Unity renders: `outputs/navigation-bar-animation/{Home,Shop,Gallery}-{1920,2340}.png`, 40 transition frames, `current-three-states.png` and `current-navigation-animation.gif`. The GIF crops the real rendered bottom bar; page content stays Home in this presentation preview. Actual page switching is exercised in PlayMode tests. Both portrait layouts and intermediate frames were inspected.
- No per-frame collections, dependency lookups, loading or coroutine allocation: two three-element arrays are cached per view; idle Tick returns immediately. No device profiling or physical-device testing performed.
