---
id: bee-fixed-grip-transport
title: 固定抱持与清晰取块交付
status: Verified
owner: Project
created: 2026-10-16
updated: 2026-10-16
---

# 固定抱持与清晰取块交付

## Goal
按用户五项要求修正已有搬运流程，先解决腹部抓取关系与唯一预约，再让减速、抓握、沿外露表面抽出、外侧飞行和入口投递可读。

## User-visible behavior
- Body下GripAnchor固定，货物视觉仍由载体拥有并逐帧对齐抓取点，避免继承Body的0.16缩放。方块始终保持原网格、材质和世界尺寸，不做拾取放大或入口缩小。
- 四腿使用固定长度的两段弯曲关节，目标超出范围则限制触点，不能拉长网格。以固定弯曲骨架重建原抓握网格，不改源prefab。
- 保留可采集/已预约/已取走三态、唯一Task、每蜂巢3并发与0.3秒出蜂间隔。相邻目标的首次接近至少错开0.2秒；同色和混色都生效。现有取消入口为重开/重新开始关卡，重置清除全部预约，不添加新的取消UI。
- 0.6秒减速接近、0.14秒悬停、现有0.26秒内完成收腿抓握，0.22秒随蜜蜂沿被选中外露面的朝外方向抽出，再沿模型外侧飞向上方蜂巢。转动时出发后的目标仍锁同一Cube、接近位置持续更新。
- 交付以货物中心进入蜂巢入口为准，隐藏并只结算一次，使用HarvestChangedEvent播放0.2秒、6%入口脉冲；纯呈现反馈保留实际时长，不随加速缩短以便看清。
- 默认停用自动旋转，保留玩家拖动和显式调试开启。其余未搬走方块不补洞、不下落、不重新居中。
- 灰棕Palette15是真实源内部方块；保留其独立色号、配色与缺口，核对无底板/占位遮挡；不擅自改灰棕颜色。原件隐藏与同色等尺寸替身出现同帧。

## Ownership and architecture
Command维护预约、出发错峰、取块/交付；Model只增加运行时调度字段；System返回外露面方向与相邻接近规则；HarvestBoardView将网格方向转为世界方向；BeeFlightView/BeeGraspPose只呈现固定抱持、提取和姿态。沿用唯一BeeHarvestApp及现有依赖缓存，不改Foundation。

## State and persistence
无存档/schema变化。出发后目标不更换；取块前唯一预约，取块后唯一交付。Retry清除任务/预约/临时视觉，暂停与后台冻结全部姿态，速度道具同步推进逻辑与动画。

## UI, prefab, scene, and Inspector impact
影响Level01/02既有脚本。无新Resources键或外部依赖，源模型/材质/关卡JSON不变；GripAnchor与两段关节为每个池化蜜蜂一次创建，随载体清理。无需Inspector重新绑定。

## Integrations and failure behavior
无联网/SDK影响。无可见同色块继续等待；已预约目标后来遮挡仍完成。大步长只结算一次；槽位复用、暂停、重开保持原规则。不能靠缩小货物或拉长腿弥补碰撞。

## Performance considerations
继续池化、缓存Mesh/Material/Transform；无每帧Resources.Load/GetModel。复用现有可见性缓存存外露面；错峰只扫描最多数十个活动任务；固定两段网格变形替代无限二次曲线。

## Out of scope
不合并04/15颜色、不改关卡额度/队列/连接、经济、UI或整体光照风格，不加入复杂避障或新取消按钮。

## Acceptance criteria
- [x] 单蜂转向/起飞/入口前货物恒尺寸、腹部距离固定、四腿有限长度。
- [x] 同色多蜂和混色目标唯一，相邻接近错峰，重开释放预约，交付不重复。
- [x] 表面朝外抽出、持续追踪拖动目标，源件与替身无重复/跳色。
- [x] 默认固定视角与手动旋转正常，取块留下真实缺口，其余格子不移动。
- [x] 完整EditMode/PlayMode、原生录帧、规范、只读复核通过。

## Verification matrix
RED/GREEN实际场景抓取点/恒尺寸/最大腿长；预约/错峰/重开规则；旧碰撞、相机、安全边界、交接、暂停/速度/代次全量回归；单蜂→同色→混色→显式旋转的原生画面。

## Definition of Done
- [x] 状态与行为验收、规范、编译、完整测试和实际画面有证据后Verified。
- [x] 报告代码/资产范围、性能边界和设备待验证项。


