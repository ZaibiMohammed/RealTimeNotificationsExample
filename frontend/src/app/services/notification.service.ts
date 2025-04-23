import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { NgEventBus } from 'ng-event-bus';
import { Notification } from '../models/notification.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private hubConnection: signalR.HubConnection | undefined;
  private baseUrl = `${environment.apiUrl}/api/notifications`;
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  private unreadCountSubject = new BehaviorSubject<number>(0);
  
  notifications$ = this.notificationsSubject.asObservable();
  unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private http: HttpClient, private eventBus: NgEventBus) {}

  // Initialize SignalR connection
  public startConnection(userId: string): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/notifications?userId=${userId}`)
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR connection established');
        this.registerSignalREvents();
        this.loadNotifications(userId);
      })
      .catch(err => {
        console.error('Error while establishing SignalR connection:', err);
        // Retry connection after a short delay
        setTimeout(() => this.startConnection(userId), 5000);
      });
  }

  // Stop SignalR connection
  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('SignalR connection stopped'))
        .catch(err => console.error('Error stopping SignalR connection:', err));
    }
  }

  // Register SignalR event handlers
  private registerSignalREvents(): void {
    if (this.hubConnection) {
      this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
        // Convert string date to Date object
        notification.createdAt = new Date(notification.createdAt);
        
        // Update notifications list
        const currentNotifications = this.notificationsSubject.value;
        const updatedNotifications = [notification, ...currentNotifications];
        this.notificationsSubject.next(updatedNotifications);
        
        // Update unread count
        this.updateUnreadCount();
        
        // Broadcast the notification to any components listening
        this.eventBus.cast('new-notification', notification);
      });
    }
  }

  // Join a notification group
  public joinGroup(groupName: string): Observable<any> {
    if (this.hubConnection) {
      return new Observable<any>(observer => {
        this.hubConnection!.invoke('JoinGroup', groupName)
          .then(() => {
            observer.next();
            observer.complete();
          })
          .catch(err => {
            observer.error(err);
          });
      });
    }
    return new Observable<any>(observer => observer.error('Hub connection not established'));
  }

  // Leave a notification group
  public leaveGroup(groupName: string): Observable<any> {
    if (this.hubConnection) {
      return new Observable<any>(observer => {
        this.hubConnection!.invoke('LeaveGroup', groupName)
          .then(() => {
            observer.next();
            observer.complete();
          })
          .catch(err => {
            observer.error(err);
          });
      });
    }
    return new Observable<any>(observer => observer.error('Hub connection not established'));
  }

  // Load notifications for a user
  public loadNotifications(userId: string): void {
    this.http.get<Notification[]>(`${this.baseUrl}/users/${userId}`)
      .subscribe({
        next: (notifications) => {
          // Convert string dates to Date objects
          const parsedNotifications = notifications.map(n => ({
            ...n,
            createdAt: new Date(n.createdAt)
          }));
          
          this.notificationsSubject.next(parsedNotifications);
          this.updateUnreadCount();
        },
        error: (error) => {
          console.error('Error loading notifications:', error);
        }
      });
  }

  // Mark a notification as read
  public markAsRead(notificationId: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/${notificationId}/read`, {})
      .pipe(
        tap(() => {
          const currentNotifications = this.notificationsSubject.value;
          const updatedNotifications = currentNotifications.map(n => 
            n.id === notificationId ? { ...n, isRead: true } : n
          );
          this.notificationsSubject.next(updatedNotifications);
          this.updateUnreadCount();
        })
      );
  }

  // Mark all notifications as read for a user
  public markAllAsRead(userId: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/users/${userId}/read-all`, {})
      .pipe(
        tap(() => {
          const currentNotifications = this.notificationsSubject.value;
          const updatedNotifications = currentNotifications.map(n => 
            ({ ...n, isRead: true })
          );
          this.notificationsSubject.next(updatedNotifications);
          this.updateUnreadCount();
        })
      );
  }

  // Send a notification
  public sendNotification(notification: Notification): Observable<any> {
    return this.http.post(`${this.baseUrl}/broadcast`, notification);
  }

  // Send a notification to a specific user
  public sendNotificationToUser(userId: string, notification: Notification): Observable<any> {
    return this.http.post(`${this.baseUrl}/users/${userId}`, notification);
  }

  // Send a notification to a group
  public sendNotificationToGroup(groupName: string, notification: Notification): Observable<any> {
    return this.http.post(`${this.baseUrl}/groups/${groupName}`, notification);
  }

  // Update the unread count
  private updateUnreadCount(): void {
    const unreadCount = this.notificationsSubject.value.filter(n => !n.isRead).length;
    this.unreadCountSubject.next(unreadCount);
  }
}
