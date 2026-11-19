using ShoesShopApp.ViewModels;
using System.Windows;

namespace ShoesShopApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
        }
    }
}
