---
id: level-one-boosters
title: 关卡底部四项辅助功能
status: Verified
owner: Project
created: 2026-10-12
updated: 2026-10-19
---

# 关卡底部四项辅助功能

## Goal

用用户提供的四个图标、独立蓝色圆底和整体金棕色底座接入实际玩法，保持现有模型与采集动画。

## User-visible behavior

- 从左到右：增加槽位、加速、随机排序、任意选择。按钮浮在底座上沿，等待蜂巢保留两行且不被遮挡。
- 增加槽位将工作位从五个增加至六个，新位在第一位正上方；本关只增加一次。
- 加速为 2 倍，出蜂、接近、抓取、抬起与回巢均按相同倍率推进，保留所有过渡与依次出蜂。
- 随机排序重新分配全部等待蜂巢的顺序和所在列；保留蜂巢身份、颜色、额度、正在工作的蜂巢与航班。
- 任意选择开启后可以点击任意等待蜂巢，包括第二行及翻页后的蜂巢；关闭后恢复每列最前方规则。
- 加速、任意选择再次点击关闭，保持至本关 Retry；Home→Start Game 保留辅助状态与当前进度。
- 暂停、设置弹窗、切到主界面、后台和跃动期间拒绝新的辅助输入；Retry 重置槽位、倍率、选择规则与等待队列。

## Out of scope

- 道具库存、购买、收费、广告、次数消耗、额外绿色购买入口。
- 放宽花朵方块的相机遮挡判定、修改蜜蜂/花朵模型或抓取路径。
- 跨应用重启存档、音乐和震动资源。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | Commands/Harvest/HarvestBoosterCommands.cs; HarvestCommands.cs | 额外槽位、开关、排列、指定蜂巢激活均由 Command 修改 |
| Observable state | Model | Models/Harvest/HarvestModel.cs | 当前槽位数、SpeedBoost、FreeSelection、五列等待 ID |
| Shared rules | System | Systems/Harvest/HarvestSystem.cs | 当前等待位置、可选蜂巢、原相机可见性规则 |
| One-shot notification | None | Existing HarvestChangedEvent | 不引入多余事件 |
| Unity presentation | ViewController | LevelOneController, LevelOneHud, HiveHopController, HiveModelView, HarvestBoosterBar, HarvestBoardView | 渲染、跳跃、按钮、翻页与布局 |
| Adapter | Utility | Existing LevelOneAssets | 沿用现有模型缓存 |
| Reusable infrastructure | None | Foundation unchanged | 游戏业务保留在项目内 |

## State and persistence

- 等待队列存储稳定蜂巢索引，不修改共享 LevelData，不重新编号活跃航班引用。
- 新槽位有单独的启用状态，容量仅 5/6；飞行动画即使该槽已释放仍保留起点。
- 开关与队列为当前会话数据。StartLevelCommand 原子重置；设置、Home 和暂停只冻结时间，不重置。
- Save schema/version/migration: None。

## UI, prefab, scene, and Inspector impact

- Scene: Assets/Scenes/Level01.unity；既有 HUD 五列及设置引用保留。
- 新直接引用 Prefab: Assets/Sprites/Prefabs/UI/HarvestBoosterBar.prefab。LevelOneHud.Boosters 引用实例。
- 新直接引用图片: Assets/Sprites/UI/Boosters/{AddSlot,Speed,Shuffle,FreeSelect,ButtonDisc,ToolbarBase}.png，六图源白底通过内建 imagegen 背景提取，保留高光；原图不变。无新增 Resources key。
- HUD 追加第六 SlotLabel/SlotTarget，第二行增加 Button；翻页按钮属于 HUD，页状态是展示状态。
- 样式沿用 Fredoka Bold，开启的按钮显示状态标记。新图只在初始化加载/绑定，实例随场景生命周期释放。
- Editor/LevelOneBoostersSetup.cs 通过 Editor API 幂等组装、保留旧序列化引用和设置弹窗。

## Integrations and failure behavior

- 无 SDK、网络、YooAsset 或离线依赖。
- 无等待蜂巢/全满/已胜利/死锁/暂停时拒绝无效请求且不消耗额度。
- Shuffle 或关闭自由选择不能使旧跃动提交到错误蜂巢；跃动按稳定 ID 和合法性校验，输入期间锁定辅助按钮。
- 设置关闭后沿用现有等待旧指针释放逻辑，辅助与后排/翻页按钮同样受保护。

## Performance considerations

- 六个槽位图块与四个辅助 Button；模型继续缓存复用。
- 队列扫描仅五列；随机排序只在点击时分配少量 ID 列表；HUD 按 Revision 更新，无每帧资源加载或依赖查找。
- 2 倍推进不增加单次出蜂批次，保留每蜂巢三个并发任务上限。
- 真机 GPU/帧时间与触控体验留作设备检查，Editor 覆盖两种竖屏尺寸。

## Acceptance criteria

