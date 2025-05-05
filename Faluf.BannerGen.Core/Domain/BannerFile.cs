using System.ComponentModel.DataAnnotations.Schema;

namespace Faluf.BannerGen.Core.Domain;

public sealed class BannerFile
	: BaseEntity
{
	public string Path { get; set; } = default!;

	public string FileName { get; set; } = default!;

	public MediaType MediaType { get; set; }

	public string Description { get; set; } = default!;

	public Guid BannerRequestId { get; set; }

	[ForeignKey(nameof(BannerRequestId))]
	public BannerRequest BannerRequest { get; set; } = default!;
}