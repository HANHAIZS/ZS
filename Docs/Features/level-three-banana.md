---
id: level-three-banana
title: 香蕉第三关与三关递进
status: Verified
owner: Project
created: 2026-10-18
updated: 2026-10-19
---

# 香蕉第三关与三关递进

## Goal
用户确认以资料包Banana制作第三关：812方块、20蜂巢、3列。接在苹果关之后，复用现有采集、出巢绕行、道具、暂停、经济与最新UI。

## User-visible behavior
- 第一关花朵→第二关苹果→第三关香蕉；第三关胜利Continue返回Home，新循环从第一关开始。重复点击仅递进一次。
- 香蕉默认呈现弯曲侧面，拖动和重开保持现有旋转规则；朝向仅写入新模型的美术父节点，数据坐标与原两关不变。
- 香蕉使用原始三维坐标、色号、队列顺序和额度；5工作槽、三排等待蜂巢。没有连接组或新增机关。
- Home按钮显示level 3，返回Start继续同关进度；Retry重置香蕉关并按原经济规则扣体力、保留道具库存。
- 两侧出巢、固定抓取、清晰驻留抽出、携货入大巢沿用已完成版本；新关显示最新槽位、道具与暂停UI。

## Out of scope
第四关、787对象关序推断、新道具/新机制、存档解锁、现有UI重设计、改变原两关数据或蜜蜂行为参数、Foundation改动。

## Ownership and architecture
- Commands：现有ContinueLevelCommand/InitializeMenuNavigationCommand/StartLevelCommand/RetryLevelCommand继续负责状态修改。
- Model/System：原MenuNavigationModel、HarvestModel、HarvestSystem，无新状态或规则。
- Utility：LevelOneAssets扩展第三关场景/模型映射与数据缓存；CubeLandPalette补充源色10，取已有783图标采样的代表色#D81818。
- Editor：CubeLandLevelSetup支持按关卡构建，新增LevelThreeSetup原生复制第二关场景、设置关号/标题/构建列表；已有跨关UI设置工具覆盖第三关。
- 离线导入：import_cube_land_levels.py保持原两关字节输出，支持Banana模型身份与Level03。
- ViewController沿用当前组件和Inspector绑定；不在View写业务状态。

## State and persistence
当前关号仍是会话状态，无存档schema迁移。金币/体力/库存沿用现有Commands和隔离测试存储。Continue代次和重复调用门限不变。

## UI, prefab, scene, and Inspector impact
- 新Assets/Scenes/Level03.unity加入BuildSettings；Controller.LevelNumber=3、标题Level 3，复用第二关安全区与三排布局，列数由原Hud动态按3列排列。
- 新Assets/Resources/Levels/Level03.json（键Levels/Level03）、Assets/Resources/Prefabs/Gameplay/Banana.prefab（键Prefabs/Gameplay/Banana）。
- 新源文件Assets/LevelSources/CubeLand/Banana.json仅导入审计，不运行时加载；原SHA-256为717b8c28f1e266f2ba314acd1281e2ba07957f1922dbf38b741780ec79c1cd53。
- 新BeePalette10/HivePalette10预制体按现有Prefabs/Gameplay键缓存，直接引用共享源色材质；方块复用现有Voxel.mesh。
- LevelOneAssets缓存资源，场景销毁原View实例；不增加每帧加载，不更改Resources/YooAsset策略。
- 标题沿用CoinyRoundedSDF，若缺3仅补字形，已有atlas/material引用保留。无需手动Inspector绑定。

## Integrations and failure behavior
离线本地资源；无SDK、网络或热更新。导入继续拒绝额度不匹配、重复坐标、未知机关、无效连接；关号越界明确拒绝。重开/后台/切场景取消规则沿用；不加载不存在的第四关。

## Performance considerations
812体素复用共享Mesh/材质，小于现有花朵1784与苹果4710。新增源色仅三份共享材质和蜂/巢预制体。没有新调度器、每帧扫描或分配；设备性能与发布构建不在本次编辑器验收范围。

## Acceptance criteria
- [x] 原始源文件、812坐标/颜色、20额度与队列对应，可复现导入且不改变前两关。
- [x] Runtime真实加载第三关、3列/5槽/3排、所有模型和源色正确，无缺失引用。
- [x] 苹果Continue进入香蕉且仅一次，第三关末关返回Home新循环正确。
- [x] 第三关实际取块和交付、道具、重开、暂停/后台与Home/Start保持。
- [x] 原生画面、编译、工程规范、聚焦与完整测试通过；报告完整通关和设备验证边界。

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| Import | Python source correspondence, reproducibility, malformed-input tests | Passed (see evidence below) |
| Navigation | real Continue from apple, final banana, duplicate/generation guard | Passed (see evidence below) |
| Scene/gameplay | runtime Banana renderers, quotas, actual collection/delivery, retry/resume | Passed (see evidence below) |
| UI | original new art, third-row layering, pause and both portrait aspects | Passed (see evidence below) |
| Regression | full EditMode/PlayMode, convention, console, scope hashes | Passed (see evidence below) |

