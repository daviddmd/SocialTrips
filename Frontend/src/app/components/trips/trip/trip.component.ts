import {AfterViewInit, Component, ElementRef, NgZone, OnInit, ViewChild} from '@angular/core';
import {ITrip} from "../../../shared/interfaces/entities/ITrip";
import {map, min, Observable, of, Subscription} from "rxjs";
import {environment} from "../../../../environments/environment";
import {IUser} from "../../../shared/interfaces/entities/IUser";
import {IActivity} from "../../../shared/interfaces/entities/IActivity";
import {AuthenticationService} from "../../../services/authentication.service";
import {TripService} from "../../../services/trip.service";
import {ToastService} from "../../../services/toast.service";
import {FormBuilder, FormControl, FormGroup, Validators} from "@angular/forms";
import {UserService} from "../../../services/user.service";
import {ActivatedRoute, Router} from "@angular/router";
import {Title} from "@angular/platform-browser";
import {MatDialog, MatDialogConfig} from "@angular/material/dialog";
import {PostService} from "../../../services/post.service";
import {ActivityService} from "../../../services/activity.service";
import {ValidateTripDate} from "../../../helpers/form-validators";
import {UserGroupRole} from "../../../shared/enums/UserGroupRole";
import {IPost} from "../../../shared/interfaces/entities/IPost";
import {EventType} from "../../../shared/enums/EventType";
import {getActivityName, getEventMessage, getTransportTypeName} from "../../../helpers/event-message";
import {ErrorType} from "../../../shared/enums/ErrorType";
import {ActivityType} from "../../../shared/enums/ActivityType";
import {TransportType} from "../../../shared/enums/TransportType";
import {MapsAPILoader} from "@agm/core";
import Geocoder = google.maps.Geocoder;
import {ITransportCreateDialogData} from "../../../shared/interfaces/ITransportCreateDialogData";
import {AddActivityDialogComponent} from "./add-activity-dialog/add-activity-dialog.component";
import {IActivityCreateDialogData} from "../../../shared/interfaces/IActivityCreateDialogData";
import {AddTransportDialogComponent} from "./add-transport-dialog/add-transport-dialog.component";
import {IActivityCreateIndividual} from "../../../shared/interfaces/IActivityCreateIndividual";
import {IActivityCreate} from "../../../shared/interfaces/IActivityCreate";
import {MatTableDataSource} from "@angular/material/table";
import {MatPaginator} from "@angular/material/paginator";
import {IActivityEditDialogData} from "../../../shared/interfaces/IActivityEditDialogData";
import {EditActivityDialogComponent} from "./edit-activity-dialog/edit-activity-dialog.component";
import {IActivityUpdate} from "../../../shared/interfaces/IActivityUpdate";
import {MatSort} from "@angular/material/sort";
import {CreatePostDialogComponent} from "./create-post-dialog/create-post-dialog.component";
import {IPostCreate} from "../../../shared/interfaces/IPostCreate";
import {IPostEditDialogData} from "../../../shared/interfaces/IPostEditDialogData";
import {EditPostDialogComponent} from "./edit-post-dialog/edit-post-dialog.component";
import {IPostUpdate} from "../../../shared/interfaces/IPostUpdate";
import {IPostEditDialogReturnData} from "../../../shared/interfaces/IPostEditDialogReturnData";

@Component({
  selector: 'app-trip',
  templateUrl: './trip.component.html',
  styleUrls: ['./trip.component.css']
})
export class TripComponent implements OnInit, AfterViewInit {
  @ViewChild('activitiesPaginator', {static: true}) paginator!: MatPaginator;
  public activitiesDataSource: MatTableDataSource<IActivity> = new MatTableDataSource<IActivity>([]);
  public displayedActivitiesColumns: string[] = ["beginningDate", "endingDate", "description", "address", "type", "cost"];

  @ViewChild('postTablePaginator', {static: true}) postTablePaginator!: MatPaginator;
  @ViewChild('postTableSort', {static: true}) postTableSort!: MatSort;
  public postsDataSource: MatTableDataSource<IPost> = new MatTableDataSource<IPost>([]);
  public displayedPostsColumns: string[] = ['description', 'user', 'date', 'attachments', 'actions'];

