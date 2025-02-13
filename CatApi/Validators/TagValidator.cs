using CatApi.Models;
using FluentValidation;

namespace CatApi.Validators
{
    public class TagValidator : AbstractValidator<TagEntity>
    {
        public TagValidator()
        {
            RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Tag name is required.")
                .MaximumLength(50).WithMessage("Tag name must be less than 50 characters.");
        }
    }
}
