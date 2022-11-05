import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditActivityDialogComponent } from './edit-activity-dialog.component';

describe('EditActivityDialogComponent', () => {
  let component: EditActivityDialogComponent;
  let fixture: ComponentFixture<EditActivityDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EditActivityDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EditActivityDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
