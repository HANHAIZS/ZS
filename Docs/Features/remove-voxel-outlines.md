---
id: remove-voxel-outlines
title: 关闭全部模型方块描边
status: Verified
owner: Project
created: 2026-11-10
updated: 2026-11-11
---

# 关闭全部模型方块描边

## Goal
移除第1–6、96–119关共30关的深灰或黑色模型方块描边。

## User-visible behavior
119关均不绘制方块描边。原方块间隙、采集缺口阴影、光照、已有贴图、取块过渡和布局保持。

## Out of scope
不修改贴图、原材质、预制体、关卡数据、场景或运行时代码；不处理之前尚未完成的1–3关贴图。

## Ownership and architecture
Resources键Levels/PresentationCatalog中的edges开关由既有LevelOneController/HarvestBoardView使用；保留着色器和材质供既有工具使用。取块结束时使用既有无描边源材质。

## UI, prefab, scene, and Inspector impact
Assets/Resources/Levels/PresentationCatalog.json是唯一生产变更。无新资源、组件或Inspector设置。

## Lifecycle, failure, and performance
无需新生命周期逻辑；关闭描边不创建原描边材质副本，既有缺口阴影仍正常创建和释放。

## Acceptance criteria
- [x] 只将上述30个edges从true改为false；其余字段原样。
- [x] 已有渲染、搬运和资源回归使用无描边的期望。

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| All levels | 119关配置比对 | Passed：30个开关关闭，其他字节一致 |
| Rendering and cargo | 已有回归断言更新；两套测试程序集用Unity编译器编译通过 | Unity PlayMode 16/16通过，覆盖去描边渲染、材质释放、搬运和重试 |
| Foundation | 项目规范校验 | Passed：0警告 |

## Definition of Done
- [x] 记录实际验证结果，保持其他本地工作不变。
- [x] Unity隔离副本的PlayMode回归通过。

## Editor preview
项目关闭了Play Mode域重载；若编辑器仍显示旧缓存，重新打开项目再进入Play。未修改场景或现有1–3关贴图草稿。
