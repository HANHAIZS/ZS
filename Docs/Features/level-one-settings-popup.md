---
id: level-one-settings-popup
title: 第一关设置按钮及棕金色设置弹窗
status: Verified
owner: Project
created: 2026-10-10
updated: 2026-10-10
---

# 第一关设置按钮及棕金色设置弹窗

## Goal

用用户指定七张图片，为第一关增加设置入口及可操作的设置弹窗；字体全部采用Fredoka Bold，顶部文字精确为setting，布局参考用户最后一张图。

## User-visible behavior

- 右上角棕金色齿轮按钮打开设置。背景变暗，采集与花朵旋转暂停；右上角关闭按钮关闭弹窗，恢复打开前的暂停状态。
- 弹窗使用用户提供的棕色面板及金色标题条；顶部setting。中部横排Music、Sound、Haptic三个圆形图标，下方Retry绿按钮、Home红按钮并排。
- 三项开关默认开启，关闭时图标变暗并显示红色斜杠。本地记录选择，重开／重新运行保留。没有现成音频资源，本次不创作BGM或音效；开关保存为后续声音播放的设置。
- 用户2026-10-10追加：删除关卡底部原RestartLevel按钮及“重开本关”文字；重开入口统一迁移到设置Retry。
- Retry关闭弹窗并重新开始第一关，清除旧飞行及未落稳的蜂巢跃动。
- 当前无主界面：Home先保留并置灰，用户未回复可选问题，采用默认范围；不新增主界面。若用户选择隐藏则只改布局/显示。
- 弹窗打开时中断未完成的蜂巢入槽跃动，不扣额度；关闭后等原指针全部释放再恢复蜂巢和旋转输入；遮罩拦截底层按钮，防止关闭／Retry之后遗留点击。
- 同一弹窗实例可重复开关；场景卸载注销Router，不产生重复监听或残留活动弹窗。

## Out of scope

主界面、后续关卡、实际音乐／音效制作、经济商城、联网、改变采集规则、升级Unity或MCP。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Command | Commands/Settings/SetPreferenceCommand.cs；现有PauseHarvestCommand/StartLevelCommand | 开关／暂停／重开只由Command写Model |
| Observable or persistent state | Model | Models/Settings/SettingsModel.cs | 三项布尔选项，无关卡进度存档变更 |
| Shared project rules | System | 现有HarvestSystem | 不新增共享规则 |
| One-shot notification | None | 无新增Event | 按钮后刷新弹窗 |
| Unity presentation | ViewController | ViewControllers/UI/SettingsPopupController.cs；现有LevelOneController/HarvestBoardView/HiveHopController | 缓存依赖，实现IUiPopupView |
| Adapter or repository | Utility | Utilities/Settings/SettingsPreferences.cs | 项目拥有三项本地偏好桥接 |
| Reusable infrastructure | Foundation | 复用UiPopupRouter | 不修改Foundation包，唯一BeeHarvestApp注册依赖 |

## State and persistence

- Runtime state: SettingsModel三项bool及Revision；弹窗持有打开前暂停状态、关卡Generation和已初始化依赖。
- Save schema/version/migration: PlayerPrefs名称空间BeeHarvest.settings.v1，Music/Sound/Haptic三项0/1，缺失值默认开启；0为关闭、非0为开启，兼容非法非0值。仅偏好，不修改玩法存档。
- State mutation path: Button→Controller→SetPreferenceCommand／PauseHarvestCommand／已有Restart入口。偏好写入由Command调用缓存／本次解析的Utility。

## UI, prefab, scene, and Inspector impact

- Affected assets: Assets/Scenes/Level01.unity；Assets/Sprites/Prefabs/UI/SettingsPopup.prefab；Assets/Sprites/UI/Settings/7张透明素材；Assets/Sprites/UI/Fonts/Fredoka-Bold.ttf及FredokaBoldSDF.asset。
- Loading mode and final paths: 直接引用UI美术和Prefab。原图备份Assets/Sprites/DesignImports/Settings；原始文件不修改。
- Resources keys or direct reference owners: SettingsPopup.prefab直接引用各Sprite及TMP字体；Level01场景实例引用Prefab；不使用Resources按路径加载这些UI图片，不使用YooAsset。
- Load/cache/instance cleanup responsibility: 场景实例缓存素材和组件；打开时不加载／实例化。场景退出释放实例、监听及Router注册；禁用注销，重新启用注册；后台拒绝新的设置请求。TMP固定图集预烘焙，运行期不依赖系统字体。
- Hierarchy or binding changes: 新增独立设置入口和SettingsPopup（根直接子节点DimBackground + PopupContent）；删除旧RestartLevel对象并将Hud.Restart引用置空，原重开入口由设置Retry接管。新增Runtime/Editor对应TMP/UI校验器asmdef引用。PopupContent含面板、标题、关闭、三开关、Retry/Home。
- Inspector confirmation: 七种图片绑定、Fredoka Bold、标题setting、三图标无亮度选项、点击关闭／Retry、默认Home不可用、SafeArea两种竖屏。PNG透明外部、内部高光保留。

## Integrations and failure behavior