  public ownId: string;
  public isAdmin: boolean = false;
  public isManager: boolean = false;
  public isModerator: boolean = false;
  public isLoadingTrip: boolean = true;
  public submittingData: boolean = false;
  private tripId: string = "";
  public isInTrip: boolean = false;
  //não se pode gerir o itinerário, fechar uma viagem ou juntar-se ao grupo quando a viagem está terminada
  public tripFinished: boolean = true;
  //apenas se pode juntar à viagem se estiver no grupo, mostrar botão com link para o grupo caso não esteja, depois alterar para entrar/sair
  public isInGroup: boolean = false;
  public trip!: ITrip;
  /*
  última actividade deste dia. usar para centrar o mapa, disponibilizar o botão para eliminar a ultima actividade e quando criar uma nota actividade para passar
  a hora de chegada dessa actividade para o transporte começar logo a seguir
  null se nao tiver actividades, o que sera interpretado pelo dialogo para nao colocar aquela caixa de transporte e formulario de transportes.
   */
  private routeSub: Subscription = new Subscription;
  public default_user_picture: string = environment.default_user_picture;
  public default_group_picture: string = environment.default_group_picture;
  public default_trip_picture: string = environment.default_trip_picture;
  //formulário de actualização de imagem de um grupo
  public tripImageToUpload!: File;
  //formulário de adição de imagem a uma publicação
  //irá retornar apenas utilizadores que estejam no grupo para convites
  usersSearch!: Observable<IUser[]>;
  //atividades do dia, actualizado no carregamento da pagina e quando se muda o dia
  public activitiesDay: IActivity[] = [];
  public lastActivityDay: IActivity | null = null;
  public tripBeginningDate!: Date;
  public tripEndingDate!: Date;
  public tripUpdateForm: FormGroup;
  public postCreateForm: FormGroup;
  public tripImageForm: FormGroup;
  public userInviteSearchForm: FormGroup;
  public selectedDate = new FormControl(new Date());
  //valores por defeito para centrar o mapa, caso não hajam actividades nesse dia
  public latitude: number = 41.14961;
  public longitude: number = -8.61099;
  public zoom: number = 16;
  public strokeWeight: number = 1.5;
  public readonly ACTIVITY_TRANSPORT: ActivityType = ActivityType.TRANSPORT;
  public fitBounds: boolean = false;
  private geoCoder!: Geocoder;
  //formulário para criar publicacao (caso esteja na viagem)
  //diálogo para atualizar post que apenas aparece se estiver na viagem

  @ViewChild('search')
  public searchElementRef!: ElementRef;

  constructor(
    private authenticationService: AuthenticationService,
    private tripService: TripService,
    private toastService: ToastService,
    private userService: UserService,
    private postService: PostService,
    private activityService: ActivityService,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private titleService: Title,
    private router: Router,
    private dialog: MatDialog,
    private mapsAPILoader: MapsAPILoader,
    private ngZone: NgZone
  ) {
    this.ownId = this.authenticationService.getUserId();
    this.isAdmin = this.authenticationService.isAdmin();
    this.tripUpdateForm = this.formBuilder.group({
      name: ["", Validators.required],
      description: ["", Validators.required],
      beginningDate: ["", Validators.required],
      endingDate: ["", Validators.required],
      //se estiver completa a viagem, depois esconder o input de isCompleted
      //dar patch a ambos com os valores actuais
      isCompleted: [false],
      isPrivate: [""]
    }, {validator: ValidateTripDate("beginningDate", "endingDate")});
    this.postCreateForm = this.formBuilder.group({
      tripId: ["", Validators.required],
      description: ["", Validators.required],
      //datepicker tem como min e max os valores de min e max definidos acima
      date: ["", Validators.required]
    });
    this.tripImageForm = this.formBuilder.group({
      image: ["", Validators.required]
    });
    //apenas serão retornados utilizadores que estejam no grupo
    this.userInviteSearchForm = this.formBuilder.group({
      nameOrEmail: ["", Validators.required]
    });
  }


  ngAfterViewInit(): void {
    this.activitiesDataSource.paginator = this.paginator;

    this.postsDataSource.paginator = this.postTablePaginator;
    this.postsDataSource.sort = this.postTableSort;
  }

  searchPosts(event: KeyboardEvent) {
    let input = (event.target as HTMLInputElement).value;
    this.postsDataSource.filter = input.trim().toLowerCase();
  }

  get tripName() {
    return this.tripUpdateForm.get("name")!;
  }

