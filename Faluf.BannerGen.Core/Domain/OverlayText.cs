using System.ComponentModel.DataAnnotations.Schema;

namespace Faluf.BannerGen.Core.Domain;

public sealed class OverlayText 
	: BaseEntity
{
	public string Text { get; set; } = default!;

	public string Color { get; set; } = "#000000";

	public string FontFamilyUrl { get; set; } = "https://fonts.googleapis.com/css2?family=Montserrat:wght@400&display=swap";

	public string FontSize { get; set; } = "16px";

	public string FontWeight { get; set; } = "normal";

	public Guid BannerRequestId { get; set; }

	[ForeignKey(nameof(BannerRequestId))]
	public BannerRequest BannerRequest { get; set; } = default!;
}