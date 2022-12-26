using AutoMapper;
using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Exceptions;
using BackendAPI.Helpers;
using BackendAPI.Models;
using BackendAPI.Models.Activity;
using BackendAPI.Models.Trip;
using BackendAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ITripRepository _tripRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IGoogleMapsHelper _googleMapsHelper;
        private readonly UserManager<User> _userManager;

        public ActivityController(IMapper mapper, ITripRepository tripRepository, IActivityRepository activityRepository, IGroupRepository groupRepository, IGoogleMapsHelper googleMapsHelper, UserManager<User> userManager)
        {
            _mapper = mapper;
            _tripRepository = tripRepository;
            _activityRepository = activityRepository;
            _groupRepository = groupRepository;
            _googleMapsHelper = googleMapsHelper;
            _userManager = userManager;
        }
        /// <summary>
        /// Criar uma Actividade no itinerário diário de uma Viagem
        /// </summary>
        /// <remarks>
        /// Cada viagem tem um itinerário. Este itinerário é dividido em sub-itinerários para cada dia da viagem onde haja actividades.
        /// 
        /// Cada itinerário diário é composto pelas suas respectivas actividades. O dia começa com uma actividade inicial, que normalmente costuma ser a reunião do grupo, apesar de não ser necessariamente sempre este o caso.
        /// 
        /// Sucedendo esta actividade inicial, ocorre a próxima actividade, porém, a mesma não será (necessariamente) na mesma localização que a actividade inicial,
        /// logo é necessário um transporte para os viajantes irem da actividade A para a B.
        /// 
        /// Este transporte, no contexto de viagens, é normalmente da natureza automóvel, pedonal, urbana (autocarro, comboio, metro) ou ciclopedonal.
        /// É também possível definir outro tipo de transporte, como por exemplo, funicular.
        /// 
        /// Cada actividade que não seja de transporte tem um tipo associado, entre estes, de alojamento (descansar ou esperar em algum sítio), visita (para alguma localização ou sítio),
        /// comida (para refeições em restaurantes ou bares), shopping (para compras) e excursão (para excursões).
        /// 
        /// Cada actividade tem necessariamente uma data de início e de fim, até mesmo a última actividade do dia, para haver consistência no itinerário. O transporte pode ser visto como uma actividade,
        /// logo tem também uma data de início e fim.
        /// 
        /// Cada actividade tem também necessariamente um custo associado, seja a taxa de admissão para o museu, o preço para almoçar, o custo estimado do bilhete de comboio
        /// ou até o custo estimado de lembranças num centro comercial.
        /// 
        /// É também possível definir a morada da actividade, seja esta de transporte, como o ponto inicial do transporte (como a morada da estação) ou da actividade em si, como a morada do sítio,
        /// porém para actividades normais tal não é recomendado devido a esta ser automaticamente preenchida após seleccionar no mapa
        /// 
        /// Então, para a primeira actividade do dia, iremos passar um modelo apenas com uma actividade normal, e com a de transporte vazia, e para as sucessivas, iremos passar a actividade de transporte,
        /// que conecta a anterior, à próxima a inserir.
        /// 
        /// Exemplo: Actividade A é a primeira actividade do dia. Começa às 10:00 e acaba às 10:30. Actividade B é a segunda actividade do dia, e espera-se que tenha o seu início às 11:00.
        /// Logo, é preciso uma actividade de transporte entre as actividades A e B, de modo a que no espaço de tempo entre as 10:30 e 11:00, os viajantes consigam chegar a B.
        /// 
        /// Para a Actividade A (nota-se que a morada está em branco - a mesma será automaticamente preenchida pelo sistema a partir do Place ID):
        ///     
        ///     {
        ///         "tripId": 3,
        ///         "activityCreate": {
        ///             "beginningDate": "2022-02-01T10:00:00",
        ///             "endingDate": "2022-02-01T10:30:00",
        ///             "address": "",
        ///             "description": "Actividade A",
        ///             "googlePlaceId": "ChIJmbuF-AxlJA0RR_v3I2ibx04",
        ///             "expectedBudget": 10,
        ///             "activityType": 2,
        ///             "transportType": 5
        ///         },
        ///         "activityTransport": null
        ///     }
        /// 
        /// Para a Actividade B:
        /// 
        ///     {
        ///         "tripId": 3,
        ///         "activityCreate": {
        ///             "beginningDate": "2022-02-11:00:00",
        ///             "endingDate": "2022-02-01T13:30:00",
        ///             "address": "",
        ///             "description": "Actividade B",
        ///             "googlePlaceId": "ChIJmbuF-AxlJA0RR_v3I2ibx04",
        ///             "expectedBudget": 20,
        ///             "activityType": 5,
        ///             "transportType": 5
        ///         },
        ///         "activityTransport": {
        ///             "beginningDate": "2022-02-01T10:30:00",
        ///             "endingDate": "2022-02-01T10:55:00",
        ///             "address": "Central de Autocarros da Cidade X",
        ///             "description": "Ida de Autocarro até à Actividade B",
        ///             "googlePlaceId": "",
        ///             "expectedBudget": 5,
        ///             "activityType": 1,
        ///             "transportType": 3
        ///         }
        ///     }
        ///     
        /// Nota-se que se a descrição da actividade de transporte for omitida, a mesma irá assumir direcções completas do Google Maps dependendo do tipo de transporte escolhido. 
        /// 
        /// Adicionar uma actividade ao itinerároo irá necessariamente recalcular o custo e distância totais da viagem.
        /// </remarks>
        /// 
        /// <param name="model">Modelo que contém a actividade em si e a actividade de transporte que conecta a actividade anterior (se aplicável) a esta a adicionar ao itinerário,
        /// assim como o ID da viagem que se pretende adicionar esta actividade</param>
        /// <returns></returns>
        /// <response code="200">Adição da actividade ao itinerário do dia bem-sucedida</response>
        /// <response code="404">Caso não exista uma viagem com o ID especificado no modelo</response>
        /// <response code="403">Caso o utilizador não seja gestor do grupo a que a viagem pertence ou administrador do sistema</response>
        /// <response code="400">
        /// Pode acontecer por vários motivos:
        /// 
        /// Tentar adicionar uma actividade a uma viagem completa.
        /// 
        /// As datas de início e fim da actividade em si ou de transporte não são no mesmo dia.
        /// 
        /// A data de início da actividade ou do transporte é maior do que a respectiva data de fim.
        /// 
        /// A actividade de transporte ocorre previamente à actividade em si.
        /// 
        /// O transporte não foi especificado no itinerário de um dia com actividades já existentes.
        /// 
        /// A actividade a criar tem um Place ID que não corresponde a um sítio real para o API do Google Maps.
        /// 
        /// A actividade a adicionar é igual à anterior no itinerário do mesmo dia.
        /// 
        /// A data da actividade não está entre a data de início e fim da viagem em si.
        /// 
        /// A data de início e fim da actividade, assim como o gasto estimado da mesma ou do transporte não foram especificados.
        /// 
        /// Limitações de API de Google Maps.
        /// 
        /// ID de tipo de transporte ou actividade inválidos.
        /// 
        /// Actividade ocorre antes de uma existente (novas actividades têm se necessariamente ser colocadas no final do itinerário diário)
        /// </response>

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TripModelAdmin>> CreateActivity(ActivityCreateModel model)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Trip trip = await _tripRepository.GetById(model.TripId);
            if (trip == null)
            {
                return NotFound();
            }
            UserGroupRole role = await _groupRepository.GetUserRole(trip.Group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER))
            {
                return Forbid();
            }
            Activity activity = _mapper.Map<ActivityCreateModelInvidual, Activity>(model.ActivityCreate);
            Activity transport = _mapper.Map<ActivityCreateModelInvidual, Activity>(model.ActivityTransport);
            try
            {
                await _activityRepository.AddActivity(trip, activity, transport);
                await _tripRepository.RecalculateTripDistanceAndBudget(trip);
                return Ok(_mapper.Map<Trip, TripModelAdmin>(trip));
            }
            catch (CustomException exception)
            {
                return BadRequest(new ErrorModel() { ErrorType = exception.ErrorType, Message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.OTHER, Message = ex.Message });
            }
        }
        /// <summary>
        /// Actualizar uma actividade no itinerário diário de uma viagem
        /// </summary>
        /// <remarks>
        /// Permite actualizar uma actividade no itinerário diário de uma viagem.
        /// 
        /// É útil caso se pretenda realizar correcções superficiais a uma actividade como a sua descrição ou custo estimado, ou duração estimada, ajustando a data de início ou fim.
        /// 
        /// Com isto, é também possível reajustar o itinerário ajustando as datas de início e fim de todas as actividades e transportes entre as mesmas.
        /// 
        /// É também possível recalcular as actividades de transporte, possivelmente obtendo uma rota mais optimizada,
        /// chamando o método responsável por obter estimativas de opções de transporte com a nova data de fim da actividade prévia ou nova data de início da actividade seguinte,
        /// obtendo um transporte com menos constrangimentos temporais.
        /// 
        /// Modificar a actividade irá necessariamente recalcular o custo total estimado da viagem.
        /// </remarks>
        /// <param name="id">ID da actividade</param>
        /// <param name="model">Modelo de actualização da actividade, com a nova data de início, fim, morada, descrição, gasto estimado, tipo de actividade e tipo de transporte, se aplicável</param>
        /// <returns></returns>
        /// <response code="200">Modificação da actividade ao itinerário do dia bem-sucedida</response>
        /// <response code="404">Caso não exista uma actividade com o ID especificado no parâmetro do URL</response>
        /// <response code="403">Caso o utilizador não seja gestor do grupo a que a viagem pertence ou administrador do sistema</response>
        /// <response code="400">Pode acontecer por vários motivos:
        /// 
        /// Tentar modificar uma actividade de uma viagem já completa.
        /// 
        /// As datas de início e fim da actividade em si ou de transporte não são no mesmo dia.
        /// 
        /// A data de início da actividade ou do transporte é maior do que a respectiva data de fim.
        /// 
        /// O transporte não foi especificado no itinerário de um dia com actividades já existentes.
        /// 
        /// A data da actividade não está entre a data de início e fim da viagem em si.
        /// 
        /// Novo dia da data de início ou fim da actividade é diferente do dia actual da actividade
        /// 
        /// Nova data de início ou fim da actividade causam sobreposição com actividades existentes
        /// 
        /// Limitações de API de Google Maps.
        /// 
        /// ID de tipo de transporte ou actividade inválidos.
        /// </response>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<TripModelAdmin>> UpdateActivity(int id, ActivityUpdateModel model)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Activity activity = await _activityRepository.GetById(id);
            if (activity == null)
            {
                return NotFound();
            }
            Trip trip = activity.Trip;
            UserGroupRole role = await _groupRepository.GetUserRole(trip.Group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER))
            {
                return Forbid();
            }
            try
            {
                await _activityRepository.UpdateActivity(activity, model);
                await _tripRepository.RecalculateTripDistanceAndBudget(trip);
                return Ok(_mapper.Map<Trip, TripModelAdmin>(trip));
            }
            catch (CustomException exception)
            {
                return BadRequest(new ErrorModel() { ErrorType = exception.ErrorType, Message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.OTHER, Message = ex.Message });
            }
        }
        /// <summary>
        /// Remover uma actividade do itinerário diário de uma viagem
        /// </summary>
        /// <remarks>
        /// Remove uma actividade do itinerário diário da viagem, assumindo que a mesma está no final desse mesmo itinerário diário, juntamente com a actividade de transporte que a conecta à actividade anterior.
        /// </remarks>
        /// <param name="id">ID da Actividade</param>
        /// <returns></returns>
        /// <response code="200">Remoção da actividade do itinerário do dia bem-sucedida</response>
        /// <response code="404">Caso não exista uma actividade com o ID especificado no parâmetro do URL</response>
        /// <response code="403">Caso o utilizador não seja gestor do grupo a que a viagem pertence ou administrador do sistema</response>
        /// <response code="400">Pode acontecer por vários motivos:
        /// 
        /// Tentar remover uma actividade do itinerário de uma viagem já completa.
        /// 
        /// Tentar remover uma actividade do itinerário diário de uma viagem, que não é a última actividade desse itinerário, ou seja, que tem actividades que a sucedem.
        /// 
        /// Tentar remover uma actividade de transporte.
        /// </response>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<TripModelAdmin>> DeleteActivity(int id)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Activity activity = await _activityRepository.GetById(id);
            if (activity == null)
            {
                return NotFound();
            }
            Trip trip = activity.Trip;
            UserGroupRole role = await _groupRepository.GetUserRole(trip.Group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER))
            {
                return Forbid();
            }
            try
            {
                await _activityRepository.RemoveActivity(trip, activity);
                await _tripRepository.RecalculateTripDistanceAndBudget(trip);
                return Ok(_mapper.Map<Trip, TripModelAdmin>(trip));
            }
            catch (CustomException exception)
            {
                return BadRequest(new ErrorModel() { ErrorType = exception.ErrorType, Message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.OTHER, Message = ex.Message });
            }
        }
        /// <summary>
        /// Pesquisar por transportes entre duas localizações (actividades)
        /// </summary>
        /// <remarks>
        /// Pesquisa o transporte entre duas actividades. Uma actividade é caracterizada pelo seu place ID, que corresponde a uma localização no google maps.
        /// 
        /// Transmite-se com estes Place IDs a data de partida pretendida, normalmente sendo imediatamente após a data de fim da actividade anterior.
        /// 
        /// Os tipos de transporte considerados são a pé, bicicleta, transportes públicos (autocarro, metro, comboio) ou a carro.
        /// 
        /// Retorna a distância, data de partida e chegada para cada um destes transportes, juntamente com as instruções de transporte.
        /// 
        /// A data de partida difere da data de partida pretendida apenas em transportes públicos.
        /// 
        /// Preferencialmente, a data de início da próxima actividade será definida pela data de fim da actividade de transporte escolhida. É de notar que as actividades de transporte retornadas são meramente
        /// sugestivas, sendo que o utilizador pode escolher criar uma actividade de transporte que o transporta 400 km que começa às 10:00 e acaba às 10:05, este endpoint sendo providenciado para providenciar exactidão ao itinerário.
        /// </remarks>
        /// <param name="model">Modelo que contém os Place ID das duas actividades e data de partida pretendida. Contém também o código ISO de 2 caracteres do país para ajustar a linguagem das instruções retornadas</param>
        /// <returns></returns>
        /// <response code="200">Modelo com uma lista com os 4 transportes possíveis, com o tipo de transporte (bicicleta, a pé, transportes públicos ou de carro), a descrição (instruções de transporte com detalhes acerca
        /// de linhas de transporte público, se aplicável), a distância em km a percorrer, a data de partida (que muda se for por transporte público), e a data de chegada prevista.</response>
        /// <response code="400">Limite de API de Google Maps</response>
        [Authorize]
        [HttpPost("SearchTransport")]
        public async Task<ActionResult<IEnumerable<ActivityTransportModel>>> SearchTransport(ActivitySearchTransportModel model)
        {
            try
            {
                IEnumerable<ActivityTransportModel> result = await _googleMapsHelper.GetAllTransporationMethods(model);
                return Ok(result);
            }
            catch (CustomException exception)
            {
                return BadRequest(new ErrorModel() { ErrorType = exception.ErrorType, Message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.OTHER, Message = ex.Message });
            }

        }
    }
}
