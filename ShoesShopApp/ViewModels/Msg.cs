using System.Windows;

namespace ShoesShopApp.ViewModels;

public static class Msg
{
    public static void Error(string t) => MessageBox.Show(t, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    public static void Warn(string t) => MessageBox.Show(t, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
    public static bool Ask(string t) => MessageBox.Show(t, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    public static bool HasErrors(List<string> e)
    {
        if (e.Count == 0) return false;
        Warn("Исправьте ошибки:\n" + string.Join("\n", e));
        return true;
    }
}