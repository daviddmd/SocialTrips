import {ActivityType} from "../enums/ActivityType";
import {TransportType} from "../enums/TransportType";

export interface IActivityUpdate{
  beginningDate: Date;
  endingDate: Date;
  address: string;
  description: string;
  expectedBudget: number;
  activityType: ActivityType;
  transportType: TransportType;
}
