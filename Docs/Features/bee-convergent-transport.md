---
id: bee-convergent-transport
title: 多巢并行出蜂与短程汇聚搬运
status: Verified
owner: Project
created: 2026-10-16
updated: 2026-10-16
---

# 多巢并行出蜂与短程汇聚搬运

## Goal
执行用户给定的16条修改方案。能看清每只蜂的预约、接近、驻留、抽出和交付，同时恢复多蜂巢并行吞吐，删除空载返小巢及其全场等待。

## User-visible behavior
- 每个槽位蜂巢独立每0.18秒派一只，首只随激活出发，唯一预约同色且当前可见的方块。Busy仅统计，不限流；不以左右半区、其他蜂入巢或飞行阻止出发。长帧不补发一群蜂。
- 出发分散曲线接近，外侧近点仅等待邻近采集任务。到位后驻留0.13秒、抱块0.12秒，再沿面法线0.22秒抽出。原块与货块同帧交接，保留真实缺口。取完即走。
- 载货先做约0.14秒局部离面，随后约0.4秒短曲线去入口前近点，再用0.12秒收束入巢。实际到达确认优先于计时，不通过状态切换重设位置。蜜蜂之间允许相交/入口短队列重叠，载货蜂及货物不与模型相交。
- 入口仅做0.02秒局部错峰，其他蜂继续全部阶段。蜂与货一起入巢后销毁/回池，结算一次，不显示空载返程。
- 花朵与苹果呈现缩放1.16→0.86，保持原居中、模型数据和各未采集块局部位置。默认固定模型，手动旋转继续更新接近目标。
- 空载出发身体0.18、贴近模型0.12，形成约1.5倍远近差；载货身体固定0.12、轻微前倾摆动，货物保持源块实际尺寸。腿段固定在身体空间；等待有小幅绕摆/朝向变化，采集驻留稳定。

## Out of scope
不改主界面、商城、设置、关卡数据、配色、蜂巢连接规则、经济/存档、Foundation包。灰棕内部真实方块保留。无物理群集/全局避障。

## Ownership and architecture
Commands拥有预约、局部采集许可、阶段、入口错峰、一次结算；HarvestModel/FlightState保存会话数据；HarvestSystem判断同色可见目标与邻近采集；HarvestBoardView提供缩放和局部安全路线；BeeFlightView唯一写根运动，Body只朝向/远近缩放/抱持。复用现有QFramework架构和资源池。

## State and persistence
Cube维持0可采集→1预约→2取走；增加局部采集许可和入口计时。Stage2为蜂与货物同时结束。重开清空预约、许可、入口时钟，旧代次确认无效。无存档版本变化。

## UI, prefab, scene, and Inspector impact
Level01/02运行时代码变化，无素材、Resources键、Prefab/Scene文本修改或Inspector重新绑定。现有Bee/Flower/Apple/Collector资源复用。View销毁清理池/网格并释放到达租约。

## Integrations and failure behavior
暂停和后台冻结所有阶段/姿态/派发间隔；重开换代，旧任务不能结算；旋转更新目标且不重设根位置；长帧限速连续补走，实际入口确认后交付；窄视口路线保持可达；目标仍独占预约，不重复取块/交付。

## Performance considerations
放宽并发后增加活动Actor数量；保留池化、材质属性块和缓存依赖。邻近冲突只在许可申请时扫描活动任务，不做逐帧全局排站位或全体物理避障。检查长时间混色采集的吞吐及帧耗时。

## Acceptance criteria
- [x] 每巢0.18秒独立出蜂，Busy>3仍派发，同侧/入巢不阻止其他出发。
- [x] 邻近采集许可局部排队、预约唯一；远点和其他蜂巢同时工作。
- [x] 驻留、抽出、飞走可辨，抽出后无长时间挂块等待，路线连续。
- [x] 载货曲线短程汇入入口，蜂与货一起消失且只结算一次。
- [x] 载货与模型不相交，腿长/源块大小颜色/腹部锚点保留。
- [x] 缩小模型、近大远小和三类姿态可见；暂停、旋转、重开、加速、视口和生命周期通过。

## Verification matrix
EditMode验证派发间隔、同时激活、Busy统计、局部采集许可、入口不阻塞及守恒；PlayMode验证花朵/苹果真实渲染帧的连续运动、取块交接/缺口、几何避让、尺寸和腿长、交付消失、远近层次及暂停旋转重开。原生1080×1920录帧与CSV记录ID/目标/阶段/位置/货物/姿态，另做混色压力。运行工程全量EditMode/PlayMode和Game Foundation规范验证。

