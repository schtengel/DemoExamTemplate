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
        private readonly Product product;
        private readonly string? oldPhoto;

        public List<Category> Categories { get; }
        public List<Manufacturer> Manufacturers { get; }
        public List<Supplier> Suppliers { get; }
        public bool IsEdit { get; }
        public int ProductId => product.ProductId;
        public string Photo => PickedFile ?? product.Photo;

        [ObservableProperty] private string article = "";
        [ObservableProperty] private string productName = "";
        [ObservableProperty] private string description = "";
        [ObservableProperty] private string unit = "шт.";
        [ObservableProperty] private string price = "";
        [ObservableProperty] private string quantity = "";
        [ObservableProperty] private string discount = "0";
        [ObservableProperty] private int categoryId;
        [ObservableProperty] private int manufacturerId;
        [ObservableProperty] private int supplierId;
        [ObservableProperty, NotifyPropertyChangedFor(nameof(Photo))] private string? pickedFile;

        public ProductFormViewModel(int? id)
        {
            Categories = db.Categories.ToList();
            Manufacturers = db.Manufacturers.ToList();
            Suppliers = db.Suppliers.ToList();
            IsEdit = id != null;
            product = IsEdit ? db.Products.Find(id!.Value)! : new Product();
            if (!IsEdit) return;

            oldPhoto = product.PhotoPath;
            Article = product.Article;
            ProductName = product.ProductName;
            Description = product.Description ?? "";
            Unit = product.Unit;
            Price = product.Price.ToString();
            Quantity = product.Quantity.ToString();
            Discount = product.Discount.ToString();
            CategoryId = product.CategoryId;
            ManufacturerId = product.ManufacturerId;
            SupplierId = product.SupplierId;
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
            if (Article.Trim() == "") errors.Add("Введите артикул");
            else if (db.Products.Any(p => p.Article == Article.Trim() && p.ProductId != product.ProductId)) errors.Add("Товар с таким артикулом уже есть");
            if (ProductName.Trim() == "") errors.Add("Введите наименование");
            if (CategoryId == 0) errors.Add("Выберите категорию");
            if (ManufacturerId == 0) errors.Add("Выберите производителя");
            if (SupplierId == 0) errors.Add("Выберите поставщика");
            if (!decimal.TryParse(Price, out var priceValue) || priceValue < 0) errors.Add("Цена должна быть числом не меньше 0");
            if (!int.TryParse(Quantity, out var quantityValue) || quantityValue < 0) errors.Add("Количество должно быть целым числом не меньше 0");
            if (!int.TryParse(Discount, out var discountValue) || discountValue is < 0 or > 100) errors.Add("Скидка должна быть целым числом от 0 до 100");

            if (errors.Count > 0)
            {
                Msg.Warn("Исправьте ошибки:\n- " + string.Join("\n", errors));
                return;
            }

            product.Article = Article.Trim();
            product.ProductName = ProductName.Trim();
            product.Description = Description;
            product.Unit = Unit;
            product.Price = Math.Round(priceValue, 2);
            product.Quantity = quantityValue;
            product.Discount = discountValue;
            product.CategoryId = CategoryId;
            product.ManufacturerId = ManufacturerId;
            product.SupplierId = SupplierId;
            if (PickedFile != null) product.PhotoPath = PhotoHelper.Save(PickedFile);
            if (!IsEdit) db.Products.Add(product);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                Msg.Error("Не удалось сохранить товар: " + ex.GetBaseException().Message);
                return;
            }

            if (PickedFile != null) PhotoHelper.Delete(oldPhoto);
            window.DialogResult = true;
        }

        [RelayCommand]
        private void Delete(Window window)
        {
            if (db.OrderItems.Any(i => i.ProductId == product.ProductId))
            {
                Msg.Warn("Этот товар есть в заказах, поэтому его нельзя удалить.");
                return;
            }
            if (!Msg.Ask("Удалить товар без возможности восстановления?")) return;
            db.Products.Remove(product);
            db.SaveChanges();
            window.DialogResult = true;
        }
    }
}
