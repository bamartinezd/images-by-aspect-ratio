using DeepBulkImageFiltering.ImageProcessing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace MauiAppDbif
{
    public partial class MainPage : ContentPage
    {
        private string? _sourcePath;
        private string? _destinationPath;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isProcessing = false;

        public MainPage()
        {
            InitializeComponent();
            UpdateUI();
        }

        private async void OnSelectSourceFolder(object sender, EventArgs e)
        {
            try
            {
                // For mobile, we'll use a simple approach - let user select a file and use its directory
                var result = await FilePicker.PickAsync();
                if (result != null)
                {
                    _sourcePath = Path.GetDirectoryName(result.FullPath);
                    SourcePathLabel.Text = _sourcePath ?? "Unknown path";
                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to select source folder: {ex.Message}", "OK");
            }
        }

        private async void OnSelectDestinationFolder(object sender, EventArgs e)
        {
            try
            {
                // For mobile, we'll use a simple approach - let user select a file and use its directory
                var result = await FilePicker.PickAsync();
                if (result != null)
                {
                    _destinationPath = Path.GetDirectoryName(result.FullPath);
                    DestinationPathLabel.Text = _destinationPath ?? "Unknown path";
                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to select destination folder: {ex.Message}", "OK");
            }
        }

        private async void OnStartProcessing(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_sourcePath) || string.IsNullOrEmpty(_destinationPath))
            {
                await DisplayAlert("Error", "Please select both source and destination folders.", "OK");
                return;
            }

            if (!Directory.Exists(_sourcePath))
            {
                await DisplayAlert("Error", "Source folder does not exist.", "OK");
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
                await DisplayAlert("Error", $"Error during processing: {ex.Message}", "OK");
            }
            finally
            {
                _isProcessing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                UpdateUI();
            }
        }

        private void OnStopProcessing(object sender, EventArgs e)
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
                await Task.Delay(50, cancellationToken);
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
                var tempPath = Path.Combine(Path.GetTempPath(), $"preview_{Guid.NewGuid()}.jpg");
                await image.SaveAsync(tempPath);

                // Load into MAUI Image control
                PreviewImage.Source = ImageSource.FromFile(tempPath);

                // Update image info
                var fileInfo = new FileInfo(filePath);
                ImageInfoLabel.Text = $"Size: {image.Width} x {image.Height}\n" +
                                    $"File: {fileInfo.Length / 1024 / 1024:F1} MB\n" +
                                    $"Aspect: {GetAspectRatio(image.Width, image.Height)}";

                // Clean up temp file after a delay
                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000); // Keep temp file for 2 seconds
                    try { File.Delete(tempPath); } catch { }
                });
            }
            catch (Exception ex)
            {
                ImageInfoLabel.Text = $"Error loading preview: {ex.Message}";
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
            
            StartButton.IsEnabled = canStart;
            StopButton.IsEnabled = _isProcessing;
            SelectSourceButton.IsEnabled = !_isProcessing;
            SelectDestinationButton.IsEnabled = !_isProcessing;
        }

        private void ResetProgress()
        {
            ProgressBar.Progress = 0;
            ProcessedLabel.Text = "0";
            CopiedLabel.Text = "0";
            CurrentFileLabel.Text = "No file being processed";
            ImageInfoLabel.Text = "No image selected";
            PreviewImage.Source = null;
        }

        private void UpdateProgress(int processed, int total, int copied)
        {
            double percentage = total > 0 ? (double)processed / total : 0;
            ProgressBar.Progress = percentage;
            ProcessedLabel.Text = processed.ToString();
            CopiedLabel.Text = copied.ToString();
        }

        private void UpdateCurrentFile(string fileName)
        {
            CurrentFileLabel.Text = fileName;
        }

        private void UpdateStatus(string message)
        {
            StatusLabel.Text = message;
        }
    }
}
