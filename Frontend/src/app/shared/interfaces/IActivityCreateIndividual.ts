import {ActivityType} from "../enums/ActivityType";
import {TransportType} from "../enums/TransportType";

export interface IActivityCreateIndividual{
  beginningDate: Date;
  endingDate: Date;
  address: string;
  description: string;
  googlePlaceId: string;
  expectedBudget: number;
  activityType: ActivityType;
  transportType: TransportType;

}
