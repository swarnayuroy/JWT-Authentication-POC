using API_Service.Models.DTO;
using API_Service.RepositoryLayer.Interface;
using API_Service.RepositoryLayer.Repository;
using API_Service.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace API_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private LoggerService<AccountController> _logger;
        private readonly IAccountRepository _accountService;
        public AccountController(IAccountRepository accountService)
        {
            this._logger = new LoggerService<AccountController>(new LoggerFactory().CreateLogger<AccountController>());
            this._accountService = accountService;
        }

        [HttpPost]
        [Route("check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Check([FromBody] UserCredential userCredential)
        {
            try
            {
                var response = await _accountService.CheckCredential(userCredential);

                return response.Status ? Ok() : Unauthorized(response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] UserDetail userRegistrationDetail)
        {
            try
            {
                var response = await _accountService.RegisterUser(userRegistrationDetail);
                return response.Status
                    ? StatusCode(StatusCodes.Status201Created, response.Message)
                    : Conflict(response.Message);
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
