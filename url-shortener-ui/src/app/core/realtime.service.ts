import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HttpTransportType } from '@microsoft/signalr';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private conn?: HubConnection;

  // event: server reported that the list of links has changed // 
  readonly urlsChanged$ = new Subject<void>();

  start(): void {
    if (this.conn) return;

    this.conn = new HubConnectionBuilder()
      .withUrl('/hubs/urls', {
        transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling,
        withCredentials: true
      })
      .withAutomaticReconnect()
      .build();

    this.conn.on('urlsChanged', () => this.urlsChanged$.next());

    this.conn.start().catch(() => {
      
    });
  }

  stop(): void {
    this.conn?.stop();
    this.conn = undefined;
  }
}
