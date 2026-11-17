using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ShoesShopApp.Models;
using ShoesShopApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoesShopApp.ViewModels
{
    public partial class ProductsViewModel : ObservableObject
    {
        private List<Product> all = new();

        public User? User { get; }
        public string UserName => User?.FullName ?? "Guest";
        public bool IsAdmin => User?.Role.RoleName == "Администратор";
        public bool IsStaff => IsAdmin || User?.Role.RoleName == "Менеджер";
        public List<Supplier> Suppliers { get; }

        [ObservableProperty, NotifyPropertyChangedFor(nameof(Products))] private string search = "";
        [ObservableProperty, NotifyPropertyChangedFor(nameof(Products))] private Supplier? selectedSupplier;
        [ObservableProperty, NotifyPropertyChangedFor(nameof(Products))] private int sortIndex;
        [ObservableProperty] private Product? selected;

        public IEnumerable<Product> Products
        {
            get
            {
                var words = Search.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var q = all.Where(p =>
                (SelectedSupplier == null || SelectedSupplier.SupplierId == 0 || p.SupplierId == SelectedSupplier.SupplierId)
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
            all = db.Products.Include(p => p.Category).Include(p => p.Manufacturer).Include(p => p.Supplier).ToList();
            OnPropertyChanged(nameof(Products));
        }

        private void OpenForm(int? id)
        {
            if (new ProductWindow(id).ShowDialog() == true) Load();
        }

        partial void OnSelectedChanged(Product? value)
        {
            if (value == null) return;
            if (IsAdmin) OpenForm(value.ProductId);
            Selected = null;
        }

        [RelayCommand] private void Add() => OpenForm(null);
        [RelayCommand] private void Orders() => new OrdersWindow(User!).ShowDialog();
        [RelayCommand] private void Logout() => Nav.To(new LoginWindow());
    }
}
