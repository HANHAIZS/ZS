# 共享游戏场景

项目只有两个场景：`Assets/Scenes/MainMenu.unity`（启动主页）和`Assets/Scenes/Game.unity`（所有关卡共用）。不要为新关卡复制Scene。

## 在编辑器中预览

打开Game，选中LevelOne对象，在LevelOneController的PreviewLevel输入关号，然后进入Play。首次直接运行Game使用该预览号；从主页进入、下一关及继续游戏使用导航关号，优先于Inspector预览值。

每次新的Play会话会清理上一次的临时导航关号，因此开启禁用域/场景重载的快速运行时也能按PreviewLevel预览。请先停止Play，再修改PreviewLevel后重新运行；运行中修改该字段不会即时切关。游戏内正常进入和下一关仍按导航关号加载。

需要在编辑状态更新标题和灯光时，使用菜单`BeeHarvest > Levels > Preview Game Inspector Level`。Preview不替换场景中的骨架，也不重写模型。

## 关卡数据

- 游戏数据：`Assets/Resources/Levels/LevelXX.json`，Resources键`Levels/LevelXX`。
- 模型：`Assets/Resources/Prefabs/Gameplay/{modelKey}.prefab`，Resources键`Prefabs/Gameplay/{modelKey}`。
- 展示目录：`Assets/Resources/Levels/PresentationCatalog.json`，Resources键`Levels/PresentationCatalog`。保存每关模型键、描边/颜色、间隙、缺口阴影、源位交接以及环境光和主光参数。

展示目录version为1，当前114条完整连续关号；每个字段必填，布尔类型不能写成字符串，RGBA及阴影数值限定0–1，模型键仅允许字母数字、下划线和连字符。运行时一次校验后缓存，编辑器预览会重新读取配置，缺项/非法值会明确报错。

扩展关卡时添加原数据、模型和展示记录，并同步LevelOneAssets.LastLevel及既有导入器目录与验收；不增加Scene。原SourceLevelSceneSetup.Apply(number)现在只构建该关模型并在Game中选择预览号。UI维护工具也只编辑Game一次。

## 保留的流程

下一关重载同名Game并释放旧模型、飞行与局部材质/纹理；重开与确认离开仍由原Commands处理。游戏内返回Home继续沿用原缓存页面/暂停语义，末关结算后Home显示Level1。第6和112–114关保留真实源位90ms交接，106及其他关的原显示差异均在展示目录中保留。

旧114个Scene由Unity原生删除，原114份关卡数据和全部模型未修改；需要查看历史时使用Git提交`b5efa9b`及以前版本。
