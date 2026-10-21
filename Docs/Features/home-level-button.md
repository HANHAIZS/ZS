---
id: home-level-button
title: Home背景与关卡开始按钮
status: Verified
owner: Project
created: 2026-10-15
updated: 2026-10-19
---

# Home背景与关卡开始按钮

## Goal
用用户新主界面背景图替换Home背景，删除旧的独立蜜蜂罐子和罐上关卡标签。将显示关卡迁移到原Start Game按钮，并以实际开始目标为唯一显示来源。移除Home与Shop右上角设置入口。

## User-visible behavior
- Home显示用户主界面背景图.png（内含蜜蜂与罐子），不再叠加旧蜜蜂或罐上文字。
- 原绿色开始按钮使用底部Home/Shop/Setting同款Fredoka字体和白字紫色描边，显示level 1或level 2，显示目标与点击进入的关卡一致；普通两关Quit返回显示/继续本关；第二关通关后原规则回第一关，此时按钮也显示level 1。
- Home和Shop右上角不显示设置按钮。底部Setting入口和原三设置功能保留；Shop、经济、导航栏与关卡暂停界面保持。

## Ownership and architecture
MainMenuNavigationController将MenuNavigationModel.CurrentLevel传给MainMenuPage.Show，MainMenuTabsController渲染开始按钮上的缓存TMP文字。Start仍发原StartGameCommand，不新增状态/Command/System/存档。HomeArtworkSetup原生更新共享MainMenuPage.prefab并从常规构建入口复用。

## State and persistence
无新增存档。DisplayedLevel仅缓存当前页面显示目标，来自同一个StartGameCommand使用的CurrentLevel。保存金币、体力、设置与进度规则不改。

## UI, prefab, scene, and Inspector impact
- 新图直接引用Assets/Sprites/UI/MainMenu/HomeBackgroundOka.png；原图字节不改，Sprite Single关闭mipmap/ReadWrite，max2048。
- 直接引用方MainMenuPage.prefab，原Resources键Prefabs/UI/MainMenuPage及生命周期保持。
- 删除HomeArtwork整棵旧装饰与标签；原StartGameButton/Label重用，缓存引用为StartLevelLabel。
- 顶部旧SettingsButton禁用并隐藏，保留引用供原页面输入/兼容弹窗生命周期使用；无法从Home/Shop点击。底部Setting不受影响。
- 三场景、暂停弹窗、Shop素材与导航栏素材保持，原生API操作prefab，不文本编辑。

## Out of scope
不改变关卡模型、胜利去向、扣体力、零体力门控、商店功能或支付；不修改Foundation。

## Integrations and failure behavior
无网络/SDK。素材或必需标签引用缺失时报错。原多指释放、后台、重复点击/异步加载门控保留，0体力不能开始。

## Performance considerations
单张共享背景替换，移除旧Image/TMP/AspectRatioFitter；只在显示或切页时更新文字，无新增Update、查询或订阅。

## Acceptance criteria
- [x] 新背景正确，无旧叠图与罐上标签；顶部Home/Shop设置入口隐藏。
- [x] 首次与两关返回/最终通关的按钮标签和实际加载/恢复目标一致。
- [x] 原关卡进度恢复、输入门控、经济与底部设置功能保持。
- [x] 原生引用/重建幂等、视觉检查、测试/编译与规范通过。

## Verification matrix
| Boundary | Evidence |
| --- | --- |
| 标签与实际开始一致 | HomeBeeLevelTests 首次、两关返回、第二关通关真实按钮与场景状态 |
| 原行为 | MainMenuFlow/Tabs、经济、settings及完整PlayMode回归 |
| UI与资产 | 原生prefab读回、背景原图hash、Home/Shop实际截图 |
| 完成门槛 | Console、prefab及项目validator、独立审查 |

## Definition of Done
- [x] 所有验收完成后标记Verified并记录证据与设备范围。

## Decision log
2026-10-15 用户明确要求按钮显示和开始关卡一致，取代home-bee-level旧spec中的“最终通关返回显示2但开始1”的展示规则。第二关最终Continue仍按既有导航重回1，按钮同步1。

## Completion evidence
- Full PlayMode 0eb5716251cd4d8892137505eaa091ab: 89/89 passed. After the final requested font/material styling, focused HomeBeeLevelTests 449c592c318540e8897ae71c5a41fdc5: 3/3 passed. Initial tests 5137f6fe32d348ee9633c6c8378a1a37 failed on old start game text as expected. An intermediate run before prefab binding was successfully applied failed on the unbound new label; corrected native Apply and final tests passed.
- Native repeated Apply: exactly one Start label, no old HomeArtwork, hidden top gear; Start label and Home navigation label have identical font and NavigationLabel.mat references. No shared font modification.
- Actual 1080×1920 screenshots: outputs/home-level-button/home-level-1-1080.png and shop-no-gear-1080.png. Shop capture checks actual Shop content active and gear inactive. Home and Shop screenshots independently reviewed and approved. Original start button position retained.
- New background PNG equals user source SHA256. All baseline Assets/Scenes, existing Sprites/UI and Sprites/Prefabs/UI files remain unchanged. The only UI prefab changed is Assets/Resources/Prefabs/UI/MainMenuPage.prefab.
- Project and target prefab validators passed; project validator zero warnings; final preview Console zero errors/warnings. No added runtime polling, lookups or subscriptions.
- Validation is Unity Editor preview and automated scene tests; no real-device run or profiling. Actual save formats/economy/settings rules unchanged; tests isolate economy in memory.

## Home背景单独替换（2026-10-15 最新请求）
仅替换HomeBackgroundOka.png图像字节为用户新图（1280×1920，紫花前景版），保持同路径与.meta/GUID、Prefab引用、适配比例、按钮/文字/导航/Shop/Setting和全部行为。使用既有Sprite导入参数，不调用完整UI重建。验证原图SHA一致、除目标PNG外Assets字节不变、原生引用与运行画面、项目规范。

本次验证：仅Assets/Sprites/UI/MainMenu/HomeBackgroundOka.png的SHA变化，其余Assets文件（包括.meta、Prefab、场景、脚本和Shop/Setting素材）逐文件SHA均未改变；目标PNG与新原图SHA一致：0c54322108ed077974d467a07ecdb8b552e826be5dd786778e278435d1676854。Unity原生读回Sprite为1280×1920，GUID仍为3da480b118b5b4a039106e768924a6d7，AspectRatioFitter仍为0.6666667；目标Page的UiPrefabValidator通过，项目规范0警告。实际Home截图为workspace outputs/home-background-refresh/home.png，已人工视觉检查；level 1按钮、导航、经济栏与隐藏右上角设置保持；运行Console无错误。此可逆纯素材替换未新增测试或修改行为。

## 2026-10-19 Quit 行为更新

关卡内 Quit 的当前行为以 [leave-level-confirmation](leave-level-confirmation.md) 为准：先确认，Leave 扣一点体力、重置当前关卡后返回 Home；下次 Start 从本关初始进度开始。取消确认保留进度。本文此前关于 Quit 保留进度的记录为历史行为；内部 ReturnHomeCommand 仍可用于保留会话。
