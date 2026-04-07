# ECommerce API

A production-ready ASP.NET Core 8 REST API for e-commerce management, built with Clean Architecture and CQRS patterns.

## Features

- [x] **Authentication & Authorization** — JWT Bearer tokens with refresh tokens, role-based access (Admin/User)
- [x] **Product Management** — Full CRUD with admin controls
- [x] **Category Management** — Hierarchical product categorization
- [x] **Shopping Cart** — Add, update, remove items per user
- [x] **Order Management** — Create orders from cart, track order history
- [x] **Payment Processing** — Initiate, process, and refund payments
- [x] **Checkout Flow** — End-to-end checkout with payment integration
- [x] **API Documentation** — Swagger/OpenAPI interactive UI
- [x] **Health Checks** — Application and database readiness endpoints
- [x] **Structured Logging** — Serilog integration
- [x] **Database Seeding** — Auto-seeds admin account and sample data
- [x] **CI/CD** — GitHub Actions for build and test automation
- [x] **Docker Support** — Multi-stage Dockerfile and docker-compose setup
- [x] **Testing** — Unit and integration tests across all layers

## Architecture

Built with Clean Architecture principles, the solution is organized into four independent layers:

```
┌─────────────────────────────────────────────────┐
│                   API Layer                     │
│  Controllers, Middleware, Swagger, Composition  │
├─────────────────────────────────────────────────┤
│               Application Layer                 │
│  Services, Interfaces, DTOs, Validators, CQRS   │
├─────────────────────────────────────────────────┤
│              Infrastructure Layer               │
│  EF Core DbContext, Repositories, External Svc  │
├─────────────────────────────────────────────────┤
│                 Domain Layer                    │
│  Entities, Enums, Domain Logic (no dependencies)│
└─────────────────────────────────────────────────┘
```

**Key Technologies:**
- **Framework:** ASP.NET Core 8
- **Database:** SQL Server with Entity Framework Core
- **Validation:** FluentValidation
- **Security:** BCrypt password hashing, JWT authentication
- **Logging:** Serilog
- **Testing:** xUnit

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (local or Docker)

### Option 1: Local Development

1. **Configure the database** — Update `src/ECommerce.Api/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
     }
   }
   ```

2. **Apply migrations:**
   ```bash
   dotnet ef database update --project src/ECommerce.Infrastructure --startup-project src/ECommerce.Api
   ```

3. **Run the API:**
   ```bash
   dotnet run --project src/ECommerce.Api
   ```

   The API starts at `http://localhost:5000`. On first run, the database is seeded with an admin account and sample data.

### Option 2: Docker (Recommended)

**Run with Docker Compose (starts API + SQL Server):**
```bash
docker compose up --build
```

This starts:
- **API** on `http://localhost:8080`
- **SQL Server** on `localhost:1433`

The database is persisted via a named volume. The API waits for the database to be healthy before starting.

**Build and run the image only:**
```bash
docker build -t ecommerce-api .
docker run -p 8080:8080 ecommerce-api
```

### Option 3: API Documentation & Testing

#### Swagger UI
- Available at `http://localhost:5000` (local) or `http://localhost:8080` (Docker) in Development mode
- Use the **Authorize** button to enter JWT tokens (`Bearer <token>`)
- Interactive endpoint documentation with request/response schemas

#### Postman
1. Import `ECommerce.postman_collection.json` into Postman
2. Set the `baseUrl` variable to your API URL (default: `http://localhost:8080`)
3. Login via `/api/auth/login` to get an access token
4. Set the `accessToken` variable with your token for authenticated requests

#### cURL Examples
```bash
# Login
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@ecommerce.com","password":"Admin@123"}'

# Get products (public)
curl -X GET http://localhost:8080/api/products

# Create order (requires auth)
curl -X POST http://localhost:8080/api/orders \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"items":[{"productId":"<guid>","quantity":2}],"shippingAddress":"123 Main St"}'
```

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

### Cart
| Endpoint | Method | Description |
|---|---|---|
| `/api/cart` | GET | Get current user's cart |
| `/api/cart/items` | POST | Add item to cart |
| `/api/cart/items/{id}` | PUT | Update cart item quantity |
| `/api/cart/items/{id}` | DELETE | Remove item from cart |
| `/api/cart/clear` | DELETE | Clear all cart items |

### Orders
| Endpoint | Method | Description |
|---|---|---|
| `/api/orders` | POST | Create a new order from cart |
| `/api/orders` | GET | Get user's orders |
| `/api/orders/{id}` | GET | Get order details |

### Checkout & Payments
| Endpoint | Method | Description |
|---|---|---|
| `/api/checkout` | POST | Process checkout from cart |
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
| **Admin** | Full access — manage products, categories, and payments |
| **User** | Browse products, create orders, manage own payments |

## Health Checks

| Endpoint | Description |
|---|---|
| `/health` | Overall application health |
| `/health/ready` | Readiness check (database connectivity) |

## Running Tests

```bash
dotnet test
```

## CI/CD

A GitHub Actions workflow (`.github/workflows/ci.yml`) runs on every push and pull request to `main`/`master`. It restores dependencies, builds the solution, and runs all tests.

## Roadmap

