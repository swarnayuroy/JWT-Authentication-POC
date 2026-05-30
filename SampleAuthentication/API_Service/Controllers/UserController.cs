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
        /// Get all users - Admins only
        /// </summary>
        [HttpGet]
        [Route("get")]
        [Authorize(Roles = "Superadmin, Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] string userId, [FromQuery] string userType, [FromQuery] int page = 1, [FromQuery] int pageSize = 5, [FromQuery] string searchText = "")
        {
            try
            {
                var response = string.IsNullOrEmpty(searchText) ? await _userRepository.GetAllUsersAsync(userId, userType, page, pageSize) : 
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

        [HttpGet]
        [Route("getDetails/{id}")]
        [Authorize(Roles = "Superadmin, Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDetails(string id)
        {
            try
            {
                var response = await _userRepository.GetUserDetailAsync(id);
                if (response.Status)
                {
                    return Ok(response as ResponseDataDetail<FullUserDetail>);
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
