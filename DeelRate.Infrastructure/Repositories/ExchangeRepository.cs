using DeelRate.Application.Abstractions.Repositories;
using DeelRate.Domain.Entities;
using DeelRate.Domain.Enums;

namespace DeelRate.Infrastructure.Repositories;

public class ExchangeRepository() : IExchangeRepository
{
    private readonly List<ExchangeOrder> _context = new();

    public async Task SaveAsync(
        ExchangeOrder exchangeOrder,
        CancellationToken cancellationToken = default
    )
    {
        _context.Add(exchangeOrder);
        
    }

    public async Task UpdateAsync(
        ExchangeOrder exchangeOrder,
        CancellationToken cancellationToken = default
    )
    {
       
    }

    public async Task DeleteAsync(
        Guid exchangeOrderId,
        CancellationToken cancellationToken = default
    )
    {
        ExchangeOrder exchangeOrder = await GetByIdAsync(exchangeOrderId, cancellationToken);
        _context.Remove(exchangeOrder);
    }

    public Task<ExchangeOrder> GetByIdAsync(
        Guid exchangeOrderId,
        CancellationToken cancellationToken = default
    ) => throw new NotImplementedException();

    public Task<List<ExchangeOrder>> GetByStatusAsync(
        ExchangeOrderStatus status,
        CancellationToken cancellationToken = default
    ) => throw new NotImplementedException();

    public Task<List<ExchangeOrder>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    ) => throw new NotImplementedException();

    public Task<List<ExchangeOrder>> GetCompletedExchangeOrdersAsync(
        CancellationToken cancellationToken = default
    ) => throw new NotImplementedException();
}
