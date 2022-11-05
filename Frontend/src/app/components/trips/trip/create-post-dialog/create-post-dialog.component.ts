import {Component, Inject, OnInit} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef} from "@angular/material/dialog";
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {TripComponent} from "../trip.component";
import {ToastService} from "../../../../services/toast.service";
import {ITrip} from "../../../../shared/interfaces/entities/ITrip";

@Component({
  selector: 'app-create-post-dialog',
  templateUrl: './create-post-dialog.component.html',
  styleUrls: ['./create-post-dialog.component.css']
})
export class CreatePostDialogComponent implements OnInit {
  public postCreateForm: FormGroup;
  public minimumDate: Date;
  public maximumDate: Date;
  constructor(
    private dialog: MatDialog,
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<TripComponent>,
    private toastService: ToastService,
    @Inject(MAT_DIALOG_DATA) trip: ITrip
  ) {
      this.minimumDate = new Date(trip.beginningDate);
      this.maximumDate = new Date(trip.endingDate);
      this.postCreateForm = this.formBuilder.group({
        tripId: [trip.id,Validators.required],
        description: ["",Validators.required],
        date:["",Validators.required]
      });
  }
  get description(){
    return this.postCreateForm.get("description")!;
  }
  get date(){
    return this.postCreateForm.get("date")!;
  }
  ngOnInit(): void {
  }
  save() {
    if (this.postCreateForm.valid) {
      this.dialogRef.close(this.postCreateForm.value);
    } else {
      this.toastService.showError("Corriga os erros do formulário de criar publicação");
    }
  }

  close() {
    this.dialogRef.close();
  }

}
