import { Component, inject, HostListener, ElementRef } from '@angular/core';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { map } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService, Theme } from '../../core/services/theme.service';
import { CartService } from '../../core/services/cart.service';
import { WishlistService } from '../../core/services/wishlist.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  private authService = inject(AuthService);
  protected themeService = inject(ThemeService);
  protected cartService = inject(CartService);
  protected wishlistService = inject(WishlistService);
  private elementRef = inject(ElementRef);
  private router = inject(Router);

  isAuthenticated$ = this.authService.authUser$.pipe(
    map(user => !!user)
  );
  authUser$ = this.authService.authUser$;
  wishlistCount$ = this.wishlistService.wishlistCount$;

  isAccountMenuOpen = false;
  isThemeMenuOpen = false;

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isAccountMenuOpen = false;
      this.isThemeMenuOpen = false;
    }
  }

  toggleAccountMenu(): void {
    this.isAccountMenuOpen = !this.isAccountMenuOpen;
  }

  closeAccountMenu(): void {
    this.isAccountMenuOpen = false;
  }

  toggleThemeMenu(): void {
    this.isThemeMenuOpen = !this.isThemeMenuOpen;
  }

  selectTheme(theme: Theme): void {
    this.themeService.setTheme(theme);
    this.isThemeMenuOpen = false;
  }

  getThemeLabel(theme: Theme): string {
    return this.themeService.getThemeLabel(theme);
  }

  logout(): void {
    this.closeAccountMenu();
    this.authService.logout();
    this.router.navigate(['/']);
  }

  toggleTheme(): void {
    this.themeService.toggle();
  }
}
