import {Component, OnInit} from '@angular/core';
import {UserService} from "../../../services/user.service";
import {AuthenticationService} from "../../../services/authentication.service";
import {Observable} from "rxjs";
import {IUser} from "../../../shared/interfaces/entities/IUser";
import {Title} from "@angular/platform-browser";
import {IGroup} from 'src/app/shared/interfaces/entities/IGroup';
import {GroupService} from 'src/app/services/group.service';
import {environment} from "../../../../environments/environment";
import {ThemePalette} from '@angular/material/core';
import {ToastService} from "../../../services/toast.service";
import {ActivatedRoute, Router} from "@angular/router";
import {group} from '@angular/animations';

@Component({
  selector: 'app-groups',
  templateUrl: './groups.component.html',
  styleUrls: ['./groups.component.css']
})

export class GroupsComponent implements OnInit {
  public allGroups!: Observable<IGroup[]>;
  public user!: IUser;
  public isLoading: boolean = true;
  public default_group_picture: string = environment.default_group_picture;

  constructor(
    private groupService: GroupService,
    private authenticationService: AuthenticationService,
    private userService: UserService,
    private toastService: ToastService,
    private router: Router,
    private titleService: Title) {
    this.titleService.setTitle("Grupos");
  }

  ngOnInit(): void {
    this.userService.getSelf().subscribe(next => {
        this.user = next as IUser;
      },
      error => {
        this.toastService.showError("Erro ao carregar utilizador");
      }).add(() => {
      this.isLoading = false;
    });
    this.allGroups = this.groupService.getAllGroups();

  }

  acceptInvitation(groupId: string, groupInviteId: string) {
    this.groupService.joinGroup(groupId, groupInviteId).subscribe(data => {
      this.toastService.showSucess("Juntou-se a este grupo com sucesso!");
      this.router.navigate(['/group', groupId]);
    }, error => {
      this.toastService.showError("Erro ao juntar-se ao grupo.");
    });
  }

  declineInvitation(groupInvitateId: string) {
    this.groupService.deleteInvitation(groupInvitateId).subscribe(data => {
        this.userService.getSelf().subscribe(next => {
          this.user = next as IUser;
        });
      },
      error => {
        this.toastService.showError("Erro ao rejeitar o convite");
      });
  }
}
