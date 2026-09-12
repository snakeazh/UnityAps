# CoinFlip — Unity 抛硬币（Android）

基于 **Unity 2022.3.62f3** 的竖屏抛硬币小游戏，目标平台 **Android**。  
抛硬币表现对齐休闲向《我的暴富日记》式 H5：暖色日记桌面、卡通金币、蓄力上抛 → 弹跳落地 → 花/字揭晓。

## 功能

- 点击屏幕或「抛一次」：蓄力下压 → 高抛快转 → 落地弹跳 → 轻微晃定
- 结果用「花 / 字」（一元硬币俗称），大字 punch 弹出
- 落地金色火花 + 浮动奖励文案（+¥1.88 / +¥0.88）
- 统计本地持久化；「清零」重置
- 竖屏与安全区适配

## 快速开始

1. 安装 [Unity Hub](https://unity.com/download) 与 **Unity 2022.3.62f3**（勾选 **Android Build Support**）
2. 用 Hub **打开本仓库根目录**
3. 打开 `Assets/Scenes/Main.unity` → Play

## Android 构建

| 菜单 | 作用 |
|------|------|
| `CoinFlip / Configure Android Player Settings` | 包名、竖屏、IL2CPP、ARM、MinSDK 23 / TargetSDK 34 |
| `CoinFlip / Build Android APK (Development)` | 输出 `Builds/Android/CoinFlip.apk` |
| `CoinFlip / Setup Main Scene` | 重新生成 Main 场景 |

- 产品名：`CoinFlip`
- 包名：`com.unityaps.coinflip`
- 方向：Portrait · IL2CPP · ARMv7+ARM64

## 工程结构

```
Assets/
  Scenes/Main.unity
  Scripts/Gameplay/   # CoinController / GameManager / GameBootstrap / CoinSparkBurst
  Scripts/UI/GameUI.cs
  Editor/CoinFlipEditorMenu.cs
```

`GameBootstrap` 运行时搭建暖色桌面、花/字金币与日记风 UI。

## 说明

不含 `Library/`。请使用与 `ProjectSettings/ProjectVersion.txt` 一致的编辑器版本。
