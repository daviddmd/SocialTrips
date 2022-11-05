import {Component, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {AuthenticationService} from "../../../services/authentication.service";
import {Router} from "@angular/router";
import {ToastService} from "../../../services/toast.service";
import {Title} from "@angular/platform-browser";
import {ErrorType} from "../../../shared/enums/ErrorType";

@Component({
  selector: 'app-resend-confirmation-email',
  templateUrl: './resend-confirmation-email.component.html',
  styleUrls: ['./resend-confirmation-email.component.css']
})
export class ResendConfirmationEmailComponent implements OnInit {
  resendEmailConfirmationForm: FormGroup;
  loading: boolean = false;

  constructor(
    private formBuilder: FormBuilder,
    private authenticationService: AuthenticationService,
    private router: Router,
    private toastService: ToastService,
    private titleService: Title
  ) {
    if (this.authenticationService.isLoggedIn()) {
      this.router.navigate(["/"]);
    }
    this.resendEmailConfirmationForm = this.formBuilder.group({
      emailOrUsername: ['', Validators.required]
    });
    this.titleService.setTitle("Re-enviar e-mail confirmação")
  }

  ngOnInit(): void {
  }

  get emailOrUsername() {
    return this.resendEmailConfirmationForm.get('emailOrUsername')!;
  }

  onSubmit(): void {
    if (!this.resendEmailConfirmationForm.valid) {
      this.toastService.showError("Invalid Form");
      return;
    }
    this.loading = true;
    this.authenticationService.resendConfirmationEmail(this.emailOrUsername?.value).subscribe(
      data => {
        this.toastService.showSucess("Link para confirmar a conta enviado por e-mail");
        this.router.navigate(["/"]);
      },
      error => {
        let {errorType, message} = error.error;
        switch (errorType) {
          case ErrorType.AUTHENTICATION_INVALID_USER:
            this.toastService.showError("Não existe uma conta com este nome de utilizador ou e-mail");
            break;
          case ErrorType.AUTHENTICATION_EMAIL_ALREADY_CONFIRMED:
            this.toastService.showError("A conta já está confirmada");
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
