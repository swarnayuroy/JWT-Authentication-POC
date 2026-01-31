using API_Service.RepositoryLayer.Interface;
using API_Service.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace API_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountService;
        public AccountController(IAccountRepository accountService)
        {
            this._accountService = accountService;
        }

        [HttpPost]
        [Route("check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Check([FromBody] UserCredential userCredential)
        {
            try
            {
                var response = await _accountService.CheckCredential(userCredential);

                return response.Status ? Ok() : Unauthorized(response.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] UserDetail userRegistrationDetail)
        {
            try
            {
                var response = await _accountService.RegisterUser(userRegistrationDetail);
                return response.Status
                    ? StatusCode(StatusCodes.Status201Created, response.Message)
                    : Conflict(response.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while processing your request.");
            }
        }
    }
}
