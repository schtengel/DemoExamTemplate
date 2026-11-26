using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace ShoesShopApp.ViewModels;

public static class PhotoHelper
{
    private static readonly string Dir = Path.Combine(AppContext.BaseDirectory, "Images");

    public static string Save(string src)
    {
        Directory.CreateDirectory(Dir);
        var rel = Path.Combine("Images", $"{Guid.NewGuid()}.png");
        using var img = Image.Load(src);
        img.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(300, 200), Mode = ResizeMode.Max }));
        img.Save(Path.Combine(AppContext.BaseDirectory, rel));
        return rel;
    }

    public static void Delete(string? rel)
    {
        if (string.IsNullOrEmpty(rel)) return;
        var full = Path.Combine(AppContext.BaseDirectory, rel);
        if (!File.Exists(full)) return;
        if (Path.GetExtension(full).Equals(".jpg", StringComparison.OrdinalIgnoreCase)) return; // сид-демо
        try { File.Delete(full); }
        catch (IOException)
        {
            // Image-контрол ещё держит файл — повторим через полсекунды, когда окно закроется
            Task.Delay(500).ContinueWith(_ => { try { File.Delete(full); } catch { } });
        }
    }
}