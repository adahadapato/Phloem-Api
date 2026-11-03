using FluentValidation;
using Phloem.Application.Features.Seo.Commands;
using Phloem.Application.Features.Seo.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Seo.Validators;

/// <summary>Input checks for <see cref="SaveSeoPageCommand"/>.</summary>
public sealed class SaveSeoPageCommandValidator : AbstractValidator<SaveSeoPageCommand>
{
    /// <summary>Title and description required and within Google's lengths; up to 10 short keywords.</summary>
    public SaveSeoPageCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(SeoEntry.KeyMaxLength);
        RuleFor(x => x.Title).NotEmpty().WithMessage("Write a title.")
                             .MaximumLength(SeoPages.TitleMaxLength);
        RuleFor(x => x.Description).NotEmpty().WithMessage("Write a description.")
                                   .MaximumLength(SeoPages.DescriptionMaxLength);
        RuleFor(x => x.Keywords).Must(k => k is null || k.Count <= 10).WithMessage("Use at most 10 keywords.");
        RuleForEach(x => x.Keywords).NotEmpty().MaximumLength(60);
    }
}

/// <summary>Input checks for <see cref="WriteSeoPageCommand"/>.</summary>
public sealed class WriteSeoPageCommandValidator : AbstractValidator<WriteSeoPageCommand>
{
    /// <summary>Key required.</summary>
    public WriteSeoPageCommandValidator() => RuleFor(x => x.Key).NotEmpty().MaximumLength(SeoEntry.KeyMaxLength);
}

/// <summary>Input checks for <see cref="GetSeoPageQuery"/>.</summary>
public sealed class GetSeoPageQueryValidator : AbstractValidator<GetSeoPageQuery>
{
    /// <summary>Key required.</summary>
    public GetSeoPageQueryValidator() => RuleFor(x => x.Key).NotEmpty().MaximumLength(SeoEntry.KeyMaxLength);
}

/// <summary>Input checks for <see cref="GetSeoPagesQuery"/>.</summary>
public sealed class GetSeoPagesQueryValidator : AbstractValidator<GetSeoPagesQuery>
{
    /// <summary>Filters must be real values when given.</summary>
    public GetSeoPagesQueryValidator()
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Country).IsInEnum();
        RuleFor(x => x.Search).MaximumLength(100);
    }
}

/// <summary>Input checks for <see cref="GetPublicSeoQuery"/>.</summary>
public sealed class GetPublicSeoQueryValidator : AbstractValidator<GetPublicSeoQuery>
{
    /// <summary>An address is required.</summary>
    public GetPublicSeoQueryValidator() => RuleFor(x => x.Path).NotEmpty().MaximumLength(200);
}
