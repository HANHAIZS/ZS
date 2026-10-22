---
id: voxel-depth-presentation
title: 前六关与106关的方块间隙与柔和深度
status: Verified
owner: Project
created: 2026-10-21
updated: 2026-10-21
---

# 前六关与106关的方块间隙与柔和深度

## Goal
仅修改1–6、106关的模型表现：7%间隙、柔和灯光、复用第7关采集缺口阴影。用户补充方块已经够小，采用模型整体放大4%补偿；单块最终视觉尺寸约为原来的99.2%。

## User-visible behavior
- 保持体素在模型内的中心坐标、源网格、调色板、数量和玩法不变。原网格宽度为中心间距的97.5%，实例局部缩放乘以0.93/0.975，形成7%间隙。
- 模型根节点原0.86倍缩放乘1.04；仍以原视口位置居中。整体放大是用户后续授权的修正，世界中心会随整体放大变化，模型内排列不动。
- 降低目标关卡Flat环境光至(0.48,0.52,0.63)，主光保留1.5强度，启用强度0.3的软阴影，保留明亮主色。
- 接入既有第7关局部缺口阴影（0.12秒过渡）；2、5、6、106的原描边宽度/颜色保留。独立货块保留缩放/描边并去掉空间阴影。
- 暂停冻结、重开复用缓存、切关销毁运行时材质/纹理。第7关效果与其他关卡视觉保持原样。

## Out of scope
关卡JSON、源Prefab/网格/原材质、队列、额度、经济、音效、UI、Foundation、其他关卡、真机打包、GitHub推送、截图录屏。

## Ownership and architecture
| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | None | 既有Commands | 不改变游戏规则 |
| Observable or persistent state | None | HarvestModel | 只读 |
| Shared project rules | None | HarvestSystem | 不改 |
| One-shot notification | None | 既有事件 | 不新增 |
| Unity presentation | ViewController | HarvestBoardView、VoxelCavityVisuals、LevelOneController | 缩放/阴影/生命周期 |
| Adapter or repository | Utility | LevelOneAssets | 逐关视觉参数 |
| Reusable infrastructure | None | None | 不改Foundation |

## State and persistence
- Runtime state: 每原色一份阴影材质、每模型一张局部纹理，复用既有缓存。
- Save schema/version/migration: None。
- State mutation path: 既有Commands，显示层只读取Stage/Generation。

## UI, prefab, scene, and Inspector impact
- Affected assets: Level01–06、Level106场景环境光与KeyLight；LevelOnePipeline仅打开soft-shadow支持能力，其他场景灯光仍不投影；Editor构建工具保留目标场景设置。
- Loading mode and final paths: 复用Assets/Resources/Materials/Level07VoxelCavity.mat，Resources键Materials/Level07VoxelCavity；Shader位于Assets/Sprites/Gameplay/Shaders/VoxelCavity.shader。
- Resources keys or direct reference owners: VoxelCavityVisuals。
- Load/cache/instance cleanup responsibility: Board销毁时先释放cavity再释放edge材质，先还原后销毁。
- Hierarchy or binding changes: 无。
- Inspector confirmation: 原生API保存并读回灯光/管线，运行时核对中心/间隙/材质。

## Integrations and failure behavior
- SDK/consent/lifecycle: 无。
- Content update/offline/rollback: 既有Resources，离线运行；Git保留可回退改动。
- Cancellation, timeout, retry: 暂停冻结过渡，重开复用缓存，切关释放；货块不采样模型阴影。

## Performance considerations
复用第7关邻域缓存，每次取块最多重算27格、过渡期间每帧最多一次纹理上传，不添加逐方块材质/属性块。目标关卡新增主光软阴影GPU成本，未测试移动真机帧率。

