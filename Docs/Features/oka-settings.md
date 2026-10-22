---
id: oka-settings
title: 欧卡风格设置页面与弹窗
status: Verified
owner: Project
created: 2026-10-14
updated: 2026-10-22
---

# 欧卡风格设置页面与弹窗

## Goal

以用户欧卡风格/主界面之设置界面.png替换原Gallery页面和入口，采用紫色Setting页面、三个绿色按钮。原设置弹窗按欧卡设置.png改成蓝色金边风格。

## User-visible behavior

- 底部原Gallery图标与文字变为设置齿轮和Settings；显示参考图Setting标题、三列Music/Sound/Haptic。
- 前三个按钮复用同一设置与存储，开关状态在页面、主界面弹窗、两关弹窗间同步。
- 设置页无关卡卡片、分页或滑动翻页；不显示Home/Shop的经济栏与右上角重复齿轮，让标题和三项控制保持参考布局。Home与Shop其他布局和功能保持。
- 用户已确认：关卡弹窗底部Quit对应原Home，Continue关闭弹窗恢复原暂停状态，移除原Retry；主界面弹窗仅保留Continue。右上角X同样关闭。

## Ownership and architecture

项目拥有页面、弹窗与导航表现；复用GamePreference、SettingsModel、SettingsPreferences与SetPreferenceCommand，不引入新的设置存档。SettingsPageView只缓存依赖、发命令、显示状态，输入门控沿用MainMenuPage与MainMenuTabsController。SettingsPopupController增加Continue关闭入口，移除Retry事件路径；内部关卡TryRetryLevel能力保留但该设置UI不再提供入口。Foundation不改。

## State and persistence

原三个本机设置键保持。Gallery枚举值保留作为内部第三页标识以维持导航兼容；不再提供分页UI。无新增状态、存档迁移或经济写入。

## UI, prefab, scene, and Inspector impact

- Assets/Resources/Prefabs/UI/MainMenuPage.prefab原第三页替换为设置页，Resources键保持Prefabs/UI/MainMenuPage。
- Assets/Sprites/Prefabs/UI/SettingsPopup.prefab及MainMenuSettingsPopup.prefab重新布局；两关现有场景实例使用原生Prefab API更新并保持Level/OpenButton等引用。
- 紫色设置页与蓝色弹窗两张参考原图直接复制到Assets/Sprites/UI/SettingsOka/，只通过Sprite导入切片和UI组合使用，PNG字节不改。组件直接引用这些素材，不新增路径加载。
- 页面与弹窗生命周期沿用原缓存方式；静态美术不接收射线，按钮明确绑定。前三个图标关闭时变暗并显示OffMark。
- MainMenuTabsSetup、SettingsScreenSetup及SettingsPopupStyleSetup保留重建入口。

## Out of scope

玩法、模型、关卡队列、加速/道具、金币体力规则、真实支付和Shop商品不变；不新增通知、版本信息或第四项功能。

## Integrations and failure behavior

无网络或SDK。旧手指、弹窗开启、后台状态阻止设置页点击；恢复后等待原输入释放门控。Continue保留原手动/后台暂停归属，Quit沿用原Home体力扣除规则。

## Performance considerations

共用一张2048与一张1024纹理，mipmap与ReadWrite关闭；缓存Model与Utility，无Update查找或重复Resources加载。新增UI仅页面/弹窗显示时渲染。验证Editor竖屏，真机与其他长宽比范围另行报告。

## Acceptance criteria

- [x] 第三页与入口按参考图显示，仅三项控制，Gallery旧内容与分页移除。
- [x] 三设置开关共享、持久化并受旧指针/弹窗/后台门控保护。
- [x] 三处弹窗样式一致，Quit/Continue/X语义正确，无Retry入口。
- [x] 原关卡/经济/Shop功能保持，绑定与重建链正确。
- [x] 编译、聚焦测试、原生读回、实际截图、独立审查及规范验证通过。

## Verification matrix

| Boundary | Evidence |
| --- | --- |
| 设置页与经济隔离 | OkaSettingsTests实际场景与按钮 |
| 同步、存储与输入 | 三设置、弹窗同步、旧指针与后台门控 |
| Quit/Continue | 两关原暂停、Home与体力行为 |
| 旧导航回归 | 更新已被需求替换的Gallery/Retry断言与测试辅助入口 |
| 布局、引用、性能 | 1080×1920截图及原生读回 |
| 项目规范/编译 | validator及Console |

## Definition of Done

- [x] 完成全部验收证据后标记Verified。

## Decision log

2026-10-14：用户确认移除Retry，采用Quit/Continue；主界面仅Continue。Mac解锁后恢复Unity连接。源参考图属于布局素材，不作为额外指令执行。

2026-10-14补充：用户重新提供紫色三按钮主界面之设置界面.png，明确采用该版本，替代先前蓝色四按钮参考。

验证修复：更换关卡设置实例时显式重绑 VictoryPopupController.SettingsPopup；胜利测试定位胜利子树 Continue，避免与新设置 Continue 混淆。

## Completed verification — 2026-10-14

