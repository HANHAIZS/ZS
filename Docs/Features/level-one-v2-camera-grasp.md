---
id: level-one-v2-camera-grasp
title: 第一关新版模型、相机可见采集与四肢抓握
status: Verified
owner: Project
created: 2026-10-11
updated: 2026-10-11
---

# 第一关新版模型、相机可见采集与四肢抓握

## Goal

将已完成的九色蜜蜂、九色六层小蜂巢、七层大蜂巢与 522 方块花朵接入第一关，蜜蜂只采相机可见的方块，呈现对准、轻震、四肢抓取、连续移入手中及返巢过程。

## User-visible behavior

- 花朵使用赤红 316、黄色 78、绿色 84、橙色 44 共 522 个独立方块；所有采集颜色均属于九色蜜蜂和小蜂巢调色板。
- 九种蜜蜂与蜂巢均提供生产 Prefab；第一关出巢蜜蜂、小蜂巢与花朵对应色一致，固定黑色结构与白色翅膀不变色。
- 五槽、点击跃动入槽、逐只出蜂、出蜂扣额度、归零释放槽位、交付累计、设置和 Retry 沿用已有流程。
- 可采方块至少存在一处朝向相机且未被其他未取走方块挡住的可见面。已预留但尚未取走的方块仍然遮挡后层。相机或花朵旋转重新计算可见性。
- 蜜蜂先将头部与身体平滑转向目标，锁定时短促轻震，再让四肢伸出、抓住方块；方块从原花朵姿态连续过渡到四肢抓握位置，托稳后返巢。
- 目标在取走前变为遮挡状态时悬停等待，重新可见后继续，避免穿过背面采集。暂时无可见方块时等待旋转或前层被取走，不误判失败。
- 身体、翼面、其他蜜蜂和花朵保持几何间距；抓握脚端允许与方块表面接触。
- 暂停冻结所有姿态与进度；Retry 清除旧任务、恢复 522 方块与额度。

## Out of scope

后续关卡、主界面、商城、体力、广告、联网、存档迁移、Foundation 包修改。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | Commands/Harvest/HarvestCommands.cs、UpdateHarvestViewCommand.cs | 视角快照、预留、取走与交付；View 不改 Model |
| Observable or persistent state | Model | Models/Harvest/HarvestModel.cs、LevelData.cs | 本地相机方向及视角版本，仅运行时 |
| Shared project rules | System | Systems/Harvest/HarvestSystem.cs | 相机朝向面、遮挡和颜色判定 |
| One-shot notification | Event | 既有 HarvestChangedEvent | 取走／交付事件 |
| Unity presentation | ViewController | HarvestBoardView.cs、BeeFlightView.cs、BeeGraspPose.cs、HiveModelView.cs、LevelOneController.cs | 相机快照、连续抓取、四肢和翼根、定位点 |
| Adapter or repository | Utility | Utilities/Harvest/LevelOneAssets.cs | 缓存九色 Prefab、严格颜色映射 |
| Editor assets | Project | Editor/LevelOneV2AssetsSetup.cs | Unity Editor API 导入、材质、Prefab、关卡 JSON |
| Reusable infrastructure | None | None | 不修改 Foundation |

## State and persistence

- Runtime state: 相机在方块网格空间中的方向与位置；可见性缓存；飞行阶段与抓取姿态；按 Generation 复位。
- Save schema/version/migration: None。
- State mutation path: 所有模型变更由 QFramework Command 执行，Systems 只提供规则，ViewControllers 发送 Command 并显示。

## UI, prefab, scene, and Inspector impact

- Affected assets: Assets/Scenes/Level01.unity；现有 Flower、Collector、Bee 与三色蜂巢 Prefab；新增九色 Bee/Hive Prefab；Level01.json。
- Loading mode and final paths: Resources，Assets/Resources/Prefabs/Gameplay/{Flower,Collector,Bee,BeeRed..BeeBlack,HiveRed..HiveBlack}.prefab；Assets/Resources/Levels/Level01.json；源 FBX 与材质位于 Assets/Sprites/Gameplay/V2。
- Resources keys or direct reference owners: LevelOneAssets 缓存 Prefabs/Gameplay/；Prefab 直接引用 V2 模型与材质。
- Load/cache/instance cleanup responsibility: LevelOneAssets 缓存资源，BeeFlightView 按颜色复用 Actor 池，重开清除活动姿态；Board/HiveView 随场景生命周期销毁。
- Hierarchy or binding changes: 新蜜蜂保持原外形与双翼转轴，补四个独立四肢转轴／脚端定位点；小蜂巢使用 Mouth/NumberAnchor，大蜂巢使用 Mouth/Inside 定位点；保留原 UI/Settings 绑定和路径。
- Inspector confirmation: 使用 MCP 读回模型尺寸、颜色、层数、独立方块与定位点；运行原场景截图。

