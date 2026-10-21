---
id: level-one-bee-handoff
title: 第一关逐只出巢与方块连续交接
status: Verified
owner: Project
created: 2026-10-10
updated: 2026-10-10
---

# 第一关逐只出巢与方块连续交接

## Goal

让小蜂巢逐只放出蜜蜂，让方块从大花朵原位置连续移动到蜜蜂手下，消除原方块消失后携带物在另一位置突然出现的问题。

## User-visible behavior

- 小蜂巢落槽后立即放出一只，后续每隔约0.3秒放出一只；保留每巢最多3只已出巢蜜蜂并行搬运。单次大步长也不能一次补发多只。
- 数字在每只蜜蜂实际飞出时减一，不预扣后三只；归零立即释放槽位。
- 蜜蜂到达后下探；取块时携带物从花朵原方块的屏幕位置、大小、朝向及材质接续显示，用0.28秒移入蜜蜂手下，托稳再回巢。
- 在安全前景深度层按原位置投影交接，不让模型穿入花朵；使用连续位置／旋转／大小曲线，无透明消失或突然缩小重生。
- 暂停冻结出巢间隔与方块过渡；Retry恢复花朵和额度，清理旧蜜蜂与交接起点；旋转花朵不带走已取出的方块。

## Out of scope

新模型、UI、声音、后续关卡、物理系统、存档及Foundation包变更。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | Commands/Harvest/HarvestCommands.cs | 逐只预留、扣额度、交接与交付结算 |
| Observable or persistent state | Model | Models/Harvest/LevelData.cs | HiveState的出巢冷却，仅运行时状态 |
| Shared project rules | System | 现有HarvestSystem | 保留外围方块、颜色与唯一预留规则 |
| One-shot notification | Event | 现有HarvestChangedEvent | 保留取块／交付事件 |
| Unity presentation | ViewController | BeeFlightView.cs、HarvestBoardView.cs | 缓存原方块姿态，连续交接，蜜蜂悬停高度与整段交接扫掠范围 |
| Adapter or repository | Utility | 现有LevelOneAssets | 不迁移资源 |
| Reusable infrastructure | None | None | 不修改Foundation |

## State and persistence

- Runtime state: 每巢出巢冷却；Actor的交接原姿态／朝向／前景深度偏移，随Generation清除。
- Save schema/version/migration: None。
- State mutation path: Command推进冷却与Flight.Progress；View只读Model并渲染，原方块与携带物在同一渲染帧交接，取块／交付只计一次。

## UI, prefab, scene, and Inspector impact

- Affected assets: 无序列化资产改动，Level01中既有View执行新表现。
- Loading mode and final paths: 保留Assets/Resources/Prefabs/Gameplay/{Bee,Flower,Collector}.prefab。
- Resources keys or direct reference owners: LevelOneAssets缓存Gameplay资源；HarvestBoardView持有原方块Transform／Renderer。
- Load/cache/instance cleanup responsibility: 沿用Actor池，缓存Mesh、Renderer、翅膀和包围盒；重开复位原方块显示，清理交接姿态。
- Hierarchy or binding changes: 复用CarriedVoxel，无新Inspector字段。
- Inspector confirmation: MCP逐帧读回原块／移动块姿态和实际运行截图。

## Integrations and failure behavior

- SDK/consent/lifecycle: None；保持设置／后台暂停。
- Content update/offline/rollback: 离线，work/bee-handoff/before备份。
- Cancellation, timeout, retry: 无匹配表层方块则等待；恢复可采颜色后仍只放出一只。暂停不累计补发；大步长不爆发放蜂；首次渲染直接处于载货阶段仍正确初始化交接。

## Performance considerations

最多15只并发，沿用对象池与资源缓存；每帧只遍历现有任务／五个槽位。交接姿态在阶段切换时缓存，无新资源或物理组件。iPhone性能未实测。

## Acceptance criteria

- [x] 各蜂巢初次只飞出一只，后续有间隔，暂停／零步长／大步长不瞬间多发或过扣。
- [x] 原方块与携带物交接起点在屏幕上连续，大小／朝向／材质相同；随后平滑移动到手下，托稳才回程。
- [x] 交接全过程不与蜜蜂身体相交，保留多蜂、花朵／蜂巢防穿模。
- [x] 暂停／重开／旋转及初次晚渲染正常，458块通关、唯一预留和即刻槽位复用回归通过。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed，0 warnings |
| Unity compilation | 编译完成，无新Error | MCP确认编译／域重载结束，实际预览Error为0 |
| Focused tests | 逐只出巢、真实源块与携带物投影连续、过渡／暂停／复位及全项目回归 | EditMode 18/18，PlayMode 27/27；含完整458块错峰／旋转采集及近裁剪检查 |
| Runtime or asset behavior | MCP逐帧特写、全景、场景校验 | 95帧原生渲染，特写GIF、四阶段图、750×1334全景；场景0问题 |
| Performance | 缓存及并发范围审查 | 已审查：最多15只，绝对前景层复用；iPhone性能未实测 |

## Definition of Done

- [x] Ownership and QFramework roles match implementation.
- [x] Acceptance criteria implemented.
- [x] Asset/Inspector impact checked.
- [x] Convention validation and compilation pass.
- [x] Tests and runtime evidence pass.
- [x] Platform limitations recorded.
- [x] Status Verified only after evidence exists.

## Decision log

- 2026-10-10：旧while循环一次生成三任务；旧Cargo从蜜蜂下方0.65倍大小出现，与原花朵方块无位置交接。用户明确要求逐只出巢和花朵到手里的连续过渡；采用0.3秒出巢间隔与0.28秒交接，保留并发额度和原关卡规则。

- 2026-10-10：并发测试捕获相邻两蜂交接范围超出缓存避让体，改为对源方块旋转扫掠体与蜂身一起检查，沿正交相机视线移至可复用的绝对前景层，保持屏幕位置、大小连续；新增0°/45°/90°/135°同时交接回归。

- 2026-10-10：真实整关错峰采集测试发现相对深度不断前推，改为按基础平面搜索绝对层、复用已腾空深度；整关近裁剪验证与所有回归一起执行。

## Verification evidence

- EditMode: job `534a586df0d549ed8bafa91d9bb5fa68`，18/18 Passed。
- PlayMode: job `6612605d52b7447fa7250ca4d99e35f6`，27/27 Passed，包含5项连续交接、4项完整几何防穿模及全关卡深度范围测试。
- 原版连续交接测试复现约60.789px位置跳变；新版交接起点小于0.5px且逐帧运动连续，大小／旋转／材质衔接一致。
- 小蜂巢出巢样本：107/1只→106/2只→105/3只；到达并返巢后下一只单独出发。
- 两次审查发现的并发避让范围与累计深度问题均修复；深度搜索从基础平面寻找空层，避免持续向相机累加；朝向判断在深度偏移前完成。
- 测试退出时第三方Unity Test Framework在PlaymodeLauncher.cs:113曾留下异常；已记录console-after-tests.json。所有验收测试明确Passed，重新载入后的实际关卡预览Error为0，未改第三方包。
- 输出：outputs/level-one/bee-handoff/{方块连续交接.gif,方块交接-四阶段.png,第一关-取块过渡.png,verification.json}。
- 限制：macOS编辑器验收；未进行iPhone真机性能／触控验证。
