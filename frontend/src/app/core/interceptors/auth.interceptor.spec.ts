import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';

describe('authInterceptor', () => {
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    sessionStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting()
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should attach Authorization header when token exists', () => {
    localStorage.setItem('access_token', 'test-token-123');

    const http = TestBed.inject(HttpClient);
    http.get('/api/products').subscribe();

    const req = httpMock.expectOne('/api/products');
    expect(req.request.headers.get('Authorization')).toBe('Bearer test-token-123');
  });

  it('should not attach Authorization header when token is missing', () => {
    const http = TestBed.inject(HttpClient);
    http.get('/api/products').subscribe();

    const req = httpMock.expectOne('/api/products');
    expect(req.request.headers.has('Authorization')).toBe(false);
  });

  it('should not attach header to auth login endpoint', () => {
    localStorage.setItem('access_token', 'test-token');

    const http = TestBed.inject(HttpClient);
    http.post('/api/auth/login', { email: 'a@b.com', password: 'pass' }).subscribe();

    const req = httpMock.expectOne('/api/auth/login');
    expect(req.request.headers.has('Authorization')).toBe(false);
  });

  it('should not attach header to auth register endpoint', () => {
    localStorage.setItem('access_token', 'test-token');

    const http = TestBed.inject(HttpClient);
    http.post('/api/auth/register', { email: 'a@b.com', password: 'pass', firstName: 'A', lastName: 'B' }).subscribe();

    const req = httpMock.expectOne('/api/auth/register');
    expect(req.request.headers.has('Authorization')).toBe(false);
  });

  it('should not attach header to refresh-token endpoint', () => {
    localStorage.setItem('access_token', 'test-token');

    const http = TestBed.inject(HttpClient);
    http.post('/api/auth/refresh-token', { refreshToken: 'refresh' }).subscribe();

    const req = httpMock.expectOne('/api/auth/refresh-token');
    expect(req.request.headers.has('Authorization')).toBe(false);
  });
});
