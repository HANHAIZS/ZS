---
id: lightweight-mystery-levels
title: 第112–114关轻量盲盒模型与第6关搬运效果
status: Verified
owner: Project
created: 2026-10-22
updated: 2026-10-22
---

# 第112–114关轻量盲盒模型与第6关搬运效果

## Goal
用户授权选1–5关，后确认仅兔子/宇航员/沙漏这三关，需兼容现有隐藏机制并沿用第6关方块外观与源位搬运效果。追加112兔子1860、113宇航员1995、114沙漏1432；保留完整源模型。

## User-visible behavior
- 三列、5槽，原始隐藏标记/颜色/配额均保留。三关均使用原队列，无需重排、删减或静默删除机关。
- 三新关使用第6关7%间隙、模型4%放大、原第6关黑边颜色/宽度、降低环境光/柔和阴影/第7关局部缺口阴影。
- 取块同帧真实源位交接、90ms平滑移至Grip和局部阴影淡出，原220ms拔出不变；暂停/加速/重开与池复用沿用第6关实现。
- 111→112→113→114，114完成回Home第一关；原111关内容保持。

## Out of scope
不新增机关/色板/玩法规则/存档/UI/音乐，不修改Foundation，不推GitHub，不截图录屏、不承诺真机性能。仅本次三模型，不扩大选材。

## Ownership and architecture
| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | existing Harvest/Continue Commands | unchanged |
| Observable or persistent state | Model | existing HarvestModel/MenuNavigationModel | no schema change |
| Shared project rules | System | HarvestSystem | unchanged |
| One-shot notification | Event | existing events | unchanged |
| Unity presentation | ViewController | LevelOneController/BeeFlightView/Board | reuse six's handoff |
| Adapter or repository | Utility/Editor | LevelOneAssets/import_cube_land_levels/SourceLevelSceneSetup | explicit range/content |
| Reusable infrastructure | None | None | Foundation unchanged |

## State and persistence
无新持久化；Actor状态与临时PropertyBlock沿用第6关。游戏写操作经过既有Commands。

## UI, prefab, scene, and Inspector impact
新增Resources/Levels/Level112..114.json和Resources/Prefabs/Gameplay/{Rabbit,Astronaut,Hourglass}.prefab，加载键分别Levels/Level112..114和Prefabs/Gameplay/{name}；既有Assets缓存、Board释放实例/临时材质/纹理。原始JSON位于Assets/LevelSources/CubeLand，不运行加载。新场景通过Unity原生API复制Level06，仅改关号/标题，加入BuildSettings。复用既有voxel/palette/ChickVoxelOutline/Level07VoxelCavity，无旧美术改写。所有Inspector由原生读回验证。

## Integrations and failure behavior
离线Resources，无SDK/网络。不能完整通关的配置不可发布；暂停冻结/重开清理临时货块、切场景释放材质纹理；保留缺失资源/未知机关的报错。只接入现有隐藏标记，不放宽不支持的机关校验。

## Performance considerations
每模型1432–1995格，复用原共享色材质、单场景局部缓存、Actor池与90ms属性更新，无新增逐体素Update/逐块材质。真机未测。

## Acceptance criteria
- [x] 三份完整源几何/颜色/深度守恒，低于2000块，3列，隐藏身份保留，SHA可追溯。
- [x] 三关正式Commands以0.1s推进完整通关，报告SHA与生产JSON一致。
- [x] 三场景真实搬运首帧/90ms、边色/光照/间隙/缩放与6一致，盲盒等待隐藏→队首恢复正确。
- [x] 新导航边界、暂停/重开/切场景回收正确，旧111关资产保持。
- [x] 编译/规范/针对性Python、EditMode、PlayMode检查通过。

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validator | Passed，0 warnings |
| Unity compilation | no new errors | Passed |
| Focused tests | content/navigation/runtime | Passed，242 EditMode、17不同PlayMode、27 Python |
| Runtime or asset behavior | source audit/native readback/full rule replay | Passed，5287块完整交付，SHA绑定生产JSON |
| Performance | shared material/cache/pool scope | Passed，复用既有共享材质与缓存；无真机帧率结论 |

