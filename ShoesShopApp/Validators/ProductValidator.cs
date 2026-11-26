using FluentValidation;
using ShoesShopApp.Services;
using ShoesShopApp.ViewModels;

namespace ShoesShopApp.Validators;

public class ProductValidator : AbstractValidator<ProductFormViewModel>
{
    public ProductValidator(AppDbContext db, int excludeId)
    {
        RuleFor(x => x.Article)
            .NotEmpty().WithMessage("Введите артикул")
            .MaximumLength(50).WithMessage("Артикул не длиннее 50 символов")
            .Must(a => !db.Products.Any(p => p.Article == a.Trim() && p.ProductId != excludeId))
            .WithMessage("Товар с таким артикулом уже существует");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Введите наименование")
            .MaximumLength(100).WithMessage("Наименование не длиннее 100 символов");

        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Выберите категорию");
        RuleFor(x => x.ManufacturerId).GreaterThan(0).WithMessage("Выберите производителя");
        RuleFor(x => x.SupplierId).GreaterThan(0).WithMessage("Выберите поставщика");

        RuleFor(x => x.Price)
            .Must(p => decimal.TryParse(p, out var v) && v >= 0)
            .WithMessage("Цена — число ≥ 0");

        RuleFor(x => x.Quantity)
            .Must(q => int.TryParse(q, out var v) && v >= 0)
            .WithMessage("Количество — целое число ≥ 0");

        RuleFor(x => x.Discount)
            .Must(d => int.TryParse(d, out var v) && v is >= 0 and <= 100)
            .WithMessage("Скидка — целое число 0..100");
    }
}