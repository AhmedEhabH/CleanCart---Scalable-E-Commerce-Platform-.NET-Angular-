namespace ECommerce.Domain.ValueObjects;

public record Address
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;

    private Address() { }

    public static Address Create(string street, string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required", nameof(country));

        return new Address
        {
            Street = street.Trim(),
            City = city.Trim(),
            State = state?.Trim() ?? string.Empty,
            PostalCode = postalCode?.Trim() ?? string.Empty,
            Country = country.Trim()
        };
    }

    public string ToFormattedString() => 
        $"{Street}, {City}, {State} {PostalCode}, {Country}";
}
