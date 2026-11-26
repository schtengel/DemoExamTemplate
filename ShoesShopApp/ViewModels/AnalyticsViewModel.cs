using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ShoesShopApp.ViewModels;

public record TopItem(string Model, string Manufacturer, int Quantity);

public partial class AnalyticsViewModel : ObservableObject
{
    public string Header { get; }

    [ObservableProperty] private DateTime start;
    [ObservableProperty] private DateTime end;
    [ObservableProperty] private List<TopItem> top = new();
    [ObservableProperty] private int ordersCount;
    [ObservableProperty] private decimal total;
    [ObservableProperty] private int manufacturersCount;
    [ObservableProperty] private PlotModel plot = new();

    public AnalyticsViewModel(User user)
    {
        Header = $"{user.Role.RoleName}: {user.FullName}";
        start  = new DateTime(2020, 1, 1);
        end    = start.AddMonths(1).AddDays(-1);
        Load();
    }

    partial void OnStartChanged(DateTime value) => Load();
    partial void OnEndChanged(DateTime value)   => Load();

    private void Load()
    {
        var first = DateOnly.FromDateTime(Start);
        var last  = DateOnly.FromDateTime(End);

        using var db = new AppDbContext();

        var orders = db.Orders
            .Where(o => o.OrderDate >= first && o.OrderDate <= last)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Manufacturer)
            .AsNoTracking()
            .ToList();

        var items = orders.SelectMany(o => o.OrderItems).ToList();

        OrdersCount = orders.Count;
        Total = items.Sum(i => i.Quantity * i.Product.Price * (100 - i.Product.Discount) / 100);

        Top = items
            .GroupBy(i => new { i.Product.ProductName, i.Product.Manufacturer.ManufacturerName })
            .Select(g => new TopItem(g.Key.ProductName, g.Key.ManufacturerName, g.Sum(i => i.Quantity)))
            .OrderByDescending(t => t.Quantity)
            .Take(5)
            .ToList();

        ManufacturersCount = Top.Select(t => t.Manufacturer).Distinct().Count();

        var axis = new CategoryAxis { Position = AxisPosition.Bottom, Angle = -30, GapWidth = 0.3 };
        axis.Labels.AddRange(Top.Select(t => $"{t.Model} ({t.Manufacturer})"));

        var plot = new PlotModel { Title = "Top 5 Items" };
        plot.Axes.Add(axis);
        plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Количество", Minimum = 0 });
        plot.Series.Add(new BarSeries
        {
            LabelFormatString = "{0}",
            FillColor = OxyColors.SteelBlue,
            ItemsSource = Top.Select(t => new BarItem(t.Quantity)).ToList()
        });
        Plot = plot;
    }

    [RelayCommand]
    private void Export()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "PDF|*.pdf",
            FileName = $"Аналитика {Start:dd.MM.yyyy}-{End:dd.MM.yyyy}.pdf"
        };
        if (dialog.ShowDialog() != true) return;

        QuestPDF.Settings.License = LicenseType.Community;
        Document.Create(d => d.Page(p =>
        {
            p.Margin(30);
            p.DefaultTextStyle(x => x.FontFamily("Times New Roman"));
            p.Content().Column(c =>
            {
                c.Item().Text($"Отчёт за период {Start:dd.MM.yyyy} – {End:dd.MM.yyyy}").FontSize(18).Bold();
                c.Item().Text($"Заказов: {OrdersCount}, общая сумма: {Total:N2}");
                c.Item().Text($"Производителей в топ-5: {ManufacturersCount}");
                foreach (var t in Top)
                    c.Item().Text($"{t.Model}, {t.Manufacturer}: {t.Quantity} пар");
            });
        })).GeneratePdf(dialog.FileName);

        MessageBox.Show("Отчёт сохранён.", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}