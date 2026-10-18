namespace Server.Invoices.Application.Dtos;

public record InvoiceDto
{
    public Guid Id { get; set; }
    
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public decimal Value { get; set; }
    
    public Guid ClientId { get; set; }
    
    public Guid ProjectId { get; set; }
}