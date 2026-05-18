import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AdminOrdersComponent } from './admin-orders.component';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

describe('AdminOrdersComponent', () => {
  let httpMock: HttpTestingController;
  let toastService: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AdminOrdersComponent],
      providers: [
        AdminService,
        ToastService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
    toastService = TestBed.inject(ToastService);

    vi.spyOn(window, 'confirm').mockReturnValue(true);
  });

  afterEach(() => {
    httpMock.verify();
    vi.restoreAllMocks();
  });

  it('should show success toast on valid status update', () => {
    const toastSpy = vi.spyOn(toastService, 'success');
    const fixture = TestBed.createComponent(AdminOrdersComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    // Flush the initial loadOrders GET from ngOnInit
    const initialGet = httpMock.expectOne({ method: 'GET' });
    initialGet.flush({ data: [] });

    component.orders.set([
      { id: 'order-1', orderNumber: 'ORD-001', status: 'Pending', subTotal: 50, taxAmount: 5, shippingCost: 10, discountAmount: 0, totalAmount: 65, createdAt: new Date().toISOString(), items: [], totalItems: 1, userEmail: 'test@example.com' }
    ]);

    const selectEl = document.createElement('select');
    const option = document.createElement('option');
    option.value = 'Confirmed';
    selectEl.appendChild(option);
    selectEl.value = 'Confirmed';
    const event = { target: selectEl } as unknown as Event;

    component.onStatusChange('order-1', event);

    const putReq = httpMock.expectOne({ method: 'PUT' });
    expect(putReq.request.url).toContain('/admin/orders/order-1/status');
    expect(putReq.request.body).toEqual({ status: 'Confirmed' });
    putReq.flush({ success: true });

    // Flush the reload GET from loadOrders after success
    const reloadGet = httpMock.expectOne({ method: 'GET' });
    reloadGet.flush({ data: [] });

    expect(toastSpy).toHaveBeenCalledWith('Order status updated to Confirmed');
  });

  it('should show error toast and revert status on 400 bad request', () => {
    const toastErrorSpy = vi.spyOn(toastService, 'error');
    const fixture = TestBed.createComponent(AdminOrdersComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    // Flush the initial loadOrders GET from ngOnInit
    const initialGet = httpMock.expectOne({ method: 'GET' });
    initialGet.flush({ data: [] });

    component.orders.set([
      { id: 'order-1', orderNumber: 'ORD-001', status: 'Pending', subTotal: 50, taxAmount: 5, shippingCost: 10, discountAmount: 0, totalAmount: 65, createdAt: new Date().toISOString(), items: [], totalItems: 1, userEmail: 'test@example.com' }
    ]);

    const selectEl = document.createElement('select');
    const shippedOption = document.createElement('option');
    shippedOption.value = 'Shipped';
    selectEl.appendChild(shippedOption);
    const pendingOption = document.createElement('option');
    pendingOption.value = 'Pending';
    selectEl.appendChild(pendingOption);
    selectEl.value = 'Shipped';
    const event = { target: selectEl } as unknown as Event;

    component.onStatusChange('order-1', event);

    const putReq = httpMock.expectOne({ method: 'PUT' });
    putReq.flush(
      { message: 'Cannot transition from Pending to Shipped' },
      { status: 400, statusText: 'Bad Request' }
    );

    // Flush the reload GET from loadOrders after error
    const reloadGet = httpMock.expectOne({ method: 'GET' });
    reloadGet.flush({ data: [] });

    expect(toastErrorSpy).toHaveBeenCalledWith(
      'Invalid status transition. Please follow the correct order lifecycle.'
    );
    expect(selectEl.value).toBe('Pending');
  });
});
