using System.ComponentModel.DataAnnotations.Schema;

namespace Faluf.BannerGen.Core.DTOs.Inputs;

public sealed class BannerFileInputModel
{
	public string Path { get; set; } = default!;

	public string Description { get; set; } = default!;

	public Guid BannerRequestId { get; set; }

	[ForeignKey(nameof(BannerRequestId))]
	public BannerRequest BannerRequest { get; set; } = default!;
}