## Integrations and failure behavior

- SDK/consent/lifecycle: None；沿用设置和后台暂停。
- Content update/offline/rollback: 离线单机，不引入远程加载；旧文件备份到本任务工作区，源模型保持。
- Cancellation, timeout, retry: 无可见目标则等待，不消耗额外额度；旋转时已预留目标在取走前重新验证；暂停不累积补发，Retry 不残留抓取物。

## Performance considerations

相机视角或 BoardRevision 变化时才更新遮挡缓存；规则使用逻辑体素网格与射线遍历，避免每个方块添加物理组件。Actor 按九色池化，材质与网格共享；四肢和翼根查找只在创建时执行。每个蜂巢最多 3 只并发；额度归零即释放槽位，因此新旧蜂巢可以同时有在途蜜蜂，不设 15 只全局上限。已覆盖超过 15 只的新旧蜂巢交叠。编辑器 15 只蜜蜂测量：动画 RenderFlights 中位数 3.44 ms、P95 3.76 ms、稳态托管分配 0 bytes/call；522 方块视角重算中位数 2.95 ms、P95 3.30 ms，缓存命中约 0.0013 ms。仅编辑器 CPU 结果，不含 GPU，iPhone 真机性能未测量。

## Acceptance criteria

- [x] 九色蜜蜂与九色蜂巢资源可加载，第一关新花朵 522 方块全部与数据对应，颜色额度精确匹配。
- [x] 全遮挡后层不可预留；前层预留仍遮挡；取走后解锁；旋转后使用新视角；可见侧面允许采集。
- [x] 蜜蜂头身对准目标，有锁定轻震、四肢抓取及完整位置／旋转／大小连续过渡。
- [x] 身体／翅膀、花朵、蜂巢、其他蜜蜂不穿模；脚端接触方块符合抓握动作。
- [x] 暂停／设置／Retry／唯一预留／额度及完整关卡交付回归通过。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed，0 warnings |
| Unity compilation | MCP 编译与 Console 无新 Error | Passed；自然 Play 运行再次检查 |
| Focused tests | 相机遮挡 EditMode；新版素材与额度；四肢／朝向／连续交接 PlayMode | Passed，26/26 EditMode，30/30 PlayMode |
| Runtime or asset behavior | 原场景截图及多阶段动画渲染、精确定位点读回 | Passed，asset-readback.json、第一关-新版素材.png、40 帧采集过渡.gif |
| Performance | 缓存与分配审查；编辑器运行统计 | editor-performance.json；iPhone 未实测 |

## Definition of Done

- [x] Ownership and QFramework roles match the implemented files.
- [x] Public behavior and acceptance criteria are implemented.
- [x] Asset and Inspector impacts handled.
- [x] Convention validation and Unity compilation pass.
- [x] Focused tests and exact behavior checks pass.
- [x] Performance risks and manual device checks reported.
- [x] Verified only after required evidence exists.

## Decision log

- 2026-10-11：新相机可见规则替代旧外围空气规则；部分可见的朝向面可采，全遮挡面不可采。
- 2026-10-11：保留整体关卡布局和 Resources 加载边界；新版模型按原画面高度适配，不重建 UI。
- 2026-10-11：给已建蜜蜂拆出独立四肢转轴，保持原几何与材质，用模拟时间驱动抓握，暂停可冻结。

## Verification details — 2026-10-11

- EditMode job: 99078d923b1d4b7eb0320f2ce79e9a60；26 passed。PlayMode job: 9de5a7379d7946a185156219bceb41df；30 passed。完整原始结果见任务输出 verification.json。
- 回归覆盖：522 方块完整交付、唯一预留、前后层遮挡、预留仍遮挡、前层取走解锁、正反／侧面／斜角可见性、旋转隐藏后恢复、抓取进度冻结、四肢触点与左右侧一致、转向逐帧变化、锁定轻震、连续方块交接、身体／翼面与携带块间距、多蜂与新旧蜂巢交叠间距、设置暂停／退出／Retry／输入阻挡。
- 可见性采用朝向面内 13 点射线的保守判定；面中心被遮挡但边缘露出的斜视角夹具已通过。小于采样间距的极小露出区域可能仍等待，避免把细缝作为主要采集入口。
- 独立代码复核发现的抓取进度倒退已用失败测试重现并修复；补充了左右触点、转向连续性和斜面可见性回归。修复后复核无 Critical / Important 问题。
- MCP 的合成截图函数触发了编辑器 PlayerLoop 递归日志，堆栈位于 MCP ScreenshotUtility；改用原生异步 ScreenCapture 保存含 UI 画面。临时检查相机已移除，重新进入正常运行验证。未修改 MCP 包。
- iPhone 真机帧率、GPU 和长时间内存使用仍需设备验证。当前结果不代表真机性能。
