# ECommerce API

A production-ready ASP.NET Core 8 REST API for e-commerce management, built with Clean Architecture.

## Architecture

The solution follows Clean Architecture with four layers:

| Layer | Project | Responsibility |
|---|---|---|
| **Domain** | `ECommerce.Domain` | Entities, enums, and domain logic |
| **Application** | `ECommerce.Application` | Services, interfaces, DTOs, validators |
| **Infrastructure** | `ECommerce.Infrastructure` | EF Core DbContext, repositories, external services |
| **API** | `ECommerce.Api` | Controllers, middleware, Swagger, composition root |

Key patterns and libraries:
- **CQRS-style** services with result pattern for error handling
- **FluentValidation** for request validation
- **BCrypt** for password hashing
- **JWT Bearer** authentication with refresh tokens
- **Serilog** for structured logging
- **Swagger/OpenAPI** for API documentation

## Prerequisites

- .NET 8 SDK
- SQL Server (local or Docker)

## Running Locally

### 1. Configure the database

Update `src/ECommerce.Api/appsettings.json` with your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 2. Apply migrations

```bash
dotnet ef database update --project src/ECommerce.Infrastructure --startup-project src/ECommerce.Api
```

### 3. Run the API

```bash
dotnet run --project src/ECommerce.Api
```

The API starts at `http://localhost:5000` (or the configured port). On first run, the database is seeded with an admin account and sample data.

## API Endpoints

### Authentication
| Endpoint | Method | Description |
|---|---|---|
| `/api/auth/register` | POST | Register a new user |
| `/api/auth/login` | POST | Login and get JWT token |
| `/api/auth/refresh` | POST | Refresh access token |

### Products
| Endpoint | Method | Description |
|---|---|---|
| `/api/products` | GET | List all products |
| `/api/products/{id}` | GET | Get product by ID |
| `/api/products` | POST | Create product (Admin) |
| `/api/products/{id}` | PUT | Update product (Admin) |
| `/api/products/{id}` | DELETE | Delete product (Admin) |

### Categories
| Endpoint | Method | Description |
|---|---|---|
| `/api/categories` | GET | List all categories |
| `/api/categories/{id}` | GET | Get category by ID |
| `/api/categories` | POST | Create category (Admin) |
| `/api/categories/{id}` | PUT | Update category (Admin) |
| `/api/categories/{id}` | DELETE | Delete category (Admin) |

### Orders
| Endpoint | Method | Description |
|---|---|---|
| `/api/orders` | POST | Create a new order |
| `/api/orders` | GET | Get user's orders |
| `/api/orders/{id}` | GET | Get order details |

### Checkout
| Endpoint | Method | Description |
|---|---|---|
| `/api/checkout` | POST | Process checkout from cart |

### Payments
| Endpoint | Method | Description |
|---|---|---|
| `/api/payments` | POST | Initiate a payment |
| `/api/payments/{id}` | GET | Get payment details |
| `/api/payments/order/{orderId}` | GET | Get payment by order |
| `/api/payments/{id}/process` | POST | Process a payment |
| `/api/payments/{id}/refund` | POST | Refund a payment |

### Authorization

All protected endpoints require a JWT token in the `Authorization` header:

```
Authorization: Bearer <your-token>
```

| Role | Permissions |
|---|---|
| **Admin** | Full access — create, update, delete products, categories, and manage payments |
| **User** | Read products, create orders, manage own payments |

### Usage Examples

```bash
# Create an order
curl -X POST http://localhost:5000/api/orders \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"items":[{"productId":"<guid>","quantity":2}],"shippingAddress":"123 Main St"}'

# Get user's orders
curl -X GET http://localhost:5000/api/orders \
  -H "Authorization: Bearer <token>"

# Process checkout
curl -X POST http://localhost:5000/api/checkout \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"items":[{"productId":"<guid>","quantity":1}],"shippingAddress":"123 Main St","paymentMethod":"CreditCard"}'

# Initiate payment
curl -X POST http://localhost:5000/api/payments \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"orderId":"<guid>","amount":99.99,"method":"CreditCard"}'

# Process payment
curl -X POST http://localhost:5000/api/payments/<payment-id>/process \
  -H "Authorization: Bearer <token>"

# Refund payment
curl -X POST http://localhost:5000/api/payments/<payment-id>/refund \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"reason":"Customer request"}'
```

## Swagger

Swagger UI is available at the root URL when running in Development mode. It includes:
- Interactive endpoint documentation with descriptions and examples
- JWT authentication via the "Authorize" button (enter `Bearer <token>`)
- Request/response schemas and sample values

## Docker

### Build and run with Docker Compose

```bash
docker compose up --build
```

This starts:
- **API** on `http://localhost:8080`
- **SQL Server** on `localhost:1433`

The database is persisted via a named volume. The API waits for the database to be healthy before starting.

### Build the image only

```bash
docker build -t ecommerce-api .
docker run -p 8080:8080 ecommerce-api
```

## Health Checks

| Endpoint | Description |
|---|---|
| `/health` | Overall application health |
| `/health/ready` | Readiness check (database connectivity) |

## Running Tests

```bash
dotnet test
```

## CI

A GitHub Actions workflow (`.github/workflows/ci.yml`) runs on every push and pull request to `main`/`master`. It restores dependencies, builds the solution, and runs all tests.
