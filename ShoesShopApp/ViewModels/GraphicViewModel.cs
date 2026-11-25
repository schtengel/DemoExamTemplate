using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ShoesShopApp.Models;

namespace ShoesShopApp.ViewModels;

public partial class GraphicViewModel : ObservableObject
{
    [ObservableProperty]
    private List<Manufacturer> _manufacturers = [];

    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _endDate;

    public GraphicViewModel()
    {
        var today = DateTime.Today;
        _startDate = new DateTime(today.Year, today.Month, 1);
        _endDate = _startDate.AddMonths(1).AddDays(-1);

        Load();
    }

    partial void OnStartDateChanged(DateTime value) => Load();
    partial void OnEndDateChanged(DateTime value) => Load();

    private void Load()
    {
        using var db = new AppDbContext();

        var startLimit = DateOnly.FromDateTime(StartDate);
        var endLimit = DateOnly.FromDateTime(EndDate);

        var list = db.Manufacturers
            .Include(m => m.Products)
                .ThenInclude(p => p.OrderItems)
                    .ThenInclude(oi => oi.Order)
            .ToList();

        foreach (var m in list)
        {
            m.FilteredCount = m.Products
                .SelectMany(p => p.OrderItems)
                .Where(oi => oi.Order.OrderDate >= startLimit && oi.Order.OrderDate <= endLimit)
                .Sum(oi => oi.Quantity); 
        }

        var top5 = list
            .OrderByDescending(m => m.FilteredCount)
            .Take(5)
            .ToList();

        int maxCount = top5.FirstOrDefault()?.FilteredCount ?? 0;

        if (maxCount > 0)
        {
            foreach (var m in top5)
            {
                m.ColumnWidth = (int)(500.0 * m.FilteredCount / maxCount);
            }
        }

        Manufacturers = top5;
    }
}
