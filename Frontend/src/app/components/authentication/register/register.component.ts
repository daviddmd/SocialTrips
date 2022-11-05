import {Component, OnInit} from '@angular/core';
import {AuthenticationService} from "../../../services/authentication.service";
import {Router} from "@angular/router";
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {ToastService} from "../../../services/toast.service";
import {MustMatch} from "../../../helpers/form-validators";
import {countries} from 'src/app/shared/components/country-data-store';
import {Title} from "@angular/platform-browser";
import {ErrorType} from "../../../shared/enums/ErrorType";

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  public countries: any = countries
  registerForm: FormGroup;
  loading: boolean = false;

  constructor(
    private authenticationService: AuthenticationService,
    private router: Router,
    private formBuilder: FormBuilder,
    private toastService: ToastService,
    private titleService: Title
  ) {
    this.registerForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.pattern("^(?=.*[0-9])(?=.*[a-zA-Z])(?=\\S+$).{6,30}$")]],
      confirmPassword: ['', Validators.required],
      name: ['', Validators.required],
      username: ['', [Validators.required, Validators.pattern("^[a-zA-Z0-9_]*$")]],
      country: ['', [Validators.required, Validators.pattern("^[A-Z]{2}$")]],
      city: [''],
      phoneNumber: ['', Validators.pattern("\\+(9[976]\\d|8[987530]\\d|6[987]\\d|5[90]\\d|42\\d|3[875]\\d|\n" +
        "2[98654321]\\d|9[8543210]|8[6421]|6[6543210]|5[87654321]|\n" +
        "4[987654310]|3[9643210]|2[70]|7|1)\\d{1,14}$")],
      description: [''],
      locale: ['', Validators.required]
    }, {
      validator: MustMatch("password", "confirmPassword")
    });
    this.titleService.setTitle("Registar Conta");
  }

  get email() {
    return this.registerForm.get("email")!;
  }

  get password() {
    return this.registerForm.get("password")!;
  }

  get confirmPassword() {
    return this.registerForm.get("confirmPassword")!;
  }

  get name() {
    return this.registerForm.get("name")!;
  }

  get username() {
    return this.registerForm.get("username")!;
  }

  get country() {
    return this.registerForm.get("country")!;
  }

  get city() {
    return this.registerForm.get("city")!;
  }

  get phoneNumber() {
    return this.registerForm.get("phoneNumber")!;
  }

  get description() {
    return this.registerForm.get("description")!;
  }

  get locale() {
    return this.registerForm.get("locale")!;
  }

  ngOnInit(): void {
    if (this.authenticationService.isLoggedIn()) {
      this.router.navigate(["/"]);
    }
  }

  onSubmit(): void {
    if (!this.registerForm.valid) {
      this.toastService.showError("Preencha ou corriga os erros no formulário de Registo");
      return;
    }
    this.loading = true;
    this.authenticationService.register(this.registerForm.value).subscribe(
      data => {
        this.toastService.showSucess("Registo efectuado com sucesso, por favor siga as instruções enviadas por e-mail");
        this.router.navigate(["/"]);
      },
      error => {
        let {errorType, message} = error.error;
        switch (errorType) {
          case ErrorType.AUTHENTICATION_EMAIL_EXISTS:
            this.toastService.showError("Existe uma conta com este e-mail, por favor escolha outro");
            break;
          case ErrorType.AUTHENTICATION_USERNAME_EXISTS:
            this.toastService.showError("Existe uma conta com este nome de utilizador, por favor escolha outro");
            break;
          case ErrorType.AUTHENTICATION_INVALID_PASSWORD:
            this.toastService.showError("As passwords não correspondem");
            break;
          default:
            this.toastService.showError(message);
        }
      }
    ).add(() => {
      this.loading = false;
    });
  }


}
