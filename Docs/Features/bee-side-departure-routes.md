---
id: bee-side-departure-routes
title: 空载蜜蜂沿模型左右两侧出巢接近
status: Verified
owner: Project
created: 2026-10-18
updated: 2026-10-18
---

# 空载蜜蜂沿模型左右两侧出巢接近

## Goal
将出巢至采集前的空载路线改到模型左右两侧，减少在模型中央掠过。

## User-visible behavior
- 先从小蜂巢洞口竖直向上离开一段，再以持续抬升的扇形弧线展开到模型左/右外侧，沿侧边弧线上升并转入采集站位；不在洞口高度水平横排，不在侧路接点逐段刹停。
- 侧路根据完整模型投影轮廓更新，花朵、苹果与手动旋转适用；转角必须真实到达，不能因计时走完而斜切模型。
- 保持当前位置连续、取块驻留/抽出，以及取块后的短程入大巢行为。

## Out of scope
不改派发间隔、Busy/预约/邻近许可规则、载货路线、模型尺寸、UI、关卡、美术资源或Foundation包。不引入左右通道锁。

## Ownership and architecture
HarvestBoardView计算模型投影外侧接近点；BeeFlightView唯一写根位移，保存呈现层上方出巢点和侧路转角的到达标记。既有Commands继续管理所有逻辑阶段和实际到达确认。无新增Model/System/Command/Event/Utility。

## State and persistence
仅Actor临时上升/转角到达标记，回池复用重置；实际到达沿用ConfirmBeeFlightArrivalCommand。无存档版本变化。

## UI, prefab, scene, and Inspector impact
Level01和Level02运行时呈现改变，无资产、加载键、Inspector字段、层级或绑定修改。复用现有资源与池清理。

## Integrations and failure behavior
暂停冻结运动；重开清空呈现标记；旋转更新侧路与目标；速度限制保留。窄屏优先保证路线可达与蜜蜂留在屏幕内。

## Performance considerations
每任务以缓存模型局部Bounds的8个角投影计算轮廓，无材质实例、场景搜索或每帧分配；新增Actor到达标记，无物理群集与调度扫描。

## Acceptance criteria
- [x] 左右接近点位于模型整体投影外侧，花朵和苹果均适用。
- [x] 实际出巢先向上飞离洞口，再到下方侧路转角并沿外侧上升，最后接近源块，无跳位或卡住。
- [x] 多巢并行、0.18秒派发、原块交接与短程入巢原有回归通过。
- [x] 旋转、窄视口、暂停、重开仍可完成交付。

## Verification matrix
新PlayMode测试先RED再GREEN，覆盖两关外侧点与实际路线/交付；全量EditMode/PlayMode；Unity编译、控制台、工程规范；原生渲染检查。

## Definition of Done
- [x] 职责和实现范围一致，无无关资产变化。
- [x] 全部验收项有新证据，编译、测试、规范通过。
- [x] 运行时画面确认并记录验证边界后标记Verified。

## Decision log
- 2026-10-18: 用户直接要求出巢改走模型左右侧，按已有目标侧别分路，仅改变空载路线几何，不恢复全局排队。


## Verification results（左右侧路线初版）
- 新增BeeSideDepartureTests三项在旧实现均RED：花朵和苹果接近点仍在模型中部，实际蜂提前上升。实现后3/3通过。
- 完整EditMode121/121、PlayMode122/122，共243项通过，0失败、0跳过。覆盖0.18秒派发、并行调度、到达/抓取/交接、载货短程、固定腿长、旋转、窄视口、暂停/重开和生命周期。
- 原生1080×1920录帧：花朵五巢4秒、苹果三巢连续旋转40°共4秒，每组400逻辑帧/67张画面。分别派发115/66、交付49/26；左右侧采样1664/211和854/902；提前穿中部0、超过阶段限速跳位0、载货Renderer与模型整体AABB相交0。
- 诊断混色只改会话内的连接标记和激活选择，不修改关卡。第一次720×1280截图使HUD像素与相机出口投影不匹配，已丢弃为验收依据，原始诊断保留在work/bee-side-departure/diagnostic-capture-720；最终两组使用与GameView相同的1080×1920，正常布局与路线均确认。
- 工程规范0警告；最终编译结束、控制台0错误/0警告。保留池化与缓存，无新材质/资源分配，每个活动Actor多8角投影计算。此为Mac Unity Editor验证，未测手机帧率。
- 测试生成器重保存的三份菜单资产已从本任务基线逐字节恢复，270份既有资产SHA审计无变化；仅两份生产ViewController、1份新增测试及本FeatureSpec变化。无需Inspector绑定。
- 停止Play，恢复MainMenu；非Git工程保留逐文件基线和范围审计。

## Artifacts
- `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/bee-side-departure/左右出巢路线.webp`
- 同目录red-results.json、focused-results.json、editmode-results.json、playmode-results.json、native-summary.json、scope-audit.json。
- flower-native与apple-rotating目录保留原生帧、100Hz轨迹和逐秒记录；实施备份与脚本位于work/bee-side-departure。


## 2026-10-18 出巢顺序修正（Verified）
用户明确要求先从蜂巢洞向上离开，再向两侧移动。增加呈现层上升段和实际到达门限，防止逻辑计时跳过上升；不增加出巢间隔或任何全局锁。新增两关实际出巢回归，先验证旧版过早横移，再检查上升后完成取块/交付。此前Verification results为上一版证据，本次结果另行补充。


