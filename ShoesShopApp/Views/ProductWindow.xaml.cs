using ShoesShopApp.ViewModels;
using System.Windows;

namespace ShoesShopApp.Views
{
    /// <summary>
    /// Логика взаимодействия для ProductWindow.xaml
    /// </summary>
    public partial class ProductWindow : Window
    {
        public ProductWindow(int? id)
        {
            InitializeComponent();
            DataContext = new ProductFormViewModel(id);
        }
    }
}
