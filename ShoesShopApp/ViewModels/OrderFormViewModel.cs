using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ShoesShopApp.Models;
using System.Windows;
using ShoesShopApp.Services;

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
        Points   = db.PickupPoints.AsNoTracking().ToList();
        IsEdit   = id != null;

        if (!IsEdit)
        {
            order = new Order { UserId = userId, PickupCode = Random.Shared.Next(100, 1000) };
            return;
        }

        order = db.Orders
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .Single(o => o.OrderId == id);

        Articles       = order.Articles;
        StatusId       = order.StatusId;
        PickupPointId  = order.PickupPointId;
        OrderDate      = order.OrderDate.ToDateTime(TimeOnly.MinValue);
        DeliveryDate   = order.DeliveryDate.ToDateTime(TimeOnly.MinValue);
    }

    [RelayCommand]
    private void Save(Window window)
    {
        var (items, errors) = ParseItems(Articles);

        if (StatusId == 0)      errors.Add("выберите статус заказа");
        if (PickupPointId == 0) errors.Add("выберите пункт выдачи");
        if (OrderDate == null || DeliveryDate == null)
            errors.Add("укажите дату заказа и дату выдачи");
        else if (DeliveryDate < OrderDate)
            errors.Add("дата выдачи не может быть раньше даты заказа");

        if (Msg.HasErrors(errors)) return;

        order.StatusId      = StatusId;
        order.PickupPointId = PickupPointId;
        order.OrderDate     = DateOnly.FromDateTime(OrderDate!.Value);
        order.DeliveryDate  = DateOnly.FromDateTime(DeliveryDate!.Value);

        order.OrderItems.Clear();
        foreach (var item in items) order.OrderItems.Add(item);
        if (!IsEdit) db.Orders.Add(order);

        if (!db.TrySave()) return;
        window.DialogResult = true;
    }

    [RelayCommand]
    private void Delete(Window window)
    {
        if (!IsEdit) return; // защита от удаления несохранённого заказа
        if (!Msg.Ask("Удалить заказ без возможности восстановления?")) return;

        db.Orders.Remove(order);
        if (!db.TrySave()) return;
        window.DialogResult = true;
    }

    /// <summary>
    /// Разбор строки «Артикул, Кол-во, Артикул, Кол-во». Один запрос в БД вместо N.
    /// </summary>
    private (List<OrderItem> items, List<string> errors) ParseItems(string text)
    {
        var items  = new List<OrderItem>();
        var errors = new List<string>();
        var parts  = text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0 || parts.Length % 2 != 0)
        {
            errors.Add("укажите состав парами «артикул, количество», например: А112Т4, 2, F635R4, 1");
            return (items, errors);
        }

        // один запрос вместо N× FirstOrDefault в цикле
        var articlesToFind = Enumerable.Range(0, parts.Length / 2)
                                       .Select(i => parts[i * 2])
                                       .ToHashSet();
        var products = db.Products
                         .Where(p => articlesToFind.Contains(p.Article))
                         .ToDictionary(p => p.Article);

        for (var i = 0; i < parts.Length; i += 2)
        {
            var article = parts[i];
            if (!products.TryGetValue(article, out var product))
                errors.Add($"товар {article} не найден");
            else if (!int.TryParse(parts[i + 1], out var count) || count <= 0)
                errors.Add($"количество для {article} должно быть целым числом больше 0");
            else if (items.Any(x => x.ProductId == product.ProductId))
                errors.Add($"артикул {article} указан дважды");
            else
                items.Add(new OrderItem { ProductId = product.ProductId, Quantity = count });
        }

        return (items, errors);
    }
}