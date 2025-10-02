using DeepBulkImageFiltering.ImageProcessing;

public class Program
{
    public static void Main (string[] args){
        /*
            string pathToImage = @"C:\Users\Public\Documents\Camera\20240218_081621.jpg";// Windows

            /media/bamartinezd/5C40703F40702246/Users/Manager/Claro drive/Cargas Automáticas/GoogleFotos_BackUp_08102024
            /media/bamartinezd/UBUNTU 24_0/Imagenes
        */
    
        Console.WriteLine("Copy and paste here your source image directory:");
        string? sourcePath = Console.ReadLine();
        while (!Directory.Exists(sourcePath))
        {
            Console.WriteLine($"{sourcePath} is an invalid directory, try again.");
            sourcePath = Console.ReadLine();
        }

        Console.WriteLine("Copy and paste here your destination image directory:");
        string? destinationPath = Console.ReadLine();
        while (!Directory.Exists(destinationPath))
        {
            Console.WriteLine($"{destinationPath} is an invalid directory, try again.");
            destinationPath = Console.ReadLine();
        }

        // var imageProcessor = new ImageProcessor(sourcePath, destinationPath);
        // imageProcessor.ProcessImages();

        var videoProcessor = new VideoProcessor(sourcePath, destinationPath);
        videoProcessor.ProcessVideos();
    }
}