using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using DeepBulkImageFiltering.ImageProcessing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaApp;

public partial class MainWindow : Window
{
    private string? _sourcePath;
    private string? _destinationPath;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isProcessing = false;

    public MainWindow()
    {
        InitializeComponent();
        UpdateUI();
    }

    private async void OnSelectSourceFolder(object sender, RoutedEventArgs args)
    {
        await SelectFolderAsync(
            "Select Source Folder",
            result => {
                _sourcePath = result;
                SourcePathText.Text = result;
            });
    }

    private async void OnSelectDestinationFolder(object sender, RoutedEventArgs args)
    {
        await SelectFolderAsync(
            "Select Destination Folder",
            result => {
                _destinationPath = result;
                DestinationPathText.Text = result;
            });
    }

    private async Task SelectFolderAsync(string title, Action<string> setPathAction)
    {
        var storageProvider = StorageProvider;
        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        var firstFolder = folders.FirstOrDefault();
        var result = firstFolder != null ? firstFolder.Path.LocalPath : null;

        if (!string.IsNullOrEmpty(result))
        {
            setPathAction(result);
            UpdateUI();
        }
    }

    private async void OnStartProcessing(object sender, RoutedEventArgs args)
    {
        if (string.IsNullOrEmpty(_sourcePath) || string.IsNullOrEmpty(_destinationPath))
        {
            await ShowError("Please select both source and destination folders.");
            return;
        }

        if (!Directory.Exists(_sourcePath))
        {
            await ShowError("Source folder does not exist.");
            return;
        }

        _isProcessing = true;
        _cancellationTokenSource = new CancellationTokenSource();
        UpdateUI();

        try
        {
            await ProcessImagesAsync(_cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            UpdateStatus("Processing cancelled by user.");
        }
        catch (Exception ex)
        {
            await ShowError($"Error during processing: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            UpdateUI();
        }
    }

    private void OnStopProcessing(object sender, RoutedEventArgs args)
    {
        _cancellationTokenSource?.Cancel();
    }

    private async Task ProcessImagesAsync(CancellationToken cancellationToken)
    {
        var imageFiles = GetImageFiles(_sourcePath!);
        int totalImages = imageFiles.Count;
        int processedImages = 0;
        int copiedImages = 0;

        UpdateStatus($"Found {totalImages} images to process. Starting...");
        ResetProgress();

        foreach (var filePath in imageFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            processedImages++;
            UpdateCurrentFile(Path.GetFileName(filePath));
            UpdateProgress(processedImages, totalImages, copiedImages);

            // Load and display preview
            await LoadImagePreview(filePath);

            bool wasCopied = await ProcessImageAsync(filePath);
            if (wasCopied)
            {
                copiedImages++;
                UpdateProgress(processedImages, totalImages, copiedImages);
            }

            // Small delay to allow UI updates
            await Task.Delay(10, cancellationToken);
        }

        UpdateStatus($"Processing complete! Processed {processedImages} images, copied {copiedImages} images.");
    }

    private List<string> GetImageFiles(string sourcePath)
    {
        var extensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.tiff", "*.tif" };
        var files = new List<string>();

        foreach (var extension in extensions)
        {
            files.AddRange(Directory.EnumerateFiles(sourcePath, extension, SearchOption.AllDirectories));
        }

        return files;
    }

    private async Task<bool> ProcessImageAsync(string filePath)
    {
        try
        {
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(filePath);
            int imageWidth = image.Width;
            int imageHeight = image.Height;

            // Check minimum resolution (4K)
            if (imageWidth < 3840 || imageHeight < 2160)
            {
                return false;
            }

            var exifProfile = image.Metadata.ExifProfile;
            if (exifProfile is not null)
            {
                exifProfile.TryGetValue(ExifTag.Orientation, out var orientation);
                string aspectRatio = GetAspectRatio(imageWidth, imageHeight);

                if (!string.IsNullOrEmpty(aspectRatio))
                {
                    string destinationFile = Path.Combine(_destinationPath!, orientation?.ToString() ?? "Unknown", aspectRatio, $"img-{Guid.NewGuid()}.jpg");
                    string? destinationDirectory = Path.GetDirectoryName(destinationFile);
                    
                    if (!string.IsNullOrEmpty(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                        File.Copy(filePath, destinationFile, true);
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error processing {Path.GetFileName(filePath)}: {ex.Message}");
        }

        return false;
    }

    private async Task LoadImagePreview(string filePath)
    {
        try
        {
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(filePath);
            
            // Create a temporary file for preview
            var tempPath = Path.GetTempFileName() + ".jpg";
            await image.SaveAsync(tempPath);

            // Load into Avalonia Image control
            using var stream = File.OpenRead(tempPath);
            var bitmap = new Bitmap(stream);
            PreviewImage.Source = bitmap;

            // Update image info
            var fileInfo = new FileInfo(filePath);
            ImageInfoText.Text = $"Size: {image.Width} x {image.Height}\n" +
                                $"File: {fileInfo.Length / 1024 / 1024:F1} MB\n" +
                                $"Aspect: {GetAspectRatio(image.Width, image.Height)}";

            // Clean up temp file
            try { File.Delete(tempPath); } catch { }
        }
        catch (Exception ex)
        {
            ImageInfoText.Text = $"Error loading preview: {ex.Message}";
            PreviewImage.Source = null;
        }
    }

    private string GetAspectRatio(double imageWidth, double imageHeight)
    {
        if (IsAspectRatioSixteenNinths(imageWidth, imageHeight)) return "16:9";
        if (IsAspectRatioFourThirds(imageWidth, imageHeight)) return "4:3";
        if (IsAspectRatioThreeTwo(imageWidth, imageHeight)) return "3:2";
        if (IsAspectRatioOneToOne(imageWidth, imageHeight)) return "1:1";
        if (IsAspectRatioTwentyOneNine(imageWidth, imageHeight)) return "21:9";
        return "Custom";
    }

    private bool IsAspectRatioSixteenNinths(double imageWidth, double imageHeight) => IsAspectRatio(imageWidth, imageHeight, 16.0 / 9.0);
    private bool IsAspectRatioFourThirds(double imageWidth, double imageHeight) => IsAspectRatio(imageWidth, imageHeight, 4.0 / 3.0);
    private bool IsAspectRatioThreeTwo(double imageWidth, double imageHeight) => IsAspectRatio(imageWidth, imageHeight, 3.0 / 2.0);
    private bool IsAspectRatioOneToOne(double imageWidth, double imageHeight) => IsAspectRatio(imageWidth, imageHeight, 1.0);
    private bool IsAspectRatioTwentyOneNine(double imageWidth, double imageHeight) => IsAspectRatio(imageWidth, imageHeight, 21.0 / 9.0);

    private bool IsAspectRatio(double imageWidth, double imageHeight, double targetRatio)
    {
        var imgAspectRatio = imageWidth / imageHeight;
        return Math.Abs(imgAspectRatio - targetRatio) < 0.01;
    }

    private void UpdateUI()
    {
        bool canStart = !string.IsNullOrEmpty(_sourcePath) && !string.IsNullOrEmpty(_destinationPath) && !_isProcessing;
        
        StartProcessingButton.IsEnabled = canStart;
        StopProcessingButton.IsEnabled = _isProcessing;
        SelectSourceButton.IsEnabled = !_isProcessing;
        SelectDestinationButton.IsEnabled = !_isProcessing;
    }

    private void ResetProgress()
    {
        ProcessingProgressBar.Value = 0;
        ProcessedCountText.Text = "0";
        CopiedCountText.Text = "0";
        CurrentFileText.Text = "No file being processed";
        ImageInfoText.Text = "No image selected";
        PreviewImage.Source = null;
    }

    private void UpdateProgress(int processed, int total, int copied)
    {
        double percentage = total > 0 ? (double)processed / total : 0;
        ProcessingProgressBar.Value = percentage;
        ProcessedCountText.Text = processed.ToString();
        CopiedCountText.Text = copied.ToString();
    }

    private void UpdateCurrentFile(string fileName)
    {
        CurrentFileText.Text = fileName;
    }

    private void UpdateStatus(string message)
    {
        StatusText.Text = message;
    }

    private async Task ShowError(string message)
    {
        var messageBox = new Window
        {
            Title = "Error",
            Width = 400,
            Height = 150,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Margin = new Avalonia.Thickness(0, 0, 0, 20)
                    },
                    new Button
                    {
                        Content = "OK",
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
                    }
                }
            }
        };

        // Add event handler properly
        var okButton = (Button)((StackPanel)messageBox.Content).Children[1];
        okButton.Click += (s, e) => messageBox.Close();

        await messageBox.ShowDialog(this);
    }
}