  get tripDescription() {
    return this.tripUpdateForm.get("description")!;
  }

  get formTripBeginningDate() {
    return this.tripUpdateForm.get("beginningDate")!;
  }

  get formTripEndingDate() {
    return this.tripUpdateForm.get("endingDate")!;
  }

  get nameOrEmailSearch() {
    return this.userInviteSearchForm.get("nameOrEmail")!;
  }

  get tripImage() {
    return this.tripImageForm.get("image")!;
  }

  ngOnInit(): void {
    this.routeSub = this.route.params.subscribe(params => {
      let tripId = params["id"];
      this.tripId = tripId;
      this.tripService.getTrip(tripId).subscribe(next => {
          this.trip = next as ITrip;
          this.titleService.setTitle(this.trip.name);
          this.tripBeginningDate = this.trip.beginningDate;
          this.tripEndingDate = this.trip.endingDate;
          this.tripUpdateForm.patchValue({
            name: this.trip.name,
            description: this.trip.description,
            beginningDate: this.trip.beginningDate,
            endingDate: this.trip.endingDate,
            isCompleted: this.trip.isCompleted,
            isPrivate: this.trip.isPrivate
          });
          this.tripFinished = this.trip.isCompleted;
          this.postCreateForm.patchValue({
            tripId: this.tripId
          });
          /*
          começar o itinerário por defeito no primeiro dia.
          procura o itinerário pela primeira actividade e define a data de início como o dia dessa actividade, caso não existam actividades
          no primeiro dia
           */
          this.selectedDate.setValue(this.tripBeginningDate);
          this.updateItinerary();
          //se o primeiro dia da viagem não tiver actividades, saltar para um que tenha
          if (this.activitiesDay.length == 0 && this.trip.activities.length != 0) {
            let activity: IActivity = this.trip.activities[0];
            this.selectedDate.setValue(activity.beginningDate);
            this.updateItinerary();
          }
          let ownId = this.authenticationService.getUserId();
          //role do utilizador actual em relação ao grupo da viagem; só disponível para gestores
          for (let user of this.trip.group.users) {
            if (user.user.id == ownId) {
              if (user.role == UserGroupRole.MANAGER) {
                this.isManager = true;
              }
              if (user.role == UserGroupRole.MODERATOR) {
                this.isModerator = true;
              }
              this.isInGroup = true;
              break;
            }
          }
          if (this.isAdmin || this.isManager) {
            this.displayedActivitiesColumns.push("action");
          }
          for (let user of this.trip.users) {
            if (user.user.id == ownId) {
              this.isInTrip = true;
              break;
            }
          }
          this.postsDataSource.data = this.trip.posts;
          this.isLoadingTrip = false;
          this.mapsAPILoader.load().then(() => {
            //potencialmente deprecar
            this.geoCoder = new google.maps.Geocoder;
            let autocomplete = new google.maps.places.Autocomplete(this.searchElementRef.nativeElement);
            //escuta até o sítio escolhido mudar (utilizador escolhe um novo), abre o formulário
            autocomplete.addListener("place_changed", () => {
              this.ngZone.run(() => {
                let place: google.maps.places.PlaceResult = autocomplete.getPlace();
                if (place.geometry === undefined || place.geometry === null) {
                  return;
                }
                this.zoom = 20;
                this.latitude = place.geometry.location.lat();
                this.longitude = place.geometry.location.lng();
                this.submittingData = true;
                //primeira actividade do dia
                //definir o lastActivity como a própria após adicionar
                if (this.lastActivityDay == null) {
                  let dialogConfig = new MatDialogConfig<IActivityCreateDialogData>();
                  dialogConfig.disableClose = true;
                  dialogConfig.autoFocus = true;
                  //definir min como a data passada às 8 da manha e max o dia seguinte
                  let minimumDate = new Date(this.selectedDate.value);
                  minimumDate.setHours(8);
                  dialogConfig.data = {placeToAdd: place, minimumBeginningDate: minimumDate}
                  let dialogActivity = this.dialog.open(AddActivityDialogComponent, dialogConfig);
                  dialogActivity.afterClosed().subscribe(
                    activity => {
                      if (activity != undefined) {
                        let activityCreate: IActivityCreateIndividual = activity as IActivityCreateIndividual;
                        let activityPost: IActivityCreate = {
                          activityCreate: activityCreate,
                          activityTransport: null,
                          tripId: this.tripId
                        }
                        this.createActivity(activityPost);
                      }
                    }
                  ).add(() => {
                    this.submittingData = false;
                  })
                } else {
                  let dialogConfig = new MatDialogConfig<ITransportCreateDialogData>();
                  dialogConfig.disableClose = true;
                  dialogConfig.autoFocus = true;
                  dialogConfig.data = {placeToAdd: place, previousPlace: this.lastActivityDay}
                  let dialogTransport = this.dialog.open(AddTransportDialogComponent, dialogConfig);
                  dialogTransport.afterClosed().subscribe(
                    transport => {
                      if (transport != undefined) {
                        let activityTransport: IActivityCreateIndividual = transport as IActivityCreateIndividual;
                        let dialogConfig = new MatDialogConfig<IActivityCreateDialogData>();
                        dialogConfig.disableClose = true;
                        dialogConfig.autoFocus = true;
                        dialogConfig.data = {placeToAdd: place, minimumBeginningDate: activityTransport.endingDate}
                        let dialogActivity = this.dialog.open(AddActivityDialogComponent, dialogConfig);
                        dialogActivity.afterClosed().subscribe(
                          activity => {
                            if (activity != undefined) {
                              let activityCreate: IActivityCreateIndividual = activity as IActivityCreateIndividual;
                              let activityPost: IActivityCreate = {
                                activityCreate: activityCreate,
                                activityTransport: activityTransport,
                                tripId: this.tripId
                              }
                              this.createActivity(activityPost);
                            }
                          }
                        ).add(() => {
                          this.submittingData = false;
                        })

                      } else {
                        this.submittingData = false;
                      }
                    }
                  );
                }
                //todo possivelmente migrar isto após a entrega para material maps em vez desta biblioteca morta visto que caixa de pesquisa parece agnóstica
              });
            });


          });
        },
        error => {
          switch (error.status) {
            case 404:
              this.toastService.showError("Não existe uma viagem com este ID.");
              break;
            case 403:
              this.toastService.showError("Esta viagem é privada e precisa de um convite para poder aceder à mesma.");
              break;
            default:
              this.toastService.showError("Erro ao obter viagem");
          }
          this.router.navigate(["/trips"]);
        })
    });
  }

