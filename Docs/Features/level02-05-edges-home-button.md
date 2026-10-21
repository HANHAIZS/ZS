---
id: level02-05-edges-home-button
title: 第2及5关黑色棱边与主界面关卡按钮字体
status: Verified
owner: Project
created: 2026-10-21
updated: 2026-10-21
---

## Goal
第2关Duck、第5关Watermelon组成方块采用现有第6关的长宽高黑色棱边；主界面绿色关卡按钮采用参考图白色圆润粗体和深绿色描边。用户最终澄清只改绿色关卡按钮，顶部数值和底部导航文字不改。

## User-visible behavior
- 第2、5关复用VoxelEdges，保留色面、全部方块/模型/队列/朝向、搬运与重开规则；原6及96–105关描边保持。
- 原level N文字、FredokaBoldSDF字体/48字号、按钮尺寸位置/交互保持；填色改白色、描边改深绿，使用专用材质避免影响共享字体。

## Out of scope
顶部数值、导航文字/造型/动效、商店、设置/弹窗、其他关卡、原始图片、模型数据、机制/存档/奖励/音频及Foundation。

## Ownership and architecture
LevelOneAssets.HasVoxelEdges仅扩展2、5显示开关；沿用HarvestBoardView与VoxelEdgeVisuals的共享材质缓存和退出释放。HomeArtworkSetup新增按钮专用样式方法，Build同步复现。无Command/Model/System/Event或状态写入变化。

## State and persistence
无存档/schema/玩法或经济变化；只使用既有显示参数。

## UI, prefab, scene, and Inspector impact
Assets/Resources/Prefabs/UI/MainMenuPage.prefab仅StartGameButton/Label文字颜色与材质。Resources键Prefabs/UI/MainMenuPage保持；新直接引用材质Assets/Sprites/UI/MainMenu/LevelButtonLabel.mat，复用原字体与图集，由已有MainMenuAssets加载页面、场景销毁释放实例。原Inspector绑定/层级/其他文字保持。第2/5关无需改Scene/Prefab/JSON。

## Integrations and failure behavior
本地资源复用、无新SDK或网络。字体缺失沿用原显式失败；新材质原生创建并引用。样式独立于NavigationLabel和共享字体，重建按钮保持样式。

## Performance considerations
无逐方块新对象/额外pass，按原色共享描边材质，原生命周期释放；仅一个共享TMP材质，无新Runtime Update/查找。

## Acceptance criteria
- [x] 2、5全部方块描黑、原mesh与色面相同，原描边关集合加2、5且其他关不变。
- [x] 关卡按钮白色圆润粗体/深绿描边，无截字；顶部数值/底部导航与其他文字保持原值。
- [x] 原生实际画面及读回通过，编译与规范校验通过，Git独立保存。

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Content/display | 开关全关读回、2/5全renderer材质/mesh/color、搬运视觉 | Passed |
| UI | Native prefab精确差异、运行画面与非目标文字对比 | Passed |
| Compilation/conventions | 编译无新错误、project validator | Passed |
| Focused tests | 小范围可逆显示配置不新增镜像测试，原生断言与截图验证 | Verified with native assertions |

## Definition of Done
- [x] 范围、所有权、资源/生命周期与用户最终要求一致。
- [x] 画面/精确值/编译/规范检查有证据，恢复预览隔离、独立Git提交。

## Decision log
2026-10-21：用户最终要求只将参考字体应用到主界面绿色关卡按钮；顶部数值/导航排除。


## Verification results
- 原生范围读回：HasVoxelEdges仅[2,5,6,96..105]，旧关开关保持。
- Play中第2关2190/2190方块采用VoxelEdges，3个共享颜色材质；第5关2840/2840方块采用VoxelEdges，5个共享颜色材质。逐Renderer比较原Prefab mesh相同、BaseColor差异低于0.00001；原Prefab材质未更换；CopyPickupVisual复制的搬运方块同样描黑。未宣称完整通关回放或真机性能测试。
- MainMenuPage全部46处TMP逐字段比较，仅Content/SafeArea/StartGameButton/Label的color与material变化；顶部数值、底部导航/商店/设置等45处文字及所有文字布局/字体/字号保持。Prefab局部diff仅这两项。独立按钮材质#006A0B描边、0.2宽、白色填充；FredokaBoldSDF/48，原level N文字保持。
- 真实运行Home两种分辨率1080×1920/1080×2340目视检查，按钮文字不截断；第2/5关原生1080×1920画面已目视检查，三向黑边与原色可见。Home-audit.truncated=false。原生Prefab规范Valid=True。
- HomeArtworkSetup.StyleLevelButton连续执行两次保持单一独立材质，并接入Build重建路径；未运行会重建无关UI的全量菜单生成器。
- Unity脚本编译完成，最后Console 0错误/警告；project validator Passed、0warnings。没有新增单元测试：此次为复用已验证渲染器的显示开关及可逆TMP样式，以逐方块/非目标文字断言与实际画面作为相关验证。
- 第一次预览碰到域重载期间MCP断线，未执行经济隔离；停止后重试，等待重连成功。最终原经济/设置隔离和Editor重载选项已恢复，停止Play并回到MainMenu。SaveAssets产生的5份Palette浮点舍入噪声通过Native FileUtil按Git HEAD原字节恢复，不纳入功能修改。
- 证据目录：[运行读回与画面](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level02-05-edges-home-button)。
- [第2关](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level02-05-edges-home-button/level2.png)、[第5关](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level02-05-edges-home-button/level5.png)、[主界面](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level02-05-edges-home-button/home-1920.png)。

