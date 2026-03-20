using API_Service.Models.DTO;
using API_Service.Models.ResponseModel;
using API_Service.RepositoryLayer.Interface;
using API_Service.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private LoggerService<UserController> _logger;
        private readonly IUserRepository _userRepository;
        public UserController(ILogger<UserController> logger, IUserRepository userRepository)
        {
            this._logger = new LoggerService<UserController>(logger);
            this._userRepository = userRepository;
        }

        /// <summary>
        /// Get all users - Admin only
        /// </summary>
        [HttpGet]
        [Route("get")]
        [Authorize(Roles = nameof(Role.Admin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 5, [FromQuery] string searchText = "")
        {
            try
            {
                // Placeholder for actual user retrieval logic
                var response = new ResponseDetail();
                response = string.IsNullOrEmpty(searchText) ? await _userRepository.GetAllUsersAsync(userId, page, pageSize) : 
                            await _userRepository.GetUserBySearch(userId, page, pageSize, searchText);
                if (response.Status)
                {
                    return Ok(response as ResponseDataDetail<PagedResult<UserDetail>>);
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

        /// <summary>
        /// Get user by ID - Any authenticated user can access
        /// </summary>
        [HttpGet]
        [Route("get/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                // Placeholder for actual user retrieval logic by ID
                var response = await _userRepository.GetUserAsync(id);
                if (response.Status)
                {
                    return Ok(response as ResponseDataDetail<UserDetail>);
                }
                else
                {
                    return NotFound(response.Message);
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
