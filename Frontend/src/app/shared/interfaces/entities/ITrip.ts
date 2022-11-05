import {IActivity} from "./IActivity";
import {IPost} from "./IPost";
import {IGroup} from "./IGroup";
import {IUserTrip} from "./IUserTrip";
import {ITripInvite} from "./ITripInvite";
import {ITripEvent} from "./ITripEvent";

export interface ITrip {
  id: string;
  name: string;
  image: string;
  description: string;
  beginningDate: Date;
  endingDate: Date;
  isCompleted: boolean;
  isPrivate : boolean;
  expectedBudget: number;
  activities: IActivity[];
  posts: IPost[];
  group: IGroup;
  totalDistance: number;
  users: IUserTrip[];
  invites: ITripInvite[];
  events: ITripEvent[];
}
