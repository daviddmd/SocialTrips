import {Component, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {MatDialog, MatDialogConfig} from '@angular/material/dialog';
import {Title} from '@angular/platform-browser';
import {Router} from '@angular/router';
import {Observable} from 'rxjs';
import {RankingService} from 'src/app/services/ranking.service';
import {ToastService} from 'src/app/services/toast.service';
import {UpdateDialogComponent} from './update-dialog/update-dialog.component';
import {IRanking} from "../../shared/interfaces/entities/IRanking";

@Component({
  selector: 'app-rankings',
  templateUrl: './rankings.component.html',
  styleUrls: ['./rankings.component.css']
})
export class RankingsComponent implements OnInit {

  public isLoading: boolean = true;
  public allRankings!: Observable<IRanking[]>;
  public createRankingForm: FormGroup;
  public dialogName!: string;
  public isSubmitting: boolean = false;

  constructor(
    private rankingService: RankingService,
    private toastService: ToastService,
    private router: Router,
    private formBuilder: FormBuilder,
    public dialog: MatDialog,
    private titleService: Title) {
    this.createRankingForm = this.formBuilder.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      color: ['', Validators.required],
      minimumKilometers: ['', Validators.required]
    });

    this.titleService.setTitle("Rankings");
  }

  get name() {
    return this.createRankingForm.get("name")!;
  }

  get description() {
    return this.createRankingForm.get("description")!;
  }

  get color() {
    return this.createRankingForm.get("color")!;
  }

  get minimumKilometers() {
    return this.createRankingForm.get("minimumKilometers")!;
  }

  ngOnInit(): void {
    this.allRankings = this.rankingService.getAllRankings();
  }

  onSubmit(): void {
    if (!this.createRankingForm.valid) {
      this.toastService.showError("Preencha ou corriga os erros no formulário de criação de ranking");
      return;
    }
    this.createRankingForm.patchValue(
      {
        color: "#" + this.color.value.hex
      }
    )
    this.isSubmitting = true;
    this.rankingService.createRanking(this.createRankingForm.value).subscribe(
      data => {
        this.toastService.showSucess("Ranking criado com sucesso");
        this.allRankings = this.rankingService.getAllRankings();
      },
      error => {
        this.toastService.showError("Erro na criação do ranking.");
      }
    ).add(() => {
      this.isSubmitting = false;
    });
  }

  deleteRanking(ranking: IRanking): void {
    this.isSubmitting = true;
    this.rankingService.removeRanking(ranking.id).subscribe(
      sucess => {
        this.toastService.showSucess("Ranking removido com sucesso.");
        this.allRankings = this.rankingService.getAllRankings();
      },
      error => {
        this.toastService.showError("Houve um problema");
      }
    ).add(() => {
      this.isSubmitting = false;
    });
  }

  updateRanking(ranking: IRanking): void {
    this.isSubmitting = true;
    let dialogConfig = new MatDialogConfig<IRanking>();
    dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;
    dialogConfig.data = ranking;
    let dialogRef = this.dialog.open(UpdateDialogComponent, dialogConfig);
    dialogRef.afterClosed().subscribe(
      val => {
        if (val != undefined) {
          let update = val as IRanking;
          this.rankingService.updateRanking(ranking.id, update).subscribe(
            success => {
              this.toastService.showSucess("Ranking atualizado com sucesso!");
              this.allRankings = this.rankingService.getAllRankings();
            },
            error => {
              switch (error.status) {
                case 400:
                  this.toastService.showError("Erro na actualização do ranking, a nova distância mínima tem que ser diferente à distância mínima de um ranking existente.");
                  break;
                case 403:
                  this.toastService.showError("Não existe um ranking com este ID");
                  break;
                default:
                  this.toastService.showError("Erro ao atualizar ranking");
              }
            }
          )
        }
      }
    ).add(() => {
      this.isSubmitting = false;
    });
  }

}
