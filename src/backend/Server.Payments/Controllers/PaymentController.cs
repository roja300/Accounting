using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Payments.Application.Abstractions;
using Server.Payments.Application.Dtos;
using Server.Payments.Application.Validation;

namespace Server.Payments.Controllers;

[ApiController]
[Route("payments")]
public sealed class PaymentController(
    IPaymentService paymentService,
    PaymentQueryValidator validator) : ControllerBase
{
    public const long MaximumFileSize = 10 * 1024 * 1024;

    [HttpGet]
    [ProducesResponseType<PaymentPageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentPageDto>> GetPage([FromQuery] PaymentListQueryDto query, CancellationToken cancellationToken)
    {
        var errors = validator.Validate(query);
        return errors.Count > 0
            ? ValidationProblem(errors)
            : Ok(await paymentService.GetPageAsync(query, cancellationToken));
    }

    [HttpGet("imports")]
    [ProducesResponseType<IReadOnlyList<PaymentImportDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaymentImportDto>>> GetImports(CancellationToken cancellationToken)
    {
        return Ok(await paymentService.GetImportsAsync(cancellationToken));
    }

    [HttpPost("imports")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaximumFileSize)]
    [ProducesResponseType<PaymentImportDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentImportDto>> Import(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return ValidationProblem(new Dictionary<string, string[]> { ["file"] = ["Select a non-empty CSV file."] });
        if (file.Length > MaximumFileSize)
            return ValidationProblem(new Dictionary<string, string[]> { ["file"] = ["The CSV file must be 10 MB or smaller."] });
        if (!string.Equals(Path.GetExtension(file.FileName), ".csv", StringComparison.OrdinalIgnoreCase))
            return ValidationProblem(new Dictionary<string, string[]> { ["file"] = ["The selected file must use the .csv extension."] });
        if (Path.GetFileName(file.FileName).Length > 255)
            return ValidationProblem(new Dictionary<string, string[]> { ["file"] = ["The CSV file name must be 255 characters or fewer."] });

        await using var stream = file.OpenReadStream();
        var result = await paymentService.ImportAsync(stream, file.FileName, cancellationToken);
        if (!result.IsValid) return ValidationProblem(result.Errors);
        return Created($"/payments/imports/{result.Import!.Id}", result.Import);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete([FromQuery] DeletePaymentsDto request, CancellationToken cancellationToken)
    {
        var errors = validator.Validate(request);
        if (errors.Count > 0) return ValidationProblem(errors);
        await paymentService.DeleteAsync(request, cancellationToken);
        return NoContent();
    }

    private ActionResult ValidationProblem(IReadOnlyDictionary<string, string[]> errors)
    {
        return base.ValidationProblem(new ValidationProblemDetails(
            errors.ToDictionary(error => error.Key, error => error.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        });
    }
}
