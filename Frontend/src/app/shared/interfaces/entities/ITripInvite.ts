import {IUser} from "./IUser";
import {ITrip} from "./ITrip";

export  interface ITripInvite {
  id: string;
  user: IUser;
  trip: ITrip;
  invitationDate: Date;
}
