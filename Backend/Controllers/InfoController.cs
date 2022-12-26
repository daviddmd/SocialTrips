using AutoMapper;
using BackendAPI.Entities;
using BackendAPI.Models.Information;
using BackendAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IInformationRespository _informationRepository;
        private readonly IMapper _mapper;

        public InfoController(UserManager<User> userManager, IInformationRespository informationRepository, IMapper mapper)
        {
            _userManager = userManager;
            _informationRepository = informationRepository;
            _mapper = mapper;
        }

        [HttpGet("Recommendations")]
        [AllowAnonymous]
        public async Task<ActionResult<RecommendationModel>> GetRecommendations()
        {
            User current_user = await _userManager.GetUserAsync(this.User);
            Recommendation recommendation = await _informationRepository.GetRecommendations(current_user);
            RecommendationModel model = _mapper.Map<Recommendation, RecommendationModel>(recommendation);
            return Ok(model);
        }
        [Authorize(Roles = "ADMIN")]
        [HttpGet("Statistics")]
        public async Task<ActionResult<StatisticsModel>> GetStatistics()
        {
            Statistic statistic = await _informationRepository.GetStatistics();
            StatisticsModel model = _mapper.Map<Statistic, StatisticsModel>(statistic);
            return Ok(model);
        }
    }
}
