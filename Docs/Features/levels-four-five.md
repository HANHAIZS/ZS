---
id: levels-four-five
title: 蜜蜂第四关与西瓜第五关
status: Verified
owner: Project
created: 2026-10-18
updated: 2026-10-18
---

# 蜜蜂第四关与西瓜第五关

## Goal
用户确认使用资料包Bee制作第4关、Watermelon制作第5关。沿用当前玩法和新UI，接在香蕉之后，末关胜利返回Home。

## User-visible behavior
- 第4关蜜蜂1105块、15蜂巢、3列；第5关西瓜2840块、51蜂巢、3列。均5工作槽、3排等待区。
- 保留西瓜4个连接组：1/20、6/24/39、12/30/45、21/37。沿用整组列首、各占一槽、容量不足整组不动；暂停/后台/重开取消整组动画。
- 香蕉→蜜蜂→西瓜顺序递进；Continue重复点击不重复加载。第五关后返回Home，下一轮从花朵第一关开始。
- Home显示并恢复level4/5的同关进度，Retry重置本关并按现有体力/库存规则执行。采集、出巢路线、固定抱块、交付及现有UI保持。
- 新模型默认朝向经真实画面确认，保留源块坐标、色号、额度、队列和原64位身份。

## Out of scope
第六关、其他785对象、原三关内容修改、UI重设计、新机关、经济/道具规则修改、存档解锁、Foundation修改。

## Ownership and architecture
- 现有Commands和MenuNavigationModel/HarvestModel/HarvestSystem继续拥有状态与规则；不新增调度或存档。
- LevelOneAssets扩展5关模型映射与缓存；CubeLandPalette增加源色2(#383838)、6(#08C858)，取已完成783原始图标采样的代表色。
- Editor将已有LevelThreeSetup的通用生成过程提取到SourceLevelSceneSetup，保留原菜单入口；新的两关复用该生成器、当前场景绑定和UI。CubeLandLevelSetup.ApplyLevel按需生成，避免重写原三关模型。
- 离线导入脚本扩展到5个显式对象，转换算法不变；QFramework注册入口、运行View和Command实现不变。

## State and persistence
关号仍为会话状态，无存档schema或迁移。重开、后台、Home/Start和库存保留沿用现有Commands、代次检查与输入门限。测试经济使用隔离内存存储。

## UI, prefab, scene, and Inspector impact
- 新Assets/Scenes/Level04.unity、Level05.unity，BuildSettings包含MainMenu和5关。
- 新Assets/Resources/Levels/Level04.json、Level05.json，键Levels/Level04、Levels/Level05；新Prefabs/Gameplay/BeeModel、Watermelon模型预制体。
- Assets/LevelSources/CubeLand/Bee.json、Watermelon.json保留完整来源，仅用于导入审计。
- 大蜜蜂模型独立使用 BeeModel 键，完整保留旧飞行蜜蜂 Bee.prefab 与 GUID。源色2、6的新BeePaletteXX/HivePaletteXX预制体按既有键缓存，方块/蜜蜂/巢共享材质在Assets/Sprites/Gameplay/CubeLand。方块复用Voxel.asset网格。
- 新场景原生复制既有场景，设置关号/标题；Coiny字体补4/5字形。复用现有紫金底栏、绿按钮、六边形槽位、暂停、胜利和菜单绑定，不新建UI资源键。
- 缓存/实例销毁由现有LevelOneAssets与View负责；不改Resources/YooAsset策略，不需手工Inspector配置。

## Integrations and failure behavior
本地离线关卡，无SDK/热更更改。导入继续拒绝重复坐标、额度不符、未知机关。连接组身份/列/容量由原规则验证；无效Continue代次、后台或重复调用不递进，不请求不存在第六关。

## Performance considerations
新增模型1105/2840 Renderer，均小于已有苹果4710；复用共享Mesh、材质、池化飞行与缓存，无新增每帧扫描/资源加载/业务依赖查询。两套源色新增共享材质和蜂/巢预制体。手机性能和发布构建另测。

## Acceptance criteria
- [x] 两份来源SHA、坐标/颜色/额度/队列/连接与导入结果对应，旧三关字节保持。
- [x] 两个真实场景均正确模型、数量、3列/5槽/3排、字形和新UI，无缺失引用。
- [x] 3→4→5→Home→1递进和重复Continue正确。
- [x] 新关取块交付、连接组、重开、Home恢复、暂停/后台与库存保持。
- [x] 原生画面与不同竖屏比例、编译、规范、聚焦/完整回归、代码审查通过，并报告整关/设备验证边界。

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| Source/import | source-audit.json；10项Python导入测试，5关CLI可复现 | Passed |
| Data/assets | native-audit4/5-result.json；128项EditMode；旧三关和旧Bee字节保持 | Passed |
| Navigation | 真实3→4→5、重复Continue、末关Home→1、5关Home恢复/Retry | Passed |
| Gameplay/UI | 真实1105/2840整关全交付Won；四组连接均启动；三联容量/暂停/后台/Retry；1080×1920及1080×2340截图 | Passed |
| Completion | Edit128/128、最终Play134/134、导入10/10、Foundation0警告、Console0、独立审查、原三关与旧Bee逐字节保留 | Passed |

## Definition of Done
- [x] 所有权、资源路径、生命周期与Inspector绑定正确。
- [x] 行为与失败/恢复路径通过，原三关保持。
- [x] 验收证据齐备后Verified，性能和设备边界如实记录。

## Decision log
- 2026-10-18: 用户选择蜜蜂→西瓜，确认沿用现有玩法/UI、第五关胜利回Home。资料包没有关序，仅显式接入两对象。

- 2026-10-18: 资源审计发现既有 Bee 是飞行蜜蜂；第四关采用独立 BeeModel 键，保留旧 Bee 预制体身份和内容。

## Final evidence and boundaries

- 验证记录：`/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/levels04-05/verification.md`。
- 最终完整运行结果 `diagnosed-full-play-results.json`：134/134。完整编辑器 `final-edit-results.json`：128/128；Python导入10/10，规范0警告，控制台0条错误/警告。
- 两关正式场景、原始队列、正式Commands及真实飞行抵达确认：1105/2840全部交付、在途0、Won=true。不是手机或人工实时通关验证。
- 两种原生竖屏比例1080×1920、1080×2340经截图确认。临时尺寸已移除，恢复1080×1920；无新增测试临时场景。
- 连续测试曾出现输入门限/动画超时波动，保留失败和成功证据。当前Editor开启DisableDomainReload/DisableSceneReload，可能保留测试状态；尚未建立其与全部波动的确定因果。只增加失败时只读状态信息，未放宽断言或修改玩法；重新编译后最终完整套件通过。原快速进入设置保留，结束时脚本重载清理测试状态。
- 尚未进行手机性能/内存、发布构建与SDK检查；无手工Inspector绑定待办。
- 既有材质的浮点舍入噪声已用完整基线恢复。MainMenu和其关联两份UI预制体保留既有测试原生重建的同批引用。
