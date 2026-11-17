<div align="center">

# SimpleDisplayFusion

一个面向 Windows 11 的多显示器窗口管理优化工具。<br>
支持类似 [Display Fusion](https://www.displayfusion.com/Help/#settings-windowmanagement) 的新窗口跟随等功能。

![Windows 11](https://img.shields.io/badge/Platform-Windows%2011-0078D4)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Latest Release](https://img.shields.io/github/v/release/0And1Story/SimpleDisplayFusion?logo=github&color=brightgreen)](https://github.com/0And1Story/SimpleDisplayFusion/releases/latest)
[![License](https://img.shields.io/github/license/0And1Story/SimpleDisplayFusion)](https://github.com/0And1Story/SimpleDisplayFusion/blob/main/LICENSE)

</div>

> [!IMPORTANT]
> 该项目只做一件事：优化多显示器下的窗口管理操作。<br>
> 因此不接受任何其他无关功能的 feature request 或 pull request，有需要请自行 [fork](https://github.com/0And1Story/SimpleDisplayFusion/fork) 或使用 Display Fusion 等其他工具。<br>
> 相关功能可在 [Issues](https://github.com/0And1Story/SimpleDisplayFusion/issues) 中提出讨论。

---

## ✨ 特色功能

- 新打开的应用窗口自动移到鼠标所在的显示器。
- 支持 Win + D 仅作用于当前显示器。
- 支持登录 Windows 时自动启动。
- 右键托盘图标即可开启或关闭各种功能。
- 断开显示器后自动关闭所有功能，不影响单显示器操作。

---

## 🚀 快速开始

1. 下载预编译版本，或 [自行构建](#build)。<br>
[![Latest Release](https://img.shields.io/github/v/release/0And1Story/SimpleDisplayFusion?style=for-the-badge&logo=github&color=brightgreen)](https://github.com/0And1Story/SimpleDisplayFusion/releases/latest)<br>
Release 中带 `runtime` 后缀的版本直接下载即可运行，无需额外安装 .NET 运行时。若您不确定是否已安装 .NET 10 Desktop Runtime，请下载带 `runtime` 后缀的版本。
2. 双击启动 `SimpleDisplayFusion.exe`。
3. 在任务栏通知区域找到蓝色显示器图标，右键即可更改设置。

---

<a name="build"></a>

## 🛠️ 构建方法

在 Windows 上安装 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)，然后在项目目录下执行以下命令：

```powershell
dotnet build -c Release
```

双击 `bin/Release/net10.0-windows/SimpleDisplayFusion.exe` 运行。该构建方式依赖 [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0/runtime)。

若要生成包含运行时的 Windows x64 单文件版，请使用以下命令：

```powershell
dotnet publish -c Release -r win-x64 --self-contained `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true -p:DebugType=None `
  --source https://api.nuget.org/v3/index.json -o bin/release/win-x64
```
ARM64 机器可把两处 `win-x64` 改为 `win-arm64`。
