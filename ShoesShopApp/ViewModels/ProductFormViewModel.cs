using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ShoesShopApp.Models;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace ShoesShopApp.ViewModels;

public partial class ProductFormViewModel : ObservableObject
{
    private readonly AppDbContext db = new();
    private readonly Product product;
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
        Categories = db.Categories.AsNoTracking().ToList();
        Manufacturers = db.Manufacturers.AsNoTracking().ToList();
        Suppliers = db.Suppliers.AsNoTracking().ToList();
        IsEdit = id != null;
        product = IsEdit ? db.Products.Find(id!.Value)! : new();
        if (!IsEdit) return;
        (Article, ProductName, Description, Unit) = (product.Article, product.ProductName, product.Description ?? "", product.Unit);
        (Price, Quantity, Discount) = (product.Price.ToString(), product.Quantity.ToString(), product.Discount.ToString());
        (CategoryId, ManufacturerId, SupplierId) = (product.CategoryId, product.ManufacturerId, product.SupplierId);
    }

    [RelayCommand] private void ChoosePhoto() { var d = new OpenFileDialog { Filter = "Изображения|*.jpg;*.jpeg;*.png" }; if (d.ShowDialog() == true) PickedFile = d.FileName; }

    [RelayCommand]
    private void Save(Window w)
    {
        var e = new List<string>();
        if (string.IsNullOrWhiteSpace(Article)) e.Add("Введите артикул");
        else if (db.Products.Any(p => p.Article == Article.Trim() && p.ProductId != product.ProductId)) e.Add("Товар с таким артикулом уже есть");
        if (string.IsNullOrWhiteSpace(ProductName)) e.Add("Введите наименование");
        if (CategoryId == 0) e.Add("Выберите категорию");
        if (ManufacturerId == 0) e.Add("Выберите производителя");
        if (SupplierId == 0) e.Add("Выберите поставщика");
        if (!decimal.TryParse(Price, out var p) || p < 0) e.Add("Цена должна быть числом не меньше 0");
        if (!int.TryParse(Quantity, out var q) || q < 0) e.Add("Количество должно быть целым числом не меньше 0");
        if (!int.TryParse(Discount, out var d) || d is < 0 or > 100) e.Add("Скидка должна быть целым числом от 0 до 100");
        if (Msg.HasErrors(e)) return;

        var old = product.PhotoPath;
        var np = PickedFile != null ? PhotoHelper.Save(PickedFile) : null;
        (product.Article, product.ProductName, product.Description, product.Unit) = (Article.Trim(), ProductName.Trim(), Description, Unit);
        (product.Price, product.Quantity, product.Discount) = (Math.Round(p, 2), q, d);
        (product.CategoryId, product.ManufacturerId, product.SupplierId) = (CategoryId, ManufacturerId, SupplierId);
        if (np != null) product.PhotoPath = np;
        if (!IsEdit) db.Products.Add(product);
        if (!db.TrySave()) { if (np != null) PhotoHelper.Delete(np); return; }
        if (np != null) PhotoHelper.Delete(old);
        w.DialogResult = true;
    }

    [RelayCommand]
    private void Delete(Window w)
    {
        if (!IsEdit) return;
        if (db.OrderItems.Any(i => i.ProductId == product.ProductId)) { Msg.Warn("Этот товар есть в заказах, поэтому его нельзя удалить."); return; }
        if (!Msg.Ask("Удалить товар без возможности восстановления?")) return;
        var ph = product.PhotoPath;
        db.Products.Remove(product);
        if (!db.TrySave()) return;
        PhotoHelper.Delete(ph);
        w.DialogResult = true;
    }
}