using ShoesShopApp.Models;
using ShoesShopApp.ViewModels;
using System.Windows;

namespace ShoesShopApp.Views;

public partial class GraphicWindow : Window
{
    public GraphicWindow(User user)
    {
        InitializeComponent();
        DataContext = new GraphicViewModel(user);
    }

}