using System.Windows;
using ShoesShopApp.Models;
using ShoesShopApp.ViewModels;

namespace ShoesShopApp.Views;

public partial class OrdersWindow : Window
{
    public OrdersWindow(User user)
    {
        InitializeComponent();
        DataContext = new OrdersViewModel(user);
    }
}