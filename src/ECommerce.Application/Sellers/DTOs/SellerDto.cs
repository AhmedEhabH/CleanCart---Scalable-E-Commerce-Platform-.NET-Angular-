namespace ECommerce.Application.Sellers.DTOs;

public record SellerDto(
    Guid Id,
    string BusinessName,
    string? Description,
    string? LogoUrl,
    int ProductsCount,
    DateTime CreatedAt
);

public record SellerDetailDto(
    Guid Id,
    string BusinessName,
    string? Description,
    string? LogoUrl,
    string? ContactEmail,
    string? ContactPhone,
    bool IsApproved,
    int ProductsCount,
    DateTime CreatedAt,
    DateTime? ApprovedAt
);