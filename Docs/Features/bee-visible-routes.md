---
id: bee-visible-routes
title: 连续外侧进场与分区搬运交通
status: Verified
owner: Project
created: 2026-10-16
updated: 2026-10-16
---

# 连续外侧进场与分区搬运交通

## Goal
按用户最新优先级解决蜜蜂接近目标时隐没/闪现观感和画面重叠，保留已经修好的固定抱持、腿段长度、颜色尺寸和真实缺口。

## Root cause evidence
原生Level01固定视角：单红蜂只取Cube406（绿色茎部旁），每0.02秒最大39.018像素，未测到身体中心被模型遮挡；无Animator/Animation/Rigidbody，只有BeeFlightView写根位置。三蜂实际约5.88秒，任务8/Cube273在Progress0.62时单帧位移4.471世界单位/187.496像素；十秒最大224.948像素。五蜂巢压力用例最大511.98像素，11426次投影包围区重叠。可确认Separate每帧重新排站位/深度层造成跳变；逻辑Stage0→1本身最大仅0.0058世界单位，不能把所有闪现归于该状态切换。基线CSV/summary在outputs/bee-visible-routes。

## User-visible behavior
- 预约后先经模型外侧接近点，再减速靠近外部抓取站位；目标持续随模型变换更新。根位移受连续速度约束，从当前位置推进，不再每帧瞬间切换避让坐标/深度层。
- 将实际到达确认纳入场景采集：未到抓取站位不能移除原块，未到入口不能交付；暂停冻结，旋转显著改变目标时重新确认到达。无View的纯规则模拟沿用确定性时长，实际游戏始终挂接呈现确认。Attach/Detach携带View身份租约，旧View不能释放新View；中途重建归一化已通过的转角/已完成返程，重开保留活跃呈现确认。
- 同一区域保持一个采集任务；投影距离与进出通道共同参与筛选，左/右通道各一个正在进场的任务，沿用独占Cube预约。全局最小出发间隔0.3秒，并让出发口在前一只离开0.75模拟秒前保持占用；所有空载蜂回巢前不派新蜂。各槽位轮询，宁可等空闲区域，也不让多蜂挤同一点。
- 完整悬停/收腿/0.22秒向外抽出，蜜蜂与货物一起离开，保持腹部锚点与原块尺寸颜色。
- 左右载货通道各一个行进/等待任务；右侧载货与空载返程错开，到入口前等待点排队，入口只允许一只进入，并等前一只实际到达侧边出口。货物中心抵达后结算一次，蜜蜂空载先到右侧出口、再到下行转角、最后回原出发点入池，所有转角均检查实际到位，禁止计时抄近道。
- 默认固定视角保留手动旋转，显式自动旋转仅用于压力验证；灰棕内层先保留，经源数据/实际网格核对后记录。

## Ownership and architecture
Commands拥有阶段、预约、到达确认、通道/入口排队、交付和返回计时；Model只新增会话级交通状态；System判断通道与投影区域；Board提供相机空间外侧/等待/离场点；BeeFlightView唯一写根位置，Body只改朝向、翅膀和抱持。无Foundation变化，无新增架构注册。

## State and persistence
Cube仍0可采集/1预约/2取走。Flight.Stage仍0采集/1载货/2已交付，新增载货子阶段（抽出、等通道、通道移动、等入口、入巢）与到达/返回计时。Stage2不再次结算。到达命令带代次/任务/阶段校验，重开丢弃旧确认与全部预约。无存档/schema修改。

## UI, prefab, scene, and Inspector impact
Level01/02脚本行为变化，无新素材/Resources键，无Inspector重新绑定；池化Root仍归BeeFlightView清理。源模型、队列、额度、色板、主界面/UI不改。测试原生生成器可能重保存菜单资产，记录SHA范围。

## Integrations and failure behavior
无网络/SDK影响。旧移动任务不并行存在；暂停不推进时间/姿态；场景销毁清理事件与网格；显著旋转会延后实际取块，保持已预约Cube，不重新抢目标。大步长/headless规则仅用于模拟，场景需真实到达后交接/结算。等待通道不会被误判为死锁。

## Performance considerations
仅扫描有限活动任务/缓存相机坐标，保留Actor/Mesh/Material池；抱持网格采用Body相对框架，未变化的中立/固定抱持姿态复用网格，减少重复顶点变换/法线重算；删除每帧全方向避让试探与深度层跳变。根运动/方向/抓握使用缓存依赖，不在每帧GetModel/Resources.Load。

## Out of scope
不再调整腿长，不改内层颜色/删除真实内部块，不做复杂寻路/物理群集，不改经济、UI、关卡文件或连接蜂巢规则。

