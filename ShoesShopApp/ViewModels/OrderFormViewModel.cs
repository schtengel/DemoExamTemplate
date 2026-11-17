using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ShoesShopApp.Models;

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
        Statuses = db.OrderStatuses.ToList();
        Points = db.PickupPoints.ToList();
        IsEdit = id != null;
        if (!IsEdit)
        {
            // Клиент нового заказа: текущий пользователь, код получения генерируется
            order = new Order { UserId = userId, PickupCode = Random.Shared.Next(100, 1000) };
            return;
        }

        order = db.Orders.Include(o => o.OrderItems).ThenInclude(i => i.Product).Single(o => o.OrderId == id);
        Articles = order.Articles;
        StatusId = order.StatusId;
        PickupPointId = order.PickupPointId;
        OrderDate = order.OrderDate.ToDateTime(TimeOnly.MinValue);
        DeliveryDate = order.DeliveryDate.ToDateTime(TimeOnly.MinValue);
    }

    [RelayCommand]
    private void Save(Window window)
    {
        var errors = new List<string>();
        var items = new List<OrderItem>();

        // Состав заказа вводится парами «артикул, количество»
        var parts = Articles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || parts.Length % 2 != 0)
            errors.Add("укажите состав парами «артикул, количество», например: А112Т4, 2, F635R4, 1");
        else
            for (var i = 0; i < parts.Length; i += 2)
            {
                var article = parts[i];
                var product = db.Products.FirstOrDefault(p => p.Article == article);
                if (product == null) errors.Add($"товар {article} не найден");
                else if (!int.TryParse(parts[i + 1], out var count) || count <= 0)
                    errors.Add($"количество для {article} должно быть целым числом больше 0");
                else if (items.Any(x => x.ProductId == product.ProductId)) errors.Add($"артикул {article} указан дважды");
                else items.Add(new OrderItem { ProductId = product.ProductId, Quantity = count });
            }

        if (StatusId == 0) errors.Add("выберите статус заказа");
        if (PickupPointId == 0) errors.Add("выберите пункт выдачи");
        if (OrderDate == null || DeliveryDate == null) errors.Add("укажите дату заказа и дату выдачи");
        else if (DeliveryDate < OrderDate) errors.Add("дата выдачи не может быть раньше даты заказа");

        if (errors.Count > 0)
        {
            Msg.Warn("Исправьте ошибки:\n- " + string.Join("\n- ", errors));
            return;
        }

        order.StatusId = StatusId;
        order.PickupPointId = PickupPointId;
        order.OrderDate = DateOnly.FromDateTime(OrderDate!.Value);
        order.DeliveryDate = DateOnly.FromDateTime(DeliveryDate!.Value);
        order.OrderItems.Clear();
        foreach (var item in items) order.OrderItems.Add(item);
        if (!IsEdit) db.Orders.Add(order);

        try { db.SaveChanges(); }
        catch (Exception ex)
        {
            Msg.Error("Не удалось сохранить заказ: " + (ex.InnerException ?? ex).Message);
            return;
        }
        window.DialogResult = true;
    }

    [RelayCommand]
    private void Delete(Window window)
    {
        if (!Msg.Ask("Удалить заказ без возможности восстановления?")) return;
        db.Orders.Remove(order);
        db.SaveChanges();
        window.DialogResult = true;
    }
}