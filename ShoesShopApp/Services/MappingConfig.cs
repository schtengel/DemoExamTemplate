using Mapster;
using ShoesShopApp.Models;
using ShoesShopApp.ViewModels;

namespace ShoesShopApp.Services;

public static class MappingConfig
{
    public static void Configure()
    {
        // Модель -> ViewModel (при открытии формы)
        TypeAdapterConfig<Product, ProductFormViewModel>.NewConfig()
            .Map(d => d.Price,     s => s.Price.ToString("0.##"))
            .Map(d => d.Quantity,  s => s.Quantity.ToString())
            .Map(d => d.Discount,  s => s.Discount.ToString())
            .Map(d => d.Description, s => s.Description ?? "")
            .Map(d => d.Photo,   s => s.PhotoPath   ?? "");

        // ViewModel -> Модель (при сохранении)
        TypeAdapterConfig<ProductFormViewModel, Product>.NewConfig()
            .Map(d => d.Article,     s => s.Article.Trim())
            .Map(d => d.ProductName, s => s.ProductName.Trim())
            .Map(d => d.Price,       s => decimal.Parse(s.Price))
            .Map(d => d.Quantity,    s => int.Parse(s.Quantity))
            .Map(d => d.Discount,    s => int.Parse(s.Discount))
            .Map(d => d.Description, s => string.IsNullOrWhiteSpace(s.Description) ? null : s.Description.Trim())
            .Map(d => d.PhotoPath,   s => string.IsNullOrWhiteSpace(s.Photo)   ? null : s.Photo.Trim());
    }
}