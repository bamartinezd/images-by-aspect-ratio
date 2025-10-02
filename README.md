# Deep Bulk Image Filtering

A powerful .NET 8 application for bulk filtering and organizing images based on metadata criteria including resolution, aspect ratio, and orientation. Available as a console application, desktop GUI (Avalonia), and mobile app (MAUI).

## 🚀 Features

### Resolution Filtering
The application supports filtering by standard resolutions:

- **3840x2160 (4K UHD)** - Professional grade resolution with four times the pixels of 1080p, ideal for graphic design and video editing
- **1920x1080 (Full HD)** - Standard resolution used by most displays worldwide *(Coming Soon)*
- **2560x1440/1600 (2K)** - High-end resolution for premium displays *(Coming Soon)*
- **7680x4320 (8K)** - Ultra-high resolution for future-proof content *(Coming Soon)*

### Aspect Ratio Detection
Automatically identifies and filters images by aspect ratio:

- **16:9 (1.78:1)** - Widescreen format, most common for HDTV and computer monitors
- **9:16** - Mobile-optimized format, popular for Instagram Stories and TikTok
- **4:3 (1.33:1)** - Traditional fullscreen format *(Coming Soon)*
- **1.85:1 & 2.40:1** - Cinematic aspect ratios *(Coming Soon)*
- **3:2** - Standard photography format *(Coming Soon)*
- **5:4 (1.25:1)** - Large format photography standard *(Coming Soon)*
- **1:1** - Square format for social media and prints *(Coming Soon)*

### Image Orientation Support
Handles all image orientations:
- **Landscape** - Normal horizontal orientation
- **Portrait** - Vertical orientation (90° rotation)
- **Landscape Flipped** - Inverted horizontal orientation
- **Portrait Flipped** - Inverted vertical orientation

## 📱 Available Applications

### 1. Console Application (`Images-by-aspect-ratio/`)
- **Cross-platform** command-line interface
- **Lightweight** and fast processing
- **Automated** batch processing
- **Progress tracking** with visual progress bars

### 2. Desktop GUI (`AvaloniaApp/`)
- **Modern desktop interface** built with Avalonia UI
- **Cross-platform** (Windows, macOS, Linux)
- **Visual progress tracking** with progress bars and counters
- **Real-time image preview** during processing
- **Folder picker dialogs** for easy source/destination selection
- **Start/Stop controls** for processing management

### 3. Mobile App (`MauiAppDbif/`)
- **Native mobile experience** built with .NET MAUI
- **Android support** (iOS coming soon)
- **Touch-optimized interface** with proper spacing and controls
- **File picker integration** for mobile file selection
- **Real-time progress updates** and image preview
- **Responsive design** for different screen sizes

## 📋 Prerequisites

- .NET 8.0 SDK or Runtime
- Windows, macOS, or Linux (for console and desktop)
- Android device or emulator (for mobile app)

## 🛠️ Installation & Usage

### Console Application

1. Navigate to the console project:
```bash
cd Images-by-aspect-ratio
```

2. Run the application:
```bash
dotnet run
```

3. Follow the interactive prompts to select source and destination directories.

### Desktop GUI (Avalonia)

1. Navigate to the Avalonia project:
```bash
cd AvaloniaApp
```

2. Run the desktop application:
```bash
dotnet run
```

3. Use the GUI to:
   - Select source and destination folders using the "Browse" buttons
   - Click "Start Processing" to begin filtering
   - Monitor progress with the progress bar and counters
   - View current image preview and processing status
   - Stop processing at any time with the "Stop" button

### Mobile App (MAUI)

1. Navigate to the MAUI project:
```bash
cd MauiAppDbif
```

2. Build for Android:
```bash
dotnet build -f net8.0-android
```

3. Create release APK:
```bash
dotnet publish -f net8.0-android -c Release
```

4. Install on Android device:
   - Enable Developer Options and USB Debugging on your device
   - Connect via USB and run: `dotnet build -f net8.0-android -t:Install`
   - Or manually install the APK from `bin/Release/net8.0-android/MauiAppDbif-Signed.apk`

## 📁 Output Structure

All applications generate the same organized output structure:

```
destination/
├── 1/          # Normal orientation
│   ├── img-{guid}.jpg
│   └── ...
├── 3/          # 180° rotation
│   ├── img-{guid}.jpg
│   └── ...
├── 6/          # 90° clockwise rotation
│   ├── img-{guid}.jpg
│   └── ...
└── 8/          # 270° clockwise rotation
    ├── img-{guid}.jpg
    └── ...
```

## 🔧 Configuration

The application currently filters for:
- **Minimum resolution**: 3840x2160 (4K)
- **Aspect ratio**: 16:9 (1.78:1)
- **File formats**: JPG, JPEG, PNG, BMP, TIFF, TIF

## 📦 Dependencies

### Core Library (`DeepBulkImageFiltering.ImageProcessing/`)
- **SixLabors.ImageSharp** (3.1.10) - Image processing and metadata extraction

### Console Application
- **SixLabors.ImageSharp** (3.1.10) - Image processing
- **MetadataExtractor** (2.8.1) - EXIF metadata handling

### Desktop GUI (Avalonia)
- **Avalonia** (11.1.4) - Cross-platform UI framework
- **Avalonia.Controls.DataGrid** (11.1.4) - Data grid controls
- **SixLabors.ImageSharp** (3.1.10) - Image processing

### Mobile App (MAUI)
- **Microsoft.Maui.Controls** - Mobile UI framework
- **CommunityToolkit.Maui** (7.0.1) - Additional UI controls
- **SixLabors.ImageSharp** (3.1.10) - Image processing

## 🎯 UI Features Comparison

| Feature | Console | Desktop GUI | Mobile App |
|---------|---------|-------------|------------|
| **Platform Support** | Windows/macOS/Linux | Windows/macOS/Linux | Android (iOS soon) |
| **Progress Tracking** | Text-based | Visual progress bar | Visual progress bar |
| **Image Preview** | ❌ | ✅ Real-time preview | ✅ Real-time preview |
| **Folder Selection** | Text input | Dialog picker | File picker |
| **Processing Control** | ❌ | ✅ Start/Stop | ✅ Start/Stop |
| **Touch Support** | ❌ | ❌ | ✅ |
| **Responsive Design** | ❌ | ✅ | ✅ |

## 🐛 Known Issues

- Mobile app uses file picker instead of folder picker (platform limitation)
- iOS support for mobile app coming soon

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with .NET 8
- Powered by SixLabors.ImageSharp for image processing
- Desktop UI built with Avalonia
- Mobile UI built with .NET MAUI
- Uses MetadataExtractor for EXIF data handling

---

**Note**: This application is designed for bulk processing of large image collections. Ensure you have sufficient disk space and backup your original images before processing.