- [x] 四按钮用六张素材，顺序与参考布局正确，等待两行可见。
- [x] 第六槽位在第一槽上方，可分配、出蜂、计数、释放，重复点击不继续增加。
- [x] 2 倍缩短出蜂和采集时间，关闭恢复正常，锁定/抓取/转移动画仍连续且无批量出蜂。
- [x] 打乱全部等待队列，稳定 ID/额度和所有工作任务不变。
- [x] 任意选择可选后排和翻页等待蜂巢，容量、工作/结束/暂停与方块遮挡限制仍有效。
- [x] Retry 重置；Home/Start 保留；设置与旧指针释放期间无底层输入穿透。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed; 0 warnings |
| Unity compilation | Compile + Console | All compiled; final error check recorded |
| Focused tests | 5 booster commands + 5 runtime cases; observed RED then GREEN | Passed; includes real press/hold/release during hop |
| Regression | Full project EditMode/PlayMode suites | 32/32 EditMode; 43/43 PlayMode |
| Assets/runtime | Exact prefab, six art and font bindings; two portrait screenshots | Component validator: 0 issues; both captures inspected |
| Performance | Cached dependency/input/render review | Reuses model caches; 6 slot tiles and 4 buttons; device timings remain unmeasured |

## Definition of Done

- [x] Ownership/Commands and QFramework folders respected.
- [x] Public behavior and acceptance criteria implemented.
- [x] UI, scene, prefab bindings checked through Editor APIs.
- [x] Convention validation, compilation, focused and regression tests pass.
- [x] Fresh read-only review findings addressed and evidence recorded.
- [x] Remaining device checks reported; only then status Verified.

## Decision log

- 2026-10-12: 用户确认 2 倍、两个开关重复点击关闭且持续至本关重开。
- 2026-10-12: 单次增加一个工作槽，最多六个；仅当前会话有效，沿用已确认继续当前进度逻辑。

## Verified evidence

- EditMode job `202bafa95560423793af52414c18d3d8`: 32 passed, 0 failed.
- PlayMode job `d3e988f65d1f49d3b688bc3bef5a2bf0`: 43 passed, 0 failed. Covers original main menu/settings, occlusion, grasp, continuous handoff and concurrent clearance tests plus five booster UI cases.
- Feature RED: command types missing (5 failures), toolbar missing (3 failures). Real pointer regression RED `297408e2e7f7413a9215946c86f295fe` captured a blocked Speed press; corrected raycast gates then full suite GREEN. Final touches during hop cannot activate Speed, rear hives or pages on late release.
- Shuffle real waiting IDs are conserved, active flights keep identity and all remaining quotas remain unchanged.
- Runtime geometry: six 2x active hives retain source-to-cargo continuity, reach/lift frames and separated visible actors. Entry shrink below 0.001 scale is excluded from zero-size floating-point AABB penetration assertions; actual convergence remains covered by original tests.
- `HarvestBoosterBar.prefab` Component validation: valid, 0 issues. Exact imported art, scene references, six slot targets and FredokaBoldSDF were read back through Unity Editor APIs.
- Screenshots: [Default 1080x1920](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level-one-boosters/level-final-1080.png), [Enabled 1080x1920](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level-one-boosters/level-enabled-1080.png), [Enabled 1170x2532](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level-one-boosters/level-enabled-1170.png). Both rows, all four buttons and sixth slot fit; temporary GameView size removed afterward.
- [Test job records](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/level-one-boosters/verification.json), [Fresh review and resolution](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/level-one-boosters/review.md), [Art provenance and exact extraction prompts](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/level-one-boosters/art/manifest.json).
- Final Unity Play Mode left on Level01 in default state after actual settings Retry: 5 slots, speed 1x, FreeSelection off, Remaining 522.

## Final changed files and boundaries

- Models/Harvest/HarvestModel.cs: booster state and stable waiting queues.
- Systems/Harvest/HarvestSystem.cs: waiting ID lookups and selection guards.
- Commands/Harvest/HarvestCommands.cs and HarvestBoosterCommands.cs: activation by ID, 5/6 capacity, toggles, shuffled waiting queues, uniform 2x clock and atomic Retry reset.
- ViewControllers/Harvest/{LevelOneController,LevelOneHud,HiveHopController,HiveModelView,HarvestBoosterBar,HarvestBoardView}.cs: toolbar and both rows, pages, six slot presentation, per-input raycast gates and layout.
- Editor/LevelOneBoostersSetup.cs: reproducible Editor API scene/prefab assembly.
- Tests/Editor/HarvestBoosterTests.cs; Tests/Runtime/HarvestBoostersUiTests.cs; HiveClickTests.cs expected slot coordinate updated to .34.
- Assets/Scenes/Level01.unity; Assets/Sprites/Prefabs/UI/HarvestBoosterBar.prefab; six Assets/Sprites/UI/Boosters images; existing FredokaBoldSDF atlas includes state/page glyphs.
- Flower moved up to world y=7.85 and scaled 1.28→1.16 to keep the lower controls clear; collector viewport y=.435; work slots y=.34; added slot y=.425 above first; queue rows .225–.300 and .140–.215; bar y=0–.115. Camera occlusion, bee rigs and grasp trajectories preserved.

## Remaining device checks

Physical phone touch feel, actual notched safe area and GPU/frame time require a device. Editor target-size captures and full tests are verified; no device performance claim. No remaining Inspector wiring work.

## Current inventory update

2026-10-12: 本页保留最初四项辅助功能的验收记录。当前库存角标、消费补充、180秒加速与Retry保留库存规则，见 [booster-inventory.md](booster-inventory.md)。

## 2026-10-19 Quit 行为更新

关卡内 Quit 的当前行为以 [leave-level-confirmation](leave-level-confirmation.md) 为准：先确认，Leave 扣一点体力、重置当前关卡后返回 Home；下次 Start 从本关初始进度开始。取消确认保留进度。本文此前关于 Quit 保留进度的记录为历史行为；内部 ReturnHomeCommand 仍可用于保留会话。