- [ ] Redis caching for products and categories
- [ ] Email notifications for order confirmations
- [ ] File upload for product images
- [ ] Pagination and filtering for list endpoints
- [ ] Rate limiting and API throttling
- [ ] Integration with external payment gateways (Stripe, PayPal)
- [ ] GraphQL API alternative
- [ ] Kubernetes deployment manifests

---

## Frontend (Angular)

A standalone Angular SPA that consumes the ECommerce API. Built with Angular 21, standalone components, and typed HTTP clients.

### Features Implemented

- [x] **App Shell** — Header with navigation, footer, responsive layout
- [x] **Authentication** — Login, Register, JWT interceptor, route guards
- [x] **Products Catalog** — Paginated product listing, product details page, reusable card/grid components

### How to Run the Frontend

**Prerequisites:**
- Node.js 18+
- The backend API must be running (see Getting Started above)

**Install dependencies:**
```bash
cd frontend
npm install
```

**Development server:**
```bash
npm start
# or
ng serve
```
The app starts at `http://localhost:4200`.

**Production build:**
```bash
npm run build
# or
ng build --configuration production
```
Output is written to `frontend/dist/frontend`.

### API Configuration

The frontend reads the backend URL from environment files:

| File | `apiBaseUrl` |
|---|---|
| `src/environments/environment.ts` | `http://localhost:5000/api` |
| `src/environments/environment.prod.ts` | `/api` |

Update `environment.ts` if your API runs on a different host or port.

### How the Frontend Uses the Backend

The frontend calls the REST API through a typed `ApiService` wrapper. Authenticated requests include a JWT Bearer token via the `AuthInterceptor`.

**Example product list response shape:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "name": "Gaming Laptop",
        "slug": "gaming-laptop",
        "description": "High-performance laptop...",
        "price": 1299.99,
        "compareAtPrice": 1499.99,
        "mainImageUrl": "https://example.com/laptop.jpg",
        "images": [{ "id": "guid", "imageUrl": "...", "altText": null, "displayOrder": 1 }],
        "isInStock": true,
        "hasDiscount": true,
        "discountPercentage": 13,
        "averageRating": 4.5,
        "reviewCount": 42,
        "sku": "LAPTOP-001",
        "stockQuantity": 15,
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 12,
    "totalPages": 13,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### Product Image Handling

Product images are resolved through a reusable `ProductImagePipe` with a three-tier strategy:

1. **Local product assets (highest priority)** — Known seeded demo products are mapped by `slug` to locally generated SVG assets stored in `frontend/src/assets/products/`. The mapping is centralized in the pipe and covers all 8 seeded products:
   - `premium-smartphone`, `budget-smartphone`, `gaming-laptop`, `ultrabook`, `wireless-earbuds`, `classic-tshirt`, `smart-led-bulb`, `garden-tool-set`

2. **Backend-relative paths** — Relative URLs (e.g. `/uploads/products/img.jpg`) are prefixed with the API base host (e.g. `http://localhost:5000/uploads/products/img.jpg`)

3. **Absolute URLs** — External URLs (`http://`, `https://`) are used as-is, except for known unreliable placeholder services like `via.placeholder.com` which are skipped

4. **Fallback** — If no valid image is available, a local SVG placeholder at `/assets/images/product-placeholder.svg` is displayed

This ensures images always display correctly without depending on external placeholder services. Demo product assets are locally generated and stored in the repo for full offline support.

### Project Structure

```
frontend/src/app/
├── core/                      # Singleton services, guards, interceptors, models
│   ├── guards/                # Route guards (authGuard)
│   ├── interceptors/          # HTTP interceptors (authInterceptor)
│   ├── models/                # Shared type definitions
│   └── services/              # Core services (ApiService, AuthService)
├── features/                  # Feature modules (lazy-load ready)
│   ├── auth/                  # Login & Register pages
│   ├── products/              # Products catalog
│   │   ├── components/        # Reusable UI (product-card, product-grid)
│   │   ├── models/            # Product & pagination types
│   │   ├── pages/             # Route-level pages (list, details)
│   │   └── services/          # ProductsService
│   ├── home/
│   ├── cart/
│   ├── checkout/
│   └── not-found/
├── layout/                    # Header, footer, shell components
├── shared/                    # Shared pipes, directives, utilities
│   └── pipes/                 # ProductImagePipe
└── environments/              # Environment-specific config
```

### Local Product Assets

The following demo product SVG images are generated and stored locally in `frontend/src/assets/products/`:

| Slug | File |
|---|---|
| `premium-smartphone` | `premium-smartphone.svg` |
| `budget-smartphone` | `budget-smartphone.svg` |
| `gaming-laptop` | `gaming-laptop.svg` |
| `ultrabook` | `ultrabook.svg` |
| `wireless-earbuds` | `wireless-earbuds.svg` |
| `classic-tshirt` | `classic-tshirt.svg` |
| `smart-led-bulb` | `smart-led-bulb.svg` |
| `garden-tool-set` | `garden-tool-set.svg` |

A generic fallback placeholder is available at `frontend/src/assets/images/product-placeholder.svg`.

### Screenshots

> **Note:** To add screenshots, run the frontend locally (`npm start`), navigate to each page, and save PNG screenshots into `docs/screenshots/`. Then update the image references below.

| File | Description |
|---|---|
| `home.png` | Home page |
| `login.png` | Login page |
| `register.png` | Register page |
| `products-list.png` | Products listing page |
| `product-details.png` | Product details page |
