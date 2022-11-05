import {ITrip} from "./ITrip";
import {EventType} from "../../enums/EventType";
import {IUser} from "./IUser";

export interface ITripEvent {
  id: string;
  trip: ITrip;
  eventType: EventType;
  date: Date;
  user: IUser | null;
}
