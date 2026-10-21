---
id: ui-art-v2
title: V2界面素材替换与关卡位置调整
status: Verified
owner: Project
created: 2026-10-12
updated: 2026-10-12
---

# V2界面素材替换与关卡位置调整

## Goal

按用户提供的六张图片替换界面底图，把大蜂巢移至花朵正上方，Level 1移至左上角。所有按钮、道具、采集、搬运、进度及花朵自转功能沿用当前实现。

## User-visible behavior

- Retry与Start Game共享图一的新绿色金边底图；Home使用图二的新红色金边底图，文字和点击行为保留。
- 图三替换蜂巢槽位，图四替换四个功能按钮的总导航背景，图五替换这四个按钮的圆形底盘；图六替换关卡背景。
- 大蜂巢位于花朵正上方并保留当前大小；交付终点仍从模型实际入口读取。Level 1保持字体与字号，移至安全区左上方。
- 图一、二外围的棋盘格与其他按钮图的外围白底提取为透明通道，按钮内部白色和高光保留。

## Out of scope

玩法、按钮绑定、存档、道具数量、任意选择次数、加速持续时间、蜂巢/蜜蜂/花朵模型、花朵旋转速度、等待区布局和主界面背景。

## Ownership and architecture

| Responsibility | Owner | Type/path | Notes |
| --- | --- | --- | --- |
| State mutation | Existing Commands | Unchanged | 无新增状态写入 |
| Rules/state | Existing Model/System | Unchanged | 数量和游戏进度保留 |
| Unity presentation | ViewController | HarvestBoardView | 仅修改大蜂巢初始化位置 |
| UI appearance | Scene/prefab/assets | Level01, HarvestBoosterBar, SettingsPopup, MainMenuPage | 替换底图和关卡标题位置 |

## State and persistence

无schema、存档、订阅或SDK变化。大蜂巢位置属于表现；Deposit仍读取入口标记。暂停、Retry、Home、Start Game及道具逻辑不改。

## UI, prefab, scene, and Inspector impact

保留原路径和GUID，覆盖以下PNG，并通过Unity原生API读回其实际引用：

| Final asset | Reference owner |
| --- | --- |
| Assets/Sprites/UI/Settings/RetryButton.png | SettingsPopup Retry / MainMenuPage StartGame |
| Assets/Sprites/UI/Settings/HomeButton.png | SettingsPopup Home |
| Assets/Sprites/UI/Gameplay/HiveSlotTile.png | HiveSlotTile.mat → HiveModelView.SlotMaterial |
| Assets/Sprites/UI/Boosters/ToolbarBase.png | HarvestBoosterBar.ToolbarBase |
| Assets/Sprites/Gameplay/Backgrounds/Level01HoneyGarden.png | Level01背景材质与LevelBackgroundView |

新增直接引用图 Assets/Sprites/UI/Boosters/ButtonDiscV2.png，只绑定四个功能按钮，原翻页按钮继续用原ButtonDisc。

所有图片直接引用，无新Resources键。动态UI仍使用现有Prefabs/UI/MainMenuPage键，缓存/销毁责任不变。通过Unity原生Prefab/Scene API保存：

- HarvestBoosterBar：ToolbarBase和四个Booster按钮的Image.sprite。
- SettingsPopup：RetryButton、HomeButton的Image.sprite。
- MainMenuPage：StartGameButton的Image.sprite。
- Level01：上述实例的sprite引用；SafeArea/LevelLabel的RectTransform锚点min(.04,.90)、max(.31,.97)、零offset，以及左对齐。原字体和41.4字号保留。

透明图导入时内部sprite ID发生变化，因此在保留PNG GUID后，显式重新绑定上述8个Image.sprite，避免空引用。保留层级、脚本字段和onClick绑定。大蜂巢初始化投影中心改为屏幕(.5,.94)，大小仍为.62；模型包围盒投影约在.90至.98之间，不挡花朵和设置按钮。Deposit与模型实际入口距离为0。

Level01保存同时将两个翻页箭头TMP的冗余序列化颜色/style字段同步到已有的cream字体颜色和Normal样式；原ButtonDisc及翻页行为保留。没有待手工配置的Inspector字段。

