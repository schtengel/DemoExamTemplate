using System.Windows;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace ShoesShopApp.ViewModels;

public partial class OrderFormViewModel : ObservableObject
{
    private readonly AppDbContext db = new();

    public Order Order { get; }
    public List<OrderStatus> Statuses { get; }
    public List<PickupPoint> Points { get; }
    public bool IsEdit { get; }

    [ObservableProperty] private string articles = "";
    [ObservableProperty] private DateTime? orderDate = DateTime.Today;
    [ObservableProperty] private DateTime? deliveryDate = DateTime.Today;

    public OrderFormViewModel(int? id, int userId)
    {
        Statuses = db.OrderStatuses.ToList();
        Points = db.PickupPoints.ToList();
        IsEdit = id != null;
        Order = IsEdit
            ? db.Orders.Include(o => o.OrderItems).ThenInclude(i => i.Product).Single(o => o.OrderId == id)
            : new Order { UserId = userId, PickupCode = Random.Shared.Next(100, 1000) };
        if (!IsEdit) return;
        Articles = Order.Articles;
        OrderDate = Order.OrderDate.ToDateTime(TimeOnly.MinValue);
        DeliveryDate = Order.DeliveryDate.ToDateTime(TimeOnly.MinValue);
    }

    [RelayCommand]
    private void Save(Window window)
    {
        var errors = new List<string>();
        var items = new List<OrderItem>();
        var parts = Articles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || parts.Length % 2 != 0)
            errors.Add("Укажите состав парами «артикул, количество», например: А112Т4, 2, F635R4, 1");
        else
            for (var i = 0; i < parts.Length; i += 2)
            {
                var article = parts[i];
                var product = db.Products.FirstOrDefault(p => p.Article == article);
                if (product == null) errors.Add($"Товар {article} не найден");
                else if (!int.TryParse(parts[i + 1], out var count) || count <= 0)
                    errors.Add($"Количество для {article} должно быть целым числом больше 0");
                else if (items.Any(x => x.ProductId == product.ProductId)) errors.Add($"Артикул {article} указан дважды");
                else items.Add(new OrderItem { ProductId = product.ProductId, Quantity = count });
            }
        if (Order.StatusId == 0) errors.Add("Выберите статус заказа");
        if (Order.PickupPointId == 0) errors.Add("Выберите пункт выдачи");
        if (OrderDate == null || DeliveryDate == null) errors.Add("Укажите дату заказа и дату выдачи");
        else if (DeliveryDate < OrderDate) errors.Add("Дата выдачи не может быть раньше даты заказа");
        if (Msg.HasErrors(errors)) return;

        Order.OrderDate = DateOnly.FromDateTime(OrderDate!.Value);
        Order.DeliveryDate = DateOnly.FromDateTime(DeliveryDate!.Value);
        Order.OrderItems.Clear();
        foreach (var item in items) Order.OrderItems.Add(item);
        if (!IsEdit) db.Orders.Add(Order);
        if (!db.TrySave()) return;
        window.DialogResult = true;
    }

    [RelayCommand]
    private void Delete(Window window)
    {
        if (!Msg.Ask("Удалить заказ без возможности восстановления?")) return;
        db.Orders.Remove(Order);
        if (!db.TrySave()) return;
        window.DialogResult = true;
    }
}