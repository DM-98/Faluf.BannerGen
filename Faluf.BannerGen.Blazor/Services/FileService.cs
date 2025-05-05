using Faluf.BannerGen.Core.Domain;
using Faluf.BannerGen.Core.Enums;
using Faluf.BannerGen.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.IO.Compression;

namespace Faluf.BannerGen.Blazor.Services;

public sealed class FileService(IHttpClientFactory httpClientFactory, IStringLocalizer<FileService> stringLocalizer, IDbContextFactory<AppDataContext> dbContextFactory, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
{
	public async Task<ResponseDTO<string>> SaveBannerAsync(GeneratedBannerDTO generatedBanner, CancellationToken cancellationToken = default)
	{
		try
		{

			string relativePath = Path.Combine("banners", generatedBanner.BannerRequestId.ToString(), $"banner-{generatedBanner.BannerId}");
			string absolutePath = Path.Combine(webHostEnvironment.ContentRootPath, relativePath);
		
			Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
			
			string htmlFilePath = Path.Combine(absolutePath, "index.html");
			string cssFilePath = Path.Combine(absolutePath, "style.css");
			string jsFilePath = Path.Combine(absolutePath, "script.js");

			await File.WriteAllTextAsync(htmlFilePath, generatedBanner.Html, cancellationToken).ConfigureAwait(false);
			await File.WriteAllTextAsync(cssFilePath, generatedBanner.Css, cancellationToken).ConfigureAwait(false);
			await File.WriteAllTextAsync(jsFilePath, generatedBanner.Js, cancellationToken).ConfigureAwait(false);

			return configuration["BaseUrl"] + "/" + relativePath.Replace("\\", "/");
		}
		catch (Exception ex)
		{
			return ex;
		}
	}

	public async Task<ResponseDTO<string>> GenerateZipFileAsync(Guid bannerId, CancellationToken cancellationToken = default)
	{
		try
		{
			await using AppDataContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
			Banner? banner = await context.Banners.AsNoTracking().FirstOrDefaultAsync(b => b.Id == bannerId, cancellationToken).ConfigureAwait(false);

			if (banner is null)
			{
				return new ResponseDTO<string>(false, stringLocalizer["Banner not found."]);
			}

			string zipRelativePath = Path.Combine("banners", banner.BannerRequestId.ToString(), $"banner-{banner.Id}.zip");
			string zipAbsolutePath = Path.Combine(webHostEnvironment.ContentRootPath, zipRelativePath);

			Directory.CreateDirectory(Path.GetDirectoryName(zipAbsolutePath)!);
			File.Delete(zipAbsolutePath);

			HttpClient httpClient = httpClientFactory.CreateClient();

			using (ZipArchive zip = ZipFile.Open(zipAbsolutePath, ZipArchiveMode.Create))
			{
				async Task AddToZipFromUrlAsync(string url, string entryName)
				{
					using Stream stream = await httpClient.GetStreamAsync(url, cancellationToken).ConfigureAwait(false);
					ZipArchiveEntry zipEntry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
					await using Stream zipStream = zipEntry.Open();
				
					await stream.CopyToAsync(zipStream, cancellationToken).ConfigureAwait(false);
				}

				async Task AddHtmlWithRewrittenSrcAsync(string url, string entryName)
				{
					string htmlContent = await httpClient.GetStringAsync(url, cancellationToken).ConfigureAwait(false);

					static string ReplacePath(string content, string? original, string relative)
					{
						if (string.IsNullOrEmpty(original))
						{
							return content;
						}

						string encodedOriginal = original.Replace(" ", "%20");

						content = content.Replace(original, relative, StringComparison.OrdinalIgnoreCase);

						if (!original.Equals(encodedOriginal, StringComparison.OrdinalIgnoreCase))
						{
							content = content.Replace(original, relative, StringComparison.OrdinalIgnoreCase);
						}

						return content;
					}

					foreach (BannerFile file in banner.BannerRequest.MediaFiles)
					{
						string mediaFolder = file.MediaType == MediaType.Image ? "images" : "videos";

						htmlContent = ReplacePath(htmlContent, file.Path, $"./assets/{mediaFolder}/{Path.GetFileName(file.Path)}");
					}

					ZipArchiveEntry entry = zip.CreateEntry(entryName);
					using StreamWriter writer = new(entry.Open());

					await writer.WriteAsync(htmlContent).ConfigureAwait(false);
				}

				if (!string.IsNullOrWhiteSpace(banner.GeneratedHtmlUrl))
				{
					await AddHtmlWithRewrittenSrcAsync(banner.GeneratedHtmlUrl, "index.html").ConfigureAwait(false);
				}

				if (!string.IsNullOrWhiteSpace(banner.GeneratedCSSUrl))
				{
					await AddToZipFromUrlAsync(banner.GeneratedCSSUrl, "assets/app.css").ConfigureAwait(false);
				}

				if (!string.IsNullOrWhiteSpace(banner.GeneratedJSUrl))
				{
					await AddToZipFromUrlAsync(banner.GeneratedJSUrl, "assets/app.js").ConfigureAwait(false);
				}

				foreach (BannerFile file in banner.BannerRequest.MediaFiles)
				{
					string mediaFolder = file.MediaType == MediaType.Image ? "images" : "videos";
					string mediaPath = Path.Combine("assets", mediaFolder, Path.GetFileName(file.Path));

					await AddToZipFromUrlAsync(file.Path, mediaPath).ConfigureAwait(false);
				}
			}

			return zipAbsolutePath;
		}
		catch (Exception ex)
		{
			return ex;
		}
	}

	public async Task<ResponseDTO<string>> UploadVideoAsync(Guid bannerRequestId, IFormFile file, CancellationToken cancellationToken = default)
	{
		try
		{
			string relativePath = Path.Combine("banners", bannerRequestId.ToString(), "uploads", "videos", Path.GetFileName(file.FileName));
			string absolutePath = Path.Combine(webHostEnvironment.ContentRootPath, relativePath);
		
			Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

			using FileStream stream = File.Create(absolutePath);
			await file.OpenReadStream().CopyToAsync(stream, cancellationToken).ConfigureAwait(false);

			return configuration["BaseUrl"] + "/" + relativePath.Replace("\\", "/");
		}
		catch (Exception ex)
		{
			return ex;
		}
	}

	public async Task<ResponseDTO<string>> UploadImageAsync(Guid bannerRequestId, IFormFile file, CancellationToken cancellationToken = default)
	{
		try
		{
			string relativePath = Path.Combine("banners", bannerRequestId.ToString(), "uploads", "images", Path.GetFileName(file.FileName));
			string absolutePath = Path.Combine(webHostEnvironment.ContentRootPath, relativePath);
		
			Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

			using FileStream stream = File.Create(absolutePath);
			await file.OpenReadStream().CopyToAsync(stream, cancellationToken).ConfigureAwait(false);

			return configuration["BaseUrl"] + "/" + relativePath.Replace("\\", "/");
		}
		catch (Exception ex)
		{
			return ex;
		}
	}
}