import {Component, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {AuthenticationService} from "../../../services/authentication.service";
import {Router} from "@angular/router";
import {ToastService} from "../../../services/toast.service";
import {Title} from "@angular/platform-browser";
import {ErrorType} from "../../../shared/enums/ErrorType";

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent implements OnInit {
  forgotPasswordForm: FormGroup;
  loading = false;

  constructor(
    private formBuilder: FormBuilder,
    private authenticationService: AuthenticationService,
    private router: Router,
    private toastService: ToastService,
    private titleService: Title
  ) {
    this.forgotPasswordForm = this.formBuilder.group({
      emailOrUsername: ['', Validators.required]
    });
    this.titleService.setTitle("Esqueci Palavra-passe");
  }

  ngOnInit(): void {
    if (this.authenticationService.isLoggedIn()) {
      this.router.navigate(["/"]);
    }
  }

  get emailOrUsername() {
    return this.forgotPasswordForm.get('emailOrUsername')!;
  }

  onSubmit(): void {
    if (!this.forgotPasswordForm.valid) {
      this.toastService.showError("Invalid Form");
      return;
    }
    this.loading = true;
    this.authenticationService.forgotPassword(this.emailOrUsername?.value).subscribe(
      data => {
        this.toastService.showSucess("Link para re-definir a password enviado por e-mail");
        this.router.navigate(["/"]);
      },
      error => {
        let {errorType, message} = error.error;
        switch (errorType) {
          case ErrorType.AUTHENTICATION_INVALID_USER:
            this.toastService.showError("Não existe uma conta com este nome de utilizador ou e-mail");
            break;
          case ErrorType.AUTHENTICATION_ACCOUNT_NOT_CONFIRMED:
            this.toastService.showError("Conta não confirmada, por favor siga as instruções enviadas por e-mail para a confirmar ou peça um novo e-mail de confirmação");
            break;
          default:
            this.toastService.showError(message);
        }
      }
    ).add(() => {
      this.loading = false;
    })
  }

}
