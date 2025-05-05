using Microsoft.AspNetCore.Mvc;

namespace Faluf.BannerGen.Blazor.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class FileController(FileService fileService)
	: ControllerBase
{
	[HttpGet("DownloadBanner/{bannerId}")]
	public async Task<IActionResult> DownloadBannerAsync(Guid bannerId, CancellationToken cancellationToken)
	{
		ResponseDTO<string> result = await fileService.GenerateZipFileAsync(bannerId, cancellationToken).ConfigureAwait(false);

		if (!result.IsSuccess)
		{
			return BadRequest(result);
		}

		string zipPath = result.Content;

		if (!System.IO.File.Exists(zipPath))
		{
			return NotFound(result);
		}

		string zipName = Path.GetFileName(zipPath);

		return PhysicalFile(zipPath, "application/zip", zipName);
	}

	[HttpPost("SaveBanner")]
	public async Task<IActionResult> SaveBannerAsync(GeneratedBannerDTO generatedBanner, CancellationToken cancellationToken)
	{
		ResponseDTO<string> result = await fileService.SaveBannerAsync(generatedBanner, cancellationToken).ConfigureAwait(false);

		if (!result.IsSuccess)
		{
			return BadRequest(result);
		}
		
		return Ok(result);
	}

	[HttpPost("UploadVideo/{bannerRequestId}")]
	public async Task<IActionResult> UploadVideoAsync([FromForm] IFormFile formFile, Guid bannerRequestId, CancellationToken cancellationToken)
	{
		ResponseDTO<string> result = await fileService.UploadVideoAsync(bannerRequestId, formFile, cancellationToken).ConfigureAwait(false);

		if (!result.IsSuccess)
		{
			return BadRequest(result);
		}

		return Ok(result);
	}

	[HttpPost("UploadImage/{bannerRequestId}")]
	public async Task<IActionResult> UploadImageAsync([FromForm] IFormFile formFile, Guid bannerRequestId, CancellationToken cancellationToken)
	{
		ResponseDTO<string> result = await fileService.UploadImageAsync(bannerRequestId, formFile, cancellationToken).ConfigureAwait(false);
		
		if (!result.IsSuccess)
		{
			return BadRequest(result);
		}
	
		return Ok(result);
	}
}