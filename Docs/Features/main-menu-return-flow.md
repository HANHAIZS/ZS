---
id: main-menu-return-flow
title: 蜜蜂主界面与关卡往返
status: Verified
owner: Project
created: 2026-10-11
updated: 2026-10-19
---

# 蜜蜂主界面与关卡往返

## Goal

用用户提供的背景与按钮组装竖屏主界面，设置 Home 返回主界面，Start Game 继续当前首关进度。

## User-visible behavior

- 首次启动展示主界面背景；底部 Shop、Home、Gallery，Home 居中凸起高亮；绿色 Start Game 放在导航条上方，准确显示 `start game`。
- 字体、奶白字色、深棕描边与设置界面一致，使用 Fredoka Bold；背景等比铺满、居中裁切，按钮不拉伸。
- Shop/Gallery 当前只显示入口图标和标签，不做商店、图鉴内容；Home 为当前选中项。
- 设置弹窗 Home 启用：关闭设置，显示主界面，暂停采集；Start Game 隐藏主界面、恢复原关卡画面和进度，不重置额度、方块、蜜蜂任务或花朵角度。若此前已暂停，仍保留原暂停状态。
- 新启动主界面 Start Game 才加载 Level01；设置返回主界面时保持原场景不卸载。Retry 仍重开当前关卡。
- 反复往返无重复页面/监听；退出设置和 Start Game 后须释放原触摸才能再次操作蜂巢。

## Out of scope

商店商品、图鉴内容、经济/体力、其他关卡、联网、跨应用退出存档、新中央角色模型；主界面设置入口由后续 main-menu-settings Feature Spec 单独管理。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | Commands/Navigation/MenuNavigationCommands.cs | 初始化页面、返回 Home、开始/继续；统一维护暂停归属 |
| Observable or persistent state | Model | Models/Navigation/MenuNavigationModel.cs | 当前 Home 状态和进入 Home 前的暂停，无持久版本变化 |
| Shared project rules | None | None | 两页具体流程由 Command 拥有 |
| One-shot notification | Event | Events/Navigation/MenuNavigationChangedEvent.cs | 通知已切换页面 |
| Unity presentation | ViewController | ViewControllers/UI/MainMenuPage.cs、MainMenuNavigationController.cs | 缓存页面，显示/隐藏关卡，首启加载场景；不直接写 Model |
| Adapter or repository | Utility | Utilities/UI/MainMenuAssets.cs | Resources 页面一次加载缓存 |
| Reusable infrastructure | Foundation | 既有 UI Page 约定、SafeArea | 不修改 Foundation 包 |

## State and persistence

- Runtime state: Home 可见状态、恢复暂停值；现有 HarvestModel 仍拥有全部关卡进度。
- Save schema/version/migration: None。进度保留于本次运行内存中。
- State mutation path: UI Button → Navigation Command → Model + Event → View 渲染；设置 Home 在 Hide(false) 后传入原暂停状态。

## UI, prefab, scene, and Inspector impact

- Affected assets: 新 MainMenu.unity；Level01 增加导航组件；SettingsPopup 启用 Home 绑定；新 MainMenuPage.prefab。
- Loading mode and final paths: 页面 `Assets/Resources/Prefabs/UI/MainMenuPage.prefab`；直接引用 art `Assets/Sprites/UI/MainMenu/{Background,ShopIcon,HomeIcon,GalleryIcon}.png`；Start Game 直接复用 `Assets/Sprites/UI/Settings/RetryButton.png`。
- Resources keys or direct reference owners: `Prefabs/UI/MainMenuPage`，MainMenuAssets 缓存；图标/背景由页面 Prefab 直接引用；共享 FredokaBoldSDF。
- Load/cache/instance cleanup responsibility: 场景导航组件每场景只实例化一次页面，切换复用，随场景销毁；不在每次点击中加载资源。
- Hierarchy or binding changes: 新页面根 MainMenuPage→Content（背景）/SafeArea（按钮）；保留原关卡层级和 Inspector 引用。
- Inspector confirmation: 竖屏画面、文本、素材绑定、两场景 Build Settings、设置 Home、继续进度与两排队列正常。

## Integrations and failure behavior

- SDK/consent/lifecycle: 无新 SDK；Home 暂停任务，后台/设置暂停归属沿用；触摸跨页不触发关卡。
- Content update/offline/rollback: 全本地；缺失页面资源明确报错；现有素材与 scene 先备份，不覆盖原图。
- Cancellation, timeout, retry: Home 取消未完成入槽跃动；首启场景加载时禁用重复 Start；Retry 不改变导航目标。

## Performance considerations

