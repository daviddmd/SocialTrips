import {Component, Inject, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef} from "@angular/material/dialog";
import {TripComponent} from "../trip.component";
import {ActivityService} from "../../../../services/activity.service";
import {ToastService} from "../../../../services/toast.service";
import {ITransportCreateDialogData} from "../../../../shared/interfaces/ITransportCreateDialogData";
import {IActivityCreateDialogData} from "../../../../shared/interfaces/IActivityCreateDialogData";
import {ActivityType} from "../../../../shared/enums/ActivityType";
import {TransportType} from "../../../../shared/enums/TransportType";
import {getActivityName} from "../../../../helpers/event-message";
import {ValidateTripDate} from "../../../../helpers/form-validators";

@Component({
  selector: 'app-add-activity-dialog',
  templateUrl: './add-activity-dialog.component.html',
  styleUrls: ['./add-activity-dialog.component.css']
})
export class AddActivityDialogComponent implements OnInit {
  public createActivityForm: FormGroup;
  public minimumDate: Date;
  public maximumDate: Date;
  public activityTypes: Map<String, number> = new Map<String, number>([
    [getActivityName(ActivityType.VISIT), ActivityType.VISIT],
    [getActivityName(ActivityType.FOOD), ActivityType.FOOD],
    [getActivityName(ActivityType.EXCURSION), ActivityType.EXCURSION],
    [getActivityName(ActivityType.SHOPPING), ActivityType.SHOPPING],
    [getActivityName(ActivityType.LODGING), ActivityType.LODGING],
  ]);

  constructor(
    private dialog: MatDialog,
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<TripComponent>,
    private toastService: ToastService,
    @Inject(MAT_DIALOG_DATA) activityCreateData: IActivityCreateDialogData
  ) {
    this.createActivityForm = this.formBuilder.group({
      beginningDate: [activityCreateData.minimumBeginningDate, Validators.required],
      endingDate: ["", Validators.required],
      address: [activityCreateData.placeToAdd.formatted_address, Validators.required],
      description: [activityCreateData.placeToAdd.name, Validators.required],
      googlePlaceId: [activityCreateData.placeToAdd.place_id, Validators.required],
      expectedBudget: [0, Validators.required],
      activityType: [ActivityType.VISIT, Validators.required],
      transportType: [TransportType.None, Validators.required]
    }, {validator: ValidateTripDate("beginningDate", "endingDate")});
    this.minimumDate = activityCreateData.minimumBeginningDate;
    let mininumDateDate = new Date(this.minimumDate);
    this.maximumDate = new Date(mininumDateDate.getFullYear(), mininumDateDate.getMonth(), mininumDateDate.getDate(), 23, 59, 59);
  }

  get beginningDate() {
    return this.createActivityForm.get("beginningDate")!;
  }

  get endingDate() {
    return this.createActivityForm.get("endingDate")!;
  }

  get address() {
    return this.createActivityForm.get("address")!;
  }

  get description() {
    return this.createActivityForm.get("description")!;
  }

  get expectedBudget() {
    return this.createActivityForm.get("expectedBudget")!;
  }

  get activityType() {
    return this.createActivityForm.get("activityType")!;
  }

  ngOnInit(): void {
  }

  save() {
    if (this.createActivityForm.valid) {
      this.dialogRef.close(this.createActivityForm.value);
    } else {
      this.toastService.showError("Corriga os erros do formulário de adicionar actividade");
    }
  }

  close() {
    this.dialogRef.close();
  }

}
