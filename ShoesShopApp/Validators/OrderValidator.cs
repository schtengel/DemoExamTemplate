using FluentValidation;
using ShoesShopApp.Models;      // AppDbContext живёт здесь, а не в Services
using ShoesShopApp.ViewModels;

namespace ShoesShopApp.Validators;

public class OrderValidator : AbstractValidator<OrderFormViewModel>
{
    public OrderValidator(AppDbContext db)
    {
        RuleFor(x => x.OrderDate)
            .NotNull().WithMessage("Укажите дату заказа");

        RuleFor(x => x.DeliveryDate)
            .NotNull().WithMessage("Укажите дату выдачи")
            .Must((vm, d) => !d.HasValue || !vm.OrderDate.HasValue || d.Value >= vm.OrderDate.Value)
            .WithMessage("Дата выдачи не может быть раньше даты заказа");

        RuleFor(x => x.StatusId)
            .GreaterThan(0).WithMessage("Выберите статус заказа");

        RuleFor(x => x.PickupPointId)
            .GreaterThan(0).WithMessage("Выберите пункт выдачи");

        RuleFor(x => x.Articles)                       // было ItemsInput
            .Custom((input, ctx) =>
            {
                var parts = (input ?? "")
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0 || parts.Length % 2 != 0)
                {
                    ctx.AddFailure("Состав: укажите пары «артикул, количество» через запятую");
                    return;
                }

                for (int i = 0; i < parts.Length; i += 2)
                {
                    var art = parts[i];
                    if (!int.TryParse(parts[i + 1], out var qty) || qty <= 0)
                        ctx.AddFailure($"Количество для «{art}» должно быть целым > 0");
                    else if (!db.Products.Any(p => p.Article == art))
                        ctx.AddFailure($"Товар с артикулом «{art}» не найден");
                }
            });
    }
}