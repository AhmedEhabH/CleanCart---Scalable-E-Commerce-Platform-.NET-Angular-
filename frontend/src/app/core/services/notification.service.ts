import { Injectable, inject, OnDestroy } from '@angular/core';
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { ToastService } from '../../shared/components/toast/toast.service';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class NotificationService implements OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private hubConnection: HubConnection | null = null;

  startConnection(): void {
    const url = environment.apiBaseUrl.replace('/api', '') + '/hubs/notifications';

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: () => this.authService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveNotification', (message: string) => {
      this.toastService.info(message);
    });

    this.hubConnection.start().catch(() => {});
  }

  ngOnDestroy(): void {
    this.hubConnection?.stop();
  }
}
