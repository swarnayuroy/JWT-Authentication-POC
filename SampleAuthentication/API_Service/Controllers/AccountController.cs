using API_Service.Models.DTO;
using API_Service.Models.ResponseModel;
using API_Service.RepositoryLayer.Interface;
using API_Service.RepositoryLayer.Repository;
using API_Service.Utils;
using Microsoft.AspNetCore.Authorization;
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
        public AccountController(
            ILogger<AccountController> logger, 
            IAccountRepository accountService
        )
        {
            this._logger = new LoggerService<AccountController>(logger);
            this._accountService = accountService;
        }

        [AllowAnonymous]
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

                return response.Status ? Ok(response as ResponseDataDetail<string>) : Unauthorized(response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while processing your request.");
            }
        }

        [AllowAnonymous]
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

        #region Check/Verify/Set New Password

        [AllowAnonymous]
        [HttpPost]
        [Route("emailexists")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmailExists([FromBody] CheckEmail userEmail)
        {
            try
            {
                var response = await _accountService.EmailExists(userEmail.Email);
                return response.Status ? StatusCode(StatusCodes.Status200OK, response.Message) : NotFound(response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while processing your request.");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("verify")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Verify([FromBody] VerifyAccount detail)
        {
            try
            {
                var response = await _accountService.Verify(detail);
                return response.Status ? StatusCode(StatusCodes.Status200OK, response.Message) : BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }

        [AllowAnonymous]
        [HttpPut]
        [Route("setpassword")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetPassword([FromBody] UserCredential userCredential)
        {
            try
            {
                var response = await _accountService.SetPassword(userCredential);
                return response.Status ? StatusCode(StatusCodes.Status200OK, response.Message) : BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while processing your request.");
            }
        }

        #endregion
    }
}
