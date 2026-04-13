import { Routes } from '@angular/router';
import { OperationComponent } from './operation/operation';

export const routes: Routes = [
  {
    path: 'operation/:type',
    component: OperationComponent
  }
];
