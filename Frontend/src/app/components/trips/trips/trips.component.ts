import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthenticationService } from 'src/app/services/authentication.service';
import { ToastService } from 'src/app/services/toast.service';
import { TripService } from 'src/app/services/trip.service';
import { UserService } from 'src/app/services/user.service';
import { ITrip } from 'src/app/shared/interfaces/entities/ITrip';
import { IUser } from 'src/app/shared/interfaces/entities/IUser';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-trips',
  templateUrl: './trips.component.html',
  styleUrls: ['./trips.component.css']
})

export class TripsComponent implements OnInit {

  public allTrips!: Observable<ITrip[]>;
  public user!: IUser;
  tripsSearch!: Observable<ITrip[]>;
  public tripsSearchForm: FormGroup;
  public isLoading: boolean = true;
  public default_trip_picture: string = environment.default_trip_picture;
  public isAdmin: boolean;

  constructor(
    private formBuilder: FormBuilder,
    private tripService: TripService,
    private authenticationService: AuthenticationService,
    private userService: UserService,
    private toastService: ToastService,
    private router: Router,
    private titleService: Title) {

    this.isAdmin = authenticationService.isAdmin();
    this.tripsSearchForm = this.formBuilder.group({
      name: [""],
      description: [""],
      destination: [""]
    });
    this.titleService.setTitle("Viagens");
  }

  get name(){
    return this.tripsSearchForm.get("name")!;
  }

  get description(){
    return this.tripsSearchForm.get("description")!;
  }

  get destination(){
    return this.tripsSearchForm.get("destination")!;
  }

  ngOnInit(): void {
    this.userService.getSelf().subscribe(next => {
      this.user = next as IUser;
    },
    error => {
      this.toastService.showError("Erro ao carregar utilizador");
    }).add(() => {
    this.isLoading = false;
  });
  this.allTrips = this.tripService.getAllTrips();

  }

  searchTrips(){
    if (!this.tripsSearchForm.valid){
      this.toastService.showError("Necessita de preencher pelo menos um campo!");
      return;
    }

    this.tripsSearch = this.tripService.searchTrips(this.name.value, this.description.value, this.destination.value);
  }

  acceptInvitation(tripId: string, tripInviteId: string) {
    this.tripService.joinTrip(tripId, tripInviteId).subscribe(data => {
      this.toastService.showSucess("Juntou-se a esta viagem com sucesso!");
      this.router.navigate(['/trip', tripId]);
    }, error => {
      this.toastService.showError("Erro ao juntar-se à viagem.");
    });
  }

  declineInvitation(tripInvitateId: string) {
    this.tripService.deleteInvitation(tripInvitateId).subscribe(data => {
        location.reload();
      },
      error => {
        this.toastService.showError("Erro ao rejeitar o convite");
      });
  }

}
