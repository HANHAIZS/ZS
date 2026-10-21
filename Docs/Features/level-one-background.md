---
id: level-one-background
title: 第一关蜂蜜花园背景
status: Verified
owner: Project
created: 2026-10-10
updated: 2026-10-10
---

# 第一关蜂蜜花园背景

## Goal

使用用户提供的1024×1536 PNG 替换 Level01 深蓝纯色背景，保留当前场景和玩法，等比例铺满不同竖屏比例。

## User-visible behavior

- 原图在花朵、蜜蜂、蜂巢和控件之后显示；保持图片比例，超出视口的部分居中裁切。
- 背景不接收点击、不遮挡三维采集对象；关内浅色区域上的文字改为深棕色，重开按钮保留白字。
- 重新打开场景和运行游戏均能加载同一背景。

## Out of scope

其他页面、关卡、玩法、额度、模型、经济系统、重新生成或绘制图片。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | None | 无 | 不改变游戏状态 |
| Observable or persistent state | None | 无 | 无存档影响 |
| Shared project rules | None | 无 | 采集规则保持现状 |
| One-shot notification | None | 无 | 无新增事件 |
| Unity presentation | ViewController | ViewControllers/Harvest/LevelBackgroundView.cs、LevelOneHud.cs | 继承 BeeHarvestController；按相机尺寸适配背景与文字颜色 |
| Adapter or repository | Editor tooling | Editor/LevelOneBackgroundSetup.cs、LevelOneSceneSetup.cs | 使用 Editor API 配置纹理、材质和场景；增量修改当前场景 |
| Reusable infrastructure | None | 无 | 不修改 Foundation |

## State and persistence

- Runtime state: 仅展示尺寸缓存；无业务状态写入。
- Save schema/version/migration: None。
- State mutation path: 本功能不写 Model/System/Command。

## UI, prefab, scene, and Inspector impact

- Affected assets: Assets/Scenes/Level01.unity；背景 PNG／材质；三个展示／编辑器脚本及一个重建钩子。
- Loading mode and final paths: 直接引用 Assets/Sprites/Gameplay/Backgrounds/Level01HoneyGarden.png 和 Level01HoneyGarden.mat，不使用路径加载。
- Resources keys or direct reference owners: None；材质引用 Texture2D，LevelBackgroundView 引用相机和 Texture2D。
- Load/cache/instance cleanup responsibility: 场景管理背景对象；尺寸改变时更新缩放，没有额外资源加载。
- Hierarchy or binding changes: LevelOneCamera/LevelBackground；正交相机后方50单位、Quad Renderer、无 Collider；URP Unlit 材质，保留现有 UI 绑定。
- Inspector confirmation: 检查 PNG 参数、材质纹理引用、相机、背景位置、尺寸比例、控件颜色和场景持久化。

## Integrations and failure behavior

- SDK/consent/lifecycle: None。
- Content update/offline/rollback: 本地纹理；无网络。修改前保存当前场景，并在工作输出目录备份。
- Cancellation, timeout, retry: 无加载等待；纹理缺失时不计算尺寸，相机底色仍可用，不中断玩法。

## Performance considerations

增加一个 Unlit Quad 绘制与一张非 Read/Write、无 Mipmap 的本地纹理。每帧仅比较缓存参数；相机比例／尺寸变化才更新 Transform，无逐帧架构查询或资源加载。

## Acceptance criteria

- [x] 第一关正确显示用户原图；3D 模型和控件完整可见，文字可读。
- [x] 9:16 与接近9:19.5的竖屏均覆盖视口，无变形与露边。
- [x] 点击蜂巢能继续采集，背景不会拦截输入。
- [x] 保存并重开场景后背景仍存在。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | 通过，0 warnings |
| Unity compilation | Error 控制台 | 0条 error |
| Focused tests | 低影响背景换图不新增测试；运行既有场景用例验证交互 | ButtonsCarryDeliverAndRestart 1/1通过，job 1cf3829523b04f329371862167ac53ce |
| Runtime or asset behavior | 9:16／长屏截图，纹理／相机／材质读回，重新加载 | 两比例覆盖宽高 true，图片比例2:3；重加载后背景1个、纹理1024×1536；已保存两张截图 |
| Performance | 检查纹理配置、单背景对象、缓存适配 | 单 Unlit Quad，无 Collider，Mipmap/ReadWrite关闭，最大纹理2048，NPOT=None，尺寸变化才更新 |

## Definition of Done

- [x] Ownership and QFramework roles match implemented files.
- [x] Public behavior and acceptance criteria implemented.
- [x] Scene/art/Inspector impact handled with Editor API.
- [x] Convention validation and Unity compilation pass.
- [x] Exact runtime and asset checks pass.
- [x] Performance and manual platform limits reported.
- [x] Status updated to Verified after evidence exists.

## Decision log

- 2026-10-10: 用户指定使用附件换关卡背景；直接使用原图，等比例居中铺满。
- 2026-10-10: 只增量修改现有场景并保存用户当前修改，避免重建整关覆盖已有配置。

## Verified evidence

原图以字节相同文件导入；保留1024×1536，关闭默认幂次缩放避免拉伸。背景位于主相机子节点 LevelBackground、局部 Z=50、材质 Universal Render Pipeline/Unlit。9:16和9:19.5均保持2:3图片比例、覆盖视口宽高。

预览归档：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level-one/background-change/preview-9-16.png 与 preview-long.png。修改前场景备份在同目录 Level01-before-background.unity。编辑器已恢复停止运行状态并重新加载保存后的 Level01，Inspector 引用已确认；不在此轮声明新增真机性能验证。
