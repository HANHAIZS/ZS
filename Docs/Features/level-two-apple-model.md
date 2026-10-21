---
id: level-two-apple-model
title: 第二关苹果方块模型与规划型分色
status: Verified
owner: Project
created: 2026-10-13
updated: 2026-10-13
---

# 第二关苹果方块模型与规划型分色

## Goal

在用户的Blender中制作第二关苹果模型，红色果皮，内部采用现有九色蜜蜂色板。配套方块与蜂巢数据须用现有采集规则验证：有规划可不使用道具完成，连续优先选第一列会缺少当前暴露颜色的蜜蜂而堵槽。

## User-visible behavior

- 独立圆角体素构成立体苹果，宽肩、顶部凹陷、收窄底部、黑色果柄和绿色叶片；果肉所有表面方块为Red。
- 果肉内层Yellow形成屏障，更深Orange/Blue/Cyan/Purple/White分区，颜色和蜂巢配额逐色精确一致。叶片Green、果柄Black同样可搬运。
- 五个常规槽位，队列只能从前端选择，不用加槽/任意选择/重排或加速来证明通关。第一列含红色开局蜂巢和深层颜色诱导，后续所需颜色位于其他列第二/三行。
- 独立Blender场景、原文件、FBX/GLB、方块清单、Level02.json、配色剖面、队列表和实测解法可检查。
- 本次建模和关卡内容制作；当前Home/Start/Continue仍运行现有第一关。第二关场景加载、Gallery点击、第三排蜂巢UI接入另行处理，现有UI仅两排。
- 修复现有可见性判定的微小掠射角漏洞：邻接方块遮住的面不透过装饰性缝隙计为可采集；封闭内层始终要先移除遮挡层。

## Out of scope

改第一关模型/数据/动画、制作第二关生产场景、改导航或Continue、关卡解锁/存档、增加新的颜色、修改Foundation、改变蜂巢选择规则；可见性只修复与现有“不透过方块缝隙采集”规则不一致的漏洞。

## Ownership and architecture

| Responsibility | Owner | Type/path | Notes |
| --- | --- | --- | --- |
| Content | Project | Blender scene + Assets/Sprites/Gameplay/Level02 | 每个方块独立稳定身份 |
| Level input | Project | Assets/Resources/Levels/Level02.json | 复用CubeData/HiveData，无schema变更 |
| State mutation/verification | Existing Command | StartLevelCommand/ActivateHiveCommand/AdvanceHarvestCommand/UpdateHarvestViewCommand | 命令未修改，复用正式状态变更路径 |
| Shared project rule | Existing System | HarvestSystem | 同色、当前相机可见、五槽前端选择；新增相邻格挡面的快速排除 |
| Presentation asset | Project | Assets/Resources/Prefabs/Gameplay/Apple.prefab | 独立Resources key，不替换Flower |
| Infrastructure | Foundation | None | 内容与项目采集判定修复，无Foundation修改 |

## State and persistence

采用现有LevelData结构；Cube id连续、整数三维格点唯一、name匹配导出模型，蜂巢各颜色配额等于对应方块数量。无存档变化。测试阶段在Edit Mode驱动现有命令，不对正在显示的生产视图换数据。

## UI, prefab, scene, and Inspector impact

- Blender在新Level02_Apple场景建模，保留用户原未保存场景和对象，先备份再创建。
- 模型原件和交付图在workspace outputs/level02-apple；直接引用FBX和材质位于Assets/Sprites/Gameplay/Level02。
- 新Apple.prefab Resources key为Prefabs/Gameplay/Apple，Level02.json为Levels/Level02，均不改变现有加载器。
- Unity原生API创建/读取新Prefab，不文本编辑.asset/.prefab/.unity。第一关场景、Flower.prefab和Level01.json保留。
- 第三排预览需要未来第二关UI适配；本次用队列图/文档表达规划信息，不宣称现有两排界面已经可显示三排。

## Integrations and failure behavior

离线程序化建模，无外部素材或生成器依赖。保存前验证方块数、名字和配额；使用新scene/collection不删除现有Blender内容。封闭内层使错选蜂巢等待；即使当前Deadlocked标志保留允许旋转，也以全角度无工作目标、五槽满、未胜利来判断设计中的不可继续局面。

## Performance considerations

