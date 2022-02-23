import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { environment } from 'src/environments/environment';
import { MatDatepicker } from '@angular/material/datepicker';

@Component({
  selector: 'app-audit-download-dialog',
  templateUrl: './audit-download-dialog.component.html',
  styleUrls: ['./audit-download-dialog.component.less']
})
export class AuditDownloadDialogComponent implements OnInit {

  dtOffsetMs: number = 1000 * 60 * (new Date().getTimezoneOffset());
  range = new FormGroup({
    start!: new FormControl(),
    end!: new FormControl(),
  });

  constructor(public dialogRef: MatDialogRef<AuditDownloadDialogComponent>, private datePipe: DatePipe) { }

  ngOnInit(): void {
  }

  validate(): boolean {
    return this.range.status==='VALID'
      && this.range.value.start instanceof Date 
      && this.range.value.end instanceof Date;
  }

  download(): void {
    if (!this.validate()) return;
    const utcStart = new Date().setTime(this.range.value.start.getTime() - this.dtOffsetMs);
    const utcEnd = new Date().setTime(this.range.value.end.getTime() - this.dtOffsetMs);
    const utcStartString = this.datePipe.transform(utcStart, 'yyyy.MM.dd');
    const utcEndString = this.datePipe.transform(utcEnd, 'yyyy.MM.dd');
    this.dialogRef.close();
    window.location.href = environment.uriRoot + `api/download/audit?utcStart=${utcStartString}&utcEnd=${utcEndString}`;
  }

}
