import {Component, OnInit} from '@angular/core';
import {Observable, Subscription} from "rxjs";
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {AuthenticationService} from "../../../services/authentication.service";
import {ActivatedRoute, Router} from "@angular/router";
import {ToastService} from "../../../services/toast.service";
import {IUser} from "../../../shared/interfaces/entities/IUser";
import {UserService} from "../../../services/user.service";
import {countries} from 'src/app/shared/components/country-data-store';
import {Countries} from "../../../shared/models/country.model";
import {environment} from "../../../../environments/environment";
import {Title} from "@angular/platform-browser";
import {ErrorType} from "../../../shared/enums/ErrorType";

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.css']
})
export class UserComponent implements OnInit {
  public countries: any = countries;
  public default_user_picture: string = environment.default_user_picture;
  private routeSub: Subscription = new Subscription;
  //Para actualizar o formulário
  public isLoading: boolean = false;
  private userId: string = "";
  //Quando está a carregar os detalhes do utilizador em questão
  public loadingData: boolean = true;
  public form: FormGroup;
  public imageForm: FormGroup;
  public user!: IUser;
  public isAdmin: boolean;
  public isFollowing: boolean = false;
  public fileToUpload!: File;
  public isMyself : boolean = false;
  public default_group_picture: string = environment.default_group_picture;
  public default_trip_picture: string = environment.default_trip_picture;

  constructor(
    private formBuilder: FormBuilder,
    private authenticationService: AuthenticationService,
    private userService: UserService,
    private router: Router,
    private toastService: ToastService,
    private route: ActivatedRoute,
    private titleService: Title
  ) {
    this.isAdmin = this.authenticationService.isAdmin();
    this.form = this.formBuilder.group({
      name: ["", Validators.required],
      email: ["", [Validators.required, Validators.email]],
      country: ["", [Validators.required, Validators.pattern("^[A-Z]{2}$")]],
      city: [""],
      description: [""],
      facebook: [""],
      twitter: [""],
      instagram: [""],
      phoneNumber: ["", Validators.pattern("\\+(9[976]\\d|8[987530]\\d|6[987]\\d|5[90]\\d|42\\d|3[875]\\d|\n" +
        "2[98654321]\\d|9[8543210]|8[6421]|6[6543210]|5[87654321]|\n" +
        "4[987654310]|3[9643210]|2[70]|7|1)\\d{1,14}$")],
      isActive: ["", Validators.required],
      locale: ['', Validators.required]
    });
    this.imageForm = this.formBuilder.group({
      image: ["", Validators.required]
    });
  }

  get image() {
    return this.imageForm.get("image")!;
  }

  get name() {
    return this.form.get("name")!;
  }

  get email() {
    return this.form.get("email")!;
  }

  get country() {
    return this.form.get("country")!;
  }

  get city() {
    return this.form.get("city")!;
  }

  get description() {
    return this.form.get("description")!;
  }

  get facebook() {
    return this.form.get("facebook")!;
  }

  get twitter() {
    return this.form.get("twitter")!;
  }

  get instagram() {
    return this.form.get("instagram")!;
  }

  get phoneNumber() {
    return this.form.get("phoneNumber")!;
  }

  get isActive() {
    return this.form.get("isActive")!;
  }

  get locale() {
    return this.form.get("locale")!;
  }

  ngOnInit(): void {
    this.routeSub = this.route.params.subscribe(params => {
      let userId: string = params["id"];
      this.userId = userId;
      this.isMyself = this.authenticationService.getUserId() == userId;
      this.userService.getUser(userId).subscribe(next => {
          this.user = next as IUser;
          this.titleService.setTitle(this.user.userName);
          this.loadingData = false;
          this.form.patchValue({
            name: this.user.name,
            email: this.user.email,
            country: this.user.country,
            city: this.user.city,
            description: this.user.description,
            facebook: this.user.facebook,
            twitter: this.user.twitter,
            instagram: this.user.instagram,
            phoneNumber: this.user.phoneNumber,
            isActive: this.user.isActive,
            locale: this.user.locale
          });
          let ownId = this.authenticationService.getUserId();
          this.isFollowing = this.user.followers.some(u => u.id == ownId);
        },
        error => {
          this.toastService.showError("Não existe um utilizador com este ID");
        });
    })
  }

