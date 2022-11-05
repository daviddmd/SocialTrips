import {IUser} from "./IUser";
import {IGroup} from "./IGroup";

export interface IGroupInvite {
  id: string;
  user: IUser;
  group: IGroup;
  invitationDate: Date;
}
