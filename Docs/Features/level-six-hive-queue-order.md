---
id: level-six-hive-queue-order
title: 第六关蜂巢队列颜色分散
status: Verified
owner: Project
created: 2026-10-23
updated: 2026-10-23
---

## Goal
用户在第6关中后段持续加入蜂巢时看到更多等待，指出前5个槽位均为白色，要求考虑将白色蜂巢往后排并调整队列。仅调整第6关15个既有蜂巢顺序，分散开局与后续同色蜂巢，保留新采集/外围缓飞和全部原功能。

## User-visible behavior
三列队首提供三种颜色，按0/1/2/0/1加入前5巢时不再全为白色，白色最多2个。白色向后与其他颜色穿插；保持三列、15巢、各巢全部字段及各颜色总额度、盲盒深度2/3/4与各列归属。使用真实规则检查持续补入可完成，采集守恒。

## Out of scope
不修改其他113关、模型/光照/UI/音效/飞行路径/派出频率/经济/存档/机关规则；不通过改变配额或删除蜂巢减轻等待。

## Ownership and architecture
仅Assets/Resources/Levels/Level06.json的hives数组重排；现有LevelOneAssets读取、StartLevelCommand初始化队列，其他角色与Foundation不变。测试属于Editor/Runtime既有测试目录，无新运行时脚本。

## State and persistence
关卡输入固定；重开重新加载同一队列，无存档迁移。运行中不修改当前排队状态。

## UI, prefab, scene, and Inspector impact
无原生场景/Prefab/材质或绑定变更；沿用Resources键Levels/Level06及缓存、生命周期；保留用户Game.unity与两份材质未提交修改。

## Integrations and failure behavior
离线，无SDK。所有蜂巢记录完整保留，包括sourceId/sourceRid/隐藏与连接字段；实际可见性/物理到达/局部互斥沿用，不把不适合先采的颜色集中到所有队首。暂停/重开仍由原流程负责。

## Performance considerations
仅数据顺序调整，无新增运行时分配/组件/逐帧逻辑；真机性能未实测。

## Acceptance criteria
- [x] 开局三列三色，前5巢白色≤2，15巢字段/列归属/颜色额度全部守恒，盲盒深度2/3/4不变。
- [x] 真实第6关持续补入蜂巢对照验证：新队列不会死局且完成，累计等待下降。
- [x] 其他关卡及原取块交接保持，规范/编译/Console/审查通过。

## Verification matrix
| Boundary | Evidence |
| --- | --- |
| 数据顺序/守恒/盲盒 | LevelSixQueueOrderTests先RED后GREEN |
| 实际可采集与排队 | LevelSixQueueOrderPlayTests新旧队列同条件持续补巢对照 |
| 资产/规则 | JSON逐字段比对、git范围、validator、Console及现有第6关交接回归 |

## Definition of Done
- [x] 验收通过并记录证据与未做的设备检查。

## Final verification — 2026-10-23
- RED：55602defb2994739bbfa8ff2e347527e，原队首仅1种颜色而要求3种，按预期失败；原记录守恒测试通过。
- 最终EditMode：a6203de7ffc74384989e263e99914559，2/2通过；验证15巢每个字段/配额/列归属及盲盒深度。
- 最终PlayMode：c2abef7e0cc04a3697f4d623de07a497，7/7通过（持续补巢完整通关对照1项及原LevelSixSourceHandoffTests 6项），共9项唯一测试通过。
- 对照使用自动旋转、默认速度、5个槽位，有空槽立即补入可选队首；同一模型/派出频率/外围缓飞规则。旧队列累计等待1742.424蜂秒、峰值64、通关59.46秒；最终队列1412.344蜂秒、峰值59、通关59.50秒。累计等待降低约19%，通关速度基本相同。两次均采完870块；并非每个时刻等待数都更低。
- 最终sourceId顺序：列0=1/10/5/13/7（白红白红白）；列1=9/2/6/15/8（橙白白黑白）；列2=14/3/11/4/12（红白红白红）。三处盲盒仍在各列深度2/3/4。
- 独立代码审查通过；逐字段对比确认仅hives数组顺序改变。其他关卡与运行时代码无改动，原用户Game.unity及两份材质修改不纳入提交。
- Unity重新编译通过，Console错误/警告0，Foundation validator警告0，git diff --check通过。编辑器关闭域重载，已通过原LevelOneAssets缓存失效与重读核对第6关队首Palette13/Palette07/Palette10，确保下一次进入使用新队列。
- 初步诊断的临时LevelSixQueueProbeTests已从项目删除，仅工作区留文本备份；永久测试仅向TestContext记录结果，不依赖本机临时路径。
- 机器可读证据位于工作区work/level-six-queue-diagnosis/final-verification.json，原始对照日志final-queue-order-comparison.txt。未截图/录屏，未穷举任意手动选巢/加速策略，未做真机性能实测；无需Inspector配置。
