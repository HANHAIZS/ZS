---
id: home-bee-level
title: 主界面蜜蜂与蜂蜜罐关卡标签
status: Verified
owner: Project
created: 2026-10-14
updated: 2026-10-14
---

# 主界面蜜蜂与蜂蜜罐关卡标签

## Goal

将用户主界面蜜蜂.png直接作为透明装饰放在Home主界面Start Game上方，参考演示2.png。罐子标签显示从哪个关卡返回。

## User-visible behavior

蜜蜂和蜂蜜罐保持原比例，整体居中，初次启动显示level 1；第一关返回显示level 1，第二关返回（含最终胜利Continue）显示level 2。切换Shop/Gallery时隐藏该装饰，回Home恢复。原有所有按钮及Start/Continue行为保持。

## Ownership and architecture

项目UI展示变更，复用MainMenuPage、MainMenuTabsController和MainMenuNavigationController。导航ViewController将当前关卡场景的LevelNumber作为页面Show参数；页面缓存仅用于显示的关卡数，Tabs负责Home装饰显隐和文字。不改Command、Model、System、Foundation或存档。最终第二关Continue虽然原导航目标回到第一关，标签仍显示返回来源第二关。

## State and persistence

展示状态为MainMenuPage.DisplayedLevel，默认1；来自实际LevelOneController.LevelNumber，生命周期随页面，非存档。无数据迁移或新增持久化。

## UI, prefab, scene, and Inspector impact

- Assets/Sprites/UI/MainMenu/HomeBee.png：用户RGBA原图直接复制，Sprite Single，共用压缩纹理、mipmap/ReadWrite关闭、max1024。直接引用方MainMenuPage.prefab的HomeArtwork Image。
- 沿用Resources键Prefabs/UI/MainMenuPage；主界面和两关的导航实例加载该共用预制体，随页面销毁；没有单独路径加载图片。
- MainMenuPage.prefab新增Content/SafeArea/HomeArtwork及Illustration Image及其子LevelLabel，图片与文字raycastTarget=false；AspectRatioFitter.FitInParent维持1:1。
- HomeArtwork位于顶部状态栏和Start按钮之间；LevelLabel在罐子米白标签内。保持原按钮位置、事件、场景和其他素材。
- MainMenuTabsSetup.Apply保持新增装饰，避免未来重建菜单遗漏。

## Out of scope

玩法、经济、胜利去向、重新启动/关卡继续逻辑、Shop/Gallery内容、背景、动画和存档不变。不添加新动画。

## Integrations and failure behavior

无网络依赖。复用页面初始化时检查Inspector引用的既有异常路径；文字与图片不拦截输入。页面显隐沿用已有导航，无新增订阅。

## Performance considerations

一张共用1024纹理，新增一个Image和一个TMP文字；无Update查询、无新增事件订阅、无重复Resources加载。只在页面显示或切换Tab时更新文字。

## Acceptance criteria

- [x] Home装饰与标签正确布局、透明、无变形，不遮住原控制。
- [x] 初次与两关返回标签正确，含第二关最终Continue；Start行为保持。
- [x] Shop/Gallery隐藏装饰，回Home正确恢复，设置正常。
- [x] 共用预制体/重建工具引用正确，原输入与关卡场景/数据保持。
- [x] 编译、聚焦测试、项目规范与运行截图检查通过。

## Verification matrix

| Boundary | Evidence |
| --- | --- |
| 初次进入和Tab显隐 | HomeBeeLevelTests实际场景测试 |
| 两关Home返回及Start保持 | HomeBeeLevelTests实际按钮与游戏状态检查 |
| 最后一关胜利 | 实际Continue按钮测试 |
| 资产引用、位置、射线 | Unity原生读回及1080×1920截图 |
| 原代码周边回归 | MainMenuFlow、MainMenuTabs、LevelTwoGameplay聚焦测试 |
| 项目规范/编译 | validator与Console |

## Definition of Done

- [x] 验收证据保存后标记Verified，报告检查及设备范围。

## Decision log

2026-10-14：用户确认居中放在Start Game上方，随后提供演示2.png。原图自带透明通道，无需改图。


## Completion evidence

/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/home-bee-level/：两个实际截图、返回标签读回、18/18测试、变化范围、Console、独立审查及验证报告。Unity原生预制体验证通过；重建Build两次保持唯一对象。项目validator零警告。原图与导入资产字节相同。新增HomeArtworkSetup和HomeBeeLevelTests，其余修改限于四个UI/编辑器文件及一个共用主界面预制体，无关卡场景或玩法变动。

## Superseded presentation
2026-10-15: 用户后续明确把新背景和关卡开始入口改为 home-level-button.md 中的规则。旧独立蜜蜂罐子叠图与来源关卡标签已移除；开始按钮现在显示并进入同一CurrentLevel。本文保留为历史实现记录，当前验收以home-level-button为准。