- SDK/consent/lifecycle: 无新增SDK；Haptic设置作为后续手机震动门控，Editor不假装实机震动。
- Content update/offline/rollback: 全部离线／直接引用；源脚本和场景变更前备份。
- Cancellation, timeout, retry: 打开时取消入槽动画。重复打开不重复订阅；关闭仅恢复同一局原暂停状态。Retry清除旧关卡状态，Home不跳转空页面。打开时通过现有StandaloneInputModule.DeactivateModule清除之前的指针按压缓存，并由LevelOneController在LateUpdate中等BaseInput所有触点及鼠标释放后恢复底层输入；覆盖uGUI重建Stationary触点行为。

## Performance considerations

UI只在场景初始化缓存依赖，按钮事件刷新，不逐帧GetModel或加载资源。复用单弹窗；三项PlayerPrefs只在改变时保存。图标导入max256、按钮max512、面板max1024，无mipmap、无CPU可读副本；字体图集预烘焙。未验收iPhone实机帧率和触控。

## Acceptance criteria

- [x] 关卡内设置图标打开正确棕金色弹窗，七张图绑定对应元素，全部弹窗文字Fredoka Bold，标题setting，布局符合参考。
- [x] 开关能显示开启／关闭并保留本地选项；没有新增未提供的亮度选项。
- [x] 打开暂停、关闭恢复、Retry重开；遮罩阻挡底层，蜂巢跃动取消不扣额，Home保留并置灰。
- [x] 重复开关无重复监听，退出／重开清理正确；原第一关玩法回归通过。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | 已通过；0 warning |
| Unity compilation | 无新增C#错误 | Editor空闲，is_compiling=false；Console error=0 |
| Focused tests | 缺省／非法偏好、保存读取、弹窗暂停恢复／遮罩／开关／Retry／重复开关／原玩法 | EditMode15/15，PlayMode16/16；实际鼠标和Touch指针路径通过 |
| Runtime or asset behavior | Sprite对应／alpha／FredokaBold字重／Prefab规范、两种竖屏截图 | 7张透明素材；OS/2字重700；7个TMP标签统一FredokaBoldSDF；Prefab有效0问题；场景0问题；750×1334、750×1625目视通过 |
| Performance | 复用／缓存／图片尺寸／静态字体图集 | 单实例复用、依赖缓存；PNG无mipmap/不可读；静态512×512字体图集；实机性能未验证 |

## Definition of Done

- [x] Ownership and QFramework roles match implementation.
- [x] Public behavior and acceptance criteria implemented.
- [x] UI, scene, prefab and Inspector impact verified through Editor APIs.
- [x] Convention validation and Unity compilation pass.
- [x] Focused tests and exact runtime checks pass.
- [x] Review, unresolved Home assumption and platform limits recorded.
- [x] Status updated to Verified after evidence exists.

## Decision log

- 2026-10-10: 根据用户素材序号和参考图制作；顶部文字使用小写setting。Home行为异步询问；默认保留置灰以遵循当前只做第一关的范围。
- 2026-10-10: 原7张PNG均有不透明白底，使用内置imagegen逐张提取透明背景，保留原图，最终prompt集保存在工作区。
- 2026-10-10: Fredoka Bold使用Google Fonts元数据列出的官方上游静态Bold文件及OFL许可证，不以Fredoka One或伪粗体替代。

- 2026-10-10: 只读审查发现禁用入口/后台打开/旧按压问题；已加入活动状态与Router清理、后台拒绝、指针缓存清理的实现与回归。场景卸载旧Hop清理访问已销毁相机，HiveModelView.Sync增加Unity对象存活检查。

- 2026-10-10: 第二次只读审查指出清除缓存后旧Stationary触点会被uGUI重建为按下；加入实际Touch处理路径回归，并在关闭/重开后等待原触点全部释放再恢复底层输入。

- 2026-10-10: 按用户追加要求，删除关卡底部旧重开按钮，三组现有玩法回归改为经过设置Retry重开，原Command重置逻辑保留。

## Verified evidence

- EditMode job `0150184290bd47329fcceb6d18f13da2`：15项全部通过，0失败／跳过。
- PlayMode job `dfddbc368cf541f8b117e781b89e136c`：16项全部通过，0失败／跳过。
- Retry回归：重开恢复458方块，清空飞行和槽位；旧RestartLevel不存在且Hud.Restart=null。关闭/重开后等待旧触点释放，随后新点击正常入槽。
- Scene validation：totalIssues=0、missingScripts=0、brokenPrefabs=0；UiPrefabValidator：IsValid=True、Issues=0。
- 最后Console无Error；Play退出时MCP WebSocket产生一次传输Warning，后续MCP调用和验证正常，无游戏代码Warning。早期被编译重载中断的测试不计入通过结果。
- 独立只读代码审查完成，最终未发现需要修复的具体遗漏。
- 最新原生预览：Captures/settings-popup-retry-only-750x1334.png、Captures/settings-popup-retry-only-750x1625.png、Captures/level01-retry-only-750x1625.png。
- 预览与测试结果副本：工作区outputs/level-one/settings-ui。未制作后续关卡或主界面。Home置灰；音频播放及iPhone震动仍需后续资源／实机接入。
