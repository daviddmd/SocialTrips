import {Component, Inject, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {getActivityName, getTransportTypeName} from "../../../../helpers/event-message";
import {ActivityType} from "../../../../shared/enums/ActivityType";
import {TransportType} from "../../../../shared/enums/TransportType";
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef} from "@angular/material/dialog";
import {TripComponent} from "../trip.component";
import {ToastService} from "../../../../services/toast.service";
import {IActivityEditDialogData} from "../../../../shared/interfaces/IActivityEditDialogData";
import {ValidateTripDate} from "../../../../helpers/form-validators";

@Component({
  selector: 'app-edit-activity-dialog',
  templateUrl: './edit-activity-dialog.component.html',
  styleUrls: ['./edit-activity-dialog.component.css']
})
export class EditActivityDialogComponent implements OnInit {
  public updateActivityForm: FormGroup;
  public minimumDate: Date;
  public isTransport : boolean;
  public maximumDate: Date;
  public activityTypes: Map<String, number> = new Map<String, number>([
    [getActivityName(ActivityType.VISIT), ActivityType.VISIT],
    [getActivityName(ActivityType.FOOD), ActivityType.FOOD],
    [getActivityName(ActivityType.EXCURSION), ActivityType.EXCURSION],
    [getActivityName(ActivityType.SHOPPING), ActivityType.SHOPPING],
    [getActivityName(ActivityType.LODGING), ActivityType.LODGING],
  ]);
  public transportTypes: Map<String, number> = new Map<String, number>([
    [getTransportTypeName(TransportType.Driving), TransportType.Driving],
    [getTransportTypeName(TransportType.Transit), TransportType.Transit],
    [getTransportTypeName(TransportType.Walking), TransportType.Walking],
    [getTransportTypeName(TransportType.Bicycling), TransportType.Bicycling],
    [getTransportTypeName(TransportType.Other), TransportType.Other],
  ]);

  //adicionar
  constructor(
    private dialog: MatDialog,
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<TripComponent>,
    private toastService: ToastService,
    @Inject(MAT_DIALOG_DATA) activityEditData: IActivityEditDialogData
  ) {
    this.minimumDate = activityEditData.minimumDate? activityEditData.minimumDate : new Date();
    this.maximumDate = activityEditData.maximumDate? activityEditData.maximumDate : new Date(2099,12,31);
    this.updateActivityForm = this.formBuilder.group({
      beginningDate: [activityEditData.activity.beginningDate, Validators.required],
      endingDate: [activityEditData.activity.endingDate, Validators.required],
      address: [activityEditData.activity.address],
      description: [activityEditData.activity.description, Validators.required],
      expectedBudget: [activityEditData.activity.expectedBudget, Validators.required],
      activityType: [activityEditData.activity.activityType, Validators.required],
      transportType: [activityEditData.activity.transportType, Validators.required]
    }, {validator: ValidateTripDate("beginningDate", "endingDate")});
    this.isTransport = activityEditData.activity.activityType == ActivityType.TRANSPORT;

  }
  get beginningDate() {
    return this.updateActivityForm.get("beginningDate")!;
  }

  get endingDate() {
    return this.updateActivityForm.get("endingDate")!;
  }

  get address() {
    return this.updateActivityForm.get("address")!;
  }

  get description() {
    return this.updateActivityForm.get("description")!;
  }

  get expectedBudget() {
    return this.updateActivityForm.get("expectedBudget")!;
  }

  get activityType() {
    return this.updateActivityForm.get("activityType")!;
  }
  get transportType(){
    return this.updateActivityForm.get("transportType")!;
  }

  ngOnInit(): void {
  }
  save() {
    if (this.updateActivityForm.valid) {
      this.dialogRef.close(this.updateActivityForm.value);
    } else {
      this.toastService.showError("Corriga os erros do formulário de actualizar actividade");
    }
  }

  close() {
    this.dialogRef.close();
  }
}
