using Server.Invoices.Application.Dtos;

namespace Server.Invoices.Application.Abstractions;

public interface IProfileService
{
    Task<BusinessProfileResponseDto?> GetBusinessAsync(CancellationToken cancellationToken);

    Task<BusinessProfileResponseDto> SaveBusinessAsync(
        BusinessProfileDto profile,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ClientResponseDto>> GetClientsAsync(CancellationToken cancellationToken);

    Task<ClientResponseDto> CreateClientAsync(
        ClientDto client,
        CancellationToken cancellationToken);

    Task<ClientResponseDto?> UpdateClientAsync(
        Guid id,
        ClientDto client,
        CancellationToken cancellationToken);

    Task<ClientDeleteResult> DeleteClientAsync(Guid id, CancellationToken cancellationToken);
}

public enum ClientDeleteResult
{
    Deleted,
    NotFound,
    InUse
}
