import {IActivity} from "./entities/IActivity";

export interface IActivityEditDialogData{
  minimumDate : Date|null;
  maximumDate : Date|null;
  activity: IActivity;
}