页面最多一个实例；4张UI图片+共享字体/按钮，无每帧加载；Home 停止关卡 Update，保留模型以免丢进度。背景限制2048，图标512，无纹理CPU副本。iPhone 真机测量留待设备验证。

## Acceptance criteria

- [x] 用户素材、Fredoka Bold、Start Game 和底部三项布局竖屏正确。
- [x] 设置 Home → 主界面 → Start Game，额度/任务/方块/角度连续保留。
- [x] 首启 MainMenu→Level01、反复切换、原暂停、后台、触摸释放、Retry 正确。
- [x] 资源加载/Prefab约定/Unity编译/Foundation验证通过，原首关测试无回归。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | Foundation Python validator + UI Page validator | Passed: Foundation 0 warnings；Page Valid=True, Issues=0 |
| Unity compilation | MCP 编译与 Console | Passed: Unity 2022.3.62f3c1 + MCP 10.3.0，Console 0 errors / warnings |
| Focused tests | Home/继续/重开、重复往返、首启、旧暂停和输入释放 | Passed: 5/5 主界面 PlayMode；重复生成 1/1 EditMode；全量 EditMode 27/27 + PlayMode 36/36 |
| Runtime or asset behavior | 1080×1920与窄竖屏截图；字体、绑定、隐藏3D、Resources读回 | Passed: 1080×1920、1170×2532 截图已目视；按钮边界在屏内；Fredoka 静态字库无缺字；Home 摄像机 mask=0、HUD隐藏；Start 恢复；花朵 27°往返不变 |
| Performance | 缓存/监听/画面隐藏审查 | Passed static review：Resources加载缓存，每场景页面1个，销毁注销事件；Home跳过关卡Update；iPhone真机性能未测 |

## Definition of Done

- [x] Ownership and QFramework roles match implemented files.
- [x] Public behavior and acceptance criteria implemented.
- [x] UI, scene, loading and Inspector impacts handled.
- [x] Convention validation and Unity compilation pass.
- [x] Tests and exact runtime checks pass.
- [x] Performance and device limits reported.
- [x] Verified only after evidence exists.

## Decision log

- 2026-10-11：用户明确选择继续当前进度；主界面默认首启入口，设置返回使用同场景页面覆盖以保留进度；Shop/Gallery 内容不在本次范围。

## Implementation and verification evidence

- Page prefab: `Assets/Resources/Prefabs/UI/MainMenuPage.prefab`。Launch: `Assets/Scenes/MainMenu.unity`；second Build scene: `Assets/Scenes/Level01.unity`。
- Inspector: Navigation Canvas/Level/Camera 引用已绑定；设置 Home 启用并绑定运行逻辑；Start 绑定；Shop/Gallery 保留展示入口并禁用内容交互。
- Feature runtime tests: `Assets/Scripts/Game/Tests/Runtime/MainMenuFlowTests.cs`（5项）；repeat setup test: `Assets/Scripts/Game/Tests/Editor/MainMenuSetupTests.cs`（1项）。
- Full Editor job: `3a2af8a1c21444baaf82606cde0ed0fe`，27/27 Passed；full Play job: `f6c5a040a00d460dba55068e22d658aa`，36/36 Passed。
- RED→GREEN: 原 Home 禁用；首次场景加载旧触摸；字体重复生成。真实 uGUI Touch 释放回归覆盖首次场景与缓存页面路径。
- Fresh static reviewer found one Important initial-scene touch issue; fixed and covered by failing then passing regression. No remaining Critical/Important findings.
- Evidence: [测试与读回记录](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/main-menu/test-evidence.json)、[审查记录](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/main-menu/review.md)。
- Visual QA: [1080×1920](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/main-menu/主界面-1080x1920.png)、[1170×2532](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/main-menu/主界面-1170x2532.png)、[关卡返回主界面](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/main-menu/关卡返回主界面.png)。
- Supplied background copied intact; nav icon variants made transparent with imagegen. [素材生成提示词与来源](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/main-menu/icon-generation.json)。Original user assets unchanged.
- No pending Inspector assignment. Device safe area uses existing Foundation UiSafeAreaFitter; the two Game View portrait ratios were verified, iPhone hardware/thermal/frame timing remain unmeasured.

## 2026-10-19 Quit 行为更新

关卡内 Quit 的当前行为以 [leave-level-confirmation](leave-level-confirmation.md) 为准：先确认，Leave 扣一点体力、重置当前关卡后返回 Home；下次 Start 从本关初始进度开始。取消确认保留进度。本文此前关于 Quit 保留进度的记录为历史行为；内部 ReturnHomeCommand 仍可用于保留会话。
