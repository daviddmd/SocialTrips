using AutoMapper;
using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Exceptions;
using BackendAPI.Models;
using BackendAPI.Models.Group;
using BackendAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IGroupRepository _repository;
        private readonly UserManager<User> _userManager;
        public GroupsController(IGroupRepository repository, IMapper mapper, UserManager<User> userManager)
        {
            _mapper = mapper;
            _repository = repository;
            _userManager = userManager;
        }
        /// <summary>
        /// Obter todos os Grupos do Sistema
        /// </summary>
        /// <remarks>
        /// Responsável por Obter todos os grupos no sistema.
        /// 
        /// Está disponível a ambos os administradores do sistema e Utilizadores Regulares.
        /// 
        /// Os Administradores do Sistema têm direito a ver todos os detalhes de todos os grupos do sistema como se fossem gestores dos mesmos.
        /// 
        /// Os utilizadores normais apenas podem ver os detalhes essenciais dos grupos que não estejam escondidos ou que estejam escondidos e os mesmos presentes neles, não contendo nem as viagens nem as publicações nas mesmas.
        /// 
        /// O propósito disto é para navegar numa lista de grupos de modo ao utilizador ver todos os grupos que o sistema tem disponíveis
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">Lista de Grupos no Sistema consoante as restrições previamente definidas</response>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupModelSimple>>> GetAllGroups()
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            IEnumerable<Group> groups = await _repository.GetAll();
            if (!is_admin)
            {
                groups = groups.Where(g => !(g.IsPrivate && !user.Groups.Any(ug => ug.Group.Id == g.Id)));
                IEnumerable<GroupModelSimple> groups_ret = from g in groups select _mapper.Map<Group, GroupModelSimple>(g);
                return Ok(groups_ret);
            }
            else
            {
                IEnumerable<GroupModelAdmin> groups_ret = from g in groups select _mapper.Map<Group, GroupModelAdmin>(g);
                return Ok(groups_ret);
            }
        }
        /// <summary>
        /// Obter os detalhes de um Grupo
        /// </summary>
        /// <remarks>
        /// Obtém a lista de viagens de um grupo, o seu nome, descrição, utilizadores, data de criação e outras informações úteis.
        /// 
        /// Para os gestores do grupo, são incluídas também informações sobre a lista de utilizadores proíbídos de se juntarem, a lista de convites e o registo de actividades do grupo.
        /// </remarks>
        /// <param name="id">ID do Grupo</param>
        /// <returns></returns>
        /// <response code="200">Detalhes do grupo para o utilizador e detalhes mais aprofundados para o gestor/moderador do mesmo (ou um administrador do sistema)</response>
        /// <response code="404">Caso não exista um grupo com o ID correspondente</response>
        /// <response code="403">Caso o grupo seja privado e o utilizador não esteja no mesmo</response>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<GroupModel>> GetGroupById(int id)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Group group = await _repository.GetById(id);
            if (group == null)
            {
                return NotFound();
            }
            UserGroupRole role = await _repository.GetUserRole(group, user);
            if (is_admin || role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR)
            {
                return Ok(_mapper.Map<Group, GroupModelAdmin>(group));
            }
            else if (role == UserGroupRole.REGULAR || (role == UserGroupRole.NONE && !group.IsPrivate))
            {
                group.Trips = group.Trips.Where(gt => !(gt.IsPrivate && !user.Trips.Any(ut => ut.Trip.Id == gt.Id))).ToList();
                return Ok(_mapper.Map<Group, GroupModel>(group));
            }
            return Forbid();
        }
        /// <summary>
        /// Atualizar os detalhes de um grupo
        /// </summary>
        /// <remarks>
        /// Permite atualizar os detalhes do grupo, nomeadamente, o seu nome, descrição e se o mesmo é privado.
        /// 
        /// Para um administrador do sistema, é possível definir se o mesmo é ou não destacado na comunidade, o que irá levar o mesmo a ser mais recomendado aos utilizadores do sistema.
        /// 
        /// Permite também remover a fotografia do grupo, sem adicionar uma nova.
        /// </remarks>
        /// <param name="id">ID do grupo a atualizar detalhes</param>
        /// <param name="groupDetailsUpdateModel">Modelo que contém os detalhes relevantes para atualização no grupo</param>
        /// <returns></returns>
        /// <response code="200">Atualização do grupo bem-sucedida</response>
        /// <response code="404">Caso não exista um grupo com o ID correspondente</response>
        /// <response code="403">Caso o utilizador não seja gestor do grupo ou administrador do sistema</response>
        /// <response code="400">Erro na atualização dos detalhes do grupo</response>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateGroupDetails(int id, GroupDetailsUpdateModel groupDetailsUpdateModel)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Group group = await _repository.GetById(id);
            if (group == null)
            {
                return NotFound();
            }
            UserGroupRole role = await _repository.GetUserRole(group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER))
            {
                return Forbid();
            }
            try
            {
                await _repository.Update(group, groupDetailsUpdateModel, is_admin);
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
        /// Criar um Convite para um Utilizador se juntar a um Grupo
        /// </summary>
        /// <remarks>
        /// Cria um convite com um ID único que permite a um utilizador se juntar a um grupo específico.
        /// 
        /// Apenas gestores ou moderadores do grupo em questão, ou administradores do sistema poderão gerar estes convites, e o utilizador terá opção de os aceitar ou rejeitar.
        /// </remarks>
        /// <param name="model">Modelo com o ID do grupo e utilizador para criar um convite</param>
        /// <returns></returns>
        /// <response code="200">Criação do convite bem-sucedida.</response>
        /// <response code="404">Caso não exista um grupo ou utilizador com os IDs passados no modelo.</response>
        /// <response code="403">Caso o utilizador não seja gestor/moderador do grupo ou administrador do sistema.</response>
        /// <response code="400">Caso o utilizador já tenha sido convidado para o grupo (convite existente) ou o mesmo já esteja presente no grupo.</response>
        [Authorize]
        [HttpPost("Invitations")]
        public async Task<ActionResult> SendInvite(GroupSendInviteModel model)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Group group = await _repository.GetById(model.GroupId);
            User user_to_add = await _userManager.FindByIdAsync(model.UserId);
            if (group == null || user_to_add == null)
            {
                return NotFound();
            }
            UserGroupRole role = await _repository.GetUserRole(group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR))
            {
                return Forbid();
            }
            GroupInvite invite = new() { Group = group, User = user_to_add, InvitationDate = DateTime.Now };
            try
            {
                await _repository.InviteUser(invite);
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
        /// Remover um Convite de um Grupo
        /// </summary>
        /// <param name="Id">ID único do convite a remover</param>
        /// <returns></returns>
        /// <response code="200">Remoção do convite bem-sucedida.</response>
        /// <response code="404">Caso não um convite com o ID especificado.</response>
        /// <response code="403">Caso o utilizador não seja gestor/moderador do grupo ou administrador do sistema, ou o próprio dono do convite.</response>
        /// <response code="400">Erro na remoção do convite.</response>
        [Authorize]
        [HttpDelete("Invitations/{id}")]
        public async Task<ActionResult> DeleteInvite(Guid Id)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            GroupInvite invite = await _repository.GetGroupInviteById(Id);
            if (invite == null)
            {
                return NotFound();
            }
            //apenas o próprio utilizador, admins, gestores e moderadores de grupo conseguem remover um convite
            UserGroupRole role = await _repository.GetUserRole(invite.Group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR || user == invite.User))
            {
                return Forbid();
            }
            try
            {
                await _repository.RemoveInvite(invite);
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
        /// Criar um Grupo
        /// </summary>
        /// <remarks>
        /// Cria um grupo com os detalhes especificados no modelo de criação do Grupo.
        /// 
        /// O utilizador que cria o grupo torna-se automaticamente gestor do mesmo, e não pode sair até existir pelo menos outro gestor (ou eliminar o grupo)
        /// 
        /// É possível tornar o grupo privado incialmente, o que faz com que seja necessário um convite para entrar no mesmo.
        /// </remarks>
        /// <param name="groupCreateModel">Modelo com os detalhes iniciais de um grupo, nomeadamente, o seu nome, descrição, e se o mesmo deve ser inicialmente privado (onde para um utilizador se juntar precisa de um convite)</param>
        /// <returns></returns>
        /// <response code="200">Criação do Grupo bem-sucedida.</response>
        /// <response code="400">Erro na criação do grupo.</response>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<GroupModelAdmin>> CreateGroup(GroupCreateModel groupCreateModel)
        {
            User user = await _userManager.GetUserAsync(this.User);
            Group group = _mapper.Map<GroupCreateModel, Group>(groupCreateModel);
            try
            {
                await _repository.Create(group);
                await _repository.AddUser(group, user, null, true);
                return Ok(_mapper.Map<Group, GroupModelAdmin>(group));
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
        /// Actualizar as permissões/papel de um Utilizador num Grupo
        /// </summary>
        /// <param name="model">Modelo de atualização do papel de um utilizador num grupo com o ID do utilizador, o ID do Grupo e o ID do papel a atribuir.</param>
        /// <returns></returns>
        /// <response code="200">Atualização do papel do utilizador no grupo bem-sucedida.</response>
        /// <response code="404">Caso não exista um grupo ou utilizador com os IDs especificados.</response>
        /// <response code="403">Caso o utilizador não seja gestor do grupo ou administrador do sistema.</response>
        /// <response code="400">Caso o papel definido seja inválido, ou o utilizador não esteja no grupo.</response>
        [Authorize]
        [HttpPost]
        [Route("UpdateUserRole")]
        public async Task<ActionResult> UpdateUserRole(GroupUpdateUserRoleModel model)
        {
            User current_user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(current_user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Group group = await _repository.GetById(model.GroupId);
            User user_to_modify = await _userManager.FindByIdAsync(model.UserId);
            if (group == null || user_to_modify == null)
            {
                return NotFound();
            }
            //Apenas Gestores ou Administradores podem modificar roles de outros utilizadores
            UserGroupRole role = await _repository.GetUserRole(group, current_user);
            if (!(role == UserGroupRole.MANAGER || is_admin))
            {
                return Forbid();
            }
            if (user_to_modify == current_user)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.GROUP_MANAGER_MANAGE_ITSELF, Message = "A manager can't modify their own roles." });
            }
            try
            {
                await _repository.UpdateUserRole(group, user_to_modify, model.UserGroupRole);
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
        /// Remover um Utilizador de um Grupo
        /// </summary>
        /// <remarks>
        /// Difere de sair de um grupo de "livre vontade".
        /// 
        /// O mesmo diferee da proibição/ban, apesar do mesmo remover o utilizador do grupo.
        /// 
        /// Apenas gestores ou moderadores do grupo, ou administradores do sistema podem remover utilizadores de um grupo
        /// </remarks>
        /// <param name="model">Modelo de Remoção do Utilizador de um Grupo com o ID do grupo e o ID do utilizador</param>
        /// <returns></returns>
        /// <response code="200">Remoção do Utilizador do Grupo bem-sucedida.</response>
        /// <response code="404">Caso não exista um grupo ou utilizador com os IDs especificados.</response>
        /// <response code="403">Caso o utilizador não seja gestor do grupo ou administrador do sistema.</response>
        /// <response code="400">Caso o gestor se esteja a tentar a remover a ele mesmo, o utilizador em questão não esteja no grupo, ou o grupo apenas tenha um gestor restante</response>
        [Authorize]
        [HttpPost]
        [Route("RemoveUser")]
        public async Task<ActionResult> RemoveUser(GroupRemoveUserModel model)
        {
            User current_user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(current_user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Group group = await _repository.GetById(model.GroupId);
            User user_to_remove = await _userManager.FindByIdAsync(model.UserId);
            if (group == null || user_to_remove == null)
            {
                return NotFound();
            }
            UserGroupRole role = await _repository.GetUserRole(group, current_user);
            if (!(role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR || is_admin))
            {
                return Forbid();
            }
            if (user_to_remove == current_user)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.GROUP_MANAGER_MANAGE_ITSELF, Message = "A manager can't remove himself, ask another one to remove you." });
            }
            try
            {
                await _repository.RemoveUser(group, user_to_remove);
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
        /// Aderir a um Grupo
        /// </summary>
        /// <remarks>
        /// Permite a um utilizador juntar-se a um grupo privado ou público.
        /// 
        /// Seja o grupo privado ou público, o utilizador pode passar um convite para se juntar ao mesmo, porém este é obrigatório se o grupo for privado.
        /// 
        /// O convite é automaticamente removido após o utilizador se juntar ao grupo.
        /// </remarks>
        /// <param name="model">Modelo de Juntar a um grupo, com o ID do grupo e ID do convite, se aplicável</param>
        /// <returns></returns>
        /// <response code="200">Utilizador juntou-se ao grupo com sucesso.</response>
        /// <response code="404">Caso não exista um grupo com o ID especificado.</response>
        /// <response code="403">Caso o grupo seja privado e o utilizador não tenha passado um convite.</response>
        /// <response code="400">Caso o utilizador esteja proibido de se juntar ao grupo, o convite for inválido ou o utilizador já estar no grupo.</response>
        [Authorize]
        [HttpPost]
        [Route("Join")]
        public async Task<ActionResult> JoinGroup(GroupJoinModel model)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Group group = await _repository.GetById(model.GroupId);
            if (group == null)
            {
                return NotFound();
            }
            if (group.IsPrivate && !is_admin && model.InviteId == null)
            {
                return Forbid();
            }
            try
            {
                //se for um admin, pode ignorar o requisito de convite caso o grupo seja privado
                await _repository.AddUser(group, user, model.InviteId, is_admin);
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
        /// Sair de um Grupo
        /// </summary>
        /// <param name="model">Modelo de Saída do Grupo com o ID do grupo em questão</param>
        /// <returns></returns>
        /// <response code="200">Saída do Utilizador bem-sucedida.</response>
        /// <response code="404">Caso não exista um grupo com o ID especificado.</response>
        /// <response code="400">Caso o utilizador não esteja presente no grupo ou se o mesmo for gestor do mesmo e for o único gestor no grupo.</response>
        [Authorize]
        [HttpPost]
        [Route("Leave")]
        public async Task<ActionResult> LeaveGroup(GroupLeaveModel model)
        {
            User user = await _userManager.GetUserAsync(this.User);
            Group group = await _repository.GetById(model.GroupId);
            if (group == null)
            {
                return NotFound();
            }
            try
            {
                await _repository.RemoveUser(group, user);
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
        /// "Remover" um Grupo
        /// </summary>
        /// <remarks>
        /// Visto que a remoção de um grupo é indesejada, devido à quantidade de informação associada ao mesmo, nomeadamente viagens, itinerários das viagens e publicações nas viagens, remover um grupo apenas irá:
        /// 
        /// Remover todos os utilizadores do grupo e de todas as viagens do grupo
        /// 
        /// Definir ambos o grupo e todas as viagens do grupo como privados
        /// 
        /// Esconder todas as publicações de todas as viagens do grupo
        /// </remarks>
        /// <param name="id">ID do grupo a "remover"</param>
        /// <returns></returns>
        /// <response code="200">"Remoção" do Grupo Bem-Sucedida.</response>
        /// <response code="403">Caso o utilizador não seja gestor do grupo ou administrador do sistema.</response>
        /// <response code="404">Caso não exista um grupo com o ID especificado.</response>
        /// <response code="400">Erro na "remoção" do grupo.</response>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Group group = await _repository.GetById(id);
            if (group == null)
            {
                return NotFound();
            }
            UserGroupRole role = await _repository.GetUserRole(group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER))
            {
                return Forbid();
            }
            try
            {
                await _repository.Delete(group);
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
        /// Actualizar ou Criar a imagem de um Grupo
        /// </summary>
        /// <remarks>
        /// Não é possível passar uma imagem por JSON ou num modelo.
        /// 
        /// Portanto, é necessário realizar a actualização ou criação da imagem de um grupo através de um método que usa um formulário (form-data)
        /// 
        /// Actualiza a imagem (removendo a posterior, se existir) do grupo, ou cria uma nova se não existir uma.
        /// </remarks>
        /// <param name="groupId">ID do Grupo</param>
        /// <param name="file">Ficheiro da Imagem. Apenas JPG, JPEG, PNG e GIFs são aceites.</param>
        /// <returns></returns>
        /// <response code="200">Actualização ou Inserção da Imagem no Grupo bem-sucedida.</response>
        /// <response code="403">Caso o utilizador não seja gestor do grupo ou administrador do sistema.</response>
        /// <response code="404">Caso não exista um grupo com o ID especificado.</response>
        /// <response code="400">Caso não tenha sido passado uma imagem válida (JPG, JPEG, PNG ou GIF), ou tenha ocorrido um erro de actualização/inserção da imagem.</response>
        [Authorize]
        [HttpPost("{groupId}/Picture")]
        public async Task<ActionResult> UpdateGroupPicture(int groupId, [FromForm] IFormFile file)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Group group = await _repository.GetById(groupId);
            if (group == null)
            {
                return NotFound();
            }
            UserGroupRole role = await _repository.GetUserRole(group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER))
            {
                return Forbid();
            }
            try
            {
                await _repository.UpdateImage(group, file);
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
        /// Remove a imagem de um grupo
        /// </summary>
        /// <param name="groupId">ID do grupo</param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("{groupId}/Picture")]
        public async Task<ActionResult> DeleteGroupPicture(int groupId)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Group group = await _repository.GetById(groupId);
            if (group == null)
            {
                return NotFound();
            }
            UserGroupRole role = await _repository.GetUserRole(group, user);
            if (!(is_admin || role == UserGroupRole.MANAGER))
            {
                return Forbid();
            }
            try
            {
                await _repository.RemoveImage(group);
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
        /// Proibir um utilizador de se juntar ao grupo
        /// </summary>
        /// <remarks>
        /// Proíbe um utilizador de se juntar a um grupo, juntamente expulsando-o do grupo.
        /// 
        /// Acompanhado a isto, pode ser ou não associado uma data de limite de proibição, em que até à mesma o utilizador não se poderá juntar ao grupo, o motivo da proibição, apenas acessível aos gestores.
        /// 
        /// É também possível definir se acompanhado da proibição do utilizador no grupo, o gestor também deseja esconder todas as suas publicações, uma opção desejável caso este utilizador for um spammer.
        /// </remarks>
        /// <param name="model">O modelo a passar no corpo do pedido com o ID do grupo, do utilizador, razão da proibição e data (se não for permanente) limite da proibição</param>
        /// <returns>Estado da operação de proibição</returns>
        /// <response code="200">Se o utilizador tiver sido proíbido do grupo com sucesso</response>
        /// <response code="400">Se o utilizador já tiver sido previamente proibido do grupo ou o mesmo não existir no grupo</response>
        /// <response code="404">Se o ID do utilizador ou grupo passados no modelo não corresponderem a nenhum utilizador ou grupo no sistema</response>
        /// <response code="403">Se o utilizador não for gestor ou moderador do grupo, ou administrador do sistema</response>
        [Authorize]
        [HttpPost("Ban")]
        public async Task<ActionResult> BanUser(GroupBanUserModel model)
        {
            Group group = await _repository.GetById(model.GroupId);
            User user = await _userManager.FindByIdAsync(model.UserId);
            if (group == null || user == null)
            {
                return NotFound();
            }
            User CurrentUser = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(CurrentUser);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            UserGroupRole role = await _repository.GetUserRole(group, CurrentUser);
            if (!(role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR || is_admin))
            {
                return Forbid();
            }
            try
            {
                await _repository.BanUser(group, user, model.BanReason, model.BanUntil, model.HidePosts);
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
        /// Remover a proibição de um utilizador poder entrar no grupo.
        /// </summary>
        /// <param name="id">ID da Proibição</param>
        /// <returns>Estado da operação de unban</returns>
        /// <response code="200">Se a proibição da entrada de um utilizador do grupo tiver sido removida com sucesso</response>
        /// <response code="400">Se o utilizador não estivesse proibido do grupo</response>
        /// <response code="404">Se o ID da proibição passada não corresponder a nenhuma proibição</response>
        /// <response code="403">Se o utilizador não for gestor ou moderador do grupo, ou administrador do sistema</response>
        [Authorize]
        [HttpDelete("Ban/{id}")]
        public async Task<ActionResult> UnbanUser(int id)
        {
            GroupBan ban = await _repository.GetBanById(id);
            if (ban == null)
            {
                return NotFound();
            }
            User CurrentUser = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(CurrentUser);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            UserGroupRole role = await _repository.GetUserRole(ban.Group, CurrentUser);
            if (!(role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR || is_admin))
            {
                return Forbid();
            }
            try
            {
                await _repository.UnbanUser(ban.Group, ban.User);
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
