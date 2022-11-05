import {IRanking} from "./IRanking";
import {IGroup} from "./IGroup";
import {ITrip} from "./ITrip";
import {IPost} from "./IPost";
import {IGroupInvite} from "./IGroupInvite";
import {ITripInvite} from "./ITripInvite";
import {IUserTrip} from "./IUserTrip";
import {IUserGroup} from "./IUserGroup";

export interface IUser {
  id: string;
  email: string;
  userName: string;
  name: string;
  photo: string;
  country: string;
  city: string;
  description: string;
  travelledKilometers: number;
  facebook: string;
  instagram: string;
  twitter: string;
  phoneNumber: string;
  ranking: IRanking;
  groups: IUserGroup[];
  trips: IUserTrip[];
  posts: IPost[];
  groupInvites: IGroupInvite[];
  tripInvites: ITripInvite[];
  isActive: boolean;
  creationDate: Date;
  locale: string;
  followers: IUser[];
  following: IUser[];

}
