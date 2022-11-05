import {Component, OnInit} from '@angular/core';
import {AuthenticationService} from "../../../services/authentication.service";
import {GroupService} from "../../../services/group.service";
import {ToastService} from "../../../services/toast.service";
import {FormBuilder, FormGroup, ValidationErrors, Validators} from "@angular/forms";
import {UserService} from "../../../services/user.service";
import {ActivatedRoute, Router} from "@angular/router";
import {Title} from "@angular/platform-browser";
import {map, Observable, of, Subscription} from "rxjs";
import {environment} from "../../../../environments/environment";
import {IGroup} from "../../../shared/interfaces/entities/IGroup";
import {ValidateTripDate} from "../../../helpers/form-validators";
import {UserGroupRole} from "../../../shared/enums/UserGroupRole";
import {ErrorType} from "../../../shared/enums/ErrorType";
import {ITrip} from "../../../shared/interfaces/entities/ITrip";
import {IUser} from "../../../shared/interfaces/entities/IUser";
import {MatDialog, MatDialogConfig} from "@angular/material/dialog";
import {BanDialogComponent} from "./ban-dialog/ban-dialog.component";
import {EventType} from "../../../shared/enums/EventType";
import {getActivityName, getEventMessage, getUserGroupRoleName} from "../../../helpers/event-message";
import {TripService} from "../../../services/trip.service";
import {ActivityType} from "../../../shared/enums/ActivityType";
import {IUserGroup} from "../../../shared/interfaces/entities/IUserGroup";

@Component({
  selector: 'app-group',
  templateUrl: './group.component.html',
  styleUrls: ['./group.component.css']
})
export class GroupComponent implements OnInit {
  public isAdmin: boolean = false;
  public isManager: boolean = false;
  public isModerator: boolean = false;
  public group!: IGroup;
  private routeSub: Subscription = new Subscription;
  public default_user_picture: string = environment.default_user_picture;
  public default_group_picture: string = environment.default_group_picture;
  public default_trip_picture: string = environment.default_trip_picture;
  public isLoadingGroup: boolean = true;
  public submittingData: boolean = false;
  public groupUpdateForm: FormGroup;
  public tripCreateForm: FormGroup;
  public groupImageForm: FormGroup;
  public userInviteSearchForm: FormGroup;
  public groupImageToUpload!: File;
  private groupId: string = "";
  public isInGroup: boolean = false;
  public ownId: string;
  usersSearch!: Observable<IUser[]>;
  public minimumTripBeginningDate: Date = new Date();
  public minimumTripEndingDate: Date = new Date();
  public groupRoles: Map<String, UserGroupRole> = new Map<String, number>([
    [getUserGroupRoleName(UserGroupRole.MANAGER), UserGroupRole.MANAGER],
    [getUserGroupRoleName(UserGroupRole.MODERATOR), UserGroupRole.MODERATOR],
    [getUserGroupRoleName(UserGroupRole.REGULAR), UserGroupRole.REGULAR]
  ]);

  constructor(
    private authenticationService: AuthenticationService,
    private groupService: GroupService,
    private tripService: TripService,
    private toastService: ToastService,
    private formBuilder: FormBuilder,
    private userService: UserService,
    private route: ActivatedRoute,
    private titleService: Title,
    private router: Router,
    private dialog: MatDialog,
  ) {
    this.ownId = this.authenticationService.getUserId();
    this.isAdmin = this.authenticationService.isAdmin();
    this.tripCreateForm = this.formBuilder.group({
      name: ["", Validators.required],
      description: ["", Validators.required],
      beginningDate: ["", Validators.required],
      endingDate: ["", Validators.required],
      isPrivate: [false],
      groupId: [""]
    }, {validator: ValidateTripDate("beginningDate", "endingDate")});
    this.groupUpdateForm = this.formBuilder.group({
      name: ["", Validators.required],
      description: ["", Validators.required],
      isPrivate: [""],
      isFeatured: [""],
    });
    this.groupImageForm = this.formBuilder.group({
      image: ["", Validators.required]
    });
    this.userInviteSearchForm = this.formBuilder.group({
      nameOrEmail: ["", Validators.required]
    });
    this.minimumTripBeginningDate.setDate(this.minimumTripBeginningDate.getDate() + 1);
    this.minimumTripEndingDate.setDate(this.minimumTripBeginningDate.getDate() + 1);

  }

  get nameOrEmailSearch() {
    return this.userInviteSearchForm.get("nameOrEmail")!;
  }

  get tripName() {
    return this.tripCreateForm.get("name")!;
  }

  get tripDescription() {
    return this.tripCreateForm.get("description")!;
  }

  get tripBeginningDate() {
    return this.tripCreateForm.get("beginningDate")!;
  }

