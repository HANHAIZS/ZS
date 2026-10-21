---
id: level-seven-voxel-cavity
title: 第七关小羊的采集缺口局部阴影
status: Verified
owner: Project
created: 2026-10-21
updated: 2026-10-21
---

# 第七关小羊的采集缺口局部阴影

## Goal
用户已确认局部体素阴影方案，并要求只修改第七关观看效果。让第7关1030块小羊在采走方块后露出有深度的内壁、底面和柔和接触阴影。

## User-visible behavior
- 只在第7关启用。保留原模型网格、颜色、数目、面形、材质亮度基准和采集/配额/蜂运动规则；不额外添加描边。
- 同色块移走后，凹坑底面轻微变暗，贴壁/交角处阴影更明显；面中心保留原色可辨识。
- 缺口扩大、遮挡减少后阴影自然减弱；局部变化约0.12秒平滑过渡。
- 原块隐藏与货物交接沿既有流程；搬离模型的货块使用原始材质，避免受模型局部阴影污染。
- 手动旋转时阴影贴合几何；暂停冻结过渡；重开恢复基础遮蔽并复用缓存；离开清理运行时纹理/材质。

## Out of scope
其他105关、源模型/JSON/场景布局、数量/颜色/经济/队列/音效、蜂运动、Foundation包、全屏SSAO、新灯光/几何和iPhone打包。

## Ownership and architecture
| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | None | 既有Commands | 不增加玩法状态 |
| Observable or persistent state | None | 既有HarvestModel | 只读取Stage/Generation |
| Shared project rules | None | 既有HarvestSystem | 可采判定保持原样 |
| One-shot notification | None | 既有取块/Board.Sync | 使用既有可见性同步 |
| Unity presentation | ViewController | ViewControllers/Harvest/VoxelCavityVisuals.cs、HarvestBoardView、LevelOneController | 网格邻居遮蔽缓存、材质与纹理生命周期 |
| Adapter or repository | Utility | Utilities/Harvest/LevelOneAssets.cs | 第7关能力开关 |
| Reusable infrastructure | None | None | 不改Foundation |

## State and persistence
- Runtime state: 体素占据表、每面四角遮蔽缓存、小型数据纹理、脏邻域与短过渡；均由View拥有。
- Save schema/version/migration: None。
- State mutation path: 既有Command移除体素，Board读取Stage后刷新显示与局部邻域；不反写模型。

## UI, prefab, scene, and Inspector impact
- Affected assets: 新增Assets/Sprites/Gameplay/Shaders/VoxelCavity.shader；Assets/Resources/Materials/Level07VoxelCavity.mat由Unity原生API创建。
- Loading mode and final paths: 模板通过Resources键Materials/Level07VoxelCavity加载；材质引用新Shader，确保构建包含。
- Resources keys or direct reference owners: Level07运行时VoxelCavityVisuals持有。
- Load/cache/instance cleanup responsibility: 每原色一份共享显示材质及一张小型线性RGBA数据纹理；销毁View统一释放；不新增逐方块材质或属性块。
- Hierarchy or binding changes: 无源Prefab、场景、Inspector字段变动。
- Inspector confirmation: 读回第7关Shader/材质数/原网格/原色；核对第6、8关材质不变。

## Integrations and failure behavior
- SDK/consent/lifecycle: 无新集成，保持离线。
- Content update/offline/rollback: 现有Resources缓存。旧文件备份在工作区work/level07-cavity/before。
- Cancellation, timeout, retry: 重开重建占据/遮蔽，暂停冻结过渡，切关释放缓存。摄像机/模型旋转只更新共享矩阵。

## Performance considerations
占据与三维网格初始化一次；只在移除时重算局部一圈并合并重复脏格。每片元一次缓存数据查询和简单颜色插值，非邻居搜索。共享4种调色板/1张纹理，保持单Forward pass与SRP Batcher兼容，不用逐Renderer属性块。纹理Apply会上传整张小纹理，变化/短过渡期间每帧最多一次；静止时不上传。第7关缓存为384×49 RGBA32，共75,264字节；GPU测试和运行时检查确认局部更新规模，不推断iPhone帧率。

## Acceptance criteria
- [x] 第7关凹坑内壁/底面/接触边在真实GPU画面中可辨，颜色和真实几何保留。
- [x] 单格移除与扩大开口的阴影符合邻居关系；远处外表面不被整块压暗。
- [x] 实际采集、旋转、暂停、重开与切关清理正常，货块不携带空间阴影。
- [x] 仅第7关启用；第6/8与其他关卡范围验证，源文件哈希不变。
- [x] 编译、相关回归、规范验证及原生预览有证据。

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed |
| Unity compilation | Shader/C#无新错误 | Passed |
| Focused tests | 真实小羊/邻域阴影/GPU像素对比/范围与生命周期 | Passed |
| Runtime or asset behavior | 真实取块、原材质货物、暂停/重开/切关、GPU取样与第7关场景读回 | Passed；用户明确自行查看，不截屏或录制 |
| Performance | 4份共享材质、1张75,264字节纹理；同色测试为1材质，无逐Renderer属性块；单次移除最多27个格重算 | Passed；iPhone未实测 |

## Definition of Done
- [x] Ownership and QFramework roles match implementation.
- [x] Acceptance criteria and failure/recovery behavior pass.
- [x] No source asset/scene/Inspector migration required.
- [x] Convention validation and compilation pass.
- [x] Tests and native visual evidence pass.
- [x] Performance and platform limits documented.
- [x] Verified status only after evidence exists.

## Decision log
- 2026-10-21: 用户明确只在第7关试做。第7关原为URP/Lit且无描边，使用独立Shader/Resources模板，仅新增凹坑明暗，不扩展旧描边范围或开启全屏效果。

## Verification evidence — 2026-10-21
- 原生MCP PlayMode任务94591fbfc93a4960894f289cc2ab7fb3：4/4通过；EditMode任务90df3cc5246244e1b4082d91398b5139：2/2通过。RED阶段两组均因缺失功能按预期失败。
- GPU真实渲染检查：同色凹坑中心、接触边缘、外表面保持、扩大开口释放遮蔽均通过。相同姿态开关阴影的亮度比例为0.8952243；旋转35度后0.8954974。
- 原小羊1030个Mesh与4种BaseColor不变；第6关旧描边、第8关URP/Lit及关卡范围、切关释放通过。
- Level07.json、Sheep.prefab与Level07.unity逐字节未改。第7关已在编辑器打开，最后读回scene dirty=false；C#编译控制台0错误，Shader编译消息0条。
- Foundation验证0 warnings；独立只读代码复核未发现Important/Critical问题。
- 用户要求不再截图，由其自行观看。未运行iPhone真机性能或打包测试。

## 后续范围更新 — 2026-10-21
用户随后要求将相同局部阴影扩展到1–6和106关，并为这些关卡增加7%间隙、轻微整体放大和柔和灯光。以[voxel-depth-presentation.md](voxel-depth-presentation.md)为当前范围；本文件上方仅描述第7关首次交付。第7关几何、缩放与灯光保持不变。
