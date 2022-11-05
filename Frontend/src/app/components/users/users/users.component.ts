import { Component, OnInit } from '@angular/core';
import {UserService} from "../../../services/user.service";
import {AuthenticationService} from "../../../services/authentication.service";
import {ToastService} from "../../../services/toast.service";
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {Observable} from "rxjs";
import {IUser} from "../../../shared/interfaces/entities/IUser";
import {Title} from "@angular/platform-browser";

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})

export class UsersComponent implements OnInit {
  //para utilizadores normais apenas apresenta o formulário de pesquisa de utilizadores. para admins apresenta esse formulário e lista de utilizadores.
  //futuramente a colocar em 2 separadores
  public isAdmin: boolean;
  public userSearchForm: FormGroup;
  usersSearch!: Observable<IUser[]>;
  allUsers!: Observable<IUser[]>;
  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService,
    private authenticationService: AuthenticationService,
    private toastService: ToastService,
    private titleService: Title
  ) {
    this.isAdmin = authenticationService.isAdmin();
    this.userSearchForm = this.formBuilder.group({
      nameOrEmail: ["", Validators.required]
    });
    this.titleService.setTitle("Utilizadores");
  }
  get nameOrEmail(){
    return this.userSearchForm.get("nameOrEmail")!;
  }

  ngOnInit(): void {
    if (this.isAdmin){
      this.allUsers = this.userService.getAllUsers();
    }
  }
  searchUsers(): void{
    if (!this.userSearchForm.valid){
      this.toastService.showError("Necessita de introduzir o nome de utilizador ou e-mail para pesquisar por um utilizador");
      return;
    }
    this.usersSearch = this.userService.searchUsers(this.nameOrEmail.value);
  }

}
