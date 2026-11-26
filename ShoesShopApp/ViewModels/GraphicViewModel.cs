using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DemEKZZ.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Diagnostics;
using System.Windows;

namespace ShoesShopApp.ViewModels;

public partial class GraphicViewModel : ObservableObject
{
    public User? User { get; }
    public string UserName => User is null ? "Гость" : $"{User.FullName} | {User.Role?.RoleName}";

    [ObservableProperty] private List<TopModelItem> _topModels = [];
    [ObservableProperty] private int _top5ManufacturersCount;
    [ObservableProperty] private int _totalOrdersCount;
    [ObservableProperty] private decimal _totalOrdersSum;

    [ObservableProperty] private DateTime _startDate;
    [ObservableProperty] private DateTime _endDate;

    public GraphicViewModel(User? user)
    {
        User = user;
        QuestPDF.Settings.License = LicenseType.Community;

        var today = DateTime.Today;
        _startDate = new DateTime(today.Year, today.Month, 1);
        _endDate = _startDate.AddMonths(1).AddDays(-1);

        Load();
    }

    partial void OnStartDateChanged(DateTime value) => Load();
    partial void OnEndDateChanged(DateTime value) => Load();

    private void Load()
    {
        var startLimit = DateOnly.FromDateTime(StartDate);
        var endLimit = DateOnly.FromDateTime(EndDate);

        using var db = new AppDbContext();

        var periodOrders = db.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Manufacturer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category) 
            .Where(o => o.OrderDate >= startLimit && o.OrderDate <= endLimit)
            .ToList();


        var top5 = periodOrders
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => new
            {
                oi.ProductId,
                ModelName = oi.Product.Title,
                ManufacturerName = oi.Product.Manufacturer.ManufacturerName
            })
            .Select(g => new TopModelItem
            {
                ModelName = g.Key.ModelName,
                ManufacturerName = g.Key.ManufacturerName,
                TotalPairs = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(x => x.TotalPairs)
            .Take(5)
            .ToList();

        int maxPairs = top5.FirstOrDefault()?.TotalPairs ?? 0;
        if (maxPairs > 0)
        {
            foreach (var item in top5)
            {
                item.ColumnWidth = (int)(420.0 * item.TotalPairs / maxPairs);
            }
        }

        TopModels = top5;

        Top5ManufacturersCount = top5
            .Select(x => x.ManufacturerName)
            .Distinct()
            .Count();

        TotalOrdersCount = periodOrders.Count;

        TotalOrdersSum = periodOrders
            .SelectMany(o => o.OrderItems)
            .Sum(oi => oi.Product.Price * (1 - oi.Product.Discount / 100m) * oi.Quantity);
    }

    [RelayCommand]
    private void ExportPdf()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "PDF документы (*.pdf)|*.pdf",
            FileName = $"Аналитика_{StartDate:dd.MM.yyyy}_{EndDate:dd.MM.yyyy}.pdf"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Column(header =>
                    {
                        header.Item().Text($"Аналитический отчёт ({StartDate:dd.MM.yyyy} — {EndDate:dd.MM.yyyy})").SemiBold().FontSize(16);
                        header.Item().Text($"Сформировал: {UserName}").FontSize(10).FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(15).Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(summary =>
                        {
                            summary.Spacing(4);
                            summary.Item().Text($"Общее количество заказов за период: {TotalOrdersCount}").SemiBold();
                            summary.Item().Text($"Общая сумма заказов за период: {TotalOrdersSum:N2} руб").SemiBold();
                            summary.Item().Text($"Общее количество производителей топ-5: {Top5ManufacturersCount}").SemiBold();
                        });

                        col.Item().Text("Топ-5 моделей обуви в разрезе производителей:").SemiBold().FontSize(14);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(35);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.5f);
                                columns.ConstantColumn(110);
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).Padding(5).Text("№").SemiBold();
                                header.Cell().BorderBottom(1).Padding(5).Text("Модель").SemiBold();
                                header.Cell().BorderBottom(1).Padding(5).Text("Производитель").SemiBold();
                                header.Cell().BorderBottom(1).Padding(5).AlignRight().Text("Заказано пар").SemiBold();
                            });

                            int index = 1;
                            foreach (var item in TopModels)
                            {
                                table.Cell().BorderBottom(0.5f).Padding(5).Text(index++.ToString());
                                table.Cell().BorderBottom(0.5f).Padding(5).Text(item.ModelName);
                                table.Cell().BorderBottom(0.5f).Padding(5).Text(item.ManufacturerName);
                                table.Cell().BorderBottom(0.5f).Padding(5).AlignRight().Text(item.TotalPairs.ToString());
                            }
                        });
                    });

                    page.Footer().AlignRight().Text($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10);
                });
            })
            .GeneratePdf(dialog.FileName);

            Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