  leaveTrip(): void {
    this.submittingData = true;
    this.tripService.leaveTrip(this.tripId).subscribe(
      success => {
        this.isInTrip = false;
        this.toastService.showSucess("Saiu da Viagem");
      },
      error => {
        this.toastService.showError("Erro ao sair da viagem");
      }
    ).add(() => {
      this.submittingData = false;
    })
  }

  joinTrip(): void {
    this.submittingData = true;
    this.tripService.joinTrip(this.tripId, null).subscribe(
      success => {
        this.isInTrip = true;
        this.toastService.showSucess("Entrou na viagem");
        location.reload();
      },
      error => {
        this.toastService.showError("Erro ao entrar na viagem");
      }
    ).add(() => {
      this.submittingData = false;
    })
  }


  removeUser(userId: string): void {
    this.submittingData = true;
    this.tripService.removeUserFromTrip(this.tripId, userId).subscribe(
      next => {
        this.toastService.showSucess("Utilizador removido da viagem com sucesso");
        this.trip.users = this.trip.users.filter(u => u.user.id != userId);
      },
      error => {
        this.toastService.showError("Erro ao remover utilizador da viagem");
      }).add(() => {
      this.submittingData = false;
    })
  }

  inviteUser(userId: string): void {
    this.submittingData = true;
    this.tripService.inviteUser(this.tripId, userId).subscribe(
      next => {
        this.toastService.showSucess("Utilizador convidado com sucesso");
        this.usersSearch = of([]);
        this.tripService.getTrip(this.tripId).subscribe(next => {
          this.trip = next as ITrip;
        });
      },
      error => {
        this.toastService.showError("Erro ao convidar o utilizador para a viagem");
      }
    ).add(() => {
      this.submittingData = false;
    });
  }

  removeInvitation(inviteId: string): void {
    this.submittingData = true;
    this.tripService.deleteInvitation(inviteId).subscribe(
      success => {
        this.toastService.showSucess("Convite removido com sucesso");
        this.trip.invites = this.trip.invites.filter(i => i.id != inviteId);
      },
      error => {
        switch (error.status) {
          case 404:
            this.toastService.showError("O convite que está a tentar remover não existe.");
            break;
          case 403:
            this.toastService.showError("Não tem permissões para gerir os convites desta viagem.");
            break;
          default:
            this.toastService.showError("Erro ao remover convite");
        }
      }
    ).add(() => {
      this.submittingData = false;
    });
  }

