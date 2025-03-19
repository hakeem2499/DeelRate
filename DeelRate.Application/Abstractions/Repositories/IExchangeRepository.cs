// Purpose: Interface for the Exchange Repository, which is responsible for handling the data access layer for the ExchangeOrder entity.
using DeelRate.Domain.Entities;
using DeelRate.Domain.Enums;

namespace DeelRate.Application.Abstractions.Repositories;

public interface IExchangeRepository
{
    // Save an exchange order
    Task SaveAsync(ExchangeOrder exchangeOrder, CancellationToken cancellationToken = default);

    // Retrieve an exchange order by its ID
    Task<ExchangeOrder> GetByIdAsync(
        Guid exchangeOrderId,
        CancellationToken cancellationToken = default
    );

    // Retrieve all exchange orders for a specific user
    Task<List<ExchangeOrder>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    // Retrieve all exchange orders with a specific status
    Task<List<ExchangeOrder>> GetByStatusAsync(
        ExchangeOrderStatus status,
        CancellationToken cancellationToken = default
    );

    // Retrieve all completed exchange orders
    Task<List<ExchangeOrder>> GetCompletedExchangeOrdersAsync(
        CancellationToken cancellationToken = default
    );

    // Update an existing exchange order
    Task UpdateAsync(ExchangeOrder exchangeOrder, CancellationToken cancellationToken = default);

    // Delete an exchange order by its ID
    Task DeleteAsync(Guid exchangeOrderId, CancellationToken cancellationToken = default);
}
