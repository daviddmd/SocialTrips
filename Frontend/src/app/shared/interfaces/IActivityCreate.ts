import {IActivityCreateIndividual} from "./IActivityCreateIndividual";

export interface IActivityCreate{
  tripId: string;
  activityCreate: IActivityCreateIndividual;
  activityTransport: IActivityCreateIndividual | null;
}
