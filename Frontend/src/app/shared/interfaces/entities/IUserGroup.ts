import {IUser} from "./IUser";
import {IGroup} from "./IGroup";
import {UserGroupRole} from "../../enums/UserGroupRole";

export interface IUserGroup {
  id: string;
  user: IUser;
  group: IGroup;
  entranceDate: Date;
  role: UserGroupRole
}
