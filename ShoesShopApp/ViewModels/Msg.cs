using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ShoesShopApp.ViewModels
{
    public static class Msg
    {
        public static void Error(string text) => MessageBox.Show(text, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        public static void Warn(string text) => MessageBox.Show(text, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        public static bool Ask(string text) =>
            MessageBox.Show(text, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
