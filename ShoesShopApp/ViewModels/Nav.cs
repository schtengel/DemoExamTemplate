using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
