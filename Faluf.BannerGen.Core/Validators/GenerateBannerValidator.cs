using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Faluf.BannerGen.Core.Validators;

public sealed class BannerRequestValidator 
	: AbstractValidator<GenerateBannerInputModel>
{
	public BannerRequestValidator(IStringLocalizer<BannerRequestValidator> localizer)
	{
		RuleFor(x => x.Instructions)
			.NotEmpty()
			.WithMessage(localizer["Instructions are required."])
			.MaximumLength(1000)
			.WithMessage(localizer["Instructions cannot exceed 1000 characters."]);

		RuleFor(x => x.Note)
			.MaximumLength(500)
			.WithMessage(localizer["Note cannot exceed 500 characters."])
			.When(x => !string.IsNullOrWhiteSpace(x.Note));

		RuleFor(x => x.OptimizedInstructions)
			.MaximumLength(1000)
			.WithMessage(localizer["Optimized instructions cannot exceed 1000 characters."])
			.When(x => !string.IsNullOrWhiteSpace(x.OptimizedInstructions));

		RuleFor(x => x.CTAButtonText)
			.MaximumLength(100)
			.WithMessage(localizer["CTA button text cannot exceed 100 characters."])
			.When(x => !string.IsNullOrWhiteSpace(x.CTAButtonText));

		RuleFor(x => x.BannerLink)
			.Must(link => Uri.TryCreate(link, UriKind.Absolute, out _))
			.WithMessage(localizer["Banner link must be a valid absolute URL."])
			.When(x => !string.IsNullOrWhiteSpace(x.BannerLink));

		RuleFor(x => x.HexColors)
			.NotNull()
			.WithMessage(localizer["Colors cannot be null."])
			.Must(colors => colors.Count > 0)
			.WithMessage(localizer["At least one color must be specified."]);

		RuleForEach(x => x.HexColors)
			.NotEmpty()
			.WithMessage(localizer["Color cannot be empty."])
			.Matches(@"^#(?:[0-9a-fA-F]{3}){1,2}$")
			.WithMessage(localizer["Color must be a valid hex code."]);

		RuleFor(x => x.MediaFiles)
			.NotNull()
			.WithMessage(localizer["Media files cannot be null."])
			.Must(list => list.Count > 0)
			.WithMessage(localizer["At least one media file is required."]);

		RuleFor(x => x.OverlayTexts)
			.NotNull()
			.WithMessage(localizer["Overlay texts cannot be null."]);

		RuleFor(x => x.AIModel)
			.IsInEnum()
			.WithMessage(localizer["AI model is not valid."]);

		RuleFor(x => x.BannerFormats)
			.Must(list => list.Count > 0)
			.WithMessage(localizer["At least one banner format is required."]);
	}
}