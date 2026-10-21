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

远程仓库为 [HANHAIZS/ZS](https://github.com/HANHAIZS/ZS)，`origin/main` 跟踪本地 `main`。GitHub HTTPS 登录保存在 macOS 钥匙串。当前网络需要代理时，单次使用 `git -c http.proxy=http://127.0.0.1:7897 push`；不把代理或凭据写入项目文件。

## 2026-10-21 忽略目录整理

- Unity 缓存、日志、用户设置、截图与 IDE 自动生成文件已从首次提交排除，本次没有把它们重新上传。
- 补充忽略构建/导出/性能采样目录、AI工具本机状态、实验和验证输出、Python/Node缓存与DCC备份。
- `.gamefoundation/onboarding-report.json` 与 `Packages/unity-mcp-install-source.txt` 是本机生成报告，使用 `git rm --cached` 停止跟踪，本机文件保留。`.gamefoundation/project.json` 继续纳入。
- 用户明确要求个人 `.agents/skills` 不上传：停止跟踪并加入忽略，本机13份skill文件保留。校验脚本不再强制要求这3个本机skill入口，AGENTS.md提供项目文档替代流程。
- `.github`、`Docs`、`Scripts` 与 AGENTS.md 是共享项目校验/说明文件，继续保留；嵌入 `Packages` 为依赖源码；`Assets/LevelSources` 为关卡重建来源，均保留。没有忽略整个 `Assets`、`Packages` 或 `ProjectSettings`，也没有忽略 `.meta`。
- 以普通清理提交推送，不重写已发布历史。忽略规则作用于当前目录与未来提交，历史版本中的报告仍可回看。
