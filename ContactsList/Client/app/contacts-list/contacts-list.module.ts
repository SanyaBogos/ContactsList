import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ContactsListComponent } from './contacts-list/contacts-list.component';
import { routing } from "./contacts-list.routes";

@NgModule({
  imports: [
    routing,
    CommonModule
  ],
  declarations: [ContactsListComponent]
})
export class ContactsListModule { }
