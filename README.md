# 🖼️ Image Overlay

A simple and practical image overlay application for Windows. Load any image and place it on top of your screen with full control over positioning, sizing, and transparency.

## ✨ Features

- 🖱️ **Easy to Use**: Simple drag-and-drop image loading
- 🔄 **Flexible Resizing**: Proportional resize with Shift key, free resize with normal drag
- 👆 **Move Mode**: Drag your image anywhere on the screen
- 🌟 **Transparency Control**: Adjust opacity with built-in slider
- 📌 **Always on Top**: Overlay stays above all other windows
- ⌨️ **Keyboard Shortcuts**: Quick access with handy hotkeys

## 🚀 Installation

### Requirements
- Windows 10/11
- .NET 8.0 Runtime

### Download
1. Download the latest version from [Releases](../../releases)
2. Extract the ZIP file
3. Run `SimpleOverlay.exe`

## 📖 Usage

### Basic Usage
1. Click **Load Image** button to select an image
2. The overlay window will appear
3. Resize and position the image as needed

### Controls

#### 🖱️ Mouse Controls
- **Left Click + Drag**: Move the image
- **Shift + Drag**: Proportional resizing
- **Normal Drag**: Free resizing
- **Bottom Right Corner**: Resize handle for precise sizing

#### ⌨️ Keyboard Shortcuts
- `Ctrl + M`: Toggle Move Mode on/off
- `Escape`: Exit Move Mode

#### 🎛️ Control Panel
- **Move Mode**: Enables image movement mode
- **Opacity Slider**: Adjusts image transparency (0.1 - 1.0)
- **Close**: Closes the application

## 🛠️ Development

### Project Structure
```
SimpleOverlay/
├── App.xaml              # Application entry point
├── App.xaml.cs
├── MainWindow.xaml       # Main control window
├── MainWindow.xaml.cs
├── OverlayWindow.xaml    # Overlay window
├── OverlayWindow.xaml.cs
└── SimpleOverlay.csproj  # Project file
```

### Building
```bash
# In project directory
dotnet build

# Release build
dotnet build -c Release

# Publish as single file
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 🎯 Use Cases

- **Reference Images**: Keep reference images on screen while drawing or designing
- **Design Comparison**: Compare mockups with actual applications
- **Presentations**: Display additional visuals during presentations
- **Gaming**: Show maps or guides while gaming
- **Education**: Keep learning materials visible on screen

## 🔧 Technical Details

- **Framework**: .NET 8.0 WPF
- **Platform**: Windows
- **Supported Formats**: PNG, JPG, JPEG, BMP, GIF, TIF, TIFF, WEBP
- **Architecture**: x64

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

1. Fork this repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📞 Contact

Feel free to open an issue for questions or suggestions.

---

⭐ If you like this project, don't forget to give it a star!