## Definition of Done
- [x] Ownership correct; no framework changes.
- [x] Acceptance and recovery pass.
- [x] Native scene/prefab references verified.
- [x] Conventions/compile/checks pass.
- [x] Evidence and device limits recorded.

## Decision log
- 2026-10-22: 用户自行选材授权后明确只做兔子/宇航员/沙漏三模型，低于2000且含现有盲盒。兔子/宇航员原队列strategy0完整通关；沙漏strategy5也已完整通关；三关均无队列重编。


## Verification evidence — 2026-10-22
- RED EditMode c6697b0bd2094d968128df9f920a3d55：8项中7按预期失败，新关超出现有111目录、111仍返回Home；PlayMode d0d27509908547bb844fc6f5b6e0d22f：2/2按预期失败，新场景不可加载。最初两次测试因新脚本尚未被Unity导入返回0项，未视为有效验证；强制原生导入后完成RED。
- GREEN全BeeHarvest.Tests EditMode 790ab7ed59a74e30afa14be6eaf2531d：242/242通过，含3新模型/隐藏/配额/目录、111→112→113→114→Home1及重复/旧代次Continue拒绝、115越界、既有内容与规则回归。
- PlayMode bb93b56d5a8e413bb2b2ba0845279db3：17项中16通过，唯一失败是新测试严格Color.Equals对Unity材质sRGB读写2.98e-8–5.16e-8误差未容忍。原生只创建临时材质复现后，将该断言改为既有测试使用的1e-5容差；没有改生产显示逻辑。
- GREEN新PlayMode 8c959e2655c14b358781c67b5c1fac07：2/2通过，内部遍历112/113/114，逐块原网格/位置/尺寸/颜色、7%间隙、4%整体放大、6关黑边、柔和/缺口阴影及灯光、源位首帧/30/90ms交接/材质淡出清理/暂停/实际交付/重开/跨场景释放；隐藏问号居中/额度隐藏/队首渐显/暂停/重开恢复。加上前次通过的6关交接6项、旧盲盒8项、107–111效果1项，共17不同运行检查最终通过。
- Python unittest discover Scripts/tests：27/27，完整114关CLI重新生成与生产JSON一致，全部三列、隐藏身份/原配额保留、旧6/106精确一致、非法机关校验保留。
- 原生正式Commands离线回放：112strategy0/1904步、113strategy0/2017步、114strategy5/1741步，固定dt0.1s，deposited分别1860/1995/1432，remaining=inTransit=0，全部原蜂巢唯一激活。报告inputSha256与生产JSON相符，共5287块。报告在共享工作区outputs/levels112-114/rules，原始来源SHA在work/levels112-114/selected.json。
- 独立源审计：每个原始坐标/颜色/深度和生产集合一致，未削减/合并颜色；31/35/26个原蜂巢、隐藏15/16/12个及原三列顺序完全保留，所有色号在既有1–20内。三来源无不支持的Voxel/Hive特殊状态，只使用既有isHidden机制；shouldLoop/shouldOfferMeteorShower等源游戏元数据仍按原适配器处理，本次不复制源游戏新系统。
- 原生Scene/Prefab创建与读回：三Scene与Level06仅关号/标题不同；源Prefab每格名称/坐标/尺度/网格/色材质对应JSON；所有GUID有效且唯一。BuildSettings115条启用唯一路径（MainMenu+114关），三场景缺失脚本0、dirty=false，所有既有UI/音频/弹窗/胜利引用保留。
- 测试引起MainMenuPage.prefab/MainMenu.unity/MainMenuSettingsPopup.prefab原生序列化变化，已用Unity FileUtil从工作前HEAD字节恢复。最终范围哈希确认全部旧非脚本Assets不变；旧改动仅3份生产C#、2旧测试C#、4份Python及BuildSettings，新增3原始JSON/3生产JSON/3Prefab/3Scene及meta、2测试及meta、本Spec。
- 独立只读复核未发现Critical/Important；已处理文档验收状态和证据更新。Foundation0warnings，最终C#/Shader无新增错误。不截图录屏、不推GitHub，iPhone打包和真机性能未测。
