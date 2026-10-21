---
id: main-menu-shop-gallery
title: 主界面Shop与Gallery展示及九关分页
status: Verified
owner: Project
created: 2026-10-13
updated: 2026-10-13
---

# 主界面Shop与Gallery展示及九关分页

## Goal

将现有底部Shop/Home/Gallery导航接入用户给定的页面背景和Gallery关卡卡片，保留Home与当前关卡进度导航。

## User-visible behavior

- Shop使用商城背景图V3.png，当前仅背景和Shop标题，无商品或购买功能。
- Gallery使用商城背景图.png。第一页三列三行，逐行从左到右level 1–9；卡片底部仅文字说明，无关卡点击或加载功能。
- 只有level 1在凹陷区域展示关卡一.png，其他卡片留空。第二页暂时不放卡片或文字；提供左右翻页按钮、两页指示及水平滑动。
- 底部导航持续可见；Home回原背景与Start Game。打开设置时禁止切页；关闭、后台恢复和切换后等待旧指针释放，避免误触。
- 从关卡设置Home返回时默认Home页。再次进入Gallery回第一页。Start Game继续现有进度。

## Out of scope

商品/IAP/广告、金币奖励、新关卡数据、level 2–9预览图、关卡卡片交互、存档变更、模型动画、Foundation改动。

## Ownership and architecture

| Responsibility | Owner | Type | Notes |
| --- | --- | --- | --- |
| State mutation | Project Command | SelectMainMenuTabCommand / SetGalleryPageCommand | 仅菜单内导航 |
| State | Existing Model | MenuNavigationModel | 内存Tab和GalleryPage；无存档 |
| Notification | Project Event | MainMenuTabChangedEvent | 不重触发关卡Home/Start导航事件 |
| Presentation/input | ViewController | MainMenuTabsController + MainMenuPage | 初始化缓存依赖，复用设置输入门禁 |

## State and persistence

MenuNavigationModel增加SelectedTab和GalleryPage；初始化及ReturnHome重置为Home/0，切入Gallery重置第一页。命令只在IsHome状态允许合法Tab及0/1页码。不修改HarvestModel或存档schema。

## UI, prefab, scene, and Inspector impact

- 扩展原Assets/Resources/Prefabs/UI/MainMenuPage.prefab，保持Resources键Prefabs/UI/MainMenuPage和现有设置嵌套实例、按钮、字体及尺寸。
- 四张直接引用图片位于Assets/Sprites/UI/MainMenu/：ShopBackground.png、GalleryBackground.png、GalleryCard.png、Level01Preview.png。两张背景和关卡一预览原文件复制，卡片仅image_gen外部白底透明处理；提示词保留在证据目录。
- 一套缓存UI：ShopContent、GalleryContent、GalleryFirstPage九张非交互卡片、Pager、原导航选中底盘复用。卡片图片凹陷区域使用圆角裁切与aspect fit，保持花朵完整。
- MainMenuTabsSetup.Apply提供可重复的原生构建，MainMenuSetup.Apply保存基础界面后调用它，保证重建主界面后Tabs绑定完整。FredokaBoldSDF补烘焙缺失的v，仍保持Static模式。
- 所有新增Inspector引用通过Unity API保存并读回。无Scene结构改动，无点击时加载/实例化；销毁移除监听与事件订阅。

## Integrations and failure behavior

纯离线，无SDK。缺引用初始化明确报错。设置、后台、旧触摸阻止导航与分页；二页边界不越界；场景重载重新初始化，不残留旧事件。Gallery卡片及level文字不接受点击。

## Performance considerations

9张卡片共用同一Sprite，4张贴图均无mipmap且关闭Read/Write；初始化一次缓存，导航时只切可见性/背景/选中底盘，不加载资源。Editor Profiler记录四张新贴图合计32,804,440 bytes（约31.28 MiB，含Editor/驱动开销），不代表真机峰值；已检查1080×1920实际渲染画面。未进行真机触控、打包及GPU帧时采样。

## Acceptance criteria

- [x] 三个Tab正确切换指定背景并保留Start Game进度。
- [x] Gallery第一页9张卡片顺序正确，level 1花朵完整嵌入，卡片无按钮功能。
- [x] 第二页为空，左右按钮/水平滑动边界正确。
- [x] 设置/后台/旧指针不导致切页或加载关卡。
- [x] 场景重载、重复显示、设置关闭和资源生命周期正常。

## Verification matrix

| Boundary | Evidence | Result |
| --- | --- | --- |
| Behavior | MainMenuTabsTests(5) + VictorySettlementTests(7), focused-final-tests.json | 12/12 focused + 65/65 full PlayMode passed |
| Compile/conventions | Fresh Console + project validator | 0 errors / 0 warnings |
| UI/resources | native-readback.json + shop.png / gallery-page-1.png / gallery-page-2.png | GUID unchanged, 1 controller, 9 cards, 0 card buttons, validator 0 issues |
| Lifecycle/performance | Held touch, settings, background, two reloads, two setup runs, read-only review | Passed; no per-click loading/instantiation |

## Definition of Done

- [x] 项目命令/状态所有权正确，无Foundation改动。
- [x] 验收项及失败恢复完成。
- [x] 原生保存并验证Prefab、全部引用及显示。
- [x] 规范、编译和相关回归通过。
- [x] 性能和设备检查范围记录。
- [x] 有证据后标记Verified。

## Decision log

- 2026-10-13: 根据唯一关卡预览素材，level 2–9保留空图片区域；第二页空白。原底部Tab流扩展，shop不增加经济行为。

## Verification notes

- Evidence directory: /Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/main-menu-tabs/.
- Initial 4 tests failed because tabs were disabled, then passed. Review added a fifth held-finger Home-entry test; RED confirmed immediate input activation; Show now uses the existing pointer-release gate.
- A first version of that new test disabled EventSystem before the cached menu popup first Start, causing a test-fixture NullReferenceException; corrected to keep EventSystem present while disabling its input module.
- An intermediate full run had one unrelated Victory Continue timing assertion under editor load (animation clamps delta to .05 while test waits .65 real seconds); the isolated full victory group passed 7/7. The final full run passed 65/65 (runtime-final-tests.json); no victory code changes.
- Scope audit: scenes, prefab meta/GUID, gameplay/animations, settings controller and Foundation packages retained. Only existing MainMenuPage prefab, four existing menu/model/command/setup scripts and missing font glyph changed, plus project-owned feature scripts/test/art/spec.
