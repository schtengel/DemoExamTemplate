using Microsoft.EntityFrameworkCore;
using ShoesShopApp.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ShoesShopApp.Views;

public partial class AnalyticsWindow : Window
{
    private readonly User user;

    private List<(string Name, string Manufacturer, int Quantity)> topProducts = new();

    private int ordersCount;
    private decimal totalSum;

    public AnalyticsWindow(User user)
    {
        InitializeComponent();

        this.user = user;
        UserInfoText.Text = $"{user.Role.RoleName}: {user.FullName}";

        var today = DateTime.Today;

        StartDatePicker.SelectedDate = new DateTime(today.Year, today.Month, 1);
        EndDatePicker.SelectedDate = today;
    }

    private void DateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StartDatePicker.SelectedDate == null ||
            EndDatePicker.SelectedDate == null)
            return;

        UpdateAnalytics();
    }

    private void UpdateAnalytics()
    {
        var start = DateOnly.FromDateTime(StartDatePicker.SelectedDate!.Value);
        var end = DateOnly.FromDateTime(EndDatePicker.SelectedDate!.Value);

        if (start > end)
        {
            OrdersCountText.Text = "—";
            ManufacturerCountText.Text = "—";
            TotalSumText.Text = "—";
            topProducts.Clear();

            ChartPanel.Children.Clear();
            ChartPanel.Children.Add(new TextBlock
            {
                Text = "Дата начала не может быть позже даты окончания."
            });

            return;
        }

        using var db = new AppDbContext();

        var orders = db.Orders
            .Where(o => o.OrderDate >= start && o.OrderDate <= end)
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p.Manufacturer)
            .ToList();

        ordersCount = orders.Count;

        totalSum = orders.Sum(o =>
            o.OrderItems.Sum(i => i.Quantity * i.Product.FinalPrice));

        topProducts = orders
            .SelectMany(o => o.OrderItems)
            .GroupBy(i => new
            {
                i.Product.ProductName,
                i.Product.Manufacturer.ManufacturerName
            })
            .Select(g => (
                Name: g.Key.ProductName,
                Manufacturer: g.Key.ManufacturerName,
                Quantity: g.Sum(i => i.Quantity)
            ))
            .OrderByDescending(p => p.Quantity)
            .Take(5)
            .ToList();

        OrdersCountText.Text = ordersCount.ToString();

        ManufacturerCountText.Text = topProducts
            .Select(p => p.Manufacturer)
            .Distinct()
            .Count()
            .ToString();

        TotalSumText.Text = $"{totalSum:N2} ₽";

        UpdateChart();
    }

    private void UpdateChart()
    {
        ChartPanel.Children.Clear();

        if (topProducts.Count == 0)
        {
            ChartPanel.Children.Add(new TextBlock
            {
                Text = "За выбранный период заказов нет."
            });

            return;
        }

        var max = topProducts.Max(p => p.Quantity);

        foreach (var product in topProducts)
        {
            var row = new Grid { Margin = new Thickness(0, 5, 0, 5) };

            row.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(2, GridUnitType.Star)
            });

            row.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(3, GridUnitType.Star)
            });

            row.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto
            });

            var name = new TextBlock
            {
                Text = $"{product.Name} ({product.Manufacturer})",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var bar = new ProgressBar
            {
                Minimum = 0,
                Maximum = max,
                Value = product.Quantity,
                Height = 18,
                VerticalAlignment = VerticalAlignment.Center
            };

            var count = new TextBlock
            {
                Text = $"{product.Quantity} пар",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            Grid.SetColumn(name, 0);
            Grid.SetColumn(bar, 1);
            Grid.SetColumn(count, 2);

            row.Children.Add(name);
            row.Children.Add(bar);
            row.Children.Add(count);

            ChartPanel.Children.Add(row);
        }
    }

    private void ExportPdf_Click(object sender, RoutedEventArgs e)
    {
        var document = new FlowDocument
        {
            FontSize = 14,
            PagePadding = new Thickness(40),
            ColumnWidth = double.PositiveInfinity
        };

        document.Blocks.Add(new Paragraph(
            new Run("Отчёт по аналитике заказов"))
        {
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        document.Blocks.Add(new Paragraph(
            new Run($"Администратор: {user.FullName} ({user.Role.RoleName})")));

        document.Blocks.Add(new Paragraph(
            new Run($"Период: {StartDatePicker.SelectedDate:dd.MM.yyyy} — {EndDatePicker.SelectedDate:dd.MM.yyyy}")));

        document.Blocks.Add(new Paragraph(
            new Run($"Количество заказов: {ordersCount}")));

        document.Blocks.Add(new Paragraph(
            new Run($"Производителей в топ-5: {topProducts.Select(p => p.Manufacturer).Distinct().Count()}")));

        document.Blocks.Add(new Paragraph(
            new Run($"Общая сумма: {totalSum:N2} ₽")));

        document.Blocks.Add(new Paragraph(new Run("Топ-5 моделей:"))
        {
            FontWeight = FontWeights.Bold
        });

        for (int i = 0; i < topProducts.Count; i++)
        {
            var p = topProducts[i];

            document.Blocks.Add(new Paragraph(
                new Run($"{i + 1}. {p.Name} — {p.Manufacturer}: {p.Quantity} пар")));
        }

        var dialog = new PrintDialog();

        if (dialog.ShowDialog() != true)
            return;

        document.PageWidth = dialog.PrintableAreaWidth;
        document.PageHeight = dialog.PrintableAreaHeight;

        dialog.PrintDocument(
            ((IDocumentPaginatorSource)document).DocumentPaginator,
            "Отчёт по аналитике");
    }
}