import {LOCALE_ID, NgModule} from '@angular/core';
import {BrowserModule} from '@angular/platform-browser';

import {AppComponent} from './app.component';
import {AppRoutingModule} from './app-routing.module';
import {HeaderComponent} from './shared/layout/header/header.component';
import {FooterComponent} from './shared/layout/footer/footer.component';
import {RouterModule} from "@angular/router";
import {NgbModule} from '@ng-bootstrap/ng-bootstrap';
import {LoginComponent} from './components/authentication/login/login.component';
import {RegisterComponent} from './components/authentication/register/register.component';
import {ForgotPasswordComponent} from './components/authentication/forgot-password/forgot-password.component';
import {ResetPasswordComponent} from './components/authentication/reset-password/reset-password.component';
import {
  ResendConfirmationEmailComponent
} from './components/authentication/resend-confirmation-email/resend-confirmation-email.component';
import {ConfirmEmailComponent} from './components/authentication/confirm-email/confirm-email.component';
import {CreateGroupComponent} from './components/groups/create-group/create-group.component';
import {GroupsComponent} from './components/groups/groups/groups.component';
import {GroupComponent} from './components/groups/group/group.component';
import {UserComponent} from './components/users/user/user.component';
import {UsersComponent} from './components/users/users/users.component';
import {TripsComponent} from './components/trips/trips/trips.component';
import {TripComponent} from './components/trips/trip/trip.component';
import {SelfComponent} from './components/users/self/self.component';
import {PostComponent} from './components/post/post.component';
import {HomeComponent} from "./components/home/home.component";
import {HttpClientModule} from "@angular/common/http";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {ToastComponent} from './shared/layout/toast/toast.component';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {authInterceptorProviders} from "./interceptors/jwt.interceptor";
import {StatisticsComponent} from './components/statistics/statistics.component';
import {BanDialogComponent} from './components/groups/group/ban-dialog/ban-dialog.component';
import {MatDialogModule} from "@angular/material/dialog";
import {MatFormFieldModule} from "@angular/material/form-field";
import {MatSelectModule} from "@angular/material/select";
import {MatDatepickerModule} from "@angular/material/datepicker";
import {MatInputModule} from "@angular/material/input";
import {MatNativeDateModule} from "@angular/material/core";
import {MatTabsModule} from '@angular/material/tabs';
import {MatCheckboxModule} from "@angular/material/checkbox";
import {RankingsComponent} from './components/rankings/rankings.component';
import {MatButtonModule} from "@angular/material/button";
import {UpdateDialogComponent} from './components/rankings/update-dialog/update-dialog.component';
import {
  MAT_COLOR_FORMATS,
  NgxMatColorPickerModule,
  NGX_MAT_COLOR_FORMATS
} from '@angular-material-components/color-picker';
import {AgmCoreModule} from "@agm/core";
import {environment} from "../environments/environment";
import {AddActivityDialogComponent} from './components/trips/trip/add-activity-dialog/add-activity-dialog.component';
import {MatRadioModule} from "@angular/material/radio";
import {MatTableModule} from "@angular/material/table";
import {registerLocaleData} from "@angular/common";
import localePt from '@angular/common/locales/pt';
import {AddTransportDialogComponent} from './components/trips/trip/add-transport-dialog/add-transport-dialog.component';
import {NgxMatDatetimePickerModule, NgxMatNativeDateModule} from "@angular-material-components/datetime-picker";
import {EditActivityDialogComponent} from './components/trips/trip/edit-activity-dialog/edit-activity-dialog.component';
import {MatPaginatorModule} from "@angular/material/paginator";
import {GoogleChartsModule} from "angular-google-charts";
import {MatSortModule} from "@angular/material/sort";
import { EditPostDialogComponent } from './components/trips/trip/edit-post-dialog/edit-post-dialog.component';
import { CreatePostDialogComponent } from './components/trips/trip/create-post-dialog/create-post-dialog.component';
import {MatDividerModule} from "@angular/material/divider";
import {MatCardModule} from "@angular/material/card";

registerLocaleData(localePt);

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    FooterComponent,
    LoginComponent,
    RegisterComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,
    ResendConfirmationEmailComponent,
    ConfirmEmailComponent,
    CreateGroupComponent,
    GroupsComponent,
    GroupComponent,
    UserComponent,
    UsersComponent,
    TripsComponent,
    TripComponent,
    SelfComponent,
    PostComponent,
    HomeComponent,
    ToastComponent,
    StatisticsComponent,
    BanDialogComponent,
    RankingsComponent,
    AddActivityDialogComponent,
    AddTransportDialogComponent,
    EditActivityDialogComponent,
    UpdateDialogComponent,
    EditPostDialogComponent,
    CreatePostDialogComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    RouterModule,
    NgbModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    BrowserAnimationsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDatepickerModule,
    MatInputModule,
    MatNativeDateModule,
    MatTabsModule,
    MatCheckboxModule,
    AgmCoreModule.forRoot({
      apiKey: environment.GOOGLE_MAPS_API_KEY,
      libraries: ['places']
    }),
    MatRadioModule,
    MatTableModule,
    NgxMatDatetimePickerModule,
    NgxMatNativeDateModule,
    MatPaginatorModule,
    MatButtonModule,
    NgxMatColorPickerModule,
    GoogleChartsModule,
    MatSortModule,
    MatDividerModule,
    MatCardModule
  ],
  providers: [
    authInterceptorProviders,
    {provide: LOCALE_ID, useValue: 'pt'},
    {provide: MAT_COLOR_FORMATS, useValue: NGX_MAT_COLOR_FORMATS}
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