## Acceptance criteria
- [x] 单蜂Cube406固定模型轨迹连续，速度受控且能看清外侧接近；无其他根位置写入者。
- [x] 多蜂和混色投影采集区/入口不重叠，无逐帧重新排站位瞬移，保持唯一预约。
- [x] 抓取站位与飞行终点一致，原块隐藏和同尺寸同色替身出现同帧，留真实缺口。
- [x] 入口等待/独占/错峰交付与另一方向空载返回，交付只一次。
- [x] 暂停/后台/速度/重开/视口与旋转回归、完整测试、实际录帧及规范检查通过。

## Verification matrix
先RED复现单蜂速度和多蜂位置跳变/投影重叠，再测区域限流、入口间隔、场景到达确认。最终单红蜂406→同色→混色→旋转逐帧记录ID/Cube/阶段/根位置与货物姿态，观察真实图像；全量规则与场景回归、资产范围和规范验证。

## Definition of Done
- [x] 全部验收有证据后Verified，报告实际变化、性能/设备边界与剩余问题。

## Review and diagnostics
独立只读审查已复查并关闭：View销毁后RequireFlightArrival残留、视口变化使旧返程Origin/载货Exit不可达、Attach中途任务的转角迁移、复用槽位返程占用。最终复查未发现新的Critical/Important阻塞。

初始Palette04在完整Level01十二个30°视角的可见Cube数量均为0，是实际内部方块；棕色回归先用真实Commands采集外层至第一个目标暴露，再逐任务检查旋转后的稳定目标和有界完成时间，不改变关卡数据或内部颜色。降低并发后不再沿用旧90秒内暴露六个内部块的假设。

## Verification results (completed)
- EditMode 75/75, PlayMode 114/114, 0 skipped；完整控制台0错误，Game Foundation规范验证0警告。
- 原生1080×1920、固定视角、每0.02s一帧，各500帧：单406、同色多蜂、五蜂巢压力三组最大根位移均约0.090002世界单位/12px。原地Stage0→1最大0.005822世界单位；三组完整活动蜂（含空载返程）投影AABB重叠均0。单406身体中心遮挡采样0。各组完成交付1/3/3次。
- 基线同口径同色最大224.948px/20ms；五蜂巢511.98px/20ms、11426次投影重叠。基线/最终CSV均保留，不能将本次有限样本外推为所有关卡/设备的绝对无碰撞证明。
- 固定腿段、源块颜色/尺寸/朝向、真实缺口、一次结算、暂停/后台/重开/速度、手动/显式自动旋转、窄视口到位/返程入池以及呈现Attach/Detach租约由回归覆盖。棕色原始内部目标专项通过。
- 实际终端/触屏设备上的帧率仍需设备测量；本次为Mac Unity Editor原生场景验证。

## Native visual evidence and scope audit
- 最终五组各10秒、500帧、1080×1920（单406、同色、五巢压力、真实混色、真实混色+旋转）全部投影AABB重叠0，最大0.090003世界单位/20ms约12px，旋转组Stage0→1最大0.006443世界单位。详见 `outputs/bee-visible-routes/all-scenarios-route-summary.json` / CSV。
- 四组各180张原生GameView帧，20fps共720帧，缩至540×960用于预览。`capture-result.json`: violations=0，原块/货物缩放误差最大1.96e-7，腹部锚点误差0，最大腿部端距0.164431世界单位。图像已人工检查外侧进场、绿色茎部抓取、外侧载货、入巢和分路返程，提供固定全局色表GIF避免预览量化闪烁。
- 规范验证0警告；189项测试通过。完整动作从位置继续，窗口重排仅在投影矩阵变化时将现有姿态移入新安全视域，不使用交通避让跳位。
- 本次新增的TestRunner临时Scene通过AssetDatabase清理；此前存在的临时Scene不动。关闭Play，恢复MainMenu。MainMenuSetupTests重保存三份既有菜单Scene/Prefab（已有测试覆盖布局/绑定），本功能不修改UI设计；模型/图片/色板/关卡JSON/游戏Prefab/材质保持原SHA。
- 当前有意采用保守限流，出发与空载回巢共用区域时等待；并发与吞吐低于旧版拥挤路线。后续可在保持当前轨迹/预约验收的前提下微调间隔，不属于本次必须扩展的复杂避障。

## Changed implementation
`Models/Harvest/{HarvestModel,LevelData}.cs`; `Commands/Harvest/{HarvestCommands,BeeArrivalCommands,UpdateHarvestViewCommand}.cs`; `Systems/Harvest/HarvestSystem.cs`; `ViewControllers/Harvest/{HarvestBoardView,BeeFlightView,BeeGraspPose}.cs`，以及规则/场景回归。资源加载键、Inspector绑定、关卡存档和Foundation不变。

## Artifacts
- `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/bee-visible-routes/蜜蜂外侧采集-单蜂近景.gif`
- `/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/bee-visible-routes/蜜蜂连续路线-四组验证.gif`
- 同目录 `test-results.json`、`all-scenarios-route-trace.csv`、`scope-audit.json` 和 `frames/transport-trace.csv`。
