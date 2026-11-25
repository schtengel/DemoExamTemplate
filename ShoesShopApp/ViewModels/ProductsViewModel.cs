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
        [RelayCommand] private void Graphic() => new GraphicWindow(User!).ShowDialog();

        [RelayCommand] private void Logout() => Nav.To(new LoginWindow());
    }
}