## Integrations and failure behavior

离线素材。导入失败保留备份，不留下空sprite。背景沿用现有等比cover适配；图标alpha导入后检查，白色槽位内部不能透明。不需要新SDK、网络、权限流程或状态迁移。

## Performance considerations

无新逐帧计算或资源加载。Retry/Home最大1024，底盘与槽位最大512，总导航背景最大2048；关卡背景保持1024×1536压缩导入。五张透明UI贴图为Uncompressed、FromInput alpha、alphaIsTransparency=true；均关闭mipmap和Read/Write，采用Clamp和双线性过滤。Sprite与材质沿用共享引用，不实例化材质。

六张贴图的Unity Editor Profiler读回合计18,117,344字节，较导入分辨率优化前46,405,008字节下降约61%；该值包含Editor资源开销，不等同于真机GPU内存。已检查1080×1920编辑器实际画面；设备GPU、触控和其他屏幕比例尚未实测。

## Acceptance criteria

- [x] 六组图片替换正确，无外围棋盘格或白矩形。
- [x] 大蜂巢在花朵上方，Level 1在左上角；模型与按钮不重叠、不截断。
- [x] 入口与Deposit一致，原采集交付、Retry/Home/Start及四道具逻辑通过回归。
- [x] 仅一个生产脚本的位置参数变更，业务代码保持原版本；原引用与按钮绑定保留。

## Verification matrix

| Boundary | Evidence | Result |
| --- | --- | --- |
| Compile/conventions | Unity Console + project validator | 0 errors；校验通过、0 warnings |
| Imported assets | asset-guids.json、asset-readback.json、imported.json | 5个原GUID保留；8个sprite绑定有效；槽位内部白色不透明 |
| UI/runtime | level.png、settings.png、home.png、runtime-position.json、capture-complete.json | 最终导入分辨率下实际截图检查通过；Home→Start返回关卡；入口距离0 |
| Behavior | playmode.json；job dfd670887768475da416597f62cd77bf | 53/53通过、0失败/跳过，48.063秒 |
| Scope/performance | source-before.json、source-scope.json、source.diff、asset-readback.json | 仅HarvestBoardView位置参数及注释变更；其他66个C#文件一致；导入设置符合上述记录 |
| Read-only review | review.md | 无Critical/Important/Minor问题 |

验证文件目录：`/Users/zhangshuai/Documents/Codex/2026-10-09/wo/outputs/ui-art-v2`。53项运行时回归覆盖蜜蜂与方块/其他蜜蜂/花朵/大蜂巢间距、完整522方块采集交付、Retry、设置与Home/Start导航、道具数量和补充、任意选择一次、3分钟加速计时、花朵自动旋转与暂停。导入分辨率优化后再次读回全部绑定并重拍三张实际截图。

审查范围说明：SettingsPopup和MainMenuPage未包含整文件修改前备份，因此无法独立做完整prefab差异比对；本次原生操作仅枚举Image.sprite赋值，实际按钮/controller引用、源码哈希以及导航回归均通过检查。其他修改前备份位于`work/ui-art-v2/before`。

## Definition of Done

- [x] 六组素材和两个位置调整完成。
- [x] Unity原生API保存资产并读回。
- [x] 编译和项目规范校验通过。
- [x] 原功能回归与真实截图检查通过。
- [x] 记录性能与设备检查范围后标记Verified。

## Decision log

- 2026-10-12: 用户已明确指定素材对应关系、两个新位置及功能保留，按现有结构直接执行；外围背景使用内置imagegen提取，Unity导入阶段裁切透明空边与孤立噪点。
- 2026-10-12: 使用内置image_gen工具、transparent_background=true，每张图单独以原图编辑；五组完整提示词、输入/输出路径和工具输出提示记录于outputs/ui-art-v2/imagegen-prompts.json，最终透明源PNG保存在work/ui-art-v2/assets。关卡背景直接使用原图，无编辑。
- 2026-10-12: Unity原生保存、实际截图、53项PlayMode回归和项目规范校验通过，标记Verified。
