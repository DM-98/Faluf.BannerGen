using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Faluf.BannerGen.Infrastructure.Helpers;

public static class JsonHelper
{
	public static JsonSerializerOptions ForgivingJsonSerializerOptions { get; } = new()
	{
		PropertyNameCaseInsensitive = true,
		WriteIndented = true,
		AllowTrailingCommas = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};
}