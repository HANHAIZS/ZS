---
id: button-press-motion
title: 全局按钮按压回弹
status: Verified
owner: Project
created: 2026-10-22
updated: 2026-10-22
---

## Goal
所有现有UI Button按下缩小、松开轻微回弹回原比例。覆盖主界面/商城/Setting/关卡/设置/胜利/离开/道具购买弹窗，保持既有点击动作、导航和弹窗动画、原素材布局及存档规则。

## User-visible behavior
按下约0.06秒缩到自身原比例94%；松开约0.18秒回弹（峰值102.5%）并精确回原比例。极快点击先完成按压再回弹；快速连点从当前比例接续，不积累缩放。非左键、不可交互按钮和其他手指不触发/抢占。拖出回弹、重新进入可再次按压；禁用/隐藏/后台立即恢复。用Time.unscaledDeltaTime，暂停时同样有效。保留原非单位/非均匀scale，只缩放Graphic/TMP绘制顶点，Transform/点击/Mask裁切矩形完全不动，保持原点击边界和padding。

## Out of scope
不改业务onClick、经济/道具/关卡/存档/设置、原美术/字体/布局、导航与弹窗已有动画；不替代已有3D蜂巢跃动。无SDK/资源或Foundation改动。

## Ownership and architecture
仅ViewControllers/UI/ButtonPressMotion负责临时展示状态及事件，ButtonPressGraphic负责Image/UGUI/TMP绘制顶点缩放，ButtonPressBackdrop负责三个既有设置组合图的烘焙按钮底图遮除，无Model/System/Command改动。MainMenuPage.Bind安装主界面树；LevelOneHud.Initialize在创建PurchasePopup后安装关卡Canvas树，包括所有隐藏弹窗。安装时遍历一次Button并去重；不每帧查找/加载、不监听onClick、不改Inspector绑定，不写Prefab或Scene。无运行时新增按钮路径遗漏：现有唯一动态购买弹窗在HUD安装前创建。

## State and persistence
只缓存button/RectTransform/所属绘制组件、拥有手指、阶段/时间；ButtonPressGraphic缓存TMP原顶点并随重建刷新。无持久化；OnDisable/暂停/失焦恢复，销毁随原对象生命周期。

## Acceptance criteria
- [x] 按下/松开/快点/连点有正确缩放且精确恢复，原业务点击只触发一次。
- [x] 多指/右键/禁用/拖出重入/隐藏/后台正确；暂停中使用unscaled时间。
- [x] 原点击边界和padding保持，原非均匀比例保持；不改文字/图片/布局与绑定。
- [x] 两场景包括动态/隐藏弹窗所有Button只有一份反馈组件；既有导航/弹窗/设置/输入回归通过。
- [x] 原用户场景/材质修改与其他资产保持，规范/编译/Console/审查通过。

## Verification matrix
| Boundary | Evidence |
| --- | --- |
| 按压/回弹/连点/异常输入 | 新ButtonPressMotionTests先RED再GREEN |
| 安装覆盖及真实生命周期 | ButtonPressMotionUiTests +现有菜单/弹窗回归 |
| 业务与旧动画保持 | onClick次数、既有导航/PopupMotion/设置测试 |
| 资产/规范 | 起始哈希、Prefab/Scene无改动、项目validator、Console |

## Definition of Done
- [x] 所有验收有新鲜证据，文档Verified，限制已记录。

## Performance and limits
每个Button缓存所属Graphic效果数组，嵌套Button分别拥有自己的绘制内容，TMP子材质由主文本统一处理；Update闲置立即返回，只有活动动画改绘制顶点，无每帧资源加载/依赖查询。TMP顶点缓冲首次生成/文本增大时分配，其余复用；图片用已有Canvas网格重建，无新材质/Graphic或Transform层级。一次初始化扫描包含隐藏按钮，原生命周期回收。无需新素材/Resources键/Inspector字段；不截图录屏、不推送GitHub，未做真机/性能profiling。

## Implementation checklist
- [x] RED：加入行为/覆盖测试，验证现有项目缺少反馈而失败。
- [x] GREEN：实现共享按压组件、两个安装入口并跑新测试。
- [x] 验证原动画、弹窗/设置输入与资产保持，审查后本地提交。

审查补充：直接缩放根节点会使Mask和子图Raycast实际范围缩小，新增真实GraphicRaycaster回归已验证RED；最终实现改用绘制顶点缩放，保留全部原输入矩形/Mask/Graphic属性。


组合图补充：三类设置面板的整图已烘焙按钮，同时叠放三个真实Button切片。ButtonPressBackdrop仅在同纹理、源切片被父图包含、且验证为同一行三个分离区域时启用；按下时把活动切片对应底图网格UV映射到同行间隔的背景列、保留V方向渐变，真实Button绘制顶点再缩放；支持两个按钮同时按压，闲置/取消恢复原四顶点原UV。不改PNG、不增加材质/Graphic/Transform节点，只在该父Image添加MeshEffect。新网格/UV测试已先复现底图残留绿色按钮RED。TMP在回弹终点重新生成一次原生未缩放文本，避免缓存早于自动字号/版式更新。

## Final verification — 2026-10-22
- PlayMode新测试11/11通过（39c9a56504214fb68e905efc802ac763）：按压/回弹/连点、所有输入生命周期、真实Mask与GraphicRaycaster边界、组合图网格与UV、多指取消、TMP重绘及两场景隐藏/动态弹窗安装。
- PlayMode旧UI回归26/26通过（c37b256692c84a5bbc820625e233c602）：MainMenuTabsTests、NavigationBarAnimationUiTests、PopupMotionUiTests、BoosterPurchasePopupTests、SettingsPopupTests。
- EditMode现有导航/弹窗插值测试8/8通过（c9c7aea12fe94ef19c72085c757b1a3d）。Unity Console错误/警告0；Foundation validator警告0；git diff --check通过。
- 独立代码审查通过；已修复审查指出的Mask点击边界及组合图原按钮残留问题，并添加先RED后GREEN回归。
- 对比任务起始SHA256：仅两个安装入口的既有脚本改变；所有既有场景/Prefab/美术/字体/材质保持起始内容，用户原有Game.unity及两份材质修改未纳入本次提交。
- 新增源：Assets/Scripts/Game/ViewControllers/UI/ButtonPressMotion.cs、ButtonPressGraphic.cs、ButtonPressBackdrop.cs；测试：Assets/Scripts/Game/Tests/Runtime/ButtonPressMotionTests.cs。现有入口：MainMenuPage.Bind、LevelOneHud.Initialize。组件按原对象生命周期销毁，无新资产路径或Resources键。影响MainMenu和Game的运行时Button，114关共享。
- 机器可读核对记录位于工作区work/button-press-motion/final-verification.json。未截图/录屏、未做真机触控与性能profiling；无剩余Inspector配置或资源导入步骤。
