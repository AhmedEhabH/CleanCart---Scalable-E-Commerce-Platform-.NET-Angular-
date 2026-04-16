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
- Click the theme button to open a dropdown with 3 theme options
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

### Three-Theme Support

The application supports three themes:

1. **Light** - Clean, bright interface with white backgrounds
2. **Dark** - Near-black background with charcoal surfaces and indigo accents
3. **GitHub** - GitHub-inspired neutral palette with professional surfaces and subtle borders

The GitHub theme features:
- White background (#ffffff) with subtle gray surfaces (#f6f8fa)
- Clean borders (#d0d7de) for definition
- GitHub blue accent (#0969da) for interactive elements
- High contrast text (#24292f) for readability
- Restrained green for success (#1a7f37)

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

## Navigation

### Header Account Dropdown

Authenticated users have access to an account dropdown menu in the header that provides:
- User avatar with initials
- User's name and email
- Quick access to My Orders
- Logout option

The dropdown:
- Opens on click with smooth animation
- Closes when clicking outside
- Is keyboard accessible
- Adapts to mobile screens (opens left-aligned)
- Maintains consistent styling with light/dark themes

## Auth UX Improvements

The application includes user experience enhancements for authentication.

### Show/Hide Password
- Toggle button on login and register forms
- Allows users to reveal/hide password while typing
- Eye/eye-off icon indicates current state
- Works consistently across all themes

### Remember Me
- Checkbox option on login form
- When checked: persists auth in localStorage (survives browser close)
- When unchecked: uses sessionStorage (session-only, cleared on tab close)
- Default behavior maintains security standards
- Does not change backend contracts

### Files Added/Changed

| File | Change |
|------|--------|
| `src/app/features/auth/login/login.component.ts` | Added showPassword, rememberMe |
| `src/app/features/auth/login/login.component.html` | Added password toggle, Remember Me checkbox |
| `src/app/features/auth/login/login.component.scss` | Added password toggle styles |
| `src/app/features/auth/register/register.component.ts` | Added showPassword |
| `src/app/features/auth/register/register.component.html` | Added password toggle |
| `src/app/features/auth/register/register.component.scss` | Added password toggle styles |
| `src/app/core/services/auth.service.ts` | Updated for session/rememberMe support |
| `src/app/core/services/theme.service.ts` | Added github theme, 3-theme toggle |
| `src/styles.scss` | Added GitHub theme CSS variables |
| `src/app/layout/header/header.component.ts` | Added 3-theme dropdown |
| `src/app/layout/header/header.component.html` | Theme dropdown UI |
| `src/app/layout/header/header.component.scss` | Theme dropdown styles |

### Navigation Links

| Link | Auth Required | Location |
|------|---------------|----------|
| Home | No | Top nav |
| Products | No | Top nav |
| Cart | No | Top nav (with badge count) |
| My Orders | Yes | Account dropdown |
| Login/Register | No | Auth section (unauthenticated) |
| Account menu | Yes | Auth section (authenticated) |

## Products Page

The products page (`/products`) includes advanced filtering and sorting capabilities for improved product discovery.

### Search
- Search input with debounce (300ms) for performance
- Searches product names
- Press Enter or wait for auto-search
- Clear search to show all products

### Filtering
- **Category filter**: Dropdown to filter by product category
- **In Stock toggle**: Show only products currently in stock
- **Featured toggle**: Show only featured products
- Filters can be combined and work together
- "Clear Filters" button appears when any filter is active

### Sorting
Available sort options:
| Option | Description |
|--------|-------------|
| Featured | Featured products first |
| Price: Low to High | Ascending price |
| Price: High to Low | Descending price |
| Name: A-Z | Alphabetical A-Z |
| Name: Z-A | Alphabetical Z-A |

### Empty State
When no products match the current search/filters, a helpful empty state is displayed with option to clear filters.

### Backend Integration
All filtering and sorting is handled server-side via query parameters:
- `SearchTerm` - Product name search
- `CategoryId` - Filter by category
- `IsInStock` - Stock availability filter
- `IsFeatured` - Featured status filter
- `SortBy` - Field to sort by
- `SortDescending` - Sort direction

## Feedback & Loading UX

The application includes a lightweight toast notification system for user feedback.

### Toast Notifications
Success and error toasts appear for key user actions:
- **Add to Cart**: Shows product name in success toast
- **Cart Updates**: Success/error feedback for quantity changes
- **Cart Remove**: Confirmation when item is removed

Toast behavior:
- Auto-dismiss after 3 seconds
- Click to dismiss early
- Stacks if multiple toasts appear
- Positioned bottom-right (bottom-left on mobile)

### Loading States
Loading feedback is provided for:
- Add to Cart buttons (product cards and details page)
- Cart item quantity updates
- Cart item removal
- Checkout submission

Loading states:
- Show spinner immediately when action starts
- Stop on success or error
- Never get stuck in loading state
- Disabled state prevents double-clicks

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
