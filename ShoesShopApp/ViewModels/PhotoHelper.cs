using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace ShoesShopApp.ViewModels
{
    public static class PhotoHelper
    {
        private static readonly string ImagesDir = Path.Combine(AppContext.BaseDirectory, "Images");

        public static string Save(string sourceFile)
        {
            Directory.CreateDirectory(ImagesDir);

            var relativePath = Path.Combine("Images", $"{Guid.NewGuid()}.png");
            var fullPath     = Path.Combine(AppContext.BaseDirectory, relativePath);

            var options = new ResizeOptions
            {
                Size = new Size(300, 200),
                Mode = ResizeMode.Max
            };

            using var image = Image.Load(sourceFile);
            image.Mutate(x => x.Resize(options));
            image.Save(fullPath);
            return relativePath;
        }

        public static void Delete(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;
            var fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
    }
}