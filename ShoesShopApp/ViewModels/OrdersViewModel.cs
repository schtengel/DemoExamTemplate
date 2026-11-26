using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ShoesShopApp.Models;
using ShoesShopApp.Views;

namespace ShoesShopApp.ViewModels;

public partial class OrdersViewModel : ObservableObject
{
    private readonly User user;
    public bool IsAdmin { get; }
    [ObservableProperty] private List<Order> orders = new();
    [ObservableProperty] private Order? selected;

    public OrdersViewModel(User user) { this.user = user; IsAdmin = user.IsAdmin; Load(); }

    private void Load()
    {
        using var db = new AppDbContext();
        var q = db.Orders.Include(o => o.Status).Include(o => o.PickupPoint).Include(o => o.OrderItems).ThenInclude(i => i.Product).AsNoTracking();
        if (!user.IsAdmin && !user.IsStaff) q = q.Where(o => o.UserId == user.UserId);
        Orders = q.OrderBy(o => o.OrderId).ToList();
    }

    private void OpenForm(int? id) { if (new OrderWindow(id, user.UserId).ShowDialog() == true) Load(); }
    partial void OnSelectedChanged(Order? v) { if (v == null) return; Application.Current.Dispatcher.BeginInvoke(() => { if (IsAdmin) OpenForm(v.OrderId); Selected = null; }); }
    [RelayCommand] private void Add() => OpenForm(null);
}