using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Dtos;
using Server.Invoices.Application.Services;
using Server.Invoices.Application.Validators;
using Server.Invoices.Domain;

namespace Server.Invoices.Controllers;

[ApiController]
[Route("exported-invoices")]
public sealed class ExportedInvoiceController(
    IExportedInvoiceService invoiceService,
    IInvoiceDocumentService documentService,
    GeneratedInvoiceValidator validator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<GeneratedInvoicePageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GeneratedInvoicePageDto>> GetPage(
        [FromQuery] GeneratedInvoiceListQueryDto query,
        CancellationToken cancellationToken)
    {
        var validation = validator.Validate(query);
        return validation.IsValid
            ? Ok(await invoiceService.GetPageAsync(query, cancellationToken))
            : Validation(validation.Errors);
    }

    [HttpGet("options")]
    [ProducesResponseType<GenerateInvoiceOptionsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GenerateInvoiceOptionsDto>> GetOptions(
        CancellationToken cancellationToken) =>
        Ok(await invoiceService.GetOptionsAsync(cancellationToken));

    [HttpGet("{id:guid}", Name = "GetExportedInvoiceById")]
    [ProducesResponseType<GeneratedInvoiceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GeneratedInvoiceDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetAsync(id, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    [ProducesResponseType<GeneratedInvoiceDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GeneratedInvoiceDto>> Generate(
        [FromBody] GenerateInvoiceDto request,
        CancellationToken cancellationToken)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid) return Validation(validation.Errors);
        try
        {
            var invoice = await invoiceService.GenerateAsync(request, cancellationToken);
            return CreatedAtRoute("GetExportedInvoiceById", new { id = invoice.Id }, invoice);
        }
        catch (InvoiceExportException exception)
        {
            return Validation(new Dictionary<string, string[]>
            {
                [exception.Field] = [exception.Message]
            });
        }
    }

    [HttpGet("{id:guid}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetDocumentSourceAsync(id, cancellationToken);
        return invoice is null
            ? NotFound()
            : File(
                documentService.Generate(invoice),
                "application/pdf",
                $"{invoice.InvoiceNumber}.pdf");
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType<GeneratedInvoiceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GeneratedInvoiceDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateGeneratedInvoiceStatusDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await invoiceService.UpdateStatusAsync(
                id,
                request.Status,
                cancellationToken);
            return invoice is null ? NotFound() : Ok(invoice);
        }
        catch (InvoiceExportException exception)
        {
            return Validation(new Dictionary<string, string[]>
            {
                [exception.Field] = [exception.Message]
            });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.UpdateStatusAsync(
            id,
            GeneratedInvoiceStatuses.Deleted,
            cancellationToken);
        return invoice is null ? NotFound() : NoContent();
    }

    private ActionResult Validation(IReadOnlyDictionary<string, string[]> errors) =>
        ValidationProblem(new ValidationProblemDetails(
            errors.ToDictionary(error => error.Key, error => error.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        });
}