  searchUsersToInvite(): void {
    this.usersSearch = this.userService.searchUsers(this.nameOrEmailSearch.value).pipe(map(users => {
      return users.filter(u =>
        this.trip.group.users.some(uf => uf.user.id == u.id) &&
        !this.trip.invites.some(ui => ui.user.id == u.id) &&
        !this.trip.users.some(ut => ut.user.id == u.id)
      );
    }));
  }

  getLocalizedEventMessage(eventType: EventType): string {
    return getEventMessage(eventType);
  }

  getLocalizedActivityType(activityType: ActivityType): string {
    return getActivityName(activityType);
  }

  getLocalizedTransportType(transportType: TransportType): string {
    return getTransportTypeName(transportType);
  }

  updateTripDetails(): void {
    if (this.tripUpdateForm.invalid) {
      this.toastService.showError("Corriga os erros no formulário de actualização de viagem");
      return;
    }
    this.submittingData = true;
    this.tripService.updateTripDetails(this.tripId, this.tripUpdateForm.value).subscribe(success => {
        this.toastService.showSucess("Detalhes da viagem actualizados com sucesso!");
        this.tripService.getTrip(this.tripId).subscribe(next => {
          this.trip = next as ITrip;
        });
      },
      error => {
        switch (error.status) {
          case 404:
            this.toastService.showError("Não existe uma viagem com este ID.");
            break;
          case 403:
            this.toastService.showError("Não tem permissões para gerir os detalhes desta viagem.");
            break;
          default:
            this.toastService.showError("Erro ao editar esta viagem");
        }
      }
    ).add(() => {
      this.submittingData = false;
    });
  }

  updateTripImage(): void {
    if (this.tripImageForm.invalid) {
      this.toastService.showError("Insira a imagem da viagem antes submeter o formulário de imagem.");
      return;
    }
    this.submittingData = true;
    this.tripService.updateTripPicture(this.tripId, this.tripImageToUpload).subscribe(
      data => {
        this.toastService.showSucess("Imagem do grupo actualizada com sucesso");
        this.tripService.getTrip(this.tripId).subscribe(
          next => {
            this.trip = next as ITrip;
          }
        );
      },
      error => {
        let {errorType, message} = error.error;
        switch (errorType) {
          case ErrorType.MEDIA_ERROR:
            this.toastService.showError("A imagem introduzida não é válida");
            break;
          default:
            this.toastService.showError(message);
        }
      }
    ).add(
      () => {
        this.submittingData = false;
      }
    );
  }

  removeTripImage(): void {
    this.submittingData = true;
    this.tripService.deleteTripPicture(this.tripId).subscribe(
      success => {
        this.toastService.showSucess("Imagem da viagem removida com sucesso");
        this.tripService.getTrip(this.tripId).subscribe(
          next => {
            this.trip = next as ITrip;
          }
        );
      },
      error => {
        this.toastService.showError("Erro ao remover a imagem da viagem");
      }
    ).add(() => {
      this.submittingData = false;
    });
  }

  createActivity(activityPost: IActivityCreate) {
    this.activityService.createActivity(activityPost).subscribe(
      //retorna uma trip. refrescar última actividade
      next => {
        this.trip = next as ITrip;
        this.updateItinerary();
        this.toastService.showSucess("Actividade adicionada com sucesso ao itinerário");
      },
      error => {
        if ("error" in error && error.error != null) {
          let {errorType, message} = error.error;
          switch (errorType) {
            case ErrorType.TRIP_COMPLETED:
              this.toastService.showError("Não pode adicionar actividades a uma viagem terminada.");
              break;
            case ErrorType.ACTIVITY_TYPE_NOT_DEFINED:
              this.toastService.showError("O tipo de actividade ou transporte não está definido.");
              break;
            case ErrorType.ACTIVITY_NOT_SAME_DAY:
              this.toastService.showError("A data de início ou fim da viagem ou transporte não estão no mesmo dia");
              break;
            case ErrorType.ACTIVITY_OVERLAP:
              this.toastService.showError("Há uma sobreposição de datas com o transporte e actividade a adicionar ou o transporte e actividade anterior.");
              break;
            case ErrorType.ACTIVITY_MISSING_TRANSPORT:
              this.toastService.showError("Existem actividades antes desta actividade e não foi providenciado um transporte");
              break;
            case ErrorType.ACTIVITY_REPEATED:
              this.toastService.showError("Não pode adicionar a mesma actividade em sucessão");
              break;
            default:
              this.toastService.showError(message + " Código: " + errorType);
              break;
          }
        } else {
          switch (error.status) {
            case 404:
              this.toastService.showError("Não existe uma viagem com este ID.");
              break;
            case 403:
              this.toastService.showError("Não tem permissões para criar uma actividade nesta viagem, ou não se encontra nela.");
              break;
            default:
              this.toastService.showError("Erro ao criar actividade");
          }
        }
      }
    );
  }

