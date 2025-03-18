namespace DeelRate.Infrastructure.Services.CheckCryptoAddressClient;

public record CryptoAddressResponse(bool Valid, string ScamReport, string Message);
