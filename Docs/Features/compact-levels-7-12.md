---
id: compact-levels-7-12
title: 第7–12关沿用前六关紧凑布局
status: Verified
owner: Project
created: 2026-10-23
updated: 2026-10-23
---

# 第7–12关沿用前六关紧凑布局

## Goal
将1–6关已确定的布局方案应用到7–12关：槽位/队列整体下移、行列距收紧、放大模型并逐关检查旋转边界。

## User-visible behavior
- 槽位Y偏移-.048、五槽列距.16居中、额外槽与最左槽对齐；蜂巢行中心.274/.204/.134、行距.07、列距.24。
- 模型中心.64，按前六关同样的屏幕安全边界测16个旋转方向，放大上限对应前六关最终显示比例（原.86基数上限1.3312）。高/宽模型逐关收敛。
- 点击区域、蜂巢和槽位大小保留，跳跃/额外槽/重试继续跟随控件位置。

## Out of scope
仅修改7–12关表现目录中的布局数值。其他关卡、所有Level JSON、原机关/额度/顺序/模型内容、光照/描边/阴影/搬运/出巢时序、Prefab/Scene/材质/存档不改。不上传GitHub。

## Ownership and architecture
LevelPresentationData既有配置由LevelOneAssets校验并缓存；LevelOneController将参数传给既有HUD/Board，HiveModelView和HiveHopController复用位置。无新增运行时C#、Command/Event或架构依赖。

## State and persistence
无新增状态、存档版本或迁移；重试由既有Command重置，布局不重复累加。

## UI, prefab, scene, and Inspector impact
仅Assets/Resources/Levels/PresentationCatalog.json六条记录（Resources键Levels/PresentationCatalog）。既有加载缓存与销毁责任保持；无Hierarchy/Inspector绑定或源资源修改。保留用户场景和材质未提交内容。

## Integrations and failure behavior
无SDK/联网/热更新影响。暂停、重试、跨关加载复用既有实现；测试后刷新表现目录缓存。

## Performance considerations
只改初始化配置；无新增逐帧计算、材质、纹理、订阅或资源实例。

## Acceptance criteria
- [x] 六关实际布局、模型旋转边界、点击/跳入槽位/额外槽/重试验证通过。
- [x] 范围外配置、所有玩法JSON及Prefab逐字节保持。
- [x] 规范/编译/针对性回归通过。

## Verification matrix
| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | validate_game_foundation_project.py | Passed |
| Unity compilation | Console无新错误 | Passed |
| Runtime | 6关16方向边界与布局、点击和重试 | Passed |
| Scope | Git与基线哈希 | Passed |
| Performance | 仅配置，无新运行时路径 | Passed |

## Definition of Done
- [x] 所有验收有证据，保留用户文件，记录平台限制并本地Git保存。

## Verification evidence (2026-10-23)
- 原配置测试确认失败：PlayMode40336ab677c94d06928855324eeb23af，第7关槽位中心实际.3975，要求.3495。
- 最终EditMode21/21：5cda1ae9a0214f1fac6472b7a763ce09（表现目录、安全校验、前25关配置、共享场景）。
- PlayMode8个不同测试全部通过：3f8231ce733440058c5f539b27446b1a中7项回归通过；新增逐关测试因循环消耗额外槽道具而失败，补充每关测试库存后9fdc089cb27c4d359fe67b87185d0a90通过。覆盖7–12实际场景、每模型16旋转方向、点击边界、跳入槽位、额外槽/重试，以及原盲盒和搬运方式、1关和115–119/13关布局隔离。
- 最终旋转边界：7关x.1556–.8444/y.4447–.8385；8关x.1661–.8339/y.5484–.7398；9关x.1612–.8388/y.4437–.8431；10关x.3256–.6744/y.4396–.8373；11关x.3912–.6088/y.4508–.8325；12关x.1634–.8366/y.4464–.8438。均位于x.05–.95/y.43–.85安全范围。
- 范围核对：仅6条记录中的7个布局字段；其余113条记录、全部119个Level JSON不变，603个关卡/Prefab相关文件哈希保持。运行时代码、材质、光照、方块尺寸/间隙和原配色不改。
- 规范0警告，git diff --check通过；只读审查无阻塞项。Unity无新编译/游戏错误，Console仅一条MCP WebSocket未初始化的工具连接提示。
- 测试结束刷新表现目录和Level缓存；无需Inspector绑定。模型整体7–11关从原比例放大33%，12关30%，不新增每帧计算或渲染资源。
- 平台限制：Unity Editor原生布局/投影验证，未做手机真机触控测试；不生成截图或录像。
