using BackendAPI.Data;
using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Exceptions;
using BackendAPI.Helpers;
using BackendAPI.Models.Activity;
using Geolocation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly DatabaseContext _context;
        private readonly IGoogleMapsHelper _googleMapsHelper;

        public ActivityRepository(DatabaseContext context, IGoogleMapsHelper googleMapsHelper)
        {
            _context = context;
            _googleMapsHelper = googleMapsHelper;
        }

        public async Task AddActivity(Trip trip, Activity activity, Activity transport)
        {
            /*
             * Regras para um itinerário válido:
             * 0: Não se pode adicionar uma actividade a uma viagem completa (finalizada, com itinerário final).
             * 1: As datas de início e fim das da actividade e transporte têm que ser no mesmo dia
             * 2: O início de cada actividade tem que ser após o final da respectiva actividade de transporte
             * 3: A data de início e fim da actividade e do transporte têm que ser diferentes, e o fim tem que ser superior ao início
             * 4: O transporte apenas pode ser nulo se não houverem actividades nesse dia 
             * 5: Apenas se podem inserir viagens no final do itinerário do dia: funciona exactamente como uma queue. os primeiros a ser removidos são sempre os últimos também.
             * Portanto, verifica-se se a data de início do transporte é maior ou igual à data de fim da actividade anterior (a última desse DIA
             * 6: Não permitir adicionar actividades com um place ID inválido. Isto porque vamos associar as coordenadas desse place ID à actividade a adicionar para depois fazer os cálculos de distância total sem fazer 
             * demasiados pedidos API (e estourar os limites do API de pedir coordenadas associadas ao place ID de cada vez que adicionamos ou removemos uma actividade)
             * 7: A actividade a adicionar não pode ser igual à anterior (place ID), caso contrário não faz sentido.
             * 8: A actividade a adicionar tem que ter a sua data de início entre a data de início e fim da viagem, para fazer parte do seu itinerário
             * 9: Mesmo que seja a primeira actividade do dia, ou a última actividade do dia, a data de início e fim são obrigatórias.
             * 10: Validar transport types e activity types
             */
            if (trip.IsCompleted)
            {
                throw new CustomException("Can't add activities in a completed trip", ErrorType.TRIP_COMPLETED);
            }
            DateTime ActivityBeginning = activity.BeginningDate;
            DateTime ActivityEnding = activity.EndingDate;
            bool IsFirstActivityOfDay = !(await _context.Activities.AnyAsync(a => a.Trip.Id == trip.Id && a.BeginningDate.Date == ActivityBeginning.Date));
            bool ActivityToAddIsInRangeOfTripDate = trip.BeginningDate.Date <= ActivityBeginning.Date && ActivityBeginning.Date <= trip.EndingDate.Date;
            if (!ActivityToAddIsInRangeOfTripDate)
            {
                throw new CustomException("The activity to add isn't in the trip date range (the beginning date is lower than the trip beginning date or the ending date is higher than the trip ending date)", ErrorType.ACTIVITY_NOT_IN_TRIP_RANGE);
            }
            if (!Enum.IsDefined(activity.ActivityType))
            {
                throw new CustomException($"Undefined Activity Type Passed - ID:{activity.ActivityType}", ErrorType.ACTIVITY_TYPE_NOT_DEFINED);
            }
            //Não pode ser transporte, mas se por algum motivo for corrigir
            if (activity.ActivityType == ActivityType.TRANSPORT)
            {
                activity.ActivityType = ActivityType.VISIT;
            }
            if (IsFirstActivityOfDay)
            {
                if (!(ActivityBeginning.Date == ActivityEnding.Date))
                {
                    throw new CustomException("The activity must have its beginning and end in the same day", ErrorType.ACTIVITY_NOT_SAME_DAY);
                }
                if (ActivityBeginning >= ActivityEnding)
                {
                    throw new CustomException("Beginning date cannot be greater than the ending date", ErrorType.ACTIVITY_OVERLAP);
                }
            }
            else
            {
                if (transport == null)
                {
                    throw new CustomException("A transport activity must be provided since there are already activities in this day", ErrorType.ACTIVITY_MISSING_TRANSPORT);
                }
                if (!Enum.IsDefined(transport.TransportType))
                {
                    throw new CustomException("Undefined transport type passed", ErrorType.ACTIVITY_TRANSPORT_TYPE_NOT_DEFINED);
                }
                DateTime TransportBeginning = transport.BeginningDate;
                DateTime TransportEnding = transport.EndingDate;
                if (ActivityBeginning < TransportEnding)
                {
                    throw new CustomException("Activity must start after the transport", ErrorType.ACTIVITY_OVERLAP);
                }
                if (ActivityBeginning >= ActivityEnding || TransportBeginning >= TransportEnding)
                {
                    throw new CustomException("Beginning date cannot be greater than the ending date", ErrorType.ACTIVITY_OVERLAP);
                }
                if (!(ActivityBeginning.Date == ActivityEnding.Date && TransportBeginning.Date == TransportEnding.Date && ActivityBeginning.Date == TransportEnding.Date))
                {
                    throw new CustomException("The activities must have their beginning and end in the same day", ErrorType.ACTIVITY_NOT_SAME_DAY);
                }
                //obter data de término da última actividade
                Activity LastActivityDay = await _context.Activities.Where(a => a.Trip.Id == trip.Id && a.EndingDate.Date == transport.BeginningDate.Date).OrderByDescending(a => a.EndingDate).FirstOrDefaultAsync();
                if (LastActivityDay.GooglePlaceId == activity.GooglePlaceId)
                {
                    throw new CustomException("The last activity has the same place ID as the activity to add.", ErrorType.ACTIVITY_REPEATED);
                }
                DateTime LastEndingDate = LastActivityDay.EndingDate;
                if (TransportBeginning < LastEndingDate)
                {
                    throw new CustomException("The beginning date of the transport cannot be lower than the ending date of the last activity", ErrorType.ACTIVITY_OVERLAP);
                }
                //associar coordenadas do place ID à actividade a adicionar
                transport.ActivityType = ActivityType.TRANSPORT;
                transport.Trip = trip;
                _context.Activities.Add(transport);
            }
            (Coordinate, String,String) CoordinateAddressName = await _googleMapsHelper.GetCoordinatesAndAddressFromPlaceId(activity.GooglePlaceId);
            activity.Latitude = CoordinateAddressName.Item1.Latitude;
            activity.Longitude = CoordinateAddressName.Item1.Longitude;
            activity.Trip = trip;
            activity.TransportType = TransportType.None;
            if (String.IsNullOrEmpty(activity.Address))
            {
                activity.Address = CoordinateAddressName.Item2;
            }
            activity.RealAddress = CoordinateAddressName.Item2;
            if (String.IsNullOrEmpty(activity.Description))
            {
                activity.Description = CoordinateAddressName.Item3;
            }
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task<Activity> GetById(int Id)
        {
            return await _context.Activities.Where(a=>a.Id == Id).FirstOrDefaultAsync();
        }
        public async Task RemoveActivity(Trip trip, Activity activity)
        {
            /*
             * 0: Não se pode remover uma actividade de uma viagem completa
             * 1: Não se pode remover uma actividade que tem uma depois, ou seja, apenas se pode remover a última actividade
             * 2: Tem que se remover a actividade de transporte imediatamente antes
             */
            if (trip.IsCompleted)
            {
                throw new CustomException("Can't remove activities in a completed trip", ErrorType.TRIP_COMPLETED);
            }
            if (activity.ActivityType == ActivityType.TRANSPORT)
            {
                throw new CustomException("Can't remove transport activities, only the ones immediately after it", ErrorType.ACTIVITY_REMOVE_TRANSPORT);
            }
            Activity LastActivityDay = await _context.Activities.Where(a => a.Trip.Id == trip.Id && a.EndingDate.Date == activity.BeginningDate.Date).OrderByDescending(a => a.EndingDate).FirstOrDefaultAsync();
            if (LastActivityDay != activity)
            {
                throw new CustomException("You can only remove activities of a daily itinerary from the end", ErrorType.ACTIVITY_REMOVE_NOT_LAST);
            }
            bool IsFirstActivityOfDay = !(await _context.Activities.AnyAsync(a => a.Trip.Id == trip.Id && a.BeginningDate.Date == activity.BeginningDate.Date && a.Id != activity.Id));
            //remover atividade de transporte imediatamente antes
            if (!IsFirstActivityOfDay)
            {
                Activity LastTransportActivityDay = await _context.Activities.Where(a => a.Trip.Id == trip.Id && a.EndingDate.Date == activity.BeginningDate.Date && a.ActivityType == ActivityType.TRANSPORT).OrderByDescending(a => a.EndingDate).FirstOrDefaultAsync();
                _context.Activities.Remove(LastTransportActivityDay);
            }
            _context.Activities.Remove(LastActivityDay);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateActivity(Activity activity, ActivityUpdateModel model)
        {
            /*
             * 0: Não se pode atualizar uma actividade de uma viagem completa
             * 1: Não se pode alterar o dia da actividade
             * 2: Apenas se podem alterar a data de início e de fim se não interferir com a actividade anterior e seguinte (sobreposição)
             * 3: Não se pode alterar o tipo de actividade para "transporte" de uma actividade normal, e alterar o tipo de actividade de uma actividade de transporte
             * 4: Não se pode alterar a localização real da actividade, apenas a "morada" e descrição. A razão de isto ser é que tendo um itinerário bem definido, poder-se-ia levar a alterar a actividade do itinerário,
             * o que iria gerar um efeito cascata que obrigaria a "puxar" todas as actividades para a frente se o tempo de transporte fosse substancialmente maior. deste modo será mais benéfico não permitir este tipo de actividade,
             * e se for necessário mudar a localização de uma actividade, será necessário remover todas as actividades após essa (incluindo a mesma), e adicionar essa localização, refazendo posteriormente o itinerário
             * 5: Alterando as datas de início/fim das actividades anteriores, eventualmente pode-se usar um método no frontend para "recalcular" a actividade de transporte, tendo em conta a nova data de fim da anterior
             * , Atualizando-se sucessivamente a data de início da próxima, usando-se este mesmo método
             */
            if (activity.Trip.IsCompleted)
            {
                throw new CustomException("Can't update activities in a completed trip", ErrorType.TRIP_COMPLETED);
            }
            DateTime ActivityDate = activity.BeginningDate.Date;
            if (model.BeginningDate.Date != ActivityDate || model.EndingDate.Date != ActivityDate)
            {
                throw new CustomException("The new beginning or ending date must be on the same date as the activity", ErrorType.ACTIVITY_NOT_SAME_DAY);
            }
            if (model.BeginningDate >= model.EndingDate)
            {
                throw new CustomException("The beginning date cannot be greater or equal than the ending date", ErrorType.ACTIVITY_OVERLAP);
            }
            bool ActivityToAddIsInRangeOfTripDate = activity.Trip.BeginningDate.Date <= model.BeginningDate.Date && model.EndingDate.Date <= activity.Trip.EndingDate.Date;
            if (!ActivityToAddIsInRangeOfTripDate)
            {
                throw new CustomException("The activity to update isn't in the trip date range (the beginning date is lower than the trip beginning date or the ending date is higher than the trip ending date)", ErrorType.ACTIVITY_NOT_IN_TRIP_RANGE);
            }
            //transporte. se for null, significa que é a primeira actividade do dia, logo não é preciso verificar a bound inferior
            Activity ActivityBefore = await _context.Activities.Where(a=>a.Trip.Id==activity.Trip.Id && a.BeginningDate.Date == ActivityDate && a.EndingDate < activity.BeginningDate).OrderByDescending(a=>a.EndingDate).FirstOrDefaultAsync();
            //transporte. se for null, significa que é a última actividade do dia, logo não é preciso verificar a bound superior
            Activity ActivityAfter = await _context.Activities.Where(a => a.Trip.Id == activity.Trip.Id && a.BeginningDate.Date == ActivityDate && a.BeginningDate > activity.EndingDate).OrderBy(a=>a.BeginningDate).FirstOrDefaultAsync();
            if (ActivityBefore != null && model.BeginningDate < ActivityBefore.EndingDate)
            {
                throw new CustomException("The new beginning date cannot be lower than the previous activity ending date", ErrorType.ACTIVITY_OVERLAP);
            }
            if(ActivityAfter != null && model.EndingDate > ActivityAfter.BeginningDate)
            {
                throw new CustomException("The new ending date cannot be lower than the next activity beginning date", ErrorType.ACTIVITY_OVERLAP);
            }
            if (activity.ActivityType != ActivityType.TRANSPORT && model.ActivityType == ActivityType.TRANSPORT)
            {
                throw new CustomException("Cannot change the activity type of a non-transport activity to a transport one.", ErrorType.ACTIVITY_CHANGE_TYPE);
            }
            if (activity.ActivityType == ActivityType.TRANSPORT && model.ActivityType != ActivityType.TRANSPORT)
            {
                throw new CustomException("Cannot change the activity type of a transport activity.", ErrorType.ACTIVITY_CHANGE_TYPE);
            }
            activity.ActivityType = model.ActivityType;
            activity.BeginningDate = model.BeginningDate;
            activity.EndingDate = model.EndingDate;
            activity.ExpectedBudget = model.ExpectedBudget;
            activity.Address = model.Address;
            activity.Description = model.Description;
            if (activity.ActivityType != ActivityType.TRANSPORT && (String.IsNullOrEmpty(activity.Address) || String.IsNullOrEmpty(activity.Description)))
            {
                (Coordinate, String, String) CoordinateAddressName = await _googleMapsHelper.GetCoordinatesAndAddressFromPlaceId(activity.GooglePlaceId);
                if (String.IsNullOrEmpty(activity.Address))
                {
                    activity.Address = CoordinateAddressName.Item2;
                }
                if (String.IsNullOrEmpty(activity.Description))
                {
                    activity.Description = CoordinateAddressName.Item3;
                }
            }
            _context.Activities.Update(activity);
            await _context.SaveChangesAsync();
        }
    }
}
