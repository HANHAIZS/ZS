---
id: level-one-bee-pickup
title: 第一关蜜蜂悬停采集动画
status: Verified
owner: Project
created: 2026-10-10
updated: 2026-10-10
---

# 第一关蜜蜂悬停采集动画

## Goal

按用户要求，让蜜蜂在采集点短暂停顿，增加取到方块的明确动作，同时保留上一版防穿模效果。

## User-visible behavior

- 出巢后飞行0.6秒到采集点，悬停约0.25秒完成采集：前0.12秒轻微下探，取到方块后用0.13秒将方块弹起收拢，身体回升，随后飞回大蜂巢。
- 方块在下探结束时才从花朵移除；搬运额度仍在蜜蜂飞出时扣减，交付后才计入完成。
- 采集动作随游戏暂停冻结；Retry恢复全部方块并回收蜜蜂，新蜜蜂没有残留下探／方块缩放。
- 保留多只蜜蜂避让、身体与方块间距、安全飞行平面、回巢入口收拢和原Blender模型。

## Out of scope

新模型、粒子资源、音频、UI、后续关卡、物理系统及规则重设计。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | Commands/Harvest/HarvestCommands.cs | 采集／交付时序仍由AdvanceHarvestCommand统一控制 |
| Observable or persistent state | Model | 现有HarvestModel | 保留0去程、1载货、2完成阶段，无新增存档 |
| Shared project rules | System | 现有HarvestSystem | 预留方块仍阻挡内部方块，不能重复采集 |
| One-shot notification | Event | 现有HarvestChangedEvent | 保留取块与交付事件 |
| Unity presentation | ViewController | ViewControllers/Harvest/BeeFlightView.cs | 下探、方块弹起、悬停及动画范围纳入避让 |
| Adapter or repository | Utility | 现有LevelOneAssets | 复用缓存的模型、Mesh与Material |
| Reusable infrastructure | None | None | 无Foundation包改动 |

## State and persistence

- Runtime state: 去程包含0.12秒下探，载货阶段包含0.13秒收拢；保留Flight.Progress由Command推进，表现只读。
- Save schema/version/migration: None。
- State mutation path: Command写Model；View只修改Actor的Transform。暂停不使用墙钟或独立协程推进动作。

## UI, prefab, scene, and Inspector impact

- Affected assets: Level01中既有BeeFlightView运行时行为，无序列化资产变更。
- Loading mode and final paths: 保留Assets/Resources/Prefabs/Gameplay/{Bee,Flower,Collector}.prefab。
- Resources keys or direct reference owners: LevelOneAssets缓存Gameplay资源。
- Load/cache/instance cleanup responsibility: 复用Actor池；Renderer、翅膀、动画总包围盒创建时缓存；重开清理旧任务映射。
- Hierarchy or binding changes: 复用CarriedVoxel，调整局部位置／尺寸，无新Inspector字段。
- Inspector confirmation: 通过MCP运行实际Blender模型采集并截帧确认。

## Integrations and failure behavior

- SDK/consent/lifecycle: None；保留设置弹窗与后台暂停。
- Content update/offline/rollback: 离线；work/bee-pickup/before保存原脚本。
- Cancellation, timeout, retry: 暂停冻结下探／收拢；Retry立即回收旧蜜蜂，下一轮Actor姿态复位；大步长可跨越动作但状态只结算一次。

## Performance considerations

最多15只蜜蜂，沿用对象池与缓存包围盒。每帧仅计算少量曲线与Transform；无新Collider、物理计算或逐帧资源加载。iPhone性能仍需设备实测。

## Acceptance criteria

- [x] 到达采集点后先停顿、下探，不能到达即取走方块。
- [x] 取块时方块从下方弹起收拢，动作完成后才回巢。
- [x] 悬停／取块／回程全阶段均保持方块与蜜蜂、蜜蜂互相、花朵与大蜂巢的安全间距。
- [x] 暂停冻结动画，Retry及池复用恢复初始姿态；458块完整通关与额度规则保留。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed，0 warning |
| Unity compilation | 无新编译错误，最终Console无Error | 编译完成；干净运行最终Console Error=0 |
| Focused tests | 采集时间／真实模型悬停收拢／暂停复位；全部项目Editor与PlayMode回归 | EditMode 16/16、PlayMode 22/22通过，0失败／跳过 |
| Runtime or asset behavior | MCP运行采集过程截帧／动画预览；Level01场景校验 | 82帧真实场景截取，逐帧及全景目视确认；Level01无问题 |
| Performance | 池化、缓存及动画范围审查 | 沿用15只并发池，创建时扩展动画包围盒；独立审查无重要问题；设备性能未实测 |

## Definition of Done

- [x] Ownership and QFramework roles match implementation.
- [x] Acceptance criteria implemented.
- [x] Asset/Inspector impact checked.
- [x] Convention validation and compilation pass.
- [x] Tests and runtime evidence pass.
- [x] Platform limitations recorded.
- [x] Status Verified only after evidence exists.

## Decision log

- 2026-10-10：现有蜜蜂到达即回程，无悬停或取块反馈。本次将0.25秒采集动作分配到现有去程／载货两阶段，不新增Model阶段或资产。用户已明确授权制作动画。


## Completion evidence

- 先行EditMode测试job `942ec922bc8e4de3a52342a7adca5397`：到达即移除方块，1/1失败；先行PlayMode job `fe1b985fe98b46c38bde8d99941feeed`：原动画无身体下探、到达即出现携带物，2/2失败。
- 最终EditMode job `10211ec2c2eb4dd390884df0a77b30fb`：16/16通过；最终PlayMode job `2d1c875892d54b1a92d1b754fde83aa3`：22/22通过。合计38，0失败／跳过。
- 新增测试直接验证到达停顿、逻辑取块边界、托起后交付、悬停位置、身体下探、Cargo升起与缩放、携带物不与身体相交、暂停冻结翅膀与姿态、Retry池复用复位。
- 既有回归保留15只并发、花朵旋转、防穿模、回巢入口、槽位即时复用、出巢扣额度、458块完整通关及设置按钮流程。旧测试只将原时间假设对齐新增采集时序，没有放宽几何断言。
- 生产脚本只有HarvestCommands.cs与BeeFlightView.cs；无需绑定Inspector字段，无Scene、Prefab、Blender模型或Foundation包改动。
- MCP for Unity 10.3.0完成所有Unity操作；沿用同一localhost MCP服务的直接协议连接。
- 预览期间一次Unity请求重新编译并域重载，失效的运行时缓存产生SettingsPopupController旧初始化异常及诊断RestartLevel异常。记录已保存到work/bee-pickup；停止并重新加载Level01后干净预览正常，最终Console Error=0。这次环境中断未计入通过测试或有效全景证据。
- 82帧采集图像保留于Captures/bee-pickup；工作区outputs/level-one/bee-pickup包含0.5倍速GIF、四阶段PNG、有效全景与检查JSON。特写临时只显示一只蜜蜂以清楚展示动作；全景恢复三只，未修改游戏并发数。
- 独立只读审查未发现Critical或Important问题；首次渲染即载货的代码路径已检查，专门测试作为后续可选补充。
- iPhone触控、耗时与发热未进行设备实测；现有验证为Unity Editor。
