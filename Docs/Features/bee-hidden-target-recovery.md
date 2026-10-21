---
id: bee-hidden-target-recovery
title: 锁定目标后不中断取块与交付
status: Verified
owner: Project
created: 2026-10-15
updated: 2026-10-16
---

# 锁定目标后不中断取块与交付

## Goal
按用户2026-10-16明确指定的规则，出巢时选中了可见同色块后，即使模型旋转使其被遮挡，也照常搬走该块，消除棕色蜜蜂在花茎附近停滞。

## Root cause
前一版在Approach之后每帧重新核验目标可见性：有替代就重新起飞，无替代则冻结。真实花朵按队列连续采集约44秒后，棕色Palette04任务278锁Cube244，Progress0.60、当前不可见、无可见同色替代，于模型旁持续等候。该分支和反复改目标都与此次用户指定的“找到后即完成搬运”相冲突。

## User-visible behavior
- 新任务仍只从当前可见、未预订的同色方块中选择；始终保留唯一预订。
- 出巢后目标身份不再改变，不因自动/手动旋转、遮挡或其他同色可见块中断。
- 接近、短停、抓握、原位拉出、前景外侧飞行和进入蜂巢沿用现有动画；被遮挡目标依然从其真实Transform取得货物，非复制另一方块。
- 未找到可见同色块时不发出新任务。暂停/后台正常冻结；Retry重置，速度道具仍整体加速。

## Out of scope
不改可选蜂巢/连接组、槽位、额度、交付次数、初始可见性筛选、关卡文件、UI、设计资源、经济、存档或Foundation。不引入空手回巢状态。

## Ownership and architecture
唯一生产改动为AdvanceHarvestCommand：移除出巢后的可见性冻结/重选分支。HarvestSystem继续负责出巢选可见目标；Model结构不变；BeeFlightView继续消费原任务、原Cube真实姿态，沿用安全前景平面和外侧路径。无新Architecture依赖或持久化schema。

## State and persistence
Flight.Cube从预订到交付不变；Cube.Task保持唯一。Hive Remaining/Reserved/Busy/Slot和Cube Remaining/InTransit/Deposited沿用已有三阶段原子结算，无新存档。

## UI, prefab, scene, and Inspector impact
Level01/Level02既有AdvanceHarvestCommand消费者受益；无需Inspector操作，无Scene/Prefab或素材设计变更，所有Resources key与组件引用沿用。

## Failure and lifecycle behavior
隐藏的已锁目标可以取走，未锁的隐藏块不能首次选中。原目标即使在抓握中被遮挡，也继续到交接而不回退。暂停不推进、不改目标；大步长/加速只交付一次；最后额度已释放槽位也不会重新占槽。Retry清理旧任务/预订/Actor。

## Performance considerations
删除每个在途任务的反复可见性复核与重选；不新增分配、资源加载或动画路径计算。全局可见性仍用于新任务挑选。Editor真实棕色场景和两关持续自转验证；真机/GPU不在本次验证范围。

## Acceptance criteria
- [x] 先红测试证明有/无替代时原版都会中断锁定任务。
- [x] 遮挡后锁定同一块、正常取块/交付，守恒、额度、并发和槽位正确。
- [x] 未出巢仍不能选择隐藏块；暂停、抓握、速度、Retry和动画回归正常。
- [x] 原始花朵棕色任务逐个跟踪，不能只用其他颜色或总交付掩盖单只停滞。
- [x] 完整测试、实际画面、规范和只读审查通过。

## Verification matrix
两/三块确定性可见性切换，已放槽后的任务、暂停及加速；原始花朵9秒首蜂锁定与连续出队棕色任务生命周期；完整EditMode/PlayMode、实际录帧、资产SHA范围与项目规范。

## Definition of Done
- [x] 验收有证据后Verified，报告真实验证范围与剩余设备边界。


## Verification evidence
- RED rules: `b36e8d57f8654dd3a0af61c89487c1a9`，三个用例均失败；原版有替代时改选Cube2，无替代时Stage0停滞。RED actual flower: `ee72c238513b419fb275a9ea3f04a5df`，原Cube269被改为273。RED original brown: `4e0facb16d394002be261a507c769f90`，棕色任务216锁Cube242超出正常飞行时长仍为Stage0。
- GREEN完整Editor: `20d9d366fe9f486d9e55545066d884de` **70/70通过**，0失败/跳过。GREEN完整PlayMode: `a36c479ed8074a5eaf66451cefb7f853` **101/101通过**，0失败/跳过。
- 棕色原始数据用例连续模拟90秒队列激活/自动自转，逐任务记录初始Cube，核验不改目标、全部在正常时限内完成；不以最初6个任务成功为停止条件，也不以其他颜色的交付掩盖单只停滞。全程保留1784总量守恒。
- 实际原生画面复现: 原始关卡模拟53.80秒后追踪棕色任务217，锁定 `FlowerV2_Voxel_0242_Palette04`。起始Progress0.6000001，**IsCameraVisible=false、FindTarget(Palette04)=-1**，与旧回归失败的同一Cube242。后续60个不同实际Editor帧中Cube始终242；相对起始0.40秒进入载货，1.95秒送达。所有帧核验Remaining+InTransit+Deposited=1784。CSV的sourceVisible表示Renderer.enabled（取块前仍显示，可能被其他块遮挡），逻辑可见性在brown-context.json中记录，二者不混用。
- 预览 `outputs/bee-committed-target/棕色蜜蜂-遮挡后继续搬运.gif`，48帧，录制时临时隐藏其他蜜蜂的Renderer以便逐只跟踪，所有任务仍完整模拟；黄色圆圈为验证标记，录制结束已退出Play还原；关键帧、原生PNG、CSV和 `verification.json` 同目录。录制Console0错误/0警告。
- 一个新槽位复用测试初次因第二蜂巢正常耗尽并放槽而失败；补足测试fixture额度后全量Editor通过，未为该fixture改动生产逻辑。以前要求冻结/重选的两份测试按用户最新规则改为锁定后推进。未降低几何碰撞、配额、交接或初始可见性断言。
- 只读独立复核：无Critical/Important代码问题。生产代码仅HarvestCommands.cs删除既有冻结/重选分支；三份测试变化。临时空手回巢测试属于本回合自建、已随用户新规则撤下；没有引入任何空手回巢生产逻辑或新Model字段。
- SHA范围: `work/bee-empty-return/baseline-hashes.json`为本回合临时测试创建前的快照，最终列表 `outputs/bee-committed-target/changed-assets.json`。除一份生产脚本及三份测试外，现有MainMenuSetupTests.Apply/SaveAssets再序列化MainMenu.unity、MainMenuPage.prefab与MainMenuSettingsPopup.prefab。未文本编辑YAML，未更改游戏关卡场景、JSON、模型、材质、素材或原有meta。
- 规范验证通过0警告，Unity无新编译错误。临时重载锁已释放、录制回调结束、Play已停止、MainMenu打开。无Inspector待调整项；无新运行时资源分配/依赖；真机性能与完整谜题人工通关不属于此次已验证范围。
