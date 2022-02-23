import { AppRoutingModule } from './app-routing.module';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { DatePipe } from '@angular/common';
import { DefaultUrlSerializer, UrlSerializer, UrlTree } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { GraphQLModule } from './graphql.module';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

// Material:
import { MatButtonModule } from '@angular/material/button';
import { MAT_DATE_LOCALE, MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorIntl, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBarModule, MAT_SNACK_BAR_DEFAULT_OPTIONS } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';

// Components:
import { AppComponent } from './app.component';
import { AuditComponent } from './audit/audit.component';
import { AuditEventDialogComponent } from './audit-event-dialog/audit-event-dialog.component';
import { AuditDownloadDialogComponent } from './audit-download-dialog/audit-download-dialog.component';
import { ErrorComponent } from './error/error.component';
import { UploadComponent } from './upload/upload.component';

// Locals:
import { MatPaginatorIntl_ruRU } from './locals/locals.paginator.ru-RU';

export class LowerCaseUrlSerializer extends DefaultUrlSerializer {
  parse(url: string): UrlTree {
    return super.parse(url.toLowerCase());
  }
}

@NgModule({
  declarations: [
    AppComponent,
    AuditComponent,
    AuditEventDialogComponent,
    AuditDownloadDialogComponent,
    ErrorComponent,
    UploadComponent
  ],
  imports: [
    AppRoutingModule,   
    BrowserAnimationsModule,
    BrowserModule,
    FormsModule,
    HttpClientModule,
    NgbModule,
    ReactiveFormsModule,

    // Material:
    MatButtonModule,
    MatDatepickerModule, MatNativeDateModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatTableModule,
    MatTabsModule,
    MatTooltipModule,

    GraphQLModule,
     
  ],
  providers: [
    { provide: MAT_SNACK_BAR_DEFAULT_OPTIONS, useValue : {} }, // Для отправки MatSnackBarConfig.data в компонент snackbar.
    { provide: UrlSerializer, useClass: LowerCaseUrlSerializer },
    { provide: DatePipe },
    { provide: MAT_DATE_LOCALE, useValue: 'ru-RU'},
    { provide: MatPaginatorIntl, useClass: MatPaginatorIntl_ruRU}
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
