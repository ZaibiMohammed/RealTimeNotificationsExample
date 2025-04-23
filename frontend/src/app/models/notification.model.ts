export interface Notification {
  id: string;
  message: string;
  title: string;
  type: NotificationType;
  targetUrl?: string;
  isRead: boolean;
  createdAt: Date;
  userId?: string;
}

export enum NotificationType {
  Info = 0,
  Success = 1,
  Warning = 2,
  Error = 3
}