## Final implemented files
LevelOneAssets.cs显示开关；HomeArtworkSetup.cs按钮样式/构建；MainMenuPage.prefab仅标签颜色与材质；LevelButtonLabel.mat及meta；本Spec。未新增Runtime组件、Scene/JSON/模型修改、Shader修改或Inspector待办。


## 2026-10-21 关卡按钮首字母大写
用户要求主界面按钮level 1首字母改为L；将所有动态关号统一显示为Level N，同步Editor默认值与Prefab。仅文案大小写改变，样式/布局/玩法和其他文字保持。不新增单元测试，现有4份Runtime测试的相关预期同步为Level N，以原生运行读回与编译/规范检查验证。原字体图集缺少大写L，使用Unity TMP原生TryAddCharacters仅补L后恢复Static；字符49→50、图集仍2张、既有glyphRect全保持，按钮颜色/字号/材质不变。验证完成：真实Home运行读回Level 1、truncated=false、FredokaBoldSDF/48/白色/原深绿材质保持；1080×1920原生截图已目视检查大写L正常显示。Unity编译无CS错误、project validator通过0警告；相关既有测试预期已更新但本次未重跑测试套件。预览隔离/Editor设置已恢复、停止Play回MainMenu。证据[主界面](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/capital-home-button/home-1920.png)。


## 2026-10-21 第2关描边调浅
用户要求只把第2关组成方块的黑色描边调浅，颜色过深。第2关使用中性深灰色(Shader RGB 0.2)，保持_EdgeWidth=0.11、原色面/mesh/数量/布局/玩法。第5、6及96–105关仍纯黑边，其他关无描边；主界面字体不变。

Shader增加_EdgeColor，默认黑色；LevelOneAssets提供第2关显示颜色，经LevelOneController与HarvestBoardView传给VoxelEdgeVisuals；按原色共享的运行材质写入颜色，搬运继续复用同一材质。默认黑色的lerp公式等价于原color*face。不修改原材质资产/Prefab/Scene/JSON、不新增资源或每帧查找/分配。只做本地提交，不推送GitHub。

验收：原生第2关画面中描边更浅、粗细不变；第5/6/96关读回仍黑色；所有2190块mesh/色面保留、搬运复制保留灰边；Shader编译无错误、相关原有GPU黑边测试、规范校验通过。此次低影响视觉参数不新增镜像单元测试，原生断言与实际渲染验证参数作用域。验证完成：

- 原有ChickVoxelEdgeTests PlayMode作业e4f515af859646c5857e38c4db8a569b，两项2/2通过：真实GPU黑边/无三角对角线、原mesh/色面/共享材质/重开/释放。修改仅第2关专用颜色后无需重复默认黑色测试。
- 最终第2关2190/2190方块使用RGB(0.2,0.2,0.2)灰边，宽度仍0.11、3份按原色共享的材质；逐方块mesh/BaseColor和原Prefab一致，搬运CopyPickupVisual继承灰边。
- 第5关2840、第6关870、第96关1291块均逐Renderer确认_EdgeColor黑色、宽0.11、原mesh/色面不变；最终全105关配置读回只有第2关有颜色覆盖。
- 首次RGB0.06预览对比显示调浅幅度偏小，最后调整到0.2；最终1080×1920原生截图已目视检查灰色棱边更柔和。[第2关效果](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level02-soft-edges/level2.png)。原始黑色对照保留在outputs/level02-05-edges-home-button/level2.png。
- Shader支持正常、ShaderUtil编译消息0条；最终Console错误/警告0条，含base HEAD的规范校验通过0警告。游戏Scene/Prefab/JSON/源模型/材质资产/UI/Packages无修改；仅4份C#、VoxelEdges.shader和本Spec。
- 文件职责：Utilities/Harvest/LevelOneAssets.cs给出按关显示参数；ViewControllers/Harvest/LevelOneController.cs传参、HarvestBoardView.cs初始化显示、VoxelEdgeVisuals.cs复用材质写入颜色；Shaders/VoxelEdges.shader保持黑色默认并混合边线色。
- 无新增资源/Inspector绑定、每帧查找或实例化，仍按颜色共享运行时材质并由原Dispose清理；无手机真机性能结论。
- 预览经济/设置隔离与Editor重载设置均恢复，停止Play回MainMenu。本次仅本地Git提交，不推送远端。
