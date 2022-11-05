using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BackendAPI.Entities;
using AutoMapper;
using BackendAPI.Repositories;
using BackendAPI.Models.Trip;
using Microsoft.AspNetCore.Identity;
using BackendAPI.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using GoogleApi.Entities.Maps.Directions.Response;
using Microsoft.AspNetCore.Http;
using BackendAPI.Models;
using BackendAPI.Exceptions;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ITripRepository _repository;
        private readonly IGroupRepository _groupRepository;
        private readonly UserManager<User> _userManager;

        public TripsController(IMapper mapper, ITripRepository repository, IGroupRepository groupRepository, UserManager<User> userManager)
        {
            _mapper = mapper;
            _repository = repository;
            _groupRepository = groupRepository;
            _userManager = userManager;
        }
        /// <summary>
        /// Criar uma Viagem
        /// </summary>
        /// <remarks>
        /// Uma viagem tem um grupo associado, uma descrição, nome, data de início, data de fim e se a mesma é, desde o momento da sua criação, privada.
        /// 
        /// Após a viagem ser criada, pode ser associada uma imagem à mesma com o endpoint da Picture.
        /// </remarks>
        /// <param name="tripCreateModel">Modelo JSON com os detalhes da viagem a criar</param>
        /// <returns></returns>
        /// <response code="200">Se a viagem foi criada com sucesso</response>
        /// <response code="400">Se a data de início da viagem for superior à data de fim da viagem</response>
        /// <response code="404">Se o ID do grupo passado não corresponder a nenhum grupo do sistema</response>
        /// <response code="403">Se o utilizador não for gestor do grupo ou administrador do sistema</response>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TripModel>> CreateTrip(TripCreateModel tripCreateModel)
        {
            User current_user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(current_user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Group group = await _groupRepository.GetById(tripCreateModel.GroupId);
            if (group == null)
            {
                return NotFound();
            }
            //verificar se a pessoa a criar a viagem é gestora do grupo
            UserGroupRole role = await _groupRepository.GetUserRole(group, current_user);
            if (!(role == UserGroupRole.MANAGER || is_admin) || role == UserGroupRole.NONE)
            {
                return Forbid();
            }
            Trip trip = _mapper.Map<TripCreateModel, Trip>(tripCreateModel);
            try
            {
                await _repository.Create(trip, group);
                await _repository.AddUser(trip,current_user,null,true);
                return Ok(_mapper.Map<Trip, TripModelAdmin>(trip)); //redireciona depois baseado no ID
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
        /// Obter todas as viagens no sistema
        /// </summary>
        /// <remarks>
        /// Os administradores conseguem ver todos os detalhes de todas as viagens no sistema, incluindo as privadas.
        /// 
        /// Os utilizadores normais não conseguirão ver viagens quee pertençam a grupos privados em que os mesmos não estejam incluídos, ou viagens privadas que os utilizadores não estejam a participar.
        /// 
        /// Pode-se usar este endpoint como um "Descubra as nossas viagens", com uma página que mostra todas as viagens, n de cada vez, estilo resultados de um motor de busca.
        /// 
        /// O utilizador quando aceder a uma viagem (https://viagens-sociais.xyz/trip/1) pode ver todos os detalhes da mesma, tendo em conta que a mesma não é privada e o utilizador não esteja na mesma
        /// ou faça parte de um grupo privado que o utilizador não faça parte, porém apenas se pode juntar à mesma se esta não tiver ainda terminado.
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">Lista de viagens no sistema</response>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TripModel>>> GetAllTrips()
        {
            IEnumerable<Trip> trips = await _repository.GetAll();
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            if (!is_admin)
            {
                trips = trips.Where(t => 
                !(t.Group.IsPrivate && !user.Groups.Any(ug => ug.Group.Id == t.Group.Id)) &&
                !(t.IsPrivate && !user.Trips.Any(ut => ut.Trip.Id == t.Id))).ToList();
                IEnumerable<TripModelSimple> tripsList = from trip in trips select _mapper.Map<Trip, TripModelSimple>(trip);
                return Ok(tripsList);
            }
            else
            {
                IEnumerable<TripModelAdmin> tripsList = from trip in trips select _mapper.Map<Trip, TripModelAdmin>(trip);
                return Ok(tripsList);
            }
        }
        //adicionar endpoint pesquisar viagens
        //estatisticas
        /*
         * top 10 locais mais visitados -> local e número de viagens (grafico barras)
         * transporte mais utilizado -> pie chart (circular)
         * top 10 viagens por distancia (lista)
         * top 10 grupos por distancia total percorrida (lista)
         * distribuicao de rankings -> grafico circular
         * 
         */
        /// <summary>
        /// Pesquisar por uma viagem
        /// </summary>
        /// <remarks>Aplicam-se as mesmas restrições para utilizadores regulares (não administradores) que no endpoint de obter todas as viagens</remarks>
        /// <param name="model">Modelo de pesquisa de viagem, que permite pesquisar por viagens pelo seu nome, descrição ou localizações/destinos da mesma</param>
        /// <returns></returns>
        /// <response code="200">Lista de viagens no sistema</response>
        [Authorize]
        [HttpPost]
        [Route("Search")]
        public async Task<ActionResult<IEnumerable<TripModel>>> SearchForTrip(TripSearchModel model)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            IEnumerable<Trip> trips = await _repository.Search(model);
            if (!is_admin)
            {
                //remover viagens que pertençam a grupos privados em que o utilizador não esteja ou viagens escondidas dentro desse mesmo grupo que o utilizador não pertença
                trips = trips.Where(t =>
                !(t.Group.IsPrivate && !user.Groups.Any(ug => ug.Group.Id == t.Group.Id)) &&
                !(t.IsPrivate && !user.Trips.Any(ut => ut.Trip.Id == t.Id))).ToList();
                IEnumerable<TripModelSimple> tripsList = from trip in trips select _mapper.Map<Trip, TripModelSimple>(trip);
                return Ok(tripsList);
            }
            else
            {
                IEnumerable<TripModelAdmin> tripsList = from trip in trips select _mapper.Map<Trip, TripModelAdmin>(trip);
                return Ok(tripsList);
            }
        }
        /// <summary>
        /// Aderir a uma Viagem
        /// </summary>
        /// <remarks>
        /// Permite a um utilizador juntar-se a uma viagem privada ou pública.
        /// 
        /// Seja o viagem privada ou pública, o utilizador pode passar um convite para se juntar à mesma, porém este é obrigatório se a viagem for privada.
        /// 
        /// O convite é automaticamente removido após o utilizador se juntar à viagem, seja esta privada ou pública, se um convite tiver sido utilizado.
        /// 
        /// Gestores ou Moderadores do grupo ou administradores do sistema não precisam de convite para se juntarem à viagem, mesmo se esta for privada.
        /// </remarks>
        /// <param name="model">Modelo de adesão a uma viagem, com o ID da viagem e o ID do convite, se aplicável</param>
        /// <returns></returns>
        /// <response code="200">Utilizador juntou-se à viagem com sucesso.</response>
        /// <response code="404">Caso não exista uma viagem com o ID especificado.</response>
        /// <response code="403">Caso a viagem seja privada e o utilizador não tenha passado um convite (e o mesmo não for um gestor/moderador do grupo ou administrador do sistema).</response>
        /// <response code="400">
        /// Pode acontecer em várias situações:
        /// 
        /// Caso o utilizador já estar na viagem
        /// 
        /// Caso o convite seja inválido (não é para a viagem ou utilizador em questão)
        /// 
        /// Caso a viagem já esteja completa, pelo que novas pessoas não se podem juntar.
        /// </response>
        [Authorize]
        [HttpPost]
        [Route("Join")]
        public async Task<ActionResult> JoinTrip(TripJoinModel model)
        {
            Trip trip = await _repository.GetById(model.TripId);
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            if (trip == null)
            {
                return NotFound();
            }
            //Moderadores, Administradores e Gestores do Grupo não precisam de convite para entrar numa viagem
            UserGroupRole role = await _groupRepository.GetUserRole(trip.Group, user);
            bool IsManager = (role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR);
            if (!(is_admin || IsManager) && (trip.IsPrivate && model.InviteId == null))
            {
                return Forbid();
            }
            try
            {
                await _repository.AddUser(trip, user,model.InviteId, (is_admin|| IsManager));
                return Ok();
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
        /// Sair de uma Viagem
        /// </summary>
        /// <param name="model">Modelo com o ID da viagem a sair</param>
        /// <returns></returns>
        /// <response code="200">Utilizador saiu da viagem com sucesso.</response>
        /// <response code="404">Caso não exista uma viagem com o ID especificado.</response>
        /// <response code="400">Caso o utilizador não esteja nesta viagem.</response>
        [Authorize]
        [HttpPost]
        [Route("Leave")]
        public async Task<ActionResult> LeaveTrip(TripLeaveModel model)
        {
            Trip trip = await _repository.GetById(model.TripId);
            User user = await _userManager.GetUserAsync(this.User);
            if (trip == null)
            {
                return NotFound();
            }
            try
            {
                await _repository.RemoveUser(trip, user);
                return Ok();
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
        /// Remover um utilizador de uma viagem
        /// </summary>
        /// <param name="model">Modelo com o ID do utilizador e ID da viagem</param>
        /// <returns></returns>
        /// <response code="200">Utilizador foi removido viagem com sucesso.</response>
        /// <response code="404">Caso não exista uma viagem ou utilizador com o ID especificado.</response>
        /// <response code="403">Caso o utilizador que realizou este pedido não seja gestor ou moderador do grupo da viagem ou não seja administrador do sistema.</response>
        /// <response code="400">Caso o utilizador não esteja nesta viagem.</response>
        [Authorize]
        [HttpPost]
        [Route("RemoveUser")]
        public async Task<ActionResult<TripModel>> RemoveUser(TripRemoveUserModel model)
        {
            Trip trip = await _repository.GetById(model.TripId);
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            User user_to_remove = await _userManager.FindByIdAsync(model.UserId);
            if (trip == null || user_to_remove == null)
            {
                return NotFound();
            }

            UserGroupRole role = await _groupRepository.GetUserRole(trip.Group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR))
            {
                return Forbid();
            }
            try
            {
                await _repository.RemoveUser(trip,user_to_remove);
                return Ok();
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
        /// Criar um novo convite para um utilizador para uma viagem
        /// </summary>
        /// <param name="model">Modelo com o ID da viagem e o ID do utilizador</param>
        /// <returns></returns>
        /// <response code="200">Utilizador foi convidado para a viagem com sucesso.</response>
        /// <response code="404">Caso não exista uma viagem ou utilizador com o ID especificado.</response>
        /// <response code="403">Caso o utilizador que realizou este pedido não seja gestor ou moderador do grupo da viagem ou não seja administrador do sistema.</response>
        /// <response code="400">
        /// Pode acontecer por vários motivos:
        /// 
        /// Caso o utilizador já tenha sido convidado para esta nesta viagem.
        /// 
        /// Caso o utilizador já esteja nela.
        /// 
        /// Caso a viagem já esteja completa, pelo que novas pessoas não se podem juntar.
        /// 
        /// Caso o utilizador não esteja no grupo da viagem.
        /// </response>
        [Authorize]
        [HttpPost("Invitations")]
        public async Task<ActionResult> SendInvite(TripSendInviteModel model)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Trip trip = await _repository.GetById(model.TripId);
            User user_to_add = await _userManager.FindByIdAsync(model.UserId);
            if (trip == null || user_to_add == null)
            {
                return NotFound();
            }
            UserGroupRole role = await _groupRepository.GetUserRole(trip.Group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR))
            {
                return Forbid();
            }
            try
            {
                await _repository.InviteUser(new TripInvite { Trip = trip, User = user_to_add, InvitationDate = DateTime.Now });
                return Ok();
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
        /// Remover o convite de uma viagem
        /// </summary>
        /// <param name="Id">ID do convite</param>
        /// <returns></returns>
        /// <response code="200">O convite do utilizador para a viagem foi removido com sucesso.</response>
        /// <response code="404">Caso não exista um convite com o ID especificado.</response>
        /// <response code="403">Caso o utilizador que realizou este pedido não seja gestor ou moderador do grupo da viagem a que o convite se refere ou não seja administrador do sistema.</response>
        [Authorize]
        [HttpDelete("Invitations/{id}")]
        public async Task<ActionResult> DeleteInvite(Guid Id)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            TripInvite invite = await _repository.GetTripInviteById(Id);
            if (invite == null)
            {
                return NotFound();
            }
            UserGroupRole role = await _groupRepository.GetUserRole(invite.Trip.Group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR || user == invite.User))
            {
                return Forbid();
            }
            await _repository.RemoveInvite(invite);
            return Ok();
        }
        /*
         * Obtém os detalhes de uma viagem. Serve para pessoas que ainda não estão ou já estão na viagem obterem detalhes na viagem e eventualmente juntarem-se à mesma se esta não estiver terminada
         * Tem a lista de utilizadores do grupo associado ao grupo da viagem. Deste modo, o frontend consegue obter o role do user atual em relação ao grupo (regular, moderador ou gestor)
         * Um moderador faz tudo o que um gestor faz excepto atualizar detalhes da viagem (atributos ou modificar itinerário)
         */
        /// <summary>
        /// Obter os Detalhes de uma Viagem
        /// </summary>
        /// <remarks>
        /// O utilizador quando aceder a uma viagem (https://viagens-sociais.xyz/trip/1) pode ver todos os detalhes da mesma (itinerário, publicações não escondidas),tendo em conta que a mesma não é privada e
        /// o utilizador não esteja na mesma ou faça parte de um grupo privado que o utilizador não faça parte, porém apenas se pode juntar à mesma se esta não tiver ainda terminado.
        /// 
        /// Tem a opção para se juntar (no frontend) caso o IsCompleted seja falso. Quando a viagem é dada como completa, todos os convites são automaticamente eliminados.
        /// 
        /// Para o utilizador se juntar, caso se pretenda juntar, mas não esteja já no grupo a que ela pertence, deve ser deparado com um pedido para se juntar ao grupo.
        /// 
        /// O modelo JSON devolvido da viagem diverge de utilizador para gestor/administrador/moderador. No frontend sabe-se se o utilizador actual é um gestor/moderador do grupo ou administrador do sistema a partir
        /// da existência de certos atributos como os convites ou registos da viagem. A partir desses atributos podem-se mostrar menus para esses utilizadores habitualmente restrictos como gestão de utilizadores, gestão do
        /// itinerário, entre outros.
        /// </remarks>
        /// <param name="Id">ID da Viagem</param>
        /// <returns></returns>
        /// <response code="200">O convite do utilizador para a viagem foi removido com sucesso.</response>
        /// <response code="404">Caso não exista uma viagem com o ID especificado.</response>
        /// <response code="403">Caso a viagem seja privada e o utilizador não esteja nela ou o grupo seja privado e o utilizador não esteja nele</response>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<TripModel>> GetTrip(int Id)
        {
            Trip trip = await _repository.GetById(Id);
            if (trip == null)
            {
                return NotFound();
            }
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            UserGroupRole role = await _groupRepository.GetUserRole(trip.Group, user);
            if (is_admin || role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR)
            {
                return Ok(_mapper.Map<Trip, TripModelAdmin>(trip));
            }
            //Se a viagem for privada e o utilizador não estiver na mesma, ou o grupo da viagem for privado e o utilizador não estiver nele
            if ((trip.IsPrivate && !user.Trips.Any(ut => ut.Trip.Id == trip.Id)) || (trip.Group.IsPrivate && role == UserGroupRole.NONE))
            {
                return Forbid();
            }
            //Remover posts escondidos para utilizadores regulares
            trip.Posts = trip.Posts.Where(tp => !tp.IsHidden).ToList();
            return _mapper.Map<Trip, TripModel>(trip);
        }
        /// <summary>
        /// Actualizar os detalhes de uma viagem
        /// </summary>
        /// <remarks>
        /// Permite actualizar os detalhes de uma viagem.
        /// 
        /// Entre estes, os tipos elementares são o seu nome, descrição, data de início e fim.
        /// 
        /// As datas são alteráveis desde que a sua alteração não afecte actividades do itinerário existente da viagem, a data de início não ultrapasse a data de fim (e vice-versa) ou alguma das datas seja inferior à data actual.
        /// 
        /// Pode-se definir a viagem como privada com o IsPrivate e remover a fotografia da viagem sem substituição com o RemovePicture.
        /// 
        /// Para completar uma viagem, irreversivelmente, define-se o IsCompleted como true.
        /// </remarks>
        /// <param name="id">ID da viagem</param>
        /// <param name="model">Modelo com os detalhes a actualizar da viagem, nomeadamente o seu nome, descrição, data de início e fim</param>
        /// <returns></returns>
        /// <response code="200">Os detalhes da viagem foram alterados com sucesso.</response>
        /// <response code="404">Caso não exista uma viagem com o ID especificado.</response>
        /// <response code="403">Caso o utilizador não seja gestor do grupo da viagem ou administrador do sistema</response>
        /// <response code="400">
        /// Pode acontecer por vários motivos:
        /// 
        /// Se existirem actividades antes da nova data de início ou depois da nova data de fim.
        /// 
        /// Se a nova data de início for superior à data da nova data de fim
        /// 
        /// Se a nova data de início ou fim for inferior à data actual (e a viagem não estiver terminada)
        /// </response>

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTripDetails(int id, TripDetailsUpdateModel model)
        {
            Trip trip = await _repository.GetById(id);
            if (trip == null)
            {
                return NotFound();
            }
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            UserGroupRole role = await _groupRepository.GetUserRole(trip.Group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER))
            {
                return Forbid();
            }
            try
            {
                await _repository.Update(trip, model);
                return Ok();
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
        /// "Eliminar" uma viagem
        /// </summary>
        /// <remarks>
        /// Não elimina realmente uma viagem, antes remove todos os utilizadores da mesma, remove todos os convites, e define todas as publicações como escondidas.
        /// </remarks>
        /// <param name="Id">ID da viagem a "eliminar"</param>
        /// <returns></returns>
        /// <response code="200">A viagem foi "eliminada" com sucesso.</response>
        /// <response code="404">Caso não exista uma viagem com o ID especificado.</response>
        /// <response code="403">Caso o utilizador não seja gestor do grupo da viagem ou administrador do sistema</response>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(int Id)
        {
            Trip trip = await _repository.GetById(Id);
            if (trip == null)
            {
                return NotFound();
            }
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            UserGroupRole role = await _groupRepository.GetUserRole(trip.Group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER))
            {
                return Forbid();
            }
            await _repository.Delete(trip);
            return Ok();
        }
        /// <summary>
        /// Actualizar ou Criar a Imagem da Viagem
        /// </summary>
        /// <param name="tripId">ID da Viagem</param>
        /// <param name="file">Imagem JPG, PNG, JPEG ou GIF da viagem</param>
        /// <returns></returns>
        /// <response code="200">A imagem da viagem foi criada ou actualizada com sucesso.</response>
        /// <response code="404">Caso não exista uma viagem com o ID especificado.</response>
        /// <response code="403">Caso o utilizador não seja gestor do grupo da viagem ou administrador do sistema</response>
        /// <response code="400">Caso tenha havido um erro ao inserir ou actualizar a imagem na Google Cloud Storage</response>
        [Authorize]
        [HttpPost("{tripId}/Picture")]
        public async Task<ActionResult> UpdateTripPicture(int tripId, [FromForm] IFormFile file)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Trip trip = await _repository.GetById(tripId);
            if (trip == null)
            {
                return NotFound();
            }
            UserGroupRole role = await _groupRepository.GetUserRole(trip.Group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER))
            {
                return Forbid();
            }
            try
            {
                await _repository.UpdateImage(trip, file);
                return Ok();
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
        [Authorize]
        [HttpDelete("{tripId}/Picture")]
        public async Task<ActionResult> DeleteTripPicture(int tripId)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Trip trip = await _repository.GetById(tripId);
            if (trip == null)
            {
                return NotFound();
            }
            UserGroupRole role = await _groupRepository.GetUserRole(trip.Group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER))
            {
                return Forbid();
            }
            try
            {
                await _repository.RemoveImage(trip);
                return Ok();
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
