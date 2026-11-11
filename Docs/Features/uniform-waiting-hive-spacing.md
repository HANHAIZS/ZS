---
id: uniform-waiting-hive-spacing
title: 全119关统一等待蜂巢间距
status: Verified
owner: Project
created: 2026-11-11
updated: 2026-11-11
---

# 全119关统一等待蜂巢间距

## Goal
全部关卡采用第1–12、96–114关的等待蜂巢中心行距0.070和列距0.24。

## User-visible behavior
119关均为三列等待蜂巢，上下相距安全区域高度的7%，左右相距安全区域宽度的24%。以各关现有第一排中心为基准压紧后排。

## Out of scope
槽位、模型、音效、关卡玩法和光照分别沿用各自现有配置。

## Ownership and architecture
配置归属Assets/Resources/Levels/PresentationCatalog.json，经LevelOneAssets读取和校验，由LevelOneHud.ApplyPresentation与LayoutColumns应用，HiveModelView依同一控件中心投影模型。沿用现有初始化和重试流程。

## State and persistence
配置在关卡初始化时读取。存档schema无变更。

## UI, prefab, scene, and Inspector impact
Resources键为Levels/PresentationCatalog。第13–95关补充queueFrontY=0.315，保留场景原先第一排中心，启用queueRowPitch。其他关卡的第一排沿用0.274。JSON修改，无新增Inspector绑定。

## Performance and lifecycle
布局沿用已有初始化和Generation更新，不增加逐帧查找或分配。配置继续使用已有数值边界校验。

## Acceptance criteria
- [x] 全119关配置queueRowPitch=0.070、queueColumnPitch=0.24，且第一排中心保持原值。
- [x] 原型、光照、槽位等其他关卡字段与修改前逐字段一致。
- [x] 全119关实际场景中三列、三行中心符合配置，点击区域无相互重叠，重试后排列一致。

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| Exact configuration | All 119 entries, changed-field allowlist | Passed. 83 row-pitch entries and 88 column-pitch entries updated. First-row centers verified. |
| Unity | ThreeColumnQueueLayoutTests expanded to all119 with vertical checks | 2/2 passed in Unity 2022.3.62f3c1. All119 loaded, row/column centers and non-overlapping targets checked, retry checked on level1. |
| Catalog validation | Each level loads through existing LevelOneAssets catalog validation | Passed through PlayMode loading |
| Conventions | Foundation validator and git diff check | Passed with 0 warnings |

## Definition of Done
验收通过。PlayMode运行63.89秒，结果记录于工作区work/uniform-queue-spacing/play.xml。验证在隔离Unity项目中执行，设备触屏体验另行实测。
