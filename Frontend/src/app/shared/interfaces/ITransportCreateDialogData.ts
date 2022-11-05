import {IActivity} from "./entities/IActivity";

export interface ITransportCreateDialogData {
  placeToAdd : google.maps.places.PlaceResult;
  previousPlace : IActivity;
}
