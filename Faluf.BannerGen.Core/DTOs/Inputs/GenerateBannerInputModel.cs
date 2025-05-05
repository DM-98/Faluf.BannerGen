namespace Faluf.BannerGen.Core.DTOs.Inputs;

public sealed class GenerateBannerInputModel
{
	public int GenerateAmount { get; set; }

	public Guid? BannerRequestId { get; set; }

	public string? Note { get; set; }

	public List<BannerFormat> BannerFormats { get; set; } = [BannerFormat.TopscrollAndInterscroll];

	public AIModel AIModel { get; set; } = AIModel.Google_Gemini25ProPreview;

	public string Instructions { get; set; } = default!;

	public bool IsOptimizeInstructionsEnabled { get; set; }

	public string? OptimizedInstructions { get; set; }

	public string? CTAButtonText { get; set; }

	public string? BannerLink { get; set; }

	public List<string> HexColors { get; set; } = [];

	public List<BannerFileInputModel> MediaFiles { get; set; } = [];

	public List<OverlayTextInputModel> OverlayTexts { get; set; } = [];
}