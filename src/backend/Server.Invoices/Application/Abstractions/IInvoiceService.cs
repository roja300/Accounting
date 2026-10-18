using System.ComponentModel.DataAnnotations;
using Server.Invoices.Application.Dtos;
using Server.Invoices.Domain;

namespace Server.Invoices.Application.Abstractions;

public interface IInvoiceService
{
    public ValidationResult Create(InvoiceDto invoice);    
}