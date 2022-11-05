import {IActivityCreateIndividual} from "./IActivityCreateIndividual";

export interface IActivityCreateDialogData {
  placeToAdd : google.maps.places.PlaceResult;
  //para ter a data de início mínima, que pode vir do transporte (se aplicável), ou se não houver transporte, as 8:00 do próprio dia
  minimumBeginningDate : Date;
}
