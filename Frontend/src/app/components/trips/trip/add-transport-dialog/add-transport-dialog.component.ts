import {Component, Inject, OnInit} from '@angular/core';
import {FormBuilder, FormControl, FormGroup, Validators} from "@angular/forms";
import {IActivity} from "../../../../shared/interfaces/entities/IActivity";
import {MatTableDataSource} from "@angular/material/table";
import {IActivityTransport} from "../../../../shared/interfaces/entities/IActivityTransport";
import {ITransportCreateDialogData} from "../../../../shared/interfaces/ITransportCreateDialogData";
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef} from "@angular/material/dialog";
import {TripComponent} from "../trip.component";
import {ActivityService} from "../../../../services/activity.service";
import {ActivityType} from "../../../../shared/enums/ActivityType";
import {TransportType} from "../../../../shared/enums/TransportType";
import {IActivityTransportSearch} from "../../../../shared/interfaces/IActivityTransportSearch";
import {ToastService} from "../../../../services/toast.service";
import {MatRadioChange} from "@angular/material/radio";
import {MatDatepickerInputEvent} from "@angular/material/datepicker";
import {ValidateTripDate} from "../../../../helpers/form-validators";
import {getTransportTypeName} from "../../../../helpers/event-message";
import {MatDatetimePickerInputEvent} from "@angular-material-components/datetime-picker";

@Component({
  selector: 'app-add-transport-dialog',
  templateUrl: './add-transport-dialog.component.html',
  styleUrls: ['./add-transport-dialog.component.css']
})
export class AddTransportDialogComponent implements OnInit {
  displayedColumns: string[] = ['selection', 'transportType', 'description', 'distance', 'departureTime', 'arrivalTime'];
  public createTransportForm: FormGroup;
  public transportSearchDate = new FormControl(new Date());
  private previousActivity: IActivity;
  public activityToAdd: google.maps.places.PlaceResult;
  public dataSource: MatTableDataSource<IActivityTransport> = new MatTableDataSource<IActivityTransport>([]);
  public minimumDate: Date;
  public maximumDate: Date;
  public transportTypes: Map<String, number> = new Map<String, number>([
    [getTransportTypeName(TransportType.Driving), TransportType.Driving],
    [getTransportTypeName(TransportType.Transit), TransportType.Transit],
    [getTransportTypeName(TransportType.Walking), TransportType.Walking],
    [getTransportTypeName(TransportType.Bicycling), TransportType.Bicycling],
    [getTransportTypeName(TransportType.Other), TransportType.Other],
  ]);

  constructor(
    private dialog: MatDialog,
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<TripComponent>,
    private activityService: ActivityService,
    private toastService: ToastService,
    @Inject(MAT_DIALOG_DATA) activityCreateData: ITransportCreateDialogData
  ) {
    this.activityToAdd = activityCreateData.placeToAdd;
    this.previousActivity = activityCreateData.previousPlace;
    this.transportSearchDate.setValue(this.previousActivity.endingDate);
    this.createTransportForm = formBuilder.group({
      beginningDate: [this.previousActivity.endingDate, Validators.required],
      endingDate: ["", Validators.required],
      address: [""],
      description: ["", Validators.required],
      googlePlaceId: [""],
      expectedBudget: [0, Validators.required],
      activityType: [ActivityType.TRANSPORT, Validators.required],
      transportType: [TransportType.Other, Validators.required]
    }, {validator: ValidateTripDate("beginningDate", "endingDate")});
    this.minimumDate = this.previousActivity.endingDate;
    let minimumDateDate = new Date(this.minimumDate);
    this.maximumDate = new Date(minimumDateDate.getFullYear(), minimumDateDate.getMonth(), minimumDateDate.getDate(), 23, 59, 59);
  }

  get beginningDate() {
    return this.createTransportForm.get("beginningDate")!;
  }

  get endingDate() {
    return this.createTransportForm.get("endingDate")!;
  }

  get address() {
    return this.createTransportForm.get("address")!;
  }

  get description() {
    return this.createTransportForm.get("description")!;
  }

  get expectedBudget() {
    return this.createTransportForm.get("expectedBudget")!;
  }

  get transportType() {
    return this.createTransportForm.get("transportType")!;
  }

  ngOnInit(): void {
    let activityTransportQuery: IActivityTransportSearch = {
      departTime: this.previousActivity.endingDate,
      originPlaceId: this.previousActivity.googlePlaceId,
      destinationPlaceId: this.activityToAdd.place_id!,
      countryCode: "PT"
    }
    this.activityService.searchTransport(activityTransportQuery).subscribe(
      next => {
        this.dataSource.data = next;
      },
      error => {
        this.toastService.showError("Erro ao obter transportes");
      }
    );
  }

  save() {
    if (this.createTransportForm.valid) {
      this.dialogRef.close(this.createTransportForm.value);
    } else {
      this.toastService.showError("Corriga os erros do formulário de adicionar transporte");
    }
  }

  close() {
    this.dialogRef.close();
  }

  onTransportDateChange(event: MatDatetimePickerInputEvent<Date>) {
    let selectedDate: Date = event.value!;
    let activityTransportQuery: IActivityTransportSearch = {
      departTime: selectedDate,
      originPlaceId: this.previousActivity.googlePlaceId,
      destinationPlaceId: this.activityToAdd.place_id!,
      countryCode: "PT"
    }
    this.activityService.searchTransport(activityTransportQuery).subscribe(
      next => {
        this.dataSource.data = next;
      },
      error => {
        this.toastService.showError("Erro ao obter transportes");
      }
    );
  }

  changeTransport(event: MatRadioChange, selectedTransport: IActivityTransport) {
    this.createTransportForm.patchValue({
      beginningDate: selectedTransport.departureTime,
      endingDate: selectedTransport.arrivalTime,
      description: selectedTransport.description,
      transportType: selectedTransport.transportType
    });
  }

  getTransportTypeName(transportType: TransportType): string {
    return getTransportTypeName(transportType);
  }

}
