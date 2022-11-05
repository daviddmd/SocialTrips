import {Component, Inject, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {MatDialog, MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import {RankingService} from 'src/app/services/ranking.service';
import {RankingsComponent} from '../rankings.component';
import {IRanking} from "../../../shared/interfaces/entities/IRanking";

@Component({
  selector: 'app-update-dialog',
  templateUrl: './update-dialog.component.html',
  styleUrls: ['./update-dialog.component.css']
})
export class UpdateDialogComponent implements OnInit {

  public updateRankingForm: FormGroup;
  public nameDialog: string;
  public isSubmitting: boolean = false;

  constructor(
    private dialog: MatDialog,
    private formBuilder: FormBuilder,
    private rankingService: RankingService,
    private dialogRef: MatDialogRef<RankingsComponent>,
    @Inject(MAT_DIALOG_DATA) rankingToUpdate: IRanking
  ) {
    this.updateRankingForm = this.formBuilder.group({
      name: [rankingToUpdate.name, Validators.required],
      description: [rankingToUpdate.description, Validators.required],
      color: [rankingToUpdate.color, Validators.required],
      minimumKilometers: [rankingToUpdate.minimumKilometers, Validators.required]
    });
    this.nameDialog = rankingToUpdate.name;
  }

  get name() {
    return this.updateRankingForm.get("name")!;
  }

  get description() {
    return this.updateRankingForm.get("description")!;
  }

  get color() {
    return this.updateRankingForm.get("color")!;
  }

  get minimumKilometers() {
    return this.updateRankingForm.get("minimumKilometers")!;
  }

  save() {
    this.isSubmitting = true;
    if (this.updateRankingForm.valid) {
      this.dialogRef.close(this.updateRankingForm.value);
    } else {
      this.isSubmitting = false;
    }
  }

  close() {
    this.dialogRef.close();
  }

  ngOnInit(): void {

  }

}
