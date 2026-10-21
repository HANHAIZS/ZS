---
id: background-art-v3
title: 两关与主界面背景素材替换
status: Verified
owner: Project
created: 2026-10-14
updated: 2026-10-14
---

# 两关与主界面背景素材替换

## Goal

两个关卡背景使用用户提供的游戏关卡背景图.png；主界面Home背景使用主页背景图V2.png。仅替换图片，其他布局与功能保持。

## User-visible behavior

第一关与第二关显示深蓝渐变背景，Home显示新的花园背景。沿用现有等比覆盖方式，不拉伸；按钮、模型、金币、体力、设置、Shop与Gallery保持原有行为。

## Ownership and architecture

项目美术资产替换；不修改运行时代码、状态、Command/Model/System、Foundation或存档。直接复制用户原图，保留原始输入和现有资产身份。

## Resource and Inspector impact

- Assets/Sprites/Gameplay/Backgrounds/Level01HoneyGarden.png：两关LevelBackgroundView.BackgroundTexture与共用材质的_BaseMap直接引用。原材质与两个场景引用保持。
- Assets/Sprites/UI/MainMenu/Background.png：MainMenuPage.prefab的MainMenuTabsController.HomeBackground与Home背景Image直接引用，运行时实例沿用。
- 无新增Resources键、对象或生命周期职责；GUID、本地引用身份、原导入配置保持。最大导入尺寸2048、mipmap关闭、Read/Write关闭、压缩纹理。

## Out of scope

玩法、UI位置与大小、蜂巢、模型、动画、关卡数据、设置、其他页面、经济与存档不变。

## Acceptance criteria

- [x] 两个关卡与Home正确显示指定背景，等比覆盖全屏。
- [x] 原引用身份、导入配置与全部源代码、场景、预制体、关卡数据保持。
- [x] Shop、Gallery背景保持。
- [x] Unity Console与项目规范检查通过。

## Verification matrix

| Boundary | Verification |
| --- | --- |
| 原图对应 | 资产PNG与用户原图SHA256一致 |
| 引用与配置 | Unity原生读回三个页面与材质的引用、纹理尺寸、GUID、Sprite身份 |
| 实际显示 | 1080×1920三页运行截图 |
| 修改范围 | 修改前后资产哈希对比 |
| Console与项目规范 | 原生Console和项目validator |

## Definition of Done

- [x] 验收完成后标记Verified，保存证据与检查范围。

## Performance and device scope

沿用一张共用关卡纹理和一张Home纹理，无新增对象或运行时代码。Unity Editor竖屏核对；未做真机测试。

## Completion evidence

证据保存在/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/background-art-v3/。


- main-menu.png、level-one.png、level-two.png：三个页面1080×1920原生运行截图已逐张核对，指定背景等比铺满。
- change-scope.json：655项资产中仅两个目标PNG变化，另653项保持。两个PNG与用户原图SHA256相同；原GUID、Sprite local ID、导入配置、场景、预制体、材质、代码、关卡数据、其他美术均保持。
- native-readback.json：两关视图与材质使用同一1165×1350新纹理，显示比例0.862962961与图片比例一致。主界面Home使用1024×1536新Sprite，GUID72f239eababae4770afdc460074075c8及local ID21300000保持。
- 主界面通过现有Start Game按钮正常进入第一关。预览未点击Home/Retry/经济增加按钮，实际金币1600、体力5保持。
- Unity Console零错误，项目规范验证零警告。未新增或重新运行自动化测试；本次纯素材替换使用实际画面、引用与哈希校验覆盖边界。
