import {IPostEditDialogData} from "./IPostEditDialogData";
import {IPost} from "./entities/IPost";

export interface IPostEditDialogReturnData {
  formValue: IPostEditDialogData | null;
  originalPost: IPost;
}
