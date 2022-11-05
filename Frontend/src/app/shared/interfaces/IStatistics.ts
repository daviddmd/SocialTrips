export interface IStatistics {
  mostVisitedPlaces: Record<string, number>;
  tripsByTotalDistance: Record<string, number>;
  tripsByTotalCost: Record<string, number>;
  tripsByNumberUsers: Record<string, number>;
  groupsByAverageDistance: Record<string, number>;
  groupsByAverageCost: Record<string, number>;
  groupsByNumberUsers: Record<string, number>;
  rankingUserDistribution: Record<string, number>;
  transportTypeDistribution: Record<string, number>;
  activityTypeDistribution: Record<string, number>;

}