  get tripEndingDate() {
    return this.tripCreateForm.get("endingDate")!;
  }

  get tripIsPrivate() {
    return this.tripCreateForm.get("isPrivate")!;
  }

  get groupName() {
    return this.groupUpdateForm.get("name")!;
  }

  get groupDescription() {
    return this.groupUpdateForm.get("description")!;
  }

  get groupIsPrivate() {
    return this.groupUpdateForm.get("isPrivate")!;
  }

  get groupIsFeatured() {
    return this.groupUpdateForm.get("isFeatured")!;
  }

  get groupImage() {
    return this.groupImageForm.get("image")!;
  }

  ngOnInit(): void {
    this.routeSub = this.route.params.subscribe(params => {
      let groupId: string = params["id"];
      this.groupId = groupId;
      this.groupService.getGroup(groupId).subscribe(next => {
          this.group = next as IGroup;
          this.titleService.setTitle(this.group.name);
          this.groupUpdateForm.patchValue({
            name: this.group.name,
            description: this.group.description,
            isPrivate: this.group.isPrivate,
            isFeatured: this.group.isFeatured
          });
          this.tripCreateForm.patchValue({
            groupId: this.groupId
          })
          let ownId = this.authenticationService.getUserId();
          for (let user of this.group.users) {
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
          this.isLoadingGroup = false;
        },
        error => {
          switch (error.status) {
            case 404:
              this.toastService.showError("Não existe um grupo com este ID.");
              break;
            case 403:
              this.toastService.showError("Este grupo é privado e precisa de um convite para poder aceder ao mesmo.");
              break;
            default:
              this.toastService.showError("Erro ao obter grupo");
          }
          this.router.navigate(["/groups"]);
        });
    })
  }

  createTrip(): void {
    if (this.tripCreateForm.invalid) {
      Object.keys(this.tripCreateForm.controls).forEach(key => {
        let controlErrors: ValidationErrors = this.tripCreateForm.get(key)!.errors!;
        if (controlErrors != null) {
          Object.keys(controlErrors).forEach(keyError => {
            console.log('Key control: ' + key + ', keyError: ' + keyError + ', err value: ', controlErrors[keyError]);
          });
        }
      });
      this.toastService.showError("O formulário de criação da viagem tem erros, por favor, corriga-os.");
      return;
    }
    this.submittingData = true;
    this.tripService.createTrip(this.tripCreateForm.value).subscribe(
      success => {
        let trip = success as ITrip;
        this.toastService.showSucess("Viagem criada com sucesso");
        this.router.navigate(['/trip', trip.id]);
      },
      error => {
        if ("error" in error && error.error != null) {
          let {errorType, message} = error.error;
          switch (errorType) {
            case ErrorType.TRIP_DATE_INVALID:
              this.toastService.showError("A data de início da viagem não pode ser superior à data de fim da viagem.");
              break;
            default:
              this.toastService.showError(message);
          }
        } else {
          switch (error.status) {
            case 404:
              this.toastService.showError("Não existe um grupo com este ID.");
              break;
            case 403:
              this.toastService.showError("Não tem permissões para criar uma viagem neste grupo, ou não se encontra nele.");
              break;
            default:
              this.toastService.showError("Erro ao criar viagem");
          }
        }
      }
    ).add(() => {
      this.submittingData = false;
    });
  }

  //devolver apenas resultados de utilizadores que NÃO estejam no grupo ou convidados para o mesmo
  searchUsersToInvite(): void {
    this.usersSearch = this.userService.searchUsers(this.nameOrEmailSearch.value).pipe(map(users => {
      return users.filter(u => !this.group.users.some(uf => uf.user.id == u.id) && !this.group.invites.some(ui => ui.user.id == u.id));
    }));
  }

  inviteUser(userId: string): void {
    this.submittingData = true;
    this.groupService.inviteUser(this.groupId, userId).subscribe(
      success => {
        this.toastService.showSucess("Utilizador convidado com sucesso");
        this.usersSearch = of([]);
        this.groupService.getGroup(this.groupId).subscribe(next => {
          this.group = next as IGroup;
        });
      },
      error => {
        if ("error" in error && error.error != null) {
          let {errorType, message} = error.error;
          switch (errorType) {
            case ErrorType.GROUP_USER_ALREADY_INVITED:
              this.toastService.showError("Este utilizador já foi convidado.");
              break;
            case ErrorType.GROUP_USER_ALREADY_PRESENT:
              this.toastService.showError("Este utilizador já se encontra neste grupo.");
              break;
            default:
              this.toastService.showError(message);
          }
        } else {
          switch (error.status) {
            case 404:
              this.toastService.showError("Não existe um utilizador ou grupo com estes IDs.");
              break;
            case 403:
              this.toastService.showError("Não tem permissões para gerir os utilizadores deste grupo.");
              break;
            default:
              this.toastService.showError("Erro ao convidar utilizador");
          }
        }
      }
    ).add(() => {
      this.submittingData = false;
    });
  }

  removeInvitation(inviteId: string): void {
    this.submittingData = true;
    this.groupService.deleteInvitation(inviteId).subscribe(
      success => {
        this.toastService.showSucess("Convite removido com sucesso");
        this.group.invites = this.group.invites.filter(i => i.id != inviteId);
      },
      error => {
        switch (error.status) {
          case 404:
            this.toastService.showError("O convite que está a tentar remover não existe.");
            break;
          case 403:
            this.toastService.showError("Não tem permissões para gerir os convites deste grupo.");
            break;
          default:
            this.toastService.showError("Erro ao remover convite");
        }
      }
    ).add(() => {
      this.submittingData = false;
    });
  }

  banUser(userId: string, userName: string): void {
    this.submittingData = true;
    let dialogConfig = new MatDialogConfig();
    dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;
    dialogConfig.data = {name: userName};
    let dialogRef = this.dialog.open(BanDialogComponent, dialogConfig);
    dialogRef.afterClosed().subscribe(
      val => {
        if (val != undefined) {
          let banUntil = val.banUntil == "" ? null : val.banUntil;
          let banReason = val.banReason;
          let hidePosts = val.hidePosts;
          this.groupService.banUser(this.groupId, userId, banUntil, banReason, hidePosts).subscribe(
            success => {
              this.toastService.showSucess("Utilizador proibido do grupo com sucesso");
              this.groupService.getGroup(this.groupId).subscribe(next => {
                this.group = next as IGroup;
              });
            },
            error => {
              if ("error" in error && error.error != null) {
                let {errorType, message} = error.error;
                switch (errorType) {
                  case ErrorType.GROUP_USER_ALREADY_BANNED:
                    this.toastService.showError("Este utilizador já foi proibido do grupo.");
                    break;
                  case ErrorType.GROUP_BAN_DATE_INVALID:
                    this.toastService.showError("A data de proibição do utilizador é inválida.");
                    break;
                  case ErrorType.GROUP_USER_NOT_PRESENT:
                    this.toastService.showError("O utilizador que está a tentar proibir não está presente no grupo.");
                    break;
                  case ErrorType.GROUP_LAST_MANAGER_LEAVE:
                    this.toastService.showError("Não pode proibir o último gestor do grupo.");
                    break;
                  default:
                    this.toastService.showError(message);
                }
              } else {
                switch (error.status) {
                  case 404:
                    this.toastService.showError("Não existe um utilizador ou grupo com estes IDs.");
                    break;
                  case 403:
                    this.toastService.showError("Não tem permissões para gerir os utilizadores deste grupo.");
                    break;
                  default:
                    this.toastService.showError("Erro ao proibir utilizador");
                }
              }
            }
          );
        }
      }
    ).add(() => {
      this.submittingData = false;
    });
  }

  removeUser(userId: string): void {
    this.submittingData = true;
    this.groupService.removeUser(this.groupId, userId).subscribe(
      success => {
        this.toastService.showSucess("Utilizador removido com sucesso.");
        this.group.users = this.group.users.filter(u => u.user.id != userId);
      },
      error => {
        if ("error" in error && error.error != null) {
          let {errorType, message} = error.error;
          switch (errorType) {
            case ErrorType.GROUP_LAST_MANAGER_LEAVE:
              this.toastService.showError("É o último gestor neste grupo, não pode sair.");
              break;
            case ErrorType.GROUP_USER_NOT_PRESENT:
              this.toastService.showError("Este utilizador nao se encontra neste grupo.");
              break;
            default:
              this.toastService.showError(message);
          }
        } else {
          switch (error.status) {
            case 404:
              this.toastService.showError("Não existe um grupo com este ID.");
              break;
            case 403:
              this.toastService.showError("Não tem permissões para gerir os utilizadores deste grupo.");
              break;
            default:
              this.toastService.showError("Erro ao remover utilizador");
          }
        }
      }
    ).add(() => {
      this.submittingData = false;
    })
  }

  unbanUser(banId: string): void {
    this.submittingData = true;
    this.groupService.unbanUser(banId).subscribe(
      success => {
        this.toastService.showSucess("Proibição removida com sucesso");
        this.group.bans = this.group.bans.filter(b => b.id != banId);
      },
      error => {
        if ("error" in error && error.error != null) {
          let {errorType, message} = error.error;
          switch (errorType) {
            case ErrorType.GROUP_USER_NOT_BANNED:
              this.toastService.showError("Este utilizador não se encontra proibido de entrar neste grupo.");
              break;
            default:
              this.toastService.showError(message);
          }
        } else {
          switch (error.status) {
            case 404:
              this.toastService.showError("Esta proibição não existe.");
              break;
            case 403:
              this.toastService.showError("Não tem permissões para gerir os utilizadores deste grupo.");
              break;
            default:
              this.toastService.showError("Erro ao remover proibição");
          }
        }
      }
    ).add(() => {
      this.submittingData = false;
    })
  }

  updateGroupDetails(): void {
    if (this.groupUpdateForm.invalid) {
      this.toastService.showError("Corriga os erros no formulário de actualização de detalhes de grupo");
      return;
    }
    this.submittingData = true;
    this.groupService.updateGroup(this.groupId, this.groupUpdateForm.value).subscribe(success => {
        this.toastService.showSucess("Detalhes do grupo actualizados com sucesso!");
        this.groupService.getGroup(this.groupId).subscribe(next => {
          this.group = next as IGroup;
        });
      },
      error => {
        switch (error.status) {
          case 404:
            this.toastService.showError("Não existe um grupo com este ID.");
            break;
          case 403:
            this.toastService.showError("Não tem permissões para gerir os detalhes deste grupo.");
            break;
          default:
            this.toastService.showError("Erro ao editar o grupo");
        }
      }
    ).add(() => {
      this.submittingData = false;
    });
  }

  join(): void {
    this.submittingData = true;
    this.groupService.joinGroup(this.groupId, null).subscribe(
      success => {
        this.toastService.showSucess("Aderiu ao grupo!");
        this.isInGroup = true;
        this.groupService.getGroup(this.groupId).subscribe(next => {
          this.group = next as IGroup;
        });
      },
      error => {
        let {errorType, message} = error.error;
        switch (errorType) {
          case ErrorType.GROUP_USER_JOIN_BANNED:
            this.toastService.showError("Está banido, logo não se pode juntar a este grupo.");
            break;
          case ErrorType.GROUP_INVITE_INVALID:
            this.toastService.showError("Este convite é inválido.");
            break;
          case ErrorType.GROUP_USER_ALREADY_PRESENT:
            this.toastService.showError("Já se encontra neste grupo.");
            break;
          default:
            this.toastService.showError(message);
        }
      }
    ).add(() => {
      this.submittingData = false;
    });
  }

  leave(): void {
    this.submittingData = true;
    this.groupService.leaveGroup(this.groupId).subscribe(
      success => {
        this.toastService.showSucess("Saiu do grupo.");
        this.isInGroup = false;
        this.groupService.getGroup(this.groupId).subscribe(next => {
          this.group = next as IGroup;
        });
      },
      error => {
        let {errorType, message} = error.error;
        switch (errorType) {
          case ErrorType.GROUP_LAST_MANAGER_LEAVE:
            this.toastService.showError("É o último gestor neste grupo, não pode sair.");
            break;
          case ErrorType.GROUP_USER_NOT_PRESENT:
            this.toastService.showError("Não se encontra neste grupo.");
            break;
          default:
            this.toastService.showError(message);
        }
      }
    ).add(() => {
      this.submittingData = false;

    });
  }

  onGroupImageChange(event: any) {
    this.groupImageToUpload = event.target.files.item(0);
  }

  updateGroupPicture(): void {
    if (this.groupImageForm.invalid) {
      this.toastService.showError("Insira a imagem do grupo antes submeter o formulário de imagem.");
      return;
    }
    this.submittingData = true;
    this.groupService.updateGroupPicture(this.groupId, this.groupImageToUpload).subscribe(
      data => {
        this.toastService.showSucess("Imagem do grupo actualizada com sucesso");
        this.groupService.getGroup(this.groupId).subscribe(
          next => {
            this.group = next as IGroup;
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

  getLocalizedEventMessage(eventType: EventType): string {
    return getEventMessage(eventType);
  }

  removeGroupPicture(): void {
    this.submittingData = true;
    this.groupService.deleteGroupPicture(this.groupId).subscribe(
      success => {
        this.toastService.showSucess("Imagem do grupo removida com sucesso");
        this.groupService.getGroup(this.groupId).subscribe(
          next => {
            this.group = next as IGroup;
          }
        );
      },
      error => {
        this.toastService.showError("Erro ao remover a imagem do grupo");
      }
    ).add(() => {
      this.submittingData = false;
    });
  }

  updateUserGroupRole(value: any, user: IUserGroup): void {
    this.submittingData=true;
    this.groupService.updateUserRole(this.groupId,user.user.id,value as UserGroupRole).subscribe(
      next=>{
        this.toastService.showSucess("Role do utilizador actualizado com sucesso");
      },
      error => {
        this.toastService.showError("Erro ao actualizar role do utilizador");

      }
    ).add(()=>{
      this.submittingData=false;
    });
  }
}
