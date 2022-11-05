import {ITrip} from "./ITrip";
import {ActivityType} from "../../enums/ActivityType";
import {TransportType} from "../../enums/TransportType";

export interface IActivity{
  id: string;
  beginningDate: Date;
  endingDate: Date;
  address: string;
  description: string;
  googlePlaceId: string;
  realAddress: string;
  latitude: number;
  longitude: number;
  expectedBudget: number;
  trip: ITrip;
  activityType: ActivityType;
  transportType: TransportType;
}
