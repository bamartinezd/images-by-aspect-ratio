using System;
using System.IO;
using System.Linq;
using MediaInfo;
using MediaInfo.DotNetWrapper;

namespace DeepBulkImageFiltering.ImageProcessing;

public class VideoProcessor
{
    private readonly string _sourcePath;
    private readonly string _destinationPath;
    private readonly string[] supportedExtensions = new[] { "*.avi", "*.mov", "*.mp4" };

    public VideoProcessor(string sourcePath, string destinationPath)
    {
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
    }

    public void ProcessVideos()
    {
        var filePaths = supportedExtensions
            .SelectMany(ext => Directory.EnumerateFiles(_sourcePath, ext, SearchOption.AllDirectories))
            .ToList();
        int totalVideos = filePaths.Count;
        int processedVideos = 0;
        int copiedVideos = 0;

        Console.WriteLine($"Found {totalVideos} videos to process.");
        Console.WriteLine("Processing videos...");

        foreach (var filePath in filePaths)
        {
            processedVideos++;
            bool wasCopied = ProcessVideo(filePath);
            if (wasCopied) copiedVideos++;
            UpdateProgressBar(processedVideos, totalVideos, copiedVideos);
        }

        Console.WriteLine();
        Console.WriteLine($"Processing complete! Processed {processedVideos} videos, copied {copiedVideos} videos.");
    }

    private bool ProcessVideo(string filePath)
    {
        var mediaInfoWrapper = new MediaInfoWrapper(filePath);

        if (mediaInfoWrapper.Success)
        {
            Console.WriteLine("Media info loaded successfully.");
        }

        var video = mediaInfoWrapper.VideoStreams.Count > 0 ? mediaInfoWrapper.VideoStreams[0] : null;
        if (video == null)
            return false;

        

        int width = video.Width;
        int height = video.Height;
        string displayAspectRatio = video.AspectRatio.ToString();
        string rotation = video.StreamPosition.ToString();
        string orientation = string.IsNullOrEmpty(rotation) || rotation == "0" ? "Landscape" : "Portrait";

        if (width < 3840 || height < 2160)
            return false;

        if (!string.IsNullOrEmpty(displayAspectRatio))
        {
            string destinationFile = $"{_destinationPath}/{orientation}/{displayAspectRatio}/vid-{Guid.NewGuid()}{Path.GetExtension(filePath)}";
            string? destinationDirectory = Path.GetDirectoryName(destinationFile);
            if (!string.IsNullOrEmpty(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
                File.Copy(filePath, destinationFile, true);
                return true;
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
}
