using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Dtos;
using Server.Invoices.Application.Services;
using Server.Invoices.Application.Validators;
using Server.Invoices.Domain;

namespace Server.Invoices.Controllers;

[ApiController]
[Route("profile")]
public sealed class ProfileController(
    IProfileService profileService,
    IInvoiceDocumentService documentService) : ControllerBase
{
    private const int MaximumLogoBytes = 2 * 1024 * 1024;
    private static readonly IReadOnlySet<string> SupportedLogoTypes = new HashSet<string>(
        ["image/png", "image/jpeg"],
        StringComparer.OrdinalIgnoreCase);
    [HttpGet("business")]
    [ProducesResponseType<BusinessProfileResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessProfileResponseDto>> GetBusiness(
        CancellationToken cancellationToken)
    {
        var profile = await profileService.GetBusinessAsync(cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("business")]
    [ProducesResponseType<BusinessProfileResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BusinessProfileResponseDto>> SaveBusiness(
        [FromBody] BusinessProfileDto profile,
        CancellationToken cancellationToken)
    {
        var validation = ProfileValidation.Validate(profile);
        if (!validation.IsValid)
        {
            return ValidationProblem(validation.Errors);
        }

        return Ok(await profileService.SaveBusinessAsync(profile, cancellationToken));
    }

    [HttpGet("clients")]
    [ProducesResponseType<IReadOnlyList<ClientResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClientResponseDto>>> GetClients(
        CancellationToken cancellationToken)
    {
        return Ok(await profileService.GetClientsAsync(cancellationToken));
    }

    [HttpPost("clients")]
    [ProducesResponseType<ClientResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientResponseDto>> CreateClient(
        [FromBody] ClientDto client,
        CancellationToken cancellationToken)
    {
        var validation = ProfileValidation.Validate(client);
        if (!validation.IsValid)
        {
            return ValidationProblem(validation.Errors);
        }

        try
        {
            var created = await profileService.CreateClientAsync(client, cancellationToken);
            return CreatedAtAction(nameof(GetClients), created);
        }
        catch (ClientNameConflictException)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "A customer with this name already exists."
            });
        }
    }

    [HttpPut("clients/{id:guid}")]
    [ProducesResponseType<ClientResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientResponseDto>> UpdateClient(
        Guid id,
        [FromBody] ClientDto client,
        CancellationToken cancellationToken)
    {
        var validation = ProfileValidation.Validate(client);
        if (!validation.IsValid)
        {
            return ValidationProblem(validation.Errors);
        }

        try
        {
            var updated = await profileService.UpdateClientAsync(id, client, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ClientNameConflictException)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "A customer with this name already exists."
            });
        }
    }

    [HttpDelete("clients/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteClient(Guid id, CancellationToken cancellationToken)
    {
        return await profileService.DeleteClientAsync(id, cancellationToken) switch
        {
            ClientDeleteResult.Deleted => NoContent(),
            ClientDeleteResult.NotFound => NotFound(),
            _ => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "This customer is used by a work record and cannot be deleted."
            })
        };
    }

    [HttpGet("invoice-configuration")]
    public async Task<ActionResult<InvoiceConfigurationResponseDto>> GetInvoiceConfiguration(
        CancellationToken cancellationToken) =>
        Ok(await profileService.GetInvoiceConfigurationAsync(cancellationToken));

    [HttpPut("invoice-configuration")]
    public async Task<ActionResult<InvoiceConfigurationResponseDto>> SaveInvoiceConfiguration(
        [FromBody] InvoiceConfigurationDto configuration,
        CancellationToken cancellationToken)
    {
        if (!InvoiceStyles.All.Contains(configuration.Style))
            return ValidationProblem(new Dictionary<string, string[]>
            {
                ["style"] = ["Style must be basic, modern, or minimal."]
            });
        return Ok(await profileService.SaveInvoiceConfigurationAsync(configuration, cancellationToken));
    }

    [HttpGet("invoice-configuration/logo")]
    public async Task<IActionResult> GetInvoiceLogo(CancellationToken cancellationToken)
    {
        var logo = await profileService.GetInvoiceLogoAsync(cancellationToken);
        return logo is null ? NotFound() : File(logo.Data, logo.ContentType, logo.FileName);
    }

    [HttpPut("invoice-configuration/logo")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<InvoiceConfigurationResponseDto>> SaveInvoiceLogo(
        IFormFile logo,
        CancellationToken cancellationToken)
    {
        if (logo.Length is <= 0 or > MaximumLogoBytes || !SupportedLogoTypes.Contains(logo.ContentType))
            return ValidationProblem(new Dictionary<string, string[]>
            {
                ["logo"] = ["Choose a PNG or JPEG logo no larger than 2 MB."]
            });
        await using var stream = new MemoryStream();
        await logo.CopyToAsync(stream, cancellationToken);
        return Ok(await profileService.SaveInvoiceLogoAsync(
            stream.ToArray(), logo.ContentType, logo.FileName, cancellationToken));
    }

    [HttpDelete("invoice-configuration/logo")]
    public async Task<ActionResult<InvoiceConfigurationResponseDto>> DeleteInvoiceLogo(
        CancellationToken cancellationToken) =>
        Ok(await profileService.DeleteInvoiceLogoAsync(cancellationToken));

    [HttpGet("invoice-configuration/demo/{style}")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInvoiceStyleDemonstration(
        string style,
        CancellationToken cancellationToken)
    {
        if (!InvoiceStyles.All.Contains(style))
            return ValidationProblem(new Dictionary<string, string[]>
            {
                ["style"] = ["Style must be basic, modern, or minimal."]
            });
        var logo = await profileService.GetInvoiceLogoAsync(cancellationToken);
        return File(
            documentService.GenerateDemonstration(style, logo?.Data),
            "application/pdf");
    }

    private ActionResult ValidationProblem(IReadOnlyDictionary<string, string[]> errors)
    {
        return ValidationProblem(new ValidationProblemDetails(
            errors.ToDictionary(error => error.Key, error => error.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        });
    }
}
