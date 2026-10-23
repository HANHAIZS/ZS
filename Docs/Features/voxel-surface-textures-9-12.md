---
id: voxel-surface-textures-9-12
title: 第9–12关接入原版方块表面贴图
status: Verified
owner: Project
created: 2026-10-23
updated: 2026-10-23
---

# 第9–12关接入原版方块表面贴图

## Goal
从用户指定Cube Land.app中定位并提取普通方块表面贴图，为9–12关增加柔和表面明暗和圆润边缘法线细节，保持原配色、模型和布局。

## User-visible behavior
- 使用原resources.assets内Mat_Base_Voxel实际引用的Tex_Base_Voxel(512²,ID255)和Tex_Normal_Voxel(512²,ID264)。不使用Icon或特殊冰冻/金属/木头材质。
- 四关每种原配色用独立共享材质副本，保留_BaseColor、_Metallic、_Smoothness及URP Lit。贴图UV缩放4×4、偏移(.5,0)适配既有四格十字展开；法线强度.45，保留所有顶点、UV和切线。
- 源方块被蜜蜂取走时材质和贴图一致；暂停、旋转、重试、跨关及盲盒机制照旧。

## Out of scope
不修改App源文件、关卡数据/目录/布局、光照/描边/阴影/几何/机关/存档，也不修改全局VoxelPalette或蜂巢/蜜蜂/其他关卡材质。不上传GitHub。

## Ownership and architecture
仅Editor资源准备：CubeLandVoxelTextureSetup配置贴图导入、建立共享材质副本并写入四个目标Prefab。CubeLandLevelSetup重建这些模型时复用同一材质选择，保持可重建性。无新增运行时组件、Command/Model/System/Event或依赖查找。

## State and persistence
无新增运行时状态或存档字段。生命周期继续由Resources Prefab及既有Board/BeeFlightView管理；不逐方块创建材质。

## UI, prefab, scene, and Inspector impact
- 原图最终路径Assets/Sprites/Gameplay/CubeLand/SurfaceTextures/Tex_Base_Voxel.png与Tex_Normal_Voxel.png；由Assets/Sprites/Gameplay/CubeLand/SurfaceMaterials内材质直接引用，不放Resources重复加载。
- 现有Broccoli、Cactustoy、Carrot、CatHead四Prefab引用新材质，Resources键Prefabs/Gameplay/<model>保持。目录确认这四个模型各仅用于目标关。
- 使用Unity PrefabUtility/AssetDatabase写资源，保留所有Hierarchy/Inspector/fileID/GUID和网格引用；无场景改动。保留用户场景和材质未提交内容。

## Integrations and failure behavior
无SDK、联网、热更变化。Editor准备缺失源素材/材质时报明确错误；幂等重建只更新目标材质，不保存其他用户脏资源。复制源图后运行不依赖本机App路径。

## Performance considerations
四关按配色复用少量持久Material，两张512²纹理启用mipmap/压缩/instancing；无新增逐帧代码、材质实例或网格。GPU额外正常纹理采样，需在现有场景验证。

## Acceptance criteria
- [x] 四关全部方块正确引用原版Base/Normal并保持原配色；范围外资源保持。
- [x] 四关实际取块/搬运/交付、重试和原盲盒/布局回归通过。
- [x] GPU内存渲染确认贴图和法线实际生效，不保存截图/录像。
- [x] 配置/编译/规范和只读审查通过，记录证据并本地提交。

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| Source | 原材质引用、两张原图及哈希 | Passed |
| Assets | 四Prefab材质、UV_ST、导入与颜色/网格 | Passed |
| Gameplay | 取块/交付/重试/盲盒/布局 | Passed |
| GPU | 内存中开启/关闭Base和Normal对照 | Passed |
| Scope | 原图/JSON/场景/其他Prefab与材质哈希 | Passed |
| Foundation | 规范、编译与审查 | Passed |

## Definition of Done
- [x] 所有验收通过；说明性能影响、无需绑定及真机验证限制；仅任务资源本地Git保存。

## Verification evidence (2026-10-23)
- App清点481个Texture2D；普通方块材质Mat_Base_Voxel(ID4)明确引用Tex_Base_Voxel(ID255)和Tex_Normal_Voxel(ID264)，两张均512×512。
- 初始EditMode27c9ac5651cc4afdba34dd8bd151eb91的2测试均按预期失败：第9关无BaseMap，贴图尚未导入；导入后2/2通过。
- 最终EditMode e2a82fa8257f40c4a0804af5e59f26f3：3/3通过，包括四模型全部材质配色/网格/导入设置、跨批次更新原配色和光滑度且保持材质资产GUID。
- PlayMode0d5ea5266b8e46d3bf627b77566347df中6项通过（GPU、7–12布局及四项盲盒回归）；手动禁用Controller的搬运测试在重试后未Sync视图，补齐既有手动驱动步骤后，0bbd6b70a1ee458ebf650cb50b22dcda逐关取块/搬运/交付/重试测试通过。共7个不同PlayMode测试通过，8/13关材质仍无贴图。
- GPU128²内存对照：BaseMap变化7010像素、最大RGB距离.1055914；Normal变化8433像素、最大距离.573944。没有保存截图/录像。
- 审查发现的Editor缓存跨批次旧配色问题先复现（7553dc311f5b449bb4aafefcd4d41b7c，颜色差.670820415），再修复：三个资源重建入口均先BeginBuild；回归以GUID校验持久资产身份，临时测试材质在finally删除。复审无阻塞项。
- 范围校验：894个既有关卡/Prefab/美术文件哈希保持；四个目标Prefab只替换材质GUID，反向替换后逐字节等于HEAD。全部119关玩法JSON和PresentationCatalog、原Voxel网格/UV/切线及调色材质不变；App源文件不变。
- 当前Editor导入为BC7、10级mipmap；Profiler.GetRuntimeMemorySizeLong两图合计1,050,000字节（约1.0MiB），四关共用13个持久材质。无新增运行时组件/逐帧分配；实际真机显存/帧率尚未测量。
- 规范0警告，C#/文档差异空白检查通过；Unity原生.mat/.meta保留序列化器生成的空字段尾空格。Unity无新编译/游戏错误；Console仅MCP WebSocket未初始化的工具连接提示。无需Inspector绑定，保留用户Game场景及5个材质未提交改动。

### Source checksums

- resources.assets SHA-256: `80ccfd63352bf267059f395d3c2b99fac34c14199721c67429ce0be8ea08dc9d`
- Tex_Base_Voxel.png SHA-256: `652f6e1f83454ae56115658ea62cf8012c7a98c85a8f2e5bb0e59dac4f7d96c5`
- Tex_Normal_Voxel.png SHA-256: `b33fee4fbcbf62dd30b64f2d40c2fe2d39442bcbbd025c48642f9a4a8637eabb`
