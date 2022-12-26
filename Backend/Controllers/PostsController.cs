using AutoMapper;
using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Exceptions;
using BackendAPI.Models;
using BackendAPI.Models.Post;
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
    public class PostsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ITripRepository _tripRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly UserManager<User> _userManager;
        private readonly IPostRepository _postRepository;

        public PostsController(IMapper mapper, ITripRepository tripRepository, IGroupRepository groupRepository, UserManager<User> userManager, IPostRepository postRepository)
        {
            _mapper = mapper;
            _tripRepository = tripRepository;
            _groupRepository = groupRepository;
            _userManager = userManager;
            _postRepository = postRepository;
        }
        /// <summary>
        /// Obter todas as publicações no sistema
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Lista de publicações</response>
        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostModel>>> GetAllPosts()
        {
            IEnumerable<Post> posts = await _postRepository.GetAll();
            IEnumerable<PostModel> posts_ret = from post in posts select _mapper.Map<Post, PostModel>(post);
            return Ok(posts_ret);
        }
        /// <summary>
        /// Obter uma publicação pelo seu ID
        /// </summary>
        /// <remarks>
        /// Este endpoint é suposto ser usado para permitir o "hotlinking" de publicações (https://viagens-sociais.xyz/post/2)
        /// </remarks>
        /// <param name="Id">ID da publicação</param>
        /// <returns></returns>
        /// <response code="200">Modelo JSON com os detalhes da publicação</response>
        /// <response code="404">
        /// Pode acontecer em vários casos:
        /// 
        /// Se não existir uma publicação com o ID especificado
        /// 
        /// Se o autor da publicação estiver inactivo e o utilizador que realizou o pedido não é um administrador do sistema
        /// 
        /// Se a publicação pertencer a uma viagem privada e o utilizador não fizer parte da viagem
        /// 
        /// Se a publicação pertencer a uma viagem que está num grupo privado e o utilizador não pertencer a esse grupo
        /// 
        /// Se a publicação estiver escondida ("eliminada") e o utilizador que realizou o pedido não é moderador ou gestor do grupo em que a viagem onde a publicação foi criada está, ou administrador do sistema
        /// </response>

        [Authorize]
        [HttpGet("{id}")]
        /*
         * Condições:
         * 1) Post não pode estar escondido, se não for gestor/moderador/admin
         * 2) Post não pode estar num grupo ou viagem privada se o utilizador que o desejar ler não estiver lá
         * 3) Post não pode pertencer a um utilizador desativado
         */
        public async Task<ActionResult<PostModel>> GetById(int Id)
        {
            Post post = await _postRepository.GetById(Id);
            if (post == null)
            {
                return NotFound();
            }
            User current_user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(current_user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            if (!post.User.IsActive && !is_admin)
            {
                return NotFound();
            }
            //Role do utilizador em relação ao grupo da viagem do post
            UserGroupRole role = await _groupRepository.GetUserRole(post.Trip.Group, current_user);
            if (post.IsHidden && !(is_admin || role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR))
            {
                return NotFound();
            }
            if (
                (post.Trip.IsPrivate && !current_user.Trips.Any(cut => cut.Trip.Id == post.Trip.Id)) ||
                (post.Trip.Group.IsPrivate && !current_user.Groups.Any(cug => cug.Group.Id == post.Trip.Group.Id))
                )
            {
                return NotFound();
            }
            return Ok(_mapper.Map<Post, PostModel>(post));
        }


        /// <summary>
        /// Criar uma publicação
        /// </summary>
        /// <remarks>
        /// Uma publicação tem um corpo (descrição), uma data de evento, a viagem a que pertence, o utilizador que a publicou, e anexos (imagens ou vídeos).
        /// 
        /// A intenção por detrás do sistema de publicações da viagem é as pessoas que participaram na viagem poderem inserir publicações na mesma durante e após a viagem.
        /// 
        /// Deste modo, mesmo que a data presente seja 15 de Junho de 2023, deve ser possível, se o utilizador tiver estado em permanência na viagem, inserir nas publicações da viagem uma publicação e dizer que a mesma
        /// decorreu em 25 de Setembro de 2022, desde que a data da publicação esteja entre as datas de início e fim da viagem em si. Deste modo, pode-se construir uma espécie de álbum para cada dia da viagem com publicações.
        /// 
        /// Nota-se que internamente as publicações têm uma data interna (PublishedDate), que é quando as mesmas efectivamente entraram no sistema (base de dados), usado para indicar quando a mesma foi publicada OU editada.
        /// 
        /// O autor da publicação é o utilizador que realiza o pedido, e o mesmo tem que estar na viagem e no grupo onde a publicação irá ser inserida (salvo se for o administrador do sistema)
        /// </remarks>
        /// <param name="model">Modelo com os detalhes da publicação a criar, nomeadamente o ID da viagem, a descrição (corpo) da publicação e a data do evento da publicação</param>
        /// <returns></returns>
        /// <response code="200">Modelo JSON com os detalhes da publicação criada (útil porque retorna o ID da publicação e em ajax permite adicionar o mesmo ao feed de publicações sem necessitar de refrescar a página)</response>
        /// <response code="404">Caso não exista uma viagem com o ID especificado</response>
        /// <response code="400">
        /// Pode acontecer por vários motivos:
        /// 
        /// Caso o utilizador não pertença à viagem ou ao grupo em que a publicação vai ser inserida (e não é administrador do sistema)
        /// 
        /// A data da publicação definida pelo utilizador não está entre a data de início e fim da viagem.
        /// </response>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<PostModelTrip>> CreatePost(PostCreateModel model)
        {
            User user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            Trip trip = await _tripRepository.GetById(model.TripId);
            if (trip == null)
            {
                return NotFound();
            }
            if ((await _tripRepository.GetUserTrip(trip, user) == null || await _groupRepository.GetUserGroup(trip.Group, user) == null) && !is_admin)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.POST_USER_NOT_IN_TRIP, Message = "Can't create a post since the user isn't in the trip" });
            }
            Post post = _mapper.Map<PostCreateModel, Post>(model);
            post.User = user;
            post.Trip = trip;
            try
            {
                await _postRepository.Create(post);
                return Ok(_mapper.Map<Post, PostModel>(post));
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
        /// Actualizar uma publicação
        /// </summary>
        /// <remarks>
        /// Apenas se pode editar a descrição, data da publicação e "eliminá-la" com o atributo "IsHidden".
        /// 
        /// A partir do momento que o autor da publicação a "elimina" (ou um gestor/administrador/moderador), este deixa de a poder ver, porém continua a ser visível para os gestores/moderadores do grupo em que a viagem da 
        /// publicação está, ou para os administradores do sistema
        /// 
        /// Apenas os gestores/moderadores do grupo, admin e próprio utilizador podem atualizar o seu post
        /// 
        /// Utilizador não pode atualizar o seu post novamente quando IsHidden é true, visto que é dado como "removido" (404).
        /// </remarks>
        /// <param name="Id">ID da publicação</param>
        /// <param name="model">Modelo JSON com a informação a actualizar na publicação</param>
        /// <returns></returns>
        /// <response code="200">Caso o post tenha sido editado ou "removido" com sucesso</response>
        /// <response code="404">Caso a publicação com este ID genuinamente não exista ou tenha sido "removida".</response>
        /// <response code="403">O utilizador que realizou este pedido não é ou o autor da publicação em si, um gestor ou moderador do grupo, ou um dos administradores do sistema.</response>
        /// <response code="400">A data da publicação não está entre a data de início e fim da viagem a que a publicação pertence.</response>

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePost(int Id, PostUpdateModel model)
        {
            Post post = await _postRepository.GetById(Id);
            if (post == null)
            {
                return NotFound();
            }
            User current_user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(current_user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            UserGroupRole role = await _groupRepository.GetUserRole(post.Trip.Group, current_user);
            if (role == UserGroupRole.REGULAR && post.IsHidden)
            {
                return NotFound();
            }
            if (!(role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR || is_admin) && current_user != post.User)
            {
                return Forbid();
            }
            try
            {
                await _postRepository.Update(post, model);
                return Ok(_mapper.Map<Post, PostModel>(post));
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
        /// Remover completamente uma publicação do sistema
        /// </summary>
        /// <remarks>
        /// Ao contrário do atributo IsHidden que é a remoção para o utilizador, e que na prática, esconde a publicação do utilizador até um gestor/moderador do grupo da viagem ou um administrador do sistema a puserem como
        /// visível novamente, este endpoint remove a publicação do sistema juntamente com os anexos.
        /// 
        /// Apenas gestores/moderadores do grupo da viagem em que esta publicação se situa, ou administradores do sistema podem chamar este endpoint.
        /// 
        /// Para os utilizadores apenas aparece o botão "Remover", que "esconde" a publicação, e para os Gestores/Moderadores aparece "Esconder" e "Remover".
        /// </remarks>
        /// <param name="Id">ID da publicação</param>
        /// <returns></returns>
        /// <response code="200">Publicação removida com sucesso</response>
        /// <response code="404">Caso uma publicação com este ID verdadeiramente não exista, ou caso um utilizador tente usar este API para descobrir se a sua publicação foi realmente removida</response>
        /// <response code="403">O utilizador que realizou este pedido não é um gestor ou moderador do grupo da viagem da publicação, ou um dos administradores do sistema.</response>
        /// <response code="400">Erro ao remover os anexos da publicação.</response>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int Id)
        {
            Post post = await _postRepository.GetById(Id);
            if (post == null)
            {
                return NotFound();
            }
            User current_user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(current_user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            UserGroupRole role = await _groupRepository.GetUserRole(post.Trip.Group, current_user);
            if (role == UserGroupRole.REGULAR && post.IsHidden)
            {
                return NotFound();
            }
            if (!(is_admin || role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR))
            {
                return Forbid();
            }
            try
            {
                await _postRepository.Delete(post);
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
        /// Adicionar um anexo (vídeo ou imagem) a uma publicação
        /// </summary>
        /// <remarks>
        /// Permite adicionar imagens (JPG, PNG, JPEG ou GIF) ou vídeos (MP4, AVI, MOV, MKV, WEBM ou WMV) a uma publicação.
        /// 
        /// O sistema de autorizações para adicionar ou remover anexos a uma publicação é o mesmo sistema para modificar uma publicação, ou seja, é necessário ser ou o autor da publicação, um gestor/moderador do grupo da viagem
        /// da publicação ou um administrador do sistema.
        /// 
        /// Quando se adiciona um anexo a uma publicação, o sistema responde com um modelo JSON da publicação em si. Deste modo, é possível dinamicamente adicionar anexos à publicação e a publicação dinamicamente ser modificada,
        /// sem ser necessário refrescar a página, se o estado de resposta for 200, caso contrário mostra-se um aviso dependendo do erro.
        /// </remarks>
        /// <param name="postId">ID da publicação a adicionar um anexo</param>
        /// <param name="file">Ficheiro a adicionar à publicação</param>
        /// <returns></returns>
        /// <response code="200">Caso anexo tenha sido adicionado à publicação com sucesso</response>
        /// <response code="404">Caso uma publicação com este ID não exista, ou esteja escondida para o utilizador autor da publicação</response>
        /// <response code="403">O utilizador que realizou este pedido não é ou o autor da publicação em si, um gestor ou moderador do grupo, ou um dos administradores do sistema.</response>
        /// <response code="400">Erro ao carregar o anexo para a Google Cloud Storage</response>
        [Authorize]
        [HttpPost("Attachment")]
        public async Task<ActionResult<PostModel>> AddAttachment([FromForm] int postId, [FromForm] IFormFile file)
        {
            Post post = await _postRepository.GetById(postId);
            if (post == null)
            {
                return NotFound();
            }
            User current_user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(current_user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            UserGroupRole role = await _groupRepository.GetUserRole(post.Trip.Group, current_user);
            if (role == UserGroupRole.REGULAR && post.IsHidden)
            {
                return NotFound();
            }
            if (!(role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR || is_admin) && current_user != post.User)
            {
                return Forbid();
            }
            try
            {
                await _postRepository.AddAttachment(post, file);
                return Ok(_mapper.Map<Post, PostModel>(post));
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
        /// Remover um anexo de uma publicação
        /// </summary>
        /// <remarks>
        /// Remove um anexo de uma publicação. Para implementar isto num frontend, realiza-se um pedido AJAX ou assíncrono com o ID do anexo, e se a resposta for 200, remove-se o mesmo do DOM/página/interface.
        /// </remarks>
        /// <param name="Id">ID do anexo</param>
        /// <returns></returns>
        /// <response code="200">Anexo removido com sucesso</response>
        /// <response code="404">Caso não exista um anexo com este ID, ou a publicação esteja eliminada/escondida (caso seja o autor da publicação)</response>
        /// <response code="403">O utilizador que realizou este pedido não é ou o autor da publicação em si, um gestor ou moderador do grupo, ou um dos administradores do sistema.</response>
        /// <response code="400">Erro ao remover o anexo da publicação (Google Cloud Storage)</response>
        [Authorize]
        [HttpDelete("Attachment/{id}")]
        public async Task<ActionResult> RemoveAttachment(Guid Id)
        {
            Attachment attachment = await _postRepository.GetAttachmentById(Id);
            if (attachment == null)
            {
                return NotFound();
            }
            Post post = attachment.Post;
            User current_user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(current_user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            UserGroupRole role = await _groupRepository.GetUserRole(post.Trip.Group, current_user);
            if (role == UserGroupRole.REGULAR && post.IsHidden)
            {
                return NotFound();
            }
            if (!(role == UserGroupRole.MANAGER || role == UserGroupRole.MODERATOR || is_admin) && current_user != post.User)
            {
                return Forbid();
            }
            try
            {
                await _postRepository.RemoveAttachment(attachment);
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