最终1048个独立体素，9个共享倒角Mesh与9材质，46112三角面，FBX约1.07MiB、GLB约264KiB，静态无动画。Apple资源尚未加载进现有场景，不增加该场景Renderer数量；邻接挡面判断为HashSet查询并减少不必要射线探测。第二关接入后的1048个Renderer、真机帧率/内存/自动旋转体验留待实机测量，命令模拟耗时不视为帧率指标。

## Acceptance criteria

- [x] Blender中的完整模型清楚像苹果，果皮红色，果柄/叶片存在；三幅原生渲染已目检。
- [x] 1048方块使用现有九色且独立可搬运，模型、数据和Prefab逐一读回对应。
- [x] Blender原件、FBX/GLB及数据保存；源文件仅Level02_Apple，新场景之外用户原11对象保留并备份。
- [x] 不用道具的18次合法前端选择路径完成1048交付，步长0.1秒且采用实际相机倾角。
- [x] 第一列连选五槽全占、剩余951；360整数角+9个掠射角无目标，214深层同色方块的六个非红邻居全部遮挡，证明所有方向不可见。
- [x] Unity原生导入读回正确；第一关模型/数据/Level01场景未改，用户批准的共享遮挡修复完成并通过149项回归测试。

## Verification matrix

| Boundary | Evidence | Result |
| --- | --- | --- |
| Model/readback | outputs/level02-apple/model-verification.json、blend-file-readback.json；previews/apple-hero.png/front/cutaway | Pass：1048体素、9Mesh/9材质、原场景11对象保留 |
| Content consistency | apple-voxels.json、Level02.json、unity-asset-readback.json | Pass：逐体素身份/位置/配额一致，位置最大误差7.08e-7，颜色误差0 |
| Gameplay difficulty | unity-gameplay-proof.json；work/level02-apple/verify-gameplay-realtime.cs.txt | Pass：0.1秒正式命令、实际倾角0.169705689、1048全交付；951剩余五槽堵死；六邻居封闭证据 |
| Project conventions | validate_game_foundation_project.py + fresh Console | Pass：0警告，最终Console错误0，编译完成 |
| Existing behavior | tests-red.json、tests-editmode.json、tests-playmode.json、existing-project-readback.json | 修复前5个新用例失败；修复后84/84 EditMode、65/65 PlayMode；Level01资源/场景哈希保留 |
| Review | outputs/level02-apple/review.md | 掠射漏洞已原生重现并在用户批准后解决；无Critical/Important遗留 |

全EditMode套件中的既有MainMenuSetupTests会通过菜单创建器重建MainMenu.unity和MainMenuPage.prefab，产生序列化字节变化；所有菜单运行测试随后通过，功能保持。唯一生产源码变更为HarvestSystem.Visible，新增测试为VoxelOcclusionTests。模型导入没有修改场景、模型数据加载器或Gallery入口。

## Definition of Done

- [x] 模型和数据所有权归项目，沿用同色/可见/前端/五槽规则；用户批准修复装饰缝隙误判。
- [x] 验收项有可重复证据，正式复现脚本已指明；旧2秒探索脚本移除避免误覆盖。
- [x] 原生创建并读取Prefab与资源。
- [x] 项目规范、编译、84编辑器和65运行测试通过，无最终Console错误。
- [x] 输出文件与三幅预览可查看，生产第二关场景/第三排HUD未接入范围已说明。
- [x] 性能与设备检查范围已记录。
- [x] 独立审查完成，有证据后标记Verified。

## Decision log

- 2026-10-13: 采用现有九色蜜蜂色板；红色果皮，Yellow隔离层与更深分区形成颜色依赖，配套蜂巢队列一起验证。
- 2026-10-13: 参考Cube Land官方描述的同色匹配与提前选择顺序（https://rotatelab.com/games/cube-land/）；难度指标以本项目规则的实测局面为准。
- 2026-10-13: 审查补测发现0.01–0.1度掠射角可透过装饰缝隙误取封闭内层。原生重现后先增加封闭3×3×3方块回归测试，再做相邻格遮挡修复，保障第二关颜色规划要求。
- 2026-10-13: 自动审批曾因共享代码跨关卡影响拒绝修改；用户明确回复“我批准了”后，应用已准备的两行遮挡修复。首轮回归5个失败用例证明漏洞；最终84+65全通过。
- 2026-10-13: 最终模型1048体素、18蜂巢，红388、黄426、橙102、蓝77、白25、绿14、黑6、紫5、青5。实测合法列顺序1→5→5→5→1→2→2→2→3→3→3→4→4→4→1→1→1→1。
