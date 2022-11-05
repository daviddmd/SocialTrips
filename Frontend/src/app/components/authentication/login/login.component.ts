import {Component, OnInit} from '@angular/core';
import {Router, ActivatedRoute, ParamMap} from '@angular/router';
import {AuthenticationService} from "../../../services/authentication.service";
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {ToastService} from "../../../services/toast.service";
import {Title} from "@angular/platform-browser";
import {ErrorType} from "../../../shared/enums/ErrorType";

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  public loading: boolean = false;
  public loginForm: FormGroup;
  private returnUrl: string;

  constructor(
    private authenticationService: AuthenticationService,
    private router: Router,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private toastService: ToastService,
    private titleService: Title
  ) {
    this.loginForm = this.formBuilder.group({
      usernameOrEmail: ['', Validators.required],
      password: ['', Validators.required],
      rememberMe: [false]
    });
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    this.titleService.setTitle("Iniciar Sessão");
  }

  ngOnInit(): void {
    if(this.authenticationService.isLoggedIn()){
      this.router.navigate(["/"]);
    }
  }

  get usernameOrEmail() {
    return this.loginForm.get('usernameOrEmail')!;
  }

  get password() {
    return this.loginForm.get('password')!;
  }

  get rememberMe() {
    return this.loginForm.get('rememberMe')!;
  }

  onSubmit(): void {
    if (!this.loginForm.valid) {
      this.toastService.showError("Invalid Form");
      return;
    }
    this.loading = true;
    this.authenticationService.login(this.usernameOrEmail?.value, this.password?.value, this.rememberMe?.value).subscribe(
      data => {
        let {token} = data;
        this.authenticationService.storeToken(token);
        this.router.navigate([this.returnUrl]).then(()=>{
          window.location.reload()
        });
      },
      err => {
        let {errorType, message} = err.error;
        switch (errorType) {
          case ErrorType.AUTHENTICATION_INVALID_USER:
            this.toastService.showError("Não existe uma conta com este nome de utilizador ou e-mail");
            break;
          case ErrorType.AUTHENTICATION_WRONG_PASSWORD:
            this.toastService.showError("Password incorrecta");
            break;
          case ErrorType.AUTHENTICATION_ACCOUNT_NOT_CONFIRMED:
            this.toastService.showError("Conta não confirmada, por favor siga as instruções enviadas por e-mail para a confirmar");
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
