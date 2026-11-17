using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ShoesShopApp.Models;
using ShoesShopApp.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ShoesShopApp.ViewModels;

public partial class OrdersViewModel : ObservableObject
{
    private readonly User user;
    public bool IsAdmin { get; }

    [ObservableProperty] private List<Order> orders = new();
    [ObservableProperty] private Order? selected;

    public OrdersViewModel(User user)
    {
        this.user = user;
        IsAdmin = user.Role.RoleName == "Администратор";
        Load();
    }

    private void Load()
    {
        using var db = new AppDbContext();
        Orders = db.Orders
            .Include(o => o.Status)
            .Include(o => o.PickupPoint)
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .OrderBy(o => o.OrderId)
            .ToList();
    }

    private void OpenForm(int? id)
    {
        if (new OrderWindow(id, user.UserId).ShowDialog() == true) Load();
    }

    partial void OnSelectedChanged(Order? value)
    {
        if (value == null) return;
        if (IsAdmin) OpenForm(value.OrderId);
        Selected = null;
    }

    [RelayCommand] private void Add() => OpenForm(null);
}