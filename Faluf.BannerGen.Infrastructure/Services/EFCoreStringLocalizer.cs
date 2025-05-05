using Microsoft.Extensions.Localization;

namespace Faluf.BannerGen.Infrastructure.Services;

public sealed class EFCoreStringLocalizer
	: IStringLocalizer
{
	// TODO: Implement the logic to fetch localized strings from the database.
	public LocalizedString this[string name] => new(name, name, true, null);

	// TODO: Implement the logic to fetch localized strings from the database.
	public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments), true, null);

	public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
	{
		// TODO: Implement the logic to fetch all localized strings from the database.
		return [];
	}
}