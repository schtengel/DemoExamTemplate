using System.Windows;

namespace ShoesShopApp.ViewModels
{
    public static class Nav
    {
        public static void To(Window next)
        {
            var current = Application.Current.MainWindow;
            Application.Current.MainWindow = next;
            next.Show();
            current.Close();
        }
    }
}
