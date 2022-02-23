import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Components:
import { AuditComponent } from './audit/audit.component';
import { UploadComponent } from './upload/upload.component';

const routes: Routes = [
  { path: 'audit', component: AuditComponent },
  { path: 'upload', component: UploadComponent }
];
@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
