using FluentValidation;

namespace ModernPhysics.Web.Areas.Admin.Pages.Manage.Categories
{
    public class InputCategoryValidator : AbstractValidator<InputCategoryModel>
    {
        public InputCategoryValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty()
                .WithMessage("Pole 'Nazwa' jest wymagane")
                .MaximumLength(64)
                .WithMessage("'Nazwa' nie może być dłuższa niż 64 znaki")
                .Matches("^[a-zA-Z0-9ąęóśłżźćńĄĘÓŚŁŻŹĆŃ _-]*$")
                .WithMessage("Dozwolone są tylko duże i małe litery, cyfry, spacje, _ oraz -");

            RuleFor(p => p.FriendlyName)
                .MaximumLength(64)
                .WithMessage("Przyjazna nazwa nie może być dłuższa niż 64 znaki")
                .Matches("^[a-zA-Z0-9_-]*$")
                .WithMessage("Dozwolone są tylko duże i małe litery, cyfry, _ oraz -");

            RuleFor(p => p.Icon)
                .MaximumLength(64)
                .WithMessage("Klasa ikony nie może być dłuższa niż 64 znaki");   
        }
    }
}