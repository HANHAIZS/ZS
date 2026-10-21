---
id: chick-voxel-edges
title: 第6关小鸡方块黑色棱边
status: Verified
owner: Project
created: 2026-10-21
updated: 2026-10-21
---

## Goal
用户要求第6关小鸡组成方块的长宽高加一圈黑色描边。将每个方块的立体棱边画成细黑线，保留面部原色；旋转时按真实几何显示可见边，不画三角形对角线或透视背面线。只对Chick（当前第6关）启用，其他94关不变。

## User-visible behavior
第6关小鸡每个方块具有细黑色棱边，保留原色面；模型正面和旋转角度的可见立体边清晰，搬运复制源材质，隐藏/重开/暂停沿原流程。

## Out of scope
其他94关、JSON/网格/颜色数据/数量/队列/盲盒/导航/蜂巢UI/音频/存档/Framework与移动设备打包。

## Ownership and architecture
- 项目ViewController纯显示：HarvestBoardView初始化时为Chick绑定VoxelEdgeVisuals，缓存每种原调色板材质的一份显示材质，使用原Mesh和Transform。源预制体和共享素材不修改。
- 新Shader保留URP PBR面着色/颜色/金属度/光滑度，在表面根据本地包围盒的两个最近边界求12条棱边并抗锯齿混合黑色；同一Forward pass完成，深度/阴影沿用立方体。无逐方块LineRenderer、子物体或新增网格。
- 共享运行材质保存原Mesh bounds（所有Chick立方体共用相同bounds，不新增逐Renderer属性块），既有CopyPickupVisual完整复制材质/属性，因此蜜蜂搬运仍使用同一描边，采走隐藏与重开恢复不变。
- 复用原Model/System/Command/事件/经济/存档，不修改关卡JSON、几何、870方块数量、采集顺序、方块色值、自动旋转或盲盒。

## Resources and lifecycle
- 新Shader：Assets/Sprites/Gameplay/Shaders/VoxelEdges.shader；单个模板材质Assets/Resources/Materials/ChickVoxelOutline.mat，Resources键Materials/ChickVoxelOutline，确保构建包含Shader。
- VoxelEdgeVisuals位于Assets/Scripts/Game/ViewControllers/Harvest，初始化加载一次模板、缓存4种原色运行材质；每个方块继续使用既有Renderer。Scene/Prefab/Inspector不变，原材质不动；随Board销毁释放运行材质。无新增Update/协程/逐帧架构查找或实例化。

## Failure and recovery
缺失模板/Shader应初始化明确报错；仅初始化Chick触发。采集隐藏/搬运/交付/暂停/旋转/重开沿既有生命周期，重开不重复分配材质，离开释放本地材质。新材质不进入存档。

## Acceptance criteria
- [x] 第6关全部870方块有细黑立体棱边，原色面保持，不出现三角对角线/背面透视线。
- [x] 旋转、源方块隐藏和搬运拷贝正确，重开不叠加资源、退出释放。
- [x] 原Chick共享Mesh/材质、其余94关、美术/场景/Prefab/数据均保持。
- [x] 实际GPU方块渲染与Native模型截图、针对性运行测试、编译/Shader错误/Console、范围哈希及项目规范验证通过。

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| GPU | 原870块同源网格、单方块GPU黑边/亮面/无对角线采样、native正面及35度截图 | Passed |
| Lifecycle | 重开不新增材质、退出释放4份运行材质、Level08仍原URPLit、CopyPickupVisual真实GPU渲染 | Passed |
| Regression | 蜜蜂搬运/源隐藏、盲盒、等待跃动 | Passed |
| Scope | 全Assets哈希，95关配置/原材质/模型/Scene/Prefab不变 | Passed |
| Project | C#编译、Shader消息、Console、Foundation validator | Passed |

## Definition of Done
- [x] 需求由项目显示层实现，无Model/System/Command/存档改动。
- [x] 实际棱边和原色经GPU与原生画面核对，资源生命周期与范围通过。
- [x] 编译、相关运行测试、Shader错误、项目规范检查有证据。
- [x] 无Inspector待填项；设备性能/构建不在本次验收内。

## Verification
- Red:85d93d47673e4c27a0484e74a711ee50，2/2按预期失败：原URPLit无描边、实际GPU边界黑像素为0。
- 初次实现07731220c1094df1b4d5307022d957cb仍有两项失败：Color严格浮点比较差异，改为1e-5容差；GPU缺黑边则是真实圆角阈值问题。随后8942f...1/2通过，保存诊断GPU图并读回原Mesh34顶点，确认外界限±.4875/圆角起点±.4375，原_EdgeWidth=.045小于圆角边最低约.0513，因此未触及棱边。调整为.11覆盖圆角棱线并保留大部分原色面。
- 最终相关PlayMode adc732e10b044585b5a23ba993f7bb95，19/19通过：ChickVoxelEdgeTests2、BeeTransportReadabilityTests7、MysteryHiveTests4、WaitingHiveBounceTests6。覆盖真实GPU边/面/对角线、源870网格与原色、4份材质复用/重开/退出、Level08不变及原搬运/盲盒/等待跳跃路径。
- 复核指出测试诊断File.WriteAllBytes含本机绝对路径；已删除该诊断输出。最终新测试复跑3eff8cd4c3784e13808976c0f8eb4007成功，completed2/2、无失败（MCP此次result为空，以progress和succeeded状态为证）。未重复全EditMode和全191项PlayMode，不作全量通过声明。
- Native1080x1920：小鸡黑色棱边-正面.png、小鸡黑色棱边-立体.png、原小鸡-立体.png。实际完整模型的正面/35度可见边连续，黑线围住各原色面，没有三角对角线或背面穿透。对照仅临时换回原材质，退出预览后销毁。
- 所有870块共用原同一Mesh，原4种无贴图URPLit材质metallic0/smoothness.4保留。新Shader使用同URP PBR，边缘黑色在着色后混合；本项目Forward、SRPBatcher开、动态批处理关。每块不增加Renderer/Mesh/独立材质或可见Forward pass，4色共享运行材质，未添加新逐Renderer属性块。
- 最终ShaderUtil无消息、isSupported=true；C#编译和Console0errors。Scope逐文件核对：现有Assets仅HarvestBoardView.cs变化；新增Shader/模板材质/显示helper/测试及meta。95份JSON、原Chick prefab/mesh/material、其他脚本/UI/场景/音频均字节相同。既有测试归一化HivePalette11.mat已由原生FileUtil恢复。
- 预览恢复原经济/偏好隔离、停止Play并返回MainMenu；运行材质离开释放，永久模板仅Resources引用。独立复核生产无重要问题，测试绝对路径问题已修正。
- 初次validator只报新增Spec缺少标准小节标题，已补齐正确标题与矩阵；最终validator Passed、0warnings。证据：工作区outputs/chick-edges/{focused,red,scope,final-console}.json及原生截图。


## Limits
Editor验证，不声明iPhone GPU性能或打包验收。新增4种共享运行材质及每像素简单边缘计算，不新增逐方块Draw pass或对象。


## 2026-10-21 描边范围扩展
后续用户要求新增10关同样描边，范围通过LevelOneAssets.HasVoxelEdges集中为第6关及第96–105关；其余旧关仍不启用。复用本Shader/模板/资源生命周期，详见levels-ninety-six-one-hundred-five.md。原本节验证记录为当时仅Chick的验收，新增范围以新Spec为准。
