import {IUser} from "./IUser";
import {ITrip} from "./ITrip";

export interface IUserTrip{
  id: string;
  user: IUser;
  trip: ITrip;
  entranceDate: Date;
}
