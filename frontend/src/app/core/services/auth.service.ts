import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, map } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, AuthResponse, AuthUser, ApiResponse } from '../models';

interface StorageInterface {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;
  private readonly platformId = inject(PLATFORM_ID);

  private readonly TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'auth_user';

  private authUserSubject = new BehaviorSubject<AuthUser | null>(this.getStoredUser());
  authUser$ = this.authUserSubject.asObservable();

  private getStorage(rememberMe: boolean): StorageInterface {
    if (!isPlatformBrowser(this.platformId)) {
      return {
        getItem: () => null,
        setItem: () => {},
        removeItem: () => {}
      };
    }
    return rememberMe ? localStorage : sessionStorage;
  }

  get isAuthenticated(): boolean {
    return !!this.getToken();
  }

  get isAdmin(): boolean {
    return this.currentUser?.role?.toLowerCase() === 'admin';
  }

  get isSeller(): boolean {
    return this.currentUser?.role?.toLowerCase() === 'seller';
  }

  get currentUser(): AuthUser | null {
    return this.authUserSubject.value;
  }

  login(request: LoginRequest, rememberMe: boolean = false): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/auth/login`, request).pipe(
      tap(response => {
        if (response.data) {
          this.handleAuthSuccess(response.data, rememberMe);
        }
      }),
      map(response => {
        if (!response.data) {
          throw new Error(response.message || 'Login failed');
        }
        return response.data;
      })
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/auth/register`, request).pipe(
      tap(response => {
        if (response.data) {
          this.handleAuthSuccess(response.data, true);
        }
      }),
      map(response => {
        if (!response.data) {
          throw new Error(response.message || 'Registration failed');
        }
        return response.data;
      })
    );
  }

  logout(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(this.TOKEN_KEY);
      localStorage.removeItem(this.REFRESH_TOKEN_KEY);
      localStorage.removeItem(this.USER_KEY);
      sessionStorage.removeItem(this.TOKEN_KEY);
      sessionStorage.removeItem(this.REFRESH_TOKEN_KEY);
      sessionStorage.removeItem(this.USER_KEY);
    }
    this.authUserSubject.next(null);
  }

  getToken(): string | null {
    if (!isPlatformBrowser(this.platformId)) {
      return null;
    }
    return localStorage.getItem(this.TOKEN_KEY) || sessionStorage.getItem(this.TOKEN_KEY);
  }

  getStoredToken(): string | null {
    if (!isPlatformBrowser(this.platformId)) {
      return null;
    }
    return localStorage.getItem(this.TOKEN_KEY) || sessionStorage.getItem(this.TOKEN_KEY);
  }

  private handleAuthSuccess(data: AuthResponse, rememberMe: boolean): void {
    const storage = this.getStorage(rememberMe);
    storage.setItem(this.TOKEN_KEY, data.accessToken);
    storage.setItem(this.REFRESH_TOKEN_KEY, data.refreshToken);

    const userId = this.extractUserIdFromToken(data.accessToken);
    const user: AuthUser = {
      id: userId || data.email,
      email: data.email,
      fullName: data.fullName,
      role: data.role
    };
    storage.setItem(this.USER_KEY, JSON.stringify(user));
    this.authUserSubject.next(user);
  }

  private extractUserIdFromToken(token: string): string | null {
    try {
      const payload = token.split('.')[1];
      if (!payload) return null;
      const decoded = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
      return decoded.sub || decoded.nameidentifier || decoded.id || null;
    } catch {
      return null;
    }
  }

  private getStoredUser(): AuthUser | null {
    if (!isPlatformBrowser(this.platformId)) {
      return null;
    }
    const userData = localStorage.getItem(this.USER_KEY) || sessionStorage.getItem(this.USER_KEY);
    if (userData) {
      try {
        return JSON.parse(userData);
      } catch {
        return null;
      }
    }
    return null;
  }
}
