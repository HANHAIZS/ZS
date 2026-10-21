---
id: victory-settlement
title: 胜利结算遮罩与Continue重开本关
status: Verified
owner: Project
created: 2026-10-13
updated: 2026-10-13
---

# 胜利结算遮罩与Continue重开本关

## Goal

花朵全部方块完成采集交付后，在当前关卡画面上覆盖全屏黑色半透明遮罩，展示用户提供的Victory、蜜蜂和金币透明图及绿色金边continue按钮。

## User-visible behavior

- 沿用HarvestModel.Won：最后一块交付到大蜂巢时判胜利，避免提前冻结仍在运输的蜜蜂。
- 保留原关卡画面，遮罩最终黑色alpha约.65，结算内容显示在遮罩上；禁止底层设置、蜂巢和道具点击。
- VictoryArtwork使用胜利结算动画V3.png原透明图；中上部展示，金币下方居中continue，字体沿用FredokaBoldSDF。
- 入场包含遮罩淡入、内容淡入和缩放弹出，完成后轻微上下浮动。入场时暂不接受continue，应用切后台时停止动画和按钮响应。
- 用户已选择continue重新开始本关。按钮调用现有RestartLevel流程，清除胜利画面与效果、恢复522方块，保留四道具剩余数量；双击只重开一次，旧触摸不会激活蜂巢。

## Out of scope

新增关卡、下一关导航、金币奖励/经济系统、金币数值、胜利图拆层重画、模型和采集动画改动、主界面改动、SDK、存档迁移。

## Ownership and architecture

| Responsibility | Owner | Type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Existing Command | StartLevelCommand via LevelOneController.RestartLevel | 保留现有重开状态路径 |
| Observable state | Existing Model | HarvestModel.Won, Generation | 胜利视图只读；无新存档状态 |
| Rules | Existing System/Command | AdvanceHarvestCommand | 最后交付判胜利保持原实现 |
| Unity presentation | ViewController | ViewControllers/UI/VictoryPopupController.cs | 场景缓存、动画、输入转发及生命周期 |
| Foundation | Existing UI conventions | DimBackground + PopupContent | 不修改Foundation包或注册第二架构 |

## State and persistence

无schema/version变化。Continue调用既有重开流程，Generation加1，Won=false，数量保留。视图只缓存动画计时、当前显示generation和依赖，不直接写Model。后台恢复、Retry或scene reload不得留下旧遮罩。

## UI, prefab, scene, and Inspector impact

- 新增直接引用图：Assets/Sprites/UI/Victory/VictoryArtwork.png，源为用户PNG，原图RGBA且角落alpha=0，无需imagegen。
- 新增直接引用prefab：Assets/Sprites/Prefabs/UI/VictoryPopup.prefab，场景Level01的CoreControls下只实例化一个。
- continue引用现有Assets/Sprites/UI/Settings/RetryButton.png，文字/字体引用现有FredokaBoldSDF，无新Resources键或重复图片。
- 根节点包含全屏DimBackground和PopupContent；后者包含SafeArea/Presentation/VictoryArtwork及ContinueButton，使用安全区比例布局避免不同屏幕比例截断。
- prefab通过Unity原生API创建与保存，并按项目Popup规范验证。DimBackground导入基础alpha沿用项目profile，运行时动画调整到本需求的.65。
- 场景实例绑定Level、SettingsPopup、设置按钮、遮罩Image、内容CanvasGroup、展示RectTransform和continue；缓存依赖于Start。无逐帧加载，场景卸载销毁实例并移除按钮监听。

## Integrations and failure behavior

纯离线素材，无网络/SDK。缺少必需引用明确报错，不生成不可操作的透明弹窗。新一轮重开/scene reload隐藏旧视图并清除模态输入。入场和后台恢复必须等待所有旧指针释放并清理缓存后才启用continue；continue之后沿用Level的等待所有旧指针释放机制。胜利Show先关闭同帧打开的设置弹窗，再接管模态和设置按钮。后台不播放动画、不重开。Disable销毁时释放自身占用的输入。

## Performance considerations

新增一张RGBA贴图（1225×1284、最大2048、无mipmap、关闭Read/Write）及一套缓存UI。Unity Editor原生读回运行内存12,583,872字节（约12 MiB；真机GPU内存未测）。动画仅做RectTransform、CanvasGroup和Image颜色更新；无每帧查找依赖、资源加载、LINQ或分配。只在可见结算期间更新动画。已核对实际1080×1920胜利及重开画面；真机GPU/触控与其他屏幕比例未验证。

## Acceptance criteria

- [x] 所有方块完成交付才显示，运输最后一块时不提前弹出。
- [x] 全屏黑色半透明遮罩保留关卡底图，结算图在遮罩上，continue位于金币下方。
- [x] 淡入、缩放入场和轻微浮动连续，无瞬移，后台暂停动画。
- [x] 入场/遮罩拦截底层点击，continue重开一次并保留道具数量。
- [x] Retry、再次胜利、禁用与scene reload不残留旧UI或重复监听。

## Verification matrix

| Boundary | Evidence | Result |
| --- | --- | --- |
| Convention/compile | Project validator + Unity Console | Passed, 0 warnings / 0 errors |
| Behavior | VictorySettlementTests + complete PlayMode suite | 7/7 settlement; 60/60 total, 0 failures |
| Assets/UI | Native prefab validation/readback + actual victory screenshot | Valid=True, Issues=0; one saved instance, exact source art, 1080×1920 checked |
| Lifecycle/performance | Background/old touch/reload tests + dependency/allocation review | Passed in Editor; mobile devices and other ratios untested |

## Definition of Done

- [x] 项目所有权与命令路径正确。
- [x] 五项验收标准完成。
- [x] 原生保存/读回场景和prefab。
- [x] 编译、规范与运行时回归通过。
- [x] 记录性能、实际画面及真机检查范围。
- [x] 仅证据完成后标记Verified。

## Evidence and review

Evidence directory: /Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/victory-settlement/

- focused.json: 7/7 Passed, job 0478ff5f916f41408bf110f380266f96.
- full-runtime.json: 60/60 Passed, job 42a43f586dd74246aaed9e3a0a5f80c9, 56.56 seconds.
- review-red.json: 两个真实输入边界修复前失败；修复后两项通过，独立复审无剩余问题（review.md）。
- native-readback.json: 原生Prefab/场景/纹理与实关522块清空验证；真实Continue双击Generation只增加1，恢复522块，库存[1,0,1,0]保持。
- victory.png / restarted-level.png: ScreenCapture原始游戏画面，胜利图透明，遮罩alpha=.65，continue位于金币下方。
- source-final.json: 67份现有脚本hash保持；仅新增UI controller及测试，源胜利PNG字节相同。

## Decision log

- 2026-10-13: 用户明确胜利覆盖式布局和源图；continue去向经确认是重新开始本关。
- 2026-10-13: 保留已有最后交付的胜利判定及库存持久逻辑；新增场景缓存UI表现，复用已有按钮底图。
- 2026-10-13: 审查发现旧触摸释放误触Continue与同帧设置所有权冲突；回归先失败再修复通过。完成全套运行验证和实际游戏画面核对后标记Verified。
