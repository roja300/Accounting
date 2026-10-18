using System.ComponentModel.DataAnnotations;
using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Dtos;

namespace Server.Invoices.Application.Services;

public class InvoiceService : IInvoiceService
{
    public ValidationResult Create(InvoiceDto invoice)
    {
        throw new NotImplementedException();
    }
}