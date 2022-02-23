import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AuditDownloadDialogComponent } from './audit-download-dialog.component';

describe('AuditDownloadDialogComponent', () => {
  let component: AuditDownloadDialogComponent;
  let fixture: ComponentFixture<AuditDownloadDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AuditDownloadDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AuditDownloadDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
