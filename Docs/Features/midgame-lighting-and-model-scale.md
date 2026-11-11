---
id: midgame-lighting-and-model-scale
title: 第8–96关统一柔和光照并放大第13–95关模型
status: Verified
owner: Project
created: 2026-11-11
updated: 2026-11-11
---

# 第8–96关统一柔和光照并放大第13–95关模型

## Goal
第8–96关环境光、主灯软阴影和采集缺口阴影采用第1–6关参数。第13–95关采用放大版显示比例，旋转全过程保持在顶部与槽位之间。

## User-visible behavior
环境光RGB为0.48/0.52/0.63，主灯软阴影强度0.30，shadowBias/NormalBias为0.04/0.20，cavity=true。第13–95关以约33%的放大上限逐模型适配，依据连续360度绕Y轴旋转边界缩小倍率，按9:16竖屏保留画面边缘和槽位间的余量，最终倍率为1.12–1.33。

## Out of scope
蜂巢队列、槽位配置、模型方块数与材质底色、方块间隙、机关、搬运和出巢时序各自沿用现有配置。

## Ownership and architecture
LevelOneAssets解析并缓存Levels/PresentationCatalog。LevelPresentationView应用主灯和环境光，HarvestBoardView应用缩放并接入VoxelCavityVisuals，局部阴影使用现有Materials/Level07VoxelCavity材质和BeeHarvest/VoxelCavity Shader。

第9–12关已有表面/法线贴图由VoxelCavityVisuals复制到阴影实例，VoxelCavity.shader通过可选贴图变体显示纹理。取下后的货物仍由原始材质显示。

第13–95关由RotatingModelViewportFit缓存连续旋转包络和每个方块的轨道。屏幕变窄或第六槽启用时，按实际可用区域收敛显示尺寸，恢复五槽或较宽屏幕时回到允许的倍率。仅在屏幕比例、相机正交尺寸、槽数或槽位实际矩形变化时重新拟合。

## State and persistence
表现配置经关卡初始化与重试使用。存档schema无变更。

## UI, prefab, scene, and Inspector impact
配置资产为Assets/Resources/Levels/PresentationCatalog.json。资源继续按已有Resources键加载。模型尺寸由既有modelScaleMultiplier控制。原始模型、场景与Prefab由现有引用加载。

## Lifecycle and performance
缩放在初始化及上述布局输入变化时应用。每帧只比较缓存输入，无逐帧几何遍历或分配。软阴影增加GPU渲染成本，缺口阴影沿用每模型共享纹理、按邻域更新和切关销毁逻辑。手机GPU性能待设备实测。

## Acceptance criteria
- [x] 第8–96关光照和阴影配置与第1关一致，运行时可读回。
- [x] 第13–95关模型放大，连续旋转边界位于横向0.05–0.95、顶部0.85以下、五个常规槽位上缘加余量以上，且避开启用的第六槽位。覆盖9:16和20:9竖屏。
- [x] 第13–95关逐关验证，以及第7/8/12/13/95/96/97关边界验证通过。
- [x] 配置差异限定在授权范围的光照、阴影和模型倍率字段。
- [x] Unity编译、针对性测试和项目约定检查通过。

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| Exact configuration | Before/after field allowlist and ranges | Passed. 88 changed lighting records, 96 already matched; 83 scale records, target1.12–1.33. |
| Geometry | Actual Unity meshes, continuous yaw,9:16/20:9,5/6slots,retry,zoom and moved slot | ExpandedMidgamePresentationTests3/3 passed, including89 lighting profiles and83 model layouts. |
| Rendering | Runtime light settings, cavity shader, existing cavity GPU test | Final focused8/8 PlayMode passed. Cavity texture pixels8887, normal pixels9305, soft-shadow pixels334. |
| Conventions | Foundation validation and git diff check | Passed,0 warnings.20/20 focused EditMode tests passed. |

## Definition of Done
验收通过。Unity2022.3.62f3c1在隔离副本验证。最终证据在工作区work/midgame-presentation的play-layout-final.xml、play-shader-final.xml和edit-final.xml。只读复核的宽屏比/第六槽位及布局缓存问题已修正。手机真机GPU性能未实测。
