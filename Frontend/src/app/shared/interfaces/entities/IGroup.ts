import {IUserGroup} from "./IUserGroup";
import {IGroupInvite} from "./IGroupInvite";
import {ITrip} from "./ITrip";
import {IGroupEvent} from "./IGroupEvent";
import {IGroupBan} from "./IGroupBan";

export interface IGroup {
  id: string;
  name: string;
  description: string;
  image: string;
  users: IUserGroup[];
  creationDate: string;
  hasExperiencedUser: boolean;
  isPrivate : boolean;
  invites: IGroupInvite[];
  trips: ITrip[];
  averageTripCost : number;
  averageTripDistance: number;
  isFeatured : boolean;
  events: IGroupEvent[];
  bans: IGroupBan[];
}
