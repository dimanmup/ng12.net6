import { ApolloError } from '@apollo/client/errors';
import { AuditEventDescriptionByIdGQL } from 'src/generated/graphql';
import { Component, OnInit } from '@angular/core';
import { ErrorComponent } from '../error/error.component';
import { Subscription } from 'rxjs';
import { DatePipe } from '@angular/common';

// Material:
import { MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

@Component({
  selector: 'app-audit-event-dialog',
  templateUrl: './audit-event-dialog.component.html',
  styleUrls: ['./audit-event-dialog.component.less']
})
export class AuditEventDialogComponent implements OnInit {

  public id: number = 1;
  public title: string | null | undefined;
  public description : string | null | undefined;
  private sub: Subscription | undefined = undefined;

  constructor(
    public dialogRef: MatDialogRef<AuditEventDialogComponent>, 
    private auditEventDescriptionById: AuditEventDescriptionByIdGQL,
    private snackBar: MatSnackBar,
    private datePipe: DatePipe) { }

  ngOnInit(): void {
    this.sub = this.auditEventDescriptionById
      .watch({id: this.id}, {fetchPolicy: 'network-only'})
      .valueChanges
      .subscribe(result => {
        if(!result.data.auditEvents?.items || !result.data.auditEvents?.items[0]) {
          this.title = 'id ' + this.id;
          this.description = 'Данные не найдены.'
        }
        else {
          this.title = 'id ' + this.id + ', ' + this.datePipe.transform(new Date(result.data.auditEvents.items[0].localDateTime), 'yyyy.MM.dd HH:mm:ss')
          this.description = result.data.auditEvents.items[0].description?.replace(/(\\r\\n)|(\\n)/g, '\n  ')
            .replace(/\\"/g, '"')
            .replace(/"\{/g, "{")
            .replace(/\}"/g, "}");
        }
      }, (error: ApolloError) => {
        const config : MatSnackBarConfig = {
          verticalPosition: 'top',
          panelClass: [ 'snackbar', 'error' ],
          data: error
        };
        this.snackBar.openFromComponent(ErrorComponent, config);
      }, () => this.sub?.unsubscribe());
  }

}
