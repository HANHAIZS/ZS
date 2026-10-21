---
id: levels-six-fifteen
title: 新增十个轻量关卡（第6至15关）
status: Verified
owner: Project
created: 2026-10-19
updated: 2026-10-19
---

# 新增十个轻量关卡（第6至15关）

## Goal
按用户要求新增十关，随后补充“不要有太多方块”，并选择将超过2000块的模型换成轻量模型。保留现有玩法、美术UI、弹窗动效、音频和前五关。

## User-visible behavior
第6至15关依次为Chick小鸡870、Sheep绵羊1030、Shark鲨鱼1054、Broccoli西兰花1084、Cactustoy仙人掌1205、Carrot胡萝卜1244、CatHead猫咪1300、Snake蛇1595、Onion洋葱1617、DodoBird渡渡鸟1622块。保留源三列队列、5工作槽、3排等待区、额度、颜色身份与连接组。第5关后进入6，依次至15，15胜利Continue返回Home，下一轮1；重复/过期Continue不推进。Home显示同关号，Leave/Retry按现有规则重置该关，库存、经济、暂停与后台恢复不变。每关真实采集交付可以完成。

## Out of scope
新机关、重新设计UI、修改前五关模型或额度、存档解锁、付费/经济规则、Foundation更改、牛油果等超过2000块的新增模型、发布或手机性能承诺。

## Ownership and architecture
原Commands/HarvestModel/HarvestSystem保持状态与规则所有权。LevelOneAssets的显式模型目录扩展15关；Editor SourceLevelSceneSetup复用现有关卡原生创建流程。Python离线适配器显式包含15个源对象，转换算法不变。CubeLandPalette增加13/14/19三个原始色号的图标代表色，生成对应共享方块材质与蜂/巢预制体。不新增运行调度、注册或架构。

## State and persistence
关号仍为当前会话状态，没有新存档或迁移。所有变化由现有Commands完成。测试使用隔离经济和设置，不写玩家存档。

## UI, prefab, scene, and Inspector impact
新增Assets/Scenes/Level06.unity至Level15.unity，加入BuildSettings。新增Assets/Resources/Levels/Level06.json至Level15.json（键Levels/Level06..15）；新增Resources/Prefabs/Gameplay/{Chick,Sheep,Shark,Broccoli,Cactustoy,Carrot,CatHead,Snake,Onion,DodoBird}.prefab（键Prefabs/Gameplay/同名）。原始来源保留Assets/LevelSources/CubeLand/同名.json用于审计。新共享色材质位于Assets/Sprites/Gameplay/CubeLand，蜂/巢键BeePalette13/14/19、HivePalette13/14/19。沿用LevelOneAssets缓存与现有View实例销毁责任。既有UI绑定原样复制；标题字体确保支持6–15。小鸡/绵羊/猫咪/渡渡鸟增加180度美术子节点，仙人掌增加90度子节点以正面展示，复用既有香蕉的朝向处理，运行时零偏航不变。无手工Inspector待办。

## Integrations and failure behavior
纯离线本地关卡，不改SDK和热更。拒绝重复坐标、额度不符、未知机关；末关不加载16，取消/后台/重开遵守既有代次和整体连接取消规则。继续防止同色拥塞、封闭目标或错误连接容量造成关卡不可完成。

## Performance considerations
新模型870–1622个Renderer，均不超过2000块，新增关只增加目录项与缓存数据，不增加每帧加载/依赖查询；复用共享网格/材质、已有飞行池与6音效声源上限。实际设备性能另测。

## Acceptance criteria
- [x] 十份来源SHA、坐标/颜色/额度/队列/连接精确保留；每关不超过2000块。
- [x] 十个实际场景/模型和标题、UI绑定、Resources、BuildSettings正确。
- [x] 第5至15递进、末关返回、重复/过期回调、Home同关号、重开正确。
- [x] 十关真实场景采集/交付及完整通关验证通过，原五关和UI保持。
- [x] 编译、规范、Python导入、EditMode/PlayMode回归、画面和审查通过。

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| Source/import | SHA256、Python导入可复现、原五关不变 | Passed |
| Native assets | 10场景/模型/调色板/UI绑定和标题原生读回 | Passed |
| Navigation/lifecycle | 5→6..15→Home、重复/代次、Home/Retry、后台 | Passed |
| Gameplay/visual | 十关真实飞行全交付、竖屏截图 | Passed |
| Completion | 编译/Console、规范、完整Edit/Play、独立审查 | Passed |

## Definition of Done
- [x] 项目所有权与资源路径/生命周期正确。
- [x] 验收证据齐备，已有游戏不被意外改动。
- [x] 状态Verified前记录通过结果与设备验证边界。

## Decision log
- 2026-10-19: 用户要求再做10关，先确认十主题，再要求方块不要过多，选择将超过2000块的模型换成轻量模型。取消Duck/PizzaSliced/Ladybug/Avocado，加入Shark/Cactustoy/Snake/Onion并按体量排序；最终上限1622块。体量递增不等同于已标定的难度。


## Final evidence and boundaries
- RED job 1bd31fdd076f47cd88bdf752f072c36c: 新关目录和第5关后的推进缺失，21项按预期失败；非法16关测试通过。实现后完整编辑器169/169（最终job afc3e7ad53924f03822373b73572473e）通过。
- 聚焦运行5/5（1f9e34ac66d044ad9eeef9fa45f5d7c9）。初次聚焦有1项失败，原因是测试每轮退出+重开花费2点体力而只补1点；隔离测试改为每轮至少3点，未修改玩家规则。
- 最终完整PlayMode 160/160（3c2d3845ea504c319064c5f56f13eba1），零失败/跳过；Python完整导入测试10/10，15关CLI可复现；Foundation规范0警告。
- 十关正式场景原生验证：原模型、原队列、5槽、正式Commands、相机可见性与真实BeeFlightView抵达确认，12621块全交付，Won=true、Remaining=InTransit=0，无道具。胡萝卜连接11/16、13/18及蛇3/11、12/18、16/25全部实际启动。旋转模型的五关均重新验证。
- 自动试玩的选巢顺序曾让西兰花/胡萝卜停下；改用原队列及连接组前置顺序、允许已有规则中的同色多巢后完成，不改关卡或运行规则。整关验证会在3秒无采集时转动模型45度。渡渡鸟正面复核分两次执行同一局：首段22秒工具预算后剩18块，继续537逻辑帧后全交付，没有重置或改状态。
- 1080×1920真实场景截图level06.png至level15.png，标题字体、3列/5槽/3排、模型数量、材质和原暂停/道具图标绑定读回正确，缺失脚本0。新小鸡/绵羊/仙人掌/猫咪/渡渡鸟已正面展示。
- 所有十份原始JSON SHA与资料包清单一致，转换结果精确可复现。前五关场景、数据与模型及原UI预制体逐字节不变。Editor测试重建的3份菜单资产与一份材质浮点舍入噪声已通过Unity FileUtil恢复。字体仅补关卡标题字形，BuildSettings新增十场景，无Inspector待办。
- 独立只读代码审查无确认缺陷；复核朝向子节点与递归名称解析、测量网格变换、相机可见性和Retry兼容。
- 证据目录：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/levels06-15；核心final-audit.json、final-edit-results.json、full-play-results.json、capture06..15.json、proof06..15.json。最终预览为新十关预览.png。
- 真机性能/内存与发布构建未测。预制体和关卡数据缓存沿用已有生命周期，已访问模型仍可能留在资源缓存；没有新增每帧查询或加载。整关验证为编辑器加速模拟及真实飞行确认，不是人工实时或手机通关。
