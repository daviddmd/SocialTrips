import {Component, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {AuthenticationService} from "../../../services/authentication.service";
import {ActivatedRoute, Router} from "@angular/router";
import {ToastService} from "../../../services/toast.service";
import {Subscription} from "rxjs";
import {MustMatch} from "../../../helpers/form-validators";
import {Title} from "@angular/platform-browser";
import {ErrorType} from "../../../shared/enums/ErrorType";

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css']
})
export class ResetPasswordComponent implements OnInit {
  public resetPasswordForm: FormGroup;
  private routeSub: Subscription = new Subscription;
  public isLoading: boolean = false;
  private confirmationToken: string = "";
  private userId: string = "";

  constructor(
    private formBuilder: FormBuilder,
    private authenticationService: AuthenticationService,
    private router: Router,
    private toastService: ToastService,
    private route: ActivatedRoute,
    private titleService: Title

  ) {
    this.resetPasswordForm = this.formBuilder.group({
        newPassword: ['', [Validators.required, Validators.pattern("^(?=.*[0-9])(?=.*[a-zA-Z])(?=\\S+$).{6,30}$")]],
        newPasswordConfirm: ['', Validators.required]
      }, {
        validator: MustMatch("newPassword", "newPasswordConfirm")
      }
    );
    this.titleService.setTitle("Redifinir Password");
  }

  get newPassword() {
    return this.resetPasswordForm.get("newPassword")!;
  }

  get newPasswordConfirm() {
    return this.resetPasswordForm.get("newPasswordConfirm")!;
  }

  ngOnInit(): void {
    this.routeSub = this.route.params.subscribe(params => {
      this.userId = params["user_id"];
      this.confirmationToken = atob(params["base64_token"]);
    });
  }

  onSubmit(): void {
    if (!this.resetPasswordForm.valid) {
      this.toastService.showError("Invalid Form");
      return;
    }
    this.isLoading = true;
    this.authenticationService.resetPassword(this.userId, this.confirmationToken, this.newPassword?.value).subscribe(
      data => {
        this.toastService.showSucess("Password redifinida com sucesso, por favor inicie sessão");
        this.router.navigate(["/login"]);
      },
      error => {
        let {errorType, message} = error.error;
        switch (errorType) {
          case ErrorType.AUTHENTICATION_INVALID_USER:
            this.toastService.showError("Não existe uma conta com este nome de utilizador ou e-mail");
            break;
          case ErrorType.AUTHENTICATION_WRONG_PASSWORD_RESET:
            this.toastService.showError("Link de redifinição de password inválido, por favor peça um novo");
            break;
          default:
            this.toastService.showError(message);
        }
        this.router.navigate(["/reset-password"]);
      }
    ).add(() => {
      this.isLoading = false;
    });
  }

}
