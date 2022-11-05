import {TransportType} from "../../enums/TransportType";

export interface IActivityTransport {
  transportType: TransportType;
  description: string;
  distance: number;
  departureTime : Date;
  arrivalTime: Date;
}
