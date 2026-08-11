namespace TestadorCLPHI.App.Ui.Controls;

internal static class IndustrialAssetCache
{
    private static readonly object Sync = new();
    private static readonly Dictionary<string, Image> Images =
        new(StringComparer.OrdinalIgnoreCase);
    private static bool _disposed;

    static IndustrialAssetCache()
    {
        Application.ApplicationExit += (_, _) => DisposeAll();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => DisposeAll();
    }

    internal static Image? Get(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            return null;
        }

        string fullPath = Path.GetFullPath(imagePath);
        lock (Sync)
        {
            if (_disposed)
            {
                return null;
            }

            if (Images.TryGetValue(fullPath, out Image? cached))
            {
                return cached;
            }

            using FileStream stream = File.OpenRead(fullPath);
            using Image loaded = Image.FromStream(stream);
            Image image = new Bitmap(loaded);
            Images.Add(fullPath, image);
            return image;
        }
    }

    private static void DisposeAll()
    {
        lock (Sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            foreach (Image image in Images.Values)
            {
                image.Dispose();
            }

            Images.Clear();
        }
    }
}
