---
id: level-one-bee-clearance
title: 第一关蜜蜂搬运动画防穿模
status: Verified
owner: Project
created: 2026-10-10
updated: 2026-10-10
---

# 第一关蜜蜂搬运动画防穿模

## Goal

修复小蜜蜂携带方块与身体重合、同点出发／回程的蜜蜂互相重合及采集花朵时进入模型内部的问题。用户授权直接修复现有动画。

## User-visible behavior

- 携带方块位于蜜蜂身体下方，按实际模型包围盒留缝，蜜蜂保持直立，只切换左右镜像朝向，保留面向镜头。
- 飞行在花朵与蜂巢前方的安全平面进行，起终点投影仍对齐入口和采集块；到达大蜂巢时缩小进入入口。
- 并发蜜蜂按身体、翅膀及携带物的整体尺寸避让，暂停时位置固定；重开回收旧蜜蜂。
- 保留三只／小蜂巢、458方块、出巢扣额度、取块／交付分开计数及点击跃动规则。

## Out of scope

新模型、物理刚体系统、导航网格、后续关卡、UI与音频改动。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | 现有HarvestCommands.cs | 不改变模拟时间及数值规则 |
| Observable or persistent state | Model | 现有HarvestModel | 不新增存档 |
| Shared project rules | None | None | 仅表示层避让 |
| One-shot notification | None | None | 无新增事件 |
| Unity presentation | ViewController | ViewControllers/Harvest/BeeFlightView.cs、HarvestBoardView.cs | 缓存包围盒、投影安全平面、池化并发避让 |
| Adapter or repository | Utility | 现有LevelOneAssets | 保留资源缓存 |
| Reusable infrastructure | None | None | 不修改Foundation包 |

## State and persistence

- Runtime state: Actor的包围盒、上一渲染时间与位置，重开Generation清理。
- Save schema/version/migration: None。
- State mutation path: 保留Command写Model，渲染只读状态。

## UI, prefab, scene, and Inspector impact

- Affected assets: 无序列化资产变更；Level01现有两个View组件执行新动画。
- Loading mode and final paths: 保留Assets/Resources/Prefabs/Gameplay/{Bee,Flower,Collector}.prefab。
- Resources keys or direct reference owners: LevelOneAssets缓存Prefabs/Gameplay资源。
- Load/cache/instance cleanup responsibility: BeeFlightView池化，场景卸载销毁；包围盒在建Actor时计算，不逐帧扫描模型组件。
- Hierarchy or binding changes: 不改Inspector引用；调整运行时CarriedVoxel位置。
- Inspector confirmation: 通过MCP运行读取模型间距及截图确认。

## Integrations and failure behavior

- SDK/consent/lifecycle: None；保持后台暂停。
- Content update/offline/rollback: 离线；工作区备份原脚本。
- Cancellation, timeout, retry: Pause冻结运动、Retry清空旧Actor映射；槽位立即复用时仍保留正确入口来源并分开显示蜜蜂。

## Performance considerations

最多15个Actor；缓存Renderer/Wing及包围盒，避让仅检查并发Actor，不向每只蜜蜂增加Collider/Rigidbody；iPhone耗时尚需实测。

## Acceptance criteria

- [x] 实际Blender蜜蜂与携带方块包围盒不交叉。
- [x] 出发、取块、回程各阶段并发蜜蜂整体包围盒不交叉。
- [x] 蜜蜂不穿过花朵／大蜂巢；旋转花朵后仍不穿模。
- [x] 暂停、重开、即刻复用槽位及完整通关回归通过。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed，0 warning |
| Unity compilation | 无新错误 | 重新编译已完成，最终Console Error=0 |
| Focused tests | 真实场景携带物／蜜蜂整体／旋转安全距离回归 | PlayMode20/20、EditMode15/15；4项防穿模／回巢回归包含实际Renderer包围盒检测 |
| Runtime or asset behavior | 搬运截图及几何读回 | 搬运全景／特写目视确认；Level01场景无问题；无永久模型／Prefab／Scene改动 |
| Performance | 缓存及15只并发检查 | Renderer、Wings和全周期包围盒建Actor时缓存；最多15只，纯Bounds避让，无新增逐帧架构查找或物理组件；iPhone性能未实测 |

## Definition of Done

- [x] Ownership and QFramework roles match implementation.
- [x] Acceptance criteria implemented.
- [x] Asset/Inspector impact checked.
- [x] Convention validation and compilation pass.
- [x] Tests and runtime evidence pass.
- [x] Platform limitations recorded.
- [x] Status Verified only after evidence exists.

## Decision log

- 2026-10-10：MCP复现：携带方块同时与头部／腹部／手脚包围盒相交；三个任务进度同为0且同点出发；旧Target仅在方块前偏移0.28。修复仅属于游戏表示层。

## Completion evidence

- 原穿模失败重现job `3147fd640654482c8aa41771eda6d555`：3/3失败，分别为方块与手臂相交、并发蜜蜂出发重合、飞行穿过Collector。
- 回巢入口失败重现job `5b71cc4302264ad68d6e9afabe05ad7f`：原满尺寸避让导致蜂距入口72.68px；按当前缩放计算避让并平滑收拢后，10px内入口测试通过。
- 最终PlayMode job `9e5057b20eb64c4eb2a8fc1c424cb549`：20/20通过，0失败／跳过，19.62秒。
- 最终EditMode job `56ce99c0c9c349eb8f6c1c5a6387ab8f`：15/15通过，0失败／跳过。
- 包括15只并发蜜蜂各阶段的整体Renderer包围盒、携带物与各部件、花朵旋转扫角、大蜂巢、暂停、重开和即时槽位复用；完整458方块通关回归保留。
- 独立只读审查发现并修复入口旁消失问题，复查无剩余具体遗漏。
- 通过本地MCP for Unity10.3.0执行全部Unity操作。内置连接失效时直接连接同一个本地10.3.0 MCP服务。一次早期测试被编译域重载中断，不计入成功；0用例的错误筛选不计入测试总数。
- 最新截图：Captures/bee-clearance-after.png、Captures/bee-clearance-detail.png；工作区outputs/level-one/bee-clearance保留图像、测试和场景检查JSON。
- 改动生产脚本：BeeFlightView.cs、HarvestBoardView.cs；新增BeeClearanceTests.cs，HivePresentationTests更新旧的同点显示断言以保留逻辑入口一致、增加视觉间距。
- 原采集Commands、Model、额度、三只并发上限和Resources模型路径不变。没有新增Inspector字段需要手动绑定。
- Editor几何和画面验证通过；iPhone触控及设备性能仍未实测。
