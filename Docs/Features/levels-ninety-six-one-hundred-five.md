---
id: levels-ninety-six-one-hundred-five
title: 新增第96–105关与方块黑色棱边
status: Verified
owner: Project
created: 2026-10-21
updated: 2026-10-21
---

## Goal
用户要求再做10关，组成模型的方块同样采用长宽高描黑。将10个未用独立主题追加到95关之后，复用第6关已验证的立体黑色棱边，不改变旧关内容。

## User-visible behavior
第96–105关依次为鸽子/可乐罐/冰山/独轮车/教堂/瓢虫/枫树/吉普车/蝎子/鼠标，共10个不同源模型，1291–2913块/关，全部3列/原5槽。新关方块使用细黑棱边、原色面保留，自动/手动旋转和搬运一致；第6关描边保留，其他旧关显示保持。95→96→..105，105胜利返回Home并回到1；沿用现有UI、音乐、弹窗、道具、胜利60及进度重置规则。

## Out of scope
旧95关JSON/模型/布局/机关/颜色额度改动、新机关、经济/存档schema、UI与音频重设计、Framework、真机性能/发布。源队列中不适用的锁链不带入新玩法，新增关用离线surface-v1三列队列；原始JSON只读保存在来源资产中。

## Ownership and architecture
- LevelOneAssets扩展显式目录与LastLevel105，并集中描边开关（6、96–105）；LevelOneController把显示选项传给HarvestBoardView。VoxelEdgeVisuals复用原Shader/模板/共享Mesh，无逐方块新对象，缓存每种颜色一份场景材质，退出释放。
- Python导入器为新增10关固定源目录/3列surface-v1规则，保持每个源体素坐标、颜色、名称和每色总配额。只重编新增队列，不改变生产Command/System以迁就内容。
- Editor复用CubeLandLevelSetup/SourceLevelSceneSetup原生创建Prefab和Scene/BuildSettings，检查起始朝向；正式Commands以0.1s步长验证10关完整交付。
- 无新架构/Model/System/存档；使用原ContinueLevelCommand的LastLevel边界、原Settings/音频/动画。原始模型源只用于离线导入，运行时不依赖外部下载目录。

## Resources and lifecycle
新增Assets/LevelSources/CubeLand/{10 names}.json、Assets/Resources/Levels/Level96..105.json（键Levels/Level96..105）、Resources/Prefabs/Gameplay/{10 names}.prefab、Scenes/Level96..105.unity。原生BuildSettings加入10场景；不文本编辑序列化资产。复用Materials/ChickVoxelOutline、VoxelEdges.shader和原palette/model mesh，旧资源路径不迁移。无需Inspector手工填项。

## Failure and recovery
缺失/未知关号明确拒绝；只从已获胜且代次匹配关卡Continue一次。重开当前新关还原源配置、搬运状态和旋转；离开释放材质。源数据未知机关/隐藏状态/颜色/配额异常仍由原导入校验拒绝；未能通过完整规则回放的模型改用更兼容素材，不修改运行规则。验证使用经济/偏好隔离。

## Acceptance criteria
- [x] 10份不同主题、3列、可复现源坐标/颜色/配额，全10正式Command完整通关证明与生产JSON SHA绑定。
- [x] 每个新场景资源/标题/模型/描边/音乐/弹窗/胜利绑定正确，实际搬运与重开可用。
- [x] 95→96、104→105、105→Home→1及重复/旧代次拒绝通过。
- [x] 旧95JSON/模型/美术/音频/UI保持，6描边保留、旧其他无描边；新增资源生命周期与导入正确。
- [x] 编译、Python、相关Unity/规范检查、10模型原生预览和范围哈希通过。

## Verification matrix
| Boundary | Required |
| --- | --- |
| Content | 源SHA、10不同形状、deterministic import、3列、每色数量与总配额 |
| Gameplay | 10关0.1s完整Command交付证明；真实场景搬运/重开 |
| Navigation | 边界/重复/代次/末关回Home |
| Presentation | 10原生图、所有方块描边/原色、共享原Mesh/材质、旋转 |
| Regression | Python/Unity相关测试、旧资产哈希、Console/Shader/validator |

## Definition of Done
- [x] 所有验收有证据，角色/资源/数据边界正确。
- [x] 独立复核、编译/运行与规范检查通过。
- [x] 范围核对和原生画面完成，恢复隔离并返回MainMenu。

