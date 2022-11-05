import {IPost} from "./entities/IPost";
import {ITrip} from "./entities/ITrip";

export interface IPostEditDialogData {
  post: IPost;
  trip: ITrip;
}
