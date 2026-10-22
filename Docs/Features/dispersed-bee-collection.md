---
id: dispersed-bee-collection
title: 前六关分散采集与外围等待飞行
status: Verified
owner: Project
created: 2026-10-22
updated: 2026-10-22
---

## Goal
前1—6关提前分散采集目标，必须等待的蜜蜂在外围缓慢飞行，减少扎堆悬停。用户已明确选择该方案及关卡范围。

## User-visible behavior
同色且可见的候选块，优先选择离所有正在接近/取块/尚未离开源位的任务最远的区域；没有分散候选时仍可预约并等待，不丢任务。等待者沿模型侧面的前景安全平面缓慢绕行，按任务错开高度和相位；获准后从当前位置平滑靠近原目标。保持相邻区域互斥、真实到达确认、配额/计数、原取块/拔块/返巢动画与第6关90ms交接。

## Out of scope
第7关及以后不启用；不改光照/描边/模型/UI/出巢频率/存档/经济，不改Foundation或原生场景资产。

## Ownership and architecture
LevelData在关卡JSON声明dispersedCollection（默认false），StartLevelCommand写HarvestModel同名只读运行态；HarvestSystem选择分散目标；BeeFlightView和HarvestBoardView负责侧面等待路径及许可后的平滑回接。前六份Levels/LevelXX配置启用。规则不由View直接写Model；沿用到达确认Command。无新注册依赖或持久化字段。

## State and persistence
StartLevelCommand从关卡输入重置开关，Retry/切关仍使用同份输入。Actor缓存等待起点/起始时间/回接姿态，复用时清空；暂停冻结，重开回收。无存档迁移。

## UI, prefab, scene, and Inspector impact
无新美术/Prefab/场景/Inspector引用；Levels/Level01—06为既有Resources键，LevelOneAssets继续缓存。共享Game场景通过同一状态开关渲染，无每帧资源加载或对象查找。

## Integrations and failure behavior
离线可用；无SDK。无分散候选仍派出，保持有限局部互斥；等待者不提前取块，许可后真实到站再抓取；取消/重试/暂停沿用原生命周期。视口缩窄沿用KeepInsideView，路线使用当前投影和模型边界。

## Performance considerations
选块沿用候选×活跃任务检查，额外比较投影距离，无逐候选分配。等待路径为常数次三角函数及原边界8角投影，缓存Actor数据，无物理组件/材质/新GameObject；真机profiling待实测。

## Acceptance criteria
- [x] 仅前6份关卡配置启用，7—114关保持原内容。
- [x] 提前选择更远的同色可见目标，兼顾尚未离开源区的载货任务；无空闲区域仍能完成且计数守恒。
- [x] 真实场景等待蜜蜂在外围持续缓慢移动、错开分布、无瞬移；获准后顺利取块交付。
- [x] 暂停冻结/重开清除，旧关闭模式与原防穿模/取块交接回归通过。
- [x] validator/编译/Console/代码审查通过，原用户场景材质修改保持。

## Verification matrix
| Boundary | Evidence |
| --- | --- |
| 分散规则/范围/退化 | EditMode DispersedCollectionTests，先RED后GREEN |
| 等待/回接/生命周期 | PlayMode BeeCollectionWaitingTests，实际共享场景及绘制对象 |
| 原运输行为 | BeeTrafficRules/TransportReservation/连续飞行/防穿模/第6关交接相关测试 |
| 资产/规范 | 起始SHA256、git范围、Foundation validator、Console |

## Definition of Done
- [x] 所有验收有通过证据，文档Verified并记录手动/设备限制。

## Final verification — 2026-10-22
- TDD EditMode首轮df0b7ab8d7f0488f838a2cf0f611a648：3项按预期失败（更远候选、仍在拔块的任务、关卡开关缺失）；PlayMode首轮fcd5ad61632744f1a40c8a5c2d01a80a：旧悬停最大移动0.06035，小于新缓飞要求0.25，按预期失败。
- 新规则5/5通过：bdcd79c791764401b65429343ed468c3；原规则8/8通过：5de6e8d9390740e48cb5223b668a76c8。
- 最终场景25/25通过：b8bd0a182b254a70a82c8e821f61a853，约46秒；包含BeeCollectionWaitingTests、BeeRouteContinuityTests、BeeClearanceTests、BeeTransportReadabilityTests及LevelSixSourceHandoffTests。
- 共38项唯一测试通过；Unity Console错误/警告0，Foundation validator警告0，git diff --check通过；独立代码审查通过。
- 第6关GPU测试原本比较“派出前整模”与“任务0刚取块后整模”；新规则下任务1/2/5已合法先取走其他块。诊断33c66a9bd3fa47169617fd4163c62951确认后，仅把GPU夹具的选中蜂巢额度拆成1和剩余额度，清除夹具锁链并重开；颜色配额守恒，断言只派出一只，原平均/像素阈值不变。最终GPU回归通过；临时DEBUG日志已删除。
- 语义比对确认前六份JSON除dispersedCollection:true外完全一致，7—114关字节保持；无Prefab/材质/美术变更。Game.unity及两份既有用户材质修改不纳入提交。测试启动时Unity保存了编辑器中待保存的PreviewLevel=3；相对于起始磁盘PreviewLevel=6，仅该预览值发生变化，保留当前编辑器状态。
- 无新增资源或Inspector配置；现有LevelData加载、StartLevelCommand与共享Game覆盖重开及正式进入。前六关每份Levels/LevelXX既有Resources输入启用；状态由Command重置，视图缓存随原Actor池回收。
- 机器可读证据：工作区work/dispersed-bee-collection/final-verification.json与scope-audit.json。未截图/录屏，未做真机触控与性能profiling。
