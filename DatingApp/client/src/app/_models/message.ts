export interface Message {
    id: number;
    senderId: number;
    senderUsename: string;
    senderPhotoUrl: string;
    recipentId: number;
    recipentUsername: string;
    recipentPhotoUrl: string;
    content: string;
    dateRead?: Date;
    messageSent: string;
  }
  