import { NgModule } from '@angular/core';
import {RouterModule, Routes} from "@angular/router";
import {RegisterComponent} from "./components/authentication/register/register.component";
import {LoginComponent} from "./components/authentication/login/login.component";
import {ForgotPasswordComponent} from "./components/authentication/forgot-password/forgot-password.component";
import {ResetPasswordComponent} from "./components/authentication/reset-password/reset-password.component";
import {
  ResendConfirmationEmailComponent
} from "./components/authentication/resend-confirmation-email/resend-confirmation-email.component";
import {ConfirmEmailComponent} from "./components/authentication/confirm-email/confirm-email.component";
import {GroupsComponent} from "./components/groups/groups/groups.component";
import {GroupComponent} from "./components/groups/group/group.component";
import {TripsComponent} from "./components/trips/trips/trips.component";
import {TripComponent} from "./components/trips/trip/trip.component";
import {HomeComponent} from "./components/home/home.component";
import {UsersComponent} from "./components/users/users/users.component";
import {UserComponent} from "./components/users/user/user.component";
import {SelfComponent} from "./components/users/self/self.component";
import {PostComponent} from "./components/post/post.component";
import {CreateGroupComponent} from "./components/groups/create-group/create-group.component";
import {AuthenticationGuard} from "./guards/authentication.guard";
import {StatisticsComponent} from "./components/statistics/statistics.component";
import {AdminGuard} from "./guards/admin.guard";
import {RankingsComponent} from "./components/rankings/rankings.component";

//tornar as routes constantes para no redireccionamento ser estável
const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent},
  { path: 'register', component: RegisterComponent },
  { path: 'statistics', component: StatisticsComponent, canActivate:[AuthenticationGuard,AdminGuard] },
  { path: 'rankings', component: RankingsComponent, canActivate:[AuthenticationGuard,AdminGuard] },
  { path: 'login', component: LoginComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password/:user_id/:base64_token', component: ResetPasswordComponent },
  { path: 'resend-confirmation-email', component: ResendConfirmationEmailComponent },
  { path: 'confirm-email/:user_id/:base64_token', component: ConfirmEmailComponent },
  { path: 'groups', component: GroupsComponent, canActivate: [AuthenticationGuard]  },
  { path: 'group/:id', component: GroupComponent, canActivate: [AuthenticationGuard]  },
  { path: 'groups/create', component: CreateGroupComponent, canActivate: [AuthenticationGuard]  },
  { path: 'trips', component: TripsComponent, canActivate: [AuthenticationGuard]  },
  { path: 'trip/:id', component: TripComponent, canActivate: [AuthenticationGuard]  },
  { path: 'users', component: UsersComponent, canActivate: [AuthenticationGuard]  },
  { path: 'user/:id', component: UserComponent, canActivate: [AuthenticationGuard]  },
  { path: 'me', component: SelfComponent, canActivate: [AuthenticationGuard]  },
  { path: 'post/:id', component: PostComponent, canActivate: [AuthenticationGuard]  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
