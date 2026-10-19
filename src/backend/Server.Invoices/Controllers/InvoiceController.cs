using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Dtos;
using Server.Invoices.Application.Validators;

namespace Server.Invoices.Controllers;

[ApiController]
[Route("invoices")]
public sealed class InvoiceController(
    IInvoiceService invoiceService,
    InvoiceValidator validator,
    InvoiceListQueryValidator listQueryValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<InvoicePageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvoicePageDto>> GetPage(
        [FromQuery] InvoiceListQueryDto query,
        CancellationToken cancellationToken)
    {
        var validation = listQueryValidator.Validate(query);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validation.Errors.ToDictionary(error => error.Key, error => error.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            });
        }

        return Ok(await invoiceService.GetPageAsync(query, cancellationToken));
    }

    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [ProducesResponseType<InvoiceResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceResponseDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetByIdAsync(id, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpGet("options")]
    [ProducesResponseType<InvoiceOptionsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<InvoiceOptionsDto>> GetOptions(
        CancellationToken cancellationToken)
    {
        return Ok(await invoiceService.GetOptionsAsync(cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType<InvoiceResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvoiceResponseDto>> Create(
        [FromBody] InvoiceDto invoice,
        CancellationToken cancellationToken)
    {
        var validation = validator.Validate(invoice);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validation.Errors.ToDictionary(error => error.Key, error => error.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            });
        }

        var created = await invoiceService.CreateAsync(invoice, cancellationToken);
        return CreatedAtRoute(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<InvoiceResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceResponseDto>> Update(
        Guid id,
        [FromBody] InvoiceDto invoice,
        CancellationToken cancellationToken)
    {
        var validation = validator.Validate(invoice);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validation.Errors.ToDictionary(error => error.Key, error => error.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            });
        }

        var updated = await invoiceService.UpdateAsync(id, invoice, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}
