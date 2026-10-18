using Server.Invoices.Application.Dtos;
using Server.Invoices.Application.Services;
using Server.Invoices.Domain;

namespace Server.Invoices.Tests;

public class InvoiceServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 12, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task ShouldPersistInvoiceWithNewClientAndProject_WhenNamesDoNotExist()
    {
        var context = new MockInvoiceContext();
        var service = new InvoiceService(context, new FixedTimeProvider(Now));
        var request = CreateInvoice();

        var response = await service.CreateAsync(request, CancellationToken.None);

        var saved = Assert.IsType<Invoice>(context.AddedInvoice);
        Assert.NotEqual(Guid.Empty, saved.Id);
        Assert.Equal("Monthly bookkeeping", saved.Description);
        Assert.Equal(125.50m, saved.Value);
        Assert.Equal("GBP", saved.Currency);
        Assert.Equal("Completed October records.", saved.Comments);
        Assert.Equal("Acme Ltd", saved.Client.Name);
        Assert.Equal("ACME LTD", saved.Client.NormalizedName);
        Assert.Equal("Retainer", saved.Project.Name);
        Assert.Equal("RETAINER", saved.Project.NormalizedName);
        Assert.Equal(Now, saved.CreatedAt);
        Assert.Equal(Now, saved.UpdatedAt);
        Assert.Equal(1, context.SaveChangesCalls);
        Assert.Equal(saved.Id, response.Id);
    }

    [Fact]
    public async Task ShouldReuseClientAndProject_WhenNormalizedNamesExist()
    {
        var existingClient = CreateClient();
        var existingProject = CreateProject();
        var context = new MockInvoiceContext
        {
            Client = existingClient,
            Project = existingProject
        };
        var service = new InvoiceService(context, new FixedTimeProvider(Now));
        var request = CreateInvoice() with
        {
            Client = " acme ltd ",
            Project = " retainer "
        };

        await service.CreateAsync(request, CancellationToken.None);

        var saved = Assert.IsType<Invoice>(context.AddedInvoice);
        Assert.Same(existingClient, saved.Client);
        Assert.Same(existingProject, saved.Project);
        Assert.Equal(existingClient.Id, saved.ClientId);
        Assert.Equal(existingProject.Id, saved.ProjectId);
    }

    [Fact]
    public async Task ShouldPersistEmptyComments_WhenCommentsAreOmitted()
    {
        var context = new MockInvoiceContext();
        var service = new InvoiceService(context, new FixedTimeProvider(Now));
        var request = CreateInvoice() with { Comments = null };

        var response = await service.CreateAsync(request, CancellationToken.None);

        var saved = Assert.IsType<Invoice>(context.AddedInvoice);
        Assert.Equal(string.Empty, saved.Comments);
        Assert.Equal(string.Empty, response.Comments);
    }

    private static InvoiceDto CreateInvoice()
    {
        return new InvoiceDto(
            " Monthly bookkeeping ",
            125.50m,
            "gbp",
            " Completed October records. ",
            " Acme Ltd ",
            " Retainer ");
    }

    private static Client CreateClient()
    {
        return new Client
        {
            Id = Guid.Parse("0af19caf-dd92-4ecb-acfe-67e22e2e7d0a"),
            Name = "Acme Ltd",
            NormalizedName = "ACME LTD",
            CreatedAt = Now,
            UpdatedAt = Now
        };
    }

    private static Project CreateProject()
    {
        return new Project
        {
            Id = Guid.Parse("d843fe1f-e8b7-45a2-8b88-361bfc977fb7"),
            Name = "Retainer",
            NormalizedName = "RETAINER",
            CreatedAt = Now,
            UpdatedAt = Now
        };
    }
}
