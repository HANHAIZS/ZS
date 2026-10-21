---
id: flower-auto-rotation
title: 全关卡模型自动旋转与手动拖拽接管
status: Verified
owner: Project
created: 2026-10-12
updated: 2026-10-20
---

# 全关卡模型自动旋转与手动拖拽接管

## Goal

2026-10-20用户要求模型自动旋转，玩家手动旋转时停止，松手后继续。沿用已有每秒10度、俯视顺时针的慢速旋转，全部15关默认启用；从玩家当前角度继续，不跳角，不加额外等待。

## User-visible behavior

- 全部15个活跃关卡中，采集模型绕世界Y轴正向每秒10°连续旋转；这个方向从上方俯视为顺时针。
- 玩家拖动花朵时手动旋转接管；释放后从当前角度继续自转，不跳回旧角度。
- 设置、主界面、后台、完成或死锁时停止自转；恢复游戏后继续。Retry重置初始方向。
- 先旋转再发布现有相机可见性快照，蜜蜂继续使用当前采集、搬运动画及遮挡规则。

## Out of scope

蜜蜂动画和模型、大蜂巢位置、采集规则、道具计数和UI布局。无新模型、素材、存档或SDK。

## Ownership and architecture

| Responsibility | Owner | Type/path | Notes |
| --- | --- | --- | --- |
| Unity presentation | ViewController | HarvestBoardView | 旋转、拖拽接管和复位 |
| Frame ordering | ViewController | LevelOneController | ObserveInput → rotation → PublishCameraView → Advance |
| State mutation | Existing Command | Existing UpdateHarvestViewCommand | 相机快照沿用原Command |
| Shared rules/state | Existing System/Model | HarvestSystem/HarvestModel | 无修改，读取缓存状态 |

## State and persistence

角度只属于现有Transform表现，无存档或schema变更。暂停和主界面沿用已有模型/导航状态及UI门禁，不直接写Model。Restart复位角度。

## UI, prefab, scene, and Inspector impact

影响Level01至Level15的共享HarvestBoardView默认启用状态；LevelOneController既有调用顺序保持。模型资源键与现有LevelOneAssets加载和生命周期沿用。无素材、场景、Prefab、层级、Inspector绑定修改。

## Integrations and failure behavior

离线功能。暂停/后台/拖拽停止推进；负数或非有限时间不推进；Retry角度复位。正常时间使用现有关卡delta，采集规则和道具加速不改变花朵旋转速度。

## Performance considerations

每帧一个Transform旋转和常数运算，无分配、加载、查找或新订阅。现有可见性快照本来每帧发布；连续旋转将触发原视角可见性缓存失效，测试和性能检查包含这一部分。设备触摸手感/完整CPU与GPU性能需真机。

## Acceptance criteria

- [x] 花朵自动俯视顺时针转动。
- [x] 拖拽接管、释放继续、设置/Home/后台暂停、Retry复位。
- [x] 现有采集、搬运、道具及可见性回归正常。
- [x] 两个生产脚本以外的现有生产代码、模型和场景保持当前版本。

## Verification matrix

| Boundary | Evidence | Result |
| --- | --- | --- |
| Compile/conventions | Unity Console and project validator | Passed |
| Focused behavior | Clockwise motion, pause/navigation/background, drag/Retry tests | Passed |
| Regressions | Existing PlayMode and relevant view rules | Passed |
| Runtime | Actual model angle readback/scene capture | Passed |
| Performance | Allocation/lookup review, rotation + visibility timing | Passed |

## Definition of Done

- [x] Minimal change and QFramework ownership preserved.
- [x] Focused and relevant regressions pass.
- [x] Compile and project conventions pass.
- [x] Actual runtime direction verified.
- [x] Performance and device limits recorded.
- [x] Verified only after evidence exists.

## Decision log

- 2026-10-12: 沿用先前已说明的10°/s慢速默认；这次范围仅花朵自转和既有交互衔接。

## Verification evidence

- RED: 4 new runtime tests fail against the prior production scripts. GREEN: all 53 PlayMode and 79 EditMode tests pass (132 total). Exact results: /Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/flower-auto-rotation.
- Runtime: 41 actual Level01 frames, world Y 0→80° in 2° increments; visual direction confirmed.
- Project validator: exit 0, 0 warnings. Unity Console: 0 errors.
- Editor manual CPU profile: 120 ticks, peak 15 active bees; whole frame mean 5.794 ms, max 26.192 ms; rotation and snapshot mean 0.01093 ms. Includes existing simulation; no baseline or device/GPU result.
- Review: no Critical/Important findings. Drag test simulates the private gate; actual OS mouse-down angle seeding was inspected, device touch hand-feel remains untested.


## 2026-10-20 全关卡默认启用验收
- [x] 无测试/运行时手工开启，15关模型自动顺时针旋转。
- [x] 手动拖拽门禁停止自转，松手后沿当前角度继续。
- [x] 暂停弹窗/Home/后台/胜利/死锁门禁与Retry复位保持。
- [x] 完整Unity回归、规范、实际场景角度验证和原资源保持证据通过，并记录输入自动化边界。

本次仅改变共享View的默认启用值，无场景/Prefab/美术/经济/音频/存档改动；无新每帧分配、资源或依赖查询。原2026-10-12证据仅作为历史记录，本次待新证据验证。


## 2026-10-20 验证证据与边界
- 生产差异仅HarvestBoardView.AutoRotate默认值改为true；15关复用原ObserveInput→TickAutoRotation顺序，速度10度/秒、手动按下停止、松手从当前角度继续；暂停/后台/Home/Won/Deadlocked/Retry门禁原样保留。
- RED job bbb33688220d4d2b9dfabe69f8082553：去除手工开启后，5/5因默认不旋转而失败。GREEN ba0d8924543b40bf8b95d0c44250ebab：5/5通过，新增一项逐个加载15场景测量真实顺时针运动，旧测试继续覆盖手动门禁和当前姿态恢复、弹窗/Home、后台和Retry。
- 最终EditMode169/169（77cee892463a4aa88fa9bcc3e10b3208），PlayMode161/161（5c09d695f9b94be8a292549e8e2fc323），零失败/跳过；Foundation规范0警告，最终Console错误/警告0。
- FixedGripTransportTests旧的“默认静止”用例与新需求冲突，改名ExplicitFixedView并显式设置AutoRotate=false；保持静态调试视角、手动朝向及采集后不回正断言，没有改变生产行为。独立只读审查确认该项并未发现生产缺陷。
- 原生Level01角度采样18069帧，59.9869模拟秒推进599.8539度，平均9.99974度/秒。背景UI自动化未拿到有效按下/拖拽帧，不冒称完成物理鼠标手势验收；手动停止/释放恢复由真实模型的受控旋转门禁测试和原输入顺序检查验证，设备手感仍待真机。
- 现有场景/Prefab/模型/材质/字体/音频逐字节保持，Editor测试重建的3份菜单资产与一份材质舍入噪声已通过Unity FileUtil恢复。无新资源、注册、订阅或每帧分配/查找；无Inspector待办。
- 证据目录：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/model-auto-rotation；red-results、green-results、edit-results、full-play-results、native-motion-summary、pointer-frames、final-scope、final-console。
- 上方2026-10-12的性能记录为历史证据；未新增设备CPU/GPU性能测量。持续自转沿用既有可见性缓存失效和采集刷新，所有当前关卡及搬运回归已验证。
