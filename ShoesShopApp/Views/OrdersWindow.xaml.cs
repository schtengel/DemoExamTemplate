using ShoesShopApp.Models;
using ShoesShopApp.ViewModels;
using System.Windows;

namespace ShoesShopApp.Views;

public partial class OrdersWindow : Window
{
    public OrdersWindow(User user)
    {
        InitializeComponent();
        DataContext = new OrdersViewModel(user);
    }
}