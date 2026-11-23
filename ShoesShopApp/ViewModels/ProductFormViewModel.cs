using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ShoesShopApp.Models;
using System.Windows;

namespace ShoesShopApp.ViewModels
{
    public partial class ProductFormViewModel : ObservableObject
    {
        private readonly AppDbContext db = new();
        private readonly string? oldPhoto;

        public Product Product { get; }
        public List<Category> Categories { get; }
        public List<Manufacturer> Manufacturers { get; }
        public List<Supplier> Suppliers { get; }
        public bool IsEdit { get; }
        public string Photo => PickedFile ?? Product.Photo;

        // Числа вводятся строками, чтобы показать понятное сообщение при неверном вводе
        [ObservableProperty] private string price = "";
        [ObservableProperty] private string quantity = "";
        [ObservableProperty] private string discount = "0";
        [ObservableProperty, NotifyPropertyChangedFor(nameof(Photo))] private string? pickedFile;


        public ProductFormViewModel(int? id)
        {
            Categories = db.Categories.ToList();
            Manufacturers = db.Manufacturers.ToList();
            Suppliers = db.Suppliers.ToList();
            IsEdit = id != null;
            Product = IsEdit ? db.Products.Find(id!.Value)! : new Product { Article = "", ProductName = "", Unit = "шт." };
            oldPhoto = Product.PhotoPath;
            if (!IsEdit) return;
            Price = Product.Price.ToString();
            Quantity = Product.Quantity.ToString();
            Discount = Product.Discount.ToString();
        }

        [RelayCommand]
        private void ChoosePhoto()
        {
            var dialog = new OpenFileDialog { Filter = "Изображения|*.jpg;*.jpeg;*.png" };
            if (dialog.ShowDialog() == true) PickedFile = dialog.FileName;
        }

        [RelayCommand]
        private void Save(Window window)
        {
            var errors = new List<string>();
            if (Product.Article.Trim() == "") errors.Add("Введите артикул");
            else if (db.Products.Any(p => p.Article == Product.Article.Trim() && p.ProductId != Product.ProductId))
                errors.Add("Товар с таким артикулом уже есть");
            if (Product.ProductName.Trim() == "") errors.Add("Введите наименование");
            if (Product.CategoryId == 0) errors.Add("Выберите категорию");
            if (Product.ManufacturerId == 0) errors.Add("Выберите производителя");
            if (Product.SupplierId == 0) errors.Add("Выберите поставщика");
            if (!decimal.TryParse(Price, out var priceValue) || priceValue < 0)
                errors.Add("Цена должна быть числом не меньше 0");
            if (!int.TryParse(Quantity, out var quantityValue) || quantityValue < 0)
                errors.Add("Количество должно быть целым числом не меньше 0");
            if (!int.TryParse(Discount, out var discountValue) || discountValue is < 0 or > 100)
                errors.Add("Скидка должна быть целым числом от 0 до 100");
            if (Msg.HasErrors(errors)) return;

            Product.Article = Product.Article.Trim();
            Product.ProductName = Product.ProductName.Trim();
            Product.Price = Math.Round(priceValue, 2);
            Product.Quantity = quantityValue;
            Product.Discount = discountValue;
            if (PickedFile != null) Product.PhotoPath = PhotoHelper.Save(PickedFile);
            if (!IsEdit) db.Products.Add(Product);
            if (!db.TrySave()) return;
            if (PickedFile != null) PhotoHelper.Delete(oldPhoto);
            window.DialogResult = true;
        }

        [RelayCommand]
        private void Delete(Window window)
        {
            if (db.OrderItems.Any(i => i.ProductId == Product.ProductId))
            {
                Msg.Warn("Этот товар есть в заказах, поэтому его нельзя удалить.");
                return;
            }
            if (!Msg.Ask("Удалить товар без возможности восстановления?")) return;
            db.Products.Remove(Product);
            if (!db.TrySave()) return;
            window.DialogResult = true;
        }
    }
}
