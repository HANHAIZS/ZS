---
id: level-one-hundred-six-soft-chick
title: 第106关复用第6关内容并调浅描边
status: Verified
owner: Project
created: 2026-10-21
updated: 2026-10-21
---

## Goal
用户要求新增一关，内容和第6关完全一样，只把模型方块黑边调浅。追加第106关，使用与第2关相同RGB0.2灰黑边，原第6关不修改。

## User-visible behavior
- 第106关为第6关小鸡：870块、3列、5槽、15蜂巢，保留配额/顺序/锁链/3个盲盒位置、模型朝向与所有玩法；只换关号标题与描边色。
- 复用Chick.prefab，不重建/复制模型。描边宽度仍0.11；搬运方块同色。
- 105胜利进入106，106胜利回主界面并回到1；其他关流程保持。重开恢复106初始内容。

## Out of scope
旧关数据/场景/模型/美术/音频、其他UI/道具/经济/存档schema与Framework；GitHub推送。

## Ownership and architecture
LevelOneAssets追加目录Chick与106范围/描边开关和颜色；原ContinueLevelCommand按LastLevel工作。SourceLevelSceneSetup用6作为106场景模板，CubeLandLevelSetup复用原6模型避免覆盖其朝向；Python导入106复用6的完整转换策略。无新Command/Model/System/Event。

## State and persistence
沿用当前关号/重开/胜利代次逻辑，无存档迁移。末关边界延至106。

## UI, prefab, scene, and Inspector impact
新增Assets/Resources/Levels/Level106.json，Resources键Levels/Level106；新增Assets/Scenes/Level106.unity并启用BuildSettings。模型继续使用Prefabs/Gameplay/Chick；同原LevelOneAssets缓存和场景析构清理。原6文件SHA保持，Native CopyAsset/Scene API创建资产和meta，标题Level 106。无新Inspector待绑定字段。

## Integrations and failure behavior
导入106完整复用6的校验/三列/盲盒，原unsupported输入仍拒绝。107越界仍拒绝。重复/旧generation Continue仍只执行一次。场景构建106不覆盖Chick.prefab，重复执行不新增重复BuildSettings项。

## Performance considerations
复用870块mesh和4色共享运行材质，无新重复模型资源或每帧查找；原Dispose释放。无真机性能结论。

## Acceptance criteria
- [x] 106 JSON与6逐字节一致、模型共享且6 JSON/Scene/Chick SHA不变。
- [x] 106原生画面/870块灰边/实际搬运与重开通过，6仍黑边。
- [x] 105→106→Home1、旧代次/重复拒绝与107越界通过。
- [x] Python确定性导入/三列/盲盒、Unity相关测试/编译/规范校验通过，本地提交。

## Verification matrix
| Boundary | Evidence | Result |
| --- | --- | --- |
| Content | JSON/SHA、完整106导入、盲盒与三列 | Passed |
| Navigation | 105/106边界、107拒绝、重复/代次 | Passed |
| Presentation | Native scene/build refs、截图、真实搬运/重开/释放 | Passed |
| Regression | Python/Unity相关测试、Console/Shader/validator | Passed |

## Definition of Done
- [x] 所有验收与资源/角色边界有证据。
- [x] 预览隔离恢复、MainMenu停止Play、本地提交完成，不上传GitHub。


## Verification results
- RED EditMode8887c38a91aa48cb91a32b459fc8f2bd：7项中3项按预期失败（106资源缺失、105仍回1、106越界），随后实施。
- Python全部26/26通过：CLI确定性生成106份配置，106精确复用6的转换结果；全106三列与合法锁链、盲盒按6的后排交错位置复制，当前有盲盒的关号[6,8,12,15,18,106]。
- EditMode67b572e599c54f8daa0a37422cac24c4，相关内容/旧关/边界114/114通过；105→106、106→Home1、重复/旧generation拒绝、107越界均覆盖。
- PlayMode3cffedaf55db45c4af339719e2f533c6，相关6/6通过：106实际Scene可加载、Level106标题、870块/3列/5槽、JSON与6完全一致、原mesh/BaseColor、4色共享材质、RGB0.2宽0.11灰边、实际蜜蜂搬运灰边与交付、重开还原870块并复用材质、切回6释放灰边材质且6仍黑边；以及原Chick GPU黑边/无对角线、主界面末关流程回归。
- 初次PlayMode41293ce25b91409db81ebfcbd05cc1fa的颜色精确相等断言失败；原生独立读回证明Unity SetColor/GetColor将0.2转换为0.199999958，距离7.74e-8。改用1e-5容差后上述6/6通过，未为测试改生产颜色或玩法。
- Native SourceLevelSceneSetup.Apply(106)重复执行：sceneStable=true、modelUnchanged=true，BuildSettings仅一个106条目，107个启用场景（MainMenu+106关）。目录LastLevel106，浅灰覆盖关仅[2,106]。
- 106场景与6逐行比较仅m_text Level106及LevelNumber106两处不同；106 JSON与6逐字节一致；原6 JSON/Scene/Chick.prefab SHA均与任务开始时完全相同。未新建重复Chick模型或材质资产。
- [原生1080×1920效果](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level106-soft-chick/level106.png)已目视检查，870块灰边、原始朝向和后排盲盒呈现正确。audit106读回870/870、4共享材质、width0.11与搬运复制颜色正确。
- 最终Unity无编译中状态、Shader消息0条，Console错误/警告0条；含base HEAD的项目规范校验通过0警告。预览经济/设置与Editor原重载选项已还原，停止Play回MainMenu。
- 仅新增Level106 JSON/Scene及meta、目录/两份原生Editor构建工具、已有边界/玩法/Python测试与导入器、BuildSettings和本Spec；不改旧关资产、玩家数据或Framework。没有手机真机性能/全关人工通关结论；复用6同一内容而不重新设计队列。

## Final implemented files
- Assets/Resources/Levels/Level106.json及meta；Assets/Scenes/Level106.unity及meta；ProjectSettings/EditorBuildSettings.asset。
- Utilities/Harvest/LevelOneAssets.cs；Editor/CubeLandLevelSetup.cs与SourceLevelSceneSetup.cs（106复用保护/6模板）。
- 原有OutlinedNewLevelsContentTests、OutlinedNewLevelsGameplayTests；Scripts/import_cube_land_levels.py和3份相关Python测试。
- 原Chick模型、原第6关和其他旧Scene/Prefab/材质/UI保持，不新增运行时组件或Inspector绑定。本次仅本地Git提交，不推送GitHub。
