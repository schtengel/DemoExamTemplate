using ShoesShopApp.ViewModels;
using System.Windows;

namespace ShoesShopApp.Views;

public partial class OrderWindow : Window
{
    public OrderWindow(int? id, int userId)
    {
        InitializeComponent();
        DataContext = new OrderFormViewModel(id, userId);
    }
}