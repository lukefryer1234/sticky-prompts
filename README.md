# Sticky Prompts

A high-performance, always-on-top Windows desktop utility for managing and deploying AI text prompts via drag-and-drop.

![Windows 11](https://img.shields.io/badge/Windows-11-0078D6?logo=windows)
![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![WinUI 3](https://img.shields.io/badge/WinUI-3-00BCF2)

## Features

- 🎯 **Always-On-Top** - Your prompts stay visible while you work
- 🖱️ **Drag-and-Drop** - Drag prompts directly into any text field
- 🎨 **Color-Coded Tiles** - Organize prompts with custom colors
- 📐 **Compact Mode** - Minimize to a thin strip on screen edge
- 💾 **Auto-Save** - Changes persist automatically
- 🌓 **Mica Backdrop** - Native Windows 11 translucent glass effect

## Screenshots

| Normal Mode | Compact Mode | Editor Dialog |
|-------------|--------------|---------------|
| Grid of colorful prompt tiles | Thin strip with icons | Add/edit prompt with color picker |

## Requirements

- Windows 11 (22H2 or later)
- Visual Studio 2022 (17.8+) with:
  - .NET 8.0 SDK
  - Windows App SDK 1.5+
  - Windows 11 SDK (10.0.22621.0)

## Building from Source

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/sticky-prompts.git
cd sticky-prompts
```

### 2. Open in Visual Studio

1. Open `StickyPrompts.sln` in Visual Studio 2022
2. Right-click the solution → **Restore NuGet Packages**
3. Select the target platform (x64 recommended)
4. Press **F5** to build and run

### 3. Command Line Build

```powershell
dotnet restore
dotnet build -c Release -p:Platform=x64
dotnet run --project StickyPrompts
```

## Project Structure

```
StickyPrompts/
├── StickyPrompts.sln           # Solution file
└── StickyPrompts/
    ├── App.xaml(.cs)           # Application entry point
    ├── MainWindow.xaml(.cs)    # Main window UI
    ├── Models/
    │   └── PromptEntry.cs      # Data model
    ├── ViewModels/
    │   ├── MainViewModel.cs    # Main app logic
    │   └── PromptEditorViewModel.cs
    ├── Services/
    │   ├── StickyNoteStorage.cs # JSON persistence
    │   └── WindowService.cs    # Window management
    ├── Controls/
    │   └── PromptTile.xaml(.cs) # Draggable tile control
    ├── Dialogs/
    │   └── PromptEditorDialog.xaml(.cs)
    ├── Converters/
    │   └── Converters.cs       # XAML value converters
    └── Assets/
        └── seed_prompts.json   # Default prompts
```

## Usage

### Adding a Prompt
1. Click the **+** button in the toolbar
2. Enter a title and the prompt content
3. Select a color for the tile
4. Click **Save**

### Using a Prompt
- **Drag & Drop**: Click and drag a tile to any text field (VS Code, Notepad, browser, etc.)
- **Copy**: Right-click a tile → **Copy to Clipboard**

### Window Controls
- **Compact Mode** (📐): Toggle between full and strip mode
- **Always on Top** (📌): Toggle whether the window stays on top

## Data Storage

Prompts are stored in:
```
%LOCALAPPDATA%\StickyPrompts\prompts.json
```

The storage uses atomic writes (write to temp file, then rename) to prevent data corruption.

## Architecture

- **MVVM Pattern** using CommunityToolkit.Mvvm
- **WinUI 3** for native Windows 11 UI
- **System.Text.Json** with source generation for efficient serialization
- **Microsoft.UI.Windowing** APIs for window management (no legacy P/Invoke)

## License

MIT License - see [LICENSE](LICENSE) for details.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request
