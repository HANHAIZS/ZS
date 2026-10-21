---
id: main-menu-settings
title: 主界面右上角设置
status: Verified
owner: Project
created: 2026-10-11
updated: 2026-10-11
---

# 主界面右上角设置

## Goal

主界面右上角加入设置按钮，打开与参考图一致的精简设置弹窗，仅保留 setting、Music、Sound、Haptic 和关闭按钮。

## User-visible behavior

- 复用现有黄色齿轮、棕黄色面板、三项图标、红色关闭斜线及 Fredoka Bold 字体。
- Music、Sound、Haptic 开关与关卡共享同一偏好和本地保存；关卡原有 Retry、Home 保留。
- 弹窗中没有 Retry、Home；关闭回到主界面，保留本次游戏进度和原暂停归属。
- 弹窗期间主界面的 Start Game 被阻止；关闭后旧触摸释放才恢复主界面按钮，避免误入关卡。
- 首启 MainMenu 和从 Level01 返回的缓存主界面使用同一设置入口。

## Out of scope

新增音乐素材、真实手机震动适配、商店/图鉴内容、新关卡、存档架构更改、主界面其他重排。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | 已有 Commands/Settings/SetPreferenceCommand.cs | 不新增状态写路径 |
| Observable or persistent state | Model | 已有 Models/Settings/SettingsModel.cs | 共享三项偏好 |
| Shared project rules | None | None | 保持已有玩法规则 |
| One-shot notification | None | None | 已有 Popup Router |
| Unity presentation | ViewController | SettingsPopupController.cs、MainMenuPage.cs | 设置支持关卡/主界面上下文；Page管理按钮输入 |
| Adapter or repository | Utility | 已有 SettingsPreferences / MainMenuAssets | 共享存储与 Resources 页面缓存 |
| Reusable infrastructure | Foundation | 已有 UiPopupRouter / SafeArea | 不改包 |

## State and persistence

- Runtime state: 弹窗打开状态和主界面输入释放等待，不改变 MenuNavigationModel 或 HarvestModel。
- Save schema/version/migration: None；沿用 BeeHarvest.settings.v1 三个键。
- State mutation path: Toggle Button → SetPreferenceCommand → SettingsModel / SettingsPreferences。

## UI, prefab, scene, and Inspector impact

- Affected assets: MainMenuPage.prefab；新增 MainMenuSettingsPopup.prefab；运行时代码及主界面生成工具。
- Loading mode and final paths: 主界面仍 Resources `Assets/Resources/Prefabs/UI/MainMenuPage.prefab`，新弹窗直接嵌套引用于 `Assets/Sprites/Prefabs/UI/MainMenuSettingsPopup.prefab`。
- Resources keys or direct reference owners: `Prefabs/UI/MainMenuPage`；设置素材由新 Popup Prefab 直接引用现有 `Assets/Sprites/UI/Settings`，字体 `Assets/Sprites/UI/Fonts/FredokaBoldSDF.asset`。
- Load/cache/instance cleanup responsibility: 随主界面实例化，每场景一个；关闭复用，场景/页面销毁注销 Router 和按钮监听。
- Hierarchy or binding changes: SafeArea右上角 SettingsButton；嵌套 Popup 根保留 DimBackground + PopupContent，移除 Retry/Home，仅标题、三项开关及 Close。
- Inspector confirmation: 生成工具绑定 MenuPage/OpenButton；原 Level01 Inspector 引用保留。

## Integrations and failure behavior

- SDK/consent/lifecycle: 无新SDK；主界面设置不恢复关卡暂停；后台输入不切换页。
- Content update/offline/rollback: 本地素材；缺失已有 Popup/字体显式报错；旧脚本已备份。
- Cancellation, timeout, retry: 关闭只关闭设置；页面隐藏关闭弹窗、清理 Router；无 Retry 操作。

## Performance considerations

复用现有素材与字体，弹窗跟随缓存页面一个实例，无每次点击加载；依赖在初始化缓存，输入等待只在关闭后检查。

