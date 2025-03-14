using DeelRate.Common;
using DeelRate.Domain.Common;
using DeelRate.Domain.Enums;

namespace DeelRate.Domain.ValueObjects;

public class DestinationAddress : ValueObject
{
    public AddressType DestinationAddressType { get; private set; }
    public string Address { get; private set; } // For Crypto: wallet address; For Fiat: account number
    public string? AccountName { get; private set; } // For Fiat only
    public string? BankName { get; private set; } // For Fiat only

    private DestinationAddress(
        AddressType addressType,
        string address,
        string? accountName = null,
        string? bankName = null
    )
    {
        DestinationAddressType = addressType;
        Address = address;
        AccountName = accountName;
        BankName = bankName;
    }

    public static Result<DestinationAddress> CreateCryptoAddress(string walletAddress)
    {
        walletAddress = (walletAddress ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(walletAddress))
        {
            return Result.Failure<DestinationAddress>(
                Error.Validation(
                    "DestinationAddress.EmptyWalletAddress",
                    "Wallet address cannot be empty."
                )
            );
        }

        // Optional: Add basic format validation (e.g., length or prefix check)
        if (walletAddress.Length < 26 || walletAddress.Length > 62) // Typical BTC/ETH address length range
        {
            return Result.Failure<DestinationAddress>(
                Error.Validation(
                    "DestinationAddress.InvalidWalletFormat",
                    "Wallet address has an invalid length."
                )
            );
        }

        return Result.Success(
            new DestinationAddress(AddressType.CryptoDepositAddress, walletAddress)
        );
    }

    public static Result<DestinationAddress> CreateFiatAddress(
        string accountNumber,
        string accountName,
        string bankName
    )
    {
        accountNumber = (accountNumber ?? string.Empty).Trim();
        accountName = (accountName ?? string.Empty).Trim();
        bankName = (bankName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            return Result.Failure<DestinationAddress>(
                Error.Validation(
                    "DestinationAddress.EmptyAccountNumber",
                    "Account number cannot be empty."
                )
            );
        }

        if (string.IsNullOrWhiteSpace(accountName))
        {
            return Result.Failure<DestinationAddress>(
                Error.Validation(
                    "DestinationAddress.EmptyAccountName",
                    "Account name cannot be empty."
                )
            );
        }

        if (string.IsNullOrWhiteSpace(bankName))
        {
            return Result.Failure<DestinationAddress>(
                Error.Validation("DestinationAddress.EmptyBankName", "Bank name cannot be empty.")
            );
        }

        // Optional: Add account number format validation (e.g., 10 digits for NGN)
        if (accountNumber.Length != 10 || !accountNumber.All(char.IsDigit))
        {
            return Result.Failure<DestinationAddress>(
                Error.Validation(
                    "DestinationAddress.InvalidAccountNumber",
                    "Account number must be a 10-digit number."
                )
            );
        }

        return Result.Success(
            new DestinationAddress(
                AddressType.FiatAccountNumber,
                accountNumber,
                accountName,
                bankName
            )
        );
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DestinationAddressType;
        yield return Address;

        if (DestinationAddressType == AddressType.FiatAccountNumber)
        {
            yield return AccountName!;
            yield return BankName!;
        }
    }
}