  onTripImageChange(event: any) {
    this.tripImageToUpload = event.target.files.item(0);
  }




  updateItinerary() {
    this.trip.activities.sort(function (a, b) {
      return new Date(a.beginningDate).getTime() - new Date(b.beginningDate).getTime();
    });
    let selectedDate: Date = this.selectedDate.value;
    let beginning = new Date(selectedDate);
    let ending = new Date(beginning);
    ending.setDate(beginning.getDate() + 1);
    this.activitiesDay = this.trip.activities.filter(
      a => new Date(a.beginningDate) >= beginning &&
        new Date(a.beginningDate) < ending
    );
    this.activitiesDataSource.data = this.activitiesDay;
    if (this.activitiesDay.length > 0) {
      this.fitBounds = true;
      this.lastActivityDay = this.activitiesDay[this.activitiesDay.length - 1];
      this.latitude = this.lastActivityDay.latitude;
      this.longitude = this.lastActivityDay.longitude;
    } else {
      this.fitBounds = false;
      this.lastActivityDay = null;
    }
  }

  editActivity(activity: IActivity) {
    this.submittingData = true;
    let dialogConfig = new MatDialogConfig<IActivityEditDialogData>();
    dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;
    let index = this.activitiesDay.findIndex(a => a.id == activity.id);
    let minimumDate: Date | null = null;
    let maximumDate: Date | null = null;
    if (index == 0) {
      minimumDate = null;
    }
    if (index == this.activitiesDay.length) {
      maximumDate = null;
    }
    if (index - 1 > -1) {
      minimumDate = this.activitiesDay[index - 1].endingDate;
    }
    if (index + 1 < this.activitiesDay.length) {
      maximumDate = this.activitiesDay[index + 1].beginningDate;
    }
    dialogConfig.data = {minimumDate: minimumDate, maximumDate: maximumDate, activity: activity};
    let dialogActivity = this.dialog.open(EditActivityDialogComponent, dialogConfig);
    dialogActivity.afterClosed().subscribe(
      edit => {
        if (edit != undefined) {
          let activityPost = edit as IActivityUpdate;
          this.activityService.updateActivity(activity.id, activityPost).subscribe(
            next => {
              this.trip = next as ITrip;
              this.updateItinerary();
              this.toastService.showSucess("Actividade actualizada com sucesso");
            },
            error => {
              if ("error" in error && error.error != null) {
                let {errorType, message} = error.error;
                switch (errorType) {
                  case ErrorType.TRIP_COMPLETED:
                    this.toastService.showError("Não pode actualizar actividades de uma viagem terminada.");
                    break;
                  case ErrorType.ACTIVITY_CHANGE_TYPE:
                    this.toastService.showError("Não pode mudar o tipo de uma actividade de transporte para outra, ou vice-versa.");
                    break;
                  case ErrorType.ACTIVITY_NOT_SAME_DAY:
                    this.toastService.showError("A data de início ou fim da viagem ou transporte não estão no mesmo dia");
                    break;
                  case ErrorType.ACTIVITY_OVERLAP:
                    this.toastService.showError("Há uma sobreposição de datas com o transporte e actividade a adicionar ou o transporte e actividade anterior.");
                    break;
                  default:
                    this.toastService.showError(message + " Código: " + errorType);
                    break;
                }
              } else {
                switch (error.status) {
                  case 404:
                    this.toastService.showError("Não existe uma viagem com este ID.");
                    break;
                  case 403:
                    this.toastService.showError("Não tem permissões para actualizar uma actividade nesta viagem, ou não se encontra nela.");
                    break;
                  default:
                    this.toastService.showError("Erro ao actualizar actividade");
                }
              }
            }
          ).add(() => {
            this.submittingData = false;
          })
        } else {
          this.submittingData = false;
        }
      }
    )
  }

