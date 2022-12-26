using BackendAPI.Data;
using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Exceptions;
using BackendAPI.Helpers;
using BackendAPI.Models.Trip;
using Geolocation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace BackendAPI.Repositories
{
    public class TripRepository : ITripRepository
    {
        private readonly IGroupRepository _groupRepository;
        private readonly DatabaseContext _context;
        private readonly IGoogleCloudStorageHelper _storageHelper;

        public TripRepository(IGroupRepository groupRepository, DatabaseContext context, IGoogleCloudStorageHelper storageHelper)
        {
            _groupRepository = groupRepository;
            _context = context;
            _storageHelper = storageHelper;
        }



        //Atualiza a distância e custo total da viagem à medida que se vai adicionando ou removendo actividades da mesma
        public async Task RecalculateTripDistanceAndBudget(Trip trip)
        {
            double distance = 0;
            double budget = _context.Activities.Where(a=>a.Trip.Id==trip.Id).Sum(a=>a.ExpectedBudget);
            List<Activity> Activities = trip.Activities.Where(t => t.ActivityType != ActivityType.TRANSPORT).OrderBy(i=>i.Id).ToList();
            for (int i = 0; i < Activities.Count - 1; i++)
            {
                distance += GeoCalculator.GetDistance(
                    new Coordinate(Activities[i].Latitude, Activities[i].Longitude),
                    new Coordinate(Activities[i + 1].Latitude, Activities[i + 1].Longitude), 2, DistanceUnit.Kilometers);
            }
            trip.TotalDistance = distance;
            trip.ExpectedBudget = budget;
            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();
        }
        /*
         * Atualiza a distância total percorrida de cada utilizador na viagem e o seu ranking por arrasto
         * Teríamos dois modos de fazer isto:
         * 1: Quando uma viagem acaba, atualizar a distância percorrida dos utilizadores desta viagem baseado nas viagens a que o mesmo pertence que estão terminadas
         * 2: Quando uma viagem acaba, atualizar a distância percorrida dos utilizadores desta viagem baseado na distância da própria viagem, sem ter em conta outras
         * Foi escolhida a segunda opção, porque se após o utilizador terminar a viagem o mesmo saísse da mesma (para libertar espaço), mal outra terminasse o mesmo iria perder os kms percorridos,
         * apesar de ter feito a mesma viagem. Como uma viagem apenas pode ser terminada uma vez, os quilómetros estão seguros.
         */
        private async Task UpdateUsersRankingsAndDistance(Trip trip)
        {
            List<Ranking> ranking_list = await _context.Rankings.OrderByDescending(cr => cr.MinimumKilometers).ToListAsync();
            foreach (User user in trip.Users.Select(t=>t.User))
            {
                user.TravelledKilometers += trip.TotalDistance;
                foreach (Ranking r in ranking_list)
                {
                    if (user.TravelledKilometers >= r.MinimumKilometers)
                    {
                        user.Ranking = r;
                        //this is the maximum rank, since it's descending, go to the next user
                        break;
                    }
                }
                _context.User.Update(user);
            }
            await _context.SaveChangesAsync();
        }
        //Após o término de uma viagem, recalcula a distância e custo médio das viagens neste mesmo grupo
        private async Task UpdateAverageCostAndDistanceTripGroup(Group group)
        {
            int NumberTripsCompleted = group.Trips.Count(t => t.IsCompleted);
            double TotalCostTripsCompleted = group.Trips.Where(t=>t.IsCompleted).Sum(t => t.ExpectedBudget);
            double TotalDistanceTripsCompleted = group.Trips.Where(t=>t.IsCompleted).Sum(t => t.TotalDistance);
            if (NumberTripsCompleted == 0)
            {
                group.AverageTripCost = 0;
                group.AverageTripDistance = 0;
            }
            else
            {
                group.AverageTripCost = (double) (TotalCostTripsCompleted / NumberTripsCompleted);
                group.AverageTripDistance = (double) (TotalDistanceTripsCompleted / NumberTripsCompleted);
            }

            _context.Groups.Update(group);
            await _context.SaveChangesAsync();
        }
        public async Task<UserTrip> GetUserTrip(Trip trip, User user)
        {
            UserTrip userTrip = await _context.UserTrips.Where(userTrip => userTrip.User == user && userTrip.Trip == trip).SingleOrDefaultAsync();
            return userTrip;
        }
        /*
         * Não nos interessa realmente remover a viagem, visto que isto irá levar a todos os posts e itinerário serem removidos.
         * Ao invés, iremos dar a mesma como escondida, completa, e iremos remover todos os utilizadores dela. Assim apenas os gestores do grupo a que ela pertence a poderão ver/juntar-se à mesma
         */
        public async Task Delete(Trip trip)
        {
            trip.IsPrivate = true;
            trip.Users.Clear();
            trip.Invites.Clear();
            trip.Posts.ForEach(post => post.IsHidden = true);
            _context.Posts.UpdateRange(trip.Posts);
            _context.Trips.Update(trip);
            TripEvent tripEvent = new()
            {
                Trip = trip,
                EventType = EventType.TRIP_DELETE,
                Date = DateTime.Now
            };
            _context.TripEvents.Add(tripEvent);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Trip>> GetAll()
        {
            return await _context.Trips.AsQueryable().ToListAsync();
        }
        public async Task<Trip> GetById(int Id)
        {
            return await _context.Trips.Where(trip => trip.Id == Id).SingleOrDefaultAsync();
        }

        public async Task Update(Trip trip, TripDetailsUpdateModel model)
        {
            /*
             * Temos que verificar se ao alterar a data de início ou de fim, estamos a deixar actividades existentes para trás. Exemplo:
             * Puxar a atividade de início para a frente se já existirem algumas atrás, ou a de fim para trás
             */
            if(_context.Activities.Any(a=>a.Trip.Id == trip.Id && (a.BeginningDate < model.BeginningDate || a.EndingDate > model.EndingDate)))
            {
                throw new CustomException("Activities already exist before the beginning date or after the ending date", ErrorType.TRIP_DATE_EXISTING_ACTIVITIES);
            }
            //A nova data de início não pode ser antes da data actual, caso contrário estaremos a criar uma viagem no passado
            if (
                (model.BeginningDate.Date != trip.BeginningDate.Date || model.EndingDate.Date != trip.EndingDate.Date) &&
                (model.BeginningDate.Date < DateTime.Now.Date || model.EndingDate.Date < DateTime.Now.Date || model.BeginningDate > model.EndingDate) &&
                !(model.IsCompleted||trip.IsCompleted)
                )
            {
                throw new CustomException("The beginning date of the trip must be in the future and the beginning date must be before the ending date", ErrorType.TRIP_DATE_INVALID);
            }
            /*
             * Apenas se pode terminar uma viagem uma vez, não se podendo reactivar a mesma
             * Ao terminar uma viagem:
             * 1: Novas pessoas não se podem "juntar" à mesma (podem ver a mesma historicamente, com o seu itinerário e publicações)
             * 2: Modificar o itinerário da viagem (atualizar uma atividade, remover atividade, adicionar atividade) não é permitido
             */
            bool TripComplete = (!trip.IsCompleted && model.IsCompleted);
            trip.Name = model.Name;
            trip.Description = model.Description;
            trip.BeginningDate = model.BeginningDate;
            trip.EndingDate = model.EndingDate;
            if (TripComplete)
            {
                trip.IsCompleted = model.IsCompleted;
            }
            trip.IsPrivate = model.IsPrivate;
            _context.Trips.Update(trip);
            if (TripComplete)
            {
                /*
                 * Desativado para Debugging, ativar em "produção"
                if (DateTime.Now.Date < model.EndingDate)
                {
                    throw new Exception("You can't change the status of the trip before its ending.");
                }
                */
                await UpdateUsersRankingsAndDistance(trip);
                await UpdateAverageCostAndDistanceTripGroup(trip.Group);
                trip.Invites.Clear(); //remover todos os convites visto que novos membros não poderão mais entrar
            }
            TripEvent tripEvent = new()
            {
                Trip = trip,
                EventType = EventType.TRIP_DETAILS_UPDATE,
                Date = DateTime.Now
            };
            _context.TripEvents.Add(tripEvent);
            await _context.SaveChangesAsync();
        }
        public async Task AddUser(Trip trip, User user, Guid? InviteId, bool IsManager)
        {
            TripInvite invite = await _context.TripInvites.Where(ti => ti.Id == InviteId && ti.User.Id == user.Id && ti.Trip.Id == trip.Id).FirstOrDefaultAsync();
            bool WasInvited = false;
            //se for gestor do grupo ou administrador do sistema, não precisa de dar convite para entrar na viagem
            if (trip.IsPrivate && !IsManager && invite == null)
            {
                throw new CustomException("This invite doesn't exist.", ErrorType.TRIP_INVITE_INVALID);

            }
            //é possível juntar-se a um grupo ou viagem que não seja privado com convite
            if (invite!=null)
            {
                WasInvited = true;
                _context.TripInvites.Remove(invite);
                await _context.SaveChangesAsync();
            }
            if (trip.IsCompleted)
            {
                throw new CustomException("The Trip is already completed, no new people can join", ErrorType.TRIP_COMPLETED);
            }
            //verificar se o user está no grupo, falhar se não estiver.
            UserGroup userGroup = await _groupRepository.GetUserGroup(trip.Group, user);
            if (userGroup == null)
            {
                throw new CustomException("This user cannot join this trip because he doesn't belong to the group.", ErrorType.TRIP_USER_NOT_PRESENT_GROUP);
            }
            //verificar se o utilizador está já na viagem, falhar se já estiver.
            UserTrip userTrip = await GetUserTrip(trip, user);
            if (userTrip == null)
            {
                UserTrip create = new() { User = user, Trip = trip, EntranceDate = DateTime.Now };
                _context.UserTrips.Add(create);
                TripEvent tripEvent = new()
                {
                    Trip = trip,
                    EventType = WasInvited ? EventType.TRIP_USER_ENTER_INVITE : EventType.TRIP_USER_ENTER,
                    User = user,
                    Date = DateTime.Now
                };
                _context.TripEvents.Add(tripEvent);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new CustomException("This user is already on this trip", ErrorType.TRIP_USER_ALREADY_PRESENT);
            }
        }
        //Adiciona uma viagem a um grupo
        public async Task Create(Trip trip, Group group)
        {
            trip.Group = group;
            trip.IsCompleted = false;
            if (trip.BeginningDate.Date < DateTime.Now || trip.EndingDate.Date < DateTime.Now || trip.BeginningDate.Date > trip.EndingDate.Date)
            {
                throw new CustomException("The beginning date of the trip must be in the future and the beginning date must be before the ending date", ErrorType.TRIP_DATE_INVALID);
            }
            _context.Trips.Add(trip);
            TripEvent tripEvent = new()
            {
                Trip = trip,
                EventType = EventType.TRIP_CREATE,
                Date = DateTime.Now
            };
            _context.TripEvents.Add(tripEvent);
            await _context.SaveChangesAsync();
        }

        public async Task InviteUser(TripInvite tripInvite)
        {
            //novos convites apenas podem ser enviados se os anteriores tiverem sido consumidos. isto irá procurar por um convite existente
            if (tripInvite.Trip.IsCompleted)
            {
                throw new CustomException("The Trip is already completed, no new people can be invited", ErrorType.TRIP_COMPLETED);
            }
            if (_context.TripInvites.Any(gi => gi.User == tripInvite.User && gi.Trip == tripInvite.Trip))
            {
                throw new CustomException("This user was already invited", ErrorType.TRIP_USER_ALREADY_INVITED);
            }
            if (_context.UserTrips.Any(ut => ut.User.Id == tripInvite.User.Id && ut.Trip.Id == tripInvite.Trip.Id))
            {
                throw new CustomException("This user is already on this trip", ErrorType.TRIP_USER_ALREADY_PRESENT);
            }
            if (!(_context.UserGroups.Any(g => g.User.Id == tripInvite.User.Id && g.Group.Id==tripInvite.Trip.Group.Id)))
            {
                throw new CustomException("This user cannot join this trip because he doesn't belong to the group.", ErrorType.TRIP_USER_NOT_PRESENT_GROUP);
            }
            _context.TripInvites.Add(tripInvite);
            TripEvent tripEvent = new()
            {
                Trip = tripInvite.Trip,
                User = tripInvite.User,
                EventType = EventType.TRIP_INVITE_CREATE,
                Date = DateTime.Now
            };
            _context.TripEvents.Add(tripEvent);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Trip>> Search(TripSearchModel model)
        {
            if (model.TripDestination == ""  && model.TripDescription == "" && model.TripName == "")
            {
                return new List<Trip>();
            }
            return await _context.Trips.Where(trip =>
            !trip.IsCompleted &&
            (
            (model.TripName != "" && trip.Name.ToLower().Contains(model.TripName.ToLower())) ||
            (model.TripDescription!="" && trip.Description.ToLower().Contains(model.TripDescription.ToLower())) ||
            (model.TripDestination!="" && trip.Activities.Any(a => a.RealAddress.ToLower().Contains(model.TripDestination.ToLower()) || a.Description.ToLower().Contains(model.TripDestination.ToLower())))
            )
            ).ToListAsync();
        }

        public async Task RemoveUser(Trip trip, User user)
        {
            UserTrip userTrip = await GetUserTrip(trip, user);
            if (userTrip == null)
            {
                throw new CustomException("This user isn't in this trip", ErrorType.TRIP_USER_NOT_PRESENT);
            }
            _context.UserTrips.Remove(userTrip);
            TripEvent tripEvent = new()
            {
                Trip = trip,
                User = user,
                EventType = EventType.TRIP_USER_LEAVE,
                Date = DateTime.Now
            };
            _context.TripEvents.Add(tripEvent);
            await _context.SaveChangesAsync();
        }
        public async Task<Activity> GetActivityById(int Id)
        {
            return await _context.Activities.Where(activity => activity.Id == Id).SingleOrDefaultAsync();
        }

        public async Task<TripInvite> GetTripInviteById(Guid? Id)
        {
            return await _context.TripInvites.Where(ti => ti.Id == Id).SingleOrDefaultAsync();
        }

        public async Task RemoveInvite(TripInvite invite)
        {
            TripEvent tripEvent = new()
            {
                Trip = invite.Trip,
                User = invite.User,
                EventType = EventType.TRIP_INVITE_REJECT,
                Date = DateTime.Now
            };
            _context.TripEvents.Add(tripEvent);
            _context.TripInvites.Remove(invite);
            await _context.SaveChangesAsync();
        }

        public Task UpdateImage(Group group, IFormFile file)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateImage(Trip trip, IFormFile file)
        {
            if (file != null && FileHelper.IsImage(file))
            {
                //delete the old one if exists
                await RemoveImage(trip);
                String OriginalFileName = file.FileName;
                String DestinationFileName = $"trip_{trip.Id}{Path.GetExtension(file.FileName).ToLower()}";
                String Url = await _storageHelper.Upload(file, DestinationFileName);
                Attachment attachment = new() { OriginalFileName = OriginalFileName, StorageName = DestinationFileName, Url = Url, UploadedDate = DateTime.Now };
                _context.Attachments.Add(attachment);
                trip.Image = attachment;
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new CustomException("No valid image passed", ErrorType.MEDIA_ERROR);
            }
        }

        public async Task RemoveImage(Trip trip)
        {
            if (trip.Image != null)
            {
                await _storageHelper.Delete(trip.Image.StorageName);
                _context.Attachments.Remove(trip.Image);
                trip.Image = null;
                await _context.SaveChangesAsync();
            }
        }
    }
}
