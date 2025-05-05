using System.ComponentModel.DataAnnotations.Schema;

namespace Faluf.BannerGen.Core.Domain;

public sealed class Banner
	: BaseEntity
{
	public AIModel AIModel { get; set; }

	public BannerFormat BannerFormat { get; set; }

	public decimal TotalCostUSD { get; set; }

	public string GeneratedHtmlUrl { get; set; } = default!;

	public string GeneratedCSSUrl { get; set; } = default!;

	public string GeneratedJSUrl { get; set; } = default!;

	public Guid BannerRequestId { get; set; }

	[ForeignKey(nameof(BannerRequestId))]
	public BannerRequest BannerRequest { get; set; } = default!;
}