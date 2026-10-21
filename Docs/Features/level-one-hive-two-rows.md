---
id: level-one-hive-two-rows
title: 第一关待选小蜂巢两行模型
status: Verified
owner: Project
created: 2026-10-11
updated: 2026-10-11
---

# 第一关待选小蜂巢两行模型

## Goal

底部五列待选区开局显示两行真实小蜂巢，共十个；数字显示在模型正面，后排能看见并在前排进入工作槽后补位。

## User-visible behavior

- 每列显示前排和下一只后排蜂巢，两排使用现有九色 Blender 模型、同样大小和数字样式；移除“后排”文字与空列破折号。
- 开局每列两只：红79/79、黄39/39、绿42/42、红79/79、橙22/22。总额度仍为红316、黄78、绿84、橙44，共522，与花朵一致。
- 只点击前排，后排仅预览；前排跃入第一个空槽后后排复用原模型向前补位。全部用完后自然留空，不生成虚假蜂巢。
- 五个工作槽、花朵和大蜂巢位置沿用当前布局；设置、暂停、Retry、逐只出蜂及额度/交付规则沿用。

## Out of scope

后续关卡、新模型、主界面、自由选择后排、拖动、联网、商城和 Foundation 修改。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | 既有 HarvestCommands | 复用进入槽位和重开命令 |
| Observable or persistent state | Model | 既有 HarvestModel/LevelData | 本地关卡队列由7只增至10只；无新字段 |
| Shared project rules | System | Systems/Harvest/HarvestSystem.cs | 前排及下一只待选查询 |
| One-shot notification | Event | 既有 HarvestChangedEvent | 不新增事件 |
| Unity presentation | ViewController | HiveModelView.cs、LevelOneHud.cs | 两行模型与数字对齐；后排只显示，不改 Model |
| Adapter or repository | Utility | 既有 LevelOneAssets | 沿用缓存和 Resources |
| Editor assets | Project | Editor/LevelOneHiveQueueSetup.cs、LevelOneV2AssetsSetup.cs | Editor API 调整原场景两排锚点及本地额度 |
| Reusable infrastructure | None | None | 不修改 Foundation |

## State and persistence

- Runtime state: 每列前两只尚未 Started 的蜂巢可见；选中仍由 Command 写入。
- Save schema/version/migration: None，离线首关没有进行中的持久存档。
- State mutation path: 现有 Button→HiveHopController→ActivateHiveCommand→Model；View 只读队列。

## UI, prefab, scene, and Inspector impact

- Affected assets: Assets/Scenes/Level01.unity、Assets/Resources/Levels/Level01.json；不改模型 Prefab。
- Loading mode and final paths: 沿用 Resources Prefabs/Gameplay/HiveRed、HiveYellow、HiveGreen、HiveOrange 和 Levels/Level01。
- Resources keys or direct reference owners: LevelOneAssets 缓存模型，HiveModelView 持有实例；沿用 HUD.QueueButtons/QueueLabels/NextLabels。
- Load/cache/instance cleanup responsibility: 每蜂巢最多创建一次、复用，视口或状态变化才布局，随场景销毁。
- Hierarchy or binding changes: 保留原按钮与 NextLabels 引用，仅改两排 RectTransform 锚点和后排数字样式；后排 Text 承接射线但没有 Button，避免穿透操作场景。
- Inspector confirmation: 开局十个模型；两行无交叠；数字对准 NumberAnchor；前排按钮可点击，后排不可激活；重开恢复。

## Integrations and failure behavior

- SDK/consent/lifecycle: 无新 SDK；沿用设置/后台暂停。
- Content update/offline/rollback: 本地 JSON，额度按原颜色总数拆分，保留旧输入在工作区备份。
- Cancellation, timeout, retry: 满槽/暂停/跃动中不接收新点击；Retry 恢复两行；空列隐藏模型和数字。

## Performance considerations

待选最多10个实例，每蜂巢2 Renderer；沿用共享材质和实例缓存，不增加逐帧 Resources 或架构查询；5个额外实例只在可见时呈现。未进行 iPhone 真机性能测量。

## Acceptance criteria

- [x] 开局五列两行共10个真实模型，模型和数字同样大小、无重叠，颜色总额度仍522。
- [x] 后排不触发入槽；点击前排完成跃动，原后排实例向前补位，数字跟随。
- [x] 暂停/Retry/满槽/重复点击和522方块通关回归通过。
- [x] 场景绑定保存，Foundation 验证与 Unity 编译通过；竖屏截图读回确认。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed，0 warnings |
| Unity compilation | MCP 编译及 Console | Passed，0 errors |
| Focused tests | 两行显示/后排输入/补位/Retry；原额度、完整交付回归 | EditMode 26/26、PlayMode 31/31；加强后排第一射线命中断言后专项 1/1 Passed |
| Runtime or asset behavior | 两排截图、屏幕边界和数字定位读回 | 1080×1920；10个可见模型、20 Renderer；两行模型边界无重叠且均在屏内；数字定位误差≤0.000104px；后排命中 Next_0…4 |
| Performance | 实例复用和共享资源审查 | Passed：缓存复用，无新逐帧资源加载；iPhone未测 |

## Definition of Done

- [x] Ownership and QFramework roles match the implemented files.
- [x] Public behavior and acceptance criteria are implemented.
- [x] UI, scene and Inspector impacts handled.
- [x] Convention validation and Unity compilation pass.
- [x] Focused tests and runtime behavior pass.
- [x] Performance limits reported.
- [x] Verified only after required evidence exists.

## Decision log

- 2026-10-11：用户要求底下等待的小蜂巢至少显示两行，按截图采用五列两排实物预览；开局补齐每列第二只，沿用只可选择前排的既有规则。

## Verification artifacts

- 预览：`/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level-one/hive-two-rows/待选蜂巢-两行.png`
- 运行时模型边界、数字定位与射线读回：同目录 `queue-readback.json`。
- 测试结果：同目录 `verification.json`，包含修复前失败、修复后全套通过与加强断言后的专项通过证据。
- 代码审查无 Critical/Important 问题；后排点击第一命中断言已补强并验证。
