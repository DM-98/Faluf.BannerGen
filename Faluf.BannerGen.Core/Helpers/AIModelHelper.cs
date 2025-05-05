using Microsoft.Extensions.Localization;

namespace Faluf.BannerGen.Core.Helpers;

public static class AIModelHelper
{
	public static IEnumerable<AIModel> SupportedModels => Enum.GetValues<AIModel>().Cast<AIModel>();

	public static AIProvider GetProvider(this AIModel model)
	{
		return model switch
		{
			AIModel.OpenAI_GPT41 => AIProvider.OpenAI,
			AIModel.OpenAI_o3 => AIProvider.OpenAI,
			AIModel.OpenAI_o4Mini => AIProvider.OpenAI,
			AIModel.Google_Gemini25ProPreview => AIProvider.Google,
			AIModel.Google_Gemini25FlashPreview => AIProvider.Google,
			_ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
		};
	}

	public static string ToModelIdentifier(this AIModel model)
	{
		return model switch
		{
			AIModel.OpenAI_GPT41 => "gpt-4.1",
			AIModel.OpenAI_o3 => "o3",
			AIModel.OpenAI_o4Mini => "4o-mini",
			AIModel.Google_Gemini25ProPreview => "gemini-2.5-pro-preview-03-25",
			AIModel.Google_Gemini25FlashPreview => "gemini-2.5-flash-preview-04-17",
			_ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
		};
	}

	public static string ToFriendlyName(this AIModel model, IStringLocalizer stringLocalizer)
	{
		return model switch
		{
			AIModel.OpenAI_GPT41 => stringLocalizer["OpenAI GPT-4.1"],
			AIModel.OpenAI_o3 => stringLocalizer["OpenAI o3"],
			AIModel.OpenAI_o4Mini => stringLocalizer["OpenAI 4o Mini"],
			AIModel.Google_Gemini25ProPreview => stringLocalizer["Google Gemini 2.5 Pro Preview"],
			AIModel.Google_Gemini25FlashPreview => stringLocalizer["Google Gemini 2.5 Flash Preview"],
			_ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
		};
	}

	public static string ToDescription(this AIModel model, IStringLocalizer stringLocalizer)
	{
		return model switch
		{
			AIModel.OpenAI_GPT41 => stringLocalizer["Fast and best for generel purposes"],
			AIModel.OpenAI_o3 => stringLocalizer["More advanced reasoning for better outputs."],
			AIModel.OpenAI_o4Mini => stringLocalizer["A more compact version of the o3 model."],
			AIModel.Google_Gemini25ProPreview => stringLocalizer["A cutting-edge AI model with advanced reasoning."],
			AIModel.Google_Gemini25FlashPreview => stringLocalizer["A more compact version of the Gemini 2.5 Pro Preview."],
			_ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
		};
	}

	public static decimal ToCachedInputPrice(this AIModel model)
	{
		return model switch
		{
			AIModel.OpenAI_GPT41 => 0.50m,
			AIModel.OpenAI_o4Mini => 0.275m,
			AIModel.OpenAI_o3 => 2.50m,
			AIModel.Google_Gemini25ProPreview => 0.00m,
			AIModel.Google_Gemini25FlashPreview => 0.00m,
			_ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
		};
	}

	public static decimal ToInputPrice(this AIModel model)
	{
		return model switch
		{
			AIModel.OpenAI_GPT41 => 2.00m,
			AIModel.OpenAI_o4Mini => 1.10m,
			AIModel.OpenAI_o3 => 10.00m,
			AIModel.Google_Gemini25ProPreview => 1.25m,
			AIModel.Google_Gemini25FlashPreview => 0.15m,
			_ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
		};
	}

	public static decimal ToOutputPrice(this AIModel model)
	{
		return model switch
		{
			AIModel.OpenAI_GPT41 => 8.00m,
			AIModel.OpenAI_o4Mini => 4.40m,
			AIModel.OpenAI_o3 => 40.00m,
			AIModel.Google_Gemini25ProPreview => 10.00m,
			AIModel.Google_Gemini25FlashPreview => 3.50m,
			_ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
		};
	}
}