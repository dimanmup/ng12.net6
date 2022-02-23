import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AuditEventDialogComponent } from './audit-event-dialog.component';

describe('AuditEventDialogComponent', () => {
  let component: AuditEventDialogComponent;
  let fixture: ComponentFixture<AuditEventDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AuditEventDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AuditEventDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
