class Program
{
    static async Task Main(string[] args)
    {
        string[] imagePaths = { "image1.jpg", "image2.jpg", "image3.jpg" };

        try
        {
            var processedImages = await ProcessImagesAsync(imagePaths);
            await UploadImagesAsync(processedImages);
            Log("All images processed and uploaded successfully.");
        }
        catch (Exception ex)
        {
            LogError("An error occurred during the pipeline.", ex);
        }
    }

    static async Task<List<string>> ProcessImagesAsync(string[] imagePaths)
    {
        var tasks = imagePaths.Select(path => Task.Run(() => ApplyImageFilters(path)));
        return (await Task.WhenAll(tasks)).ToList();
    }

    static async Task UploadImagesAsync(List<string> processedImages)
    {
        var uploadTasks = processedImages.Select(img => Task.Run(() => UploadImage(img)));
        await Task.WhenAll(uploadTasks);
    }

    static string ApplyImageFilters(string imagePath)
    {
        // Simulate filter application
        Log($"Applying filters to {imagePath}");
        return $"{imagePath}.processed";
    }

    static void UploadImage(string processedImagePath)
    {
        // Simulate upload
        Log($"Uploading {processedImagePath}");
    }

    static void Log(string message) => Console.WriteLine($"[LOG] {message}");
    static void LogError(string message, Exception ex) => Console.WriteLine($"[ERROR] {message}: {ex}");
}
