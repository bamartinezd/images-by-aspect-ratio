using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace DeepBulkImageFiltering.ImageProcessing;

public class ImageProcessor
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;

    public ImageProcessor(string sourcePath, string destinationPath)
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
    }

    public void ProcessImages()
    {
    var supportedExtensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.tiff" };
        var filePaths = supportedExtensions
            .SelectMany(ext => Directory.EnumerateFiles(_sourcePath, ext, SearchOption.AllDirectories))
            .ToList();
        int totalImages = filePaths.Count;
        int processedImages = 0;
        int copiedImages = 0;

        Console.WriteLine($"Found {totalImages} images to process.");
        Console.WriteLine("Processing images...");

        foreach (var filePath in filePaths)
        {
            processedImages++;
            bool wasCopied = ProcessImage(filePath);
            if (wasCopied) copiedImages++;

            // Update progress bar
            UpdateProgressBar(processedImages, totalImages, copiedImages);
        }

        Console.WriteLine();
        Console.WriteLine($"Processing complete! Processed {processedImages} images, copied {copiedImages} images.");
    }

    private bool ProcessImage(string filePath)
    {
        using (var image = Image.Load(filePath))
        {
            int imageWidth = image.Width;
            int imageHeight = image.Height;

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
                    string destinationFile = $"{_destinationPath}/{orientation}/{aspectRatio}/img-{Guid.NewGuid()}.jpg";

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
        return false;
    }

    private void UpdateProgressBar(int current, int total, int copied)
    {
        int progressBarWidth = 50;
        float percentage = (float)current / total;
        int filledWidth = (int)(progressBarWidth * percentage);
        
        string progressBar = "[";
        progressBar += new string('█', filledWidth);
        progressBar += new string('░', progressBarWidth - filledWidth);
        progressBar += "]";
        
        Console.Write($"\r{progressBar} {current}/{total} ({percentage:P1}) - Copied: {copied}");
    }

    private string GetAspectRatio(double imageWidth, double imageHeight)
    {
        if (IsAspectRatioSixteenNinths(imageWidth, imageHeight)) return "16_9";
        //if (IsAspectRatioFourThirds(imageWidth, imageHeight)) return "4_3";
        //if (IsAspectRatioThreeTwo(imageWidth, imageHeight)) return "3_2";
        //if (IsAspectRatioOneToOne(imageWidth, imageHeight)) return "1_1";
        //if (IsAspectRatioTwentyOneNine(imageWidth, imageHeight)) return "21_9";
        return string.Empty;
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
}