## Definition of Done
- [x] 所有权、资源生命周期和原生绑定正确，相关行为与失败路径通过。
- [x] 素材来源可追溯，原两关内容保持，新增工程资产可重建。
- [x] 验收证据存在后Verified；设备和完整通关限制明确记录。

## Decision log
- 2026-10-18: 用户选择Banana，确认接在苹果之后、末关返回Home，沿用现有UI/玩法。资料包没有关卡顺序，仅选中该对象。


## Verification evidence (2026-10-18)
Evidence root: `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level03-banana/`.
- Python importer:9/9 passed. Full812 source coordinate/color/depth correspondence and all20 original64-bit shooter references/queue/quota values match; total palette04=31, palette12=401, palette07=256, palette10=124. Source SHA matches catalog. Level01/02 generated JSON and Flower/Apple prefabs/scenes remain byte-for-byte unchanged.
- Final EditMode job `71b6e7a64b4049fc9a9a841766f42e60`:123/123 passed,9.92s; final PlayMode job `2a708952311b4a5691ee8b7fac823418`:130/130 passed,150.54s. No failed or skipped tests. Covers third-level actual collection/deposit, Home label3 and same-generation resume, Retry inventory, second→third double-Continue protection, final→Home→first, three-level pause/restart/input and existing bee-route/economy regressions.
- Tests ran RED before implementation: missing Level03 registration and second-win returningHome; Python Banana names incorrectly Apple. Visual regression also ran RED at silhouette ratio0.334 before the authored-side orientation; final default/restart test passed.
- Initial fullPlay had one outdated two-level restart expected-count assertion (4710 for Banana812); corrected only the third-level expectation, all business assertions retained. Historical failure in `playmode-initial-results.json`; reviewed independently and final full run green.
- Native audit:812 unique renderers, one shared Voxel mesh,4 shared source voxel materials,0 missing materials, authored orientation90; sourcePalette10 bee retains4 grasp limbs and2 wing hinges. Level03 Controller=3,3 rows, settings references its own controller, new Oka gear/green buttons retained, Coiny font includesLevel3. Build scene list MainMenu/Level01/Level02/Level03.
- Visual checks: `level03.png`1080×1920, `level03-tall.png`1080×2340, `victory.png`, `verification-preview.jpg`. Default curved side readable, model/collector/work slots do not overlap, waiting-row quotas/toolbar layering preserved. Temporary resolution removed, original1080×1920 restored.
- Full puzzle verified: `full-puzzle-proof.json` / `verified-solution-moves.json`. Real production Banana scene, legal front-hive Commands, actual camera/grid visibility and bee-render arrival acknowledgements; no boosters. Twenty hives selected,812 deposited, remaining0, transit0, Won=true.4937 steps×.02s simulated time,6.985s wall time. Rotates45deg only after pickup count unchanged for3 simulated seconds. This proves one legal solution in command-driven scene stepping; it is not a human real-time playthrough or a phone performance measurement.
- Independent code review identified the restart assertion above, then re-reviewed the fix and Banana-only orientation/grid mapping with no remaining actionable issue (`review.md`).
- Foundation validator passes0 warnings. Final runtime preview console0 warnings/errors. All Inspector bindings assigned; final state is Edit Mode, Level03 open. No Foundation/package changes.

## Changed scripts and assets
- Runtime utilities: `Utilities/Harvest/LevelOneAssets.cs` third-level mapping; `CubeLandPalette.cs` source10 identity. No runtime collector, animation, model-state, System or Command implementation changed.
- Editor: `CubeLandLevelSetup.cs` selected-level build and Banana-only authored orientation; new `LevelThreeSetup.cs`; cross-level UI regeneration loops in OkaUiRefreshSetup, OkaSettingsSetup, LevelPauseSetup include all playable levels.
- Import script/tests: Scripts/import_cube_land_levels.py and Scripts/tests/test_cube_land_import.py. New LevelThreeContentTests/LevelThreeGameplayTests; existing HomeBeeLevelTests,LevelTwoGameplayTests,OkaSettingsTests,PausedDialogTests updated for third/final-level scope.
- New Level03 scene, Banana source/Level03 JSON/Banana prefab, Bee/HivePalette10 prefabs and3 source10 materials. EditorBuildSettings addsLevel03; existing CoinyRoundedSDF gains glyph3. Existing menu rebuild tests reserialize a coherent MainMenu/MainMenuPage/MainMenuSettingsPopup batch without changing layout or references. Unrelated material float normalization restored to baseline.
- Shared asset cache, pooled flight presentation and input lifecycles unchanged. One additional authored Transform under Banana;812 renderers vs1784/4710 in existing levels. No new runtime bitmap, per-frame load, dependency lookup, or animation scheduling allocation.

## Remaining checks
No pending Inspector work. Physical-device graphics/performance, release build and full human playthrough were not tested. Original exact materialRGB is unavailable; source IDs are exact and display colors use representative original-icon samples.

## 2026-10-19 Quit 行为更新

关卡内 Quit 的当前行为以 [leave-level-confirmation](leave-level-confirmation.md) 为准：先确认，Leave 扣一点体力、重置当前关卡后返回 Home；下次 Start 从本关初始进度开始。取消确认保留进度。本文此前关于 Quit 保留进度的记录为历史行为；内部 ReturnHomeCommand 仍可用于保留会话。
