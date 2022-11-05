import {Component, Inject, OnInit} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef} from "@angular/material/dialog";
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {GroupComponent} from "../group.component";
import {IBanDialogData} from "../../../../shared/interfaces/IBanDialogData";
@Component({
  selector: 'app-ban-dialog',
  templateUrl: './ban-dialog.component.html',
  styleUrls: ['./ban-dialog.component.css']
})
export class BanDialogComponent implements OnInit {
  public banReasonForm: FormGroup;
  public userName: string;
  public minimumBanDate : Date = new Date();
  constructor(
    private dialog: MatDialog,
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<GroupComponent>,
    @Inject(MAT_DIALOG_DATA) banDialogData: IBanDialogData
  ) {
    this.banReasonForm = this.formBuilder.group({
      banReason: ["", Validators.required],
      banUntil: [""],
      hidePosts: [false]
    });
    this.userName = banDialogData.name;
    this.minimumBanDate.setDate(this.minimumBanDate.getDate()+1);
  }
  get banReason(){
    return this.banReasonForm.get("banReason")!;
  }
  save() {
    if (this.banReasonForm.valid){
      this.dialogRef.close(this.banReasonForm.value);
    }
  }

  close() {
    this.dialogRef.close();
  }

  ngOnInit(): void {
  }

}
