---
id: paused-dialog
title: 关卡紫色Paused暂停弹窗
status: Verified
owner: Project
created: 2026-10-15
updated: 2026-10-19
---

# 关卡紫色Paused暂停弹窗

## Goal

仅更新两个关卡内的设置弹窗，使用用户六张暂停素材，按第七张参考图组合为紫色金边Paused弹窗，恢复Restart入口。

## User-visible behavior

- 两关统一采用紫色金边背景，顶部独立标题牌写Paused，字体使用现有Fredoka Bold，文字和Quit相同的乳白色及深红描边。
- 根据用户后补412×900参考图，弹窗整体居中，高度至多为屏幕55%、宽度至多90%，素材及文字等比缩放，避免大面积占屏。
- 右上角红色关闭按钮关闭弹窗，恢复仅由弹窗获得的暂停；关闭不扣体力。
- 三项Music/Sound/Haptic面板嵌入背景上方，复用原设置键与开关行为。
- Quit居中在下半部上方，调用原Home返回主界面；Restart居中在Quit下方，调用原Retry重开当前关卡。
- Quit/Restart沿用中途退出/重开扣一点体力规则；0体力不能Restart，体力恢复后按钮同步可用。连续点击只生效一次；后台或禁用弹窗时不能操作；回前台必须等旧指针全部结束，再接受新点击。
- 本次替代2026-10-14关卡Quit/Continue布局。主界面设置页和主界面设置弹窗保持原样。

## Out of scope

主界面/Shop、关卡模型、蜂巢队列、胜利结算与下一关规则、经济规则、真实支付、音效资源和Foundation不改。

## Ownership and architecture

项目SettingsPopupController缓存现有EconomyModel，按钮回调调用Level.TryRetryLevel（原RetryLevelCommand）及原Home路径；三设置仍由SetPreferenceCommand变更。PausedDialogFit只在启用或尺寸变更时等比适配可用画布，保证长屏完整显示。无新增Model/System/Event或存档。项目Editor LevelPauseSetup导入/布局，SettingsPopupStyleSetup在关卡构建分支调用新布局，主界面分支保持。现有唯一BeeHarvestApp架构和依赖注册保持。

## State and persistence

存档键、经济余额、设置值均保留。测试使用MemorySaveStore与隔离设置前缀。Restart成功后新关卡恢复运行，旧指针释放前保持关卡输入门控；X保留原手动/后台暂停归属。

## UI, prefab, scene, and Inspector impact

- 直接引用原图：Assets/Sprites/UI/PauseDialog/{Background,Header,Close,Preferences,Quit,Restart}.png，源PNG字节保持，通过Unity Sprite导入切片使用。
- 修改Assets/Sprites/Prefabs/UI/SettingsPopup.prefab的PopupContent子树，保留DimBackground、外部Level/OpenButton与胜利弹窗引用；两个场景实例更新布局，保持原场景父节点及层级关系。
- 原生Prefab/Scene API操作；不文本修改序列化资产。不改变Resources键或加载器。原场景缓存、显示/关闭、销毁生命周期保持。
- 标题单独使用项目字体的材质副本，深红描边不更改共用字体材质。专用字形与材质位于Assets/Sprites/UI/PauseDialog/PausedTitleFont.asset，仅标题引用，共用原字体资产不改。
- 构建入口LevelPauseSetup.Apply仅处理关卡Prefab与两个关卡；原通用构建入口的关卡分支复用同一Build方法。主界面设置弹窗与字体保持哈希；主界面页面由后续导航栏需求单独修改。

## Integrations and failure behavior

无网络或SDK。素材缺失明确报错；编辑态构建拒绝未保存场景。Restart无体力保持弹窗，后台暂停拒绝Restart/Quit；关闭与重开均不让旧触摸穿透。胜利界面继续引用当前设置实例。

## Performance considerations

六张共享压缩纹理，不开启mipmap或ReadWrite；仅弹窗可见时显示。缓存依赖，不每帧查找；Retry可见时最多每秒刷新体力。Editor竖屏1080×1920与1170×2532验证，未承诺真机性能采样。

## Acceptance criteria

- [x] 两关布局与参考一致：Paused顶部、右上X、三设置、竖排Quit/Restart。
- [x] Paused标题字体/乳白深红描边与Quit一致，使用用户原始素材。
- [x] Restart重开本关、扣一次体力，零体力与后台门控正确，恢复体力后可用。
- [x] Quit与X语义及输入/暂停归属正确，三开关状态共享。
- [x] 主界面设置弹窗及共用字体不变、胜利绑定正确，编译/测试/规范/截图和独立审查通过。

## Verification matrix

| Boundary | Required evidence |
| --- | --- |
| Restart真实状态变化/经济/零值/恢复 | PausedDialogTests，现有经济测试 |
| 输入与生命周期 | SettingsPopupTests旧触摸、后台、重载与关闭检查 |
| 主界面与胜利回归 | OkaSettingsTests、MainMenuSettingsTests、VictorySettlementTests |
| 导入/布局/绑定 | 两关原生读回、源图SHA256、实际截图 |
| 完成门槛 | Unity Console、完整PlayMode、聚焦EditMode、Prefab/项目validator及独立审查 |

## Definition of Done

- [x] 验收项有真实证据后标记Verified；手动设备范围明确。

## Decision log

2026-10-15：用户明确Quit是原Home，不是关闭应用进程；Restart替代关卡Continue。以已给定素材与图七布局实施，不新增确认流程。

2026-10-15补充：以用户新发的整体缩小参考作为弹窗占屏比例目标，55%高度/90%宽度双上限，保留长宽比。

## Final verification evidence
- Full PlayMode: job 44be070f664a411dbf34b03910134e6f, 89/89 passed, no failed or skipped tests. Focused EditMode economy/settings job 1164fda48c2d4f12b76e7982eb8e3077 succeeded, 14/14 completed. PausedDialogTests real pointer regression was reproduced red and then 4/4 green after the lifecycle guard.
- Actual final screenshots: outputs/paused-dialog/level-1-paused-1080.png, level-2-paused-1080.png, level-2-paused-1170.png. At 1080×1920 content root is 74.864% width and 55% height; at 1170×2532 it is 90% width and 54.316% height. Tall capture asserts IsOpen, PopupContent and DimBackground before and after RenderView. Final tall preview uses actual saved OFF settings; the temporary preview preference key was removed.
- Six source PNGs byte-identical; all three scenes, original shared FredokaBoldSDF.asset and MainMenuSettingsPopup.prefab hashes unchanged. Native scene readback confirms original Victory SettingsPopup reference and Restart/Quit/Close bindings.
- Prefab validation, zero-warning project convention validation, fresh preview Console clean; independent code and all target screenshots reviewed with no remaining issue. One historical Unity Test Framework BackgroundWatcher error from an interrupted runner was distinguished from production code; final test run and fresh preview passed. No Foundation/package modifications.
- Actual user economy restored/read as 1600 coins and 5 energy; original Music/Sound/Haptic PlayerPrefs remain 0/0/0. Tests used isolated memory storage/preferences. No real-device validation or performance profiling performed.

## 2026-10-19 Quit 行为更新

关卡内 Quit 的当前行为以 [leave-level-confirmation](leave-level-confirmation.md) 为准：先确认，Leave 扣一点体力、重置当前关卡后返回 Home；下次 Start 从本关初始进度开始。取消确认保留进度。本文此前关于 Quit 保留进度的记录为历史行为；内部 ReturnHomeCommand 仍可用于保留会话。