  getCountryFromId(countryId?: string): Countries {
    return this.countries.find((c: Countries) => c.code == countryId);
  }

  update(): void {
    if (!this.form.valid) {
      this.toastService.showError("Preencha ou corriga os erros no formulário de Actualização de Informação");
      return;
    }
    this.isLoading = true;
    this.userService.updateUser(this.userId, this.form.value).subscribe(data => {
        this.toastService.showSucess("Detalhes actualizados com sucesso");
        this.userService.getUser(this.userId).subscribe(next => {
          this.user = next as IUser;
        });
      },
      error => {
        let {errorType, message} = error.error;
        this.toastService.showError(message);
      }).add(() => {
      this.isLoading = false;
    });
  }

  delete(): void {
    if (confirm("Tem a certeza que pretende remover este utilizador?")) {
      this.isLoading = true;
      this.userService.deleteUser(this.userId).subscribe(data => {
          this.toastService.showSucess("Utilizador removido com sucesso");
          this.router.navigate(["/users"]);
        },
        error => {
          this.toastService.showError("Erro ao remover utilizador");
        }).add(() => {
        this.isLoading = false;
      });
    }
  }

  follow(): void {
    this.isLoading = true;
    this.userService.followUser(this.userId).subscribe(data => {
        this.toastService.showSucess("Utilizador seguido");
        this.isFollowing = true;
        this.userService.getUser(this.userId).subscribe(next => {
          this.user = next as IUser;
        });
      },
      error => {
        let {errorType, message} = error.error;
        switch (errorType) {
          case ErrorType.USER_ALREADY_FOLLOWING:
            this.toastService.showError("Já segue este utilizador!");
            break;
          case ErrorType.USER_NOT_FOLLOW_SELF:
            this.toastService.showError("O utilizador não se pode seguir a si próprio");
            break;
          default:
            this.toastService.showError(message);
        }
      }).add(() => {
        this.isLoading = false;
      }
    );
  }

  unfollow(): void {
    this.isLoading = true;
    this.userService.unfollowUser(this.userId).subscribe(data => {
        this.toastService.showSucess("Utilizador deixado de seguir");
        this.isFollowing = false;
        this.user.followers = this.user.followers.filter(uf => uf.id != this.authenticationService.getUserId());
      },
      error => {
        let {errorType, message} = error.error;
        switch (errorType) {
          case ErrorType.USER_NOT_FOLLOWING:
            this.toastService.showError("Não segue este utilizador!");
            break;
          default:
            this.toastService.showError(message);
        }
      }).add(() => {
        this.isLoading = false;
      }
    );
  }

  changePicture(): void {
    if (!this.imageForm.valid) {
      this.toastService.showError("Insira a imagem antes de submeter o formulário de imagem.");
      return;
    }
    this.isLoading = true;
    this.userService.updateUserPicture(this.fileToUpload, this.userId).subscribe(
      data => {
        this.toastService.showSucess("Imagem actualizada com sucesso");
        this.userService.getUser(this.userId).subscribe(next => {
          this.user = next as IUser;
        });
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
    ).add(() => {
      this.isLoading = false;
    });
  }

  onFileChange(event: any) {
    this.fileToUpload = event.target.files.item(0);
  }

  deletePicture(): void {
    if (!this.user.photo) {
      this.toastService.showError("Este utilizador não tem uma imagem.");
      return;
    }
    this.isLoading = true;
    this.userService.deleteUserPicture(this.userId).subscribe(
      success => {
        this.toastService.showSucess("Imagem eliminada com sucesso.");
        this.userService.getUser(this.userId).subscribe(next => {
          this.user = next as IUser;
        });
      },
      failure => {
        this.toastService.showError("Erro ao eliminar imagem de perfil");
      }
    ).add(() => {
      this.isLoading = false;
    });
  }

}