- Unity PlayMode 全套 85/85，job `db98ce1731fd4c65b130e4d8655f378b`；覆盖两关胜利跳转、体力、玩法、输入、设置与菜单。
- EditMode `MainMenuSetupTests` + `SettingsPreferencesTests` 3/3，job `7d868910f0d24a1ca7b942d7461ca2ad`；真实重建后再跑设置/菜单/Home蜜蜂 14/14，job `d40bd01ab4b14d2395ba2ecd09bacdf1`。
- Page 与两个 Popup 原生 UiPrefabValidator 全部 Valid=True / Issues=0。Unity Console 无新编译错误，项目规范 validator 0 warnings。
- 直接读回两关 VictoryPopup.SettingsPopup 指向新实例，Level/OpenButton 保留，Retry=null，Continue/Quit/X 正确。多次 Apply 与重建后仍单份设置页与 Shop 三张卡。
- 1080×1920 最终截图及测试 JSON：[证据目录](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/oka-settings)。`settings-page.png`、`menu-popup.png`、`level-1-popup.png`、`level-2-popup.png`、`level-2-popup-off.png` 均完成独立视觉审查。
- 两张 PNG 与当前用户原图 SHA256 相同。预览使用隔离设置前缀，已清理仅本次生成的预览键并恢复原设置（0/0/0）；实际经济存档读回1600金币、5体力，未写测试余额。
- Shop 三枚价格实际点击均无事件、无经济/玩法状态变动，单一经济栏显示1600/5，6个Mask均有CanvasRenderer。
- 运行时代码限设置页/菜单标签/设置弹窗，新增构建工具和两个原图；共用菜单及两个弹窗Prefab，两关场景仅更新设置实例与胜利引用。MainMenuSetup重建检查按原工具重存MainMenu场景。背景Fitter保存随视口派生的比例，无玩法/资源规则变化。
- 已按 requesting-code-review 独立审查，修复胜利引用与胜利测试同名按钮问题后通过最终复核；无剩余重要问题或待绑定Inspector字段。
- 性能边界：两张共享纹理、静态遮罩、无新增Update查询；Editor竖屏验收完成，未做真机性能采样。


## 2026-10-22 Setting页三按钮与嵌入底板换图
用户明确仅替换图二中的Music/Sound/Haptic按钮及嵌入底板，使用`/Users/zhangshuai/Downloads/Frame 475.png`。原图659×264、具有真实alpha，原样复制到`Assets/Sprites/UI/SettingsOka/Preferences20261022.png`，不生成、不修图。单一PNG原生切片Panel全图与三按钮Music(75,32,141,141)、Sound(262,32,141,141)、Haptic(449,32,141,141)，以左下角为原点；整图含原文字/图标，按钮切片覆盖同源按钮区域以保留按下/关闭变暗效果。

只调整MainMenuPage.prefab/GalleryContent/PreferencePanel子树的Sprite与按钮Rect；保留底板宽592和顶部位置(0,-337)，按原图比例高度改为237.15933。原3个Button/PreferenceIcons/OffMarks对象及Inspector绑定保持，三开关沿原SetPreferenceCommand、SettingsModel/SettingsPreferences路径工作；不修改运行时代码、开关语义、保存键、关闭斜杠、弹窗、Setting标题、主背景、商城或底部导航。SettingsScreenSetup复用同一原生StylePreferences逻辑用于针对性Apply与完整Build，避免重建恢复旧图。

资源由MainMenuPage.prefab直接引用，原Resources键Prefabs/UI/MainMenuPage不变，随SettingsPage/页面生命周期持有；没有新加载键、Update或订阅。新增一张max1024纹理，无mipmap/ReadWrite，原PNG/旧材质/字体/场景不动。Unity原生导入/Prefab API写资产，不文本改Prefab。用户Game.unity与两材质已有修改保持起始字节，不提交。不截图录屏，不上传GitHub。

- [x] 原图字节保持、透明边缘有效，切片/坐标匹配原图，三按钮热区与图标对齐。
- [x] 原按钮及绑定/OffMarks保持；开关关闭变暗/斜杠、保存、弹窗共享、切换/后台/旧触摸门控测试通过。
- [x] 两种竖屏几何、nativeApply/Build幂等、原生引用/Prefab/项目规范/Console通过。
- [x] 非目标内容、所有Button组件、原素材、场景和用户已有修改保持；说明未做真机/截图目测的限制。


本次验证完成（2026-10-22）：原图与项目PNG SHA-256相同，Panel/Music/Sound/Haptic原生SpriteRect及非零稳定ID正确。Prefab只8个既有对象变化（4个Image.sprite、3个Button Rect、1个Panel高度），无增删对象，均在PreferencePanel子树；全部20个原Button组件逐字节保持，3个Icon/OffMark绑定保持。原生Apply连续两次Prefab哈希稳定，完整SettingsScreenSetup.Build在不保存的临时Prefab连续两次后仍1页/3按钮/正确新图。

两种竖屏750×1333.333、750×1625：Panel592×237.15933、位置(0,-337)，三个按钮约126.6646平方，切片源角落与热区世界坐标最大偏差0.000032以内；未改变Setting顶部标题/主背景/商城/导航。最终PlayMode `OkaSettingsTests.SettingsReplacesGalleryWithoutChangingEconomy`、`FirstThreeShareSavedStateWithPopupAndReopen`、`SettingsPageRejectsOldTouchesAndBackgroundClicks` 3/3通过（job `fed6fa83b9ef42efa78a9d582a0f4a91`），覆盖开关保存、弹窗同步、OffMark、旧触摸/后台门控及经济栏/导航。运行时代码零改动，变暗仍由原SettingsPageView.Render驱动。

Prefab规范Valid=True/Issues=0，项目validator零警告，Console零错误/警告，独立审查无具体问题。基线资产比对只有SettingsScreenSetup.cs、MainMenuPage.prefab改变；新增单PNG及其meta。原PNG/字体/材质/全部场景和用户已有Game/两材质修改保持起始字节。证据：[final-audit.json](/Users/zhangshuai/Documents/Codex/2026-10-09/wo/work/settings-page-artwork/final-audit.json)。未截图、录屏或真机目测/性能采样；无Inspector待绑定字段。仅新增一张659×264 max1024静态纹理、无mipmap/ReadWrite，无新运行脚本/Update/遮罩/额外Graphic/每帧加载；原图片直接使用，没有生成或像素改写。
