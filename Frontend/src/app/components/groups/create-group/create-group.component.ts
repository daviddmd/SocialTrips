import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from "../../../services/authentication.service";
import { Router } from "@angular/router";
import { Form, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ToastService } from "../../../services/toast.service";
import { Title } from "@angular/platform-browser";
import { GroupService } from 'src/app/services/group.service';

@Component({
  selector: 'app-create-group',
  templateUrl: './create-group.component.html',
  styleUrls: ['./create-group.component.css']
})
export class CreateGroupComponent implements OnInit {

  createGroupForm: FormGroup;
  loading: boolean = false;

  constructor(private authenticationService: AuthenticationService,
    private GroupService: GroupService,
    private router: Router,
    private formBuilder: FormBuilder,
    private toastService: ToastService,
    private titleService: Title) {
    this.createGroupForm = this.formBuilder.group({
      name:['', Validators.required],
      description:['', Validators.required],
      IsPrivate:[false, Validators.required]
    });
    this.titleService.setTitle("Criar Grupo");
  }

  get name() {
    return this.createGroupForm.get("name")!;
  }

  get description() {
    return this.createGroupForm.get("description")!;
  }

  get IsPrivate() {
    return this.createGroupForm.get("IsPrivate")!;
  }

  ngOnInit(): void {
  }

  onSubmit(): void {
    if (!this.createGroupForm.valid) {
      this.toastService.showError("Preencha ou corriga os erros no formulário de criação de grupo");
      return;
    }
    this.loading = true;
    this.GroupService.createGroup(this.createGroupForm.value).subscribe(
      data => {
        this.toastService.showSucess("Grupo criado com sucesso");
        this.router.navigate(["/groups"]);
      },
      error => {
        this.toastService.showError("Erro na criação do grupo.");
      }

    ).add(() => {
      this.loading = false;
    });
  }

}
