namespace Faluf.BannerGen.Core.Interfaces;

public interface IBannerService
{
	IAsyncEnumerable<ResponseDTO<Banner>> GenerateBannersAsync(GenerateBannerInputModel generateBannerInputModel, CancellationToken cancellationToken = default);

	Task<ResponseDTO<IReadOnlyCollection<BannerRequest>>> GetBannersAsync(BannerRequestFilter? bannerRequestFilter = null, CancellationToken cancellationToken = default);
	
	Task<ResponseDTO> DeleteBannerAsync(Guid bannerId, CancellationToken cancellationToken = default);
}