using System.Text.Json.Serialization;

namespace Faluf.BannerGen.Core.DTOs.Outputs;

public sealed class GeneratedBannerDTO
{
	public Guid BannerRequestId { get; set; }

	public Guid BannerId { get; set; }

	[JsonPropertyName("html")]
	public string Html { get; set; } = default!;

	[JsonPropertyName("css")]
	public string Css { get; set; } = default!;

	[JsonPropertyName("js")]
	public string Js { get; set; } = default!;
}