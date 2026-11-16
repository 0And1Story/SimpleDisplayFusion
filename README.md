# SimpleDisplayFusion

Windows 11 托盘工具。新打开的应用窗口会移动到鼠标指针所在的显示器。

## 使用

双击 `SimpleDisplayFusion.exe`，在任务栏通知区域找到蓝色显示器图标。若图标被折叠，点通知区域的上箭头展开。右键菜单包含三项。

- **新窗口跟随鼠标** 默认启用，可随时暂停。设置会自动保存。
- **登录 Windows 时启动** 默认关闭，勾选后在当前用户登录 Windows 时运行。若要移动程序，请先取消勾选，移动后重新勾选。
- **退出** 结束托盘程序。下次启动仍会读取原来的设置。

程序按窗口首次出现时的鼠标位置选择显示器。同一个应用进程新建多个窗口时，每个窗口独立处理。窗口初始化较慢或在启动时自行改变位置时，程序会在约 200 ms 后再复查一次。常规窗口尽量保持大小和相对位置，必要时适配目标屏幕的工作区。最大化窗口移动后会在目标屏重新最大化。跨屏时应用也可能按目标屏幕的缩放比例重新布局。

切换到已打开的窗口、恢复已打开的最小化窗口、任务栏、工具窗和常见对话框会被跳过。权限较高的窗口、部分特殊应用窗口及在复查之后自行改回位置的窗口受 Windows 和应用自身行为影响。

## 构建和发布

在 Windows 上使用 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)。

```powershell
dotnet build -c Release
```

双击 `bin/Release/net10.0-windows/SimpleDisplayFusion.exe` 运行。该构建需要 .NET 10 Desktop Runtime，并应和同目录的文件一起保留。

生成包含运行时的 Windows x64 单文件版。ARM64 机器可把两处 `win-x64` 改为 `win-arm64`。

```powershell
dotnet publish -c Release -r win-x64 --self-contained `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true -p:DebugType=None `
  --source https://api.nuget.org/v3/index.json -o tmp/release/win-x64
```
