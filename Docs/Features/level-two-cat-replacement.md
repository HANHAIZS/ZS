---
id: level-two-cat-replacement
title: 第2关小黄鸭与第35关猫咪内容对调
status: Verified
owner: Project
created: 2026-10-20
updated: 2026-10-20
---

# 第2关小黄鸭与第35关猫咪

## Goal
按用户最终确认：将第2关猫咪与第35关小黄鸭的完整内容对调，其他不变。原苹果已由完整2471块Cat_Luci替换；猫咪最终位于35关，小黄鸭位于2关。

## User-visible behavior
- 第2关：原第35关小黄鸭Duck，2190块、30蜂巢。第35关：猫咪Cat_Luci，2471块、106蜂巢。
- 交换完整模型、体素/颜色/配额/队列/连接关系，两个JSON内容逐字节对调；标题仍Level 2/Level 35，关卡进度顺序与95关总数保持。
- 3列×3等待行、列中心0.20/0.50/0.80、5工作槽、默认朝向/自动旋转、现有UI、音频/道具/结算不变。

## Out of scope
其他93关的资源、所有现有prefab/场景/UI/共享材质、导航/商店、用户存档和账户体力金币、Foundation、渲染管线。旧Apple源和prefab保留为历史资产，不在当前关卡目录表加载。无需清除玩家存档。

## Ownership and architecture
- Utility：LevelOneAssets.ModelKeys仅交换第2/35项Duck/Cat_Luci，沿用原Levels/LevelNN与模型缓存/实例生命周期。
- 作者工具：import_cube_land_levels.py同步源目录表、显式surface-v1队列位置（2→35）与猫咪三列override（2→35），重导入可复现全部95份数据。
- 状态突变仍走原StartLevel/Activate/Advance/Retry/Leave/Continue Commands，无新Model/System/Event或架构注册。
- 第2/35关已有模型均原生读回180度正面；此交换不重建prefab、不修改Inspector或场景。

## Resources and lifecycle
- Levels/Level02 → Duck完整数据；模型Prefabs/Gameplay/Duck。
- Levels/Level35 → Cat_Luci完整数据；模型Prefabs/Gameplay/Cat_Luci。
- Assets/Scenes/Level02.unity/Level35.unity名称和LevelNumber不变，加载/销毁由原LevelOneAssets/HarvestBoardView负责。
- 原Cat_Luci.json来源SHA与全部2471原网格点/颜色保留；2与35内容以交换前SHA逐字节核对。其他93份生产JSON和既有美术/场景按本任务baseline核对。

## Failure and persistence
缺资源沿原明确异常路径；无保存schema变更或清档。实际Retry/Leave/Continue、真实搬运、暂停/后台在相关原测试中验证。手工预览使用现有隔离fixture保护真实设置与经济；验证后恢复测试生成UI和浮点规范化材质。

## Performance
仅目录表两项、两份JSON以及离线导入位置变化；两只已有模型与已有队列均不增减，不新增每帧查找/分配。猫咪2471块处于用户2000–2500预算。未进行真机帧率/内存测量。

## Acceptance criteria
- [x] 2关Duck2190/30；35关Cat_Luci2471/106；Resources实际加载与ModelKey一致。
- [x] 两JSON逐字节互换、其他93份JSON不变；既有prefab/场景/UI/音频字节保留。
- [x] 两份当前生产SHA以0.1f正式Commands无道具完整交付；模型源网格/颜色预算不变。
- [x] 实际两场景布局、搬运/重试、退出/Start、Continue顺序正确，标签不交换。
- [x] 导入95关可复现，Python/相关Unity检查/项目校验通过；原生截图与Console核对。

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| Content scope | swap baseline/两个交换SHA/93关字节 | Pass |
| Rules | 新2/35当前SHA、0.1f完整交付 | Pass |
| Resources | 原生加载和默认朝向，两个已有prefab不变 | Pass |
| Importer | 全95生产数据重导入比对/Python tests | Pass |
| Runtime | LevelTwo/Expanded35/布局/搬运/重试/退出/结算测试 | Pass |
| Project | 编译/Editor tests/Foundation validator/Console | Pass |
| Appearance | 两个关卡的实际竖屏图 | Pass |

## Definition of Done
全部验收有证据后标记Verified；记录93关/美术不变与测试范围，恢复临时预览隔离、回到主界面。

## Verification results — 2026-10-20
- 两JSON与swap baseline交叉SHA/字节完全相等；其他93份关卡与93个目录表项均不变。所有95关导入对象一致，Python22/22。
- 新Level02 Duck2190/30，正式0.1f无道具全部交付，生产SHA匹配；新Level35 Cat_Luci2471/106，同样完整交付。proof位于workspace outputs/swap-levels02-35/rules/。
- Editor256/256（job ba3f0bb4d749496cb2c522263cad10da）。相关PlayMode首轮32/33，唯一失败源于旧测试将小黄鸭第三行联动蜂巢当单只；仅改测试，改选第三行单独蜂巢并断言联动规则保持，LevelTwo6/6重跑通过（job7a76b118f73c41fabd1353f0ba8f3dc0）。其余27项覆盖已通过，包括全部80个新增场景实际加载、35关真实搬运/Retry、退出/Start、Continue、布局与飞行边界。
- 不改变任何运行时规则来适应测试。保存完整源数据/配额/连接关系，两个已有模型与所有原场景/材质/UI/prefab/音频逐字节保持。测试自动重建的3个界面文件和2个浮点规范化材质经本任务baseline原生FileUtil恢复后，serializedAssets差异为空。
- 实际1080×1920截图：outputs/swap-levels02-35/level02.png（Level 2小黄鸭）、level35.png（Level 35猫咪）。编号、正面朝向、三列三行与五槽均已目视核对。临时设置/经济隔离已恢复，退出Play回到MainMenu。
- 最终Console Error0；Foundation项目约定校验通过。未进行手机真机帧率/内存测试。
- 汇总：workspace outputs/swap-levels02-35/verification-summary.json和scope.json。
