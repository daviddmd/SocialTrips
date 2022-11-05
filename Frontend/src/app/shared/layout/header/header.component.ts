import {Component, OnInit} from '@angular/core';
import {AuthenticationService} from "../../../services/authentication.service";

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {
  isLoggedIn: boolean = false;
  isAdmin: boolean = false;
  username: string = "";

  constructor(private authenticationService: AuthenticationService) {
  }

  ngOnInit(): void {
    this.isLoggedIn = this.authenticationService.isLoggedIn();
    if (this.isLoggedIn) {
      this.username = this.authenticationService.getUserName();
      this.isAdmin = this.authenticationService.isAdmin();
    }
  }

  logout(): void {
    this.authenticationService.logout();
    window.location.reload();
  }

}
