import { Injectable } from '@angular/core';
import { Subject } from "rxjs/Subject";

export enum Status {
  Success,
  Warn,
  Error,
  Info
}

export class MessageDescription {
  constructor(public message: string, public status: Status, public title?: string) { }
}

export class WaiterDictionary {
  [status: string]: number;
}

@Injectable()
export class NotifficationsService {

  private notificationSource = new Subject<MessageDescription>();

  notification$ = this.notificationSource.asObservable();

  waiters: WaiterDictionary;

  constructor() {
    this.waiters = {};
    this.waiters[Status.Success] = 4000;
    this.waiters[Status.Warn] = 6000;
    this.waiters[Status.Info] = 6000;
    this.waiters[Status.Error] = 10000;
  }

  notify(message: string, status: Status, title?: string) {
    let messageDescription = new MessageDescription(message, status, title);
    this.notificationSource.next(messageDescription);
  }

}
