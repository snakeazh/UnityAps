# CoinFlip — Unity 抛硬币（Android）

基于 **Unity 2022.3.62f3** 的竖屏抛硬币小游戏，目标平台 **Android**。

## 功能

- 点击屏幕或「抛一次」按钮翻转 3D 硬币
- 结果展示：正面 / 反面
- 统计累计次数（PlayerPrefs 本地持久化）
- 「清零」重置统计
- 竖屏、安全区适配，适合手机操作

## 快速开始

1. 安装 [Unity Hub](https://unity.com/download) 与 **Unity 2022.3.62f3**（需勾选 **Android Build Support**，含 SDK / NDK / OpenJDK）
2. 用 Hub **打开本仓库根目录**
3. 打开场景 `Assets/Scenes/Main.unity`
4. 点击 Play 即可试玩

首次导入若提示升级/导入 TMP 等资源，按默认确认即可。

## Android 构建

菜单栏提供快捷项：

| 菜单 | 作用 |
|------|------|
| `CoinFlip / Configure Android Player Settings` | 包名、竖屏、IL2CPP、ARM、MinSDK 23 / TargetSDK 34 |
| `CoinFlip / Build Android APK (Development)` | 输出开发版 APK 到 `Builds/Android/CoinFlip.apk` |
| `CoinFlip / Setup Main Scene` | 若场景丢失，可重新生成 Main 场景 |

也可手动：`File → Build Settings → Android → Switch Platform → Build`。

### 已配置的 Player 要点

- 产品名：`CoinFlip`
- 包名：`com.unityaps.coinflip`
- 方向：Portrait
- Scripting Backend：IL2CPP
- 架构：ARMv7 + ARM64
- Min API：23 · Target API：34

正式上架请自行配置 Keystore 签名，并按需改为 Release 构建。

## 工程结构

```
Assets/
  Scenes/Main.unity          # 主场景（含 GameBootstrap）
  Scripts/Gameplay/          # 硬币动画、游戏逻辑、运行时搭建
  Scripts/UI/GameUI.cs       # UI 绑定与点击输入
  Editor/CoinFlipEditorMenu.cs
ProjectSettings/             # Unity 2022.3.62f3 + Android 相关设置
Packages/manifest.json
```

运行时由 `GameBootstrap` 自动创建硬币、灯光与 UI，无需再拖预制体。

## 操作说明

- **点击空白处** 或 **抛一次**：开始翻转
- **清零**：清空本地统计

## 说明

本仓库不含 `Library/`（由本机 Unity 首次打开时生成）。请使用与 `ProjectSettings/ProjectVersion.txt` 一致的编辑器版本打开，避免强制升级。
