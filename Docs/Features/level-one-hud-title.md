---
id: level-one-hud-title
title: 第一关标题样式与文字精简
status: Verified
owner: Project
created: 2026-10-10
updated: 2026-10-10
---

# 第一关标题样式与文字精简

## Goal

按用户指定样式将第一关标题改为 Level 1，并移除三个辅助文字对象。

## User-visible behavior

- 标题为 `Level 1`，Coiny 粗圆字体，填色 `#FFF6E5`，描边 `#5A3A18`，TMP SDF 描边宽度0.2。
- 字号从36增加15%到41.4，保持原有顶部居中位置。
- 删除计数 `0 / 458`、操作提示 `点击蜂巢出蜂 · 拖动旋转` 和标题 `五个工作槽`。
- 五槽采集机制、各槽额度、蜂巢选择、旋转和重开按钮保留。

## Out of scope

玩法、其他关卡或页面、模型、背景、更改蜂巢按钮或五个实际工作槽。删除的是用户截图列举的三项文字。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | None | 无 | 不改变 Command/Model/System |
| Observable or persistent state | None | 无 | 不涉及存档 |
| Shared project rules | None | 无 | 玩法保持现状 |
| One-shot notification | None | 无 | 不新增事件 |
| Unity presentation | ViewController | ViewControllers/Harvest/LevelOneHud.cs | 移除计数和提示绑定；继续只读绘制槽位和队列 |
| Adapter or repository | Editor tooling | Editor/LevelOneTitleSetup.cs、LevelOneSceneSetup.cs | 增量配置场景与重建钩子 |
| Reusable infrastructure | None | 无 | 不修改 Foundation 包 |

## State and persistence

- Runtime state: 无新增业务状态；标题使用静态字体图集。
- Save schema/version/migration: None。
- State mutation path: 仍由既有 Command 驱动；HUD 不写状态。

## UI, prefab, scene, and Inspector impact

- Affected assets: Assets/Scenes/Level01.unity；标题字体及 SDF 图集；Editor asmdef 添加 Unity.TextMeshPro 引用；既有场景测试改为检查已交付数量。
- Loading mode and final paths: 直接引用 `Assets/Sprites/UI/Fonts/Coiny-Regular.ttf`、`CoinyRoundedSDF.asset`；同目录保存 OFL.txt。安装 TMP 官方 Essential Resources 提供文字 Shader 和设置。
- Resources keys or direct reference owners: 标题 TextMeshProUGUI 直接引用字体和材质，字体资产拥有材质及512×512图集子资产；不新增业务 Resources 加载键。
- Load/cache/instance cleanup responsibility: 场景持有组件与字体引用，Unity 随场景管理实例；静态图集无需运行时生成字形。
- Hierarchy or binding changes: 将 SafeArea/LevelLabel 的旧 Text 替换为 TMP；删除 CollectedCount、Instruction、SlotTitle；删除 HUD 对前两项的引用与刷新。
- Inspector confirmation: 字体、41.4字号、填色、描边颜色／宽度；已删除对象不存在；现有槽位、按钮引用完整。

## Integrations and failure behavior

- SDK/consent/lifecycle: None。
- Content update/offline/rollback: 字体与图集随工程打包，标题无需系统字体或网络；场景和改动脚本已备份。
- Cancellation, timeout, retry: Editor 配置在缺字体／Shader 时明确报错，重复执行不会重复创建标题或图集。

## Performance considerations

标题使用一个 TMP 文字组件和512×512静态 Alpha8图集；不增加逐帧查找或动态加载，删除三个 Text 组件及其计数／提示刷新。

## Acceptance criteria

- [x] 保存并重新加载后，标题、字体、字号、填色和描边均与指定值一致。
- [x] 三个辅助文字对象在编辑态和运行态均不存在。
- [x] 竖屏预览标题可读，不裁切，现有按钮与核心玩法正常。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | 通过，0 warnings |
| Unity compilation | Error 控制台 | 编译成功；最终运行0 errors、0 warnings |
| Focused tests | 既有 ButtonsCarryDeliverAndRestart；低影响展示修改不新增实现镜像测试 | PlayMode 1/1通过，job 4ad482dc050f41a2b110275faa0d3a4f |
| Runtime or asset behavior | 保存重载、精确属性读回、9:16截图 | 标题Level 1、Coiny、41.4、FFF6E5、5A3A18、0.2；三个对象不存在；未截断；槽位和按钮各5个 |
| Performance | 单标题组件、静态图集、删除三项文字 | 单TMP标题、512×512静态图集，无新增逐帧查找；删除三项Text |

## Definition of Done

- [x] Ownership and QFramework roles match implemented files.
- [x] Public behavior and acceptance criteria implemented.
- [x] UI, assets and Inspector impact handled with Editor API.
- [x] Convention validation and compilation pass.
- [x] Focused tests and exact asset/runtime checks pass.
- [x] Performance and platform limits reported.
- [x] Status updated to Verified after evidence exists.

## Decision log

- 2026-10-10: 用户明确指定文字和样式；按这份已授权规格直接实施。五槽机制保留，删除截图中的“五个工作槽”标题。
- 2026-10-10: 采用 Coiny 的粗圆字形和 TMP SDF 描边；字号36×1.15=41.4。
- 字体来源：[Google Fonts Coiny](https://github.com/google/fonts/tree/main/ofl/coiny)，许可证 SIL OFL 1.1 随字体保存。

## Verified evidence

修改文件：LevelOneHud.cs（删除两项文字绑定与刷新），LevelOneSceneSetup.cs（删除三项文字创建并接入标题配置），LevelOneTitleSetup.cs（Editor资产配置），BeeHarvest.LevelOne.Editor.asmdef（TMP引用），LevelOneSceneTests.cs（通过Model检查458已交付，适配计数文字删除）。新增字体TTF、OFL许可证、CoinyRoundedSDF字体／材质／图集子资产，以及TMP官方Essential Resources。

使用MCP for Unity完成组件替换、字体图集烘焙、保存并重新加载Level01；未文本修改场景或字体资产。标题填色／描边属性读回分别为FFF6E5／5A3A18，宽度0.2；字号浮点读回41.3999977，对应41.4。运行态文字宽142.04、容器宽630，未截断。计数、操作提示和槽位标题对象均不存在，五个实际槽位与蜂巢按钮引用完整。

测试验证真实点击产生采集和交付，花朵旋转不影响回巢货物，完成与重开正常。TMP内部会查询下划线字形：补齐`_`后再次保存静态图集并重载运行，控制台0 errors、0 warnings。保留开始时的Play状态，当前可以直接查看新版画面。

预览：/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/level-one/hud-title-change/preview.png。原场景与改动脚本备份保存在本次工作目录。只进行本次UI编辑所需的Editor／PlayMode验证，未在本次任务执行iOS构建或真机测试。