## Definition of Done
- [x] 上述验收与验证证据齐全。
- [x] 新旧回归按最新行为契约更新，完整测试通过，无新控制台错误。
- [x] 报告实测载货时长、并发/吞吐、性能及设备验证边界。

## Decision log
- 2026-10-16: 用户已直接给出完整修改方案并要求全面实施；据此连续实施，例行实现参数不再另设审批。非Git工程使用逐文件备份和SHA范围审计。

## Verification results
- 全量 EditMode121/121、PlayMode119/119，共240项，0失败/0跳过。入口重建先RED（新货块距离错误起点10.56世界单位）再GREEN；六巢入口积压先RED再通过。
- 最终1080×1920原生渲染三组各6秒、100Hz模拟、20fps录帧：花朵五巢固定/花朵五巢旋转/苹果三巢。只在诊断会话里选取可见色蜂巢并清除连接标记便于并行压力，不修改关卡文件、原始色板或游戏默认队列。
- 花朵固定：170次派发，105次交付，活动峰值69；旋转：170/105，峰值71；苹果：88/53，峰值41。
- 载货阶段2开始到交付中位数三组均0.66秒，最大0.73/0.77/0.69秒；入口局部等待最大0.08/0.12/0.04秒。初始0.06秒入口间隔在五巢压力下曾积压1.62秒，最终改为0.02秒，且不恢复任何全局限流。
- 最终360张原生帧、1800个逻辑采样：载货蜂所有Renderer与模型整体AABB相交0；源块提前移除、尺寸/材质错误0；货块lossyScale最大误差1.81e-7。根位移最大0.229402世界单位/10ms，无超过阶段限速的记录。三组入巢时其他蜂仍接近和载货的帧数394/387/338。
- 固定腿段、腹部抓取点、原块/替身同帧交接、真实缺口、暂停/后台/重开/加速/视口和View生命周期由完整场景回归覆盖。默认固定模型、手动旋转保留，自动旋转仅显式测试开启。
- 测得模拟更新（不包含截图编码/GPU渲染）平均2.21/2.34/1.60ms，单步最高11.96ms，Mac Unity Editor数据；未声称手机帧率，真机性能仍需设备测量。
- Game Foundation规范验证0警告。独立只读审查1项Important已修复并由新回归覆盖，无Critical，无延期Minor。

## Cleanup and asset scope
- 生产修改7份：Commands/Harvest/HarvestCommands.cs；Models/Harvest/{HarvestModel,LevelData}.cs；Systems/Harvest/HarvestSystem.cs；ViewControllers/Harvest/{BeeFlightView,HarvestBoardView,HiveModelView}.cs。新增两份测试和相关旧契约回归更新；不新增资源加载键或Inspector绑定。
- 284份既有图片/模型/材质/关卡/Prefab/Scene做SHA范围审计；游戏模型/素材/关卡数据/游戏Prefab未变。现有MainMenuSetupTests会重保存MainMenu.unity、MainMenuSettingsPopup.prefab和MainMenuPage.prefab，属于测试生成器重建，保留当前UI设计及绑定，完整菜单/设置回归通过。
- 清理本任务新建的3份InitTestScene，保留此前临时Scene。中途测试热重载留下的DontSave TestRunner对象引发测试框架PlaymodeLauncher.cs:113退出异常，已保存日志并清理该临时对象，不修改Test Framework包；正常游戏运行已完成40次派发/40次交付且方块守恒1784、游戏运行控制台0错误；退出时另一个旧测试回调自注销后报告TestRunnerCallback.cs:20异常，已保存诊断，不改包。清理后再次进入/退出控制台0错误/0警告，TestRunner残留对象0。
- 非Git工程留逐文件快照和SHA审计；不创建Git仓库或更改用户分支。关闭Play后恢复MainMenu。

## Artifacts
- `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/bee-convergent-transport/native-summary.json`
- 同目录 `editmode-results.json`、`playmode-results.json`、`scope-audit.json`、三个场景的trace.csv和frames。
- 动画预览 `蜜蜂并行搬运-预览.webp`；完整GIF `蜜蜂并行搬运-花朵与苹果.gif`；原生对照 `并行搬运画面.png`。
- 执行记录、基线备份、审查diff和诊断脚本位于 `work/bee-convergent-transport`；规则调整的代价是允许蜂间重叠，符合用户明确要求，仍保留载货与模型的几何约束。
