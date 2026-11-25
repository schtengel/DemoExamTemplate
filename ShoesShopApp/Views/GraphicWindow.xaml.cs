using ShoesShopApp.Models;
using ShoesShopApp.ViewModels;
using System.Windows;

namespace ShoesShopApp.Views;

public partial class GraphicWindow : Window
{
    public GraphicWindow()
    {
        InitializeComponent();
        DataContext = new GraphicViewModel();
    }
}