## Acceptance criteria

- [x] 右上角设置按钮及精简弹窗在竖屏正确，无 Retry/Home。
- [x] 三项偏好在主界面和关卡同步且本地保存，关闭不改进度/暂停。
- [x] 弹窗与残留触摸不误触 Start Game；反复打开和场景切换无重复监听。
- [x] 编译、约定、相关测试与实际资产读回通过。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | Foundation Python + Page/Popup validator | Passed，Foundation 0 warnings；Page/Popup均 Valid=True, Issues=0 |
| Unity compilation | Console | Passed，无代码/运行错误；调整Game View时出现两条 depth memoryless 编辑器渲染警告，记录于证据 |
| Focused tests | 主界面精简设置、共享偏好、模态阻止Start和残留触摸 | 新增2项先RED→GREEN；全量EditMode 27/27，PlayMode 38/38 |
| Runtime or asset behavior | 两种竖屏截图、Prefab层级及字体/素材读回 | Passed：1080×1920、1170×2532目视；4个弹窗按钮，只有Close+三项；字体FredokaBoldSDF，绑定有效，单实例 |
| Performance | 缓存/生命周期检查 | 初始化缓存依赖，随Page缓存一个Popup，关闭复用并注销Router；真机未测 |

## Definition of Done

- [x] Ownership and QFramework roles match the implemented files.
- [x] Public behavior and acceptance criteria are implemented.
- [x] Save, SDK, hot-update, UI, prefab, and Inspector impacts are handled where relevant.
- [x] Convention validation and Unity compilation pass.
- [x] Focused tests and exact behavior checks pass.
- [x] Performance risks and manual platform checks are reported.
- [x] Status is updated to Verified only after evidence exists.

## Decision log

- 2026-10-11: 用户明确要求主界面设置不含 Retry/Home；作为现有设置与主界面的局部扩展，直接复用素材和偏好，不引入新架构。

## Changed files and evidence

- Scripts: `ViewControllers/UI/MainMenuPage.cs`（模态输入释放）、`ViewControllers/UI/SettingsPopupController.cs`（共享偏好和主界面/关卡上下文）、`Editor/MainMenuSetup.cs`（原素材组装精简Popup、右上角入口）；测试 `Tests/Runtime/MainMenuSettingsTests.cs`。
- Assets: `Assets/Resources/Prefabs/UI/MainMenuPage.prefab`、`Assets/Sprites/Prefabs/UI/MainMenuSettingsPopup.prefab`；MainMenu/Level01消费原Page缓存路径，两个场景由原生成工具保存，关卡设置Prefab及其Retry/Home引用保留。
- Full Play job `85f7a318c3ce4f0ead6046fce9d24ab1`: 38/38 Passed；full Edit job `9df036e36cd24357a398daa47db59690`: 27/27 Passed；focused job `7ef28445c788404f80fd8568037f2ddd`: 2/2 Passed。
- [验证记录](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/main-menu-settings/verification.json)。
- [设置入口1080×1920](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/main-menu-settings/主界面设置入口-1080x1920.png)、[设置弹窗1080×1920](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/main-menu-settings/主界面设置-1080x1920.png)、[设置弹窗1170×2532](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/main-menu-settings/主界面设置-1170x2532.png)。
- Bounds诊断：MCP执行时Screen读取编辑器窗口3024×1588，而Camera与Canvas实际渲染1170×2532；按实际渲染区域确认所有按钮均在屏内，截图正常。
- 原素材无需生成新图片；无新增点击时Resources加载，无新增存档字段或Foundation包改动。无待绑定Inspector；真机安全区域/帧耗时尚未实测。

## Review

Fresh read-only reviewer found no Critical/Important issue in preference sharing, pause isolation, Retry/Home preservation, page lifecycle, held-pointer gating or native prefab bindings. One minor defensive concern is that PopupId currently derives from the Level reference and could change if Level is destroyed before popup teardown; supported-flow scene/reopening tests pass and no stale registration was demonstrated. No additional scope is required for this delivery.
