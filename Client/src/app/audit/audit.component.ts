import { Component, OnInit, ViewChild } from '@angular/core';
import { ApolloError } from '@apollo/client/errors';
import { DatePipe } from '@angular/common';
import { QueryRef } from 'apollo-angular';
import { Subscription } from 'rxjs';
import { AuditEventsGQL, AuditEventsQuery, Exact } from 'src/generated/graphql';
import { ErrorComponent } from '../error/error.component';
import { Title } from '@angular/platform-browser';

// Dialogs:
import { AuditEventDialogComponent } from '../audit-event-dialog/audit-event-dialog.component';
import { AuditDownloadDialogComponent } from '../audit-download-dialog/audit-download-dialog.component';

// Material:
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';

export interface IAuditEvent {
  id: number;
  localDateTimeString: string;
  object: string,
  source: string | undefined,
  code: number
}

@Component({
  selector: 'app-audit',
  templateUrl: './audit.component.html',
  styleUrls: ['./audit.component.less']
})
export class AuditComponent implements OnInit {
  loading: boolean = true;

  displayedColumns: string[] = ['id', 'code', 'localDateTimeString', 'object', 'source', 'detail'];
  dataSource: MatTableDataSource<IAuditEvent> = new MatTableDataSource<IAuditEvent>();
  pageSizes: Array<number> = [ 10, 15 ];
  length: number = 0;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  
  oldestDateIsoString: string = new Date().toISOString();
  auditEventsSubscription: Subscription | undefined = undefined;
  auditEventsQueryRef : QueryRef<AuditEventsQuery, Exact<{
    skip: number;
    take: number;
    oldestUtcDate: any;
  }>> | undefined = undefined;
  
  constructor(
    //private apollo: Apollo, 
    private auditEventsGQL: AuditEventsGQL, 
    private snackBar: MatSnackBar, 
    private datePipe: DatePipe, 
    public dialog: MatDialog,
    private titleService: Title) { 
      this.titleService.setTitle('Аудит');
    }

  ngOnInit(): void {

    this.auditEventsQueryRef = this.auditEventsGQL
      .watch({skip: 0, take: this.pageSizes[0], oldestUtcDate: this.oldestDateIsoString}, 
        {fetchPolicy: 'network-only'}
      );

    this.auditEventsSubscription = this.auditEventsQueryRef.valueChanges
      .subscribe(result /* typed */ => {

        this.loading = false;
        if(!result.data.auditEvents?.totalCount || !result.data.auditEvents?.items || !result.data.auditEvents?.items[0]) return;

        this.dataSource.data = result.data.auditEvents.items.map(row => <IAuditEvent>{ 
          id: row.id, 
          localDateTimeString: this.datePipe.transform(new Date(row.localDateTime), 'yyyy.MM.dd HH:mm:ss'),
          object: row.object,
          source: row.source,
          code: row.httpStatusCode
        });

        this.length = result.data.auditEvents.totalCount;
        // this.paginator.length = result.data.auditEvents.totalCount; // not working (*)

      }, (error: ApolloError) => {
        this.loading = false;
        const config : MatSnackBarConfig = {
          verticalPosition: 'top',
          panelClass: [ 'snackbar', 'error' ],
          data: error
        };
        this.snackBar.openFromComponent(ErrorComponent, config);
      });
  }

  ngAfterViewInit(): void {
    //this.dataSource.paginator = this.paginator; (*)
  }
  
  ngOnDestroy(): void {
    this.auditEventsSubscription?.unsubscribe();
  }

  pageChanged(event: PageEvent): void {
    this.loading = true;
    this.auditEventsQueryRef?.refetch({skip: event.pageIndex * event.pageSize, take: event.pageSize, oldestUtcDate: this.oldestDateIsoString});
  }

  openDetailDialog(id: number): void {
    const dialogRef = this.dialog.open(AuditEventDialogComponent);
    dialogRef.componentInstance.id = id;
  }

  openDownloadDialog(): void {
    this.dialog.open(AuditDownloadDialogComponent);
  }
}
