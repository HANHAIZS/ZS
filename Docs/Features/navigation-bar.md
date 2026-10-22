---
id: navigation-bar
title: Purple Bottom Navigation
status: Verified
owner: Project
created: 2026-10-15
updated: 2026-10-22
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


## 2026-10-22 Purple boundary outside gold rim
用户要求按参考图二保留顶部紫色边界，黄色外圈不再盖住紫色边界。修正仅限当前MainMenuPage导航框层级/贴边位置，使用已有Bar/Rim原图，不重画UI或更改图标/字号/三等分/命中区/选中板高度/0.25秒动效/页面行为。

将平直GoldRim与选中曲线外圈置于两个无Graphic父层，作为紫色底板/选中板之前的同级背景绘制。按原图底部8像素阴影余量对齐可见金色轮廓；平直金边与曲线端点的可见金色底缘贴齐紫色横栏顶缘，凸起部分藏于紫色前景背后；紫色图始终在前，原顶部深紫边和高光完整保留。选择外圈Rect通过现有ApplyPose与选择板同步；不新增Update、材质、贴图或Raycast目标。新Rect层无Graphic，不增加绘制图元。原生成器和针对性native修正入口都复用相同布局，避免重建恢复旧遮挡。

资源仍为Resources键Prefabs/UI/MainMenuPage，原Bar/Rim/图标/字体/材质引用不变。只通过Unity原生Prefab API修改；保留用户已改的Game及BeePalette19/HivePalette11原字节，不提交其修改。验证原生深度/贴边位置、三选中态和切换中间帧、原动效与输入生命周期、Prefab/项目规范及Console；不截图录屏、不上传GitHub。

- [x] 原生绘制深度证明金边不覆盖紫色顶缘，直边/凸起金边向外贴合。
- [x] 三个选中态与快速切换过程中外圈和底板同步，无图标/文字/命中区或原图改写。
- [x] 既有导航动效/输入回归、原生引用、Prefab/项目规范通过；记录限制。


验证证据（2026-10-22）：修改前原生Canvas深度为紫色120/124、金边121/128，确认覆盖原因；修正后Home/Shop/Gallery三态金边depth1–4、紫色从5开始，快速切换中间帧外圈anchor与选中板一致。平直金边与曲线端点可见金色底缘均为147，金色顶缘159.281815/159.2889（差0.008以内），消除原图阴影余量造成的接缝。针对性Apply连续两次及完整Build连续两次，仍各一个背景层/选择层，四张金边图，无重复层或新增Graphic。

既有EditMode导航动画4/4通过（job `3016ffba5bb84c9aab0ef0fb47f7b478`）；最终PlayMode导航交互及动画7/7通过（job `dfa58380cb764f87984eb2b85bf39db3`）。Prefab规范零问题；项目验证零警告；最终Console零错误/警告。独立审查发现原图阴影边距引起接缝偏移，已按像素轮廓校正。起始哈希比对仅MainMenuPage.prefab、导航生成器、导航View及原测试fixture改变；图片/字体/材质/所有场景及用户已有修改均保持起始字节。证据：`/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/nav-rim-boundary/final-preservation.json`。未截图、录屏或做真机目测；本次以原生运行时层级/几何及交互测试验证，仍建议用户在Game视图查看最终观感。没有新增每帧循环、绘制图元、依赖查找或资源加载，现有ApplyPose多同步一个Rect；未做性能profiling。
