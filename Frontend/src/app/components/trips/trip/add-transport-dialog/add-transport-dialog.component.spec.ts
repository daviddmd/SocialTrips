import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddTransportDialogComponent } from './add-transport-dialog.component';

describe('AddTransportDialogComponent', () => {
  let component: AddTransportDialogComponent;
  let fixture: ComponentFixture<AddTransportDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddTransportDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddTransportDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
