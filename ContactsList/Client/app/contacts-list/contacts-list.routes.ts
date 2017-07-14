import { Routes, RouterModule } from '@angular/router';
import { ContactsListComponent } from "./contacts-list/contacts-list.component";

const routes: Routes = [
  { path: '', component: ContactsListComponent }
];

export const routing = RouterModule.forChild(routes);
