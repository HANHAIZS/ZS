# Git 版本管理

仓库位于游戏项目根目录 `BeeHarvest`，默认分支为 `main`。首次提交保存启用 Git 时的项目版本（105 关）。历史从本次首次提交开始，之前的工作区备份没有伪造成历史提交。

## 保存一次修改

在项目目录运行：

```sh
git status --short
git diff --stat
git add Assets Packages ProjectSettings Scripts Docs
git diff --cached --stat
git commit -m "描述本次修改"
```

按实际修改选择路径；修改根目录配置时，将对应文件一并暂存。每次提交前核对暂存内容。新素材和它的 `.meta` 文件须一起提交；资源移动、删除也须同时保留相应 `.meta` 变更。不要通过删掉再生成 `.meta` 的方式解决资源引用问题。

## 查看历史

```sh
git log --oneline --decorate -10
git show --stat HEAD
git diff HEAD~1 HEAD -- Assets/Scripts/Game
```

需要回看旧版本时，先保存当前修改，再使用独立分支或工作树。不要在有未保存修改时直接执行强制重置。

## 纳入和忽略的内容

- 纳入 `Assets`、`Packages`、`ProjectSettings`、`Scripts`、`Docs` 和项目治理文件；包括场景、预制体、材质、源关卡数据、音频、图片及对应 `.meta`。
- 嵌入的本地 Foundation 包也纳入，避免只记录 `file:` 引用却遗漏包源码。
- `.gitignore` 排除 Unity 的 `Library`、`Temp`、日志、用户设置、构建物、生成的 IDE 工程、本机设置及 `Captures` 截图输出。
- 本地环境变量和签名文件不纳入；Unity YAML 与源码使用文本差异，图片、声音和模型使用二进制属性。

目前只配置本地仓库，没有远程地址。若要云端备份，另行绑定用户指定的远程仓库后再推送。本地 Git 记录历史，但不能代替异地备份。
