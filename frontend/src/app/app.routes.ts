import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { CartComponent } from './features/cart/cart.component';
import { CheckoutComponent } from './features/checkout/checkout.component';
import { OrderSuccessComponent } from './features/order-success/order-success.component';
import { OrdersComponent } from './features/orders/orders.component';
import { WishlistComponent } from './features/wishlist/wishlist.component';
import { NotFoundComponent } from './features/not-found/not-found.component';
import { ProductListPage } from './features/products/pages/product-list.page';
import { ProductDetailsPage } from './features/products/pages/product-details.page';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent, title: 'Home' },
  { path: 'login', component: LoginComponent, title: 'Login' },
  { path: 'register', component: RegisterComponent, title: 'Register' },
  { path: 'products', component: ProductListPage, title: 'Products' },
  { path: 'products/:id', component: ProductDetailsPage, title: 'Product Details' },
  { path: 'wishlist', component: WishlistComponent, title: 'My Wishlist' },
  { path: 'cart', component: CartComponent, title: 'Cart' },
  { path: 'checkout', component: CheckoutComponent, canActivate: [authGuard], title: 'Checkout' },
  { path: 'order-success', component: OrderSuccessComponent, title: 'Order Success' },
  { path: 'orders', component: OrdersComponent, canActivate: [authGuard], title: 'My Orders' },
  { path: '**', component: NotFoundComponent, title: 'Not Found' }
];
