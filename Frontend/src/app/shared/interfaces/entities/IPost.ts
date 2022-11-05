import {IUser} from "./IUser";
import {ITrip} from "./ITrip";
import {IAttachment} from "./IAttachment";

export interface IPost {
  id: string;
  description: string;
  user: IUser;
  trip: ITrip;
  date: Date;
  publishedDate: Date;
  attachments: IAttachment[];
  isHidden: boolean;
}
