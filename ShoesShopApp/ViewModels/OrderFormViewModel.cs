using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ShoesShopApp.Models;
using System.Windows;

namespace ShoesShopApp.ViewModels;

public partial class OrderFormViewModel : ObservableObject
{
    private readonly AppDbContext db = new();
    private readonly Order order;
    public List<OrderStatus> Statuses { get; }
    public List<PickupPoint> Points { get; }
    public bool IsEdit { get; }
    public int OrderId => order.OrderId;
    [ObservableProperty] private string articles = "";
    [ObservableProperty] private int statusId;
    [ObservableProperty] private int pickupPointId;
    [ObservableProperty] private DateTime? orderDate = DateTime.Today;
    [ObservableProperty] private DateTime? deliveryDate = DateTime.Today;

    public OrderFormViewModel(int? id, int userId)
    {
        Statuses = db.OrderStatuses.AsNoTracking().ToList();
        Points = db.PickupPoints.AsNoTracking().ToList();
        IsEdit = id != null;
        if (!IsEdit) { order = new() { UserId = userId, PickupCode = Random.Shared.Next(100, 1000) }; return; }
        order = db.Orders.Include(o => o.OrderItems).ThenInclude(i => i.Product).Single(o => o.OrderId == id);
        (Articles, StatusId, PickupPointId) = (order.Articles, order.StatusId, order.PickupPointId);
        (OrderDate, DeliveryDate) = (order.OrderDate.ToDateTime(TimeOnly.MinValue), order.DeliveryDate.ToDateTime(TimeOnly.MinValue));
    }

    [RelayCommand]
    private void Save(Window w)
    {
        var e = new List<string>();
        var parts = Articles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var items = new List<OrderItem>();
        if (parts.Length == 0 || parts.Length % 2 != 0) e.Add("укажите состав парами «артикул, количество»");
        else
        {
            var arts = Enumerable.Range(0, parts.Length / 2).Select(i => parts[i * 2]).ToHashSet();
            var prods = db.Products.Where(p => arts.Contains(p.Article)).ToDictionary(p => p.Article);
            for (int i = 0; i < parts.Length; i += 2)
            {
                var a = parts[i];
                if (!prods.TryGetValue(a, out var pr)) e.Add($"товар {a} не найден");
                else if (!int.TryParse(parts[i + 1], out var c) || c <= 0) e.Add($"количество для {a} должно быть целым > 0");
                else if (items.Any(x => x.ProductId == pr.ProductId)) e.Add($"артикул {a} указан дважды");
                else items.Add(new() { ProductId = pr.ProductId, Quantity = c });
            }
        }
        if (StatusId == 0) e.Add("выберите статус заказа");
        if (PickupPointId == 0) e.Add("выберите пункт выдачи");
        if (OrderDate == null || DeliveryDate == null) e.Add("укажите даты");
        else if (DeliveryDate < OrderDate) e.Add("дата выдачи не может быть раньше даты заказа");
        if (Msg.HasErrors(e)) return;

        order.StatusId = StatusId;
        order.PickupPointId = PickupPointId;
        order.OrderDate = DateOnly.FromDateTime(OrderDate!.Value);
        order.DeliveryDate = DateOnly.FromDateTime(DeliveryDate!.Value);
        order.OrderItems.Clear();
        foreach (var i in items) order.OrderItems.Add(i);
        if (!IsEdit) db.Orders.Add(order);
        if (!db.TrySave()) return;
        w.DialogResult = true;
    }

    [RelayCommand]
    private void Delete(Window w)
    {
        if (!IsEdit) return;
        if (!Msg.Ask("Удалить заказ без возможности восстановления?")) return;
        db.Orders.Remove(order);
        if (!db.TrySave()) return;
        w.DialogResult = true;
    }
}