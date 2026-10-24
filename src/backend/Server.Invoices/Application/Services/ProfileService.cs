using Microsoft.EntityFrameworkCore;
using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Dtos;
using Server.Invoices.Domain;
using Server.Invoices.Infrastructure;

namespace Server.Invoices.Application.Services;

public sealed class ProfileService(
    InvoiceContext context,
    TimeProvider timeProvider) : IProfileService
{
    private static readonly Guid BusinessProfileId =
        Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid InvoiceConfigurationId =
        Guid.Parse("00000000-0000-0000-0000-000000000002");

    public async Task<BusinessProfileResponseDto?> GetBusinessAsync(
        CancellationToken cancellationToken)
    {
        var profile = await context.BusinessProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(profile => profile.Id == BusinessProfileId, cancellationToken);
        return profile is null ? null : Map(profile);
    }

    public async Task<BusinessProfileResponseDto> SaveBusinessAsync(
        BusinessProfileDto profile,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var entity = await context.BusinessProfiles
            .SingleOrDefaultAsync(item => item.Id == BusinessProfileId, cancellationToken);

        if (entity is null)
        {
            entity = new BusinessProfile
            {
                Id = BusinessProfileId,
                CreatedAt = now
            };
            await context.BusinessProfiles.AddAsync(entity, cancellationToken);
        }

        entity.BusinessType = profile.BusinessType;
        entity.LegalName = profile.LegalName.Trim();
        entity.TradingName = Clean(profile.TradingName);
        entity.Email = profile.Email.Trim();
        entity.Phone = Clean(profile.Phone);
        entity.AddressLine1 = profile.AddressLine1.Trim();
        entity.AddressLine2 = Clean(profile.AddressLine2);
        entity.City = profile.City.Trim();
        entity.County = Clean(profile.County);
        entity.Postcode = profile.Postcode.Trim().ToUpperInvariant();
        entity.CompanyNumber = Clean(profile.CompanyNumber).ToUpperInvariant();
        entity.VatNumber = Clean(profile.VatNumber).ToUpperInvariant();
        entity.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<IReadOnlyList<ClientResponseDto>> GetClientsAsync(
        CancellationToken cancellationToken)
    {
        return await context.Clients
            .AsNoTracking()
            .OrderBy(client => client.Name)
            .Select(client => Map(client))
            .ToListAsync(cancellationToken);
    }

    public async Task<ClientResponseDto> CreateClientAsync(
        ClientDto client,
        CancellationToken cancellationToken)
    {
        var normalizedName = Normalize(client.Name);
        if (await context.Clients.AnyAsync(
                item => item.NormalizedName == normalizedName,
                cancellationToken))
        {
            throw new ClientNameConflictException();
        }

        var now = timeProvider.GetUtcNow();
        var entity = new Client
        {
            Id = Guid.NewGuid(),
            CreatedAt = now
        };
        Apply(entity, client, normalizedName, now);
        await context.Clients.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<ClientResponseDto?> UpdateClientAsync(
        Guid id,
        ClientDto client,
        CancellationToken cancellationToken)
    {
        var entity = await context.Clients.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var normalizedName = Normalize(client.Name);
        if (await context.Clients.AnyAsync(
                item => item.Id != id && item.NormalizedName == normalizedName,
                cancellationToken))
        {
            throw new ClientNameConflictException();
        }

        Apply(entity, client, normalizedName, timeProvider.GetUtcNow());
        await context.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<ClientDeleteResult> DeleteClientAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var entity = await context.Clients.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken);
        if (entity is null)
        {
            return ClientDeleteResult.NotFound;
        }

        if (await context.Invoices.AnyAsync(
                invoice => invoice.ClientId == id,
                cancellationToken))
        {
            return ClientDeleteResult.InUse;
        }

        context.Clients.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return ClientDeleteResult.Deleted;
    }

    public async Task<InvoiceConfigurationResponseDto> GetInvoiceConfigurationAsync(
        CancellationToken cancellationToken)
    {
        var configuration = await context.InvoiceConfigurations
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
        return configuration is null
            ? new InvoiceConfigurationResponseDto(InvoiceStyles.Basic, false, string.Empty, null)
            : Map(configuration);
    }

    public async Task<InvoiceConfigurationResponseDto> SaveInvoiceConfigurationAsync(
        InvoiceConfigurationDto configuration,
        CancellationToken cancellationToken)
    {
        var entity = await GetOrCreateInvoiceConfigurationAsync(cancellationToken);
        entity.Style = configuration.Style;
        entity.UpdatedAt = timeProvider.GetUtcNow();
        await context.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<InvoiceConfigurationResponseDto> SaveInvoiceLogoAsync(
        byte[] data,
        string contentType,
        string fileName,
        CancellationToken cancellationToken)
    {
        var entity = await GetOrCreateInvoiceConfigurationAsync(cancellationToken);
        entity.LogoData = data;
        entity.LogoContentType = contentType;
        entity.LogoFileName = Path.GetFileName(fileName);
        entity.UpdatedAt = timeProvider.GetUtcNow();
        await context.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<InvoiceLogoDto?> GetInvoiceLogoAsync(CancellationToken cancellationToken)
    {
        var entity = await context.InvoiceConfigurations.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        return entity?.LogoData is null
            ? null
            : new InvoiceLogoDto(entity.LogoData, entity.LogoContentType, entity.LogoFileName);
    }

    public async Task<InvoiceConfigurationResponseDto> DeleteInvoiceLogoAsync(
        CancellationToken cancellationToken)
    {
        var entity = await GetOrCreateInvoiceConfigurationAsync(cancellationToken);
        entity.LogoData = null;
        entity.LogoContentType = string.Empty;
        entity.LogoFileName = string.Empty;
        entity.UpdatedAt = timeProvider.GetUtcNow();
        await context.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    private async Task<InvoiceConfiguration> GetOrCreateInvoiceConfigurationAsync(
        CancellationToken cancellationToken)
    {
        var entity = await context.InvoiceConfigurations.SingleOrDefaultAsync(cancellationToken);
        if (entity is not null) return entity;
        var now = timeProvider.GetUtcNow();
        entity = new InvoiceConfiguration
        {
            Id = InvoiceConfigurationId,
            Style = InvoiceStyles.Basic,
            CreatedAt = now,
            UpdatedAt = now
        };
        await context.InvoiceConfigurations.AddAsync(entity, cancellationToken);
        return entity;
    }

    private static void Apply(
        Client entity,
        ClientDto client,
        string normalizedName,
        DateTimeOffset now)
    {
        entity.Name = client.Name.Trim();
        entity.NormalizedName = normalizedName;
        entity.ContactName = Clean(client.ContactName);
        entity.Email = client.Email.Trim();
        entity.Phone = Clean(client.Phone);
        entity.AddressLine1 = client.AddressLine1.Trim();
        entity.AddressLine2 = Clean(client.AddressLine2);
        entity.City = client.City.Trim();
        entity.County = Clean(client.County);
        entity.Postcode = client.Postcode.Trim().ToUpperInvariant();
        entity.UpdatedAt = now;
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static string Clean(string? value) => value?.Trim() ?? string.Empty;

    private static BusinessProfileResponseDto Map(BusinessProfile profile) => new(
        profile.BusinessType,
        profile.LegalName,
        profile.TradingName,
        profile.Email,
        profile.Phone,
        profile.AddressLine1,
        profile.AddressLine2,
        profile.City,
        profile.County,
        profile.Postcode,
        profile.CompanyNumber,
        profile.VatNumber,
        profile.UpdatedAt);

    private static ClientResponseDto Map(Client client) => new(
        client.Id,
        client.Name,
        client.ContactName,
        client.Email,
        client.Phone,
        client.AddressLine1,
        client.AddressLine2,
        client.City,
        client.County,
        client.Postcode,
        client.CreatedAt,
        client.UpdatedAt);

    private static InvoiceConfigurationResponseDto Map(InvoiceConfiguration configuration) => new(
        configuration.Style,
        configuration.LogoData is { Length: > 0 },
        configuration.LogoFileName,
        configuration.UpdatedAt);
}

public sealed class ClientNameConflictException : Exception;