  deleteActivity(activity: IActivity) {
    if (confirm("Tem a certeza que pretende remover esta actividade?")) {
      this.submittingData = true;
      this.activityService.deleteActivity(activity.id).subscribe(next => {
          this.trip = next as ITrip;
          this.updateItinerary();
          this.toastService.showSucess("Actividade removida com sucesso");
        },
        error => {
          if ("error" in error && error.error != null) {
            let {errorType, message} = error.error;
            switch (errorType) {
              case ErrorType.TRIP_COMPLETED:
                this.toastService.showError("Não pode remover actividades de uma viagem terminada.");
                break;
              case ErrorType.ACTIVITY_REMOVE_TRANSPORT:
                this.toastService.showError("Não pode remover actividades do tipo transporte.");
                break;
              case ErrorType.ACTIVITY_REMOVE_NOT_LAST:
                this.toastService.showError("Apenas pode remover a última actividade do itinerário diário de uma viagem.");
                break;
              default:
                this.toastService.showError(message + " Código: " + errorType);
                break;
            }
          } else {
            switch (error.status) {
              case 404:
                this.toastService.showError("Não existe uma viagem com este ID.");
                break;
              case 403:
                this.toastService.showError("Não tem permissões para gerir o itinerário desta viagem.");
                break;
              default:
                this.toastService.showError("Erro ao remover actividade");
            }
          }
        }
      ).add(() => {
        this.submittingData = false;
      })
    }
  }

  editPost(post: IPost): void {
    this.submittingData = true;
    let dialogConfig = new MatDialogConfig<IPostEditDialogData>();
    dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;
    dialogConfig.data = {trip:this.trip,post:post};
    let dialogActivity = this.dialog.open(EditPostDialogComponent, dialogConfig);
    dialogActivity.afterClosed().subscribe(
      postUpdate => {
        if (postUpdate!=null) {
          let postPost: IPostUpdate = postUpdate as IPostUpdate;
          this.postService.updatePost(post.id,postPost).subscribe(
            next => {
              let updatedPost = next as IPost;
              let index = this.trip.posts.findIndex(p=>p.id == post.id);
              this.trip.posts[index] = updatedPost;
              this.postsDataSource.data = this.trip.posts;
              this.toastService.showSucess("Publicação actualizada com sucesso");
            },
            error => {
              this.toastService.showError("Erro ao actualizar publicação");
            }
          ).add(() => {
            this.submittingData = false;
          })
        }
        else {
          this.submittingData = false;
        }
      }
    )
  }

  createPost(): void {
    this.submittingData = true;
    let dialogConfig = new MatDialogConfig<ITrip>();
    dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;
    dialogConfig.data = this.trip;
    let dialogActivity = this.dialog.open(CreatePostDialogComponent, dialogConfig);
    dialogActivity.afterClosed().subscribe(
      post => {
        if (post != undefined) {
          let postPost: IPostCreate = post as IPostCreate;
          this.postService.createPost(postPost).subscribe(
            next => {
              this.trip.posts.push(next as IPost);
              this.postsDataSource.data = this.trip.posts;
              this.toastService.showSucess("Publicação adicionada com sucesso");
            },
            error => {
              this.toastService.showError("Erro ao adicionar publicação");
            }
          ).add(() => {
            this.submittingData = false;
          })
        } else {
          this.submittingData = false;
        }
      }
    )

  }

  deletePost(post: IPost): void {
    this.submittingData=true;
    if(confirm("Tem a certeza que pretende remover esta publicação?")){
      this.postService.deletePost(post.id).subscribe(
        next=>{
          this.trip.posts = this.trip.posts.filter(p=>p.id!=post.id);
          this.postsDataSource.data = this.trip.posts;
          this.toastService.showSucess("Publicação actualizada com sucesso");
        },
        error=>{
          this.toastService.showError("Erro ao eliminar publicação");
        }
      ).add(()=>{
        this.submittingData=false;
      })
    }
  }

  /*

  tablela dropdown para ver imagens
   */

}
