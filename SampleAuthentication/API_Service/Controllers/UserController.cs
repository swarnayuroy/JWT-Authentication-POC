using API_Service.RepositoryLayer.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API_Service.Utils;

namespace API_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private LoggerService<UserController> _logger;
        private readonly IUserRepository _userRepository;
        public UserController(IUserRepository userRepository)
        {
            this._logger = new LoggerService<UserController>(new LoggerFactory().CreateLogger<UserController>());
            this._userRepository = userRepository;
        }

        [HttpGet]
        [Route("get")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Placeholder for actual user retrieval logic
                var users = await _userRepository.GetAllUsersAsync();
                if (users.Any())
                {
                    return Ok(users);
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }

        [HttpGet]
        [Route("get/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                // Placeholder for actual user retrieval logic by ID
                var user = await _userRepository.GetUserAsync(id);
                if (user != null)
                {
                    return Ok(user);
                }
                else
                {
                    return NotFound("User not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }
    }
}