## Implementation evidence
- 生产脚本6份：BeeFlightView/BeeGraspPose/HarvestBoardView、HarvestCommands/HarvestSystem、LevelData。无Foundation修改。任务正常仍0/1/2三态；Flight仅新增PickupNormal，未引入第二套预约。
- 腹部GripAnchor为Body子节点(0,-1.5,1.65)，Cargo由载体拥有但以world pose对齐，保留原source世界缩放；无拾取pop或入口shrink。每腿两段各0.525个Body单位（现有0.16缩放后每段0.084世界单位）。超距只限制触点，不伸长骨段。脚尖选择真实方块上离髋点最近的表面接触位置。
- 原生灰内层核对：1784个Renderer对应1784个真实体素，0额外遮挡底板；183个Palette15均使用VoxelPalette15，保持资料包独立色号。
- 首轮PlayMode20条失败中，朝向/自动旋转/同时出发/货物单独提升/缩放等旧契约按用户明确的新要求更新；不删除碰撞/守恒/唯一目标检查。接近调度寻找可见且不冲突的其他同色块，而非让所有蜜蜂堵在首个候选。
- 原世界AABB会在相对旋转后把空角区域判成碰撞；test-only BeeGeometryAssertions改用实际Mesh局部Bounds的15轴分离测试。不依赖生产避障算法生成期望值，仍逐部件校验真实相交。
- RED：b76ad248fc3340c1ba980652df578045（无Anchor/远目标伸腿/默认仍自转三项）；50c6a0b1594a4761b14c394e70c9efdf（同色与混色相邻同时出发两项）；eb812b81b354484796336f5240133164（无投递反馈）；c3d46bbb5e864ff4a120d66be6a551eb（重开反馈残留）；41b3e64aa80f4e4e8bba75555afb9ab3（入口0.0002秒跳0.2328单位）；b42dbe9df334400eafc60bebe4fa9d8e（脚尖未贴表面）。各失败均已先观察到，才修正对应生产行为。
- 独立只读审查发现入口末段硬切，已移除并有连续性回归；Minor为入口pulse不随速度缩短，作为纯呈现反馈刻意保持0.2秒可读时长。无需为了此反馈改变采集结算时间。
- 初轮256原生帧（最后抱持触点精修之前）已归档first-pass-before-contact-refinement，不能当作最终图；最终图和逐帧指标另录。


## Final verification
- 最终Editor：44da3c335bb84213af087ea4f9eacb21，72/72通过。最终PlayMode：7d561a29ce1d4a96834ce74c60466c01，109/109通过，0失败/跳过。合计181。
- 最终原生录帧256张：单蜂→同色多蜂→混色→显式旋转，每组64帧。模式0仅把原测试数据中首蜂巢额度分成1与余量以隔离单蜂；模式2/3仅在内存调整队列以并列展示原始粉/蓝/奶油三色，不保存任何关卡/Prefab变化。
- capture-result.json：violations=0；货物世界尺寸最大误差2.14e-7，GripAnchor距离误差0，髋到脚尖最长0.16443世界单位（每段长度回归固定0.084）。原件隐藏与货物同色共享材质逐帧核对，全部1784总量守恒、活动任务Cube唯一。
- 最终预览：outputs/bee-fixed-grip/蜜蜂固定抱持-单蜂近景.gif、蜜蜂搬运-四组验证.gif；原生PNG、transport-trace.csv、固定抱持-关键帧.png、verification.json同目录。原生画面Console0错误/0警告。
- 原灰棕183块全部材质匹配VoxelPalette15，模型共1784个Renderer，无额外遮挡底板；未更改光照或把真实内层当占位删掉。
- 此前包内TestFramework BackgroundWatcher在退出个别测试时会出现已知异常，未修改Package；最终独立游戏预览没有复现。Bootstrap生成的InitTestScene仅删除本轮新增且不在baseline中的文件，使用Unity原生API；原有同名场景不动。
- MainMenuSetupTests会通过既有原生生成器重新保存MainMenu.unity、MainMenuPage.prefab、MainMenuSettingsPopup.prefab，因此SHA发生变化。未文本编辑任何Unity资产；原关卡场景、源JSON、模型、材质、美术素材均未变化。
- 规范验证0警告；Play已停止、临时reload锁已释放，MainMenu恢复。无Inspector重新绑定项。增加法线缓存与短窗口任务检查，沿用池化；真机帧率/GPU与整关人工解法不属于此次已验证范围。
