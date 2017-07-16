import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ContactsListComponent } from './contacts-list/contacts-list.component';
import { routing } from "./contacts-list.routes";
import { FileUploaderComponent } from './file-uploader/file-uploader.component';
import { FileUploadModule } from "ng2-file-upload";

@NgModule({
  imports: [
    routing,
    CommonModule,
    FileUploadModule
  ],
  declarations: [ContactsListComponent, FileUploaderComponent, ]
})
export class ContactsListModule { }
