using AutoMapper;
using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Exceptions;
using BackendAPI.Models;
using BackendAPI.Models.Ranking;
using BackendAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class RankingsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IRankingRepository _repository;

        public RankingsController(IMapper mapper, IRankingRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }
        /// <summary>
        /// Obter todos os rankings no sistema
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Lista de Rankings no Sistema</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RankingModel>>> GetAll()
        {
            IEnumerable<Ranking> rr = await _repository.GetAll();
            IEnumerable<RankingModelAdmin> rankings_ret = from r in rr select _mapper.Map<Ranking, RankingModelAdmin>(r);
            return Ok(rankings_ret);
        }
        /// <summary>
        /// Obter os detalhes de um ranking
        /// </summary>
        /// <param name="Id">Id do Ranking</param>
        /// <returns></returns>
        /// <response code="200">Detalhes de um Ranking</response>
        /// <response code="404">Não existe um ranking com este ID</response>
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<RankingModelAdmin>> GetById(int Id)
        {
            Ranking ranking = await _repository.GetById(Id);
            if (ranking == null)
            {
                return NotFound();
            }
            return _mapper.Map<Ranking, RankingModelAdmin>(ranking);
        }
        /// <summary>
        /// Atualizar os detalhes de um ranking no sistema
        /// </summary>
        /// <remarks>
        /// Não é possível atualizar os detalhes de um ranking de modo a que a sua distância mínima se torne igual à de um ranking existente
        /// </remarks>
        /// <param name="Id">ID do Ranking a Actualizar</param>
        /// <param name="model">Modelo JSON com os novos detalhes do ranking</param>
        /// <returns></returns>
        /// <response code="200">Ranking actualizado com sucesso</response>
        /// <response code="404">Não existe um ranking com este ID</response>
        /// <response code="400">Erro na actualização do ranking, nomeadamente em que a nova distância mínima seria igual à distância mínima de um ranking existente.</response>
        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult<RankingModelAdmin>> UpdateRanking(int Id, RankingUpdateModel model)
        {
            Ranking ranking = await _repository.GetById(Id);
            if (ranking == null)
            {
                return NotFound();
            }
            try
            {
                await _repository.Update(ranking, model);
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
        /// Criar um Ranking
        /// </summary>
        /// <param name="model">Modelo JSON do ranking a criar</param>
        /// <returns></returns>
        /// <response code="200">Ranking criado com sucesso</response>
        /// <response code="400">Erro na criação do ranking, nomeadamente em que a distância mínima deste novo ranking seria igual à distância mínima de um ranking existente.</response>
        [HttpPost]
        public async Task<ActionResult<RankingModelAdmin>> CreateRanking(RankingCreateModel model)
        {
            Ranking r = _mapper.Map<RankingCreateModel, Ranking>(model);
            try
            {
                await _repository.Create(r);
                return Ok(_mapper.Map<Ranking, RankingModelAdmin>(r));
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
        /// Remover um ranking
        /// </summary>
        /// <param name="Id">ID do Ranking a remover</param>
        /// <returns></returns>
        /// <response code="200">Ranking removido com sucesso</response>
        /// <response code="404">Não existe um ranking no sistema com o ID passado por parâmetro de URL</response>
        /// <response code="400">Erro na remoção do ranking, caso se esteja a tentar remover o ranking "default", com 0 quilómetros, ou caso apenas reste um ranking.</response>
        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> DeleteRanking(int Id)
        {
            Ranking ranking = await _repository.GetById(Id);
            if (ranking == null)
            {
                return NotFound();
            }
            try
            {
                await _repository.Delete(ranking);
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
