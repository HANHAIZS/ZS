---
id: bee-surface-cling
title: 蜜蜂趴附与花朵自转更改撤销记录
status: Verified
owner: Project
created: 2026-10-12
updated: 2026-10-12
---

# 蜜蜂趴附与花朵自转更改撤销记录

## Goal

用户要求撤销本次更改。恢复到引入趴附采集姿态和花朵自动旋转之前的版本。

## User-visible behavior

蜜蜂恢复本次更改前的抓取、提取与返巢动画。花朵恢复手动旋转，不再自动顺时针旋转。

## Out of scope

主界面、设置、四个道具、单次任意选择和现有Blender素材均不作新的修改。

## Ownership and architecture

恢复此次修改过的12个C#文件的逐字节备份，移除此次新建的4个C#文件及其Unity元数据。恢复原蜜蜂动画、花朵手动旋转、采集规则和大蜂巢运行时位置。此前道具数量、单次任意选择、主界面、设置和模型保持原版本。状态仍由原QFramework Commands所有，依赖注册不变。

操作前已比对16个文件与此次交付的SHA-256清单，确认没有后续文件改动；当前版本另存为可恢复备份。

## Acceptance criteria

- [x] 12个已有脚本与本次更改前备份完全一致。
- [x] 4个新增脚本及其Unity元数据已移除。
- [x] Unity恢复编译，Console无错误，项目约定验证通过。
- [x] 运行时花朵没有自动旋转，原采集与道具回归通过。

## Impact and evidence

作用于Level01运行时组件；没有场景、Prefab、模型或素材文件变更，无新增Inspector绑定。还原不引入新的性能开销，设备检查沿用原版本范围。

恢复文件清单与备份：[撤销计划](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/bee-cling/undo/plan.json)、[撤销前版本](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/bee-cling/undo/before-undo)。此前实现与测试记录保留在工作区，表示已撤销功能的历史证据。

## Verification

12个文件逐字节恢复，4个新增脚本及meta移出Unity工程。运行时两次读回花朵Y角度均为0°，期间正常Update继续运行。原有19项PlayMode采集、姿态、连续交接、防穿模、完整522方块交付与道具测试全部通过（16.953秒，job 548f80490e9d413f87fe64a10479c601）。Console无错误，项目约定验证0警告。

## Verification matrix

| Boundary | Evidence | Result |
| --- | --- | --- |
| Restore | Exact byte comparison against 12 backups; 4 added scripts/meta absent | Passed |
| Compile | Unity Console | 0 errors |
| Conventions | Game Foundation validator | Passed，0 warning |
| Runtime | Flower angle readback and original pickup/booster tests | 0°→0°，19/19 PlayMode passed |

## Definition of Done

- [x] Original sources restored exactly.
- [x] Added source files archived outside the Unity project.
- [x] Unity compiles without errors.
- [x] Convention validation passes.
- [x] Runtime rotation and focused regressions pass.
- [x] Rollback evidence recorded.

## Restored files

恢复原有7个生产脚本：`BeeFlightView`、`BeeGraspPose`、`HarvestBoardView`、`LevelOneController`、`HarvestCommands`、`LevelData`、`HarvestSystem`；恢复原有5个测试脚本：`BeeGraspAnimationTests`、`BeeClearanceTests`、`BeePickupAnimationTests`、`VoxelHandoffTests`、`HarvestBoostersUiTests`。

移出4个本次新增脚本：`HarvestGeometry`、`BeeContactRulesTests`、`BeeGeometryAssertions`、`BeeSurfaceClingTests`。无场景、Prefab、素材或Inspector变更。

最终证据：[撤销验证](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/bee-cling/undo/verification.json)、[19项回归结果](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/bee-cling/undo/playmode.json)。
