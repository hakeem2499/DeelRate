using DeelRate.Common;
using DeelRate.Domain.Common;
using DeelRate.Domain.Enums;

namespace DeelRate.Domain.ValueObjects
{
    public class DestinationAddress : ValueObject
    {
        public AddressType DestinationAddressType { get; private set; }
        public string Address { get; private set; }

        private DestinationAddress(AddressType addressType, string address)
        {
            Address = address;
        }

        public Result<DestinationAddress> CreateDestinationAddress(
            AddressType addressType,
            string destinationAddress
        )
        {
            destinationAddress = (destinationAddress ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(destinationAddress))
                return Result.Failure<DestinationAddress>(
                    Error.Validation(
                        "DestinationAddress.Empty",
                        "Destination address can't be empty."
                    )
                );

            return Result.Success(new DestinationAddress(addressType, destinationAddress));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return DestinationAddressType;
            yield return Address;
        }
    }
}
