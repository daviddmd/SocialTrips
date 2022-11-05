import {Component, OnInit} from '@angular/core';
import {FormBuilder} from "@angular/forms";
import {AuthenticationService} from "../../../services/authentication.service";
import {ActivatedRoute, Router} from "@angular/router";
import {ToastService} from "../../../services/toast.service";
import {Subscription} from "rxjs";
import {Title} from "@angular/platform-browser";
import {ErrorType} from "../../../shared/enums/ErrorType";

@Component({
  selector: 'app-confirm-email',
  templateUrl: './confirm-email.component.html',
  styleUrls: ['./confirm-email.component.css']
})
export class ConfirmEmailComponent implements OnInit {
  private routeSub: Subscription = new Subscription;
  public isLoading: boolean = true;
  public result: boolean = false;

  constructor(
    private formBuilder: FormBuilder,
    private authenticationService: AuthenticationService,
    private router: Router,
    private toastService: ToastService,
    private route: ActivatedRoute,
    private titleService: Title
  ) {
    this.titleService.setTitle("Confirmar E-Mail");
  }

  ngOnInit(): void {
    this.routeSub = this.route.params.subscribe(params => {
      let userId = params["user_id"];
      let confirmationToken = atob(params["base64_token"]);
      this.authenticationService.confirmEmail(userId, confirmationToken).subscribe(
        data => {
          this.result = true;
          this.toastService.showSucess("Conta confirmada com sucesso.");
        },
        error => {
          let {errorType, message} = error.error;
          switch (errorType) {
            case ErrorType.AUTHENTICATION_INVALID_USER:
              this.toastService.showError("Não existe uma conta com este nome de utilizador ou e-mail");
              break;
            case ErrorType.AUTHENTICATION_EMAIL_ALREADY_CONFIRMED:
              this.toastService.showError("Esta conta já se encontra confirmada");
              break;
            case ErrorType.AUTHENTICATION_WRONG_EMAIL_CONFIRMATION:
              this.toastService.showError("Link de confirmação inválido, por favor peça um novo");
              break;
            default:
              this.toastService.showError(message);
          }
        }).add(() => {
        this.isLoading = false;
        setTimeout(() => {
          this.router.navigate([this.result ? '/login' : '/resend-confirmation-email']);
        }, 5 * 1000);
      });
    })
  }

}