## Acceptance criteria
- [x] 仅目标7关实例间隙为6–8%且模型内中心不动，模型放大4%，源内容不变。
- [x] 原描边保留，原位置居中；第7关与其他关卡不受影响。
- [x] 柔和灯光配置可读回，真实GPU验证缺口/接触边及旋转保持。
- [x] 实际取块、独立货块、暂停/重开/切关生命周期正确。
- [x] 编译、规范、针对性测试通过。

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed；0 warnings |
| Unity compilation | 无新增C#/Shader错误 | Passed；控制台0 errors，Shader messages空 |
| Focused tests | 中心/间隙/范围、GPU局部阴影、货物/生命周期 | Passed；16个EditMode与39个不同PlayMode检查 |
| Runtime or asset behavior | 原生参数读回与源文件哈希 | Passed；275份基线文件未改 |
| Performance | 共享材质/纹理与局部更新；真机未测 | Passed；缓存复用/清理检查，真机限制保留 |

## Definition of Done
- [x] Ownership and QFramework roles match implementation.
- [x] Acceptance criteria and recovery paths pass.
- [x] Scene/pipeline changes saved by native APIs and read back.
- [x] Convention validation and compilation pass.
- [x] Focused tests and exact behavior checks pass.
- [x] Performance limits documented.
- [x] Verified only after evidence exists.

## Decision log
- 2026-10-21: 用户指定1–6与106，采用7%间隙，保留描边；随后明确允许模型略微放大，选择4%补偿方块缩小。
- 复用工作区中已完成但未提交的第7关cavity实现及测试，保留其两参数构造函数和Resources键，不改变第7关显示参数。

## Verification evidence — 2026-10-21
- RED：PlayMode任务3a0077a70d044652bb07fb520d220cd2，新检查按预期在模型尚未放大时失败。
- EditMode任务ed4e8eb863aa495487d8c30345a28d68：16/16通过，覆盖cavity全关卡范围、资源Shader与关卡源内容规则。
- PlayMode任务0d7eda58b29a4e48999ba0325fa5210f：8/8通过（第7关GPU缺口/接触边/旋转/暂停/重开/清理，6与106描边/真实运输，全部目标关间隙）。
- PlayMode任务a2b2d095fe5149eda7c9224bb23dcb1f：32项回归中31项通过，唯一失败是旧FixedGrip测试仍要求显示材质与货块材质相同。按空间阴影不应跟随货物的既定契约修正该断言，保留原色、姿态、尺寸、握持、运动与腿部接触检查。
- 最终PlayMode任务0bb3d920bc2d4440b0214ee9b7c348bb：上述握持全过程、新增目标关中心/尺寸/间隙/居中/画面边界与真实游戏cavity Shader软阴影3/3通过。以上不同PlayMode检查合计39项最终通过，无剩余失败。
- 真实cavity Shader软投影开关对比：334个像素亮度下降超过0.01，最大下降0.1011764，符合低强度柔和投影；未保存截图。
- 目标模型视口AABB范围：L1/3/4为(0.32,0.50)–(0.68,0.82)，L2为(0.29,0.53)–(0.71,0.79)，L5为(0.25,0.58)–(0.75,0.74)，L6/106为(0.34,0.49)–(0.66,0.83)。均保留原居中位置和画面余量。
- 275份关卡JSON/源Prefab/第7与8关场景文件SHA256与工作前一致；Git差异仅目标7场景的环境色和阴影参数，管线仅soft-shadow能力开关。其他场景没有启用投影的灯光，视觉不会因该能力开关变化。
- 原生API最终读回Level106：ambient=(0.48,0.52,0.63)，KeyLight=Soft，shaderSupported=true，shaderMessages=[]，scene dirty=false；C#控制台0 errors。未截图录屏，编辑器停在第106关。
- 独立只读代码复核未发现Critical/Important问题；源码/Shader/文档/场景/管线的git diff --cached --check及Foundation验证通过（Unity自动生成的.mat与.meta空字段保留原生尾随空格）。曾出现Unity自动序列化导致的无关HivePalette11旧_Color尾数变化，已用SerializedObject原生API恢复，不纳入改动。
- 所有修改保存在本地Git，未推送GitHub。未进行iPhone真机GPU性能或打包测试。

## 后续扩展 — 2026-10-22
用户要求新增107–111关完整沿用106效果，包括4%整体放大、7%间隙、浅黑边与柔和/局部阴影。原1–106关资产保持，范围扩展以[levels-one-hundred-seven-eleven.md](levels-one-hundred-seven-eleven.md)为准。
