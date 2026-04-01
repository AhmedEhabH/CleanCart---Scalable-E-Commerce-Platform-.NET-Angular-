using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Address : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }
    public AddressType AddressType { get; private set; } = AddressType.Both;

    private Address() { }

    public static Address Create(
        Guid userId,
        string street,
        string city,
        string state,
        string postalCode,
        string country,
        AddressType addressType = AddressType.Both,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required", nameof(country));

        return new Address
        {
            UserId = userId,
            Street = street.Trim(),
            City = city.Trim(),
            State = state?.Trim() ?? string.Empty,
            PostalCode = postalCode?.Trim() ?? string.Empty,
            Country = country.Trim(),
            AddressType = addressType,
            IsDefault = isDefault
        };
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        MarkAsUpdated();
    }

    public void RemoveDefault()
    {
        IsDefault = false;
        MarkAsUpdated();
    }

    public void Update(string street, string city, string state, string postalCode, string country)
    {
        Street = street?.Trim() ?? Street;
        City = city?.Trim() ?? City;
        State = state?.Trim() ?? State;
        PostalCode = postalCode?.Trim() ?? PostalCode;
        Country = country?.Trim() ?? Country;
        MarkAsUpdated();
    }

    public string FormattedAddress => $"{Street}, {City}, {State} {PostalCode}, {Country}";
}
