---
id: three-column-queues
title: 全95关统一三列蜂巢并收紧列间距
status: Verified
owner: Project
created: 2026-10-20
updated: 2026-10-20
---

# 全95关统一三列蜂巢

## Goal

全95关提供一致的三列队列，并稍微收紧水平间距。

## User-visible behavior
用户确认统一3列、每列显示3个蜂巢、保留5槽，并要求列间距稍微减少。采用屏幕归一化中心0.20/0.50/0.80（原三列0.1667/0.50/0.8333），中心距减少10%；蜂巢大小、纵向位置、配额字体、槽位和当前UI风格不变。

## Ownership and architecture
现有项目流程的限定调整，不新增架构/服务/存档schema。LevelOneHud仅调整三列的水平排布，继续使用原5套Inspector控件并隐藏第4/5列，保留几何测试或非生产数据的其他列数支持。原采集、飞行、道具、经济、音乐、自动旋转、结算和进度规则不变。

95关中的38份三列数据保留，56份其他列数离线重排；第2关另按用户后续授权轻量化。优先使用原成功通关激活顺序，将相连蜂巢按组件保持连续，组内分配不同列；稳定重排hives数组并重映射connected索引。除下述明确调整外，保留所有cubes及蜂巢sourceRid/sourceId、颜色、配额和连接边，不靠隐藏有数据的列实现统一。所有生产列数最终为3。若某队列无法保留且验证通过，记录具体内容调整，不改变运行时规则来迁就数据。


## Authorized content adjustments
- 第2关 Apple：用户明确允许削减方块。将原规则网格按0.7重采样，同格取多数色、最近中心的原体素（确定性tie），保留source坐标/深度便于追溯。4710→1831块（减少61.1%），原九种颜色保留。按新实际色额度用既有surface-v1工具配置81只蜂巢、3列，每只1..80；原75只蜂巢和连接配置随重采样重新编排。仅用CubeLandLevelSetup.ApplyLevel(2)原生重建Apple.prefab，共用网格和材质不变。原Apple原始source文件保留。
- 第73关 Croissant：原4成员连接链无法同时处于3列队首，仅断开sourceRid1010↔1022，得到两对双连。保留4只蜂巢的身份、颜色、配额；同一原时间步按队首顺序先激活第一对、再激活第二对。
- 56条稳定重排recipe持久化Scripts/cube_land_three_column_orders.json；缺recipe会报错。低层原始转换测试显式repack=False，不将临时fixture当生产关卡。

## Out of scope

不重做导航栏或商城，不改UI美术、纵向布局、五槽规则、采集/飞行/经济/存档和其他94个模型。真机帧率与95关完整人工游玩另行验证。

## Resources, scenes and lifecycle
- Assets/Resources/Levels/Level01.json..Level95.json；键Levels/LevelNN，仍由LevelOneAssets加载/缓存。
- 仅需重排的JSON与Python导入器/显式顺序配置调整；原始Assets/LevelSources资料、94个模型、95个场景、UI prefab及Inspector引用保留。
- StartLevel/Retry/Leave/Continue沿用现有Commands；HUD按Generation重新布局，蜂巢模型沿原HiveModelView初始化/回收。
- 离线计划/验证属于Editor/Python工具，不进入玩家逻辑，不消耗真实存档或道具。

## Acceptance criteria
- [x] 95份生产数据全部3列，蜂巢列索引0..2；连接组仍可同时在队首出现，Apple以外原色额度及模型不变。
- [x] 56份重排数据与轻量Apple以0.1f秒正式Commands完整交付；记录输入SHA和动作计划，不混用旧数据证明。
- [x] 全95场景实际加载，显示3×3；各行列中心一致、点击区域不重叠、5槽与大小不变，代表关实际搬运/Retry/后台与连接组正常。
- [x] 导入95关可复现，Python与全量Unity回归通过；Foundation校验0警告，Console无新增错误。
- [x] 竖屏新旧对比/截图；除Apple外原模型、UI美术、音频与场景字节不变（测试自动保存项还原）。

## Performance and limits
布局仍只在Generation改变时更新，常数计算无新每帧分配/查找。重排尽量不增加蜂巢数。真机帧率/内存与95关完整人工游玩不在本次范围；逻辑回放与真实飞行样例分开记录。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Pass |
| Unity compilation | Native build/readback and Console | Pass |
| Content and full delivery | migration-audit.csv, 56 migrated rule reports, lightweight Apple report | Pass |
| Real scenes and interaction | ThreeColumnQueueLayoutTests and LevelTwoGameplayTests | Pass |
| Regression | Full EditMode 254/254, PlayMode 169/169 and Python22/22 | Pass |
| Asset scope and appearance | Native prefab readback, portrait screenshot and baseline hashes | Pass |
| Performance | Generation-only layout; no new frame allocation, mesh count 4710→1831 | Pass; device measurements out of scope |

## Definition of Done
全部检查有证据后标记Verified，记录资产、脚本、验证结果和设备验证范围。

## Verification results — 2026-10-20
- 95份生产JSON列数均3，38份原三列文件逐字节保留；56份只重排列和connected索引（Croissant明确切一边），Apple按用户授权减量。导入全部95关对象可复现。
- 56个保留内容候选重放均通过正常Start/Activate/UpdateView/Advance命令完整交付；Apple1831块另以相同0.1f步长完整交付，无加槽/选取/加速/洗牌道具。每份证明核对候选SHA、level/strategy、won/pending、columns、total/deposited。73关分对后同一步按队首顺序激活两组。
- 实际专项7/7：ThreeColumnQueueLayoutTests加载全部95场景并检查实际蜂巢中心、各行宽度、隐藏4/5列、5槽、点击不串列及Restart；LevelTwoGameplayTests检查1831网格/模型/额度、第三行选择/层级/晋升和Leave/Retry。
- 全量EditMode254/254（job8c1383bedbc94587b64699191dbb5116），PlayMode169/169（job1afa6bae409b49aa87d8c9c282087195），Python22/22；项目约定校验0警告。
- 原生Apple.prefab读回1831个唯一方块名称、九种原共享材质。最终baseline差异中，序列化美术/场景/配置仅Apple.prefab变化，其余94模型、所有场景、UI和音频逐字节保留；测试自动重建的MainMenu/Settings及4个浮点规范化材质用本任务baseline经FileUtil恢复。Python运行时缓存不属于功能源码变更。
- 实际竖屏截图：workspace outputs/three-column-queues/apple-three-columns.png（1080×1920）；预览结束恢复真实设置/经济隔离并退出Play、返回MainMenu。
- 最终Console Error0。编辑器进入预览时有2条URP memoryless depth load/store提示，未修改渲染管线配置来规避；本次功能和全部测试无失败。未进行手机真机帧率或内存测试。
- 工作区证据：outputs/three-column-queues/verification-summary.json、migration-audit.csv、migrated-rules/、apple-lightweight/rules-02-10.json、final-scope.json、final-console-errors.json。