## Verification results（先上升再分流，上一版本）
- 新增FlowerBeeRisesOutOfTheMouthBeforeBranchingToItsSide和AppleBeeRisesOutOfTheMouthBeforeBranchingToItsSide，旧版均因提前横移RED，修正后连同旧侧路测试5/5通过。
- 初始0.1秒屏幕横向偏移不得超过0.001，上升必须可见；进入明显横移前高度差必须超过视口0.035；然后实际完成取块和一次交付。
- 呈现层加入0.85世界单位上升点，约占原接近时长的0.16，真实到达后才开放侧向分流；侧路转角也需实际到达。回池和重开重置两项到达标记。派发仍每巢0.18秒，所有逻辑规则及载货路线不变。
- 完整EditMode121/121、PlayMode124/124，共245项通过，0失败/0跳过。工程规范0警告；最终编译完成，退出Play后控制台0错误/0警告。
- 原生1080×1920两组各4秒/400步/67张帧：花朵五巢115派发、49交付，苹果三巢旋转40°时66派发、24交付；合计1941次初始出巢采样提前横移0。提前穿中部0、超过阶段限速跳位0、载货Renderer与模型整体AABB相交0。
- 诊断仅会话内选择混色蜂巢，不改关卡数据。270份既有资产SHA审计无变化，测试重保存的3份菜单资产已从本任务基线恢复。此轮仅生产BeeFlightView、原侧路测试和本FeatureSpec变化，无Inspector绑定。
- 多一次上升点投影和Actor布尔标记，不新增材质、对象或调度锁；保留原资源池。Mac Unity Editor验证，未测手机性能。停止Play并恢复MainMenu。
- 最新预览、测试记录、原生轨迹和范围审计：`/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/bee-upward-departure/`；动画`先向上再分流.webp`含洞口近景。逐文件基线和诊断脚本位于work/bee-upward-departure。


## 2026-10-18 按参考视频连续弧线调整（Verified）
参考`/Users/zhangshuai/Downloads/Screenrecording_20261018_115925.mp4`，21.8062秒、720×1600，视频帧显示出巢后上升并展开，模型边缘转向接近，采集后短程向顶部大巢汇聚。重点修正原版上升后的水平横移和分段缓动停顿：侧路转角提高到模型下缘以下，使用起止向上切线的扇形曲线；呈现进度按曲线导数限速并受逻辑进度约束，避免动画计时跑完后抄近路。保留0.18秒多巢并行、实际到达确认、固定抓取/源块一致性、局部采集与短程载货。视频不能证明内部预约、调度锁或具体算法，不据此推断。旧验收记录保留为历史，本轮另补新证据。


## Verification results（按参考视频连续弧线，当前版本）
- 读取21.8062秒参考视频，0.6–2.6秒和3.6–6.2秒以0.1秒采样；关键帧/观察/推断边界保存在outputs/bee-route-reference。可见上升、展开、绕边接近与载货汇聚；不据图像推断内部预约或调度算法。
- 新增花朵/苹果RisingFan回归：旧版在横向展开一半时高度约0.4526视口，低于独立模型轮廓要求0.4616/0.4628，先RED。改为持续抬升弧线后全部7项路线回归GREEN。
- DepartureTurn按完整模型下缘生成更高的安全侧路接点。扇形曲线起止切线朝上；扇形和侧边上升移除逐段SmoothStep刹停。Actor增加局部DepartureProgress，使用三倍最长控制边界作为三次曲线导数上界，按13世界单位/秒推进，受逻辑进度和实际到达门限共同约束，防止长路线计时跑完后直追终点。
- 身体远近缩放和接近朝向随实际呈现进度，避免长弧线未走完就提前缩小到采集姿态。暂停冻结、回池/重开清零；沿用现有唯一根位置写入、限速、目标旋转更新和载货固定锚点。
- 完整EditMode121/121、PlayMode126/126，共247项通过，0失败/0跳过；工程规范0警告；最终编译完成、停止Play后控制台0错误/0警告。
- 原生1080×1920两组各4秒、400逻辑步/67张图像：花朵五巢115派发/43交付，苹果三巢旋转40°时66派发/19交付。两组初始出巢采样共1935次，提前横移0、提前穿模型中部0、超限跳位0、载货Renderer与模型整体AABB相交0。出巢现在实际走完弧线，长路线到达时间随距离增长，调度与载货速度未改；详细载货时长见native-summary.json。
- 诊断仅会话内选择混色蜂巢与清连接便于压力验证，不改关卡。270份既有资产SHA无变化；恢复测试重保存的3份菜单资产原始副本。此轮仅两份生产ViewController、原路线测试和本FeatureSpec修改，无Inspector绑定。
- 新增呈现浮点进度、8角模型下缘投影和定长向量计算，无每帧对象/材质分配，无调度锁。Mac Unity Editor验证，未测手机性能。停止Play并恢复MainMenu。
- 最新动画`/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/bee-reference-routes/参考视频路线-新版.webp`，同目录保存测试、原生轨迹、源帧、统计和范围审计；基线/实现脚本在work/bee-reference-routes。
