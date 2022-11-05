import {KeyValuePipe} from '@angular/common';
import {Component, OnInit} from '@angular/core';
import {Title} from '@angular/platform-browser';
import {Observable} from 'rxjs';
import {InfoService} from 'src/app/services/info.service';
import {IStatistics} from 'src/app/shared/interfaces/IStatistics';
import {ToastService} from "../../services/toast.service";

@Component({
  selector: 'app-statistics',
  templateUrl: './statistics.component.html',
  styleUrls: ['./statistics.component.css']
})
export class StatisticsComponent implements OnInit {
  isLoading: boolean = true;
  title: any;
  type: any;
  data: any;
  columnNames: any;
  options: any;
  width: any;
  height: any;

  title1: any;
  type1: any;
  data1: any;
  columnNames1: any;
  options1: any;
  width1: any;
  height1: any;

  title2: any;
  type2: any;
  data2: any;
  columnNames2: any;
  options2: any;
  width2: any;
  height2: any;

  title3: any;
  type3: any;
  data3: any;
  columnNames3: any;
  options3: any;
  width3: any;
  height3: any;

  title4: any;
  type4: any;
  data4: any;
  columnNames4: any;
  options4: any;
  width4: any;
  height4: any;

  public statistics!: IStatistics;

  constructor(private infoService: InfoService,
              private titleService: Title,
              private toastService: ToastService) {
    this.titleService.setTitle("Estatísticas");
  }

  ngOnInit(): void {
    this.title = 'Locais mais visitados';
    this.type = 'BarChart';

    this.columnNames = ['Locais', 'Nº de vezes']
    this.options = {};
    this.width = 800;
    this.height = 500;


    this.title1 = 'Transporte mais utilizado';
    this.type1 = 'PieChart';

    this.options1 = {};
    this.width1 = 700;
    this.height1 = 500;

    this.title2 = 'Top 10 de viagens com mais Kms';
    this.type2 = 'Bar';

    this.columnNames2 = ['Viagens', 'Kms percorridos'];
    this.options2 = {};
    this.width2 = 700;
    this.height2 = 500;

    this.title3 = 'Top 10 de grupos com mais Kms';
    this.type3 = 'Bar';

    this.columnNames3 = ['Grupos', 'Kms percorridos'];
    this.options3 = {};
    this.width3 = 700;
    this.height3 = 500;

    this.title4 = 'Distribuição de rankings';
    this.type4 = 'PieChart';

    this.options4 = {};
    this.width4 = 700;
    this.height4 = 500;
    this.infoService.getAllStatistics().subscribe(data => {
        this.statistics = data as IStatistics;

        this.data = Object.entries(this.statistics.mostVisitedPlaces);
        this.data1 = Object.entries(this.statistics.transportTypeDistribution);
        this.data2 = Object.entries(this.statistics.tripsByTotalDistance);
        this.data3 = Object.entries(this.statistics.groupsByAverageDistance);
        this.data4 = Object.entries(this.statistics.rankingUserDistribution);
        this.isLoading = false;
      },
      error => {
        this.toastService.showError("Erro ao carregar estatísticas");
      }
      );
  }

}
