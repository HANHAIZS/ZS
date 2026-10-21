---
id: foundation-rebuild
title: BeeHarvest 按 Unity Game Foundation 重建
status: Verified
owner: Project
created: 2026-10-09
updated: 2026-10-09
---

# BeeHarvest 按 Unity Game Foundation 重建

## Goal

备份旧工程后移除旧代码、场景、资源及测试，使用指定仓库官方完整入口重新接入 QFramework 和 Foundation，按仓库架构重新设计游戏。

## User-visible behavior

- 原项目路径仍为 /Users/zhangshuai/BeeHarvest。
- 原游戏原型内容不再出现在工作工程；保留可恢复备份。
- 新工程只有 BeeHarvest.BeeHarvestApp 一个 Architecture 和 BeeHarvestController 基类。
- 本阶段范围收窄为第一关核心玩法；设计依据为 Docs/GameDesign.md。

## Out of scope

- 本工作项不实现完整玩法、生产美术、商城或 iPhone 发行。
- 不修改 Foundation 公共包，不接入真实广告、CDN、IAP 或统计 SDK。
- 不沿用旧游戏脚本、测试或场景，不清理项目外用户数据。

## Ownership and architecture

| Responsibility | Owner | Planned type/path | Notes |
| --- | --- | --- | --- |
| State mutation | None | 无本工作项运行时业务变更 | 后续功能通过 Command |
| Observable or persistent state | None | 无本工作项玩家数据 | 旧存档不迁入新格式 |
| Shared project rules | None | 无本工作项游戏规则代码 | 已确认规则记录在设计中 |
| One-shot notification | None | 无 | 后续功能再定义 |
| Unity presentation | Project | Assets/Scripts/Game/Architecture/BeeHarvestController.cs | 官方生成 Controller 基类 |
| Adapter or repository | Project | Assets/Scripts/Game/Architecture/BeeHarvestApp.cs | 唯一项目组合入口 |
| Reusable infrastructure | Foundation | 7 个官方 UPM 能力包及 QFramework | 不改供应商源码 |

## State and persistence

- Runtime state: 仅基础设施组合，不创建旧游戏状态。
- Save schema/version/migration: 新玩家 DTO 在后续独立工作项设计；本次不迁移旧原型。
- State mutation path: 本工作项无玩家状态修改；工程文件按用户重建指令替换。

## UI, prefab, scene, and Inspector impact

- Affected assets: 旧生产资产移出；官方生成 UI Profile 和 YooAsset Collector。
- Loading mode and final paths: Resources 默认加载目录与 Assets/Sprites 直接引用目录由官方生成。
- Resources keys or direct reference owners: 本工作项不创建运行时加载资源。
- Load/cache/instance cleanup responsibility: 后续 Feature Spec 单独定义。
- Hierarchy or binding changes: 清理旧场景；新生产场景稍后制作，启动空场景通过 Editor API 创建。
- Inspector confirmation: 读取官方生成 Profile／Collector 路径和设置，读取空启动场景。

## Integrations and failure behavior

- SDK/consent/lifecycle: 默认 NoOp；无第三方账号或密钥。
- Content update/offline/rollback: 官方 YooAsset 依赖与 Collector 就绪，首版离线不下载；替换前校验可恢复备份；本关使用本地资源，无在线下载流程。
- Cancellation, timeout, retry: Unity 操作仅通过 MCP 调用官方 Editor Scaffolder／Onboarding API；替换前校验备份，失败可恢复。

## Performance considerations

- 无游戏逐帧运行逻辑。官方包安装增加工程依赖，不等于引入联网或热更运行流程。

## Acceptance criteria

- [x] 旧工程有可读备份及 SHA-256 校验记录。
- [x] 旧游戏目录、测试、原型场景和模板资产不在新工作工程。
- [x] 官方完整接入报告 localReady 为 true，compiledArchitecture、uiProfile、yooasset 均 ready。
- [x] 项目规范检查通过，Unity 编译无错误，只有一个 Architecture。
- [x] 官方 QFramework 组合与 Safe Area 的适用测试通过。
- [x] Unity MCP 重连到相同项目路径，读取新启动场景。
- [x] 新架构设计对应仓库职责、资源加载和规范流程。

## Verification matrix

| Boundary | Evidence required | Result |
| --- | --- | --- |
| Project conventions | python3 Scripts/validate_game_foundation_project.py --project . | 通过；见 Docs/LevelOneVerification.md |
| Unity compilation | MCP 执行官方验证 API、编译后控制台 | 通过；见 Docs/LevelOneVerification.md |
| Focused tests | 官方接入与第一关适用测试 | 通过；见 Docs/LevelOneVerification.md |
| Runtime or asset behavior | 新工程文件清单、唯一 App、MCP 场景读回 | 通过；见 Docs/LevelOneVerification.md |
| Performance | 无新增游戏循环，包依赖与离线边界复核 | 通过；见 Docs/LevelOneVerification.md |

## Definition of Done

- [x] Ownership and QFramework roles match the implemented files.
- [x] Public behavior and acceptance criteria are implemented.
- [x] Save, SDK, hot-update, UI, prefab, and Inspector impacts are handled where relevant.
- [x] Convention validation and Unity compilation pass.
- [x] Focused tests and exact behavior checks pass.
- [x] Performance risks and manual platform checks are reported.
- [x] Status is updated to Verified only after all required evidence exists.

## Decision log

- 2026-10-09: 用户要求重建并完整采用 nn555k/unity-game-foundation 架构；旧原型不沿用。
- 2026-10-09: 旧工程已备份；按用户指定改用 MCP 调用官方生成与校验 API，保留 Unity MCP 10.3.0。暂存工程不作为生产工程。

## Verified evidence

Foundation v0.6.0（仓库提交 abcc15a1105fe68ec9a1f142db427a1a723de2fb）；286个公共包源文件与仓库一致。官方报告 stage=verified、localReady=true，三个检查均 ready。生产场景为 Level01；玩法属于独立 level-one-core 工作项。旧工程备份 SHA-256：ab8315fd1a696bdb585e9af590fb6117adc6ed1b8bef0672b6bf228eeb107105。
