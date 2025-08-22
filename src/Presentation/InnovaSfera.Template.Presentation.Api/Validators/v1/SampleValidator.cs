using DomainDrivenDesign.Application.Entities;
using FluentValidation;

namespace InnovaSfera.Template.Presentation.Api.Validators.v1;

[ExcludeFromDescription]

public class SampleValidator : AbstractValidator<SampleDataDto>
{
    public SampleValidator()
    {
        RuleFor(x => x.Message)
           .NotEmpty()
           .WithMessage("Please enter with message")
           .NotNull()
           .WithMessage("Please enter with message");
    }
}
