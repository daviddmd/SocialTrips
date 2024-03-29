# SocialTrips

SocialTrips or Viagens Sociais is an ASP.NET Core (API) + Angular 13 application, that's a Trips Social Network.

It allows members of this network to create groups, and in each group trips, that have a multi-day itinerary made up of
multiple activities that are linked to each other with different means of transportation (provided by Google Maps) and
have multiple attributes associated with them, such as distance, type and cost.

Members of each group may create posts to each trip, that are aggregated in each group to all group members, if the trip
isn't set to private. Finally, it provides facilities for group and trip discovery to new members, if the groups or
trips aren't set to private, as well as facilities for featuring groups or trips.

It provides moderation tools for posts, groups and trips to prevent member abuse, as well as a comprehensive statistics
page that aggregates trips and groups based on their attributes, such as the most expensive trips (that's a sum of the
cost of all activities in them), the most travelled groups (that did the most trips or trips with the most cumulative
distance), the most travelled members, amongst other statistics.

It also contains an extensive auditing system for all activities performed by all members in Groups or Trips.

Finally, it provides a leaderboard for members, based on their travel activity, having a built-in and customizable rank
system.

Installation instructions and Technical Aspects are available at [INSTALL.md](INSTALL.md).

This project is licensed under the [AGPL 3 license](https://www.gnu.org/licenses/agpl-3.0.en.html), a copy of which may
be found on [LICENSE](LICENSE).

