import {Component, OnInit} from '@angular/core';
import {ToastService} from "../../services/toast.service";
import {Title} from "@angular/platform-browser";
import {InfoService} from 'src/app/services/info.service';
import {IGroup} from "../../shared/interfaces/entities/IGroup";
import {ITrip} from "../../shared/interfaces/entities/ITrip";
import {IPost} from "../../shared/interfaces/entities/IPost";
import {AuthenticationService} from "../../services/authentication.service";
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  public recommendedGroups: IGroup[] = [];
  public featuredGroups: IGroup[] = [];
  public recommendedTrips: ITrip[] = [];
  public latestPostsCommunity: IPost[] = [];
  public latestPostsFriends: IPost[] = [];
  public isLoading: boolean = true;
  public isLoggedIn: boolean = false;
  public default_group_picture: string = environment.default_group_picture;
  public default_trip_picture: string = environment.default_trip_picture;

  constructor(
    public toastService: ToastService,
    private infoService: InfoService,
    private titleService: Title,
    private authenticationService: AuthenticationService
  ) {
    this.titleService.setTitle("Viagens Sociais");
    this.isLoggedIn=authenticationService.isLoggedIn();
  }

  ngOnInit(): void {
    this.infoService.getRecommendations().subscribe(
      next => {
        this.recommendedGroups = next.recommendedGroups;
        this.featuredGroups = next.featuredGroups;
        this.recommendedTrips = next.recommendedTrips;
        this.latestPostsCommunity = next.latestPostsCommunity;
        this.latestPostsFriends = next.latestPostsFriends;
        this.isLoading = false;
      },
      error => {
        this.toastService.showError("Erro ao carregar recomendações.");
      }
    )
  }
}
