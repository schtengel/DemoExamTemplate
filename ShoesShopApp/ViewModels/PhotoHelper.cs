using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace ShoesShopApp.ViewModels
{
    public static class PhotoHelper
    {
        public static string Save(string file)
        {
            var path = $"Images\\{Guid.NewGuid()}.png";
            var options = new ResizeOptions { Size = new Size(300, 200), Mode = ResizeMode.Max };
            using var image = Image.Load(file);
            image.Mutate(x => x.Resize(options));
            image.Save(Path.Combine(AppContext.BaseDirectory, path));
            return path;
        }

        public static void Delete(string? path)
        {
            if(path != null) File.Delete(Path.Combine(AppContext.BaseDirectory, path));
        }

    }
}
