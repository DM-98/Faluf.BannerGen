using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;
using ChatResponseFormat = OpenAI.Chat.ChatResponseFormat;

namespace Faluf.BannerGen.Infrastructure.Services;

public sealed class BannerService(IStringLocalizer<BannerService> stringLocalizer, IConfiguration configuration, ILogger<BannerService> logger, IHttpClientFactory httpClientFactory, Kernel kernel, IDbContextFactory<AppDataContext> dbContextFactory)
	: IBannerService
{
	public async IAsyncEnumerable<ResponseDTO<Banner>> GenerateBannersAsync(GenerateBannerInputModel generateBannerInputModel, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await using AppDataContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
		IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>(generateBannerInputModel.AIModel.ToModelIdentifier());
		HttpClient apiClient = httpClientFactory.CreateClient("API");

		BannerRequest? bannerRequest = await context.BannerRequests.AsTracking().FirstOrDefaultAsync(b => b.Id == generateBannerInputModel.BannerRequestId, cancellationToken).ConfigureAwait(false);
		bool isNewBannerRequest = bannerRequest is null;

		bannerRequest ??= new();
		bannerRequest.AIModel = generateBannerInputModel.AIModel;
		bannerRequest.BannerLink = generateBannerInputModel.BannerLink;
		bannerRequest.CTAButtonText = generateBannerInputModel.CTAButtonText;
		bannerRequest.HexColors = generateBannerInputModel.HexColors;
		bannerRequest.Instructions = generateBannerInputModel.Instructions;
		bannerRequest.Note = generateBannerInputModel.Note;
		bannerRequest.BannerFormats = generateBannerInputModel.BannerFormats;

		if (isNewBannerRequest)
		{
			await context.BannerRequests.AddAsync(bannerRequest, cancellationToken).ConfigureAwait(false);
		}

		await context.OverlayTexts.Where(x => x.BannerRequestId == bannerRequest.Id).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

		foreach (OverlayTextInputModel overlayTextInputModel in generateBannerInputModel.OverlayTexts)
		{
			OverlayText overlayTextToAdd = new()
			{
				BannerRequestId = bannerRequest.Id,
				Text = overlayTextInputModel.Text,
				FontSize = overlayTextInputModel.FontSize,
				FontFamilyUrl = overlayTextInputModel.FontFamilyUrl,
				FontWeight = overlayTextInputModel.FontWeight,
				Color = overlayTextInputModel.Color
			};

			await context.OverlayTexts.AddAsync(overlayTextToAdd, cancellationToken).ConfigureAwait(false);
		}

		await context.BannerFiles.Where(x => x.BannerRequestId == bannerRequest.Id).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

		foreach (BannerFileInputModel bannerFileInputModel in generateBannerInputModel.MediaFiles)
		{
			BannerFile bannerFileToAdd = new()
			{
				BannerRequestId = bannerRequest.Id,
				Description = bannerFileInputModel.Description,
				Path = bannerFileInputModel.Path
			};

			await context.BannerFiles.AddAsync(bannerFileToAdd, cancellationToken).ConfigureAwait(false);
		}

		if (generateBannerInputModel.IsOptimizeInstructionsEnabled)
		{
			ChatHistory chatHistory = new(systemMessage: PromptHelper.PromptOptimizer);
			chatHistory.AddUserMessage(generateBannerInputModel.Instructions);

			ChatMessageContent result = await chatCompletionService.GetChatMessageContentAsync(chatHistory, new OpenAIPromptExecutionSettings()
			{
				ModelId = AIModel.OpenAI_GPT41.ToModelIdentifier(),
				MaxTokens = 800,
				ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
			}, kernel, cancellationToken).ConfigureAwait(false);

			generateBannerInputModel.OptimizedInstructions = JsonSerializer.Deserialize<string>(result.ToString(), JsonHelper.ForgivingJsonSerializerOptions)!;
		}

		await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

		PromptExecutionSettings settings;

		if (generateBannerInputModel.AIModel.GetProvider() is AIProvider.OpenAI)
		{
			settings = new OpenAIPromptExecutionSettings()
			{
				ModelId = generateBannerInputModel.AIModel.ToModelIdentifier(),
				MaxTokens = 32768,
				ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
			};
		}
		else
		{
			settings = new GeminiPromptExecutionSettings()
			{
				ModelId = generateBannerInputModel.AIModel.ToModelIdentifier(),
				MaxTokens = 32768,
				ResponseMimeType = "application/json"
			};
		}

		foreach (BannerFormat bannerFormat in generateBannerInputModel.BannerFormats)
		{
			for (int i = 0; i < generateBannerInputModel.GenerateAmount; i++)
			{
				StringBuilder promptTemplateBuilder = new();

				promptTemplateBuilder.AppendLine("The instructions are as follows:");
				promptTemplateBuilder.AppendLine(generateBannerInputModel.IsOptimizeInstructionsEnabled ? generateBannerInputModel.OptimizedInstructions : generateBannerInputModel.Instructions);

				promptTemplateBuilder.AppendLine("The banner format is as follows:");
				promptTemplateBuilder.AppendLine(bannerFormat.ToString());

				promptTemplateBuilder.AppendLine("The colors to choose from are as follows:");
				promptTemplateBuilder.AppendLine(string.Join(", ", generateBannerInputModel.HexColors));

				promptTemplateBuilder.AppendLine("The media files are as follows:");
				foreach ((int Index, BannerFileInputModel MediaFile) in generateBannerInputModel.MediaFiles.Index())
				{
					promptTemplateBuilder.AppendLine($"---");
					promptTemplateBuilder.AppendLine($"File #{Index + 1}");
					promptTemplateBuilder.AppendLine($"Description: {MediaFile.Description}");
					promptTemplateBuilder.AppendLine($"Src: {MediaFile.Path}");
					promptTemplateBuilder.AppendLine($"---");
				}

				promptTemplateBuilder.AppendLine("The overlay texts are as follows:");
				foreach ((int Index, OverlayTextInputModel OverlayText) in generateBannerInputModel.OverlayTexts.Index())
				{
					promptTemplateBuilder.AppendLine($"---");
					promptTemplateBuilder.AppendLine($"Overlay Text #{Index + 1}");
					promptTemplateBuilder.AppendLine($"Text: {OverlayText.Text}");
					promptTemplateBuilder.AppendLine($"Font Size: {OverlayText.FontSize}");
					promptTemplateBuilder.AppendLine($"Font Family URL: {OverlayText.FontFamilyUrl}");
					promptTemplateBuilder.AppendLine($"Font Weight: {OverlayText.FontWeight}");
					promptTemplateBuilder.AppendLine($"Color: {OverlayText.Color}");
					promptTemplateBuilder.AppendLine($"---");
				}

				if (!string.IsNullOrWhiteSpace(generateBannerInputModel.BannerLink))
				{
					promptTemplateBuilder.AppendLine("The banner link is as follows:");
					promptTemplateBuilder.AppendLine(generateBannerInputModel.BannerLink);
				}

				if (!string.IsNullOrWhiteSpace(generateBannerInputModel.CTAButtonText))
				{
					promptTemplateBuilder.AppendLine("The CTA button text is as follows:");
					promptTemplateBuilder.AppendLine(generateBannerInputModel.CTAButtonText);
				}

				ChatHistory chatHistory = new(systemMessage: PromptHelper.BannerExpert);
				chatHistory.AddUserMessage(promptTemplateBuilder.ToString());

				ChatMessageContent result = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel, cancellationToken).ConfigureAwait(false);
				GeneratedBannerDTO generatedBanner = JsonSerializer.Deserialize<GeneratedBannerDTO>(result.ToString(), JsonHelper.ForgivingJsonSerializerOptions)!;

				generatedBanner.BannerId = Guid.NewGuid();
				generatedBanner.BannerRequestId = bannerRequest.Id;

				HttpResponseMessage saveResponse = await apiClient.PostAsJsonAsync("File/SaveBanner", generatedBanner, cancellationToken).ConfigureAwait(false);
				ResponseDTO<string>? saveResult = await saveResponse.Content.ReadFromJsonAsync<ResponseDTO<string>>(cancellationToken: cancellationToken).ConfigureAwait(false);

				if (saveResult is not { IsSuccess: true })
				{
					logger.LogError("Failed to save generated banner. Response: {Response}", saveResult);

					yield return new ResponseDTO<Banner>(false, stringLocalizer["Failed to save generated banner."]);

					continue;
				}

				Banner bannerToAdd = new()
				{
					Id = generatedBanner.BannerId,
					BannerRequestId = bannerRequest.Id,
					AIModel = generateBannerInputModel.AIModel,
					BannerFormat = bannerFormat,
					GeneratedHtmlUrl = $"{configuration["BaseUrl"]}/{saveResult.Content}/index.html",
					GeneratedCSSUrl = $"{configuration["BaseUrl"]}/{saveResult.Content}/app.css",
					GeneratedJSUrl = $"{configuration["BaseUrl"]}/{saveResult.Content}/app.js",
					TotalCostUSD = CalculatePrice(result.Metadata, generateBannerInputModel.AIModel)
				};

				await context.Banners.AddAsync(bannerToAdd, cancellationToken).ConfigureAwait(false);
				await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

				yield return new ResponseDTO<Banner>(true, content: bannerToAdd);
			}
		}
	}

	private static decimal CalculatePrice(IReadOnlyDictionary<string, object?>? metadata, AIModel model)
	{
		if (metadata is GeminiMetadata geminiMetadata)
		{
			decimal inputCacheCost = 0;
			decimal inputCost = CalculateTokenCost(geminiMetadata.PromptTokenCount, model.ToInputPrice());
			decimal outputCost = CalculateTokenCost(geminiMetadata.CandidatesTokenCount, model.ToOutputPrice());

			decimal totalCost = Math.Round(inputCacheCost + inputCost + outputCost, 6, MidpointRounding.AwayFromZero);

			return totalCost == 0 ? -1 : totalCost;
		}
		else if (metadata?["Usage"] is ChatTokenUsage chatTokenUsage)
		{
			decimal inputCacheCost = CalculateTokenCost(chatTokenUsage.InputTokenDetails.CachedTokenCount, model.ToCachedInputPrice());
			decimal inputCost = CalculateTokenCost(chatTokenUsage.InputTokenCount - chatTokenUsage.InputTokenDetails.CachedTokenCount, model.ToInputPrice());
			decimal outputCost = CalculateTokenCost(chatTokenUsage.OutputTokenCount, model.ToOutputPrice());

			decimal totalCost = Math.Round(inputCacheCost + inputCost + outputCost, 6, MidpointRounding.AwayFromZero);

			return totalCost == 0 ? -1 : totalCost;
		}

		return -1;
	}

	private static decimal CalculateTokenCost(int tokens, decimal ratePer1M) => tokens / 1_000_000 * ratePer1M;

	public async Task<ResponseDTO> DeleteBannerAsync(Guid bannerId, CancellationToken cancellationToken = default)
	{
		try
		{
			await using AppDataContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

			await context.Banners.Where(b => b.Id == bannerId).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

			return new ResponseDTO(true);
		}
		catch (Exception ex)
		{
			return ex;
		}
	}

	public async Task<ResponseDTO<IReadOnlyCollection<BannerRequest>>> GetBannersAsync(BannerRequestFilter? bannerRequestFilter = null, CancellationToken cancellationToken = default)
	{
		await using AppDataContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
		IQueryable<BannerRequest> query = context.BannerRequests.Include(x => x.Banners).OrderByDescending(x => x.CreatedDate);

		bannerRequestFilter ??= new();

		if (bannerRequestFilter.AIModel.HasValue)
		{
			query = query.Where(x => x.AIModel == bannerRequestFilter.AIModel.Value);
		}

		if (bannerRequestFilter.FromCreatedDate.HasValue)
		{
			query = query.Where(x => x.CreatedDate >= bannerRequestFilter.FromCreatedDate.Value);
		}

		if (bannerRequestFilter.ToCreatedDate.HasValue)
		{
			query = query.Where(x => x.CreatedDate <= bannerRequestFilter.ToCreatedDate.Value);
		}

		if (!string.IsNullOrWhiteSpace(bannerRequestFilter.SearchString))
		{
			query = query.Where(x => x.Instructions.Contains(bannerRequestFilter.SearchString) 
									|| x.OverlayTexts.Any(overlayText => overlayText.Text.Contains(bannerRequestFilter.SearchString)) 
									|| x.BannerLink!.Contains(bannerRequestFilter.SearchString) 
									|| x.CTAButtonText!.Contains(bannerRequestFilter.SearchString) 
									|| x.Note!.Contains(bannerRequestFilter.SearchString) 
									|| x.HexColors.Contains(bannerRequestFilter.SearchString) 
									|| x.BannerFormats.ToString()!.Contains(bannerRequestFilter.SearchString));
		}

		int recordCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

		query = query.Skip((bannerRequestFilter.Page - 1) * bannerRequestFilter.PageSize).Take(bannerRequestFilter.PageSize);

		IReadOnlyCollection<BannerRequest> bannerRequests = await query.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

		return new ResponseDTO<IReadOnlyCollection<BannerRequest>>(true, content: bannerRequests, recordCount: recordCount);
	}
}