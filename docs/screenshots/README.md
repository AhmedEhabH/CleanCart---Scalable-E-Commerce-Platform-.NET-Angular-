# Screenshots

This directory contains screenshots of the ECommerce platform for use in the README.

## Guidelines for Capturing Screenshots

### Desktop Resolution
- Use a resolution of 1920x1080 (Full HD) for consistency
- Ensure browser zoom is set to 100%
- Capture the entire browser window or just the viewport as needed

### Mobile Viewport
- Use a mobile viewport size of 375x667 (typical smartphone)
- Capture in portrait orientation
- Ensure the device frame is not included unless specifically desired

### Theme Capture
- For light theme screenshots, ensure the application is in light mode
- For dark theme screenshots, enable dark theme in the application
- Wait for theme transition to complete before capturing

### Data Loading
- Wait for all API data to load before capturing
- Ensure spinners, loaders, or skeleton states have disappeared
- Wait for product images to fully load
- Ensure no partial UI states are visible

### Stable Data
- Use the seeded demo data provided by the backend
- Do not use empty states unless intentionally documenting an empty state
- Avoid capturing during form validation errors unless documenting validation

### Clean State
- Close browser devtools before capturing
- Ensure no browser notifications or popups are visible
- Clear any temporary UI states (like toast notifications) before capturing
- Ensure the URL is stable and not in transition

### Naming Convention
Use the following naming convention for screenshots:
- `[page-name]-[theme].png` for theme-specific screenshots (e.g., `home-light.png`)
- `[page-name].png` for theme-agnostic screenshots
- `mobile-[page-name].png` for mobile viewport screenshots
- `[feature].png` for specific features (e.g., `reviews.png`)

## Screenshot List

The following screenshots are referenced in the README:

### Light Theme
- home-light.png
- products-light.png
- product-details-light.png
- cart-light.png
- checkout-light.png
- orders-light.png
- wishlist-light.png
- login-light.png
- register-light.png

### Dark Theme
- home-dark.png
- products-dark.png
- product-details-dark.png
- cart-dark.png
- checkout-dark.png
- orders-dark.png
- wishlist-dark.png
- login-dark.png
- register-dark.png

### Mobile View
- mobile-menu.png
- mobile-home.png
- mobile-products.png

### Additional Features
- reviews.png
- admin-products.png (if applicable)

## Capturing Process

1. Ensure the backend is running (API accessible)
2. Start the frontend development server
3. Navigate to the target page
4. Wait for full data load and UI stability
5. Apply theme if needed (light/dark)
6. Adjust browser window to target resolution
7. Capture screenshot
8. Save with the appropriate name in this directory
9. Repeat for all required screenshots

## Troubleshooting

- If images are missing, check the product image pipe configuration
- If authentication is required, complete the login flow first
- If data is not loading, verify the backend API is accessible
- If UI appears broken, check for console errors and ensure dependencies are installed