import {IGroup} from "./IGroup";
import {EventType} from "../../enums/EventType";
import {IUser} from "./IUser";

export interface IGroupEvent{
  id: string;
  group: IGroup;
  eventType: EventType;
  date: Date;
  user: IUser | null;
}
