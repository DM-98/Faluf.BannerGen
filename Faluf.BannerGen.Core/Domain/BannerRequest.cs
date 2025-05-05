namespace Faluf.BannerGen.Core.Domain;

public sealed class BannerRequest 
	: BaseEntity
{
	public string? Note { get; set; }

	public AIModel AIModel { get; set; } = AIModel.Google_Gemini25ProPreview;

	public List<BannerFormat> BannerFormats { get; set; } = [BannerFormat.TopscrollAndInterscroll];

	public string Instructions { get; set; } = default!;
	
	public string? OptimizedInstructions { get; set; }

	public string? CTAButtonText { get; set; }

	public string? BannerLink { get; set; }

	public List<string> HexColors { get; set; } = [];

	public ICollection<BannerFile> MediaFiles { get; set; } = [];

	public ICollection<OverlayText> OverlayTexts { get; set; } = [];

	public ICollection<Banner> Banners { get; set; } = [];
}