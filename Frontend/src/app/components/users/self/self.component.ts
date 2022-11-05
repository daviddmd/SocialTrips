import {Component, OnInit} from '@angular/core';
import {UserService} from "../../../services/user.service";
import {IUser} from "../../../shared/interfaces/entities/IUser";
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {Router} from "@angular/router";
import {ToastService} from "../../../services/toast.service";
import {countries} from 'src/app/shared/components/country-data-store';
import {environment} from "../../../../environments/environment";
import {AuthenticationService} from "../../../services/authentication.service";
import {Observable} from "rxjs";
import {Title} from "@angular/platform-browser";
import {ErrorType} from "../../../shared/enums/ErrorType";
import {getMatIconFailedToSanitizeUrlError} from "@angular/material/icon";

@Component({
  selector: 'app-self',
  templateUrl: './self.component.html',
  styleUrls: ['./self.component.css']
})
export class SelfComponent implements OnInit {
  public countries: any = countries;
  public default_user_picture: string = environment.default_user_picture;
  public form: FormGroup;
  public imageForm: FormGroup;
  public fileToUpload!: File;
  //Para actualizar o formulário
  public loading: boolean = false;
  //Quando está a carregar os detalhes do utilizador em questão
  public loadingData: boolean = true;
  public user!: IUser;
  public default_group_picture: string = environment.default_group_picture;
  public default_trip_picture: string = environment.default_trip_picture;

  constructor(
    private userService: UserService,
    private formBuilder: FormBuilder,
    private router: Router,
    private toastService: ToastService,
    private authenticationService: AuthenticationService,
    private titleService: Title
  ) {
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
      isActive: [true, Validators.required],
      locale: ['', Validators.required]
    });
    this.imageForm = this.formBuilder.group({
      image: ["", Validators.required]
    });
    this.titleService.setTitle("Perfil");
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
    this.userService.getSelf().subscribe(
      data => {
        this.user = data as IUser;
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
          locale: this.user.locale
        });
      },
      error => {
        this.toastService.showError("Erro ao carregar informações do utilizador");
      },
    );
  }

  update(): void {
    if (!this.form.valid) {
      this.toastService.showError("Preencha ou corriga os erros no formulário de Actualização de Informação");
      return;
    }
    this.loading = true;
    this.userService.updateSelf(this.form.value).subscribe(data => {
        this.toastService.showSucess("Detalhes actualizados com sucesso");
        if (!this.form.get("isActive")?.value) {
          this.authenticationService.logout();
          this.router.navigate(["/"]).then(() => {
            window.location.reload();
          });
        }
      },
      error => {
        let {errorType, message} = error.error;
        this.toastService.showError(message);
      }).add(() => {
      this.loading = false;
    });
  }

  changePicture(): void {
    if (!this.imageForm.valid) {
      this.toastService.showError("Insira a sua imagem antes de submeter o formulário de imagem.");
      return;
    }
    this.loading = true;
    this.userService.updateOwnPicture(this.fileToUpload).subscribe(
      data => {
        this.toastService.showSucess("Imagem actualizada com sucesso");
        this.userService.getSelf().subscribe(next => {
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
      this.loading = false;
    });
  }

  deleteOwnPicture(): void {
    if (!this.user.photo) {
      this.toastService.showError("Não tem uma imagem, carregue uma primeiro!");
      return;
    }
    this.loading = true;
    this.userService.deleteOwnPicture().subscribe(
      success => {
        this.toastService.showSucess("Imagem eliminada com sucesso.");
        this.userService.getSelf().subscribe(next => {
          this.user = next as IUser;
        });
      },
      failure => {
        this.toastService.showError("Erro ao eliminar imagem de perfil");
      }
    ).add(() => {
      this.loading = false;
    });
  }

  onFileChange(event: any) {
    this.fileToUpload = event.target.files.item(0);
  }
}
