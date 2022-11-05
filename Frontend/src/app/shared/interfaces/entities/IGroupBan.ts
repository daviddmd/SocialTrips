import {IUser} from "./IUser";
import {IGroup} from "./IGroup";

export interface IGroupBan{
  id: string;
  user: IUser;
  group: IGroup;
  banReason: string;
  banDate: Date;
  banUntil: Date | null;
}
