using Microsoft.EntityFrameworkCore;

namespace Faluf.BannerGen.Infrastructure.Contexts;

public sealed class AppDataContext(DbContextOptions<AppDataContext> dbContextOptions) 
	: DbContext(dbContextOptions)
{
	public DbSet<BannerRequest> BannerRequests => Set<BannerRequest>();

	public DbSet<BannerFile> BannerFiles => Set<BannerFile>();

	public DbSet<OverlayText> OverlayTexts => Set<OverlayText>();

	public DbSet<Banner> Banners => Set<Banner>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Banner>()
			.Property(x => x.TotalCostUSD)
			.HasPrecision(18, 6);
	}
}