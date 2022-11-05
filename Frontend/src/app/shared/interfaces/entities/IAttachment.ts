import {IPost} from "./IPost";

export interface IAttachment{
  id: string;
  originalFileName: string;
  storageName: string;
  url: string;
  uploadedDate: Date;
  post: IPost;
}
