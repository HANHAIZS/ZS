---
id: settings-background-v3
title: 主界面与关卡设置背景V3替换
status: Verified
owner: Project
created: 2026-10-13
updated: 2026-10-13
---

# 主界面与关卡设置背景V3替换

## Goal

使用用户提供的设置界面V3.png替换主界面和关卡共用的设置背景。此次仅修改图片和必要的透明裁切导入信息；按钮、文字、布局、交互、状态与存档不变。无行为修改、无新代码、无Foundation修改。

## User-visible behavior

主界面和关卡内设置显示新金棕色背景；各自现有文字、控件、大小和交互保持。

## Out of scope

代码、功能、存档、按钮、字体、布局、关卡与Foundation均不改动。

## Ownership and architecture

- 输入：/Users/zhangshuai/Documents/UI素材V2/设置界面V3.png（RGB白底）。通过内置image_gen移除外部白底并保留金棕色面板。提示词及工具记录位于outputs/settings-art-v3/imagegen-record.md。
- 替换：Assets/Sprites/UI/Settings/SettingsPanel.png及.meta。保留GUID、Sprite local ID和九宫格border；裁切区域调整为Rect(20,127,1498,804)，边框完整。
- 共用直接引用：Assets/Sprites/Prefabs/UI/SettingsPopup.prefab、MainMenuSettingsPopup.prefab及Assets/Resources/Prefabs/UI/MainMenuPage.prefab。预制体和场景文件本身未修改，Level01和MainMenu均使用新背景。

## Acceptance criteria

- [x] 实际打开关卡内设置：背景更新，620×580布局保持，三项开关及Retry/Home保留。
- [x] 通过Home返回主界面并打开设置：背景更新，620×410布局保持，仍只有三项开关及关闭按钮；关闭成功。
- [x] 两个Popup原生规范验证Valid=True、Issues=0；Unity Console无错误，项目规范验证0警告。
- [x] GUID和Sprite local ID保持，三处引用正确，无白色方框或被裁掉的金色边框。
- [x] 69份脚本hash保持，两个设置预制体、MainMenuPage及两场景文件字节保持；仅图片及其导入信息变化。

证据：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/settings-art-v3/，包含level-settings.png、menu-settings.png、change-scope.json、native-readback.json。仅素材替换，未新增测试或重复运行完整游戏回归。

## Verification matrix

| Boundary | Evidence | Result |
| --- | --- | --- |
| Both popup displays | level-settings.png / menu-settings.png | Passed |
| Shared references and native import | native-readback.json | Passed |
| Scope preservation | change-scope.json | Passed |
| Conventions and Console | Native Popup validators + project validator | Passed |

## Definition of Done

- [x] 两处背景替换并保留Sprite身份。
- [x] 控件、脚本、预制体及场景保持。
- [x] 实际画面、原生引用和项目规范检查完成。

## Performance and device scope

仍为一张共用1024×683纹理，Read/Write关闭、mipmap关闭；无新对象、资源加载或运行时代码。已核对Unity Editor 1080×1920画面，未进行真机或其他比例检查。
