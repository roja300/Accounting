using Server.Tax.Application.Dtos;

namespace Server.Tax.Application.Abstractions;

public interface ITaxYearService
{
    Task<IReadOnlyList<TaxYearDto>> GetTaxYearsAsync(CancellationToken cancellationToken);

    Task<TaxYearDto> ConfigureAsync(
        int startYear,
        TaxYearConfigurationDto configuration,
        CancellationToken cancellationToken);
}
