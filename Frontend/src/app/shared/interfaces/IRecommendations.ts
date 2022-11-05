import { IGroup } from "./entities/IGroup";
import {ITrip} from "./entities/ITrip";
import {IPost} from "./entities/IPost";

export interface IRecommendations{
  recommendedGroups : IGroup[];
  featuredGroups: IGroup[];
  recommendedTrips : ITrip[];
  latestPostsCommunity : IPost[];
  latestPostsFriends: IPost[];
}
