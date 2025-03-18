using Refit;

namespace DeelRate.Infrastructure.Services.CheckCryptoAddressClient;

public interface ICryptoAddress
{
    [Post("/wallet-checks")]
    Task<CryptoAddressResponse?> CheckCryptoAddressAsync([Body] CryptoAddressRequest request);
}
