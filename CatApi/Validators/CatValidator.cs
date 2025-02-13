using CatApi.Models;
using FluentValidation;

namespace CatApi.Validators
{
    public class CatValidator : AbstractValidator<CatEntity>
    {
        public CatValidator()
        {
            RuleFor(c => c.CatId)
                .NotEmpty().WithMessage("CatId is required.");

            RuleFor(c => c.Width)
                .GreaterThan(0).WithMessage("Width must be greater than 0.");

            RuleFor(c => c.Height)
                .GreaterThan(0).WithMessage("Height must be greater than 0.");
        }
    }
}