## Verification
- Red EditMode73d6973fe670474e8f5d32500e1c61af：5项中4项按预期失败，原95胜利回1/96–105越界；Red PlayModeab970f3d917543be9477d933c24d9a88，Level96不可加载。
- Python全部25/25通过，包含105关确定性导入、全关三列、盲盒原有5关标记、原Apple/Cat/Duck迁移和特殊输入拒绝。按关号数值排序支持Level100..105文件名。
- 完整EditMode8a2447a241d84b2c9fa1da3f41e84afa：261/261通过，含新范围/95→96、104→105、105→Home1、重复Continue与错误generation/106拒绝和旧内容测试。
- 相关PlayMode78f978039da44efeaf35c5545f271ec0：12/12通过，包含遍历全部10新场景的模型数/三列/原5槽/所有方块黑边/每色共享材质/当前UI音频/实际搬运描边/采走隐藏/交付与重开，以及原Chick GPU测试、自动旋转、盲盒回归。
- Native正反面比对后，仅新模型99/101/103/104/105在生成器中增加180度美术Orientation节点，展示车轮、眼睛、车头、钳子和鼠标按键；其他旧95条朝向不变。新场景全10实际搬运/重开测试12a7f27fc30e4d5a8ea92ef989ec0263再次1/1通过（该测试内部遍历10关）。逻辑数据和完整通关SHA不受美术朝向影响。未重跑全部旧PlayMode，不宣称全量运行测试通过。
- 全10正式Commands逻辑通关proof：各生产JSON SHA绑定0.1f步长、won=true/pending=false、remaining=inTransit=0、deposited=total，激活order完整且无重复。总22602方块全部交付；各关1291/1685/1732/1804/2402/2591/2648/2660/2876/2913块。证明见rules/rules-96-10.json..rules-105-10.json与rule-decisions.json。逻辑回放和10场景的实际搬运为不同验证边界，未声明全10场景人工连续通关。
- 复核发现工作区verify-rules.py可能沿用旧磁盘报告，已补首调用清理target、RPC.error/isError/success、返回报告=磁盘报告、level/name/strategy/SHA/步长/总量/order校验，并全10重新真实生成证明。独立只读复核确认这些新证明匹配生产SHA且漏洞关闭、无剩余重要问题。
- 源raw10份SHA均匹配选材目录；完整坐标/颜色集合等原来源，不抽空或删减模型；每色体素数=总配额。surface-v1仅新10配置使用3列按层配额，源原始锁链不带入新队列。旧95份JSON逐文件SHA不变。
- 本批模型包括mouse_2；原生画面确认是电脑鼠标，已把中文主题从初选“小老鼠”纠正为“鼠标”。最终10份Native1080x1920场景截图与裁切拼版总览检查完成，正面和黑边清晰。原始场景PNG保留，拼版只作查看，不修改游戏美术。
- 第一次隔离模型预览的直接DestroyImmediate(sharedMaterial)清理方案被自动安全审核拒绝，理由是可能误删共享资产；没有执行该命令。改用正常场景加载/卸载截图，游戏自身释放运行材质，不做共享资产删除；全部10图已成功生成。
- 原生最终读回106个启用场景（MainMenu+105关）、路径无重复、新10全启用；HasVoxelEdges仅[6,96..105]，Shader消息为空。最后Console0errors、非Play/非编译、MainMenu。
- scope.json：既有Assets仅8份C#（目录/显示开关/Editor和旧边界测试），Scripts4份和EditorBuildSettings改动。新增84份Assets文件（10raw/JSON/Prefab/Scene各带meta，共80；2新测试带meta，共4）。无缺失旧文件，旧Scene/Prefab/美术/音频/材质/95JSON字节保持；测试保存的MainMenu/UI和材质已Native FileUtil恢复。
- 最终project validator Passed、0warnings。未增加每帧依赖/资源查找，描边按颜色共享场景材质并在卸载释放，无每方块新渲染对象/额外Forward pass；未在手机真机进行GPU/内存/触控评估，不声明iOS打包发布完成。
- 预览隔离已恢复原经济/设置，停止Play并回到MainMenu。总览、清单、content-audit/scope、各测试与通关证明在工作区outputs/levels96-105。

## Final level list
| 关卡 | 主题 | 模型 | 方块 | 蜂巢 |
| --- | --- | --- | --- | --- |

| 96 | 鸽子 | Pigeon_2 | 1291 | 84 |
| 97 | 可乐罐 | Cola | 1685 | 95 |
| 98 | 冰山 | iceberg | 1732 | 56 |
| 99 | 独轮车 | Wheelbarrow | 1804 | 60 |
| 100 | 教堂 | Medieval_church | 2402 | 113 |
| 101 | 瓢虫 | Ladybug | 2591 | 104 |
| 102 | 枫树 | Maple | 2648 | 123 |
| 103 | 吉普车 | Willys | 2660 | 103 |
| 104 | 蝎子 | Scorpion | 2876 | 83 |
| 105 | 鼠标 | mouse_2 | 2913 | 68 |
