# SimpleDisplayFusion

一个面向 Windows 11 的多显示器托盘工具。

---

## 🎯 项目目的

省去多屏使用时手动将新窗口拖到当前屏幕的操作。

---

## ✨ 特色功能

- 新打开的应用窗口自动移到鼠标所在的显示器。
- 右键托盘图标即可暂停或恢复窗口跟随。
- 支持登录 Windows 时自动启动。

---

## 🚀 快速开始

1. 按下文构建单文件版，双击 `SimpleDisplayFusion.exe`。
2. 在任务栏通知区域找到蓝色显示器图标，右键即可更改设置。

---

## 🛠️ 构建方法

在 Windows 上安装 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)，然后在项目目录执行以下命令，生成 Windows x64 单文件版。

```powershell
dotnet publish -c Release -r win-x64 --self-contained `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true -p:DebugType=None `
  --source https://api.nuget.org/v3/index.json -o tmp/release/win-x64
```

生成的文件位于 `tmp/release/win-x64/SimpleDisplayFusion.exe`。
