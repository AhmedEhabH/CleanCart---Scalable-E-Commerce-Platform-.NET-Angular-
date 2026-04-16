# Frontend

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.2.6.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Theming

The application supports light and dark themes with a ChatGPT-inspired design aesthetic.

### Theme System

The theming system uses CSS custom properties (variables) defined in `src/styles.scss`. Semantic tokens are used for consistent styling across light and dark modes:

| Token | Description |
|-------|-------------|
| `--color-bg` | Main background |
| `--color-surface` | Card/component surfaces |
| `--color-surface-elevated` | Elevated surfaces (modals, dropdowns) |
| `--color-border` | Border color |
| `--color-text` | Primary text |
| `--color-text-secondary` | Secondary text |
| `--color-text-muted` | Muted/disabled text |
| `--color-primary` | Primary accent color |
| `--color-primary-dark` | Primary hover/active color |
| `--color-error` | Error/danger color |
| `--color-success` | Success color |
| `--color-warning` | Warning color |

### Theme Toggle

- Theme toggle button is located in the header, next to authentication controls
- Click the sun/moon icon to switch between light and dark modes
- Theme preference is persisted in localStorage under the key `app-theme`
- On first load, the system checks:
  1. Saved user preference (if previously selected)
  2. System `prefers-color-scheme` media query (if no saved preference)
  3. Defaults to light mode

### Dark Mode Design

The dark mode features:
- Near-black background (#0f0f0f) for reduced eye strain
- Charcoal surfaces (#1a1a1a) with subtle elevation
- Soft borders (#2e2e2e) for definition without harshness
- Indigo accent (#6366f1) for interactive elements
- Muted text colors for hierarchy and readability
- Smooth transitions between themes

### Adding New Themeable Components

When creating new components, use CSS variables instead of hardcoded colors:

```scss
.my-component {
  background-color: var(--color-surface);
  color: var(--color-text);
  border: 1px solid var(--color-border);
}
```

Avoid hardcoded colors like `#ffffff`, `#000000`, or specific hex values. Use the semantic tokens for consistency.

### Theme Persistence

Theme preference is stored in `localStorage` and read before the app initializes to prevent flash of unstyled content (FOUC). The initialization script in `index.html` applies the correct theme immediately.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.

## Cart

Cart UI shell is now present with Add to Cart buttons on product cards and details page, cart link in header, and cart page layout. Cart functionality will be added in subsequent steps.

## Checkout Foundation

Checkout flow from cart to order placement is now implemented.

### Current Capabilities

- **Checkout Page** (`/checkout`)
  - Contact information form (full name, email, phone)
  - Shipping address form (street, city, state, postal code, country)
  - Order notes (optional, max 500 characters)
  - Real-time form validation
  - Order summary sidebar with cart items

- **Cart-to-Checkout Integration**
  - "Proceed to Checkout" button on cart page
  - Redirect to cart if cart is empty
  - Cart state integration via CartService

- **Order Placement**
  - POST to `/api/Checkout` endpoint
  - Authenticated users only (via authGuard)
  - Loading state during submission
  - Error handling with user feedback

- **Order Success Page** (`/order-success`)
  - Displays order confirmation
  - Shows order number, status, total, and item count
  - Links to continue shopping and view orders

- **Order History Page** (`/orders`)
  - Displays list of user's past orders
  - Shows order number, date, status, items count, and total
  - Authenticated users only (via authGuard)
  - Empty state when no orders exist
  - Orders refresh immediately after successful checkout (no manual refresh needed)

### Backend Contract

**Endpoint:** `POST /api/Checkout`

**Request Body:**
```typescript
{
  shippingAddress: {
    street: string;      // required
    city: string;        // required
    state: string;       // optional
    postalCode: string;  // optional
    country: string;     // required
  };
  billingAddress?: {...} // optional (same structure)
  notes?: string;       // optional, max 500 chars
}
```

**Response:** `OrderDto` with order details

### Files Added/Changed

| File | Change |
|------|--------|
| `src/app/core/models/order.model.ts` | Added - Order DTOs and types |
| `src/app/core/services/order.service.ts` | Added - Order API service |
| `src/app/features/checkout/checkout.component.ts` | Updated - Full implementation |
| `src/app/features/checkout/checkout.component.html` | Updated - Complete form UI |
| `src/app/features/checkout/checkout.component.scss` | Updated - Styles for textarea |
| `src/app/features/order-success/order-success.component.ts` | Updated - Added View Orders button |
| `src/app/features/orders/orders.component.ts` | Added - Order history page |
| `src/app/layout/header/header.component.html` | Updated - My Orders nav link |
| `src/app/app.routes.ts` | Updated - Order success and orders routes |
| `README.md` | Updated - Documentation |

### Deferred/Not Implemented

- Payment gateway UI (no backend payment integration ready)
- Coupon/discount system
- Order details page
- Email confirmation display
- Order tracking
- Billing address form (separate from shipping)
- Pagination on orders page (if order count is high)
