using BackendAPI.Entities;
using BackendAPI.Models.Activity;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    public interface IActivityRepository
    {
        Task AddActivity(Trip trip, Activity activity, Activity transport);
        Task RemoveActivity(Trip trip, Activity activity);
        Task<Activity> GetById(int Id);
        Task UpdateActivity(Activity activity, ActivityUpdateModel model);
    }
}
