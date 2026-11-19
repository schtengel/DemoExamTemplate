using ShoesShopApp.ViewModels;
using System.Windows;

namespace ShoesShopApp.Views
{
    public partial class ProductWindow : Window
    {
        public ProductWindow(int? id)
        {
            InitializeComponent();
            DataContext = new ProductFormViewModel(id);
        }
    }
}
