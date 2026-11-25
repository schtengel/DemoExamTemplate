using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShoesShopApp.Views;

namespace ShoesShopApp.ViewModels
{
    public partial class ProductsViewModel : ObservableObject
    {
        private List<Product> _all = [];

        public User? User { get; }
        public string UserName => (User?.FullName ?? "Гость") + (User is null ? string.Empty : $" | {User?.Role.RoleName}");
        public bool IsAdmin => User?.IsAdmin == true;
        public bool IsStaff => User?.IsStaff == true;
        public List<Supplier> Suppliers { get; }

        [ObservableProperty,
            NotifyPropertyChangedFor(nameof(Products))]
        private string search = "";
        [ObservableProperty,
            NotifyPropertyChangedFor(nameof(Products))]
        private Supplier? selectedSupplier;
        [ObservableProperty,
            NotifyPropertyChangedFor(nameof(Products))]
        private int sortIndex;

        [ObservableProperty] private Product? selected;

        [ObservableProperty]
        private DateTime _startDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);

        [ObservableProperty]
        private DateTime _endDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);

        public IEnumerable<Product> Products
        {
            get
            {
                var id = SelectedSupplier?.SupplierId ?? 0;
                var words = Search.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var q = _all.Where(p =>
                (id == 0 || p.SupplierId == id)
                && words.All(w => p.SearchText.Contains(w, StringComparison.OrdinalIgnoreCase)));
                return SortIndex switch
                {
                    1 => q.OrderBy(p => p.Quantity),
                    2 => q.OrderByDescending(p => p.Quantity),
                    _ => q
                };
            }
        }

        public ProductsViewModel(User? user)
        {
            User = user;
            using var db = new AppDbContext();
            Suppliers = new List<Supplier> { new() { SupplierName = "Все поставщики" } }.Concat(db.Suppliers).ToList();
            SelectedSupplier = Suppliers[0];
            Load();
        }

        private void Load()
        {
            using var db = new AppDbContext();
            _all = db.Products.Include(p => p.Category).Include(p => p.Manufacturer).Include(p => p.Supplier).ToList();
            OnPropertyChanged(nameof(Products));
        }

        private void OpenForm(int? id)
        {
            if (new ProductWindow(id).ShowDialog() == true) Load();
        }

        partial void OnSelectedChanged(Product? value)
        {
            if (value == null) return;
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (IsAdmin) OpenForm(value.ProductId);
                Selected = null;
            });
        }

        [RelayCommand] private void Add() => OpenForm(null);
        [RelayCommand] private void Orders() => new OrdersWindow(User!).ShowDialog();
        [RelayCommand] private void Graphic() => new GraphicWindow().ShowDialog();
        [RelayCommand] private void Logout() => Nav.To(new LoginWindow());

        [RelayCommand]
        private void ExportPdf()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PDF документы (*.pdf)|*.pdf",
                FileName = $"Отчет_заказы_{StartDate:dd.MM.yyyy}_{EndDate:dd.MM.yyyy}.pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                var startLimit = DateOnly.FromDateTime(StartDate);
                var endLimit = DateOnly.FromDateTime(EndDate);

                using var db = new AppDbContext();

                var reportData = db.Manufacturers
                    .Include(m => m.Products)
                        .ThenInclude(p => p.OrderItems)
                            .ThenInclude(oi => oi.Order)
                    .ToList()
                    .Select(m => new
                    {
                        Name = m.ManufacturerName,
                        Count = m.Products
                            .SelectMany(p => p.OrderItems)
                            .Where(oi => oi.Order.OrderDate >= startLimit && oi.Order.OrderDate <= endLimit)
                            .Sum(oi => oi.Quantity)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header()
                            .PaddingBottom(15)
                            .Text($"Отчёт за период с {StartDate:dd.MM.yyyy} по {EndDate:dd.MM.yyyy}")
                            .SemiBold().FontSize(16);

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn();
                                columns.ConstantColumn(120);
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).Padding(5).Text("№").SemiBold();
                                header.Cell().BorderBottom(1).Padding(5).Text("Производитель").SemiBold();
                                header.Cell().BorderBottom(1).Padding(5).AlignRight().Text("Количество").SemiBold();
                            });

                            int index = 1;
                            foreach (var item in reportData)
                            {
                                table.Cell().BorderBottom(0.5f).Padding(5).Text(index++.ToString());
                                table.Cell().BorderBottom(0.5f).Padding(5).Text(item.Name);
                                table.Cell().BorderBottom(0.5f).Padding(5).AlignRight().Text(item.Count.ToString());
                            }
                        });

                        page.Footer()
                            .AlignRight()
                            .Text($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(10);
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
}
