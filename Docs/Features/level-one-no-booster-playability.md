---
id: level-one-no-booster-playability
title: 第一关无道具可通关与难度修复
status: Verified
owner: Project
created: 2026-10-23
updated: 2026-10-23
---

## Goal
用户反馈第一关不用道具及使用道具都过不去。修复第一关队列与锁链造成的阻塞，使正常三列选择、五槽、不依赖道具能完整交付原1784块，降低教学关门槛。

## User-visible behavior
保留原花朵、8色、30个蜂巢及每巢额度。开局先给外层可采红/黄/橙，内层颜色后排；30巢按已验证采集顺序均匀排入三列，第一关取消锁链，避免两巢必须同时到队首且占两空槽的入门阻塞。保留原手动/自动旋转、分散取块、外围等待、画面与音效。

## Out of scope
不改其他113关、模型/方块/材质/光照/经济/存档、全局槽数/道具规则、运输与碰撞逻辑。其他关卡的锁链保持。

## Ownership and architecture
仅Assets/Resources/Levels/Level01.json的hives顺序/column/connected调整；所有源蜂巢记录与额度保留。LevelOneAssets仍加载Levels/Level01，StartLevelCommand及HarvestSystem执行原规则。无需新运行时脚本、架构注册或Foundation修改。

## State and persistence
重开使用同一关卡输入；五个默认槽位与道具库存不变，无存档迁移。不会修改玩家进行中的关卡状态。

## UI, prefab, scene, and Inspector impact
无原生场景/Prefab/绑定或新美术。保留用户既有Game.unity及材质修改。沿用既有资源缓存，编辑后刷新Level01缓存确保下次进入生效。

## Integrations and failure behavior
离线，无SDK。列数3、全部配额与颜色总量一致；无空额度或删除方块绕过胜利。取消第一关锁链，其余关卡机关不变。暂停/重开仍由原流程恢复。

## Performance considerations
只改输入顺序，无逐帧开销。定向验证：快速真实规则回归+一次真实场景全1784块通关，无全项目测试循环。真机性能未测。

## Acceptance criteria
- [x] 无道具、五槽，左/右列优先的真实规则模拟均能采完1784块。
- [x] 真实Game场景自动旋转、正常点击队首能完整交付并胜利，重开正确。
- [x] 模型/每巢额度/8色总量守恒，30巢、3列，第一关无锁链；其他关保持。
- [x] 编译、validator、Console、独立审查通过。

## Verification matrix
| Boundary | Evidence |
| --- | --- |
| 不可通关复现/不同选列顺序 | FirstLevelProgressionTests先RED后GREEN，默认五槽、不用道具 |
| 原生完整玩法 | FirstLevelCompletionTests真实场景全交付、胜利与重开 |
| 资产与范围 | 起始SHA256、蜂巢sourceId逐字段比对、Console、validator |

## Definition of Done
- [x] 所有验收有完整证据，文档Verified并记录限制。

## Final verification — 2026-10-23
- 原配置RED：c071f5685b9d4293a52b5541e9d98fb7，3/3失败。左右列优先均在1062/1784后停止推进，20/30巢已入场；完整循环覆盖多轮旋转，并等待400步无新增交付。单独六槽诊断停在1253/1784。配额本身准确，问题来自不可见内层颜色提前占槽、外层颜色后置和双巢锁链的槽位要求。
- 新配置Editor：93aa8e243aea48b091e4b3fc1b529f78，3/3通过：两种选列顺序均完整交付1784，默认5槽，四种道具库存始终1、加速时长0；另验证1784块/8色/30巢/3列及配额守恒。
- 真实Game PlayMode：926f0022db2f4e8082765ca189714da8，1/1通过，约38.5秒。正常队首入槽、自动旋转、真实采集/拔块/到达确认，交付1784并Won；原5槽，不消耗道具；重开恢复1784块/空槽/无旧胜利/无锁链。
- 共4项定向测试通过；独立代码审查通过。无需全项目回归或重复整关对照；项目validator警告0，Unity Console错误/警告0，git diff --check通过。
- 逐字段比对：非hives内容完全不变；每个sourceId对应的color/quota/sourceRid/sourceColor及其他原字段完全保留，仅顺序/column/connected改变。只有Level01生产数据改动，其他关卡字节不变，既有Game.unity与两份用户材质修改不纳入本次提交。
- 新sourceId顺序：69,63,80,72,82,67,62,83,77,74,66,86,64,75,68,87,84,59,71,76,73,61,85,79,70,60,78,88,65,58；按序轮流分配列0/1/2。第一巢仍source69、额度40，保留原首个红蜂采集入口；内层Palette03/04在队尾。其他关卡锁链未改。
- 原始排查快速循环最初不足全部交付步数，后已扩至5000并用停滞检测确认真实阻塞；最终结论以以上完整1784块规则/原生场景回归为准。
- 已刷新LevelOneAssets第1关缓存并读回队首Palette17/Palette01/Palette07，下一次重开/进入生效；无Inspector配置或新Resources键。仍由原缓存和场景生命周期管理。
- 证据位于工作区work/level-one-playability/final-verification.json。未截图/录屏，未穷举所有人为错误选巢组合或真机性能；本关难度调整不承诺任意错误操作都能补救。
