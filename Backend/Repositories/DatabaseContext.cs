using Microsoft.EntityFrameworkCore;
using BackendAPI.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BackendAPI.Data
{
    public class DatabaseContext : IdentityDbContext<User>
    {
        public DatabaseContext (DbContextOptions<DatabaseContext> options): base(options){
        }

        public DbSet<User> User { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupBan> GroupBans { get; set; }
        public DbSet<GroupEvent> GroupEvents { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<GroupInvite> GroupInvites { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripEvent> TripEvents { get; set; }
        public DbSet<UserTrip> UserTrips { get; set; }
        public DbSet<TripInvite> TripInvites { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Ranking> Rankings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>().HasMany(x => x.Followers).WithMany(x => x.Following).UsingEntity(x => x.ToTable("Followers"));
            base.OnModelCreating(builder);
        }
    }
}
