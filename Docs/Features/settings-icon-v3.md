---
id: settings-icon-v3
title: 主界面与两关设置图标V3替换
status: Verified
owner: Project
created: 2026-10-14
updated: 2026-10-14
---

# 主界面与两关设置图标V3替换

## Goal

将主界面、第一关、第二关右上角设置图标替换为用户提供的设置V3.png。仅图片及必要的透明裁切导入信息变化；所有功能、按钮位置、大小和绑定保持。

## User-visible behavior

主界面与两个关卡右上角显示新的蓝色齿轮金边设置图标；点击仍打开原设置弹窗，其他功能、布局和状态保持。

## Ownership and architecture

项目拥有美术替换，无运行时代码、状态、Command/Model/System、存档或Foundation修改。输入RGB白底图片经内置image_gen做透明底提取，原始输入不变。直接引用资产保留路径Assets/Sprites/UI/Settings/SettingsGear.png、GUID及Sprite身份。

## Resource and Inspector impact

MainMenuPage.prefab及Level01.unity、Level02.unity的设置按钮直接引用同一Sprite。保留80×80大小、右上角锚点及(-60,-60)位置。不创建新Resources加载键，不增加对象、运行时加载或生命周期职责。保留256最大导入尺寸、mipmap关闭、Read/Write关闭。

## Out of scope

弹窗、音效、开关、Home/Retry、金币/体力、关卡、布局与源代码不变。

## Acceptance criteria

- [x] 三个页面显示同一新蓝色齿轮金边图标，透明边缘无白色方框。
- [x] 位置大小、Sprite身份和设置功能保持。
- [x] 现有源码、预制体和场景字节哈希保持；只有素材和导入信息更新。
- [x] 原生引用、实际运行显示和设置打开关闭通过；Console与规范验证通过。

## Verification matrix

| Boundary | Evidence | Result |
| --- | --- | --- |
| 引用与身份 | settingsV3NativeFinal.json | Passed |
| 三页实际显示与设置交互 | 原生截图与运行检查 | Passed |
| 源码与序列化对象保持 | change-scope.json | Passed |
| Console与项目规范 | 检查输出 | Passed |

## Definition of Done

- [x] 验收证据保存后标记Verified，报告资产路径、提示词和检查范围。

## Performance and device scope

保留现有一张共用256纹理，不增加渲染对象或运行时代码。验证Unity Editor竖屏；未做真机测试。

## Completion evidence

证据目录：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/settings-icon-v3/。

- main-menu.png、level-one.png、level-two.png：1080×1920实际运行画面，蓝色齿轮金边完整、无白底。
- 三处实际打开关闭设置均成功；10/10既有设置PlayMode测试通过，job b00734f0ca1d43a5a44e2b27a96068df。
- Sprite GUID 4bd6bffc226984868abffbc66ae80571、local ID -3420385633532392368保持；主界面预制体及两关共用直接引用，按钮80×80、位置(-60,-60)保持。
- change-scope.json：117份源码、预制体、场景文件哈希保持，无新增运行时代码或对象。
- Unity Console零错误，项目规范检查零警告。
- 仅替换SettingsGear.png及原生调整其透明Sprite裁切；输入图片未修改。内置image_gen的最终提示词、保存路径和模式见imagegen-record.md。

