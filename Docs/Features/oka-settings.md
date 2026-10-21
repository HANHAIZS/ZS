---
id: oka-settings
title: 欧卡风格设置页面与弹窗
status: Verified
owner: Project
created: 2026-10-14
updated: 2026-10-14
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
