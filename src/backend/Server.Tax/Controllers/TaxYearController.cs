using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Tax.Application.Abstractions;
using Server.Tax.Application.Dtos;
using Server.Tax.Application.Services;

namespace Server.Tax.Controllers;

[ApiController]
[Route("tax-years")]
public sealed class TaxYearController(ITaxYearService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TaxYearDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaxYearDto>>> Get(
        CancellationToken cancellationToken) =>
        Ok(await service.GetTaxYearsAsync(cancellationToken));

    [HttpPut("{startYear:int}")]
    [ProducesResponseType<TaxYearDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaxYearDto>> Configure(
        int startYear,
        [FromBody] TaxYearConfigurationDto configuration,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ConfigureAsync(startYear, configuration, cancellationToken));
        }
        catch (TaxYearValidationException exception)
        {
            ModelState.AddModelError(exception.Field, exception.Message);
            return ValidationProblem(ModelState);
        }
    }
}
