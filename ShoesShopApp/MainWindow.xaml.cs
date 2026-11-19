using ShoesShopApp.Models;
using ShoesShopApp.ViewModels;
using System.Windows;

namespace ShoesShopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(User? user)
        {
            InitializeComponent();
            DataContext = new ProductsViewModel(user);
        }
    }
}